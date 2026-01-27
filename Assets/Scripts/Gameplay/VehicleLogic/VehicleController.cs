using Gameplay.InputLogic;
using UnityEngine;

namespace Gameplay.VehicleLogic
{
    /// <summary>
    /// WheelCollider-based vehicle controller.
    /// Applies motor, steering and braking in FixedUpdate.
    /// Includes a simple anti-roll force to prevent rollovers on sharp steering at speed.
    /// </summary>
    public sealed class VehicleController
    {
        private readonly Rigidbody body;
        private readonly AxleSetup[] axles;

        private float motorTorque;
        private float brakeTorque;
        private float maxSteerAngle;

        private Vector3 centerOfMassOffset;

        private Vector2 moveInput;
        private bool brakeHeld;

        private float frontAntiRollForce;
        private float rearAntiRollForce;

        public VehicleController(Rigidbody body, AxleSetup[] axles)
        {
            this.body = body;
            this.axles = axles;

            motorTorque = 1200f;
            brakeTorque = 3000f;
            maxSteerAngle = 22f;

            centerOfMassOffset = new Vector3(0f, -0.6f, 0f);

            moveInput = Vector2.zero;
            brakeHeld = false;

            frontAntiRollForce = 9000f;
            rearAntiRollForce = 7000f;
        }

        public void ApplySettings(float motorTorque, float brakeTorque, float maxSteerAngle, Vector3 centerOfMassOffset)
        {
            this.motorTorque = motorTorque;
            this.brakeTorque = brakeTorque;
            this.maxSteerAngle = maxSteerAngle;
            this.centerOfMassOffset = centerOfMassOffset;

            if (body != null)
                body.centerOfMass += centerOfMassOffset;
        }

        public void SetAntiRoll(float frontForce, float rearForce)
        {
            frontAntiRollForce = Mathf.Max(0f, frontForce);
            rearAntiRollForce = Mathf.Max(0f, rearForce);
        }

        public void SetInput(in GameplayInput input)
        {
            moveInput = input.Move;
            brakeHeld = input.BrakeHeld;
        }

        public void ClearInput()
        {
            moveInput = Vector2.zero;
            brakeHeld = false;
        }

        public void FixedTick()
        {
            if (axles == null || axles.Length == 0)
                return;

            float steerInput = Mathf.Clamp(moveInput.x, -1f, 1f);
            float throttleInput = Mathf.Clamp(moveInput.y, -1f, 1f);

            float speed = body != null ? body.velocity.magnitude : 0f;

            float steerFactor = ComputeSteerFactor(speed);
            float steerAngle = steerInput * maxSteerAngle * steerFactor;

            float motor = throttleInput * motorTorque;
            float brake = brakeHeld ? brakeTorque : 0f;

            int i = 0;
            while (i < axles.Length)
            {
                AxleSetup axle = axles[i];
                if (axle == null)
                {
                    i++;
                    continue;
                }

                if (axle.Steer)
                {
                    SetSteer(axle.LeftCollider, steerAngle);
                    SetSteer(axle.RightCollider, steerAngle);
                }
                else
                {
                    SetSteer(axle.LeftCollider, 0f);
                    SetSteer(axle.RightCollider, 0f);
                }

                if (axle.Motor)
                {
                    SetMotor(axle.LeftCollider, motor);
                    SetMotor(axle.RightCollider, motor);
                }
                else
                {
                    SetMotor(axle.LeftCollider, 0f);
                    SetMotor(axle.RightCollider, 0f);
                }

                SetBrake(axle.LeftCollider, brake);
                SetBrake(axle.RightCollider, brake);

                i++;
            }

            ApplyAntiRollBars();
        }

        private float ComputeSteerFactor(float speed)
        {
            // Aggressive stability curve:
            // 0-3 m/s  -> full steering
            // 12+ m/s -> 25% steering
            float t = Mathf.InverseLerp(3f, 12f, speed);
            return Mathf.Lerp(1f, 0.25f, t);
        }

        private void ApplyAntiRollBars()
        {
            if (body == null)
                return;

            WheelCollider fl = null;
            WheelCollider fr = null;
            WheelCollider rl = null;
            WheelCollider rr = null;

            int i = 0;
            while (i < axles.Length)
            {
                AxleSetup axle = axles[i];
                if (axle == null)
                {
                    i++;
                    continue;
                }

                bool hasLeft = axle.LeftCollider != null;
                bool hasRight = axle.RightCollider != null;

                if (!hasLeft || !hasRight)
                {
                    i++;
                    continue;
                }

                if (axle.Steer)
                {
                    fl = axle.LeftCollider;
                    fr = axle.RightCollider;
                }
                else
                {
                    rl = axle.LeftCollider;
                    rr = axle.RightCollider;
                }

                i++;
            }

            ApplyAntiRollPair(fl, fr, frontAntiRollForce);
            ApplyAntiRollPair(rl, rr, rearAntiRollForce);
        }

        private void ApplyAntiRollPair(WheelCollider left, WheelCollider right, float antiRollForce)
        {
            if (left == null || right == null)
                return;

            float leftTravel = GetSuspensionTravel01(left);
            float rightTravel = GetSuspensionTravel01(right);

            float antiRoll = (leftTravel - rightTravel) * antiRollForce;

            if (left.isGrounded)
                body.AddForceAtPosition(left.transform.up * -antiRoll, left.transform.position, ForceMode.Force);

            if (right.isGrounded)
                body.AddForceAtPosition(right.transform.up * antiRoll, right.transform.position, ForceMode.Force);
        }

        private float GetSuspensionTravel01(WheelCollider wheel)
        {
            WheelHit hit;
            bool grounded = wheel.GetGroundHit(out hit);
            if (!grounded)
                return 0f;

            float travel = (-wheel.transform.InverseTransformPoint(hit.point).y - wheel.radius) / wheel.suspensionDistance;
            return Mathf.Clamp01(travel);
        }

        private void SetSteer(WheelCollider collider, float steerAngle)
        {
            if (collider == null)
                return;

            collider.steerAngle = steerAngle;
        }

        private void SetMotor(WheelCollider collider, float torque)
        {
            if (collider == null)
                return;

            collider.motorTorque = torque;
        }

        private void SetBrake(WheelCollider collider, float torque)
        {
            if (collider == null)
                return;

            collider.brakeTorque = torque;
        }
    }
}
