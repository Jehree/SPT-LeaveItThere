using Comfort.Common;
using EFT;
using EFT.UI;
using LeaveItThere.Common;
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

        private static ObjectMover _instance = null;
        public static ObjectMover Instance
        {
            get
            {
                if (_instance == null)
                {
                    CreateNewObjectMover();
                }
                return _instance;
            }
            private set
            {
                _instance = value;
            }
        }

        internal static void CreateNewObjectMover()
        {
            _instance = LITSession.Instance.Player.gameObject.GetOrAddComponent<ObjectMover>();
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
            LITSession.Instance.SetInteractionsEnabled(false);
        }

        public void Disable(bool save)
        {
            Enabled = false;
            _translationModeEnabled = false;
            SetRotationModeEnabled(false);
            Target.transform.parent = null;

            InteractionHelper.RefreshPrompt();
            ItemHelper.SetItemColor(Settings.PlacedItemTint.Value, Target.gameObject);
            if (_disabledCallback != null) _disabledCallback(save);

            if (save && Settings.ImmersivePhysics.Value)
            {
                SetPhysicsModeEnabled(true, false);
            }
            else
            {
                SetPhysicsModeEnabled(false);
            }

            LITSession.Instance.SetInteractionsEnabled(true);
            InteractionHelper.SetCameraRotationLocked(false);
        }

        private void Awake()
        {
            List<ActionsTypesClass> interactions = [];
            interactions.Add(new ToggleTranslationInteraction().GetActionsTypesClass());
            interactions.Add(new ToggleRotationInteraction().GetActionsTypesClass());
            interactions.Add(new TogglePhysicsInteraction().GetActionsTypesClass());
            interactions.Add(new MoveToPlayerInteraction().GetActionsTypesClass());
            interactions.Add(new SaveAndExitMoveModeInteraction().GetActionsTypesClass());
            interactions.Add(new CancelMoveModeInteraction().GetActionsTypesClass());

            _moveMenu = new ActionsReturnClass { Actions = interactions };
        }

        private void Update()
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
                RotationProcess();
            }

            LITSession session = LITSession.Instance;
            if (session.GamePlayerOwner.AvailableInteractionState.Value == _moveMenu) return;
            session.GamePlayerOwner.AvailableInteractionState.Value = _moveMenu;
            Singleton<CommonUI>.Instance.EftBattleUIScreen.ActionPanel.method_2(false);
        }

        private void RotationProcess()
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            int xInversion = Settings.InvertHorizontalRotation.Value ? 1 : -1;
            int yInversion = Settings.InvertVerticalRotation.Value ? 1 : -1;

            Target.gameObject.transform.Rotate(Vector3.up, xInversion * mouseX * Settings.RotationSpeed.Value, Space.World);

            // vertical rotation is relative to player camera
            Vector3 cameraForward = Camera.main.transform.forward;
            cameraForward.y = 0;
            Quaternion cameraRotation = Quaternion.LookRotation(cameraForward);
            Target.gameObject.transform.Rotate(cameraRotation * Vector3.left, yInversion * mouseY * Settings.RotationSpeed.Value, Space.World);
        }

        private void LockRotation()
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

        public void SetRotationModeEnabled(bool enabled)
        {
            _rotationModeEnabled = enabled;
            InteractionHelper.SetCameraRotationLocked(enabled);
        }

        public class SaveAndExitMoveModeInteraction : CustomInteraction
        {
            public override string Name => "Save";
            public override bool AutoPromptRefresh => true;
            public override void OnInteract() => Instance.Disable(true);
        }

        public class CancelMoveModeInteraction : CustomInteraction
        {
            public override string Name => "Cancel";
            public override bool AutoPromptRefresh => true;
            public override void OnInteract() => Instance.Disable(false);
        }

        public class TogglePhysicsInteraction : CustomInteraction
        {
            public override string Name => "Toggle Physics";
            public override void OnInteract()
            {
                Instance._translationModeEnabled = false;
                Instance.SetRotationModeEnabled(false);
                Instance.SetPhysicsModeEnabled(!Instance.Target.PhysicsIsEnabled);

                InteractionHelper.NotificationLong($"Physics enabled: {Instance.Target.PhysicsIsEnabled}");
            }
        }

        public class ToggleTranslationInteraction : CustomInteraction
        {
            public override string Name => "Toggle Translation Mode";
            public override void OnInteract()
            {
                ObjectMover mover = Instance;

                mover.SetPhysicsModeEnabled(false);
                mover.SetRotationModeEnabled(false);

                Transform targetTransform = mover.Target.gameObject.transform;
                Transform cameraTransform = LITSession.Instance.Player.CameraContainer.gameObject.transform;
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

                InteractionHelper.NotificationLong($"Translation mode enabled: {mover._translationModeEnabled}");
            }
        }

        public class ToggleRotationInteraction : CustomInteraction
        {
            public override string Name => "Toggle Rotation Mode";
            public override void OnInteract()
            {
                ObjectMover mover = Instance;

                mover.SetPhysicsModeEnabled(false);
                mover._translationModeEnabled = false;

                Transform targetTransform = mover.Target.gameObject.transform;
                Transform cameraTransform = LITSession.Instance.Player.CameraContainer.gameObject.transform;
                mover.SetRotationModeEnabled(!mover._rotationModeEnabled);
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

                InteractionHelper.NotificationLong($"Rotation mode enabled: {mover._rotationModeEnabled}");
            }

        }

        public class MoveToPlayerInteraction : CustomInteraction
        {
            public override string Name => "Move To Player";
            public override void OnInteract()
            {
                ObjectMover mover = Instance;
                Player player = LITSession.Instance.Player;
                mover.Target.MoveToPlayer();
                mover.SetPhysicsModeEnabled(false);
                mover._translationModeEnabled = false;
                mover.SetRotationModeEnabled(false);

                InteractionHelper.NotificationLong("Moved item to player");
            }
        }
    }
}
