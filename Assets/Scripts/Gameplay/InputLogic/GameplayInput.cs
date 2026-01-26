using UnityEngine;

namespace Gameplay.InputLogic
{
    /// <summary>
    /// Unified input snapshot for a single frame.
    /// This keeps character and vehicle controls consistent and easy to swap for network input.
    /// </summary>
    public readonly struct GameplayInput
    {
        public readonly Vector2 Move;
        public readonly Vector2 Look;

        public readonly bool SprintHeld;
        public readonly bool InteractPressed;

        public readonly bool BrakeHeld;

        public GameplayInput(
            Vector2 move,
            Vector2 look,
            bool sprintHeld,
            bool interactPressed,
            bool brakeHeld)
        {
            Move = move;
            Look = look;
            SprintHeld = sprintHeld;
            InteractPressed = interactPressed;
            BrakeHeld = brakeHeld;
        }
    }
}