using InteractableInteractionsAPI.Common;
using LeaveItThere.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LeaveItThere.Components
{
    internal class MoveableObject : MonoBehaviour
    {
        private ActionsReturnClass _moveMenu;
        private Action<MoveableObject> _exitMenuCallback;
        private Action<MoveableObject> _moveModeActiveUpdateCallback;

        public bool MoveModeActive = false;
        public bool IsInitialized { get; private set; } = false;
        private bool _translationModeEnabled = false;
        private bool _rotationModeEnabled = false;
        private bool _physicsEnabled = false;

        private Quaternion _lockedRotation;
        private Vector3 _lockedPosition;

        public void Init(Action<MoveableObject> exitMenuCallback, Action<MoveableObject> moveModeActiveUpdateCallback)
        {
            var rigidBody = gameObject.GetOrAddComponent<Rigidbody>();
            EFTPhysicsClass.GClass712.SupportRigidbody(rigidBody);
            rigidBody.isKinematic = true;
            var interactions = new List<ActionsTypesClass>();
            interactions.Add(GetToggleTranslationModeAction().GetActionsTypesClass());
            interactions.Add(GetToggleRotationModeAction().GetActionsTypesClass());
            interactions.Add(GetTogglePhysicsAction().GetActionsTypesClass());
            interactions.Add(GetMoveToPlayerAction().GetActionsTypesClass());
            interactions.Add(GetExitMoveModeAction().GetActionsTypesClass());

            _moveMenu = new ActionsReturnClass { Actions = interactions };
            IsInitialized = true;
            _exitMenuCallback = exitMenuCallback;
            _moveModeActiveUpdateCallback = moveModeActiveUpdateCallback;
        }

        public void Update()
        {
            if (!MoveModeActive) return;
            if (_moveModeActiveUpdateCallback != null) _moveModeActiveUpdateCallback(this);
            if (!MoveModeActive) return; //checking this again is necessary in case the callback changes it

            if (_translationModeEnabled)
            {
                TranslationModeActive();
            }
            if (_rotationModeEnabled)
            {
                RotationModeActive();
            }

            var session = ModSession.GetSession();
            if (session.GamePlayerOwner.AvailableInteractionState.Value == _moveMenu) return;
            session.GamePlayerOwner.AvailableInteractionState.Value = _moveMenu;
        }

        /// <param name="exitMenuCallback">Called once when move mode is exited.</param>
        /// <param name="moveModeActiveUpdateCallback">Called every frame that move mode is active, return false to force exit move mode.</param>
        public void EnterMoveMode(Action<MoveableObject> exitMenuCallback = null, Action<MoveableObject> moveModeActiveUpdateCallback = null)
        {
            if (!IsInitialized)
            {
                Init(exitMenuCallback, moveModeActiveUpdateCallback);
            }
            MoveModeActive = true;
        }

        public void RotationModeActive()
        {
            gameObject.transform.position = _lockedPosition;
        }

        public void TranslationModeActive()
        {
            gameObject.transform.rotation = _lockedRotation;
        }

        public CustomInteraction GetExitMoveModeAction()
        {
            return new CustomInteraction(
                "Exit Move Menu",
                false,
                ExitMoveMode
            );
        }
        public CustomInteraction GetTogglePhysicsAction()
        {
            return new CustomInteraction(
                "Toggle Physics",
                false,
                () =>
                {
                    _translationModeEnabled = false;
                    _rotationModeEnabled = false;
                    SetPhysicsEnabled(!PhysicsIsEnabled());

                    InteractionHelper.RefreshPrompt();
                    InteractionHelper.NotificationLong($"Physics enabled: {PhysicsIsEnabled()}");
                }
            );
        }

        public CustomInteraction GetToggleTranslationModeAction()
        {
            return new CustomInteraction(
                "Toggle Translation Mode",
                false,
                () =>
                {
                    SetPhysicsEnabled(false);
                    _rotationModeEnabled = false;

                    var objectTransform = gameObject.transform;
                    var cameraTransform = ModSession.GetSession().Player.CameraContainer.gameObject.transform;
                    _translationModeEnabled = !_translationModeEnabled;
                    if (_translationModeEnabled)
                    {
                        _lockedRotation = gameObject.transform.rotation;
                        objectTransform.parent = cameraTransform;
                        ItemHelper.SetItemColor(Color.green, gameObject);
                    }
                    else
                    {
                        objectTransform.parent = null;
                        ItemHelper.SetItemColor(Settings.PlacedItemTint.Value, gameObject);
                    }

                    InteractionHelper.RefreshPrompt();
                    InteractionHelper.NotificationLong($"Translation mode enabled: {_translationModeEnabled}");
                }
            );
        }

        public CustomInteraction GetToggleRotationModeAction()
        {
            return new CustomInteraction(
                "Toggle Rotation Mode",
                false,
                () =>
                {
                    SetPhysicsEnabled(false);
                    _translationModeEnabled = false;

                    var objectTransform = gameObject.transform;
                    var cameraTransform = ModSession.GetSession().Player.CameraContainer.gameObject.transform;
                    _rotationModeEnabled = !_rotationModeEnabled;
                    if (_rotationModeEnabled)
                    {
                        _lockedPosition = gameObject.transform.position;
                        objectTransform.parent = cameraTransform;
                        ItemHelper.SetItemColor(Color.red, gameObject);
                    }
                    else
                    {
                        objectTransform.parent = null;
                        ItemHelper.SetItemColor(Settings.PlacedItemTint.Value, gameObject);
                    }

                    InteractionHelper.RefreshPrompt();
                    InteractionHelper.NotificationLong($"Rotation mode enabled: {_rotationModeEnabled}");
                }
            );
        }

        public CustomInteraction GetMoveToPlayerAction()
        {
            return new CustomInteraction(
                "Move To Player Feet",
                false,
                () =>
                {
                    gameObject.transform.position = ModSession.GetSession().Player.Transform.position;
                    SetPhysicsEnabled(false);
                    _translationModeEnabled = false;
                    _rotationModeEnabled = false;

                    InteractionHelper.RefreshPrompt();
                    InteractionHelper.NotificationLong("Moved to player feet.");
                }
            );
        }

        public void ExitMoveMode()
        {
            MoveModeActive = false;
            SetPhysicsEnabled(false);
            _translationModeEnabled = false;
            _rotationModeEnabled = false;

            InteractionHelper.RefreshPrompt(true);
            if (_exitMenuCallback != null) _exitMenuCallback(this);
        }

        public void SetPhysicsEnabled(bool enabled)
        {
            var rigidBody = gameObject.GetComponent<Rigidbody>();
            rigidBody.isKinematic = !enabled;
            gameObject.transform.parent = null;

            if (enabled)
            {
                ItemHelper.SetItemColor(Color.magenta, gameObject);
            }
            else
            {
                ItemHelper.SetItemColor(Settings.PlacedItemTint.Value, gameObject);
            }
        }

        public bool PhysicsIsEnabled()
        {
            return !gameObject.GetComponent<Rigidbody>().isKinematic;
        }
    }
}
