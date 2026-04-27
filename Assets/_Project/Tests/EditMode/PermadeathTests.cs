using NUnit.Framework;
using UnityEngine;
using WildernessCultivation.Core;
using WildernessCultivation.Items;
using WildernessCultivation.Player;

namespace WildernessCultivation.Tests.EditMode
{
    /// <summary>
    /// EditMode tests cho PlayerStats.ExecutePermadeath: dump inventory thành tombstone,
    /// xoá save slot, ghi MetaStats lifetime stats.
    /// EditMode không reload scene (Application.isPlaying = false trong test runner).
    /// </summary>
    public class PermadeathTests
    {
        GameObject playerGo;
        PlayerStats stats;
        Inventory inv;
        ItemSO wood;

        [SetUp]
        public void Setup()
        {
            Graveyard.Clear();
            MetaStats.Clear();
            SaveSystem.Delete();

            wood = ScriptableObject.CreateInstance<ItemSO>();
            wood.itemId = "wood";
            wood.maxStack = 99;

            playerGo = new GameObject("Player");
            stats = playerGo.AddComponent<PlayerStats>();
            inv = playerGo.AddComponent<Inventory>();
            inv.slotCount = 4;
            TestHelpers.Boot(stats, inv);

            inv.Add(wood, 7);
        }

        [TearDown]
        public void Teardown()
        {
            if (playerGo != null) Object.DestroyImmediate(playerGo);
            if (wood != null) Object.DestroyImmediate(wood);
            Graveyard.Clear();
            MetaStats.Clear();
            SaveSystem.Delete();
        }

        [Test]
        public void ExecutePermadeath_AppendsTombstoneWithInventorySnapshot()
        {
            stats.ExecutePermadeath();
            var data = Graveyard.Load();
            Assert.AreEqual(1, data.tombstones.Count);
            var t = data.tombstones[0];
            Assert.AreEqual(1, t.items.Count, "1 slot wood snapshot");
            Assert.AreEqual("wood", t.items[0].itemId);
            Assert.AreEqual(7, t.items[0].count);
        }

        [Test]
        public void ExecutePermadeath_RecordsMetaStats()
        {
            stats.IsAwakened = true;
            stats.ExecutePermadeath();
            var meta = MetaStats.Load();
            Assert.AreEqual(1, meta.totalDeaths);
            Assert.IsTrue(meta.everAwakened, "wasAwakened=true → everAwakened flag");
        }

        [Test]
        public void ExecutePermadeath_DeletesSaveSlot()
        {
            // Tạo save trước.
            SaveSystem.Save(new SaveData
            {
                player = new PlayerSaveData { hp = 50f },
                world = new WorldSaveData(),
            });
            Assert.IsTrue(SaveSystem.HasSave);

            stats.ExecutePermadeath();
            Assert.IsFalse(SaveSystem.HasSave, "Save slot phải bị xoá khi chết");
        }

        [Test]
        public void ExecutePermadeath_KeepsGraveyardFile()
        {
            // Append 1 tombstone trước (đại diện cho đời cũ).
            var prev = new TombstoneData { id = "older" };
            prev.items.Add(new InventorySlotData { itemId = "wood", count = 1 });
            Graveyard.Append(prev);

            stats.ExecutePermadeath();
            var data = Graveyard.Load();
            Assert.AreEqual(2, data.tombstones.Count,
                "Graveyard phải giữ entry cũ + add mới (cross-life)");
        }
    }
}
