using Gameplay.Spawning;
using UnityEngine;

namespace Gameplay.VehicleLogic
{
    /// <summary>
    /// Per-vehicle tuning data stored as a ScriptableObject.
    /// </summary>
    [CreateAssetMenu(menuName = "Gameplay/Vehicle/Vehicle Config", fileName = "VehicleConfig_")]
    public sealed class VehicleConfig : SpawnableConfig
    {
        [Header("Prefab")]
        public GameObject Prefab;

        [Header("Handling")]
        [Min(0f)] public float MotorTorque = 1200f;
        [Min(0f)] public float BrakeTorque = 3000f;
        [Range(1f, 45f)] public float MaxSteerAngle = 30f;

        [Header("Speed Limit")]
        [Tooltip("Max speed in meters per second. 18 m/s is ~65 km/h.")]
        [Min(1f)] public float MaxSpeedMps = 18f;

        [Header("Stability")]
        public Vector3 CenterOfMassOffset = new Vector3(0f, -0.5f, 0f);
        [Min(0f)] public float FrontAntiRollForce = 9000f;
        [Min(0f)] public float RearAntiRollForce = 6500f;

        [Header("Nitro")]
        [Tooltip("Additional motor torque multiplier at full nitro.")]
        [Min(0f)] public float NitroTorqueMultiplier = 1.6f;

        [Tooltip("How fast nitro reaches full power (seconds).")]
        [Min(0.01f)] public float NitroRampUpSeconds = 1.2f;

        [Tooltip("Nitro resource in seconds (how long you can hold Shift).")]
        [Min(0.1f)] public float NitroCapacitySeconds = 3.0f;

        [Tooltip("How fast nitro regenerates when not used (seconds per second). 1 means full regen in capacity time.")]
        [Min(0f)] public float NitroRegenRate = 0.75f;

        [Header("Damage")]
        [Tooltip("Relative collision speed required to count as a 'hit'.")]
        [Min(0f)] public float DamageHitMinRelativeSpeed = 6.0f;

        [Tooltip("Max hits before explosion. Recommended: 4 (1 light smoke, 2 heavy smoke, 3 fire, 4 explode).")]
        [Range(1, 10)] public int DamageMaxHits = 4;

        [Tooltip("Cooldown between hit registrations (seconds). Prevents multi-hit spam in one collision).")]
        [Min(0f)] public float DamageHitCooldownSeconds = 0.35f;
    }
}
