using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WildernessCultivation.Items;
using WildernessCultivation.Player;

namespace WildernessCultivation.UI
{
    /// <summary>
    /// Grid hiển thị inventory. Tap slot → consume (ăn/uống) nếu là food/drink/consumable.
    /// </summary>
    public class InventoryUI : MonoBehaviour
    {
        public Inventory inventory;
        public PlayerStats playerStats;

        [Header("Slot prefab + container")]
        public GameObject slotPrefab;
        public Transform slotsParent;

        readonly List<InventorySlotUI> spawned = new();

        void Start()
        {
            if (inventory == null) inventory = FindObjectOfType<Inventory>();
            if (playerStats == null) playerStats = FindObjectOfType<PlayerStats>();
            if (inventory == null) return;

            for (int i = 0; i < inventory.Slots.Count; i++)
            {
                var go = Instantiate(slotPrefab, slotsParent);
                var ui = go.GetComponent<InventorySlotUI>();
                ui.slotIndex = i;
                ui.onClick = OnSlotClicked;
                spawned.Add(ui);
            }
            inventory.OnInventoryChanged += Refresh;
            Refresh();
        }

        void OnDestroy()
        {
            if (inventory != null) inventory.OnInventoryChanged -= Refresh;
        }

        void Refresh()
        {
            for (int i = 0; i < spawned.Count; i++)
            {
                var slot = inventory.Slots[i];
                spawned[i].Bind(slot);
            }
        }

        void OnSlotClicked(int index)
        {
            var slot = inventory.Slots[index];
            if (slot.IsEmpty || playerStats == null) return;
            var item = slot.item;

            switch (item.category)
            {
                case ItemCategory.Food:
                case ItemCategory.Drink:
                case ItemCategory.Consumable:
                    if (item.restoreHunger > 0) playerStats.Eat(item.restoreHunger);
                    if (item.restoreThirst > 0) playerStats.Drink(item.restoreThirst);
                    if (item.restoreHP > 0) playerStats.Heal(item.restoreHP);
                    if (item.restoreSanity > 0) playerStats.RestoreSanity(item.restoreSanity);
                    if (item.restoreMana > 0) playerStats.AddMana(item.restoreMana);
                    inventory.TryConsumeSlot(index, 1);
                    break;
            }
        }
    }
}
