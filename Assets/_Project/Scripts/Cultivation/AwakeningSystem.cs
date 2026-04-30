using System;
using UnityEngine;
using WildernessCultivation.Core;
using WildernessCultivation.Player;

namespace WildernessCultivation.Cultivation
{
    /// <summary>
    /// Khai mở tu tiên (awakening) — chạy khi player tương tác với 1 kì ngộ
    /// (Linh Tuyền / Linh Quả). Kiểm tra điều kiện cần (day, HP, sanity) rồi roll
    /// outcome theo bảng <see cref="AwakeningConfigSO"/>.
    ///
    /// Component này gắn vào Player GameObject. Trigger code (SpiritSpring, eat
    /// Linh Quả) gọi <see cref="TryAwaken(out AwakenResult)"/>; trigger sẽ tự consume
    /// regardless of outcome (kì ngộ chỉ dùng được 1 lần).
    /// </summary>
    [RequireComponent(typeof(PlayerStats))]
    public class AwakeningSystem : MonoBehaviour
    {
        public AwakeningConfigSO config;

        [Tooltip("Random seed cho test deterministic. -1 = dùng UnityEngine.Random global.")]
        public int testSeed = -1;

        public event Action<AwakenResult> OnAwakenAttempted;

        PlayerStats stats;
        SpiritRoot spiritRoot;
        RealmSystem realm;
        TimeManager timeManager;
        System.Random rng;

        void Awake()
        {
            stats = GetComponent<PlayerStats>();
            spiritRoot = GetComponent<SpiritRoot>();
            realm = GetComponent<RealmSystem>();
            if (config == null) config = AwakeningConfigSO.CreateDefault();
            if (testSeed >= 0) rng = new System.Random(testSeed);
            ServiceLocator.Register<AwakeningSystem>(this);
        }

        void OnDestroy() => ServiceLocator.Unregister<AwakeningSystem>(this);

        void Start()
        {
            timeManager = GameManager.Instance != null ? GameManager.Instance.timeManager : ServiceLocator.Get<TimeManager>();
        }

        /// <summary>Inject TimeManager cho EditMode test (tránh phụ thuộc Start).</summary>
        public void SetTimeManager(TimeManager t) => timeManager = t;

        /// <summary>Inject RNG seed cho EditMode test (deterministic outcome).</summary>
        public void SetSeed(int seed) => rng = new System.Random(seed);

        /// <summary>Eligibility check không trigger roll. Dùng cho UI hint hoặc trigger tự gate.</summary>
        public AwakenEligibility CheckEligibility()
        {
            if (stats == null) return AwakenEligibility.NoPlayer;
            if (stats.IsAwakened) return AwakenEligibility.AlreadyAwakened;
            int days = timeManager != null ? timeManager.daysSurvived : 0;
            if (days < config.minDaysSurvived) return AwakenEligibility.NotEnoughDays;
            float hpFrac = stats.maxHP > 0f ? stats.HP / stats.maxHP : 0f;
            if (hpFrac < config.minHpFraction) return AwakenEligibility.LowHP;
            if (stats.Sanity < config.minSanity) return AwakenEligibility.LowSanity;
            return AwakenEligibility.Eligible;
        }

        /// <summary>Trigger awakening attempt. Trả về true nếu đã ROLL (regardless of outcome).
        /// Trả về false nếu không đủ điều kiện cần (caller KHÔNG nên consume trigger).</summary>
        public bool TryAwaken(out AwakenResult result)
        {
            result = default;
            var eligibility = CheckEligibility();
            if (eligibility != AwakenEligibility.Eligible)
            {
                result.eligibility = eligibility;
                result.outcome = AwakenOutcome.Ineligible;
                OnAwakenAttempted?.Invoke(result);
                Core.GameEvents.RaiseAwakeningAttempted(result);
                return false;
            }

            result.eligibility = AwakenEligibility.Eligible;
            int streakBefore = stats != null ? stats.phamFailStreak : 0;
            result.outcome = RollOutcome(streakBefore);
            result.phamFailStreakBefore = streakBefore;

            if (result.outcome == AwakenOutcome.Fail)
            {
                if (stats != null) stats.phamFailStreak = streakBefore + 1;
                Debug.Log($"[Awaken] Linh khí tản mất, ngươi chưa đủ duyên... (pity streak={streakBefore + 1})");
            }
            else
            {
                if (stats != null) stats.phamFailStreak = 0;
                var rolledRoot = PickRootFor(result.outcome);
                result.spiritRoot = rolledRoot;
                ApplySuccess(rolledRoot);
                Debug.Log($"[Awaken] Khai mở thành công! [{result.outcome}] root={rolledRoot?.displayName ?? "<null>"}");
            }

            OnAwakenAttempted?.Invoke(result);
            Core.GameEvents.RaiseAwakeningAttempted(result);
            return true;
        }

        /// <summary>
        /// Roll outcome có pity. Mỗi fail Phàm liên tiếp giảm <see cref="AwakeningConfigSO.failChance"/>
        /// đi <see cref="AwakeningConfigSO.pityFailReductionPerStreak"/> (cap 0). Phần giảm được
        /// redistribute hoàn toàn vào Tạp — Đơn/Thiên giữ nguyên cho rare grades aspirational.
        /// Streak cap bởi <see cref="AwakeningConfigSO.pityMaxStreak"/>.
        /// </summary>
        public AwakenOutcome RollOutcome(int streak)
        {
            int clamped = Mathf.Clamp(streak, 0, Mathf.Max(0, config.pityMaxStreak));
            float reduction = config.pityFailReductionPerStreak * clamped;
            float effectiveFail = Mathf.Max(0f, config.failChance - reduction);
            float effectiveTap = config.tapChance + (config.failChance - effectiveFail);

            float r = NextFloat();
            float c = effectiveFail;
            if (r < c) return AwakenOutcome.Fail;
            c += effectiveTap;
            if (r < c) return AwakenOutcome.SuccessTap;
            c += config.donChance;
            if (r < c) return AwakenOutcome.SuccessDon;
            // Mọi giá trị còn lại = Thien (kể cả khi tổng < 1.0 để tránh lost-roll do float drift).
            return AwakenOutcome.SuccessThien;
        }

        /// <summary>Backward-compat overload — không pity (streak=0).</summary>
        AwakenOutcome RollOutcome() => RollOutcome(0);

        SpiritRootSO PickRootFor(AwakenOutcome outcome)
        {
            SpiritRootSO[] pool = outcome switch
            {
                AwakenOutcome.SuccessTap => config.tapRoots,
                AwakenOutcome.SuccessDon => config.donRoots,
                AwakenOutcome.SuccessThien => config.thienRoots,
                _ => null,
            };
            if (pool == null || pool.Length == 0) return null;
            int idx = NextInt(pool.Length);
            return pool[idx];
        }

        void ApplySuccess(SpiritRootSO rolledRoot)
        {
            stats.IsAwakened = true;
            if (rolledRoot != null && spiritRoot != null)
            {
                spiritRoot.SetSpiritRoot(rolledRoot);
                stats.ReapplySpiritRootMaxHP();
                if (realm != null) realm.ReapplyAccumulatedBonuses();
            }
            // Sanity check: ensure realm tier is at entry point (Phàm Nhân = 0).
            if (realm != null && realm.currentTier < 0) realm.currentTier = 0;
        }

        float NextFloat()
        {
            if (rng != null) return (float)rng.NextDouble();
            return UnityEngine.Random.value;
        }

        int NextInt(int upperExclusive)
        {
            if (upperExclusive <= 0) return 0;
            if (rng != null) return rng.Next(upperExclusive);
            return UnityEngine.Random.Range(0, upperExclusive);
        }
    }

    public enum AwakenEligibility
    {
        Eligible,
        AlreadyAwakened,
        NotEnoughDays,
        LowHP,
        LowSanity,
        NoPlayer,
    }

    public enum AwakenOutcome
    {
        Ineligible,
        Fail,
        SuccessTap,
        SuccessDon,
        SuccessThien,
    }

    public struct AwakenResult
    {
        public AwakenEligibility eligibility;
        public AwakenOutcome outcome;
        public SpiritRootSO spiritRoot;
        /// <summary>Streak Phàm trước khi roll (snapshot để debug/UI).</summary>
        public int phamFailStreakBefore;
    }
}
