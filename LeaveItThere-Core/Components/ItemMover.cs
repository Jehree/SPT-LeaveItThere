using EFT.InputSystem;
using Helpers.CursorHelper;
using LeaveItThere.Common;
using LeaveItThere.CustomUI;
using LeaveItThere.Fika;
using LeaveItThere.Helpers;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using static LeaveItThere.CustomUI.MoveModeUI;

namespace LeaveItThere.Components
{
    internal class ItemMover : MonoBehaviour
    {
        private static ItemMover _instance = null;
        public static ItemMover Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = LITSession.Instance.Player.gameObject.GetOrAddComponent<ItemMover>();
                }
                return _instance;
            }
            private set
            {
                _instance = value;
            }
        }

        #region Target
        private FakeItem _target = null;
        public FakeItem Target
        {
            get => _target;
            private set
            {
                _target = value;
                if (_target != null)
                {
                    _targetCachedRotation = _targetTransform.rotation;
                }
            }
        }
        private Moveable _targetMoveable = null;
        public Moveable TargetMoveable
        {
            get
            {
                if (Target == null)
                {
                    throw new Exception("Tried to access TargetMoveable while Target was null!");
                }

                if (_targetMoveable == null)
                {
                    _targetMoveable = Target.gameObject.GetOrAddComponent<Moveable>();
                }

                return _targetMoveable;
            }
        }
        private Transform _targetTransform => Target.gameObject.transform;
        #endregion

        #region Cached Values
        private Vector3 _undoPosition;
        private Quaternion _undoRotation;
        private Quaternion _targetCachedRotation;
        #endregion

        #region UI
        public MoveModeUI UI => MoveModeUI.Instance;
        public ETabType CurrentMode => UI.SelectedTabType;

        public bool WillFloat => UI.PhysTab.ItemFloats.isOn;
        #endregion

        #region Input + Config Settings
        private bool _lmbDown = false;
        public bool LMBDown
        {
            get => _lmbDown;
            private set
            {
                if (value)
                {
                    // cursor ALWAYS HIDDEN WHEN LMB IS DOWN
                    CursorHelper.ReturnCursorControlToEFT();
                }
                else if (!RMBDown)
                {
                    // cursor is ONLY UNHIDDEN when BOTH mouse buttons are UP
                    CursorHelper.ForceUnlockCursor();
                }

                _lmbDown = value;
            }
        }
        private bool _rmbDown = false;
        public bool RMBDown
        {
            get => _rmbDown;
            private set
            {
                // handling for cursor and camera separated below because it is kinda confusing

                // CURSOR HANDLING:
                if (value)
                {
                    // cursor ALWAYS HIDDEN WHEN RMB IS DOWN
                    CursorHelper.ReturnCursorControlToEFT();
                }
                else if (!LMBDown)
                {
                    // cursor is ONLY UNHIDDEN when BOTH mouse buttons are UP
                    CursorHelper.ForceUnlockCursor();
                }

                // CAMERA HANDLING:
                if (value)
                {
                    // camera locking and unlocking is ONLY controlled by RMB
                    InteractionHelper.SetCameraRotationLocked(false);
                }
                else
                {

                    // camera locking and unlocking is ONLY controlled by RMB
                    InteractionHelper.SetCameraRotationLocked(true);
                }

                _rmbDown = value;
            }
        }
        private float _mouseX => Input.GetAxis("Mouse X");
        private float _mouseY => Input.GetAxis("Mouse Y");
        private float _scrollInput => Input.GetAxis("Mouse ScrollWheel");
        private float _precisionMultiplier => Settings.PrecisionKey.Value.IsPressed() ? Settings.PrecisionMultiplier.Value : 1;
        private float _mouseRepositionSpeedMultiplier => Settings.RepositionSpeed.Value * _precisionMultiplier;
        private float _scrollRepositionSpeedMultiplier => Settings.RepositionScrollSpeed.Value * _precisionMultiplier;
        private float _mouseRotationSpeedMultipier => Settings.RotationSpeed.Value * _precisionMultiplier;
        private float _scrollRotationSpeedMultiplier => Settings.RotationScrollSpeed.Value * _precisionMultiplier;
        #endregion

        private Vector3 _cameraForward
        {
            get
            {
                Vector3 forward = Camera.main.transform.forward;
                forward.y = 0;
                return forward;
            }
        }
        private Quaternion _cameraRotation => Quaternion.LookRotation(_cameraForward);

        #region Player Movement & Rotation Delta Tracking
        // orig = 1, 1, 1
        // new = -2, 1, 2
        // expected delta = -3, 0, 1
        // math: new - orig

        /// <summary>
        /// Should be called in OnEnable() and at the end of Update() to keep track of last frame's player pos
        /// </summary>
        private void UpdatePlayerLastFrameDeltas()
        {
            _playerPositionLastFrame = LITSession.Instance.Player.Transform.position;
        }
        private Vector3 _playerPositionLastFrame;

        /// <summary>
        /// Will only be correct while ItemMover is enabled
        /// </summary>
        public Vector3 PlayerMovementDelta => LITSession.Instance.Player.Transform.position - _playerPositionLastFrame;
        #endregion

        #region UI event subscriptions
        private void OnMovedToPlayerClicked() => TargetMoveable.MoveToPlayer();
        private void OnUndoMoveClicked() => _targetTransform.position = _undoPosition;
        private void OnResetRotationClicked() => _targetTransform.rotation = Quaternion.identity;
        private void OnUndoRotationClicked() => _targetTransform.rotation = _undoRotation;
        private void OnSaveButtonClicked() => Disable(true);
        private void OnCancelButtonClicked() => Disable(false);
        #endregion

        public ItemMover()
        {
            enabled = false;
        }

        private void Awake()
        {
            UI.TabSwitched += OnTabSwitched;

            UI.PosTab.MoveToPlayerButton.onClick.AddListener(OnMovedToPlayerClicked);
            UI.PosTab.UndoMoveButton.onClick.AddListener(OnUndoMoveClicked);

            UI.RotTab.ResetRotationButton.onClick.AddListener(OnResetRotationClicked);
            UI.RotTab.UndoRotationButton.onClick.AddListener(OnUndoRotationClicked);

            UI.SaveButton.onClick.AddListener(OnSaveButtonClicked);
            UI.CancelButton.onClick.AddListener(OnCancelButtonClicked);
        }

        private void OnEnable()
        {
            UpdatePlayerLastFrameDeltas();
            UI.PhysTab.ItemFloats.isOn = !Settings.ImmersivePhysics.Value;
            UI.ChangeTabs<PositionTab>();
            ItemHelper.SetItemColor(Color.green, Target.gameObject);
        }

        private void Update()
        {
            if (MoveModeDisallowed(Target, out string reason))
            {
                Disable(true);
                InteractionHelper.ErrorPlayerFeedback($"Move Mode cancelled! Reason: {reason}");
                return;
            }

            MouseInputProcess();
            UIHotkeyInputProcess(out bool cancelRemainingFrameLogic);
            if (cancelRemainingFrameLogic) return;

            if (CurrentMode == ETabType.Position)
            {
                PositionProcess();
            }

            if (CurrentMode == ETabType.Rotation)
            {
                RotationProcess();
            }

            if (CurrentMode == ETabType.Physics)
            {
                PhysicsProcess();
            }

            UpdatePlayerLastFrameDeltas();
        }

        private void MouseInputProcess()
        {
            if (EventSystem.current.IsPointerOverGameObject()) return;
            if (Input.GetMouseButtonDown(0))
            {
                LMBDown = true;
            }
            if (Input.GetMouseButtonUp(0))
            {
                LMBDown = false;
            }
            if (Input.GetMouseButtonDown(1))
            {
                RMBDown = true;
            }
            if (Input.GetMouseButtonUp(1))
            {
                RMBDown = false;
            }
        }

        private void UIHotkeyInputProcess(out bool cancelRemainingFrameLogic)
        {
            if (Settings.SaveHotkey.Value.IsDown())
            {
                Disable(true);
                cancelRemainingFrameLogic = true;
                return;
            }
            if (Settings.CancelHotkey.Value.IsDown())
            {
                Disable(false);
                cancelRemainingFrameLogic = true;
                return;
            }
            if (Settings.RepositionTabHotkey.Value.IsDown())
            {
                UI.ChangeTabs<PositionTab>();
            }
            if (Settings.RotationTabHotkey.Value.IsDown())
            {
                UI.ChangeTabs<RotationTab>();
            }
            if (Settings.PhysicsTabHotkey.Value.IsDown())
            {
                UI.ChangeTabs<PhysicsTab>();
            }
            cancelRemainingFrameLogic = false;
        }

        private void OnTabSwitched(ETabType tabType)
        {
            if (tabType == ETabType.Position)
            {
                TargetMoveable.DisablePhysics();
                ItemHelper.SetItemColor(Color.green, Target.gameObject);
            }
            else if (tabType == ETabType.Rotation)
            {
                TargetMoveable.DisablePhysics();
                ItemHelper.SetItemColor(Color.red, Target.gameObject);
            }
            else if (tabType == ETabType.Physics)
            {
                ItemHelper.SetItemColor(Color.magenta, Target.gameObject);
            }
        }

        public void Enable(FakeItem target)
        {
            if (enabled == true) return;

            LITSession.Instance.SetInteractionsEnabled(false);
            LITSession.Instance.GamePlayerOwner.ClearInteractionState();

            UI.SetActive(true);

            Target = target;
            _undoPosition = _targetTransform.position;
            _undoRotation = _targetTransform.rotation;
            Target.SetPlayerAndBotCollisionEnabled(false);

            InteractionHelper.SetCameraRotationLocked(true);
            InteractionHelper.SetMostInputsIgnored(true, [ECommand.Jump, ECommand.ToggleSprinting, ECommand.EndSprinting, ECommand.ToggleDuck, ECommand.ResetLookDirection]);
            CursorHelper.ForceUnlockCursor();

            enabled = true;
        }

        public void Disable(bool movementSaved)
        {
            if (enabled == false) return;

            enabled = false;

            if (movementSaved)
            {
                Target.PlaceAtPosition(Target.gameObject.transform.position, Target.gameObject.transform.rotation);
                FikaBridge.SendPlacedStateChangedPacket(Target, true, WillFloat);
                InteractionHelper.NotificationLong("Placement edit saved!");
            }
            else
            {
                _targetTransform.position = _undoPosition;
                _targetTransform.rotation = _undoRotation;
                InteractionHelper.NotificationLongWarning("Move Mode cancelled.");
            }

            if (movementSaved && !WillFloat)
            {
                TargetMoveable.EnablePhysics(true);
            }
            else
            {
                TargetMoveable.DisablePhysics();
            }

            Target.SetPlayerAndBotCollisionEnabled(Settings.PlacedItemsHaveCollision.Value);
            ItemHelper.SetItemColor(Settings.PlacedItemTint.Value, Target.gameObject);
            Target.gameObject.transform.SetParent(null); // so that item unparents if both mouse buttons were held
            Target = null;

            LITSession.Instance.SetInteractionsEnabled(true);
            UI.SetActive(false);

            LMBDown = false;
            RMBDown = false;

            InteractionHelper.SetCameraRotationLocked(false);
            CursorHelper.ReturnCursorControlToEFT();
            InteractionHelper.SetMostInputsIgnored(false);
            InteractionHelper.RefreshPrompt();
        }

        public static bool MoveModeDisallowed(FakeItem fakeItem, out string reason)
        {
            if (fakeItem.Flags.MoveModeDisabled)
            {
                reason = fakeItem.Flags.MoveModeDisabledReason;
                return true;
            }
            if (Settings.MoveModeRequiresInventorySpace.Value && !ItemHelper.ItemCanBePickedUp(fakeItem.LootItem.Item))
            {
                reason = "No Space";
                return true;
            }
            if (Settings.MoveModeCancelsSprinting.Value && LITSession.Instance.Player.Physical.Sprinting)
            {
                reason = "Sprinting";
                return true;
            }

            reason = "";
            return false;
        }

        private void RotationProcess()
        {
            Vector3 rotation = Vector3.zero;

            int xInversion = Settings.InvertHorizontalRotation.Value ? 1 : -1;
            int yInversion = Settings.InvertVerticalRotation.Value ? -1 : 1;

            Vector3 xAxis = UI.RotationReference == ESpaceReference.Player
                ? _cameraRotation * Vector3.right
                : Vector3.right;

            Vector3 zAxis = UI.RotationReference == ESpaceReference.Player
                ? _cameraRotation * Vector3.forward
                : Vector3.forward;

            Vector3 yAxis = Vector3.up;

            Space space = UI.RotationReference == ESpaceReference.Item
                ? Space.Self   // if reference is the item itself, use it's own local space
                : Space.World; // if reference is the player, or the world, use world space

            // X axis
            if (LMBDown && UI.RotTab.LockX.isOn == false)
            {
                _targetTransform.Rotate(xAxis, yInversion * _mouseY * _mouseRotationSpeedMultipier, space);
            }

            // Y axis
            if (LMBDown && UI.RotTab.LockY.isOn == false)
            {
                _targetTransform.Rotate(yAxis, xInversion * _mouseX * _mouseRotationSpeedMultipier, space);
            }

            // Z axis
            if (_scrollInput != 0 && UI.RotTab.LockZ.isOn == false)
            {
                _targetTransform.Rotate(zAxis, _scrollInput * _scrollRotationSpeedMultiplier, space);
            }
        }
        private void PositionProcess()
        {
            if (TryBothMouseButtonItemHoldReturnsSuccess() == false)
            {
                MouseDragPositionProcess();
            }
        }

        private bool TryBothMouseButtonItemHoldReturnsSuccess()
        {
            // this code is terrible... it is effectivly mimicking the old translation mode so that when you hold both mouse buttons and rotate your camera its less brainfuck
            if (LMBDown && RMBDown)
            {
                // set parent to player and cache rotation to cancel it out only on the first frame that both are held down
                if (Target.gameObject.transform.parent == null)
                {
                    _targetCachedRotation = Target.gameObject.transform.rotation;
                    Target.gameObject.transform.SetParent(LITSession.Instance.Player.CameraContainer.gameObject.transform);
                }

                // set rotation back to cached rotation to cancel it
                Target.gameObject.transform.rotation = _targetCachedRotation;

                // return because we don't want to apply any translation in this case
                return true;
            }
            // set parent back to null in the case that either mouse button is UP and the parent is still the player camera
            else if ((!LMBDown || !RMBDown) && Target.gameObject.transform.parent != null)
            {
                Target.gameObject.transform.SetParent(null);
            }

            return false;
        }

        private void MouseDragPositionProcess()
        {
            Vector3 translation = Vector3.zero;

            Vector3 xAxis = UI.RepositionReference == ESpaceReference.Player
                ? _cameraRotation * Vector3.right
                : Vector3.right;

            Vector3 zAxis = UI.RepositionReference == ESpaceReference.Player
                ? _cameraRotation * Vector3.forward
                : Vector3.forward;

            Vector3 yAxis = Vector3.up;

            if (LMBDown)
            {
                translation += new Vector3(PlayerMovementDelta.x, 0, PlayerMovementDelta.z);
            }

            if (LMBDown && UI.PosTab.LockX.isOn == false)
            {
                translation += xAxis * _mouseX * _mouseRepositionSpeedMultiplier;
            }

            if (LMBDown && UI.PosTab.LockY.isOn == false)
            {
                translation += yAxis * _mouseY * _mouseRepositionSpeedMultiplier;
            }

            if (_scrollInput != 0 && UI.PosTab.LockZ.isOn == false)
            {
                translation += zAxis * _scrollInput * _scrollRepositionSpeedMultiplier;
            }

            Space space = UI.RepositionReference == ESpaceReference.Item
                ? Space.Self   // if reference is the item itself, use it's own local space
                : Space.World; // if reference is the player, or the world, use world space

            if (UI.RepositionReference == ESpaceReference.Item)
            {
                // if reference is the item itself, use it's own local space
                space = Space.Self;
            }
            else
            {
                // if reference is the player, or the world, use world space
                space = Space.World;
            }

            _targetTransform.Translate(translation, space);
        }

        private void PhysicsProcess()
        {
            if (LMBDown && TargetMoveable.PhysicsIsEnabled == false)
            {
                TargetMoveable.SetPhysicsEnabled(true, false);
            }

            if (LMBDown == false && TargetMoveable.PhysicsIsEnabled == true)
            {
                TargetMoveable.DisablePhysics();
            }
        }

        public class EnterMoveModeInteraction(FakeItem fakeItem) : CustomInteraction(fakeItem)
        {
            public override string Name
            {
                get
                {
                    if (MoveModeDisallowed(FakeItem, out string reason))
                    {
                        return $"Move: {reason}";
                    }
                    else
                    {
                        return "Move";
                    }
                }
            }

            public override bool Enabled => !MoveModeDisallowed(FakeItem, out _);

            public override void OnInteract()
            {
                Instance.Enable(FakeItem);
            }
        }
    }
}
