#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using WildernessCultivation.World;

namespace WildernessCultivation.EditorTools
{
    /// <summary>
    /// Auto-import sprite PNG ở Assets/_Project/Art/Tiles/{biomeId}/ thành Tile asset
    /// và wire vào BiomeSO.groundTileVariants[]. Fallback: nếu folder empty, biome giữ
    /// nguyên placeholder groundTile.
    ///
    /// Workflow:
    /// 1. Artist (hoặc Leonardo output) drop *.png vào Assets/_Project/Art/Tiles/{biomeId}/.
    /// 2. Chạy menu Tools > Wilderness Cultivation > Import Biome Tiles (hoặc auto qua Bootstrap).
    /// 3. Importer apply settings (PPU=64, Bilinear, transparent BG), tạo Tile asset ở
    ///    Assets/_Project/SOs/Tiles/Tile_{biomeId}_{filename}.asset, wire variant.
    /// 4. WorldGenerator.PickGroundTile() pick deterministic per cell qua hash(seed,x,y).
    /// </summary>
    public static class BiomeTileImporter
    {
        const string ArtTilesRoot = "Assets/_Project/Art/Tiles";
        const string TileSOsDir = "Assets/_Project/SOs/Tiles";

        /// <summary>Default per-sprite import settings cho ground tile (PPU=64, painterly Bilinear).</summary>
        public static void ApplyTileImportSettings(string spriteAssetPath)
        {
            var importer = AssetImporter.GetAtPath(spriteAssetPath) as TextureImporter;
            if (importer == null) return;

            bool dirty = false;
            if (importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                dirty = true;
            }
            if (importer.spriteImportMode != SpriteImportMode.Single)
            {
                importer.spriteImportMode = SpriteImportMode.Single;
                dirty = true;
            }
            if (Mathf.Abs(importer.spritePixelsPerUnit - 64f) > 0.01f)
            {
                importer.spritePixelsPerUnit = 64f;
                dirty = true;
            }
            if (importer.filterMode != FilterMode.Bilinear)
            {
                importer.filterMode = FilterMode.Bilinear;
                dirty = true;
            }
            if (importer.mipmapEnabled)
            {
                importer.mipmapEnabled = false;
                dirty = true;
            }
            if (importer.alphaIsTransparency != true)
            {
                importer.alphaIsTransparency = true;
                dirty = true;
            }
            if (importer.wrapMode != TextureWrapMode.Clamp)
            {
                importer.wrapMode = TextureWrapMode.Clamp;
                dirty = true;
            }
            if (dirty)
            {
                importer.SaveAndReimport();
            }
        }

        /// <summary>
        /// Quét folder Art/Tiles/{biomeId}/ → return list TileBase đã tạo. Folder không tồn tại
        /// hoặc empty → return empty array (caller fallback dùng placeholder groundTile).
        /// </summary>
        public static TileBase[] ImportBiomeTiles(string biomeId)
        {
            string folder = $"{ArtTilesRoot}/{biomeId}";
            if (!AssetDatabase.IsValidFolder(folder)) return System.Array.Empty<TileBase>();

            EnsureTileSOsDir();

            string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { folder });
            if (guids == null || guids.Length == 0) return System.Array.Empty<TileBase>();

            var tiles = new List<TileBase>(guids.Length);
            // Sort path để variant order deterministic (file system order khác nhau giữa OS).
            System.Array.Sort(guids, (a, b) =>
                string.Compare(AssetDatabase.GUIDToAssetPath(a), AssetDatabase.GUIDToAssetPath(b),
                    System.StringComparison.Ordinal));

            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!path.EndsWith(".png", System.StringComparison.OrdinalIgnoreCase) &&
                    !path.EndsWith(".jpg", System.StringComparison.OrdinalIgnoreCase) &&
                    !path.EndsWith(".jpeg", System.StringComparison.OrdinalIgnoreCase))
                    continue;

                ApplyTileImportSettings(path);

                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (sprite == null) continue;

                string baseName = Path.GetFileNameWithoutExtension(path);
                string tilePath = $"{TileSOsDir}/Tile_{biomeId}_{baseName}.asset";
                var tile = AssetDatabase.LoadAssetAtPath<Tile>(tilePath);
                if (tile == null)
                {
                    tile = ScriptableObject.CreateInstance<Tile>();
                    AssetDatabase.CreateAsset(tile, tilePath);
                }
                tile.sprite = sprite;
                tile.colliderType = Tile.ColliderType.None;
                EditorUtility.SetDirty(tile);
                tiles.Add(tile);
            }

            return tiles.ToArray();
        }

        /// <summary>
        /// Wire variant array vào BiomeSO. Empty input → KHÔNG clear field (bảo vệ wire thủ công
        /// ngoài Inspector). Caller tự quyết fallback path.
        /// </summary>
        public static bool WireVariantsToBiome(BiomeSO biome, TileBase[] variants)
        {
            if (biome == null || variants == null || variants.Length == 0) return false;
            biome.groundTileVariants = variants;
            EditorUtility.SetDirty(biome);
            return true;
        }

        /// <summary>
        /// Menu entry độc lập — quét tất cả sub-folder Art/Tiles/, import + wire vào BiomeSO
        /// match biomeId. Hữu ích khi artist drop sprite mới mà không muốn chạy full Bootstrap.
        /// </summary>
        [MenuItem("Tools/Wilderness Cultivation/Import Biome Tiles")]
        public static void ImportAllBiomeTilesMenu()
        {
            EnsureTileSOsDir();

            int wired = 0;
            int skipped = 0;
            var sb = new System.Text.StringBuilder();

            string[] biomeGuids = AssetDatabase.FindAssets("t:" + nameof(BiomeSO));
            foreach (var biomeGuid in biomeGuids)
            {
                string biomePath = AssetDatabase.GUIDToAssetPath(biomeGuid);
                var biome = AssetDatabase.LoadAssetAtPath<BiomeSO>(biomePath);
                if (biome == null || string.IsNullOrEmpty(biome.biomeId)) continue;

                var variants = ImportBiomeTiles(biome.biomeId);
                if (variants.Length > 0)
                {
                    WireVariantsToBiome(biome, variants);
                    wired++;
                    sb.AppendLine($"  - {biome.biomeId}: wired {variants.Length} variant");
                }
                else
                {
                    skipped++;
                    sb.AppendLine($"  - {biome.biomeId}: skipped (no PNG ở Art/Tiles/{biome.biomeId}/)");
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "Import Biome Tiles",
                $"Wired: {wired} biome\nSkipped: {skipped} biome\n\n{sb}",
                "OK");
        }

        static void EnsureTileSOsDir()
        {
            if (!AssetDatabase.IsValidFolder(TileSOsDir))
            {
                Directory.CreateDirectory(TileSOsDir);
                AssetDatabase.Refresh();
                if (!AssetDatabase.IsValidFolder(TileSOsDir))
                {
                    string parent = Path.GetDirectoryName(TileSOsDir).Replace('\\', '/');
                    string leaf = Path.GetFileName(TileSOsDir);
                    AssetDatabase.CreateFolder(parent, leaf);
                }
            }
        }
    }
}
#endif
