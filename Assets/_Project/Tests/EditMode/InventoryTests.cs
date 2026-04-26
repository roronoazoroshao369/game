using NUnit.Framework;
using UnityEngine;
using WildernessCultivation.Items;

namespace WildernessCultivation.Tests.EditMode
{
    /// <summary>
    /// EditMode tests cho Inventory: Repair / UseDurability / TransferSlot và invariant
    /// "broken items không được tính bởi CountOf/TryConsume".
    /// Chạy qua Unity Test Runner (game-ci/unity-test-runner@v4 trong CI).
    /// </summary>
    public class InventoryTests
    {
        GameObject go;
        Inventory inv;

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
            go = new GameObject("Inv");
            inv = go.AddComponent<Inventory>();
            // EditMode does NOT auto-fire MonoBehaviour.Awake — invoke
            // manually so Inventory.Awake populates the slots list.
            TestHelpers.Boot(inv);
        }

        [TearDown]
        public void Teardown()
        {
            if (go != null) Object.DestroyImmediate(go);
        }

        // ===== Repair =====

        [Test]
        public void Repair_NegativeAmount_RestoresFullyToMax()
        {
            var rod = MakeItem("rod", durable: true);
            inv.Add(rod, 1);
            Assert.IsTrue(inv.UseDurability(0, 60f)); // 100 -> 40
            Assert.AreEqual(40f, inv.GetSlot(0).durability, 0.01f);

            Assert.IsTrue(inv.Repair(0, -1f));
            Assert.AreEqual(100f, inv.GetSlot(0).durability, 0.01f);
        }

        [Test]
        public void Repair_PartialAmount_AddsAndClampsToMax()
        {
            var rod = MakeItem("rod", durable: true);
            inv.Add(rod, 1);
            inv.UseDurability(0, 80f); // 100 -> 20
            Assert.IsTrue(inv.Repair(0, 30f));
            Assert.AreEqual(50f, inv.GetSlot(0).durability, 0.01f);

            Assert.IsTrue(inv.Repair(0, 999f)); // clamp
            Assert.AreEqual(100f, inv.GetSlot(0).durability, 0.01f);
        }

        [Test]
        public void Repair_AlreadyFullDurability_ReturnsFalse()
        {
            var rod = MakeItem("rod", durable: true);
            inv.Add(rod, 1);
            Assert.IsFalse(inv.Repair(0, -1f));
            Assert.AreEqual(100f, inv.GetSlot(0).durability, 0.01f);
        }

        [Test]
        public void Repair_NonDurableItem_ReturnsFalse()
        {
            var stick = MakeItem("stick", durable: false);
            inv.Add(stick, 1);
            Assert.IsFalse(inv.Repair(0, -1f));
        }

        [Test]
        public void Repair_EmptySlot_ReturnsFalse()
        {
            Assert.IsFalse(inv.Repair(0, -1f));
        }

        [Test]
        public void Repair_BrokenItem_RestoresAndClearsBrokenFlag()
        {
            var rod = MakeItem("rod", durable: true);
            inv.Add(rod, 1);
            inv.UseDurability(0, 200f); // dur -> 0
            Assert.IsTrue(inv.GetSlot(0).IsBroken);

            Assert.IsTrue(inv.Repair(0, -1f));
            Assert.IsFalse(inv.GetSlot(0).IsBroken);
            Assert.AreEqual(100f, inv.GetSlot(0).durability, 0.01f);
        }

        // ===== UseDurability =====

        [Test]
        public void UseDurability_DefaultAmount_DecrementsByPerUse()
        {
            var rod = MakeItem("rod", durable: true); // perUse = 10
            inv.Add(rod, 1);
            Assert.IsTrue(inv.UseDurability(0));
            Assert.AreEqual(90f, inv.GetSlot(0).durability, 0.01f);
        }

        [Test]
        public void UseDurability_ToZero_KeepsItemInSlotAsBroken()
        {
            var rod = MakeItem("rod", durable: true);
            inv.Add(rod, 1);
            Assert.IsTrue(inv.UseDurability(0, 200f));

            // Sau PR #15: broken item không bị xoá khỏi inventory nữa
            Assert.IsFalse(inv.GetSlot(0).IsEmpty);
            Assert.IsTrue(inv.GetSlot(0).IsBroken);
            Assert.AreEqual(0f, inv.GetSlot(0).durability, 0.01f);
        }

        [Test]
        public void UseDurability_OnBrokenItem_ReturnsFalse()
        {
            var rod = MakeItem("rod", durable: true);
            inv.Add(rod, 1);
            inv.UseDurability(0, 200f); // -> broken

            Assert.IsFalse(inv.UseDurability(0));
        }

        [Test]
        public void UseDurability_NonDurableItem_ReturnsFalse()
        {
            var stick = MakeItem("stick", durable: false);
            inv.Add(stick, 1);
            Assert.IsFalse(inv.UseDurability(0));
        }

        [Test]
        public void UseDurability_EmptySlot_ReturnsFalse()
        {
            Assert.IsFalse(inv.UseDurability(0));
        }

        // ===== CountOf / TryConsume skip broken =====

        [Test]
        public void CountOf_ByItem_SkipsBrokenSlots()
        {
            var rod = MakeItem("rod", durable: true);
            inv.Add(rod, 1); // slot 0
            inv.Add(rod, 1); // slot 1 (durable không stack)
            inv.UseDurability(0, 200f); // slot 0 broken

            Assert.AreEqual(1, inv.CountOf(rod));
        }

        [Test]
        public void CountOf_ById_SkipsBrokenSlots()
        {
            var rod = MakeItem("rod", durable: true);
            inv.Add(rod, 1);
            inv.Add(rod, 1);
            inv.UseDurability(0, 200f);

            Assert.AreEqual(1, inv.CountOf("rod"));
        }

        [Test]
        public void TryConsume_SkipsBrokenSlot_ConsumesNonBrokenOnly()
        {
            var rod = MakeItem("rod", durable: true);
            inv.Add(rod, 1);
            inv.Add(rod, 1);
            inv.UseDurability(0, 200f); // slot 0 broken, slot 1 vẫn full

            Assert.IsTrue(inv.TryConsume(rod, 1));
            Assert.IsTrue(inv.GetSlot(0).IsBroken, "broken slot không bị consume");
            // CountOf=0 vì slot 1 đã consume, slot 0 broken bị exclude
            Assert.AreEqual(0, inv.CountOf(rod));
        }

        [Test]
        public void TryConsume_OnlyBrokenAvailable_ReturnsFalseWithoutMutation()
        {
            var rod = MakeItem("rod", durable: true);
            inv.Add(rod, 1);
            inv.UseDurability(0, 200f); // chỉ có 1, đã broken

            Assert.IsFalse(inv.TryConsume(rod, 1));
            Assert.IsTrue(inv.GetSlot(0).IsBroken);
            Assert.IsFalse(inv.GetSlot(0).IsEmpty);
        }

        // ===== TransferSlot =====

        [Test]
        public void TransferSlot_DurableItem_PreservesDurability()
        {
            var rod = MakeItem("rod", durable: true);
            inv.Add(rod, 1);
            inv.UseDurability(0, 30f); // dur 100 -> 70

            var go2 = new GameObject("Inv2");
            var inv2 = go2.AddComponent<Inventory>(); TestHelpers.Boot(inv2);
            int leftover = inv.TransferSlot(0, inv2);
            Assert.AreEqual(0, leftover);
            Assert.IsTrue(inv.GetSlot(0).IsEmpty);

            int dst = -1;
            for (int i = 0; i < inv2.Slots.Count; i++)
                if (inv2.Slots[i].item == rod) { dst = i; break; }
            Assert.GreaterOrEqual(dst, 0, "dst phải có rod sau transfer");
            Assert.AreEqual(70f, inv2.Slots[dst].durability, 0.01f,
                "TransferSlot giữ nguyên durability, KHÔNG reset về maxDurability như Add");

            Object.DestroyImmediate(go2);
        }

        [Test]
        public void TransferSlot_PerishableItem_PreservesFreshness()
        {
            var meat = MakeItem("meat", perishable: true);
            inv.Add(meat, 1);
            inv.GetSlot(0).freshRemaining = 30f; // giả lập đã spoil một phần

            var go2 = new GameObject("Inv2");
            var inv2 = go2.AddComponent<Inventory>(); TestHelpers.Boot(inv2);
            inv.TransferSlot(0, inv2);

            int dst = -1;
            for (int i = 0; i < inv2.Slots.Count; i++)
                if (inv2.Slots[i].item == meat) { dst = i; break; }
            Assert.GreaterOrEqual(dst, 0);
            Assert.AreEqual(30f, inv2.Slots[dst].freshRemaining, 0.01f,
                "TransferSlot giữ freshRemaining, không reset về freshSeconds");

            Object.DestroyImmediate(go2);
        }

        [Test]
        public void TransferSlot_StackableItem_StacksIntoExistingDestSlot()
        {
            var stick = MakeItem("stick", durable: false, maxStack: 99);
            inv.Add(stick, 5);

            var go2 = new GameObject("Inv2");
            var inv2 = go2.AddComponent<Inventory>(); TestHelpers.Boot(inv2);
            inv2.Add(stick, 3);

            inv.TransferSlot(0, inv2);
            Assert.AreEqual(8, inv2.CountOf(stick));
            Assert.AreEqual(0, inv.CountOf(stick));

            Object.DestroyImmediate(go2);
        }

        [Test]
        public void TransferSlot_DurableItem_DoesNotStack()
        {
            var rod = MakeItem("rod", durable: true);
            inv.Add(rod, 1);
            inv.UseDurability(0, 25f); // dur 75

            var go2 = new GameObject("Inv2");
            var inv2 = go2.AddComponent<Inventory>(); TestHelpers.Boot(inv2);
            inv2.Add(rod, 1); // dst đã có 1 rod full

            inv.TransferSlot(0, inv2);

            int countFull = 0, count75 = 0;
            foreach (var s in inv2.Slots)
            {
                if (s.IsEmpty || s.item != rod) continue;
                if (Mathf.Approximately(s.durability, 100f)) countFull++;
                else if (Mathf.Approximately(s.durability, 75f)) count75++;
            }
            Assert.AreEqual(1, countFull, "rod full bền giữ nguyên ở slot riêng");
            Assert.AreEqual(1, count75, "rod 75 bền vào slot riêng (không stack)");

            Object.DestroyImmediate(go2);
        }

        [Test]
        public void TransferSlot_NullDst_ReturnsSourceCountAndDoesNotMutate()
        {
            var stick = MakeItem("stick");
            inv.Add(stick, 5);
            int leftover = inv.TransferSlot(0, null);
            // Contract: return = items còn lại ở src. Không transfer được → toàn bộ còn lại.
            Assert.AreEqual(5, leftover);
            Assert.AreEqual(5, inv.CountOf(stick));
        }

        [Test]
        public void TransferSlot_SameInventory_ReturnsSourceCountAndDoesNotMutate()
        {
            var stick = MakeItem("stick");
            inv.Add(stick, 5);
            int leftover = inv.TransferSlot(0, inv);
            Assert.AreEqual(5, leftover);
            Assert.AreEqual(5, inv.CountOf(stick));
        }

        // ===== SwapSlots (drag & drop reorder) =====

        [Test]
        public void SwapSlots_DifferentItems_SwapsContents()
        {
            var stick = MakeItem("stick");
            var stone = MakeItem("stone");
            inv.Add(stick, 3);          // slot 0
            inv.Add(stone, 2);          // slot 1

            Assert.IsTrue(inv.SwapSlots(0, 1));
            Assert.AreEqual(stone, inv.GetSlot(0).item);
            Assert.AreEqual(2, inv.GetSlot(0).count);
            Assert.AreEqual(stick, inv.GetSlot(1).item);
            Assert.AreEqual(3, inv.GetSlot(1).count);
        }

        [Test]
        public void SwapSlots_EmptyDst_MovesItem()
        {
            var stick = MakeItem("stick");
            inv.Add(stick, 4);           // slot 0
            Assert.IsTrue(inv.SwapSlots(0, 5));
            Assert.IsTrue(inv.GetSlot(0).IsEmpty);
            Assert.AreEqual(stick, inv.GetSlot(5).item);
            Assert.AreEqual(4, inv.GetSlot(5).count);
        }

        [Test]
        public void SwapSlots_SameIndex_ReturnsFalse()
        {
            var stick = MakeItem("stick");
            inv.Add(stick, 3);
            Assert.IsFalse(inv.SwapSlots(0, 0));
            Assert.AreEqual(3, inv.GetSlot(0).count);
        }

        [Test]
        public void SwapSlots_OutOfRange_ReturnsFalse()
        {
            var stick = MakeItem("stick");
            inv.Add(stick, 3);
            Assert.IsFalse(inv.SwapSlots(0, 999));
            Assert.IsFalse(inv.SwapSlots(-1, 0));
            Assert.AreEqual(3, inv.GetSlot(0).count);
        }

        [Test]
        public void SwapSlots_StackableSameItem_MergesIntoDst()
        {
            var stick = MakeItem("stick", maxStack: 10);
            inv.Add(stick, 4);           // slot 0 = 4
            // Simulate 2 non-merged stacks (bình thường Add tự merge; bypass để test merge-trên-swap).
            inv.GetSlot(1).item = stick;
            inv.GetSlot(1).count = 3;

            Assert.IsTrue(inv.SwapSlots(0, 1));
            Assert.AreEqual(stick, inv.GetSlot(1).item);
            Assert.AreEqual(7, inv.GetSlot(1).count, "merged 4+3 vào dst");
            Assert.IsTrue(inv.GetSlot(0).IsEmpty, "src rỗng sau merge hết");
        }

        [Test]
        public void SwapSlots_StackableMergeOverflow_LeavesRemainderInSrc()
        {
            var stick = MakeItem("stick", maxStack: 10);
            inv.GetSlot(0).item = stick; inv.GetSlot(0).count = 6;
            inv.GetSlot(1).item = stick; inv.GetSlot(1).count = 8;

            Assert.IsTrue(inv.SwapSlots(0, 1));
            Assert.AreEqual(10, inv.GetSlot(1).count, "dst lấp đầy maxStack");
            Assert.AreEqual(stick, inv.GetSlot(0).item);
            Assert.AreEqual(4, inv.GetSlot(0).count, "src còn 6+8-10=4");
        }

        [Test]
        public void SwapSlots_PreservesPerishableFreshness()
        {
            var meat = MakeItem("meat", perishable: true);
            inv.Add(meat, 1);             // slot 0
            inv.GetSlot(0).freshRemaining = 42f;

            Assert.IsTrue(inv.SwapSlots(0, 3));
            Assert.AreEqual(42f, inv.GetSlot(3).freshRemaining, 0.001f);
            Assert.IsTrue(inv.GetSlot(0).IsEmpty);
        }

        [Test]
        public void SwapSlots_PreservesDurableDurability()
        {
            var rod = MakeItem("rod", durable: true);
            inv.Add(rod, 1);
            inv.UseDurability(0, 35f);    // dur 65
            Assert.IsTrue(inv.SwapSlots(0, 7));
            Assert.AreEqual(65f, inv.GetSlot(7).durability, 0.001f);
            Assert.IsTrue(inv.GetSlot(0).IsEmpty);
        }

        [Test]
        public void SwapSlots_PerishableSameItem_DoesNotMerge()
        {
            var meat = MakeItem("meat", perishable: true);
            inv.GetSlot(0).item = meat; inv.GetSlot(0).count = 1; inv.GetSlot(0).freshRemaining = 50f;
            inv.GetSlot(1).item = meat; inv.GetSlot(1).count = 1; inv.GetSlot(1).freshRemaining = 20f;

            // Không merge vì perishable → swap thuần
            Assert.IsTrue(inv.SwapSlots(0, 1));
            Assert.AreEqual(20f, inv.GetSlot(0).freshRemaining, 0.001f);
            Assert.AreEqual(50f, inv.GetSlot(1).freshRemaining, 0.001f);
        }
    }
}
