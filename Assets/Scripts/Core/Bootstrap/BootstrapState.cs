using Core.Services;
using Core.StateMachine;
using Cysharp.Threading.Tasks;

namespace Core.Bootstrap
{
    /// <summary>
    /// Loads the main gameplay scene and transfers control to GameplayState.
    /// </summary>
    public sealed class BootstrapState : IGameState
    {
        private readonly GameConfig config;
        private readonly ISceneLoader sceneLoader;
        private readonly GameStateMachine stateMachine;

        public BootstrapState(GameConfig config, ISceneLoader sceneLoader, GameStateMachine stateMachine)
        {
            this.config = config;
            this.sceneLoader = sceneLoader;
            this.stateMachine = stateMachine;
        }

        public async UniTask Enter()
        {
            await sceneLoader.LoadSingle(config.GameplaySceneName);
            await stateMachine.SwitchTo<GameplayState>();
        }

        public UniTask Exit()
        {
            return UniTask.CompletedTask;
        }
    }
}