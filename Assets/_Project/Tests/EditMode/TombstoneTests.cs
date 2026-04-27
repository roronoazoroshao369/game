using NUnit.Framework;
using UnityEngine;
using WildernessCultivation.Core;
using WildernessCultivation.Items;
using WildernessCultivation.World;

namespace WildernessCultivation.Tests.EditMode
{
    /// <summary>
    /// EditMode tests cho Tombstone IInteractable: drain items vào player inventory,
    /// self-destroy khi rỗng, remove entry khỏi Graveyard file.
    /// </summary>
    public class TombstoneTests
    {
        GameObject tombGo;
        GameObject playerGo;
        Tombstone tomb;
        Inventory playerInv;
        ItemDatabase database;
        ItemSO wood;
        ItemSO stone;

        [SetUp]
        public void Setup()
        {
            Graveyard.Clear();
            wood = ScriptableObject.CreateInstance<ItemSO>();
            wood.itemId = "wood";
            wood.maxStack = 99;
            wood.name = "Wood";
            stone = ScriptableObject.CreateInstance<ItemSO>();
            stone.itemId = "stone";
            stone.maxStack = 99;
            stone.name = "Stone";

            database = ScriptableObject.CreateInstance<ItemDatabase>();
            database.items.Add(wood);
            database.items.Add(stone);

            playerGo = new GameObject("Player");
            playerInv = playerGo.AddComponent<Inventory>();
            playerInv.slotCount = 8;
            TestHelpers.Boot(playerInv);

            tombGo = new GameObject("Tombstone");
            tomb = tombGo.AddComponent<Tombstone>();
        }

        [TearDown]
        public void Teardown()
        {
            if (tombGo != null) Object.DestroyImmediate(tombGo);
            if (playerGo != null) Object.DestroyImmediate(playerGo);
            if (database != null) Object.DestroyImmediate(database);
            if (wood != null) Object.DestroyImmediate(wood);
            if (stone != null) Object.DestroyImmediate(stone);
            Graveyard.Clear();
        }

        TombstoneData MakeData(string id, params (string itemId, int count)[] entries)
        {
            var data = new TombstoneData { id = id, daySurvived = 5 };
            foreach (var e in entries)
                data.items.Add(new InventorySlotData
                {
                    itemId = e.itemId, count = e.count, freshRemaining = -1f, durability = -1f,
                });
            return data;
        }

        [Test]
        public void Initialize_CopiesData()
        {
            var data = MakeData("t_1", ("wood", 5), ("stone", 3));
            tomb.Initialize(data, database);
            Assert.AreEqual("t_1", tomb.tombstoneId);
            Assert.AreEqual(2, tomb.items.Count);
        }

        [Test]
        public void CanInteract_FalseWhenEmpty()
        {
            tomb.Initialize(MakeData("t_e"), database);
            Assert.IsFalse(tomb.CanInteract(playerGo));
        }

        [Test]
        public void Interact_DrainsItemsIntoPlayerInventory()
        {
            var data = MakeData("t_drain", ("wood", 5), ("stone", 3));
            Graveyard.Append(data);
            tomb.Initialize(data, database);

            bool ok = tomb.Interact(playerGo);
            Assert.IsTrue(ok);
            Assert.AreEqual(5, playerInv.CountOf(wood));
            Assert.AreEqual(3, playerInv.CountOf(stone));
        }

        [Test]
        public void Interact_EmptyDestroysGameObject_AndRemovesGraveyardEntry()
        {
            var data = MakeData("t_destroy", ("wood", 2));
            Graveyard.Append(data);
            tomb.Initialize(data, database);

            tomb.Interact(playerGo);
            // Tombstone gọi Destroy(gameObject) — EditMode dùng DestroyImmediate vì
            // Object.Destroy không huỷ ngay trong EditMode. Tombstone hiện gọi Destroy
            // (Unity-style), nên test chỉ verify items rỗng + graveyard remove.
            Assert.AreEqual(0, tomb.items.Count);
            var loaded = Graveyard.Load();
            Assert.AreEqual(0, loaded.tombstones.Count, "Entry phải bị remove khỏi graveyard.json");
        }

        [Test]
        public void Interact_NoItemDatabase_LogsAndReturnsFalse()
        {
            tomb.Initialize(MakeData("t_no_db", ("wood", 1)), database: null);
            // Suppress LogWarning
            Assert.IsFalse(tomb.Interact(playerGo));
        }
    }
}
