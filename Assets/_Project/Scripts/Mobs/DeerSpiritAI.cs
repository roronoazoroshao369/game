using UnityEngine;

namespace WildernessCultivation.Mobs
{
    /// <summary>
    /// Lộc Thần — passive, fast flee. Cảnh giác cao (sightRange) hơn thỏ; chạy
    /// nhanh hơn (fleeSpeedMultiplier). Drop nguyên liệu cao cấp (gạc linh, thịt
    /// linh) cho alchemy / cooking — phải săn khôn ngoan, không chạy bộ kịp.
    /// </summary>
    public class DeerSpiritAI : MobBase
    {
        [Header("Deer tuning")]
        [Tooltip("Phạm vi nhìn thấy player → bắt đầu chạy.")]
        public float sightRange = 5f;
        [Tooltip("Tốc độ chạy = moveSpeed × multiplier khi flee.")]
        public float fleeSpeedMultiplier = 2.2f;
        [Tooltip("Wander radius khi không thấy player.")]
        public float wanderRadius = 3.5f;
        [Tooltip("Khoảng giữa 2 lần đổi đích wander.")]
        public float wanderInterval = 3.5f;
        [Tooltip("Sau bao lâu mất dấu player thì quay về wander.")]
        public float fleeMemorySec = 2f;

        Vector2 wanderTarget;
        float nextWanderAt;
        float lastSeenAt = -999f;
        Vector2 lastSeenPlayerPos;
        public bool IsFleeing => Time.time - lastSeenAt < fleeMemorySec;

        protected override void Awake()
        {
            base.Awake();
            if (string.IsNullOrEmpty(mobName) || mobName == "Quái") mobName = "Lộc Thần";
            wanderTarget = transform.position;
        }

        void Update()
        {
            // Sight: player trong sightRange → cập nhật lastSeenAt.
            var hit = Physics2D.OverlapCircle(transform.position, sightRange, playerMask);
            if (hit != null)
            {
                lastSeenAt = Time.time;
                lastSeenPlayerPos = hit.transform.position;
            }

            if (IsFleeing)
            {
                Vector2 fleeDir = ((Vector2)transform.position - lastSeenPlayerPos).normalized;
                if (fleeDir.sqrMagnitude < 0.001f) fleeDir = Vector2.right;
                rb.velocity = fleeDir * (moveSpeed * fleeSpeedMultiplier);
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
