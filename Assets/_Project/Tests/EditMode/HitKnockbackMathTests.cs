using NUnit.Framework;
using UnityEngine;
using WildernessCultivation.Vfx;

namespace WildernessCultivation.Tests.EditMode
{
    /// <summary>
    /// Pure math tests cho <see cref="HitKnockback.ComputeImpulse"/>.
    /// </summary>
    public class HitKnockbackMathTests
    {
        static readonly Vector2 Fallback = Vector2.right;

        [Test]
        public void ComputeImpulse_TargetRightOfSource_PointsRight()
        {
            // Source (0,0), target (1,0) → dir = (1,0), impulse 2 → (2,0).
            var imp = HitKnockback.ComputeImpulse(Vector2.zero, Vector2.right, 2f, Fallback);
            Assert.AreEqual(2f, imp.x, 0.0001f);
            Assert.AreEqual(0f, imp.y, 0.0001f);
        }

        [Test]
        public void ComputeImpulse_TargetAboveSource_PointsUp()
        {
            var imp = HitKnockback.ComputeImpulse(Vector2.zero, new Vector2(0, 3), 1.5f, Fallback);
            Assert.AreEqual(0f, imp.x, 0.0001f);
            Assert.AreEqual(1.5f, imp.y, 0.0001f);
        }

        [Test]
        public void ComputeImpulse_OverlappingPositions_UsesFallbackDirection()
        {
            // Source = target → diff = 0 → dùng fallback (Vector2.right normalized) * impulse.
            var imp = HitKnockback.ComputeImpulse(Vector2.one, Vector2.one, 4f, Fallback);
            Assert.AreEqual(4f, imp.x, 0.0001f, "Fallback Vector2.right normalized → x = impulse.");
            Assert.AreEqual(0f, imp.y, 0.0001f);
        }

        [Test]
        public void ComputeImpulse_OverlappingWithZeroFallback_UsesRightAsSafeDefault()
        {
            // Defensive: source≈target AND fallback = zero → ComputeImpulse phải KHÔNG NaN; default sang Vector2.right.
            var imp = HitKnockback.ComputeImpulse(Vector2.zero, Vector2.zero, 3f, Vector2.zero);
            Assert.AreEqual(3f, imp.x, 0.0001f);
            Assert.AreEqual(0f, imp.y, 0.0001f);
        }

        [Test]
        public void ComputeImpulse_NormalizesDiagonalDirection()
        {
            // Source (0,0), target (3,4) → diff (3,4), magnitude 5 → dir (0.6, 0.8). impulse 10 → (6, 8).
            var imp = HitKnockback.ComputeImpulse(Vector2.zero, new Vector2(3, 4), 10f, Fallback);
            Assert.AreEqual(6f, imp.x, 0.001f);
            Assert.AreEqual(8f, imp.y, 0.001f);
            Assert.AreEqual(10f, imp.magnitude, 0.001f, "Magnitude phải = impulse (normalized dir).");
        }

        [Test]
        public void ComputeImpulse_LargerImpulseScalesLinearly()
        {
            var src = Vector2.zero;
            var tgt = new Vector2(1, 0);
            var a = HitKnockback.ComputeImpulse(src, tgt, 1f, Fallback);
            var b = HitKnockback.ComputeImpulse(src, tgt, 5f, Fallback);
            Assert.AreEqual(5f * a.x, b.x, 0.001f);
        }
    }
}
