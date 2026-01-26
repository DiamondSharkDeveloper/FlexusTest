namespace Gameplay.InputLogic
{
    /// <summary>
    /// Receives input snapshots.
    /// Character and vehicle controlers implement this to process input consistently.
    /// </summary>
    public interface IInputConsumer
    {
        void Consume(in GameplayInput input);
    }
}