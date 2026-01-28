using Gameplay.InputLogic;
using UnityEngine;

namespace Gameplay.VehicleLogic
{
    /// <summary>
    /// WheelCollider-based vehicle controller.
    /// - Motor, steering, braking in FixedTick.
    /// - Nitro on Shift (uses SprintHeld from GameplayInput).
    /// - Anti-roll integrated for stability.
    /// </summary>
    public sealed class VehicleController
    {
        private readonly Rigidbody body;
        private readonly AxleSetup[] axles;

        private VehicleConfig config;

        private bool centerOfMassApplied;

        private Vector2 moveInput;
        private bool brakeHeld;
        private bool nitroHeld;

        private float nitroAmount01;
        private float nitroRamp01;

        public bool IsNitroActive
        {
            get
            {
                if (config == null)
                    return false;

                return nitroHeld && nitroAmount01 > 0.001f;
            }
        }

        public VehicleController(Rigidbody body, AxleSetup[] axles)
        {
            this.body = body;
            this.axles = axles;

            config = null;
            centerOfMassApplied = false;

            moveInput = Vector2.zero;
            brakeHeld = false;
            nitroHeld = false;

            nitroAmount01 = 1f;
            nitroRamp01 = 0f;
        }

        public void ApplySettings(VehicleConfig config)
        {
            this.config = config;

            if (this.config == null)
                return;

            if (!centerOfMassApplied && body != null)
            {
                body.centerOfMass += this.config.CenterOfMassOffset;
                centerOfMassApplied = true;
            }

            nitroAmount01 = 1f;
            nitroRamp01 = 0f;
        }

        public void SetInput(in GameplayInput input)
        {
            moveInput = input.Move;
            brakeHeld = input.BrakeHeld;
            nitroHeld = input.SprintHeld;
        }

        public void ClearInput()
        {
            moveInput = Vector2.zero;
            brakeHeld = false;
            nitroHeld = false;
        }

        public void FixedTick()
        {
            if (config == null)
                return;

            if (axles == null || axles.Length == 0)
                return;

            UpdateNitro(Time.fixedDeltaTime);

            float steerInput = Mathf.Clamp(moveInput.x, -1f, 1f);
            float throttleInput = Mathf.Clamp(moveInput.y, -1f, 1f);

            float speed = body != null ? body.velocity.magnitude : 0f;

            float steerFactor = ComputeSteerFactor(speed);
            float steerAngle = steerInput * config.MaxSteerAngle * steerFactor;

            float nitroMultiplier = GetNitroTorqueMultiplier();
            float motor = throttleInput * config.MotorTorque * nitroMultiplier;

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

        private void UpdateNitro(float dt)
        {
            float capacity = Mathf.Max(0.01f, config.NitroCapacitySeconds);
            float regen = Mathf.Max(0f, config.NitroRegenRate);

            bool wantsNitro = nitroHeld && nitroAmount01 > 0f;

            if (wantsNitro)
            {
                float spendPerSecond = 1f / capacity;
                nitroAmount01 = Mathf.Clamp01(nitroAmount01 - spendPerSecond * dt);

                float rampSpeed = 1f / Mathf.Max(0.01f, config.NitroRampUpSeconds);
                nitroRamp01 = Mathf.Clamp01(nitroRamp01 + rampSpeed * dt);
            }
            else
            {
                float regenPerSecond = regen / capacity;
                nitroAmount01 = Mathf.Clamp01(nitroAmount01 + regenPerSecond * dt);

                float rampDownSpeed = 2.5f;
                nitroRamp01 = Mathf.Clamp01(nitroRamp01 - rampDownSpeed * dt);
            }

            if (nitroAmount01 <= 0.0001f)
                nitroRamp01 = 0f;
        }

        private float GetNitroTorqueMultiplier()
        {
            bool active = nitroHeld && nitroAmount01 > 0.001f;
            if (!active)
                return 1f;

            float extra = Mathf.Max(0f, config.NitroTorqueMultiplier - 1f);
            return 1f + extra * nitroRamp01;
        }

        private float ComputeSteerFactor(float speed)
        {
            float t = Mathf.InverseLerp(5f, 18f, speed);
            return Mathf.Lerp(1f, 0.55f, t);
        }

        private void LimitSpeedHard()
        {
            if (body == null)
                return;

            float speed = body.velocity.magnitude;
            if (speed <= config.MaxSpeedMps)
                return;

            body.velocity = body.velocity.normalized * config.MaxSpeedMps;
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

        private void ApplyAntiRollBars()
        {
            if (body == null)
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
