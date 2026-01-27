using UnityEngine;

namespace Gameplay.VehicleLogic
{
    /// <summary>
    /// Updates wheel visual transforms based on WheelCollider pose.
    /// Keeps visuals aligned with suspension and wheel rotation.
    /// </summary>
    public sealed class WheelVisualSync
    {
        public void Sync(WheelCollider collider, Transform visual)
        {
            if (collider == null || visual == null)
                return;

            Vector3 position;
            Quaternion rotation;

            collider.GetWorldPose(out position, out rotation);

            visual.position = position;
            visual.rotation = rotation;
        }
    }
}