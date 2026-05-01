using UnityEngine;

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
        [Tooltip("Idle torso bob biên độ (subtle breathing).")]
        public float idleBreathAmplitude = 0.015f;
        [Tooltip("Idle breath tần số.")]
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

        [Header("Tail (optional)")]
        [Tooltip("Tail sway tần số.")]
        public float tailSwayFrequency = 1.5f;
        [Tooltip("Tail sway max degrees.")]
        public float tailSwayDeg = 12f;

        // Cached base local rotations / positions (init from inspector / scene-time pose).
        Quaternion baseArmLeftRot, baseArmRightRot, baseLegLeftRot, baseLegRightRot, baseTailRot;
        Vector3 baseTorsoPos, baseHeadPos;
        Vector3 baseSpriteRootScale;

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
        }

        void CacheBasePose()
        {
            if (armLeft != null) baseArmLeftRot = armLeft.localRotation;
            if (armRight != null) baseArmRightRot = armRight.localRotation;
            if (legLeft != null) baseLegLeftRot = legLeft.localRotation;
            if (legRight != null) baseLegRightRot = legRight.localRotation;
            if (tail != null) baseTailRot = tail.localRotation;
            if (torso != null) baseTorsoPos = torso.localPosition;
            if (head != null) baseHeadPos = head.localPosition;
            if (spriteRoot != null) baseSpriteRootScale = spriteRoot.localScale;
        }

        void Update()
        {
            float dt = Time.deltaTime;
            float t = Time.time;

            Vector2 vel = body != null ? body.velocity : Vector2.zero;
            float speed = vel.magnitude;
            bool moving = speed > movingThreshold;

            // Sprite root direction flip (re-use rig vs duplicate clip).
            if (spriteRoot != null && Mathf.Abs(vel.x) > movingThreshold)
            {
                spriteRoot.localScale = new Vector3(
                    BoneAnimController.ComputeFlipScaleX(spriteRoot.localScale.x, vel.x, movingThreshold),
                    spriteRoot.localScale.y,
                    spriteRoot.localScale.z);
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
            }
            else
            {
                // Reset to neutral khi idle.
                if (armLeft != null) armLeft.localRotation = baseArmLeftRot;
                if (armRight != null) armRight.localRotation = baseArmRightRot;
                if (legLeft != null) legLeft.localRotation = baseLegLeftRot;
                if (legRight != null) legRight.localRotation = baseLegRightRot;
            }

            // Torso bob: walking → speed-proportional sin; idle → slow breath.
            float bobY = moving
                ? walkSin * torsoBobAmplitude
                : Mathf.Sin(t * 2f * Mathf.PI * idleBreathFrequency) * idleBreathAmplitude;

            // Crouch lerp (sticky toggle).
            float crouchTarget = crouching ? crouchTorsoYOffset : 0f;
            currentCrouchY = MobAnimController.ApplyExponentialDamping(currentCrouchY, crouchTarget, crouchLerpRate, dt);

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
    }
}
