using UnityEngine;

namespace Gameplay.InputLogic
{
    /// <summary>
    /// Local keyboard and mouse input provider.
    /// Uses Unity built-in Input API to avoid external dependencies.
    /// </summary>
    public sealed class LocalInputSource : IInputSource
    {
        private readonly string horizontalAxis;
        private readonly string verticalAxis;
        private readonly string mouseXAxis;
        private readonly string mouseYAxis;

        public LocalInputSource(
            string horizontalAxis = "Horizontal",
            string verticalAxis = "Vertical",
            string mouseXAxis = "Mouse X",
            string mouseYAxis = "Mouse Y")
        {
            this.horizontalAxis = horizontalAxis;
            this.verticalAxis = verticalAxis;
            this.mouseXAxis = mouseXAxis;
            this.mouseYAxis = mouseYAxis;
        }

        public GameplayInput Read()
        {
            float x = Input.GetAxisRaw(horizontalAxis);
            float y = Input.GetAxisRaw(verticalAxis);

            Vector2 move = new Vector2(x, y);

            float lookX = Input.GetAxis(mouseXAxis);
            float lookY = Input.GetAxis(mouseYAxis);
            Vector2 look = new Vector2(lookX, lookY);

            bool sprintHeld = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            bool interactPressed = Input.GetKeyDown(KeyCode.E);
            bool brakeHeld = Input.GetKey(KeyCode.Space);

            GameplayInput input = new GameplayInput(move, look, sprintHeld, interactPressed, brakeHeld);
            return input;
        }
    }
}