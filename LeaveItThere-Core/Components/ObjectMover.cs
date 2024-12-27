using EFT;
using EFT.Interactive;
using EFT.InventoryLogic;
using InteractableInteractionsAPI.Common;
using LeaveItThere.Helpers;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace LeaveItThere.Components
{
    internal class ObjectMover : MonoBehaviour
    {
        public bool Enabled { get; private set; } = false;
        public GameObject Target { get; private set; } = null;

        private ActionsReturnClass _moveMenu;
        private Action<GameObject, bool> _disabledCallback;
        private Action<GameObject> _enabledUpdateCallback;

        private bool _translationModeEnabled = false;
        private bool _rotationModeEnabled = false;
        private bool _physicsEnabled = false;

        private Quaternion _lockedRotation;
        private Vector3 _lockedPosition;

        public static ObjectMover GetMover()
        {
            return ModSession.GetSession().Player.gameObject.GetComponent<ObjectMover>();
        }

        public static void CreateNewObjectMover()
        {
            ModSession.GetSession().Player.gameObject.AddComponent<ObjectMover>();
        }

        /// <param name="exitMenuCallback">Called once when move mode is exited.</param>
        /// <param name="moveModeActiveUpdateCallback">Called every frame that move mode is active, return false to force exit move mode.</param>
        public void Enable(GameObject target, Action<GameObject, bool> disabledCallback, Action<GameObject> enabledUpdateCallback)
        {
            if (Enabled)
            {
                Disable(true);
            }

            Target = target;
            _disabledCallback = disabledCallback;
            _enabledUpdateCallback = enabledUpdateCallback;

            var rigidBody = Target.GetOrAddComponent<Rigidbody>();
            EFTPhysicsClass.GClass712.SupportRigidbody(rigidBody);
            rigidBody.isKinematic = true;
            Enabled = true;
        }

        public void Disable(bool save)
        {
            Enabled = false;
            _translationModeEnabled = false;
            _rotationModeEnabled = false;
            Target.transform.parent = null;

            InteractionHelper.RefreshPrompt(true);
            ItemHelper.SetItemColor(Settings.PlacedItemTint.Value, Target);
            if (_disabledCallback != null) _disabledCallback(Target, save);

            if (Settings.ImmersivePhysics.Value)
            {
                SetPhysicsEnabled(true, false);
            }
            else
            {
                SetPhysicsEnabled(false);
            }
        }

        public void Awake()
        {
            var interactions = new List<ActionsTypesClass>();
            interactions.Add(GetToggleTranslationModeAction().GetActionsTypesClass());
            interactions.Add(GetToggleRotationModeAction().GetActionsTypesClass());
            interactions.Add(GetTogglePhysicsAction().GetActionsTypesClass());
            interactions.Add(GetMoveToPlayerAction().GetActionsTypesClass());
            interactions.Add(GetSaveAndExitMoveModeAction().GetActionsTypesClass());
            interactions.Add(GetCancelAndExitMoveModeAction().GetActionsTypesClass());

            _moveMenu = new ActionsReturnClass { Actions = interactions };
        }

        public void Update()
        {
            if (!Enabled) return;
            if (_enabledUpdateCallback != null) _enabledUpdateCallback(Target);
            if (!Enabled) return; //checking this again is necessary in case the callback changes it

            if (_translationModeEnabled)
            {
                LockRotation();
            }
            if (_rotationModeEnabled)
            {
                LockPosition();
            }

            var session = ModSession.GetSession();
            if (session.GamePlayerOwner.AvailableInteractionState.Value == _moveMenu) return;
            session.GamePlayerOwner.AvailableInteractionState.Value = _moveMenu;
        }

        public void LockPosition()
        {
            Target.transform.position = _lockedPosition;
        }

        public void LockRotation()
        {
            Target.transform.rotation = _lockedRotation;
        }

        public void SetPhysicsEnabled(bool enabled, bool colorChange = true)
        {
            var rigidBody = Target.GetComponent<Rigidbody>();
            if (rigidBody == null)
            {
                InteractionHelper.NotificationLongWarning("This game object has no rigidbody!");
            }
            rigidBody.isKinematic = !enabled;
            Target.transform.parent = null;

            if (enabled && colorChange)
            {
                ItemHelper.SetItemColor(Color.magenta, Target);
            }
            else
            {
                ItemHelper.SetItemColor(Settings.PlacedItemTint.Value, Target);
            }
        }

        public bool PhysicsIsEnabled()
        {
            var rigidBody = Target.GetComponent<Rigidbody>();
            if (rigidBody == null)
            {
                return false;
            }
            return !rigidBody.isKinematic;
        }

        public static CustomInteraction GetSaveAndExitMoveModeAction()
        {
            return new CustomInteraction(
                "Save",
                false,
                () =>
                {
                    var mover = ObjectMover.GetMover();
                    mover.Disable(true);
                }
            );
        }

        public static CustomInteraction GetCancelAndExitMoveModeAction()
        {
            return new CustomInteraction(
                "Cancel",
                false,
                () =>
                {
                    var mover = ObjectMover.GetMover();
                    mover.Disable(false);
                }
            );
        }

        public static CustomInteraction GetTogglePhysicsAction()
        {
            return new CustomInteraction(
                "Toggle Physics",
                false,
                () =>
                {
                    var mover = ObjectMover.GetMover();

                    mover._translationModeEnabled = false;
                    mover._rotationModeEnabled = false;
                    mover.SetPhysicsEnabled(!mover.PhysicsIsEnabled());

                    InteractionHelper.RefreshPrompt();
                    InteractionHelper.NotificationLong($"Physics enabled: {mover.PhysicsIsEnabled()}");
                }
            );
        }

        public static CustomInteraction GetToggleTranslationModeAction()
        {
            return new CustomInteraction(
                "Toggle Translation Mode",
                false,
                () =>
                {
                    var mover = ObjectMover.GetMover();

                    mover.SetPhysicsEnabled(false);
                    mover._rotationModeEnabled = false;

                    var targetTransform = mover.Target.transform;
                    var cameraTransform = ModSession.GetSession().Player.CameraContainer.gameObject.transform;
                    mover._translationModeEnabled = !mover._translationModeEnabled;
                    if (mover._translationModeEnabled)
                    {
                        mover._lockedRotation = mover.Target.transform.rotation;
                        targetTransform.parent = cameraTransform;
                        ItemHelper.SetItemColor(Color.green, mover.Target);
                    }
                    else
                    {
                        targetTransform.parent = null;
                        ItemHelper.SetItemColor(Settings.PlacedItemTint.Value, mover.Target);
                    }

                    InteractionHelper.RefreshPrompt();
                    InteractionHelper.NotificationLong($"Translation mode enabled: {mover._translationModeEnabled}");
                }
            );
        }

        public static CustomInteraction GetToggleRotationModeAction()
        {
            return new CustomInteraction(
                "Toggle Rotation Mode",
                false,
                () =>
                {
                    var mover = ObjectMover.GetMover();

                    mover.SetPhysicsEnabled(false);
                    mover._translationModeEnabled = false;

                    var targetTransform = mover.Target.transform;
                    var cameraTransform = ModSession.GetSession().Player.CameraContainer.gameObject.transform;
                    mover._rotationModeEnabled = !mover._rotationModeEnabled;
                    if (mover._rotationModeEnabled)
                    {
                        mover._lockedPosition = mover.Target.transform.position;
                        targetTransform.parent = cameraTransform;
                        ItemHelper.SetItemColor(Color.red, mover.Target);
                    }
                    else
                    {
                        targetTransform.parent = null;
                        ItemHelper.SetItemColor(Settings.PlacedItemTint.Value, mover.Target);
                    }

                    InteractionHelper.RefreshPrompt();
                    InteractionHelper.NotificationLong($"Rotation mode enabled: {mover._rotationModeEnabled}");
                }
            );
        }

        public static CustomInteraction GetMoveToPlayerAction()
        {
            return new CustomInteraction(
                "Move To Player",
                false,
                () =>
                {
                    var mover = ObjectMover.GetMover();
                    //Vector3 playerPosition = ModSession.GetSession().Player.Transform.position;
                    Player player = ModSession.GetSession().Player;
                    mover.Target.transform.position = player.Transform.Original.position + player.Transform.Original.forward + (player.Transform.Original.up / 2);
                    mover.SetPhysicsEnabled(false);
                    mover._translationModeEnabled = false;
                    mover._rotationModeEnabled = false;

                    InteractionHelper.RefreshPrompt();
                    InteractionHelper.NotificationLong("Moved item to player");
                }
            );
        }
    }
}
