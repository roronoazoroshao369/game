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
    /// Workflow:
    /// 1. Artist gen 5-7 PNG riêng từng part (head.png, torso.png, arm_left.png, ...).
    /// 2. Drop vào folder <c>{characterId}/</c>.
    /// 3. BootstrapWizard call <see cref="TryLoadCharacterSprites"/> → return Dictionary&lt;role, Sprite&gt;.
    ///    Nếu found ≥ 2 required parts (Head + Torso) → build puppet hierarchy.
    ///    Nếu thiếu → fallback single-sprite placeholder.
    ///
    /// Filename → role mapping qua <see cref="CharacterArtSpec.TryParseRole"/>.
    /// </summary>
    public static class CharacterArtImporter
    {
        /// <summary>
        /// Scan <c>Art/Characters/{characterId}/</c> → load all recognized PNG into dict by role.
        /// Returns null nếu folder empty hoặc không có required parts (Head + Torso).
        /// </summary>
        public static Dictionary<CharacterArtSpec.PuppetRole, Sprite> TryLoadCharacterSprites(
            string characterId, int placeholderHeightPx)
        {
            string folder = $"{CharacterArtSpec.ArtCharactersRoot}/{characterId}";
            if (!AssetDatabase.IsValidFolder(folder)) return null;

            string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { folder });
            if (guids == null || guids.Length == 0) return null;

            // Sort path alphabetical → variant pick deterministic giữa OS / re-import.
            System.Array.Sort(guids, (a, b) =>
                string.Compare(AssetDatabase.GUIDToAssetPath(a), AssetDatabase.GUIDToAssetPath(b),
                    System.StringComparison.Ordinal));

            var result = new Dictionary<CharacterArtSpec.PuppetRole, Sprite>();
            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!IsImageFile(path)) continue;

                string filename = Path.GetFileNameWithoutExtension(path);
                var role = CharacterArtSpec.TryParseRole(filename);
                if (role == CharacterArtSpec.PuppetRole.Unknown) continue;
                if (result.ContainsKey(role)) continue; // first-write wins (alphabetical preferred).

                var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                if (tex == null) continue;

                float ppu = ResourceArtSpec.ComputeAutoPPU(tex.height, placeholderHeightPx);
                ResourceArtImporter.ApplySpriteImportSettings(path, ppu);

                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (sprite != null) result[role] = sprite;
            }

            // Bắt buộc tối thiểu Head + Torso để build puppet (arms/legs/tail optional).
            if (!result.ContainsKey(CharacterArtSpec.PuppetRole.Head) ||
                !result.ContainsKey(CharacterArtSpec.PuppetRole.Torso))
            {
                return null;
            }

            return result;
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
                var sprites = TryLoadCharacterSprites(id, placeholderHeightPx: 32);
                if (sprites != null)
                {
                    Debug.Log($"CharacterArt: {id} → {sprites.Count} parts loaded.");
                    totalLoaded++;
                }
            }
            Debug.Log($"CharacterArt: {totalLoaded} character(s) ready.");
        }
    }
}
#endif
