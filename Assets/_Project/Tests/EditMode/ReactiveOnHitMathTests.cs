using NUnit.Framework;
using UnityEngine;
using WildernessCultivation.Vfx;

namespace WildernessCultivation.Tests.EditMode
{
    /// <summary>
    /// Pure math tests cho <see cref="ReactiveOnHit.ComputeShakeAngleDeg"/>.
    /// Formula: <c>amp · sin(2π·freq·t) · exp(-decay·t)</c>.
    /// </summary>
    public class ReactiveOnHitMathTests
    {
        [Test]
        public void ComputeShakeAngleDeg_AtZero_ReturnsZero()
        {
            // sin(0) = 0 → angle = 0 bất kể amp / decay.
            Assert.AreEqual(0f, ReactiveOnHit.ComputeShakeAngleDeg(0f, 8f, 6f, 8f), 0.0001f);
            Assert.AreEqual(0f, ReactiveOnHit.ComputeShakeAngleDeg(0f, 1f, 1f, 1f), 0.0001f);
        }

        [Test]
        public void ComputeShakeAngleDeg_NegativeTime_ClampedToZero()
        {
            // Defensive — caller không nên truyền t<0 nhưng nếu có thì không divide-by-zero / NaN.
            float a = ReactiveOnHit.ComputeShakeAngleDeg(-0.5f, 8f, 6f, 8f);
            Assert.AreEqual(0f, a, 0.0001f);
        }

        [Test]
        public void ComputeShakeAngleDeg_AtQuarterPeriod_PeaksNearAmplitude()
        {
            // Tại t = 1/(4·freq) → sin(2π · freq · t) = sin(π/2) = 1 → đỉnh sin.
            // Decay envelope tại đó = exp(-decay / (4·freq)). Verify rằng output dương + có magnitude.
            float freq = 6f;
            float amp = 10f;
            float decay = 8f;
            float t = 1f / (4f * freq); // ~0.0417s

            float a = ReactiveOnHit.ComputeShakeAngleDeg(t, amp, freq, decay);
            float envelope = amp * Mathf.Exp(-decay * t);

            Assert.Greater(a, 0f, "Quarter-period sin(π/2)=1 → amp positive.");
            Assert.AreEqual(envelope, a, 0.001f, "At sin peak, output = amp · envelope.");
        }

        [Test]
        public void ComputeShakeAngleDeg_AtLargeTime_DecaysToNearZero()
        {
            // Tại t = 1.0s với decay=8 → exp(-8) ≈ 0.000335 → magnitude << amp.
            float a = ReactiveOnHit.ComputeShakeAngleDeg(1.0f, 10f, 6f, 8f);
            Assert.Less(Mathf.Abs(a), 0.01f, "Decay phải tắt magnitude về < 0.01° khi t=1s, decay=8.");
        }

        [Test]
        public void ComputeShakeAngleDeg_ScalesLinearlyWithAmplitude()
        {
            float t = 0.05f;
            float a1 = ReactiveOnHit.ComputeShakeAngleDeg(t, 5f, 6f, 8f);
            float a2 = ReactiveOnHit.ComputeShakeAngleDeg(t, 10f, 6f, 8f);

            Assert.AreEqual(2f * a1, a2, 0.001f, "Doubling amp doubles output (linear scaling).");
        }

        [Test]
        public void ComputeShakeAngleDeg_LargerDecayDampsFaster()
        {
            // Cùng t, freq, amp — decay lớn hơn → magnitude nhỏ hơn (envelope tắt nhanh).
            float t = 0.1f;
            float slow = ReactiveOnHit.ComputeShakeAngleDeg(t, 10f, 6f, 4f);
            float fast = ReactiveOnHit.ComputeShakeAngleDeg(t, 10f, 6f, 12f);

            Assert.Greater(Mathf.Abs(slow), Mathf.Abs(fast),
                "Decay lớn hơn → magnitude nhỏ hơn ở cùng t.");
        }
    }
}
