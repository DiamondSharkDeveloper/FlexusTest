using Gameplay.InputLogic;
using UnityEngine;

namespace Gameplay.CharacterLogic
{
    /// <summary>
    /// Character movement based on CharacterController.
    /// Movement is camera-relative: input direction is converted into world-space using camera forward/right.
    /// </summary>
    public sealed class CharacterMovementController
    {
        private readonly CharacterController controller;
        private readonly Transform characterTransform;

        private Transform cameraTransform;

        private Vector2 moveInput;
        private bool sprintHeld;

        private float currentSpeed01;
        private float currentSpeed;

        public float CurrentSpeed01 => currentSpeed01;
        public float CurrentSpeed => currentSpeed;

        public float WalkSpeed { get; set; }
        public float SprintSpeed { get; set; }
        public float RotationSpeed { get; set; }

        public CharacterMovementController(CharacterController controller, Transform characterTransform)
        {
            this.controller = controller;
            this.characterTransform = characterTransform;

            cameraTransform = null;

            WalkSpeed = 3.5f;
            SprintSpeed = 6.5f;
            RotationSpeed = 12f;
        }

        public void SetCameraTransform(Transform cameraTransform)
        {
            this.cameraTransform = cameraTransform;
        }

        public void SetInput(in GameplayInput input)
        {
            moveInput = input.Move;
            sprintHeld = input.SprintHeld;
        }

        public void ClearInput()
        {
            moveInput = Vector2.zero;
            sprintHeld = false;
            currentSpeed01 = 0f;
            currentSpeed = 0f;
        }

        public void Tick(float deltaTime)
        {
            Vector3 desiredLocal = new Vector3(moveInput.x, 0f, moveInput.y);
            float magnitude = Mathf.Clamp01(desiredLocal.magnitude);

            float targetSpeed = sprintHeld ? SprintSpeed : WalkSpeed;
            float speed = targetSpeed * magnitude;

            currentSpeed = speed;

            float maxSpeed = sprintHeld ? SprintSpeed : WalkSpeed;
            if (maxSpeed > 0.001f)
                currentSpeed01 = Mathf.Clamp01(speed / maxSpeed);
            else
                currentSpeed01 = 0f;

            if (magnitude <= 0.001f)
                return;

            Vector3 worldMove = BuildWorldMoveDirection(desiredLocal);

            if (worldMove.sqrMagnitude < 0.0001f)
                return;

            Vector3 worldDir = worldMove.normalized;

            Quaternion targetRotation = Quaternion.LookRotation(worldDir, Vector3.up);
            characterTransform.rotation = Quaternion.Slerp(
                characterTransform.rotation,
                targetRotation,
                RotationSpeed * deltaTime);

            controller.Move(worldDir * speed * deltaTime);
        }

        private Vector3 BuildWorldMoveDirection(Vector3 desiredLocal)
        {
            if (cameraTransform == null)
            {
                // Fallback: movement relative to character if camera is not available.
                Vector3 forward = characterTransform.forward;
                Vector3 right = characterTransform.right;

                forward.y = 0f;
                right.y = 0f;

                forward = forward.sqrMagnitude > 0.0001f ? forward.normalized : Vector3.forward;
                right = right.sqrMagnitude > 0.0001f ? right.normalized : Vector3.right;

                Vector3 move = (right * desiredLocal.x + forward * desiredLocal.z);
                move.y = 0f;
                return move;
            }

            Vector3 camForward = cameraTransform.forward;
            Vector3 camRight = cameraTransform.right;

            camForward.y = 0f;
            camRight.y = 0f;

            camForward = camForward.sqrMagnitude > 0.0001f ? camForward.normalized : Vector3.forward;
            camRight = camRight.sqrMagnitude > 0.0001f ? camRight.normalized : Vector3.right;

            Vector3 world = (camRight * desiredLocal.x + camForward * desiredLocal.z);
            world.y = 0f;
            return world;
        }
    }
}
