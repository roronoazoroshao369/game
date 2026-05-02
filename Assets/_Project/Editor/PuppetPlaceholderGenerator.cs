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
    /// colored-rectangle placeholder sprite (head / torso / arm L+R / forearm L+R / leg L+R /
    /// shin L+R / tail) vào <c>Sprites/puppet/{id}/{role}.asset</c>. PuppetAnimController dùng
    /// những placeholder này để chạy demo full procedural motion (walk swing + lunge + crouch +
    /// 4-dir swap) ngay không cần asset.
    ///
    /// Real PNG drop sau này → <see cref="CharacterArtImporter.TryLoadCharacterSpriteSet"/>
    /// trả non-null → BootstrapWizard skip placeholder fallback. Idempotent: chạy lại không
    /// overwrite real art.
    ///
    /// PR R (asset bypass): Trước đây flow ghi PNG → AssetDatabase.Refresh → ImportAsset →
    /// LoadAssetAtPath&lt;Sprite&gt; gặp race conditions trên macOS Unity (importer null,
    /// LoadAssetAtPath null) → 0 sprite load được → Player invisible. Fix: dùng
    /// AssetDatabase.CreateAsset trực tiếp với Texture2D in-memory + AddObjectToAsset cho Sprite
    /// sub-asset → bypass file system PNG + import pipeline hoàn toàn. Asset .asset file là
    /// binary blob Unity manage internal — không phụ thuộc texture import settings, không có
    /// async race.
    ///
    /// Pure-data spec (palette + rect + role) sống ở
    /// <see cref="WildernessCultivation.Core.PuppetPlaceholderSpec"/> — testable từ EditMode
    /// runtime asmdef. File này là Editor-side IO wrapper around spec.
    /// </summary>
    public static class PuppetPlaceholderGenerator
    {
        public const string PlaceholderRoot = "Assets/_Project/Sprites/puppet";
        // Single source of truth: PuppetPlaceholderSpec.PuppetPlaceholderPPU. CharacterArtImporter
        // cũng dùng const này khi compute auto-PPU cho user art (giữ world size khớp placeholder).
        const float PlaceholderPpu = PuppetPlaceholderSpec.PuppetPlaceholderPPU;

        /// <summary>
        /// Ensure placeholder Sprite assets ở <c>Sprites/puppet/{characterId}/{role}.asset</c>.
        /// Idempotent: re-run reuses existing assets nếu loadable; regenerates broken assets.
        /// Returns dict role → Sprite.
        /// </summary>
        public static Dictionary<CharacterArtSpec.PuppetRole, Sprite> EnsureSpriteSet(
            string characterId, bool includeTail = true)
        {
            return EnsureSpriteSet(characterId, includeTail, includeWings: false);
        }

        /// <summary>
        /// Phase 3 overload — opt-in wing roles cho flying mob (Crow / Bat).
        /// Roles = humanoid joints + tail/wings opt-in (full 13-role anatomy max).
        /// Dùng <see cref="EnsureSpriteSet(string, IEnumerable{CharacterArtSpec.PuppetRole})"/>
        /// nếu cần custom anatomy (ví dụ Crow chỉ 6 roles: head/torso/wing×2/leg×2).
        /// </summary>
        public static Dictionary<CharacterArtSpec.PuppetRole, Sprite> EnsureSpriteSet(
            string characterId, bool includeTail, bool includeWings)
        {
            return EnsureSpriteSet(characterId,
                PuppetPlaceholderSpec.DefaultRoles(includeTail, includeWings));
        }

        /// <summary>
        /// Phase 3 — core impl với explicit role list. Crow / Bat có anatomy thu hẹp
        /// (no arms/forearms/shins/tail) → caller pass 6-role list (head/torso/wing×2/leg×2)
        /// để placeholder demo không có rect dư thừa flailing quanh torso.
        /// </summary>
        public static Dictionary<CharacterArtSpec.PuppetRole, Sprite> EnsureSpriteSet(
            string characterId, IEnumerable<CharacterArtSpec.PuppetRole> roleSource)
        {
            string folder = $"{PlaceholderRoot}/{characterId}";
            EnsureFolder(folder);

            var palette = PuppetPlaceholderSpec.PaletteFor(characterId);
            var dict = new Dictionary<CharacterArtSpec.PuppetRole, Sprite>();

            // Materialize IEnumerable một lần — DefaultRoles dùng yield return nên iter
            // 2 lần sẽ enum lại; copy vào List ổn định + cho phép Count.
            var roles = new List<CharacterArtSpec.PuppetRole>(roleSource);

            // Cleanup legacy: xoá .png file cũ (PR M-Q approach) nếu còn — đảm bảo không có
            // 2 asset cùng path conflict.
            CleanupLegacyPngs(folder, roles);

            AssetDatabase.StartAssetEditing();
            try
            {
                foreach (var role in roles)
                {
                    string assetPath = $"{folder}/{RoleToFilename(role)}.asset";
                    Sprite sprite = null;

                    // Idempotent: nếu .asset đã tồn tại + load được → reuse.
                    if (File.Exists(assetPath))
                    {
                        sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
                    }

                    if (sprite == null)
                    {
                        sprite = CreateSpriteAsset(assetPath, role, palette);
                    }

                    if (sprite != null)
                    {
                        dict[role] = sprite;
                    }
                    else
                    {
                        Debug.LogWarning(
                            $"[PuppetPlaceholderGenerator] Failed to create/load Sprite for {characterId}/{role} at {assetPath}. " +
                            "Skeleton sẽ thiếu body part này.");
                    }
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }

            AssetDatabase.SaveAssets();

            Debug.Log(
                $"[PuppetPlaceholderGenerator] {characterId}: loaded {dict.Count}/{roles.Count} placeholder sprites at {folder}/");

            return dict;
        }

        /// <summary>
        /// Generate Texture2D + Sprite in-memory, save as .asset file (Texture main asset +
        /// Sprite sub-asset). Bypasses PNG file system + texture import pipeline race.
        /// </summary>
        static Sprite CreateSpriteAsset(string assetPath,
            CharacterArtSpec.PuppetRole role,
            PuppetPlaceholderSpec.Palette palette)
        {
            var (w, h) = PuppetPlaceholderSpec.RectFor(role);
            var color = PuppetPlaceholderSpec.ColorFor(role, palette);

            var tex = BuildRectTexture(w, h, color);
            tex.name = $"{RoleToFilename(role)}_tex";

            // Delete existing .asset nếu có (corrupt từ run trước).
            if (File.Exists(assetPath))
            {
                AssetDatabase.DeleteAsset(assetPath);
            }

            // Save texture as main asset của file .asset.
            AssetDatabase.CreateAsset(tex, assetPath);

            // Sprite sub-asset cùng file, PPU 64, joint-anchor pivot per role
            // (head bottom-center, arm/leg top-center, torso center) — match
            // CharacterArtImporter user-art pivots để placeholder + real art swap-able.
            var sprite = Sprite.Create(
                tex,
                new Rect(0, 0, w, h),
                PuppetPlaceholderSpec.PivotFor(role),
                PlaceholderPpu,
                0,
                SpriteMeshType.FullRect);
            sprite.name = RoleToFilename(role);
            AssetDatabase.AddObjectToAsset(sprite, assetPath);

            // Force save sub-asset reference.
            EditorUtility.SetDirty(tex);
            EditorUtility.SetDirty(sprite);

            return sprite;
        }

        /// <summary>
        /// Build solid-color Texture2D với 1px darker border. Returns texture in-memory chưa
        /// save disk — caller phải pass vào AssetDatabase.CreateAsset.
        /// </summary>
        static Texture2D BuildRectTexture(int w, int h, Color color)
        {
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            var pixels = new Color32[w * h];
            var c32 = (Color32)color;
            for (int i = 0; i < pixels.Length; i++) pixels[i] = c32;
            // 1px darker border để skeleton visible khi overlap nhau.
            Color32 border = new Color32(
                (byte)(c32.r * 0.5f), (byte)(c32.g * 0.5f), (byte)(c32.b * 0.5f), 255);
            for (int x = 0; x < w; x++) { pixels[x] = border; pixels[(h - 1) * w + x] = border; }
            for (int y = 0; y < h; y++) { pixels[y * w] = border; pixels[y * w + (w - 1)] = border; }
            tex.SetPixels32(pixels);
            tex.Apply(false, false);
            return tex;
        }

        /// <summary>
        /// Xoá .png file legacy từ PR M-Q approach. Nếu .png .meta dangling → cũng xoá.
        /// </summary>
        static void CleanupLegacyPngs(string folder, List<CharacterArtSpec.PuppetRole> roles)
        {
            foreach (var role in roles)
            {
                string pngPath = $"{folder}/{RoleToFilename(role)}.png";
                if (File.Exists(pngPath))
                {
                    AssetDatabase.DeleteAsset(pngPath);
                }
            }
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
                case CharacterArtSpec.PuppetRole.WingLeft: return CharacterArtSpec.FilenameWingLeft;
                case CharacterArtSpec.PuppetRole.WingRight: return CharacterArtSpec.FilenameWingRight;
                case CharacterArtSpec.PuppetRole.BodySegment1: return CharacterArtSpec.FilenameBodySeg1;
                case CharacterArtSpec.PuppetRole.BodySegment2: return CharacterArtSpec.FilenameBodySeg2;
                case CharacterArtSpec.PuppetRole.BodySegment3: return CharacterArtSpec.FilenameBodySeg3;
                case CharacterArtSpec.PuppetRole.BodySegment4: return CharacterArtSpec.FilenameBodySeg4;
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
    }
}
#endif
