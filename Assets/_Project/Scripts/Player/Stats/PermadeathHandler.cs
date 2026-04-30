using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using WildernessCultivation.Core;
using WildernessCultivation.Cultivation;
using WildernessCultivation.Items;
using WildernessCultivation.World;

namespace WildernessCultivation.Player.Stats
{
    /// <summary>
    /// Permadeath subsystem: khi player chết → snapshot inventory thành tombstone, ghi MetaStats
    /// lifetime, xoá save slot, reseed world, reload scene (chỉ trong PlayMode).
    ///
    /// Tách khỏi <see cref="PlayerStats"/> (R1 refactor). PlayerStats vẫn fire OnDeath event và
    /// gọi <see cref="Execute"/> — façade method <see cref="PlayerStats.ExecutePermadeath"/>
    /// giữ public surface cho test.
    /// </summary>
    public class PermadeathHandler : MonoBehaviour
    {
        [Tooltip("True = khi HP về 0 sẽ wipe save slot, dump inventory thành tombstone, reload scene. False = chỉ raise OnDeath (test / tutorial mode).")]
        public bool permadeathEnabled = true;
        [Tooltip("Giây delay trước khi reload scene sau khi chết (cho tử vong overlay).")]
        public float deathReloadDelay = 1.5f;

        PlayerStats stats;
        TimeManager timeManager;

        void Awake()
        {
            stats = GetComponent<PlayerStats>();
        }

        void Start()
        {
            timeManager = GameManager.Instance != null
                ? GameManager.Instance.timeManager
                : ServiceLocator.Get<TimeManager>();
        }

        /// <summary>
        /// Dump inventory → tombstone, record meta stats, xoá save slot, reload scene với seed mới.
        /// Public để test gọi trực tiếp (test runner: Application.isPlaying = false → skip reload).
        /// </summary>
        public void Execute()
        {
            // Lazy resolve cho EditMode (Awake không tự chạy với AddComponent runtime).
            if (stats == null) stats = GetComponent<PlayerStats>();

            int days = 0;
            int worldSeed = 0;
            var tm = timeManager != null ? timeManager
                   : (GameManager.Instance != null ? GameManager.Instance.timeManager : null);
            if (tm != null) days = tm.daysSurvived;
            var wg = WorldGenerator.Instance;
            if (wg != null) worldSeed = wg.seed;

            int realmTier = 0;
            var realm = GetComponent<RealmSystem>();
            if (realm != null) realmTier = realm.currentTier;

            var inv = GetComponent<Inventory>();
            var entry = new TombstoneData
            {
                worldSeed = worldSeed,
                position = transform.position,
                daySurvived = days,
                previousLifeRealmTier = realmTier,
                previousLifeWasAwakened = stats != null && stats.IsAwakened,
                items = SnapshotInventoryItems(inv),
            };
            try { Graveyard.Append(entry); }
            catch (Exception e) { Debug.LogError($"[Permadeath] Graveyard.Append thất bại: {e}"); }

            try { MetaStats.RecordDeath(days, realmTier, stats != null && stats.IsAwakened); }
            catch (Exception e) { Debug.LogError($"[Permadeath] MetaStats.RecordDeath thất bại: {e}"); }

            try { SaveSystem.Delete(); }
            catch (Exception e) { Debug.LogError($"[Permadeath] SaveSystem.Delete thất bại: {e}"); }

            // Reseed world cho run kế tiếp.
            if (wg != null) wg.seed = UnityEngine.Random.Range(1, int.MaxValue);

            if (Application.isPlaying)
            {
                if (deathReloadDelay > 0f) Invoke(nameof(ReloadActiveScene), deathReloadDelay);
                else ReloadActiveScene();
            }
        }

        static List<InventorySlotData> SnapshotInventoryItems(Inventory inv)
        {
            var list = new List<InventorySlotData>();
            if (inv == null) return list;
            foreach (var s in inv.Slots)
            {
                if (s == null || s.IsEmpty || s.item == null) continue;
                list.Add(new InventorySlotData
                {
                    itemId = s.item.itemId,
                    count = s.count,
                    freshRemaining = s.IsPerishable ? s.freshRemaining : -1f,
                    durability = s.IsDurable ? s.durability : -1f,
                });
            }
            return list;
        }

        void ReloadActiveScene()
        {
            GameManager.ResetInstanceForSceneReload();
            var active = SceneManager.GetActiveScene();
            SceneManager.LoadScene(active.buildIndex >= 0 ? active.buildIndex : 0);
        }
    }
}
