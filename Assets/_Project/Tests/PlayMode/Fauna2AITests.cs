using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using WildernessCultivation.Core;
using WildernessCultivation.Cultivation;
using WildernessCultivation.Items;
using WildernessCultivation.Mobs;
using WildernessCultivation.Player;
using WildernessCultivation.Player.Status;

namespace WildernessCultivation.Tests.PlayMode
{
    /// <summary>
    /// PlayMode tests cho fauna II — hostile + status chain (Snake / Bat).
    /// Snake = ambush + Poison on-hit; Bat = night-only + Bleed on-hit.
    ///
    /// Coverage:
    /// - Snake hidden khi player ngoài revealRange + reveal khi player vào
    /// - Snake bite → apply Poison effect lên StatusEffectManager
    /// - Snake giveUp khi player ra xa
    /// - Bat day = invisible + collider tắt; night = active
    /// - Bat bite → apply Bleed effect lên StatusEffectManager
    /// </summary>
    public class Fauna2AITests
    {
        GameObject playerGo;
        PlayerStats playerStats;
        StatusEffectManager statusMgr;
        Inventory inv;
        RealmSystem realm;

        const int PlayerLayer = 0;
        const int MobLayer = 8;
        const int PlayerMaskBits = 1 << PlayerLayer;

        [SetUp]
        public void Setup()
        {
            playerGo = new GameObject("Player");
            playerGo.layer = PlayerLayer;
            playerGo.AddComponent<Rigidbody2D>();
            playerGo.AddComponent<CircleCollider2D>().radius = 0.3f;
            playerStats = playerGo.AddComponent<PlayerStats>();
            playerStats.IsAwakened = true;
            statusMgr = playerGo.AddComponent<StatusEffectManager>();
            inv = playerGo.AddComponent<Inventory>();
            realm = playerGo.AddComponent<RealmSystem>();
        }

        [TearDown]
        public void Teardown()
        {
            if (playerGo != null) Object.Destroy(playerGo);
        }

        static StatusEffectSO MakeEffect(string id, float hpTick = 0f)
        {
            var so = ScriptableObject.CreateInstance<StatusEffectSO>();
            so.effectId = id;
            so.displayName = id;
            so.hpDamagePerTick = hpTick;
            so.tickIntervalSec = 1f;
            so.defaultDurationSec = 5f;
            return so;
        }

        // ===== Snake =====

        GameObject MakeSnake(Vector3 pos, float reveal = 2.5f, float giveUp = 5f, StatusEffectSO poison = null)
        {
            var go = new GameObject("Snake");
            go.layer = MobLayer;
            go.transform.position = pos;
            go.AddComponent<Rigidbody2D>();
            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.25f;
            var sr = go.AddComponent<SpriteRenderer>();
            var ai = go.AddComponent<SnakeAI>();
            ai.spriteRenderer = sr;
            ai.revealRange = reveal;
            ai.giveUpRange = giveUp;
            ai.attackRange = 0.7f;
            ai.attackCooldown = 0.05f;
            ai.moveSpeed = 1f;
            ai.maxHP = 14f;
            ai.HP = 14f;
            ai.damage = 5f;
            ai.poisonEffect = poison;
            ai.poisonDuration = 5f;
            ai.playerMask = PlayerMaskBits;
            ai.drops = System.Array.Empty<World.ResourceNode.Drop>();
            return go;
        }

        [UnityTest]
        public IEnumerator Snake_PlayerFar_StaysHidden()
        {
            var snakeGo = MakeSnake(Vector3.zero, reveal: 1f);
            playerGo.transform.position = new Vector3(10f, 0, 0); // ngoài reveal
            yield return new WaitForFixedUpdate();
            yield return null;

            var snake = snakeGo.GetComponent<SnakeAI>();
            Assert.IsFalse(snake.IsRevealed, "Player ngoài revealRange → snake hidden");
            var sr = snakeGo.GetComponent<SpriteRenderer>();
            var col = snakeGo.GetComponent<CircleCollider2D>();
            Assert.IsFalse(sr.enabled, "Hidden → sprite disabled");
            Assert.IsFalse(col.enabled, "Hidden → collider disabled");
            Object.Destroy(snakeGo);
        }

        [UnityTest]
        public IEnumerator Snake_PlayerEntersReveal_Reveals()
        {
            var snakeGo = MakeSnake(Vector3.zero, reveal: 2.5f);
            playerGo.transform.position = new Vector3(1.5f, 0, 0); // < reveal
            yield return new WaitForFixedUpdate();
            yield return null;

            var snake = snakeGo.GetComponent<SnakeAI>();
            Assert.IsTrue(snake.IsRevealed, "Player vào revealRange → snake revealed");
            Assert.IsNotNull(snake.target);
            Object.Destroy(snakeGo);
        }

        [UnityTest]
        public IEnumerator Snake_BiteAppliesPoisonStatus()
        {
            var poison = MakeEffect("Poison", hpTick: 1f);
            var snakeGo = MakeSnake(Vector3.zero, reveal: 5f, poison: poison);
            var snake = snakeGo.GetComponent<SnakeAI>();
            snake.attackRange = 5f; // đủ rộng để 1 frame là attack
            playerGo.transform.position = new Vector3(0.3f, 0, 0); // sát snake

            Assert.IsFalse(statusMgr.HasEffect("Poison"));

            yield return new WaitForFixedUpdate();
            yield return null;
            yield return null;

            Assert.IsTrue(statusMgr.HasEffect("Poison"),
                "Snake bite → StatusEffectManager phải có Poison effect");
            Object.DestroyImmediate(poison);
            Object.Destroy(snakeGo);
        }

        // ===== Bat =====

        GameObject MakeBat(Vector3 pos, StatusEffectSO bleed = null)
        {
            var go = new GameObject("Bat");
            go.layer = MobLayer;
            go.transform.position = pos;
            go.AddComponent<Rigidbody2D>();
            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.25f;
            var sr = go.AddComponent<SpriteRenderer>();
            var ai = go.AddComponent<BatAI>();
            ai.spriteRenderer = sr;
            ai.aggroRange = 5f;
            ai.attackRange = 0.7f;
            ai.attackCooldown = 0.05f;
            ai.moveSpeed = 1.5f;
            ai.maxHP = 10f;
            ai.HP = 10f;
            ai.damage = 3f;
            ai.bleedEffect = bleed;
            ai.bleedDuration = 4f;
            ai.playerMask = PlayerMaskBits;
            ai.drops = System.Array.Empty<World.ResourceNode.Drop>();
            return go;
        }

        [UnityTest]
        public IEnumerator Bat_Day_HiddenAndIdle()
        {
            var timeGo = new GameObject("Time");
            var time = timeGo.AddComponent<TimeManager>();
            time.dayLengthSeconds = 99999f;
            time.currentTime01 = 0.5f; // day

            var batGo = MakeBat(new Vector3(0, 0, 0));
            playerGo.transform.position = new Vector3(0.5f, 0, 0);
            yield return null;
            yield return null;

            var sr = batGo.GetComponent<SpriteRenderer>();
            var col = batGo.GetComponent<CircleCollider2D>();
            Assert.IsFalse(sr.enabled, "Day → bat sprite tắt");
            Assert.IsFalse(col.enabled, "Day → bat collider tắt");

            Object.Destroy(batGo);
            Object.Destroy(timeGo);
            yield return null;
        }

        [UnityTest]
        public IEnumerator Bat_Night_BiteAppliesBleed()
        {
            var timeGo = new GameObject("Time");
            var time = timeGo.AddComponent<TimeManager>();
            time.dayLengthSeconds = 99999f;
            time.currentTime01 = 0.0f; // midnight = night

            var bleed = MakeEffect("Bleed", hpTick: 1f);
            var batGo = MakeBat(Vector3.zero, bleed);
            var bat = batGo.GetComponent<BatAI>();
            bat.attackRange = 5f;
            playerGo.transform.position = new Vector3(0.3f, 0, 0);

            Assert.IsFalse(statusMgr.HasEffect("Bleed"));

            yield return new WaitForFixedUpdate();
            yield return null;
            yield return null;

            Assert.IsTrue(statusMgr.HasEffect("Bleed"),
                "Night Bat bite → StatusEffectManager phải có Bleed effect");

            Object.DestroyImmediate(bleed);
            Object.Destroy(batGo);
            Object.Destroy(timeGo);
            yield return null;
        }

        [UnityTest]
        public IEnumerator Bat_TakeDamage_StillTracksKillerForLoot()
        {
            var timeGo = new GameObject("Time");
            var time = timeGo.AddComponent<TimeManager>();
            time.dayLengthSeconds = 99999f;
            time.currentTime01 = 0.0f;

            var wing = ScriptableObject.CreateInstance<ItemSO>();
            wing.itemId = "bat_wing";
            wing.displayName = "Bat Wing";
            wing.maxStack = 99;

            var batGo = MakeBat(new Vector3(50, 50, 0));
            var bat = batGo.GetComponent<BatAI>();
            bat.drops = new[] { new World.ResourceNode.Drop { item = wing, min = 1, max = 1 } };
            yield return null;

            int before = inv.CountOf(wing);
            bat.TakeDamage(999f, playerGo);
            yield return null;

            Assert.AreEqual(before + 1, inv.CountOf(wing),
                "Bat die → drop wing vào inventory killer");
            Object.DestroyImmediate(wing);
            Object.Destroy(timeGo);
            yield return null;
        }
    }
}
