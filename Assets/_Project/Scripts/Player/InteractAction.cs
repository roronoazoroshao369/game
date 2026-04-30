using UnityEngine;
using WildernessCultivation.Core;
using WildernessCultivation.World;

namespace WildernessCultivation.Player
{
    /// <summary>
    /// Tìm <see cref="IInteractable"/> gần nhất quanh player rồi gọi Interact.
    /// PC: phím E. Mobile: gọi <see cref="TryInteract"/> từ nút UI.
    /// </summary>
    public class InteractAction : MonoBehaviour
    {
        void Awake()
        {
            ServiceLocator.Register<InteractAction>(this);
        }

        void OnDestroy() => ServiceLocator.Unregister<InteractAction>(this);

        [Header("Detect")]
        public float interactRadius = 1.6f;
        public LayerMask interactMask = ~0;

        [Header("Input (PC)")]
        public KeyCode interactKey = KeyCode.E;

        /// <summary>Interactable đang được nhắm tới (gần nhất, có thể tương tác). Null nếu không có.</summary>
        public IInteractable CurrentTarget { get; private set; }
        public string CurrentLabel => CurrentTarget != null ? CurrentTarget.InteractLabel : string.Empty;

        void Update()
        {
            CurrentTarget = FindNearest();

            if (Input.GetKeyDown(interactKey))
                TryInteract();
        }

        public bool TryInteract()
        {
            var target = CurrentTarget ?? FindNearest();
            if (target == null) return false;
            return target.Interact(gameObject);
        }

        IInteractable FindNearest()
        {
            var hits = Physics2D.OverlapCircleAll(transform.position, interactRadius, interactMask);
            IInteractable best = null;
            float bestSqr = float.PositiveInfinity;

            foreach (var h in hits)
            {
                if (h == null) continue;
                var i = h.GetComponent<IInteractable>() ?? h.GetComponentInParent<IInteractable>();
                if (i == null || !i.CanInteract(gameObject)) continue;

                float sqr = ((Vector2)h.transform.position - (Vector2)transform.position).sqrMagnitude;
                if (sqr < bestSqr) { bestSqr = sqr; best = i; }
            }
            return best;
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, interactRadius);
        }
    }
}
