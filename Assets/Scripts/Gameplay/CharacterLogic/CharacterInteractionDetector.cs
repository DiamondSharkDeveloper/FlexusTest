using UnityEngine;

namespace Gameplay.CharacterLogic
{
    /// <summary>
    /// Finds the closest interactable in range and triggers interaction on request.
    /// </summary>
    public sealed class CharacterInteractionDetector : MonoBehaviour
    {
        [SerializeField] private float radius = 2.0f;
        [SerializeField] private LayerMask interactableMask = ~0;

        public void TryInteract()
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, radius, interactableMask, QueryTriggerInteraction.Collide);
            if (hits == null || hits.Length == 0)
                return;

            float bestDist = float.MaxValue;
            MonoBehaviour best = null;

            int i = 0;
            while (i < hits.Length)
            {
                Collider hit = hits[i];
                if (hit == null)
                {
                    i++;
                    continue;
                }

                MonoBehaviour candidate = hit.GetComponentInParent<MonoBehaviour>();
                if (candidate == null)
                {
                    i++;
                    continue;
                }

                // This is intentionally loose at this stage; concrete interactables are introduced in InteractionLogic.
                // The actual interaction interface is added in Part 9.
                float d = Vector3.Distance(transform.position, hit.ClosestPoint(transform.position));
                if (d < bestDist)
                {
                    bestDist = d;
                    best = candidate;
                }

                i++;
            }

            // Part 9 will replace this with a strict IInteractable contract.
            // For now, detector stays harmless and does not assume any specific type.
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }
}