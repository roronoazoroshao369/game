using System.Collections.Generic;
using UnityEngine;

namespace WildernessCultivation.World
{
    /// <summary>
    /// Lều / túp / nhà nhỏ — tạo aura bảo vệ:
    /// - chặn tác động xấu của Rain/Storm trong khi đứng trong aura
    /// - cộng warmth nhẹ qua <see cref="LightSource"/> (nếu shelter có lửa) hoặc qua biến riêng
    /// - cho <see cref="Player.SleepAction"/> bonus hồi HP/SAN khi ngủ trong shelter
    ///
    /// Đặt prefab Shelter trong scene; có thể đặt thêm 1 Campfire bên trong.
    /// </summary>
    public class Shelter : MonoBehaviour
    {
        public float radius = 3.5f;
        [Tooltip("Cộng vào nhiệt độ ambient cho người trong shelter (mặc định +5).")]
        public float warmthBonus = 5f;
        [Tooltip("Multiplier hồi HP/SAN khi ngủ trong shelter.")]
        public float sleepRecoveryMultiplier = 2f;

        static readonly List<Shelter> active = new();
        public static IReadOnlyList<Shelter> All => active;

        void OnEnable() { if (!active.Contains(this)) active.Add(this); }
        void OnDisable() { active.Remove(this); }

        public bool ContainsPosition(Vector3 worldPos)
        {
            float sqr = ((Vector2)worldPos - (Vector2)transform.position).sqrMagnitude;
            return sqr <= radius * radius;
        }

        /// <summary>Trả về Shelter chứa vị trí (nếu có nhiều, lấy gần nhất).</summary>
        public static Shelter NearestSheltering(Vector3 worldPos)
        {
            Shelter best = null;
            float bestSqr = float.PositiveInfinity;
            foreach (var s in active)
            {
                if (s == null) continue;
                float sqr = ((Vector2)worldPos - (Vector2)s.transform.position).sqrMagnitude;
                if (sqr <= s.radius * s.radius && sqr < bestSqr)
                {
                    bestSqr = sqr;
                    best = s;
                }
            }
            return best;
        }

        public static bool IsSheltered(Vector3 worldPos) => NearestSheltering(worldPos) != null;

        void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.6f, 0.4f, 0.2f, 0.4f);
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }
}
