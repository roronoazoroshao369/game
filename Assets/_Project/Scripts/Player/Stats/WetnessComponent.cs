using UnityEngine;
using WildernessCultivation.Player.Status;
using WildernessCultivation.World;

namespace WildernessCultivation.Player.Stats
{
    /// <summary>
    /// Wetness subsystem: gauge [0..maxWetness] + 4 tier (Dry/Damp/Wet/Drenched).
    /// Rain/Storm cộng, fire/shelter/sun trừ. Tier càng cao → cold drift × multiplier
    /// + tụt SAN; Drenched + cold → roll Sickness status.
    ///
    /// Tách khỏi <see cref="PlayerStats"/> (R1 refactor) — SRP: tất cả logic ướt
    /// + sickness chain ở 1 chỗ. PlayerStats giữ properties façade
    /// (<see cref="PlayerStats.Wetness"/>, <see cref="PlayerStats.AddWetness"/>, …)
    /// để consumer + test cũ không break.
    /// </summary>
    public class WetnessComponent : MonoBehaviour
    {
        [Tooltip("Ướt hiện tại. Tier: <20 Dry, 20..50 Damp, 50..80 Wet, >=80 Drenched.")]
        [Range(0f, 100f)] public float Wetness = 0f;
        public float maxWetness = 100f;

        [Header("Sources (per second)")]
        [Tooltip("Tốc độ ướt thêm khi đứng mưa.")]
        public float wetnessRainPerSec = 4f;
        [Tooltip("Storm: ướt nhanh hơn rain (multiplier).")]
        public float wetnessStormMultiplier = 2f;

        [Header("Sinks (per second)")]
        [Tooltip("Tốc độ khô base khi không bị mưa.")]
        public float wetnessDryBasePerSec = 0.5f;
        [Tooltip("Bonus khi gần lửa trại.")]
        public float wetnessDryFireBonus = 4f;
        [Tooltip("Bonus khi trong shelter.")]
        public float wetnessDryShelterBonus = 2f;
        [Tooltip("Bonus ban ngày (nắng), pro-rate theo light intensity.")]
        public float wetnessDryDayBonus = 1.5f;

        [Header("Cold drift coupling (multiplier vào thermalDriftRate khi ambient lạnh)")]
        public float dampColdDriftMultiplier = 1.2f;
        public float wetColdDriftMultiplier = 1.4f;
        public float drenchedColdDriftMultiplier = 1.7f;

        [Header("SAN penalty per tier (per second)")]
        public float wetSanityPenaltyPerSec = 0.2f;
        public float drenchedSanityPenaltyPerSec = 0.3f;

        [Header("Sickness chain (Drenched + cold → roll Sickness)")]
        [Tooltip("Ngưỡng BodyTemp dưới đó mới roll. -1 = dùng comfortMin của ThermalComponent.")]
        public float sicknessColdThreshold = -1f;
        [Tooltip("Xác suất / giây apply Sickness khi điều kiện đạt. 0 = tắt chain.")]
        public float sicknessChancePerSec = 0.02f;
        [Tooltip("Sickness status apply khi điều kiện đạt. Null = không apply.")]
        public StatusEffectSO sicknessEffect;
        [Tooltip("Cooldown sau khi apply Sickness (giây).")]
        public float sicknessApplyCooldownSec = 30f;
        float nextSicknessAllowedAt;

        PlayerStats stats;
        StatusEffectManager statusManager;

        void Awake()
        {
            ResolveDeps();
        }

        // Lazy resolve cho EditMode: Unity không tự gọi Awake khi AddComponent
        // trong EditMode, nên Tick/Add có thể chạy trước Awake. TestHelpers.Boot
        // chỉ Boot component được test gọi trực tiếp — sub-component được
        // PlayerStats.Awake AddComponent ra sẽ không có Awake chạy.
        void ResolveDeps()
        {
            if (stats == null) stats = GetComponent<PlayerStats>();
            if (statusManager == null) statusManager = GetComponent<StatusEffectManager>();
        }

        /// <summary>Tier hiện tại của Wetness.</summary>
        public WetnessTier CurrentTier => TierOf(Wetness);

        /// <summary>Map giá trị wetness → tier. Pure static — test friendly.</summary>
        public static WetnessTier TierOf(float wetness)
        {
            if (wetness >= 80f) return WetnessTier.Drenched;
            if (wetness >= 50f) return WetnessTier.Wet;
            if (wetness >= 20f) return WetnessTier.Damp;
            return WetnessTier.Dry;
        }

        /// <summary>Cộng wetness (vd splash khi uống nước, lội vũng). Clamp 0..max.</summary>
        public void Add(float amount)
        {
            Wetness = Mathf.Clamp(Wetness + amount, 0f, maxWetness);
        }

        /// <summary>Multiplier vào thermalDriftRate khi ambient lạnh hơn BodyTemp, theo tier ướt.</summary>
        public float ColdDriftMultiplier()
        {
            return CurrentTier switch
            {
                WetnessTier.Damp => dampColdDriftMultiplier,
                WetnessTier.Wet => wetColdDriftMultiplier,
                WetnessTier.Drenched => drenchedColdDriftMultiplier,
                _ => 1f,
            };
        }

        /// <summary>
        /// Pure tick — testable (không phụ thuộc TimeManager / Shelter static / Campfire).
        /// Caller cung cấp weather + sheltered + warm + dayLight (0..1).
        /// applySanityPenalty: trừ SAN theo tier hiện tại. applySicknessRoll: thử apply Sickness.
        /// </summary>
        public void Tick(float dt, Weather weather, bool sheltered, bool warm, float dayLight,
            bool applySanityPenalty = true, bool applySicknessRoll = true)
        {
            ResolveDeps();
            // Sources: rain/storm chỉ tăng Wetness nếu KHÔNG sheltered.
            float gain = 0f;
            if (!sheltered)
            {
                if (weather == Weather.Rain) gain = wetnessRainPerSec;
                else if (weather == Weather.Storm) gain = wetnessRainPerSec * Mathf.Max(1f, wetnessStormMultiplier);
            }

            // Sinks: base + fire + shelter + nắng (chỉ khi không bị mưa).
            float dry = wetnessDryBasePerSec;
            if (warm) dry += wetnessDryFireBonus;
            if (sheltered) dry += wetnessDryShelterBonus;
            if (gain <= 0f)
            {
                float dayBonus = wetnessDryDayBonus * Mathf.Clamp01(dayLight);
                dry += Mathf.Max(0f, dayBonus);
            }

            float net = gain - dry;
            Wetness = Mathf.Clamp(Wetness + net * dt, 0f, maxWetness);

            var tier = CurrentTier;

            if (applySanityPenalty && stats != null)
            {
                // Tier SAN penalty stack: Drenched cũng dính Wet penalty.
                if (tier >= WetnessTier.Wet)
                    stats.Sanity = Mathf.Max(0f, stats.Sanity - wetSanityPenaltyPerSec * dt);
                if (tier == WetnessTier.Drenched)
                    stats.Sanity = Mathf.Max(0f, stats.Sanity - drenchedSanityPenaltyPerSec * dt);
            }

            if (applySicknessRoll) TryRollSickness(dt, tier);
        }

        void TryRollSickness(float dt, WetnessTier tier)
        {
            if (tier != WetnessTier.Drenched) return;
            if (sicknessEffect == null || sicknessChancePerSec <= 0f) return;
            if (stats == null) return;

            float coldT = sicknessColdThreshold >= 0f
                ? sicknessColdThreshold
                : (stats.thermal != null ? stats.thermal.comfortMin : 30f);
            if (stats.BodyTemp >= coldT) return;
            if (Time.time < nextSicknessAllowedAt) return;

            // > thay >= để (a) cap saturation 100% khi chance*dt >= 1 (lag spike), (b) tránh
            // edge case Random.value = 1.0 (Unity docs: inclusive [0..1]) flake với chance=1.
            float roll = Random.value;
            if (roll > sicknessChancePerSec * dt) return;

            if (statusManager == null) statusManager = GetComponent<StatusEffectManager>();
            if (statusManager == null) return;

            statusManager.Apply(sicknessEffect);
            nextSicknessAllowedAt = Time.time + Mathf.Max(0f, sicknessApplyCooldownSec);
            Debug.Log($"[Wetness] Drenched + cold → áp Sickness ({sicknessEffect.displayName}).");
        }
    }
}
