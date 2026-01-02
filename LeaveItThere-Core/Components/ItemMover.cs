using EFT.InputSystem;
using Helpers.CursorHelper;
using LeaveItThere.Common;
using LeaveItThere.Fika;
using LeaveItThere.Helpers;
using LeaveItThere.ModSettings;
using LeaveItThere.UI;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using static LeaveItThere.UI.MoveModeUI;

namespace LeaveItThere.Components;

internal class ItemMover : MonoBehaviour
{
    #region Singleton Setup
    private static ItemMover _instance = null;
    public static ItemMover Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = RaidSession.Instance.Player.gameObject.GetOrAddComponent<ItemMover>();
            }

            return _instance;
        }
    }
    #endregion

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
                _targetCachedRotation = Target.gameObject.transform.rotation;
            }
        }
    }
    #endregion

    #region Cached Target Values
    private Vector3 _undoPosition;
    private Quaternion _undoRotation;
    private Quaternion _targetCachedRotation;
    #endregion

    #region Input + Config Settings
    private bool _lmbDown = false;
    public bool LMBDown => _lmbDown;

    private void HandleLMBPressedStatusChange(bool isDown)
    {
        if (isDown)
        {
            // cursor ALWAYS HIDDEN WHEN LMB IS DOWN
            CursorHelper.ReturnCursorControlToEFT();
        }
        else if (!RMBDown)
        {
            // cursor is ONLY UNHIDDEN when BOTH mouse buttons are UP
            CursorHelper.ForceUnlockCursor();
        }

        _lmbDown = isDown;
    }

    private bool _rmbDown = false;
    public bool RMBDown => _rmbDown;

    public void HandleRMBPressedStatusChange(bool isDown)
    {
        // handling for cursor and camera separated below because it is kinda confusing

        // CURSOR HANDLING:
        if (isDown)
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
        if (isDown)
        {
            // camera locking and unlocking is ONLY controlled by RMB
            InteractionHelper.SetCameraRotationLocked(false);
        }
        else
        {
            // camera locking and unlocking is ONLY controlled by RMB
            InteractionHelper.SetCameraRotationLocked(true);
        }

        _rmbDown = isDown;
    }

    private float MouseX => Input.GetAxis("Mouse X");
    private float MouseY => Input.GetAxis("Mouse Y");
    private float ScrollInputAxis => Input.GetAxis("Mouse ScrollWheel");
    private float PrecisionMultiplier => Settings.PrecisionKey.Value.IsPressed() ? Settings.PrecisionMultiplier.Value : 1;
    private float MouseRepositionSpeedMultiplier => Settings.RepositionSpeed.Value * PrecisionMultiplier;
    private float ScrollRepositionSpeedMultiplier => Settings.RepositionScrollSpeed.Value * PrecisionMultiplier;
    private float MouseRotationSpeedMultipier => Settings.RotationSpeed.Value * PrecisionMultiplier;
    private float ScrollRotationSpeedMultiplier => Settings.RotationScrollSpeed.Value * PrecisionMultiplier;
    #endregion

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
        _playerPositionLastFrame = RaidSession.Instance.Player.Transform.position;
    }
    private Vector3 _playerPositionLastFrame;

    /// <summary>
    /// Will only be correct while ItemMover is enabled
    /// </summary>
    public Vector3 PlayerMovementDelta => RaidSession.Instance.Player.Transform.position - _playerPositionLastFrame;

    private Quaternion CameraRotation => Quaternion.LookRotation(LITUtils.CameraForward);
    #endregion

    #region UI Helper Getters
    public MoveModeUI UI => MoveModeUI.Instance;
    public ETabType CurrentMode => UI.SelectedTabType;
    public bool WillFloat => UI.PhysTab.ItemFloats.isOn;
    #endregion

    #region UI Event Subscriptions
    private void OnMovedToPlayerClicked() => Target.Moveable.MoveToPlayer();
    private void OnUndoMoveClicked() => Target.gameObject.transform.position = _undoPosition;
    private void OnResetRotationClicked() => Target.gameObject.transform.rotation = Quaternion.identity;
    private void OnUndoRotationClicked() => Target.gameObject.transform.rotation = _undoRotation;
    private void OnSaveButtonClicked() => Disable(true);
    private void OnCancelButtonClicked() => Disable(false);
    private void OnTabSwitched(ETabType tabType)
    {
        if (tabType == ETabType.Position)
        {
            Target.Moveable.DisablePhysics();
            ItemHelper.SetItemColor(Color.green, Target.gameObject);
        }
        else if (tabType == ETabType.Rotation)
        {
            Target.Moveable.DisablePhysics();
            ItemHelper.SetItemColor(Color.red, Target.gameObject);
        }
        else if (tabType == ETabType.Physics)
        {
            ItemHelper.SetItemColor(Color.magenta, Target.gameObject);
        }
    }
    #endregion

    #region Init
    public ItemMover()
    {
        enabled = false;
    }

    internal void Awake()
    {
        UI.TabSwitched += OnTabSwitched;

        UI.PosTab.MoveToPlayerButton.onClick.AddListener(OnMovedToPlayerClicked);
        UI.PosTab.UndoMoveButton.onClick.AddListener(OnUndoMoveClicked);

        UI.RotTab.ResetRotationButton.onClick.AddListener(OnResetRotationClicked);
        UI.RotTab.UndoRotationButton.onClick.AddListener(OnUndoRotationClicked);

        UI.SaveButton.onClick.AddListener(OnSaveButtonClicked);
        UI.CancelButton.onClick.AddListener(OnCancelButtonClicked);
    }

    internal void OnEnable()
    {
        UpdatePlayerLastFrameDeltas();
        UI.PhysTab.ItemFloats.isOn = !Settings.ImmersivePhysics.Value;
        UI.ChangeTabs<PositionTab>();
        ItemHelper.SetItemColor(Color.green, Target.gameObject);
    }
    #endregion

    #region Process
    internal void Update()
    {
        if (MoveModeDisallowed(Target, out string reason))
        {
            Disable(true);
            InteractionHelper.NotificationError($"Move Mode cancelled! Reason: {reason}");
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
            HandleLMBPressedStatusChange(true);
        }

        if (Input.GetMouseButtonUp(0))
        {
            HandleLMBPressedStatusChange(false);
        }

        if (Input.GetMouseButtonDown(1))
        {
            HandleRMBPressedStatusChange(true);
        }

        if (Input.GetMouseButtonUp(1))
        {
            HandleRMBPressedStatusChange(false);
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

    private void RotationProcess()
    {
        int xInversion = Settings.InvertHorizontalRotation.Value ? 1 : -1;
        int yInversion = Settings.InvertVerticalRotation.Value ? -1 : 1;

        Vector3 xAxis = UI.RotationReference == ESpaceReference.Player
            ? CameraRotation * Vector3.right
            : Vector3.right;

        Vector3 zAxis = UI.RotationReference == ESpaceReference.Player
            ? CameraRotation * Vector3.forward
            : Vector3.forward;

        Vector3 yAxis = Vector3.up;

        Space space = UI.RotationReference == ESpaceReference.Item
            ? Space.Self   // if reference is the item itself, use it's own local space
            : Space.World; // if reference is the player, or the world, use world space

        // X axis
        if (LMBDown && UI.RotTab.LockX.isOn == false)
        {
            Target.gameObject.transform.Rotate(xAxis, yInversion * MouseY * MouseRotationSpeedMultipier, space);
        }

        // Y axis
        if (LMBDown && UI.RotTab.LockY.isOn == false)
        {
            Target.gameObject.transform.Rotate(yAxis, xInversion * MouseX * MouseRotationSpeedMultipier, space);
        }

        // Z axis
        if (ScrollInputAxis != 0 && UI.RotTab.LockZ.isOn == false)
        {
            Target.gameObject.transform.Rotate(zAxis, ScrollInputAxis * ScrollRotationSpeedMultiplier, space);
        }
    }
    private void PositionProcess()
    {
        if (TryBothMouseButtonItemHoldReturnsSuccess() == false)
        {
            MouseDragPositionProcess();
        }
    }

    private void MouseDragPositionProcess()
    {
        Vector3 translation = Vector3.zero;

        Vector3 xAxis = UI.RepositionReference == ESpaceReference.Player
            ? CameraRotation * Vector3.right
            : Vector3.right;

        Vector3 zAxis = UI.RepositionReference == ESpaceReference.Player
            ? CameraRotation * Vector3.forward
            : Vector3.forward;

        Vector3 yAxis = Vector3.up;

        if (LMBDown)
        {
            translation += new Vector3(PlayerMovementDelta.x, 0, PlayerMovementDelta.z);
        }

        if (LMBDown && UI.PosTab.LockX.isOn == false)
        {
            translation += xAxis * MouseX * MouseRepositionSpeedMultiplier;
        }

        if (LMBDown && UI.PosTab.LockY.isOn == false)
        {
            translation += yAxis * MouseY * MouseRepositionSpeedMultiplier;
        }

        if (ScrollInputAxis != 0 && UI.PosTab.LockZ.isOn == false)
        {
            translation += zAxis * ScrollInputAxis * ScrollRepositionSpeedMultiplier;
        }

        Space space = UI.RepositionReference == ESpaceReference.Item
            ? Space.Self   // if reference is the item itself, use it's own local space
            : Space.World; // if reference is the player, or the world, use world space

        Target.gameObject.transform.Translate(translation, space);
    }

    private void PhysicsProcess()
    {
        if (LMBDown && Target.Moveable.PhysicsIsEnabled == false)
        {
            Target.Moveable.SetPhysicsEnabled(true, false);
        }

        if (LMBDown == false && Target.Moveable.PhysicsIsEnabled == true)
        {
            Target.Moveable.DisablePhysics();
        }
    }
    #endregion

    #region Enable / Disable
    public void Enable(FakeItem target)
    {
        if (enabled == true) return;

        RaidSession.Instance.InteractionsAllowed = false;
        RaidSession.Instance.GamePlayerOwner.ClearInteractionState();

        UI.gameObject.SetActive(true);

        Target = target;
        _undoPosition = Target.gameObject.transform.position;
        _undoRotation = Target.gameObject.transform.rotation;
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
            Target.Place(Target.gameObject.transform.position, Target.gameObject.transform.rotation);
            FikaBridge.SendPlacedStateChangedPacket(Target, true, WillFloat);
            InteractionHelper.NotificationLong("Placement edit saved!");
        }
        else
        {
            Target.gameObject.transform.position = _undoPosition;
            Target.gameObject.transform.rotation = _undoRotation;
            InteractionHelper.NotificationLongWarning("Move Mode cancelled.");
        }

        if (movementSaved && !WillFloat)
        {
            Target.Moveable.EnablePhysics(true);
        }
        else
        {
            Target.Moveable.DisablePhysics();
        }

        Target.SetPlayerAndBotCollisionEnabled(Settings.PlacedItemsHaveCollision.Value);
        ItemHelper.SetItemColor(Settings.PlacedItemTint.Value, Target.gameObject);
        Target.gameObject.transform.SetParent(null); // so that item unparents if both mouse buttons were held
        Target = null;

        RaidSession.Instance.InteractionsAllowed = true;
        UI.gameObject.SetActive(false);

        HandleLMBPressedStatusChange(false);
        HandleRMBPressedStatusChange(false);

        InteractionHelper.SetCameraRotationLocked(false);
        CursorHelper.ReturnCursorControlToEFT();
        InteractionHelper.SetMostInputsIgnored(false);
        InteractionHelper.RefreshPrompt();
    }
    #endregion

    #region Tests
    public static bool MoveModeDisallowed(FakeItem fakeItem, out string reason)
    {
        /*
        if (fakeItem.Flags.MoveModeDisabled)
        {
            reason = fakeItem.Flags.MoveModeDisabledReason;
            return true;
        }
        */

        if (Settings.MoveModeRequiresInventorySpace.Value && !ItemHelper.ItemCanBePickedUp(fakeItem.LootItem.Item))
        {
            reason = "No Space";
            return true;
        }

        if (Settings.MoveModeCancelsSprinting.Value && RaidSession.Instance.Player.Physical.Sprinting)
        {
            reason = "Sprinting";
            return true;
        }

        reason = "";
        return false;
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
                Target.gameObject.transform.SetParent(RaidSession.Instance.Player.CameraContainer.gameObject.transform);
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
    #endregion

    public class EnterMoveModeInteraction(FakeItem fakeItem) : CustomInteraction
    {
        public override string Name => MoveModeDisallowed(fakeItem, out string reason)
                                            ? $"Move: {reason}"
                                            : "Move";
        public override bool Enabled => !MoveModeDisallowed(fakeItem, out _);

        public override void OnInteract()
        {
            Instance.Enable(fakeItem);
        }
    }
}
