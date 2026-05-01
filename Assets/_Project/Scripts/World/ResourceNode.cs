using UnityEngine;
using WildernessCultivation.Combat;
using WildernessCultivation.Items;
using WildernessCultivation.Player;
using WildernessCultivation.Vfx;

namespace WildernessCultivation.World
{
    /// <summary>
    /// Cây / đá / bụi cỏ / linh thảo. Đập = đập tay = nhận drop. Có HP và drop list.
    /// Optional harvest side-effects áp lên harvester (vd Cactus prick -2 HP,
    /// Death Lily -5 SAN khi pick). Restore (nước/đói) thường đi qua ItemSO drop
    /// thay vì side-effect — chỉ dùng restoreXxx field khi pick action implicit
    /// đem lại lợi (vd herb tự cộng máu khi nhổ).
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class ResourceNode : MonoBehaviour, IDamageable
    {
        [System.Serializable]
        public struct Drop
        {
            public ItemSO item;
            public int min, max;
        }

        public string nodeName = "Cây";
        public float maxHP = 30f;
        public float currentHP;
        public Drop[] drops;
        public GameObject onDestroyVfx;

        // Cache reactive feedback (BootstrapWizard auto-attach lên prefab).
        // GetComponent 1 lần ở Awake, Hit() vô số lần → tránh overhead per hit.
        ReactiveOnHit reactiveFx;
        ProgressiveCrackOverlay crackOverlay;

        [Header("Optional harvest side-effects on harvester (PlayerStats)")]
        [Tooltip("Sát thương HP áp lên harvester sau khi node chết (vd Cactus -2).")]
        public float harvestHpDamage = 0f;
        [Tooltip("Phục hồi HP harvester (vd thảo dược cộng máu nhỏ).")]
        public float harvestHpHeal = 0f;
        [Tooltip("Tăng Hunger harvester (linh nấm +18, berry +10).")]
        public float harvestHungerRestore = 0f;
        [Tooltip("Tăng Thirst harvester (cactus +12).")]
        public float harvestThirstRestore = 0f;
        [Tooltip("Trừ Sanity harvester (Death Lily -5).")]
        public float harvestSanityDamage = 0f;

        void Awake()
        {
            currentHP = maxHP;
            reactiveFx = GetComponent<ReactiveOnHit>();
            crackOverlay = GetComponent<ProgressiveCrackOverlay>();
        }

        public void TakeDamage(float amount, GameObject source)
        {
            // Nếu source là Projectile → resolve về Owner (player) để Harvest tìm được Inventory.
            if (source != null)
            {
                var proj = source.GetComponent<Projectile>();
                if (proj != null && proj.Owner != null) source = proj.Owner;
            }
            currentHP -= amount;
            // Reactive feedback: flash + shake + leaf burst (cây đang bị chặt rung rinh).
            // Trigger sau khi HP trừ — nếu Harvest sẽ Destroy ngay frame này, coroutine
            // shake/flash sẽ stop tự nhiên. Burst particles tách parent → vẫn play.
            if (reactiveFx != null) reactiveFx.Hit();
            if (crackOverlay != null && maxHP > 0f) crackOverlay.SetHpRatio(currentHP / maxHP);
            if (currentHP <= 0f) Harvest(source);
        }

        void Harvest(GameObject source)
        {
            var inv = source != null ? source.GetComponentInParent<Inventory>() : null;
            foreach (var d in drops)
            {
                int n = Random.Range(d.min, d.max + 1);
                if (n <= 0 || d.item == null) continue;
                if (inv != null) inv.Add(d.item, n);
                else Debug.Log($"[Node] Drop {n}x {d.item.displayName} (no inventory)");
            }

            ApplyHarvestSideEffects(source);

            // Mark cell harvested → chunk reload skip respawn (PR #3c persistence). Pos
            // floor về cell coord vì spawn dùng (x+0.5, y+0.5). Chỉ mark khi
            // WorldGenerator instance available (test fixture có thể không có).
            var wg = WorldGenerator.Instance;
            if (wg != null)
            {
                int cx = Mathf.FloorToInt(transform.position.x);
                int cy = Mathf.FloorToInt(transform.position.y);
                wg.MarkHarvested(cx, cy);
            }

            if (onDestroyVfx != null) Destroy(Instantiate(onDestroyVfx, transform.position, Quaternion.identity), 1f);
            Destroy(gameObject);
        }

        void ApplyHarvestSideEffects(GameObject source)
        {
            if (source == null) return;
            if (harvestHpDamage <= 0f && harvestHpHeal <= 0f && harvestHungerRestore <= 0f
                && harvestThirstRestore <= 0f && harvestSanityDamage <= 0f) return;

            var ps = source.GetComponent<PlayerStats>() ?? source.GetComponentInParent<PlayerStats>();
            if (ps == null) return;

            // Raw — bypass i-frames + IncomingDamageMultiplier. Cactus prick là toll
            // môi trường cố định, không phải combat dame nên không nên bị Burn ×1.2 hay
            // dodge i-frame negate.
            if (harvestHpDamage > 0f) ps.TakeDamageRaw(harvestHpDamage);
            if (harvestHpHeal > 0f) ps.Heal(harvestHpHeal);
            if (harvestHungerRestore > 0f) ps.Eat(harvestHungerRestore);
            if (harvestThirstRestore > 0f) ps.Drink(harvestThirstRestore);
            if (harvestSanityDamage > 0f) ps.DamageSanity(harvestSanityDamage);
        }
    }
}
