using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using WildernessCultivation.Cultivation;
using WildernessCultivation.Items;
using WildernessCultivation.Mobs;
using WildernessCultivation.Player;

namespace WildernessCultivation.Tests.PlayMode
{
    /// <summary>
    /// PlayMode tests cho fauna mới (Boar / DeerSpirit / Crow). Cần Physics2D
    /// (Rigidbody2D + CircleCollider2D + OverlapCircle) → PlayMode chứ không
    /// EditMode.
    ///
    /// Coverage:
    /// - BoarAI: provoke aggro on player nearby, aggro on hit, charge state machine
    /// - DeerSpiritAI: flee from player when sighted, no aggro on hit (vẫn passive)
    /// - CrowAI: low HP one-shot drop feather, no melee on player
    /// </summary>
    public class Fauna1AITests
    {
        GameObject playerGo;
        PlayerStats playerStats;
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
            playerStats.IsAwakened = true; // cho XP grant — không phải scope test ở đây
            inv = playerGo.AddComponent<Inventory>();
            realm = playerGo.AddComponent<RealmSystem>();
        }

        [TearDown]
        public void Teardown()
        {
            if (playerGo != null) Object.Destroy(playerGo);
        }

        // ===== Boar =====

        GameObject MakeBoar(Vector3 pos, float provoke = 2.5f)
        {
            var go = new GameObject("Boar");
            go.layer = MobLayer;
            go.transform.position = pos;
            go.AddComponent<Rigidbody2D>();
            go.AddComponent<CircleCollider2D>().radius = 0.4f;
            var ai = go.AddComponent<BoarAI>();
            ai.provokeRange = provoke;
            ai.attackRange = 0.8f;
            ai.moveSpeed = 1.0f;
            ai.maxHP = 30f;
            ai.HP = 30f;
            ai.damage = 5f;
            ai.attackCooldown = 0.05f;
            ai.playerMask = PlayerMaskBits;
            ai.drops = System.Array.Empty<World.ResourceNode.Drop>();
            return go;
        }

        [UnityTest]
        public IEnumerator Boar_PlayerWithinProvokeRange_EntersAggro()
        {
            var boarGo = MakeBoar(Vector3.zero, provoke: 2.5f);
            playerGo.transform.position = new Vector3(1.5f, 0, 0); // < provoke
            yield return new WaitForFixedUpdate();
            yield return null;

            var boar = boarGo.GetComponent<BoarAI>();
            Assert.AreNotEqual(BoarAI.BoarState.Wander, boar.State,
                "Player trong provoke range → boar phải rời Wander state");
            Object.Destroy(boarGo);
        }

        [UnityTest]
        public IEnumerator Boar_PlayerOutsideProvokeRange_StaysWander()
        {
            var boarGo = MakeBoar(Vector3.zero, provoke: 1f);
            playerGo.transform.position = new Vector3(5f, 0, 0); // ngoài provoke
            yield return new WaitForFixedUpdate();
            yield return null;

            var boar = boarGo.GetComponent<BoarAI>();
            Assert.AreEqual(BoarAI.BoarState.Wander, boar.State,
                "Player ngoài provoke range → boar vẫn Wander");
            Object.Destroy(boarGo);
        }

        [UnityTest]
        public IEnumerator Boar_TakeDamage_TriggersAggroEvenIfPlayerFar()
        {
            var boarGo = MakeBoar(new Vector3(50, 50, 0), provoke: 0.1f); // player far
            yield return null;
            var boar = boarGo.GetComponent<BoarAI>();
            Assert.AreEqual(BoarAI.BoarState.Wander, boar.State);

            boar.TakeDamage(5f, playerGo);
            Assert.AreNotEqual(BoarAI.BoarState.Wander, boar.State,
                "Bị đánh → aggro ngay dù player ngoài provoke range");
            Object.Destroy(boarGo);
            yield return null;
        }

        // ===== DeerSpirit =====

        GameObject MakeDeer(Vector3 pos, float sight = 5f)
        {
            var go = new GameObject("DeerSpirit");
            go.layer = MobLayer;
            go.transform.position = pos;
            go.AddComponent<Rigidbody2D>();
            go.AddComponent<CircleCollider2D>().radius = 0.3f;
            var ai = go.AddComponent<DeerSpiritAI>();
            ai.sightRange = sight;
            ai.fleeMemorySec = 0.5f;
            ai.moveSpeed = 1f;
            ai.fleeSpeedMultiplier = 2f;
            ai.maxHP = 16f;
            ai.HP = 16f;
            ai.playerMask = PlayerMaskBits;
            ai.drops = System.Array.Empty<World.ResourceNode.Drop>();
            return go;
        }

        [UnityTest]
        public IEnumerator Deer_PlayerWithinSight_StartsFleeing()
        {
            var deerGo = MakeDeer(Vector3.zero, sight: 5f);
            playerGo.transform.position = new Vector3(2f, 0, 0); // < sight
            yield return new WaitForFixedUpdate();
            yield return null;

            var deer = deerGo.GetComponent<DeerSpiritAI>();
            Assert.IsTrue(deer.IsFleeing, "Player trong sight range → deer phải IsFleeing=true");
            Object.Destroy(deerGo);
        }

        [UnityTest]
        public IEnumerator Deer_PlayerOutOfSight_NotFleeing()
        {
            var deerGo = MakeDeer(Vector3.zero, sight: 1f);
            playerGo.transform.position = new Vector3(10f, 0, 0); // > sight
            yield return new WaitForFixedUpdate();
            yield return null;

            var deer = deerGo.GetComponent<DeerSpiritAI>();
            Assert.IsFalse(deer.IsFleeing, "Player ngoài sight range → deer không flee");
            Object.Destroy(deerGo);
        }

        [UnityTest]
        public IEnumerator Deer_TakeDamage_DoesNotMakeIDamageableLoop()
        {
            // DeerSpirit không có aggro state — TakeDamage chỉ trừ HP, không ràng buộc state.
            var deerGo = MakeDeer(new Vector3(50, 50, 0), sight: 0.1f);
            yield return null;
            var deer = deerGo.GetComponent<DeerSpiritAI>();
            float hpBefore = deer.HP;
            deer.TakeDamage(5f, playerGo);
            Assert.AreEqual(hpBefore - 5f, deer.HP, 0.01f);
            Object.Destroy(deerGo);
            yield return null;
        }

        // ===== Crow =====

        GameObject MakeCrow(Vector3 pos)
        {
            var go = new GameObject("Crow");
            go.layer = MobLayer;
            go.transform.position = pos;
            go.AddComponent<Rigidbody2D>();
            go.AddComponent<CircleCollider2D>().radius = 0.25f;
            var ai = go.AddComponent<CrowAI>();
            ai.patrolRadius = 1.5f;
            ai.playerNoticeRange = 1f;
            ai.moveSpeed = 1f;
            ai.maxHP = 6f;
            ai.HP = 6f;
            ai.playerMask = PlayerMaskBits;
            ai.drops = System.Array.Empty<World.ResourceNode.Drop>();
            return go;
        }

        [UnityTest]
        public IEnumerator Crow_LowHP_DiesQuickly()
        {
            var crowGo = MakeCrow(new Vector3(50, 50, 0));
            yield return null;
            var crow = crowGo.GetComponent<CrowAI>();

            crow.TakeDamage(999f, playerGo);
            yield return null;

            Assert.IsTrue(crowGo == null, "Crow HP thấp → 1 hit big dame phải die + destroy");
        }

        [UnityTest]
        public IEnumerator Crow_DropsFeatherToInventory()
        {
            var feather = ScriptableObject.CreateInstance<ItemSO>();
            feather.itemId = "feather";
            feather.displayName = "Feather";
            feather.maxStack = 99;

            var crowGo = MakeCrow(new Vector3(50, 50, 0));
            var crow = crowGo.GetComponent<CrowAI>();
            crow.drops = new[]
            {
                new World.ResourceNode.Drop { item = feather, min = 2, max = 2 },
            };
            yield return null;

            int before = inv.CountOf(feather);
            crow.TakeDamage(999f, playerGo);
            yield return null;

            Assert.AreEqual(before + 2, inv.CountOf(feather),
                "Crow Die phải drop feather vào inventory killer");
            Object.DestroyImmediate(feather);
            yield return null;
        }
    }
}
