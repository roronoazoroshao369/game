using UnityEngine;
using WildernessCultivation.Core;
using WildernessCultivation.Mobs.States;

namespace WildernessCultivation.Mobs
{
    /// <summary>
    /// Thỏ — passive wandering, flee khi player tới gần.
    /// R7 FSM: <see cref="RabbitStates.Wander"/> ↔ <see cref="RabbitStates.Flee"/>.
    /// </summary>
    public class RabbitAI : MobBase
    {
        public float fleeRange = 3f;
        public float wanderRadius = 2.5f;
        public float wanderInterval = 3f;

        internal readonly StateMachine<RabbitAI> Fsm = new();

        Vector2 wanderTarget;
        float nextWanderAt;

        // State helpers (internal cho state class truy cập qua same-assembly).
        internal Vector2 WanderTarget => wanderTarget;
        internal float NextWanderAt => nextWanderAt;

        protected override void Awake()
        {
            base.Awake();
            mobName = "Thỏ Rừng";
            wanderTarget = transform.position;
            Fsm.Init(this, RabbitStates.Wander);
        }

        void Update()
        {
            if (!ShouldTickAI()) return;
            Fsm.Tick(Time.deltaTime);
        }

        protected override void Die(GameObject killer)
        {
            Fsm.Shutdown();
            base.Die(killer);
        }

        internal void ReseedWanderTargetIfNeeded()
        {
            if (wanderTarget == (Vector2)transform.position) ReseedWanderTarget();
        }

        internal void ReseedWanderTarget()
        {
            wanderTarget = (Vector2)transform.position + Random.insideUnitCircle * wanderRadius;
            nextWanderAt = Time.time + wanderInterval;
        }

        internal void SetFleeFrom(Transform t) { /* reserved cho future flee-target aggro */ }

        internal void SetVelocity(Vector2 v)
        {
            var rb = GetComponent<Rigidbody2D>();
            if (rb != null) rb.velocity = v;
        }

        internal void FlipSpriteToward(Vector2 dir)
        {
            if (spriteRenderer != null && Mathf.Abs(dir.x) > 0.05f)
                spriteRenderer.flipX = dir.x < 0;
        }
    }
}
