using UnityEngine;

namespace WildernessCultivation.Vfx
{
    /// <summary>
    /// Per-mob procedural animation — driven by Rigidbody2D velocity (auto walk bob + tilt)
    /// và explicit hooks (lunge / squash / crouch) caller (FSM state) call. KHÔNG cần Animator;
    /// effects toàn bộ qua transform scale + rotation + brief position translate.
    ///
    /// Composes với <see cref="ReactiveOnHit"/> shake (also writes localRotation) — shake
    /// dominate trong window ngắn (~0.2s) khi mob bị hit, sau đó tilt resume.
    ///
    /// Pure math static methods (<see cref="ComputeWalkBobScale"/>, <see cref="ComputeTiltDeg"/>,
    /// <see cref="ComputeLungeOffset"/>, <see cref="ComputeSquashFactor"/>) → EditMode test.
    /// </summary>
    public class MobAnimController : MonoBehaviour
    {
        [Header("Walk bob (Y scale modulation while moving)")]
        [Tooltip("Biên độ bob max, cộng/trừ vào scale Y khi đi nhanh nhất. 0.05 = ±5%.")]
        public float walkBobAmplitude = 0.05f;
        [Tooltip("Tần số bob (Hz). Mob nhỏ rabbit ~6, wolf ~4.5, boar ~3.5.")]
        public float walkBobFrequency = 5f;
        [Tooltip("Reference speed mà tại đó bob đạt full amplitude. Chậm hơn → bob nhỏ tỉ lệ.")]
        public float referenceSpeed = 2f;

        [Header("Idle breathing (subtle Y scale when stationary)")]
        public float idleBreathAmplitude = 0.02f;
        public float idleBreathFrequency = 1.2f;

        [Header("Direction tilt (sprite Z rotation by velocity X)")]
        [Tooltip("Biên độ tilt max (độ). 0 = tắt tilt.")]
        public float maxTiltDeg = 5f;
        [Tooltip("|vx| dưới ngưỡng → tilt = 0 (tránh jitter khi mob đứng yên).")]
        public float tiltSpeedThreshold = 0.05f;

        [Header("Lunge (one-shot forward translate khi attack)")]
        [Tooltip("Khoảng cách lunge tối đa (unit). 0.3 = wolf bite forward.")]
        public float lungeDistance = 0.3f;
        [Tooltip("Tổng thời gian lunge (out + return). Bell curve sin(π·t/d).")]
        public float lungeDuration = 0.3f;

        [Header("Squash punch (scale 1→peak→1 trong window)")]
        [Tooltip("Peak scale factor. 1.2 = mob phình to 20% rồi co lại lúc attack.")]
        public float squashPeakScale = 1.2f;

        [Header("Crouch (sticky scale Y modifier)")]
        [Tooltip("Multiplier scale Y khi crouch (wolf chase, stalking). 0.9 = thấp 10%.")]
        public float crouchScaleY = 0.9f;

        Rigidbody2D rb;
        Vector3 baseLocalScale;
        Quaternion baseLocalRotation;
        Vector3 baseLocalPosition;

        bool crouching;

        // Lunge state. -1 = idle.
        float lungeStartTime = -1f;
        Vector2 lungeDir;

        // Squash punch state. -1 = idle. Optionally separate from lunge để caller punch không cần lunge.
        float squashStartTime = -1f;
        float squashDuration;

        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            baseLocalScale = transform.localScale;
            baseLocalRotation = transform.localRotation;
            baseLocalPosition = transform.localPosition;
        }

        void Update()
        {
            float dt = Time.deltaTime;
            float t = Time.time;

            Vector2 vel = rb != null ? rb.velocity : Vector2.zero;
            float speed = vel.magnitude;
            bool moving = speed > tiltSpeedThreshold;

            // Bob: walking → speed-proportional sine; idle → slow breath.
            float bobYFactor = moving
                ? ComputeWalkBobScale(t, speed, walkBobAmplitude, walkBobFrequency, referenceSpeed)
                : ComputeIdleBreathScale(t, idleBreathAmplitude, idleBreathFrequency);

            // Squash punch (one-shot).
            float squashFactor = 1f;
            if (squashStartTime >= 0f)
            {
                float age = t - squashStartTime;
                squashFactor = ComputeSquashFactor(age, squashDuration, squashPeakScale);
                if (age >= squashDuration) squashStartTime = -1f;
            }

            // Crouch sticky multiplier.
            float crouchY = crouching ? crouchScaleY : 1f;

            // Apply scale: Y combines bob + squash + crouch; X uses squash only (squash-stretch shape).
            Vector3 scale = baseLocalScale;
            scale.x = baseLocalScale.x * squashFactor;
            scale.y = baseLocalScale.y * bobYFactor * squashFactor * crouchY;
            transform.localScale = scale;

            // Tilt: from velocity X.
            float tiltDeg = ComputeTiltDeg(vel.x, maxTiltDeg, tiltSpeedThreshold);
            transform.localRotation = baseLocalRotation * Quaternion.Euler(0f, 0f, tiltDeg);

            // Lunge translate (only while active). transform.position = base + dir * offset.
            // Caller (Wolf attack) đảm bảo rb stopped in attack tick → không fight với physics.
            if (lungeStartTime >= 0f)
            {
                float age = t - lungeStartTime;
                float offset = ComputeLungeOffset(age, lungeDuration, lungeDistance);
                Vector3 d = (Vector3)(lungeDir * offset);
                transform.localPosition = baseLocalPosition + d;
                if (age >= lungeDuration)
                {
                    lungeStartTime = -1f;
                    transform.localPosition = baseLocalPosition;
                }
            }

            // Avoid unused warning trên dt: future expand cho damped tilt smoothing.
            _ = dt;
        }

        /// <summary>Caller (e.g., WolfChase.OnEnter) toggle crouch posture. Sticky cho tới khi tắt.</summary>
        public void SetCrouch(bool on) { crouching = on; }

        /// <summary>
        /// Trigger lunge forward + squash punch đồng thời. Caller (e.g., WolfAttack OnTick khi
        /// AttackReadyAt fired) truyền direction tới target.
        /// </summary>
        public void TriggerLunge(Vector2 direction)
        {
            // Capture base lúc trigger (mob có thể đã xê dịch sau Awake).
            baseLocalPosition = transform.localPosition;
            lungeDir = direction.sqrMagnitude > 1e-4f ? direction.normalized : Vector2.right;
            lungeStartTime = Time.time;
            // Squash đồng pha với lunge (cùng duration).
            TriggerSquash(lungeDuration);
        }

        /// <summary>Squash punch không có lunge (vd impact effect).</summary>
        public void TriggerSquash(float duration)
        {
            squashDuration = Mathf.Max(0.05f, duration);
            squashStartTime = Time.time;
        }

        // ============ Pure math (EditMode testable) ============

        /// <summary>
        /// Walking Y scale factor — 1 + (speed/refSpeed) * amplitude * sin(2π · freq · t).
        /// Range: clamp speed/refSpeed ∈ [0,1]. speed=0 → 1 (no bob). speed=refSpeed → ±amplitude.
        /// </summary>
        public static float ComputeWalkBobScale(float time, float speed,
            float amplitude, float frequencyHz, float referenceSpeed)
        {
            if (referenceSpeed <= 0f) return 1f;
            float speedFactor = Mathf.Clamp01(speed / referenceSpeed);
            float wave = Mathf.Sin(2f * Mathf.PI * frequencyHz * time);
            return 1f + speedFactor * amplitude * wave;
        }

        /// <summary>Idle breathing Y scale factor — slow sin around 1.</summary>
        public static float ComputeIdleBreathScale(float time, float amplitude, float frequencyHz)
        {
            return 1f + amplitude * Mathf.Sin(2f * Mathf.PI * frequencyHz * time);
        }

        /// <summary>
        /// Sprite tilt degrees from velocity X. |vx| dưới threshold → 0 (tránh jitter).
        /// vx > 0 (đi phải) → -maxTilt (sprite tilt forward right). vx &lt; 0 → +maxTilt.
        /// </summary>
        public static float ComputeTiltDeg(float velocityX, float maxTiltDeg, float speedThreshold)
        {
            if (Mathf.Abs(velocityX) < speedThreshold) return 0f;
            return velocityX > 0f ? -maxTiltDeg : maxTiltDeg;
        }

        /// <summary>
        /// Lunge offset bell curve — distance * sin(π · t/duration).
        /// 0 at t=0 và t=duration, peak (=distance) at t=duration/2.
        /// </summary>
        public static float ComputeLungeOffset(float t, float duration, float distance)
        {
            if (duration <= 0f) return 0f;
            float u = Mathf.Clamp01(t / duration);
            return distance * Mathf.Sin(Mathf.PI * u);
        }

        /// <summary>
        /// Squash punch scale factor — 1 + (peakScale - 1) * sin(π · t/duration).
        /// 1 at t=0, peakScale at t=duration/2, 1 at t=duration.
        /// </summary>
        public static float ComputeSquashFactor(float t, float duration, float peakScale)
        {
            if (duration <= 0f) return 1f;
            float u = Mathf.Clamp01(t / duration);
            return 1f + (peakScale - 1f) * Mathf.Sin(Mathf.PI * u);
        }
    }
}
