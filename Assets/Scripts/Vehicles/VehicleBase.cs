// =============================================================================
// NeonCity — Vehicles / VehicleBase.cs
// -----------------------------------------------------------------------------
// Base class for cars, bikes, helicopters, boats. All vehicles share a
// physics-based controller using Rigidbody + WheelColliders (or thrusters
// for helis). Player can enter/exit with the Interact key.
//
// The same script handles ground vehicles and air vehicles via the
// MovementMode enum — that lets us reuse the same UI, sound system, damage
// model and networking code.
// =============================================================================

using UnityEngine;
using NeonCity.Combat;

namespace NeonCity.Vehicles
{
    public enum MovementMode { Ground, Air, Sea }

    [RequireComponent(typeof(Rigidbody))]
    public abstract class VehicleBase : MonoBehaviour, IDamageable
    {
        [Header("Stats")]
        public MovementMode mode = MovementMode.Ground;
        public float maxHealth = 600f;
        public float currentHealth;
        public float engineForce = 6000f;
        public float steerAngle  = 30f;
        public float brakeForce  = 8000f;
        public float maxSpeed    = 50f;

        [Header("Seating")]
        public Transform[] driverSeats;
        public Transform[] passengerSeats;

        public bool HasDriver { get; protected set; }
        public float SpeedKmh => rigidbody.velocity.magnitude * 3.6f;

        protected Rigidbody rigidbody;

        public virtual void Awake()
        {
            rigidbody = GetComponent<Rigidbody>();
            currentHealth = maxHealth;
            rigidbody.centerOfMass = new Vector3(0, -0.5f, 0);
        }

        // -------------------------------------------------------------------
        public abstract void OnInput(float throttle, float steer, float brake);

        public virtual void FixedUpdate()
        {
            // Subclasses override for ground / air handling.
        }

        // -------------------------------------------------------------------
        public virtual void Enter(GameObject passenger, int seatIndex = 0)
        {
            if (seatIndex < 0 || seatIndex >= driverSeats.Length) return;
            HasDriver = true;
            passenger.transform.SetParent(driverSeats[seatIndex]);
            passenger.transform.localPosition = Vector3.zero;
            passenger.transform.localRotation = Quaternion.identity;
            var ctrl = passenger.GetComponent<NeonCity.Player.PlayerController>();
            if (ctrl != null) ctrl.enabled = false;
        }

        public virtual void Exit(GameObject passenger)
        {
            passenger.transform.SetParent(null);
            passenger.transform.position += transform.right * 1.5f;
            var ctrl = passenger.GetComponent<NeonCity.Player.PlayerController>();
            if (ctrl != null) ctrl.enabled = true;
            HasDriver = false;
        }

        // -------------------------------------------------------------------
        public virtual void ApplyDamage(float dmg, Vector3 point, Vector3 normal)
        {
            currentHealth -= dmg;
            if (currentHealth <= 0) BlowUp();
        }

        protected virtual void BlowUp()
        {
            // Spawn VFX, ragdoll, drop loot.
            Debug.Log($"[Vehicle] {name} destroyed");
            Destroy(gameObject, 4f);
        }
    }
}