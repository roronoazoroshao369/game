using System;
using WildernessCultivation.Core;
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
    public class Inventory : MonoBehaviour, ISaveable
    {
        [Header("Config")]
        public int slotCount = 16;

        [Tooltip("ItemDatabase để resolve ItemSO khi load save. R6: move từ SaveLoadController.itemDatabase " +
            "để Inventory tự implement ISaveable.RestoreState (dispatcher pattern).")]
        public ItemDatabase itemDatabase;

        [SerializeField] List<InventorySlot> slots = new();

        public event Action OnInventoryChanged;
        public IReadOnlyList<InventorySlot> Slots => slots;

        // ===== R6 ISaveable =====

        public string SaveKey => "Player/Inventory";
        public int Order => 60; // Sau PlayerStats (30). Không cần fixup.

        public void CaptureState(SaveData data)
        {
            if (data == null) return;
            data.inventory ??= new List<InventorySlotData>();
            data.inventory.Clear();
            foreach (var s in slots)
            {
                if (s.IsEmpty) continue;
                data.inventory.Add(new InventorySlotData
                {
                    itemId = s.item.itemId,
                    count = s.count,
                    freshRemaining = s.IsPerishable ? s.freshRemaining : -1f,
                    durability = s.IsDurable ? s.durability : -1f,
                });
            }
        }

        public void RestoreState(SaveData data)
        {
            if (data == null || data.inventory == null) return;
            if (itemDatabase == null)
            {
                Debug.LogWarning("[Save] ItemDatabase chưa được gán → không thể restore inventory.");
                return;
            }

            // Xóa toàn bộ slot rồi nạp lại
            for (int i = 0; i < slots.Count; i++)
                TryConsumeSlot(i, slots[i].count);

            // Snapshot count per slot trước mỗi Add để định vị slot vừa được Add ghi vào
            // (perishable/durable không stack → mỗi entry tạo 1 slot mới). Regression
            // test: SaveLoadControllerTests.RoundTrip_MultiplePerishableStacks_PreservePerSlotFreshness.
            int slotCount = slots.Count;
            var preCounts = new int[slotCount];

            foreach (var s in data.inventory)
            {
                if (string.IsNullOrEmpty(s.itemId) || s.count <= 0) continue;
                var item = itemDatabase.GetById(s.itemId);
                if (item == null)
                {
                    Debug.LogWarning($"[Save] ItemDatabase không có itemId='{s.itemId}', bỏ qua.");
                    continue;
                }

                for (int i = 0; i < slotCount; i++) preCounts[i] = slots[i].count;
                int leftover = Add(item, s.count);
                if (leftover > 0)
                    Debug.LogWarning($"[Save] Inventory đầy khi restore {s.itemId}, {leftover} item bị mất.");

                if (s.freshRemaining >= 0f || s.durability >= 0f)
                {
                    for (int i = 0; i < slotCount; i++)
                    {
                        var slot = slots[i];
                        if (slot.item == item && slot.count > preCounts[i])
                        {
                            if (s.freshRemaining >= 0f && slot.IsPerishable) slot.freshRemaining = s.freshRemaining;
                            if (s.durability >= 0f && slot.IsDurable) slot.durability = s.durability;
                            break;
                        }
                    }
                }
            }
        }

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
            ServiceLocator.Register<Inventory>(this);
        }

        void OnDestroy() => ServiceLocator.Unregister<Inventory>(this);

        void OnEnable()
        {
            SaveRegistry.RegisterSaveable(this);
        }

        void OnDisable()
        {
            SaveRegistry.UnregisterSaveable(this);
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

        // CountOf / TryConsume bỏ qua slot đã hỏng (IsBroken) — đồ vỡ vẫn nằm trong
        // inventory để đem đến Workbench sửa, không được tính như tài nguyên khả dụng cho
        // recipe / consume khác.
        public int CountOf(ItemSO item)
        {
            if (item == null) return 0;
            int total = 0;
            foreach (var s in slots) if (s.item == item && !s.IsBroken) total += s.count;
            return total;
        }

        public int CountOf(string itemId)
        {
            int total = 0;
            foreach (var s in slots)
                if (s.item != null && s.item.itemId == itemId && !s.IsBroken) total += s.count;
            return total;
        }

        /// <summary>
        /// Tiêu thụ count item; trả về true nếu đủ. Bỏ qua slot <see cref="InventorySlot.IsBroken"/>
        /// (đồ vỡ chỉ sửa được qua Workbench, không bị recipe khuất đi).
        /// </summary>
        public bool TryConsume(ItemSO item, int count)
        {
            if (CountOf(item) < count) return false;
            int remaining = count;
            foreach (var s in slots)
            {
                if (remaining <= 0) break;
                if (s.item == item && !s.IsBroken)
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

        /// <summary>
        /// Hao mòn 1 lần dùng tool/weapon ở slot. Khi durability cạn → slot vẫn giữ
        /// item nhưng <see cref="InventorySlot.IsBroken"/> = true (không còn dùng được
        /// nhưng có thể đem đến Workbench để sửa).
        /// </summary>
        public bool UseDurability(int slotIndex, float amountOverride = -1f)
        {
            if (slotIndex < 0 || slotIndex >= slots.Count) return false;
            var s = slots[slotIndex];
            if (!s.IsDurable || s.IsEmpty || s.IsBroken) return false;
            float amount = amountOverride > 0f ? amountOverride : s.item.durabilityPerUse;
            s.durability = Mathf.Max(0f, s.durability - amount);
            if (s.durability <= 0f)
                Debug.Log($"[Inventory] {s.item.displayName} bị hỏng — cần sửa ở Workbench.");
            OnInventoryChanged?.Invoke();
            return true;
        }

        /// <summary>
        /// Sửa đồ ở slot — cộng <paramref name="amount"/> vào durability, clamp về maxDurability
        /// (nếu amount &lt;= 0 → sửa full về maxDurability). Trả về true nếu có gì để sửa.
        /// </summary>
        public bool Repair(int slotIndex, float amount = -1f)
        {
            if (slotIndex < 0 || slotIndex >= slots.Count) return false;
            var s = slots[slotIndex];
            if (s.IsEmpty || !s.IsDurable) return false;
            if (s.durability >= s.item.maxDurability) return false;
            float add = amount > 0f ? amount : s.item.maxDurability;
            s.durability = Mathf.Min(s.item.maxDurability, s.durability + add);
            OnInventoryChanged?.Invoke();
            return true;
        }

        /// <summary>Tìm slot đầu tiên cần sửa (durable + durability &lt; max). Trả -1 nếu không có.</summary>
        public int FindFirstDamagedSlot()
        {
            for (int i = 0; i < slots.Count; i++)
            {
                var s = slots[i];
                if (s.IsEmpty || !s.IsDurable) continue;
                if (s.durability < s.item.maxDurability) return i;
            }
            return -1;
        }

        public InventorySlot GetSlot(int i) => (i >= 0 && i < slots.Count) ? slots[i] : null;

        /// <summary>
        /// Chuyển toàn bộ nội dung 1 slot sang <paramref name="dst"/> giữ nguyên
        /// freshRemaining / durability (khác <see cref="Add"/> — Add reset 2 trường này).
        /// Trả về số lượng còn lại trong slot nguồn (=0 nếu chuyển hết, &gt;0 nếu dst đầy).
        /// </summary>
        public int TransferSlot(int slotIndex, Inventory dst)
        {
            var src = GetSlot(slotIndex);
            if (src == null || src.IsEmpty) return 0;
            // No-op: dst không hợp lệ → trả về src.count để caller biết KHÔNG transfer
            // được gì (contract: return value = items còn lại ở src; 0 = chuyển hết).
            if (dst == null || dst == this) return src.count;

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

        /// <summary>
        /// Hoán đổi nội dung 2 slot trong cùng 1 inventory (cho drag &amp; drop reorder).
        /// Giữ nguyên freshRemaining / durability theo slot. Nếu 2 slot cùng <see cref="ItemSO"/>
        /// và không phải perishable/durable → cố gắng merge vào slot đích (stack tối đa maxStack),
        /// phần dư ở lại slot nguồn. Trả về true nếu có thay đổi.
        /// </summary>
        public bool SwapSlots(int a, int b)
        {
            if (a == b) return false;
            if (a < 0 || a >= slots.Count) return false;
            if (b < 0 || b >= slots.Count) return false;

            var sa = slots[a];
            var sb = slots[b];

            // Merge 2 stack cùng item thường (không perishable / durable vì mỗi slot có tracking riêng).
            if (!sa.IsEmpty && !sb.IsEmpty
                && sa.item == sb.item
                && !sa.item.isPerishable && !sa.item.hasDurability
                && sb.count < sa.item.maxStack)
            {
                int room = sa.item.maxStack - sb.count;
                int move = Mathf.Min(room, sa.count);
                sb.count += move;
                sa.count -= move;
                if (sa.count <= 0) ResetSlot(sa);
                OnInventoryChanged?.Invoke();
                return true;
            }

            // Swap field-by-field (giữ ref của slot để không phá list order).
            var tmpItem = sa.item;
            var tmpCount = sa.count;
            var tmpFresh = sa.freshRemaining;
            var tmpDur = sa.durability;

            sa.item = sb.item;
            sa.count = sb.count;
            sa.freshRemaining = sb.freshRemaining;
            sa.durability = sb.durability;

            sb.item = tmpItem;
            sb.count = tmpCount;
            sb.freshRemaining = tmpFresh;
            sb.durability = tmpDur;

            OnInventoryChanged?.Invoke();
            return true;
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
