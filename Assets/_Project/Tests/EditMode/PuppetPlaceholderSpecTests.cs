using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using WildernessCultivation.Core;

namespace WildernessCultivation.Tests.EditMode
{
    /// <summary>
    /// PR M — pure-data invariants cho PuppetPlaceholderSpec. Editor-side
    /// PuppetPlaceholderGenerator dùng spec này để bake skeleton PNGs (test indirect).
    ///
    /// Đảm bảo:
    /// - DefaultRoles luôn enumerate đủ 13 joints (or 10 nếu skip tail).
    /// - Per-character palettes distinct + non-magenta (tránh fallback "loud").
    /// - Rect dimensions positive + reasonable (head smaller than torso, shin smaller than leg).
    /// - ColorFor maps role → palette field đúng.
    /// </summary>
    public class PuppetPlaceholderSpecTests
    {
        [Test]
        public void DefaultRoles_WithTail_HasAll11()
        {
            var roles = new HashSet<CharacterArtSpec.PuppetRole>(
                PuppetPlaceholderSpec.DefaultRoles(includeTail: true));
            // 10 humanoid joints (head/torso/2 arm/2 forearm/2 leg/2 shin) + 1 tail = 11.
            Assert.AreEqual(11, roles.Count);
            Assert.IsTrue(roles.Contains(CharacterArtSpec.PuppetRole.Head));
            Assert.IsTrue(roles.Contains(CharacterArtSpec.PuppetRole.Torso));
            Assert.IsTrue(roles.Contains(CharacterArtSpec.PuppetRole.ArmLeft));
            Assert.IsTrue(roles.Contains(CharacterArtSpec.PuppetRole.ArmRight));
            Assert.IsTrue(roles.Contains(CharacterArtSpec.PuppetRole.ForearmLeft));
            Assert.IsTrue(roles.Contains(CharacterArtSpec.PuppetRole.ForearmRight));
            Assert.IsTrue(roles.Contains(CharacterArtSpec.PuppetRole.LegLeft));
            Assert.IsTrue(roles.Contains(CharacterArtSpec.PuppetRole.LegRight));
            Assert.IsTrue(roles.Contains(CharacterArtSpec.PuppetRole.ShinLeft));
            Assert.IsTrue(roles.Contains(CharacterArtSpec.PuppetRole.ShinRight));
            Assert.IsTrue(roles.Contains(CharacterArtSpec.PuppetRole.Tail));
        }

        [Test]
        public void DefaultRoles_WithoutTail_Has10()
        {
            var roles = new HashSet<CharacterArtSpec.PuppetRole>(
                PuppetPlaceholderSpec.DefaultRoles(includeTail: false));
            Assert.AreEqual(10, roles.Count);
            Assert.IsFalse(roles.Contains(CharacterArtSpec.PuppetRole.Tail));
        }

        [Test]
        public void DefaultRoles_IncludesL2Joints()
        {
            // PR K elbow + knee joints phải xuất hiện trong placeholder skeleton để demo
            // bend math hiển thị (forearm bends khi punch, shin bends khi crouch / walk back-swing).
            var roles = new HashSet<CharacterArtSpec.PuppetRole>(
                PuppetPlaceholderSpec.DefaultRoles(includeTail: false));
            Assert.IsTrue(roles.Contains(CharacterArtSpec.PuppetRole.ForearmLeft));
            Assert.IsTrue(roles.Contains(CharacterArtSpec.PuppetRole.ForearmRight));
            Assert.IsTrue(roles.Contains(CharacterArtSpec.PuppetRole.ShinLeft));
            Assert.IsTrue(roles.Contains(CharacterArtSpec.PuppetRole.ShinRight));
        }

        // ---------- Palette ----------

        [Test]
        public void PaletteFor_Player_HasBlueRobeTunic()
        {
            var p = PuppetPlaceholderSpec.PaletteFor(PuppetPlaceholderSpec.PlayerId);
            // Tunic blue: B > R + G/2 → robe-blue dominance.
            Assert.Greater(p.tunic.b, p.tunic.r);
            Assert.Greater(p.tunic.b, p.tunic.g);
        }

        [Test]
        public void PaletteFor_FoxSpirit_HasOrangeTunic()
        {
            var p = PuppetPlaceholderSpec.PaletteFor(PuppetPlaceholderSpec.FoxSpiritId);
            // Orange dominance: R > G > B.
            Assert.Greater(p.tunic.r, p.tunic.g);
            Assert.Greater(p.tunic.g, p.tunic.b);
        }

        [Test]
        public void PaletteFor_Wolf_GrayMonochrome()
        {
            var p = PuppetPlaceholderSpec.PaletteFor(PuppetPlaceholderSpec.WolfId);
            // Wolf fur near-monochrome: R, G, B trong ±0.05 của nhau.
            Assert.That(Mathf.Abs(p.tunic.r - p.tunic.g), Is.LessThan(0.05f));
            Assert.That(Mathf.Abs(p.tunic.g - p.tunic.b), Is.LessThan(0.05f));
        }

        [Test]
        public void PaletteFor_Rabbit_HasBrownTanTunic()
        {
            var p = PuppetPlaceholderSpec.PaletteFor(PuppetPlaceholderSpec.RabbitId);
            // Brown/tan dominance: R > G > B (warm earthy fur).
            Assert.Greater(p.tunic.r, p.tunic.g);
            Assert.Greater(p.tunic.g, p.tunic.b);
        }

        [Test]
        public void PaletteFor_Rabbit_TailIsBrightWhite()
        {
            // Rabbit puffy white tail = hero feature, brighter than torso fur.
            var p = PuppetPlaceholderSpec.PaletteFor(PuppetPlaceholderSpec.RabbitId);
            float tailBrightness = (p.tail.r + p.tail.g + p.tail.b) / 3f;
            float tunicBrightness = (p.tunic.r + p.tunic.g + p.tunic.b) / 3f;
            Assert.Greater(tailBrightness, tunicBrightness,
                "Rabbit puffy tail should be brighter than torso fur (hero feature visibility).");
        }

        [Test]
        public void PaletteFor_Boar_HasDarkBrownBristlyFur()
        {
            // Boar wild bristly fur — dark brown dominance: R > G > B + tunic brightness < 0.4.
            var p = PuppetPlaceholderSpec.PaletteFor(PuppetPlaceholderSpec.BoarId);
            Assert.Greater(p.tunic.r, p.tunic.g);
            Assert.Greater(p.tunic.g, p.tunic.b);
            float tunicBrightness = (p.tunic.r + p.tunic.g + p.tunic.b) / 3f;
            Assert.Less(tunicBrightness, 0.4f, "Boar fur should be dark (heavy menacing mob silhouette).");
        }

        [Test]
        public void PaletteFor_DeerSpirit_HasCreamFawnFur()
        {
            // Deer spirit cream fawn fur — warm earthy R > G > B same as boar/rabbit, but lighter:
            // tunic brightness > 0.5 (vs boar < 0.4) phân biệt visual hierarchy.
            var p = PuppetPlaceholderSpec.PaletteFor(PuppetPlaceholderSpec.DeerSpiritId);
            Assert.Greater(p.tunic.r, p.tunic.g);
            Assert.Greater(p.tunic.g, p.tunic.b);
            float tunicBrightness = (p.tunic.r + p.tunic.g + p.tunic.b) / 3f;
            Assert.Greater(tunicBrightness, 0.5f, "Deer fawn fur should be lighter than boar (graceful vs heavy).");
        }

        [Test]
        public void PaletteFor_DeerSpirit_TailIsWhiteFlick()
        {
            // Deer white tail flick — brighter than fawn body fur (visual silhouette accent).
            var p = PuppetPlaceholderSpec.PaletteFor(PuppetPlaceholderSpec.DeerSpiritId);
            float tailBrightness = (p.tail.r + p.tail.g + p.tail.b) / 3f;
            float tunicBrightness = (p.tunic.r + p.tunic.g + p.tunic.b) / 3f;
            Assert.Greater(tailBrightness, tunicBrightness,
                "Deer tail flick should be brighter than fawn body fur.");
        }

        [Test]
        public void PaletteFor_Boss_HasDarkVillainRobe()
        {
            // Boss Hắc Vương — humanoid villain. Tunic must be very dark (menacing overlord
            // silhouette, contrast với player blue tunic). Brightness < 0.2 → much darker
            // than Boar (< 0.4) để Boss đứng trên cùng visual threat hierarchy.
            var p = PuppetPlaceholderSpec.PaletteFor(PuppetPlaceholderSpec.BossId);
            float tunicBrightness = (p.tunic.r + p.tunic.g + p.tunic.b) / 3f;
            Assert.Less(tunicBrightness, 0.2f,
                "Boss robe should be very dark (apex menace silhouette).");
        }

        [Test]
        public void PaletteFor_Boss_TunicDistinctFromPlayer()
        {
            // Boss tunic must clearly differ from Player blue tunic — same blue dominance =
            // visual confusion lúc spawn boss vs player. Boss = dark crimson/black, NOT blue.
            var boss = PuppetPlaceholderSpec.PaletteFor(PuppetPlaceholderSpec.BossId);
            var player = PuppetPlaceholderSpec.PaletteFor(PuppetPlaceholderSpec.PlayerId);
            Assert.LessOrEqual(boss.tunic.b, boss.tunic.r,
                "Boss tunic should NOT be blue-dominant (player owns that palette).");
            Assert.Greater(player.tunic.b, boss.tunic.b,
                "Boss tunic blue channel should be much lower than player blue robe.");
        }

        [Test]
        public void PaletteFor_UnknownId_FallsBackToNeutralGray()
        {
            var p = PuppetPlaceholderSpec.PaletteFor("doesnt_exist");
            // Just verify all colors are populated (alpha=1 default for Color struct ctor).
            Assert.AreEqual(1f, p.tunic.a);
            Assert.AreEqual(1f, p.skin.a);
            Assert.AreEqual(1f, p.trousers.a);
        }

        [Test]
        public void PaletteFor_ShinIsDarkerThanTrousers()
        {
            // Visual hierarchy: shin (calf) usually rendered darker để tách khỏi thigh.
            foreach (var id in new[]
            {
                PuppetPlaceholderSpec.PlayerId,
                PuppetPlaceholderSpec.WolfId,
                PuppetPlaceholderSpec.FoxSpiritId,
                PuppetPlaceholderSpec.RabbitId,
                PuppetPlaceholderSpec.BoarId,
                PuppetPlaceholderSpec.DeerSpiritId,
                PuppetPlaceholderSpec.BossId
            })
            {
                var p = PuppetPlaceholderSpec.PaletteFor(id);
                float trousersBrightness = (p.trousers.r + p.trousers.g + p.trousers.b) / 3f;
                float shinBrightness = (p.shin.r + p.shin.g + p.shin.b) / 3f;
                Assert.LessOrEqual(shinBrightness, trousersBrightness,
                    $"{id}: shin should be ≤ trousers brightness");
            }
        }

        // ---------- RectFor ----------

        [Test]
        public void RectFor_AllRolesPositive()
        {
            foreach (var role in PuppetPlaceholderSpec.DefaultRoles(includeTail: true))
            {
                var (w, h) = PuppetPlaceholderSpec.RectFor(role);
                Assert.Greater(w, 0, $"{role} width");
                Assert.Greater(h, 0, $"{role} height");
            }
        }

        [Test]
        public void RectFor_TorsoTallerThanHead()
        {
            var (_, headH) = PuppetPlaceholderSpec.RectFor(CharacterArtSpec.PuppetRole.Head);
            var (_, torsoH) = PuppetPlaceholderSpec.RectFor(CharacterArtSpec.PuppetRole.Torso);
            Assert.Greater(torsoH, headH);
        }

        [Test]
        public void RectFor_ForearmShorterThanArm()
        {
            // Anatomy proportion check — forearm should not exceed full arm length.
            var (_, armH) = PuppetPlaceholderSpec.RectFor(CharacterArtSpec.PuppetRole.ArmLeft);
            var (_, foreH) = PuppetPlaceholderSpec.RectFor(CharacterArtSpec.PuppetRole.ForearmLeft);
            Assert.Less(foreH, armH);
        }

        [Test]
        public void RectFor_ShinShorterThanLeg()
        {
            var (_, legH) = PuppetPlaceholderSpec.RectFor(CharacterArtSpec.PuppetRole.LegLeft);
            var (_, shinH) = PuppetPlaceholderSpec.RectFor(CharacterArtSpec.PuppetRole.ShinLeft);
            Assert.Less(shinH, legH);
        }

        [Test]
        public void RectFor_TailIsHorizontal()
        {
            // Tail wider than tall (extends behind body).
            var (w, h) = PuppetPlaceholderSpec.RectFor(CharacterArtSpec.PuppetRole.Tail);
            Assert.Greater(w, h);
        }

        // ---------- ColorFor ----------

        [Test]
        public void ColorFor_Head_ReturnsSkin()
        {
            var p = PuppetPlaceholderSpec.PaletteFor(PuppetPlaceholderSpec.PlayerId);
            Assert.AreEqual(p.skin, PuppetPlaceholderSpec.ColorFor(CharacterArtSpec.PuppetRole.Head, p));
        }

        [Test]
        public void ColorFor_Forearm_ReturnsSkin()
        {
            // Forearm = exposed flesh (sleeve cuts off at elbow).
            var p = PuppetPlaceholderSpec.PaletteFor(PuppetPlaceholderSpec.PlayerId);
            Assert.AreEqual(p.skin, PuppetPlaceholderSpec.ColorFor(CharacterArtSpec.PuppetRole.ForearmLeft, p));
            Assert.AreEqual(p.skin, PuppetPlaceholderSpec.ColorFor(CharacterArtSpec.PuppetRole.ForearmRight, p));
        }

        [Test]
        public void ColorFor_Torso_ReturnsTunic()
        {
            var p = PuppetPlaceholderSpec.PaletteFor(PuppetPlaceholderSpec.PlayerId);
            Assert.AreEqual(p.tunic, PuppetPlaceholderSpec.ColorFor(CharacterArtSpec.PuppetRole.Torso, p));
        }

        [Test]
        public void ColorFor_UnknownRole_ReturnsLoudMagenta()
        {
            // Magenta sentinel: any unmapped role rendered as loud bright pink → catch immediately.
            var p = PuppetPlaceholderSpec.PaletteFor(PuppetPlaceholderSpec.PlayerId);
            Assert.AreEqual(Color.magenta, PuppetPlaceholderSpec.ColorFor(CharacterArtSpec.PuppetRole.Unknown, p));
        }

        [Test]
        public void ColorFor_AllDefaultRoles_NotMagenta()
        {
            // Sanity: tất cả default roles có mapping rõ ràng (không leak magenta vào demo).
            var p = PuppetPlaceholderSpec.PaletteFor(PuppetPlaceholderSpec.PlayerId);
            foreach (var role in PuppetPlaceholderSpec.DefaultRoles(includeTail: true))
            {
                var c = PuppetPlaceholderSpec.ColorFor(role, p);
                Assert.AreNotEqual(Color.magenta, c, $"{role} should not be magenta sentinel");
            }
        }
    }
}
