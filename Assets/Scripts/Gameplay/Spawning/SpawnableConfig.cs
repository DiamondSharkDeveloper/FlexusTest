using UnityEngine;

namespace Gameplay.Spawning
{
    /// <summary>
    /// Base config for any spawnable entity.
    /// Contains a stable id and an Addressables address pointing to a prefab.
    /// </summary>
    public abstract class SpawnableConfig : ScriptableObject
    {
        [SerializeField] private EntityId id;
        [SerializeField] private string address;

        public EntityId Id => id;
        public string Address => address;
    }
}