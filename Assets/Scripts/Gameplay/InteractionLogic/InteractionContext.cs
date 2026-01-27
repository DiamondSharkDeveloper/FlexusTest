using Gameplay.CharacterLogic;

namespace Gameplay.InteractionLogic
{
    /// <summary>
    /// Interaction context passed to interactables.
    /// Keeps interaction code decoupled from scene singletons.
    /// </summary>
    public readonly struct InteractionContext
    {
        public readonly CharacterRoot Character;
        public readonly PlayerControlService ControlService;

        public InteractionContext(CharacterRoot character, PlayerControlService controlService)
        {
            Character = character;
            ControlService = controlService;
        }
    }
}