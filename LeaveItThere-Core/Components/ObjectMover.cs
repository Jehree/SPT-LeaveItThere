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
        public MoveableObject Target { get; private set; } = null; 

        private ActionsReturnClass _moveMenu;
        private Action<bool> _disabledCallback;
        private Action _enabledUpdateCallback;

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
        public void Enable(MoveableObject target, Action<bool> disabledCallback, Action enabledUpdateCallback)
        {
            if (Enabled)
            {
                Disable(true);
            }

            Target = target;
            _disabledCallback = disabledCallback;
            _enabledUpdateCallback = enabledUpdateCallback;

            Target.DisablePhysics();
            Enabled = true;
        }

        public void Disable(bool save)
        {
            Enabled = false;
            _translationModeEnabled = false;
            _rotationModeEnabled = false;
            Target.transform.parent = null;

            InteractionHelper.RefreshPrompt(true);
            ItemHelper.SetItemColor(Settings.PlacedItemTint.Value, Target.gameObject);
            if (_disabledCallback != null) _disabledCallback(save);

            if (Settings.ImmersivePhysics.Value)
            {
                SetPhysicsModeEnabled(true, false);
            }
            else
            {
                SetPhysicsModeEnabled(false);
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
            if (_enabledUpdateCallback != null) _enabledUpdateCallback();
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
            Target.gameObject.transform.position = _lockedPosition;
        }

        public void LockRotation()
        {
            Target.gameObject.transform.rotation = _lockedRotation;
        }

        public void SetPhysicsModeEnabled(bool enabled, bool colorChange = true)
        {
            Target.gameObject.transform.parent = null;

            if (enabled)
            {
                Target.EnablePhysics();
            }
            else
            {
                Target.DisablePhysics();
            }

            if (enabled && colorChange)
            {
                ItemHelper.SetItemColor(Color.magenta, Target.gameObject);
            }
            else
            {
                ItemHelper.SetItemColor(Settings.PlacedItemTint.Value, Target.gameObject);
            }
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
                    mover.SetPhysicsModeEnabled(!mover.Target.PhysicsIsEnabled);

                    InteractionHelper.RefreshPrompt();
                    InteractionHelper.NotificationLong($"Physics enabled: {mover.Target.PhysicsIsEnabled}");
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

                    mover.SetPhysicsModeEnabled(false);
                    mover._rotationModeEnabled = false;

                    var targetTransform = mover.Target.gameObject.transform;
                    var cameraTransform = ModSession.GetSession().Player.CameraContainer.gameObject.transform;
                    mover._translationModeEnabled = !mover._translationModeEnabled;
                    if (mover._translationModeEnabled)
                    {
                        mover._lockedRotation = mover.Target.gameObject.transform.rotation;
                        targetTransform.parent = cameraTransform;
                        ItemHelper.SetItemColor(Color.green, mover.Target.gameObject);
                    }
                    else
                    {
                        targetTransform.parent = null;
                        ItemHelper.SetItemColor(Settings.PlacedItemTint.Value, mover.Target.gameObject);
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

                    mover.SetPhysicsModeEnabled(false);
                    mover._translationModeEnabled = false;

                    var targetTransform = mover.Target.gameObject.transform;
                    var cameraTransform = ModSession.GetSession().Player.CameraContainer.gameObject.transform;
                    mover._rotationModeEnabled = !mover._rotationModeEnabled;
                    if (mover._rotationModeEnabled)
                    {
                        mover._lockedPosition = mover.Target.gameObject.transform.position;
                        targetTransform.parent = cameraTransform;
                        ItemHelper.SetItemColor(Color.red, mover.Target.gameObject);
                    }
                    else
                    {
                        targetTransform.parent = null;
                        ItemHelper.SetItemColor(Settings.PlacedItemTint.Value, mover.Target.gameObject);
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
                    Player player = ModSession.GetSession().Player;
                    mover.Target.MoveToPlayer();
                    mover.SetPhysicsModeEnabled(false);
                    mover._translationModeEnabled = false;
                    mover._rotationModeEnabled = false;

                    InteractionHelper.RefreshPrompt();
                    InteractionHelper.NotificationLong("Moved item to player");
                }
            );
        }
    }
}
