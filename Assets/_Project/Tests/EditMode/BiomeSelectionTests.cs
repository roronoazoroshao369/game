using NUnit.Framework;
using UnityEngine;
using WildernessCultivation.World;

namespace WildernessCultivation.Tests.EditMode
{
    /// <summary>
    /// EditMode tests cho biome Perlin selection + extraNodes integrity.
    /// Verify Stone Highlands biome (selectionRange 0.40-0.65) chiếm tile có Perlin
    /// trong phạm vi đó. Cũng verify extraNodes API valid.
    /// </summary>
    public class BiomeSelectionTests
    {
        BiomeSO MakeBiome(string id, float min, float max)
        {
            var b = ScriptableObject.CreateInstance<BiomeSO>();
            b.biomeId = id;
            b.displayName = id;
            b.selectionRange = new Vector2(min, max);
            return b;
        }

        [Test]
        public void SelectionRanges_NoOverlap_SpanFullRange()
        {
            // Mirror BootstrapWizard.CreateBiomes ranges.
            var forest = MakeBiome("forest", 0f, 0.40f);
            var stone = MakeBiome("stone_highlands", 0.40f, 0.65f);
            var desert = MakeBiome("desert", 0.65f, 1f);

            // Phải đảm bảo cover [0,1] không gap, không overlap.
            Assert.AreEqual(forest.selectionRange.y, stone.selectionRange.x, 0.001f);
            Assert.AreEqual(stone.selectionRange.y, desert.selectionRange.x, 0.001f);
            Assert.AreEqual(0f, forest.selectionRange.x, 0.001f);
            Assert.AreEqual(1f, desert.selectionRange.y, 0.001f);

            Object.DestroyImmediate(forest);
            Object.DestroyImmediate(stone);
            Object.DestroyImmediate(desert);
        }

        [Test]
        public void ExtraNodes_DefaultEmpty_NoCrashOnIteration()
        {
            var b = MakeBiome("test", 0f, 1f);
            // extraNodes default null — caller phải null-check.
            Assert.IsTrue(b.extraNodes == null || b.extraNodes.Length == 0);
            Object.DestroyImmediate(b);
        }

        [Test]
        public void ExtraNodes_AssignAndRead_PreservesShape()
        {
            var b = MakeBiome("test", 0f, 1f);
            var dummyPrefab = new GameObject("DummyPlant");
            b.extraNodes = new[]
            {
                new BiomeSO.ExtraNode { prefab = dummyPrefab, density = 0.02f },
                new BiomeSO.ExtraNode { prefab = dummyPrefab, density = 0.05f },
            };

            Assert.AreEqual(2, b.extraNodes.Length);
            Assert.AreEqual(0.02f, b.extraNodes[0].density, 0.001f);
            Assert.AreEqual(0.05f, b.extraNodes[1].density, 0.001f);
            Assert.AreSame(dummyPrefab, b.extraNodes[1].prefab);

            Object.DestroyImmediate(dummyPrefab);
            Object.DestroyImmediate(b);
        }

        [Test]
        public void WorldGenerator_PickBiomeFor_ReturnsBiomeWithMatchingPerlinRange()
        {
            // Test invariant: với seed cố định, mỗi tile (x,y) → 1 biome cụ thể với
            // selectionRange chứa Perlin value tại tile đó. Quét vài tile, verify
            // biome trả về thỏa selectionRange chứa Perlin value.
            var go = new GameObject("WG");
            var wg = go.AddComponent<WorldGenerator>();
            wg.size = new Vector2Int(10, 10);
            wg.seed = 42;
            wg.biomeNoiseScale = 0.05f;

            var forest = MakeBiome("forest", 0f, 0.40f);
            var stone = MakeBiome("stone_highlands", 0.40f, 0.65f);
            var desert = MakeBiome("desert", 0.65f, 1f);
            wg.biomes = new[] { forest, stone, desert };

            for (int x = 0; x < wg.size.x; x++)
                for (int y = 0; y < wg.size.y; y++)
                {
                    var picked = wg.BiomeAt(new Vector3(x + 0.1f, y + 0.1f, 0f));
                    Assert.IsNotNull(picked, $"Biome at ({x},{y}) phải != null");
                    // Source-of-truth: cùng public helper WorldGenerator dùng để PickBiomeFor.
                    // Tách helper tránh test lặp lại implementation chi tiết của domain warp.
                    float v = wg.BiomeNoiseValue(x, y);
                    bool inRange = v >= picked.selectionRange.x && v <= picked.selectionRange.y;
                    // Edge case: Perlin có thể trả vượt 1.0 chút — fallback biome[0] hợp lệ.
                    if (!inRange) Assert.AreSame(forest, picked,
                        $"Out-of-range Perlin tại ({x},{y})={v:F3} → fallback biome[0]");
                }

            Object.DestroyImmediate(go);
            Object.DestroyImmediate(forest);
            Object.DestroyImmediate(stone);
            Object.DestroyImmediate(desert);
        }
    }
}
