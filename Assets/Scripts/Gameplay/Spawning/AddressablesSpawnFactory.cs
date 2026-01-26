using Cysharp.Threading.Tasks;
using Project.Core.Services;
using UnityEngine;

namespace Project.Gameplay.Spawning
{
    /// <summary>
    /// Spawns prefabs using Addressables address provided in the config.
    /// Loads prefab through IAssetProvider and instantiates it in the scene.
    /// </summary>
    public sealed class AddressablesSpawnFactory<TConfig, TObject> : ISpawnFactory<TConfig, TObject>
        where TConfig : SpawnableConfig
        where TObject : Component
    {
        private readonly IAssetProvider assetProvider;

        public AddressablesSpawnFactory(IAssetProvider assetProvider)
        {
            this.assetProvider = assetProvider;
        }

        public async UniTask<TObject> Spawn(TConfig config, Vector3 position, Quaternion rotation)
        {
            if (config == null)
            {
                Debug.LogError("Spawn failed: config is null.");
                return null;
            }

            if (string.IsNullOrWhiteSpace(config.Address))
            {
                Debug.LogError($"Spawn failed: empty Addressables address for config {config.name}.");
                return null;
            }

            GameObject prefab = await assetProvider.Load<GameObject>(config.Address);
            if (prefab == null)
            {
                Debug.LogError($"Spawn failed: could not load prefab at address '{config.Address}'.");
                return null;
            }

            GameObject instance = Object.Instantiate(prefab, position, rotation);

            TObject component = instance.GetComponent<TObject>();
            if (component == null)
            {
                Debug.LogError($"Spawned prefab '{instance.name}' does not contain component {typeof(TObject).Name}.");
            }

            return component;
        }
    }
}