using Gameplay.Control;
using Gameplay.InputLogic;
using Gameplay.Spawning;
using UnityEngine;

namespace Gameplay.VehicleLogic
{
    /// <summary>
    /// Vehicle entry point for control and physics.
    /// Implements IControllable to support switching between character and vehicle control.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public sealed class VehicleRoot : MonoBehaviour, IControllable
    {
        [Header("References")]
        [SerializeField] private Transform cameraTarget;

        [Header("Optional Anchors")]
        [SerializeField] private Transform driverSeat;
        [SerializeField] private Transform exitPoint;

        [Header("Wheel Setup")]
        [SerializeField] private AxleSetup[] axles;

        [Header("Config (optional)")]
        [SerializeField] private VehicleConfig vehicleConfig;

        private Rigidbody body;
        private VehicleController controller;
        private WheelVisualSync wheelVisualSync;

        private bool isControlEnabled;

        public Transform CameraTarget => cameraTarget;
        public Transform DriverSeat => driverSeat;
        public Transform ExitPoint => exitPoint;

        private void Awake()
        {
            body = GetComponent<Rigidbody>();

            controller = new VehicleController(body, axles);
            wheelVisualSync = new WheelVisualSync();

            ApplyConfigIfAvailable();

            isControlEnabled = false;
        }

        private void FixedUpdate()
        {
            if (!isControlEnabled)
                return;

            controller.FixedTick();
            SyncWheelVisuals();
        }

        public void EnableControl()
        {
            isControlEnabled = true;

            body.isKinematic = false;
            body.interpolation = RigidbodyInterpolation.Interpolate;
        }

        public void DisableControl()
        {
            isControlEnabled = false;

            controller.ClearInput();

            StopWheelForces();
        }

        public void Consume(in GameplayInput input)
        {
            if (!isControlEnabled)
                return;

            controller.SetInput(in input);
        }

        private void ApplyConfigIfAvailable()
        {
            if (vehicleConfig == null)
                return;

            controller.ApplySettings(
                vehicleConfig.MotorTorque,
                vehicleConfig.BrakeTorque,
                vehicleConfig.MaxSteerAngle,
                vehicleConfig.CenterOfMassOffset);
        }

        private void SyncWheelVisuals()
        {
            if (axles == null || axles.Length == 0)
                return;

            int i = 0;
            while (i < axles.Length)
            {
                AxleSetup axle = axles[i];
                if (axle == null)
                {
                    i++;
                    continue;
                }

                wheelVisualSync.Sync(axle.LeftCollider, axle.LeftVisual);
                wheelVisualSync.Sync(axle.RightCollider, axle.RightVisual);

                i++;
            }
        }

        private void StopWheelForces()
        {
            if (axles == null || axles.Length == 0)
                return;

            int i = 0;
            while (i < axles.Length)
            {
                AxleSetup axle = axles[i];
                if (axle == null)
                {
                    i++;
                    continue;
                }

                if (axle.LeftCollider != null)
                {
                    axle.LeftCollider.motorTorque = 0f;
                    axle.LeftCollider.brakeTorque = 0f;
                    axle.LeftCollider.steerAngle = 0f;
                }

                if (axle.RightCollider != null)
                {
                    axle.RightCollider.motorTorque = 0f;
                    axle.RightCollider.brakeTorque = 0f;
                    axle.RightCollider.steerAngle = 0f;
                }

                i++;
            }
        }
    }
}
