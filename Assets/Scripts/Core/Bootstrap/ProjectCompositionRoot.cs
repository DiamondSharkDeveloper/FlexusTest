using Core.DI;
using Core.Services;
using Core.StateMachine;
using Cysharp.Threading.Tasks;
using Gameplay.Spawning;
using UnityEngine;

namespace Core.Bootstrap
{
    /// <summary>
    /// Application entry point responsible for composition:
    /// - binds services into DI container
    /// - registers and runs the game state machine
    /// </summary>
    public sealed class ProjectCompositionRoot : MonoBehaviour
    {
        [SerializeField] private GameConfig gameConfig;

        private DIContainer container;
        private GameStateMachine stateMachine;

        private void Start()
        {
            Run().Forget();
        }

        private async UniTaskVoid Run()
        {
            container = new DIContainer();
            DIContainerAccessor.Set(container);

            BindCore(container);
            BindSpawning(container);

            stateMachine = new GameStateMachine();
            container.BindSingleton(stateMachine);

            RegisterStates(container, stateMachine);

            await stateMachine.SwitchTo<BootstrapState>();
        }

        private void BindCore(DIContainer di)
        {
            di.BindSingleton(gameConfig);

            di.BindSingleton<ISceneLoader>(c =>
            {
                SceneLoader loader = new SceneLoader();
                return loader;
            });

            di.BindSingleton<IAssetProvider>(c =>
            {
                AddressablesAssetProvider provider = new AddressablesAssetProvider();
                return provider;
            });
        }

        private void BindSpawning(DIContainer di)
        {
            di.BindSingleton<ISpawnFactory<CharacterConfig, Component>>(c =>
            {
                IAssetProvider assetProvider = c.Resolve<IAssetProvider>();
                AddressablesSpawnFactory<CharacterConfig, Component> factory =
                    new AddressablesSpawnFactory<CharacterConfig, Component>(assetProvider);
                return factory;
            });

            di.BindSingleton<ISpawnFactory<VehicleConfig, Component>>(c =>
            {
                IAssetProvider assetProvider = c.Resolve<IAssetProvider>();
                AddressablesSpawnFactory<VehicleConfig, Component> factory =
                    new AddressablesSpawnFactory<VehicleConfig, Component>(assetProvider);
                return factory;
            });

            di.BindSingleton(c =>
            {
                ISpawnFactory<CharacterConfig, Component> characterFactory =
                    c.Resolve<ISpawnFactory<CharacterConfig, Component>>();

                ISpawnFactory<VehicleConfig, Component> vehicleFactory =
                    c.Resolve<ISpawnFactory<VehicleConfig, Component>>();

                SpawnService spawnService = new SpawnService(characterFactory, vehicleFactory);
                return spawnService;
            });
        }

        private void RegisterStates(DIContainer di, GameStateMachine fsm)
        {
            GameConfig config = di.Resolve<GameConfig>();
            ISceneLoader sceneLoader = di.Resolve<ISceneLoader>();

            BootstrapState bootstrapState = new BootstrapState(config, sceneLoader, fsm);
            GameplayState gameplayState = new GameplayState();

            fsm.Register(bootstrapState);
            fsm.Register(gameplayState);
        }
    }
}
