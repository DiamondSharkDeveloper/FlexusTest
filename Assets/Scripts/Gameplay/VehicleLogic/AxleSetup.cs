using System;
using UnityEngine;

namespace Gameplay.VehicleLogic
{
    /// <summary>
    /// Describes a wheel axle: left/right WheelColliders and optional visual meshes.
    /// Flags control whether the axle is used for steering and/or motor torque.
    /// </summary>
    [Serializable]
    public sealed class AxleSetup
    {
        public WheelCollider LeftCollider;
        public WheelCollider RightCollider;

        public Transform LeftVisual;
        public Transform RightVisual;

        public bool Steer;
        public bool Motor;
    }
}