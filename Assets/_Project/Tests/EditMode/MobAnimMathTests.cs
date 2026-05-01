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

        // ---------- Wave shaping ----------

        [Test]
        public void ShapeWave_Identity_WhenExponentIsOne()
        {
            Assert.AreEqual(0.5f, MobAnimController.ShapeWave(0.5f, 1f), 0.0001f);
            Assert.AreEqual(-0.7f, MobAnimController.ShapeWave(-0.7f, 1f), 0.0001f);
        }

        [Test]
        public void ShapeWave_PreservesSign_AndZero()
        {
            Assert.AreEqual(0f, MobAnimController.ShapeWave(0f, 0.5f), 0.0001f);
            // Snappier (exponent < 1): |0.5|^0.5 = √0.5 ≈ 0.7071 → biên độ tăng (gần peak hơn).
            Assert.That(MobAnimController.ShapeWave(0.5f, 0.5f), Is.GreaterThan(0.5f));
            // Sign giữ nguyên.
            Assert.That(MobAnimController.ShapeWave(-0.5f, 0.5f), Is.LessThan(0f));
        }

        [Test]
        public void ShapeWave_PowerGreaterThanOne_Softens()
        {
            // Softer (exponent > 1): |0.5|^2 = 0.25 → biên độ thấp hơn (gần zero hơn).
            Assert.That(MobAnimController.ShapeWave(0.5f, 2f), Is.LessThan(0.5f));
        }

        [Test]
        public void WalkBobScale_WithShape_DefaultsToPureSinWhenShapeOne()
        {
            // 6-arg overload với shape=1 phải = 5-arg overload (back-compat).
            float a = MobAnimController.ComputeWalkBobScale(0.05f, 2f, 0.05f, 5f, 2f);
            float b = MobAnimController.ComputeWalkBobScale(0.05f, 2f, 0.05f, 5f, 2f, 1f);
            Assert.AreEqual(a, b, 0.0001f);
        }

        // ---------- Lunge with anticipation ----------

        [Test]
        public void LungeAnticipation_AtStart_ReturnsZero()
        {
            float v = MobAnimController.ComputeLungeOffsetWithAnticipation(0f, 0.3f, 0.3f, 0.15f, 0.25f);
            Assert.AreEqual(0f, v, 0.0001f);
        }

        [Test]
        public void LungeAnticipation_InAnticipationPhase_IsNegative()
        {
            // u = 0.075/0.3 = 0.25, af = 0.15 → trong anticipation.
            // Bell pull-back peak ở giữa anticipation phase (u/af = 0.5).
            float vMid = MobAnimController.ComputeLungeOffsetWithAnticipation(
                0.0225f, 0.3f, 0.3f, 0.15f, 0.25f);
            Assert.That(vMid, Is.LessThan(0f));
            // Magnitude khoảng -0.3 * 0.25 = -0.075.
            Assert.AreEqual(-0.075f, vMid, 0.001f);
        }

        [Test]
        public void LungeAnticipation_AfterAnticipation_IsPositive()
        {
            // u = 0.5 (giữa duration), af=0.15 → forward phase.
            // fu = (0.5 - 0.15) / (1 - 0.15) ≈ 0.4118; sin(π*0.4118) ≈ 0.967.
            float v = MobAnimController.ComputeLungeOffsetWithAnticipation(
                0.15f, 0.3f, 0.3f, 0.15f, 0.25f);
            Assert.That(v, Is.GreaterThan(0f));
        }

        [Test]
        public void LungeAnticipation_AtEnd_ReturnsZero()
        {
            float v = MobAnimController.ComputeLungeOffsetWithAnticipation(0.3f, 0.3f, 0.3f, 0.15f, 0.25f);
            Assert.AreEqual(0f, v, 0.001f);
        }

        [Test]
        public void LungeAnticipation_ZeroFraction_ReducesToBellCurve()
        {
            // af=0 → identical to ComputeLungeOffset.
            float a = MobAnimController.ComputeLungeOffset(0.1f, 0.3f, 0.3f);
            float b = MobAnimController.ComputeLungeOffsetWithAnticipation(0.1f, 0.3f, 0.3f, 0f, 0.25f);
            Assert.AreEqual(a, b, 0.0001f);
        }

        [Test]
        public void LungeAnticipation_ZeroDuration_DefensiveReturnsZero()
        {
            float v = MobAnimController.ComputeLungeOffsetWithAnticipation(0.1f, 0f, 0.3f, 0.15f, 0.25f);
            Assert.AreEqual(0f, v, 0.0001f);
        }

        // ---------- Exponential damping ----------

        [Test]
        public void ExponentialDamping_ZeroDt_ReturnsTarget()
        {
            // dt=0 → defensive snap to target (avoid stuck at intermediate value).
            Assert.AreEqual(10f, MobAnimController.ApplyExponentialDamping(0f, 10f, 12f, 0f), 0.0001f);
        }

        [Test]
        public void ExponentialDamping_LargeDt_ApproachesTarget()
        {
            // dt = 1, rate = 12 → alpha ≈ 1 - e^-12 ≈ 0.999994 → very close.
            float v = MobAnimController.ApplyExponentialDamping(0f, 10f, 12f, 1f);
            Assert.That(v, Is.GreaterThan(9.9f));
        }

        [Test]
        public void ExponentialDamping_SmallDt_LerpsPartial()
        {
            // dt = 0.0167 (1 frame at 60fps), rate = 12 → alpha ≈ 1 - e^-0.2 ≈ 0.181.
            float v = MobAnimController.ApplyExponentialDamping(0f, 10f, 12f, 0.0167f);
            Assert.That(v, Is.GreaterThan(1.5f));
            Assert.That(v, Is.LessThan(2.2f));
        }

        [Test]
        public void ExponentialDamping_AlreadyAtTarget_NoChange()
        {
            float v = MobAnimController.ApplyExponentialDamping(5f, 5f, 12f, 0.016f);
            Assert.AreEqual(5f, v, 0.0001f);
        }
    }
}
