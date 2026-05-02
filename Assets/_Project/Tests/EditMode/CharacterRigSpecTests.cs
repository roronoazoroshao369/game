using NUnit.Framework;
using UnityEngine;
using WildernessCultivation.Core;

namespace WildernessCultivation.Tests.EditMode
{
    /// <summary>
    /// Tests cho <see cref="CharacterRigSpec"/> — pure data SO + helper math.
    /// Verify defaults match legacy hardcoded constants trong BootstrapWizard
    /// (backwards compat) + side-view occlusion helpers correct.
    /// </summary>
    public class CharacterRigSpecTests
    {
        [Test]
        public void CreateDefault_FieldsMatchLegacyConstants()
        {
            // Legacy hardcoded values trong BuildPuppetHierarchy trước PR này:
            //   shoulderY=0.55, shoulderX=0.30, hipY=-0.55, hipX=0.13,
            //   elbowOverlap=0.06, kneeOverlap=0.10
            // Default RigSpec MUST match → khi character không có RigSpec asset,
            // fallback giữ behavior cũ y nguyên (zero visual change).
            var spec = CharacterRigSpec.CreateDefault();
            Assert.AreEqual(0.55f, spec.shoulderY, 0.0001f, "shoulderY default");
            Assert.AreEqual(0.30f, spec.shoulderX, 0.0001f, "shoulderX default");
            Assert.AreEqual(-0.55f, spec.hipY, 0.0001f, "hipY default");
            Assert.AreEqual(0.13f, spec.hipX, 0.0001f, "hipX default");
            Assert.AreEqual(0.06f, spec.elbowOverlap, 0.0001f, "elbowOverlap default");
            Assert.AreEqual(0.10f, spec.kneeOverlap, 0.0001f, "kneeOverlap default");
            Object.DestroyImmediate(spec);
        }

        [Test]
        public void CreateDefault_OcclusionFieldsHaveSensibleDefaults()
        {
            var spec = CharacterRigSpec.CreateDefault();
            Assert.AreEqual(0.92f, spec.farLimbScale, 0.0001f, "farLimbScale default");
            Assert.AreEqual(-2, spec.farLimbSortingOffset, "farLimbSortingOffset default");
            Assert.IsTrue(spec.enableSideViewOcclusion, "enableSideViewOcclusion default true");
            Object.DestroyImmediate(spec);
        }

        // ---------- ComputeFarLimbScaleX ----------

        [Test]
        public void FarLimbScaleX_BaseScalePositive_ReturnsScaled()
        {
            // baseScale=1, farLimbScale=0.92 → 0.92.
            Assert.AreEqual(0.92f,
                CharacterRigSpec.ComputeFarLimbScaleX(0.92f, 1f), 0.0001f);
        }

        [Test]
        public void FarLimbScaleX_BaseScaleNegative_ReturnsAbsScaled()
        {
            // baseScale=-1 (West-flipped spriteRoot) → abs(-1) * 0.92 = 0.92.
            // Caller multiply với flip sign sau, nên helper return absolute.
            Assert.AreEqual(0.92f,
                CharacterRigSpec.ComputeFarLimbScaleX(0.92f, -1f), 0.0001f);
        }

        [Test]
        public void FarLimbScaleX_DisabledOcclusion_ReturnsBaseUnchanged()
        {
            // farLimbScale=1.0 → no shrink, equivalent to disabled occlusion.
            Assert.AreEqual(1f,
                CharacterRigSpec.ComputeFarLimbScaleX(1f, 1f), 0.0001f);
        }

        [Test]
        public void FarLimbScaleX_OutOfRange_Clamped()
        {
            // Defensive: scale clamped [0.1, 2] → invalid input không tạo zero / negative.
            Assert.AreEqual(0.1f,
                CharacterRigSpec.ComputeFarLimbScaleX(0f, 1f), 0.0001f);
            Assert.AreEqual(0.1f,
                CharacterRigSpec.ComputeFarLimbScaleX(-5f, 1f), 0.0001f);
            Assert.AreEqual(2f,
                CharacterRigSpec.ComputeFarLimbScaleX(10f, 1f), 0.0001f);
        }

        // ---------- ComputeFarLimbSortingOrder ----------

        [Test]
        public void FarLimbSortingOrder_DefaultOffset_RendersBehindNear()
        {
            // nearOrder=8 (typical arm sortingOrderBase + 3), offset=-2 → 6 < 8.
            // Far limb renders sau near limb → depth illusion.
            int near = 8;
            int far = CharacterRigSpec.ComputeFarLimbSortingOrder(near, -2);
            Assert.AreEqual(6, far);
            Assert.Less(far, near, "Far limb sorting < near limb");
        }

        [Test]
        public void FarLimbSortingOrder_ZeroOffset_NoChange()
        {
            // Offset=0 disable z-order swap → far == near.
            Assert.AreEqual(8, CharacterRigSpec.ComputeFarLimbSortingOrder(8, 0));
        }

        [Test]
        public void FarLimbSortingOrder_PositiveOffset_RendersInFront()
        {
            // Edge case: positive offset puts far limb IN FRONT (unusual nhưng support).
            // Nếu user muốn override depth flip cho stylized look.
            Assert.AreEqual(10, CharacterRigSpec.ComputeFarLimbSortingOrder(8, 2));
        }
    }
}
