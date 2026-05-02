using System.Collections.Generic;
using UnityEngine;

namespace WildernessCultivation.Core
{
    /// <summary>
    /// Pure-data spec cho puppet placeholder skeleton (PR M). Editor-side
    /// generator dùng spec này để bake colored-rectangle PNGs ra disk khi user chưa drop real
    /// art vào <c>Art/Characters/{id}/</c>. Không có Unity Editor dependency → testable từ
    /// EditMode tests.
    ///
    /// Adding new character = 1 case trong <see cref="PaletteFor"/> + folder
    /// <c>Art/Characters/{newId}/</c>. KHÔNG cần code thay đổi nào khác (animation logic
    /// hoàn toàn data-driven).
    /// </summary>
    public static class PuppetPlaceholderSpec
    {
        /// <summary>
        /// Pixels-per-unit dùng cho placeholder sprite tự sinh + user-provided body-part PNG.
        /// Phải khớp giữa placeholder generator và CharacterArtImporter để user art render
        /// world size bằng placeholder (head 40px → 0.625u, torso 80px → 1.25u, etc.).
        /// </summary>
        public const float PuppetPlaceholderPPU = 64f;

        /// <summary>
        /// World height (units) của một role placeholder = RectFor(role).h / PuppetPlaceholderPPU.
        /// Helper cho importer khi compute auto-PPU per role để user PNG khớp size placeholder.
        /// </summary>
        public static float WorldHeightFor(CharacterArtSpec.PuppetRole role)
        {
            var rect = RectFor(role);
            return rect.h / PuppetPlaceholderPPU;
        }

        /// <summary>
        /// Joint-anchor sprite pivot (normalized 0..1) per role. Pivot = vị trí khớp trên sprite,
        /// nơi part attach vào parent (head→neck, arm→shoulder, leg→hip, etc.). Khi part rotate
        /// quanh transform.localPosition (do PuppetAnimController), pivot quyết định điểm xoay
        /// thật trên sprite — top-center pivot cho arm = arm vung quanh shoulder (correct anatomy)
        /// thay vì xoay quanh middle of arm (cây que tự quay, sai).
        ///
        /// Pivot scheme:
        /// - Head: (0.5, 0)   — bottom-center, neck joint
        /// - Torso: (0.5, 0.5) — center (mass-centered, không có parent joint riêng)
        /// - Arm/Forearm/Leg/Shin: (0.5, 1) — top-center, hang DOWN từ joint
        /// - Tail: (1, 0.5)   — right-center, attach ở rear-of-body (East-facing default)
        /// - Wing: (0.5, 0.5) — center (Phase 3 keep neutral)
        /// - BodySegment1..4: (1, 0.5) — right-center, snake chain pivot ở head-side junction
        /// </summary>
        public static Vector2 PivotFor(CharacterArtSpec.PuppetRole role)
        {
            switch (role)
            {
                case CharacterArtSpec.PuppetRole.Head:
                    return new Vector2(0.5f, 0f);
                case CharacterArtSpec.PuppetRole.Torso:
                    return new Vector2(0.5f, 0.5f);
                case CharacterArtSpec.PuppetRole.ArmLeft:
                case CharacterArtSpec.PuppetRole.ArmRight:
                case CharacterArtSpec.PuppetRole.ForearmLeft:
                case CharacterArtSpec.PuppetRole.ForearmRight:
                case CharacterArtSpec.PuppetRole.LegLeft:
                case CharacterArtSpec.PuppetRole.LegRight:
                case CharacterArtSpec.PuppetRole.ShinLeft:
                case CharacterArtSpec.PuppetRole.ShinRight:
                    return new Vector2(0.5f, 1f);
                case CharacterArtSpec.PuppetRole.Tail:
                    return new Vector2(1f, 0.5f);
                case CharacterArtSpec.PuppetRole.WingLeft:
                case CharacterArtSpec.PuppetRole.WingRight:
                    return new Vector2(0.5f, 0.5f);
                case CharacterArtSpec.PuppetRole.BodySegment1:
                case CharacterArtSpec.PuppetRole.BodySegment2:
                case CharacterArtSpec.PuppetRole.BodySegment3:
                case CharacterArtSpec.PuppetRole.BodySegment4:
                    return new Vector2(1f, 0.5f);
                default:
                    return new Vector2(0.5f, 0.5f);
            }
        }

        /// <summary>
        /// Per-character color palette. Skin / sleeve / trousers / tail riêng.
        /// </summary>
        public struct Palette
        {
            public Color skin;     // head + forearm fill
            public Color tunic;    // torso + arm sleeve
            public Color trousers; // leg
            public Color shin;     // shin (darker than trousers)
            public Color tail;     // tail (mob only)
            public Color wing;     // wing (flying mob only — Phase 3)
        }

        public const string PlayerId = "player";
        public const string WolfId = "wolf";
        public const string FoxSpiritId = "fox_spirit";
        public const string RabbitId = "rabbit";
        public const string BoarId = "boar";
        public const string DeerSpiritId = "deer_spirit";
        public const string BossId = "boss";
        // Phase 3 flying mob — wings + reduced anatomy. CrowId Phase 3 PR #113, BatId PR #114.
        public const string CrowId = "crow";
        public const string BatId = "bat";

        /// <summary>
        /// Default palette per character ID. Unknown id → fallback "neutral" gray. Distinct
        /// torso colors giúp user phân biệt 3 character từ xa khi chạy demo.
        /// </summary>
        public static Palette PaletteFor(string characterId)
        {
            switch (characterId)
            {
                case PlayerId:
                    return new Palette
                    {
                        skin = new Color(0.95f, 0.82f, 0.70f),
                        tunic = new Color(0.32f, 0.46f, 0.78f),
                        trousers = new Color(0.22f, 0.30f, 0.52f),
                        shin = new Color(0.16f, 0.20f, 0.36f),
                        tail = new Color(0.55f, 0.40f, 0.30f),
                        wing = new Color(0.30f, 0.30f, 0.32f),
                    };
                case WolfId:
                    return new Palette
                    {
                        skin = new Color(0.55f, 0.55f, 0.55f),
                        tunic = new Color(0.38f, 0.38f, 0.40f),
                        trousers = new Color(0.30f, 0.30f, 0.32f),
                        shin = new Color(0.20f, 0.20f, 0.22f),
                        tail = new Color(0.45f, 0.40f, 0.35f),
                        wing = new Color(0.30f, 0.30f, 0.32f),
                    };
                case FoxSpiritId:
                    return new Palette
                    {
                        skin = new Color(0.96f, 0.88f, 0.78f),
                        tunic = new Color(0.85f, 0.45f, 0.20f),
                        trousers = new Color(0.65f, 0.32f, 0.15f),
                        shin = new Color(0.40f, 0.20f, 0.10f),
                        tail = new Color(0.92f, 0.55f, 0.25f),
                        wing = new Color(0.30f, 0.30f, 0.32f),
                    };
                case RabbitId:
                    return new Palette
                    {
                        skin = new Color(0.91f, 0.84f, 0.72f),
                        tunic = new Color(0.72f, 0.58f, 0.44f),
                        trousers = new Color(0.55f, 0.42f, 0.30f),
                        shin = new Color(0.36f, 0.28f, 0.20f),
                        tail = new Color(0.95f, 0.92f, 0.85f),
                        wing = new Color(0.30f, 0.30f, 0.32f),
                    };
                case BoarId:
                    // Wild boar — bristly dark brown coarse fur, ivory tusks (hero), heavy mob.
                    return new Palette
                    {
                        skin = new Color(0.45f, 0.36f, 0.28f),
                        tunic = new Color(0.30f, 0.22f, 0.16f),
                        trousers = new Color(0.22f, 0.16f, 0.12f),
                        shin = new Color(0.14f, 0.10f, 0.08f),
                        tail = new Color(0.18f, 0.14f, 0.10f),
                        wing = new Color(0.30f, 0.30f, 0.32f),
                    };
                case DeerSpiritId:
                    // Spirit deer — cream fawn fur with subtle warm tones, white tail flick, antler
                    // ivory rendered ở head sprite (Tail color không phải antler — Tail = stub flick).
                    return new Palette
                    {
                        skin = new Color(0.92f, 0.85f, 0.72f),
                        tunic = new Color(0.72f, 0.60f, 0.42f),
                        trousers = new Color(0.55f, 0.45f, 0.30f),
                        shin = new Color(0.38f, 0.30f, 0.20f),
                        tail = new Color(0.96f, 0.93f, 0.86f),
                        wing = new Color(0.30f, 0.30f, 0.32f),
                    };
                case BossId:
                    // Boss Hắc Vương — humanoid villain overlord. Black robe with crimson trim
                    // (menacing dark palette → contrast with player blue tunic). Pale ivory skin
                    // (regal/imperious). Tail unused (humanoid). Mirror Player limb structure.
                    return new Palette
                    {
                        skin = new Color(0.88f, 0.82f, 0.74f),
                        tunic = new Color(0.18f, 0.10f, 0.14f),
                        trousers = new Color(0.12f, 0.06f, 0.10f),
                        shin = new Color(0.08f, 0.04f, 0.06f),
                        tail = new Color(0.55f, 0.10f, 0.14f),
                        wing = new Color(0.30f, 0.30f, 0.32f),
                    };
                case CrowId:
                    // Crow — glossy black corvid. Wing đậm hơn skin/tunic (slick feather sheen).
                    // skin = beak/feet (charcoal gray), tunic = body plumage (deeper black),
                    // wing = primary flight feathers (jet black với hint xanh đêm để đọc rõ
                    // dark amplitude vs torso). Tail short stub (stub flick — same as torso).
                    return new Palette
                    {
                        skin = new Color(0.18f, 0.18f, 0.20f),
                        tunic = new Color(0.10f, 0.10f, 0.12f),
                        trousers = new Color(0.10f, 0.10f, 0.12f),
                        shin = new Color(0.06f, 0.06f, 0.08f),
                        tail = new Color(0.10f, 0.10f, 0.12f),
                        wing = new Color(0.08f, 0.09f, 0.14f),
                    };
                case BatId:
                    // Bat — leathery dark-brown mammal (warm dark vs Crow's cool black). Wing =
                    // membrane stretched giữa elongated finger bones, tinge red-brown blood vessel
                    // (NOT pure black — distinct from Crow để player phân biệt 2 flying mob).
                    // skin = ear/snout/face fur, tunic = body fur, wing = membrane (slightly darker
                    // hơn fur nhưng warm hue, NOT cool jet như Crow).
                    return new Palette
                    {
                        skin = new Color(0.34f, 0.22f, 0.18f),
                        tunic = new Color(0.28f, 0.18f, 0.14f),
                        trousers = new Color(0.24f, 0.16f, 0.12f),
                        shin = new Color(0.18f, 0.12f, 0.09f),
                        tail = new Color(0.24f, 0.16f, 0.12f),
                        wing = new Color(0.20f, 0.12f, 0.11f),
                    };
                default:
                    return new Palette
                    {
                        skin = new Color(0.85f, 0.75f, 0.65f),
                        tunic = new Color(0.50f, 0.50f, 0.55f),
                        trousers = new Color(0.35f, 0.35f, 0.40f),
                        shin = new Color(0.20f, 0.20f, 0.25f),
                        tail = new Color(0.60f, 0.50f, 0.40f),
                        wing = new Color(0.30f, 0.30f, 0.32f),
                    };
            }
        }

        /// <summary>
        /// Rect spec (w_px, h_px) cho mỗi PuppetRole. Ratio xấp xỉ humanoid khoảng 1.6-1.8u tall.
        /// All sprites import ở 64 PPU → world height 0.6u (head) - 1.25u (torso).
        /// </summary>
        public static (int w, int h) RectFor(CharacterArtSpec.PuppetRole role)
        {
            switch (role)
            {
                case CharacterArtSpec.PuppetRole.Head: return (40, 40);
                case CharacterArtSpec.PuppetRole.Torso: return (52, 80);
                case CharacterArtSpec.PuppetRole.ArmLeft:
                case CharacterArtSpec.PuppetRole.ArmRight: return (16, 56);
                case CharacterArtSpec.PuppetRole.ForearmLeft:
                case CharacterArtSpec.PuppetRole.ForearmRight: return (14, 44);
                case CharacterArtSpec.PuppetRole.LegLeft:
                case CharacterArtSpec.PuppetRole.LegRight: return (18, 60);
                case CharacterArtSpec.PuppetRole.ShinLeft:
                case CharacterArtSpec.PuppetRole.ShinRight: return (16, 44);
                case CharacterArtSpec.PuppetRole.Tail: return (50, 18);
                // Phase 3 wings: wide span (54 px) + medium-thin chord (28 px). Rotates quanh
                // shoulder pivot — amplitude 50° cần silhouette đủ dài để đọc được motion.
                case CharacterArtSpec.PuppetRole.WingLeft:
                case CharacterArtSpec.PuppetRole.WingRight: return (54, 28);
                // Phase 4 body segments (Snake). Oblong horizontal shape (wider than tall) +
                // tapering tail-ward: seg1 (neck) widest → seg4 (tail) thinnest. Each segment
                // pivot ở junction trước, rotation quanh Z propagate qua chain hierarchy.
                case CharacterArtSpec.PuppetRole.BodySegment1: return (38, 26);
                case CharacterArtSpec.PuppetRole.BodySegment2: return (36, 26);
                case CharacterArtSpec.PuppetRole.BodySegment3: return (32, 24);
                case CharacterArtSpec.PuppetRole.BodySegment4: return (26, 20);
                default: return (32, 32);
            }
        }

        /// <summary>
        /// Color cho mỗi PuppetRole từ palette. Skin tone cho exposed flesh (head, forearm),
        /// tunic cho upper body covered (torso, arm sleeve), trousers/shin cho lower limbs.
        /// </summary>
        public static Color ColorFor(CharacterArtSpec.PuppetRole role, in Palette palette)
        {
            switch (role)
            {
                case CharacterArtSpec.PuppetRole.Head: return palette.skin;
                case CharacterArtSpec.PuppetRole.Torso: return palette.tunic;
                case CharacterArtSpec.PuppetRole.ArmLeft:
                case CharacterArtSpec.PuppetRole.ArmRight: return palette.tunic;
                case CharacterArtSpec.PuppetRole.ForearmLeft:
                case CharacterArtSpec.PuppetRole.ForearmRight: return palette.skin;
                case CharacterArtSpec.PuppetRole.LegLeft:
                case CharacterArtSpec.PuppetRole.LegRight: return palette.trousers;
                case CharacterArtSpec.PuppetRole.ShinLeft:
                case CharacterArtSpec.PuppetRole.ShinRight: return palette.shin;
                case CharacterArtSpec.PuppetRole.Tail: return palette.tail;
                case CharacterArtSpec.PuppetRole.WingLeft:
                case CharacterArtSpec.PuppetRole.WingRight: return palette.wing;
                // Phase 4 body segments (Snake). Reuse tunic color (body fur) — Phase 4 PR #116
                // sẽ add SnakeId palette với distinct scale color (currently fallback tunic).
                case CharacterArtSpec.PuppetRole.BodySegment1:
                case CharacterArtSpec.PuppetRole.BodySegment2:
                case CharacterArtSpec.PuppetRole.BodySegment3:
                case CharacterArtSpec.PuppetRole.BodySegment4: return palette.tunic;
                default: return Color.magenta; // unknown → loud
            }
        }

        /// <summary>
        /// Roles to generate cho character. Defaults = full 10-joint humanoid set. Caller có thể
        /// skip Tail nếu character không có tail (Player) và opt-in Wings cho flying mob
        /// (Crow / Bat — Phase 3). Forearm/shin always-on để showcase L2 elbow/knee bend trong demo.
        /// </summary>
        public static IEnumerable<CharacterArtSpec.PuppetRole> DefaultRoles(bool includeTail)
        {
            return DefaultRoles(includeTail, includeWings: false, includeBodySegments: false);
        }

        /// <summary>
        /// Phase 3 overload — opt-in wing roles cho flying mob. <paramref name="includeWings"/>=true
        /// adds <see cref="CharacterArtSpec.PuppetRole.WingLeft"/> + <see cref="CharacterArtSpec.PuppetRole.WingRight"/>.
        /// Crow / Bat keep full quadruped joints (head/torso/arms/legs/tail) plus wings
        /// to maintain consistent 6 PNG flat / 18 PNG multi-dir count với Wolf/Fox quadruped tier.
        /// </summary>
        public static IEnumerable<CharacterArtSpec.PuppetRole> DefaultRoles(bool includeTail, bool includeWings)
        {
            return DefaultRoles(includeTail, includeWings, includeBodySegments: false);
        }

        /// <summary>
        /// Phase 4 overload — opt-in body segment roles cho serpentine mob (Snake).
        /// <paramref name="includeBodySegments"/>=true adds BodySegment1..4 (chain head→tail).
        /// Snake puppet: head + 4 segments (no arm/leg/wing/tail) — caller sẽ pass
        /// includeTail=false, includeWings=false, includeBodySegments=true. Currently DefaultRoles
        /// always emit limb set; Snake prefab sẽ filter qua một path khác (deferred Phase 4 #116).
        /// </summary>
        public static IEnumerable<CharacterArtSpec.PuppetRole> DefaultRoles(bool includeTail,
            bool includeWings, bool includeBodySegments)
        {
            yield return CharacterArtSpec.PuppetRole.Head;
            yield return CharacterArtSpec.PuppetRole.Torso;
            yield return CharacterArtSpec.PuppetRole.ArmLeft;
            yield return CharacterArtSpec.PuppetRole.ArmRight;
            yield return CharacterArtSpec.PuppetRole.ForearmLeft;
            yield return CharacterArtSpec.PuppetRole.ForearmRight;
            yield return CharacterArtSpec.PuppetRole.LegLeft;
            yield return CharacterArtSpec.PuppetRole.LegRight;
            yield return CharacterArtSpec.PuppetRole.ShinLeft;
            yield return CharacterArtSpec.PuppetRole.ShinRight;
            if (includeTail) yield return CharacterArtSpec.PuppetRole.Tail;
            if (includeWings)
            {
                yield return CharacterArtSpec.PuppetRole.WingLeft;
                yield return CharacterArtSpec.PuppetRole.WingRight;
            }
            if (includeBodySegments)
            {
                yield return CharacterArtSpec.PuppetRole.BodySegment1;
                yield return CharacterArtSpec.PuppetRole.BodySegment2;
                yield return CharacterArtSpec.PuppetRole.BodySegment3;
                yield return CharacterArtSpec.PuppetRole.BodySegment4;
            }
        }
    }
}
