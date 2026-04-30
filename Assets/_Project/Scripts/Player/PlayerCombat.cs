using UnityEngine;
using WildernessCultivation.Core;
using WildernessCultivation.Audio;
using WildernessCultivation.Combat;
using WildernessCultivation.Cultivation;
using WildernessCultivation.Items;

namespace WildernessCultivation.Player
{
    /// <summary>
    /// Đánh thường (melee swing) + cast 1 công pháp gắn slot.
    /// </summary>
    [RequireComponent(typeof(PlayerController), typeof(PlayerStats))]
    public class PlayerCombat : MonoBehaviour
    {
        [Header("Melee")]
        public float meleeRange = 1.2f;
        public float meleeDamage = 8f;
        public float meleeCooldown = 0.6f;
        public LayerMask hitMask;

        [Header("Skill (gắn 1 ScriptableObject công pháp)")]
        public TechniqueSO equippedTechnique;

        [Header("Equipped weapon (optional — slot index trong Inventory)")]
        [Tooltip("Inventory để check durability của vũ khí đang trang bị. Null = bỏ qua durability.")]
        public Inventory inventory;
        [Tooltip("Slot index trong Inventory chứa vũ khí. -1 = không có; bonus dame = item.weaponDamage.")]
        public int equippedWeaponSlotIndex = -1;

        [Header("Inputs (PC test)")]
        public KeyCode attackKey = KeyCode.J;
        public KeyCode skillKey = KeyCode.K;

        PlayerController controller;
        PlayerStats stats;
        SpiritRoot spiritRoot;
        float meleeReadyAt;
        float skillReadyAt;
        float baseMeleeDamage;

        void Awake()
        {
            controller = GetComponent<PlayerController>();
            stats = GetComponent<PlayerStats>();
            spiritRoot = GetComponent<SpiritRoot>();
            baseMeleeDamage = meleeDamage;
            ServiceLocator.Register<PlayerCombat>(this);
        }

        void OnDestroy() => ServiceLocator.Unregister<PlayerCombat>(this);

        /// <summary>Reset meleeDamage về giá trị gốc (Awake-time). Gọi từ SaveLoadController
        /// trước <see cref="WildernessCultivation.Cultivation.RealmSystem.ReapplyAccumulatedBonuses"/>
        /// để tránh stack double khi LoadAndApply chạy nhiều lần trong cùng scene.</summary>
        public void ResetMeleeDamageToBase()
        {
            if (baseMeleeDamage > 0f) meleeDamage = baseMeleeDamage;
        }

        void Update()
        {
            if (Input.GetKeyDown(attackKey)) TryMeleeAttack();
            if (Input.GetKeyDown(skillKey)) TryCastSkill();
        }

        public void TryMeleeAttack()
        {
            if (Time.time < meleeReadyAt) return;

            // Tính damage tổng (base + bonus từ vũ khí trang bị)
            float damage = meleeDamage;
            InventorySlot weaponSlot = null;
            if (inventory != null && equippedWeaponSlotIndex >= 0)
            {
                weaponSlot = inventory.GetSlot(equippedWeaponSlotIndex);
                if (weaponSlot != null && !weaponSlot.IsEmpty && !weaponSlot.IsBroken && weaponSlot.item.weaponDamage > 0)
                    damage += weaponSlot.item.weaponDamage;
            }
            // Linh căn Kim multiplier
            if (spiritRoot != null) damage *= spiritRoot.WeaponDamageMul;

            meleeReadyAt = Time.time + meleeCooldown;
            AudioManager.Instance?.PlaySfx(AudioManager.SfxKind.MeleeSwing);

            Vector2 origin = (Vector2)transform.position + controller.Facing * 0.3f;
            Vector2 hitCenter = origin + controller.Facing * (meleeRange * 0.5f);
            var hits = Physics2D.OverlapCircleAll(hitCenter, meleeRange * 0.5f, hitMask);
            int actualHits = 0;
            foreach (var h in hits)
            {
                if (h.gameObject == gameObject) continue;
                var dmg = h.GetComponent<IDamageable>();
                if (dmg != null) { dmg.TakeDamage(damage, gameObject); actualHits++; }
            }

            // Hao mòn vũ khí 1 đòn (chỉ khi thực sự đánh trúng để tránh "quẩy không")
            if (actualHits > 0 && weaponSlot != null && weaponSlot.IsDurable)
            {
                float wear = weaponSlot.item.durabilityPerUse * (spiritRoot != null ? spiritRoot.DurabilityWearMul : 1f);
                inventory.UseDurability(equippedWeaponSlotIndex, wear);
            }
        }

        public bool TryCastSkill()
        {
            if (equippedTechnique == null) return false;
            if (Time.time < skillReadyAt) return false;
            if (!stats.TryConsumeMana(equippedTechnique.manaCost)) return false;

            skillReadyAt = Time.time + equippedTechnique.cooldown;
            AudioManager.Instance?.PlaySfx(AudioManager.SfxKind.SkillCast);
            equippedTechnique.Cast(this);
            return true;
        }

        public PlayerController Controller => controller;
        public PlayerStats Stats => stats;

        /// <summary>Multiplier dame cho 1 technique theo linh căn caster (cùng element → bonus dame).</summary>
        /// <remarks>Chỉ dùng <c>sameElementDamageMultiplier</c> (Combat). XP affinity (<c>techniqueAffinityMultiplier</c>)
        /// đã được áp riêng tại <see cref="WildernessCultivation.Cultivation.RealmSystem.AddTechniqueXp"/>.</remarks>
        public float WeaponDamageMultiplierForElement(SpiritElement element)
        {
            if (spiritRoot == null || spiritRoot.Current == null || element == SpiritElement.None) return 1f;
            return spiritRoot.Current.primaryElement == element
                ? spiritRoot.Current.sameElementDamageMultiplier
                : 1f;
        }

        void OnDrawGizmosSelected()
        {
            if (controller == null) return;
            Gizmos.color = Color.red;
            Vector2 origin = (Vector2)transform.position + controller.Facing * 0.3f;
            Vector2 hitCenter = origin + controller.Facing * (meleeRange * 0.5f);
            Gizmos.DrawWireSphere(hitCenter, meleeRange * 0.5f);
        }
    }
}
