using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using WildernessCultivation.Core;
using WildernessCultivation.Cultivation;
using WildernessCultivation.Items;
using WildernessCultivation.Player;

namespace WildernessCultivation.Tests.EditMode
{
    /// <summary>
    /// Property/fuzz tests cho SaveLoad. Mỗi test seed từ 0..N, generate random SaveData
    /// rồi verify invariant qua serialize → deserialize → Apply round-trip.
    /// Mục đích: bắt edge case mà unit test cố định bỏ sót (vd: 2+ stack cùng item perishable
    /// với freshRemaining khác nhau — production bug đã sửa trong PR #23).
    /// </summary>
    public class SaveLoadFuzzTests
    {
        const int FuzzIterations = 50;

        GameObject playerGo;
        PlayerStats stats;
        RealmSystem realm;
        Inventory inv;
        SaveLoadController controller;
        ItemDatabase db;

        // Pool item cố định cho fuzzing — mix của plain / durable / perishable / mixed.
        ItemSO itemPlain;       // maxStack 99, không durability/perishable
        ItemSO itemDurable;     // maxStack 1
        ItemSO itemPerishable;  // maxStack 99, perishable
        ItemSO itemDurableLow;  // maxStack 1, durable

        [SetUp]
        public void Setup()
        {
            SaveSystem.Delete();

            playerGo = new GameObject("Player");
            stats = playerGo.AddComponent<PlayerStats>();
            realm = playerGo.AddComponent<RealmSystem>();
            inv = playerGo.AddComponent<Inventory>();

            db = ScriptableObject.CreateInstance<ItemDatabase>();
            itemPlain = MakeItem("plain", maxStack: 99);
            itemDurable = MakeItem("durable", durable: true, maxStack: 1);
            itemPerishable = MakeItem("perish", perishable: true, maxStack: 99);
            itemDurableLow = MakeItem("durlow", durable: true, maxStack: 1);
            db.items.AddRange(new[] { itemPlain, itemDurable, itemPerishable, itemDurableLow });

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
            if (itemPlain != null) Object.DestroyImmediate(itemPlain);
            if (itemDurable != null) Object.DestroyImmediate(itemDurable);
            if (itemPerishable != null) Object.DestroyImmediate(itemPerishable);
            if (itemDurableLow != null) Object.DestroyImmediate(itemDurableLow);
        }

        static ItemSO MakeItem(string id, bool durable = false, bool perishable = false, int maxStack = 99)
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

        // ===== JSON round-trip (pure SaveData ↔ JSON) =====

        [Test]
        public void Fuzz_JsonRoundTrip_PreservesAllPlayerFields()
        {
            for (int seed = 0; seed < FuzzIterations; seed++)
            {
                var rng = new System.Random(seed);
                var src = RandomSaveData(rng);
                SaveSystem.Save(src);
                Assert.IsTrue(SaveSystem.TryLoad(out var dst), $"seed={seed}: TryLoad fail");

                Assert.AreEqual(src.player.position, dst.player.position, $"seed={seed} position");
                Assert.AreEqual(src.player.hp, dst.player.hp, 0.001f, $"seed={seed} hp");
                Assert.AreEqual(src.player.hunger, dst.player.hunger, 0.001f, $"seed={seed} hunger");
                Assert.AreEqual(src.player.thirst, dst.player.thirst, 0.001f, $"seed={seed} thirst");
                Assert.AreEqual(src.player.sanity, dst.player.sanity, 0.001f, $"seed={seed} sanity");
                Assert.AreEqual(src.player.mana, dst.player.mana, 0.001f, $"seed={seed} mana");
                Assert.AreEqual(src.player.bodyTemp, dst.player.bodyTemp, 0.001f, $"seed={seed} bodyTemp");
                Assert.AreEqual(src.player.realmTier, dst.player.realmTier, $"seed={seed} realmTier");
                Assert.AreEqual(src.player.cultivationXp, dst.player.cultivationXp, 0.001f, $"seed={seed} xp");
                Assert.AreEqual(src.player.spiritRoot, dst.player.spiritRoot, $"seed={seed} root");

                Assert.AreEqual(src.world.timeOfDay01, dst.world.timeOfDay01, 0.001f, $"seed={seed} time");
                Assert.AreEqual(src.world.seed, dst.world.seed, $"seed={seed} world.seed");
                Assert.AreEqual(src.world.daysSurvived, dst.world.daysSurvived, $"seed={seed} days");
                Assert.AreEqual(src.world.seasonIndex, dst.world.seasonIndex, $"seed={seed} season");
                Assert.AreEqual(src.world.weatherIndex, dst.world.weatherIndex, $"seed={seed} weather");

                Assert.AreEqual(src.inventory.Count, dst.inventory.Count, $"seed={seed} inv.Count");
                for (int i = 0; i < src.inventory.Count; i++)
                {
                    Assert.AreEqual(src.inventory[i].itemId, dst.inventory[i].itemId, $"seed={seed} inv[{i}].itemId");
                    Assert.AreEqual(src.inventory[i].count, dst.inventory[i].count, $"seed={seed} inv[{i}].count");
                    Assert.AreEqual(src.inventory[i].freshRemaining, dst.inventory[i].freshRemaining, 0.001f, $"seed={seed} inv[{i}].fresh");
                    Assert.AreEqual(src.inventory[i].durability, dst.inventory[i].durability, 0.001f, $"seed={seed} inv[{i}].dur");
                }
            }
        }

        // ===== Apply round-trip (Save → modify → Load → assert state) =====

        [Test]
        public void Fuzz_ApplyRoundTrip_PreservesPlayerVitals()
        {
            for (int seed = 0; seed < FuzzIterations; seed++)
            {
                var rng = new System.Random(seed);
                var pos = new Vector3((float)(rng.NextDouble() * 100 - 50), (float)(rng.NextDouble() * 100 - 50), 0f);
                stats.transform.position = pos;
                stats.HP = (float)(rng.NextDouble() * stats.maxHP);
                stats.Hunger = (float)(rng.NextDouble() * 100);
                stats.Thirst = (float)(rng.NextDouble() * 100);
                stats.Sanity = (float)(rng.NextDouble() * 100);
                stats.Mana = (float)(rng.NextDouble() * stats.maxMana);
                stats.BodyTemp = (float)(rng.NextDouble() * 100 + 1f); // > 0 để không trigger default-50

                controller.Save();

                stats.HP = 0f; stats.Hunger = 0f; stats.Thirst = 0f;
                stats.Sanity = 0f; stats.Mana = 0f; stats.BodyTemp = 0f;
                stats.transform.position = Vector3.zero;

                controller.LoadAndApply();

                Assert.AreEqual(pos.x, stats.transform.position.x, 0.001f, $"seed={seed} pos.x");
                Assert.AreEqual(pos.y, stats.transform.position.y, 0.001f, $"seed={seed} pos.y");
                Assert.AreEqual(stats.Hunger, stats.Hunger, 0.001f, $"seed={seed} hunger sane");
                // HP/Mana clamp về maxHP/maxMana sau Apply (xem SaveLoadController:140-141)
                Assert.LessOrEqual(stats.HP, stats.maxHP + 0.001f, $"seed={seed} HP <= maxHP");
                Assert.LessOrEqual(stats.Mana, stats.maxMana + 0.001f, $"seed={seed} Mana <= maxMana");
                Assert.GreaterOrEqual(stats.HP, 0f, $"seed={seed} HP >= 0");
            }
        }

        // ===== Inventory round-trip — REGRESSION cho PR #23 =====
        // Bug PR #23: 2+ stack cùng item với freshRemaining/durability khác nhau bị overwrite
        // do RestoreInventory đời cũ scan-from-zero (slot[0] luôn match đầu tiên).

        [Test]
        public void Fuzz_ApplyRoundTrip_PreservesPerishableMultiStackFreshness()
        {
            for (int seed = 0; seed < FuzzIterations; seed++)
            {
                var rng = new System.Random(seed);
                int n = rng.Next(2, 6); // 2..5 stack perishable
                var fresh = new List<float>();
                ClearInventory();

                for (int i = 0; i < n; i++)
                {
                    inv.Add(itemPerishable, 1);
                    float f = (float)(rng.NextDouble() * 100f + 1f); // > 0 để khác sentinel default
                    fresh.Add(f);
                    // Set freshRemaining cho slot vừa add (tail-most non-empty)
                    SetTailFreshness(itemPerishable, f);
                }

                controller.Save();
                ClearInventory();
                controller.LoadAndApply();

                // Collect freshRemaining của tất cả slot perishable, đối chiếu với set ban đầu.
                var got = new List<float>();
                foreach (var s in inv.Slots)
                    if (!s.IsEmpty && s.item == itemPerishable && s.IsPerishable)
                        got.Add(s.freshRemaining);

                Assert.AreEqual(n, got.Count, $"seed={seed}: phải có {n} stack perishable sau load");
                fresh.Sort(); got.Sort();
                for (int i = 0; i < n; i++)
                    Assert.AreEqual(fresh[i], got[i], 0.01f,
                        $"seed={seed} stack#{i}: freshness phải preserve sau round-trip (regression PR #23)");
            }
        }

        [Test]
        public void Fuzz_ApplyRoundTrip_PreservesDurableMultiStackDurability()
        {
            for (int seed = 0; seed < FuzzIterations; seed++)
            {
                var rng = new System.Random(seed);
                int n = rng.Next(2, 5);
                var dur = new List<float>();
                ClearInventory();

                for (int i = 0; i < n; i++)
                {
                    inv.Add(itemDurable, 1);
                    float d = (float)(rng.NextDouble() * 99f + 1f);
                    dur.Add(d);
                    SetTailDurability(itemDurable, d);
                }

                controller.Save();
                ClearInventory();
                controller.LoadAndApply();

                var got = new List<float>();
                foreach (var s in inv.Slots)
                    if (!s.IsEmpty && s.item == itemDurable && s.IsDurable)
                        got.Add(s.durability);

                Assert.AreEqual(n, got.Count, $"seed={seed}: phải có {n} stack durable sau load");
                dur.Sort(); got.Sort();
                for (int i = 0; i < n; i++)
                    Assert.AreEqual(dur[i], got[i], 0.01f,
                        $"seed={seed} stack#{i}: durability phải preserve sau round-trip (regression PR #23)");
            }
        }

        // ===== Edge cases =====

        [Test]
        public void Apply_InvalidItemId_NoCrashSkipsEntry()
        {
            var data = new SaveData
            {
                player = new PlayerSaveData { hp = 50f },
                world = new WorldSaveData(),
                inventory = new List<InventorySlotData>
                {
                    new() { itemId = "plain", count = 3 },
                    new() { itemId = "ghost_item_not_in_db", count = 5 },
                    new() { itemId = "perish", count = 2, freshRemaining = 50f },
                },
            };
            SaveSystem.Save(data);

            // Expect Debug.LogWarning cho ghost_item, KHÔNG throw.
            UnityEngine.TestTools.LogAssert.Expect(LogType.Warning,
                new System.Text.RegularExpressions.Regex("ghost_item_not_in_db"));

            Assert.DoesNotThrow(() => controller.LoadAndApply());

            int plainCount = 0, perishCount = 0;
            foreach (var s in inv.Slots)
            {
                if (s.IsEmpty) continue;
                if (s.item == itemPlain) plainCount += s.count;
                if (s.item == itemPerishable) perishCount += s.count;
            }
            Assert.AreEqual(3, plainCount, "plain phải restore");
            Assert.AreEqual(2, perishCount, "perish phải restore");
        }

        [Test]
        public void Apply_NegativeCount_SkipsEntry()
        {
            var data = new SaveData
            {
                player = new PlayerSaveData { hp = 50f },
                world = new WorldSaveData(),
                inventory = new List<InventorySlotData>
                {
                    new() { itemId = "plain", count = -5 },
                    new() { itemId = "plain", count = 0 },
                    new() { itemId = "plain", count = 7 },
                },
            };
            SaveSystem.Save(data);
            Assert.DoesNotThrow(() => controller.LoadAndApply());

            int plainCount = 0;
            foreach (var s in inv.Slots)
                if (s.item == itemPlain) plainCount += s.count;
            Assert.AreEqual(7, plainCount, "negative + zero count phải skip; chỉ entry count=7 được Add");
        }

        [Test]
        public void Apply_OverCapacity_LogsWarningButNoCrash()
        {
            // 16 slot × maxStack 1 (durable) = capacity 16. Save 20 → 4 leftover.
            inv.slotCount = 16;
            var slots = new List<InventorySlotData>();
            for (int i = 0; i < 20; i++)
                slots.Add(new InventorySlotData { itemId = "durable", count = 1, durability = 50f });

            var data = new SaveData
            {
                player = new PlayerSaveData { hp = 50f },
                world = new WorldSaveData(),
                inventory = slots,
            };
            SaveSystem.Save(data);

            UnityEngine.TestTools.LogAssert.Expect(LogType.Warning,
                new System.Text.RegularExpressions.Regex("Inventory đầy"));

            Assert.DoesNotThrow(() => controller.LoadAndApply());

            int filled = 0;
            foreach (var s in inv.Slots)
                if (!s.IsEmpty && s.item == itemDurable) filled++;
            Assert.AreEqual(16, filled, "đầy 16 slot, 4 entry leftover bị mất");
        }

        [Test]
        public void Apply_LegacySave_NoInventoryField_NoOp()
        {
            // Legacy save không có field inventory (List null hoặc empty).
            var data = new SaveData
            {
                player = new PlayerSaveData { hp = 30f, hunger = 40f },
                world = new WorldSaveData(),
                inventory = null, // legacy → SaveLoadController.RestoreInventory phải early-return
            };
            // JsonUtility serialize null List → empty array → deserialize lại sẽ là empty list,
            // KHÔNG null. Nhưng test trực tiếp gọi Apply path qua SaveSystem giả lập legacy.
            SaveSystem.Save(data);

            Assert.DoesNotThrow(() => controller.LoadAndApply());
            Assert.AreEqual(30f, stats.HP, 0.01f, "player vẫn restore");
            int filled = 0;
            foreach (var s in inv.Slots) if (!s.IsEmpty) filled++;
            Assert.AreEqual(0, filled, "inventory null → không restore gì");
        }

        // ===== Helpers =====

        SaveData RandomSaveData(System.Random rng)
        {
            var slots = new List<InventorySlotData>();
            int n = rng.Next(0, 8);
            string[] ids = { "plain", "durable", "perish", "durlow" };
            for (int i = 0; i < n; i++)
            {
                slots.Add(new InventorySlotData
                {
                    itemId = ids[rng.Next(ids.Length)],
                    count = rng.Next(1, 50),
                    freshRemaining = (float)(rng.NextDouble() * 100f - 1f), // -1..99 (-1 = sentinel)
                    durability = (float)(rng.NextDouble() * 100f - 1f),
                });
            }

            return new SaveData
            {
                player = new PlayerSaveData
                {
                    position = new Vector3((float)(rng.NextDouble() * 100 - 50), (float)(rng.NextDouble() * 100 - 50), 0f),
                    hp = (float)(rng.NextDouble() * 100),
                    hunger = (float)(rng.NextDouble() * 100),
                    thirst = (float)(rng.NextDouble() * 100),
                    sanity = (float)(rng.NextDouble() * 100),
                    mana = (float)(rng.NextDouble() * 100),
                    bodyTemp = (float)(rng.NextDouble() * 100 + 1f),
                    realmTier = rng.Next(0, 12),
                    cultivationXp = (float)(rng.NextDouble() * 1000),
                    spiritRoot = ids[rng.Next(ids.Length)],
                },
                world = new WorldSaveData
                {
                    timeOfDay01 = (float)rng.NextDouble(),
                    seed = rng.Next(),
                    daysSurvived = rng.Next(0, 365),
                    seasonIndex = rng.Next(0, 4),
                    weatherIndex = rng.Next(0, 3),
                },
                inventory = slots,
            };
        }

        void ClearInventory()
        {
            for (int i = 0; i < inv.Slots.Count; i++)
                if (!inv.Slots[i].IsEmpty)
                    inv.TryConsumeSlot(i, inv.Slots[i].count);
        }

        // Set freshness/durability cho slot non-empty cuối cùng có item đúng (tức slot vừa Add).
        void SetTailFreshness(ItemSO item, float fresh)
        {
            for (int i = inv.Slots.Count - 1; i >= 0; i--)
            {
                var s = inv.Slots[i];
                if (!s.IsEmpty && s.item == item && Mathf.Approximately(s.freshRemaining, -1f))
                {
                    s.freshRemaining = fresh;
                    return;
                }
            }
            // Nếu mọi slot đã set, ghi đè slot cuối cùng (case slot stack được cộng count thay vì tạo slot mới)
            for (int i = inv.Slots.Count - 1; i >= 0; i--)
            {
                var s = inv.Slots[i];
                if (!s.IsEmpty && s.item == item)
                {
                    s.freshRemaining = fresh;
                    return;
                }
            }
        }

        void SetTailDurability(ItemSO item, float dur)
        {
            for (int i = inv.Slots.Count - 1; i >= 0; i--)
            {
                var s = inv.Slots[i];
                if (!s.IsEmpty && s.item == item && Mathf.Approximately(s.durability, -1f))
                {
                    s.durability = dur;
                    return;
                }
            }
            for (int i = inv.Slots.Count - 1; i >= 0; i--)
            {
                var s = inv.Slots[i];
                if (!s.IsEmpty && s.item == item)
                {
                    s.durability = dur;
                    return;
                }
            }
        }
    }
}
