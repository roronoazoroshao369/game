using UnityEngine;
using WildernessCultivation.Core;
using WildernessCultivation.Mobs.States;
using WildernessCultivation.World;

namespace WildernessCultivation.Mobs
{
    /// <summary>
    /// Thỏ — passive wandering, flee khi player tới gần, eat grass-tile khi đói.
    /// R7 FSM: <see cref="RabbitStates.Wander"/> ↔ <see cref="RabbitStates.Flee"/> ↔ <see cref="RabbitStates.Eat"/>.
    /// Hunger decay theo thời gian → khi dưới <see cref="hungerEatThreshold"/> + có grass-tile gần
    /// → transition Eat → harvest grass + restore hunger.
    /// </summary>
    public class RabbitAI : MobBase
    {
        public float fleeRange = 3f;
        public float wanderRadius = 2.5f;
        public float wanderInterval = 3f;

        [Header("Hunger / Eat")]
        [Tooltip("Hunger max (full). Decay theo hungerDecayPerSecond → khi xuống dưới threshold, rabbit tìm grass.")]
        public float maxHunger = 100f;
        [Tooltip("Hunger hiện tại. Init = maxHunger * 0.8 mặc định.")]
        public float hunger = 80f;
        [Tooltip("Decay per second. 1 = full → empty trong 100s (~1.7 min).")]
        public float hungerDecayPerSecond = 1f;
        [Tooltip("Dưới ngưỡng này → rabbit chủ động tìm grass-tile để ăn.")]
        public float hungerEatThreshold = 50f;
        [Tooltip("Khoảng cách scan grass-tile xung quanh rabbit (unit). Mặc định = wanderRadius.")]
        public float grassScanRadius = 3f;
        [Tooltip("Khoảng cách rabbit phải vào để bắt đầu Eat (unit).")]
        public float eatRange = 0.5f;
        [Tooltip("Tổng duration eat animation (sec). Sau đó tile bị harvest + hunger restore.")]
        public float eatDuration = 1.5f;
        [Tooltip("Hunger restore per eat. 10 ≈ ngắn hạn (rabbit phải eat nhiều tile để full).")]
        public float hungerRestorePerEat = 10f;

        internal readonly StateMachine<RabbitAI> Fsm = new();

        Vector2 wanderTarget;
        float nextWanderAt;

        // Eat state context.
        GrassTile eatTarget;
        float eatStartTime;

        // State helpers (internal cho state class truy cập qua same-assembly).
        internal Vector2 WanderTarget => wanderTarget;
        internal float NextWanderAt => nextWanderAt;
        internal GrassTile EatTarget => eatTarget;
        internal float EatStartTime => eatStartTime;
        internal bool IsHungry => hunger < hungerEatThreshold;

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
            TickHunger(Time.deltaTime);
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

        /// <summary>Decay hunger theo dt. Pure additive → clamp 0..maxHunger.</summary>
        internal void TickHunger(float dt)
        {
            hunger = ComputeHungerAfterDecay(hunger, dt, hungerDecayPerSecond, maxHunger);
        }

        /// <summary>Restore hunger sau eat. Clamp tránh exceed maxHunger.</summary>
        internal void RestoreHunger(float amount)
        {
            hunger = Mathf.Clamp(hunger + amount, 0f, maxHunger);
        }

        /// <summary>
        /// Tìm GrassTile gần nhất trong scan radius. Skip tiles đã eaten (race khi multiple
        /// rabbits target cùng tile). Trả null nếu không có.
        /// </summary>
        internal GrassTile FindNearestGrassTile()
        {
            var hits = Physics2D.OverlapCircleAll(transform.position, grassScanRadius);
            GrassTile best = null;
            float bestSqr = float.MaxValue;
            foreach (var h in hits)
            {
                if (h == null) continue;
                var gt = h.GetComponent<GrassTile>();
                if (gt == null || gt.IsEaten) continue;
                float d = ((Vector2)gt.transform.position - (Vector2)transform.position).sqrMagnitude;
                if (d < bestSqr) { best = gt; bestSqr = d; }
            }
            return best;
        }

        internal void SetEatTarget(GrassTile target)
        {
            eatTarget = target;
            eatStartTime = Time.time;
        }

        internal void ClearEatTarget()
        {
            eatTarget = null;
        }

        // ============ Pure math (EditMode testable) ============

        /// <summary>Hunger sau dt giây decay. Clamp 0..max.</summary>
        public static float ComputeHungerAfterDecay(float currentHunger, float dt,
            float decayPerSecond, float maxHunger)
        {
            float v = currentHunger - dt * decayPerSecond;
            return Mathf.Clamp(v, 0f, maxHunger);
        }
    }
}
