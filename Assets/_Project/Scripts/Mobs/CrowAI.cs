using UnityEngine;

namespace WildernessCultivation.Mobs
{
    /// <summary>
    /// Quạ — flying scavenger. Bay quanh theo random patrol, không aggressive,
    /// HP thấp, drop lông (feather, dùng làm tên). Khi player tới gần tạm thời
    /// flit đi xa rồi tiếp tục patrol — không panic-flee triệt để như deer/rabbit.
    /// </summary>
    public class CrowAI : MobBase
    {
        [Header("Crow tuning")]
        [Tooltip("Bán kính patrol quanh điểm anchor ban đầu.")]
        public float patrolRadius = 5f;
        [Tooltip("Khoảng giữa 2 lần đổi đích patrol.")]
        public float patrolInterval = 2.5f;
        [Tooltip("Player vào tầm này → flit (đổi hướng patrol đột ngột).")]
        public float playerNoticeRange = 1.8f;

        Vector2 anchor;
        Vector2 patrolTarget;
        float nextPatrolAt;

        protected override void Awake()
        {
            base.Awake();
            if (string.IsNullOrEmpty(mobName) || mobName == "Quái") mobName = "Quạ";
            anchor = transform.position;
            patrolTarget = anchor;
        }

        void Update()
        {
            if (!ShouldTickAI()) return;

            // Player gần → flit ngay sang patrol point mới (xa player), rồi tiếp tục patrol bình thường.
            var hit = Physics2D.OverlapCircle(transform.position, playerNoticeRange, playerMask);
            if (hit != null)
            {
                Vector2 away = ((Vector2)transform.position - (Vector2)hit.transform.position).normalized;
                if (away.sqrMagnitude < 0.001f) away = Vector2.right;
                patrolTarget = anchor + away * patrolRadius;
                nextPatrolAt = Time.time + patrolInterval;
            }

            if (Time.time >= nextPatrolAt || Vector2.Distance(transform.position, patrolTarget) < 0.2f)
            {
                patrolTarget = anchor + Random.insideUnitCircle * patrolRadius;
                nextPatrolAt = Time.time + patrolInterval;
            }

            MoveTowards(patrolTarget);
        }
    }
}
