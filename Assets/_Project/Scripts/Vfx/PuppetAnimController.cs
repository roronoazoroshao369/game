using UnityEngine;
using WildernessCultivation.Core;

namespace WildernessCultivation.Vfx
{
    /// <summary>
    /// Procedural puppet animation: drive multi-piece child sprite hierarchy thay vì single sprite.
    /// User cung cấp PNG riêng từng body part (head, torso, arms, legs) → BootstrapWizard /
    /// CharacterArtImporter build prefab với child <see cref="SpriteRenderer"/> per part →
    /// PuppetAnimController rotate / offset transforms theo time.
    ///
    /// Visual feel: stylized "rigid limb" (Don't Starve / Cuphead). Mỗi part xoay quanh pivot
    /// (shoulder cho arm, hip cho leg) — KHÔNG mesh deform. Trade-off: artist authoring cost
    /// thấp (PNG riêng dễ AI-gen), perf tốt (transform writes only), KHÔNG cần Unity Animator.
    ///
    /// Implements <see cref="IMobAnim"/> nên FSM (WolfStates / RabbitStates / etc.) call qua
    /// interface không cần biết underlying là procedural / bone / puppet.
    /// </summary>
    [DisallowMultipleComponent]
    public class PuppetAnimController : MonoBehaviour, IMobAnim
    {
        [Header("Refs (auto-find theo name nếu null)")]
        [Tooltip("Wrapper transform flip X scale theo direction. Default = transform của component.")]
        public Transform spriteRoot;
        [Tooltip("Torso sprite — bob Y khi đi.")]
        public Transform torso;
        [Tooltip("Head sprite — bob theo torso, optional sub-bob.")]
        public Transform head;
        [Tooltip("Arm left transform — rotate Z theo walk phase (forward khi step).")]
        public Transform armLeft;
        [Tooltip("Arm right transform — rotate Z opposite phase với armLeft.")]
        public Transform armRight;
        [Tooltip("Leg left transform — rotate Z + Y offset theo step.")]
        public Transform legLeft;
        [Tooltip("Leg right transform — opposite phase với legLeft.")]
        public Transform legRight;
        [Tooltip("Optional tail (cho mob có tail).")]
        public Transform tail;
        [Tooltip("PR K — Forearm child of armLeft (elbow bend khi attack). Optional, null skip.")]
        public Transform forearmLeft;
        [Tooltip("PR K — Forearm child of armRight (elbow bend khi attack). Optional.")]
        public Transform forearmRight;
        [Tooltip("PR K — Shin child of legLeft (knee bend khi crouch + walk back-swing).")]
        public Transform shinLeft;
        [Tooltip("PR K — Shin child of legRight (knee bend khi crouch + walk back-swing).")]
        public Transform shinRight;
        [Tooltip("Phase 3 — Wing left transform (Crow / Bat). Optional, null skip flap.")]
        public Transform wingLeft;
        [Tooltip("Phase 3 — Wing right transform (Crow / Bat). Optional, null skip flap.")]
        public Transform wingRight;
        [Tooltip("Phase 4 — Body segment chain (Snake). Optional, null skip serpentine math. " +
                 "Slot 0 = first segment after head (closest), slot 3 = tail. Mỗi segment child " +
                 "của previous (head → seg1 → seg2 → seg3 → seg4) — Unity transform tree propagate " +
                 "rotation tự nhiên khi parent rotates.")]
        public Transform bodySegment1;
        public Transform bodySegment2;
        public Transform bodySegment3;
        public Transform bodySegment4;
        [Tooltip("Rigidbody2D đọc velocity. Auto-assign nếu null.")]
        public Rigidbody2D body;

        [Header("Walk")]
        [Tooltip("Tần số step (Hz). Player ~3, Wolf ~4, Rabbit ~6.")]
        public float walkFrequency = 3f;
        [Tooltip("Max arm swing degrees (±). 25-40 đẹp nhất.")]
        public float armSwingDeg = 30f;
        [Tooltip("Max leg swing degrees (±). 15-25.")]
        public float legSwingDeg = 20f;
        [Tooltip("Torso bob Y biên độ (units). 0.03-0.06.")]
        public float torsoBobAmplitude = 0.04f;
        [Tooltip("Speed nominal cho frequency scaling. Speed thấp hơn → step chậm dần.")]
        public float referenceSpeed = 2.5f;
        [Tooltip("Speed dưới ngưỡng = idle (no walk anim).")]
        public float movingThreshold = 0.05f;

        [Header("Idle breathing")]
        [Tooltip("Idle torso bob biên độ (subtle breathing). 0.025-0.04 = visible chest rise/fall " +
                 "(DST grade); 0.01 = barely-perceptible.")]
        public float idleBreathAmplitude = 0.03f;
        [Tooltip("Idle breath tần số (Hz). 1.0-1.4 = relaxed breathing.")]
        public float idleBreathFrequency = 1.2f;

        [Header("Lunge / Attack")]
        [Tooltip("Arm rotate forward khi attack (degrees, signed direction handled internally).")]
        public float lungeArmDeg = 60f;
        [Tooltip("Lunge duration.")]
        public float lungeDuration = 0.3f;

        [Header("Crouch")]
        [Tooltip("Torso lower Y khi crouch.")]
        public float crouchTorsoYOffset = -0.08f;
        [Tooltip("Lerp speed crouch transition.")]
        public float crouchLerpRate = 12f;

        [Header("Joints — elbow / knee (PR K, L2)")]
        [Tooltip("Max forearm bend (deg) khi attack lunge. 30-50 = stylized punch.")]
        public float elbowBendDeg = 40f;
        [Tooltip("Max shin bend (deg) khi crouch knee bend.")]
        public float kneeCrouchBendDeg = 35f;
        [Tooltip("Max shin bend (deg) khi walk back-swing (foot lift). 0 = disable, 10-20 đẹp.")]
        public float kneeWalkBendDeg = 12f;

        [Header("Tail (optional)")]
        [Tooltip("Tail sway tần số.")]
        public float tailSwayFrequency = 1.5f;
        [Tooltip("Tail sway max degrees.")]
        public float tailSwayDeg = 12f;

        [Header("Wing flap (Phase 3 — Crow / Bat)")]
        [Tooltip("Flap tần số (Hz). Crow ~6, Bat ~7.5. Independent of walkFrequency — " +
                 "flying mob vỗ cánh cả khi idle (hovering) và khi di chuyển.")]
        public float flapFrequency = 6f;
        [Tooltip("Max wing rotation (°). 45-60 = cánh xoải loại với amplitude (vs arm 25-40).")]
        public float wingFlapAmplitudeDeg = 50f;
        [Tooltip("True → wing flap không bị walkFrequency gating (always-on cho flying mob). " +
                 "False → chỉ flap khi moving (bộ truyện nhân vật).")]
        public bool flapAlwaysOn = true;

        [Header("Serpentine (Phase 4 — Snake)")]
        [Tooltip("Serpentine wave tần số (Hz). Snake ~2-3, baseline 2.5Hz. Independent of " +
                 "walkFrequency — body chain ripple liên tục cả khi idle (subtle wiggle) và moving " +
                 "(full S-curve).")]
        public float serpentineFrequency = 2.5f;
        [Tooltip("Max body segment rotation tail (°). Head segment ~20% of this, tail full. " +
                 "30-45 đẹp = stylized S-curve.")]
        public float serpentineMaxAmplitudeDeg = 35f;
        [Tooltip("Phase delay per segment (radians). Larger → spread S-curve khắp body, " +
                 "smaller → tight wave. 0.5-0.8 = readable S-shape; 1.0+ = standing wave.")]
        public float serpentineWaveSpreadPerSegment = 0.6f;
        [Tooltip("True → serpentine luôn-on (Snake always wiggle). False → chỉ wiggle khi moving.")]
        public bool serpentineAlwaysOn = true;

        [Header("Side-view occlusion (PR 2 — anti rời rạc)")]
        [Tooltip("True → khi direction = E hoặc W, far-side arm/leg (away-from-camera) " +
                 "render BEHIND torso + scale.x shrink (perspective depth). False = legacy " +
                 "co-planar render (cả 2 cánh tay đè side-by-side, cảm giác rời rạc).")]
        public bool enableSideViewOcclusion = true;
        [Tooltip("Scale.x cho far limb trong side view. < 1 = thu nhỏ (perspective). 0.92 default. " +
                 "1.0 = disable scale shrink.")]
        public float farLimbScale = 0.92f;
        [Tooltip("SortingOrder offset cho far limb (subtract từ near-side default). Negative = " +
                 "render sau body. -2 default (giữ far limb phía dưới torso bằng cách đẩy " +
                 "sortingOrder về phía trước torso).")]
        public int farLimbSortingOffset = -2;
        [Tooltip("True (default) → khi direction = N hoặc S (front/back view), tắt render " +
                 "arm/forearm sprites — để torso PNG (đã có sleeves baked) own silhouette. " +
                 "Theo DST convention front-view không articulate cánh tay riêng. " +
                 "False = giữ arm sprites visible cho mob có art tách rời (Wolf, FoxSpirit).")]
        public bool hideArmsInFrontBackView = true;

        [Header("Multi-direction sprites (PR J — L3+)")]
        [Tooltip("Sprite arrays indexed by PuppetDirection enum value (0=E, 1=N, 2=S, 3=W). " +
                 "Null entries → fallback East sprite. West dirty render bằng East + flipX. " +
                 "BootstrapWizard wire arrays tự động từ CharacterArtImporter (Editor) — KHÔNG " +
                 "populate runtime. Empty array → single-dir mode (legacy PR G/H/I).")]
        public Sprite[] headSpritesByDir;
        public Sprite[] torsoSpritesByDir;
        public Sprite[] armLeftSpritesByDir;
        public Sprite[] armRightSpritesByDir;
        public Sprite[] legLeftSpritesByDir;
        public Sprite[] legRightSpritesByDir;
        public Sprite[] tailSpritesByDir;
        [Tooltip("PR K — Forearm sprite arrays (multi-dir). Null entries → forearm part missing, skip render.")]
        public Sprite[] forearmLeftSpritesByDir;
        public Sprite[] forearmRightSpritesByDir;
        public Sprite[] shinLeftSpritesByDir;
        public Sprite[] shinRightSpritesByDir;
        [Tooltip("Phase 3 — Wing sprite arrays (multi-dir). Null entries → wing part missing, skip render.")]
        public Sprite[] wingLeftSpritesByDir;
        public Sprite[] wingRightSpritesByDir;
        [Tooltip("Phase 4 — Body segment sprite arrays (multi-dir). Null entries → segment missing, skip render.")]
        public Sprite[] bodySegment1SpritesByDir;
        public Sprite[] bodySegment2SpritesByDir;
        public Sprite[] bodySegment3SpritesByDir;
        public Sprite[] bodySegment4SpritesByDir;
        [Tooltip("Hysteresis (degrees) cho direction snap — tránh flicker khi velocity gần biên E↔N / E↔S.")]
        public float directionHysteresisDeg = 8f;

        // Cached base local rotations / positions (init from inspector / scene-time pose).
        Quaternion baseArmLeftRot, baseArmRightRot, baseLegLeftRot, baseLegRightRot, baseTailRot;
        Quaternion baseForearmLeftRot, baseForearmRightRot, baseShinLeftRot, baseShinRightRot;
        Quaternion baseWingLeftRot, baseWingRightRot;
        Quaternion baseBodySegment1Rot, baseBodySegment2Rot, baseBodySegment3Rot, baseBodySegment4Rot;
        Vector3 baseTorsoPos, baseHeadPos;
        Vector3 baseSpriteRootScale;

        // Cached SpriteRenderers từ body part transforms — dung sprite swap khi đổi direction.
        SpriteRenderer headRenderer, torsoRenderer, armLeftRenderer, armRightRenderer,
            legLeftRenderer, legRightRenderer, tailRenderer;
        SpriteRenderer forearmLeftRenderer, forearmRightRenderer, shinLeftRenderer, shinRightRenderer;
        SpriteRenderer wingLeftRenderer, wingRightRenderer;
        SpriteRenderer bodySegment1Renderer, bodySegment2Renderer, bodySegment3Renderer, bodySegment4Renderer;

        // Cached base sortingOrder + parent scale.x cho side-view occlusion (PR 2).
        // Lưu giá trị gốc tại Awake (set bởi BootstrapWizard.BuildPuppetHierarchy) → restore
        // khi direction đổi từ side (E/W) → front/back (N/S).
        int baseArmLeftOrder, baseArmRightOrder, baseLegLeftOrder, baseLegRightOrder;
        int baseForearmLeftOrder, baseForearmRightOrder, baseShinLeftOrder, baseShinRightOrder;
        Vector3 baseArmLeftScale, baseArmRightScale, baseLegLeftScale, baseLegRightScale;

        CharacterArtSpec.PuppetDirection currentDir = CharacterArtSpec.PuppetDirection.East;

        bool crouching;
        float currentCrouchY;
        float lungeStartTime = -1f;
        Vector2 lungeDir;
        float squashStartTime = -1f;
        float squashDuration;

        void Awake()
        {
            if (spriteRoot == null) spriteRoot = transform;
            if (body == null) body = GetComponent<Rigidbody2D>();
            CacheBasePose();
            CacheRenderers();
            // Apply once at start để initial side-view depth + arm visibility correct
            // (currentDir defaults East → arms visible by default).
            ApplySideViewOcclusion(currentDir);
            ApplyArmVisibility(currentDir);
        }

        void CacheBasePose()
        {
            if (armLeft != null) baseArmLeftRot = armLeft.localRotation;
            if (armRight != null) baseArmRightRot = armRight.localRotation;
            if (legLeft != null) baseLegLeftRot = legLeft.localRotation;
            if (legRight != null) baseLegRightRot = legRight.localRotation;
            if (tail != null) baseTailRot = tail.localRotation;
            if (forearmLeft != null) baseForearmLeftRot = forearmLeft.localRotation;
            if (forearmRight != null) baseForearmRightRot = forearmRight.localRotation;
            if (shinLeft != null) baseShinLeftRot = shinLeft.localRotation;
            if (shinRight != null) baseShinRightRot = shinRight.localRotation;
            if (wingLeft != null) baseWingLeftRot = wingLeft.localRotation;
            if (wingRight != null) baseWingRightRot = wingRight.localRotation;
            if (bodySegment1 != null) baseBodySegment1Rot = bodySegment1.localRotation;
            if (bodySegment2 != null) baseBodySegment2Rot = bodySegment2.localRotation;
            if (bodySegment3 != null) baseBodySegment3Rot = bodySegment3.localRotation;
            if (bodySegment4 != null) baseBodySegment4Rot = bodySegment4.localRotation;
            if (torso != null) baseTorsoPos = torso.localPosition;
            if (head != null) baseHeadPos = head.localPosition;
            if (spriteRoot != null) baseSpriteRootScale = spriteRoot.localScale;
        }

        void CacheRenderers()
        {
            if (head != null) headRenderer = head.GetComponent<SpriteRenderer>();
            if (torso != null) torsoRenderer = torso.GetComponent<SpriteRenderer>();
            if (armLeft != null) armLeftRenderer = armLeft.GetComponent<SpriteRenderer>();
            if (armRight != null) armRightRenderer = armRight.GetComponent<SpriteRenderer>();
            if (legLeft != null) legLeftRenderer = legLeft.GetComponent<SpriteRenderer>();
            if (legRight != null) legRightRenderer = legRight.GetComponent<SpriteRenderer>();
            if (tail != null) tailRenderer = tail.GetComponent<SpriteRenderer>();
            if (forearmLeft != null) forearmLeftRenderer = forearmLeft.GetComponent<SpriteRenderer>();
            if (forearmRight != null) forearmRightRenderer = forearmRight.GetComponent<SpriteRenderer>();
            if (shinLeft != null) shinLeftRenderer = shinLeft.GetComponent<SpriteRenderer>();
            if (shinRight != null) shinRightRenderer = shinRight.GetComponent<SpriteRenderer>();
            if (wingLeft != null) wingLeftRenderer = wingLeft.GetComponent<SpriteRenderer>();
            if (wingRight != null) wingRightRenderer = wingRight.GetComponent<SpriteRenderer>();
            if (bodySegment1 != null) bodySegment1Renderer = bodySegment1.GetComponent<SpriteRenderer>();
            if (bodySegment2 != null) bodySegment2Renderer = bodySegment2.GetComponent<SpriteRenderer>();
            if (bodySegment3 != null) bodySegment3Renderer = bodySegment3.GetComponent<SpriteRenderer>();
            if (bodySegment4 != null) bodySegment4Renderer = bodySegment4.GetComponent<SpriteRenderer>();

            // Cache base sortingOrder + scale cho side-view occlusion (PR 2).
            if (armLeftRenderer != null) baseArmLeftOrder = armLeftRenderer.sortingOrder;
            if (armRightRenderer != null) baseArmRightOrder = armRightRenderer.sortingOrder;
            if (legLeftRenderer != null) baseLegLeftOrder = legLeftRenderer.sortingOrder;
            if (legRightRenderer != null) baseLegRightOrder = legRightRenderer.sortingOrder;
            if (forearmLeftRenderer != null) baseForearmLeftOrder = forearmLeftRenderer.sortingOrder;
            if (forearmRightRenderer != null) baseForearmRightOrder = forearmRightRenderer.sortingOrder;
            if (shinLeftRenderer != null) baseShinLeftOrder = shinLeftRenderer.sortingOrder;
            if (shinRightRenderer != null) baseShinRightOrder = shinRightRenderer.sortingOrder;
            if (armLeft != null) baseArmLeftScale = armLeft.localScale;
            if (armRight != null) baseArmRightScale = armRight.localScale;
            if (legLeft != null) baseLegLeftScale = legLeft.localScale;
            if (legRight != null) baseLegRightScale = legRight.localScale;
        }

        /// <summary>
        /// True nếu ít nhất 1 part có sprite mới cho direction khác E (N/S) → enable multi-dir
        /// switching. Else fallback legacy side-only mode (E only + flipX).
        /// </summary>
        bool HasMultiDirSprites()
        {
            return ArrayHasNonEastEntry(headSpritesByDir)
                || ArrayHasNonEastEntry(torsoSpritesByDir)
                || ArrayHasNonEastEntry(armLeftSpritesByDir)
                || ArrayHasNonEastEntry(armRightSpritesByDir)
                || ArrayHasNonEastEntry(legLeftSpritesByDir)
                || ArrayHasNonEastEntry(legRightSpritesByDir)
                || ArrayHasNonEastEntry(tailSpritesByDir)
                || ArrayHasNonEastEntry(forearmLeftSpritesByDir)
                || ArrayHasNonEastEntry(forearmRightSpritesByDir)
                || ArrayHasNonEastEntry(shinLeftSpritesByDir)
                || ArrayHasNonEastEntry(shinRightSpritesByDir)
                || ArrayHasNonEastEntry(wingLeftSpritesByDir)
                || ArrayHasNonEastEntry(wingRightSpritesByDir)
                || ArrayHasNonEastEntry(bodySegment1SpritesByDir)
                || ArrayHasNonEastEntry(bodySegment2SpritesByDir)
                || ArrayHasNonEastEntry(bodySegment3SpritesByDir)
                || ArrayHasNonEastEntry(bodySegment4SpritesByDir);
        }

        static bool ArrayHasNonEastEntry(Sprite[] arr)
        {
            if (arr == null || arr.Length < 2) return false;
            for (int i = 1; i < arr.Length; i++) if (arr[i] != null) return true;
            return false;
        }

        void Update()
        {
            float dt = Time.deltaTime;
            float t = Time.time;

            Vector2 vel = body != null ? body.velocity : Vector2.zero;
            float speed = vel.magnitude;
            bool moving = speed > movingThreshold;

            // Direction snap (multi-dir) + sprite swap. Side-only mode → fallback flipX legacy.
            bool multiDir = HasMultiDirSprites();
            if (multiDir && moving)
            {
                var newDir = CharacterArtSpec.ComputeDirectionFromVelocity(
                    vel.x, vel.y, currentDir, directionHysteresisDeg);
                if (newDir != currentDir)
                {
                    currentDir = newDir;
                    ApplyDirectionSprites(currentDir);
                }
            }

            // Sprite root direction flip:
            // - Multi-dir mode: flip chỉ khi West; N/S/E giữ scale.x positive.
            // - Side-only mode (legacy): flipX theo vel.x (PR G/H/I behavior).
            if (spriteRoot != null)
            {
                if (multiDir)
                {
                    bool flipped = currentDir == CharacterArtSpec.PuppetDirection.West;
                    var s = spriteRoot.localScale;
                    spriteRoot.localScale = new Vector3(
                        (flipped ? -1f : 1f) * Mathf.Abs(s.x), s.y, s.z);
                }
                else if (Mathf.Abs(vel.x) > movingThreshold)
                {
                    spriteRoot.localScale = new Vector3(
                        BoneAnimController.ComputeFlipScaleX(spriteRoot.localScale.x, vel.x, movingThreshold),
                        spriteRoot.localScale.y,
                        spriteRoot.localScale.z);
                }
            }

            // Walk phase phụ thuộc speed (slower mob → slower step).
            float speedRatio = referenceSpeed > 0f ? Mathf.Clamp(speed / referenceSpeed, 0.3f, 2f) : 1f;
            float walkPhase = moving ? t * walkFrequency * speedRatio : 0f;
            float walkSin = Mathf.Sin(walkPhase * 2f * Mathf.PI);

            // Arm/leg opposite-phase swing.
            if (moving)
            {
                if (armLeft != null)
                    armLeft.localRotation = baseArmLeftRot * Quaternion.Euler(0f, 0f, walkSin * armSwingDeg);
                if (armRight != null)
                    armRight.localRotation = baseArmRightRot * Quaternion.Euler(0f, 0f, -walkSin * armSwingDeg);
                if (legLeft != null)
                    legLeft.localRotation = baseLegLeftRot * Quaternion.Euler(0f, 0f, -walkSin * legSwingDeg);
                if (legRight != null)
                    legRight.localRotation = baseLegRightRot * Quaternion.Euler(0f, 0f, walkSin * legSwingDeg);

                // Knee bend on walk back-swing: shin rotates forward when foot lifts up
                // (leg swinging backward). Left leg back-swing = walkSin > 0; right = walkSin < 0.
                if (shinLeft != null)
                    shinLeft.localRotation = baseShinLeftRot * Quaternion.Euler(0f, 0f, ComputeWalkKneeBend(walkSin, kneeWalkBendDeg));
                if (shinRight != null)
                    shinRight.localRotation = baseShinRightRot * Quaternion.Euler(0f, 0f, ComputeWalkKneeBend(-walkSin, kneeWalkBendDeg));
            }
            else
            {
                // Reset to neutral khi idle.
                if (armLeft != null) armLeft.localRotation = baseArmLeftRot;
                if (armRight != null) armRight.localRotation = baseArmRightRot;
                if (legLeft != null) legLeft.localRotation = baseLegLeftRot;
                if (legRight != null) legRight.localRotation = baseLegRightRot;
                if (shinLeft != null) shinLeft.localRotation = baseShinLeftRot;
                if (shinRight != null) shinRight.localRotation = baseShinRightRot;
            }

            // Forearm follows arm rigidly during walk (no extra bend). Reset to neutral every
            // frame — lunge block below overrides for attacking arm.
            if (forearmLeft != null) forearmLeft.localRotation = baseForearmLeftRot;
            if (forearmRight != null) forearmRight.localRotation = baseForearmRightRot;

            // Torso bob: walking → speed-proportional sin; idle → slow breath.
            float bobY = moving
                ? walkSin * torsoBobAmplitude
                : Mathf.Sin(t * 2f * Mathf.PI * idleBreathFrequency) * idleBreathAmplitude;

            // Crouch lerp (sticky toggle).
            float crouchTarget = crouching ? crouchTorsoYOffset : 0f;
            currentCrouchY = MobAnimController.ApplyExponentialDamping(currentCrouchY, crouchTarget, crouchLerpRate, dt);

            // Knee crouch bend: proportional to current crouch Y (lerped). Apply additive
            // on top of walk back-swing bend (max wins via additive deg — in practice walk
            // bend is small and crouch state is sticky, so additive is OK).
            float crouchAmount = crouchTorsoYOffset != 0f
                ? Mathf.Clamp01(currentCrouchY / crouchTorsoYOffset)
                : 0f;
            float kneeCrouchAngle = ComputeCrouchKneeBend(crouchAmount, kneeCrouchBendDeg);
            if (shinLeft != null && kneeCrouchAngle > 0.01f)
                shinLeft.localRotation *= Quaternion.Euler(0f, 0f, kneeCrouchAngle);
            if (shinRight != null && kneeCrouchAngle > 0.01f)
                shinRight.localRotation *= Quaternion.Euler(0f, 0f, kneeCrouchAngle);

            // Squash punch — applied as scale on spriteRoot.
            float squashFactor = 1f;
            if (squashStartTime >= 0f)
            {
                float age = t - squashStartTime;
                squashFactor = MobAnimController.ComputeSquashFactor(age, squashDuration, 1.2f);
                if (age >= squashDuration) squashStartTime = -1f;
            }

            // Apply torso pos.
            if (torso != null)
            {
                torso.localPosition = new Vector3(
                    baseTorsoPos.x,
                    baseTorsoPos.y + bobY + currentCrouchY,
                    baseTorsoPos.z);
            }

            // Head follows torso (already child of torso typically), additional bob optional.
            // Skip independent head transform if head is child of torso (Unity propagate auto).

            // Tail sway (independent slow sin).
            if (tail != null)
            {
                float tailSin = Mathf.Sin(t * 2f * Mathf.PI * tailSwayFrequency);
                tail.localRotation = baseTailRot * Quaternion.Euler(0f, 0f, tailSin * tailSwayDeg);
            }

            // Wing flap (Phase 3 — Crow / Bat). Independent of walkFrequency: when
            // flapAlwaysOn, wings flap continuously (flying mob hovers + cruises). Else
            // gated by moving (walking creature with optional wings). Left + right wings
            // flap in-phase (both up + both down together — vs arm opposite-phase swing).
            bool flapping = (wingLeft != null || wingRight != null) && (flapAlwaysOn || moving);
            if (flapping)
            {
                float flapAngle = ComputeFlapAngle(t, flapFrequency, wingFlapAmplitudeDeg);
                if (wingLeft != null)
                    wingLeft.localRotation = baseWingLeftRot * Quaternion.Euler(0f, 0f, flapAngle);
                if (wingRight != null)
                    wingRight.localRotation = baseWingRightRot * Quaternion.Euler(0f, 0f, -flapAngle);
            }
            else
            {
                if (wingLeft != null) wingLeft.localRotation = baseWingLeftRot;
                if (wingRight != null) wingRight.localRotation = baseWingRightRot;
            }

            // Serpentine S-curve (Phase 4 — Snake). Body chain head → seg1 → seg2 → seg3 → seg4
            // (each child of previous). Wave travels head→tail: phase delay grows by index,
            // amplitude grows linearly (head = 1/N of max, tail = full). Each segment rotates
            // around its junction pivot — Unity transform tree propagate spatial wave automatically.
            // Independent of walkFrequency: serpentineAlwaysOn → continuous wiggle (Snake idle
            // poise still sways), else gated by moving.
            int segCount = 0;
            if (bodySegment1 != null) segCount = 1;
            if (bodySegment2 != null) segCount = 2;
            if (bodySegment3 != null) segCount = 3;
            if (bodySegment4 != null) segCount = 4;
            bool serpentine = segCount > 0 && (serpentineAlwaysOn || moving);
            if (serpentine)
            {
                if (bodySegment1 != null)
                    bodySegment1.localRotation = baseBodySegment1Rot * Quaternion.Euler(0f, 0f,
                        ComputeSerpentineAngle(t, 0, segCount, serpentineFrequency,
                            serpentineMaxAmplitudeDeg, serpentineWaveSpreadPerSegment));
                if (bodySegment2 != null)
                    bodySegment2.localRotation = baseBodySegment2Rot * Quaternion.Euler(0f, 0f,
                        ComputeSerpentineAngle(t, 1, segCount, serpentineFrequency,
                            serpentineMaxAmplitudeDeg, serpentineWaveSpreadPerSegment));
                if (bodySegment3 != null)
                    bodySegment3.localRotation = baseBodySegment3Rot * Quaternion.Euler(0f, 0f,
                        ComputeSerpentineAngle(t, 2, segCount, serpentineFrequency,
                            serpentineMaxAmplitudeDeg, serpentineWaveSpreadPerSegment));
                if (bodySegment4 != null)
                    bodySegment4.localRotation = baseBodySegment4Rot * Quaternion.Euler(0f, 0f,
                        ComputeSerpentineAngle(t, 3, segCount, serpentineFrequency,
                            serpentineMaxAmplitudeDeg, serpentineWaveSpreadPerSegment));
            }
            else
            {
                if (bodySegment1 != null) bodySegment1.localRotation = baseBodySegment1Rot;
                if (bodySegment2 != null) bodySegment2.localRotation = baseBodySegment2Rot;
                if (bodySegment3 != null) bodySegment3.localRotation = baseBodySegment3Rot;
                if (bodySegment4 != null) bodySegment4.localRotation = baseBodySegment4Rot;
            }

            // Lunge: snap arms forward then return.
            if (lungeStartTime >= 0f)
            {
                float age = t - lungeStartTime;
                float u = lungeDuration > 0f ? Mathf.Clamp01(age / lungeDuration) : 1f;
                float lungeAngle = ComputeLungeArmAngle(u, lungeArmDeg);
                // Direction: positive x → arms swing forward (right). Sign mirrors with direction.
                float signedAngle = lungeDir.x >= 0f ? -lungeAngle : lungeAngle;
                if (armRight != null)
                    armRight.localRotation = baseArmRightRot * Quaternion.Euler(0f, 0f, signedAngle);
                if (armLeft != null)
                    armLeft.localRotation = baseArmLeftRot * Quaternion.Euler(0f, 0f, signedAngle);

                // Elbow bend during punch: forearm bends back at mid-lunge (windup peak)
                // then extends straight at end. Bell curve sin(π × u) signed opposite of arm swing.
                float elbowAngle = ComputeLungeElbowBend(u, elbowBendDeg);
                float signedElbow = lungeDir.x >= 0f ? elbowAngle : -elbowAngle;
                if (forearmRight != null)
                    forearmRight.localRotation = baseForearmRightRot * Quaternion.Euler(0f, 0f, signedElbow);
                if (forearmLeft != null)
                    forearmLeft.localRotation = baseForearmLeftRot * Quaternion.Euler(0f, 0f, signedElbow);
                if (age >= lungeDuration) lungeStartTime = -1f;
            }

            // Squash applied on spriteRoot scale (preserves flip sign).
            if (spriteRoot != null && squashStartTime >= 0f)
            {
                float sxSign = Mathf.Sign(spriteRoot.localScale.x);
                spriteRoot.localScale = new Vector3(
                    Mathf.Abs(baseSpriteRootScale.x) * squashFactor * sxSign,
                    baseSpriteRootScale.y * squashFactor,
                    baseSpriteRootScale.z);
            }
        }

        // ============ Direction sprite swap ============

        /// <summary>
        /// Apply sprite swap cho all body parts từ per-role array ứng với <paramref name="dir"/>.
        /// West fallback: render bằng East sprite (spriteRoot.flipX handle bởi caller). Nếu
        /// dir không có art → fallback East sprite (đảm bảo luuôn có sprite render).
        /// </summary>
        public void ApplyDirectionSprites(CharacterArtSpec.PuppetDirection dir)
        {
            // West dirty: render East sprite (flipX handled in spriteRoot).
            int idx = dir == CharacterArtSpec.PuppetDirection.West
                ? (int)CharacterArtSpec.PuppetDirection.East
                : (int)dir;
            ApplySpriteFromArray(headRenderer, headSpritesByDir, idx);
            ApplySpriteFromArray(torsoRenderer, torsoSpritesByDir, idx);
            ApplySpriteFromArray(armLeftRenderer, armLeftSpritesByDir, idx);
            ApplySpriteFromArray(armRightRenderer, armRightSpritesByDir, idx);
            ApplySpriteFromArray(legLeftRenderer, legLeftSpritesByDir, idx);
            ApplySpriteFromArray(legRightRenderer, legRightSpritesByDir, idx);
            ApplySpriteFromArray(tailRenderer, tailSpritesByDir, idx);
            ApplySpriteFromArray(forearmLeftRenderer, forearmLeftSpritesByDir, idx);
            ApplySpriteFromArray(forearmRightRenderer, forearmRightSpritesByDir, idx);
            ApplySpriteFromArray(shinLeftRenderer, shinLeftSpritesByDir, idx);
            ApplySpriteFromArray(shinRightRenderer, shinRightSpritesByDir, idx);
            ApplySpriteFromArray(wingLeftRenderer, wingLeftSpritesByDir, idx);
            ApplySpriteFromArray(wingRightRenderer, wingRightSpritesByDir, idx);
            ApplySpriteFromArray(bodySegment1Renderer, bodySegment1SpritesByDir, idx);
            ApplySpriteFromArray(bodySegment2Renderer, bodySegment2SpritesByDir, idx);
            ApplySpriteFromArray(bodySegment3Renderer, bodySegment3SpritesByDir, idx);
            ApplySpriteFromArray(bodySegment4Renderer, bodySegment4SpritesByDir, idx);
            ApplySideViewOcclusion(dir);
            ApplyArmVisibility(dir);
        }

        /// <summary>
        /// PR 2 — anti rời rạc. Khi <paramref name="dir"/> = E hoặc W, far limb (cánh tay /
        /// chân ở phía xa camera) được đẩy sortingOrder về phía trước torso (thay vì cùng
        /// layer với near limb) + scale.x shrink → perspective depth illusion.
        ///
        /// <para>Far-limb side mapping:
        /// <list type="bullet">
        ///   <item>E (facing right): left arm/leg is far (back of character)</item>
        ///   <item>W (facing left): right arm/leg is far (back of character)</item>
        ///   <item>N/S (front/back view): không apply — reset cả 2 sides về base.</item>
        /// </list>
        /// </para>
        ///
        /// <para>Forearm/shin (children) cũng được đẩy sortingOrder để match upper limb depth.
        /// Scale chỉ apply cho parent — children tự inherit qua transform hierarchy.</para>
        /// </summary>
        public void ApplySideViewOcclusion(CharacterArtSpec.PuppetDirection dir)
        {
            bool sideView = IsSideView(dir);
            if (!enableSideViewOcclusion || !sideView)
            {
                ResetLimbsToBase();
                return;
            }

            bool leftIsFar = LeftIsFarInSideView(dir);
            ApplyLimbOcclusion(leftIsFar);
        }

        /// <summary>
        /// Pure helper: true nếu direction là E hoặc W (side view) — chỉ side view có depth
        /// occlusion vì N/S thấy character từ trước/sau với cả 2 cánh tay đối xứng visible.
        /// </summary>
        public static bool IsSideView(CharacterArtSpec.PuppetDirection dir)
        {
            return dir == CharacterArtSpec.PuppetDirection.East
                || dir == CharacterArtSpec.PuppetDirection.West;
        }

        /// <summary>
        /// Pure helper: true nếu left limb là far (back-side) trong side view.
        /// E (facing right) → camera thấy right side → left arm/leg là back (far).
        /// W (facing left) → camera thấy left side → right arm/leg là back. Caller fallback
        /// false (right is far) cho W.
        /// </summary>
        public static bool LeftIsFarInSideView(CharacterArtSpec.PuppetDirection dir)
        {
            return dir == CharacterArtSpec.PuppetDirection.East;
        }

        /// <summary>
        /// Pure helper: true nếu direction là N hoặc S (front / back view) — DST convention
        /// hide separate arm sprites trong các view này vì torso PNG đã có sleeves baked.
        /// </summary>
        public static bool IsFrontBackView(CharacterArtSpec.PuppetDirection dir)
        {
            return dir == CharacterArtSpec.PuppetDirection.North
                || dir == CharacterArtSpec.PuppetDirection.South;
        }

        void ApplyArmVisibility(CharacterArtSpec.PuppetDirection dir)
        {
            bool armsVisible = !(hideArmsInFrontBackView && IsFrontBackView(dir));
            if (armLeftRenderer != null) armLeftRenderer.enabled = armsVisible;
            if (armRightRenderer != null) armRightRenderer.enabled = armsVisible;
            if (forearmLeftRenderer != null) forearmLeftRenderer.enabled = armsVisible;
            if (forearmRightRenderer != null) forearmRightRenderer.enabled = armsVisible;
        }

        void ResetLimbsToBase()
        {
            if (armLeftRenderer != null) armLeftRenderer.sortingOrder = baseArmLeftOrder;
            if (armRightRenderer != null) armRightRenderer.sortingOrder = baseArmRightOrder;
            if (legLeftRenderer != null) legLeftRenderer.sortingOrder = baseLegLeftOrder;
            if (legRightRenderer != null) legRightRenderer.sortingOrder = baseLegRightOrder;
            if (forearmLeftRenderer != null) forearmLeftRenderer.sortingOrder = baseForearmLeftOrder;
            if (forearmRightRenderer != null) forearmRightRenderer.sortingOrder = baseForearmRightOrder;
            if (shinLeftRenderer != null) shinLeftRenderer.sortingOrder = baseShinLeftOrder;
            if (shinRightRenderer != null) shinRightRenderer.sortingOrder = baseShinRightOrder;
            if (armLeft != null) armLeft.localScale = baseArmLeftScale;
            if (armRight != null) armRight.localScale = baseArmRightScale;
            if (legLeft != null) legLeft.localScale = baseLegLeftScale;
            if (legRight != null) legRight.localScale = baseLegRightScale;
        }

        void ApplyLimbOcclusion(bool leftIsFar)
        {
            // Arm: leftIsFar → push armLeft + forearmLeft behind, restore armRight + forearmRight.
            if (leftIsFar)
            {
                ApplyFarArm(armLeft, armLeftRenderer, baseArmLeftOrder, baseArmLeftScale,
                    forearmLeftRenderer, baseForearmLeftOrder);
                ApplyNearArm(armRight, armRightRenderer, baseArmRightOrder, baseArmRightScale,
                    forearmRightRenderer, baseForearmRightOrder);
                ApplyFarLeg(legLeft, legLeftRenderer, baseLegLeftOrder, baseLegLeftScale,
                    shinLeftRenderer, baseShinLeftOrder);
                ApplyNearLeg(legRight, legRightRenderer, baseLegRightOrder, baseLegRightScale,
                    shinRightRenderer, baseShinRightOrder);
            }
            else
            {
                ApplyFarArm(armRight, armRightRenderer, baseArmRightOrder, baseArmRightScale,
                    forearmRightRenderer, baseForearmRightOrder);
                ApplyNearArm(armLeft, armLeftRenderer, baseArmLeftOrder, baseArmLeftScale,
                    forearmLeftRenderer, baseForearmLeftOrder);
                ApplyFarLeg(legRight, legRightRenderer, baseLegRightOrder, baseLegRightScale,
                    shinRightRenderer, baseShinRightOrder);
                ApplyNearLeg(legLeft, legLeftRenderer, baseLegLeftOrder, baseLegLeftScale,
                    shinLeftRenderer, baseShinLeftOrder);
            }
        }

        void ApplyFarArm(Transform arm, SpriteRenderer armR, int baseOrder, Vector3 baseScale,
            SpriteRenderer forearmR, int forearmBaseOrder)
        {
            if (armR != null)
                armR.sortingOrder = CharacterRigSpec.ComputeFarLimbSortingOrder(baseOrder, farLimbSortingOffset);
            if (forearmR != null)
                forearmR.sortingOrder = CharacterRigSpec.ComputeFarLimbSortingOrder(forearmBaseOrder, farLimbSortingOffset);
            if (arm != null)
            {
                float sx = CharacterRigSpec.ComputeFarLimbScaleX(farLimbScale, baseScale.x)
                    * Mathf.Sign(baseScale.x == 0f ? 1f : baseScale.x);
                arm.localScale = new Vector3(sx, baseScale.y, baseScale.z);
            }
        }

        void ApplyNearArm(Transform arm, SpriteRenderer armR, int baseOrder, Vector3 baseScale,
            SpriteRenderer forearmR, int forearmBaseOrder)
        {
            if (armR != null) armR.sortingOrder = baseOrder;
            if (forearmR != null) forearmR.sortingOrder = forearmBaseOrder;
            if (arm != null) arm.localScale = baseScale;
        }

        void ApplyFarLeg(Transform leg, SpriteRenderer legR, int baseOrder, Vector3 baseScale,
            SpriteRenderer shinR, int shinBaseOrder)
        {
            if (legR != null)
                legR.sortingOrder = CharacterRigSpec.ComputeFarLimbSortingOrder(baseOrder, farLimbSortingOffset);
            if (shinR != null)
                shinR.sortingOrder = CharacterRigSpec.ComputeFarLimbSortingOrder(shinBaseOrder, farLimbSortingOffset);
            if (leg != null)
            {
                float sx = CharacterRigSpec.ComputeFarLimbScaleX(farLimbScale, baseScale.x)
                    * Mathf.Sign(baseScale.x == 0f ? 1f : baseScale.x);
                leg.localScale = new Vector3(sx, baseScale.y, baseScale.z);
            }
        }

        void ApplyNearLeg(Transform leg, SpriteRenderer legR, int baseOrder, Vector3 baseScale,
            SpriteRenderer shinR, int shinBaseOrder)
        {
            if (legR != null) legR.sortingOrder = baseOrder;
            if (shinR != null) shinR.sortingOrder = shinBaseOrder;
            if (leg != null) leg.localScale = baseScale;
        }

        static void ApplySpriteFromArray(SpriteRenderer renderer, Sprite[] arr, int idx)
        {
            if (renderer == null || arr == null || arr.Length == 0) return;
            // Fallback: nếu idx ngoài range hoặc null → dùng East (idx 0).
            Sprite s = (idx >= 0 && idx < arr.Length) ? arr[idx] : null;
            if (s == null && arr.Length > 0) s = arr[(int)CharacterArtSpec.PuppetDirection.East];
            if (s != null) renderer.sprite = s;
        }

        // ============ IMobAnim ============

        public void SetCrouch(bool on) => crouching = on;

        public void TriggerLunge(Vector2 direction)
        {
            lungeStartTime = Time.time;
            lungeDir = direction;
            // Flip spriteRoot to face attack direction immediately.
            if (spriteRoot != null && Mathf.Abs(direction.x) > 0.01f)
            {
                spriteRoot.localScale = new Vector3(
                    BoneAnimController.ComputeFlipScaleX(spriteRoot.localScale.x, direction.x, 0.01f),
                    spriteRoot.localScale.y,
                    spriteRoot.localScale.z);
            }
        }

        public void TriggerSquash(float duration)
        {
            squashStartTime = Time.time;
            squashDuration = duration;
        }

        // ============ Pure math (EditMode testable) ============

        /// <summary>
        /// Lunge arm angle envelope: bell curve sin. u=0 → 0, u=0.5 → max, u=1 → 0.
        /// Same shape as <see cref="MobAnimController.ComputeLungeOffset"/> nhưng angular.
        /// </summary>
        public static float ComputeLungeArmAngle(float u, float maxDeg)
        {
            float clamped = Mathf.Clamp01(u);
            return Mathf.Sin(Mathf.PI * clamped) * maxDeg;
        }

        /// <summary>
        /// Walk swing degrees per frame. Pure helper cho test.
        /// Caller pass <c>walkSin</c> (already computed sin) + max amplitude + isLeft (left/right opposite).
        /// </summary>
        public static float ComputeArmSwingDeg(float walkSin, float maxDeg, bool isLeft)
        {
            return (isLeft ? walkSin : -walkSin) * maxDeg;
        }

        /// <summary>
        /// Walk speed ratio: clamped speed / reference, dùng để scale step frequency.
        /// </summary>
        public static float ComputeSpeedRatio(float speed, float referenceSpeed,
            float minRatio = 0.3f, float maxRatio = 2f)
        {
            if (referenceSpeed <= 0f) return 1f;
            return Mathf.Clamp(speed / referenceSpeed, minRatio, maxRatio);
        }

        /// <summary>
        /// Walk knee bend (PR K, L2): shin rotates forward when leg swings backward.
        /// Pass leg-specific sin (positive = back-swing). Returns deg ∈ [0, maxDeg]
        /// (knee never hyper-extends backward — clamped at 0).
        /// </summary>
        public static float ComputeWalkKneeBend(float legBackSin, float maxDeg)
        {
            return Mathf.Max(0f, legBackSin) * maxDeg;
        }

        /// <summary>
        /// Crouch knee bend (PR K, L2): shin rotates forward proportional to crouch amount.
        /// <paramref name="crouchAmount"/> ∈ [0, 1] (0 = standing, 1 = full crouch). Returns deg.
        /// </summary>
        public static float ComputeCrouchKneeBend(float crouchAmount, float maxDeg)
        {
            return Mathf.Clamp01(crouchAmount) * maxDeg;
        }

        /// <summary>
        /// Lunge elbow bend (PR K, L2): forearm bends inward at mid-lunge windup peak,
        /// straightens at end. Bell curve sin(π × u). Returns positive deg — caller signs
        /// based on lunge direction (mirror with arm signedAngle).
        /// </summary>
        public static float ComputeLungeElbowBend(float u, float maxDeg)
        {
            float clamped = Mathf.Clamp01(u);
            return Mathf.Sin(Mathf.PI * clamped) * maxDeg;
        }

        /// <summary>
        /// Wing flap angle (Phase 3 — Crow / Bat). Pure sin oscillation at <paramref name="frequency"/>
        /// Hz with amplitude <paramref name="maxDeg"/>. Independent of walk speed — flying mob
        /// flap liên tục cả khi idle (hovering) và khi cruise. Caller mirrors sign cho left/right
        /// wing để 2 cánh flap cùng phase (xoãi xuống cùng → đẩy lên).
        ///
        /// Range: [-maxDeg, +maxDeg]. Returns 0 nếu frequency &lt;= 0 (defensive — wing standstill).
        /// </summary>
        public static float ComputeFlapAngle(float time, float frequency, float maxDeg)
        {
            if (frequency <= 0f) return 0f;
            return Mathf.Sin(time * 2f * Mathf.PI * frequency) * maxDeg;
        }

        /// <summary>
        /// Serpentine body-segment angle (Phase 4 — Snake). S-curve wave traveling head→tail.
        /// Phase delay per segment: each successive segment lags by <paramref name="waveSpreadPerSegment"/>
        /// radians → wave propagates down chain. Amplitude scales linearly with position from head:
        /// segmentIndex 0 → maxDeg × 1/segmentCount (small wiggle near head), segmentIndex
        /// (segmentCount-1) → maxDeg (full whip at tail).
        ///
        /// <para>API: <paramref name="segmentIndex"/> 0-based (0 = first segment after head,
        /// segmentCount-1 = tail). <paramref name="segmentCount"/> = total segments in chain
        /// (excluding head).</para>
        ///
        /// <para>Range: [-maxDeg, +maxDeg]. Defensive returns 0 cho:
        /// <list type="bullet">
        ///   <item>frequency &lt;= 0</item>
        ///   <item>segmentCount &lt;= 0</item>
        ///   <item>segmentIndex &lt; 0 hoặc &gt;= segmentCount</item>
        /// </list>
        /// </para>
        /// </summary>
        public static float ComputeSerpentineAngle(float time, int segmentIndex, int segmentCount,
            float frequency, float maxDeg, float waveSpreadPerSegment)
        {
            if (frequency <= 0f) return 0f;
            if (segmentCount <= 0) return 0f;
            if (segmentIndex < 0 || segmentIndex >= segmentCount) return 0f;

            // Amplitude scales linear với position trong chain: idx 0 → 1/N (small), idx N-1 → 1 (full).
            float amplitudeScale = (segmentIndex + 1) / (float)segmentCount;
            float amplitude = maxDeg * amplitudeScale;

            // Phase: time-driven oscillation, each segment delayed by waveSpreadPerSegment radians
            // → wave travels head→tail (segmentIndex 0 leads, segmentIndex N-1 lags).
            float phase = time * 2f * Mathf.PI * frequency - segmentIndex * waveSpreadPerSegment;
            return Mathf.Sin(phase) * amplitude;
        }
    }
}
