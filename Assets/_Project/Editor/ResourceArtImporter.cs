#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using WildernessCultivation.Core;

namespace WildernessCultivation.EditorTools
{
    /// <summary>
    /// Auto-import sprite PNG ở Assets/_Project/Art/Resources/{id}/ làm sprite cho resource
    /// prefab (tree / rock / grass / herb / flora / water / structures).
    ///
    /// Workflow:
    /// 1. Artist drop *.png vào Assets/_Project/Art/Resources/{id}/.
    /// 2. Run menu Tools > Wilderness Cultivation > Import Resource Art (hoặc auto qua Bootstrap).
    /// 3. Importer scan folder, pick first PNG (alphabetical sort), apply settings + auto-PPU.
    /// 4. BootstrapWizard.CreateSprites dùng sprite này thay placeholder.
    ///
    /// Fallback chain:
    ///   Art/Resources/{id}/*.png  →  Sprites/{id}.png (gen_sprites.py)  →  procedural color rect.
    /// </summary>
    public static class ResourceArtImporter
    {
        /// <summary>
        /// Scan Art/Resources/{id}/ → return first PNG path (alphabetical sort), null nếu empty.
        /// </summary>
        public static string PickFirstSpritePath(string id)
        {
            string folder = $"{ResourceArtSpec.ArtResourcesRoot}/{id}";
            if (!AssetDatabase.IsValidFolder(folder)) return null;

            string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { folder });
            if (guids == null || guids.Length == 0) return null;

            // Sort path alphabetical → variant pick deterministic giữa OS / re-import.
            System.Array.Sort(guids, (a, b) =>
                string.Compare(AssetDatabase.GUIDToAssetPath(a), AssetDatabase.GUIDToAssetPath(b),
                    System.StringComparison.Ordinal));

            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.EndsWith(".png", System.StringComparison.OrdinalIgnoreCase) ||
                    path.EndsWith(".jpg", System.StringComparison.OrdinalIgnoreCase) ||
                    path.EndsWith(".jpeg", System.StringComparison.OrdinalIgnoreCase))
                    return path;
            }
            return null;
        }

        /// <summary>
        /// Apply texture import settings cho user-provided sprite. Bilinear filter (cao-res
        /// art mượt hơn Point), alphaIsTransparency, no mipmap, clamp wrap.
        /// <paramref name="pivotNormalized"/> optional: nếu set, sprite import với
        /// SpriteAlignment.Custom + custom pivot (dùng cho puppet body parts với joint-anchor pivot
        /// — head bottom-center, arm/leg top-center, etc.). Mặc định null = giữ alignment hiện tại
        /// của importer (Center Unity default cho legacy resource art).
        /// </summary>
        public static void ApplySpriteImportSettings(
            string spriteAssetPath, float ppu, Vector2? pivotNormalized = null)
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
            if (Mathf.Abs(importer.spritePixelsPerUnit - ppu) > 0.01f)
            {
                importer.spritePixelsPerUnit = ppu;
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

            // Sprite pivot — TextureImporter only exposes spriteAlignment + spritePivot via
            // TextureImporterSettings (read-modify-write). Skip nếu caller không yêu cầu pivot.
            if (pivotNormalized.HasValue)
            {
                var settings = new TextureImporterSettings();
                importer.ReadTextureSettings(settings);
                bool pivotDirty = false;
                if (settings.spriteAlignment != (int)SpriteAlignment.Custom)
                {
                    settings.spriteAlignment = (int)SpriteAlignment.Custom;
                    pivotDirty = true;
                }
                if ((settings.spritePivot - pivotNormalized.Value).sqrMagnitude > 0.0001f)
                {
                    settings.spritePivot = pivotNormalized.Value;
                    pivotDirty = true;
                }
                if (pivotDirty)
                {
                    importer.SetTextureSettings(settings);
                    dirty = true;
                }
            }

            if (dirty) importer.SaveAndReimport();
        }

        /// <summary>
        /// Try load user-provided sprite cho resource id. Return null nếu folder empty —
        /// caller fallback dùng Sprites/{id}.png hoặc procedural placeholder.
        /// placeholderHeightPx dùng để auto-tính PPU (giữ world size đồng nhất với placeholder).
        /// </summary>
        public static Sprite TryLoadSprite(string id, int placeholderHeightPx)
        {
            string path = PickFirstSpritePath(id);
            if (path == null) return null;

            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (tex == null) return null;

            float ppu = ResourceArtSpec.ComputeAutoPPU(tex.height, placeholderHeightPx);
            ApplySpriteImportSettings(path, ppu);

            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        /// <summary>
        /// Menu entry độc lập — quét tất cả known resource ids, import + log summary.
        /// Hữu ích khi artist drop sprite mới mà không muốn chạy full Bootstrap.
        /// </summary>
        [MenuItem("Tools/Wilderness Cultivation/Import Resource Art")]
        public static void ImportAllResourceArtMenu()
        {
            var summary = new List<string>(KnownResourceIds.Count);
            int imported = 0, skipped = 0;
            foreach (var entry in KnownResourceIds)
            {
                var sp = TryLoadSprite(entry.id, entry.placeholderH);
                if (sp != null)
                {
                    imported++;
                    summary.Add($"  - {entry.id}: imported '{sp.name}' (PPU {sp.pixelsPerUnit:F0})");
                }
                else
                {
                    skipped++;
                    summary.Add($"  - {entry.id}: skipped (no PNG ở Art/Resources/{entry.id}/)");
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "Import Resource Art",
                $"Imported: {imported}\nSkipped: {skipped}\n\n{string.Join("\n", summary)}",
                "OK");
        }

        /// <summary>
        /// Resource id + placeholder height (px) — dùng để compute auto PPU.
        /// Khớp BootstrapWizard.CreateSprites defs (giữ same world scale).
        /// </summary>
        public readonly struct ResourceEntry
        {
            public readonly string id;
            public readonly int placeholderH;
            public ResourceEntry(string id, int placeholderH)
            {
                this.id = id;
                this.placeholderH = placeholderH;
            }
        }

        // Khớp BootstrapWizard.CreateSprites defs (id + height px). Folder name PHẢI khớp
        // sprite id để BootstrapWizard look up đúng. Mineral rock + water spring chia sẻ
        // sprite "rock" / "water" với resource thường nên KHÔNG có folder riêng.
        public static readonly IReadOnlyList<ResourceEntry> KnownResourceIds = new[]
        {
            new ResourceEntry("tree",          48),
            new ResourceEntry("rock",          24),
            new ResourceEntry("water",         40),
            new ResourceEntry("linh_mushroom", 24),
            new ResourceEntry("berry_bush",    22),
            new ResourceEntry("cactus",        32),
            new ResourceEntry("death_lily",    28),
            new ResourceEntry("linh_bamboo",   40),
        };
    }
}
#endif
