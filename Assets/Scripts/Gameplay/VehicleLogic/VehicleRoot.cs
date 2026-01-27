using Gameplay.Control;
using Gameplay.InputLogic;
using Gameplay.InteractionLogic;
using Gameplay.Spawning;
using UnityEngine;

namespace Gameplay.VehicleLogic
{
    /// <summary>
    /// Vehicle entry point for control and physics.
    /// - Rigidbody stays dynamic all the time.
    /// - When controlled: applies motor/steer/brake in FixedUpdate.
    /// - When not controlled: applies a parking brake for stability.
    /// - Handles exit input (E) while driving, so player can always leave the vehicle.
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

        [Header("Stability")]
        [SerializeField] private float parkingBrakeTorque = 2000f;

        private Rigidbody body;
        private VehicleController controller;
        private WheelVisualSync wheelVisualSync;

        private bool isControlEnabled;

        private PlayerControlService controlService;

        public Transform CameraTarget => cameraTarget;
        public Transform DriverSeat => driverSeat;
        public Transform ExitPoint => exitPoint;

        public void SetControlService(PlayerControlService controlService)
        {
            this.controlService = controlService;
        }

        private void Awake()
        {
            body = GetComponent<Rigidbody>();

            SetupRigidbodyDefaults();

            controller = new VehicleController(body, axles);
            wheelVisualSync = new WheelVisualSync();

            ApplyConfigIfAvailable();

            isControlEnabled = false;
        }

        private void FixedUpdate()
        {
            SyncWheelVisuals();

            if (isControlEnabled)
            {
                controller.FixedTick();
            }
            else
            {
                ApplyParkingBrake();
            }
        }

        public void EnableControl()
        {
            isControlEnabled = true;

            controller.ClearInput();
            ReleaseParkingBrake();
        }

        public void DisableControl()
        {
            isControlEnabled = false;

            controller.ClearInput();
            ApplyParkingBrake();
        }

        public void Consume(in GameplayInput input)
        {
            if (!isControlEnabled)
                return;

            if (input.InteractPressed && controlService != null)
            {
                controlService.ExitVehicle();
                return;
            }

            controller.SetInput(in input);
        }

        private void SetupRigidbodyDefaults()
        {
            body.isKinematic = false;

            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            if (body.mass < 500f)
                body.mass = 1400f;

            if (body.angularDrag < 0.05f)
                body.angularDrag = 0.7f;

            if (body.drag < 0.01f)
                body.drag = 0.05f;
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

        private void ApplyParkingBrake()
        {
            if (axles == null || axles.Length == 0)
                return;

            float torque = Mathf.Max(0f, parkingBrakeTorque);

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
                    axle.LeftCollider.brakeTorque = torque;
                    axle.LeftCollider.steerAngle = 0f;
                }

                if (axle.RightCollider != null)
                {
                    axle.RightCollider.motorTorque = 0f;
                    axle.RightCollider.brakeTorque = torque;
                    axle.RightCollider.steerAngle = 0f;
                }

                i++;
            }
        }

        private void ReleaseParkingBrake()
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
                    axle.LeftCollider.brakeTorque = 0f;

                if (axle.RightCollider != null)
                    axle.RightCollider.brakeTorque = 0f;

                i++;
            }
        }
    }
}
