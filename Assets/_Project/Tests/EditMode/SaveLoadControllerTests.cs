using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using WildernessCultivation.Core;
using WildernessCultivation.Cultivation;
using WildernessCultivation.Items;
using WildernessCultivation.Player;

namespace WildernessCultivation.Tests.EditMode
{
    /// <summary>
    /// EditMode tests cho SaveLoadController.LoadAndApply round-trip.
    /// Mỗi test dựng GameObject "Player" với PlayerStats + RealmSystem + Inventory +
    /// SaveLoadController, gắn ItemDatabase ScriptableObject in-memory, rồi:
    ///   1) set state (HP, tier, inventory…)
    ///   2) controller.Save()  (ghi JSON)
    ///   3) reset state về default
    ///   4) controller.LoadAndApply()  (đọc JSON + Apply)
    ///   5) assert state khôi phục đúng.
    /// </summary>
    public class SaveLoadControllerTests
    {
        GameObject playerGo;
        PlayerStats stats;
        RealmSystem realm;
        Inventory inv;
        SaveLoadController controller;
        ItemDatabase db;

        static ItemSO MakeItem(string id, bool durable = false, bool perishable = false,
            int maxStack = 99)
        {
            var so = ScriptableObject.CreateInstance<ItemSO>();
            so.itemId = id;
            so.displayName = id;
            so.maxStack = maxStack;
            so.hasDurability = durable;
            so.maxDurability = 100f;
            so.durabilityPerUse = 10f;
            so.isPerishable = perishable;
            so.freshSeconds = 100f;
            return so;
        }

        [SetUp]
        public void Setup()
        {
            SaveSystem.Delete();

            playerGo = new GameObject("Player");
            stats = playerGo.AddComponent<PlayerStats>();
            realm = playerGo.AddComponent<RealmSystem>();
            inv = playerGo.AddComponent<Inventory>();
            // EditMode does NOT auto-fire MonoBehaviour.Awake — invoke
            // manually so PlayerStats caches base maxHP, RealmSystem caches
            // sibling refs and default realms, and Inventory populates slots.
            TestHelpers.Boot(stats, realm, inv);

            db = ScriptableObject.CreateInstance<ItemDatabase>();

            controller = playerGo.AddComponent<SaveLoadController>();
            controller.playerStats = stats;
            controller.realm = realm;
            controller.inventory = inv;
            controller.itemDatabase = db;
            controller.autoLoadOnStart = false;
        }

        [TearDown]
        public void Teardown()
        {
            SaveSystem.Delete();
            if (playerGo != null) Object.DestroyImmediate(playerGo);
            if (db != null) Object.DestroyImmediate(db);
        }

        // ===== Player round-trip =====

        [Test]
        public void RoundTrip_PreservesPlayerVitals()
        {
            stats.transform.position = new Vector3(5f, 7f, 0f);
            stats.HP = 70f;
            stats.Hunger = 55f;
            stats.Thirst = 40f;
            stats.Sanity = 88f;
            stats.Mana = 25f;
            stats.BodyTemp = 35f;

            controller.Save();

            stats.HP = 0f;
            stats.Hunger = 0f;
            stats.Thirst = 0f;
            stats.Sanity = 0f;
            stats.Mana = 0f;
            stats.BodyTemp = 0f;
            stats.transform.position = Vector3.zero;

            controller.LoadAndApply();

            Assert.AreEqual(new Vector3(5f, 7f, 0f), stats.transform.position);
            Assert.AreEqual(70f, stats.HP, 0.01f);
            Assert.AreEqual(55f, stats.Hunger, 0.01f);
            Assert.AreEqual(40f, stats.Thirst, 0.01f);
            Assert.AreEqual(88f, stats.Sanity, 0.01f);
            Assert.AreEqual(25f, stats.Mana, 0.01f);
            Assert.AreEqual(35f, stats.BodyTemp, 0.01f);
        }

        [Test]
        public void LoadAndApply_BodyTempZero_DefaultsTo50()
        {
            // Save với bodyTemp = 0 (save cũ trước khi field tồn tại)
            var data = new SaveData
            {
                player = new PlayerSaveData { hp = 50f, mana = 10f, bodyTemp = 0f },
                world = new WorldSaveData(),
            };
            SaveSystem.Save(data);

            stats.BodyTemp = 99f;
            controller.LoadAndApply();
            Assert.AreEqual(50f, stats.BodyTemp, 0.01f, "bodyTemp <= 0 → default 50");
        }

        [Test]
        public void LoadAndApply_HPClampsToMaxHP()
        {
            var data = new SaveData
            {
                player = new PlayerSaveData { hp = 9999f, mana = 9999f },
                world = new WorldSaveData(),
            };
            SaveSystem.Save(data);

            controller.LoadAndApply();
            Assert.AreEqual(stats.maxHP, stats.HP, 0.01f, "HP clamp về maxHP");
            Assert.AreEqual(stats.maxMana, stats.Mana, 0.01f, "Mana clamp về maxMana");
        }

        [Test]
        public void LoadAndApply_NoFile_NoOp()
        {
            stats.HP = 42f;
            controller.LoadAndApply();
            Assert.AreEqual(42f, stats.HP, 0.01f, "không file → state không đổi");
        }

        // ===== Realm round-trip =====

        [Test]
        public void RoundTrip_PreservesRealmTierAndXp()
        {
            realm.currentTier = 4;
            realm.currentXp = 250.5f;
            realm.SpiritRoot = "Thủy";

            controller.Save();

            realm.currentTier = 0;
            realm.currentXp = 0f;
            realm.SpiritRoot = "Hỏa";

            controller.LoadAndApply();
            Assert.AreEqual(4, realm.currentTier);
            Assert.AreEqual(250.5f, realm.currentXp, 0.01f);
            Assert.AreEqual("Thủy", realm.SpiritRoot);
        }

        [Test]
        public void LoadAndApply_ReappliesAccumulatedBonusesAfterLoad()
        {
            // Default realms: T1 hpBonus=10, T2 hpBonus=10. currentTier=2 → maxHP += 20.
            float baseMaxHP = stats.maxHP;
            var data = new SaveData
            {
                player = new PlayerSaveData
                {
                    hp = 1f,
                    mana = 1f,
                    realmTier = 2,
                    cultivationXp = 0f,
                },
                world = new WorldSaveData(),
            };
            SaveSystem.Save(data);

            controller.LoadAndApply();
            Assert.AreEqual(baseMaxHP + 20f, stats.maxHP, 0.01f,
                "ReapplyAccumulatedBonuses cộng bonus tier 1..2 sau load");
        }

        // ===== Inventory round-trip =====

        [Test]
        public void RoundTrip_PreservesInventoryWithDurabilityAndFreshness()
        {
            var stick = MakeItem("stick");
            var rod = MakeItem("rod", durable: true);
            var meat = MakeItem("meat", perishable: true);
            db.items.Add(stick);
            db.items.Add(rod);
            db.items.Add(meat);

            inv.Add(stick, 5);
            inv.Add(rod, 1);
            inv.UseDurability(1, 30f); // rod: dur 100 -> 70
            inv.Add(meat, 2);
            inv.GetSlot(2).freshRemaining = 60f;

            controller.Save();

            // Wipe inventory
            for (int i = 0; i < inv.Slots.Count; i++)
                inv.TryConsumeSlot(i, inv.Slots[i].count);
            Assert.AreEqual(0, inv.CountOf(stick));

            controller.LoadAndApply();

            Assert.AreEqual(5, inv.CountOf(stick));
            Assert.AreEqual(1, inv.CountOf(rod));
            // Đếm meat — broken-skip không match perishable nên CountOf trả 2
            Assert.AreEqual(2, inv.CountOf(meat));

            // Find rod slot và check durability
            int rodSlot = -1;
            for (int i = 0; i < inv.Slots.Count; i++)
                if (inv.Slots[i].item == rod) { rodSlot = i; break; }
            Assert.GreaterOrEqual(rodSlot, 0);
            Assert.AreEqual(70f, inv.GetSlot(rodSlot).durability, 0.01f);

            // Find meat slot và check freshness
            int meatSlot = -1;
            for (int i = 0; i < inv.Slots.Count; i++)
                if (inv.Slots[i].item == meat) { meatSlot = i; break; }
            Assert.GreaterOrEqual(meatSlot, 0);
            Assert.AreEqual(60f, inv.GetSlot(meatSlot).freshRemaining, 0.01f);
        }

        [Test]
        public void RoundTrip_MultiplePerishableStacks_PreservePerSlotFreshness()
        {
            // Regression test cho bug RestoreInventory targeting wrong slot khi 2+ stack
            // cùng perishable item với freshRemaining khác nhau (Devin Review #21).
            var meat = MakeItem("meat", perishable: true);
            db.items.Add(meat);

            inv.Add(meat, 2); // 2 slots: slot 0 fresh=100, slot 1 fresh=100
            inv.GetSlot(0).freshRemaining = 30f;
            inv.GetSlot(1).freshRemaining = 80f;

            controller.Save();

            // Wipe
            for (int i = 0; i < inv.Slots.Count; i++)
                inv.TryConsumeSlot(i, inv.Slots[i].count);

            controller.LoadAndApply();

            // Sau restore: slot 0 phải là fresh=30, slot 1 phải là fresh=80 (per-slot preserved)
            Assert.AreEqual(2, inv.CountOf(meat));
            // Tìm 2 slot meat đầu tiên theo thứ tự, không assume index cố định
            int firstMeatSlot = -1, secondMeatSlot = -1;
            for (int i = 0; i < inv.Slots.Count; i++)
            {
                if (inv.Slots[i].item != meat) continue;
                if (firstMeatSlot < 0) firstMeatSlot = i;
                else { secondMeatSlot = i; break; }
            }
            Assert.GreaterOrEqual(firstMeatSlot, 0);
            Assert.GreaterOrEqual(secondMeatSlot, 0);
            Assert.AreEqual(30f, inv.GetSlot(firstMeatSlot).freshRemaining, 0.01f,
                "stack-1 freshness=30 phải giữ — KHÔNG bị stack-2 (fresh=80) ghi đè");
            Assert.AreEqual(80f, inv.GetSlot(secondMeatSlot).freshRemaining, 0.01f,
                "stack-2 freshness=80 phải giữ");
        }

        [Test]
        public void RoundTrip_MultipleDurableItems_PreservePerSlotDurability()
        {
            // Regression test tương tự cho durable items (2 rod khác durability).
            var rod = MakeItem("rod", durable: true);
            db.items.Add(rod);

            inv.Add(rod, 2); // 2 rod ở 2 slot khác nhau (durable không stack)
            inv.GetSlot(0).durability = 25f;
            inv.GetSlot(1).durability = 88f;

            controller.Save();

            for (int i = 0; i < inv.Slots.Count; i++)
                inv.TryConsumeSlot(i, inv.Slots[i].count);

            controller.LoadAndApply();

            int firstRodSlot = -1, secondRodSlot = -1;
            for (int i = 0; i < inv.Slots.Count; i++)
            {
                if (inv.Slots[i].item != rod) continue;
                if (firstRodSlot < 0) firstRodSlot = i;
                else { secondRodSlot = i; break; }
            }
            Assert.AreEqual(25f, inv.GetSlot(firstRodSlot).durability, 0.01f,
                "rod-1 dur=25 phải giữ — KHÔNG bị rod-2 (dur=88) ghi đè");
            Assert.AreEqual(88f, inv.GetSlot(secondRodSlot).durability, 0.01f);
        }

        [Test]
        public void LoadAndApply_NoItemDatabase_LogsWarningAndSkipsInventory()
        {
            controller.itemDatabase = null;

            var data = new SaveData
            {
                player = new PlayerSaveData(),
                world = new WorldSaveData(),
            };
            data.inventory.Add(new InventorySlotData { itemId = "stick", count = 3 });
            SaveSystem.Save(data);

            LogAssert.Expect(LogType.Warning, "[Save] ItemDatabase chưa được gán → không thể restore inventory.");
            controller.LoadAndApply();

            // Inventory vẫn empty
            int total = 0;
            foreach (var s in inv.Slots) total += s.count;
            Assert.AreEqual(0, total);
        }

        [Test]
        public void LoadAndApply_UnknownItemId_LogsWarningAndSkipsBadItem()
        {
            var stick = MakeItem("stick");
            db.items.Add(stick);

            var data = new SaveData
            {
                player = new PlayerSaveData(),
                world = new WorldSaveData(),
            };
            data.inventory.Add(new InventorySlotData { itemId = "stick", count = 4 });
            data.inventory.Add(new InventorySlotData { itemId = "ghost_item", count = 1 });
            SaveSystem.Save(data);

            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex(@"\[Save\] ItemDatabase không có itemId='ghost_item'"));
            controller.LoadAndApply();

            // stick vẫn restore được, ghost_item bị skip
            Assert.AreEqual(4, inv.CountOf(stick));
        }

        [Test]
        public void Save_DoesNotEmitEmptySlots()
        {
            var stick = MakeItem("stick");
            db.items.Add(stick);
            inv.Add(stick, 2);

            controller.Save();
            Assert.IsTrue(SaveSystem.TryLoad(out var loaded));
            Assert.AreEqual(1, loaded.inventory.Count, "chỉ slot có item mới được serialize");
            Assert.AreEqual("stick", loaded.inventory[0].itemId);
            Assert.AreEqual(2, loaded.inventory[0].count);
        }
    }
}
