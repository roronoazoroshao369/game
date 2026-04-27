using NUnit.Framework;
using UnityEngine;
using WildernessCultivation.Core;
using WildernessCultivation.Player;
using WildernessCultivation.Player.Status;

namespace WildernessCultivation.Tests.EditMode
{
    /// <summary>
    /// Wetness deep system: gauge [0..100] + 4 tier (Dry / Damp / Wet / Drenched).
    /// Rain/Storm tăng, fire/shelter/nắng giảm. Tier càng cao → cold drift × multiplier
    /// + tụt SAN; Drenched + cold → roll Sickness status.
    ///
    /// Test pure surface qua public TickWetness(dt, weather, sheltered, warm, dayLight)
    /// — không phụ thuộc TimeManager / Shelter static / Campfire để chạy deterministic.
    /// </summary>
    public class WetnessTests
    {
        GameObject go;
        PlayerStats stats;
        StatusEffectManager statusManager;

        [SetUp]
        public void Setup()
        {
            go = new GameObject("Player");
            stats = go.AddComponent<PlayerStats>();
            statusManager = go.AddComponent<StatusEffectManager>();
            TestHelpers.Boot(stats, statusManager);
            stats.Wetness = 0f;
            stats.Sanity = stats.maxSanity;
            stats.BodyTemp = 50f;
        }

        [TearDown]
        public void Teardown()
        {
            if (go != null) Object.DestroyImmediate(go);
        }

        // ===== Tier mapping =====

        [Test]
        public void WetnessTierOf_Boundaries()
        {
            Assert.AreEqual(WetnessTier.Dry, PlayerStats.WetnessTierOf(0f));
            Assert.AreEqual(WetnessTier.Dry, PlayerStats.WetnessTierOf(19.999f));
            Assert.AreEqual(WetnessTier.Damp, PlayerStats.WetnessTierOf(20f));
            Assert.AreEqual(WetnessTier.Damp, PlayerStats.WetnessTierOf(49.999f));
            Assert.AreEqual(WetnessTier.Wet, PlayerStats.WetnessTierOf(50f));
            Assert.AreEqual(WetnessTier.Wet, PlayerStats.WetnessTierOf(79.999f));
            Assert.AreEqual(WetnessTier.Drenched, PlayerStats.WetnessTierOf(80f));
            Assert.AreEqual(WetnessTier.Drenched, PlayerStats.WetnessTierOf(100f));
        }

        [Test]
        public void CurrentWetnessTier_ReflectsField()
        {
            stats.Wetness = 30f;
            Assert.AreEqual(WetnessTier.Damp, stats.CurrentWetnessTier);
            stats.Wetness = 60f;
            Assert.AreEqual(WetnessTier.Wet, stats.CurrentWetnessTier);
            stats.Wetness = 90f;
            Assert.AreEqual(WetnessTier.Drenched, stats.CurrentWetnessTier);
        }

        // ===== AddWetness =====

        [Test]
        public void AddWetness_AccumulatesAndClampsToMax()
        {
            stats.AddWetness(30f);
            Assert.AreEqual(30f, stats.Wetness, 0.01f);
            stats.AddWetness(80f);
            Assert.AreEqual(stats.maxWetness, stats.Wetness, 0.01f);
        }

        [Test]
        public void AddWetness_NegativeClampsToZero()
        {
            stats.Wetness = 10f;
            stats.AddWetness(-50f);
            Assert.AreEqual(0f, stats.Wetness, 0.01f);
        }

        // ===== Cold drift coupling =====

        [Test]
        public void WetnessColdDriftMultiplier_PerTier()
        {
            stats.Wetness = 0f;
            Assert.AreEqual(1f, stats.WetnessColdDriftMultiplier(), 0.001f, "Dry → 1× (no extra cold drift).");
            stats.Wetness = 30f;
            Assert.AreEqual(stats.dampColdDriftMultiplier, stats.WetnessColdDriftMultiplier(), 0.001f);
            stats.Wetness = 60f;
            Assert.AreEqual(stats.wetColdDriftMultiplier, stats.WetnessColdDriftMultiplier(), 0.001f);
            stats.Wetness = 90f;
            Assert.AreEqual(stats.drenchedColdDriftMultiplier, stats.WetnessColdDriftMultiplier(), 0.001f);
        }

        // ===== TickWetness — sources =====

        [Test]
        public void TickWetness_RainAccumulates_WhenNotSheltered()
        {
            // dt = 1s, rain, not sheltered → gain = wetnessRainPerSec, dry = base only (no fire/shelter/sun).
            stats.TickWetness(1f, Weather.Rain, sheltered: false, warm: false, dayLight: 0f,
                applySanityPenalty: false, applySicknessRoll: false);
            float expected = stats.wetnessRainPerSec - stats.wetnessDryBasePerSec;
            Assert.AreEqual(expected, stats.Wetness, 0.01f);
        }

        [Test]
        public void TickWetness_StormFasterThanRain()
        {
            stats.Wetness = 0f;
            stats.TickWetness(1f, Weather.Rain, false, false, 0f, false, false);
            float rainGain = stats.Wetness;

            stats.Wetness = 0f;
            stats.TickWetness(1f, Weather.Storm, false, false, 0f, false, false);
            float stormGain = stats.Wetness;

            Assert.Greater(stormGain, rainGain, "Storm phải làm ướt nhanh hơn Rain (multiplier > 1).");
        }

        [Test]
        public void TickWetness_Sheltered_RainDoesNotGain_AndDriesFaster()
        {
            stats.Wetness = 50f;
            stats.TickWetness(1f, Weather.Rain, sheltered: true, warm: false, dayLight: 0f,
                applySanityPenalty: false, applySicknessRoll: false);
            // Sheltered: gain=0, dry = base + shelterBonus
            float expectedDry = stats.wetnessDryBasePerSec + stats.wetnessDryShelterBonus;
            Assert.AreEqual(50f - expectedDry, stats.Wetness, 0.01f);
        }

        // ===== TickWetness — sinks =====

        [Test]
        public void TickWetness_FireDriesQuickly()
        {
            stats.Wetness = 80f;
            stats.TickWetness(1f, Weather.Clear, sheltered: false, warm: true, dayLight: 0f,
                applySanityPenalty: false, applySicknessRoll: false);
            float expectedDry = stats.wetnessDryBasePerSec + stats.wetnessDryFireBonus + 0f;
            Assert.AreEqual(80f - expectedDry, stats.Wetness, 0.01f);
        }

        [Test]
        public void TickWetness_DayLight_HelpsDrying_WhenNotRaining()
        {
            stats.Wetness = 50f;
            stats.TickWetness(1f, Weather.Clear, false, false, dayLight: 1f, false, false);
            float expectedDry = stats.wetnessDryBasePerSec + stats.wetnessDryDayBonus;
            Assert.AreEqual(50f - expectedDry, stats.Wetness, 0.01f);
        }

        [Test]
        public void TickWetness_DayLight_NoBonus_WhenRaining()
        {
            stats.Wetness = 50f;
            // Rain + dayLight=1: nắng không trừ, chỉ rain - base.
            stats.TickWetness(1f, Weather.Rain, false, false, dayLight: 1f, false, false);
            float expectedNet = stats.wetnessRainPerSec - stats.wetnessDryBasePerSec;
            Assert.AreEqual(50f + expectedNet, stats.Wetness, 0.01f);
        }

        [Test]
        public void TickWetness_ClampsToZero()
        {
            stats.Wetness = 0.1f;
            stats.TickWetness(1f, Weather.Clear, false, true, 1f, false, false);
            Assert.AreEqual(0f, stats.Wetness, 0.01f);
        }

        [Test]
        public void TickWetness_ClampsToMax()
        {
            stats.Wetness = stats.maxWetness - 0.1f;
            // Storm spam 100s → vẫn cap ở max.
            stats.TickWetness(100f, Weather.Storm, false, false, 0f, false, false);
            Assert.AreEqual(stats.maxWetness, stats.Wetness, 0.01f);
        }

        // ===== SAN penalty =====

        [Test]
        public void TickWetness_DrySanity_NoPenalty()
        {
            stats.Wetness = 0f;
            stats.Sanity = 100f;
            stats.TickWetness(1f, Weather.Clear, false, false, 0f, applySanityPenalty: true, applySicknessRoll: false);
            Assert.AreEqual(100f, stats.Sanity, 0.01f, "Dry tier → no SAN penalty.");
        }

        [Test]
        public void TickWetness_WetTier_AppliesSanityPenalty()
        {
            stats.Wetness = 60f;
            stats.Sanity = 100f;
            stats.TickWetness(1f, Weather.Clear, sheltered: true, warm: true, dayLight: 0f,
                applySanityPenalty: true, applySicknessRoll: false);
            // Wet (chưa Drenched) → chỉ trừ wetSanityPenaltyPerSec.
            Assert.Less(stats.Sanity, 100f);
            Assert.AreEqual(100f - stats.wetSanityPenaltyPerSec, stats.Sanity, 0.05f);
        }

        [Test]
        public void TickWetness_DrenchedTier_AppliesBothWetAndDrenchedPenalty()
        {
            stats.Wetness = 90f;
            stats.Sanity = 100f;
            stats.TickWetness(1f, Weather.Clear, sheltered: true, warm: true, dayLight: 0f,
                applySanityPenalty: true, applySicknessRoll: false);
            float total = stats.wetSanityPenaltyPerSec + stats.drenchedSanityPenaltyPerSec;
            Assert.AreEqual(100f - total, stats.Sanity, 0.05f, "Drenched chồng cả Wet penalty.");
        }

        // ===== Sickness chain =====

        [Test]
        public void TickWetness_SicknessChain_NotTriggered_WhenDryAndCold()
        {
            stats.Wetness = 10f; // Dry
            stats.BodyTemp = 5f;
            stats.sicknessChancePerSec = 1f;
            stats.sicknessEffect = MakeSicknessEffect();

            stats.TickWetness(1f, Weather.Clear, false, false, 0f, applySanityPenalty: false, applySicknessRoll: true);
            Assert.IsFalse(statusManager.HasEffect("test_sickness"), "Dry + cold → không trigger Sickness.");
        }

        [Test]
        public void TickWetness_SicknessChain_NotTriggered_WhenDrenchedButWarm()
        {
            stats.Wetness = 90f;
            stats.BodyTemp = 60f; // > comfortMin
            stats.sicknessChancePerSec = 1f;
            stats.sicknessEffect = MakeSicknessEffect();

            stats.TickWetness(1f, Weather.Rain, false, false, 0f, applySanityPenalty: false, applySicknessRoll: true);
            Assert.IsFalse(statusManager.HasEffect("test_sickness"), "Drenched nhưng ấm → không Sickness.");
        }

        [Test]
        public void TickWetness_SicknessChain_Triggers_WhenDrenchedAndCold()
        {
            stats.Wetness = 90f;
            stats.BodyTemp = 5f; // < comfortMin (30) → cold
            stats.sicknessChancePerSec = 1f; // 100% / sec, dt=1 → roll < 1.0 always pass.
            stats.sicknessEffect = MakeSicknessEffect();

            stats.TickWetness(1f, Weather.Rain, false, false, 0f, applySanityPenalty: false, applySicknessRoll: true);
            Assert.IsTrue(statusManager.HasEffect("test_sickness"), "Drenched + cold + chance=1 → áp Sickness.");
        }

        [Test]
        public void TickWetness_SicknessChain_NoEffect_WhenChanceZero()
        {
            stats.Wetness = 90f;
            stats.BodyTemp = 5f;
            stats.sicknessChancePerSec = 0f; // disabled
            stats.sicknessEffect = MakeSicknessEffect();

            stats.TickWetness(1f, Weather.Rain, false, false, 0f, applySanityPenalty: false, applySicknessRoll: true);
            Assert.IsFalse(statusManager.HasEffect("test_sickness"), "chance=0 → tắt chain.");
        }

        [Test]
        public void TickWetness_SicknessChain_NoEffect_WhenEffectNull()
        {
            stats.Wetness = 90f;
            stats.BodyTemp = 5f;
            stats.sicknessChancePerSec = 1f;
            stats.sicknessEffect = null;

            // Phải KHÔNG throw exception.
            Assert.DoesNotThrow(() => stats.TickWetness(1f, Weather.Rain, false, false, 0f,
                applySanityPenalty: false, applySicknessRoll: true));
        }

        // ===== Helpers =====

        StatusEffectSO MakeSicknessEffect()
        {
            var so = ScriptableObject.CreateInstance<StatusEffectSO>();
            so.effectId = "test_sickness";
            so.displayName = "Test Sickness";
            so.type = StatusEffectType.Sickness;
            so.tickIntervalSec = 1f;
            so.defaultDurationSec = 30f;
            so.hpDamagePerTick = 1f;
            return so;
        }
    }
}
