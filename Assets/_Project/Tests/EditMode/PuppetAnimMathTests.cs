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

        // ---------- ComputeFlapAngle (Phase 3 — wing flap) ----------

        [Test]
        public void FlapAngle_AtZero_Zero()
        {
            // sin(0) = 0 → wing ở neutral pose tại t=0.
            Assert.AreEqual(0f, PuppetAnimController.ComputeFlapAngle(0f, 6f, 50f), 0.0001f);
        }

        [Test]
        public void FlapAngle_AtQuarterPeriod_Peak()
        {
            // sin(π/2) = 1 → max amplitude. Quarter period of 6Hz = 1/(4·6) = 0.04167s.
            float quarterPeriod = 1f / (4f * 6f);
            Assert.AreEqual(50f, PuppetAnimController.ComputeFlapAngle(quarterPeriod, 6f, 50f), 0.001f);
        }

        [Test]
        public void FlapAngle_AtHalfPeriod_ZeroAgain()
        {
            // sin(π) = 0 → wing crosses neutral going down.
            float halfPeriod = 1f / (2f * 6f);
            Assert.AreEqual(0f, PuppetAnimController.ComputeFlapAngle(halfPeriod, 6f, 50f), 0.001f);
        }

        [Test]
        public void FlapAngle_AtThreeQuarterPeriod_NegPeak()
        {
            // sin(3π/2) = -1 → wing at full down stroke (flapped down peak).
            float threeQuarterPeriod = 3f / (4f * 6f);
            Assert.AreEqual(-50f, PuppetAnimController.ComputeFlapAngle(threeQuarterPeriod, 6f, 50f), 0.001f);
        }

        [Test]
        public void FlapAngle_RangeBoundedByAmplitude()
        {
            // Sample many times across multiple periods → all must stay in [-maxDeg, +maxDeg].
            float maxDeg = 50f;
            float freq = 6f;
            for (int i = 0; i < 200; i++)
            {
                float t = i * 0.0125f; // 2.5 seconds total = 15 full periods
                float a = PuppetAnimController.ComputeFlapAngle(t, freq, maxDeg);
                Assert.LessOrEqual(Mathf.Abs(a), maxDeg + 0.0001f, $"t={t}");
            }
        }

        [Test]
        public void FlapAngle_ZeroFrequency_DefensiveZero()
        {
            // Defensive: freq=0 → wing standstill (avoid sin(0·t)=0 anyway, but explicit guard).
            Assert.AreEqual(0f, PuppetAnimController.ComputeFlapAngle(1.5f, 0f, 50f), 0.0001f);
        }

        [Test]
        public void FlapAngle_NegativeFrequency_DefensiveZero()
        {
            // Defensive: invalid input → wing standstill thay vì reverse-spinning.
            Assert.AreEqual(0f, PuppetAnimController.ComputeFlapAngle(1.5f, -2f, 50f), 0.0001f);
        }

        [Test]
        public void FlapAngle_PeriodicityHolds()
        {
            // Invariant: f(t) == f(t + 1/freq) cho mọi t (1 full period repeats).
            float freq = 6f, maxDeg = 50f;
            float[] testTimes = { 0.013f, 0.087f, 0.21f, 0.55f, 1.27f };
            foreach (var t in testTimes)
            {
                float a0 = PuppetAnimController.ComputeFlapAngle(t, freq, maxDeg);
                float a1 = PuppetAnimController.ComputeFlapAngle(t + 1f / freq, freq, maxDeg);
                Assert.AreEqual(a0, a1, 0.001f, $"t={t} period mismatch");
            }
        }

        // ---------- ComputeSerpentineAngle (Phase 4 — Snake S-curve) ----------

        [Test]
        public void SerpentineAngle_AtZero_HeadsegLeads()
        {
            // t=0, segIndex=0, phase = 0 - 0*spread = 0 → sin(0)=0.
            // Smallest amplitude (1/N) but result is 0 anyway since sin(0)=0.
            Assert.AreEqual(0f,
                PuppetAnimController.ComputeSerpentineAngle(0f, 0, 4, 2.5f, 35f, 0.6f),
                0.0001f);
        }

        [Test]
        public void SerpentineAngle_AmplitudeScalesLinearWithIndex()
        {
            // Sample tại quarter-period of seg0 (sin = 1) — segIndex=0 sees full sin=1.
            // Other segments at this same time have phase delay → not at peak. So instead
            // verify at sin-aligned time per-segment: peak amplitude per-segment = maxDeg × (idx+1)/N.
            // Equivalent: at the segment's own sin-peak, amplitude = maxDeg × (idx+1)/N.
            float maxDeg = 100f, freq = 1f, spread = 0.6f;
            int N = 4;
            for (int idx = 0; idx < N; idx++)
            {
                // Time tại đó phase = π/2: t * 2π = π/2 + idx * spread → t = (π/2 + idx*spread) / (2π)
                float t = ((float)System.Math.PI / 2f + idx * spread) / (2f * (float)System.Math.PI);
                float a = PuppetAnimController.ComputeSerpentineAngle(t, idx, N, freq, maxDeg, spread);
                float expected = maxDeg * (idx + 1) / (float)N;
                Assert.AreEqual(expected, a, 0.01f, $"segIndex={idx} expected amp={expected}");
            }
        }

        [Test]
        public void SerpentineAngle_PhaseTravelsHeadToTail()
        {
            // Wave should travel head→tail: at any fixed time, segIndex 0 leads phase,
            // segIndex N-1 lags. Verify by checking that when seg0 hits sin-peak, seg(N-1)
            // hasn't yet (its sin value < 1).
            int N = 4;
            float freq = 1f, spread = 0.6f, maxDeg = 100f;
            // Time when seg0 is at its sin-peak (phase = π/2): t = 1/(4·freq) = 0.25
            float tPeak0 = 1f / (4f * freq);
            float a0 = PuppetAnimController.ComputeSerpentineAngle(tPeak0, 0, N, freq, maxDeg, spread);
            float aLast = PuppetAnimController.ComputeSerpentineAngle(tPeak0, N - 1, N, freq, maxDeg, spread);
            // Seg0 at full-amplitude peak (1/N = 25%): a0 = 25.
            Assert.AreEqual(maxDeg / N, a0, 0.01f, "seg0 should be at its sin-peak");
            // Seg(N-1) lags by (N-1)*spread radians = 1.8 rad → sin(π/2 - 1.8) ~ -0.227.
            // Amplitude tail = maxDeg, so a_last ~ -22.7.
            Assert.Less(aLast, a0, "tail (seg N-1) should lag — value differs from head leader");
        }

        [Test]
        public void SerpentineAngle_BoundedByMaxDeg()
        {
            // Sample many (time, idx) → all values in [-maxDeg, +maxDeg].
            int N = 4;
            float freq = 2.5f, spread = 0.6f, maxDeg = 35f;
            for (int i = 0; i < 200; i++)
            {
                float t = i * 0.013f; // ~2.6 sec total
                for (int idx = 0; idx < N; idx++)
                {
                    float a = PuppetAnimController.ComputeSerpentineAngle(t, idx, N, freq, maxDeg, spread);
                    Assert.LessOrEqual(System.Math.Abs(a), maxDeg + 0.0001f,
                        $"t={t} idx={idx} a={a}");
                }
            }
        }

        [Test]
        public void SerpentineAngle_ZeroFrequency_DefensiveZero()
        {
            // freq=0 → snake dừng wiggle (avoid sin(0·t) trivial).
            Assert.AreEqual(0f,
                PuppetAnimController.ComputeSerpentineAngle(1.5f, 1, 4, 0f, 35f, 0.6f),
                0.0001f);
        }

        [Test]
        public void SerpentineAngle_NegativeFrequency_DefensiveZero()
        {
            // Invalid input → standstill thay vì reverse-traveling wave.
            Assert.AreEqual(0f,
                PuppetAnimController.ComputeSerpentineAngle(1.5f, 1, 4, -2f, 35f, 0.6f),
                0.0001f);
        }

        [Test]
        public void SerpentineAngle_InvalidSegmentIndex_DefensiveZero()
        {
            // segIndex out-of-range → 0 (defensive — null segment slot or mis-config).
            Assert.AreEqual(0f,
                PuppetAnimController.ComputeSerpentineAngle(0.5f, -1, 4, 2.5f, 35f, 0.6f),
                0.0001f);
            Assert.AreEqual(0f,
                PuppetAnimController.ComputeSerpentineAngle(0.5f, 4, 4, 2.5f, 35f, 0.6f),
                0.0001f);
            Assert.AreEqual(0f,
                PuppetAnimController.ComputeSerpentineAngle(0.5f, 99, 4, 2.5f, 35f, 0.6f),
                0.0001f);
        }

        [Test]
        public void SerpentineAngle_ZeroSegmentCount_DefensiveZero()
        {
            // segmentCount=0 → nothing to drive (defensive — caller filtered out).
            Assert.AreEqual(0f,
                PuppetAnimController.ComputeSerpentineAngle(0.5f, 0, 0, 2.5f, 35f, 0.6f),
                0.0001f);
            Assert.AreEqual(0f,
                PuppetAnimController.ComputeSerpentineAngle(0.5f, 0, -1, 2.5f, 35f, 0.6f),
                0.0001f);
        }

        [Test]
        public void SerpentineAngle_PeriodicityHolds()
        {
            // Invariant: f(t) == f(t + 1/freq) cho cùng segIndex (1 full period repeats).
            int N = 4;
            float freq = 2.5f, spread = 0.6f, maxDeg = 35f;
            float[] testTimes = { 0.013f, 0.087f, 0.21f, 0.55f };
            foreach (var t in testTimes)
            {
                for (int idx = 0; idx < N; idx++)
                {
                    float a0 = PuppetAnimController.ComputeSerpentineAngle(t, idx, N, freq, maxDeg, spread);
                    float a1 = PuppetAnimController.ComputeSerpentineAngle(t + 1f / freq, idx, N, freq, maxDeg, spread);
                    Assert.AreEqual(a0, a1, 0.001f, $"t={t} idx={idx} period mismatch");
                }
            }
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

        // ---------- Phase 3: WingLeft / WingRight (flying mob — Crow / Bat) ----------

        [Test]
        public void TryParseRole_WingFilenames_Recognized()
        {
            Assert.AreEqual(CharacterArtSpec.PuppetRole.WingLeft, CharacterArtSpec.TryParseRole("wing_left"));
            Assert.AreEqual(CharacterArtSpec.PuppetRole.WingRight, CharacterArtSpec.TryParseRole("wing_right"));
        }

        [Test]
        public void TryParseRole_WingFilenames_CaseInsensitive()
        {
            Assert.AreEqual(CharacterArtSpec.PuppetRole.WingLeft, CharacterArtSpec.TryParseRole("Wing_Left"));
            Assert.AreEqual(CharacterArtSpec.PuppetRole.WingRight, CharacterArtSpec.TryParseRole("WING_RIGHT"));
        }

        [Test]
        public void IsRequiredForPuppet_Wings_AreOptional()
        {
            // Wing chỉ cần cho flying mob (Crow / Bat) — không phải required cho puppet build.
            Assert.IsFalse(CharacterArtSpec.IsRequiredForPuppet(CharacterArtSpec.PuppetRole.WingLeft));
            Assert.IsFalse(CharacterArtSpec.IsRequiredForPuppet(CharacterArtSpec.PuppetRole.WingRight));
        }

        // ---------- Phase 4: BodySegment1..4 (serpentine — Snake) ----------

        [Test]
        public void TryParseRole_BodySegmentFilenames_Recognized()
        {
            Assert.AreEqual(CharacterArtSpec.PuppetRole.BodySegment1, CharacterArtSpec.TryParseRole("body_seg_1"));
            Assert.AreEqual(CharacterArtSpec.PuppetRole.BodySegment2, CharacterArtSpec.TryParseRole("body_seg_2"));
            Assert.AreEqual(CharacterArtSpec.PuppetRole.BodySegment3, CharacterArtSpec.TryParseRole("body_seg_3"));
            Assert.AreEqual(CharacterArtSpec.PuppetRole.BodySegment4, CharacterArtSpec.TryParseRole("body_seg_4"));
        }

        [Test]
        public void TryParseRole_BodySegmentFilenames_CaseInsensitive()
        {
            Assert.AreEqual(CharacterArtSpec.PuppetRole.BodySegment1, CharacterArtSpec.TryParseRole("Body_Seg_1"));
            Assert.AreEqual(CharacterArtSpec.PuppetRole.BodySegment4, CharacterArtSpec.TryParseRole("BODY_SEG_4"));
        }

        [Test]
        public void IsRequiredForPuppet_BodySegments_AreOptional()
        {
            // Body segments chỉ cần cho serpentine mob (Snake) — không required cho puppet build.
            Assert.IsFalse(CharacterArtSpec.IsRequiredForPuppet(CharacterArtSpec.PuppetRole.BodySegment1));
            Assert.IsFalse(CharacterArtSpec.IsRequiredForPuppet(CharacterArtSpec.PuppetRole.BodySegment2));
            Assert.IsFalse(CharacterArtSpec.IsRequiredForPuppet(CharacterArtSpec.PuppetRole.BodySegment3));
            Assert.IsFalse(CharacterArtSpec.IsRequiredForPuppet(CharacterArtSpec.PuppetRole.BodySegment4));
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

        // ---------- TryParseDirection (PR J — L3+) ----------

        [Test]
        public void TryParseDirection_StandardLetters_AllRecognized()
        {
            bool ok;
            Assert.AreEqual(CharacterArtSpec.PuppetDirection.East, CharacterArtSpec.TryParseDirection("e", out ok));
            Assert.IsTrue(ok);
            Assert.AreEqual(CharacterArtSpec.PuppetDirection.North, CharacterArtSpec.TryParseDirection("n", out ok));
            Assert.IsTrue(ok);
            Assert.AreEqual(CharacterArtSpec.PuppetDirection.South, CharacterArtSpec.TryParseDirection("s", out ok));
            Assert.IsTrue(ok);
        }

        [Test]
        public void TryParseDirection_FullNames_AlsoRecognized()
        {
            bool ok;
            Assert.AreEqual(CharacterArtSpec.PuppetDirection.East, CharacterArtSpec.TryParseDirection("East", out ok));
            Assert.IsTrue(ok);
            Assert.AreEqual(CharacterArtSpec.PuppetDirection.North, CharacterArtSpec.TryParseDirection("NORTH", out ok));
            Assert.IsTrue(ok);
        }

        [Test]
        public void TryParseDirection_UnknownFolder_OkFalse()
        {
            bool ok;
            CharacterArtSpec.TryParseDirection("xyz", out ok);
            Assert.IsFalse(ok);
            CharacterArtSpec.TryParseDirection("", out ok);
            Assert.IsFalse(ok);
            CharacterArtSpec.TryParseDirection(null, out ok);
            Assert.IsFalse(ok);
        }

        // ---------- ComputeDirectionFromAngleDeg (strict, no hysteresis) ----------

        [Test]
        public void ComputeDir_PureEast_ReturnsEast()
        {
            // Cone: [-45, 45]. 0° = pure East.
            Assert.AreEqual(CharacterArtSpec.PuppetDirection.East,
                CharacterArtSpec.ComputeDirectionFromAngleDeg(0f, CharacterArtSpec.PuppetDirection.North, 0f));
        }

        [Test]
        public void ComputeDir_PureNorth_ReturnsNorth()
        {
            // 90° = pure North.
            Assert.AreEqual(CharacterArtSpec.PuppetDirection.North,
                CharacterArtSpec.ComputeDirectionFromAngleDeg(90f, CharacterArtSpec.PuppetDirection.East, 0f));
        }

        [Test]
        public void ComputeDir_PureSouth_ReturnsSouth()
        {
            // -90° = pure South.
            Assert.AreEqual(CharacterArtSpec.PuppetDirection.South,
                CharacterArtSpec.ComputeDirectionFromAngleDeg(-90f, CharacterArtSpec.PuppetDirection.East, 0f));
        }

        [Test]
        public void ComputeDir_PureWest_ReturnsWest()
        {
            // 180° = pure West.
            Assert.AreEqual(CharacterArtSpec.PuppetDirection.West,
                CharacterArtSpec.ComputeDirectionFromAngleDeg(180f, CharacterArtSpec.PuppetDirection.East, 0f));
            // -180° also West (wrap).
            Assert.AreEqual(CharacterArtSpec.PuppetDirection.West,
                CharacterArtSpec.ComputeDirectionFromAngleDeg(-180f, CharacterArtSpec.PuppetDirection.East, 0f));
        }

        [Test]
        public void ComputeDir_DiagonalNE_SnapsToEast_Or_North()
        {
            // 45° = boundary E↔N. Strict mapping (hysteresis 0): exactly 45 → East
            // (cone [-45, 45] inclusive). Test 30° → East, 60° → North.
            Assert.AreEqual(CharacterArtSpec.PuppetDirection.East,
                CharacterArtSpec.ComputeDirectionFromAngleDeg(30f, CharacterArtSpec.PuppetDirection.East, 0f));
            Assert.AreEqual(CharacterArtSpec.PuppetDirection.North,
                CharacterArtSpec.ComputeDirectionFromAngleDeg(60f, CharacterArtSpec.PuppetDirection.East, 0f));
        }

        // ---------- Hysteresis prevents flicker ----------

        [Test]
        public void ComputeDir_Hysteresis_NearBoundary_KeepsCurrentDir()
        {
            // currentDir=East, hysteresis=8°. Boundary E↔N tại 45°. Angle=50° (vượt boundary
            // 5°) → vẫn còn trong hysteresis zone → giữ East.
            Assert.AreEqual(CharacterArtSpec.PuppetDirection.East,
                CharacterArtSpec.ComputeDirectionFromAngleDeg(50f, CharacterArtSpec.PuppetDirection.East, 8f));
        }

        [Test]
        public void ComputeDir_Hysteresis_FarFromBoundary_Switches()
        {
            // Angle=60° (vượt boundary 15° > hysteresis 8°) → switch sang North.
            Assert.AreEqual(CharacterArtSpec.PuppetDirection.North,
                CharacterArtSpec.ComputeDirectionFromAngleDeg(60f, CharacterArtSpec.PuppetDirection.East, 8f));
        }

        // ---------- ComputeDirectionFromVelocity (idle case) ----------

        [Test]
        public void ComputeDir_FromVelocity_IdleZeroVel_KeepsCurrentDir()
        {
            // Velocity ~0 → caller giữ direction cũ (no flicker khi mob dừng).
            Assert.AreEqual(CharacterArtSpec.PuppetDirection.North,
                CharacterArtSpec.ComputeDirectionFromVelocity(0f, 0f, CharacterArtSpec.PuppetDirection.North, 8f));
            Assert.AreEqual(CharacterArtSpec.PuppetDirection.East,
                CharacterArtSpec.ComputeDirectionFromVelocity(0.001f, 0.001f,
                    CharacterArtSpec.PuppetDirection.East, 8f));
        }

        [Test]
        public void ComputeDir_FromVelocity_PureMotion_MatchesAngle()
        {
            // Velocity (1, 0) → East.
            Assert.AreEqual(CharacterArtSpec.PuppetDirection.East,
                CharacterArtSpec.ComputeDirectionFromVelocity(1f, 0f, CharacterArtSpec.PuppetDirection.North, 8f));
            // Velocity (0, 1) → North.
            Assert.AreEqual(CharacterArtSpec.PuppetDirection.North,
                CharacterArtSpec.ComputeDirectionFromVelocity(0f, 1f, CharacterArtSpec.PuppetDirection.East, 8f));
            // Velocity (0, -1) → South.
            Assert.AreEqual(CharacterArtSpec.PuppetDirection.South,
                CharacterArtSpec.ComputeDirectionFromVelocity(0f, -1f, CharacterArtSpec.PuppetDirection.East, 8f));
            // Velocity (-1, 0) → West.
            Assert.AreEqual(CharacterArtSpec.PuppetDirection.West,
                CharacterArtSpec.ComputeDirectionFromVelocity(-1f, 0f, CharacterArtSpec.PuppetDirection.East, 8f));
        }

        // ---------- Distance-from-boundary helper ----------

        [Test]
        public void DistanceFromBoundary_East_AtCenter_Is45()
        {
            // East cone [-45, 45], center=0, distance từ boundary = 45.
            Assert.AreEqual(45f,
                CharacterArtSpec.ComputeDistanceFromBoundary(0f, CharacterArtSpec.PuppetDirection.East),
                0.001f);
        }

        [Test]
        public void DistanceFromBoundary_East_AtBoundary_IsZero()
        {
            // Angle=45° (boundary E↔N) → distance=0 từ East.
            Assert.AreEqual(0f,
                CharacterArtSpec.ComputeDistanceFromBoundary(45f, CharacterArtSpec.PuppetDirection.East),
                0.001f);
        }

        // ---------- PR K (L2) — Forearm + shin parsing ----------

        [Test]
        public void TryParseRole_ForearmShin_AllRecognized()
        {
            Assert.AreEqual(CharacterArtSpec.PuppetRole.ForearmLeft, CharacterArtSpec.TryParseRole("forearm_left"));
            Assert.AreEqual(CharacterArtSpec.PuppetRole.ForearmRight, CharacterArtSpec.TryParseRole("forearm_right"));
            Assert.AreEqual(CharacterArtSpec.PuppetRole.ShinLeft, CharacterArtSpec.TryParseRole("shin_left"));
            Assert.AreEqual(CharacterArtSpec.PuppetRole.ShinRight, CharacterArtSpec.TryParseRole("shin_right"));
        }

        [Test]
        public void IsRequiredForPuppet_ForearmShin_AreOptional()
        {
            // Joint upgrade — missing PNG fallback to legacy 7-joint behavior.
            Assert.IsFalse(CharacterArtSpec.IsRequiredForPuppet(CharacterArtSpec.PuppetRole.ForearmLeft));
            Assert.IsFalse(CharacterArtSpec.IsRequiredForPuppet(CharacterArtSpec.PuppetRole.ShinRight));
        }

        // ---------- PR K — ComputeWalkKneeBend ----------

        [Test]
        public void WalkKneeBend_BackSwing_BendsForward()
        {
            // Positive sin (leg swinging back) → shin bends forward (positive deg).
            Assert.AreEqual(12f, PuppetAnimController.ComputeWalkKneeBend(1f, 12f), 0.0001f);
            Assert.AreEqual(6f, PuppetAnimController.ComputeWalkKneeBend(0.5f, 12f), 0.0001f);
        }

        [Test]
        public void WalkKneeBend_ForwardSwing_NoBend()
        {
            // Negative sin (leg forward) → shin straight (clamped at 0, knee không hyper-extend).
            Assert.AreEqual(0f, PuppetAnimController.ComputeWalkKneeBend(-1f, 12f), 0.0001f);
            Assert.AreEqual(0f, PuppetAnimController.ComputeWalkKneeBend(-0.5f, 12f), 0.0001f);
        }

        [Test]
        public void WalkKneeBend_ZeroSin_NoBend()
        {
            Assert.AreEqual(0f, PuppetAnimController.ComputeWalkKneeBend(0f, 12f), 0.0001f);
        }

        // ---------- PR K — ComputeCrouchKneeBend ----------

        [Test]
        public void CrouchKneeBend_FullCrouch_FullBend()
        {
            Assert.AreEqual(35f, PuppetAnimController.ComputeCrouchKneeBend(1f, 35f), 0.0001f);
        }

        [Test]
        public void CrouchKneeBend_HalfCrouch_HalfBend()
        {
            Assert.AreEqual(17.5f, PuppetAnimController.ComputeCrouchKneeBend(0.5f, 35f), 0.0001f);
        }

        [Test]
        public void CrouchKneeBend_Standing_NoBend()
        {
            Assert.AreEqual(0f, PuppetAnimController.ComputeCrouchKneeBend(0f, 35f), 0.0001f);
        }

        [Test]
        public void CrouchKneeBend_OutOfRange_Clamped()
        {
            // crouchAmount > 1 clamped to 1; < 0 clamped to 0.
            Assert.AreEqual(35f, PuppetAnimController.ComputeCrouchKneeBend(1.5f, 35f), 0.0001f);
            Assert.AreEqual(0f, PuppetAnimController.ComputeCrouchKneeBend(-0.5f, 35f), 0.0001f);
        }

        // ---------- PR K — ComputeLungeElbowBend ----------

        [Test]
        public void LungeElbowBend_AtStart_Zero()
        {
            Assert.AreEqual(0f, PuppetAnimController.ComputeLungeElbowBend(0f, 40f), 0.0001f);
        }

        [Test]
        public void LungeElbowBend_AtMid_Peak()
        {
            // Bell curve: u=0.5 → sin(π/2)=1 → maxDeg.
            Assert.AreEqual(40f, PuppetAnimController.ComputeLungeElbowBend(0.5f, 40f), 0.0001f);
        }

        [Test]
        public void LungeElbowBend_AtEnd_Zero()
        {
            Assert.AreEqual(0f, PuppetAnimController.ComputeLungeElbowBend(1f, 40f), 0.0001f);
        }

        [Test]
        public void LungeElbowBend_OutOfRange_Clamped()
        {
            Assert.AreEqual(0f, PuppetAnimController.ComputeLungeElbowBend(1.5f, 40f), 0.0001f);
            Assert.AreEqual(0f, PuppetAnimController.ComputeLungeElbowBend(-0.5f, 40f), 0.0001f);
        }
    }
}
