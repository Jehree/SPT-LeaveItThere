using LeaveItThere.Helpers;
using LeaveItThere.ModSettings;
using UnityEngine;

namespace LeaveItThere.Components;

public class PhysicsMoveable : MonoBehaviour
{
    public Rigidbody Rigidbody { get; private set; }
    public bool PhysicsIsEnabled { get => !Rigidbody.isKinematic; }

    public int AttemptedPhysicsPauseCount { get; private set; } = 0;
    public bool Pausable { get; private set; } = true;

    internal void Awake()
    {
        Rigidbody = gameObject.GetOrAddComponent<Rigidbody>();
        EFTPhysicsClass.GClass745.SupportRigidbody(Rigidbody);
        DisablePhysics();
    }

    internal void FixedUpdate()
    {
        if (Pausable)
        {
            TryPausePhysics();
        }
    }

    private void TryPausePhysics()
    {
        AttemptedPhysicsPauseCount++;

        if (AttemptedPhysicsPauseCount < Settings.FramesToWakeUpPhysicsObject.Value) return;

        if (MostlyStill())
        {
            AttemptedPhysicsPauseCount = 0;
            DisablePhysics();
        }
    }

    private bool MostlyStill()
    {
        return Rigidbody.velocity.sqrMagnitude < Settings.RigidbodySleepThreshold.Value &&
               Rigidbody.angularVelocity.sqrMagnitude < Settings.RigidbodySleepThreshold.Value;
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
        Pausable = pausable;
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