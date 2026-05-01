using System.Collections.Generic;
using UnityEngine;
using WildernessCultivation.Mobs;

namespace WildernessCultivation.World
{
    /// <summary>
    /// Chunk streaming cho toroidal world. Track player position → load chunks trong
    /// render window, unload chunks ngoài window.
    ///
    /// Mỗi chunk = 1 child GameObject "Chunk_{x}_{y}" làm parent cho resources spawn
    /// trong chunk đó. Destroy parent = remove cả chunk's resources một lần
    /// (`Destroy(parent)`). Tilemap cells clear riêng qua `WorldGenerator.ClearChunkTiles`.
    ///
    /// Tilemap & resource pos render tại world coord UNWRAPPED — biome/variant lookup
    /// internally wrap (PR #3a foundation) → cell vượt biên cho cùng nội dung như
    /// canonical. Player walks past edge → seamless wrap (visual identical).
    ///
    /// MobSpawner.SetupBounds (nếu reference set) update theo player position mỗi
    /// chunk transition → spawn quanh player thay vì khắp world.
    /// </summary>
    [RequireComponent(typeof(WorldGenerator))]
    public class ChunkManager : MonoBehaviour
    {
        [Header("Render window")]
        [Tooltip("Bán kính render (chunks). 4 = 9×9 = 81 chunks active. Chunk size lấy từ WorldGenerator.chunkSize. 4×16 = 64 cells xa nhất → đủ camera viewport thông thường + padding để player không thấy chunk pop khi đi.")]
        [Range(1, 8)]
        public int renderRadiusChunks = 4;

        [Tooltip("Player follow target. Null → dùng WorldGenerator.player.")]
        public Transform player;

        [Tooltip("Optional MobSpawner để update spawn bounds theo player. Null → skip (mobs sẽ giữ bounds set lúc khởi đầu).")]
        public MobSpawner mobSpawner;

        WorldGenerator wg;
        readonly Dictionary<Vector2Int, GameObject> activeChunks = new();
        Vector2Int lastCenter = new(int.MinValue, int.MinValue);
        bool initialized;

        void Awake()
        {
            wg = GetComponent<WorldGenerator>();
        }

        void Start()
        {
            if (player == null && wg != null) player = wg.player;
            // Initial load — tránh hang Start với map lớn: load ngay 1 lần ở chunk player
            // hiện đang đứng. Nếu player chưa set: skip đến lần Update đầu tiên có target.
            TryUpdateChunks(force: true);
        }

        void Update()
        {
            TryUpdateChunks(force: false);
        }

        /// <summary>Force reload (vd sau load save) — clear hết chunks rồi gen lại quanh player.</summary>
        public void Rebuild()
        {
            UnloadAll();
            initialized = false;
            TryUpdateChunks(force: true);
        }

        // public — EditMode test invoke trực tiếp thay vì chờ Update auto. Production code
        // chỉ cần Update() / Rebuild() — KHÔNG gọi TryUpdateChunks bên ngoài.
        public void TryUpdateChunks(bool force)
        {
            if (wg == null || player == null) return;
            var pcell = new Vector2Int(Mathf.FloorToInt(player.position.x), Mathf.FloorToInt(player.position.y));
            var center = wg.ChunkCoordOf(pcell.x, pcell.y);
            if (!force && initialized && center == lastCenter) return;
            lastCenter = center;
            initialized = true;
            SyncWindow(center);
            UpdateMobBounds(pcell);
        }

        void SyncWindow(Vector2Int center)
        {
            int r = Mathf.Max(0, renderRadiusChunks);
            // Build target set
            var target = new HashSet<Vector2Int>();
            for (int dx = -r; dx <= r; dx++)
                for (int dy = -r; dy <= r; dy++)
                    target.Add(new Vector2Int(center.x + dx, center.y + dy));

            // Unload chunks not in target
            var toRemove = new List<Vector2Int>();
            foreach (var kv in activeChunks)
            {
                if (!target.Contains(kv.Key)) toRemove.Add(kv.Key);
            }
            foreach (var c in toRemove) UnloadChunk(c);

            // Load missing
            foreach (var c in target)
            {
                if (!activeChunks.ContainsKey(c)) LoadChunk(c);
            }
        }

        void LoadChunk(Vector2Int chunkCoord)
        {
            var parent = new GameObject($"Chunk_{chunkCoord.x}_{chunkCoord.y}");
            parent.transform.SetParent(wg.contentParent != null ? wg.contentParent : transform, false);
            wg.GenerateChunk(chunkCoord, parent.transform);
            activeChunks[chunkCoord] = parent;
        }

        void UnloadChunk(Vector2Int chunkCoord)
        {
            if (activeChunks.TryGetValue(chunkCoord, out var parent))
            {
                DestroySafe(parent);
                activeChunks.Remove(chunkCoord);
            }
            wg.ClearChunkTiles(chunkCoord);
        }

        void UnloadAll()
        {
            foreach (var kv in activeChunks)
            {
                DestroySafe(kv.Value);
                wg.ClearChunkTiles(kv.Key);
            }
            activeChunks.Clear();
        }

        // EditMode test gọi từ ngoài play loop → Destroy() log warning. Dùng
        // DestroyImmediate khi !isPlaying (test path), Destroy ở runtime (defer 1 frame).
        static void DestroySafe(GameObject go)
        {
            if (go == null) return;
            if (Application.isPlaying) Destroy(go);
            else Object.DestroyImmediate(go);
        }

        void UpdateMobBounds(Vector2Int playerCell)
        {
            if (mobSpawner == null || wg == null) return;
            int radiusCells = renderRadiusChunks * Mathf.Max(1, wg.chunkSize);
            // Mob spawn bounds = render window quanh player. KHÔNG wrap — bounds là
            // unwrapped world coord; mobs sẽ spawn trên unwrapped tilemap khớp với chunks.
            Vector2 min = new(playerCell.x - radiusCells, playerCell.y - radiusCells);
            Vector2 max = new(playerCell.x + radiusCells, playerCell.y + radiusCells);
            mobSpawner.SetupBounds(min, max);
        }

        // Diagnostic / test hooks. Production code KHÔNG mutate.
        public int ActiveChunkCount => activeChunks.Count;
        public bool HasChunk(Vector2Int c) => activeChunks.ContainsKey(c);
        public IReadOnlyDictionary<Vector2Int, GameObject> ActiveChunks => activeChunks;
    }
}
