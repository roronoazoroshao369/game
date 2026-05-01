using NUnit.Framework;
using UnityEngine;
using WildernessCultivation.Vfx;

namespace WildernessCultivation.Tests.EditMode
{
    /// <summary>
    /// Pure math tests cho <see cref="WaterRipple.ComputeRingScale"/> và
    /// <see cref="WaterRipple.ComputeRingAlpha"/> (ring expand + triangle alpha).
    /// </summary>
    public class WaterRippleMathTests
    {
        [Test]
        public void ComputeRingScale_AtZero_ReturnsStartScale()
        {
            Assert.AreEqual(0.3f, WaterRipple.ComputeRingScale(0f, 0.8f, 0.3f, 1.6f), 0.0001f);
        }

        [Test]
        public void ComputeRingScale_AtLifetime_ReturnsEndScale()
        {
            Assert.AreEqual(1.6f, WaterRipple.ComputeRingScale(0.8f, 0.8f, 0.3f, 1.6f), 0.0001f);
        }

        [Test]
        public void ComputeRingScale_AtHalf_LerpsToMidpoint()
        {
            // Lerp(0.3, 1.6, 0.5) = 0.95.
            Assert.AreEqual(0.95f, WaterRipple.ComputeRingScale(0.4f, 0.8f, 0.3f, 1.6f), 0.0001f);
        }

        [Test]
        public void ComputeRingScale_PastLifetime_ClampsToEndScale()
        {
            // u clamp 0..1 → t > lifetime → end.
            Assert.AreEqual(1.6f, WaterRipple.ComputeRingScale(2f, 0.8f, 0.3f, 1.6f), 0.0001f);
        }

        [Test]
        public void ComputeRingScale_ZeroLifetime_ReturnsEndScale()
        {
            // Defensive: chia 0 → caller fallback về end.
            Assert.AreEqual(1.6f, WaterRipple.ComputeRingScale(0f, 0f, 0.3f, 1.6f), 0.0001f);
        }

        [Test]
        public void ComputeRingAlpha_AtZero_ReturnsZero()
        {
            // Triangle 1 - |2u - 1| tại u=0 → 1 - 1 = 0.
            Assert.AreEqual(0f, WaterRipple.ComputeRingAlpha(0f, 0.8f), 0.0001f);
        }

        [Test]
        public void ComputeRingAlpha_AtMid_ReturnsOne()
        {
            // u=0.5 → 1 - 0 = 1.
            Assert.AreEqual(1f, WaterRipple.ComputeRingAlpha(0.4f, 0.8f), 0.0001f);
        }

        [Test]
        public void ComputeRingAlpha_AtLifetime_ReturnsZero()
        {
            // u=1 → 1 - 1 = 0 (fade tắt cuối).
            Assert.AreEqual(0f, WaterRipple.ComputeRingAlpha(0.8f, 0.8f), 0.0001f);
        }

        [Test]
        public void ComputeRingAlpha_QuarterTime_ReturnsHalf()
        {
            // u=0.25 → 1 - 0.5 = 0.5.
            Assert.AreEqual(0.5f, WaterRipple.ComputeRingAlpha(0.2f, 0.8f), 0.0001f);
        }

        [Test]
        public void ComputeRingAlpha_ZeroLifetime_ReturnsZero()
        {
            Assert.AreEqual(0f, WaterRipple.ComputeRingAlpha(0f, 0f), 0.0001f);
        }
    }
}
