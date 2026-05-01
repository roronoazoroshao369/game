using NUnit.Framework;
using WildernessCultivation.Vfx;

namespace WildernessCultivation.Tests.EditMode
{
    /// <summary>
    /// Pure math tests cho <see cref="DropShadow.ComputeShadowBobScale"/>.
    /// Foot-impact feedback: bob &lt; 1 (mob squash) → shadow expand; bob &gt; 1 (extend) → shadow shrink.
    /// </summary>
    public class DropShadowMathTests
    {
        [Test]
        public void ShadowBob_NeutralBob_NoScale()
        {
            // bobYFactor = 1 → shadow scale = 1 regardless of influence.
            Assert.AreEqual(1f, DropShadow.ComputeShadowBobScale(1f, 0.3f), 0.0001f);
            Assert.AreEqual(1f, DropShadow.ComputeShadowBobScale(1f, 1f), 0.0001f);
        }

        [Test]
        public void ShadowBob_Squash_Expands()
        {
            // bobY = 0.95 (5% squash), influence 0.3 → shadow = 1 + 0.05*0.3 = 1.015.
            Assert.AreEqual(1.015f, DropShadow.ComputeShadowBobScale(0.95f, 0.3f), 0.0001f);
        }

        [Test]
        public void ShadowBob_Extend_Shrinks()
        {
            // bobY = 1.05 (5% extend), influence 0.3 → shadow = 1 - 0.05*0.3 = 0.985.
            Assert.AreEqual(0.985f, DropShadow.ComputeShadowBobScale(1.05f, 0.3f), 0.0001f);
        }

        [Test]
        public void ShadowBob_ZeroInfluence_AlwaysOne()
        {
            // influence = 0 → shadow ignore bob.
            Assert.AreEqual(1f, DropShadow.ComputeShadowBobScale(0.8f, 0f), 0.0001f);
            Assert.AreEqual(1f, DropShadow.ComputeShadowBobScale(1.2f, 0f), 0.0001f);
        }

        [Test]
        public void ShadowBob_InfluenceClampedAtOne()
        {
            // Caller may pass > 1 (defensive) → clamped, output bounded.
            float v = DropShadow.ComputeShadowBobScale(0.5f, 2f);
            Assert.AreEqual(1.5f, v, 0.0001f); // 1 + 0.5 * 1 = 1.5 (influence clamped to 1).
        }
    }
}
