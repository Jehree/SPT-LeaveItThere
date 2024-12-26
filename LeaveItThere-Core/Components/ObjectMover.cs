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
        private GameObject _target = null;

        private ActionsReturnClass _moveMenu;
        private Action<GameObject> _disabledCallback;
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
        public void Enable(GameObject target, Action<GameObject> disabledCallback, Action<GameObject> enabledUpdateCallback)
        {
            if (Enabled)
            {
                Disable();
            }

            _target = target;
            _disabledCallback = disabledCallback;
            _enabledUpdateCallback = enabledUpdateCallback;

            var rigidBody = _target.GetOrAddComponent<Rigidbody>();
            EFTPhysicsClass.GClass712.SupportRigidbody(rigidBody);
            rigidBody.isKinematic = true;
            Enabled = true;
        }

        public void Disable()
        {
            Enabled = false;
            SetPhysicsEnabled(false);
            _translationModeEnabled = false;
            _rotationModeEnabled = false;

            InteractionHelper.RefreshPrompt(true);
            ItemHelper.SetItemColor(Settings.PlacedItemTint.Value, _target);
            if (_disabledCallback != null) _disabledCallback(_target);
            _target = null;
        }

        public void Awake()
        {
            var interactions = new List<ActionsTypesClass>();
            interactions.Add(GetToggleTranslationModeAction().GetActionsTypesClass());
            interactions.Add(GetToggleRotationModeAction().GetActionsTypesClass());
            interactions.Add(GetTogglePhysicsAction().GetActionsTypesClass());
            interactions.Add(GetMoveToPlayerAction().GetActionsTypesClass());
            interactions.Add(GetExitMoveModeAction().GetActionsTypesClass());

            _moveMenu = new ActionsReturnClass { Actions = interactions };
        }

        public void Update()
        {
            if (!Enabled) return;
            if (_enabledUpdateCallback != null) _enabledUpdateCallback(_target);
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
            _target.transform.position = _lockedPosition;
        }

        public void LockRotation()
        {
            _target.transform.rotation = _lockedRotation;
        }

        public void SetPhysicsEnabled(bool enabled)
        {
            var rigidBody = _target.GetComponent<Rigidbody>();
            if (rigidBody == null)
            {
                InteractionHelper.NotificationLongWarning("This game object has no rigidbody!");
            }
            rigidBody.isKinematic = !enabled;
            _target.transform.parent = null;

            if (enabled)
            {
                ItemHelper.SetItemColor(Color.magenta, _target);
            }
            else
            {
                ItemHelper.SetItemColor(Settings.PlacedItemTint.Value, _target);
            }
        }

        public bool PhysicsIsEnabled()
        {
            var rigidBody = _target.GetComponent<Rigidbody>();
            if (rigidBody == null)
            {
                return false;
            }
            return !rigidBody.isKinematic;
        }

        public static CustomInteraction GetExitMoveModeAction()
        {
            return new CustomInteraction(
                "Exit Move Menu",
                false,
                () =>
                {
                    var mover = ObjectMover.GetMover();
                    mover.Disable();
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

                    var targetTransform = mover._target.transform;
                    var cameraTransform = ModSession.GetSession().Player.CameraContainer.gameObject.transform;
                    mover._translationModeEnabled = !mover._translationModeEnabled;
                    if (mover._translationModeEnabled)
                    {
                        mover._lockedRotation = mover._target.transform.rotation;
                        targetTransform.parent = cameraTransform;
                        ItemHelper.SetItemColor(Color.green, mover._target);
                    }
                    else
                    {
                        targetTransform.parent = null;
                        ItemHelper.SetItemColor(Settings.PlacedItemTint.Value, mover._target);
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

                    var targetTransform = mover._target.transform;
                    var cameraTransform = ModSession.GetSession().Player.CameraContainer.gameObject.transform;
                    mover._rotationModeEnabled = !mover._rotationModeEnabled;
                    if (mover._rotationModeEnabled)
                    {
                        mover._lockedPosition = mover._target.transform.position;
                        targetTransform.parent = cameraTransform;
                        ItemHelper.SetItemColor(Color.red, mover._target);
                    }
                    else
                    {
                        targetTransform.parent = null;
                        ItemHelper.SetItemColor(Settings.PlacedItemTint.Value, mover._target);
                    }

                    InteractionHelper.RefreshPrompt();
                    InteractionHelper.NotificationLong($"Rotation mode enabled: {mover._rotationModeEnabled}");
                }
            );
        }

        public static CustomInteraction GetMoveToPlayerAction()
        {
            return new CustomInteraction(
                "Move To Player Feet",
                false,
                () =>
                {
                    var mover = ObjectMover.GetMover();

                    mover._target.transform.position = ModSession.GetSession().Player.Transform.position;
                    mover.SetPhysicsEnabled(false);
                    mover._translationModeEnabled = false;
                    mover._rotationModeEnabled = false;

                    InteractionHelper.RefreshPrompt();
                    InteractionHelper.NotificationLong("Moved to player feet.");
                }
            );
        }
    }
}
