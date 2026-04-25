using System;
using UnityEngine;
using WildernessCultivation.Player;

namespace WildernessCultivation.Cultivation
{
    /// <summary>
    /// Hệ thống cảnh giới tu tiên. MVP: từ Phàm Nhân (0) → Luyện Khí 1..9 (1..9).
    /// Mỗi tầng yêu cầu 1 lượng XP nhất định + đột phá thành công (tỉ lệ).
    /// Lên tầng → buff maxHP, maxMana, dame.
    /// </summary>
    public class RealmSystem : MonoBehaviour
    {
        [Serializable]
        public struct RealmDefinition
        {
            public string name;          // "Phàm Nhân", "Luyện Khí Tầng 1", ...
            public float xpRequired;     // XP để đủ điều kiện đột phá lên tầng này
            public float breakthroughChance; // [0..1]
            public float hpBonus, manaBonus, damageBonus;
        }

        [Header("Bảng cảnh giới")]
        public RealmDefinition[] realms;

        [Header("State")]
        [Min(0)] public int currentTier;
        public float currentXp;

        public string SpiritRoot = "Hỏa"; // random hoặc set lúc tạo nhân vật

        public event Action<int> OnRealmAdvanced;
        public event Action<bool> OnBreakthroughAttempted; // success?

        PlayerStats stats;
        PlayerCombat combat;

        void Awake()
        {
            stats = GetComponent<PlayerStats>();
            combat = GetComponent<PlayerCombat>();
            if (realms == null || realms.Length == 0) realms = DefaultRealms();
        }

        public RealmDefinition Current => realms[Mathf.Clamp(currentTier, 0, realms.Length - 1)];
        public bool HasNext => currentTier + 1 < realms.Length;
        public RealmDefinition Next => realms[Mathf.Clamp(currentTier + 1, 0, realms.Length - 1)];

        /// <summary>Cộng XP tu luyện. Gọi từ MeditationAction hoặc khi giết quái.</summary>
        public void AddCultivationXp(float amount)
        {
            currentXp += amount;
        }

        /// <summary>Người chơi chủ động bấm "Đột phá". Trả về true nếu thành công.</summary>
        public bool TryBreakthrough()
        {
            if (!HasNext) return false;
            var nextRealm = Next;
            if (currentXp < nextRealm.xpRequired) return false;

            currentXp -= nextRealm.xpRequired;
            bool success = UnityEngine.Random.value <= nextRealm.breakthroughChance;
            OnBreakthroughAttempted?.Invoke(success);

            if (success)
            {
                currentTier++;
                ApplyBonuses(nextRealm);
                OnRealmAdvanced?.Invoke(currentTier);
                Debug.Log($"[Realm] Đột phá thành công lên {nextRealm.name}!");
            }
            else
            {
                // Thất bại: mất một phần SAN
                if (stats != null) stats.Sanity = Mathf.Max(0f, stats.Sanity - 15f);
                Debug.Log($"[Realm] Đột phá thất bại lên {nextRealm.name}.");
            }
            return success;
        }

        void ApplyBonuses(RealmDefinition r)
        {
            if (stats != null)
            {
                stats.maxHP += r.hpBonus;
                stats.maxMana += r.manaBonus;
                stats.HP = stats.maxHP;
                stats.Mana = stats.maxMana;
            }
            if (combat != null)
            {
                combat.meleeDamage += r.damageBonus;
            }
        }

        static RealmDefinition[] DefaultRealms()
        {
            return new[]
            {
                new RealmDefinition{ name="Phàm Nhân",          xpRequired=0,    breakthroughChance=1f,  hpBonus=0,  manaBonus=0,  damageBonus=0 },
                new RealmDefinition{ name="Luyện Khí Tầng 1",  xpRequired=50,   breakthroughChance=0.95f, hpBonus=10, manaBonus=10, damageBonus=2 },
                new RealmDefinition{ name="Luyện Khí Tầng 2",  xpRequired=100,  breakthroughChance=0.90f, hpBonus=10, manaBonus=10, damageBonus=2 },
                new RealmDefinition{ name="Luyện Khí Tầng 3",  xpRequired=180,  breakthroughChance=0.85f, hpBonus=15, manaBonus=10, damageBonus=3 },
                new RealmDefinition{ name="Luyện Khí Tầng 4",  xpRequired=300,  breakthroughChance=0.80f, hpBonus=15, manaBonus=15, damageBonus=3 },
                new RealmDefinition{ name="Luyện Khí Tầng 5",  xpRequired=480,  breakthroughChance=0.75f, hpBonus=20, manaBonus=15, damageBonus=4 },
                new RealmDefinition{ name="Luyện Khí Tầng 6",  xpRequired=720,  breakthroughChance=0.70f, hpBonus=20, manaBonus=20, damageBonus=4 },
                new RealmDefinition{ name="Luyện Khí Tầng 7",  xpRequired=1050, breakthroughChance=0.60f, hpBonus=25, manaBonus=20, damageBonus=5 },
                new RealmDefinition{ name="Luyện Khí Tầng 8",  xpRequired=1500, breakthroughChance=0.50f, hpBonus=25, manaBonus=25, damageBonus=5 },
                new RealmDefinition{ name="Luyện Khí Tầng 9",  xpRequired=2100, breakthroughChance=0.40f, hpBonus=30, manaBonus=30, damageBonus=8 },
            };
        }
    }
}
