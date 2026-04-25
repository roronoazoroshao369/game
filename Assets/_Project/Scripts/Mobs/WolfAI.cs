using UnityEngine;
using WildernessCultivation.Combat;

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
                    dmg?.TakeDamage(damage, gameObject);
                }
            }
        }
    }
}
