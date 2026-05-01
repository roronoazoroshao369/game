namespace WildernessCultivation.Core
{
    /// <summary>
    /// Pure spec cho user-provided resource art (sprite drop ở Art/Resources/{id}/).
    /// Editor-only importer (`WildernessCultivation.EditorTools.ResourceArtImporter`) đọc các
    /// constant + math function ở đây — runtime-accessible để EditMode test reach trực tiếp
    /// không cần asmdef refactor.
    /// </summary>
    public static class ResourceArtSpec
    {
        public const string ArtResourcesRoot = "Assets/_Project/Art/Resources";

        // Placeholder PPU (BootstrapWizard.CreateSprites apply 32 cho tất cả sprite tự sinh).
        // Auto PPU formula giữ world size đồng nhất giữa user PNG và placeholder.
        public const float PlaceholderPPU = 32f;

        /// <summary>
        /// PPU sao cho user PNG render world size bằng placeholder. Vd placeholder tree
        /// 32×48 (PPU=32) → world 1.5u cao; user PNG 1024×1536 → PPU=1024 → world 1.5u cao
        /// (giữ scale, chỉ đẹp hơn). Defensive return PlaceholderPPU khi input invalid.
        /// </summary>
        public static float ComputeAutoPPU(int sourceHeightPx, int placeholderHeightPx)
        {
            if (sourceHeightPx <= 0 || placeholderHeightPx <= 0) return PlaceholderPPU;
            return sourceHeightPx * PlaceholderPPU / placeholderHeightPx;
        }
    }
}
