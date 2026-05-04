# Blender sprite renderer

Headless Blender pipeline that bakes a rigged 3D character (FBX from Mixamo /
Meshy) into a 4-direction × N-frame sprite atlas + JSON metadata, ready for the
Unity `BakedSpriteCharacterImporter`.

This is **Stage 4** of [`Documentation/pipelines/PNG_TO_3D_TO_SPRITE_PIPELINE.md`](../../Documentation/pipelines/PNG_TO_3D_TO_SPRITE_PIPELINE.md).
See that doc for the end-to-end pipeline, decision matrix vs 2D puppet, style
preservation strategy, and troubleshooting.

## Files

| File | Purpose |
|---|---|
| `render_character.py` | Main script — runs inside Blender (`blender --background --python …`) |
| `frame_packer.py` | Pure-Python atlas packer + metadata writer (testable without Blender) |
| `tests/test_frame_packer.py` | Unit tests for packer (vanilla `unittest`) |

## Prerequisites

- Blender **4.2 LTS** preferred (EEVEE Next). 3.x works (falls back to classic Eevee).
- Python 3.10+ inside Blender (default for 4.x).
- `Pillow` available to Blender's bundled Python:
  ```bash
  /path/to/blender/python/bin/python3.* -m ensurepip
  /path/to/blender/python/bin/python3.* -m pip install Pillow
  ```
  On Linux apt Blender, the bundled python typically lives in
  `/snap/blender/current/4.2/python/bin/` or similar. If Blender can't find
  `PIL`, run `bpy.utils.execfile(...)` will fail at the atlas-pack step —
  fix by installing Pillow into the same Python.

## Required input layout

Two paths, both produced by the user (Stage 2-3 in the pipeline doc):

```
~/work/<character_id>/
├── base.fbx              # T-pose mesh + armature (Mixamo "With Skin" recommended)
└── anim/
    ├── idle.fbx          # File stem becomes anim name in metadata + Unity AnimatorController
    ├── walk.fbx
    ├── run.fbx
    ├── attack.fbx
    ├── hit.fbx
    ├── dead.fbx
    ├── jump.fbx
    ├── victory.fbx
    ├── cast.fbx
    └── meditation.fbx
```

**FBX export checklist (Mixamo download settings):**
- Format: **FBX Binary (.fbx)**
- Skin: **With Skin** for `base.fbx`. Per-anim FBX may use **Without Skin** (smaller).
- Frame rate: **30 fps**
- Keyframe Reduction: **None**
- "In Place" option: **OFF** — script strips root motion itself (see `--strip_root_motion`)

**Mesh requirements (verified pre-Mixamo):**
- Single mesh object (`Ctrl+J` to join).
- Polycount < 10k tris (Mixamo cap — Decimate in Blender if Tripo3D output is over).
- T-pose, Y-up, all transforms applied (`Ctrl+A` → All Transforms).
- No embedded textures inside the FBX (Mixamo doesn't need them; strip before upload).

## Running the bake

```bash
blender --background --python tools/blender_sprite_render/render_character.py -- \
    --character_id player \
    --base_mesh ~/work/player/base.fbx \
    --anim_dir ~/work/player/anim \
    --out_dir Assets/_Project/Art/Characters/player/atlas \
    --frames_per_anim 12 \
    --cell 128 \
    --frame_rate 12
```

Arguments:

| Flag | Default | Description |
|---|---|---|
| `--character_id` | (required) | id baked into JSON; matches Unity importer downstream |
| `--base_mesh`    | (required) | T-pose FBX with mesh + armature |
| `--anim_dir`     | (required) | Folder of one FBX per anim (file stem = anim name) |
| `--out_dir`      | (required) | Output dir for `sprite_atlas.png` + `frame_metadata.json` |
| `--frames_per_anim` | `12` | N frames sampled uniformly per anim/direction |
| `--cell`         | `256` | Square cell px size — drop to `128` to fit 480 cells in a 4096 atlas |
| `--frame_rate`   | `12` | Playback fps recorded in metadata |
| `--keep_raw`     | off  | Keep `<out_dir>/_raw/` scratch frames after atlas build (debug) |
| `--strip_root_motion` | on | Zero out Hips X/Y translation curves |
| `--loop_anims`   | (defaults) | Comma-separated anims that loop. Empty = `idle,walk,run,meditation,victory` |

Output:

```
Assets/_Project/Art/Characters/<id>/atlas/
├── sprite_atlas.png       # PoT RGBA atlas — Unity TextureImporter slices via metadata
└── frame_metadata.json    # see frame_packer.py §SCHEMA
```

## Atlas size math

Atlas is square, smallest Power-of-Two fitting all cells:

| Anims | Frames/anim | Cell | Total cells | Atlas |
|---|---|---|---|---|
| 5  | 12 | 256 | 240 | 4096² |
| 10 | 12 | 256 | 480 | 8192² |
| 10 | 12 | 128 | 480 | 4096² ← recommended for mobile |
| 10 | 8  | 128 | 320 | 4096² |

For Android target: prefer `--cell 128` to keep atlas ≤ 4096² (Mali-G76 max
guaranteed texture). Sprites scale up to ~512px world on screen; 128² source
× point filter still reads as DST-style chunky pixels.

## Unit tests

```bash
python3 tools/blender_sprite_render/tests/test_frame_packer.py
# or
python3 -m pytest tools/blender_sprite_render/tests/ -v
```

Tests cover packing math, schema field names (must match Unity DTO), JSON
round-trip, and the loop-flag heuristic.

## Common failures

| Symptom | Fix |
|---|---|
| `No armature found in <fbx>` | Mesh exported without skeleton — re-export with skin |
| `No action in FBX … SKIP` | Anim FBX lacks animation data — re-download from Mixamo with anim selected |
| `EEVEE Next not available` (silent) | Blender < 4.2 — falls back to BLENDER_EEVEE automatically |
| Outline missing | `solidify.thickness = -OUTLINE_THICKNESS_RATIO * mesh.dimensions.z` — check mesh has non-zero height |
| Atlas size 8192² blows mobile budget | Pass `--cell 128` (halves total area) |
| Wrong sprite name in Unity | `frame_metadata.json` `anims[].name` must match Mixamo file stem (case-sensitive) |
| Render is white / no transparency | Ensure `--cell` ≥ 64 — too small breaks PIL paste alpha |

## Style tuning

Constants at top of `render_character.py`:

| Constant | Default | Effect |
|---|---|---|
| `PALETTE` | sepia + cream + jade-shifted | Match `Documentation/art/ART_STYLE.md` palette lock |
| `OUTLINE_THICKNESS_RATIO` | `0.012` (1.2% body height) | Thicker → chunkier DST line |
| `WATERCOLOR_NOISE_FAC` | `0.15` | 0 = clean cel, 0.30 = heavy paper grain |
| `RAMP_STOPS` | `(0.0, 0.45, 0.85)` | 3-stop hard cel; move stops for harder/softer banding |
| `WORLD_AMBIENT` | `(0.50, 0.45, 0.40)` sepia | Tint applied in shadow regions |
| `CAM_TILT_DEG` | `60` | Top-down 2.5D angle. Lower = more side view |
| `ORTHO_SCALE_DEFAULT` | `2.5` | Frame width in world units. Larger = smaller sprite in cell |

After a re-bake to tune outline / palette: pass `--keep_raw` then diff the
`_raw/idle/S/00.png` between runs to see exactly what the change did before
re-packing the full 480-cell atlas.

## See also

- [`Documentation/pipelines/PNG_TO_3D_TO_SPRITE_PIPELINE.md`](../../Documentation/pipelines/PNG_TO_3D_TO_SPRITE_PIPELINE.md) — end-to-end pipeline doc (decision matrix, cost, Stage 1-3 user steps)
- [`Documentation/art/AI_PROMPTS.md`](../../Documentation/art/AI_PROMPTS.md) — PNG concept generation (Stage 1)
- [`Documentation/art/ART_STYLE.md`](../../Documentation/art/ART_STYLE.md) — palette + outline lock
- `Assets/_Project/Editor/BakedSpriteCharacterImporter.cs` — Unity-side importer (Stage 5)
