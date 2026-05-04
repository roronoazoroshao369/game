#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using WildernessCultivation.Core;
using WildernessCultivation.Vfx;

namespace WildernessCultivation.EditorTools
{
    /// <summary>
    /// Pilot path B (DST-style): one-click build a bone-rig Player prefab dùng existing
    /// 22 PNG body parts ở <c>Art/Characters/player/{E,N,S}/</c> (extracted bởi PR #140).
    ///
    /// Output:
    ///   • <c>Assets/_Project/Prefabs/Player_Rigged.prefab</c> — root + SpriteRoot + bone tree
    ///     + <see cref="BoneAnimController"/> + <see cref="Animator"/> + <see cref="Rigidbody2D"/>.
    ///   • <c>Assets/_Project/Animations/Player_Rigged/Player.controller</c> — 5 state FSM.
    ///   • <c>Assets/_Project/Animations/Player_Rigged/{Idle,Walk,Crouch,Lunge,Squash}.anim</c> —
    ///     5 clips, curves từ <see cref="PlayerBoneClipSpecs"/>.
    ///
    /// Idempotent: re-run regenerates (overwrite existing assets).
    ///
    /// SCOPE: pilot KHÔNG modify existing <c>BootstrapWizard.BuildPlayerPrefab</c> path
    /// (PuppetAnimController vẫn là default flow). User compare prefab side-by-side trong scene.
    /// Sau review tích cực → follow-up PR có thể swap default Player path sang rig này.
    /// </summary>
    public static class PlayerBoneRigBuilder
    {
        const string CharacterId = "player";
        const string PrefabPath = "Assets/_Project/Prefabs/Player_Rigged.prefab";
        const string AnimFolder = "Assets/_Project/Animations/Player_Rigged";
        const string ControllerPath = AnimFolder + "/Player.controller";

        // Joint offsets — match BootstrapWizard hardcoded defaults (shoulderY=0.55, shoulderX=0.30,
        // hipY=-0.55, hipX=0.13, elbowOverlap=0.06, kneeOverlap=0.10) để rig size khớp Puppet Player.
        const float ShoulderY = 0.55f;
        const float ShoulderX = 0.30f;
        const float HipY = -0.55f;
        const float HipX = 0.13f;
        const float ElbowOverlap = 0.06f;
        const float KneeOverlap = 0.10f;

        // Joint length defaults — match PuppetPlaceholderSpec heights / PPU (placeholder PPU=64).
        const float ArmLen = 56f / 64f;
        const float LegLen = 60f / 64f;
        const float TorsoHalf = 80f / 64f * 0.5f;

        const int SortingOrderBase = 100;

        [MenuItem("Tools/Wilderness Cultivation/Build Player Bone Rig")]
        public static void BuildMenuItem()
        {
            var prefab = BuildPlayerRigged(showDialogs: true);
            if (prefab != null) EditorGUIUtility.PingObject(prefab);
        }

        /// <summary>
        /// Programmatic entry. Returns saved prefab asset, hoặc null nếu sprites missing.
        /// </summary>
        public static GameObject BuildPlayerRigged(bool showDialogs)
        {
            var spriteSet = CharacterArtImporter.TryLoadCharacterSpriteSet(
                CharacterId, placeholderHeightPx: 32);
            if (spriteSet == null || spriteSet.EastSprites == null || spriteSet.EastSprites.Count == 0)
            {
                if (showDialogs)
                {
                    EditorUtility.DisplayDialog(
                        "Player Bone Rig — sprites missing",
                        "Không tìm thấy PNG ở Art/Characters/player/E/. Cần ≥ Head + Torso để build rig.\n\n" +
                        "Drop PNG vào folder rồi chạy lại menu.",
                        "OK");
                }
                return null;
            }

            EnsureFolder(Path.GetDirectoryName(PrefabPath));
            EnsureFolder(AnimFolder);

            // Build clips first → controller binds chúng vào state Motion.
            var clips = BuildAndSaveClips();
            var controller = BuildController(clips);

            var go = BuildHierarchy(spriteSet.EastSprites);
            WireComponents(go, controller);

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, PrefabPath);
            Object.DestroyImmediate(go);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            if (showDialogs)
            {
                EditorUtility.DisplayDialog(
                    "Player Bone Rig — done",
                    $"Built:\n  • {PrefabPath}\n  • {ControllerPath}\n  • {clips.Count} clips ở {AnimFolder}/\n\n" +
                    "Drag prefab vào scene + hit Play để xem Idle. Animator window cho test Walk/Crouch/Lunge.",
                    "OK");
            }

            return prefab;
        }

        // ============ Hierarchy ============

        static GameObject BuildHierarchy(Dictionary<CharacterArtSpec.PuppetRole, Sprite> sprites)
        {
            var root = new GameObject("Player_Rigged");
            root.tag = "Untagged"; // Pilot prefab; user manually retag "Player" nếu replace default.

            var spriteRoot = new GameObject("SpriteRoot");
            spriteRoot.transform.SetParent(root.transform, false);

            // Order matters cho Z-sorting trong same orderInLayer:
            //   LegLeft (back) > LegRight (front) > Torso > ArmLeft (back arm) > ArmRight (front arm).
            // Mirror cấu trúc BootstrapWizard.BuildPuppetHierarchy.
            var legLeft = AddPart(spriteRoot.transform, sprites,
                CharacterArtSpec.PuppetRole.LegLeft,
                SortingOrderBase + 1, new Vector3(-HipX, HipY, 0f));
            var legRight = AddPart(spriteRoot.transform, sprites,
                CharacterArtSpec.PuppetRole.LegRight,
                SortingOrderBase + 1, new Vector3(HipX, HipY, 0f));

            // Shins child của legs — overlap up bằng kneeOverlap để hide thin top edge của shin.
            if (legLeft != null)
            {
                AddPart(legLeft, sprites, CharacterArtSpec.PuppetRole.ShinLeft,
                    SortingOrderBase + 1, new Vector3(0f, -LegLen + KneeOverlap, 0f));
            }
            if (legRight != null)
            {
                AddPart(legRight, sprites, CharacterArtSpec.PuppetRole.ShinRight,
                    SortingOrderBase + 1, new Vector3(0f, -LegLen + KneeOverlap, 0f));
            }

            var torso = AddPart(spriteRoot.transform, sprites,
                CharacterArtSpec.PuppetRole.Torso,
                SortingOrderBase + 2, Vector3.zero);

            // Head child của torso — bob theo torso tự nhiên.
            if (torso != null)
            {
                AddPart(torso, sprites, CharacterArtSpec.PuppetRole.Head,
                    SortingOrderBase + 5, new Vector3(0f, TorsoHalf, 0f));
            }

            var armLeft = AddPart(spriteRoot.transform, sprites,
                CharacterArtSpec.PuppetRole.ArmLeft,
                SortingOrderBase + 3, new Vector3(-ShoulderX, ShoulderY, 0f));
            var armRight = AddPart(spriteRoot.transform, sprites,
                CharacterArtSpec.PuppetRole.ArmRight,
                SortingOrderBase + 3, new Vector3(ShoulderX, ShoulderY, 0f));

            if (armLeft != null)
            {
                AddPart(armLeft, sprites, CharacterArtSpec.PuppetRole.ForearmLeft,
                    SortingOrderBase + 4, new Vector3(0f, -ArmLen + ElbowOverlap, 0f));
            }
            if (armRight != null)
            {
                AddPart(armRight, sprites, CharacterArtSpec.PuppetRole.ForearmRight,
                    SortingOrderBase + 4, new Vector3(0f, -ArmLen + ElbowOverlap, 0f));
            }

            return root;
        }

        static Transform AddPart(Transform parent,
            Dictionary<CharacterArtSpec.PuppetRole, Sprite> sprites,
            CharacterArtSpec.PuppetRole role,
            int sortingOrder,
            Vector3 localPos)
        {
            if (!sprites.TryGetValue(role, out var sprite) || sprite == null) return null;
            var go = new GameObject(role.ToString());
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = sortingOrder;
            return go.transform;
        }

        // ============ Components ============

        static void WireComponents(GameObject root, AnimatorController controller)
        {
            var rb = root.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;

            var col = root.AddComponent<CircleCollider2D>();
            col.radius = 0.4f;

            var animator = root.AddComponent<Animator>();
            animator.runtimeAnimatorController = controller;
            animator.applyRootMotion = false;
            // Always animate trong cả khi off-screen — pilot chạy trong Editor view không cần optimize.
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

            var bone = root.AddComponent<BoneAnimController>();
            bone.animator = animator;
            bone.spriteRoot = root.transform.Find("SpriteRoot");
            bone.body = rb;
        }

        // ============ AnimationClip ============

        static List<AnimationClip> BuildAndSaveClips()
        {
            var specs = PlayerBoneClipSpecs.All();
            var clips = new List<AnimationClip>(specs.Count);
            foreach (var spec in specs)
            {
                var clip = new AnimationClip { name = spec.name };
                var settings = AnimationUtility.GetAnimationClipSettings(clip);
                settings.loopTime = spec.loop;
                AnimationUtility.SetAnimationClipSettings(clip, settings);

                foreach (var binding in spec.curves)
                {
                    var curve = new AnimationCurve(binding.keys);
                    var b = new EditorCurveBinding
                    {
                        path = binding.bonePath,
                        type = typeof(Transform),
                        propertyName = binding.property,
                    };
                    AnimationUtility.SetEditorCurve(clip, b, curve);
                }

                string path = $"{AnimFolder}/{spec.name}.anim";
                var existing = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
                if (existing != null)
                {
                    EditorUtility.CopySerialized(clip, existing);
                    clips.Add(existing);
                }
                else
                {
                    AssetDatabase.CreateAsset(clip, path);
                    clips.Add(clip);
                }
            }
            return clips;
        }

        // ============ AnimatorController ============

        static AnimatorController BuildController(List<AnimationClip> clips)
        {
            // Re-create from scratch — tránh stale state khi user re-run menu sau code change.
            AssetDatabase.DeleteAsset(ControllerPath);
            var controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);

            controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
            controller.AddParameter("Moving", AnimatorControllerParameterType.Bool);
            controller.AddParameter("Crouch", AnimatorControllerParameterType.Bool);
            controller.AddParameter("Lunge", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Squash", AnimatorControllerParameterType.Trigger);

            var sm = controller.layers[0].stateMachine;

            var clipByName = new Dictionary<string, AnimationClip>();
            foreach (var c in clips) clipByName[c.name] = c;

            var idleState = sm.AddState(PlayerBoneClipSpecs.ClipIdle);
            idleState.motion = clipByName[PlayerBoneClipSpecs.ClipIdle];
            sm.defaultState = idleState;

            var walkState = sm.AddState(PlayerBoneClipSpecs.ClipWalk);
            walkState.motion = clipByName[PlayerBoneClipSpecs.ClipWalk];

            var crouchState = sm.AddState(PlayerBoneClipSpecs.ClipCrouch);
            crouchState.motion = clipByName[PlayerBoneClipSpecs.ClipCrouch];

            var lungeState = sm.AddState(PlayerBoneClipSpecs.ClipLunge);
            lungeState.motion = clipByName[PlayerBoneClipSpecs.ClipLunge];

            var squashState = sm.AddState(PlayerBoneClipSpecs.ClipSquash);
            squashState.motion = clipByName[PlayerBoneClipSpecs.ClipSquash];

            // ========== Transitions ==========

            // Idle → Walk: Moving=true.
            AddTransition(idleState, walkState, "Moving", true, hasExit: false, duration: 0.1f);
            // Walk → Idle: Moving=false.
            AddTransition(walkState, idleState, "Moving", false, hasExit: false, duration: 0.1f);

            // Idle → Crouch / Walk → Crouch: Crouch=true.
            AddTransition(idleState, crouchState, "Crouch", true, hasExit: false, duration: 0.1f);
            AddTransition(walkState, crouchState, "Crouch", true, hasExit: false, duration: 0.1f);
            // Crouch → Idle: Crouch=false.
            AddTransition(crouchState, idleState, "Crouch", false, hasExit: false, duration: 0.1f);

            // Any → Lunge: Lunge trigger.
            AddTriggerTransition(sm, lungeState, "Lunge");
            // Lunge → Idle: hasExitTime, exitTime=0.95.
            AddExitTimeTransition(lungeState, idleState, 0.95f, 0.1f);

            // Any → Squash: Squash trigger.
            AddTriggerTransition(sm, squashState, "Squash");
            AddExitTimeTransition(squashState, idleState, 0.95f, 0.1f);

            EditorUtility.SetDirty(controller);
            return controller;
        }

        static void AddTransition(AnimatorState src, AnimatorState dst, string param, bool boolValue,
            bool hasExit, float duration)
        {
            var tr = src.AddTransition(dst);
            tr.hasExitTime = hasExit;
            tr.duration = duration;
            tr.canTransitionToSelf = false;
            tr.AddCondition(boolValue ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot,
                0f, param);
        }

        static void AddTriggerTransition(AnimatorStateMachine sm, AnimatorState dst, string trigger)
        {
            var tr = sm.AddAnyStateTransition(dst);
            tr.hasExitTime = false;
            tr.duration = 0.05f;
            tr.canTransitionToSelf = false;
            tr.AddCondition(AnimatorConditionMode.If, 0f, trigger);
        }

        static void AddExitTimeTransition(AnimatorState src, AnimatorState dst,
            float exitTime, float duration)
        {
            var tr = src.AddTransition(dst);
            tr.hasExitTime = true;
            tr.exitTime = exitTime;
            tr.duration = duration;
            tr.canTransitionToSelf = false;
        }

        // ============ Folder helper ============

        static void EnsureFolder(string folderPath)
        {
            if (string.IsNullOrEmpty(folderPath)) return;
            if (AssetDatabase.IsValidFolder(folderPath)) return;
            var parent = Path.GetDirectoryName(folderPath);
            var leaf = Path.GetFileName(folderPath);
            if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
            {
                EnsureFolder(parent);
            }
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                AssetDatabase.CreateFolder(parent, leaf);
            }
        }
    }
}
#endif
