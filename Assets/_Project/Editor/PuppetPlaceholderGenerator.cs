#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using WildernessCultivation.Core;

namespace WildernessCultivation.EditorTools
{
    /// <summary>
    /// PR M (skeleton demo mode): when user chưa drop real PNG vào
    /// <c>Art/Characters/{id}/</c>, BootstrapWizard fallback gọi generator này để sinh 13
    /// colored-rectangle PNG (head / torso / arm L+R / forearm L+R / leg L+R / shin L+R / tail)
    /// vào <c>Sprites/puppet/{id}/</c>. PuppetAnimController dùng những placeholder này để
    /// chạy demo full procedural motion (walk swing + lunge + crouch + 4-dir swap) ngay không
    /// cần asset.
    ///
    /// Real PNG drop sau này → <see cref="CharacterArtImporter.TryLoadCharacterSpriteSet"/>
    /// trả non-null → BootstrapWizard skip placeholder fallback. Idempotent: chạy lại không
    /// overwrite real art.
    ///
    /// Pure-data spec (palette + rect + role) sống ở
    /// <see cref="WildernessCultivation.Core.PuppetPlaceholderSpec"/> — testable từ EditMode
    /// runtime asmdef. File này là Editor-side IO wrapper around spec.
    /// </summary>
    public static class PuppetPlaceholderGenerator
    {
        public const string PlaceholderRoot = "Assets/_Project/Sprites/puppet";

        /// <summary>
        /// Ensure placeholder PNGs ở <c>Sprites/puppet/{characterId}/{role}.png</c>. Idempotent:
        /// nếu file đã tồn tại thì re-import; nếu chưa thì write fresh. Returns dict role → Sprite.
        /// </summary>
        public static Dictionary<CharacterArtSpec.PuppetRole, Sprite> EnsureSpriteSet(
            string characterId, bool includeTail = true)
        {
            string folder = $"{PlaceholderRoot}/{characterId}";
            EnsureFolder(folder);

            var palette = PuppetPlaceholderSpec.PaletteFor(characterId);
            var dict = new Dictionary<CharacterArtSpec.PuppetRole, Sprite>();

            // Materialize IEnumerable một lần — DefaultRoles dùng yield return nên iter
            // 2 lần (write + load) sẽ enum lại; copy vào List ổn định + cho phép Count.
            var roles = new List<CharacterArtSpec.PuppetRole>(
                PuppetPlaceholderSpec.DefaultRoles(includeTail));
            bool wroteAny = false;

            foreach (var role in roles)
            {
                var (w, h) = PuppetPlaceholderSpec.RectFor(role);
                var color = PuppetPlaceholderSpec.ColorFor(role, palette);
                string path = $"{folder}/{RoleToFilename(role)}.png";

                if (!File.Exists(path))
                {
                    WriteRectPng(path, w, h, color);
                    wroteAny = true;
                }
            }

            // Refresh AssetDatabase một lần sau khi đã ghi mọi file mới — Unity cần biết về
            // file mới qua Refresh trước khi GetAtPath trả về importer hợp lệ. Trước fix này
            // first-run gen có thể fail (importer null → textureType giữ Default → LoadAsset
            // trả null → puppet không có sprite → Player invisible).
            if (wroteAny) AssetDatabase.Refresh();

            foreach (var role in roles)
            {
                string path = $"{folder}/{RoleToFilename(role)}.png";
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                ApplySpriteImport(path);
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (sprite != null)
                {
                    dict[role] = sprite;
                }
                else
                {
                    Debug.LogWarning(
                        $"[PuppetPlaceholderGenerator] Failed to load sprite for {characterId}/{role} at {path}. " +
                        "Skeleton sẽ thiếu body part này.");
                }
            }

            Debug.Log(
                $"[PuppetPlaceholderGenerator] {characterId}: loaded {dict.Count}/{roles.Count} placeholder sprites at {folder}/");

            return dict;
        }

        static string RoleToFilename(CharacterArtSpec.PuppetRole role)
        {
            switch (role)
            {
                case CharacterArtSpec.PuppetRole.Head: return CharacterArtSpec.FilenameHead;
                case CharacterArtSpec.PuppetRole.Torso: return CharacterArtSpec.FilenameTorso;
                case CharacterArtSpec.PuppetRole.ArmLeft: return CharacterArtSpec.FilenameArmLeft;
                case CharacterArtSpec.PuppetRole.ArmRight: return CharacterArtSpec.FilenameArmRight;
                case CharacterArtSpec.PuppetRole.LegLeft: return CharacterArtSpec.FilenameLegLeft;
                case CharacterArtSpec.PuppetRole.LegRight: return CharacterArtSpec.FilenameLegRight;
                case CharacterArtSpec.PuppetRole.Tail: return CharacterArtSpec.FilenameTail;
                case CharacterArtSpec.PuppetRole.ForearmLeft: return CharacterArtSpec.FilenameForearmLeft;
                case CharacterArtSpec.PuppetRole.ForearmRight: return CharacterArtSpec.FilenameForearmRight;
                case CharacterArtSpec.PuppetRole.ShinLeft: return CharacterArtSpec.FilenameShinLeft;
                case CharacterArtSpec.PuppetRole.ShinRight: return CharacterArtSpec.FilenameShinRight;
                default: return role.ToString().ToLowerInvariant();
            }
        }

        static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
            string leaf = Path.GetFileName(path);
            if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
            {
                EnsureFolder(parent);
            }
            if (!string.IsNullOrEmpty(parent) && !string.IsNullOrEmpty(leaf))
            {
                AssetDatabase.CreateFolder(parent, leaf);
            }
        }

        // Solid-color rect with 1px darker border. Reuse pattern from BootstrapWizard.WritePng
        // (separate file → avoid coupling generator to wizard internals).
        static void WriteRectPng(string path, int w, int h, Color color)
        {
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            var pixels = new Color32[w * h];
            var c32 = (Color32)color;
            for (int i = 0; i < pixels.Length; i++) pixels[i] = c32;
            // 1px darker border.
            Color32 border = new Color32(
                (byte)(c32.r * 0.5f), (byte)(c32.g * 0.5f), (byte)(c32.b * 0.5f), 255);
            for (int x = 0; x < w; x++) { pixels[x] = border; pixels[(h - 1) * w + x] = border; }
            for (int y = 0; y < h; y++) { pixels[y * w] = border; pixels[y * w + (w - 1)] = border; }
            tex.SetPixels32(pixels);
            tex.Apply(false, false);
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
        }

        // PPU=64 to match world scale (head sprite 40px → 0.625u tall — humanoid scale OK).
        // Pivot=center default → matches existing real-art assumption (PR G/H/I).
        static void ApplySpriteImport(string path)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) return;
            bool dirty = false;
            if (importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                dirty = true;
            }
            if (importer.spritePixelsPerUnit != 64f)
            {
                importer.spritePixelsPerUnit = 64f;
                dirty = true;
            }
            if (importer.filterMode != FilterMode.Bilinear)
            {
                importer.filterMode = FilterMode.Bilinear;
                dirty = true;
            }
            if (dirty)
            {
                importer.SaveAndReimport();
            }
        }
    }
}
#endif
