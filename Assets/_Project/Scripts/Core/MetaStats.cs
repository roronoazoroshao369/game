using System;
using System.IO;
using UnityEngine;

namespace WildernessCultivation.Core
{
    /// <summary>
    /// Lifetime stats xuyên các đời (run-of-runs telemetry). Giữ riêng khỏi
    /// <see cref="SaveSystem"/> + <see cref="Graveyard"/> vì:
    ///  - Save slot bị wipe khi permadeath → mất daysSurvived của đời hiện tại.
    ///  - Graveyard chỉ chứa snapshot per-life; aggregate cần file riêng.
    ///
    /// Lưu ở <c>Application.persistentDataPath/meta.json</c>. Persist mãi.
    /// </summary>
    public static class MetaStats
    {
        const string FileName = "meta.json";
        static string Path => System.IO.Path.Combine(Application.persistentDataPath, FileName);

        public static MetaStatsData Load()
        {
            if (!File.Exists(Path)) return new MetaStatsData();
            try
            {
                string json = File.ReadAllText(Path);
                var data = JsonUtility.FromJson<MetaStatsData>(json);
                return data ?? new MetaStatsData();
            }
            catch (Exception e)
            {
                Debug.LogError($"[MetaStats] Load failed: {e}");
                return new MetaStatsData();
            }
        }

        public static void Save(MetaStatsData data)
        {
            if (data == null) data = new MetaStatsData();
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(Path, json);
        }

        /// <summary>Cộng dồn 1 đời vừa kết thúc (chết). Update bestDaysSurvived,
        /// totalDeaths, ever-awakened/ever-foundation cờ.</summary>
        public static void RecordDeath(int daysSurvived, int realmTier, bool wasAwakened)
        {
            var data = Load();
            data.totalDeaths++;
            data.totalDaysLived += Mathf.Max(0, daysSurvived);
            if (daysSurvived > data.bestDaysSurvived) data.bestDaysSurvived = daysSurvived;
            if (wasAwakened) data.everAwakened = true;
            // Foundation = Trúc Cơ (tier ≥ 10 trong default realms[])
            if (realmTier >= 10) data.everReachedFoundation = true;
            if (realmTier > data.bestRealmTier) data.bestRealmTier = realmTier;
            Save(data);
        }

        public static void Clear()
        {
            if (File.Exists(Path)) File.Delete(Path);
        }

        public static bool HasFile => File.Exists(Path);
    }

    [Serializable]
    public class MetaStatsData
    {
        public int totalDeaths;
        public int totalDaysLived;
        public int bestDaysSurvived;
        public int bestRealmTier;
        public bool everAwakened;
        public bool everReachedFoundation;
    }
}
