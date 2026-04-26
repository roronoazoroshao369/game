using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using WildernessCultivation.Player;

namespace WildernessCultivation.Tests.PlayMode
{
    /// <summary>
    /// PlayMode tests cho DodgeAction: CanDodge gates / mana cost / IsDodging + MovementLocked
    /// + i-frames flag / dodge end clears velocity + locks / cooldown / OnDisable cleanup.
    /// </summary>
    public class DodgeActionTests
    {
        GameObject playerGo;
        PlayerController controller;
        PlayerStats stats;
        Rigidbody2D rb;
        DodgeAction dodge;

        [SetUp]
        public void Setup()
        {
            playerGo = new GameObject("Player");
            rb = playerGo.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            controller = playerGo.AddComponent<PlayerController>();
            stats = playerGo.AddComponent<PlayerStats>();
            stats.Mana = 50f;

            dodge = playerGo.AddComponent<DodgeAction>();
            dodge.dodgeDistance = 3f;
            dodge.dodgeDuration = 0.1f;
            dodge.cooldown = 0.2f;
            dodge.manaCost = 5f;
            dodge.invulnerabilityDuration = 0.1f;
        }

        [TearDown]
        public void Teardown()
        {
            if (playerGo != null) Object.Destroy(playerGo);
        }

        [Test]
        public void CanDodge_DefaultState_True()
        {
            Assert.IsTrue(dodge.CanDodge());
        }

        [Test]
        public void CanDodge_WhenDead_False()
        {
            stats.HP = 0f;
            Assert.IsTrue(stats.IsDead);
            Assert.IsFalse(dodge.CanDodge());
        }

        [Test]
        public void CanDodge_WhenMovementLocked_False()
        {
            controller.MovementLocked = true;
            Assert.IsFalse(dodge.CanDodge(), "đang sleep/meditate (MovementLocked) → không dodge");
        }

        [Test]
        public void CanDodge_WhenManaInsufficient_False()
        {
            stats.Mana = 2f; // < manaCost=5
            Assert.IsFalse(dodge.CanDodge());
        }

        [Test]
        public void CanDodge_ManaZero_AndManaCostZero_True()
        {
            dodge.manaCost = 0f;
            stats.Mana = 0f;
            Assert.IsTrue(dodge.CanDodge(), "manaCost=0 → cho dodge dù không có mana");
        }

        [UnityTest]
        public IEnumerator TryDodge_ConsumesMana_AndSetsLocksAndIFrames()
        {
            stats.Mana = 30f;
            float manaBefore = stats.Mana;

            bool ok = dodge.TryDodge();
            Assert.IsTrue(ok, "TryDodge phải trả true khi đủ điều kiện");
            Assert.AreEqual(manaBefore - 5f, stats.Mana, 0.01f, "tốn manaCost=5");
            Assert.IsTrue(dodge.IsDodging, "IsDodging=true ngay sau khi start");
            Assert.IsTrue(controller.MovementLocked, "MovementLocked=true trong duration");
            Assert.IsTrue(stats.IsInvulnerable, "i-frames active trong duration");
            yield return null;
        }

        [UnityTest]
        public IEnumerator TryDodge_EmitsOnDodgeStartAndOnDodgeEnd()
        {
            int starts = 0, ends = 0;
            dodge.OnDodgeStart += () => starts++;
            dodge.OnDodgeEnd += () => ends++;

            Assert.IsTrue(dodge.TryDodge());
            yield return new WaitForSeconds(dodge.dodgeDuration + 0.05f);

            Assert.AreEqual(1, starts, "OnDodgeStart fire 1 lần");
            Assert.AreEqual(1, ends, "OnDodgeEnd fire 1 lần khi xong");
        }

        [UnityTest]
        public IEnumerator AfterDodgeDuration_ClearsVelocityAndLocks()
        {
            Assert.IsTrue(dodge.TryDodge());
            yield return new WaitForSeconds(dodge.dodgeDuration + 0.05f);

            Assert.IsFalse(dodge.IsDodging, "IsDodging=false khi xong");
            Assert.IsFalse(controller.MovementLocked, "MovementLocked=false khi xong");
            Assert.AreEqual(Vector2.zero, rb.velocity, "velocity reset về 0");
        }

        [UnityTest]
        public IEnumerator TryDodge_WhileAlreadyDodging_ReturnsFalse()
        {
            Assert.IsTrue(dodge.TryDodge());
            // Đang dodge → call lần nữa
            Assert.IsFalse(dodge.TryDodge(), "không thể dodge khi IsDodging=true");
            yield return new WaitForSeconds(dodge.dodgeDuration + 0.05f);
        }

        [UnityTest]
        public IEnumerator Dodge_EnforcesCooldown()
        {
            Assert.IsTrue(dodge.TryDodge());
            yield return new WaitForSeconds(dodge.dodgeDuration + 0.05f);

            // Vừa xong dodge, đang trong cooldown
            Assert.IsFalse(dodge.CanDodge(), "trong cooldown → không dodge");

            // Wait đến hết cooldown
            yield return new WaitForSeconds(dodge.cooldown + 0.05f);
            Assert.IsTrue(dodge.CanDodge(), "hết cooldown → dodge lại được");
        }

        [UnityTest]
        public IEnumerator TryDodge_NoInput_UsesFacingFallback()
        {
            // controller.InputDir mặc định Vector2.zero. Facing default = Vector2.down.
            Assert.IsTrue(dodge.TryDodge(), "dù input=0 vẫn dodge được (fallback Facing/down)");
            yield return null;
            Assert.IsTrue(dodge.IsDodging);
            yield return new WaitForSeconds(dodge.dodgeDuration + 0.05f);
        }

        [UnityTest]
        public IEnumerator OnDisable_MidDodge_CleansUpStateAndVelocity()
        {
            Assert.IsTrue(dodge.TryDodge());
            yield return null;
            Assert.IsTrue(dodge.IsDodging);

            dodge.enabled = false;
            yield return null;

            Assert.IsFalse(dodge.IsDodging, "OnDisable dừng coroutine + reset IsDodging");
            Assert.IsFalse(controller.MovementLocked, "OnDisable unlock movement");
            Assert.AreEqual(Vector2.zero, rb.velocity, "OnDisable reset velocity");
        }
    }
}
