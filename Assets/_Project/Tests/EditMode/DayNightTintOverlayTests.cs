using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using WildernessCultivation.UI;

namespace WildernessCultivation.Tests.EditMode
{
    /// <summary>
    /// EditMode tests cho <see cref="DayNightTintOverlay.SampleTint"/> — verify gradient
    /// keypoint correctness và alpha clamp.
    /// </summary>
    public class DayNightTintOverlayTests
    {
        GameObject go;
        DayNightTintOverlay overlay;

        [SetUp]
        public void Setup()
        {
            go = new GameObject("TintOverlay", typeof(RectTransform), typeof(Image), typeof(DayNightTintOverlay));
            overlay = go.GetComponent<DayNightTintOverlay>();
        }

        [TearDown]
        public void Teardown()
        {
            if (go != null) Object.DestroyImmediate(go);
        }

        [Test]
        public void SampleTint_AtKeypoints_ReturnsExactKeypointColors()
        {
            // Tại t=0 (midnight) — clamp alpha với maxAlpha (mặc định 0.55), midnightTint.a = 0.55 → giữ nguyên.
            AssertColorClose(overlay.midnightTint, overlay.SampleTint(0f));
            AssertColorClose(overlay.dawnTint,     overlay.SampleTint(0.25f));
            AssertColorClose(overlay.noonTint,     overlay.SampleTint(0.5f));
            AssertColorClose(overlay.duskTint,     overlay.SampleTint(0.75f));
            // Wrap: t=1 = midnight.
            AssertColorClose(overlay.midnightTint, overlay.SampleTint(1f));
        }

        [Test]
        public void SampleTint_BetweenDawnAndNoon_LerpsLinearly()
        {
            // t=0.375 = midpoint dawn (0.25) → noon (0.5).
            var expected = Color.Lerp(overlay.dawnTint, overlay.noonTint, 0.5f);
            // Alpha clamp tới maxAlpha (0.55). Expected alpha = (0.30 + 0.0)/2 = 0.15 — không bị cap.
            expected.a = Mathf.Min(expected.a, overlay.maxAlpha);
            AssertColorClose(expected, overlay.SampleTint(0.375f));
        }

        [Test]
        public void SampleTint_AlphaCappedAtMaxAlpha()
        {
            overlay.maxAlpha = 0.10f;
            // Tại midnight, raw alpha = 0.55, sau clamp phải = 0.10.
            var c = overlay.SampleTint(0f);
            Assert.AreEqual(0.10f, c.a, 0.001f);
        }

        [Test]
        public void SampleTint_NegativeOrOverflowTime_WrapsCorrectly()
        {
            AssertColorClose(overlay.SampleTint(0f),    overlay.SampleTint(2f));
            AssertColorClose(overlay.SampleTint(0.25f), overlay.SampleTint(1.25f));
            AssertColorClose(overlay.SampleTint(0.5f),  overlay.SampleTint(-0.5f));
        }

        static void AssertColorClose(Color expected, Color actual, float eps = 0.0005f)
        {
            Assert.AreEqual(expected.r, actual.r, eps, $"r expected={expected.r} actual={actual.r}");
            Assert.AreEqual(expected.g, actual.g, eps, $"g expected={expected.g} actual={actual.g}");
            Assert.AreEqual(expected.b, actual.b, eps, $"b expected={expected.b} actual={actual.b}");
            Assert.AreEqual(expected.a, actual.a, eps, $"a expected={expected.a} actual={actual.a}");
        }
    }
}
