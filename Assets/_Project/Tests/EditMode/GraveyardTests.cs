using System.IO;
using NUnit.Framework;
using UnityEngine;
using WildernessCultivation.Core;

namespace WildernessCultivation.Tests.EditMode
{
    /// <summary>
    /// EditMode tests cho Graveyard persistence: append/load/cap/remove/clear round-trip
    /// qua file <c>graveyard.json</c> ở Application.persistentDataPath.
    /// </summary>
    public class GraveyardTests
    {
        [SetUp]
        public void Setup() => Graveyard.Clear();

        [TearDown]
        public void Teardown() => Graveyard.Clear();

        [Test]
        public void Load_NoFile_ReturnsEmpty()
        {
            var data = Graveyard.Load();
            Assert.IsNotNull(data);
            Assert.IsNotNull(data.tombstones);
            Assert.AreEqual(0, data.tombstones.Count);
        }

        [Test]
        public void Append_PersistsToFile()
        {
            var entry = new TombstoneData
            {
                worldSeed = 42,
                position = new Vector3(10, 20, 0),
                daySurvived = 5,
                previousLifeRealmTier = 0,
                previousLifeWasAwakened = false,
            };
            entry.items.Add(new InventorySlotData { itemId = "wood", count = 3 });
            string id = Graveyard.Append(entry);

            Assert.IsTrue(Graveyard.HasFile);
            Assert.IsFalse(string.IsNullOrEmpty(id), "Append phải gán id");

            var data = Graveyard.Load();
            Assert.AreEqual(1, data.tombstones.Count);
            Assert.AreEqual(42, data.tombstones[0].worldSeed);
            Assert.AreEqual("wood", data.tombstones[0].items[0].itemId);
        }

        [Test]
        public void Append_RespectsMaxEntries_FIFO()
        {
            for (int i = 0; i < Graveyard.MaxEntries + 5; i++)
            {
                var t = new TombstoneData { id = $"t_{i}", daySurvived = i };
                t.items.Add(new InventorySlotData { itemId = "wood", count = 1 });
                Graveyard.Append(t);
            }
            var data = Graveyard.Load();
            Assert.LessOrEqual(data.tombstones.Count, Graveyard.MaxEntries,
                "Cap MaxEntries=10 phải giữ");
            // FIFO: 5 cái đầu (id=0..4) bị drop, còn lại t_5..t_14.
            Assert.AreEqual("t_5", data.tombstones[0].id, "Entry cũ nhất bị drop trước");
        }

        [Test]
        public void Remove_DeletesEntryById()
        {
            var t1 = new TombstoneData { id = "alpha" };
            t1.items.Add(new InventorySlotData { itemId = "wood", count = 1 });
            var t2 = new TombstoneData { id = "beta" };
            t2.items.Add(new InventorySlotData { itemId = "stone", count = 1 });
            Graveyard.Append(t1);
            Graveyard.Append(t2);
            Assert.IsTrue(Graveyard.Remove("alpha"));
            var data = Graveyard.Load();
            Assert.AreEqual(1, data.tombstones.Count);
            Assert.AreEqual("beta", data.tombstones[0].id);
        }

        [Test]
        public void Remove_NonExistentId_NoOp()
        {
            Assert.IsFalse(Graveyard.Remove("nope"));
        }

        [Test]
        public void Clear_DeletesFile()
        {
            var t = new TombstoneData();
            t.items.Add(new InventorySlotData { itemId = "wood", count = 1 });
            Graveyard.Append(t);
            Assert.IsTrue(Graveyard.HasFile);
            Graveyard.Clear();
            Assert.IsFalse(Graveyard.HasFile);
        }
    }
}
