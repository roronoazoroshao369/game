using NUnit.Framework;
using WildernessCultivation.Mobs;

namespace WildernessCultivation.Tests.EditMode
{
    /// <summary>
    /// Test pure logic <see cref="MobBase.ComputeShouldTick"/>. Tách static khỏi
    /// MonoBehaviour để verify không cần Rigidbody2D / scene host. Kiểm tra:
    ///  - Mob trong tầm (near) → luôn tick + simulated.
    ///  - Mob xa → simulated=false + chỉ tick 1/N frame.
    ///  - lodFarDistance &lt;= 0 → tắt LOD, luôn tick.
    /// </summary>
    public class MobLodTests
    {
        [Test]
        public void Near_AlwaysTick_AndSimulated()
        {
            bool tick = MobBase.ComputeShouldTick(
                distance: 5f, lodFarDistance: 18f, lodSlowFrameMod: 8,
                frameCount: 0, instanceId: 0, out bool wantSim);
            Assert.IsTrue(tick, "Near mob phải tick mỗi frame.");
            Assert.IsTrue(wantSim, "Near mob phải bật simulated.");
        }

        [Test]
        public void Far_PhysicsDisabled()
        {
            MobBase.ComputeShouldTick(
                distance: 50f, lodFarDistance: 18f, lodSlowFrameMod: 8,
                frameCount: 0, instanceId: 0, out bool wantSim);
            Assert.IsFalse(wantSim, "Far mob phải tắt simulated để tiết kiệm physics tick.");
        }

        [Test]
        public void Far_TicksOncePerSlowMod()
        {
            // (frameCount + instanceId) % mod == 0 → tick. mod=8, instanceId=0:
            // frame 0 tick, 1..7 skip, 8 tick.
            int hits = 0;
            for (int f = 0; f < 16; f++)
            {
                bool t = MobBase.ComputeShouldTick(
                    distance: 50f, lodFarDistance: 18f, lodSlowFrameMod: 8,
                    frameCount: f, instanceId: 0, out _);
                if (t) hits++;
            }
            Assert.AreEqual(2, hits, "Far mob phải tick đúng 2 lần trong 16 frame (mod=8).");
        }

        [Test]
        public void Far_DifferentInstancesPhaseOffset()
        {
            // Hai mob xa với instanceId khác nhau → tick ở frame khác nhau (avoid spike).
            int instA = 0, instB = 3;
            bool tickA0 = MobBase.ComputeShouldTick(50f, 18f, 8, 0, instA, out _);
            bool tickB0 = MobBase.ComputeShouldTick(50f, 18f, 8, 0, instB, out _);
            // Chỉ 1 trong 2 tick ở frame 0 — pha lệch.
            Assert.AreNotEqual(tickA0, tickB0,
                "Mob khác instanceId phải lệch pha tick để tránh spike đồng loạt.");
        }

        [Test]
        public void LodDisabled_AlwaysTick()
        {
            // lodFarDistance <= 0 → tắt LOD. Player ở mọi distance đều tick.
            bool t1 = MobBase.ComputeShouldTick(0f, 0f, 8, 0, 0, out bool sim1);
            bool t2 = MobBase.ComputeShouldTick(1000f, -1f, 8, 5, 99, out bool sim2);
            Assert.IsTrue(t1);
            Assert.IsTrue(sim1);
            Assert.IsTrue(t2);
            Assert.IsTrue(sim2);
        }

        [Test]
        public void Boundary_ExactlyAtFarDistance_TreatedAsNear()
        {
            // distance == lodFarDistance → near (predicate `distance > lodFarDistance` false).
            bool tick = MobBase.ComputeShouldTick(
                distance: 18f, lodFarDistance: 18f, lodSlowFrameMod: 8,
                frameCount: 5, instanceId: 7, out bool wantSim);
            Assert.IsTrue(tick);
            Assert.IsTrue(wantSim);
        }
    }
}
