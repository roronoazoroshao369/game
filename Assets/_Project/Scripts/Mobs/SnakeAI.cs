using UnityEngine;
using WildernessCultivation.Combat;
using WildernessCultivation.Player;
using WildernessCultivation.Player.Status;

namespace WildernessCultivation.Mobs
{
    /// <summary>
    /// Rắn Độc — ambush. Ẩn (collider/sprite tắt) khi player ngoài revealRange,
    /// "hiện hình" + chase + lunge khi player vào tầm. Đập trúng player → áp
    /// <see cref="poisonEffect"/> (StatusEffectSO Poison) trong poisonDuration giây.
    /// Aggro phạm vi nhỏ (gần) — phải đứng quá gần mới lộ.
    /// </summary>
    public class SnakeAI : MobBase
    {
        [Header("Ambush")]
        [Tooltip("Player phải vào tầm này thì rắn mới hiện hình + chase. Nhỏ hơn aggroRange của wolf.")]
        public float revealRange = 2.5f;
        [Tooltip("Khi player rời khỏi giveUpRange (>= revealRange) thì rắn ẩn lại + reset target.")]
        public float giveUpRange = 5f;

        [Header("On-hit Poison")]
        public StatusEffectSO poisonEffect;
        [Tooltip("Duration apply lên player khi rắn cắn trúng. Re-apply sẽ refresh.")]
        public float poisonDuration = 6f;

        Collider2D col;
        bool revealed;
        public bool IsRevealed => revealed;

        protected override void Awake()
        {
            base.Awake();
            if (string.IsNullOrEmpty(mobName) || mobName == "Quái") mobName = "Rắn Độc";
            col = GetComponent<Collider2D>();
            SetVisible(false);
        }

        void SetVisible(bool show)
        {
            revealed = show;
            if (spriteRenderer != null) spriteRenderer.enabled = show;
            if (col != null) col.enabled = show;
        }

        void Update()
        {
            // Reveal check: player vào revealRange.
            var hit = Physics2D.OverlapCircle(transform.position, revealRange, playerMask);
            if (hit != null)
            {
                if (!revealed) SetVisible(true);
                target = hit.transform;
            }
            else if (revealed && target != null)
            {
                float d = Vector2.Distance(target.position, transform.position);
                if (d > giveUpRange)
                {
                    target = null;
                    SetVisible(false);
                    StopMoving();
                    return;
                }
            }

            if (!revealed || target == null) { StopMoving(); return; }

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

            // Apply Poison status nếu có effect + manager.
            if (poisonEffect != null)
            {
                var mgr = target.GetComponent<StatusEffectManager>() ?? target.GetComponentInParent<StatusEffectManager>();
                if (mgr != null) mgr.Apply(poisonEffect, poisonDuration);
            }
        }
    }
}
