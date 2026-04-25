using System;
using System.Collections.Generic;
using UnityEngine;

namespace WildernessCultivation.Items
{
    [Serializable]
    public class InventorySlot
    {
        public ItemSO item;
        public int count;
        public bool IsEmpty => item == null || count <= 0;
    }

    /// <summary>
    /// Inventory cố định N slot. Stack theo maxStack của item. Hỗ trợ Add / Remove / TryConsume.
    /// </summary>
    public class Inventory : MonoBehaviour
    {
        [Header("Config")]
        public int slotCount = 16;

        [SerializeField] List<InventorySlot> slots = new();

        public event Action OnInventoryChanged;
        public IReadOnlyList<InventorySlot> Slots => slots;

        void Awake()
        {
            while (slots.Count < slotCount) slots.Add(new InventorySlot());
        }

        /// <summary>Thêm item; trả về số lượng KHÔNG add được (=0 nếu đầy đủ).</summary>
        public int Add(ItemSO item, int count)
        {
            if (item == null || count <= 0) return count;
            int remaining = count;

            // Stack vào slot đã có
            foreach (var s in slots)
            {
                if (remaining <= 0) break;
                if (s.item == item && s.count < item.maxStack)
                {
                    int can = Mathf.Min(item.maxStack - s.count, remaining);
                    s.count += can;
                    remaining -= can;
                }
            }

            // Slot mới
            foreach (var s in slots)
            {
                if (remaining <= 0) break;
                if (s.IsEmpty)
                {
                    s.item = item;
                    int can = Mathf.Min(item.maxStack, remaining);
                    s.count = can;
                    remaining -= can;
                }
            }

            OnInventoryChanged?.Invoke();
            return remaining;
        }

        public int CountOf(ItemSO item)
        {
            if (item == null) return 0;
            int total = 0;
            foreach (var s in slots) if (s.item == item) total += s.count;
            return total;
        }

        public int CountOf(string itemId)
        {
            int total = 0;
            foreach (var s in slots) if (s.item != null && s.item.itemId == itemId) total += s.count;
            return total;
        }

        /// <summary>Tiêu thụ count item; trả về true nếu đủ.</summary>
        public bool TryConsume(ItemSO item, int count)
        {
            if (CountOf(item) < count) return false;
            int remaining = count;
            foreach (var s in slots)
            {
                if (remaining <= 0) break;
                if (s.item == item)
                {
                    int take = Mathf.Min(s.count, remaining);
                    s.count -= take;
                    remaining -= take;
                    if (s.count <= 0) { s.item = null; s.count = 0; }
                }
            }
            OnInventoryChanged?.Invoke();
            return true;
        }

        public bool TryConsumeSlot(int slotIndex, int count = 1)
        {
            if (slotIndex < 0 || slotIndex >= slots.Count) return false;
            var s = slots[slotIndex];
            if (s.IsEmpty || s.count < count) return false;
            s.count -= count;
            if (s.count <= 0) { s.item = null; s.count = 0; }
            OnInventoryChanged?.Invoke();
            return true;
        }
    }
}
