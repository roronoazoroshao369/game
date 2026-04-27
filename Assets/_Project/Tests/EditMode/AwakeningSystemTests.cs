using NUnit.Framework;
using UnityEngine;
using WildernessCultivation.Core;
using WildernessCultivation.Cultivation;
using WildernessCultivation.Player;

namespace WildernessCultivation.Tests.EditMode
{
    /// <summary>
    /// EditMode tests cho AwakeningSystem: eligibility gating, roll outcome distribution,
    /// success applies spirit root + IsAwakened.
    /// </summary>
    public class AwakeningSystemTests
    {
        GameObject playerGo;
        GameObject timeGo;
        PlayerStats stats;
        RealmSystem realm;
        SpiritRoot spiritRoot;
        AwakeningSystem awaken;
        AwakeningConfigSO config;
        TimeManager time;

        [SetUp]
        public void Setup()
        {
            playerGo = new GameObject("Player");
            stats = playerGo.AddComponent<PlayerStats>();
            realm = playerGo.AddComponent<RealmSystem>();
            spiritRoot = playerGo.AddComponent<SpiritRoot>();
            awaken = playerGo.AddComponent<AwakeningSystem>();
            TestHelpers.Boot(stats, realm, spiritRoot, awaken);

            timeGo = new GameObject("Time");
            time = timeGo.AddComponent<TimeManager>();

            config = AwakeningConfigSO.CreateDefault();
            // Pool: 1 root mỗi grade.
            var tap = SpiritRootSO.CreateDefault();
            tap.name = "Tap";
            tap.grade = SpiritRootGrade.Tap;
            var don = SpiritRootSO.CreateDefault();
            don.name = "Don";
            don.grade = SpiritRootGrade.Don;
            var thien = SpiritRootSO.CreateDefault();
            thien.name = "Thien";
            thien.grade = SpiritRootGrade.Thien;
            config.tapRoots = new[] { tap };
            config.donRoots = new[] { don };
            config.thienRoots = new[] { thien };
            awaken.config = config;
            awaken.SetTimeManager(time);

            // Default state: day 10, full HP, full sanity → eligible.
            time.daysSurvived = 10;
            stats.HP = stats.maxHP;
            stats.Sanity = 100f;
            stats.IsAwakened = false;
        }

        [TearDown]
        public void Teardown()
        {
            if (playerGo != null) Object.DestroyImmediate(playerGo);
            if (timeGo != null) Object.DestroyImmediate(timeGo);
            if (config != null) Object.DestroyImmediate(config);
        }

        [Test]
        public void CheckEligibility_BlockedBeforeMinDay()
        {
            time.daysSurvived = 6;
            Assert.AreEqual(AwakenEligibility.NotEnoughDays, awaken.CheckEligibility());
        }

        [Test]
        public void CheckEligibility_BlockedWhenLowHP()
        {
            stats.HP = stats.maxHP * 0.4f;
            Assert.AreEqual(AwakenEligibility.LowHP, awaken.CheckEligibility());
        }

        [Test]
        public void CheckEligibility_BlockedWhenLowSanity()
        {
            stats.Sanity = 30f;
            Assert.AreEqual(AwakenEligibility.LowSanity, awaken.CheckEligibility());
        }

        [Test]
        public void CheckEligibility_BlockedWhenAlreadyAwakened()
        {
            stats.IsAwakened = true;
            Assert.AreEqual(AwakenEligibility.AlreadyAwakened, awaken.CheckEligibility());
        }

        [Test]
        public void CheckEligibility_EligibleAtDefaultGoodState()
        {
            Assert.AreEqual(AwakenEligibility.Eligible, awaken.CheckEligibility());
        }

        [Test]
        public void TryAwaken_Ineligible_ReturnsFalse_NoAwaken()
        {
            time.daysSurvived = 3;
            bool rolled = awaken.TryAwaken(out var r);
            Assert.IsFalse(rolled);
            Assert.AreEqual(AwakenOutcome.Ineligible, r.outcome);
            Assert.IsFalse(stats.IsAwakened);
        }

        [Test]
        public void TryAwaken_Force0Roll_FailOutcome()
        {
            // Seed picks chosen so first NextDouble() = small value < 0.5 (failChance).
            awaken.SetSeed(42);
            // Override config: failChance = 1.0 to force fail regardless of seed.
            config.failChance = 1f;
            config.tapChance = 0f;
            config.donChance = 0f;
            config.thienChance = 0f;
            bool rolled = awaken.TryAwaken(out var r);
            Assert.IsTrue(rolled);
            Assert.AreEqual(AwakenOutcome.Fail, r.outcome);
            Assert.IsFalse(stats.IsAwakened, "Fail KHÔNG awaken");
        }

        [Test]
        public void TryAwaken_ForceTap_SetsIsAwakenedAndRoot()
        {
            config.failChance = 0f;
            config.tapChance = 1f;
            config.donChance = 0f;
            config.thienChance = 0f;
            bool rolled = awaken.TryAwaken(out var r);
            Assert.IsTrue(rolled);
            Assert.AreEqual(AwakenOutcome.SuccessTap, r.outcome);
            Assert.IsTrue(stats.IsAwakened);
            Assert.IsNotNull(r.spiritRoot);
            Assert.AreSame(r.spiritRoot, spiritRoot.Current,
                "SpiritRoot.Current phải = root vừa roll");
        }

        [Test]
        public void TryAwaken_ForceThien_SetsThienRoot()
        {
            config.failChance = 0f;
            config.tapChance = 0f;
            config.donChance = 0f;
            config.thienChance = 1f;
            awaken.TryAwaken(out var r);
            Assert.AreEqual(AwakenOutcome.SuccessThien, r.outcome);
            Assert.AreEqual(SpiritRootGrade.Thien, r.spiritRoot.grade);
        }

        [Test]
        public void RollDistribution_50kRolls_MatchesProbabilityWithin2Percent()
        {
            // Seed deterministic. Reset IsAwakened+grade pool roll thông qua
            // direct RollOutcome (private) — test qua TryAwaken nhưng reset state mỗi lần.
            const int n = 50000;
            int fail = 0, tap = 0, don = 0, thien = 0;
            awaken.SetSeed(12345);
            for (int i = 0; i < n; i++)
            {
                stats.IsAwakened = false; // reset để eligibility lại Eligible
                stats.phamFailStreak = 0; // không tính pity vào distribution baseline
                bool rolled = awaken.TryAwaken(out var r);
                Assert.IsTrue(rolled);
                switch (r.outcome)
                {
                    case AwakenOutcome.Fail: fail++; break;
                    case AwakenOutcome.SuccessTap: tap++; break;
                    case AwakenOutcome.SuccessDon: don++; break;
                    case AwakenOutcome.SuccessThien: thien++; break;
                }
            }
            float pFail = (float)fail / n;
            float pTap = (float)tap / n;
            float pDon = (float)don / n;
            float pThien = (float)thien / n;
            Assert.AreEqual(0.50f, pFail, 0.02f, $"failChance ~50% (got {pFail:P2})");
            Assert.AreEqual(0.35f, pTap, 0.02f, $"tapChance ~35% (got {pTap:P2})");
            Assert.AreEqual(0.13f, pDon, 0.02f, $"donChance ~13% (got {pDon:P2})");
            Assert.AreEqual(0.02f, pThien, 0.01f, $"thienChance ~2% (got {pThien:P2})");
        }
    }
}
