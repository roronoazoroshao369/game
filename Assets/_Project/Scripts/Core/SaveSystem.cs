using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace WildernessCultivation.Core
{
    /// <summary>
    /// Save/Load JSON đơn giản vào Application.persistentDataPath. 1 slot.
    /// </summary>
    public static class SaveSystem
    {
        const string FileName = "save_slot_0.json";
        static string Path => System.IO.Path.Combine(Application.persistentDataPath, FileName);

        public static void Save(SaveData data)
        {
            data.savedAtUtc = DateTime.UtcNow.ToString("o");
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(Path, json);
            Debug.Log($"[Save] Saved to {Path}");
        }

        public static bool TryLoad(out SaveData data)
        {
            data = null;
            if (!File.Exists(Path)) return false;
            try
            {
                string json = File.ReadAllText(Path);
                data = JsonUtility.FromJson<SaveData>(json);
                return data != null;
            }
            catch (Exception e)
            {
                Debug.LogError($"[Save] Load failed: {e}");
                return false;
            }
        }

        public static void Delete()
        {
            if (File.Exists(Path)) File.Delete(Path);
        }

        /// <summary>True nếu file save tồn tại — dùng để quyết định hiển thị nút Continue/New Game.</summary>
        public static bool HasSave => File.Exists(Path);
    }

    [Serializable]
    public class SaveData
    {
        public string savedAtUtc;
        public PlayerSaveData player;
        public WorldSaveData world;
        public List<InventorySlotData> inventory = new();
        // R5 follow-up: per-vendor persistent stock. Backward-compat: JsonUtility sẽ
        // default null / empty list cho save cũ (không có field này) — VendorNPC.RestoreState
        // early-return khi null.
        public List<VendorSaveData> vendors = new();
        // R5 follow-up: per-companion state (position + hp + hunger + mode).
        public List<CompanionSaveData> companions = new();
    }

    [Serializable]
    public class PlayerSaveData
    {
        public Vector3 position;
        public float hp, hunger, thirst, sanity, mana;
        public float bodyTemp = 50f;
        public int realmTier;          // 0=Phàm Nhân, 1..9 = Luyện Khí 1..9
        public float cultivationXp;
        public string spiritRoot;      // "Hỏa", "Thủy", ...
        public bool isAwakened;        // True = đã khai mở tu tiên. False = Thường Nhân.
        public int phamFailStreak;     // Pity counter — số fail Phàm liên tiếp. Reset 0 khi success.
        public float wetness;          // Wetness gauge [0..100]; persist để vào shelter rồi save không mất tier.
    }

    [Serializable]
    public class WorldSaveData
    {
        public float timeOfDay01;
        public int seed;
        public int daysSurvived;
        public int seasonIndex;        // 0=Spring..3=Winter
        public int weatherIndex;       // 0=Clear, 1=Rain, 2=Storm
        // Cells player đã harvest (chop tree / mine rock / pick herb …). GenerateCellAt
        // skip resource spawn ở các cell này → walk away / walk back vẫn gone.
        // Chunk streaming-safe: chunk unload/reload tôn trọng list này. Save persist.
        public List<Vector2Int> harvestedCells = new();
        // Cells có grass-tile decoration đã bị mob (rabbit) / player ăn. Track riêng
        // để không block resource spawn (tree/rock/grass-bush vẫn có thể xuất hiện ở cell này
        // sau khi grass-tile bị eat). Backward-compat: save cũ không có field → null,
        // RestoreState handle.
        public List<Vector2Int> harvestedGrassCells = new();
    }

    [Serializable]
    public class InventorySlotData
    {
        public string itemId;
        public int count;
        public float freshRemaining = -1f;
        public float durability = -1f;
    }

    [Serializable]
    public class VendorSaveData
    {
        public string vendorId;
        public List<int> stocks = new();
    }

    [Serializable]
    public class CompanionSaveData
    {
        public string companionId;
        public Vector3 position;
        public float hp;
        public float hunger;
        public int mode; // CompanionMode enum
    }
}
