// =============================================================================
// NeonCity — Vehicles / CarController.cs
// -----------------------------------------------------------------------------
// Wheel-collider based arcade car. Acceleration, braking, steering, drift.
// Designed for an arcade feel like GTA V: forgiving, snappy, fun.
// =============================================================================

using UnityEngine;

namespace NeonCity.Vehicles
{
    [RequireComponent(typeof(Rigidbody))]
    public class CarController : VehicleBase
    {
        public WheelCollider[] frontWheels;
        public WheelCollider[] rearWheels;
        public Transform[]     wheelMeshes;
        public float downforce  = 100f;
        public float driftFactor = 0.7f;

        public override void OnInput(float throttle, float steer, float brake)
        {
            ApplySteering(steer);
            ApplyMotor(throttle);
            ApplyBrake(brake);
            UpdateWheelMeshes();
            rigidbody.AddForce(-transform.up * downforce * rigidbody.velocity.magnitude);
        }

        private void ApplySteering(float steer)
        {
            var sa = steer * steerAngle;
            foreach (var w in frontWheels) w.steerAngle = Mathf.Lerp(w.steerAngle, sa, 0.5f);
            foreach (var w in rearWheels)  w.steerAngle = Mathf.Lerp(w.steerAngle, sa * 0.3f, 0.3f);
        }

        private void ApplyMotor(float throttle)
        {
            if (SpeedKmh > maxSpeed) throttle = 0;
            foreach (var w in rearWheels)
            {
                w.motorTorque = throttle * engineForce * 0.05f;
            }
            foreach (var w in frontWheels)
            {
                w.motorTorque = throttle * engineForce * 0.02f;
            }
        }

        private void ApplyBrake(float brake)
        {
            var bf = brake * brakeForce;
            foreach (var w in frontWheels) w.brakeTorque = bf;
            foreach (var w in rearWheels)  w.brakeTorque = bf;
        }

        private void UpdateWheelMeshes()
        {
            for (int i = 0; i < wheelMeshes.Length; i++)
            {
                WheelCollider col = i < frontWheels.Length ? frontWheels[i]
                              : i < frontWheels.Length + rearWheels.Length ? rearWheels[i - frontWheels.Length]
                              : null;
                if (col == null) continue;
                col.GetWorldPose(out var pos, out var rot);
                wheelMeshes[i].position = pos;
                wheelMeshes[i].rotation = rot;
            }
        }

        public override void FixedUpdate() { /* input applied externally per frame */ }
    }
}