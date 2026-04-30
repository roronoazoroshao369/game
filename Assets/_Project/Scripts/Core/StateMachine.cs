using System;
using System.Collections.Generic;

namespace WildernessCultivation.Core
{
    /// <summary>
    /// Contract cho state trong <see cref="StateMachine{TContext}"/>.
    /// <typeparamref name="TContext"/> là "owner" truyền vào (MobBase / PlayerController / BossMobAI…),
    /// state đọc/ghi qua ref đó thay vì đóng gói field riêng.
    ///
    /// <para>Lifecycle: <see cref="OnEnter"/> gọi 1 lần khi enter → <see cref="OnTick"/>
    /// gọi mỗi frame state này đang active → <see cref="OnExit"/> gọi 1 lần khi switch sang state khác.
    /// State object typically là <c>readonly</c> instance (pooled) — KHÔNG giữ state mutable instance-level;
    /// mutable state sống trên <typeparamref name="TContext"/>.</para>
    /// </summary>
    public interface IState<TContext>
    {
        void OnEnter(TContext ctx);
        void OnTick(TContext ctx, float dt);
        void OnExit(TContext ctx);
    }

    /// <summary>
    /// Finite state machine viết tay, lightweight (no alloc mỗi frame). Thiết kế cho:
    /// - Mob AI (Patrol/Chase/Attack/Flee/Dead)
    /// - Player (Idle/Move/Dodge/Attack/Channel/Sleep/Dead) qua hook UI/animation/save.
    ///
    /// <para>Usage (trong MonoBehaviour):</para>
    /// <code>
    /// readonly StateMachine&lt;WolfAI&gt; fsm = new();
    /// void Awake() { fsm.Init(this, MobStates.Idle); }
    /// void Update() { fsm.Tick(Time.deltaTime); }
    /// // Transition bất cứ đâu:
    /// fsm.ChangeState(MobStates.Chase);
    /// </code>
    ///
    /// <para>Reentrant-safe: <c>ChangeState</c> trong <c>OnTick</c>/<c>OnEnter</c>/<c>OnExit</c>
    /// được queue + apply sau call frame hiện tại để tránh state-corruption khi Enter fire
    /// một transition khác. Multi-enqueue trong cùng tick: last wins.</para>
    /// </summary>
    public sealed class StateMachine<TContext>
    {
        TContext ctx;
        IState<TContext> current;
        IState<TContext> pending;
        bool hasPending;
        bool ticking;

        public IState<TContext> Current => current;

        /// <summary>Khởi tạo FSM với context + initial state. Gọi 1 lần trong <c>Awake</c>/<c>Start</c>.</summary>
        public void Init(TContext context, IState<TContext> initial)
        {
            ctx = context;
            current = initial;
            current?.OnEnter(ctx);
        }

        /// <summary>Chuyển state. An toàn khi gọi trong OnEnter/OnTick/OnExit (queue + apply cuối frame).</summary>
        public void ChangeState(IState<TContext> next)
        {
            if (next == null) return;
            if (ticking)
            {
                pending = next;
                hasPending = true;
                return;
            }
            ApplyChange(next);
        }

        void ApplyChange(IState<TContext> next)
        {
            if (ReferenceEquals(current, next)) return;
            current?.OnExit(ctx);
            current = next;
            current?.OnEnter(ctx);
        }

        /// <summary>Tick state hiện tại. Gọi từ <c>Update</c>/<c>FixedUpdate</c> của host MonoBehaviour.</summary>
        public void Tick(float dt)
        {
            if (current == null) return;
            ticking = true;
            try { current.OnTick(ctx, dt); }
            finally { ticking = false; }
            if (hasPending)
            {
                var next = pending;
                pending = null;
                hasPending = false;
                ApplyChange(next);
            }
        }

        /// <summary>Force exit — không Enter state khác. Dùng khi host bị destroy / disable.</summary>
        public void Shutdown()
        {
            current?.OnExit(ctx);
            current = null;
            pending = null;
            hasPending = false;
        }
    }

    /// <summary>
    /// Stateless state helper — implement IState bằng 3 delegate. Tiện cho state nhỏ không cần class riêng.
    /// Vẫn giữ invariant "state object readonly": delegate capture field của host (context), không giữ
    /// local state mutable trong lambda.
    /// </summary>
    public sealed class DelegateState<TContext> : IState<TContext>
    {
        readonly Action<TContext> enter;
        readonly Action<TContext, float> tick;
        readonly Action<TContext> exit;

        public DelegateState(
            Action<TContext> enter = null,
            Action<TContext, float> tick = null,
            Action<TContext> exit = null)
        {
            this.enter = enter;
            this.tick = tick;
            this.exit = exit;
        }

        public void OnEnter(TContext ctx) => enter?.Invoke(ctx);
        public void OnTick(TContext ctx, float dt) => tick?.Invoke(ctx, dt);
        public void OnExit(TContext ctx) => exit?.Invoke(ctx);
    }

    /// <summary>Enum broadcast cho Player state (UI/animation/save subscribe). R7 lightweight view.</summary>
    public enum PlayerActivityState
    {
        Idle,
        Moving,
        Dodging,
        Attacking,
        Channeling,
        Sleeping,
        Dead,
    }
}
