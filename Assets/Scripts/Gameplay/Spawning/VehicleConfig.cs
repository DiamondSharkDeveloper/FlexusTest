using UnityEngine;

namespace Gameplay.Spawning
{
    [CreateAssetMenu(menuName = "Project/Spawning/Vehicle Config")]
    public sealed class VehicleConfig : SpawnableConfig
    {
        [Header("Prefab")] public GameObject Prefab;

        [Header("Handling")] [Min(0f)] public float MotorTorque = 1200f;
        [Min(0f)] public float BrakeTorque = 3000f;
        [Range(1f, 45f)] public float MaxSteerAngle = 30f;

        [Header("Speed Limit")] [Tooltip("Max speed in meters per second. 18 m/s is ~65 km/h.")] [Min(1f)]
        public float MaxSpeedMps = 18f;

        [Header("Stability")] public Vector3 CenterOfMassOffset = new Vector3(0f, -0.5f, 0f);

        [Tooltip("Anti-roll force for the steering axle (front).")] [Min(0f)]
        public float FrontAntiRollForce = 9000f;

        [Tooltip("Anti-roll force for the non-steering axle (rear).")] [Min(0f)]
        public float RearAntiRollForce = 6500f;
    }
}