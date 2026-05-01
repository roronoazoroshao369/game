namespace WildernessCultivation.Core
{
    /// <summary>
    /// Pure spec cho character puppet art: user gen PNG riêng từng body part (head, torso,
    /// arms, legs) → drop vào <c>Art/Characters/{characterId}/</c> → CharacterArtImporter pick up.
    ///
    /// Pipeline reuse pattern của <see cref="ResourceArtSpec"/> nhưng cho multi-piece character.
    /// EditMode test reach trực tiếp (no asmdef gymnastics).
    /// </summary>
    public static class CharacterArtSpec
    {
        public const string ArtCharactersRoot = "Assets/_Project/Art/Characters";

        // Filename → role enum (alphabetical pivot deterministic). Caller dùng
        // <see cref="TryParseRole"/> để map filename → <see cref="PuppetRole"/> ổn định.
        public const string FilenameHead = "head";
        public const string FilenameTorso = "torso";
        public const string FilenameArmLeft = "arm_left";
        public const string FilenameArmRight = "arm_right";
        public const string FilenameLegLeft = "leg_left";
        public const string FilenameLegRight = "leg_right";
        public const string FilenameTail = "tail";

        /// <summary>
        /// Body part roles trong puppet hierarchy. Order = sortingOrder default (back→front).
        /// Tail vẽ phía sau body, leg dưới torso, arm trên (cover body), head cao nhất.
        /// </summary>
        public enum PuppetRole
        {
            Tail = 0,        // optional, sau body
            LegLeft = 1,
            LegRight = 2,
            Torso = 3,
            ArmLeft = 4,
            ArmRight = 5,
            Head = 6,
            Unknown = -1
        }

        /// <summary>
        /// Parse filename (without extension, lowercase) → role. "arm_left" → ArmLeft.
        /// Defensive: trim path + extension trước khi gọi.
        /// </summary>
        public static PuppetRole TryParseRole(string filenameNoExt)
        {
            if (string.IsNullOrEmpty(filenameNoExt)) return PuppetRole.Unknown;
            string lower = filenameNoExt.ToLowerInvariant();
            if (lower == FilenameHead) return PuppetRole.Head;
            if (lower == FilenameTorso) return PuppetRole.Torso;
            if (lower == FilenameArmLeft) return PuppetRole.ArmLeft;
            if (lower == FilenameArmRight) return PuppetRole.ArmRight;
            if (lower == FilenameLegLeft) return PuppetRole.LegLeft;
            if (lower == FilenameLegRight) return PuppetRole.LegRight;
            if (lower == FilenameTail) return PuppetRole.Tail;
            return PuppetRole.Unknown;
        }

        /// <summary>
        /// Required parts cho puppet build → KHÔNG có một trong số này thì caller fallback
        /// single-sprite. Tail / arms optional cho mob (vd rabbit không có arm).
        /// </summary>
        public static bool IsRequiredForPuppet(PuppetRole role)
        {
            return role == PuppetRole.Torso || role == PuppetRole.Head;
        }
    }
}
