using System.Collections;
using UnityEngine;

namespace Gameplay.VehicleLogic
{
    /// <summary>
    /// Hit-based vehicle damage with stage VFX spawned at the "Engine" point.
    /// 1 hit  - light smoke
    /// 2 hits - heavy smoke
    /// 3 hits - fire
    /// 4 hits - explosion, force exit to character, burn a bit, then desaturate and disable vehicle
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public sealed class VehicleDamage : MonoBehaviour
    {
        [Header("Engine point (optional). If not set, will search child named 'Engine'.")]
        [SerializeField] private Transform enginePoint;

        [Header("VFX prefabs (ParticleSystem)")]
        [SerializeField] private ParticleSystem lightSmokePrefab;
        [SerializeField] private ParticleSystem heavySmokePrefab;
        [SerializeField] private ParticleSystem firePrefab;
        [SerializeField] private ParticleSystem explosionPrefab;

        [Header("Optional fallback config (if not injected)")]
        [SerializeField] private VehicleConfig fallbackConfig;

        [Header("Optional vehicle root")]
        [SerializeField] private VehicleRoot vehicleRoot;

        [Header("Burnout visuals")]
        [Tooltip("How long the car keeps burning after explosion before starting the fade.")]
        [SerializeField] private float burnSeconds = 2.0f;

        [Tooltip("Fade duration (desaturate + darken) before disabling the vehicle.")]
        [SerializeField] private float fadeSeconds = 1.25f;

        [Tooltip("How strong desaturation gets at the end (0 = none, 1 = full grayscale).")]
        [Range(0f, 1f)]
        [SerializeField] private float finalDesaturation = 0.85f;

        [Tooltip("How much the base color is darkened at the end (1 = no darken).")]
        [Range(0.1f, 1f)]
        [SerializeField] private float finalDarkenMultiplier = 0.55f;

        private Rigidbody body;
        private VehicleConfig config;

        private ParticleSystem lightSmokeInstance;
        private ParticleSystem heavySmokeInstance;
        private ParticleSystem fireInstance;

        private int hitCount;
        private float nextHitTime;
        private bool exploded;

        private Renderer[] cachedRenderers;
        private MaterialPropertyBlock propertyBlock;
        private Color[] initialBaseColors;
        private bool canTint;

        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");

        public void SetConfig(VehicleConfig config)
        {
            this.config = config;
        }

        private void Awake()
        {
            body = GetComponent<Rigidbody>();

            if (vehicleRoot == null)
                vehicleRoot = GetComponent<VehicleRoot>();

            if (enginePoint == null)
                enginePoint = FindEnginePoint(transform);

            if (enginePoint == null)
                enginePoint = transform;

            if (config == null)
                config = fallbackConfig;

            hitCount = 0;
            nextHitTime = 0f;
            exploded = false;

            lightSmokeInstance = CreateChildVfx(lightSmokePrefab);
            heavySmokeInstance = CreateChildVfx(heavySmokePrefab);
            fireInstance = CreateChildVfx(firePrefab);

            StopIfAssigned(lightSmokeInstance);
            StopIfAssigned(heavySmokeInstance);
            StopIfAssigned(fireInstance);

            CacheRenderers();
        }

        private void OnDestroy()
        {
            DestroyIfAssigned(lightSmokeInstance);
            DestroyIfAssigned(heavySmokeInstance);
            DestroyIfAssigned(fireInstance);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (exploded)
                return;

            if (config == null)
                return;

            float now = Time.time;
            if (now < nextHitTime)
                return;

            float relativeSpeed = collision.relativeVelocity.magnitude;
            if (relativeSpeed < config.DamageHitMinRelativeSpeed)
                return;

            nextHitTime = now + config.DamageHitCooldownSeconds;

            hitCount++;
            ApplyDamageStage(hitCount);

            if (hitCount >= config.DamageMaxHits)
                Explode();
        }

        private void ApplyDamageStage(int hits)
        {
            if (hits == 1)
            {
                PlayIfAssigned(lightSmokeInstance);
                StopIfAssigned(heavySmokeInstance);
                StopIfAssigned(fireInstance);
                return;
            }

            if (hits == 2)
            {
                StopIfAssigned(lightSmokeInstance);
                PlayIfAssigned(heavySmokeInstance);
                StopIfAssigned(fireInstance);
                return;
            }

            if (hits == 3)
            {
                StopIfAssigned(lightSmokeInstance);
                StopIfAssigned(heavySmokeInstance);
                PlayIfAssigned(fireInstance);
                return;
            }
        }

        private void Explode()
        {
            if (exploded)
                return;

            exploded = true;

            StopIfAssigned(lightSmokeInstance);
            StopIfAssigned(heavySmokeInstance);

            // Keep fire on during burnout phase
            PlayIfAssigned(fireInstance);

            SpawnOneShot(explosionPrefab);

            // THIS is the important part: same flow as pressing E.
            if (vehicleRoot != null)
                vehicleRoot.ForceExitToCharacter();

            // Optional impulse so the explosion reads well even without camera shake
            if (body != null)
            {
                body.AddForce(Vector3.up * 2200f, ForceMode.Impulse);
                body.AddTorque(Random.onUnitSphere * 1200f, ForceMode.Impulse);
            }

            StartCoroutine(BurnoutRoutine());
        }

        private IEnumerator BurnoutRoutine()
        {
            float burn = Mathf.Max(0f, burnSeconds);
            if (burn > 0f)
                yield return new WaitForSeconds(burn);

            float fade = Mathf.Max(0.01f, fadeSeconds);
            float t = 0f;

            while (t < fade)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / fade);
                ApplyBurnTint(k);
                yield return null;
            }

            ApplyBurnTint(1f);

            // Stop fire at the end (optional). You can keep it if you prefer.
            StopIfAssigned(fireInstance);
            yield return new WaitForSeconds(1f);
            gameObject.SetActive(false);
        }

        private void CacheRenderers()
        {
            cachedRenderers = GetComponentsInChildren<Renderer>(true);
            propertyBlock = new MaterialPropertyBlock();

            initialBaseColors = new Color[cachedRenderers.Length];
            canTint = false;

            int i = 0;
            while (i < cachedRenderers.Length)
            {
                Renderer r = cachedRenderers[i];
                Color c = Color.white;

                if (r != null && r.sharedMaterial != null)
                {
                    if (r.sharedMaterial.HasProperty(BaseColorId))
                    {
                        c = r.sharedMaterial.GetColor(BaseColorId);
                        canTint = true;
                    }
                    else if (r.sharedMaterial.HasProperty(ColorId))
                    {
                        c = r.sharedMaterial.GetColor(ColorId);
                        canTint = true;
                    }
                }

                initialBaseColors[i] = c;
                i++;
            }
        }

        private void ApplyBurnTint(float normalized)
        {
            if (!canTint || cachedRenderers == null || cachedRenderers.Length == 0)
                return;

            float desat = Mathf.Lerp(0f, finalDesaturation, normalized);
            float darken = Mathf.Lerp(1f, finalDarkenMultiplier, normalized);

            int i = 0;
            while (i < cachedRenderers.Length)
            {
                Renderer r = cachedRenderers[i];
                if (r == null)
                {
                    i++;
                    continue;
                }

                Color baseColor = initialBaseColors[i];

                // grayscale via luminance
                float gray = baseColor.r * 0.2126f + baseColor.g * 0.7152f + baseColor.b * 0.0722f;
                Color grayColor = new Color(gray, gray, gray, baseColor.a);

                Color mixed = Color.Lerp(baseColor, grayColor, desat);
                mixed = new Color(mixed.r * darken, mixed.g * darken, mixed.b * darken, mixed.a);

                r.GetPropertyBlock(propertyBlock);

                if (r.sharedMaterial != null && r.sharedMaterial.HasProperty(BaseColorId))
                    propertyBlock.SetColor(BaseColorId, mixed);
                else
                    propertyBlock.SetColor(ColorId, mixed);

                r.SetPropertyBlock(propertyBlock);

                i++;
            }
        }

        private Transform FindEnginePoint(Transform root)
        {
            Transform found = root.Find("Engine");
            if (found != null)
                return found;

            int childCount = root.childCount;
            int i = 0;
            while (i < childCount)
            {
                Transform child = root.GetChild(i);

                if (child.name == "Engine")
                    return child;

                Transform nested = FindEnginePoint(child);
                if (nested != null)
                    return nested;

                i++;
            }

            return null;
        }

        private ParticleSystem CreateChildVfx(ParticleSystem prefab)
        {
            if (prefab == null)
                return null;

            ParticleSystem instance = Instantiate(prefab, enginePoint);
            Transform tr = instance.transform;
            tr.localPosition = Vector3.zero;
            tr.localRotation = Quaternion.identity;
            tr.localScale = Vector3.one;

            return instance;
        }

        private void SpawnOneShot(ParticleSystem prefab)
        {
            if (prefab == null)
                return;

            ParticleSystem instance = Instantiate(prefab, enginePoint.position, enginePoint.rotation);
            instance.Play(true);

            float lifetime = GetApproxLifetime(instance);
            Destroy(instance.gameObject, lifetime);
        }

        private float GetApproxLifetime(ParticleSystem ps)
        {
            if (ps == null)
                return 2f;

            ParticleSystem.MainModule main = ps.main;
            float duration = main.duration;

            float maxLifetime = 0f;
            ParticleSystem.MinMaxCurve lifetime = main.startLifetime;

            if (lifetime.mode == ParticleSystemCurveMode.Constant)
                maxLifetime = lifetime.constant;
            else if (lifetime.mode == ParticleSystemCurveMode.TwoConstants)
                maxLifetime = Mathf.Max(lifetime.constantMin, lifetime.constantMax);
            else
                maxLifetime = 2f;

            return Mathf.Max(1f, duration + maxLifetime + 0.25f);
        }

        private void PlayIfAssigned(ParticleSystem ps)
        {
            if (ps == null)
                return;

            if (!ps.isPlaying)
                ps.Play(true);
        }

        private void StopIfAssigned(ParticleSystem ps)
        {
            if (ps == null)
                return;

            if (ps.isPlaying)
                ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        private void DestroyIfAssigned(ParticleSystem ps)
        {
            if (ps == null)
                return;

            Destroy(ps.gameObject);
        }
    }
}
