using LeaveItThere.Helpers;
using UnityEngine;

namespace LeaveItThere.Components
{
    public class MoveableObject : MonoBehaviour
    {
        private Rigidbody _rigidbody = null;

        public bool PhysicsIsEnabled
        {
            get
            {
                return !Rigidbody.isKinematic;
            }
        }

        public Rigidbody Rigidbody
        {
            get
            {
                if (_rigidbody == null)
                {
                    _rigidbody = gameObject.GetOrAddComponent<Rigidbody>();
                }
                return _rigidbody;
            }
        }

        private int _frameCounter = 0;

        private void Awake()
        {
            _rigidbody = gameObject.GetOrAddComponent<Rigidbody>();
            DisablePhysics();
            EFTPhysicsClass.GClass712.SupportRigidbody(Rigidbody);
        }

        private void FixedUpdate()
        {
            PhysicsProcess();
        }

        private void PhysicsProcess()
        {
            if (!PhysicsIsEnabled) return;

            _frameCounter++;
            if (_frameCounter < Settings.FramesToWakeUpPhysicsObject.Value)
            {
                return;
            }

            if (
                Rigidbody.velocity.sqrMagnitude < Settings.RigidbodySleepThreshold.Value &&
                Rigidbody.angularVelocity.sqrMagnitude < Settings.RigidbodySleepThreshold.Value
            )
            {
                _frameCounter = 0;
                DisablePhysics();
            }
        }

        public void SetPhysicsEnabled(bool enabled)
        {
            if (enabled)
            {
                EnablePhysics();
            }
            else
            {
                DisablePhysics();
            }
        }

        /// <summary>
        /// Allows object to interact with physics systems. Will automatically re-disable after sleep threshold is met.
        /// </summary>
        public void EnablePhysics()
        {
            _frameCounter = 0;
            Rigidbody.isKinematic = false;
            Rigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;
        }

        /// <summary>
        /// Disables physics, making object kinematic.
        /// </summary>
        public void DisablePhysics()
        {
            Rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            Rigidbody.isKinematic = true;
        }

        public void MoveToPlayer()
        {
            gameObject.transform.position = LITUtils.PlayerFront;
        }
    }
}
