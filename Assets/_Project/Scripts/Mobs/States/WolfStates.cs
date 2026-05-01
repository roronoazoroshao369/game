using UnityEngine;
using WildernessCultivation.Combat;
using WildernessCultivation.Core;

namespace WildernessCultivation.Mobs.States
{
    /// <summary>
    /// Wolf chase+melee FSM. Singleton state instance per state (không alloc per frame).
    /// Pattern exemplar cho mob AI mới — xem <c>.agents/skills/add-mob/SKILL.md</c>.
    /// </summary>
    public static class WolfStates
    {
        public static readonly IState<WolfAI> Idle = new WolfIdle();
        public static readonly IState<WolfAI> Chase = new WolfChase();
        public static readonly IState<WolfAI> Attack = new WolfAttack();
        public static readonly IState<WolfAI> Dead = new WolfDead();
    }

    sealed class WolfIdle : IState<WolfAI>
    {
        public void OnEnter(WolfAI w) => w.StopMoving();
        public void OnExit(WolfAI w) { }
        public void OnTick(WolfAI w, float dt)
        {
            if (w.IsDead) { w.Fsm.ChangeState(WolfStates.Dead); return; }
            if (w.TryFindPlayer()) w.Fsm.ChangeState(WolfStates.Chase);
        }
    }

    sealed class WolfChase : IState<WolfAI>
    {
        // Stalking posture — wolf duck thấp khi rượt. SetCrouch sticky trên MobAnimController.
        public void OnEnter(WolfAI w) { w.Anim?.SetCrouch(true); }
        public void OnExit(WolfAI w) { w.Anim?.SetCrouch(false); }
        public void OnTick(WolfAI w, float dt)
        {
            if (w.IsDead) { w.Fsm.ChangeState(WolfStates.Dead); return; }
            if (w.target == null) { w.Fsm.ChangeState(WolfStates.Idle); return; }

            float dist = Vector2.Distance(w.target.position, w.transform.position);
            if (dist <= w.attackRange) { w.Fsm.ChangeState(WolfStates.Attack); return; }
            w.MoveTowards(w.target.position);
        }
    }

    sealed class WolfAttack : IState<WolfAI>
    {
        public void OnEnter(WolfAI w) => w.StopMoving();
        public void OnExit(WolfAI w) { }
        public void OnTick(WolfAI w, float dt)
        {
            if (w.IsDead) { w.Fsm.ChangeState(WolfStates.Dead); return; }
            if (w.target == null) { w.Fsm.ChangeState(WolfStates.Idle); return; }

            float dist = Vector2.Distance(w.target.position, w.transform.position);
            if (dist > w.attackRange) { w.Fsm.ChangeState(WolfStates.Chase); return; }

            w.StopMoving();
            if (Time.time >= w.AttackReadyAt)
            {
                w.AttackReadyAt = Time.time + w.attackCooldown;
                // Lunge forward + squash punch đồng pha với đòn damage.
                Vector2 dir = ((Vector2)w.target.position - (Vector2)w.transform.position).normalized;
                w.Anim?.TriggerLunge(dir);
                var dmg = w.target.GetComponent<IDamageable>() ?? w.target.GetComponentInParent<IDamageable>();
                if (dmg != null) dmg.TakeDamage(w.damage, w.gameObject);
            }
        }
    }

    sealed class WolfDead : IState<WolfAI>
    {
        public void OnEnter(WolfAI w) => w.StopMoving();
        public void OnTick(WolfAI w, float dt) { }
        public void OnExit(WolfAI w) { }
    }
}
