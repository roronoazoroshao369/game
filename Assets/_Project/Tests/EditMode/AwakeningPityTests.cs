using NUnit.Framework;
using UnityEngine;
using WildernessCultivation.Core;
using WildernessCultivation.Cultivation;
using WildernessCultivation.Player;

namespace WildernessCultivation.Tests.EditMode
{
    /// <summary>
    /// Pity system: mỗi fail Phàm liên tiếp giảm failChance đi
    /// pityFailReductionPerStreak (cap 0). Reset 0 khi success bất kỳ grade.
    /// Per-run scope (PlayerStats persists qua main save → wipe khi permadeath).
    /// </summary>
    public class AwakeningPityTests
    {
        GameObject playerGo;
        GameObject timeGo;
        PlayerStats stats;
        SpiritRoot spiritRoot;
        AwakeningSystem awaken;
        AwakeningConfigSO config;
        TimeManager time;

        [SetUp]
        public void Setup()
        {
            playerGo = new GameObject("Player");
            stats = playerGo.AddComponent<PlayerStats>();
            playerGo.AddComponent<RealmSystem>();
            spiritRoot = playerGo.AddComponent<SpiritRoot>();
            awaken = playerGo.AddComponent<AwakeningSystem>();
            TestHelpers.Boot(stats, spiritRoot, awaken);

            timeGo = new GameObject("Time");
            time = timeGo.AddComponent<TimeManager>();
            time.daysSurvived = 10;

            config = AwakeningConfigSO.CreateDefault();
            var tap = SpiritRootSO.CreateDefault(); tap.name = "Tap"; tap.grade = SpiritRootGrade.Tap;
            var don = SpiritRootSO.CreateDefault(); don.name = "Don"; don.grade = SpiritRootGrade.Don;
            var thien = SpiritRootSO.CreateDefault(); thien.name = "Thien"; thien.grade = SpiritRootGrade.Thien;
            config.tapRoots = new[] { tap };
            config.donRoots = new[] { don };
            config.thienRoots = new[] { thien };
            awaken.config = config;
            awaken.SetTimeManager(time);

            stats.HP = stats.maxHP;
            stats.Sanity = 100f;
            stats.IsAwakened = false;
            stats.phamFailStreak = 0;
        }

        [TearDown]
        public void Teardown()
        {
            if (playerGo != null) Object.DestroyImmediate(playerGo);
            if (timeGo != null) Object.DestroyImmediate(timeGo);
            if (config != null) Object.DestroyImmediate(config);
        }

        [Test]
        public void Fail_IncrementsStreak()
        {
            // Force fail.
            config.failChance = 1f;
            config.tapChance = 0f;
            config.donChance = 0f;
            config.thienChance = 0f;
            awaken.TryAwaken(out var r1);
            Assert.AreEqual(AwakenOutcome.Fail, r1.outcome);
            Assert.AreEqual(0, r1.phamFailStreakBefore);
            Assert.AreEqual(1, stats.phamFailStreak);

            stats.IsAwakened = false; // re-eligible
            awaken.TryAwaken(out var r2);
            Assert.AreEqual(1, r2.phamFailStreakBefore);
            Assert.AreEqual(2, stats.phamFailStreak);
        }

        [Test]
        public void Success_ResetsStreakToZero()
        {
            stats.phamFailStreak = 3;
            // Force Tap success.
            config.failChance = 0f;
            config.tapChance = 1f;
            config.donChance = 0f;
            config.thienChance = 0f;
            awaken.TryAwaken(out var r);
            Assert.AreEqual(AwakenOutcome.SuccessTap, r.outcome);
            Assert.AreEqual(3, r.phamFailStreakBefore);
            Assert.AreEqual(0, stats.phamFailStreak, "Success bất kỳ grade phải reset streak về 0");
        }

        [Test]
        public void RollOutcome_Streak0_DistributesByBaseConfig()
        {
            const int n = 30000;
            int fail = 0, tap = 0;
            awaken.SetSeed(7);
            for (int i = 0; i < n; i++)
            {
                var o = awaken.RollOutcome(0);
                if (o == AwakenOutcome.Fail) fail++;
                if (o == AwakenOutcome.SuccessTap) tap++;
            }
            Assert.AreEqual(0.50f, (float)fail / n, 0.02f, "Streak=0 → fail ~50%");
            Assert.AreEqual(0.35f, (float)tap / n, 0.02f, "Streak=0 → tap ~35%");
        }

        [Test]
        public void RollOutcome_Streak2_FailDropsBy20PercentRedistributedToTap()
        {
            const int n = 30000;
            int fail = 0, tap = 0, don = 0, thien = 0;
            awaken.SetSeed(11);
            for (int i = 0; i < n; i++)
            {
                var o = awaken.RollOutcome(2);
                switch (o)
                {
                    case AwakenOutcome.Fail: fail++; break;
                    case AwakenOutcome.SuccessTap: tap++; break;
                    case AwakenOutcome.SuccessDon: don++; break;
                    case AwakenOutcome.SuccessThien: thien++; break;
                }
            }
            // 50% - 0.10 * 2 = 30% fail. Redistribute 20% vào tap → 35% + 20% = 55%.
            Assert.AreEqual(0.30f, (float)fail / n, 0.02f, "Streak=2 → fail ~30%");
            Assert.AreEqual(0.55f, (float)tap / n, 0.02f, "Streak=2 → tap ~55%");
            Assert.AreEqual(0.13f, (float)don / n, 0.02f, "don giữ ~13%");
            Assert.AreEqual(0.02f, (float)thien / n, 0.01f, "thien giữ ~2%");
        }

        [Test]
        public void RollOutcome_Streak5_FailIsZero_TapAbsorbsAllFailWeight()
        {
            const int n = 30000;
            int fail = 0, tap = 0, don = 0, thien = 0;
            awaken.SetSeed(99);
            for (int i = 0; i < n; i++)
            {
                var o = awaken.RollOutcome(5);
                switch (o)
                {
                    case AwakenOutcome.Fail: fail++; break;
                    case AwakenOutcome.SuccessTap: tap++; break;
                    case AwakenOutcome.SuccessDon: don++; break;
                    case AwakenOutcome.SuccessThien: thien++; break;
                }
            }
            Assert.AreEqual(0, fail, "Streak=5 (=pityMaxStreak) → fail PHẢI = 0 (guaranteed pass)");
            Assert.AreEqual(0.85f, (float)tap / n, 0.02f, "Streak=5 → tap ~85% (35% + 50% redistribution)");
            Assert.AreEqual(0.13f, (float)don / n, 0.02f);
            Assert.AreEqual(0.02f, (float)thien / n, 0.01f);
        }

        [Test]
        public void RollOutcome_StreakAbovePityMax_ClampsToPityMax()
        {
            // streak=99 → clamp to pityMaxStreak=5 → fail = 0.
            const int n = 5000;
            int fail = 0;
            awaken.SetSeed(1);
            for (int i = 0; i < n; i++)
            {
                if (awaken.RollOutcome(99) == AwakenOutcome.Fail) fail++;
            }
            Assert.AreEqual(0, fail, "Streak vượt pityMaxStreak phải clamp, không phải over-reduce sang negative.");
        }

        [Test]
        public void Ineligible_DoesNotAffectStreak()
        {
            stats.phamFailStreak = 2;
            time.daysSurvived = 1; // ineligible
            bool rolled = awaken.TryAwaken(out var r);
            Assert.IsFalse(rolled);
            Assert.AreEqual(2, stats.phamFailStreak, "Ineligible không trigger roll → streak không thay đổi");
        }

        [Test]
        public void EndToEnd_FiveFails_ThenGuaranteedSuccess()
        {
            // 5 fail liên tiếp với seed cố định + force config Phàm 100%/Tạp 0%.
            // Sau 5 fail, set lại config về default + streak ở pityMaxStreak → fail PHẢI = 0.
            config.failChance = 1f;
            config.tapChance = 0f;
            config.donChance = 0f;
            config.thienChance = 0f;
            for (int i = 0; i < 5; i++)
            {
                stats.IsAwakened = false;
                awaken.TryAwaken(out _);
            }
            Assert.AreEqual(5, stats.phamFailStreak);

            // Restore default — bây giờ streak=5 phải đảm bảo pass (Phàm effective = 0).
            config.failChance = 0.50f;
            config.tapChance = 0.35f;
            config.donChance = 0.13f;
            config.thienChance = 0.02f;
            stats.IsAwakened = false;
            awaken.TryAwaken(out var r);
            Assert.AreNotEqual(AwakenOutcome.Fail, r.outcome,
                "Sau 5 fail liên tiếp, streak=pityMaxStreak → fail effective = 0 → guaranteed success");
            Assert.IsTrue(stats.IsAwakened);
            Assert.AreEqual(0, stats.phamFailStreak, "Success reset streak");
        }
    }
}
