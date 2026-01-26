using UnityEngine;

namespace Project.Gameplay.Spawning
{
    [CreateAssetMenu(menuName = "Project/Spawning/Vehicle Config")]
    public sealed class VehicleConfig : SpawnableConfig
    {
        [Header("Vehicle Physics")]
        public float MotorTorque = 1600f;

        public float BrakeTorque = 3000f;

        public float MaxSteerAngle = 28f;

        [Tooltip("Adjusts stability by moving rigidbody center of mass.")]
        public Vector3 CenterOfMassOffset = new Vector3(0f, -0.4f, 0f);
    }
}