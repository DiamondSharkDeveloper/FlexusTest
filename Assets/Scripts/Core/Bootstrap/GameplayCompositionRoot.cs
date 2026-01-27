using Cysharp.Threading.Tasks;
using Core.DI;
using Gameplay.CameraLogic;
using Gameplay.CharacterLogic;
using Gameplay.Control;
using Gameplay.InputLogic;
using Gameplay.InteractionLogic;
using Gameplay.Spawning;
using Gameplay.VehicleLogic;
using UnityEngine;

namespace Core.Bootstrap
{
    /// <summary>
    /// Scene-level composition root for the gameplay scene.
    /// Spawns the character once and spawns one random vehicle per vehicle spawn point.
    /// Wires input, camera follow, control switching and a minimal interaction prompt.
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
        [SerializeField] private InteractionPromptUI interactionPromptUI;

        private SpawnService spawnService;

        private InputRouter inputRouter;
        private ControlSwitcher controlSwitcher;
        private PlayerControlService playerControlService;

        private CharacterRoot spawnedCharacter;
        private VehicleRoot[] spawnedVehicles;

        private bool initialized;

        private async void Start()
        {
            await Initialize();
        }

        private async UniTask Initialize()
        {
            if (initialized)
                return;

            initialized = true;

            DIContainer container = DIContainerAccessor.Container;
            if (container == null)
            {
                Debug.LogError("DI container is not available. Make sure Bootstrap scene runs first.");
                return;
            }

            spawnService = container.Resolve<SpawnService>();
            if (spawnService == null)
            {
                Debug.LogError("SpawnService is not registered in DI container.");
                return;
            }

            SetupInputAndCamera();
            SetupControlAndInteractionServices();

            await SpawnCharacter();
            await SpawnVehiclesRandomPerSpawnPoint();
        }

        private void SetupInputAndCamera()
        {
            if (cameraRigController == null)
                cameraRigController = Object.FindFirstObjectByType<CameraRigController>();

            IInputSource inputSource = new LocalInputSource();
            inputRouter = new InputRouter(inputSource);

            if (cameraRigController != null)
                inputRouter.SetSecondaryConsumer(cameraRigController);

            InputRouterDriver driver = Object.FindFirstObjectByType<InputRouterDriver>();
            if (driver == null)
            {
                GameObject driverObject = new GameObject("InputRouterDriver");
                driver = driverObject.AddComponent<InputRouterDriver>();
            }

            driver.Construct(inputRouter);
        }

        private void SetupControlAndInteractionServices()
        {
            ICameraRig cameraRig = cameraRigController;
            controlSwitcher = new ControlSwitcher(inputRouter, cameraRig);
            playerControlService = new PlayerControlService(controlSwitcher);

            spawnedVehicles = new VehicleRoot[0];
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

            Component spawned = await spawnService.SpawnCharacter(
                characterConfig,
                characterSpawnPoint.position,
                characterSpawnPoint.rotation);

            if (spawned == null)
            {
                Debug.LogError("SpawnCharacter returned null.");
                return;
            }

            CharacterRoot characterRoot = spawned.GetComponent<CharacterRoot>();
            if (characterRoot == null)
            {
                Debug.LogError("Spawned character prefab has no CharacterRoot.");
                return;
            }

            spawnedCharacter = characterRoot;

            playerControlService.RegisterCharacter(characterRoot);
            characterRoot.SetControlService(playerControlService);

            controlSwitcher.RegisterCharacter(characterRoot, characterRoot.CameraTarget);
            controlSwitcher.SetCharacterActive();
        }

        private async UniTask SpawnVehiclesRandomPerSpawnPoint()
        {
            if (vehicleSpawnPoints == null || vehicleSpawnPoints.Length == 0)
            {
                Debug.LogError("Vehicle spawn points array is empty.");
                return;
            }

            if (vehicleConfigs == null || vehicleConfigs.Length == 0)
            {
                Debug.LogError("VehicleConfigs array is empty.");
                return;
            }

            int countToSpawn = vehicleSpawnPoints.Length;
            spawnedVehicles = new VehicleRoot[countToSpawn];

            int i = 0;
            while (i < countToSpawn)
            {
                Transform spawnPoint = vehicleSpawnPoints[i];
                if (spawnPoint == null)
                {
                    Debug.LogError($"Vehicle spawn point at index {i} is not assigned.");
                    i++;
                    continue;
                }

                VehicleConfig randomConfig = PickRandomVehicleConfig();
                if (randomConfig == null)
                {
                    Debug.LogError("Random VehicleConfig is null. Check vehicleConfigs array.");
                    i++;
                    continue;
                }

                Component spawned = await spawnService.SpawnVehicle(
                    randomConfig,
                    spawnPoint.position,
                    spawnPoint.rotation);

                if (spawned == null)
                {
                    Debug.LogError($"SpawnVehicle returned null for spawn point index {i}.");
                    i++;
                    continue;
                }

                VehicleRoot vehicleRoot = spawned.GetComponent<VehicleRoot>();
                if (vehicleRoot == null)
                {
                    Debug.LogError("Spawned vehicle prefab has no VehicleRoot.");
                    i++;
                    continue;
                }
                vehicleRoot.SetControlService(playerControlService);
                spawnedVehicles[i] = vehicleRoot;

                i++;
            }
        }

        private VehicleConfig PickRandomVehicleConfig()
        {
            if (vehicleConfigs == null || vehicleConfigs.Length == 0)
                return null;

            int index = Random.Range(0, vehicleConfigs.Length);
            return vehicleConfigs[index];
        }

        private void Update()
        {
            UpdateInteractionPrompt();
        }

        private void UpdateInteractionPrompt()
        {
            if (interactionPromptUI == null)
                return;

            if (spawnedCharacter == null)
            {
                interactionPromptUI.Hide();
                return;
            }

            if (!spawnedCharacter.gameObject.activeInHierarchy)
            {
                interactionPromptUI.Hide();
                return;
            }

            CharacterInteractionDetector detector = spawnedCharacter.GetComponent<CharacterInteractionDetector>();
            if (detector == null)
            {
                interactionPromptUI.Hide();
                return;
            }

            IInteractable current = detector.Current;
            if (current == null)
            {
                interactionPromptUI.Hide();
                return;
            }

            interactionPromptUI.Show(current.Prompt);
        }
    }
}
