namespace Gameplay.InteractionLogic
{
    /// <summary>
    /// Represents an object that can be interacted with by the player.
    /// Used for enter/exit vehicle and can be extended for pickups later.
    /// </summary>
    public interface IInteractable
    {
        string Prompt { get; }

        void Interact(InteractionContext context);
    }
}