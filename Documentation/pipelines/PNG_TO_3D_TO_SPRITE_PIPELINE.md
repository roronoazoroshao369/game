---
name: png-to-3d-to-sprite-pipeline
audience: ai-agent + human
status: research / proof-of-concept (2026-05) — chưa ship character nào qua pipeline này
scope: 1 character → 3D mesh → rigged → 10 anim → sprite atlas + AnimatorController Unity
depends-on:
  - Documentation/art/AI_PROMPTS.md          # PNG concept input phải pass §6 acceptance
  - Documentation/art/ART_STYLE.md           # palette + outline thickness target
  - Documentation/pipelines/PUPPET_PIPELINE.md  # pipeline 2D hiện tại — quyết định dùng cái nào
  - ARCHITECTURE.md                          # §1 folder layout cho output
prerequisites:
  - Blender 4.2+ (EEVEE Next)
  - Mixamo account (Adobe ID, free)
  - Tripo3D account (Basic plan free, 300 credits/tháng) HOẶC Meshy account (Free, 100 credits/tháng)
  - Unity 6 LTS 6000.4.4f1 + 2D Feature Set (đã có sẵn repo)
---

# PNG → 3D → Mixamo → Blender Toon → Unity Sprite Atlas

> **Pipeline song song với [`PUPPET_PIPELINE.md`](PUPPET_PIPELINE.md) — không thay thế nó.**
> Pipeline 2D puppet hiện tại (30-part atomic, procedural rig) vẫn là default cho mob phụ + resource.
> Pipeline 3D-mediated này dùng cho character cần animation library lớn (10+ clip cùng character) — Player + 3 hero mob (Wolf, FoxSpirit, Boss).
> Output là **2D sprite atlas** giữ nguyên render path 2D Unity hiện tại — KHÔNG đổi engine, KHÔNG thêm 3D dependencies vào APK.

---

## §1 TL;DR

```
┌────────────┐  ┌──────────────┐  ┌────────────┐  ┌─────────────────┐  ┌────────────────┐
│ User: PNG  │→ │ Tripo3D /    │→ │ Mixamo     │→ │ Blender Python  │→ │ Unity importer │
│ concept    │  │ Meshy        │  │ auto-rig + │  │ toon shader +   │  │ atlas + Animator│
│ (1 image)  │  │ → 3D mesh    │  │ 10 anim FBX│  │ ortho 4-dir bake│  │ Controller plug│
└────────────┘  └──────────────┘  └────────────┘  └─────────────────┘  └────────────────┘
                  ~10 min            ~5 min             ~30 min              ~1 min CI
                  (free tier)        (FREE Adobe)       (headless)           (auto)
```

**Output per character:**
- 1 sprite atlas PNG (~2048×2048 hoặc 4096×4096 PoT)
- 1 frame_metadata.json (slice rect + anim mapping)
- 1 AnimatorController.asset (10 state, default = Idle)
- ~30-60 phút per character end-to-end (lần đầu ~3 giờ vì set up scripts)

**480 frame target:** 12 frame × 4 direction × 10 anim = 480 sprite cell per character.

---

## §2 Khi nào dùng pipeline này

### Quyết định matrix

| Asset | Pipeline | Lý do |
|---|---|---|
| **Player** | 3D-mediated (this doc) | Focal point, cần idle/walk/run/attack/hit/dead/jump/victory + meditation/cast — animation library lớn nhất, ROI cao. |
| **Wolf, FoxSpirit, Boss** (3 hero mob) | 3D-mediated (this doc) hoặc 2D puppet | Combat-focused. 3D nếu cần multi-attack pattern, 2D nếu đủ idle/walk/attack/hit/death. |
| **Rabbit, Crow, Bat, Boar, Snake, DeerSpirit** | 2D puppet ([`PUPPET_PIPELINE.md`](PUPPET_PIPELINE.md)) | Background mob, AI-gen 1 PNG đủ. 480 frame quá over-budget. |
| **NPC humanoid (Vendor, Companion)** | 2D puppet | Stationary hoặc slow-walk, không cần combat anim. |
| **Tree, Rock, Resource node** | Procedural sway/wiggle | Sub-second feedback. |

**Cost rule of thumb:** 3D pipeline ROI khi character dùng ≥ 6 animation clip. Dưới 6 clip → 2D puppet rẻ hơn (cả compute + iteration).

### Khi KHÔNG dùng pipeline này

- ✗ Character ≤ 5 anim clip (idle/walk/attack/hit/death) → 2D puppet
- ✗ Cần real-time customization (gear swap, color variant) → 2D puppet (mỗi part 1 sprite)
- ✗ APK budget < 100 MB và đã có ≥ 5 character → 2D puppet (sprite atlas 4K nặng ~2-4 MB/character)
- ✗ Iteration speed > polish (gameplay prototype phase) → 2D puppet (re-bake 30 phút vs 5 phút)

---

## §3 Pipeline 7 bước

```
[1] User: gen PNG concept (1 image / character) — § 4
        ↓ upload cho Devin
[2] Devin: image → 3D mesh (Tripo3D / Meshy free tier) — § 5
        ↓ download GLB/FBX
[3] Devin: 3D mesh → Mixamo auto-rig + 10 anim — § 6
        ↓ download 10 FBX (one per anim)
[4] Devin: Blender Python script — § 7
    - Toon shader match style PNG (cel + outline + watercolor)
    - Render orthographic camera 4 direction (E/W/N/S)
    - 12 frame per anim × 4 direction × 10 anim = 480 frame
    - Export: sprite_atlas.png + frame_metadata.json
        ↓ commit vào Assets/_Project/Art/Characters/{id}/atlas/
[5] Devin: Unity importer — § 8
    - Auto-slice atlas (TextureImporter.spritesheet)
    - Auto-create AnimatorController với 10 state
    - Plug vào CharacterController hiện có
        ↓ open Unity → Bootstrap Scene
[6] User: Play Unity, review smooth + style — § 9
        ↓ feedback (specific anim / shader knob)
[7] Devin: tune shader / tune anim mapping → loop bước 4 hoặc bước 7 — § 9
```

---

## §4 Stage 1 — PNG concept input

### Style requirements (LOCKED)

Game đã LOCK style "Chibi Wuxia × Soft-DST" — xem [`Documentation/art/AI_PROMPTS.md`](../art/AI_PROMPTS.md) §1 (8 luật) + §3 (Player master prompt v2 PASS 10/10).

**Input PNG cho Stage 2 PHẢI:**

| Yêu cầu | Lý do |
|---|---|
| Full-body T-pose hoặc A-pose | Mixamo auto-rig cần T-pose; Tripo3D suy mesh tốt hơn từ T-pose |
| 1024×1024 hoặc 2048×2048, transparent BG | Tripo3D / Meshy auto-detect alpha, kết quả cleaner |
| 3 view (front + side + back) khuyến nghị | Multi-view mode của Tripo3D / Meshy cho mesh chính xác hơn ~30% so với single-view |
| Chibi proportion 3.5–4 head-tall | Match anatomy spec §2 trong AI_PROMPTS.md |
| Outline rõ, palette muddied (sat ≤ 30%) | Toon shader Blender sẽ extract palette từ mesh texture — input càng "stylized" càng giữ được style |

### Workflow gen PNG

Reuse existing prompt workflow:
1. [`Documentation/art/AI_PROMPTS.md`](../art/AI_PROMPTS.md) §3 — master prompt + acceptance test
2. [`Documentation/art/PLAYER_FULL_ASSET_SOURCE_PROMPT.md`](../art/PLAYER_FULL_ASSET_SOURCE_PROMPT.md) — full-body source board (1 PNG có cả 4 angle T-pose)
3. Save vào `Documentation/assets/concept/{character_id}_T_pose.png`

> Nếu input chưa pass acceptance test → re-gen, đừng vào Stage 2. Toon shader Blender không "rescue" được mesh sai proportion.

---

## §5 Stage 2 — Image → 3D mesh

### Tripo3D vs Meshy free tier (so sánh)

| Tiêu chí | Tripo3D Basic ($0) | Meshy Free ($0) |
|---|---|---|
| Credits/tháng | 300 | 100 |
| Concurrent tasks | 1 | 1 (low priority queue) |
| Image-to-3D credits/lần | ~30-50 (single view) | 5–30 (Meshy-5 hay Meshy-6) |
| Multi-view (front+side+back) | ✓ Available | ✓ Available |
| Output formats | GLB, FBX, OBJ, USDZ, STL, 3MF | GLB, FBX, OBJ, USDZ |
| Polycount max | Free tier không control được | 10,000 (free tier hard cap) |
| Auto-rig built-in | ✗ (Pro plan only — "Export with skeleton") | ✗ free tier (5 credits/call để rig) |
| Animation library | ✗ free | ✓ 20 preset animation (free tier) |
| License free tier | CC BY 4.0 (public, attribution required) | CC BY 4.0 (public, attribution required) |
| Texture quality | Standard 1024² | Standard 1024² |
| Estimated free models/month | ~6-10 character | ~3-5 character |

### Recommendation

**Tripo3D Basic ưu tiên** cho game này vì:
1. 300 credits/tháng đủ gen ~6-10 character (project chỉ cần ~12 character total — xem `AI_PROMPTS.md` §7).
2. Multi-view mode quality tốt hơn cho chibi proportion.
3. **Vẫn dùng Mixamo external** cho rigging (FREE, không tính credits) — không cần Pro plan của Tripo.

**Meshy fallback** nếu:
- Tripo3D queue quá lâu (free tier low priority).
- Cần animation preset có sẵn để skip Mixamo (animation library 20 preset của Meshy free tier).

### Bước thực hiện (Tripo3D web flow)

1. Login `studio.tripo3d.ai` → New Task → Image-to-3D.
2. Upload `Documentation/assets/concept/{id}_T_pose.png` (single view) hoặc 3 view (multi-view tab).
3. Settings:
   - Quality: Standard (free tier không có Ultra)
   - Texture: ON
   - Style: NONE (tự áp toon shader sau, không dùng "cartoon" preset của Tripo vì sẽ conflict với palette)
4. Wait ~5-10 phút (free tier queue).
5. Download → chọn format **FBX** (Mixamo support FBX tốt nhất). Save vào `~/work/{id}/raw_mesh.fbx`.

> **KHÔNG commit raw_mesh.fbx vào repo** — chỉ commit final atlas PNG + metadata. Raw mesh giữ ở `Documentation/assets/3d_source/` (gitignored) hoặc external storage.

### Common failures

| Lỗi | Fix |
|---|---|
| Mesh có 2 mặt (front+back overlap) | Re-gen với multi-view 3 angle, không single-view |
| Limb floating (tay/chân không attach) | Input PNG phải có outline kín, không có gap ở shoulder/hip |
| Texture vẽ "tu sĩ" trên mesh sai chỗ (vd kimono trên đùi) | Tripo3D không hiểu wuxia ornament — xử lý texture re-paint trong Blender §7.3 |
| Polycount > 10k (Tripo) hoặc đúng 10k cap (Meshy) | Decimate trong Blender (Mesh → Decimate → 0.5) trước khi upload Mixamo |

---

## §6 Stage 3 — Mixamo auto-rig + 10 anim

Mixamo (Adobe, FREE) — auto-rig 65-bone humanoid skeleton + ~2500 animation library.

### Upload character

1. Login `mixamo.com` (Adobe ID free).
2. **Upload Character** → drag `raw_mesh.fbx` vào dialog.
3. Auto-rigger UI: place 7 marker chuẩn (chin, wrists, elbows, knees, groin). Mixamo auto-suggest, chỉ cần adjust nếu sai > 5cm.
4. Skeleton LOD: chọn **Standard Skeleton** (65 bone, no fingers) cho chibi character. Fingers tốn polycount, không cần cho ortho render 1024px.
5. Apply → wait ~30s. Mixamo trả về preview T-pose có rig.

### Yêu cầu mesh (Mixamo upload spec)

| Yêu cầu | Lý do |
|---|---|
| **Single mesh** (1 object) | Mixamo auto-rig fail nếu mesh là multi-part. Trong Blender: `Ctrl+J` để join all. |
| **T-pose hoặc A-pose** | Auto-rig dùng T-pose làm reference. A-pose cần manual marker adjust. |
| **Polycount < 10k tris** | Mixamo upload limit ~10MB FBX. Decimate xuống 5k-8k tris cho chibi. |
| **No textures embedded** | Textures làm FBX nặng, Mixamo không cần (rig trên geometry only). Strip textures trước upload. |
| **Y-up axis** | Default Blender export. Nếu Z-up → "skeleton mapping failed". |
| **Apply transforms** | Scale/rotation phải = 1.0 trước export. Trong Blender: `Ctrl+A` → All Transforms. |

### Download 10 animation

Animation list mục tiêu (10 anim, đúng yêu cầu user):

| Slot | Mixamo search query | Note |
|---|---|---|
| **Idle** | `idle` (chọn "Idle" cơ bản, không "Bored Idle") | Loop, ~3s |
| **Walk** | `walking` | Loop, không cần "in place" — sẽ strip root motion sau |
| **Run** | `running` | Loop |
| **Attack** | `sword and shield slash` hoặc `mma kick` | One-shot ~1s |
| **Hit** | `hit reaction` (chọn "Standing React Small Forward") | One-shot ~0.5s |
| **Dead** | `dying` (chọn "Falling Back Death") | One-shot ~2s |
| **Jump** | `jump` (chọn "Jump" cơ bản) | One-shot ~1s |
| **Victory** | `victory` (chọn "Victory Idle" hoặc "Cheering") | Loop hoặc one-shot |
| **Cast** | `casting spell` | One-shot ~1.5s — cho Tụ Linh Quyết / Kiếm Khí Trảm |
| **Meditation** | `sitting meditation` (Yoga preview) | Loop — khớp với MeditationAction phím M |

> **Không dùng "in place"** option — Mixamo strip translation nhưng giữ root rotation. Blender Python sẽ tự strip root motion khi bake (§7.5).

**Download settings cho mỗi anim:**
- Format: **FBX Binary (.fbx)**
- Skin: **With Skin** (lần đầu mỗi character) — có mesh + bone + 1 anim. Lần 2+ chỉ cần "Without Skin" (rẻ hơn ~10x size).
- Frame rate: **30 fps**
- Keyframe Reduction: **None** (giữ full keyframe để Blender bake mượt)

Output: `~/work/{id}/anim/{anim_name}.fbx` — 10 file.

### Cost & time

- Upload + rig: ~5 phút (1 lần per character).
- Tải 10 anim: ~3 phút (search + click download).
- Total stage 3: ~8 phút per character. Mixamo HOÀN TOÀN FREE.

---

## §7 Stage 4 — Blender Python: toon shader + ortho 4-direction render

### 7.1 Cấu trúc script

Script đặt tại `tools/blender_sprite_render/render_character.py` (sẽ thêm vào repo trong PR follow-up khi pilot character pass review).

```bash
# Headless invocation
blender --background --python tools/blender_sprite_render/render_character.py -- \
    --character_id player \
    --base_mesh ~/work/player/anim/idle.fbx \
    --anim_dir ~/work/player/anim/ \
    --out_dir Assets/_Project/Art/Characters/player/atlas/ \
    --frames_per_anim 12 \
    --resolution 256
```

Reference repos (apache/MIT licensed, copy idea — không vendor):
- [`dbarton-uk/blender-sprite-render`](https://github.com/dbarton-uk/blender-sprite-render) — 4-dir batch, auto-crop PoT, JSON metadata sidecar (Dec 2025)
- [`pekkavaa/SpriteBatchRender`](https://github.com/pekkavaa/SpriteBatchRender) — Blender plugin classic (8-dir, anim cycle)
- [`yasendinkov.com/posts/sprite-generator/`](https://yasendinkov.com/posts/sprite-generator/) — orbital camera, 8-angle Z rotation

### 7.2 Skeleton script (key functions)

```python
# tools/blender_sprite_render/render_character.py
import bpy, os, math, json, sys, argparse

# ---- Args (Blender truyền qua "--") ----
def parse_args():
    argv = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
    p = argparse.ArgumentParser()
    p.add_argument("--character_id", required=True)
    p.add_argument("--base_mesh", required=True)      # 1 FBX có mesh + skeleton
    p.add_argument("--anim_dir", required=True)       # folder 10 FBX anim
    p.add_argument("--out_dir", required=True)
    p.add_argument("--frames_per_anim", type=int, default=12)
    p.add_argument("--resolution", type=int, default=256)  # cell size px
    return p.parse_args(argv)

# ---- 1. Clean scene ----
def clean_scene():
    bpy.ops.object.select_all(action='SELECT')
    bpy.ops.object.delete(use_global=False)
    for block in (bpy.data.meshes, bpy.data.materials, bpy.data.armatures, bpy.data.actions):
        for d in list(block):
            block.remove(d)

# ---- 2. Import base mesh ----
def import_fbx(path):
    bpy.ops.import_scene.fbx(filepath=path, automatic_bone_orientation=True)
    armature = next(o for o in bpy.context.selected_objects if o.type == 'ARMATURE')
    mesh = next(o for o in armature.children if o.type == 'MESH')
    return armature, mesh

# ---- 3. Toon shader (cel + outline + watercolor) ----
def apply_toon_shader(mesh, palette_target):
    """
    palette_target = dict from Documentation/art/ART_STYLE.md §2:
      { 'outline': '#1a1408', 'cinnabar': '#8b3a3a', 'jade': '#7a9078', 'cream': '#e8d8b8' }
    Build node tree:
      Diffuse → ColorRamp (constant interp, 3 stop @ 0.0 / 0.45 / 0.85) → Output
      Solidify modifier flipped normals @ outline thickness ≈ 1% mesh extent
      Mix watercolor noise overlay @ 0.15 factor
    """
    mat = bpy.data.materials.new(name=f"Toon_{mesh.name}")
    mat.use_nodes = True
    nt = mat.node_tree
    nt.nodes.clear()
    # Diffuse from existing texture
    img = nt.nodes.new('ShaderNodeTexImage')
    diff = nt.nodes.new('ShaderNodeBsdfDiffuse')
    ramp = nt.nodes.new('ShaderNodeValToRGB')
    ramp.color_ramp.interpolation = 'CONSTANT'
    # 3 stop: shadow / mid / highlight
    ramp.color_ramp.elements[0].position = 0.0
    ramp.color_ramp.elements[1].position = 0.45
    ramp.color_ramp.elements.new(0.85)
    # Watercolor noise
    noise = nt.nodes.new('ShaderNodeTexNoise')
    noise.inputs['Scale'].default_value = 8.0
    mix = nt.nodes.new('ShaderNodeMixRGB')
    mix.blend_type = 'OVERLAY'; mix.inputs['Fac'].default_value = 0.15
    out = nt.nodes.new('ShaderNodeOutputMaterial')
    # Wire: img → diff → BSDF dot N → ramp → mix(overlay noise) → out
    # ... (xem dbarton-uk repo cho full node graph)

    # Solidify outline
    sol = mesh.modifiers.new('Outline', 'SOLIDIFY')
    sol.thickness = -0.01 * mesh.dimensions.z   # ≈ 1% body height
    sol.use_flip_normals = True
    sol.material_offset = 1
    # Outline material (emissive black, single-sided culling)
    outline_mat = bpy.data.materials.new("Outline")
    outline_mat.use_nodes = True
    # ... emit shader với color = palette_target['outline']
    mesh.data.materials.append(outline_mat)

    if mesh.data.materials:
        mesh.data.materials[0] = mat
    return mat

# ---- 4. Setup orthographic camera + lighting ----
DIRECTIONS = {
    "S": math.radians(0),    # facing +Y → camera on -Y axis (DST south = "down")
    "E": math.radians(90),
    "N": math.radians(180),
    "W": math.radians(270),
}

def setup_camera(distance=4.0, ortho_scale=2.5):
    cam_data = bpy.data.cameras.new("OrthoCam")
    cam_data.type = 'ORTHO'
    cam_data.ortho_scale = ortho_scale
    cam = bpy.data.objects.new("OrthoCam", cam_data)
    bpy.context.collection.objects.link(cam)
    # Empty parent ở origin để rotate
    pivot = bpy.data.objects.new("CamPivot", None)
    bpy.context.collection.objects.link(pivot)
    cam.parent = pivot
    cam.location = (0, -distance, distance * 0.5)  # slight high angle ~25° (top-down feel)
    cam.rotation_euler = (math.radians(60), 0, 0)
    bpy.context.scene.camera = cam
    return pivot, cam

def setup_lighting():
    # Key light + fill — flat lighting cho cel ramp
    key = bpy.data.lights.new("Key", type='SUN')
    key.energy = 3.0
    key.angle = 0
    key_obj = bpy.data.objects.new("Key", key)
    key_obj.rotation_euler = (math.radians(-30), 0, math.radians(45))
    bpy.context.collection.objects.link(key_obj)
    # Ambient cao để shadow không quá deep
    bpy.context.scene.world.color = (0.5, 0.45, 0.4)  # sepia tint

# ---- 5. Render render settings ----
def setup_render(resolution):
    s = bpy.context.scene
    s.render.engine = 'BLENDER_EEVEE_NEXT'   # Blender 4.2+
    s.render.resolution_x = resolution
    s.render.resolution_y = resolution
    s.render.film_transparent = True
    s.render.image_settings.file_format = 'PNG'
    s.render.image_settings.color_mode = 'RGBA'
    s.render.image_settings.compression = 0
    # Freestyle outline ON nếu muốn double-line (solidify + freestyle)
    s.render.use_freestyle = False  # mặc định off, bật nếu solidify không đủ chunky

# ---- 6. Bake animation → 12 frame x 4 dir x 10 anim ----
def bake_animation(armature, mesh, action_name, frames_target, pivot, out_dir):
    """
    1. Apply NLA action
    2. Frame range strip → uniform 12 sample
    3. Loop 4 direction → render PNG vào out_dir/{anim}/{dir}/{frame_idx}.png
    """
    action = bpy.data.actions.get(action_name)
    if not action:
        print(f"WARN: no action {action_name}"); return
    armature.animation_data.action = action

    f_start = int(action.frame_range[0])
    f_end = int(action.frame_range[1])
    step = max(1, (f_end - f_start) // (frames_target - 1))

    for dir_name, dir_rad in DIRECTIONS.items():
        pivot.rotation_euler = (0, 0, dir_rad)
        for i in range(frames_target):
            frame = min(f_start + i * step, f_end)
            bpy.context.scene.frame_set(frame)
            out_path = os.path.join(out_dir, "_raw", action_name, dir_name, f"{i:02d}.png")
            os.makedirs(os.path.dirname(out_path), exist_ok=True)
            bpy.context.scene.render.filepath = out_path
            bpy.ops.render.render(write_still=True)

# ---- 7. Composite atlas + export metadata ----
def build_atlas(out_dir, anim_list, frames_target, cell):
    """
    Layout:
      atlas[anim_row][direction_col_block][frame_col]
      anim_row: 10
      4 direction × frames_target (12) = 48 col per row
      atlas size: 48*256 = 12288 wide, 10*256 = 2560 tall  → KHÔNG PoT
    Better layout: pack vertical 4 direction × 10 anim = 40 row, 12 frame col
      atlas size: 12*256 = 3072 wide, 40*256 = 10240 tall  → vẫn không PoT
    Pragmatic: 4096×4096 PoT, pack stright, leave padding.
    """
    from PIL import Image
    target = 4096
    atlas = Image.new("RGBA", (target, target), (0, 0, 0, 0))
    metadata = {"cell": cell, "frames_per_anim": frames_target, "anims": {}}
    cells_per_row = target // cell        # 16 cell @ 256
    cell_idx = 0
    for anim in anim_list:
        for dir_name in ("S", "E", "N", "W"):
            anim_dir_meta = []
            for f in range(frames_target):
                src = os.path.join(out_dir, "_raw", anim, dir_name, f"{f:02d}.png")
                if not os.path.exists(src): continue
                row = cell_idx // cells_per_row
                col = cell_idx % cells_per_row
                px, py = col * cell, row * cell
                atlas.paste(Image.open(src), (px, py))
                anim_dir_meta.append({"frame": f, "x": px, "y": py, "w": cell, "h": cell})
                cell_idx += 1
            metadata["anims"].setdefault(anim, {})[dir_name] = anim_dir_meta

    atlas.save(os.path.join(out_dir, "sprite_atlas.png"))
    with open(os.path.join(out_dir, "frame_metadata.json"), "w") as f:
        json.dump(metadata, f, indent=2)

# ---- 8. Main ----
def main():
    args = parse_args()
    clean_scene()
    armature, mesh = import_fbx(args.base_mesh)
    apply_toon_shader(mesh, palette_target={
        'outline': '#1a1408', 'cinnabar': '#8b3a3a',
        'jade': '#7a9078', 'cream': '#e8d8b8',
    })
    pivot, cam = setup_camera()
    setup_lighting()
    setup_render(args.resolution)

    # Import từng anim FBX, copy action vào current armature
    anim_list = []
    for anim_fbx in sorted(os.listdir(args.anim_dir)):
        if not anim_fbx.endswith(".fbx"): continue
        anim_name = os.path.splitext(anim_fbx)[0]
        anim_list.append(anim_name)
        # Import anim chỉ để copy action — sau đó cleanup
        # ... (xem dbarton-uk cho idiom; key: bpy.ops.import_scene.fbx with use_anim=True)
        bake_animation(armature, mesh, anim_name, args.frames_per_anim, pivot, args.out_dir)

    build_atlas(args.out_dir, anim_list, args.frames_per_anim, args.resolution)
    print(f"DONE: {len(anim_list)} anim × 4 dir × {args.frames_per_anim} frame = {len(anim_list)*4*args.frames_per_anim} cell")

if __name__ == "__main__":
    main()
```

> **Đây là skeleton, chưa run được end-to-end.** PR follow-up sẽ harden script + add unit test với 1 character pilot. Mục tiêu doc này là **document architecture + decision**, không phải ship script production-ready.

### 7.3 Style match: preserve "Chibi Wuxia × Soft-DST"

**Vấn đề:** Tripo3D / Meshy mesh texture không hiểu wuxia ornament (jade pendant, cloud sigil chest, gold sash). Output texture nhìn "generic 3D character", mất 60% style identity.

**Giải pháp ưu tiên (chọn 1 hoặc kết hợp):**

1. **Re-paint texture trong Blender** trước render (manual, ~30 phút/character):
   - Mở `mesh.fbx` trong Blender, chuyển sang Texture Paint mode.
   - Drop ornament PNG (jade pendant, sash bow) làm stencil texture.
   - Paint lên UV correct location (chest, waist).
   - Save updated texture vào `~/work/{id}/repaint_diffuse.png`, dùng cho shader §7.2.

2. **Decal modifier** (procedural, ~5 phút/character):
   - Add `Decal` modifier (Blender 4.2+) hoặc shrinkwrap plane với ornament texture.
   - Project decal lên mesh ở fixed UV coord (chest = UV (0.5, 0.7), waist = UV (0.55, 0.4)).
   - Render shader sẽ blend decal vào diffuse trước cel ramp.

3. **AI inpaint texture sau bake** (post-process, ~15 phút/character):
   - Chạy 480 frame qua bake.
   - Dùng Stable Diffusion / Krita inpaint để vẽ jade pendant lên frame Idle / Front (mấu chốt — chỉ cần 1 frame canon, các frame khác consistency tự handle qua puppet rig).
   - **Risk:** ornament jitter giữa frame nếu không lock seed.

**Recommended: Option 2 (decal)** — fastest + consistency cao nhất.

### 7.4 Outline calibration

`AI_PROMPTS.md` §1 luật 1: outline thickness 8–16px @ 1024 canvas (1–1.5% cạnh dài).

Script `apply_toon_shader` mặc định `solidify.thickness = -0.01 * mesh.dimensions.z` — tương đương ~1% body height. Sau bake, đo outline thực tế trên render:
- Pixel outline target: `0.01 × 256 = 2.56 px` cho cell 256, hoặc `0.01 × 1024 = 10 px` cho cell 1024.
- Nếu outline mỏng hơn (< 2 px @ cell 256) → tăng `solidify.thickness` lên `-0.015`.
- Nếu outline quá dày (> 5 px @ cell 256) → giảm xuống `-0.006`.

Thử 1 frame Idle/S trước, không bake full 480 frame.

### 7.5 Strip root motion

Mixamo "in place" option strip translation NHƯNG giữ root rotation/scale. Để render đứng yên giữa giữa cell, set armature root bone → animation curves trên `location` = constant.

```python
def strip_root_motion(armature, action_name):
    action = bpy.data.actions[action_name]
    for fcurve in action.fcurves:
        if fcurve.data_path == "pose.bones[\"mixamorig:Hips\"].location":
            for kp in fcurve.keyframe_points:
                kp.co[1] = 0.0
                kp.handle_left[1] = 0.0; kp.handle_right[1] = 0.0
```

Walk/Run anim: nếu user MUỐN visible step (sprite nhảy lên/xuống nhẹ), giữ Z translation, strip X+Y. Nếu muốn tile-locked grid movement (DST style), strip cả 3.

---

## §8 Stage 5 — Unity importer

### 8.1 Auto-slice atlas

Editor script `Assets/_Project/Editor/SpriteAtlasImporter.cs`:

```csharp
using UnityEngine;
using UnityEditor;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace WildernessCultivation.Editor
{
    public static class SpriteAtlasImporter
    {
        [MenuItem("Tools/Wilderness Cultivation/Import 3D-Baked Atlas")]
        public static void ImportAtlas()
        {
            string dir = EditorUtility.OpenFolderPanel(
                "Pick atlas folder (chứa sprite_atlas.png + frame_metadata.json)",
                "Assets/_Project/Art/Characters", "");
            if (string.IsNullOrEmpty(dir)) return;
            ImportFolder(dir);
        }

        public static void ImportFolder(string absDir)
        {
            string atlasPath = Path.Combine(absDir, "sprite_atlas.png");
            string metaPath = Path.Combine(absDir, "frame_metadata.json");
            string projectAtlas = ToAssetPath(atlasPath);
            // 1. Re-import atlas as Multiple sprite
            var ti = (TextureImporter)AssetImporter.GetAtPath(projectAtlas);
            ti.textureType = TextureImporterType.Sprite;
            ti.spriteImportMode = SpriteImportMode.Multiple;
            ti.filterMode = FilterMode.Point;          // pixel-art crispness
            ti.mipmapEnabled = false;
            ti.spritePixelsPerUnit = 100f;             // tunable per role

            // 2. Build SpriteMetaData[] from frame_metadata.json
            JObject meta = JObject.Parse(File.ReadAllText(metaPath));
            int cell = (int)meta["cell"];
            var sprites = new List<SpriteMetaData>();
            foreach (var anim in meta["anims"].Children<JProperty>())
            {
                string animName = anim.Name;
                foreach (var dir in anim.Value.Children<JProperty>())
                {
                    string dirName = dir.Name;          // "S","E","N","W"
                    int frameIdx = 0;
                    foreach (var frame in dir.Value)
                    {
                        sprites.Add(new SpriteMetaData
                        {
                            name = $"{animName}_{dirName}_{frameIdx:D2}",
                            rect = new Rect(
                                (float)frame["x"], (float)frame["y"],
                                (float)frame["w"], (float)frame["h"]),
                            alignment = (int)SpriteAlignment.Custom,
                            pivot = new Vector2(0.5f, 0.25f),  // foot pivot for 2.5D
                        });
                        frameIdx++;
                    }
                }
            }
            ti.spritesheet = sprites.ToArray();
            ti.SaveAndReimport();

            // 3. Generate AnimatorController
            BuildAnimatorController(absDir, meta, projectAtlas);
            AssetDatabase.Refresh();
        }

        static string ToAssetPath(string absPath)
        {
            string project = Application.dataPath; // .../Assets
            return "Assets" + absPath.Substring(project.Length).Replace('\\','/');
        }

        // ... BuildAnimatorController § 8.2
    }
}
```

### 8.2 Auto-create AnimatorController

```csharp
using UnityEditor.Animations;

static void BuildAnimatorController(string absDir, JObject meta, string atlasAssetPath)
{
    string controllerPath = Path.Combine(
        ToAssetPath(absDir), "Animator.controller").Replace('\\','/');
    var controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);

    // 1. Add parameter "Direction" (int 0-3 cho S/E/N/W) + "State" (int 0-9)
    controller.AddParameter("Direction", AnimatorControllerParameterType.Int);
    controller.AddParameter("State", AnimatorControllerParameterType.Int);

    // 2. Load all sliced sprites
    var allSprites = AssetDatabase.LoadAllAssetsAtPath(atlasAssetPath);
    var spriteByName = new Dictionary<string, Sprite>();
    foreach (var s in allSprites)
        if (s is Sprite sp) spriteByName[sp.name] = sp;

    // 3. Per anim → AnimationClip với 4 sub-clip per direction
    var sm = controller.layers[0].stateMachine;
    int stateIdx = 0;
    foreach (var anim in meta["anims"].Children<JProperty>())
    {
        string animName = anim.Name;
        var state = sm.AddState($"{animName}");
        foreach (var dir in anim.Value.Children<JProperty>())
        {
            string dirName = dir.Name;
            var clip = new AnimationClip { frameRate = 12 };
            var binding = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");
            int total = ((JArray)dir.Value).Count;
            var keys = new ObjectReferenceKeyframe[total];
            for (int i = 0; i < total; i++)
            {
                string spriteName = $"{animName}_{dirName}_{i:D2}";
                keys[i] = new ObjectReferenceKeyframe
                {
                    time = i / 12f,
                    value = spriteByName[spriteName],
                };
            }
            AnimationUtility.SetObjectReferenceCurve(clip, binding, keys);
            // Loop khi anim is Idle/Walk/Run/Meditation/Victory
            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = IsLoopAnim(animName);
            AnimationUtility.SetAnimationClipSettings(clip, settings);

            string clipPath = Path.Combine(
                Path.GetDirectoryName(controllerPath),
                $"{animName}_{dirName}.anim").Replace('\\','/');
            AssetDatabase.CreateAsset(clip, clipPath);
            // BlendTree per direction → fed by "Direction" param
            // (simplification: 1 state per anim, blend tree chọn direction)
        }
        // Set state.motion = blend tree (chi tiết omitted for brevity)
        if (stateIdx == 0) sm.defaultState = state;
        stateIdx++;
    }
    AssetDatabase.SaveAssets();
}

static bool IsLoopAnim(string name) =>
    name == "idle" || name == "walk" || name == "run" ||
    name == "meditation" || name == "victory";
```

### 8.3 Plug vào CharacterController hiện có

Repo hiện có `PlayerController` (xem `Scripts/Player/PlayerController.cs`) dùng `PuppetAnimController` cho 30-part rig 2D. Để swap sang 3D-baked atlas:

1. Add component `Animator` lên Player GameObject (sibling của `SpriteRenderer`).
2. Drag `Animator.controller` (vừa generate § 8.2) vào field `Animator.runtimeAnimatorController`.
3. Disable `PuppetAnimController` (giữ component, set enabled = false — fallback nếu user revert).
4. Add `BakedSpriteCharacterController` component (mới — bridge giữa `PlayerController` input và `Animator`):
   ```csharp
   namespace WildernessCultivation.Vfx
   {
       [RequireComponent(typeof(Animator))]
       public class BakedSpriteCharacterController : MonoBehaviour
       {
           Animator anim;
           static readonly int DirHash   = Animator.StringToHash("Direction");
           static readonly int StateHash = Animator.StringToHash("State");

           void Awake() => anim = GetComponent<Animator>();

           public void SetDirection(Vector2 facing)
           {
               // Map facing vector → 0=S, 1=E, 2=N, 3=W
               int dir = Mathf.Abs(facing.x) > Mathf.Abs(facing.y)
                   ? (facing.x > 0 ? 1 : 3)
                   : (facing.y > 0 ? 2 : 0);
               anim.SetInteger(DirHash, dir);
           }

           public void SetState(string stateName)
           {
               // Map state name → int (xem dictionary trong meta)
               anim.SetInteger(StateHash, AnimNameToInt(stateName));
           }
       }
   }
   ```

5. Update `PlayerController.Update` gọi `bakedCtrl.SetDirection(...)` + `SetState(...)` thay vì gọi `puppetAnim.PlayWalk()` hiện tại.

> **Khuyến nghị:** giữ cả 2 path (puppet + baked) qua flag `useBakedSprites: bool` trên `BootstrapWizard`. Toggle để A/B test trong Editor mà không phải xoá puppet placeholder.

---

## §9 Stage 6-7 — Review + tune loop

### Review checklist (Unity Play mode)

- [ ] Idle loop mượt, không jump frame ở wrap-around
- [ ] Walk + Run direction E/W mirror đúng (Mixamo bias right-handed)
- [ ] Attack hit frame timing match `PlayerCombat.attackCooldown`
- [ ] Outline thickness consistent giữa S/E/N/W (camera angle khác → outline projection khác)
- [ ] Palette match `AI_PROMPTS.md` §1 luật 6 (cinnabar/jade/cream tri-color present)
- [ ] Wuxia ornament (jade pendant + cloud sigil chest + sash) visible ít nhất ở Idle/S frame 0
- [ ] Meditation pose match phím M action
- [ ] No clipping (arm xuyên qua torso ở Walk peak frame)

### Tune feedback loop

| Vấn đề user feedback | Stage cần rerun | Time |
|---|---|---|
| Outline mỏng/dày | Stage 4 §7.4 (chỉ adjust `solidify.thickness`, re-render) | ~15 phút |
| Palette saturation cao | Stage 4 §7.2 (adjust ColorRamp stops, re-render) | ~15 phút |
| Wuxia ornament miss | Stage 4 §7.3 (decal placement adjust, re-render) | ~30 phút |
| Anim timing sai | Stage 5 §8.2 (chỉ regen AnimationClip, không re-render Blender) | ~1 phút |
| Mesh proportion sai | Stage 1 (re-gen PNG concept) — không "fix" được sau Tripo | ~1 giờ |

**Iteration budget mục tiêu:** ≤ 3 round per character. Round 4+ → revert sang 2D puppet pipeline.

---

## §10 Cost estimation per character

| Stage | Tool | Cost (USD) | Time (lần đầu) | Time (lần thứ 2+) |
|---|---|---|---|---|
| 1. PNG concept | AI image gen (Midjourney $10/mo, Leonardo $0 hoặc GPT image $0.04/img) | $0.04–$0.20 | 30 phút (gen + acceptance test) | 10 phút |
| 2. Image → 3D mesh | Tripo3D Basic free (~30-50 credit) hoặc Meshy free (~30 credit) | $0 | 10 phút | 5 phút |
| 3. Mixamo rig + 10 anim | Mixamo (free) | $0 | 10 phút | 5 phút |
| 4. Blender bake 480 frame | Headless Blender, AWS spot ~$0.05/h | $0 (local) hoặc $0.02 (cloud) | 30 phút | 5 phút (re-bake only) |
| 5. Unity import | Editor script | $0 | 1 phút | 1 phút |
| **Total per character** |  | **$0.04–$0.22** | **~80 phút (1.5 giờ)** | **~30 phút** |

**Project budget (12 character):**
- Lần đầu: ~16 giờ work + ~$1 cloud cost.
- Re-bake (style tune sau lock): ~6 giờ work + ~$0.30.
- Tripo3D Basic 300 credit/tháng đủ cho ~6-10 character/tháng → 2 tháng phủ hết 12 character.

**So với 2D puppet pipeline (current):**
| | 3D-mediated | 2D puppet |
|---|---|---|
| Time per character | 1.5 giờ (lần đầu) / 30 phút (re-bake) | 2-3 giờ (gen 30 PNG + acceptance test) |
| Asset count per character | 1 atlas PNG (~3-5 MB) | 30 PNG (~30-60 KB total) |
| Animation library | 10 clip out-of-box | 5 clip (idle/walk/attack/hit/death) — extend cần code |
| Iteration time | 5-15 phút (re-render 1 stage) | 5 phút (drop new PNG) |
| AI agent friendliness | Cao (script automate Stage 4+5) | Cao (procedural rig) |

→ **3D pipeline thắng** khi cần animation richness. **2D puppet thắng** cho asset count nhỏ + iteration speed.

---

## §11 Style match — preserve Chibi Wuxia × Soft-DST

Đây là rủi ro #1 của pipeline — không address kỹ thì output nhìn "generic 3D character toon", mất identity.

### Defense-in-depth (4 layer)

1. **Input layer** (Stage 1):
   - PNG concept PHẢI pass `AI_PROMPTS.md` §6 acceptance test (≥ 7/10) trước khi vào Stage 2.
   - Multi-view (front+side+back) bắt buộc cho Player + 3 hero mob — single-view chỉ cho mob phụ.

2. **Mesh layer** (Stage 2-3):
   - Tripo3D / Meshy texture sẽ NOT capture wuxia ornament — accept loss.
   - Decimate xuống 5-8k tris cho chibi proportion (Mixamo cap + render performance).
   - Re-paint texture / decal trong Blender để recover ornament — § 7.3.

3. **Shader layer** (Stage 4):
   - ColorRamp 3-stop CONSTANT interp → cel shading hard.
   - Solidify outline ≈ 1% body height + sepia color `#1a1408` → match §1 luật 1.
   - Watercolor noise overlay 0.15 fac → match §1 luật 2.
   - Sepia world ambient `(0.5, 0.45, 0.4)` → match §1 luật 3 (muddied palette).

4. **Render layer** (Stage 4):
   - Orthographic camera + slight high angle 25° → DST signature top-down feel.
   - Resolution per cell = 256 px (cell trong atlas), final display ở Unity scale up tới ~512 px world.
   - Filter mode: Point (pixel-art crisp) — KHÔNG bilinear (sẽ blur outline).

### Failure modes

| Symptom | Layer fix | Cụ thể |
|---|---|---|
| Outline broken at silhouette edge | Shader | Tăng `solidify.thickness`, hoặc bật Freestyle layer chồng |
| Texture nhìn "plastic" | Shader | Tăng watercolor noise factor 0.15 → 0.25; giảm specular ramp |
| Ornament missing on side/back view | Mesh | Decal § 7.3 phải project từ 4 angle, không chỉ front |
| Saturation cao | Shader | Add Hue/Saturation node sau diffuse, sat = 0.3 (clamp 30%) |
| Anime eye instead of dot pupil | Mesh | Re-paint texture eye region — Tripo gen anime eye by default |

---

## §12 Troubleshooting

### Tripo3D / Meshy

- **Queue > 30 phút** → switch sang Meshy hoặc retry sau 1h.
- **Mesh có "tail" geometry sau lưng** (Tripo single-view bug) → re-gen với multi-view 3 angle.
- **Polycount cap reached (Meshy 10k)** → Decimate trong Blender trước upload Mixamo.
- **Texture seam visible** → re-bake UV trong Blender (Smart UV Project, 0.1° angle limit).

### Mixamo

- **"Sorry, unable to map your existing skeleton"** → mesh không phải single object, hoặc không T-pose. Trong Blender: `Ctrl+J` join + `Ctrl+A` apply pose.
- **Hand bones twisted** → Standard Skeleton (no fingers) thay vì Skeleton + Fingers. Hoặc adjust elbow marker chính xác.
- **Anim không loop seamless** → Mixamo có "Trim" sliders — set Start/End sao cho frame 0 = frame N.

### Blender Python

- **EEVEE không render outline** → bật `solidify.show_render = True`. Hoặc check `mesh.data.use_auto_smooth = True`.
- **Render trắng (no transparency)** → `s.render.film_transparent = True` + `s.render.image_settings.color_mode = 'RGBA'`.
- **Animation không play** → check action name match exact (Mixamo prefix `mixamo.com|...`). Use `armature.animation_data.action = bpy.data.actions[...]` explicit.

### Unity

- **Sprite slice rect off-by-1** → check JSON metadata Y axis (Blender bottom-up vs Unity top-down). Có thể cần `y = atlas_height - y - h`.
- **AnimatorController state machine empty** → reload after `AssetDatabase.SaveAssets()` + `AssetDatabase.Refresh()`.
- **Character pivot off-center** → adjust `pivot = new Vector2(0.5f, 0.25f)` (foot anchor cho 2.5D top-down).

---

## §13 Decision matrix vs `PUPPET_PIPELINE.md`

| Câu hỏi | 3D-mediated (this) | 2D puppet ([`PUPPET_PIPELINE.md`](PUPPET_PIPELINE.md)) |
|---|---|---|
| Asset có cần ≥ 6 anim? | ✓ | ✗ |
| Cần real-time gear/color swap? | ✗ | ✓ |
| AI agent có thể automate end-to-end? | ✓ (Stage 2-5) | ✓ (Stage 1-3, art layer manual) |
| APK budget cho 1 character? | 3-5 MB | 30-60 KB |
| Iteration time per round | 15 phút | 5 phút |
| Setup time (lần đầu) | 80 phút | 30 phút |
| Style identity strict? | Risky (cần § 11 defense) | Native (PNG = source of truth) |
| Default cho Player? | ✓ (recommended) | Fallback |
| Default cho mob phụ? | ✗ | ✓ (recommended) |

---

## §14 Future TODO

- [ ] Pilot Player end-to-end qua pipeline này (PR follow-up — sau khi user approve doc này)
- [ ] Add `tools/blender_sprite_render/render_character.py` production-ready (skeleton ở § 7.2)
- [ ] Add `Assets/_Project/Editor/SpriteAtlasImporter.cs` production-ready (skeleton ở § 8.1-8.2)
- [ ] Add `BakedSpriteCharacterController` component bridge (skeleton ở § 8.3)
- [ ] Add EditMode test: import 1 fixture atlas + verify AnimatorController có 10 state đúng tên
- [ ] Benchmark APK size impact: 1 character 3D vs 12 character 2D — quyết định mass-conversion threshold
- [ ] Document Wolf / FoxSpirit / Boss conversion sau Player pilot pass review
- [ ] Add CI guard: `Assets/_Project/Art/Characters/{id}/atlas/` size cap (5 MB/character)

---

## Tham khảo

**Tools / docs:**
- Tripo3D Studio pricing: https://www.tripo3d.ai/pricing
- Meshy pricing: https://www.meshy.ai/pricing — API docs: https://docs.meshy.ai/api/pricing
- Mixamo: https://www.mixamo.com/ (Adobe ID free)
- Blender Python API (import_scene.fbx): https://docs.blender.org/api/current/bpy.ops.import_scene.html
- Unity AnimatorController API: https://docs.unity3d.com/ScriptReference/Animations.AnimatorController.html

**Reference repos (MIT/Apache, copy idea — không vendor):**
- `dbarton-uk/blender-sprite-render` (Dec 2025) — 4-dir batch + auto-crop PoT + JSON metadata
- `pekkavaa/SpriteBatchRender` (2013) — 8-dir Blender plugin classic
- Yasen Dinkov sprite renderer blog: https://yasendinkov.com/posts/sprite-generator/

**Internal docs:**
- [`Documentation/art/AI_PROMPTS.md`](../art/AI_PROMPTS.md) — master AI prompt catalog (style lock + acceptance test)
- [`Documentation/art/ART_STYLE.md`](../art/ART_STYLE.md) — palette + outline thickness target
- [`Documentation/pipelines/PUPPET_PIPELINE.md`](PUPPET_PIPELINE.md) — pipeline 2D song song
- [`Documentation/pipelines/BONE_RIG_GUIDE.md`](BONE_RIG_GUIDE.md) — Unity 2D bone rig (alternative)
- [`ARCHITECTURE.md`](../../ARCHITECTURE.md) §1 — folder layout
