using Core.DI;
using Cysharp.Threading.Tasks;
using Gameplay.CameraLogic;
using Gameplay.InputLogic;
using Gameplay.Spawning;
using UnityEngine;

namespace Core.Bootstrap
{
    /// <summary>
    /// Scene-level composition root for the gameplay scene.
    /// Spawns initial entities and wires up input and camera.
    /// </summary>
    public sealed class GameplayCompositionRoot : MonoBehaviour
    {
        [Header("Configs")]
        [SerializeField] private CharacterConfig characterConfig;
        [SerializeField] private VehicleConfig[] vehicleConfigs;

        [Header("Spawn Points")]
        [SerializeField] private Transform characterSpawnPoint;
        [SerializeField] private Transform[] vehicleSpawnPoints;

        [Header("Scene References")]
        [SerializeField] private CameraRigController cameraRigController;

        private SpawnService spawnService;

        private InputRouter inputRouter;

        private async void Start()
        {
            await Initialize();
        }

        private async UniTask Initialize()
        {
            DIContainer container = DIContainerAccessor.Container;
            if (container == null)
            {
                Debug.LogError("DI container is not available. Make sure Bootstrap scene runs first.");
                return;
            }

            spawnService = container.Resolve<SpawnService>();

            SetupInputAndCamera();

            await SpawnCharacter();
            await SpawnVehicles();
        }

        private void SetupInputAndCamera()
        {
            if (cameraRigController == null)
            {
                cameraRigController = Object.FindFirstObjectByType<CameraRigController>();
            }

            IInputSource inputSource = new LocalInputSource();
            inputRouter = new InputRouter(inputSource);

            if (cameraRigController != null)
            {
                inputRouter.SetSecondaryConsumer(cameraRigController);
            }

            InputRouterDriver driver = Object.FindFirstObjectByType<InputRouterDriver>();
            if (driver == null)
            {
                GameObject driverObject = new GameObject("InputRouterDriver");
                driver = driverObject.AddComponent<InputRouterDriver>();
            }

            driver.Construct(inputRouter);
        }

        private async UniTask SpawnCharacter()
        {
            if (characterConfig == null)
            {
                Debug.LogError("CharacterConfig is not assigned.");
                return;
            }

            if (characterSpawnPoint == null)
            {
                Debug.LogError("Character spawn point is not assigned.");
                return;
            }

            await spawnService.SpawnCharacter(
                characterConfig,
                characterSpawnPoint.position,
                characterSpawnPoint.rotation);
        }

        private async UniTask SpawnVehicles()
        {
            if (vehicleConfigs == null || vehicleConfigs.Length == 0)
            {
                Debug.LogError("VehicleConfigs array is empty.");
                return;
            }

            if (vehicleSpawnPoints == null || vehicleSpawnPoints.Length == 0)
            {
                Debug.LogError("Vehicle spawn points array is empty.");
                return;
            }

            int countToSpawn = Mathf.Min(vehicleConfigs.Length, vehicleSpawnPoints.Length);

            int i = 0;
            while (i < countToSpawn)
            {
                VehicleConfig config = vehicleConfigs[i];
                Transform spawnPoint = vehicleSpawnPoints[i];

                if (config == null)
                {
                    Debug.LogError($"Vehicle config at index {i} is not assigned.");
                    i++;
                    continue;
                }

                if (spawnPoint == null)
                {
                    Debug.LogError($"Vehicle spawn point at index {i} is not assigned.");
                    i++;
                    continue;
                }

                await spawnService.SpawnVehicle(config, spawnPoint.position, spawnPoint.rotation);
                i++;
            }
        }
    }
}
