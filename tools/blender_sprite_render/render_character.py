"""Blender Python: bake rigged FBX → 4-direction × N-frame ortho sprite atlas.

Headless Blender script for Stage 4 of `Documentation/pipelines/PNG_TO_3D_TO_SPRITE_PIPELINE.md`.

Usage (headless):
    blender --background --python tools/blender_sprite_render/render_character.py -- \
        --character_id player \
        --base_mesh ~/work/player/anim/idle.fbx \
        --anim_dir ~/work/player/anim/ \
        --out_dir Assets/_Project/Art/Characters/player/atlas/ \
        --frames_per_anim 12 \
        --cell 256 \
        --frame_rate 12

Inputs:
    --base_mesh        FBX with mesh + armature in T-pose (e.g. idle.fbx from Mixamo "With Skin")
    --anim_dir         Folder with one FBX per anim (Mixamo "Without Skin" downloads ok).
                       File stem becomes anim name (e.g. walk.fbx → "walk").
    --out_dir          Output folder for sprite_atlas.png + frame_metadata.json
                       (intermediate frames in <out_dir>/_raw/<anim>/<dir>/<frame>.png).
    --character_id     Character id for metadata + downstream Unity importer.
    --frames_per_anim  N frames sampled uniformly per anim/direction (default 12).
    --cell             Square cell side in px (default 256). Smaller = smaller atlas.
    --frame_rate       Playback fps recorded in metadata (default 12).
    --keep_raw         Keep <out_dir>/_raw/ scratch frames after atlas build (debug).
    --strip_root_motion  Zero out Hips X/Y translation curves (default True).
    --loop_anims       Comma-separated list of anim names that loop. Empty = use builtin
                       defaults (idle/walk/run/meditation/victory).

Output schema: see frame_packer.py §SCHEMA.

Style configuration (toon shader): tunable via constants below. Defaults match
`Documentation/art/ART_STYLE.md` palette + outline thickness lock.

Compatible with Blender 3.x + 4.x. EEVEE Next (4.2+) preferred; falls back to
classic Eevee on 3.x.
"""
from __future__ import annotations

import argparse
import math
import os
import shutil
import sys
import traceback
from typing import Dict, List, Optional, Tuple

# ---- Blender imports (only when running inside Blender) ----
try:
    import bpy  # type: ignore
    import mathutils  # type: ignore
    BLENDER_AVAILABLE = True
except ImportError:
    BLENDER_AVAILABLE = False

# Add sibling dir for frame_packer import (works headless inside Blender too)
HERE = os.path.dirname(os.path.abspath(__file__))
if HERE not in sys.path:
    sys.path.insert(0, HERE)

import frame_packer  # noqa: E402

# ---- Style tuning constants (see ART_STYLE.md palette lock) ----
PALETTE = {
    "outline": (0.10, 0.08, 0.03, 1.0),   # #1a1408 sepia black
    "shadow": (0.45, 0.40, 0.35, 1.0),
    "midtone": (0.78, 0.70, 0.60, 1.0),
    "highlight": (0.92, 0.86, 0.74, 1.0),
}
OUTLINE_THICKNESS_RATIO = 0.012     # ≈ 1.2% body height (AI_PROMPTS §1 luật 1)
WATERCOLOR_NOISE_FAC = 0.15
RAMP_STOPS = (0.0, 0.45, 0.85)      # 3-stop hard cel
WORLD_AMBIENT = (0.50, 0.45, 0.40)  # sepia tint (§11 layer 4)

# Camera config — slight high angle for top-down 2.5D feel
CAM_DISTANCE = 4.0
CAM_HEIGHT_RATIO = 0.5              # cam.z = distance * ratio
CAM_TILT_DEG = 60                   # tilt down from horizontal
ORTHO_SCALE_DEFAULT = 2.5

# Direction → world-space pivot Z rotation (radians). DST convention:
#   S = facing camera (+Y forward, cam from -Y), E = facing +X, N = away, W = facing -X
DIRECTIONS_RAD = {
    "S": 0.0,
    "E": math.radians(90),
    "N": math.radians(180),
    "W": math.radians(270),
}
DIRECTION_ORDER = ("S", "E", "N", "W")

DEFAULT_LOOP_ANIMS = ("idle", "walk", "run", "meditation", "victory")


# =====================================================================
#                              ARGUMENT PARSING
# =====================================================================
def parse_args() -> argparse.Namespace:
    """Parse args after Blender's '--' sentinel."""
    if "--" in sys.argv:
        argv = sys.argv[sys.argv.index("--") + 1:]
    else:
        argv = sys.argv[1:]
    p = argparse.ArgumentParser(prog="render_character.py")
    p.add_argument("--character_id", required=True)
    p.add_argument("--base_mesh", required=True,
                   help="FBX with mesh + armature in T-pose")
    p.add_argument("--anim_dir", required=True,
                   help="Folder containing one FBX per anim (file stem = anim name)")
    p.add_argument("--out_dir", required=True)
    p.add_argument("--frames_per_anim", type=int, default=12)
    p.add_argument("--cell", type=int, default=256)
    p.add_argument("--frame_rate", type=int, default=12)
    p.add_argument("--keep_raw", action="store_true",
                   help="Keep <out_dir>/_raw frames after atlas build")
    p.add_argument("--strip_root_motion", action="store_true", default=True)
    p.add_argument("--loop_anims", default="",
                   help="Comma-separated anim names that loop. Empty = builtin defaults")
    return p.parse_args(argv)


# =====================================================================
#                              SCENE SETUP
# =====================================================================
def clean_scene() -> None:
    """Wipe all data-blocks before importing fresh."""
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete(use_global=False)
    for collection in (
        bpy.data.meshes, bpy.data.materials, bpy.data.armatures,
        bpy.data.actions, bpy.data.images, bpy.data.lights, bpy.data.cameras,
        bpy.data.objects,
    ):
        for d in list(collection):
            try:
                collection.remove(d, do_unlink=True)
            except RuntimeError:
                pass


def import_fbx_with_anim(filepath: str) -> Tuple[object, object]:
    """Import FBX, return (armature, mesh) of the imported character."""
    if not os.path.isfile(filepath):
        raise FileNotFoundError(f"FBX not found: {filepath}")
    bpy.ops.import_scene.fbx(
        filepath=filepath,
        automatic_bone_orientation=True,
        anim_offset=0.0,
    )
    armature = next(
        (o for o in bpy.context.selected_objects if o.type == "ARMATURE"),
        None,
    )
    if armature is None:
        raise RuntimeError(f"No armature found in {filepath}")
    mesh = next((c for c in armature.children if c.type == "MESH"), None)
    if mesh is None:
        raise RuntimeError(f"No mesh child of armature in {filepath}")
    return armature, mesh


def import_action_only(filepath: str, armature: object, anim_name: str) -> Optional[object]:
    """Import an FBX and steal its action onto the existing armature.

    Returns the action attached, or None if no action was found in the FBX.
    Cleans up the imported armature/mesh after stealing.
    """
    pre_objects = set(bpy.data.objects)
    pre_actions = set(bpy.data.actions)
    bpy.ops.import_scene.fbx(filepath=filepath, automatic_bone_orientation=True)
    new_actions = set(bpy.data.actions) - pre_actions
    new_objects = set(bpy.data.objects) - pre_objects

    action = None
    if new_actions:
        action = sorted(new_actions, key=lambda a: a.name)[0]
        action.name = anim_name
        if armature.animation_data is None:
            armature.animation_data_create()
        armature.animation_data.action = action

    # Cleanup the imported temp objects (keep the action only)
    for obj in new_objects:
        try:
            bpy.data.objects.remove(obj, do_unlink=True)
        except RuntimeError:
            pass

    return action


# =====================================================================
#                              SHADER
# =====================================================================
def apply_toon_shader(mesh: object) -> None:
    """Apply 3-stop cel + outline solidify + watercolor noise overlay.

    Builds node graph minimally — works on Blender 3.0+.
    """
    mat = bpy.data.materials.new(name=f"Toon_{mesh.name}")
    mat.use_nodes = True
    nt = mat.node_tree
    for n in list(nt.nodes):
        nt.nodes.remove(n)

    # Existing diffuse texture (if FBX brought one) → BSDF
    tex_image = None
    for s in mesh.material_slots:
        if s.material and s.material.use_nodes:
            for node in s.material.node_tree.nodes:
                if node.type == "TEX_IMAGE" and node.image is not None:
                    tex_image = node.image
                    break
            if tex_image is not None:
                break

    img_node = nt.nodes.new("ShaderNodeTexImage")
    if tex_image is not None:
        img_node.image = tex_image
    img_node.location = (-800, 200)

    diffuse = nt.nodes.new("ShaderNodeBsdfDiffuse")
    diffuse.location = (-600, 200)

    geom = nt.nodes.new("ShaderNodeNewGeometry")
    geom.location = (-800, 0)

    # ColorRamp on shading factor (geom.normal dot light) — approximate via diffuse.color
    ramp = nt.nodes.new("ShaderNodeValToRGB")
    ramp.color_ramp.interpolation = "CONSTANT"
    ramp.color_ramp.elements[0].position = RAMP_STOPS[0]
    ramp.color_ramp.elements[0].color = PALETTE["shadow"]
    ramp.color_ramp.elements[1].position = RAMP_STOPS[1]
    ramp.color_ramp.elements[1].color = PALETTE["midtone"]
    high = ramp.color_ramp.elements.new(RAMP_STOPS[2])
    high.color = PALETTE["highlight"]
    ramp.location = (-400, 200)

    # Multiply texture × ramp (so cel banding tints the diffuse texture)
    mult = nt.nodes.new("ShaderNodeMixRGB")
    mult.blend_type = "MULTIPLY"
    mult.inputs["Fac"].default_value = 1.0
    mult.location = (-200, 200)

    # Watercolor noise overlay
    noise = nt.nodes.new("ShaderNodeTexNoise")
    noise.inputs["Scale"].default_value = 8.0
    noise.location = (-400, -200)

    overlay = nt.nodes.new("ShaderNodeMixRGB")
    overlay.blend_type = "OVERLAY"
    overlay.inputs["Fac"].default_value = WATERCOLOR_NOISE_FAC
    overlay.location = (0, 0)

    emit = nt.nodes.new("ShaderNodeEmission")
    emit.inputs["Strength"].default_value = 1.0
    emit.location = (200, 0)

    out = nt.nodes.new("ShaderNodeOutputMaterial")
    out.location = (400, 0)

    links = nt.links
    links.new(img_node.outputs["Color"], mult.inputs["Color1"])
    links.new(ramp.outputs["Color"], mult.inputs["Color2"])
    links.new(mult.outputs["Color"], overlay.inputs["Color1"])
    links.new(noise.outputs["Color"], overlay.inputs["Color2"])
    links.new(overlay.outputs["Color"], emit.inputs["Color"])
    links.new(emit.outputs["Emission"], out.inputs["Surface"])

    # Replace existing materials
    while mesh.data.materials:
        mesh.data.materials.pop(index=0)
    mesh.data.materials.append(mat)

    # Outline modifier (Solidify flipped normals)
    sol = mesh.modifiers.new(name="Outline", type="SOLIDIFY")
    height = max(mesh.dimensions.z, 0.1)
    sol.thickness = -OUTLINE_THICKNESS_RATIO * height
    sol.use_flip_normals = True
    sol.material_offset = 1
    outline_mat = bpy.data.materials.new(name=f"Outline_{mesh.name}")
    outline_mat.use_nodes = True
    onodes = outline_mat.node_tree.nodes
    for n in list(onodes):
        onodes.remove(n)
    em = onodes.new("ShaderNodeEmission")
    em.inputs["Color"].default_value = PALETTE["outline"]
    em.inputs["Strength"].default_value = 1.0
    o_out = onodes.new("ShaderNodeOutputMaterial")
    outline_mat.node_tree.links.new(em.outputs["Emission"], o_out.inputs["Surface"])
    mesh.data.materials.append(outline_mat)


# =====================================================================
#                              CAMERA + LIGHTING
# =====================================================================
def setup_camera(ortho_scale: float = ORTHO_SCALE_DEFAULT) -> Tuple[object, object]:
    """Create camera + empty pivot at origin. Camera tilts down toward origin."""
    cam_data = bpy.data.cameras.new("OrthoCam")
    cam_data.type = "ORTHO"
    cam_data.ortho_scale = ortho_scale
    cam = bpy.data.objects.new("OrthoCam", cam_data)
    bpy.context.collection.objects.link(cam)

    pivot = bpy.data.objects.new("CamPivot", None)
    bpy.context.collection.objects.link(pivot)
    cam.parent = pivot

    cam.location = (0.0, -CAM_DISTANCE, CAM_DISTANCE * CAM_HEIGHT_RATIO)
    cam.rotation_euler = (math.radians(CAM_TILT_DEG), 0.0, 0.0)
    bpy.context.scene.camera = cam
    return pivot, cam


def setup_lighting() -> None:
    key = bpy.data.lights.new("Key", type="SUN")
    key.energy = 3.0
    key_obj = bpy.data.objects.new("Key", key)
    key_obj.rotation_euler = (math.radians(-30), 0.0, math.radians(45))
    bpy.context.collection.objects.link(key_obj)

    fill = bpy.data.lights.new("Fill", type="SUN")
    fill.energy = 1.0
    fill_obj = bpy.data.objects.new("Fill", fill)
    fill_obj.rotation_euler = (math.radians(-30), 0.0, math.radians(-90))
    bpy.context.collection.objects.link(fill_obj)

    if bpy.context.scene.world is None:
        bpy.context.scene.world = bpy.data.worlds.new("World")
    bpy.context.scene.world.color = WORLD_AMBIENT


def setup_render(resolution: int) -> None:
    s = bpy.context.scene
    # Prefer EEVEE Next on Blender 4.2+, fall back to classic on 3.x
    try:
        s.render.engine = "BLENDER_EEVEE_NEXT"
    except TypeError:
        s.render.engine = "BLENDER_EEVEE"
    s.render.resolution_x = resolution
    s.render.resolution_y = resolution
    s.render.resolution_percentage = 100
    s.render.film_transparent = True
    s.render.image_settings.file_format = "PNG"
    s.render.image_settings.color_mode = "RGBA"
    s.render.image_settings.compression = 0
    s.render.use_freestyle = False


# =====================================================================
#                              ANIMATION HANDLING
# =====================================================================
def strip_root_motion(action: object) -> None:
    """Zero X/Y translation on Hips bone (Mixamo names it 'mixamorig:Hips')."""
    hips_paths = (
        'pose.bones["mixamorig:Hips"].location',
        'pose.bones["Hips"].location',
        'pose.bones["root"].location',
    )
    for fcurve in list(action.fcurves):
        if fcurve.data_path in hips_paths and fcurve.array_index in (0, 1):
            for kp in fcurve.keyframe_points:
                kp.co[1] = 0.0
                kp.handle_left[1] = 0.0
                kp.handle_right[1] = 0.0


def discover_anim_fbx_files(anim_dir: str) -> List[Tuple[str, str]]:
    """List (anim_name, abs_path) for each .fbx in anim_dir, sorted by name."""
    if not os.path.isdir(anim_dir):
        raise NotADirectoryError(f"--anim_dir not found: {anim_dir}")
    out = []
    for fname in sorted(os.listdir(anim_dir)):
        if fname.lower().endswith(".fbx"):
            stem = os.path.splitext(fname)[0]
            out.append((stem, os.path.join(anim_dir, fname)))
    if not out:
        raise FileNotFoundError(f"No .fbx files in --anim_dir: {anim_dir}")
    return out


# =====================================================================
#                              BAKE LOOP
# =====================================================================
def render_one_anim(
    armature: object,
    pivot: object,
    action_name: str,
    frames_target: int,
    out_root: str,
) -> None:
    """Render `frames_target` × 4 directions for the active action."""
    action = bpy.data.actions.get(action_name)
    if action is None:
        raise RuntimeError(f"Action {action_name} not loaded")
    if armature.animation_data is None:
        armature.animation_data_create()
    armature.animation_data.action = action

    f_start = int(action.frame_range[0])
    f_end = int(action.frame_range[1])
    span = max(1, f_end - f_start)
    if frames_target < 2:
        frames_target = 2  # avoid zero-divide
    step = span / float(frames_target - 1)

    for dir_name in DIRECTION_ORDER:
        pivot.rotation_euler = (0.0, 0.0, DIRECTIONS_RAD[dir_name])
        for i in range(frames_target):
            frame = int(round(f_start + i * step))
            frame = min(max(f_start, frame), f_end)
            bpy.context.scene.frame_set(frame)
            out_path = os.path.join(out_root, action_name, dir_name, f"{i:02d}.png")
            os.makedirs(os.path.dirname(out_path), exist_ok=True)
            bpy.context.scene.render.filepath = out_path
            bpy.ops.render.render(write_still=True)


# =====================================================================
#                              MAIN
# =====================================================================
def main() -> int:
    if not BLENDER_AVAILABLE:
        print("ERROR: This script must run inside Blender (bpy not importable).",
              file=sys.stderr)
        print("Usage: blender --background --python render_character.py -- <args>",
              file=sys.stderr)
        return 2

    args = parse_args()

    # Resolve loop overrides
    loop_overrides = None
    if args.loop_anims.strip():
        loop_overrides = tuple(s.strip() for s in args.loop_anims.split(",") if s.strip())

    out_dir = os.path.abspath(args.out_dir)
    raw_dir = os.path.join(out_dir, "_raw")
    os.makedirs(out_dir, exist_ok=True)
    os.makedirs(raw_dir, exist_ok=True)

    print(f"=== render_character: {args.character_id} ===")
    print(f"  base_mesh        = {args.base_mesh}")
    print(f"  anim_dir         = {args.anim_dir}")
    print(f"  out_dir          = {out_dir}")
    print(f"  frames_per_anim  = {args.frames_per_anim}")
    print(f"  cell             = {args.cell}")
    print(f"  frame_rate       = {args.frame_rate}")

    # 1. Setup scene
    clean_scene()
    armature, mesh = import_fbx_with_anim(args.base_mesh)
    apply_toon_shader(mesh)
    pivot, cam = setup_camera()
    setup_lighting()
    setup_render(args.cell)

    # Center mesh at world origin (Mixamo pivot ≈ feet, may need centering on Y)
    armature.location = (0.0, 0.0, 0.0)

    # 2. Discover anims
    anim_files = discover_anim_fbx_files(args.anim_dir)
    print(f"  found {len(anim_files)} anim FBX: {[n for n, _ in anim_files]}")

    # 3. Per anim — import action + render 4 dir × N frame
    rendered_anims: List[str] = []
    for anim_name, fbx_path in anim_files:
        print(f"  --> {anim_name} ({fbx_path})")
        action = import_action_only(fbx_path, armature, anim_name)
        if action is None:
            print(f"     SKIP: no action in FBX")
            continue
        if args.strip_root_motion:
            strip_root_motion(action)
        render_one_anim(armature, pivot, anim_name, args.frames_per_anim, raw_dir)
        rendered_anims.append(anim_name)

    if not rendered_anims:
        print("ERROR: No anims rendered. Check FBX files contain actions.",
              file=sys.stderr)
        return 3

    # 4. Pack atlas + write metadata
    meta = frame_packer.pack_layout(
        character_id=args.character_id,
        anim_names=rendered_anims,
        directions=DIRECTION_ORDER,
        frames_per_anim=args.frames_per_anim,
        cell=args.cell,
        frame_rate=args.frame_rate,
        loop_overrides=loop_overrides,
    )
    atlas_path = os.path.join(out_dir, "sprite_atlas.png")
    metadata_path = os.path.join(out_dir, "frame_metadata.json")
    frame_packer.build_atlas_from_renders(meta, raw_dir, atlas_path, metadata_path)

    print(f"DONE: {atlas_path}")
    print(f"      {metadata_path}")
    print(f"      anim_count={len(rendered_anims)} cells={len(rendered_anims)*4*args.frames_per_anim}"
          f" atlas={meta.atlasWidth}x{meta.atlasHeight}")

    if not args.keep_raw:
        shutil.rmtree(raw_dir, ignore_errors=True)

    return 0


if __name__ == "__main__":
    try:
        sys.exit(main())
    except Exception:
        traceback.print_exc()
        sys.exit(1)
