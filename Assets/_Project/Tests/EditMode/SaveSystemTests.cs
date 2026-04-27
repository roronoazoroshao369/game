using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using WildernessCultivation.Core;

namespace WildernessCultivation.Tests.EditMode
{
    /// <summary>
    /// EditMode tests cho SaveSystem (low-level JSON file I/O round-trip).
    /// Mỗi test tự Delete trước + sau để tránh leak giữa các test (SaveSystem dùng path
    /// duy nhất Application.persistentDataPath/save_slot_0.json).
    /// </summary>
    public class SaveSystemTests
    {
        [SetUp]
        public void Setup() => SaveSystem.Delete();

        [TearDown]
        public void Teardown() => SaveSystem.Delete();

        [Test]
        public void TryLoad_NoFile_ReturnsFalseAndNullData()
        {
            Assert.IsFalse(SaveSystem.TryLoad(out var data));
            Assert.IsNull(data);
        }

        [Test]
        public void Save_TryLoad_RoundTrip_PreservesAllPlayerFields()
        {
            var orig = new SaveData
            {
                player = new PlayerSaveData
                {
                    position = new Vector3(12.5f, -3f, 0f),
                    hp = 75f,
                    hunger = 60f,
                    thirst = 50f,
                    sanity = 80f,
                    mana = 22f,
                    bodyTemp = 48f,
                    realmTier = 5,
                    cultivationXp = 1234.5f,
                    spiritRoot = "Hỏa",
                    isAwakened = true,
                    phamFailStreak = 3,
                },
                world = new WorldSaveData
                {
                    timeOfDay01 = 0.42f,
                    seed = 99887766,
                    daysSurvived = 7,
                    seasonIndex = 2,
                    weatherIndex = 1,
                },
            };

            SaveSystem.Save(orig);
            Assert.IsTrue(SaveSystem.TryLoad(out var loaded));
            Assert.IsNotNull(loaded);
            Assert.IsNotNull(loaded.player);
            Assert.AreEqual(orig.player.position, loaded.player.position);
            Assert.AreEqual(orig.player.hp, loaded.player.hp, 0.01f);
            Assert.AreEqual(orig.player.hunger, loaded.player.hunger, 0.01f);
            Assert.AreEqual(orig.player.thirst, loaded.player.thirst, 0.01f);
            Assert.AreEqual(orig.player.sanity, loaded.player.sanity, 0.01f);
            Assert.AreEqual(orig.player.mana, loaded.player.mana, 0.01f);
            Assert.AreEqual(orig.player.bodyTemp, loaded.player.bodyTemp, 0.01f);
            Assert.AreEqual(orig.player.realmTier, loaded.player.realmTier);
            Assert.AreEqual(orig.player.cultivationXp, loaded.player.cultivationXp, 0.01f);
            Assert.AreEqual(orig.player.spiritRoot, loaded.player.spiritRoot);
            Assert.AreEqual(orig.player.isAwakened, loaded.player.isAwakened);
            Assert.AreEqual(orig.player.phamFailStreak, loaded.player.phamFailStreak);
        }

        [Test]
        public void Save_TryLoad_RoundTrip_PreservesWorldFields()
        {
            var orig = new SaveData
            {
                player = new PlayerSaveData(),
                world = new WorldSaveData
                {
                    timeOfDay01 = 0.99f,
                    seed = 42,
                    daysSurvived = 100,
                    seasonIndex = 3,
                    weatherIndex = 2,
                },
            };
            SaveSystem.Save(orig);
            Assert.IsTrue(SaveSystem.TryLoad(out var loaded));
            Assert.IsNotNull(loaded.world);
            Assert.AreEqual(orig.world.timeOfDay01, loaded.world.timeOfDay01, 0.01f);
            Assert.AreEqual(orig.world.seed, loaded.world.seed);
            Assert.AreEqual(orig.world.daysSurvived, loaded.world.daysSurvived);
            Assert.AreEqual(orig.world.seasonIndex, loaded.world.seasonIndex);
            Assert.AreEqual(orig.world.weatherIndex, loaded.world.weatherIndex);
        }

        [Test]
        public void Save_TryLoad_RoundTrip_PreservesInventoryList()
        {
            var orig = new SaveData
            {
                player = new PlayerSaveData(),
                world = new WorldSaveData(),
                inventory = new List<InventorySlotData>
                {
                    new() { itemId = "stick", count = 7, freshRemaining = -1f, durability = -1f },
                    new() { itemId = "raw_meat", count = 3, freshRemaining = 200f, durability = -1f },
                    new() { itemId = "rod", count = 1, freshRemaining = -1f, durability = 65.5f },
                },
            };
            SaveSystem.Save(orig);
            Assert.IsTrue(SaveSystem.TryLoad(out var loaded));
            Assert.AreEqual(3, loaded.inventory.Count);
            Assert.AreEqual("stick", loaded.inventory[0].itemId);
            Assert.AreEqual(7, loaded.inventory[0].count);
            Assert.AreEqual(200f, loaded.inventory[1].freshRemaining, 0.01f);
            Assert.AreEqual(65.5f, loaded.inventory[2].durability, 0.01f);
        }

        [Test]
        public void Save_OverwritesExistingFile()
        {
            SaveSystem.Save(new SaveData { player = new PlayerSaveData { hp = 10f } });
            SaveSystem.Save(new SaveData { player = new PlayerSaveData { hp = 99f } });

            Assert.IsTrue(SaveSystem.TryLoad(out var loaded));
            Assert.AreEqual(99f, loaded.player.hp, 0.01f);
        }

        [Test]
        public void Save_StampsSavedAtUtc()
        {
            SaveSystem.Save(new SaveData { player = new PlayerSaveData() });
            Assert.IsTrue(SaveSystem.TryLoad(out var loaded));
            Assert.IsFalse(string.IsNullOrEmpty(loaded.savedAtUtc));
            // ISO8601 format có chữ T
            Assert.IsTrue(loaded.savedAtUtc.Contains("T"));
        }

        [Test]
        public void Delete_RemovesFile_TryLoadAfterReturnsFalse()
        {
            SaveSystem.Save(new SaveData { player = new PlayerSaveData() });
            Assert.IsTrue(SaveSystem.TryLoad(out _));

            SaveSystem.Delete();
            Assert.IsFalse(SaveSystem.TryLoad(out var data));
            Assert.IsNull(data);
        }

        [Test]
        public void Delete_OnMissingFile_DoesNotThrow()
        {
            // Đã Delete trong SetUp, file không tồn tại
            Assert.DoesNotThrow(() => SaveSystem.Delete());
        }

        [Test]
        public void HasSave_FalseWhenNoFile()
        {
            Assert.IsFalse(SaveSystem.HasSave);
        }

        [Test]
        public void HasSave_TrueAfterSave_FalseAfterDelete()
        {
            SaveSystem.Save(new SaveData { player = new PlayerSaveData() });
            Assert.IsTrue(SaveSystem.HasSave);

            SaveSystem.Delete();
            Assert.IsFalse(SaveSystem.HasSave);
        }
    }
}
