using UnityEngine;
using WildernessCultivation.Combat;
using WildernessCultivation.Core;

namespace WildernessCultivation.Mobs
{
    /// <summary>
    /// Yêu Hồ — chỉ xuất hiện ban đêm. Tốc độ nhanh, drop linh dược.
    /// Khi ngày: ẩn (collider/sprite tắt). Khi đêm: hiện + chase player.
    /// </summary>
    public class FoxSpiritAI : MobBase
    {
        public float dayHiddenTimer;
        TimeManager time;
        Collider2D col;

        protected override void Awake()
        {
            base.Awake();
            mobName = "Yêu Hồ";
            col = GetComponent<Collider2D>();
        }

        void Start()
        {
            time = GameManager.Instance != null ? GameManager.Instance.timeManager : FindObjectOfType<TimeManager>();
        }

        void Update()
        {
            bool active = time == null || time.isNight;
            if (spriteRenderer != null) spriteRenderer.enabled = active;
            if (col != null) col.enabled = active;
            if (!active) { StopMoving(); return; }

            if (!TryFindPlayer()) { StopMoving(); return; }

            float dist = Vector2.Distance(target.position, transform.position);
            if (dist > attackRange)
            {
                MoveTowards(target.position);
            }
            else
            {
                StopMoving();
                if (Time.time >= attackReadyAt)
                {
                    attackReadyAt = Time.time + attackCooldown;
                    var dmg = target.GetComponent<IDamageable>() ?? target.GetComponentInParent<IDamageable>();
                    dmg?.TakeDamage(damage, gameObject);
                }
            }
        }
    }
}
