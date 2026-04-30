using NUnit.Framework;
using UnityEngine;
using WildernessCultivation.Combat;
using WildernessCultivation.Mobs;
using WildernessCultivation.Player;

namespace WildernessCultivation.Tests.EditMode
{
    /// <summary>
    /// EditMode tests cho R5 <see cref="CharacterBase"/> polymorphic view.
    /// Mục tiêu: code consumer (UI bar / AI targeting / save / dialog) lấy 1 ref
    /// CharacterBase thay vì rẽ nhánh PlayerStats vs MobBase. Verify:
    /// - PlayerStats và MobBase đều IS-A CharacterBase + IDamageable.
    /// - CurrentHP / CurrentMaxHP / IsDead / IsAlive / HPRatio01 báo đúng.
    /// - TakeDamage qua CharacterBase ref hoạt động (virtual dispatch tới subclass).
    /// </summary>
    public class CharacterBaseTests
    {
        // ===== Type identity =====

        [Test]
        public void PlayerStats_IsCharacterBase_AndIDamageable()
        {
            var go = new GameObject("Player");
            var stats = go.AddComponent<PlayerStats>();
            TestHelpers.Boot(stats);

            Assert.IsInstanceOf<CharacterBase>(stats);
            Assert.IsInstanceOf<IDamageable>(stats);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void Wolf_IsCharacterBase_AndIDamageable()
        {
            var go = new GameObject("Wolf");
            go.AddComponent<Rigidbody2D>();
            go.AddComponent<BoxCollider2D>();
            var wolf = go.AddComponent<WolfAI>();
            wolf.maxHP = 30f;
            wolf.HP = 30f;

            Assert.IsInstanceOf<CharacterBase>(wolf);
            Assert.IsInstanceOf<IDamageable>(wolf);
            Object.DestroyImmediate(go);
        }

        // ===== Polymorphic view =====

        [Test]
        public void PlayerStats_AsCharacterBase_ReportsHP()
        {
            var go = new GameObject("Player");
            var stats = go.AddComponent<PlayerStats>();
            TestHelpers.Boot(stats);
            stats.HP = 75f;
            stats.maxHP = 100f;

            CharacterBase cb = stats;
            Assert.AreEqual(75f, cb.CurrentHP, 0.01f);
            Assert.AreEqual(100f, cb.CurrentMaxHP, 0.01f);
            Assert.AreEqual(0.75f, cb.HPRatio01, 0.01f);
            Assert.IsFalse(cb.IsDead);
            Assert.IsTrue(cb.IsAlive);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void Wolf_AsCharacterBase_ReportsHP()
        {
            var go = new GameObject("Wolf");
            go.AddComponent<Rigidbody2D>();
            go.AddComponent<BoxCollider2D>();
            var wolf = go.AddComponent<WolfAI>();
            wolf.maxHP = 30f;
            wolf.HP = 15f;

            CharacterBase cb = wolf;
            Assert.AreEqual(15f, cb.CurrentHP, 0.01f);
            Assert.AreEqual(30f, cb.CurrentMaxHP, 0.01f);
            Assert.AreEqual(0.5f, cb.HPRatio01, 0.01f);
            Assert.IsFalse(cb.IsDead);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void HPRatio01_ZeroMaxHP_ReturnsZero()
        {
            var go = new GameObject("Player");
            var stats = go.AddComponent<PlayerStats>();
            TestHelpers.Boot(stats);
            stats.maxHP = 0f;
            stats.HP = 0f;

            CharacterBase cb = stats;
            Assert.AreEqual(0f, cb.HPRatio01, 0.001f);
            Object.DestroyImmediate(go);
        }

        // ===== TakeDamage polymorphic dispatch =====

        [Test]
        public void TakeDamage_ViaCharacterBaseRef_DispatchesToSubclass()
        {
            var go = new GameObject("Wolf");
            go.AddComponent<Rigidbody2D>();
            go.AddComponent<BoxCollider2D>();
            var wolf = go.AddComponent<WolfAI>();
            wolf.maxHP = 30f;
            wolf.HP = 30f;

            CharacterBase cb = wolf;
            cb.TakeDamage(10f, null);

            Assert.AreEqual(20f, wolf.HP, 0.01f, "MobBase.TakeDamage phải fire qua CharacterBase ref");
            Object.DestroyImmediate(go);
        }

        [Test]
        public void IsDead_FlipsAfterFatalDamage()
        {
            var go = new GameObject("Wolf");
            go.AddComponent<Rigidbody2D>();
            go.AddComponent<BoxCollider2D>();
            var wolf = go.AddComponent<WolfAI>();
            wolf.maxHP = 5f;
            wolf.HP = 5f;

            CharacterBase cb = wolf;
            Assert.IsFalse(cb.IsDead);
            wolf.HP = 0f; // direct field set tránh trigger Die() → Destroy(gameObject) NRE trong test
            Assert.IsTrue(cb.IsDead);
            Assert.IsFalse(cb.IsAlive);
            Object.DestroyImmediate(go);
        }
    }
}
