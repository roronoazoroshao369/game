using NUnit.Framework;
using UnityEngine;
using WildernessCultivation.Vfx;

namespace WildernessCultivation.Tests.EditMode
{
    /// <summary>
    /// Pure math tests cho <see cref="MobAnimController"/> static methods:
    /// ComputeWalkBobScale, ComputeIdleBreathScale, ComputeTiltDeg,
    /// ComputeLungeOffset, ComputeSquashFactor.
    /// </summary>
    public class MobAnimMathTests
    {
        // ---------- Walk bob scale ----------

        [Test]
        public void WalkBobScale_ZeroSpeed_ReturnsOne()
        {
            // Đứng yên → no bob.
            float v = MobAnimController.ComputeWalkBobScale(time: 1.234f, speed: 0f,
                amplitude: 0.05f, frequencyHz: 5f, referenceSpeed: 2f);
            Assert.AreEqual(1f, v, 0.0001f);
        }

        [Test]
        public void WalkBobScale_AtSinPeak_AtRefSpeed_ReturnsOnePlusAmplitude()
        {
            // sin(2π·freq·t) = 1 khi 2π·freq·t = π/2 → freq·t = 0.25 → t = 0.25 / freq.
            // Với freq=5 → t = 0.05. speed = refSpeed → factor = 1.
            float t = 0.25f / 5f;
            float v = MobAnimController.ComputeWalkBobScale(t, 2f, 0.05f, 5f, 2f);
            Assert.AreEqual(1f + 0.05f, v, 0.001f);
        }

        [Test]
        public void WalkBobScale_HalfRefSpeed_ScalesAmplitudeHalf()
        {
            // speed = refSpeed/2 → speedFactor = 0.5 → bob amplitude = 0.5 * 0.05.
            float t = 0.25f / 5f;
            float v = MobAnimController.ComputeWalkBobScale(t, 1f, 0.05f, 5f, 2f);
            Assert.AreEqual(1f + 0.5f * 0.05f, v, 0.001f);
        }

        [Test]
        public void WalkBobScale_OverRefSpeed_ClampsToFullAmplitude()
        {
            // Speed > refSpeed → clamp 1 → full amplitude.
            float t = 0.25f / 5f;
            float v = MobAnimController.ComputeWalkBobScale(t, 10f, 0.05f, 5f, 2f);
            Assert.AreEqual(1f + 0.05f, v, 0.001f);
        }

        [Test]
        public void WalkBobScale_ZeroRefSpeed_DefensiveReturnsOne()
        {
            float v = MobAnimController.ComputeWalkBobScale(0.5f, 1f, 0.05f, 5f, 0f);
            Assert.AreEqual(1f, v, 0.0001f);
        }

        // ---------- Idle breath ----------

        [Test]
        public void IdleBreath_AtZero_ReturnsOne()
        {
            // sin(0) = 0 → factor = 1.
            Assert.AreEqual(1f, MobAnimController.ComputeIdleBreathScale(0f, 0.02f, 1.2f), 0.0001f);
        }

        [Test]
        public void IdleBreath_AtPeakTime_ReturnsOnePlusAmplitude()
        {
            // sin peak khi 2π·freq·t = π/2 → t = 1/(4·freq).
            float t = 1f / (4f * 1.2f);
            float v = MobAnimController.ComputeIdleBreathScale(t, 0.02f, 1.2f);
            Assert.AreEqual(1f + 0.02f, v, 0.001f);
        }

        // ---------- Tilt ----------

        [Test]
        public void Tilt_BelowThreshold_ReturnsZero()
        {
            // Tránh jitter khi mob đứng yên.
            Assert.AreEqual(0f, MobAnimController.ComputeTiltDeg(0.01f, 5f, 0.05f), 0.0001f);
            Assert.AreEqual(0f, MobAnimController.ComputeTiltDeg(-0.01f, 5f, 0.05f), 0.0001f);
        }

        [Test]
        public void Tilt_PositiveVelocity_ReturnsNegativeMaxTilt()
        {
            // Đi phải → sprite leans forward → -maxTilt (Z rotation âm).
            Assert.AreEqual(-5f, MobAnimController.ComputeTiltDeg(2f, 5f, 0.05f), 0.0001f);
        }

        [Test]
        public void Tilt_NegativeVelocity_ReturnsPositiveMaxTilt()
        {
            Assert.AreEqual(5f, MobAnimController.ComputeTiltDeg(-2f, 5f, 0.05f), 0.0001f);
        }

        // ---------- Lunge offset ----------

        [Test]
        public void LungeOffset_AtZero_ReturnsZero()
        {
            // sin(0) = 0.
            Assert.AreEqual(0f, MobAnimController.ComputeLungeOffset(0f, 0.3f, 0.3f), 0.0001f);
        }

        [Test]
        public void LungeOffset_AtMid_ReturnsPeakDistance()
        {
            // u=0.5 → sin(π/2) = 1 → distance.
            Assert.AreEqual(0.3f, MobAnimController.ComputeLungeOffset(0.15f, 0.3f, 0.3f), 0.0001f);
        }

        [Test]
        public void LungeOffset_AtDuration_ReturnsZero()
        {
            // u=1 → sin(π) = 0 → 0 (mob về vị trí cũ).
            Assert.AreEqual(0f, MobAnimController.ComputeLungeOffset(0.3f, 0.3f, 0.3f), 0.0001f);
        }

        [Test]
        public void LungeOffset_PastDuration_ClampsToZero()
        {
            // u clamp 0..1 → past lifetime → sin(π·1) = 0.
            Assert.AreEqual(0f, MobAnimController.ComputeLungeOffset(1f, 0.3f, 0.3f), 0.0001f);
        }

        [Test]
        public void LungeOffset_ZeroDuration_DefensiveReturnsZero()
        {
            Assert.AreEqual(0f, MobAnimController.ComputeLungeOffset(0.1f, 0f, 0.3f), 0.0001f);
        }

        // ---------- Squash factor ----------

        [Test]
        public void SquashFactor_AtZero_ReturnsOne()
        {
            // sin(0) = 0 → 1 + 0 = 1 (no squash at start).
            Assert.AreEqual(1f, MobAnimController.ComputeSquashFactor(0f, 0.3f, 1.2f), 0.0001f);
        }

        [Test]
        public void SquashFactor_AtMid_ReturnsPeakScale()
        {
            // u=0.5 → sin(π/2) = 1 → 1 + (1.2 - 1) * 1 = 1.2.
            Assert.AreEqual(1.2f, MobAnimController.ComputeSquashFactor(0.15f, 0.3f, 1.2f), 0.0001f);
        }

        [Test]
        public void SquashFactor_AtDuration_ReturnsOne()
        {
            // u=1 → sin(π) = 0 → 1.
            Assert.AreEqual(1f, MobAnimController.ComputeSquashFactor(0.3f, 0.3f, 1.2f), 0.0001f);
        }

        [Test]
        public void SquashFactor_PeakLessThanOne_StretchesDown()
        {
            // Caller có thể pass peak < 1 (anticipate stretch). u=0.5 → 1 + (0.8 - 1) = 0.8.
            Assert.AreEqual(0.8f, MobAnimController.ComputeSquashFactor(0.15f, 0.3f, 0.8f), 0.0001f);
        }

        [Test]
        public void SquashFactor_ZeroDuration_DefensiveReturnsOne()
        {
            Assert.AreEqual(1f, MobAnimController.ComputeSquashFactor(0.1f, 0f, 1.2f), 0.0001f);
        }
    }
}
