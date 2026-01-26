using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Gameplay.Spawning
{
    /// <summary>
    /// Factoy abstraction for spawning prefab instances based on a config.
    /// Helps keep gameplay code independent from Addressables or networking.
    /// </summary>
    public interface ISpawnFactory<in TConfig, TObject>
        where TConfig : SpawnableConfig
        where TObject : Component
    {
        UniTask<TObject> Spawn(TConfig config, Vector3 position, Quaternion rotation);
    }
}