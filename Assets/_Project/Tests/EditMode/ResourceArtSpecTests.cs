using NUnit.Framework;
using WildernessCultivation.Core;

namespace WildernessCultivation.Tests.EditMode
{
    /// <summary>
    /// Verify ResourceArtSpec.ComputeAutoPPU formula. World size invariant: user PNG render
    /// đúng cùng world size với placeholder bất kể source resolution. Editor-only AssetDatabase
    /// path (PickFirstSpritePath, ApplySpriteImportSettings, TryLoadSprite) không test ở đây
    /// — theo precedent của BiomeTileImporter (cũng chưa có test).
    /// </summary>
    public class ResourceArtSpecTests
    {
        [Test]
        public void ComputeAutoPPU_SourceMatchesPlaceholder_ReturnsBasePPU()
        {
            float ppu = ResourceArtSpec.ComputeAutoPPU(48, 48);
            Assert.AreEqual(ResourceArtSpec.PlaceholderPPU, ppu, 0.01f);
        }

        [Test]
        public void ComputeAutoPPU_HiResSource_ScalesProportionally()
        {
            // Tree placeholder 48px → 1.5 world unit cao @ PPU=32. User PNG 1536px (32×) →
            // PPU phải = 32 × 32 = 1024 để giữ 1.5 world unit (1536 / 1024 = 1.5).
            float ppu = ResourceArtSpec.ComputeAutoPPU(1536, 48);
            Assert.AreEqual(1024f, ppu, 0.01f);
        }

        [Test]
        public void ComputeAutoPPU_LoResSource_ScalesProportionally()
        {
            // Source 24px ở placeholder 48px → world size = 24/16 = 1.5 (giữ scale với 48px@PPU=32).
            float ppu = ResourceArtSpec.ComputeAutoPPU(24, 48);
            Assert.AreEqual(16f, ppu, 0.01f);
        }

        [Test]
        public void ComputeAutoPPU_ZeroOrNegativeInput_ReturnsBasePPU()
        {
            // Defensive: invalid input không crash + fallback PlaceholderPPU.
            Assert.AreEqual(ResourceArtSpec.PlaceholderPPU, ResourceArtSpec.ComputeAutoPPU(0, 48), 0.01f);
            Assert.AreEqual(ResourceArtSpec.PlaceholderPPU, ResourceArtSpec.ComputeAutoPPU(48, 0), 0.01f);
            Assert.AreEqual(ResourceArtSpec.PlaceholderPPU, ResourceArtSpec.ComputeAutoPPU(-100, 48), 0.01f);
            Assert.AreEqual(ResourceArtSpec.PlaceholderPPU, ResourceArtSpec.ComputeAutoPPU(48, -100), 0.01f);
        }

        [Test]
        public void ArtResourcesRoot_PointsToArtResourcesFolder()
        {
            // Sanity: const phải khớp folder thực (Art/Resources/), tránh typo silent break
            // BootstrapWizard look up.
            Assert.AreEqual("Assets/_Project/Art/Resources", ResourceArtSpec.ArtResourcesRoot);
        }
    }
}
