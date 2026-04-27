using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace WildernessCultivation.Core
{
    /// <summary>
    /// Persistent kho mộ phần (tombstone) độc lập với <see cref="SaveSystem"/>.
    ///
    /// Lý do tách file: permadeath logic xoá save_slot_0.json mỗi lần chết, nhưng
    /// tombstone phải sống xuyên các đời để run sau pickup được. Lưu ở
    /// <c>Application.persistentDataPath/graveyard.json</c>; <see cref="SaveSystem.Delete"/>
    /// KHÔNG đụng tới file này.
    ///
    /// Cap entries (FIFO) tránh file phình + clutter world.
    /// </summary>
    public static class Graveyard
    {
        const string FileName = "graveyard.json";
        public const int MaxEntries = 10;

        static string Path => System.IO.Path.Combine(Application.persistentDataPath, FileName);

        /// <summary>Read-only snapshot mới nhất từ disk. Trả về list rỗng nếu file chưa tồn tại.</summary>
        public static GraveyardData Load()
        {
            if (!File.Exists(Path)) return new GraveyardData();
            try
            {
                string json = File.ReadAllText(Path);
                var data = JsonUtility.FromJson<GraveyardData>(json);
                return data ?? new GraveyardData();
            }
            catch (Exception e)
            {
                Debug.LogError($"[Graveyard] Load failed: {e}");
                return new GraveyardData();
            }
        }

        public static void Save(GraveyardData data)
        {
            if (data == null) data = new GraveyardData();
            // FIFO cap — drop tombstone cũ nhất nếu vượt MaxEntries.
            while (data.tombstones.Count > MaxEntries)
                data.tombstones.RemoveAt(0);
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(Path, json);
            Debug.Log($"[Graveyard] Saved {data.tombstones.Count} tombstone(s) to {Path}");
        }

        /// <summary>Append 1 tombstone. Snapshot list, append, save.
        /// Returns the assigned id của tombstone mới.</summary>
        public static string Append(TombstoneData entry)
        {
            if (entry == null) throw new ArgumentNullException(nameof(entry));
            if (string.IsNullOrEmpty(entry.id))
                entry.id = $"tomb_{DateTime.UtcNow.Ticks}";
            var data = Load();
            data.tombstones.Add(entry);
            Save(data);
            return entry.id;
        }

        /// <summary>Remove tombstone by id (vd sau khi player drain hết items). No-op nếu id không tồn tại.</summary>
        public static bool Remove(string id)
        {
            if (string.IsNullOrEmpty(id)) return false;
            var data = Load();
            int removed = data.tombstones.RemoveAll(t => t != null && t.id == id);
            if (removed > 0) Save(data);
            return removed > 0;
        }

        /// <summary>Wipe toàn bộ graveyard. Chỉ dùng cho test / debug menu.</summary>
        public static void Clear()
        {
            if (File.Exists(Path)) File.Delete(Path);
        }

        /// <summary>True nếu file tồn tại trên disk.</summary>
        public static bool HasFile => File.Exists(Path);
    }

    [Serializable]
    public class GraveyardData
    {
        public List<TombstoneData> tombstones = new();
    }

    [Serializable]
    public class TombstoneData
    {
        public string id;
        public int worldSeed;          // seed của run mà player chết
        public Vector3 position;       // toạ độ chết (world của run cũ — thông tin lịch sử)
        public int daySurvived;        // số ngày đã survive khi chết
        public int previousLifeRealmTier;
        public bool previousLifeWasAwakened;
        public List<InventorySlotData> items = new();
    }
}
