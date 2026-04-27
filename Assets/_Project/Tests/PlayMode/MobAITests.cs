using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using WildernessCultivation.Combat;
using WildernessCultivation.Core;
using WildernessCultivation.Cultivation;
using WildernessCultivation.Items;
using WildernessCultivation.Mobs;
using WildernessCultivation.Player;

namespace WildernessCultivation.Tests.PlayMode
{
    /// <summary>
    /// PlayMode tests cho MobBase / WolfAI / FoxSpiritAI: aggro range detection, attack range,
    /// projectile owner aggro resolve, drop-on-die loot, XP reward, FoxSpirit night-only gating.
    /// Cần Physics2D (Rigidbody2D + Collider2D + OverlapCircle) → PlayMode.
    /// </summary>
    public class MobAITests
    {
        GameObject playerGo;
        PlayerStats playerStats;
        Inventory inv;
        RealmSystem realm;

        // Layer 0 = Default (player), Layer 8 = first user layer (mob). Distinct layers để
        // Physics2D.OverlapCircle(_, _, playerMask) KHÔNG self-detect mob's own collider.
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
            // MobBase.Die gate XP grant by killer.IsAwakened — set true để
            // Die_AwardsXpToKillerRealm vẫn pass (intent test này KHÔNG phải gating).
            playerStats.IsAwakened = true;
            inv = playerGo.AddComponent<Inventory>();
            realm = playerGo.AddComponent<RealmSystem>();
        }

        [TearDown]
        public void Teardown()
        {
            if (playerGo != null) Object.Destroy(playerGo);
        }

        GameObject MakeWolf(Vector3 pos, float aggro = 4f, float attack = 0.8f)
        {
            var go = new GameObject("Wolf");
            go.layer = MobLayer; // KHÁC player layer → OverlapCircle không self-detect
            go.transform.position = pos;
            go.AddComponent<Rigidbody2D>();
            go.AddComponent<CircleCollider2D>().radius = 0.3f;
            var wolf = go.AddComponent<WolfAI>();
            wolf.aggroRange = aggro;
            wolf.attackRange = attack;
            wolf.moveSpeed = 5f;
            wolf.maxHP = 20f;
            wolf.damage = 5f;
            wolf.attackCooldown = 0.05f;
            wolf.playerMask = PlayerMaskBits; // chỉ dò player layer
            wolf.drops = System.Array.Empty<World.ResourceNode.Drop>();
            return go;
        }

        // ===== MobBase aggro =====

        [UnityTest]
        public IEnumerator Wolf_PlayerWithinAggro_AcquiresTarget()
        {
            var wolfGo = MakeWolf(new Vector3(0, 0, 0));
            playerGo.transform.position = new Vector3(2f, 0, 0); // dist 2 < aggro 4
            yield return new WaitForFixedUpdate();
            yield return null;

            var wolf = wolfGo.GetComponent<WolfAI>();
            Assert.IsNotNull(wolf.target, "wolf phải lock target khi player trong aggro range");
            Object.Destroy(wolfGo);
        }

        [UnityTest]
        public IEnumerator Wolf_PlayerOutsideAggro_NoTarget()
        {
            var wolfGo = MakeWolf(new Vector3(0, 0, 0), aggro: 1f);
            playerGo.transform.position = new Vector3(5f, 0, 0); // dist 5 > aggro 1
            yield return new WaitForFixedUpdate();
            yield return null;

            var wolf = wolfGo.GetComponent<WolfAI>();
            Assert.IsNull(wolf.target, "ngoài aggro range → wolf không acquire target");
            Object.Destroy(wolfGo);
        }

        // ===== MobBase TakeDamage =====

        [UnityTest]
        public IEnumerator TakeDamage_ReducesHP_AndAggroOnHit()
        {
            var wolfGo = MakeWolf(new Vector3(50f, 50f, 0), aggro: 0.1f); // far away
            playerGo.transform.position = new Vector3(0, 0, 0);
            yield return null;

            var wolf = wolfGo.GetComponent<WolfAI>();
            Assert.IsNull(wolf.target);
            float startHP = wolf.HP;

            wolf.TakeDamage(7f, playerGo);
            Assert.AreEqual(startHP - 7f, wolf.HP, 0.01f);
            Assert.AreSame(playerGo.transform, wolf.target,
                "TakeDamage source != null + target null → aggro lên source (player)");
            Object.Destroy(wolfGo);
            yield return null;
        }

        [UnityTest]
        public IEnumerator TakeDamage_FromProjectile_AggroOnProjectileOwner()
        {
            var wolfGo = MakeWolf(new Vector3(50f, 50f, 0), aggro: 0.1f);
            yield return null;
            var wolf = wolfGo.GetComponent<WolfAI>();

            var projGo = new GameObject("Projectile");
            projGo.AddComponent<Rigidbody2D>();
            projGo.AddComponent<CircleCollider2D>().isTrigger = true;
            var proj = projGo.AddComponent<Projectile>();
            proj.Launch(Vector2.right, playerGo); // set owner = playerGo

            wolf.TakeDamage(3f, projGo);
            Assert.AreSame(playerGo.transform, wolf.target,
                "Projectile.Owner phải resolve thành aggro target, KHÔNG phải projectile");
            Object.Destroy(wolfGo);
            Object.Destroy(projGo);
            yield return null;
        }

        // ===== Die: loot + xp =====

        [UnityTest]
        public IEnumerator Die_DropsLootToKillerInventory()
        {
            var stick = ScriptableObject.CreateInstance<ItemSO>();
            stick.itemId = "stick";
            stick.displayName = "Stick";
            stick.maxStack = 99;

            var wolfGo = MakeWolf(new Vector3(50, 50, 0), aggro: 0.1f);
            var wolf = wolfGo.GetComponent<WolfAI>();
            wolf.drops = new[]
            {
                new World.ResourceNode.Drop { item = stick, min = 3, max = 3 },
            };
            yield return null;

            int before = inv.CountOf(stick);
            wolf.TakeDamage(999f, playerGo);
            yield return null;

            Assert.AreEqual(before + 3, inv.CountOf(stick),
                "Die phải Add loot vào inventory người giết");
            Object.DestroyImmediate(stick);
            yield return null;
        }

        [UnityTest]
        public IEnumerator Die_AwardsXpToKillerRealm()
        {
            var wolfGo = MakeWolf(new Vector3(50, 50, 0), aggro: 0.1f);
            var wolf = wolfGo.GetComponent<WolfAI>();
            wolf.xpReward = 12f;
            yield return null;

            float xpBefore = realm.currentXp;
            wolf.TakeDamage(999f, playerGo);
            yield return null;

            Assert.AreEqual(xpBefore + 12f, realm.currentXp, 0.01f,
                "Die phải AddCultivationXp(xpReward) cho RealmSystem của killer");
            yield return null;
        }

        [UnityTest]
        public IEnumerator Die_DestroysGameObject()
        {
            var wolfGo = MakeWolf(new Vector3(50, 50, 0), aggro: 0.1f);
            var wolf = wolfGo.GetComponent<WolfAI>();
            yield return null;

            wolf.TakeDamage(999f, playerGo);
            yield return null;
            Assert.IsTrue(wolfGo == null, "wolf gameObject phải bị Destroy sau khi HP <= 0");
        }

        // ===== Wolf attack =====

        [UnityTest]
        public IEnumerator Wolf_WithinAttackRange_DamagesPlayerOnCooldown()
        {
            var wolfGo = MakeWolf(new Vector3(0, 0, 0), aggro: 5f, attack: 5f);
            playerGo.transform.position = new Vector3(0.5f, 0, 0); // bên trong attackRange=5

            float hpBefore = playerStats.HP;

            yield return new WaitForFixedUpdate();
            yield return null;
            yield return null; // cho 1 attack pass

            Assert.Less(playerStats.HP, hpBefore, "wolf phải gây sát thương lên player khi trong attack range");
            Object.Destroy(wolfGo);
            yield return null;
        }

        // ===== FoxSpirit night-only =====

        [UnityTest]
        public IEnumerator FoxSpirit_Day_DisablesSpriteAndCollider()
        {
            // Tạo TimeManager với currentTime01=0.5 (day)
            var timeGo = new GameObject("Time");
            var time = timeGo.AddComponent<TimeManager>();
            time.dayLengthSeconds = 99999f;
            time.currentTime01 = 0.5f;

            var foxGo = new GameObject("Fox");
            foxGo.layer = MobLayer;
            foxGo.transform.position = new Vector3(50, 50, 0);
            foxGo.AddComponent<Rigidbody2D>();
            var col = foxGo.AddComponent<CircleCollider2D>();
            var sr = foxGo.AddComponent<SpriteRenderer>();
            var fox = foxGo.AddComponent<FoxSpiritAI>();
            fox.spriteRenderer = sr;
            fox.aggroRange = 0.1f;
            fox.playerMask = PlayerMaskBits;
            fox.drops = System.Array.Empty<World.ResourceNode.Drop>();

            yield return null;
            yield return null;

            Assert.IsFalse(sr.enabled, "ban ngày → sprite phải tắt");
            Assert.IsFalse(col.enabled, "ban ngày → collider phải tắt");
            Object.Destroy(foxGo);
            Object.Destroy(timeGo);
            yield return null;
        }

        [UnityTest]
        public IEnumerator FoxSpirit_Night_EnablesSpriteAndCollider()
        {
            var timeGo = new GameObject("Time");
            var time = timeGo.AddComponent<TimeManager>();
            time.dayLengthSeconds = 99999f;
            time.currentTime01 = 0.0f; // midnight → isNight

            var foxGo = new GameObject("Fox");
            foxGo.layer = MobLayer;
            foxGo.transform.position = new Vector3(50, 50, 0);
            foxGo.AddComponent<Rigidbody2D>();
            var col = foxGo.AddComponent<CircleCollider2D>();
            var sr = foxGo.AddComponent<SpriteRenderer>();
            var fox = foxGo.AddComponent<FoxSpiritAI>();
            fox.spriteRenderer = sr;
            fox.aggroRange = 0.1f;
            fox.playerMask = PlayerMaskBits;
            fox.drops = System.Array.Empty<World.ResourceNode.Drop>();

            yield return null;
            yield return null;

            Assert.IsTrue(sr.enabled, "ban đêm → sprite phải bật");
            Assert.IsTrue(col.enabled, "ban đêm → collider phải bật");
            Object.Destroy(foxGo);
            Object.Destroy(timeGo);
            yield return null;
        }
    }
}
