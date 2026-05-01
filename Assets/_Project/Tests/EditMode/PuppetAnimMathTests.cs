using NUnit.Framework;
using WildernessCultivation.Core;
using WildernessCultivation.Vfx;

namespace WildernessCultivation.Tests.EditMode
{
    /// <summary>
    /// Pure math + role parsing tests cho puppet system.
    /// </summary>
    public class PuppetAnimMathTests
    {
        // ---------- ComputeLungeArmAngle ----------

        [Test]
        public void LungeArmAngle_AtStart_Zero()
        {
            Assert.AreEqual(0f, PuppetAnimController.ComputeLungeArmAngle(0f, 60f), 0.0001f);
        }

        [Test]
        public void LungeArmAngle_AtMid_Peak()
        {
            // u=0.5 → sin(π/2)=1 → maxDeg.
            Assert.AreEqual(60f, PuppetAnimController.ComputeLungeArmAngle(0.5f, 60f), 0.0001f);
        }

        [Test]
        public void LungeArmAngle_AtEnd_ReturnsZero()
        {
            Assert.AreEqual(0f, PuppetAnimController.ComputeLungeArmAngle(1f, 60f), 0.0001f);
        }

        [Test]
        public void LungeArmAngle_OutOfRange_Clamped()
        {
            // u > 1 clamped to 1 → 0; u < 0 clamped to 0 → 0.
            Assert.AreEqual(0f, PuppetAnimController.ComputeLungeArmAngle(1.5f, 60f), 0.0001f);
            Assert.AreEqual(0f, PuppetAnimController.ComputeLungeArmAngle(-0.5f, 60f), 0.0001f);
        }

        // ---------- ComputeArmSwingDeg ----------

        [Test]
        public void ArmSwing_LeftArm_DirectSign()
        {
            // Left arm = walkSin * maxDeg.
            Assert.AreEqual(15f, PuppetAnimController.ComputeArmSwingDeg(0.5f, 30f, isLeft: true), 0.0001f);
            Assert.AreEqual(-15f, PuppetAnimController.ComputeArmSwingDeg(-0.5f, 30f, isLeft: true), 0.0001f);
        }

        [Test]
        public void ArmSwing_RightArm_OppositeSign()
        {
            // Right arm = -walkSin * maxDeg → opposite phase.
            Assert.AreEqual(-15f, PuppetAnimController.ComputeArmSwingDeg(0.5f, 30f, isLeft: false), 0.0001f);
            Assert.AreEqual(15f, PuppetAnimController.ComputeArmSwingDeg(-0.5f, 30f, isLeft: false), 0.0001f);
        }

        [Test]
        public void ArmSwing_LeftAndRight_AlwaysOppositeAtAnyTime()
        {
            // Invariant: left + right == 0 ở mọi walk phase (perfect symmetry).
            float[] phases = { -1f, -0.5f, 0f, 0.3f, 0.7f, 1f };
            foreach (var p in phases)
            {
                float l = PuppetAnimController.ComputeArmSwingDeg(p, 30f, isLeft: true);
                float r = PuppetAnimController.ComputeArmSwingDeg(p, 30f, isLeft: false);
                Assert.AreEqual(0f, l + r, 0.0001f, $"phase={p}");
            }
        }

        // ---------- ComputeSpeedRatio ----------

        [Test]
        public void SpeedRatio_AtReference_One()
        {
            Assert.AreEqual(1f, PuppetAnimController.ComputeSpeedRatio(2.5f, 2.5f), 0.0001f);
        }

        [Test]
        public void SpeedRatio_BelowMin_Clamped()
        {
            // Speed=0 → ratio 0/2.5 = 0, clamped to minRatio=0.3.
            Assert.AreEqual(0.3f, PuppetAnimController.ComputeSpeedRatio(0f, 2.5f), 0.0001f);
        }

        [Test]
        public void SpeedRatio_AboveMax_Clamped()
        {
            // Speed=10, ref=2.5 → 4, clamped to maxRatio=2.
            Assert.AreEqual(2f, PuppetAnimController.ComputeSpeedRatio(10f, 2.5f), 0.0001f);
        }

        [Test]
        public void SpeedRatio_ZeroReference_DefensiveOne()
        {
            // Defensive: ref=0 → return 1 (avoid div by zero).
            Assert.AreEqual(1f, PuppetAnimController.ComputeSpeedRatio(5f, 0f), 0.0001f);
        }

        // ---------- CharacterArtSpec.TryParseRole ----------

        [Test]
        public void TryParseRole_StandardFilenames_AllRecognized()
        {
            Assert.AreEqual(CharacterArtSpec.PuppetRole.Head, CharacterArtSpec.TryParseRole("head"));
            Assert.AreEqual(CharacterArtSpec.PuppetRole.Torso, CharacterArtSpec.TryParseRole("torso"));
            Assert.AreEqual(CharacterArtSpec.PuppetRole.ArmLeft, CharacterArtSpec.TryParseRole("arm_left"));
            Assert.AreEqual(CharacterArtSpec.PuppetRole.ArmRight, CharacterArtSpec.TryParseRole("arm_right"));
            Assert.AreEqual(CharacterArtSpec.PuppetRole.LegLeft, CharacterArtSpec.TryParseRole("leg_left"));
            Assert.AreEqual(CharacterArtSpec.PuppetRole.LegRight, CharacterArtSpec.TryParseRole("leg_right"));
            Assert.AreEqual(CharacterArtSpec.PuppetRole.Tail, CharacterArtSpec.TryParseRole("tail"));
        }

        [Test]
        public void TryParseRole_CaseInsensitive()
        {
            Assert.AreEqual(CharacterArtSpec.PuppetRole.Head, CharacterArtSpec.TryParseRole("HEAD"));
            Assert.AreEqual(CharacterArtSpec.PuppetRole.ArmLeft, CharacterArtSpec.TryParseRole("Arm_Left"));
        }

        [Test]
        public void TryParseRole_Unknown_ReturnsUnknownEnum()
        {
            Assert.AreEqual(CharacterArtSpec.PuppetRole.Unknown, CharacterArtSpec.TryParseRole("body"));
            Assert.AreEqual(CharacterArtSpec.PuppetRole.Unknown, CharacterArtSpec.TryParseRole("hat"));
            Assert.AreEqual(CharacterArtSpec.PuppetRole.Unknown, CharacterArtSpec.TryParseRole(""));
            Assert.AreEqual(CharacterArtSpec.PuppetRole.Unknown, CharacterArtSpec.TryParseRole(null));
        }

        [Test]
        public void IsRequiredForPuppet_HeadAndTorso_AreRequired()
        {
            Assert.IsTrue(CharacterArtSpec.IsRequiredForPuppet(CharacterArtSpec.PuppetRole.Head));
            Assert.IsTrue(CharacterArtSpec.IsRequiredForPuppet(CharacterArtSpec.PuppetRole.Torso));
        }

        [Test]
        public void IsRequiredForPuppet_LimbsAndTail_AreOptional()
        {
            Assert.IsFalse(CharacterArtSpec.IsRequiredForPuppet(CharacterArtSpec.PuppetRole.ArmLeft));
            Assert.IsFalse(CharacterArtSpec.IsRequiredForPuppet(CharacterArtSpec.PuppetRole.LegLeft));
            Assert.IsFalse(CharacterArtSpec.IsRequiredForPuppet(CharacterArtSpec.PuppetRole.Tail));
        }
    }
}
