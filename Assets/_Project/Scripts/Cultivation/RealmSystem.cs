using System;
using WildernessCultivation.Core;
using UnityEngine;
using WildernessCultivation.Items;
using WildernessCultivation.Player;

namespace WildernessCultivation.Cultivation
{
    /// <summary>
    /// Hệ thống cảnh giới tu tiên. MVP: từ Phàm Nhân (0) → Luyện Khí 1..9 (1..9).
    /// Mỗi tầng yêu cầu 1 lượng XP nhất định + đột phá thành công (tỉ lệ).
    /// Lên tầng → buff maxHP, maxMana, dame.
    /// </summary>
    public class RealmSystem : MonoBehaviour, MagicTreasureSO.RealmSystemHook
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

        [Header("Tuning")]
        [Range(0f, 1f)]
        [Tooltip("Tỉ lệ XP được hoàn lại khi đột phá thất bại. 0 = mất toàn bộ XP, 1 = không mất XP.")]
        public float xpRefundOnFailure = 0.5f;
        [Tooltip("Lượng SAN mất khi đột phá thất bại (cộng dồn theo độ khó next tier).")]
        public float failureSanityPenalty = 15f;

        public event Action<int> OnRealmAdvanced;
        public event Action<bool> OnBreakthroughAttempted; // success?

        // Bonus chance đột phá tạm thời do pháp bảo BreakthroughAid cấp.
        float tempBreakthroughBonus;
        float tempBreakthroughEndsAt;

        public float TemporaryBreakthroughBonus =>
            Time.time < tempBreakthroughEndsAt ? tempBreakthroughBonus : 0f;

        /// <summary>Implement <see cref="MagicTreasureSO.RealmSystemHook"/>.</summary>
        public void AddTemporaryBreakthroughBonus(float bonusChance, float durationSec)
        {
            if (bonusChance <= 0f || durationSec <= 0f) return;
            // Lấy max — không cộng dồn nhiều pháp bảo cùng lúc
            tempBreakthroughBonus = Mathf.Max(tempBreakthroughBonus, bonusChance);
            tempBreakthroughEndsAt = Mathf.Max(tempBreakthroughEndsAt, Time.time + durationSec);
            Debug.Log($"[Realm] Bonus đột phá +{bonusChance:P0} trong {durationSec}s.");
        }

        PlayerStats stats;
        PlayerCombat combat;
        SpiritRoot spiritRootHolder;

        void Awake()
        {
            stats = GetComponent<PlayerStats>();
            combat = GetComponent<PlayerCombat>();
            spiritRootHolder = GetComponent<SpiritRoot>();
            if (realms == null || realms.Length == 0) realms = DefaultRealms();
            ServiceLocator.Register<RealmSystem>(this);
        }

        void OnDestroy() => ServiceLocator.Unregister<RealmSystem>(this);

        public RealmDefinition Current => realms[Mathf.Clamp(currentTier, 0, realms.Length - 1)];
        public bool HasNext => currentTier + 1 < realms.Length;
        public RealmDefinition Next => realms[Mathf.Clamp(currentTier + 1, 0, realms.Length - 1)];

        /// <summary>XP cần có để đột phá realm kế tiếp, đã nhân BreakthroughCostMul từ linh căn.</summary>
        public float EffectiveNextXpRequired
            => HasNext ? Next.xpRequired * (spiritRootHolder != null ? spiritRootHolder.BreakthroughCostMul : 1f) : 0f;

        /// <summary>Cộng XP tu luyện. Gọi từ MeditationAction hoặc khi giết quái.</summary>
        public void AddCultivationXp(float amount)
        {
            float mul = spiritRootHolder != null ? spiritRootHolder.XpGainMul : 1f;
            currentXp += amount * mul;
        }

        /// <summary>Cộng XP từ technique cùng element (FireBall + Hoả căn → x2). Caller truyền element của technique.</summary>
        public void AddTechniqueXp(float amount, SpiritElement element)
        {
            float baseMul = spiritRootHolder != null ? spiritRootHolder.XpGainMul : 1f;
            float aff = spiritRootHolder != null ? spiritRootHolder.TechniqueAffinityMulFor(element) : 1f;
            currentXp += amount * baseMul * aff;
        }

        /// <summary>Người chơi chủ động bấm "Đột phá". Trả về true nếu thành công.</summary>
        public bool TryBreakthrough()
        {
            if (!HasNext) return false;
            var nextRealm = Next;
            float xpRequired = EffectiveNextXpRequired;
            if (currentXp < xpRequired) return false;

            float spent = xpRequired;
            currentXp -= spent;
            float effectiveChance = Mathf.Clamp01(nextRealm.breakthroughChance + TemporaryBreakthroughBonus);
            bool success = UnityEngine.Random.value <= effectiveChance;
            // Bonus pháp bảo chỉ dùng được 1 lần / 1 lần đột phá
            tempBreakthroughBonus = 0f;
            tempBreakthroughEndsAt = 0f;
            OnBreakthroughAttempted?.Invoke(success);
            Core.GameEvents.RaiseBreakthroughAttempted(success);

            if (success)
            {
                currentTier++;
                ApplyBonuses(nextRealm);
                OnRealmAdvanced?.Invoke(currentTier);
                Core.GameEvents.RaiseRealmAdvanced(currentTier);
                Debug.Log($"[Realm] Đột phá thành công lên {nextRealm.name}!");
            }
            else
            {
                // Thất bại: hoàn lại 1 phần XP, mất SAN
                float refund = spent * Mathf.Clamp01(xpRefundOnFailure);
                currentXp += refund;
                if (stats != null)
                    stats.Sanity = Mathf.Max(0f, stats.Sanity - failureSanityPenalty);
                Debug.Log($"[Realm] Đột phá thất bại lên {nextRealm.name}. Refund {refund:F0} XP, mất {failureSanityPenalty} SAN.");
            }
            return success;
        }

        void ApplyBonuses(RealmDefinition r)
        {
            ApplyBonusToStats(r);
            // Đột phá thành công → hồi đầy HP/Mana
            if (stats != null)
            {
                stats.HP = stats.maxHP;
                stats.Mana = stats.maxMana;
            }
        }

        void ApplyBonusToStats(RealmDefinition r)
        {
            if (stats != null)
            {
                stats.maxHP += r.hpBonus;
                stats.maxMana += r.manaBonus;
            }
            if (combat != null) combat.meleeDamage += r.damageBonus;
        }

        /// <summary>Re-apply tích luỹ bonus từ tier 1..currentTier lên stats hiện tại (không reset HP/Mana).
        /// Gọi từ <see cref="WildernessCultivation.Core.SaveLoadController"/> sau khi
        /// <see cref="WildernessCultivation.Player.PlayerStats.ReapplySpiritRootMaxHP"/> để khôi phục
        /// hpBonus/manaBonus/damageBonus tích luỹ qua các lần đột phá.</summary>
        public void ReapplyAccumulatedBonuses()
        {
            for (int i = 1; i <= currentTier && i < realms.Length; i++)
                ApplyBonusToStats(realms[i]);
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
                // Trúc Cơ — đại đột phá. Yêu cầu XP cao + chance thấp; cần đan dược/pháp bảo hỗ trợ (tăng chance qua effect).
                new RealmDefinition{ name="Trúc Cơ Sơ Kỳ",     xpRequired=3000, breakthroughChance=0.30f, hpBonus=60, manaBonus=60, damageBonus=12 },
                new RealmDefinition{ name="Trúc Cơ Trung Kỳ",  xpRequired=4200, breakthroughChance=0.25f, hpBonus=70, manaBonus=70, damageBonus=14 },
                new RealmDefinition{ name="Trúc Cơ Hậu Kỳ",    xpRequired=5800, breakthroughChance=0.20f, hpBonus=80, manaBonus=80, damageBonus=16 },
            };
        }
    }
}
