using UnityEngine;
using WildernessCultivation.Combat;
using WildernessCultivation.Items;
using WildernessCultivation.Player;

namespace WildernessCultivation.World
{
    /// <summary>
    /// Cây / đá / bụi cỏ / linh thảo. Đập = đập tay = nhận drop. Có HP và drop list.
    /// Optional harvest side-effects áp lên harvester (vd Cactus prick -2 HP +5 Thirst,
    /// Death Lily -5 SAN khi pick).
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

        void Awake() { currentHP = maxHP; }

        public void TakeDamage(float amount, GameObject source)
        {
            // Nếu source là Projectile → resolve về Owner (player) để Harvest tìm được Inventory.
            if (source != null)
            {
                var proj = source.GetComponent<Projectile>();
                if (proj != null && proj.Owner != null) source = proj.Owner;
            }
            currentHP -= amount;
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

            if (harvestHpDamage > 0f) ps.TakeDamage(harvestHpDamage);
            if (harvestHpHeal > 0f) ps.Heal(harvestHpHeal);
            if (harvestHungerRestore > 0f) ps.Eat(harvestHungerRestore);
            if (harvestThirstRestore > 0f) ps.Drink(harvestThirstRestore);
            if (harvestSanityDamage > 0f) ps.Sanity = Mathf.Max(0f, ps.Sanity - harvestSanityDamage);
        }
    }
}
