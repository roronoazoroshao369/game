using UnityEngine;

namespace WildernessCultivation.Vfx
{
    /// <summary>
    /// Animator-driven implementation của <see cref="IMobAnim"/> cho character có 2D bone rig
    /// (Unity 2D Animation package: <c>com.unity.feature.2d</c> > 2D Animation + PSDImporter).
    ///
    /// Drives Animator parameters mỗi frame:
    ///   - <c>Speed</c> (float) — magnitude of Rigidbody2D.velocity.
    ///   - <c>Moving</c> (bool) — speed > <see cref="movingThreshold"/>.
    /// State hooks (FSM gọi):
    ///   - <see cref="SetCrouch"/> → bool param <see cref="crouchParam"/>.
    ///   - <see cref="TriggerLunge"/> → trigger <see cref="lungeTrigger"/> + flip sprite root by direction.
    ///   - <see cref="TriggerSquash"/> → trigger <see cref="squashTrigger"/> (duration ignored — clip own length).
    ///
    /// Setup workflow xem <c>Documentation/pipelines/BONE_RIG_GUIDE.md</c>.
    ///
    /// Compose: GameObject phải có <see cref="Animator"/> + <see cref="Rigidbody2D"/>. Sprite root
    /// (default <c>this.transform</c>) flips X scale based on movement direction → re-uses rig
    /// thay vì duplicate left/right clip.
    /// </summary>
    [DisallowMultipleComponent]
    public class BoneAnimController : MonoBehaviour, IMobAnim
    {
        [Header("Refs")]
        [Tooltip("Animator drive bởi component. Auto-assign từ GetComponent nếu null.")]
        public Animator animator;
        [Tooltip("Transform flip X scale khi đổi hướng. Auto-assign = transform nếu null.")]
        public Transform spriteRoot;
        [Tooltip("Rigidbody2D đọc velocity. Auto-assign nếu null.")]
        public Rigidbody2D body;

        [Header("Animator parameter names (phải match Animator Controller)")]
        public string speedParam = "Speed";
        public string movingParam = "Moving";
        public string crouchParam = "Crouch";
        public string lungeTrigger = "Lunge";
        public string squashTrigger = "Squash";

        [Header("Tuning")]
        [Tooltip("Speed dưới ngưỡng → Moving=false (tránh jitter idle). 0.05u/s default.")]
        public float movingThreshold = 0.05f;

        void Awake()
        {
            if (animator == null) animator = GetComponent<Animator>();
            if (spriteRoot == null) spriteRoot = transform;
            if (body == null) body = GetComponent<Rigidbody2D>();
        }

        void Update()
        {
            if (animator == null) return;

            Vector2 vel = body != null ? body.velocity : Vector2.zero;
            float speed = vel.magnitude;

            SafeSetFloat(speedParam, speed);
            SafeSetBool(movingParam, speed > movingThreshold);

            // Auto-flip horizontal khi đổi hướng. Re-use bone rig thay vì duplicate clip.
            if (spriteRoot != null && Mathf.Abs(vel.x) > movingThreshold)
            {
                var s = spriteRoot.localScale;
                spriteRoot.localScale = new Vector3(
                    ComputeFlipScaleX(s.x, vel.x, movingThreshold),
                    s.y, s.z);
            }
        }

        public void SetCrouch(bool on) => SafeSetBool(crouchParam, on);

        public void TriggerLunge(Vector2 direction)
        {
            if (spriteRoot != null && Mathf.Abs(direction.x) > 0.01f)
            {
                var s = spriteRoot.localScale;
                spriteRoot.localScale = new Vector3(
                    ComputeFlipScaleX(s.x, direction.x, 0.01f),
                    s.y, s.z);
            }
            SafeSetTrigger(lungeTrigger);
        }

        public void TriggerSquash(float duration)
        {
            // duration ignored — Animator clip có own length. Procedural MobAnimController dùng tham số này.
            _ = duration;
            SafeSetTrigger(squashTrigger);
        }

        void SafeSetBool(string name, bool v)
        {
            if (animator != null && !string.IsNullOrEmpty(name)) animator.SetBool(name, v);
        }

        void SafeSetFloat(string name, float v)
        {
            if (animator != null && !string.IsNullOrEmpty(name)) animator.SetFloat(name, v);
        }

        void SafeSetTrigger(string name)
        {
            if (animator != null && !string.IsNullOrEmpty(name)) animator.SetTrigger(name);
        }

        // ============ Pure math (EditMode testable) ============

        /// <summary>
        /// Compute new scale.x based on velocity direction.
        /// |vx| dưới threshold → giữ scale hiện tại (no flip).
        /// vx > 0 → +|currentScaleX| (face right). vx &lt; 0 → -|currentScaleX| (face left).
        /// Magnitude của input giữ nguyên (preserve user-set scale).
        /// </summary>
        public static float ComputeFlipScaleX(float currentScaleX, float velocityX, float threshold)
        {
            if (Mathf.Abs(velocityX) < threshold) return currentScaleX;
            float mag = Mathf.Abs(currentScaleX);
            return velocityX > 0f ? mag : -mag;
        }
    }
}
