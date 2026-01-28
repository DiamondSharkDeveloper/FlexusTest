using Gameplay.InputLogic;
using Gameplay.Spawning;
using UnityEngine;

namespace Gameplay.VehicleLogic
{
    /// <summary>
    /// WheelCollider-based vehicle controller.
    /// - Applies motor, steering and braking in FixedTick.
    /// - Uses VehicleConfig for tuning including max speed.
    /// - Includes integrated anti-roll forces for stability.
    /// </summary>
    public sealed class VehicleController
    {
        private readonly Rigidbody body;
        private readonly AxleSetup[] axles;

        private VehicleConfig config;

        private bool centerOfMassApplied;

        private Vector2 moveInput;
        private bool brakeHeld;

        public VehicleController(Rigidbody body, AxleSetup[] axles)
        {
            this.body = body;
            this.axles = axles;

            config = null;
            centerOfMassApplied = false;

            moveInput = Vector2.zero;
            brakeHeld = false;
        }

        public void ApplySettings(VehicleConfig config)
        {
            this.config = config;

            if (this.config == null)
                return;

            // Apply COM once to avoid stacking on re-enter.
            if (!centerOfMassApplied && body != null)
            {
                body.centerOfMass += this.config.CenterOfMassOffset;
                centerOfMassApplied = true;
            }
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
            if (config == null)
                return;

            if (axles == null || axles.Length == 0)
                return;

            float steerInput = Mathf.Clamp(moveInput.x, -1f, 1f);
            float throttleInput = Mathf.Clamp(moveInput.y, -1f, 1f);

            float speed = body != null ? body.velocity.magnitude : 0f;

            float steerFactor = ComputeSteerFactor(speed);
            float steerAngle = steerInput * config.MaxSteerAngle * steerFactor;

            float motor = throttleInput * config.MotorTorque;

            // Soft limiter: stop applying positive motor torque when above max speed.
            if (speed >= config.MaxSpeedMps && motor > 0f)
                motor = 0f;

            float brake = brakeHeld ? config.BrakeTorque : 0f;

            int i = 0;
            while (i < axles.Length)
            {
                AxleSetup axle = axles[i];
                if (axle == null)
                {
                    i++;
                    continue;
                }

                ApplyAxle(axle, steerAngle, motor, brake);

                i++;
            }

            ApplyAntiRollBars();
            LimitSpeedHard();
        }

        private void ApplyAxle(AxleSetup axle, float steerAngle, float motor, float brake)
        {
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
        }

        private float ComputeSteerFactor(float speed)
        {
            // Responsive steering. Stability is mostly handled by speed limit + anti-roll.
            // 0-5 m/s  -> 100% steer
            // 25+ m/s -> 65% steer
            float t = Mathf.InverseLerp(5f, 25f, speed);
            return Mathf.Lerp(1f, 0.65f, t);
        }

        private void LimitSpeedHard()
        {
            if (body == null)
                return;

            if (config == null)
                return;

            float speed = body.velocity.magnitude;
            if (speed <= config.MaxSpeedMps)
                return;

            body.velocity = body.velocity.normalized * config.MaxSpeedMps;
        }

        private void ApplyAntiRollBars()
        {
            if (body == null)
                return;

            if (config == null)
                return;

            WheelCollider frontLeft = null;
            WheelCollider frontRight = null;
            WheelCollider rearLeft = null;
            WheelCollider rearRight = null;

            int i = 0;
            while (i < axles.Length)
            {
                AxleSetup axle = axles[i];
                if (axle == null || axle.LeftCollider == null || axle.RightCollider == null)
                {
                    i++;
                    continue;
                }

                if (axle.Steer)
                {
                    frontLeft = axle.LeftCollider;
                    frontRight = axle.RightCollider;
                }
                else
                {
                    rearLeft = axle.LeftCollider;
                    rearRight = axle.RightCollider;
                }

                i++;
            }

            ApplyAntiRollPair(frontLeft, frontRight, config.FrontAntiRollForce);
            ApplyAntiRollPair(rearLeft, rearRight, config.RearAntiRollForce);
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
