namespace Gameplay.InputLogic
{
    /// <summary>
    /// Provides gameplay input snapshots.
    /// Implementations can be local (keyboard/mouse) or remote/network-driven.
    /// </summary>
    public interface IInputSource
    {
        GameplayInput Read();
    }
}