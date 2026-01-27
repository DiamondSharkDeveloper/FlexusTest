using Gameplay.Control;
using Gameplay.InputLogic;
using UnityEngine;

namespace Gameplay.CharacterLogic
{
    /// <summary>
    /// High-level character entry point: receives input and controls character subsystems.
    /// Implements IControllable so it can be switched in/out as an active player-controlled entity.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public sealed class CharacterRoot : MonoBehaviour, IControllable
    {
        [Header("References")]
        [SerializeField] private Transform cameraTarget;
        [SerializeField] private Animator animator;

        private CharacterController characterController;
        private CharacterMovementController movementController;
        private CharacterAnimationController animationController;
        private CharacterInteractionDetector interactionDetector;

        private bool isControlEnabled;

        public Transform CameraTarget => cameraTarget;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();

            if (animator == null)
                animator = GetComponentInChildren<Animator>();

            movementController = new CharacterMovementController(characterController, transform);
            animationController = new CharacterAnimationController(animator);
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
                movementController.SetCameraTransform(mainCamera.transform);
            interactionDetector = GetComponent<CharacterInteractionDetector>();
            if (interactionDetector == null)
                interactionDetector = gameObject.AddComponent<CharacterInteractionDetector>();
        }

        private void Update()
        {
            if (!isControlEnabled)
                return;

            movementController.Tick(Time.deltaTime);
        }

        public void EnableControl()
        {
            isControlEnabled = true;

            characterController.enabled = true;
            animationController.SetEnabled(true);
        }

        public void DisableControl()
        {
            isControlEnabled = false;

            movementController.ClearInput();
            animationController.SetSpeed01(0f);
            animationController.SetSprinting(false);

            characterController.enabled = false;
        }

        public void Consume(in GameplayInput input)
        {
            if (!isControlEnabled)
                return;

            movementController.SetInput(input);

            animationController.SetSprinting(input.SprintHeld);
            animationController.SetSpeed01(movementController.CurrentSpeed01);
            
            float animSpeed = Mathf.Lerp(0.9f, 1.6f, movementController.CurrentSpeed01);
            animationController.SetMoveAnimSpeed(animSpeed);

            if (input.InteractPressed)
                interactionDetector.TryInteract();
        }
    }
}
