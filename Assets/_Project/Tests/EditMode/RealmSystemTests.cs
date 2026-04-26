using NUnit.Framework;
using UnityEngine;
using WildernessCultivation.Cultivation;
using WildernessCultivation.Player;

namespace WildernessCultivation.Tests.EditMode
{
    /// <summary>
    /// EditMode tests cho RealmSystem: XP gain / breakthrough success-fail / temp bonus /
    /// reapply accumulated bonuses.
    /// Tests dùng custom realms[] với breakthroughChance ∈ {0, 1} để loại Random.value
    /// non-determinism.
    /// </summary>
    public class RealmSystemTests
    {
        GameObject go;
        RealmSystem realm;
        PlayerStats stats;

        static RealmSystem.RealmDefinition Tier(string name, float xp, float chance,
            float hp = 0, float mana = 0, float dmg = 0)
        {
            return new RealmSystem.RealmDefinition
            {
                name = name,
                xpRequired = xp,
                breakthroughChance = chance,
                hpBonus = hp,
                manaBonus = mana,
                damageBonus = dmg
            };
        }

        [SetUp]
        public void Setup()
        {
            go = new GameObject("Player");
            stats = go.AddComponent<PlayerStats>(); // baseMaxHP=100, etc.
            realm = go.AddComponent<RealmSystem>();
            // Awake đã set realms = default. Override sau Awake để controlled test data.
            realm.realms = new[]
            {
                Tier("T0",   0f, 1f),
                Tier("T1",  10f, 1f, hp: 10, mana: 5, dmg: 2),  // success guaranteed
                Tier("T2",  20f, 0f, hp: 20, mana: 10, dmg: 3), // failure guaranteed
                Tier("T3",  30f, 1f, hp: 30, mana: 15, dmg: 4),
            };
        }

        [TearDown]
        public void Teardown()
        {
            if (go != null) Object.DestroyImmediate(go);
        }

        // ===== AddCultivationXp =====

        [Test]
        public void AddCultivationXp_AddsToCurrentXp()
        {
            realm.AddCultivationXp(25f);
            Assert.AreEqual(25f, realm.currentXp, 0.01f);
            realm.AddCultivationXp(10f);
            Assert.AreEqual(35f, realm.currentXp, 0.01f);
        }

        [Test]
        public void Current_ClampsAtTopTier()
        {
            realm.currentTier = 999;
            Assert.AreEqual("T3", realm.Current.name);
        }

        [Test]
        public void HasNext_FalseAtLastTier()
        {
            realm.currentTier = 3; // last
            Assert.IsFalse(realm.HasNext);
            realm.currentTier = 0;
            Assert.IsTrue(realm.HasNext);
        }

        [Test]
        public void EffectiveNextXpRequired_NoNext_ReturnsZero()
        {
            realm.currentTier = 3;
            Assert.AreEqual(0f, realm.EffectiveNextXpRequired, 0.01f);
        }

        [Test]
        public void EffectiveNextXpRequired_NoSpiritRoot_EqualsRawXpRequired()
        {
            realm.currentTier = 0;
            // Next = T1 với xpRequired=10. Không có SpiritRoot component → mul = 1.
            Assert.AreEqual(10f, realm.EffectiveNextXpRequired, 0.01f);
        }

        // ===== TryBreakthrough =====

        [Test]
        public void TryBreakthrough_NoNextTier_ReturnsFalse()
        {
            realm.currentTier = 3;
            realm.currentXp = 9999f;
            Assert.IsFalse(realm.TryBreakthrough());
            Assert.AreEqual(3, realm.currentTier);
            Assert.AreEqual(9999f, realm.currentXp, 0.01f, "không tốn XP khi không có tier kế tiếp");
        }

        [Test]
        public void TryBreakthrough_InsufficientXp_ReturnsFalseAndDoesNotDeductXp()
        {
            realm.currentTier = 0;
            realm.currentXp = 5f; // < 10 required for T1
            Assert.IsFalse(realm.TryBreakthrough());
            Assert.AreEqual(0, realm.currentTier);
            Assert.AreEqual(5f, realm.currentXp, 0.01f);
        }

        [Test]
        public void TryBreakthrough_GuaranteedSuccess_AdvancesTierConsumesXp()
        {
            realm.currentTier = 0;
            realm.currentXp = 50f;

            int eventTier = -1;
            bool? eventResult = null;
            realm.OnRealmAdvanced += t => eventTier = t;
            realm.OnBreakthroughAttempted += s => eventResult = s;

            Assert.IsTrue(realm.TryBreakthrough());
            Assert.AreEqual(1, realm.currentTier);
            Assert.AreEqual(40f, realm.currentXp, 0.01f, "currentXp = 50 - 10 (cost) = 40");
            Assert.AreEqual(1, eventTier);
            Assert.IsTrue(eventResult ?? false);
        }

        [Test]
        public void TryBreakthrough_Success_AppliesBonuses_HPManaFullHeal()
        {
            realm.currentTier = 0;
            realm.currentXp = 100f;

            float baseMaxHP = stats.maxHP;
            float baseMaxMana = stats.maxMana;
            stats.HP = 30f;
            stats.Mana = 5f;

            Assert.IsTrue(realm.TryBreakthrough());
            // T1 hpBonus=10, manaBonus=5
            Assert.AreEqual(baseMaxHP + 10f, stats.maxHP, 0.01f);
            Assert.AreEqual(baseMaxMana + 5f, stats.maxMana, 0.01f);
            // Hồi đầy HP / Mana
            Assert.AreEqual(stats.maxHP, stats.HP, 0.01f);
            Assert.AreEqual(stats.maxMana, stats.Mana, 0.01f);
        }

        [Test]
        public void TryBreakthrough_GuaranteedFailure_RefundsPartialXpAndDropsSanity()
        {
            // Đẩy lên T1 rồi thử lên T2 (chance=0)
            realm.currentTier = 1;
            realm.currentXp = 100f;
            realm.xpRefundOnFailure = 0.5f;
            realm.failureSanityPenalty = 15f;

            stats.Sanity = 80f;

            int advancedFires = 0;
            bool? attemptedResult = null;
            realm.OnRealmAdvanced += _ => advancedFires++;
            realm.OnBreakthroughAttempted += s => attemptedResult = s;

            Assert.IsFalse(realm.TryBreakthrough());
            Assert.AreEqual(1, realm.currentTier, "không advance khi fail");
            // currentXp = 100 - 20 (cost) + 20*0.5 (refund) = 90
            Assert.AreEqual(90f, realm.currentXp, 0.01f);
            Assert.AreEqual(65f, stats.Sanity, 0.01f, "Sanity 80 - 15 = 65");
            Assert.AreEqual(0, advancedFires, "OnRealmAdvanced KHÔNG fire khi fail");
            Assert.IsFalse(attemptedResult ?? true, "OnBreakthroughAttempted fire với false");
        }

        // ===== Temporary breakthrough bonus =====

        [Test]
        public void AddTemporaryBreakthroughBonus_StacksMaxNotSum()
        {
            realm.AddTemporaryBreakthroughBonus(0.2f, 60f);
            realm.AddTemporaryBreakthroughBonus(0.5f, 60f); // larger
            Assert.AreEqual(0.5f, realm.TemporaryBreakthroughBonus, 0.01f);

            realm.AddTemporaryBreakthroughBonus(0.1f, 60f); // smaller — không hạ bonus
            Assert.AreEqual(0.5f, realm.TemporaryBreakthroughBonus, 0.01f);
        }

        [Test]
        public void AddTemporaryBreakthroughBonus_NonPositiveAmount_NoEffect()
        {
            realm.AddTemporaryBreakthroughBonus(0f, 10f);
            realm.AddTemporaryBreakthroughBonus(-0.5f, 10f);
            realm.AddTemporaryBreakthroughBonus(0.3f, 0f);
            Assert.AreEqual(0f, realm.TemporaryBreakthroughBonus, 0.01f);
        }

        [Test]
        public void TryBreakthrough_ClearsTemporaryBonusAfterAttempt()
        {
            realm.AddTemporaryBreakthroughBonus(0.4f, 60f);
            Assert.Greater(realm.TemporaryBreakthroughBonus, 0f);

            realm.currentTier = 0;
            realm.currentXp = 50f;
            realm.TryBreakthrough();

            // Bonus tiêu sau 1 lần attempt (success hay fail đều clear)
            Assert.AreEqual(0f, realm.TemporaryBreakthroughBonus, 0.01f);
        }

        // ===== ReapplyAccumulatedBonuses =====

        [Test]
        public void ReapplyAccumulatedBonuses_StacksBonusesFromTier1ToCurrent()
        {
            realm.currentTier = 3; // đã ở Tier 3 (giả lập đã đột phá qua T1+T2+T3)
            float baseMaxHP = stats.maxHP;
            float baseMaxMana = stats.maxMana;

            realm.ReapplyAccumulatedBonuses();
            // T1 hp+10, T2 hp+20, T3 hp+30 = +60
            Assert.AreEqual(baseMaxHP + 60f, stats.maxHP, 0.01f);
            // T1 mana+5, T2 mana+10, T3 mana+15 = +30
            Assert.AreEqual(baseMaxMana + 30f, stats.maxMana, 0.01f);
        }

        [Test]
        public void ReapplyAccumulatedBonuses_AtTier0_NoBonusApplied()
        {
            realm.currentTier = 0;
            float baseMaxHP = stats.maxHP;
            realm.ReapplyAccumulatedBonuses();
            Assert.AreEqual(baseMaxHP, stats.maxHP, 0.01f);
        }
    }
}
