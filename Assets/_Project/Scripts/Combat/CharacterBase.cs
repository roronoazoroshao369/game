using UnityEngine;

namespace WildernessCultivation.Combat
{
    /// <summary>
    /// Abstract base cho mọi "character" có HP + nhận damage: Player, NPC humanoid
    /// (companion / pet / vendor / quest giver — R5 roadmap), Mob.
    ///
    /// <para><b>Mục tiêu R5:</b> code tiêu thụ (UI HP bar, AI targeting, save system, dialog
    /// trigger) lấy 1 ref <see cref="CharacterBase"/> thay vì rẽ nhánh
    /// <c>PlayerStats</c> vs <c>MobBase</c> vs sau này <c>HumanoidNPC</c>. Mỗi subclass tự
    /// quản state HP nội bộ (PlayerStats có shield + i-frames + status modifier; MobBase
    /// có drop loot + xp reward) — base class chỉ expose view read-only.</para>
    ///
    /// <para><b>Coexistence với <see cref="IDamageable"/>:</b> CharacterBase implement
    /// IDamageable (qua abstract <see cref="TakeDamage(float, GameObject)"/>) — code cũ
    /// nhận IDamageable vẫn nhận CharacterBase. Mob attack pattern: thử CharacterBase
    /// trước (rich view), fallback IDamageable (cho ResourceNode tree/rock không phải
    /// character).</para>
    ///
    /// <para><b>Pattern subclass:</b>
    /// <list type="bullet">
    /// <item>Override <see cref="CurrentHP"/> + <see cref="CurrentMaxHP"/> + <see cref="IsDead"/>
    /// = expression-bodied delegate tới field nội bộ. KHÔNG đổi tên field công khai
    /// (HP / maxHP) — Unity serialization tham chiếu theo tên.</item>
    /// <item>Override <see cref="TakeDamage(float, GameObject)"/> = logic damage gốc của
    /// subclass.</item>
    /// </list></para>
    /// </summary>
    public abstract class CharacterBase : MonoBehaviour, IDamageable
    {
        /// <summary>HP hiện tại (read-only view). Subclass override delegate tới field
        /// nội bộ — không expose setter để tránh bypass shield / i-frame / status modifier
        /// pipeline của subclass.</summary>
        public abstract float CurrentHP { get; }

        /// <summary>Max HP hiện tại (sau buff / debuff / realm bonus). Read-only view.</summary>
        public abstract float CurrentMaxHP { get; }

        /// <summary>True nếu HP &lt;= 0 (hoặc subclass coi là chết theo logic riêng — vd
        /// permadeath flag, soulbound flag). Subscribe <see cref="WildernessCultivation.Core.GameEvents.OnPlayerDied"/>
        /// (R4 hub) cho event-driven logic, không poll IsDead mỗi frame.</summary>
        public abstract bool IsDead { get; }

        /// <summary>Convenience inverse của <see cref="IsDead"/>.</summary>
        public bool IsAlive => !IsDead;

        /// <summary>Vị trí thế giới (transform.position). Override nếu cần custom pivot
        /// (vd boss với hitbox lệch tâm).</summary>
        public virtual Vector3 Position => transform.position;

        /// <summary>Damage handler — bắt buộc subclass implement (đã là contract của
        /// <see cref="IDamageable"/>).</summary>
        public abstract void TakeDamage(float amount, GameObject source);

        /// <summary>Tỷ lệ HP 0..1 (clamp). Tiện cho UI bar.</summary>
        public float HPRatio01 => CurrentMaxHP > 0f ? Mathf.Clamp01(CurrentHP / CurrentMaxHP) : 0f;
    }
}
