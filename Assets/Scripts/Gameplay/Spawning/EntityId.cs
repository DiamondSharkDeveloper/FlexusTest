using System;
using UnityEngine;

namespace Gameplay.Spawning
{
    /// <summary>
    /// Lightweight identifier used to reference spawnable entities from configs.
    /// Stored as string for readability in assets and stability across renames.
    /// </summary>
    [Serializable]
    public struct EntityId : IEquatable<EntityId>
    {
        [SerializeField] private string value;

        public string Value => value;

        public bool Equals(EntityId other)
        {
            return string.Equals(value, other.value, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            return obj is EntityId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return value != null ? StringComparer.Ordinal.GetHashCode(value) : 0;
        }

        public override string ToString()
        {
            return value;
        }
    }
}