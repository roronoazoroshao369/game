using UnityEngine;
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
        float meleeReadyAt;
        float skillReadyAt;

        void Awake()
        {
            controller = GetComponent<PlayerController>();
            stats = GetComponent<PlayerStats>();
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

            meleeReadyAt = Time.time + meleeCooldown;

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
                inventory.UseDurability(equippedWeaponSlotIndex);
        }

        public bool TryCastSkill()
        {
            if (equippedTechnique == null) return false;
            if (Time.time < skillReadyAt) return false;
            if (!stats.TryConsumeMana(equippedTechnique.manaCost)) return false;

            skillReadyAt = Time.time + equippedTechnique.cooldown;
            equippedTechnique.Cast(this);
            return true;
        }

        public PlayerController Controller => controller;
        public PlayerStats Stats => stats;

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
