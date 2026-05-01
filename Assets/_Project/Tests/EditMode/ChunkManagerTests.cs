using NUnit.Framework;
using UnityEngine;
using UnityEngine.Tilemaps;
using WildernessCultivation.World;

namespace WildernessCultivation.Tests.EditMode
{
    /// <summary>
    /// EditMode tests cho ChunkManager streaming logic. Verify 4 invariant cốt lõi:
    ///  1. Initial load — chunks active = (2r+1)² quanh player chunk.
    ///  2. Player move trong cùng chunk → no reload (idempotent Update).
    ///  3. Player cross chunk boundary → load chunks mới + unload chunks ngoài range.
    ///  4. Rebuild() clear hết rồi gen lại quanh player hiện tại.
    ///
    /// Test sử dụng Reflection helper để gọi private TryUpdateChunks (force) — Update()
    /// EditMode không tự fire, phải kích thủ công.
    /// </summary>
    public class ChunkManagerTests
    {
        GameObject worldGo;
        WorldGenerator wg;
        ChunkManager cm;
        GameObject playerGo;
        Tilemap tilemap;

        [SetUp]
        public void Setup()
        {
            worldGo = new GameObject("World");
            wg = worldGo.AddComponent<WorldGenerator>();
            wg.size = new Vector2Int(256, 256);
            wg.seed = 42;
            wg.chunkSize = 16;
            wg.contentParent = worldGo.transform;
            wg.wrapWorld = true;

            // Tilemap để test ClearChunkTiles không null-deref.
            var tmGo = new GameObject("Ground", typeof(Grid));
            tmGo.transform.SetParent(worldGo.transform, false);
            var ground = new GameObject("Ground", typeof(Tilemap), typeof(TilemapRenderer));
            ground.transform.SetParent(tmGo.transform, false);
            tilemap = ground.GetComponent<Tilemap>();
            wg.groundTilemap = tilemap;
            wg.legacyGroundTile = ScriptableObject.CreateInstance<Tile>();

            playerGo = new GameObject("Player");
            playerGo.transform.position = new Vector3(128f, 128f, 0f); // ở giữa world
            wg.player = playerGo.transform;

            cm = worldGo.AddComponent<ChunkManager>();
            cm.player = playerGo.transform;
            cm.renderRadiusChunks = 2; // 5×5 = 25 chunks
        }

        [TearDown]
        public void Teardown()
        {
            if (worldGo != null) Object.DestroyImmediate(worldGo);
            if (playerGo != null) Object.DestroyImmediate(playerGo);
            if (wg != null && wg.legacyGroundTile != null)
                Object.DestroyImmediate(wg.legacyGroundTile);
        }

        /// <summary>EditMode không có Start/Update auto. Gọi Rebuild để force initial load.</summary>
        void ForceLoad()
        {
            cm.Rebuild();
        }

        [Test]
        public void Rebuild_LoadsExpectedChunkCount()
        {
            ForceLoad();
            // 5×5 = 25 chunks active quanh player
            int expected = (2 * cm.renderRadiusChunks + 1) * (2 * cm.renderRadiusChunks + 1);
            Assert.AreEqual(expected, cm.ActiveChunkCount,
                $"Render radius {cm.renderRadiusChunks} → {expected} chunks active");
        }

        [Test]
        public void Rebuild_LoadsChunksAroundPlayer()
        {
            ForceLoad();
            // Player ở (128, 128), chunkSize=16 → chunk (8, 8).
            var center = new Vector2Int(8, 8);
            int r = cm.renderRadiusChunks;
            for (int dx = -r; dx <= r; dx++)
                for (int dy = -r; dy <= r; dy++)
                {
                    var c = new Vector2Int(center.x + dx, center.y + dy);
                    Assert.IsTrue(cm.HasChunk(c), $"Chunk {c} phải active");
                }
            // Chunk ngoài range không active.
            Assert.IsFalse(cm.HasChunk(new Vector2Int(center.x + r + 1, center.y)),
                "Chunk vượt ngoài render radius KHÔNG được active");
        }

        [Test]
        public void Rebuild_GeneratesGroundTilesInActiveChunks()
        {
            ForceLoad();
            // Cell (128, 128) phải có tile (player ở chunk center).
            Assert.IsNotNull(tilemap.GetTile(new Vector3Int(128, 128, 0)),
                "Cell tại player position phải có ground tile sau initial load");
        }

        [Test]
        public void PlayerMoveSameChunk_NoReload()
        {
            ForceLoad();
            int initialCount = cm.ActiveChunkCount;
            var initialKeys = new System.Collections.Generic.HashSet<Vector2Int>(cm.ActiveChunks.Keys);

            // Move player vài cell trong cùng chunk (chunk 16 → move 5 cells vẫn cùng chunk).
            playerGo.transform.position = new Vector3(133f, 130f, 0f);
            cm.TryUpdateChunks(false);

            Assert.AreEqual(initialCount, cm.ActiveChunkCount, "Active chunk count không đổi khi player ở cùng chunk");
            foreach (var k in initialKeys)
                Assert.IsTrue(cm.HasChunk(k), $"Chunk {k} vẫn phải active");
        }

        [Test]
        public void PlayerCrossChunkBoundary_LoadsNewUnloadsOld()
        {
            ForceLoad();
            int initialCount = cm.ActiveChunkCount;
            // Player ở chunk (8, 8), move +16 cells x → chunk (9, 8).
            playerGo.transform.position = new Vector3(144f, 128f, 0f);
            cm.TryUpdateChunks(false);

            // Vẫn cùng số chunk active (sliding window).
            Assert.AreEqual(initialCount, cm.ActiveChunkCount,
                "Sliding window: số chunks active không đổi khi cross boundary");
            // Chunk (9 + r, 8) load mới (right edge).
            Assert.IsTrue(cm.HasChunk(new Vector2Int(9 + cm.renderRadiusChunks, 8)),
                "Chunk ở edge mới phải load");
            // Chunk (8 - r, 8) unload (left edge cũ).
            Assert.IsFalse(cm.HasChunk(new Vector2Int(8 - cm.renderRadiusChunks, 8)),
                "Chunk ở edge cũ phải unload");
        }

        [Test]
        public void Rebuild_ClearsAllAndReloads()
        {
            ForceLoad();
            int firstLoadCount = cm.ActiveChunkCount;

            // Move player far + Rebuild → chunks load quanh new pos.
            playerGo.transform.position = new Vector3(64f, 64f, 0f); // chunk (4, 4)
            cm.Rebuild();

            Assert.AreEqual(firstLoadCount, cm.ActiveChunkCount,
                "Rebuild giữ cùng số chunks active");
            // Chunk (4, 4) phải có (player chunk).
            Assert.IsTrue(cm.HasChunk(new Vector2Int(4, 4)), "Chunk player chunk phải có");
            // Chunk (8, 8) cũ KHÔNG có.
            Assert.IsFalse(cm.HasChunk(new Vector2Int(8, 8)), "Chunk player cũ phải đã unload");
        }

        [Test]
        public void GenerateChunk_Deterministic_ReproducibleGroundTiles()
        {
            ForceLoad();
            // Snapshot tiles của chunk (8, 8).
            var chunkCoord = new Vector2Int(8, 8);
            int x0 = chunkCoord.x * wg.chunkSize;
            int y0 = chunkCoord.y * wg.chunkSize;
            var firstSnapshot = new TileBase[wg.chunkSize, wg.chunkSize];
            for (int dx = 0; dx < wg.chunkSize; dx++)
                for (int dy = 0; dy < wg.chunkSize; dy++)
                    firstSnapshot[dx, dy] = tilemap.GetTile(new Vector3Int(x0 + dx, y0 + dy, 0));

            // Rebuild → re-generate same chunk.
            cm.Rebuild();
            for (int dx = 0; dx < wg.chunkSize; dx++)
                for (int dy = 0; dy < wg.chunkSize; dy++)
                {
                    var t = tilemap.GetTile(new Vector3Int(x0 + dx, y0 + dy, 0));
                    Assert.AreSame(firstSnapshot[dx, dy], t,
                        $"Tile ({x0 + dx},{y0 + dy}) phải reproducible across regenerate");
                }
        }
    }
}
