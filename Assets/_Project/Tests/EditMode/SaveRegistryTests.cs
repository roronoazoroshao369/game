using NUnit.Framework;
using UnityEngine;
using WildernessCultivation.Core;
using WildernessCultivation.Cultivation;
using WildernessCultivation.Items;
using WildernessCultivation.Player;

namespace WildernessCultivation.Tests.EditMode
{
    /// <summary>
    /// R6 tests: verify ISaveable dispatcher pattern (SaveLoadController dispatches
    /// via SaveRegistry, systems own their slices, fixup ordering is respected).
    /// </summary>
    public class SaveRegistryTests
    {
        [TearDown]
        public void Teardown()
        {
            SaveRegistry.ClearAll();
            SaveSystem.Delete();
        }

        [Test]
        public void Register_Deduplicates_SameInstance()
        {
            var go = new GameObject("Player");
            try
            {
                var stats = go.AddComponent<PlayerStats>();
                TestHelpers.Boot(stats);
                TestHelpers.Boot(stats); // Boot lần 2 → OnEnable register lại

                int count = 0;
                foreach (var s in SaveRegistry.OrderedSaveables())
                    if (ReferenceEquals(s, stats)) count++;
                Assert.AreEqual(1, count, "Register phải dedup cùng instance");
            }
            finally { Object.DestroyImmediate(go); }
        }

        [Test]
        public void ClearAll_RemovesAllSaveablesAndFixups()
        {
            var go = new GameObject("Player");
            try
            {
                var stats = go.AddComponent<PlayerStats>();
                var realm = go.AddComponent<RealmSystem>();
                var inv = go.AddComponent<Inventory>();
                TestHelpers.Boot(stats, realm, inv);

                SaveRegistry.ClearAll();

                Assert.AreEqual(0, SaveRegistry.OrderedSaveables().Count);
                Assert.AreEqual(0, SaveRegistry.OrderedFixupActions().Count);
            }
            finally { Object.DestroyImmediate(go); }
        }

        [Test]
        public void OrderedSaveables_SortsByOrderAscending()
        {
            // WorldGenerator(0) < TimeManager(5) < RealmSystem(10) < PlayerStats(30) < Inventory(60)
            var go = new GameObject("SceneRoot");
            try
            {
                var stats = go.AddComponent<PlayerStats>();
                var realm = go.AddComponent<RealmSystem>();
                var inv = go.AddComponent<Inventory>();
                var time = go.AddComponent<TimeManager>();
                TestHelpers.Boot(stats, realm, inv, time);

                var ordered = SaveRegistry.OrderedSaveables();
                int prev = int.MinValue;
                foreach (var s in ordered)
                {
                    Assert.GreaterOrEqual(s.Order, prev, $"{s.SaveKey} order {s.Order} < prev {prev}");
                    prev = s.Order;
                }
                // RealmSystem phải đứng trước PlayerStats (spiritRoot name set trước khi vitals restore chạy)
                int realmIdx = -1, statsIdx = -1, invIdx = -1;
                for (int i = 0; i < ordered.Count; i++)
                {
                    if (ReferenceEquals(ordered[i], realm)) realmIdx = i;
                    else if (ReferenceEquals(ordered[i], stats)) statsIdx = i;
                    else if (ReferenceEquals(ordered[i], inv)) invIdx = i;
                }
                Assert.Greater(statsIdx, realmIdx, "PlayerStats sau RealmSystem");
                Assert.Greater(invIdx, statsIdx, "Inventory sau PlayerStats");
            }
            finally { Object.DestroyImmediate(go); }
        }

        [Test]
        public void OnDisable_Unregisters_BothSaveableAndFixups()
        {
            var go = new GameObject("Player");
            try
            {
                var stats = go.AddComponent<PlayerStats>();
                TestHelpers.Boot(stats);
                Assert.Greater(SaveRegistry.OrderedSaveables().Count, 0);
                Assert.Greater(SaveRegistry.OrderedFixupActions().Count, 0);

                // Manually simulate OnDisable (EditMode không fire tự động).
                TestHelpers.InvokeLifecycle(stats, "OnDisable");

                bool statsStill = false;
                foreach (var s in SaveRegistry.OrderedSaveables())
                    if (ReferenceEquals(s, stats)) { statsStill = true; break; }
                Assert.IsFalse(statsStill, "OnDisable phải unregister khỏi saveables");
            }
            finally { Object.DestroyImmediate(go); }
        }

        [Test]
        public void Dispatcher_CapturesFromAllSaveables_WithoutControllerKnowingSchema()
        {
            // Controller chỉ enumerate registry — không chạm trực tiếp PlayerStats/Inventory field.
            var go = new GameObject("Player");
            try
            {
                var stats = go.AddComponent<PlayerStats>();
                var realm = go.AddComponent<RealmSystem>();
                var inv = go.AddComponent<Inventory>();
                TestHelpers.Boot(stats, realm, inv);

                stats.HP = 55f;
                stats.Hunger = 44f;
                realm.currentTier = 3;
                realm.currentXp = 175f;

                var controller = go.AddComponent<SaveLoadController>();
                controller.playerStats = stats;
                controller.autoLoadOnStart = false;

                controller.Save();
                Assert.IsTrue(SaveSystem.TryLoad(out var data));
                Assert.AreEqual(55f, data.player.hp, 0.01f);
                Assert.AreEqual(44f, data.player.hunger, 0.01f);
                Assert.AreEqual(3, data.player.realmTier);
                Assert.AreEqual(175f, data.player.cultivationXp, 0.01f);
            }
            finally { Object.DestroyImmediate(go); }
        }

        [Test]
        public void Dispatcher_FixupOrder_ReappliesMaxHPBeforeAccumulatedBonuses()
        {
            // Regression: fixup 30 (ReapplySpiritRootMaxHP) phải chạy TRƯỚC fixup 50
            // (ReapplyAccumulatedBonuses), nếu ngược lại maxHP sẽ reset mất tier bonus.
            var go = new GameObject("Player");
            try
            {
                var stats = go.AddComponent<PlayerStats>();
                var realm = go.AddComponent<RealmSystem>();
                var inv = go.AddComponent<Inventory>();
                TestHelpers.Boot(stats, realm, inv);

                float baseMaxHP = stats.maxHP;

                var data = new SaveData
                {
                    player = new PlayerSaveData { hp = 1f, mana = 1f, realmTier = 3, cultivationXp = 0f },
                    world = new WorldSaveData(),
                };
                SaveSystem.Save(data);

                var controller = go.AddComponent<SaveLoadController>();
                controller.playerStats = stats;
                controller.realm = realm;
                controller.inventory = inv;
                controller.autoLoadOnStart = false;
                controller.LoadAndApply();

                // Tier 1-3 default bonuses: 10 + 10 + 15 = 35.
                Assert.AreEqual(baseMaxHP + 35f, stats.maxHP, 0.01f,
                    "maxHP phải là base + tier 1..3 bonus sau fixup 30 → 50");
            }
            finally { Object.DestroyImmediate(go); }
        }

        [Test]
        public void Dispatcher_ClampFixup_RunsLast_HP_ClampsToMaxHP()
        {
            var go = new GameObject("Player");
            try
            {
                var stats = go.AddComponent<PlayerStats>();
                var realm = go.AddComponent<RealmSystem>();
                var inv = go.AddComponent<Inventory>();
                TestHelpers.Boot(stats, realm, inv);

                var data = new SaveData
                {
                    player = new PlayerSaveData { hp = 99999f, mana = 99999f },
                    world = new WorldSaveData(),
                };
                SaveSystem.Save(data);

                var controller = go.AddComponent<SaveLoadController>();
                controller.playerStats = stats;
                controller.realm = realm;
                controller.inventory = inv;
                controller.autoLoadOnStart = false;
                controller.LoadAndApply();

                Assert.AreEqual(stats.maxHP, stats.HP, 0.01f, "HP phải clamp về maxHP");
                Assert.AreEqual(stats.maxMana, stats.Mana, 0.01f, "Mana phải clamp về maxMana");
            }
            finally { Object.DestroyImmediate(go); }
        }

        [Test]
        public void Inventory_OwnsItemDatabase_NotController()
        {
            // R6: itemDatabase move từ SaveLoadController → Inventory.itemDatabase.
            // Restore phải đọc từ inventory.itemDatabase, không phụ thuộc controller.
            var go = new GameObject("Player");
            ItemDatabase db = null;
            ItemSO stick = null;
            try
            {
                var stats = go.AddComponent<PlayerStats>();
                var inv = go.AddComponent<Inventory>();
                TestHelpers.Boot(stats, inv);

                db = ScriptableObject.CreateInstance<ItemDatabase>();
                stick = ScriptableObject.CreateInstance<ItemSO>();
                stick.itemId = "stick";
                stick.displayName = "stick";
                stick.maxStack = 99;
                db.items.Add(stick);

                // Set TRỰC TIẾP trên Inventory — KHÔNG qua controller.
                inv.itemDatabase = db;
                inv.Add(stick, 3);

                var data = new SaveData();
                // Capture qua ISaveable — không gọi SaveLoadController.
                foreach (var s in SaveRegistry.OrderedSaveables()) s.CaptureState(data);
                Assert.AreEqual(1, data.inventory.Count);
                Assert.AreEqual("stick", data.inventory[0].itemId);

                // Wipe + restore
                inv.TryConsumeSlot(0, inv.Slots[0].count);
                Assert.AreEqual(0, inv.CountOf(stick));
                foreach (var s in SaveRegistry.OrderedSaveables()) s.RestoreState(data);
                Assert.AreEqual(3, inv.CountOf(stick));
            }
            finally
            {
                Object.DestroyImmediate(go);
                if (db != null) Object.DestroyImmediate(db);
                if (stick != null) Object.DestroyImmediate(stick);
            }
        }

        [Test]
        public void SaveLoadController_DispatchesWithoutCustomItemDbWiring_IfInventoryOwnsIt()
        {
            // Controller KHÔNG cần .itemDatabase — miễn là Inventory.itemDatabase đã được set.
            var go = new GameObject("Player");
            ItemDatabase db = null;
            ItemSO stick = null;
            try
            {
                var stats = go.AddComponent<PlayerStats>();
                var inv = go.AddComponent<Inventory>();
                TestHelpers.Boot(stats, inv);

                db = ScriptableObject.CreateInstance<ItemDatabase>();
                stick = ScriptableObject.CreateInstance<ItemSO>();
                stick.itemId = "stick";
                stick.maxStack = 99;
                db.items.Add(stick);

                inv.itemDatabase = db;
                inv.Add(stick, 2);

                var controller = go.AddComponent<SaveLoadController>();
                controller.playerStats = stats;
                controller.inventory = inv;
                controller.autoLoadOnStart = false;
                // KHÔNG set controller.itemDatabase — test rằng dispatcher không cần nó.

                controller.Save();
                inv.TryConsumeSlot(0, inv.Slots[0].count);
                controller.LoadAndApply();

                Assert.AreEqual(2, inv.CountOf(stick),
                    "Controller dispatcher không cần itemDatabase nếu Inventory tự own nó");
            }
            finally
            {
                Object.DestroyImmediate(go);
                if (db != null) Object.DestroyImmediate(db);
                if (stick != null) Object.DestroyImmediate(stick);
            }
        }
    }
}
