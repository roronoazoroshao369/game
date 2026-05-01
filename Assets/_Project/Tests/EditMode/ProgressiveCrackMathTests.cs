using NUnit.Framework;
using UnityEngine;
using WildernessCultivation.Vfx;

namespace WildernessCultivation.Tests.EditMode
{
    /// <summary>
    /// Pure math tests cho <see cref="ProgressiveCrackOverlay.ComputeCrackAlpha"/>.
    /// Curve: hpRatio>=startThreshold → 0; hpRatio=0 → maxAlpha; lerp giữa 2 mốc.
    /// </summary>
    public class ProgressiveCrackMathTests
    {
        [Test]
        public void ComputeCrackAlpha_FullHp_ReturnsZero()
        {
            // HP đầy (ratio=1) → no crack visible.
            Assert.AreEqual(0f, ProgressiveCrackOverlay.ComputeCrackAlpha(1f, 0.85f, 0.6f), 0.0001f);
        }

        [Test]
        public void ComputeCrackAlpha_AtThreshold_ReturnsZero()
        {
            // Tại đúng threshold → vẫn 0 (mới bắt đầu xuất hiện).
            Assert.AreEqual(0f, ProgressiveCrackOverlay.ComputeCrackAlpha(0.85f, 0.85f, 0.6f), 0.0001f);
        }

        [Test]
        public void ComputeCrackAlpha_ZeroHp_ReturnsMaxAlpha()
        {
            // HP cạn → alpha = max.
            Assert.AreEqual(0.6f, ProgressiveCrackOverlay.ComputeCrackAlpha(0f, 0.85f, 0.6f), 0.0001f);
        }

        [Test]
        public void ComputeCrackAlpha_BelowThreshold_LerpsLinearly()
        {
            // threshold=0.8, hpRatio=0.4 → t = 1 - 0.4/0.8 = 0.5 → alpha = 0.5 * 0.6 = 0.3.
            float a = ProgressiveCrackOverlay.ComputeCrackAlpha(0.4f, 0.8f, 0.6f);
            Assert.AreEqual(0.3f, a, 0.001f);
        }

        [Test]
        public void ComputeCrackAlpha_NegativeHp_ClampsToMaxAlpha()
        {
            // Defensive — negative ratio không được explode → clamp về 0 → max alpha.
            Assert.AreEqual(0.6f, ProgressiveCrackOverlay.ComputeCrackAlpha(-0.5f, 0.85f, 0.6f), 0.0001f);
        }

        [Test]
        public void ComputeCrackAlpha_OverOneHp_ClampsToZero()
        {
            // Defensive — ratio>1 (heal beyond max) → clamp về 1 → no crack.
            Assert.AreEqual(0f, ProgressiveCrackOverlay.ComputeCrackAlpha(1.5f, 0.85f, 0.6f), 0.0001f);
        }

        [Test]
        public void ComputeCrackAlpha_LowerThreshold_ShowsCrackLater()
        {
            // hpRatio=0.7: với threshold 0.85 → có crack; với threshold 0.5 → 0 (chưa hiện).
            float at85 = ProgressiveCrackOverlay.ComputeCrackAlpha(0.7f, 0.85f, 0.6f);
            float at50 = ProgressiveCrackOverlay.ComputeCrackAlpha(0.7f, 0.5f, 0.6f);

            Assert.Greater(at85, 0f, "Threshold 0.85 → ratio 0.7 < threshold → có crack.");
            Assert.AreEqual(0f, at50, 0.0001f, "Threshold 0.5 → ratio 0.7 > threshold → chưa crack.");
        }
    }
}
