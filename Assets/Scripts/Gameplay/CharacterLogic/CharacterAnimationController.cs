using UnityEngine;

namespace Gameplay.CharacterLogic
{
    /// <summary>
    /// Thin animator wrapper to keep animation logic separate from movement.
    /// </summary>
    public sealed class CharacterAnimationController
    {
        private readonly Animator animator;

        private readonly int speedHash;
        private readonly int sprintHash;
        private readonly int moveAnimSpeedHash;

        private bool enabled;

        public CharacterAnimationController(Animator animator)
        {
            this.animator = animator;

            speedHash = Animator.StringToHash("Speed");
            sprintHash = Animator.StringToHash("IsSprinting");
            moveAnimSpeedHash = Animator.StringToHash("MoveAnimSpeed");

            enabled = animator != null;
        }
        public void SetMoveAnimSpeed(float value)
        {
            if (!enabled || animator == null)
                return;

            animator.SetFloat(moveAnimSpeedHash, value);
        }
        public void SetEnabled(bool value)
        {
            enabled = value;

            if (animator != null)
                animator.enabled = value;
        }

        public void SetSpeed01(float value01)
        {
            if (!enabled || animator == null)
                return;

            animator.SetFloat(speedHash, value01);
        }

        public void SetSprinting(bool value)
        {
            if (!enabled || animator == null)
                return;

            animator.SetBool(sprintHash, value);
        }
    }
}