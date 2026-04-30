using NUnit.Framework;
using UnityEngine;
using WildernessCultivation.Player;
using WildernessCultivation.Player.Stats;

namespace WildernessCultivation.Tests.EditMode
{
    /// <summary>
    /// EditMode tests cho R1 phase 2 component split: 7 subsystem bổ sung (Health / Hunger /
    /// Thirst / Sanity / Mana / Shield / Invulnerability) tách khỏi PlayerStats. Verify:
    /// - Sub-component auto-add trong Awake + auto-add lazy khi property getter chạm.
    /// - Façade get/set đồng bộ 2-chiều với storage trên component.
    /// - Prefab pre-installed component KHÔNG bị Awake override.
    /// - Component pure API hoạt động độc lập (HealthComponent / ShieldComponent / …)
    ///   để NPC humanoid tương lai reuse.
    /// </summary>
    public class StatComponentsR1P2Tests
    {
        GameObject go;
        PlayerStats stats;

        [SetUp]
        public void Setup()
        {
            go = new GameObject("Player");
            stats = go.AddComponent<PlayerStats>();
            TestHelpers.Boot(stats);
        }

        [TearDown]
        public void Teardown()
        {
            if (go != null) Object.DestroyImmediate(go);
        }

        // ===== Auto-add all 7 components =====

        [Test]
        public void Awake_AutoAddsAllR1P2Components()
        {
            Assert.IsNotNull(stats.health, "HealthComponent phải được Awake auto-add");
            Assert.IsNotNull(stats.hunger, "HungerComponent phải được Awake auto-add");
            Assert.IsNotNull(stats.thirst, "ThirstComponent phải được Awake auto-add");
            Assert.IsNotNull(stats.sanity, "SanityComponent phải được Awake auto-add");
            Assert.IsNotNull(stats.mana, "ManaComponent phải được Awake auto-add");
            Assert.IsNotNull(stats.shieldComp, "ShieldComponent phải được Awake auto-add");
            Assert.IsNotNull(stats.invuln, "InvulnerabilityComponent phải được Awake auto-add");

            Assert.IsNotNull(go.GetComponent<HealthComponent>());
            Assert.IsNotNull(go.GetComponent<HungerComponent>());
            Assert.IsNotNull(go.GetComponent<ThirstComponent>());
            Assert.IsNotNull(go.GetComponent<SanityComponent>());
            Assert.IsNotNull(go.GetComponent<ManaComponent>());
            Assert.IsNotNull(go.GetComponent<ShieldComponent>());
            Assert.IsNotNull(go.GetComponent<InvulnerabilityComponent>());
        }

        // ===== Façade delegation: 2-way sync =====

        [Test]
        public void HealthFacade_DelegatesToComponent()
        {
            stats.HP = 42f;
            Assert.AreEqual(42f, stats.health.HP, 0.001f);
            stats.health.HP = 73f;
            Assert.AreEqual(73f, stats.HP, 0.001f);

            stats.maxHP = 150f;
            Assert.AreEqual(150f, stats.health.maxHP, 0.001f);
        }

        [Test]
        public void HungerFacade_DelegatesToComponent()
        {
            stats.Hunger = 60f;
            Assert.AreEqual(60f, stats.hunger.Hunger, 0.001f);
            stats.hunger.Hunger = 20f;
            Assert.AreEqual(20f, stats.Hunger, 0.001f);

            stats.hungerDecay = 0.9f;
            Assert.AreEqual(0.9f, stats.hunger.hungerDecay, 0.001f);
        }

        [Test]
        public void ThirstFacade_DelegatesToComponent()
        {
            stats.Thirst = 44f;
            Assert.AreEqual(44f, stats.thirst.Thirst, 0.001f);
            stats.thirst.Thirst = 11f;
            Assert.AreEqual(11f, stats.Thirst, 0.001f);

            stats.rainThirstRefillPerSec = 2.5f;
            Assert.AreEqual(2.5f, stats.thirst.rainThirstRefillPerSec, 0.001f);
        }

        [Test]
        public void SanityFacade_DelegatesToComponent()
        {
            stats.Sanity = 35f;
            Assert.AreEqual(35f, stats.sanity.Sanity, 0.001f);
            stats.sanity.Sanity = 88f;
            Assert.AreEqual(88f, stats.Sanity, 0.001f);

            stats.stormSanityPenaltyPerSec = 1.2f;
            Assert.AreEqual(1.2f, stats.sanity.stormSanityPenaltyPerSec, 0.001f);
        }

        [Test]
        public void ManaFacade_DelegatesToComponent()
        {
            stats.Mana = 22f;
            Assert.AreEqual(22f, stats.mana.Mana, 0.001f);
            stats.mana.Mana = 33f;
            Assert.AreEqual(33f, stats.Mana, 0.001f);

            stats.maxMana = 120f;
            Assert.AreEqual(120f, stats.mana.maxMana, 0.001f);
        }

        [Test]
        public void ShieldFacade_DelegatesToComponent()
        {
            stats.AddShield(15f, 5f);
            Assert.AreEqual(15f, stats.shieldComp.Shield, 0.001f);
            Assert.AreEqual(stats.shieldComp.ShieldEndsAt, stats.ShieldEndsAt, 0.001f);
            Assert.IsTrue(stats.shieldComp.HasShield == stats.HasShield);
        }

        [Test]
        public void InvulnerabilityFacade_DelegatesToComponent()
        {
            stats.SetInvulnerable(1f);
            Assert.AreEqual(stats.invuln.InvulnerableUntil, stats.InvulnerableUntil, 0.001f);
            Assert.IsTrue(stats.IsInvulnerable == stats.invuln.IsInvulnerable);
        }

        // ===== Prefab pre-installed component NOT overridden =====

        [Test]
        public void PreInstalled_HealthComponent_NotOverridden()
        {
            var go2 = new GameObject("PlayerCustom");
            var preHealth = go2.AddComponent<HealthComponent>();
            preHealth.maxHP = 250f; // marker
            var stats2 = go2.AddComponent<PlayerStats>();
            TestHelpers.Boot(stats2);

            Assert.AreSame(preHealth, stats2.health, "PlayerStats phải reuse component có sẵn");
            Assert.AreEqual(250f, stats2.maxHP, "Cấu hình prefab phải được giữ");

            Object.DestroyImmediate(go2);
        }

        [Test]
        public void PreInstalled_ManaComponent_NotOverridden()
        {
            var go2 = new GameObject("PlayerCustom");
            var preMana = go2.AddComponent<ManaComponent>();
            preMana.maxMana = 200f;
            var stats2 = go2.AddComponent<PlayerStats>();
            TestHelpers.Boot(stats2);

            Assert.AreSame(preMana, stats2.mana);
            Assert.AreEqual(200f, stats2.maxMana, 0.001f);

            Object.DestroyImmediate(go2);
        }

        // ===== Lazy auto-add: property access without Awake =====

        [Test]
        public void LazyAdd_HealthProperty_WithoutBoot()
        {
            // Simulate PlayerStatsTests pattern: AddComponent without Boot.
            var go3 = new GameObject("LazyPlayer");
            var stats3 = go3.AddComponent<PlayerStats>();
            // Không Boot → Awake chưa chạy → stats3.health == null.
            Assert.IsNull(stats3.health);

            // First property access triggers EnsureComponent.
            float hp = stats3.HP;
            Assert.AreEqual(100f, hp, 0.001f, "Default HP=100 từ HealthComponent field initializer");
            Assert.IsNotNull(stats3.health, "HealthComponent phải được lazy-add khi chạm property");

            Object.DestroyImmediate(go3);
        }

        // ===== Component pure API (for NPC humanoid reuse) =====

        [Test]
        public void HealthComponent_PureAPI_Standalone()
        {
            var go4 = new GameObject("NPC");
            var h = go4.AddComponent<HealthComponent>();
            // Defaults
            Assert.AreEqual(100f, h.HP, 0.001f);
            Assert.AreEqual(100f, h.maxHP, 0.001f);
            Assert.IsFalse(h.IsDead);

            h.TakeRaw(30f);
            Assert.AreEqual(70f, h.HP, 0.001f);

            h.Heal(50f);
            Assert.AreEqual(100f, h.HP, 0.001f, "Heal clamp <= maxHP");

            h.TakeRaw(999f);
            Assert.AreEqual(0f, h.HP, 0.001f);
            Assert.IsTrue(h.IsDead);

            // Dead guard: further damage no-op.
            h.TakeRaw(10f);
            Assert.AreEqual(0f, h.HP, 0.001f);

            Object.DestroyImmediate(go4);
        }

        [Test]
        public void ShieldComponent_Absorb_ReturnsRemaining()
        {
            var go5 = new GameObject("ShieldHost");
            var s = go5.AddComponent<ShieldComponent>();
            s.Add(20f, 10f);

            float r1 = s.Absorb(15f);
            Assert.AreEqual(0f, r1, 0.001f, "Shield đủ đỡ → remaining=0");
            Assert.AreEqual(5f, s.Shield, 0.001f);

            float r2 = s.Absorb(10f);
            Assert.AreEqual(5f, r2, 0.001f, "Shield 5 đỡ 5 dame, còn 5 → remaining=5");
            Assert.AreEqual(0f, s.Shield, 0.001f);

            Object.DestroyImmediate(go5);
        }

        [Test]
        public void ShieldComponent_Add_TakesMaxDuration()
        {
            var go6 = new GameObject("ShieldHost");
            var s = go6.AddComponent<ShieldComponent>();
            s.Add(10f, 100f);
            float longEnd = s.ShieldEndsAt;
            s.Add(10f, 1f);
            Assert.AreEqual(longEnd, s.ShieldEndsAt, 0.001f, "Duration ngắn hơn không ghi đè");
            Assert.AreEqual(20f, s.Shield, 0.001f, "Amount phải stack");

            Object.DestroyImmediate(go6);
        }

        [Test]
        public void InvulnerabilityComponent_Set_DoesNotShrink()
        {
            var go7 = new GameObject("IframeHost");
            var iv = go7.AddComponent<InvulnerabilityComponent>();
            iv.Set(10f);
            float longEnd = iv.InvulnerableUntil;
            iv.Set(0.1f);
            Assert.AreEqual(longEnd, iv.InvulnerableUntil, 0.001f, "Duration ngắn hơn không rút ngắn i-frames");

            Object.DestroyImmediate(go7);
        }

        [Test]
        public void HungerComponent_Tick_DecaysByRateAndMul()
        {
            var go8 = new GameObject("HungerHost");
            var h = go8.AddComponent<HungerComponent>();
            h.Hunger = 50f;
            h.hungerDecay = 2f;
            h.Tick(dt: 1f, decayMul: 2f);
            Assert.AreEqual(46f, h.Hunger, 0.001f, "50 - 2*2*1 = 46");

            Object.DestroyImmediate(go8);
        }

        [Test]
        public void ManaComponent_TryConsume_GuardsInsufficient()
        {
            var go9 = new GameObject("ManaHost");
            var m = go9.AddComponent<ManaComponent>();
            m.Mana = 10f;

            Assert.IsFalse(m.TryConsume(20f), "Thiếu mana → false + không trừ");
            Assert.AreEqual(10f, m.Mana, 0.001f);

            Assert.IsTrue(m.TryConsume(10f));
            Assert.AreEqual(0f, m.Mana, 0.001f);

            Object.DestroyImmediate(go9);
        }

        [Test]
        public void SanityComponent_Damage_ClampsToZero()
        {
            var go10 = new GameObject("SanityHost");
            var s = go10.AddComponent<SanityComponent>();
            s.Sanity = 5f;
            s.Damage(100f);
            Assert.AreEqual(0f, s.Sanity, 0.001f);

            s.Damage(-10f);
            Assert.AreEqual(0f, s.Sanity, 0.001f, "Damage âm → no-op");

            Object.DestroyImmediate(go10);
        }
    }
}
