namespace Gameplay.InputLogic
{
    /// <summary>
    /// Routes input from a source to input consumers.
    /// Primary consumer is the active controllable (character/vehicle).
    /// Secondary consumer is typically the camera rig (look input).
    /// </summary>
    public sealed class InputRouter
    {
        private readonly IInputSource inputSource;

        private IInputConsumer primaryConsumer;
        private IInputConsumer secondaryConsumer;

        public InputRouter(IInputSource inputSource)
        {
            this.inputSource = inputSource;
        }

        public void SetPrimaryConsumer(IInputConsumer consumer)
        {
            primaryConsumer = consumer;
        }

        public void SetSecondaryConsumer(IInputConsumer consumer)
        {
            secondaryConsumer = consumer;
        }

        public void Tick()
        {
            if (primaryConsumer == null && secondaryConsumer == null)
                return;

            GameplayInput input = inputSource.Read();

            if (secondaryConsumer != null)
                secondaryConsumer.Consume(in input);

            if (primaryConsumer != null)
                primaryConsumer.Consume(in input);
        }
    }
}