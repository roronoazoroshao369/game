using UnityEngine;

namespace WildernessCultivation.Vfx
{
    /// <summary>
    /// Common animation hook surface mà FSM state class call (xem WolfStates / RabbitStates).
    /// 2 implementations:
    ///   - <see cref="MobAnimController"/> — procedural transform animation (rabbit, wolf, generic mobs).
    ///     Single sprite + code-driven scale/rotation/translate. Cheap, "Don't Starve" feel.
    ///   - <see cref="BoneAnimController"/> — Animator-driven (player, hero mobs với 2D bone rig).
    ///     Uses Unity 2D Animation package + Animator clips. Heavier setup but per-joint motion.
    ///
    /// Caller (MobBase) caches qua <c>GetComponent&lt;IMobAnim&gt;()</c>; FSM khong care implementation
    /// nào active (KHÔNG cần `is BoneAnim` check).
    /// </summary>
    public interface IMobAnim
    {
        /// <summary>Toggle crouch posture (sticky cho tới khi tắt). Wolf dùng khi chase / stalking.</summary>
        void SetCrouch(bool on);

        /// <summary>One-shot lunge forward attack. <paramref name="direction"/> hint cho sprite flip.</summary>
        void TriggerLunge(Vector2 direction);

        /// <summary>One-shot squash punch. <paramref name="duration"/> dùng bởi procedural; Animator-based bỏ qua.</summary>
        void TriggerSquash(float duration);
    }
}
