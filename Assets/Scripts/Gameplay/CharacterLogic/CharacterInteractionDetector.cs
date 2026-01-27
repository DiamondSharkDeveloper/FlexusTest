using Gameplay.InteractionLogic;
using UnityEngine;

namespace Gameplay.CharacterLogic
{
    /// <summary>
    /// Detects nearby interactables using trigger colliders.
    /// Uses a robust interface lookup (works even if GetComponent<IInterface> is unreliable).
    /// </summary>
    public sealed class CharacterInteractionDetector : MonoBehaviour
    {
        [SerializeField] private float radius = 2.0f;
        [SerializeField] private LayerMask interactableMask = ~0;

        [SerializeField] private bool debugLogs = true;

        private IInteractable current;

        public IInteractable Current => current;

        public void TickScan()
        {
            Collider[] hits = Physics.OverlapSphere(
                transform.position,
                radius,
                interactableMask,
                QueryTriggerInteraction.Collide);

            if (hits == null || hits.Length == 0)
            {
                if (current != null && debugLogs)
                    Debug.Log("Interactable lost.");

                current = null;
                return;
            }

            float bestDist = float.MaxValue;
            IInteractable best = null;

            int i = 0;
            while (i < hits.Length)
            {
                Collider hit = hits[i];
                if (hit == null)
                {
                    i++;
                    continue;
                }

                IInteractable candidate = FindInteractableOnParents(hit.transform);
                if (candidate == null)
                {
                    i++;
                    continue;
                }

                float d = Vector3.Distance(transform.position, hit.ClosestPoint(transform.position));
                if (d < bestDist)
                {
                    bestDist = d;
                    best = candidate;
                }

                i++;
            }

            if (best != current && debugLogs)
            {
                if (best == null)
                    Debug.Log("Interactable not found in range.");
                else
                    Debug.Log("Interactable found: " + best.Prompt);
            }

            current = best;
        }

        public void TryInteract(InteractionContext context)
        {
            if (current == null)
            {
                if (debugLogs)
                    Debug.Log("Interact pressed, but no interactable nearby.");

                return;
            }

            if (debugLogs)
                Debug.Log("Interacting with: " + current.Prompt);

            current.Interact(context);
        }

        private IInteractable FindInteractableOnParents(Transform start)
        {
            Transform t = start;
            while (t != null)
            {
                MonoBehaviour[] behaviours = t.GetComponents<MonoBehaviour>();
                int i = 0;
                while (i < behaviours.Length)
                {
                    MonoBehaviour b = behaviours[i];
                    if (b != null && b is IInteractable)
                        return (IInteractable)b;

                    i++;
                }

                t = t.parent;
            }

            return null;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }
}
