using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using WildernessCultivation.Vfx;

namespace WildernessCultivation.EditorTools
{
    /// <summary>
    /// Stage 5 of <c>Documentation/pipelines/PNG_TO_3D_TO_SPRITE_PIPELINE.md</c>:
    /// import the atlas + frame_metadata.json produced by
    /// <c>tools/blender_sprite_render/render_character.py</c> into Unity.
    ///
    /// <para>What it does:</para>
    /// <list type="number">
    ///   <item>Slice <c>sprite_atlas.png</c> into named sprites (<c>{anim}_{dir}_{frameIdx:D2}</c>).</item>
    ///   <item>Build one <c>AnimationClip</c> per (anim, direction) — sprite-keyframe curve at <c>frameRate</c> fps.</item>
    ///   <item>Build one <c>AnimatorController</c> with one state per anim (each state uses a 1D BlendTree on
    ///         <c>Direction</c> int parameter, thresholds 0..3 = S/E/N/W). Default state = first anim
    ///         (typically <c>idle</c>).</item>
    /// </list>
    ///
    /// <para>Y-axis flip:</para>
    /// Blender packer writes JSON with PIL/top-left origin. Unity's <c>SpriteMetaData.rect</c> expects
    /// bottom-left origin → we flip on import: <c>unityY = atlasHeight - jsonY - h</c>.
    /// </summary>
    public static class BakedSpriteCharacterImporter
    {
        const string MenuPath = "Tools/Wilderness Cultivation/Import Baked Sprite Character";
        const string MetadataFileName = "frame_metadata.json";
        const string AtlasFileName = "sprite_atlas.png";
        const string ControllerFileName = "Animator.controller";

        // Direction → BlendTree threshold (must match BakedSpriteCharacterController if we add one)
        static readonly Dictionary<string, int> DirectionThresholds = new Dictionary<string, int>
        {
            { "S", 0 }, { "E", 1 }, { "N", 2 }, { "W", 3 },
        };

        [MenuItem(MenuPath)]
        public static void ImportMenuItem()
        {
            string startDir = Path.Combine(Application.dataPath, "_Project/Art/Characters");
            if (!Directory.Exists(startDir))
            {
                startDir = Application.dataPath;
            }
            string absDir = EditorUtility.OpenFolderPanel(
                "Pick atlas folder (chứa sprite_atlas.png + frame_metadata.json)",
                startDir, "");
            if (string.IsNullOrEmpty(absDir))
            {
                return;
            }
            try
            {
                ImportFolder(absDir, showDialogs: true);
            }
            catch (System.Exception e)
            {
                EditorUtility.DisplayDialog(
                    "Import failed", e.Message + "\n\n" + e.StackTrace, "OK");
                throw;
            }
        }

        public static AnimatorController ImportFolder(string absDir, bool showDialogs)
        {
            string atlasAbs = Path.Combine(absDir, AtlasFileName);
            string metaAbs = Path.Combine(absDir, MetadataFileName);
            if (!File.Exists(atlasAbs))
            {
                throw new FileNotFoundException(
                    $"{AtlasFileName} not found in {absDir}. Run tools/blender_sprite_render/render_character.py first.");
            }
            if (!File.Exists(metaAbs))
            {
                throw new FileNotFoundException(
                    $"{MetadataFileName} not found in {absDir}.");
            }

            string atlasAssetPath = AbsToAssetPath(atlasAbs);
            string folderAssetPath = AbsToAssetPath(absDir);

            // 1. Parse metadata (validates schema + bounds)
            string json = File.ReadAllText(metaAbs);
            var meta = BakedSpriteCharacterMetadata.Parse(json);

            // 2. Configure TextureImporter + slice into named sprites
            SliceAtlas(atlasAssetPath, meta);

            // 3. Build AnimationClips per (anim, direction) + one AnimatorController
            var controller = BuildAnimatorController(folderAssetPath, atlasAssetPath, meta);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            if (showDialogs)
            {
                EditorUtility.DisplayDialog(
                    "Import complete",
                    $"Imported {meta.characterId}:\n" +
                    $"  Atlas: {atlasAssetPath}\n" +
                    $"  Controller: {AssetDatabase.GetAssetPath(controller)}\n" +
                    $"  Anims: {meta.anims.Count} × directions × {meta.framesPerAnim} frames",
                    "OK");
            }
            return controller;
        }

        // ---------------------------------------------------------------------
        // Atlas slicing
        // ---------------------------------------------------------------------
        static void SliceAtlas(string atlasAssetPath, BakedSpriteCharacterMetadata meta)
        {
            var ti = AssetImporter.GetAtPath(atlasAssetPath) as TextureImporter;
            if (ti == null)
            {
                throw new System.InvalidOperationException(
                    $"No TextureImporter at {atlasAssetPath}");
            }
            ti.textureType = TextureImporterType.Sprite;
            ti.spriteImportMode = SpriteImportMode.Multiple;
            ti.filterMode = FilterMode.Point;
            ti.mipmapEnabled = false;
            ti.alphaIsTransparency = true;
            ti.spritePixelsPerUnit = 100f;
            ti.npotScale = TextureImporterNPOTScale.None;
            ti.maxTextureSize = Mathf.Max(meta.atlasWidth, meta.atlasHeight);

            var sprites = new List<SpriteMetaData>();
            foreach (var anim in meta.anims)
            {
                foreach (var dpack in anim.directions)
                {
                    for (int i = 0; i < dpack.frames.Count; i++)
                    {
                        var f = dpack.frames[i];
                        // Flip Y: JSON top-left origin → Unity bottom-left origin
                        int unityY = meta.atlasHeight - f.y - f.h;
                        sprites.Add(new SpriteMetaData
                        {
                            name = SpriteName(anim.name, dpack.dir, i),
                            rect = new Rect(f.x, unityY, f.w, f.h),
                            alignment = (int)SpriteAlignment.Custom,
                            // foot-anchor pivot for 2.5D top-down characters
                            pivot = new Vector2(0.5f, 0.25f),
                        });
                    }
                }
            }
            ti.spritesheet = sprites.ToArray();
            EditorUtility.SetDirty(ti);
            ti.SaveAndReimport();
        }

        public static string SpriteName(string anim, string dir, int frameIdx)
        {
            return $"{anim}_{dir}_{frameIdx:D2}";
        }

        // ---------------------------------------------------------------------
        // AnimatorController construction
        // ---------------------------------------------------------------------
        static AnimatorController BuildAnimatorController(
            string folderAssetPath, string atlasAssetPath,
            BakedSpriteCharacterMetadata meta)
        {
            string controllerPath = $"{folderAssetPath}/{ControllerFileName}";

            // Delete pre-existing controller for idempotent re-import
            if (AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath) != null)
            {
                AssetDatabase.DeleteAsset(controllerPath);
            }
            var controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);

            controller.AddParameter("Direction", AnimatorControllerParameterType.Int);
            controller.AddParameter("Speed", AnimatorControllerParameterType.Float);

            // Index sprites by sprite name for fast lookup
            var spritesByName = new Dictionary<string, Sprite>();
            foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(atlasAssetPath))
            {
                if (asset is Sprite sp)
                {
                    spritesByName[sp.name] = sp;
                }
            }

            var rootSm = controller.layers[0].stateMachine;
            AnimatorState defaultState = null;

            foreach (var anim in meta.anims)
            {
                var state = rootSm.AddState(anim.name);
                var blend = new BlendTree
                {
                    name = anim.name,
                    blendType = BlendTreeType.Simple1D,
                    blendParameter = "Direction",
                    useAutomaticThresholds = false,
                };
                AssetDatabase.AddObjectToAsset(blend, controller);

                foreach (var dpack in anim.directions)
                {
                    var clip = BuildClip(anim, dpack, meta, spritesByName);
                    string clipPath = $"{folderAssetPath}/{anim.name}_{dpack.dir}.anim";
                    if (AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath) != null)
                    {
                        AssetDatabase.DeleteAsset(clipPath);
                    }
                    AssetDatabase.CreateAsset(clip, clipPath);
                    int threshold;
                    if (!DirectionThresholds.TryGetValue(dpack.dir, out threshold))
                    {
                        // Unknown direction — append at next index
                        threshold = blend.children?.Length ?? 0;
                    }
                    blend.AddChild(clip, threshold);
                }

                state.motion = blend;
                if (defaultState == null)
                {
                    defaultState = state;
                }
            }

            if (defaultState != null)
            {
                rootSm.defaultState = defaultState;
            }
            EditorUtility.SetDirty(controller);
            return controller;
        }

        static AnimationClip BuildClip(
            BakedAnimSpec anim, BakedDirectionSpec dpack,
            BakedSpriteCharacterMetadata meta,
            Dictionary<string, Sprite> spritesByName)
        {
            var clip = new AnimationClip { frameRate = meta.frameRate };
            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = anim.loop;
            AnimationUtility.SetAnimationClipSettings(clip, settings);

            var binding = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");
            var keys = new ObjectReferenceKeyframe[dpack.frames.Count];
            float perFrame = 1f / Mathf.Max(1, meta.frameRate);

            for (int i = 0; i < dpack.frames.Count; i++)
            {
                string spriteName = SpriteName(anim.name, dpack.dir, i);
                if (!spritesByName.TryGetValue(spriteName, out var sprite))
                {
                    throw new System.InvalidOperationException(
                        $"Sprite '{spriteName}' not found in atlas — slice failed?");
                }
                keys[i] = new ObjectReferenceKeyframe
                {
                    time = i * perFrame,
                    value = sprite,
                };
            }
            AnimationUtility.SetObjectReferenceCurve(clip, binding, keys);
            return clip;
        }

        // ---------------------------------------------------------------------
        // Path helpers
        // ---------------------------------------------------------------------
        static string AbsToAssetPath(string absPath)
        {
            string projectRoot = Application.dataPath;          // .../Assets
            string normalized = absPath.Replace('\\', '/');
            if (!normalized.StartsWith(projectRoot.Replace('\\', '/')))
            {
                throw new System.ArgumentException(
                    $"Path {absPath} is outside the project Assets/ folder.");
            }
            return "Assets" + normalized.Substring(projectRoot.Length);
        }
    }
}
