using UnityEngine;

namespace Gameplay.CameraLogic
{
    /// <summary>
    /// Camera ri g contract used by gameplay code.
    /// Allows switching folow targets without tying logic to a specific camera implementation.
    /// </summary>
    public interface ICameraRig
    {
        void SetTarget(Transform target);
    }
}