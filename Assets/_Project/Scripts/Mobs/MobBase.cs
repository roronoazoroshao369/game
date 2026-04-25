using UnityEngine;
using WildernessCultivation.Combat;
using WildernessCultivation.Items;
using WildernessCultivation.World;

namespace WildernessCultivation.Mobs
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public abstract class MobBase : MonoBehaviour, IDamageable
    {
        [Header("Identity")]
        public string mobName = "Quái";

        [Header("Stats")]
        public float maxHP = 20f;
        public float HP = 20f;
        public float moveSpeed = 1.5f;
        public float damage = 5f;
        public float attackCooldown = 1f;
        public float xpReward = 5f;

        [Header("Drops")]
        public ResourceNode.Drop[] drops;

        [Header("Senses")]
        public float aggroRange = 4f;
        public float attackRange = 0.8f;
        public LayerMask playerMask;

        [Header("Refs")]
        public Transform target;
        public SpriteRenderer spriteRenderer;
        public Animator animator;

        protected Rigidbody2D rb;
        protected float attackReadyAt;

        protected virtual void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            HP = maxHP;
        }

        public virtual void TakeDamage(float amount, GameObject source)
        {
            // Nếu source là Projectile → resolve về Owner cho aggro/loot, không lấy projectile gameObject.
            GameObject resolvedSource = source;
            if (source != null)
            {
                var proj = source.GetComponent<Projectile>();
                if (proj != null && proj.Owner != null) resolvedSource = proj.Owner;
            }
            HP -= amount;
            FlashHit();
            if (resolvedSource != null && target == null) target = resolvedSource.transform; // aggro on hit
            if (HP <= 0f) Die(resolvedSource);
        }

        protected virtual void FlashHit()
        {
            if (spriteRenderer != null) StartCoroutine(FlashCoroutine());
        }

        System.Collections.IEnumerator FlashCoroutine()
        {
            var orig = spriteRenderer.color;
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.08f);
            if (spriteRenderer != null) spriteRenderer.color = orig;
        }

        protected virtual void Die(GameObject killer)
        {
            // Drop loot vào inventory người giết (nếu có)
            var inv = killer != null ? killer.GetComponentInParent<Inventory>() : null;
            foreach (var d in drops)
            {
                int n = Random.Range(d.min, d.max + 1);
                if (n <= 0 || d.item == null) continue;
                if (inv != null) inv.Add(d.item, n);
            }

            // Cho XP tu luyện
            if (killer != null)
            {
                var realm = killer.GetComponentInParent<WildernessCultivation.Cultivation.RealmSystem>();
                if (realm != null) realm.AddCultivationXp(xpReward);
            }

            Destroy(gameObject);
        }

        protected void MoveTowards(Vector2 dest)
        {
            Vector2 dir = ((Vector2)dest - (Vector2)transform.position).normalized;
            rb.velocity = dir * moveSpeed;
            if (spriteRenderer != null && Mathf.Abs(dir.x) > 0.05f)
                spriteRenderer.flipX = dir.x < 0;
        }

        protected void StopMoving() => rb.velocity = Vector2.zero;

        protected bool TryFindPlayer()
        {
            if (target != null) return true;
            var hit = Physics2D.OverlapCircle(transform.position, aggroRange, playerMask);
            if (hit != null) { target = hit.transform; return true; }
            return false;
        }
    }
}
