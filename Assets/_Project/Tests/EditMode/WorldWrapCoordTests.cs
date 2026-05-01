using NUnit.Framework;
using UnityEngine;
using UnityEngine.Tilemaps;
using WildernessCultivation.World;

namespace WildernessCultivation.Tests.EditMode
{
    /// <summary>
    /// Tests cho toroidal wrap-around foundation tại <see cref="WorldGenerator"/>.
    ///
    /// Toroidal world = tọa độ wrap mod size khi query biome/tile/noise → player đi mãi
    /// không tới điểm cuối (hình bánh donut). PR foundation: wrap chỉ ở API queries —
    /// chunk streaming + render padding ở PR sau.
    ///
    /// Verify 3 nhóm invariant:
    ///  1. WrapCoord static helper — modulo dương đúng cho input mọi dấu + safety guard.
    ///  2. wrapWorld = true — query ở `(x, y)` và `(x + N, y + N)` trả về cùng giá trị.
    ///  3. wrapWorld = false — backward compat: behavior trong [0, N) không đổi.
    /// </summary>
    public class WorldWrapCoordTests
    {
        WorldGenerator wg;
        GameObject go;

        [SetUp]
        public void Setup()
        {
            go = new GameObject("WG");
            wg = go.AddComponent<WorldGenerator>();
            wg.size = new Vector2Int(10, 10);
            wg.seed = 42;
            wg.biomeNoiseScale = 0.05f;
            wg.biomeBoundaryWarp = 0f; // tách concern khỏi domain warp
            wg.wrapWorld = true;
        }

        [TearDown]
        public void Teardown()
        {
            if (go != null) Object.DestroyImmediate(go);
        }

        // ===== 1. WrapCoord static helper =====

        [Test]
        public void WrapCoord_PositiveInRange_Identity()
        {
            Assert.AreEqual(0, WorldGenerator.WrapCoord(0, 10));
            Assert.AreEqual(5, WorldGenerator.WrapCoord(5, 10));
            Assert.AreEqual(9, WorldGenerator.WrapCoord(9, 10));
        }

        [Test]
        public void WrapCoord_AtBoundary_WrapsToZero()
        {
            Assert.AreEqual(0, WorldGenerator.WrapCoord(10, 10));
            Assert.AreEqual(0, WorldGenerator.WrapCoord(20, 10));
        }

        [Test]
        public void WrapCoord_BeyondBoundary_Wraps()
        {
            Assert.AreEqual(5, WorldGenerator.WrapCoord(15, 10));
            Assert.AreEqual(3, WorldGenerator.WrapCoord(123, 10));
        }

        [Test]
        public void WrapCoord_NegativeInput_WrapsToPositive()
        {
            // C# `-1 % 10 == -1` (sign-preserving) → cần điều chỉnh về [0, N).
            Assert.AreEqual(9, WorldGenerator.WrapCoord(-1, 10));
            Assert.AreEqual(0, WorldGenerator.WrapCoord(-10, 10));
            Assert.AreEqual(5, WorldGenerator.WrapCoord(-25, 10));
        }

        [Test]
        public void WrapCoord_NonPositiveSize_PassThrough()
        {
            // Safety guard: size chưa init → không divide-by-zero, return v as-is.
            Assert.AreEqual(5, WorldGenerator.WrapCoord(5, 0));
            Assert.AreEqual(-3, WorldGenerator.WrapCoord(-3, 0));
            Assert.AreEqual(7, WorldGenerator.WrapCoord(7, -10));
        }

        // ===== 2. wrapWorld = true (default) — periodic query =====

        [Test]
        public void BiomeNoiseValue_WrapEnabled_PeriodicAtSize()
        {
            // (0, 0) phải == (size.x, 0) == (-size.x, 0) == (size.x * 2, 0).
            float baseV = wg.BiomeNoiseValue(0, 0);
            Assert.AreEqual(baseV, wg.BiomeNoiseValue(wg.size.x, 0), 1e-6f);
            Assert.AreEqual(baseV, wg.BiomeNoiseValue(-wg.size.x, 0), 1e-6f);
            Assert.AreEqual(baseV, wg.BiomeNoiseValue(wg.size.x * 2, 0), 1e-6f);
            Assert.AreEqual(baseV, wg.BiomeNoiseValue(0, wg.size.y), 1e-6f);
            Assert.AreEqual(baseV, wg.BiomeNoiseValue(0, -wg.size.y), 1e-6f);
        }

        [Test]
        public void BiomeNoiseValue_WrapEnabled_NegativeWrapsToCanonical()
        {
            // (-1, 0) wraps tới (size.x - 1, 0).
            float canonical = wg.BiomeNoiseValue(wg.size.x - 1, 0);
            float wrapped = wg.BiomeNoiseValue(-1, 0);
            Assert.AreEqual(canonical, wrapped, 1e-6f);
        }

        [Test]
        public void PickGroundTile_WrapEnabled_PeriodicAtSize()
        {
            // Setup biome có 4 variants giả (TileBase asset).
            var biome = ScriptableObject.CreateInstance<BiomeSO>();
            biome.biomeId = "test";
            biome.groundTileVariants = new TileBase[4];
            for (int i = 0; i < 4; i++)
                biome.groundTileVariants[i] = ScriptableObject.CreateInstance<Tile>();

            for (int x = 0; x < wg.size.x; x++)
                for (int y = 0; y < wg.size.y; y++)
                {
                    var canonical = wg.PickGroundTile(biome, x, y);
                    var wrappedHi = wg.PickGroundTile(biome, x + wg.size.x, y + wg.size.y);
                    var wrappedLo = wg.PickGroundTile(biome, x - wg.size.x, y - wg.size.y);
                    Assert.AreSame(canonical, wrappedHi,
                        $"PickGroundTile tại (+size, +size) phải trả tile như canonical ({x},{y})");
                    Assert.AreSame(canonical, wrappedLo,
                        $"PickGroundTile tại (-size, -size) phải trả tile như canonical ({x},{y})");
                }

            for (int i = 0; i < biome.groundTileVariants.Length; i++)
                Object.DestroyImmediate(biome.groundTileVariants[i]);
            Object.DestroyImmediate(biome);
        }

        [Test]
        public void BiomeAt_WrapEnabled_NegativeWorldPosWrapsCorrectly()
        {
            var forest = ScriptableObject.CreateInstance<BiomeSO>();
            forest.biomeId = "forest";
            forest.selectionRange = new Vector2(0f, 1f); // catch-all → mọi noise → forest
            wg.biomes = new[] { forest };

            // Vị trí (-3, -7) wrap tới (size.x - 3, size.y - 7) = (7, 3).
            var canonical = wg.BiomeAt(new Vector3(7.5f, 3.5f, 0f));
            var wrapped = wg.BiomeAt(new Vector3(-2.5f, -6.5f, 0f));
            Assert.AreSame(canonical, wrapped,
                "BiomeAt(-2.5, -6.5) phải wrap tới BiomeAt(7.5, 3.5)");

            Object.DestroyImmediate(forest);
        }

        // ===== 3. wrapWorld = false — backward compat =====

        [Test]
        public void BiomeNoiseValue_WrapDisabled_NotPeriodic()
        {
            wg.wrapWorld = false;
            // Khi wrap tắt: (0,0) và (size.x, 0) là 2 cell xa nhau trên Perlin → noise khác.
            float a = wg.BiomeNoiseValue(0, 0);
            float b = wg.BiomeNoiseValue(wg.size.x, 0);
            Assert.AreNotEqual(a, b,
                "wrapWorld=false phải trả Perlin riêng tại 2 vị trí khác nhau (không wrap)");
        }

        [Test]
        public void BiomeNoiseValue_WrapDisabled_InRangeBehaviorUnchanged()
        {
            // Backward compat: trong [0, size) thì wrap không ảnh hưởng (identity wrap).
            // Test cùng kết quả ở wrap=on vs wrap=off cho input ∈ canonical range.
            wg.wrapWorld = true;
            float on = wg.BiomeNoiseValue(3, 7);
            wg.wrapWorld = false;
            float off = wg.BiomeNoiseValue(3, 7);
            Assert.AreEqual(on, off, 1e-6f,
                "Trong canonical range, wrap on/off phải trả cùng giá trị");
        }

        [Test]
        public void BiomeAt_WrapDisabled_NegativeClampsToOrigin()
        {
            wg.wrapWorld = false;
            var forest = ScriptableObject.CreateInstance<BiomeSO>();
            forest.biomeId = "forest";
            forest.selectionRange = new Vector2(0f, 1f);
            wg.biomes = new[] { forest };

            // Vị trí (-100, -100) clamp về (0, 0).
            var canonical = wg.BiomeAt(new Vector3(0f, 0f, 0f));
            var clamped = wg.BiomeAt(new Vector3(-100f, -100f, 0f));
            Assert.AreSame(canonical, clamped, "wrapWorld=false phải clamp negative về origin");

            Object.DestroyImmediate(forest);
        }
    }
}
