using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Gameplay.Spawning
{
    /// <summary>
    /// Central spawn entry point used by gameplay initialization.
    /// Keeps spawning logic consistent and makes it easy to swap factories later.
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

        public UniTask<Component> SpawnVehicle(VehicleConfig config, Vector3 position, Quaternion rotation)
        {
            return vehicleFactory.Spawn(config, position, rotation);
        }
    }
}