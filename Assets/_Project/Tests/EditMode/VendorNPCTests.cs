using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using WildernessCultivation.Core;
using WildernessCultivation.Items;
using WildernessCultivation.World;

namespace WildernessCultivation.Tests.EditMode
{
    /// <summary>
    /// R5 follow-up: exemplar NPC humanoid — VendorNPC. Verify:
    /// <list type="bullet">
    /// <item>Composition reuse (HealthComponent + InvulnerabilityComponent auto-add)</item>
    /// <item>CharacterBase polymorphic view (CurrentHP/CurrentMaxHP/IsDead)</item>
    /// <item>IInteractable contract (CanInteract + Interact fires OnVendorOpened)</item>
    /// <item>TryExecuteTrade atomic invariants (stock, player inventory, rollback on full)</item>
    /// <item>ISaveable round-trip (stock persist across save/restore)</item>
    /// <item>GameEvents hub (OnVendorOpened + OnTradeCompleted)</item>
    /// <item>Invulnerability respected trong TakeDamage</item>
    /// </list>
    /// </summary>
    public class VendorNPCTests
    {
        GameObject vendorGo;
        GameObject playerGo;
        VendorNPC vendor;
        Inventory playerInv;
        ItemSO apple, herb, potion;

        [SetUp]
        public void Setup()
        {
            GameEvents.ClearAllSubscribers();
            SaveRegistry.ClearAll();

            apple = MakeItem("apple", maxStack: 99);
            herb = MakeItem("herb", maxStack: 99);
            potion = MakeItem("potion", maxStack: 99);

            vendorGo = new GameObject("Vendor");
            vendor = vendorGo.AddComponent<VendorNPC>();
            vendor.vendorId = "vendor_test";
            vendor.displayName = "Test Merchant";
            vendor.invulnerable = true;
            vendor.maxHP = 50f;
            vendor.offers = new List<TradeOffer>
            {
                // 3 apple -> 1 potion, stock 5
                new TradeOffer { receiveItem = apple, receiveCount = 3, giveItem = potion, giveCount = 1, stock = 5 },
                // 2 herb -> 1 apple, stock infinite
                new TradeOffer { receiveItem = herb, receiveCount = 2, giveItem = apple, giveCount = 1, stock = -1 },
            };
            TestHelpers.Boot(vendor);

            playerGo = new GameObject("Player");
            playerInv = playerGo.AddComponent<Inventory>();
            TestHelpers.Boot(playerInv);
        }

        [TearDown]
        public void Teardown()
        {
            GameEvents.ClearAllSubscribers();
            SaveRegistry.ClearAll();
            SaveSystem.Delete();
            if (vendorGo != null) Object.DestroyImmediate(vendorGo);
            if (playerGo != null) Object.DestroyImmediate(playerGo);
            if (apple != null) Object.DestroyImmediate(apple);
            if (herb != null) Object.DestroyImmediate(herb);
            if (potion != null) Object.DestroyImmediate(potion);
        }

        static ItemSO MakeItem(string id, int maxStack = 99)
        {
            var so = ScriptableObject.CreateInstance<ItemSO>();
            so.itemId = id;
            so.displayName = id;
            so.maxStack = maxStack;
            return so;
        }

        // ===== Composition + CharacterBase =====

        [Test]
        public void Awake_AutoAdds_HealthAndInvulnerabilityComponents()
        {
            Assert.IsNotNull(vendorGo.GetComponent<Player.Stats.HealthComponent>(),
                "HealthComponent phải được auto-add");
            Assert.IsNotNull(vendorGo.GetComponent<Player.Stats.InvulnerabilityComponent>(),
                "InvulnerabilityComponent phải được auto-add");
        }

        [Test]
        public void CharacterBase_View_ReflectsHealthComponent()
        {
            Assert.AreEqual(50f, vendor.CurrentMaxHP);
            Assert.AreEqual(50f, vendor.CurrentHP);
            Assert.IsFalse(vendor.IsDead);
            Assert.IsTrue(vendor.IsAlive);
        }

        [Test]
        public void TakeDamage_Invulnerable_DoesNotReduceHP()
        {
            vendor.TakeDamage(20f, null);
            Assert.AreEqual(50f, vendor.CurrentHP, "Invulnerable vendor giữ HP nguyên");
        }

        [Test]
        public void TakeDamage_Vulnerable_ReducesHP()
        {
            // Force vulnerable: set invuln expiry vào quá khứ.
            var invuln = vendorGo.GetComponent<Player.Stats.InvulnerabilityComponent>();
            invuln.InvulnerableUntil = 0f;
            vendor.TakeDamage(20f, null);
            Assert.AreEqual(30f, vendor.CurrentHP, 0.01f);
        }

        // ===== IInteractable =====

        [Test]
        public void CanInteract_NullActor_ReturnsFalse()
        {
            Assert.IsFalse(vendor.CanInteract(null));
            Assert.IsTrue(vendor.CanInteract(playerGo));
        }

        [Test]
        public void Interact_RaisesOnVendorOpenedEvent()
        {
            object received = null;
            GameEvents.OnVendorOpened += v => received = v;

            bool ok = vendor.Interact(playerGo);

            Assert.IsTrue(ok);
            Assert.AreSame(vendor, received, "OnVendorOpened phải pass chính vendor instance");
        }

        // ===== TryExecuteTrade =====

        [Test]
        public void TryExecuteTrade_Success_ConsumesReceiveAndAddsGive()
        {
            playerInv.Add(apple, 3);
            Assert.IsTrue(vendor.TryExecuteTrade(0, playerInv));
            Assert.AreEqual(0, playerInv.CountOf(apple), "3 apple đã bị consume");
            Assert.AreEqual(1, playerInv.CountOf(potion), "1 potion đã được add");
            Assert.AreEqual(4, vendor.offers[0].stock, "Stock giảm 5 -> 4");
        }

        [Test]
        public void TryExecuteTrade_InfiniteStock_DoesNotDecrement()
        {
            playerInv.Add(herb, 2);
            Assert.IsTrue(vendor.TryExecuteTrade(1, playerInv));
            Assert.AreEqual(-1, vendor.offers[1].stock, "Infinite stock giữ -1");
        }

        [Test]
        public void TryExecuteTrade_InsufficientPlayerItems_Fails()
        {
            playerInv.Add(apple, 2); // need 3
            Assert.IsFalse(vendor.TryExecuteTrade(0, playerInv));
            Assert.AreEqual(2, playerInv.CountOf(apple), "Rollback: apple không bị consume");
            Assert.AreEqual(0, playerInv.CountOf(potion), "Không nhận potion");
            Assert.AreEqual(5, vendor.offers[0].stock, "Stock không đổi");
        }

        [Test]
        public void TryExecuteTrade_StockDepleted_Fails()
        {
            vendor.offers[0].stock = 0;
            playerInv.Add(apple, 3);
            Assert.IsFalse(vendor.TryExecuteTrade(0, playerInv));
            Assert.AreEqual(3, playerInv.CountOf(apple), "Apple không bị consume khi hết stock");
        }

        [Test]
        public void TryExecuteTrade_InvalidIndex_Fails()
        {
            playerInv.Add(apple, 3);
            Assert.IsFalse(vendor.TryExecuteTrade(-1, playerInv));
            Assert.IsFalse(vendor.TryExecuteTrade(99, playerInv));
        }

        [Test]
        public void TryExecuteTrade_NullInventory_Fails()
        {
            Assert.IsFalse(vendor.TryExecuteTrade(0, null));
        }

        [Test]
        public void TryExecuteTrade_RaisesOnTradeCompletedEvent()
        {
            object receivedVendor = null;
            int receivedIndex = -1;
            GameEvents.OnTradeCompleted += (v, i) => { receivedVendor = v; receivedIndex = i; };

            playerInv.Add(apple, 3);
            vendor.TryExecuteTrade(0, playerInv);

            Assert.AreSame(vendor, receivedVendor);
            Assert.AreEqual(0, receivedIndex);
        }

        [Test]
        public void TryExecuteTrade_StockExhaustAcrossMultipleCalls()
        {
            vendor.offers[0].stock = 2;
            playerInv.Add(apple, 9);
            Assert.IsTrue(vendor.TryExecuteTrade(0, playerInv)); // 2 -> 1
            Assert.IsTrue(vendor.TryExecuteTrade(0, playerInv)); // 1 -> 0
            Assert.IsFalse(vendor.TryExecuteTrade(0, playerInv), "Hết stock, trade thất bại");
            Assert.AreEqual(3, playerInv.CountOf(apple), "Còn 9 - 3 - 3 = 3 apple");
            Assert.AreEqual(2, playerInv.CountOf(potion));
            Assert.AreEqual(0, vendor.offers[0].stock);
        }

        // ===== ISaveable round-trip =====

        [Test]
        public void ISaveable_CaptureAndRestore_PersistsStock()
        {
            vendor.offers[0].stock = 2;
            vendor.offers[1].stock = -1;

            var data = new SaveData();
            vendor.CaptureState(data);
            Assert.AreEqual(1, data.vendors.Count);
            Assert.AreEqual("vendor_test", data.vendors[0].vendorId);
            CollectionAssert.AreEqual(new[] { 2, -1 }, data.vendors[0].stocks);

            // Reset stock xong restore.
            vendor.offers[0].stock = 0;
            vendor.offers[1].stock = 0;
            vendor.RestoreState(data);
            Assert.AreEqual(2, vendor.offers[0].stock);
            Assert.AreEqual(-1, vendor.offers[1].stock);
        }

        [Test]
        public void ISaveable_RestoreState_Null_EarlyReturns()
        {
            Assert.DoesNotThrow(() => vendor.RestoreState(null));
            Assert.DoesNotThrow(() => vendor.RestoreState(new SaveData { vendors = null }));
        }

        [Test]
        public void ISaveable_RestoreState_SaveWithoutThisVendor_NoOp()
        {
            vendor.offers[0].stock = 1;
            var data = new SaveData();
            data.vendors.Add(new VendorSaveData { vendorId = "other_vendor", stocks = { 99 } });
            vendor.RestoreState(data);
            Assert.AreEqual(1, vendor.offers[0].stock, "Save không có vendor này → stock giữ nguyên");
        }

        [Test]
        public void ISaveable_OnEnable_RegistersWithSaveRegistry()
        {
            bool found = false;
            foreach (var s in SaveRegistry.OrderedSaveables())
                if (ReferenceEquals(s, vendor)) { found = true; break; }
            Assert.IsTrue(found, "VendorNPC phải tự register trong OnEnable");
        }
    }
}
