using System;
using System.Collections.Generic;

namespace WildernessCultivation.Core
{
    /// <summary>
    /// Contract cho mỗi subsystem tự serialize slice riêng vào <see cref="SaveData"/>.
    /// Thay cho việc <see cref="SaveLoadController"/> biết chi tiết schema từng hệ thống.
    ///
    /// <para>Implementer MUST register với <see cref="SaveRegistry"/> trong
    /// <c>OnEnable</c> và unregister trong <c>OnDisable</c> — mirror pattern
    /// R4 GameEvents (AGENTS rule 7).</para>
    ///
    /// <para>Thứ tự restore: <see cref="Order"/> thấp trước. Ví dụ:
    /// RealmSystem(10) sets tier trước → SpiritRoot(20) lookup catalog theo name
    /// → PlayerStats(30) set HP (chưa clamp) → Inventory(60) add items.
    /// Cross-system fixup chạy sau Restore phase qua <see cref="SaveRegistry.RegisterFixup"/>.</para>
    /// </summary>
    public interface ISaveable
    {
        /// <summary>Tên logic dùng cho log / diagnostic. Ví dụ "Player/Vitals".</summary>
        string SaveKey { get; }

        /// <summary>Thứ tự cả Capture lẫn Restore. Thấp → trước.</summary>
        int Order { get; }

        /// <summary>Ghi state vào <paramref name="data"/> (được gọi trong <see cref="SaveLoadController.Save"/>).</summary>
        void CaptureState(SaveData data);

        /// <summary>Đọc state từ <paramref name="data"/> (được gọi trong <see cref="SaveLoadController.LoadAndApply"/>, sau khi TryLoad).</summary>
        void RestoreState(SaveData data);
    }

    /// <summary>
    /// Static registry các <see cref="ISaveable"/> + cross-system fixup actions.
    /// Dispatcher (SaveLoadController) đọc registry thay vì hard-coupling từng component.
    /// </summary>
    public static class SaveRegistry
    {
        static readonly List<ISaveable> saveables = new();
        static readonly List<FixupEntry> fixups = new();

        struct FixupEntry
        {
            public object Owner;
            public int Order;
            public Action<SaveData> Action;
        }

        public static void RegisterSaveable(ISaveable s)
        {
            if (s == null || saveables.Contains(s)) return;
            saveables.Add(s);
        }

        public static void UnregisterSaveable(ISaveable s)
        {
            if (s == null) return;
            saveables.Remove(s);
        }

        /// <summary>Đăng ký post-restore fixup. <paramref name="owner"/> cần thiết để
        /// <see cref="UnregisterFixupsFor"/> khi component disable.
        /// Dùng cho cross-system ordering (vd PlayerStats.ReapplySpiritRootMaxHP sau khi
        /// SpiritRoot đã SetSpiritRoot, trước RealmSystem.ReapplyAccumulatedBonuses).</summary>
        public static void RegisterFixup(object owner, int order, Action<SaveData> action)
        {
            if (owner == null || action == null) return;
            fixups.Add(new FixupEntry { Owner = owner, Order = order, Action = action });
        }

        /// <summary>Gỡ hết fixup đã đăng ký bởi <paramref name="owner"/>. Gọi trong OnDisable.</summary>
        public static void UnregisterFixupsFor(object owner)
        {
            if (owner == null) return;
            fixups.RemoveAll(f => ReferenceEquals(f.Owner, owner));
        }

        /// <summary>Snapshot copy đã sort theo Order — an toàn khi foreach mà registry đổi giữa chừng.</summary>
        public static IReadOnlyList<ISaveable> OrderedSaveables()
        {
            saveables.Sort((a, b) => a.Order.CompareTo(b.Order));
            return saveables.ToArray();
        }

        /// <summary>Snapshot copy các fixup action đã sort theo order.</summary>
        public static IReadOnlyList<Action<SaveData>> OrderedFixupActions()
        {
            fixups.Sort((a, b) => a.Order.CompareTo(b.Order));
            var arr = new Action<SaveData>[fixups.Count];
            for (int i = 0; i < fixups.Count; i++) arr[i] = fixups[i].Action;
            return arr;
        }

        /// <summary>Clear toàn bộ — CHỈ dùng trong test SetUp/TearDown (tránh leak giữa test).</summary>
        public static void ClearAll()
        {
            saveables.Clear();
            fixups.Clear();
        }
    }
}
