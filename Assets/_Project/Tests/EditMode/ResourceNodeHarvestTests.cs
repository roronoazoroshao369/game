using NUnit.Framework;
using UnityEngine;
using WildernessCultivation.Items;
using WildernessCultivation.Player;
using WildernessCultivation.World;

namespace WildernessCultivation.Tests.EditMode
{
    /// <summary>
    /// EditMode tests cho ResourceNode harvest side-effects (PR C — flora).
    /// Cactus prick = -2 HP, Death Lily = -5 SAN, Linh Mushroom = no side-effects (food only).
    /// </summary>
    public class ResourceNodeHarvestTests
    {
        GameObject playerGo;
        PlayerStats stats;
        Inventory inv;

        [SetUp]
        public void Setup()
        {
            playerGo = new GameObject("Player");
            stats = playerGo.AddComponent<PlayerStats>();
            inv = playerGo.AddComponent<Inventory>();
        }

        [TearDown]
        public void Teardown()
        {
            if (playerGo != null) Object.DestroyImmediate(playerGo);
        }

        static ItemSO MakeItem(string id)
        {
            var so = ScriptableObject.CreateInstance<ItemSO>();
            so.itemId = id;
            so.displayName = id;
            so.maxStack = 99;
            return so;
        }

        GameObject MakeNode(ItemSO drop, float maxHP = 1f, int min = 1, int max = 1)
        {
            var go = new GameObject("Node");
            go.AddComponent<CircleCollider2D>();
            var node = go.AddComponent<ResourceNode>();
            node.maxHP = maxHP;
            node.drops = new[] { new ResourceNode.Drop { item = drop, min = min, max = max } };
            // Awake đã chạy → currentHP=maxHP set đúng.
            return go;
        }

        [Test]
        public void Harvest_NoSideEffects_HpAndSanityUnchanged()
        {
            var item = MakeItem("linh_mushroom");
            var node = MakeNode(item);
            float hpBefore = stats.HP;
            float sanBefore = stats.Sanity;

            node.GetComponent<ResourceNode>().TakeDamage(999f, playerGo);

            Assert.AreEqual(hpBefore, stats.HP, 0.01f, "Plain harvest không đụng HP");
            Assert.AreEqual(sanBefore, stats.Sanity, 0.01f, "Plain harvest không đụng SAN");
            Assert.AreEqual(1, inv.CountOf(item), "Drop vào inventory");
            Object.DestroyImmediate(item);
        }

        [Test]
        public void Harvest_HpDamage_AppliesToHarvester()
        {
            // Cactus pattern: prick -2 HP khi pick.
            var item = MakeItem("cactus_water");
            var go = MakeNode(item);
            var node = go.GetComponent<ResourceNode>();
            node.harvestHpDamage = 2f;
            float hpBefore = stats.HP;

            node.TakeDamage(999f, playerGo);

            Assert.AreEqual(hpBefore - 2f, stats.HP, 0.01f, "Cactus prick -2 HP");
            Object.DestroyImmediate(item);
        }

        [Test]
        public void Harvest_SanityDamage_AppliesToHarvester()
        {
            // Death Lily pattern: -5 SAN khi pick.
            var item = MakeItem("death_pollen");
            var go = MakeNode(item);
            var node = go.GetComponent<ResourceNode>();
            node.harvestSanityDamage = 5f;
            float sanBefore = stats.Sanity;

            node.TakeDamage(999f, playerGo);

            Assert.AreEqual(sanBefore - 5f, stats.Sanity, 0.01f, "Death Lily -5 SAN");
            Assert.AreEqual(1, inv.CountOf(item), "Vẫn drop death_pollen vào inv");
            Object.DestroyImmediate(item);
        }

        [Test]
        public void Harvest_SanityDamage_ClampsAtZero()
        {
            // SAN không âm.
            var item = MakeItem("death_pollen");
            var go = MakeNode(item);
            var node = go.GetComponent<ResourceNode>();
            node.harvestSanityDamage = 999f;

            node.TakeDamage(999f, playerGo);

            Assert.AreEqual(0f, stats.Sanity, 0.01f, "SAN clamp >= 0");
            Object.DestroyImmediate(item);
        }

        [Test]
        public void Harvest_NoSource_NoSideEffectsAndNoCrash()
        {
            // Source null (vd npc đập, projectile no owner) — không crash, không apply.
            var item = MakeItem("death_pollen");
            var go = MakeNode(item);
            var node = go.GetComponent<ResourceNode>();
            node.harvestSanityDamage = 5f;

            Assert.DoesNotThrow(() => node.TakeDamage(999f, null));
            Object.DestroyImmediate(item);
        }

        [Test]
        public void Harvest_HpDamage_BypassesInvulnerability()
        {
            // Environment toll (cactus prick) phải apply ngay cả khi player đang i-frame
            // dodge — không phải combat damage.
            var item = MakeItem("cactus_water");
            var go = MakeNode(item);
            var node = go.GetComponent<ResourceNode>();
            node.harvestHpDamage = 2f;

            stats.SetInvulnerable(10f);
            float hpBefore = stats.HP;

            node.TakeDamage(999f, playerGo);

            Assert.AreEqual(hpBefore - 2f, stats.HP, 0.01f,
                "TakeDamageRaw bypass i-frame, prick vẫn apply -2 HP");
            Object.DestroyImmediate(item);
        }

        [Test]
        public void Harvest_SourceWithoutPlayerStats_NoCrash()
        {
            // Source là 1 GO không có PlayerStats (vd mob).
            var item = MakeItem("cactus_water");
            var go = MakeNode(item);
            var node = go.GetComponent<ResourceNode>();
            node.harvestHpDamage = 2f;

            var nonPlayerGo = new GameObject("Bot");
            Assert.DoesNotThrow(() => node.TakeDamage(999f, nonPlayerGo));

            Object.DestroyImmediate(nonPlayerGo);
            Object.DestroyImmediate(item);
        }
    }
}
