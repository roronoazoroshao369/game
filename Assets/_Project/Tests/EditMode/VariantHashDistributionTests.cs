using NUnit.Framework;
using UnityEngine;
using UnityEngine.Tilemaps;
using WildernessCultivation.World;

namespace WildernessCultivation.Tests.EditMode
{
    /// <summary>
    /// Regression tests cho <see cref="WorldGenerator.VariantHash"/> + <see cref="WorldGenerator.PickGroundTile"/>.
    ///
    /// Bug trước fix: hash `seed*A ^ x*B ^ y*C` với 3 multiplier đều odd → LSB output =
    /// (x+y) parity → `idx % N` (N nhỏ) alias với checkerboard, các đường chéo nhận cùng
    /// variant. 4 variants brightness hơi khác → sọc chéo lặp đều = "ô caro" trên screenshot.
    ///
    /// Test verify 3 invariant:
    ///  1. Determinism: cùng (seed,x,y) → cùng index across calls.
    ///  2. Distribution uniform trên grid lớn (mỗi variant ~ 1/N total cells, ±10%).
    ///  3. Không có parity correlation (parity của idx ≠ (x+y)%2 trong ~50% case).
    /// </summary>
    public class VariantHashDistributionTests
    {
        const int Seed = 12345;
        const int GridSize = 64; // 4096 samples — đủ statistical significance.
        const int VariantCount = 4;

        [Test]
        public void VariantHash_Deterministic_SameInputSameOutput()
        {
            uint h1 = WorldGenerator.VariantHash(Seed, 7, 11);
            uint h2 = WorldGenerator.VariantHash(Seed, 7, 11);
            Assert.AreEqual(h1, h2, "VariantHash phải deterministic");

            // Khác seed → khác hash (high probability).
            uint h3 = WorldGenerator.VariantHash(Seed + 1, 7, 11);
            Assert.AreNotEqual(h1, h3, "Khác seed phải sinh khác hash");
        }

        [Test]
        public void VariantHash_DistributionUniform_OverGrid()
        {
            int[] counts = new int[VariantCount];
            for (int y = 0; y < GridSize; y++)
                for (int x = 0; x < GridSize; x++)
                {
                    int idx = (int)(WorldGenerator.VariantHash(Seed, x, y) % (uint)VariantCount);
                    counts[idx]++;
                }

            int total = GridSize * GridSize;
            int target = total / VariantCount;
            int tolerance = target / 10; // ±10%
            for (int i = 0; i < VariantCount; i++)
            {
                Assert.That(counts[i], Is.InRange(target - tolerance, target + tolerance),
                    $"Variant {i} count = {counts[i]}, expected ~{target} (±{tolerance})");
            }
        }

        [Test]
        public void VariantHash_NoParityCheckerboard()
        {
            // Bug cũ: idx%2 == (x+y)%2 cho 100% cells (checkerboard hoàn hảo).
            // Sau fix: ~50% (random).
            int parityCorrelated = 0;
            int total = 0;
            for (int y = 0; y < GridSize; y++)
                for (int x = 0; x < GridSize; x++)
                {
                    int idx = (int)(WorldGenerator.VariantHash(Seed, x, y) % (uint)VariantCount);
                    int idxParity = idx & 1;
                    int xyParity = (x + y) & 1;
                    if (idxParity == xyParity) parityCorrelated++;
                    total++;
                }
            float ratio = (float)parityCorrelated / total;
            Assert.That(ratio, Is.InRange(0.40f, 0.60f),
                $"Parity correlation = {ratio:F3}, expected ~0.50 (random). Bug cũ là 1.00 (checkerboard).");
        }

        [Test]
        public void VariantHash_NoDiagonalStripes()
        {
            // Bug cũ: cell (x,y) cùng variant với (x+1,y+1), (x+2,y+2), … (đường chéo same value).
            // Đo: trong 1 đường chéo dài 8 cell, có bao nhiêu % cell cùng variant với cell start.
            // Bug cũ ~ 100%. Sau fix ~ 25% (random).
            int diagonalSame = 0;
            int diagonalTotal = 0;
            for (int startY = 0; startY < GridSize - 8; startY += 8)
                for (int startX = 0; startX < GridSize - 8; startX += 8)
                {
                    int startIdx = (int)(WorldGenerator.VariantHash(Seed, startX, startY) % (uint)VariantCount);
                    for (int k = 1; k < 8; k++)
                    {
                        int idx = (int)(WorldGenerator.VariantHash(Seed, startX + k, startY + k) % (uint)VariantCount);
                        if (idx == startIdx) diagonalSame++;
                        diagonalTotal++;
                    }
                }
            float ratio = (float)diagonalSame / diagonalTotal;
            Assert.That(ratio, Is.LessThan(0.40f),
                $"Diagonal same-variant rate = {ratio:F3}, expected ~0.25 (random). Bug cũ là ~1.00.");
        }

        [Test]
        public void PickGroundTile_UsesVariantHash_SmokeTest()
        {
            // Integration: PickGroundTile phải pick variant từ array dựa trên VariantHash.
            // Tạo BiomeSO với 4 Tile distinct → mỗi variant phải xuất hiện ít nhất 1 lần trên grid.
            var biome = ScriptableObject.CreateInstance<BiomeSO>();
            var tiles = new Tile[VariantCount];
            for (int i = 0; i < VariantCount; i++) tiles[i] = ScriptableObject.CreateInstance<Tile>();
            biome.groundTileVariants = tiles;

            var wgGo = new GameObject("WG");
            try
            {
                var wg = wgGo.AddComponent<WorldGenerator>();
                wg.seed = Seed;

                var seen = new System.Collections.Generic.HashSet<TileBase>();
                for (int y = 0; y < GridSize; y++)
                    for (int x = 0; x < GridSize; x++)
                        seen.Add(wg.PickGroundTile(biome, x, y));

                Assert.AreEqual(VariantCount, seen.Count,
                    "PickGroundTile phải sử dụng cả 4 variants trên grid 64×64");
            }
            finally
            {
                Object.DestroyImmediate(wgGo);
                for (int i = 0; i < tiles.Length; i++)
                    if (tiles[i] != null) Object.DestroyImmediate(tiles[i]);
                Object.DestroyImmediate(biome);
            }
        }
    }
}
