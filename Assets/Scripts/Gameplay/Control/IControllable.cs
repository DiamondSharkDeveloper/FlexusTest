using Gameplay.InputLogic;

namespace Gameplay.Control
{
    /// <summary>
    /// Represents an entity that can receive player control.
    /// Implementations usually enable/disable movement components and input processing.
    /// </summary>
    public interface IControllable : IInputConsumer
    {
        void EnableControl();
        void DisableControl();
    }
}