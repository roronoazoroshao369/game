using NUnit.Framework;
using WildernessCultivation.Vfx;

namespace WildernessCultivation.Tests.EditMode
{
    /// <summary>
    /// Verify <see cref="PlayerBoneClipSpecs"/> ra spec hợp lệ cho 5 anim clip
    /// (Idle/Walk/Crouch/Lunge/Squash). Smoke-tests:
    ///   • Length + loop flag đúng kỳ vọng (loop=true cho Idle/Walk, false cho Crouch/Lunge/Squash).
    ///   • Path bone reference paths sống trong whitelist (tránh typo break Animator binding).
    ///   • Walk arm trái + phải đối phase (sign opposite).
    ///   • Lunge arm right có 4 keyframe chứa peak đúng giá trị.
    ///   • Mỗi binding có ≥ 2 keyframe (Animator skip curve có 1 key).
    /// </summary>
    public class PlayerBoneClipSpecsTests
    {
        static readonly string[] AllowedBonePaths =
        {
            PlayerBoneClipSpecs.PathSpriteRoot,
            PlayerBoneClipSpecs.PathTorso,
            PlayerBoneClipSpecs.PathHead,
            PlayerBoneClipSpecs.PathArmLeft,
            PlayerBoneClipSpecs.PathArmRight,
            PlayerBoneClipSpecs.PathForearmLeft,
            PlayerBoneClipSpecs.PathForearmRight,
            PlayerBoneClipSpecs.PathLegLeft,
            PlayerBoneClipSpecs.PathLegRight,
            PlayerBoneClipSpecs.PathShinLeft,
            PlayerBoneClipSpecs.PathShinRight,
        };

        [Test]
        public void All_returns_five_clips_with_expected_names()
        {
            var clips = PlayerBoneClipSpecs.All();
            Assert.AreEqual(5, clips.Count);
            Assert.AreEqual(PlayerBoneClipSpecs.ClipIdle, clips[0].name);
            Assert.AreEqual(PlayerBoneClipSpecs.ClipWalk, clips[1].name);
            Assert.AreEqual(PlayerBoneClipSpecs.ClipCrouch, clips[2].name);
            Assert.AreEqual(PlayerBoneClipSpecs.ClipLunge, clips[3].name);
            Assert.AreEqual(PlayerBoneClipSpecs.ClipSquash, clips[4].name);
        }

        [Test]
        public void Loop_flags_match_intent()
        {
            Assert.IsTrue(PlayerBoneClipSpecs.Idle().loop, "Idle phải loop.");
            Assert.IsTrue(PlayerBoneClipSpecs.Walk().loop, "Walk phải loop.");
            Assert.IsFalse(PlayerBoneClipSpecs.Crouch().loop, "Crouch không loop (hold last frame).");
            Assert.IsFalse(PlayerBoneClipSpecs.Lunge().loop, "Lunge không loop (one-shot).");
            Assert.IsFalse(PlayerBoneClipSpecs.Squash().loop, "Squash không loop (one-shot).");
        }

        [Test]
        public void Lengths_are_positive_and_reasonable()
        {
            foreach (var clip in PlayerBoneClipSpecs.All())
            {
                Assert.Greater(clip.length, 0f, $"{clip.name} length phải > 0");
                Assert.Less(clip.length, 5f, $"{clip.name} length quá dài (sanity check)");
            }
        }

        [Test]
        public void Every_binding_uses_whitelisted_bone_path()
        {
            foreach (var clip in PlayerBoneClipSpecs.All())
            {
                foreach (var curve in clip.curves)
                {
                    CollectionAssert.Contains(AllowedBonePaths, curve.bonePath,
                        $"{clip.name} có bonePath '{curve.bonePath}' ngoài whitelist (typo?).");
                }
            }
        }

        [Test]
        public void Every_binding_has_at_least_two_keyframes()
        {
            // Animator skip animation curve có < 2 keys → state freeze. Guard against accidental
            // single-key curves.
            foreach (var clip in PlayerBoneClipSpecs.All())
            {
                foreach (var curve in clip.curves)
                {
                    Assert.GreaterOrEqual(curve.keys.Length, 2,
                        $"{clip.name}/{curve.bonePath}/{curve.property} chỉ có {curve.keys.Length} key.");
                }
            }
        }

        [Test]
        public void Walk_arms_swing_in_opposite_phase()
        {
            var walk = PlayerBoneClipSpecs.Walk();
            PlayerBoneClipSpecs.CurveBinding? armLeft = null, armRight = null;
            foreach (var c in walk.curves)
            {
                if (c.bonePath == PlayerBoneClipSpecs.PathArmLeft && c.property == PlayerBoneClipSpecs.PropRotZ)
                    armLeft = c;
                if (c.bonePath == PlayerBoneClipSpecs.PathArmRight && c.property == PlayerBoneClipSpecs.PropRotZ)
                    armRight = c;
            }
            Assert.IsTrue(armLeft.HasValue, "Walk thiếu ArmLeft Z curve.");
            Assert.IsTrue(armRight.HasValue, "Walk thiếu ArmRight Z curve.");
            // Peak key (index 1) phải opposite sign.
            float lPeak = armLeft.Value.keys[1].value;
            float rPeak = armRight.Value.keys[1].value;
            Assert.Greater(lPeak * rPeak, -1f, "Arm left+right peak nên opposite sign (đối phase).");
            Assert.AreNotEqual(0f, lPeak, "ArmLeft peak khác 0.");
            Assert.AreNotEqual(0f, rPeak, "ArmRight peak khác 0.");
            Assert.IsTrue((lPeak > 0f && rPeak < 0f) || (lPeak < 0f && rPeak > 0f),
                "Arm swing phải đối dấu — đối phase.");
        }

        [Test]
        public void Lunge_arm_right_snap_value_is_negative_and_steep()
        {
            var lunge = PlayerBoneClipSpecs.Lunge();
            PlayerBoneClipSpecs.CurveBinding? armRight = null;
            foreach (var c in lunge.curves)
            {
                if (c.bonePath == PlayerBoneClipSpecs.PathArmRight && c.property == PlayerBoneClipSpecs.PropRotZ)
                    armRight = c;
            }
            Assert.IsTrue(armRight.HasValue, "Lunge thiếu ArmRight Z curve.");
            float minValue = float.PositiveInfinity;
            foreach (var k in armRight.Value.keys)
            {
                if (k.value < minValue) minValue = k.value;
            }
            // -90° = forward stab. Allow ±5° tolerance.
            Assert.Less(minValue, -50f,
                $"Lunge ArmRight snap value {minValue}° quá yếu (mong đợi ~-90°).");
        }

        [Test]
        public void Idle_torso_breath_amplitude_is_subtle()
        {
            var idle = PlayerBoneClipSpecs.Idle();
            PlayerBoneClipSpecs.CurveBinding? torso = null;
            foreach (var c in idle.curves)
            {
                if (c.bonePath == PlayerBoneClipSpecs.PathTorso && c.property == PlayerBoneClipSpecs.PropPosY)
                    torso = c;
            }
            Assert.IsTrue(torso.HasValue, "Idle thiếu Torso Y curve.");
            float maxAbs = 0f;
            foreach (var k in torso.Value.keys)
            {
                float a = k.value < 0f ? -k.value : k.value;
                if (a > maxAbs) maxAbs = a;
            }
            Assert.Less(maxAbs, 0.10f,
                $"Idle breath amplitude {maxAbs} quá lớn (DST subtle range 0.02-0.06).");
            Assert.Greater(maxAbs, 0.01f,
                $"Idle breath amplitude {maxAbs} quá nhỏ (invisible).");
        }
    }
}
