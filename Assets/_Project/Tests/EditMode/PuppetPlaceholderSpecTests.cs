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

        // ---------- Phase 3: WingLeft / WingRight (Crow / Bat) ----------

        [Test]
        public void DefaultRoles_NoWings_DefaultBehavior()
        {
            // Default overload (1 arg) phải KHÔNG include wings — backward compat cho Wolf/Fox/etc.
            var roles = new HashSet<CharacterArtSpec.PuppetRole>(
                PuppetPlaceholderSpec.DefaultRoles(includeTail: true));
            Assert.IsFalse(roles.Contains(CharacterArtSpec.PuppetRole.WingLeft));
            Assert.IsFalse(roles.Contains(CharacterArtSpec.PuppetRole.WingRight));
        }

        [Test]
        public void DefaultRoles_WithWings_AddsWingPair()
        {
            // includeWings=true → +2 roles (WingLeft + WingRight). Combined với includeTail=true
            // = 13 roles total (Crow / Bat full quadruped + tail + 2 wings).
            var roles = new HashSet<CharacterArtSpec.PuppetRole>(
                PuppetPlaceholderSpec.DefaultRoles(includeTail: true, includeWings: true));
            Assert.AreEqual(13, roles.Count);
            Assert.IsTrue(roles.Contains(CharacterArtSpec.PuppetRole.WingLeft));
            Assert.IsTrue(roles.Contains(CharacterArtSpec.PuppetRole.WingRight));
        }

        [Test]
        public void DefaultRoles_WithWingsNoTail_HasTwelveRoles()
        {
            // Bipedal flying mob (no tail) hypothetical — count = 10 humanoid + 2 wings = 12.
            var roles = new HashSet<CharacterArtSpec.PuppetRole>(
                PuppetPlaceholderSpec.DefaultRoles(includeTail: false, includeWings: true));
            Assert.AreEqual(12, roles.Count);
            Assert.IsFalse(roles.Contains(CharacterArtSpec.PuppetRole.Tail));
            Assert.IsTrue(roles.Contains(CharacterArtSpec.PuppetRole.WingLeft));
            Assert.IsTrue(roles.Contains(CharacterArtSpec.PuppetRole.WingRight));
        }

        [Test]
        public void RectFor_Wings_PositiveAndWiderThanTall()
        {
            // Wing silhouette = wide span > thin chord (xoãi rộng cho amplitude 50° đọc rõ).
            foreach (var role in new[]
            {
                CharacterArtSpec.PuppetRole.WingLeft,
                CharacterArtSpec.PuppetRole.WingRight,
            })
            {
                var (w, h) = PuppetPlaceholderSpec.RectFor(role);
                Assert.Greater(w, 0, $"{role} width");
                Assert.Greater(h, 0, $"{role} height");
                Assert.Greater(w, h, $"{role} expected wider than tall");
            }
        }

        [Test]
        public void ColorFor_Wings_ReturnsWingColor()
        {
            var p = PuppetPlaceholderSpec.PaletteFor(PuppetPlaceholderSpec.PlayerId);
            Assert.AreEqual(p.wing, PuppetPlaceholderSpec.ColorFor(CharacterArtSpec.PuppetRole.WingLeft, p));
            Assert.AreEqual(p.wing, PuppetPlaceholderSpec.ColorFor(CharacterArtSpec.PuppetRole.WingRight, p));
        }

        [Test]
        public void PaletteFor_AllKnownIds_HaveWingAlphaOne()
        {
            // Phase 3 wing field: tất cả palette PHẢI populate wing color (alpha=1) — kể cả non-flying
            // characters (default fallback gray). Crow (PR #113) wing populated với glossy black.
            foreach (var id in new[]
            {
                PuppetPlaceholderSpec.PlayerId,
                PuppetPlaceholderSpec.WolfId,
                PuppetPlaceholderSpec.FoxSpiritId,
                PuppetPlaceholderSpec.RabbitId,
                PuppetPlaceholderSpec.BoarId,
                PuppetPlaceholderSpec.DeerSpiritId,
                PuppetPlaceholderSpec.BossId,
                PuppetPlaceholderSpec.CrowId,
                PuppetPlaceholderSpec.BatId,
            })
            {
                var p = PuppetPlaceholderSpec.PaletteFor(id);
                Assert.AreEqual(1f, p.wing.a, $"{id} wing alpha");
            }
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
        public void PaletteFor_Crow_HasGlossyBlackPlumage()
        {
            // Crow corvid — body plumage RẤT tối (jet-black silhouette nổi bật vs sky biome).
            // Wing đậm hơn cả tunic (glossy primary feathers reflect xanh đêm) → wing
            // brightness ≤ tunic. Tunic brightness < 0.15 (very dark).
            var p = PuppetPlaceholderSpec.PaletteFor(PuppetPlaceholderSpec.CrowId);
            float tunicBrightness = (p.tunic.r + p.tunic.g + p.tunic.b) / 3f;
            float wingBrightness = (p.wing.r + p.wing.g + p.wing.b) / 3f;
            Assert.Less(tunicBrightness, 0.15f, "Crow plumage should be jet-black silhouette.");
            Assert.LessOrEqual(wingBrightness, tunicBrightness + 0.02f,
                "Crow wing should be at most ~tunic brightness (glossy black, không sáng hơn body).");
        }

        [Test]
        public void PaletteFor_Crow_DistinctFromOtherMobs()
        {
            // Crow tunic must be distinguishably darker than Wolf/Boar/DeerSpirit (non-flying mobs).
            var crow = PuppetPlaceholderSpec.PaletteFor(PuppetPlaceholderSpec.CrowId);
            float crowTunic = (crow.tunic.r + crow.tunic.g + crow.tunic.b) / 3f;
            foreach (var id in new[]
            {
                PuppetPlaceholderSpec.WolfId,
                PuppetPlaceholderSpec.BoarId,
                PuppetPlaceholderSpec.DeerSpiritId,
            })
            {
                var p = PuppetPlaceholderSpec.PaletteFor(id);
                float tunic = (p.tunic.r + p.tunic.g + p.tunic.b) / 3f;
                Assert.Less(crowTunic, tunic,
                    $"Crow plumage should be darker than {id} (jet-black silhouette).");
            }
        }

        [Test]
        public void PaletteFor_Bat_HasLeatheryDarkBrownFur()
        {
            // Bat — warm dark brown mammal (NOT cool jet black like Crow). Tunic brightness
            // dưới 0.25 (dark fur). r > b để đảm bảo warm hue (red-brown), distinct visual
            // identity vs Crow's cool palette.
            var p = PuppetPlaceholderSpec.PaletteFor(PuppetPlaceholderSpec.BatId);
            float tunicBrightness = (p.tunic.r + p.tunic.g + p.tunic.b) / 3f;
            Assert.Less(tunicBrightness, 0.25f, "Bat fur should be dark.");
            Assert.Greater(p.tunic.r, p.tunic.b,
                "Bat fur should have warm red-brown hue (r > b), distinct from Crow's cool palette.");
        }

        [Test]
        public void PaletteFor_Bat_DistinctFromCrow()
        {
            // 2 flying mob: Bat warm brown vs Crow cool jet-black. Player phải phân biệt được
            // mà không cần label. Đối chiếu wing color hue (Bat r > b, Crow b > r).
            var bat = PuppetPlaceholderSpec.PaletteFor(PuppetPlaceholderSpec.BatId);
            var crow = PuppetPlaceholderSpec.PaletteFor(PuppetPlaceholderSpec.CrowId);
            Assert.Greater(bat.wing.r, crow.wing.r + 0.05f,
                "Bat wing membrane should be redder/warmer than Crow's cool feathers.");
            Assert.Greater(crow.wing.b, bat.wing.b,
                "Crow wing should be cooler (more blue) than Bat's warm membrane.");
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

        // ---------- Phase 4: BodySegment1..4 (serpentine — Snake) ----------

        [Test]
        public void DefaultRoles_NoBodySegments_DefaultBehavior()
        {
            // 1-arg và 2-arg overload phải KHÔNG include body segments — backward compat.
            var rolesArg1 = new HashSet<CharacterArtSpec.PuppetRole>(
                PuppetPlaceholderSpec.DefaultRoles(includeTail: true));
            var rolesArg2 = new HashSet<CharacterArtSpec.PuppetRole>(
                PuppetPlaceholderSpec.DefaultRoles(includeTail: true, includeWings: true));
            Assert.IsFalse(rolesArg1.Contains(CharacterArtSpec.PuppetRole.BodySegment1));
            Assert.IsFalse(rolesArg2.Contains(CharacterArtSpec.PuppetRole.BodySegment4));
        }

        [Test]
        public void DefaultRoles_WithBodySegments_AddsAllFour()
        {
            // includeBodySegments=true → +4 roles. Combined với includeTail=true & includeWings=false
            // = 11 (10 humanoid + tail) + 4 segments = 15. Snake actual prefab sẽ filter ra
            // chỉ head + 4 segments — DefaultRoles giữ full set cho consistency.
            var roles = new HashSet<CharacterArtSpec.PuppetRole>(
                PuppetPlaceholderSpec.DefaultRoles(includeTail: true, includeWings: false,
                    includeBodySegments: true));
            Assert.AreEqual(15, roles.Count);
            Assert.IsTrue(roles.Contains(CharacterArtSpec.PuppetRole.BodySegment1));
            Assert.IsTrue(roles.Contains(CharacterArtSpec.PuppetRole.BodySegment2));
            Assert.IsTrue(roles.Contains(CharacterArtSpec.PuppetRole.BodySegment3));
            Assert.IsTrue(roles.Contains(CharacterArtSpec.PuppetRole.BodySegment4));
        }

        [Test]
        public void RectFor_BodySegments_PositiveAndOblong()
        {
            // Snake body segment — wider than tall (oblong horizontal). Tapering tail-ward
            // (seg1 widest → seg4 thinnest).
            var (w1, h1) = PuppetPlaceholderSpec.RectFor(CharacterArtSpec.PuppetRole.BodySegment1);
            var (w2, h2) = PuppetPlaceholderSpec.RectFor(CharacterArtSpec.PuppetRole.BodySegment2);
            var (w3, h3) = PuppetPlaceholderSpec.RectFor(CharacterArtSpec.PuppetRole.BodySegment3);
            var (w4, h4) = PuppetPlaceholderSpec.RectFor(CharacterArtSpec.PuppetRole.BodySegment4);

            // All positive + wider than tall.
            Assert.Greater(w1, 0); Assert.Greater(h1, 0); Assert.Greater(w1, h1);
            Assert.Greater(w2, 0); Assert.Greater(h2, 0); Assert.Greater(w2, h2);
            Assert.Greater(w3, 0); Assert.Greater(h3, 0); Assert.Greater(w3, h3);
            Assert.Greater(w4, 0); Assert.Greater(h4, 0); Assert.Greater(w4, h4);

            // Tapering: seg1 ≥ seg2 ≥ seg3 ≥ seg4 width (head-most thickest, tail thinnest).
            Assert.GreaterOrEqual(w1, w2);
            Assert.GreaterOrEqual(w2, w3);
            Assert.GreaterOrEqual(w3, w4);
            Assert.Greater(w1, w4, "seg1 (neck) wider than seg4 (tail) — strict taper across chain");
        }

        [Test]
        public void ColorFor_BodySegments_ReturnsTunicByDefault()
        {
            // Phase 4 #115: body segments reuse tunic color (Phase 4 #116 sẽ add SnakeId palette
            // với scale-specific color override).
            var p = PuppetPlaceholderSpec.PaletteFor(PuppetPlaceholderSpec.PlayerId);
            Assert.AreEqual(p.tunic, PuppetPlaceholderSpec.ColorFor(CharacterArtSpec.PuppetRole.BodySegment1, p));
            Assert.AreEqual(p.tunic, PuppetPlaceholderSpec.ColorFor(CharacterArtSpec.PuppetRole.BodySegment2, p));
            Assert.AreEqual(p.tunic, PuppetPlaceholderSpec.ColorFor(CharacterArtSpec.PuppetRole.BodySegment3, p));
            Assert.AreEqual(p.tunic, PuppetPlaceholderSpec.ColorFor(CharacterArtSpec.PuppetRole.BodySegment4, p));
        }

        [Test]
        public void ColorFor_BodySegments_NotMagenta()
        {
            // Body segment colors must be properly mapped — no fallback magenta sentinel.
            var p = PuppetPlaceholderSpec.PaletteFor(PuppetPlaceholderSpec.PlayerId);
            foreach (var role in new[]
            {
                CharacterArtSpec.PuppetRole.BodySegment1,
                CharacterArtSpec.PuppetRole.BodySegment2,
                CharacterArtSpec.PuppetRole.BodySegment3,
                CharacterArtSpec.PuppetRole.BodySegment4,
            })
            {
                var c = PuppetPlaceholderSpec.ColorFor(role, p);
                Assert.AreNotEqual(Color.magenta, c, $"{role} should not be magenta sentinel");
            }
        }
    }
}
