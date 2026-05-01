namespace WildernessCultivation.Core
{
    /// <summary>
    /// Pure spec cho character puppet art: user gen PNG riêng từng body part (head, torso,
    /// arms, legs) → drop vào <c>Art/Characters/{characterId}/</c> → CharacterArtImporter pick up.
    ///
    /// L3 multi-direction (PR J): folder layout có thể là **flat** (legacy side-only —
    /// PR G/H/I) hoặc **directional** (subfolders <c>E/</c>, <c>N/</c>, <c>S/</c>).
    /// Khi multi-dir → PuppetAnimController swap sprite refs trên child SpriteRenderer
    /// theo velocity angle. West = flip horizontal của East (free, không cần art).
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

        // Direction subfolder names (lowercase, single char). Importer match
        // case-insensitive — user có thể dùng "E"/"e"/"East" tùy thích.
        public const string DirectionFolderEast = "e";
        public const string DirectionFolderNorth = "n";
        public const string DirectionFolderSouth = "s";

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
        /// Facing direction cho multi-directional puppet. East = side-view facing right
        /// (default + legacy single-dir). North = back-view (lưng quay về camera). South =
        /// front-view (mặt thẳng vào camera). West dùng East sprite + flipX (free, không
        /// cần art riêng → giảm 25% art cost). Diagonal velocity (NE/SE/NW/SW) snap về
        /// nearest cardinal qua <see cref="ComputeDirectionFromVelocity"/>.
        ///
        /// Order = enum value index trong sprite array của PuppetAnimController.
        /// </summary>
        public enum PuppetDirection
        {
            East = 0,
            North = 1,
            South = 2,
            West = 3, // special: render bằng East sprite + flipX
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
        /// Parse subfolder name → direction. Match case-insensitive: "e"/"E"/"east" all OK.
        /// Returns -1 nếu không phải dir folder (vd folder con khác).
        /// </summary>
        public static PuppetDirection TryParseDirection(string folderName, out bool ok)
        {
            ok = false;
            if (string.IsNullOrEmpty(folderName)) return PuppetDirection.East;
            string lower = folderName.ToLowerInvariant();
            if (lower == DirectionFolderEast || lower == "east") { ok = true; return PuppetDirection.East; }
            if (lower == DirectionFolderNorth || lower == "north") { ok = true; return PuppetDirection.North; }
            if (lower == DirectionFolderSouth || lower == "south") { ok = true; return PuppetDirection.South; }
            return PuppetDirection.East;
        }

        /// <summary>
        /// Required parts cho puppet build → KHÔNG có một trong số này thì caller fallback
        /// single-sprite. Tail / arms optional cho mob (vd rabbit không có arm).
        /// </summary>
        public static bool IsRequiredForPuppet(PuppetRole role)
        {
            return role == PuppetRole.Torso || role == PuppetRole.Head;
        }

        /// <summary>
        /// Snap velocity vector về nearest cardinal direction (E/N/S/W). Pure math,
        /// EditMode-testable.
        ///
        /// Mapping (velocity angle in degrees, atan2 convention 0°=E, 90°=N, 180°=W, -90°=S):
        /// - [-45, 45]   → East
        /// - [45, 135]   → North
        /// - [135, 180] U [-180, -135] → West
        /// - [-135, -45] → South
        ///
        /// Hysteresis qua <paramref name="hysteresisDeg"/>: chỉ đổi direction khi angle vượt
        /// boundary + hysteresis, tránh flicker khi velocity gần đường biên.
        /// Caller pass <paramref name="currentDir"/> để decide có giữ direction cũ không.
        /// </summary>
        public static PuppetDirection ComputeDirectionFromVelocity(float vx, float vy,
            PuppetDirection currentDir, float hysteresisDeg = 8f)
        {
            // Idle / very small velocity → giữ direction cũ.
            float speedSq = vx * vx + vy * vy;
            if (speedSq < 0.0001f) return currentDir;

            float angleDeg = UnityEngine.Mathf.Atan2(vy, vx) * UnityEngine.Mathf.Rad2Deg;
            return ComputeDirectionFromAngleDeg(angleDeg, currentDir, hysteresisDeg);
        }

        /// <summary>
        /// Same as <see cref="ComputeDirectionFromVelocity"/> nhưng nhận angle trực tiếp
        /// (pre-computed). Hysteresis logic: chỉ accept direction mới nếu angle vượt past
        /// boundary của current dir một khoảng &gt; hysteresis. Đỡ flicker khi velocity
        /// quay đầu sát biên.
        /// </summary>
        public static PuppetDirection ComputeDirectionFromAngleDeg(float angleDeg,
            PuppetDirection currentDir, float hysteresisDeg = 8f)
        {
            // Normalize to [-180, 180].
            while (angleDeg > 180f) angleDeg -= 360f;
            while (angleDeg < -180f) angleDeg += 360f;

            // Strict mapping (no hysteresis) — first pass.
            PuppetDirection strict;
            float abs = UnityEngine.Mathf.Abs(angleDeg);
            if (abs <= 45f) strict = PuppetDirection.East;
            else if (angleDeg > 45f && angleDeg <= 135f) strict = PuppetDirection.North;
            else if (angleDeg < -45f && angleDeg >= -135f) strict = PuppetDirection.South;
            else strict = PuppetDirection.West;

            if (strict == currentDir) return currentDir;

            // Hysteresis: chỉ accept switch nếu angle xa biên đủ. Vd current=East,
            // boundary E↔N tại 45°. Switch sang North chỉ khi angle > 45° + 8° = 53°.
            float distFromBoundary = ComputeDistanceFromBoundary(angleDeg, currentDir);
            if (distFromBoundary < hysteresisDeg) return currentDir;
            return strict;
        }

        /// <summary>
        /// Khoảng cách (degrees) từ angle hiện tại đến boundary nearest của
        /// <paramref name="currentDir"/>. Dùng cho hysteresis check.
        /// </summary>
        public static float ComputeDistanceFromBoundary(float angleDeg, PuppetDirection currentDir)
        {
            // Normalize.
            while (angleDeg > 180f) angleDeg -= 360f;
            while (angleDeg < -180f) angleDeg += 360f;

            // Boundaries của mỗi cardinal cone:
            // East:  [-45, 45]   → boundaries ±45
            // North: [45, 135]   → boundaries 45, 135
            // South: [-135, -45] → boundaries -135, -45
            // West:  [135, 180] ∪ [-180, -135] → boundaries 135, -135 (wrap)
            switch (currentDir)
            {
                case PuppetDirection.East:
                    return UnityEngine.Mathf.Min(UnityEngine.Mathf.Abs(angleDeg - 45f),
                        UnityEngine.Mathf.Abs(angleDeg + 45f));
                case PuppetDirection.North:
                    return UnityEngine.Mathf.Min(UnityEngine.Mathf.Abs(angleDeg - 45f),
                        UnityEngine.Mathf.Abs(angleDeg - 135f));
                case PuppetDirection.South:
                    return UnityEngine.Mathf.Min(UnityEngine.Mathf.Abs(angleDeg + 45f),
                        UnityEngine.Mathf.Abs(angleDeg + 135f));
                case PuppetDirection.West:
                    {
                        float d1 = UnityEngine.Mathf.Abs(angleDeg - 135f);
                        float d2 = UnityEngine.Mathf.Abs(angleDeg + 135f);
                        return UnityEngine.Mathf.Min(d1, d2);
                    }
            }
            return 0f;
        }
    }
}
