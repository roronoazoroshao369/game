using NUnit.Framework;
using UnityEngine;
using UnityEngine.Tilemaps;
using WildernessCultivation.World;

namespace WildernessCultivation.Tests.EditMode
{
    /// <summary>
    /// EditMode tests cho BiomeSO upgrade: groundTileVariants (deterministic hash pick),
    /// ExtraNode Perlin band filter, Decoration entries shape + Perlin band.
    /// Backward-compat: biome cũ không set variants/decorations vẫn render như cũ.
    /// </summary>
    public class BiomeUpgradeTests
    {
        BiomeSO MakeBiome(string id)
        {
            var b = ScriptableObject.CreateInstance<BiomeSO>();
            b.biomeId = id;
            b.displayName = id;
            b.selectionRange = new Vector2(0f, 1f);
            return b;
        }

        WorldGenerator MakeWG(int seed = 42)
        {
            var go = new GameObject("WG");
            var wg = go.AddComponent<WorldGenerator>();
            wg.size = new Vector2Int(8, 8);
            wg.seed = seed;
            return wg;
        }

        // ---- groundTileVariants ----

        [Test]
        public void PickGroundTile_NoVariants_ReturnsBiomeGroundTile()
        {
            var wg = MakeWG();
            var b = MakeBiome("forest");
            var tile = ScriptableObject.CreateInstance<Tile>();
            b.groundTile = tile;
            // groundTileVariants null → fallback groundTile.
            Assert.AreSame(tile, wg.PickGroundTile(b, 0, 0));
            Assert.AreSame(tile, wg.PickGroundTile(b, 5, 3));

            Object.DestroyImmediate(wg.gameObject);
            Object.DestroyImmediate(b);
            Object.DestroyImmediate(tile);
        }

        [Test]
        public void PickGroundTile_EmptyVariants_FallbackGroundTile()
        {
            var wg = MakeWG();
            var b = MakeBiome("forest");
            var tile = ScriptableObject.CreateInstance<Tile>();
            b.groundTile = tile;
            b.groundTileVariants = new TileBase[0];
            // Empty array → fallback groundTile (no crash).
            Assert.AreSame(tile, wg.PickGroundTile(b, 0, 0));

            Object.DestroyImmediate(wg.gameObject);
            Object.DestroyImmediate(b);
            Object.DestroyImmediate(tile);
        }

        [Test]
        public void PickGroundTile_WithVariants_DeterministicPick()
        {
            var wg = MakeWG(seed: 100);
            var b = MakeBiome("forest");
            var t1 = ScriptableObject.CreateInstance<Tile>();
            var t2 = ScriptableObject.CreateInstance<Tile>();
            var t3 = ScriptableObject.CreateInstance<Tile>();
            b.groundTileVariants = new TileBase[] { t1, t2, t3 };

            // Same input → same output (deterministic).
            var picked1 = wg.PickGroundTile(b, 5, 7);
            var picked2 = wg.PickGroundTile(b, 5, 7);
            Assert.AreSame(picked1, picked2);

            // All picks belong to the variant set.
            for (int x = 0; x < 8; x++)
                for (int y = 0; y < 8; y++)
                {
                    var p = wg.PickGroundTile(b, x, y);
                    Assert.IsTrue(p == t1 || p == t2 || p == t3,
                        $"Picked tile at ({x},{y}) phải thuộc variants set");
                }

            Object.DestroyImmediate(wg.gameObject);
            Object.DestroyImmediate(b);
            Object.DestroyImmediate(t1);
            Object.DestroyImmediate(t2);
            Object.DestroyImmediate(t3);
        }

        [Test]
        public void PickGroundTile_DifferentSeeds_DifferentDistribution()
        {
            // Verify seed thực sự ảnh hưởng pick — không phải 100% cells khác nhau (variance OK)
            // nhưng ít nhất 1 cell phải khác giữa 2 seed.
            var wgA = MakeWG(seed: 1);
            var wgB = MakeWG(seed: 999);
            var b = MakeBiome("forest");
            var t1 = ScriptableObject.CreateInstance<Tile>();
            var t2 = ScriptableObject.CreateInstance<Tile>();
            b.groundTileVariants = new TileBase[] { t1, t2 };

            int diff = 0;
            for (int x = 0; x < 8; x++)
                for (int y = 0; y < 8; y++)
                {
                    if (wgA.PickGroundTile(b, x, y) != wgB.PickGroundTile(b, x, y)) diff++;
                }
            Assert.Greater(diff, 0, "Seed khác phải dẫn đến ít nhất 1 cell pick khác variant");

            Object.DestroyImmediate(wgA.gameObject);
            Object.DestroyImmediate(wgB.gameObject);
            Object.DestroyImmediate(b);
            Object.DestroyImmediate(t1);
            Object.DestroyImmediate(t2);
        }

        [Test]
        public void PickGroundTile_NullVariantSlot_FallbackGroundTile()
        {
            var wg = MakeWG(seed: 0);
            var b = MakeBiome("forest");
            var tile = ScriptableObject.CreateInstance<Tile>();
            b.groundTile = tile;
            // Nhỡ user set partial config — variant slot null không được crash.
            b.groundTileVariants = new TileBase[] { null, null, null };
            // Tất cả slot null → fallback groundTile.
            Assert.AreSame(tile, wg.PickGroundTile(b, 0, 0));
            Assert.AreSame(tile, wg.PickGroundTile(b, 5, 3));

            Object.DestroyImmediate(wg.gameObject);
            Object.DestroyImmediate(b);
            Object.DestroyImmediate(tile);
        }

        [Test]
        public void PickGroundTile_NoBiome_UsesLegacyGroundTile()
        {
            var wg = MakeWG();
            var legacy = ScriptableObject.CreateInstance<Tile>();
            wg.legacyGroundTile = legacy;
            Assert.AreSame(legacy, wg.PickGroundTile(null, 0, 0));

            Object.DestroyImmediate(wg.gameObject);
            Object.DestroyImmediate(legacy);
        }

        // ---- Perlin band ----

        [Test]
        public void InPerlinBand_DefaultZeroZero_NoConstraint()
        {
            // Backward compat: ExtraNode/Decoration cũ không set perlinMin/Max (= 0/0)
            // phải behave như no constraint.
            Assert.IsTrue(WorldGenerator.InPerlinBand(0.0f, 0f, 0f));
            Assert.IsTrue(WorldGenerator.InPerlinBand(0.5f, 0f, 0f));
            Assert.IsTrue(WorldGenerator.InPerlinBand(1.0f, 0f, 0f));
        }

        [Test]
        public void InPerlinBand_OnlyLowerBound_TreatAsLowerOnly()
        {
            // perlinMin=0.5, perlinMax=0 (unset) → chỉ filter lower bound.
            Assert.IsFalse(WorldGenerator.InPerlinBand(0.3f, 0.5f, 0f));
            Assert.IsTrue(WorldGenerator.InPerlinBand(0.5f, 0.5f, 0f));
            Assert.IsTrue(WorldGenerator.InPerlinBand(0.9f, 0.5f, 0f));
        }

        [Test]
        public void InPerlinBand_BothBounds_FilterRange()
        {
            // Mountain ore vein: perlin 0.8..1.0
            Assert.IsFalse(WorldGenerator.InPerlinBand(0.5f, 0.8f, 1.0f));
            Assert.IsFalse(WorldGenerator.InPerlinBand(0.79f, 0.8f, 1.0f));
            Assert.IsTrue(WorldGenerator.InPerlinBand(0.85f, 0.8f, 1.0f));
            Assert.IsTrue(WorldGenerator.InPerlinBand(1.0f, 0.8f, 1.0f));
        }

        // ---- ExtraNode shape ----

        [Test]
        public void ExtraNode_PerlinFields_Default0()
        {
            var en = new BiomeSO.ExtraNode();
            Assert.AreEqual(0f, en.perlinMin, 0.001f);
            Assert.AreEqual(0f, en.perlinMax, 0.001f);
        }

        [Test]
        public void ExtraNode_AssignPerlinBand_Preserved()
        {
            var b = MakeBiome("test");
            b.extraNodes = new[]
            {
                new BiomeSO.ExtraNode { density = 0.05f, perlinMin = 0.8f, perlinMax = 1.0f },
            };
            Assert.AreEqual(0.8f, b.extraNodes[0].perlinMin, 0.001f);
            Assert.AreEqual(1.0f, b.extraNodes[0].perlinMax, 0.001f);
            Object.DestroyImmediate(b);
        }

        // ---- Decoration shape ----

        [Test]
        public void Decorations_DefaultEmpty_NoCrash()
        {
            var b = MakeBiome("test");
            Assert.IsTrue(b.decorations == null || b.decorations.Length == 0);
            Object.DestroyImmediate(b);
        }

        [Test]
        public void Decorations_AssignAndRead_PreservesShape()
        {
            var b = MakeBiome("test");
            var dummyPrefab = new GameObject("DummyFlower");
            b.decorations = new[]
            {
                new BiomeSO.DecorationEntry { prefab = dummyPrefab, density = 0.10f },
                new BiomeSO.DecorationEntry { prefab = dummyPrefab, density = 0.05f, perlinMin = 0.6f, perlinMax = 1f },
            };

            Assert.AreEqual(2, b.decorations.Length);
            Assert.AreEqual(0.10f, b.decorations[0].density, 0.001f);
            Assert.AreEqual(0.05f, b.decorations[1].density, 0.001f);
            Assert.AreEqual(0.6f, b.decorations[1].perlinMin, 0.001f);
            Assert.AreEqual(1f, b.decorations[1].perlinMax, 0.001f);
            Assert.AreSame(dummyPrefab, b.decorations[0].prefab);

            Object.DestroyImmediate(dummyPrefab);
            Object.DestroyImmediate(b);
        }
    }
}
