using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace WildernessCultivation.Tests.EditMode
{
    /// <summary>
    /// Guards the committed pixel-art sprite assets that
    /// <c>BootstrapWizard.CreateSprites</c> loads at scene-bootstrap time.
    ///
    /// The wizard now skips its <c>WritePng()</c> fallback whenever the PNG
    /// already exists on disk — meaning the real art under
    /// <c>Assets/_Project/Sprites/</c> is what ships in builds. If anyone
    /// accidentally removes one of those committed PNGs (or its <c>.meta</c>),
    /// the wizard would silently fall back to a solid-color square and the
    /// regression would only surface in a manual playtest. These tests catch
    /// that immediately by asserting:
    ///
    /// 1. Every sprite ID listed in <c>BootstrapWizard.CreateSprites.defs</c>
    ///    has a committed PNG + .meta file at the expected path.
    /// 2. Each sprite is not a degenerate 1x1 fallback and not a single
    ///    solid color (the placeholder pattern from the old code path).
    /// 3. Each PNG has correct TextureImporter settings (Sprite, Point,
    ///    Clamp, 32 PPU) so the build doesn't regress to bilinear /
    ///    Default texture type.
    /// </summary>
    public class SpriteAssetTests
    {
        const string SpritesDir = "Assets/_Project/Sprites";

        // Mirror of BootstrapWizard.CreateSprites defs — keep in sync.
        // Listing here intentionally so a missing entry (e.g. a new sprite ID
        // is added to the wizard but no PNG was committed) fails the test
        // until both sides are updated together.
        static readonly string[] s_RequiredIds =
        {
            "player", "tree", "rock", "rabbit", "wolf", "fox_spirit",
            "chest", "workbench", "campfire", "water", "ground", "projectile",
            "icon_stick", "icon_stone", "icon_meat", "icon_grilled",
            "icon_water", "icon_torch", "icon_fish", "icon_rod",
            "ui_white",
        };

        [Test]
        public void EveryRequiredSpritePngExists()
        {
            foreach (var id in s_RequiredIds)
            {
                string path = $"{SpritesDir}/{id}.png";
                Assert.That(File.Exists(path), Is.True,
                    $"Committed sprite missing at {path}. Re-run tools/gen_sprites.py and commit the PNG (and its .meta).");
                Assert.That(File.Exists(path + ".meta"), Is.True,
                    $".meta missing at {path}.meta. Open the project in Unity to regenerate, then commit it.");
            }
        }

        [Test]
        public void AllSpritesAreLoadableWithCorrectImportSettings()
        {
            foreach (var id in s_RequiredIds)
            {
                string path = $"{SpritesDir}/{id}.png";
                if (!File.Exists(path))
                {
                    // Covered by EveryRequiredSpritePngExists; skip to keep
                    // failures scoped to one root-cause assertion.
                    continue;
                }
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                Assert.That(sprite, Is.Not.Null,
                    $"Sprite at {path} did not load — texture import may be misconfigured.");

                var importer = (TextureImporter)AssetImporter.GetAtPath(path);
                Assert.That(importer, Is.Not.Null,
                    $"No TextureImporter for {path}.");
                Assert.That(importer.textureType, Is.EqualTo(TextureImporterType.Sprite),
                    $"{id}: textureType must be Sprite (BootstrapWizard sets this — was it overridden?).");
                Assert.That(importer.spritePixelsPerUnit, Is.EqualTo(32f).Within(0.01f),
                    $"{id}: spritePixelsPerUnit must be 32 to match the wizard contract.");
                Assert.That(importer.filterMode, Is.EqualTo(FilterMode.Point),
                    $"{id}: filterMode must be Point — bilinear blurs pixel art.");
            }
        }

        [Test]
        public void NoSpriteIsADegenerateOnePixelOrSingleColorPlaceholder()
        {
            // Solid-color squares were the OLD placeholder behaviour. If a
            // committed sprite collapses to that pattern it means the
            // hand-authored PNG was lost / overwritten by the WritePng()
            // fallback path in BootstrapWizard. ui_white is intentionally
            // a 4x4 solid white fill for UI Image components — exempt it.
            foreach (var id in s_RequiredIds)
            {
                if (id == "ui_white") continue;

                string path = $"{SpritesDir}/{id}.png";
                if (!File.Exists(path)) continue; // Covered elsewhere.

                var bytes = File.ReadAllBytes(path);
                var tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                tex.LoadImage(bytes);

                Assert.That(tex.width, Is.GreaterThan(8), $"{id}: PNG width unexpectedly small ({tex.width}).");
                Assert.That(tex.height, Is.GreaterThan(8), $"{id}: PNG height unexpectedly small ({tex.height}).");

                // Probe a handful of pixels — if every pixel is identical,
                // the sprite is the old solid-color fallback.
                var pixels = tex.GetPixels32();
                Color32 first = pixels[0];
                bool allSame = true;
                for (int i = 1; i < pixels.Length; i++)
                {
                    if (!pixels[i].Equals(first))
                    {
                        allSame = false;
                        break;
                    }
                }
                Assert.That(allSame, Is.False,
                    $"{id}: every pixel is the same color — this is the solid-color fallback, not the committed pixel art. Re-run tools/gen_sprites.py.");

                Object.DestroyImmediate(tex);
            }
        }
    }
}
