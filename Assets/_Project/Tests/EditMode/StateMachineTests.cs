using NUnit.Framework;
using WildernessCultivation.Core;

namespace WildernessCultivation.Tests.EditMode
{
    public class StateMachineTests
    {
        class Ctx
        {
            public string log = "";
        }

        static DelegateState<Ctx> MakeState(string name)
        {
            return new DelegateState<Ctx>(
                enter: c => c.log += $"E<{name}>",
                tick: (c, dt) => c.log += $"T<{name}>",
                exit: c => c.log += $"X<{name}>"
            );
        }

        class CtxWithFsm : Ctx
        {
            public StateMachine<Ctx> Fsm = new();
        }

        [Test]
        public void Init_CallsOnEnter()
        {
            var fsm = new StateMachine<Ctx>();
            var ctx = new Ctx();
            fsm.Init(ctx, MakeState("A"));
            Assert.AreEqual("E<A>", ctx.log);
        }

        [Test]
        public void Tick_CallsOnTick()
        {
            var fsm = new StateMachine<Ctx>();
            var ctx = new Ctx();
            fsm.Init(ctx, MakeState("A"));
            fsm.Tick(0.016f);
            fsm.Tick(0.016f);
            Assert.AreEqual("E<A>T<A>T<A>", ctx.log);
        }

        [Test]
        public void ChangeState_CallsExitThenEnter()
        {
            var fsm = new StateMachine<Ctx>();
            var ctx = new Ctx();
            var a = MakeState("A");
            var b = MakeState("B");
            fsm.Init(ctx, a);
            fsm.ChangeState(b);
            Assert.AreEqual("E<A>X<A>E<B>", ctx.log);
        }

        [Test]
        public void ChangeState_SameState_NoOp()
        {
            var fsm = new StateMachine<Ctx>();
            var ctx = new Ctx();
            var a = MakeState("A");
            fsm.Init(ctx, a);
            fsm.ChangeState(a);
            Assert.AreEqual("E<A>", ctx.log, "ChangeState với same instance → không re-enter");
        }

        [Test]
        public void ChangeState_InsideTick_Queued_AppliedAfterTick()
        {
            // Verify reentrant-safe: ChangeState trong OnTick không corrupt state giữa tick.
            var ctx = new CtxWithFsm();
            var b = new DelegateState<Ctx>(
                enter: c => c.log += "E<B>",
                tick: (c, dt) => c.log += "T<B>",
                exit: c => c.log += "X<B>");
            var a = new DelegateState<Ctx>(
                enter: c => c.log += "E<A>",
                tick: (c, dt) =>
                {
                    c.log += "T<A>";
                    ((CtxWithFsm)c).Fsm.ChangeState(b);
                    // Log sau ChangeState: vẫn trong tick A, B chưa Enter.
                    c.log += "after";
                },
                exit: c => c.log += "X<A>");
            ctx.Fsm.Init(ctx, a);
            ctx.Fsm.Tick(0.016f);
            // Order phải là: T<A> → afterT<A> → X<A> → E<B>
            Assert.AreEqual("E<A>T<A>afterX<A>E<B>", ctx.log);
        }

        [Test]
        public void Shutdown_CallsOnExit_AndClearsCurrent()
        {
            var fsm = new StateMachine<Ctx>();
            var ctx = new Ctx();
            fsm.Init(ctx, MakeState("A"));
            fsm.Shutdown();
            Assert.AreEqual("E<A>X<A>", ctx.log);
            Assert.IsNull(fsm.Current);
        }

        [Test]
        public void Tick_WithoutInit_NoOp()
        {
            var fsm = new StateMachine<Ctx>();
            Assert.DoesNotThrow(() => fsm.Tick(0.016f));
        }

        [Test]
        public void ChangeState_Null_NoOp()
        {
            var fsm = new StateMachine<Ctx>();
            var ctx = new Ctx();
            var a = MakeState("A");
            fsm.Init(ctx, a);
            fsm.ChangeState(null);
            Assert.AreSame(a, fsm.Current);
        }
    }
}
