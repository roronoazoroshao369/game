using System.Collections.Generic;
using UnityEngine;

namespace WildernessCultivation.Vfx
{
    /// <summary>
    /// Pure-data specs cho 5 AnimationClip của Player rigged: Idle / Walk / Crouch / Lunge / Squash.
    /// Map 1-1 với param surface của <see cref="BoneAnimController"/> (Speed/Moving/Crouch/Lunge/Squash)
    /// nên KHÔNG cần extend API.
    ///
    /// Mỗi spec = list <see cref="CurveBinding"/> mô tả keyframe trên (bonePath, property). Editor-side
    /// wrapper (<c>PlayerBoneRigBuilder</c>) consume specs này để build <c>AnimationClip</c> qua
    /// <c>AnimationUtility.SetEditorCurve</c>. Pure-data sống ở runtime asmdef → testable từ EditMode
    /// runtime tests (không cần UnityEditor reference).
    ///
    /// Bone path convention (relative tới Animator GameObject): <c>SpriteRoot/Torso</c>,
    /// <c>SpriteRoot/Torso/Head</c>, <c>SpriteRoot/ArmLeft</c>, <c>SpriteRoot/ArmLeft/ForearmLeft</c>, …
    /// Path khớp đúng tên Transform trong hierarchy <c>PlayerBoneRigBuilder</c> dựng.
    ///
    /// Property names match Unity's reflection convention cho Transform animation:
    ///   <c>m_LocalPosition.x/y/z</c> · <c>localEulerAnglesRaw.z</c>.
    /// Z rotation thường ở local euler (không quaternion) để curve tuyến tính dễ tune.
    /// </summary>
    public static class PlayerBoneClipSpecs
    {
        public const string ClipIdle = "Idle";
        public const string ClipWalk = "Walk";
        public const string ClipCrouch = "Crouch";
        public const string ClipLunge = "Lunge";
        public const string ClipSquash = "Squash";

        // Bone paths — phải match đúng tên Transform trong hierarchy PlayerBoneRigBuilder dựng.
        public const string PathSpriteRoot = "SpriteRoot";
        public const string PathTorso = "SpriteRoot/Torso";
        public const string PathHead = "SpriteRoot/Torso/Head";
        public const string PathArmLeft = "SpriteRoot/ArmLeft";
        public const string PathArmRight = "SpriteRoot/ArmRight";
        public const string PathForearmLeft = "SpriteRoot/ArmLeft/ForearmLeft";
        public const string PathForearmRight = "SpriteRoot/ArmRight/ForearmRight";
        public const string PathLegLeft = "SpriteRoot/LegLeft";
        public const string PathLegRight = "SpriteRoot/LegRight";
        public const string PathShinLeft = "SpriteRoot/LegLeft/ShinLeft";
        public const string PathShinRight = "SpriteRoot/LegRight/ShinRight";

        // Property names cho AnimationUtility.SetEditorCurve.
        public const string PropPosY = "m_LocalPosition.y";
        public const string PropPosX = "m_LocalPosition.x";
        public const string PropRotZ = "localEulerAnglesRaw.z";

        public struct CurveBinding
        {
            public string bonePath;
            public string property;
            public Keyframe[] keys;
            public bool loop;
        }

        public class ClipSpec
        {
            public string name;
            public float length;
            public bool loop;
            public List<CurveBinding> curves = new List<CurveBinding>();
        }

        public static List<ClipSpec> All()
        {
            return new List<ClipSpec>
            {
                Idle(),
                Walk(),
                Crouch(),
                Lunge(),
                Squash(),
            };
        }

        // ============ Idle ============
        // 2s loop. Subtle DST-style breath: torso Y bob + head sub-bob + arm sway.
        public static ClipSpec Idle()
        {
            const float len = 2f;
            var spec = new ClipSpec { name = ClipIdle, length = len, loop = true };

            // Torso Y: 0 → +0.04 (inhale peak ~1s) → 0. Smooth ease-in-out.
            spec.curves.Add(SineLoop(PathTorso, PropPosY, len, 0.04f, 0f));
            // Head Y: 0 → +0.012 in-phase với torso (tiny extra bob, neck stretch).
            spec.curves.Add(SineLoop(PathHead, PropPosY, len, 0.012f, 0f));
            // ArmLeft Z: ±2° slow sway.
            spec.curves.Add(SineLoop(PathArmLeft, PropRotZ, len, 2f, 0f));
            // ArmRight Z: opposite sign (mirrored sway).
            spec.curves.Add(SineLoop(PathArmRight, PropRotZ, len, -2f, 0f));
            return spec;
        }

        // ============ Walk ============
        // 0.5s loop @ animator speed=1 (clip-time 0.5 = 1 step cycle). Animator scales speed
        // theo Speed param (tuning trong AnimatorController state Walk speedMultiplier).
        public static ClipSpec Walk()
        {
            const float len = 0.5f;
            var spec = new ClipSpec { name = ClipWalk, length = len, loop = true };

            // ArmLeft Z: 0 → +28° → 0 → -28° → 0 (full swing).
            spec.curves.Add(FullSwingLoop(PathArmLeft, PropRotZ, len, 28f));
            spec.curves.Add(FullSwingLoop(PathArmRight, PropRotZ, len, -28f));
            // ForearmLeft Z: ±10° follow-through (elbow bend trên down-swing).
            spec.curves.Add(FullSwingLoop(PathForearmLeft, PropRotZ, len, 10f));
            spec.curves.Add(FullSwingLoop(PathForearmRight, PropRotZ, len, -10f));

            // LegLeft Z: opposite phase với ArmLeft (right arm forward + left leg forward).
            spec.curves.Add(FullSwingLoop(PathLegLeft, PropRotZ, len, -22f));
            spec.curves.Add(FullSwingLoop(PathLegRight, PropRotZ, len, 22f));
            // ShinLeft Z: ±15° back-swing (knee bend khi leg passes vertical).
            spec.curves.Add(KneeBendLoop(PathShinLeft, PropRotZ, len, 15f, phaseOffset: 0f));
            spec.curves.Add(KneeBendLoop(PathShinRight, PropRotZ, len, 15f, phaseOffset: 0.5f));

            // Torso Y bob: peak Y khi 1 chân chạm đất → 2 peaks per cycle (freq 2x).
            spec.curves.Add(StepBobLoop(PathTorso, PropPosY, len, 0.05f));
            return spec;
        }

        // ============ Crouch ============
        // 0.4s one-shot transition (Animator state Crouch hold last frame khi Crouch=true).
        // Loop = false; Animator state có exit time = false để stay ở pose này khi bool active.
        public static ClipSpec Crouch()
        {
            const float len = 0.4f;
            var spec = new ClipSpec { name = ClipCrouch, length = len, loop = false };

            // Torso Y drop -0.08 over 0.4s, ease-out.
            spec.curves.Add(EaseTwoKey(PathTorso, PropPosY, len, 0f, -0.08f));
            // ShinLeft/Right Z bend (knee bend): 0 → -25° (forward bend).
            spec.curves.Add(EaseTwoKey(PathShinLeft, PropRotZ, len, 0f, -25f));
            spec.curves.Add(EaseTwoKey(PathShinRight, PropRotZ, len, 0f, 25f));
            // ArmLeft/Right slight bend forward (defensive crouch arm).
            spec.curves.Add(EaseTwoKey(PathArmLeft, PropRotZ, len, 0f, -15f));
            spec.curves.Add(EaseTwoKey(PathArmRight, PropRotZ, len, 0f, 15f));
            return spec;
        }

        // ============ Lunge (Attack) ============
        // 0.4s one-shot. Anticipation → snap → return.
        public static ClipSpec Lunge()
        {
            const float len = 0.4f;
            var spec = new ClipSpec { name = ClipLunge, length = len, loop = false };

            // ArmRight Z: 0 → +20° anticipation (0.0-0.08s) → -90° snap forward (0.08-0.18s)
            //          → 0 return (0.18-0.40s).
            spec.curves.Add(new CurveBinding
            {
                bonePath = PathArmRight,
                property = PropRotZ,
                keys = new[]
                {
                    new Keyframe(0f, 0f),
                    new Keyframe(0.08f, 20f),
                    new Keyframe(0.18f, -90f),
                    new Keyframe(0.40f, 0f),
                },
            });
            // ForearmRight follows: 0 → -45° at snap → 0.
            spec.curves.Add(new CurveBinding
            {
                bonePath = PathForearmRight,
                property = PropRotZ,
                keys = new[]
                {
                    new Keyframe(0f, 0f),
                    new Keyframe(0.18f, -45f),
                    new Keyframe(0.40f, 0f),
                },
            });
            // Torso Y: 0 → -0.02 (squash into stab) → +0.01 (release) → 0.
            spec.curves.Add(new CurveBinding
            {
                bonePath = PathTorso,
                property = PropPosY,
                keys = new[]
                {
                    new Keyframe(0f, 0f),
                    new Keyframe(0.10f, -0.02f),
                    new Keyframe(0.18f, 0.01f),
                    new Keyframe(0.40f, 0f),
                },
            });
            return spec;
        }

        // ============ Squash ============
        // 0.25s vertical compress + bounce (e.g. landing impact / dodge prep).
        public static ClipSpec Squash()
        {
            const float len = 0.25f;
            var spec = new ClipSpec { name = ClipSquash, length = len, loop = false };

            // Torso Y: 0 → -0.10 (squash) → +0.05 (overshoot) → 0.
            spec.curves.Add(new CurveBinding
            {
                bonePath = PathTorso,
                property = PropPosY,
                keys = new[]
                {
                    new Keyframe(0f, 0f),
                    new Keyframe(0.08f, -0.10f),
                    new Keyframe(0.18f, 0.05f),
                    new Keyframe(0.25f, 0f),
                },
            });
            // Head follows torso slightly delayed (anticipation lag).
            spec.curves.Add(new CurveBinding
            {
                bonePath = PathHead,
                property = PropPosY,
                keys = new[]
                {
                    new Keyframe(0f, 0f),
                    new Keyframe(0.10f, -0.04f),
                    new Keyframe(0.20f, 0.02f),
                    new Keyframe(0.25f, 0f),
                },
            });
            return spec;
        }

        // ============ Curve helpers ============

        // 4-key sine-shaped loop: 0 → +amp → 0 → -amp → 0 over len, repeating.
        // Used for Idle breath bob + arm sway.
        static CurveBinding SineLoop(string path, string property, float len, float amp, float baseValue)
        {
            float quarter = len * 0.25f;
            return new CurveBinding
            {
                bonePath = path,
                property = property,
                keys = new[]
                {
                    new Keyframe(0f, baseValue),
                    new Keyframe(quarter, baseValue + amp),
                    new Keyframe(quarter * 2f, baseValue),
                    new Keyframe(quarter * 3f, baseValue - amp),
                    new Keyframe(len, baseValue),
                },
                loop = true,
            };
        }

        // Walk swing 1 cycle: 0 → +amp → 0 → -amp → 0 (5 keys, sine-like).
        static CurveBinding FullSwingLoop(string path, string property, float len, float ampDeg)
        {
            float quarter = len * 0.25f;
            return new CurveBinding
            {
                bonePath = path,
                property = property,
                keys = new[]
                {
                    new Keyframe(0f, 0f),
                    new Keyframe(quarter, ampDeg),
                    new Keyframe(quarter * 2f, 0f),
                    new Keyframe(quarter * 3f, -ampDeg),
                    new Keyframe(len, 0f),
                },
                loop = true,
            };
        }

        // Knee bend during walk: bend kicks in when leg swings backward (phase 0.5).
        // phaseOffset shifts cycle for left/right asymmetry.
        static CurveBinding KneeBendLoop(string path, string property, float len, float ampDeg,
            float phaseOffset)
        {
            float t = (phaseOffset % 1f) * len;
            return new CurveBinding
            {
                bonePath = path,
                property = property,
                keys = new[]
                {
                    new Keyframe(0f, 0f),
                    new Keyframe((t + len * 0.25f) % len, -ampDeg),
                    new Keyframe((t + len * 0.5f) % len, 0f),
                    new Keyframe(len, 0f),
                },
                loop = true,
            };
        }

        // Step bob: 2 peaks per cycle (heel-strike each foot).
        static CurveBinding StepBobLoop(string path, string property, float len, float amp)
        {
            return new CurveBinding
            {
                bonePath = path,
                property = property,
                keys = new[]
                {
                    new Keyframe(0f, 0f),
                    new Keyframe(len * 0.25f, amp),
                    new Keyframe(len * 0.5f, 0f),
                    new Keyframe(len * 0.75f, amp),
                    new Keyframe(len, 0f),
                },
                loop = true,
            };
        }

        // Two-key ease (start → end). Caller chọn property + magnitudes.
        static CurveBinding EaseTwoKey(string path, string property, float len, float a, float b)
        {
            return new CurveBinding
            {
                bonePath = path,
                property = property,
                keys = new[]
                {
                    new Keyframe(0f, a),
                    new Keyframe(len, b),
                },
                loop = false,
            };
        }
    }
}
