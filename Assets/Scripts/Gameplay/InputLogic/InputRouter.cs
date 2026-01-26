using Gameplay.InputLogic;

namespace Project.Gameplay.Input
{
    /// <summary>
    /// Routes input from a source to the current active consumer.
    /// This allows switching control targets without changing input code.
    /// </summary>
    public sealed class InputRouter
    {
        private readonly IInputSource inputSource;
        private IInputConsumer consumer;

        public InputRouter(IInputSource inputSource)
        {
            this.inputSource = inputSource;
        }

        public void SetConsumer(IInputConsumer consumer)
        {
            this.consumer = consumer;
        }

        public void Tick()
        {
            if (consumer == null)
                return;

            GameplayInput input = inputSource.Read();
            consumer.Consume(in input);
        }
    }
}