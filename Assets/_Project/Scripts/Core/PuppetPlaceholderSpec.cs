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
        /// Per-character color palette. Skin / sleeve / trousers / tail riêng.
        /// </summary>
        public struct Palette
        {
            public Color skin;     // head + forearm fill
            public Color tunic;    // torso + arm sleeve
            public Color trousers; // leg
            public Color shin;     // shin (darker than trousers)
            public Color tail;     // tail (mob only)
        }

        public const string PlayerId = "player";
        public const string WolfId = "wolf";
        public const string FoxSpiritId = "fox_spirit";

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
                    };
                case WolfId:
                    return new Palette
                    {
                        skin = new Color(0.55f, 0.55f, 0.55f),
                        tunic = new Color(0.38f, 0.38f, 0.40f),
                        trousers = new Color(0.30f, 0.30f, 0.32f),
                        shin = new Color(0.20f, 0.20f, 0.22f),
                        tail = new Color(0.45f, 0.40f, 0.35f),
                    };
                case FoxSpiritId:
                    return new Palette
                    {
                        skin = new Color(0.96f, 0.88f, 0.78f),
                        tunic = new Color(0.85f, 0.45f, 0.20f),
                        trousers = new Color(0.65f, 0.32f, 0.15f),
                        shin = new Color(0.40f, 0.20f, 0.10f),
                        tail = new Color(0.92f, 0.55f, 0.25f),
                    };
                default:
                    return new Palette
                    {
                        skin = new Color(0.85f, 0.75f, 0.65f),
                        tunic = new Color(0.50f, 0.50f, 0.55f),
                        trousers = new Color(0.35f, 0.35f, 0.40f),
                        shin = new Color(0.20f, 0.20f, 0.25f),
                        tail = new Color(0.60f, 0.50f, 0.40f),
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
                default: return Color.magenta; // unknown → loud
            }
        }

        /// <summary>
        /// Roles to generate cho character. Defaults = full 13-joint set. Caller có thể skip
        /// Tail nếu character không có tail (Player). Forearm/shin always-on để showcase L2
        /// elbow/knee bend trong demo.
        /// </summary>
        public static IEnumerable<CharacterArtSpec.PuppetRole> DefaultRoles(bool includeTail)
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
        }
    }
}
