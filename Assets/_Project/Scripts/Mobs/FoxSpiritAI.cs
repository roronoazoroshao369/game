using UnityEngine;
using WildernessCultivation.Combat;
using WildernessCultivation.Core;
using WildernessCultivation.Items;
using WildernessCultivation.Player;

namespace WildernessCultivation.Mobs
{
    /// <summary>
    /// Yêu Hồ — chỉ xuất hiện ban đêm. Tốc độ nhanh, drop linh dược.
    /// Khi ngày: ẩn (collider/sprite tắt). Khi đêm: hiện + chase player.
    /// Sau day 7 có chance drop Linh Quả (kì ngộ khai mở tu tiên).
    /// </summary>
    public class FoxSpiritAI : MobBase
    {
        public float dayHiddenTimer;

        [Header("Spirit Fruit drop (kì ngộ khai mở)")]
        [Tooltip("Item Linh Quả. Drop 10% sau day 7 — null thì không drop.")]
        public ItemSO spiritFruitItem;
        [Tooltip("Số ngày tối thiểu trước khi Linh Quả có thể drop.")]
        public int spiritFruitMinDay = 7;
        [Range(0f, 1f)]
        [Tooltip("Tỉ lệ drop Linh Quả mỗi lần kill (sau khi đủ ngày).")]
        public float spiritFruitDropChance = 0.10f;

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
            time = GameManager.Instance != null ? GameManager.Instance.timeManager : ServiceLocator.Get<TimeManager>();
        }

        protected override void Die(GameObject killer)
        {
            // Drop Linh Quả vào inventory player TRƯỚC khi base.Die destroy GO.
            if (killer != null && spiritFruitItem != null)
            {
                int day = time != null ? time.daysSurvived : 0;
                if (day >= spiritFruitMinDay && Random.value <= spiritFruitDropChance)
                {
                    var inv = killer.GetComponentInParent<Inventory>();
                    if (inv != null) inv.Add(spiritFruitItem, 1);
                    Debug.Log("[FoxSpirit] Drop Linh Quả!");
                }
            }
            base.Die(killer);
        }

        void Update()
        {
            if (!ShouldTickAI()) return;

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
                    if (dmg != null)
                    {
                        dmg.TakeDamage(damage, gameObject);
                    }
                    else
                    {
                        // Player KHÔNG implement IDamageable; fallback giống BossMobAI/WolfAI.
                        var ps = target.GetComponent<PlayerStats>() ?? target.GetComponentInParent<PlayerStats>();
                        ps?.TakeDamage(damage);
                    }
                }
            }
        }
    }
}
