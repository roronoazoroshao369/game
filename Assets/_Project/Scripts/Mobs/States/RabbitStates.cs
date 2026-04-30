using UnityEngine;
using WildernessCultivation.Core;

namespace WildernessCultivation.Mobs.States
{
    /// <summary>Rabbit passive wander + flee FSM. Exemplar cho "flee-type" mob (Deer, Crow tương tự).</summary>
    public static class RabbitStates
    {
        public static readonly IState<RabbitAI> Wander = new RabbitWander();
        public static readonly IState<RabbitAI> Flee = new RabbitFlee();
    }

    sealed class RabbitWander : IState<RabbitAI>
    {
        public void OnEnter(RabbitAI r) => r.ReseedWanderTargetIfNeeded();
        public void OnExit(RabbitAI r) { }
        public void OnTick(RabbitAI r, float dt)
        {
            // Gần player quá → flee.
            var hit = Physics2D.OverlapCircle(r.transform.position, r.fleeRange, r.playerMask);
            if (hit != null) { r.SetFleeFrom(hit.transform); r.Fsm.ChangeState(RabbitStates.Flee); return; }

            // Reseed wander target khi tới nơi hoặc hết interval.
            if (Time.time >= r.NextWanderAt || Vector2.Distance(r.transform.position, r.WanderTarget) < 0.2f)
                r.ReseedWanderTarget();

            r.MoveTowards(r.WanderTarget);
        }
    }

    sealed class RabbitFlee : IState<RabbitAI>
    {
        public void OnEnter(RabbitAI r) { }
        public void OnExit(RabbitAI r) { }
        public void OnTick(RabbitAI r, float dt)
        {
            // Player còn trong flee range?
            var hit = Physics2D.OverlapCircle(r.transform.position, r.fleeRange, r.playerMask);
            if (hit == null) { r.Fsm.ChangeState(RabbitStates.Wander); return; }

            Vector2 fleeDir = ((Vector2)r.transform.position - (Vector2)hit.transform.position).normalized;
            r.SetVelocity(fleeDir * (r.moveSpeed * 1.6f));
            r.FlipSpriteToward(fleeDir);
        }
    }
}
