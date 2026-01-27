using Gameplay.InputLogic;
using UnityEngine;

namespace Gameplay.CharacterLogic
{
    /// <summary>
    /// Character movement based on CharacterController.
    /// Uses normalized input vector and applies sprint multiplier.
    /// </summary>
    public sealed class CharacterMovementController
    {
        private readonly CharacterController controller;
        private readonly Transform characterTransform;

        private Vector2 moveInput;
        private bool sprintHeld;

        private float currentSpeed01;

        public float CurrentSpeed01 => currentSpeed01;

        public float WalkSpeed { get; set; }
        public float SprintSpeed { get; set; }
        public float RotationSpeed { get; set; }

        public CharacterMovementController(CharacterController controller, Transform characterTransform)
        {
            this.controller = controller;
            this.characterTransform = characterTransform;

            WalkSpeed = 3.5f;
            SprintSpeed = 6.5f;
            RotationSpeed = 12f;
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
        }

        public void Tick(float deltaTime)
        {
            Vector3 desired = new Vector3(moveInput.x, 0f, moveInput.y);
            float magnitude = Mathf.Clamp01(desired.magnitude);

            float targetSpeed = sprintHeld ? SprintSpeed : WalkSpeed;
            float speed = targetSpeed * magnitude;

            currentSpeed01 = WalkSpeed > 0f ? Mathf.Clamp01(speed / WalkSpeed) : 0f;

            if (magnitude > 0.001f)
            {
                Vector3 forward = characterTransform.forward;
                Vector3 right = characterTransform.right;

                Vector3 worldMove = (right * desired.x + forward * desired.z);
                worldMove.y = 0f;

                if (worldMove.sqrMagnitude > 0.0001f)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(worldMove.normalized, Vector3.up);
                    characterTransform.rotation = Quaternion.Slerp(
                        characterTransform.rotation,
                        targetRotation,
                        RotationSpeed * deltaTime);
                }

                controller.Move(worldMove.normalized * speed * deltaTime);
            }
        }
    }
}
