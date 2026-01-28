using Cysharp.Threading.Tasks;
using Gameplay.VehicleLogic;
using UnityEngine;

namespace Gameplay.Spawning
{
    /// <summary>
    /// Central spawn entry point used by gameplay initialization.
    /// Spawns prefabs and performs minimal post-spawn wiring.
    /// </summary>
    public sealed class SpawnService
    {
        private readonly ISpawnFactory<CharacterConfig, Component> characterFactory;
        private readonly ISpawnFactory<VehicleConfig, Component> vehicleFactory;

        public SpawnService(
            ISpawnFactory<CharacterConfig, Component> characterFactory,
            ISpawnFactory<VehicleConfig, Component> vehicleFactory)
        {
            this.characterFactory = characterFactory;
            this.vehicleFactory = vehicleFactory;
        }

        public UniTask<Component> SpawnCharacter(CharacterConfig config, Vector3 position, Quaternion rotation)
        {
            return characterFactory.Spawn(config, position, rotation);
        }

        public async UniTask<Component> SpawnVehicle(VehicleConfig config, Vector3 position, Quaternion rotation)
        {
            Component spawned = await vehicleFactory.Spawn(config, position, rotation);
            if (spawned == null)
                return null;

            VehicleRoot vehicleRoot = spawned.GetComponent<VehicleRoot>();
            if (vehicleRoot != null)
                vehicleRoot.SetConfig(config);

            return spawned;
        }
    }
}