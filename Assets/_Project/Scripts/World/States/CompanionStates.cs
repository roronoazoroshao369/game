using WildernessCultivation.Core;

namespace WildernessCultivation.World.States
{
    /// <summary>
    /// FSM state singletons cho <see cref="CompanionNPC"/>. Minimal 3 state cho scope MVP:
    /// Idle (chờ) / Follow (đi theo target) / Dead. Pattern mirror <c>Mobs.States.WolfStates</c>.
    /// Combat state (attack mob) là future work.
    /// </summary>
    public static class CompanionStates
    {
        public static readonly IState<CompanionNPC> Idle = new CompanionIdle();
        public static readonly IState<CompanionNPC> Follow = new CompanionFollow();
        public static readonly IState<CompanionNPC> Dead = new CompanionDead();
    }

    sealed class CompanionIdle : IState<CompanionNPC>
    {
        public void OnEnter(CompanionNPC c) { }
        public void OnExit(CompanionNPC c) { }
        public void OnTick(CompanionNPC c, float dt)
        {
            if (c.IsDead) { c.Fsm.ChangeState(CompanionStates.Dead); return; }
            if (c.mode != CompanionMode.Follow) return;
            if (c.followTarget == null) return;
            if (c.DistanceToTarget() > c.followDistance)
                c.Fsm.ChangeState(CompanionStates.Follow);
        }
    }

    sealed class CompanionFollow : IState<CompanionNPC>
    {
        public void OnEnter(CompanionNPC c) { }
        public void OnExit(CompanionNPC c) { }
        public void OnTick(CompanionNPC c, float dt)
        {
            if (c.IsDead) { c.Fsm.ChangeState(CompanionStates.Dead); return; }
            if (c.mode == CompanionMode.Stay)
            {
                c.Fsm.ChangeState(CompanionStates.Idle);
                return;
            }
            if (c.followTarget == null)
            {
                c.Fsm.ChangeState(CompanionStates.Idle);
                return;
            }
            if (c.DistanceToTarget() <= c.stopDistance)
            {
                c.Fsm.ChangeState(CompanionStates.Idle);
                return;
            }
            c.MoveTowardTargetStep(dt);
        }
    }

    sealed class CompanionDead : IState<CompanionNPC>
    {
        public void OnEnter(CompanionNPC c) { }
        public void OnTick(CompanionNPC c, float dt) { }
        public void OnExit(CompanionNPC c) { }
    }
}
