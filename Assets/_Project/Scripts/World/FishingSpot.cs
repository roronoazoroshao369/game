using System.Collections.Generic;
using UnityEngine;
using WildernessCultivation.Items;

namespace WildernessCultivation.World
{
    /// <summary>
    /// Điểm câu cá — gắn vào WaterSpring hoặc 1 prefab nước riêng. <see cref="FishingAction"/>
    /// query <see cref="NearestSpot"/> để xem player có đứng đủ gần không, rồi cast trong
    /// <see cref="castTimeSeconds"/> để rút random 1 mục từ <see cref="lootTable"/>.
    /// </summary>
    public class FishingSpot : MonoBehaviour
    {
        [System.Serializable]
        public struct LootEntry
        {
            public ItemSO item;
            [Tooltip("Trọng số chọn ngẫu nhiên (lớn = hay ra hơn).")]
            public float weight;
            public int min;
            public int max;
        }

        public float castRangeFromSpot = 2.5f;
        [Tooltip("Khoảng giây 1 lần cast.")]
        public Vector2 castTimeSeconds = new Vector2(3f, 8f);
        public LootEntry[] lootTable;

        static readonly List<FishingSpot> active = new();
        public static IReadOnlyList<FishingSpot> All => active;

        void OnEnable() { if (!active.Contains(this)) active.Add(this); }
        void OnDisable() { active.Remove(this); }

        public static FishingSpot NearestSpotInRange(Vector3 worldPos)
        {
            FishingSpot best = null;
            float bestSqr = float.PositiveInfinity;
            foreach (var s in active)
            {
                if (s == null) continue;
                float sqr = ((Vector2)worldPos - (Vector2)s.transform.position).sqrMagnitude;
                if (sqr <= s.castRangeFromSpot * s.castRangeFromSpot && sqr < bestSqr)
                {
                    bestSqr = sqr;
                    best = s;
                }
            }
            return best;
        }

        /// <summary>Random 1 entry theo weight; trả về null nếu lootTable rỗng.</summary>
        public LootEntry? RollLoot()
        {
            if (lootTable == null || lootTable.Length == 0) return null;
            float total = 0f;
            foreach (var e in lootTable) total += Mathf.Max(0f, e.weight);
            if (total <= 0f) return null;
            float roll = Random.value * total;
            float acc = 0f;
            foreach (var e in lootTable)
            {
                acc += Mathf.Max(0f, e.weight);
                if (roll <= acc) return e;
            }
            return lootTable[lootTable.Length - 1];
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.2f, 0.6f, 1f, 0.4f);
            Gizmos.DrawWireSphere(transform.position, castRangeFromSpot);
        }
    }
}
