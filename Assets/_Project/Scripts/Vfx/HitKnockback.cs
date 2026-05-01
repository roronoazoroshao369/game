using UnityEngine;

namespace WildernessCultivation.Vfx
{
    /// <summary>
    /// Knockback impulse khi mob bị hit. Apply 1 lần / hit qua
    /// <see cref="ApplyFromSource"/> — caller (MobBase.TakeDamage) truyền vị trí
    /// nguồn gây dame, component tính direction (target - source).normalized rồi
    /// <c>rb.AddForce(impulse * dir, Impulse)</c>.
    ///
    /// Hợp với reactive feedback: flash + shake + leaf burst (<see cref="ReactiveOnHit"/>) +
    /// knockback nhẹ tạo "feel" mob trúng đòn không phải sprite chuyển màu suông.
    ///
    /// Pure <see cref="ComputeImpulse"/> static → EditMode test.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class HitKnockback : MonoBehaviour
    {
        [Tooltip("Magnitude impulse (kg·m/s). 1.5-2.5 cho mob nhỏ (rabbit, crow), 2.5-4 mob trung (wolf, boar), 5-8 boss.")]
        public float impulse = 2f;

        [Tooltip("Fallback direction khi source = target (zero magnitude). Mặc định +X. Tránh NaN/0 vector.")]
        public Vector2 fallbackDirection = Vector2.right;

        Rigidbody2D rb;

        void Awake() { rb = GetComponent<Rigidbody2D>(); }

        /// <summary>Apply knockback đẩy target ra xa source. Skip nếu rb null hoặc rb.simulated=false.</summary>
        public void ApplyFromSource(Vector2 sourcePos)
        {
            if (rb == null || !rb.simulated) return;
            Vector2 imp = ComputeImpulse(sourcePos, rb.position, impulse, fallbackDirection);
            rb.AddForce(imp, ForceMode2D.Impulse);
        }

        /// <summary>
        /// Pure: vector (target - source).normalized * impulse. Khi source≈target dùng fallback dir.
        /// Trả Vector2 — caller AddForce.
        /// </summary>
        public static Vector2 ComputeImpulse(Vector2 sourcePos, Vector2 targetPos,
            float impulse, Vector2 fallbackDirection)
        {
            Vector2 diff = targetPos - sourcePos;
            Vector2 dir;
            if (diff.sqrMagnitude < 1e-6f)
            {
                // Source overlap target → fallback dir cố định, normalize defensively.
                dir = fallbackDirection.sqrMagnitude < 1e-6f ? Vector2.right : fallbackDirection.normalized;
            }
            else
            {
                dir = diff.normalized;
            }
            return dir * impulse;
        }
    }
}
