using WildernessCultivation.Core;
using WildernessCultivation.Mobs.States;

namespace WildernessCultivation.Mobs
{
    /// <summary>
    /// Sói — chase + melee. R7 FSM: <see cref="WolfStates.Idle"/> → Chase → Attack → Dead.
    /// Transition logic sống trong state class (WolfStates.cs). Update() chỉ tick FSM.
    /// </summary>
    public class WolfAI : MobBase
    {
        internal readonly StateMachine<WolfAI> Fsm = new();

        protected override void Awake()
        {
            base.Awake();
            mobName = "Sói Hoang";
            Fsm.Init(this, WolfStates.Idle);
        }

        void Update()
        {
            if (!ShouldTickAI()) return;
            Fsm.Tick(UnityEngine.Time.deltaTime);
        }

        protected override void Die(UnityEngine.GameObject killer)
        {
            Fsm.Shutdown();
            base.Die(killer);
        }
    }
}
