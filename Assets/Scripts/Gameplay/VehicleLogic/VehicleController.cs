using Gameplay.InputLogic;
using UnityEngine;

namespace Gameplay.VehicleLogic
{
    /// <summary>
    /// WheelCollider-based vehicle controller.
    /// - Input is collected in Update (via Consume) and applied in FixedTick.
    /// - Motor torque and steering are distributed by axle flags.
    /// </summary>
    public sealed class VehicleController
    {
        private readonly Rigidbody rigidbody;
        private readonly AxleSetup[] axles;

        private float motorTorque;
        private float brakeTorque;
        private float maxSteerAngle;

        private Vector2 moveInput;
        private bool brakeHeld;

        public VehicleController(Rigidbody rigidbody, AxleSetup[] axles)
        {
            this.rigidbody = rigidbody;
            this.axles = axles;

            motorTorque = 1600f;
            brakeTorque = 3000f;
            maxSteerAngle = 28f;

            moveInput = Vector2.zero;
            brakeHeld = false;
        }

        public void ApplySettings(float motorTorque, float brakeTorque, float maxSteerAngle, Vector3 centerOfMassOffset)
        {
            this.motorTorque = motorTorque;
            this.brakeTorque = brakeTorque;
            this.maxSteerAngle = maxSteerAngle;

            if (rigidbody != null)
                rigidbody.centerOfMass += centerOfMassOffset;
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

            float steerAngle = steerInput * maxSteerAngle;
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
