using NUnit.Framework;
using UnityEngine;
using WildernessCultivation.Crafting;
using WildernessCultivation.Items;
using WildernessCultivation.World;

namespace WildernessCultivation.Tests.PlayMode
{
    /// <summary>
    /// PlayMode tests cho Workbench.Interact: repair flow + atomic consume
    /// (Devin Review #15.2 invariant: Repair fail → 0 material consumed).
    /// </summary>
    public class WorkbenchTests
    {
        GameObject playerGo;
        Inventory playerInv;
        GameObject workbenchGo;
        Workbench workbench;

        static ItemSO MakeItem(string id, bool durable = false, int maxStack = 99)
        {
            var so = ScriptableObject.CreateInstance<ItemSO>();
            so.itemId = id;
            so.displayName = id;
            so.maxStack = maxStack;
            so.hasDurability = durable;
            so.maxDurability = 100f;
            so.durabilityPerUse = 10f;
            return so;
        }

        [SetUp]
        public void Setup()
        {
            playerGo = new GameObject("Player");
            playerInv = playerGo.AddComponent<Inventory>();

            workbenchGo = new GameObject("Workbench");
            workbench = workbenchGo.AddComponent<Workbench>();
        }

        [TearDown]
        public void Teardown()
        {
            if (playerGo != null) Object.Destroy(playerGo);
            if (workbenchGo != null) Object.Destroy(workbenchGo);
        }

        [Test]
        public void Awake_SetsCraftStationMarkerToWorkbench()
        {
            var marker = workbenchGo.GetComponent<CraftStationMarker>();
            Assert.IsNotNull(marker, "RequireComponent thêm CraftStationMarker");
            Assert.AreEqual(CraftStation.Workbench, marker.station);
        }

        [Test]
        public void Interact_NoDamagedItem_DoesNotConsumeMaterial()
        {
            var stick = MakeItem("stick");
            var rod = MakeItem("rod", durable: true);
            playerInv.Add(stick, 5);
            playerInv.Add(rod, 1); // full bền

            workbench.repairMaterial = stick;
            workbench.repairCost = 1;

            Assert.IsTrue(workbench.Interact(playerGo));
            Assert.AreEqual(5, playerInv.CountOf(stick), "không có gì cần sửa → 0 stick consumed");
        }

        [Test]
        public void Interact_DamagedItemAndEnoughMaterial_RepairsAndConsumes()
        {
            var stick = MakeItem("stick");
            var rod = MakeItem("rod", durable: true);
            playerInv.Add(stick, 5);
            playerInv.Add(rod, 1);
            playerInv.UseDurability(playerInv.Slots.Count - 1, 50f); // dur 100 -> 50 (rod ở slot cuối)

            int rodSlot = playerInv.FindFirstDamagedSlot();
            Assert.GreaterOrEqual(rodSlot, 0);

            workbench.repairMaterial = stick;
            workbench.repairCost = 1;
            workbench.repairAmount = -1f;

            Assert.IsTrue(workbench.Interact(playerGo));
            Assert.AreEqual(100f, playerInv.GetSlot(rodSlot).durability, 0.01f, "rod sửa full");
            Assert.AreEqual(4, playerInv.CountOf(stick), "tốn đúng 1 stick");
        }

        [Test]
        public void Interact_DamagedItemButInsufficientMaterial_DoesNotConsumeOrRepair()
        {
            var stick = MakeItem("stick");
            var rod = MakeItem("rod", durable: true);
            playerInv.Add(rod, 1);
            playerInv.UseDurability(0, 50f); // rod dur 50

            workbench.repairMaterial = stick;
            workbench.repairCost = 2;
            workbench.repairAmount = -1f;
            // Player có 0 stick

            Assert.IsTrue(workbench.Interact(playerGo));
            Assert.AreEqual(50f, playerInv.GetSlot(0).durability, 0.01f,
                "rod KHÔNG được sửa khi thiếu material");
            Assert.AreEqual(0, playerInv.CountOf(stick));
        }

        [Test]
        public void Interact_RepairMaterialEqualsDamagedItem_RefusesAndDoesNotMutate()
        {
            // Edge case: cấu hình silly — repairMaterial trùng với loại item đang sửa.
            var rod = MakeItem("rod", durable: true);
            playerInv.Add(rod, 1);
            playerInv.UseDurability(0, 50f);

            workbench.repairMaterial = rod;
            workbench.repairCost = 1;

            // Workbench log Warning ở case này — không fail test (chỉ LogError mới fail mặc định).
            Assert.IsTrue(workbench.Interact(playerGo));
            Assert.AreEqual(50f, playerInv.GetSlot(0).durability, 0.01f, "không repair");
            Assert.AreEqual(1, playerInv.CountOf(rod), "không consume");
        }

        [Test]
        public void Interact_NoRepairMaterialConfigured_RepairsForFree()
        {
            var rod = MakeItem("rod", durable: true);
            playerInv.Add(rod, 1);
            playerInv.UseDurability(0, 50f);

            workbench.repairMaterial = null;
            workbench.repairCost = 0;

            Assert.IsTrue(workbench.Interact(playerGo));
            Assert.AreEqual(100f, playerInv.GetSlot(0).durability, 0.01f);
        }

        [Test]
        public void Interact_NullActor_ReturnsFalse()
        {
            Assert.IsFalse(workbench.Interact(null));
        }

        [Test]
        public void Interact_ActorWithoutInventory_ReturnsFalse()
        {
            var noInv = new GameObject("NoInv");
            try
            {
                Assert.IsFalse(workbench.Interact(noInv));
            }
            finally
            {
                Object.Destroy(noInv);
            }
        }
    }
}
