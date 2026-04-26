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
        [Tooltip("Giây tươi còn lại (chỉ dùng khi item.isPerishable). <=0 = đã hỏng.")]
        public float freshRemaining = -1f;
        [Tooltip("Độ bền còn lại (chỉ dùng khi item.hasDurability). <=0 = hỏng.")]
        public float durability = -1f;
        public bool IsEmpty => item == null || count <= 0;
        public bool IsPerishable => item != null && item.isPerishable;
        public bool IsDurable => item != null && item.hasDurability;
        public bool IsSpoiled => IsPerishable && freshRemaining <= 0f;
        public bool IsBroken => IsDurable && durability <= 0f;
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

        /// <summary>Tổng khối lượng inventory (sum item.weight * count). Dùng cho encumbrance.</summary>
        public float TotalWeight
        {
            get
            {
                float w = 0f;
                foreach (var s in slots)
                    if (!s.IsEmpty && s.item != null) w += s.item.weight * s.count;
                return w;
            }
        }

        void Awake()
        {
            while (slots.Count < slotCount) slots.Add(new InventorySlot());
        }

        /// <summary>Thêm item; trả về số lượng KHÔNG add được (=0 nếu đầy đủ).</summary>
        public int Add(ItemSO item, int count)
        {
            if (item == null || count <= 0) return count;
            int remaining = count;

            // Perishable / durable không nên stack (để giữ tracking riêng cho mỗi slot)
            bool noStack = item.isPerishable || item.hasDurability;
            int effectiveStack = noStack ? 1 : item.maxStack;

            if (!noStack)
            {
                // Stack vào slot đã có (chỉ cho item thường)
                foreach (var s in slots)
                {
                    if (remaining <= 0) break;
                    if (s.item == item && s.count < effectiveStack)
                    {
                        int can = Mathf.Min(effectiveStack - s.count, remaining);
                        s.count += can;
                        remaining -= can;
                    }
                }
            }

            // Slot mới
            foreach (var s in slots)
            {
                if (remaining <= 0) break;
                if (s.IsEmpty)
                {
                    s.item = item;
                    int can = Mathf.Min(effectiveStack, remaining);
                    s.count = can;
                    s.freshRemaining = item.isPerishable ? item.freshSeconds : -1f;
                    s.durability = item.hasDurability ? item.maxDurability : -1f;
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
            if (s.count <= 0) ResetSlot(s);
            OnInventoryChanged?.Invoke();
            return true;
        }

        /// <summary>Tick freshness mỗi frame; flag spoiled khi freshRemaining về 0.</summary>
        void Update()
        {
            float dt = Time.deltaTime;
            bool anyChanged = false;
            foreach (var s in slots)
            {
                if (!s.IsPerishable || s.IsEmpty) continue;
                if (s.freshRemaining <= 0f) continue; // đã spoiled, không tick nữa
                s.freshRemaining = Mathf.Max(0f, s.freshRemaining - dt);
                if (s.freshRemaining == 0f) anyChanged = true;
            }
            if (anyChanged) OnInventoryChanged?.Invoke();
        }

        /// <summary>Hao mòn 1 lần dùng tool/weapon ở slot. Khi durability cạn → slot bị xoá (đồ vỡ).</summary>
        public bool UseDurability(int slotIndex, float amountOverride = -1f)
        {
            if (slotIndex < 0 || slotIndex >= slots.Count) return false;
            var s = slots[slotIndex];
            if (!s.IsDurable || s.IsEmpty || s.IsBroken) return false;
            float amount = amountOverride > 0f ? amountOverride : s.item.durabilityPerUse;
            s.durability = Mathf.Max(0f, s.durability - amount);
            if (s.durability <= 0f)
            {
                Debug.Log($"[Inventory] {s.item.displayName} bị hỏng.");
                ResetSlot(s);
            }
            OnInventoryChanged?.Invoke();
            return true;
        }

        public InventorySlot GetSlot(int i) => (i >= 0 && i < slots.Count) ? slots[i] : null;

        /// <summary>
        /// Chuyển toàn bộ nội dung 1 slot sang <paramref name="dst"/> giữ nguyên
        /// freshRemaining / durability (khác <see cref="Add"/> — Add reset 2 trường này).
        /// Trả về số lượng còn lại trong slot nguồn (=0 nếu chuyển hết, &gt;0 nếu dst đầy).
        /// </summary>
        public int TransferSlot(int slotIndex, Inventory dst)
        {
            if (dst == null || dst == this) return 0;
            var src = GetSlot(slotIndex);
            if (src == null || src.IsEmpty) return 0;

            var item = src.item;
            bool noStack = item.isPerishable || item.hasDurability;
            int effectiveStack = noStack ? 1 : item.maxStack;

            // Stack vào slot đã có (chỉ cho item thường, không phải perishable/durable —
            // vì 2 loại này có tracking per-slot riêng, stack lại sẽ làm mất dữ liệu).
            if (!noStack)
            {
                foreach (var d in dst.slots)
                {
                    if (src.count <= 0) break;
                    if (d.item == item && d.count < effectiveStack)
                    {
                        int can = Mathf.Min(effectiveStack - d.count, src.count);
                        d.count += can;
                        src.count -= can;
                    }
                }
            }

            // Slot trống trong dst
            foreach (var d in dst.slots)
            {
                if (src.count <= 0) break;
                if (d.IsEmpty)
                {
                    d.item = item;
                    int can = noStack ? 1 : Mathf.Min(effectiveStack, src.count);
                    d.count = can;
                    d.freshRemaining = src.freshRemaining;
                    d.durability = src.durability;
                    src.count -= can;
                }
            }

            int leftover = src.count;
            if (leftover <= 0) ResetSlot(src);

            OnInventoryChanged?.Invoke();
            dst.OnInventoryChanged?.Invoke();
            return leftover;
        }

        void ResetSlot(InventorySlot s)
        {
            s.item = null;
            s.count = 0;
            s.freshRemaining = -1f;
            s.durability = -1f;
        }
    }
}
