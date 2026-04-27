using UnityEngine;
using WildernessCultivation.Combat;
using WildernessCultivation.Player;

namespace WildernessCultivation.Mobs
{
    /// <summary>
    /// Sói — chase + melee. Aggro ngay khi thấy player trong tầm.
    /// </summary>
    public class WolfAI : MobBase
    {
        protected override void Awake()
        {
            base.Awake();
            mobName = "Sói Hoang";
        }

        void Update()
        {
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
                        // Player KHÔNG implement IDamageable (TakeDamage(float) trên PlayerStats);
                        // fallback giống BossMobAI để wolf vẫn có thể đánh player.
                        var ps = target.GetComponent<PlayerStats>() ?? target.GetComponentInParent<PlayerStats>();
                        ps?.TakeDamage(damage);
                    }
                }
            }
        }
    }
}
