using UnityEngine;

namespace Project.Gameplay.Spawning
{
    [CreateAssetMenu(menuName = "Project/Spawning/Character Config")]
    public sealed class CharacterConfig : SpawnableConfig
    {
        [Header("Movement")]
        public float WalkSpeed = 3.5f;

        public float SprintSpeed = 6.5f;

        public float RotationSpeed = 12f;
    }
}