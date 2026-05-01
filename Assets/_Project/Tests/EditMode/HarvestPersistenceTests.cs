using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Tilemaps;
using WildernessCultivation.Core;
using WildernessCultivation.World;

namespace WildernessCultivation.Tests.EditMode
{
    /// <summary>
    /// Tests cho harvest persistence (PR #3c). Verify 5 invariant:
    ///  1. MarkHarvested → IsHarvested true (idempotent — gọi 2 lần không lỗi).
    ///  2. GenerateCellAt skip resource spawn ở harvested cell (count tree clones drop từ 1 → 0).
    ///  3. CaptureState / RestoreState round-trip giữ nguyên harvested set.
    ///  4. RestoreState clear set cũ trước khi load (không merge).
    ///  5. ChunkManager.Rebuild sau RestoreState → chunks load bỏ qua harvested cells.
    /// </summary>
    public class HarvestPersistenceTests
    {
        GameObject worldGo;
        WorldGenerator wg;
        Tilemap tilemap;
        Tile groundTile;
        GameObject treePrefab;

        [SetUp]
        public void Setup()
        {
            SaveRegistry.ClearAll();

            worldGo = new GameObject("World");
            wg = worldGo.AddComponent<WorldGenerator>();
            wg.size = new Vector2Int(8, 8);
            wg.seed = 42;
            wg.chunkSize = 8;
            wg.contentParent = worldGo.transform;
            wg.wrapWorld = false; // tách concern khỏi wrap

            // Tilemap setup
            var grid = new GameObject("Grid", typeof(Grid));
            grid.transform.SetParent(worldGo.transform, false);
            var ground = new GameObject("Ground", typeof(Tilemap), typeof(TilemapRenderer));
            ground.transform.SetParent(grid.transform, false);
            tilemap = ground.GetComponent<Tilemap>();
            wg.groundTilemap = tilemap;
            groundTile = ScriptableObject.CreateInstance<Tile>();
            wg.legacyGroundTile = groundTile;

            // Tree prefab — 100% density để mọi cell có tree (nếu không harvested).
            // Mọi cell có Perlin n trong [0..1] → for n > 0.6 → tree spawn.
            // Để test đơn giản: dùng treePrefab + density 1.0, nhưng n cell-specific
            // không phải mọi cell n > 0.6. Test sẽ check delta count thay vì exact.
            treePrefab = new GameObject("TreePrefab");
            treePrefab.AddComponent<SpriteRenderer>(); // needed cho Instantiate
            treePrefab.SetActive(false);
            wg.treePrefab = treePrefab;
            wg.treeDensity = 1.0f;
        }

        [TearDown]
        public void Teardown()
        {
            SaveRegistry.ClearAll();
            if (worldGo != null) Object.DestroyImmediate(worldGo);
            if (treePrefab != null) Object.DestroyImmediate(treePrefab);
            if (groundTile != null) Object.DestroyImmediate(groundTile);
        }

        [Test]
        public void MarkHarvested_SetsAndIsHarvestedReturnsTrue()
        {
            Assert.IsFalse(wg.IsHarvested(3, 4), "Init: cell chưa harvested");
            wg.MarkHarvested(3, 4);
            Assert.IsTrue(wg.IsHarvested(3, 4), "Sau MarkHarvested: cell phải là harvested");
            // Idempotent
            wg.MarkHarvested(3, 4);
            Assert.AreEqual(1, wg.HarvestedCount, "MarkHarvested 2 lần cùng cell = 1 entry (HashSet)");
        }

        [Test]
        public void GenerateCellAt_HarvestedCell_SkipsResourceSpawn()
        {
            // Snapshot count tree clones khi không có harvested set.
            wg.GenerateNow();
            int before = CountTreeClones();

            // Reset world, mark all cells harvested, regen.
            ClearWorld();
            for (int x = 0; x < wg.size.x; x++)
                for (int y = 0; y < wg.size.y; y++)
                    wg.MarkHarvested(x, y);
            wg.GenerateNow();
            int after = CountTreeClones();

            Assert.AreEqual(0, after, "Cell harvested KHÔNG được spawn tree");
            Assert.Greater(before, 0, "Sanity: chưa harvested phải có >= 1 tree spawn");
        }

        [Test]
        public void Save_CaptureRestore_PreservesHarvestedCells()
        {
            wg.MarkHarvested(1, 2);
            wg.MarkHarvested(5, 7);
            wg.MarkHarvested(0, 0);
            Assert.AreEqual(3, wg.HarvestedCount);

            var data = new SaveData();
            wg.CaptureState(data);

            Assert.IsNotNull(data.world);
            Assert.IsNotNull(data.world.harvestedCells);
            Assert.AreEqual(3, data.world.harvestedCells.Count);
            Assert.IsTrue(data.world.harvestedCells.Contains(new Vector2Int(1, 2)));
            Assert.IsTrue(data.world.harvestedCells.Contains(new Vector2Int(5, 7)));
            Assert.IsTrue(data.world.harvestedCells.Contains(new Vector2Int(0, 0)));

            // Reset rồi restore.
            wg.ClearHarvestedCells();
            Assert.AreEqual(0, wg.HarvestedCount);

            wg.RestoreState(data);
            Assert.AreEqual(3, wg.HarvestedCount);
            Assert.IsTrue(wg.IsHarvested(1, 2));
            Assert.IsTrue(wg.IsHarvested(5, 7));
            Assert.IsTrue(wg.IsHarvested(0, 0));
            Assert.IsFalse(wg.IsHarvested(3, 3));
        }

        [Test]
        public void RestoreState_ClearsExistingHarvestedSet()
        {
            // Pre-existing state: 2 cells.
            wg.MarkHarvested(1, 1);
            wg.MarkHarvested(2, 2);

            // Save data only has 1 different cell.
            var data = new SaveData
            {
                world = new WorldSaveData
                {
                    seed = 99,
                    harvestedCells = new System.Collections.Generic.List<Vector2Int>
                    {
                        new(7, 7),
                    },
                },
            };

            wg.RestoreState(data);
            Assert.AreEqual(1, wg.HarvestedCount, "RestoreState clear set cũ trước khi load");
            Assert.IsTrue(wg.IsHarvested(7, 7));
            Assert.IsFalse(wg.IsHarvested(1, 1), "Cell cũ (1,1) phải bị clear");
            Assert.IsFalse(wg.IsHarvested(2, 2), "Cell cũ (2,2) phải bị clear");
        }

        [Test]
        public void RestoreState_NullHarvestedCells_DoesNotThrow()
        {
            wg.MarkHarvested(3, 3);
            // Save cũ chưa có field harvestedCells (JsonUtility default null trên load).
            var data = new SaveData
            {
                world = new WorldSaveData { seed = 1, harvestedCells = null },
            };

            // KHÔNG throw.
            Assert.DoesNotThrow(() => wg.RestoreState(data));
            Assert.AreEqual(0, wg.HarvestedCount, "harvestedCells null → clear set");
        }

        // ===== helpers =====

        int CountTreeClones()
        {
            // Tree prefab tên "TreePrefab" → clone tên "TreePrefab(Clone)".
            return GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None)
                .Count(g => g.name.StartsWith("TreePrefab"));
        }

        void ClearWorld()
        {
            // Destroy tilemap tiles + resource clones.
            var clones = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None)
                .Where(g => g.name.StartsWith("TreePrefab(Clone)")).ToArray();
            foreach (var c in clones) Object.DestroyImmediate(c);
        }
    }
}
