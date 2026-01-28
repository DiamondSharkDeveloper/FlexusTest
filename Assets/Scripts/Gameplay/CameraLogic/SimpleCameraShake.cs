using UnityEngine;

namespace Gameplay.CameraLogic
{
    /// <summary>
    /// Minimal camera shake without Cinemachine dependencies.
    /// Applies small local position offsets for a short duration.
    /// </summary>
    public sealed class SimpleCameraShake : MonoBehaviour
    {
        [SerializeField] private Transform target;

        private Vector3 initialLocalPos;
        private float remaining;
        private float amplitude;

        private void Awake()
        {
            if (target == null)
                target = transform;

            initialLocalPos = target.localPosition;
            remaining = 0f;
            amplitude = 0f;
        }

        private void LateUpdate()
        {
            if (remaining <= 0f)
            {
                target.localPosition = initialLocalPos;
                return;
            }

            remaining -= Time.deltaTime;

            Vector3 offset = Random.insideUnitSphere * amplitude;
            target.localPosition = initialLocalPos + offset;

            if (remaining <= 0f)
                target.localPosition = initialLocalPos;
        }

        public void Shake(float durationSeconds, float amplitude)
        {
            remaining = Mathf.Max(0f, durationSeconds);
            this.amplitude = Mathf.Max(0f, amplitude);
        }
    }
}