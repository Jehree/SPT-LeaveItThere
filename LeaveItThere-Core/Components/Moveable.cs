using LeaveItThere.Helpers;
using UnityEngine;

namespace LeaveItThere.Components
{
    public class Moveable : MonoBehaviour
    {
        public Rigidbody Rigidbody { get; private set; }
        public bool PhysicsIsEnabled { get => !Rigidbody.isKinematic; }

        private int _frameCountBeforePausable = 0;
        private bool _pausable = true;

        private void Awake()
        {
            Rigidbody = gameObject.GetOrAddComponent<Rigidbody>();
            EFTPhysicsClass.GClass723.SupportRigidbody(Rigidbody);
            DisablePhysics();
        }

        private void FixedUpdate()
        {
            if (_pausable)
            {
                TryPausePhysics();
            }
        }

        private void TryPausePhysics()
        {
            _frameCountBeforePausable++;
            if (_frameCountBeforePausable < Settings.FramesToWakeUpPhysicsObject.Value)
            {
                return;
            }

            if (
                Rigidbody.velocity.sqrMagnitude < Settings.RigidbodySleepThreshold.Value &&
                Rigidbody.angularVelocity.sqrMagnitude < Settings.RigidbodySleepThreshold.Value
            )
            {
                _frameCountBeforePausable = 0;
                DisablePhysics();
            }
        }

        public void SetPhysicsEnabled(bool enabled, bool pausable = true)
        {
            if (enabled)
            {
                EnablePhysics(pausable);
            }
            else
            {
                DisablePhysics();
            }
        }

        public void EnablePhysics(bool pausable)
        {
            enabled = true;
            _pausable = pausable;
            Rigidbody.isKinematic = false;
            Rigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;
        }

        public void DisablePhysics()
        {
            Rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            Rigidbody.isKinematic = true;
            enabled = false;
        }

        public void MoveToPlayer()
        {
            gameObject.transform.position = LITUtils.PlayerFront;
        }

        public void ResetRotation()
        {
            SetRotation(Quaternion.identity);
        }

        public void SetRotation(Quaternion rotation)
        {
            gameObject.transform.rotation = rotation;
        }
    }
}
