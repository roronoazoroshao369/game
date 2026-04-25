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
    }

    [Serializable]
    public class SaveData
    {
        public string savedAtUtc;
        public PlayerSaveData player;
        public WorldSaveData world;
        public List<InventorySlotData> inventory = new();
    }

    [Serializable]
    public class PlayerSaveData
    {
        public Vector3 position;
        public float hp, hunger, thirst, sanity, mana;
        public int realmTier;          // 0=Phàm Nhân, 1..9 = Luyện Khí 1..9
        public float cultivationXp;
        public string spiritRoot;      // "Hỏa", "Thủy", ...
    }

    [Serializable]
    public class WorldSaveData
    {
        public float timeOfDay01;
        public int seed;
        public int daysSurvived;
    }

    [Serializable]
    public class InventorySlotData
    {
        public string itemId;
        public int count;
    }
}
