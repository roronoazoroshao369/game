using NUnit.Framework;
using UnityEngine;
using WildernessCultivation.World;

namespace WildernessCultivation.Tests.EditMode
{
    /// <summary>
    /// Tests cho domain-warp ở <see cref="WorldGenerator.BiomeNoiseValue"/>.
    ///
    /// Domain warp = secondary Perlin layer offset (x,y) trước khi sample primary biome
    /// noise → boundary giữa biomes uốn lượn ragged thay vì smooth contour curves
    /// (= ranh giới biome nhìn cứng, hard edge giữa hai màu ground biome khác nhau).
    ///
    /// Verify 4 invariant:
    ///  1. Determinism — cùng (seed, x, y) → cùng noise.
    ///  2. Range — mọi sample ∈ [0, 1].
    ///  3. Backward compat — biomeBoundaryWarp = 0 cho exact công thức Perlin cũ.
    ///  4. Warp does work — biomeBoundaryWarp > 0 phải khác warp = 0 trên đa số cell.
    /// </summary>
    public class BiomeBoundaryWarpTests
    {
        WorldGenerator wg;
        GameObject go;

        [SetUp]
        public void Setup()
        {
            go = new GameObject("WG");
            wg = go.AddComponent<WorldGenerator>();
            wg.seed = 42;
            wg.biomeNoiseScale = 0.05f;
        }

        [TearDown]
        public void Teardown()
        {
            if (go != null) Object.DestroyImmediate(go);
        }

        [Test]
        public void BiomeNoiseValue_Deterministic_SameInputSameOutput()
        {
            wg.biomeBoundaryWarp = 6f;
            float a = wg.BiomeNoiseValue(7, 11);
            float b = wg.BiomeNoiseValue(7, 11);
            Assert.AreEqual(a, b, 1e-6f, "BiomeNoiseValue phải deterministic");
        }

        [Test]
        public void BiomeNoiseValue_InRange_0to1()
        {
            wg.biomeBoundaryWarp = 6f;
            for (int y = 0; y < 32; y++)
                for (int x = 0; x < 32; x++)
                {
                    float v = wg.BiomeNoiseValue(x, y);
                    Assert.That(v, Is.InRange(0f, 1f),
                        $"BiomeNoiseValue tại ({x},{y}) = {v} ngoài [0,1]");
                }
        }

        [Test]
        public void BiomeNoiseValue_WarpZero_MatchesLegacyFormula()
        {
            // Backward compat: warp=0 phải trả exact công thức Perlin trước R8 (đã xài trong
            // BiomeSelectionTests cũ). Nếu test này fail = code đã đổi base formula → save game
            // cũ load lại sẽ map khác biome trên cùng seed.
            wg.biomeBoundaryWarp = 0f;
            for (int y = 0; y < 16; y++)
                for (int x = 0; x < 16; x++)
                {
                    float legacy = Mathf.PerlinNoise((x + wg.seed * 0.7f) * wg.biomeNoiseScale,
                                                      (y - wg.seed * 0.7f) * wg.biomeNoiseScale);
                    float actual = wg.BiomeNoiseValue(x, y);
                    Assert.AreEqual(legacy, actual, 1e-6f,
                        $"Warp=0 phải match legacy formula tại ({x},{y})");
                }
        }

        [Test]
        public void BiomeNoiseValue_WarpPositive_DiffersFromZero_OnMajorityOfCells()
        {
            // Warp > 0 phải thực sự perturb noise. Đo: trên 32×32 grid, bao nhiêu % cell
            // có warp_v != zero_v (epsilon 1e-4)? Target > 80% (warp ảnh hưởng hầu như
            // toàn bộ grid; chỉ vài cell vô tình collide).
            int differ = 0;
            int total = 0;
            for (int y = 0; y < 32; y++)
                for (int x = 0; x < 32; x++)
                {
                    wg.biomeBoundaryWarp = 0f;
                    float zero = wg.BiomeNoiseValue(x, y);
                    wg.biomeBoundaryWarp = 6f;
                    float warped = wg.BiomeNoiseValue(x, y);
                    if (Mathf.Abs(warped - zero) > 1e-4f) differ++;
                    total++;
                }
            float ratio = (float)differ / total;
            Assert.That(ratio, Is.GreaterThan(0.80f),
                $"Warp=6 phải khác warp=0 trên >80% cells (actual {ratio:F3})");
        }
    }
}
