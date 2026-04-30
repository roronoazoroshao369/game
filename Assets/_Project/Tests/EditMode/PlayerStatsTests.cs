using NUnit.Framework;
using UnityEngine;
using WildernessCultivation.Combat;
using WildernessCultivation.Player;

namespace WildernessCultivation.Tests.EditMode
{
    /// <summary>
    /// EditMode tests cho public API deterministic của PlayerStats: TakeDamage / Heal / Eat /
    /// Drink / RestoreSanity / TryConsumeMana / AddMana / AddShield / SetInvulnerable / Die.
    ///
    /// KHÔNG test Update() (depends Time.deltaTime + TimeManager + SpiritRoot + Campfire +
    /// Shelter + LightSource — quá nhiều system runtime). Update() invariants nên cover
    /// bằng PlayMode tests sau.
    /// </summary>
    public class PlayerStatsTests
    {
        GameObject go;
        PlayerStats stats;

        [SetUp]
        public void Setup()
        {
            go = new GameObject("Player");
            stats = go.AddComponent<PlayerStats>();
            // Awake đã chạy: stats.maxHP = 100, HP = 100, etc. (default field).
        }

        [TearDown]
        public void Teardown()
        {
            if (go != null) Object.DestroyImmediate(go);
        }

        // ===== TakeDamage / TakeDamageRaw =====

        [Test]
        public void TakeDamage_ReducesHP()
        {
            stats.TakeDamage(30f);
            Assert.AreEqual(70f, stats.HP, 0.01f);
        }

        [Test]
        public void TakeDamage_OverkillClampsToZero()
        {
            stats.TakeDamage(999f);
            Assert.AreEqual(0f, stats.HP, 0.01f);
            Assert.IsTrue(stats.IsDead);
        }

        [Test]
        public void TakeDamage_TriggersOnDeath_OnceAtHPZero()
        {
            int deathCount = 0;
            stats.OnDeath += () => deathCount++;

            stats.TakeDamage(100f);
            Assert.AreEqual(1, deathCount);

            // Damage thêm khi đã chết — không trigger OnDeath nữa (TakeDamageRaw guard IsDead)
            stats.TakeDamageRaw(10f);
            Assert.AreEqual(1, deathCount);
        }

        [Test]
        public void TakeDamage_WhileInvulnerable_NoEffect()
        {
            stats.SetInvulnerable(10f);
            Assert.IsTrue(stats.IsInvulnerable);

            stats.TakeDamage(50f);
            Assert.AreEqual(100f, stats.HP, 0.01f, "i-frames block dame ngoài");
        }

        [Test]
        public void TakeDamageRaw_BypassesInvulnerability()
        {
            stats.SetInvulnerable(10f);
            stats.TakeDamageRaw(30f);
            // TakeDamageRaw không check IsInvulnerable — chỉ check IsDead
            Assert.AreEqual(70f, stats.HP, 0.01f, "TakeDamageRaw không bị i-frames chặn (dùng cho status tick)");
        }

        // ===== IDamageable contract (R2) =====

        [Test]
        public void ImplementsIDamageable()
        {
            Assert.IsInstanceOf<IDamageable>(stats, "PlayerStats phải implement IDamageable (R2 refactor — bỏ fallback ở 6 mob AI).");
        }

        [Test]
        public void TakeDamage_ViaIDamageable_ReducesHP()
        {
            IDamageable dmg = stats;
            dmg.TakeDamage(25f, go);
            Assert.AreEqual(75f, stats.HP, 0.01f);
        }

        [Test]
        public void TakeDamage_ViaIDamageable_RespectsInvulnerability()
        {
            stats.SetInvulnerable(10f);
            IDamageable dmg = stats;
            dmg.TakeDamage(50f, go);
            Assert.AreEqual(100f, stats.HP, 0.01f, "interface entry phải đi qua i-frame guard giống overload TakeDamage(float).");
        }

        // ===== Shield =====

        [Test]
        public void AddShield_StacksAmount()
        {
            stats.AddShield(20f, 5f);
            stats.AddShield(15f, 5f);
            Assert.AreEqual(35f, stats.Shield, 0.01f);
        }

        [Test]
        public void AddShield_KeepsLongerDuration()
        {
            stats.AddShield(10f, 100f);
            float longEnd = stats.ShieldEndsAt;
            stats.AddShield(10f, 1f);   // duration ngắn hơn
            Assert.AreEqual(longEnd, stats.ShieldEndsAt, 0.01f, "không cho duration ngắn ghi đè");
        }

        [Test]
        public void AddShield_NonPositiveAmount_NoEffect()
        {
            stats.AddShield(0f, 5f);
            stats.AddShield(-10f, 5f);
            stats.AddShield(20f, 0f);
            Assert.AreEqual(0f, stats.Shield, 0.01f);
        }

        [Test]
        public void TakeDamage_AbsorbsFromShieldFirst()
        {
            stats.AddShield(30f, 10f);
            Assert.IsTrue(stats.HasShield);

            stats.TakeDamage(20f);
            Assert.AreEqual(10f, stats.Shield, 0.01f, "shield 30 - dmg 20 = 10");
            Assert.AreEqual(100f, stats.HP, 0.01f, "HP không trừ khi shield đỡ hết");

            stats.TakeDamage(15f);
            Assert.AreEqual(0f, stats.Shield, 0.01f);
            Assert.AreEqual(95f, stats.HP, 0.01f, "shield 10 đỡ → còn 5 dame vào HP");
        }

        // ===== SetInvulnerable =====

        [Test]
        public void SetInvulnerable_ExtendsExistingDuration()
        {
            stats.SetInvulnerable(0.5f);
            float t1 = stats.InvulnerableUntil;
            stats.SetInvulnerable(2f);
            Assert.Greater(stats.InvulnerableUntil, t1, "duration dài hơn ghi đè");
        }

        [Test]
        public void SetInvulnerable_ShorterDuration_DoesNotShrink()
        {
            stats.SetInvulnerable(10f);
            float t1 = stats.InvulnerableUntil;
            stats.SetInvulnerable(0.1f);
            Assert.AreEqual(t1, stats.InvulnerableUntil, 0.01f, "duration ngắn hơn không rút ngắn i-frames hiện tại");
        }

        [Test]
        public void SetInvulnerable_NegativeDuration_ClampedToZero()
        {
            stats.SetInvulnerable(-5f);
            // InvulnerableUntil = Time.time + max(0, -5) = Time.time → IsInvulnerable false (strict <)
            Assert.IsFalse(stats.IsInvulnerable);
        }

        // ===== Heal / Eat / Drink / RestoreSanity =====

        [Test]
        public void Heal_ClampsToMaxHP()
        {
            stats.HP = 50f;
            stats.Heal(80f);
            Assert.AreEqual(100f, stats.HP, 0.01f);
        }

        [Test]
        public void Eat_ClampsToMaxHunger()
        {
            stats.Hunger = 90f;
            stats.Eat(50f);
            Assert.AreEqual(100f, stats.Hunger, 0.01f);
        }

        [Test]
        public void Drink_ClampsToMaxThirst()
        {
            stats.Thirst = 80f;
            stats.Drink(30f);
            Assert.AreEqual(100f, stats.Thirst, 0.01f);
        }

        [Test]
        public void RestoreSanity_ClampsToMaxSanity()
        {
            stats.Sanity = 70f;
            stats.RestoreSanity(50f);
            Assert.AreEqual(100f, stats.Sanity, 0.01f);
        }

        // ===== Mana =====

        [Test]
        public void TryConsumeMana_Sufficient_ReturnsTrueAndSubtracts()
        {
            stats.Mana = 30f;
            Assert.IsTrue(stats.TryConsumeMana(10f));
            Assert.AreEqual(20f, stats.Mana, 0.01f);
        }

        [Test]
        public void TryConsumeMana_Insufficient_ReturnsFalseAndDoesNotSubtract()
        {
            stats.Mana = 5f;
            Assert.IsFalse(stats.TryConsumeMana(10f));
            Assert.AreEqual(5f, stats.Mana, 0.01f);
        }

        [Test]
        public void AddMana_ClampsToMaxMana()
        {
            stats.Mana = 40f;
            stats.AddMana(20f);
            Assert.AreEqual(50f, stats.Mana, 0.01f, "default maxMana = 50");
        }

        // ===== OnStatsChanged =====

        [Test]
        public void OnStatsChanged_FiresForEachMutator()
        {
            int count = 0;
            stats.OnStatsChanged += () => count++;

            stats.Heal(1f);
            stats.Eat(1f);
            stats.Drink(1f);
            stats.RestoreSanity(1f);
            stats.AddMana(1f);
            stats.AddShield(1f, 1f);
            stats.TakeDamage(1f);

            Assert.AreEqual(7, count);
        }
    }
}
