using NUnit.Framework;
using WildernessCultivation.Vfx;

namespace WildernessCultivation.Tests.EditMode
{
    /// <summary>
    /// Pure math tests cho <see cref="BoneAnimController.ComputeFlipScaleX"/>.
    /// Verify flip preserves magnitude + ignores sub-threshold velocity.
    /// </summary>
    public class BoneAnimFlipTests
    {
        [Test]
        public void Flip_BelowThreshold_KeepsCurrentScale()
        {
            // |vx| < threshold → no flip (avoid jitter standing still).
            Assert.AreEqual(1.5f, BoneAnimController.ComputeFlipScaleX(1.5f, 0.02f, 0.05f), 0.0001f);
            Assert.AreEqual(-1.5f, BoneAnimController.ComputeFlipScaleX(-1.5f, 0.01f, 0.05f), 0.0001f);
        }

        [Test]
        public void Flip_PositiveVelocity_FacesRight()
        {
            // vx > threshold → +|currentScaleX|. Magnitude preserved.
            Assert.AreEqual(2f, BoneAnimController.ComputeFlipScaleX(2f, 1f, 0.05f), 0.0001f);
            Assert.AreEqual(2f, BoneAnimController.ComputeFlipScaleX(-2f, 1f, 0.05f), 0.0001f);
        }

        [Test]
        public void Flip_NegativeVelocity_FacesLeft()
        {
            // vx < -threshold → -|currentScaleX|. Magnitude preserved.
            Assert.AreEqual(-2f, BoneAnimController.ComputeFlipScaleX(2f, -1f, 0.05f), 0.0001f);
            Assert.AreEqual(-2f, BoneAnimController.ComputeFlipScaleX(-2f, -1f, 0.05f), 0.0001f);
        }

        [Test]
        public void Flip_PreservesMagnitude_NotSign()
        {
            // Caller's scale magnitude is preserved across flip (e.g., 0.8 stays 0.8 absolute).
            Assert.AreEqual(0.8f, BoneAnimController.ComputeFlipScaleX(0.8f, 1f, 0.05f), 0.0001f);
            Assert.AreEqual(-0.8f, BoneAnimController.ComputeFlipScaleX(0.8f, -1f, 0.05f), 0.0001f);
        }
    }
}
