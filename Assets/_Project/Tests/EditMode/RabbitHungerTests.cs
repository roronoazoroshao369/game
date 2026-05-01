using NUnit.Framework;
using WildernessCultivation.Mobs;

namespace WildernessCultivation.Tests.EditMode
{
    /// <summary>
    /// Pure math tests cho <see cref="RabbitAI.ComputeHungerAfterDecay"/>.
    /// Verify decay/clamp invariants không cần MonoBehaviour instance.
    /// </summary>
    public class RabbitHungerTests
    {
        [Test]
        public void HungerDecay_NoTime_ReturnsCurrent()
        {
            float v = RabbitAI.ComputeHungerAfterDecay(50f, 0f, 1f, 100f);
            Assert.AreEqual(50f, v, 0.0001f);
        }

        [Test]
        public void HungerDecay_StandardTick_ReducesByDtTimesRate()
        {
            // 50 - 0.5 * 2 = 49.
            float v = RabbitAI.ComputeHungerAfterDecay(50f, 0.5f, 2f, 100f);
            Assert.AreEqual(49f, v, 0.0001f);
        }

        [Test]
        public void HungerDecay_ClampsAtZero()
        {
            // Decay quá tay → clamp 0, KHÔNG âm.
            float v = RabbitAI.ComputeHungerAfterDecay(2f, 5f, 1f, 100f);
            Assert.AreEqual(0f, v, 0.0001f);
        }

        [Test]
        public void HungerDecay_ClampsAtMax_NegativeDt()
        {
            // Defensive: dt âm (clock skew) không cho hunger > max.
            float v = RabbitAI.ComputeHungerAfterDecay(95f, -10f, 1f, 100f);
            Assert.AreEqual(100f, v, 0.0001f);
        }

        [Test]
        public void HungerDecay_ZeroRate_NoChange()
        {
            float v = RabbitAI.ComputeHungerAfterDecay(80f, 1f, 0f, 100f);
            Assert.AreEqual(80f, v, 0.0001f);
        }
    }
}
