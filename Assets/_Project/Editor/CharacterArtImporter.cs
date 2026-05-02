#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using WildernessCultivation.Core;

namespace WildernessCultivation.EditorTools
{
    /// <summary>
    /// Auto-import body-part PNG ở <c>Assets/_Project/Art/Characters/{characterId}/</c> làm
    /// puppet sprite map cho character (Player / Wolf / FoxSpirit / Boss).
    ///
    /// Hai folder layout:
    /// 1. **Flat (legacy, PR G/H/I side-only):** <c>{id}/head.png</c>, <c>{id}/torso.png</c>...
    ///    → load vào <see cref="PuppetDirection.East"/> only. PuppetAnimController flipX khi
    ///    velocity.x &lt; 0. Backward compat với art user đã gen từ PR G.
    /// 2. **Directional (L3+, PR J):** <c>{id}/E/head.png</c>, <c>{id}/N/head.png</c>,
    ///    <c>{id}/S/head.png</c>... → load full multi-dir map, PuppetAnimController swap
    ///    sprite refs theo velocity.
    ///
    /// Detection: nếu có ≥ 1 subfolder match dir name (E/N/S, case-insensitive) → directional.
    /// Else → flat. Khi flat thì West = flip(East), không cần W subfolder.
    ///
    /// Pipeline reuse pattern của <see cref="ResourceArtImporter"/>.
    /// </summary>
    public static class CharacterArtImporter
    {
        /// <summary>
        /// Resulting sprite map indexed by direction → role → sprite. Empty dirs = direction
        /// chưa gen art (caller fallback East với flipX).
        /// </summary>
        public class CharacterSpriteSet
        {
            public Dictionary<CharacterArtSpec.PuppetDirection,
                Dictionary<CharacterArtSpec.PuppetRole, Sprite>> spritesByDir =
                new Dictionary<CharacterArtSpec.PuppetDirection,
                    Dictionary<CharacterArtSpec.PuppetRole, Sprite>>();

            public bool isMultiDirectional;

            /// <summary>Convenience accessor cho direction East (luôn có khi puppet valid).</summary>
            public Dictionary<CharacterArtSpec.PuppetRole, Sprite> EastSprites
            {
                get
                {
                    spritesByDir.TryGetValue(CharacterArtSpec.PuppetDirection.East, out var dict);
                    return dict;
                }
            }

            /// <summary>Total sprite count across all dirs (for logging).</summary>
            public int TotalSprites
            {
                get
                {
                    int total = 0;
                    foreach (var kv in spritesByDir) total += kv.Value.Count;
                    return total;
                }
            }
        }

        /// <summary>
        /// Scan <c>Art/Characters/{characterId}/</c> → load all recognized PNG.
        /// Returns null nếu folder empty hoặc không có required parts (Head + Torso) ở East.
        /// </summary>
        public static CharacterSpriteSet TryLoadCharacterSpriteSet(
            string characterId, int placeholderHeightPx)
        {
            string folder = $"{CharacterArtSpec.ArtCharactersRoot}/{characterId}";
            if (!AssetDatabase.IsValidFolder(folder)) return null;

            // Detect layout: nếu có subfolder E/N/S → directional. Else flat.
            var subFolders = AssetDatabase.GetSubFolders(folder);
            var dirSubfolders = new Dictionary<CharacterArtSpec.PuppetDirection, string>();
            foreach (var sub in subFolders)
            {
                string subName = Path.GetFileName(sub);
                var dir = CharacterArtSpec.TryParseDirection(subName, out bool ok);
                if (ok) dirSubfolders[dir] = sub;
            }

            var result = new CharacterSpriteSet
            {
                isMultiDirectional = dirSubfolders.Count > 0,
            };

            if (dirSubfolders.Count > 0)
            {
                // Directional layout — load each dir folder.
                foreach (var kv in dirSubfolders)
                {
                    var dirSprites = LoadSpritesFromFolder(kv.Value, placeholderHeightPx);
                    if (dirSprites.Count > 0) result.spritesByDir[kv.Key] = dirSprites;
                }
            }
            else
            {
                // Flat layout — all sprites go to East.
                var flatSprites = LoadSpritesFromFolder(folder, placeholderHeightPx);
                if (flatSprites.Count > 0)
                    result.spritesByDir[CharacterArtSpec.PuppetDirection.East] = flatSprites;
            }

            // Validate: East phải có Head + Torso (other dirs optional).
            var east = result.EastSprites;
            if (east == null ||
                !east.ContainsKey(CharacterArtSpec.PuppetRole.Head) ||
                !east.ContainsKey(CharacterArtSpec.PuppetRole.Torso))
            {
                return null;
            }
            return result;
        }

        /// <summary>
        /// Load PNG files trực tiếp trong folder (KHÔNG recurse subfolders) → role-keyed dict.
        /// Caller decide layout (flat vs per-dir).
        /// </summary>
        static Dictionary<CharacterArtSpec.PuppetRole, Sprite> LoadSpritesFromFolder(
            string folder, int placeholderHeightPx)
        {
            var result = new Dictionary<CharacterArtSpec.PuppetRole, Sprite>();
            string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { folder });
            if (guids == null || guids.Length == 0) return result;

            // Sort path alphabetical → variant pick deterministic giữa OS / re-import.
            System.Array.Sort(guids, (a, b) =>
                string.Compare(AssetDatabase.GUIDToAssetPath(a), AssetDatabase.GUIDToAssetPath(b),
                    System.StringComparison.Ordinal));

            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!IsImageFile(path)) continue;

                // Skip files trong subfolders sâu hơn — chỉ load top-level của folder hiện tại.
                string parentFolder = Path.GetDirectoryName(path)?.Replace('\\', '/');
                if (parentFolder != folder) continue;

                string filename = Path.GetFileNameWithoutExtension(path);
                var role = CharacterArtSpec.TryParseRole(filename);
                if (role == CharacterArtSpec.PuppetRole.Unknown) continue;
                if (result.ContainsKey(role)) continue; // first-write wins (alphabetical preferred).

                var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                if (tex == null) continue;

                // Per-role auto-PPU: giữ world size mỗi part bằng placeholder rectangle. Dùng
                // RectFor(role).h × PuppetPlaceholderPPU làm target world height per role
                // (head 40/64 = 0.625u, torso 80/64 = 1.25u, leg 60/64 = 0.94u…). Trước đây
                // importer pass placeholderHeightPx duy nhất cho mọi part → head/torso/arm
                // render cùng world size 1u → tỷ lệ character vỡ. Now mỗi part match placeholder.
                int rolePlaceholderH = PuppetPlaceholderSpec.RectFor(role).h;
                float ppu = tex.height * PuppetPlaceholderSpec.PuppetPlaceholderPPU / rolePlaceholderH;
                // Per-role joint pivot — head bottom-center, arm/leg top-center, etc. Khi
                // BootstrapWizard.BuildPuppetHierarchy đặt arm tại shoulder world position, top-pivot
                // làm sprite hang DOWN từ shoulder (correct anatomy). PR #118 importer dùng default
                // pivot=Center → arm sprite center ở shoulder, half-arm trồi lên trên shoulder = sai.
                Vector2 pivot = PuppetPlaceholderSpec.PivotFor(role);
                ResourceArtImporter.ApplySpriteImportSettings(path, ppu, pivot);

                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (sprite != null) result[role] = sprite;
            }
            return result;
        }

        /// <summary>
        /// Backward-compat wrapper: returns East-only dict (PR G/H/I callers chưa migrate).
        /// New code nên call <see cref="TryLoadCharacterSpriteSet"/> để có full multi-dir.
        /// </summary>
        public static Dictionary<CharacterArtSpec.PuppetRole, Sprite> TryLoadCharacterSprites(
            string characterId, int placeholderHeightPx)
        {
            var set = TryLoadCharacterSpriteSet(characterId, placeholderHeightPx);
            return set?.EastSprites;
        }

        static bool IsImageFile(string path)
        {
            return path.EndsWith(".png", System.StringComparison.OrdinalIgnoreCase) ||
                   path.EndsWith(".jpg", System.StringComparison.OrdinalIgnoreCase) ||
                   path.EndsWith(".jpeg", System.StringComparison.OrdinalIgnoreCase);
        }

        [MenuItem("Tools/Wilderness Cultivation/Import Character Art")]
        public static void MenuImportAll()
        {
            string root = CharacterArtSpec.ArtCharactersRoot;
            if (!AssetDatabase.IsValidFolder(root))
            {
                Debug.LogWarning($"CharacterArt: folder {root} chưa tồn tại — tạo trước rồi drop PNG.");
                return;
            }

            var subFolders = AssetDatabase.GetSubFolders(root);
            int totalLoaded = 0;
            foreach (var sub in subFolders)
            {
                string id = Path.GetFileName(sub);
                var set = TryLoadCharacterSpriteSet(id, placeholderHeightPx: 32);
                if (set != null)
                {
                    string mode = set.isMultiDirectional ? "multi-dir" : "flat (side-only)";
                    Debug.Log($"CharacterArt: {id} → {set.TotalSprites} parts loaded ({mode}, {set.spritesByDir.Count} direction(s)).");
                    totalLoaded++;
                }
            }
            Debug.Log($"CharacterArt: {totalLoaded} character(s) ready.");
        }
    }
}
#endif
