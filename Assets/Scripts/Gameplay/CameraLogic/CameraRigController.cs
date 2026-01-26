using Gameplay.InputLogic;
using UnityEngine;

namespace Gameplay.CameraLogic
{
    /// <summary>
    /// Simple third-person camera rig:
    /// - follows a target transform
    /// - rotates around it using mouse look input
    /// </summary>
    public sealed class CameraRigController : MonoBehaviour, ICameraRig, IInputConsumer
    {
        [Header("References")]
        [SerializeField] private Camera sceneCamera;

        [Header("Orbit")]
        [SerializeField] private float distance = 4.5f;
        [SerializeField] private float height = 1.7f;

        [Header("Look")]
        [SerializeField] private float sensitivity = 2.0f;
        [SerializeField] private float minPitch = -35f;
        [SerializeField] private float maxPitch = 70f;

        private Transform target;

        private float yaw;
        private float pitch;

        private void Awake()
        {
            if (sceneCamera == null)
                sceneCamera = GetComponentInChildren<Camera>();
        }

        public void SetTarget(Transform target)
        {
            this.target = target;
        }

        public void Consume(in GameplayInput input)
        {
            Vector2 look = input.Look;

            yaw += look.x * sensitivity;
            pitch -= look.y * sensitivity;

            if (pitch < minPitch) pitch = minPitch;
            if (pitch > maxPitch) pitch = maxPitch;
        }

        private void LateUpdate()
        {
            if (target == null || sceneCamera == null)
                return;

            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);

            Vector3 pivot = target.position + new Vector3(0f, height, 0f);
            Vector3 offset = rotation * new Vector3(0f, 0f, -distance);

            sceneCamera.transform.position = pivot + offset;
            sceneCamera.transform.rotation = rotation;
        }
    }
}