using System.Collections.Generic;
using UnityEngine;

namespace WildernessCultivation.World
{
    /// <summary>
    /// Đăng ký 1 nguồn sáng (campfire / torch / lantern). Nếu đêm + player đứng ngoài tất cả
    /// LightSource đang active → bị "deep dark" (SAN tụt nhanh, mob spawn nhiều hơn).
    /// Cũng đóng vai trò warmth source: cộng <see cref="warmthBonus"/> vào ambient temperature
    /// nếu player đứng trong <see cref="radius"/>.
    /// </summary>
    public class LightSource : MonoBehaviour
    {
        public float radius = 3f;
        [Tooltip("True khi nguồn sáng đang phát (vd lửa trại đang cháy). False = không tính.")]
        public bool emitting = true;
        [Tooltip("Cộng nhiệt độ vào player nếu đứng trong aura.")]
        public float warmthBonus = 15f;

        static readonly List<LightSource> active = new();
        public static IReadOnlyList<LightSource> All => active;

        void OnEnable() { if (!active.Contains(this)) active.Add(this); }
        void OnDisable() { active.Remove(this); }

        public bool ContainsPosition(Vector3 worldPos)
        {
            if (!emitting) return false;
            float sqr = ((Vector2)worldPos - (Vector2)transform.position).sqrMagnitude;
            return sqr <= radius * radius;
        }

        /// <summary>True nếu vị trí đang trong aura của ít nhất 1 LightSource đang phát.</summary>
        public static bool AnyLightAt(Vector3 worldPos)
        {
            foreach (var ls in active)
                if (ls != null && ls.ContainsPosition(worldPos)) return true;
            return false;
        }

        /// <summary>Tổng warmthBonus của mọi LightSource active đang chứa vị trí (cộng dồn).</summary>
        public static float TotalWarmthAt(Vector3 worldPos)
        {
            float sum = 0f;
            foreach (var ls in active)
                if (ls != null && ls.ContainsPosition(worldPos)) sum += ls.warmthBonus;
            return sum;
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = emitting ? new Color(1f, 0.6f, 0.2f, 0.6f) : new Color(0.4f, 0.4f, 0.4f, 0.4f);
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }
}
