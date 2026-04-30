using UnityEngine;
using WildernessCultivation.Combat;
using WildernessCultivation.Core;
using WildernessCultivation.Player;
using WildernessCultivation.Player.Status;

namespace WildernessCultivation.Mobs
{
    /// <summary>
    /// Hắc Bức — chỉ hoạt động ban đêm. Tốc độ trung bình, swarm aggressive khi
    /// thấy player, cắn → áp <see cref="bleedEffect"/> (Bleeding) ngắn nhưng
    /// stack được nếu nhiều con cùng đập. Ngày: ẩn (collider/sprite tắt).
    /// </summary>
    public class BatAI : MobBase
    {
        [Header("On-hit Bleed")]
        public StatusEffectSO bleedEffect;
        [Tooltip("Duration apply lên player khi bat cắn trúng.")]
        public float bleedDuration = 4f;

        TimeManager time;
        Collider2D col;

        protected override void Awake()
        {
            base.Awake();
            if (string.IsNullOrEmpty(mobName) || mobName == "Quái") mobName = "Hắc Bức";
            col = GetComponent<Collider2D>();
        }

        void Start()
        {
            time = GameManager.Instance != null ? GameManager.Instance.timeManager : ServiceLocator.Get<TimeManager>();
        }

        void Update()
        {
            if (!ShouldTickAI()) return;

            // Day = hidden, night = active. Same pattern FoxSpiritAI.
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
                    DealBiteDamage();
                }
            }
        }

        void DealBiteDamage()
        {
            if (target == null) return;

            var dmg = target.GetComponent<IDamageable>() ?? target.GetComponentInParent<IDamageable>();
            if (dmg != null) dmg.TakeDamage(damage, gameObject);
            else
            {
                var ps = target.GetComponent<PlayerStats>() ?? target.GetComponentInParent<PlayerStats>();
                ps?.TakeDamage(damage);
            }

            if (bleedEffect != null)
            {
                var mgr = target.GetComponent<StatusEffectManager>() ?? target.GetComponentInParent<StatusEffectManager>();
                if (mgr != null) mgr.Apply(bleedEffect, bleedDuration);
            }
        }
    }
}
