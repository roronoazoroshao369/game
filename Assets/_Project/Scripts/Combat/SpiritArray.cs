using System.Collections.Generic;
using UnityEngine;

namespace WildernessCultivation.Combat
{
    /// <summary>
    /// "Tụ Linh Trận" — trận pháp tạm thời do người chơi đặt xuống. Nhân multiplier cho mana/XP regen
    /// của <see cref="WildernessCultivation.Cultivation.MeditationAction"/> khi player ngồi thiền trong aura.
    /// Hết <see cref="lifetime"/> giây thì tự huỷ.
    /// </summary>
    public class SpiritArray : MonoBehaviour
    {
        public float radius = 3f;
        public float spiritMultiplier = 2f;
        public float lifetime = 30f;

        float dieAt;
        static readonly List<SpiritArray> active = new();

        void OnEnable() { if (!active.Contains(this)) active.Add(this); dieAt = Time.time + lifetime; }
        void OnDisable() { active.Remove(this); }

        void Update()
        {
            if (Time.time >= dieAt) Destroy(gameObject);
        }

        /// <summary>Trả về multiplier (≥1) tổng hợp tại vị trí này. 1.0 nếu không có array nào.</summary>
        public static float SpiritMultiplierAt(Vector3 worldPos)
        {
            float result = 1f;
            foreach (var a in active)
            {
                if (a == null) continue;
                float sqr = ((Vector2)worldPos - (Vector2)a.transform.position).sqrMagnitude;
                if (sqr <= a.radius * a.radius && a.spiritMultiplier > 1f)
                    result *= a.spiritMultiplier;
            }
            return result;
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.4f, 0.8f, 1f, 0.5f);
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }
}
