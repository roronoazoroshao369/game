using UnityEngine;
using WildernessCultivation.Combat;
using WildernessCultivation.Items;

namespace WildernessCultivation.World
{
    /// <summary>
    /// Cây / đá / bụi cỏ. Đập = đập tay = nhận drop. Có HP và drop list.
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
            if (onDestroyVfx != null) Destroy(Instantiate(onDestroyVfx, transform.position, Quaternion.identity), 1f);
            Destroy(gameObject);
        }
    }
}
