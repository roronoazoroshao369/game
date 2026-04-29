using NUnit.Framework;
using UnityEngine;
using UnityEngine.Tilemaps;
using WildernessCultivation.World;

namespace WildernessCultivation.Tests.EditMode
{
    /// <summary>
    /// EditMode tests cho ground rendering của WorldGenerator.
    /// Verify hai path:
    ///  1. Khi groundTilemap + tile được wire → SetTile per cell, KHÔNG Instantiate ground prefab.
    ///  2. Khi groundTilemap == null nhưng groundPrefab set → fallback per-tile Instantiate (legacy).
    ///
    /// Mục tiêu invariant: ở map lớn ground không tạo N×M GameObjects khi Tilemap path active.
    /// </summary>
    public class WorldGeneratorGroundTests
    {
        GameObject wgGo;
        GameObject contentGo;
        GameObject gridGo;
        WorldGenerator wg;
        Tilemap tilemap;
        Tile groundTile;
        GameObject groundPrefab;

        [SetUp]
        public void Setup()
        {
            wgGo = new GameObject("WG");
            contentGo = new GameObject("Content");
            wg = wgGo.AddComponent<WorldGenerator>();
            wg.size = new Vector2Int(4, 3);
            wg.seed = 7;
            wg.contentParent = contentGo.transform;
            // Legacy biomes-empty path để tránh BiomeSO setup phức tạp; cover Tilemap branch via
            // legacyGroundTile + groundPrefab fallback. Biome path test riêng nếu cần.

            gridGo = new GameObject("Grid", typeof(Grid));
            var tmGo = new GameObject("Ground", typeof(Tilemap));
            tmGo.transform.SetParent(gridGo.transform, false);
            tilemap = tmGo.GetComponent<Tilemap>();

            groundTile = ScriptableObject.CreateInstance<Tile>();

            groundPrefab = new GameObject("GroundPrefab");
            // Disable so test môi trường không leak instances vô hình.
            groundPrefab.SetActive(false);
        }

        [TearDown]
        public void Teardown()
        {
            if (wgGo != null) Object.DestroyImmediate(wgGo);
            if (contentGo != null) Object.DestroyImmediate(contentGo);
            if (gridGo != null) Object.DestroyImmediate(gridGo);
            if (groundTile != null) Object.DestroyImmediate(groundTile);
            if (groundPrefab != null) Object.DestroyImmediate(groundPrefab);
        }

        [Test]
        public void Generate_WithTilemapAndTile_PaintsTilesAndSkipsInstantiate()
        {
            wg.groundTilemap = tilemap;
            wg.legacyGroundTile = groundTile;
            wg.groundPrefab = groundPrefab; // nếu Tilemap path active thì prefab phải được skip.

            int contentChildrenBefore = contentGo.transform.childCount;
            wg.GenerateNow();

            // Tilemap path: mọi cell trong size được set tile (resource nodes spawn vào contentParent
            // chứ không phải tilemap, nên tilemap chỉ chứa ground tile).
            for (int x = 0; x < wg.size.x; x++)
                for (int y = 0; y < wg.size.y; y++)
                {
                    var t = tilemap.GetTile(new Vector3Int(x, y, 0));
                    Assert.AreSame(groundTile, t,
                        $"Tile tại ({x},{y}) phải là legacyGroundTile khi groundTilemap được wire");
                }

            // Ground prefab Instantiate phải bị SKIP: contentParent không có child "GroundPrefab(Clone)".
            int groundClones = 0;
            for (int i = 0; i < contentGo.transform.childCount; i++)
            {
                var child = contentGo.transform.GetChild(i);
                if (child.name.StartsWith("GroundPrefab"))
                    groundClones++;
            }
            Assert.AreEqual(0, groundClones,
                "GroundPrefab phải KHÔNG được Instantiate khi Tilemap path active");
            Assert.GreaterOrEqual(contentGo.transform.childCount, contentChildrenBefore,
                "contentParent có thể có resource node spawn nhưng không < trước");
        }

        [Test]
        public void Generate_WithoutTilemap_FallsBackToInstantiateGroundPrefab()
        {
            wg.groundTilemap = null;
            wg.legacyGroundTile = groundTile; // có tile nhưng tilemap null → bỏ qua, dùng prefab.
            wg.groundPrefab = groundPrefab;

            wg.GenerateNow();

            // Legacy path: mỗi cell instantiate 1 GroundPrefab clone vào contentParent.
            int groundClones = 0;
            for (int i = 0; i < contentGo.transform.childCount; i++)
            {
                var child = contentGo.transform.GetChild(i);
                if (child.name.StartsWith("GroundPrefab"))
                    groundClones++;
            }
            int expected = wg.size.x * wg.size.y;
            Assert.AreEqual(expected, groundClones,
                $"Legacy fallback phải Instantiate {expected} GroundPrefab clones (1 per tile)");

            // Tilemap không có tile nào (fallback path không touch tilemap).
            Assert.IsNull(tilemap.GetTile(new Vector3Int(0, 0, 0)),
                "Tilemap không được set tile khi groundTilemap == null");
        }

        [Test]
        public void Generate_TilemapWiredButTileNull_FallsBackToInstantiate()
        {
            // Edge case: tilemap được gán nhưng cả legacyGroundTile và biome.groundTile đều null
            // → không có tile để paint, phải fallback sang Instantiate path để giữ ground hiện hữu.
            wg.groundTilemap = tilemap;
            wg.legacyGroundTile = null;
            wg.groundPrefab = groundPrefab;

            wg.GenerateNow();

            int groundClones = 0;
            for (int i = 0; i < contentGo.transform.childCount; i++)
            {
                var child = contentGo.transform.GetChild(i);
                if (child.name.StartsWith("GroundPrefab"))
                    groundClones++;
            }
            int expected = wg.size.x * wg.size.y;
            Assert.AreEqual(expected, groundClones,
                "Khi tile null, Tilemap path vô hiệu → phải fallback Instantiate prefab");
        }
    }
}
