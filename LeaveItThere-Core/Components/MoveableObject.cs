using EFT;
using EFT.UI;
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

        public void Awake()
        {
            _rigidbody = gameObject.GetOrAddComponent<Rigidbody>();
            EFTPhysicsClass.GClass712.SupportRigidbody(Rigidbody);
            DisablePhysics();
        }

        public void Update()
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
                Rigidbody.velocity.sqrMagnitude < Settings.RigidbodySleepThreshold.Value * Settings.RigidbodySleepThreshold.Value &&
                Rigidbody.angularVelocity.sqrMagnitude < Settings.RigidbodySleepThreshold.Value * Settings.RigidbodySleepThreshold.Value
            )
            {
                _frameCounter = 0;
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
        }

        /// <summary>
        /// Disables physics, making object kinematic.
        /// </summary>
        public void DisablePhysics()
        {
            Rigidbody.isKinematic = true;
        }

        public void MoveToPlayer()
        {
            gameObject.transform.position = Utils.PlayerFront;
        }
    }
}
