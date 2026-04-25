using UnityEngine;

namespace WildernessCultivation.Mobs
{
    /// <summary>
    /// Thỏ — wandering passive, chạy trốn khi player tới gần.
    /// </summary>
    public class RabbitAI : MobBase
    {
        public float fleeRange = 3f;
        public float wanderRadius = 2.5f;
        public float wanderInterval = 3f;

        Vector2 wanderTarget;
        float nextWanderAt;

        protected override void Awake()
        {
            base.Awake();
            mobName = "Thỏ Rừng";
            wanderTarget = transform.position;
        }

        void Update()
        {
            // Flee from player if too close
            var hit = Physics2D.OverlapCircle(transform.position, fleeRange, playerMask);
            if (hit != null)
            {
                Vector2 fleeDir = ((Vector2)transform.position - (Vector2)hit.transform.position).normalized;
                rb.velocity = fleeDir * (moveSpeed * 1.6f);
                if (spriteRenderer != null) spriteRenderer.flipX = fleeDir.x < 0;
                return;
            }

            // Wander
            if (Time.time >= nextWanderAt || Vector2.Distance(transform.position, wanderTarget) < 0.2f)
            {
                wanderTarget = (Vector2)transform.position + Random.insideUnitCircle * wanderRadius;
                nextWanderAt = Time.time + wanderInterval;
            }
            MoveTowards(wanderTarget);
        }
    }
}
