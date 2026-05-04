---
name: bone-rig-guide
audience: both
status: active — alternative pipeline
scope: Unity 2D bone rig (PSB layered). Alternative cho character cần advanced animation.
depends-on:
  - ../art/ART_STYLE.md
  - PUPPET_PIPELINE.md
---
# Bone Rig Guide — 2D Animation cho Player + Hero Mobs

> Hướng dẫn rig 2D bone skeleton cho character (Player, Wolf, FoxSpirit, Boss) dùng Unity 2D Animation package. Mob phụ (Rabbit, Crow, Bat...) + world resource (tree/rock/grass) GIỮ procedural — đừng rig.

## Khi nào dùng bone rig

| Asset | Approach | Lý do |
|---|---|---|
| **Player** | Bone rig | Focal point, user nhìn 99% thời gian. Walk/run/attack/hurt deserve articulated motion. |
| **Wolf, FoxSpirit, Boss** (3 hero mobs) | Bone rig | Combat-focused, chase/attack feel cần "weight" rõ rệt. |
| **Rabbit, Crow, Bat, Boar, DeerSpirit, Snake** | Procedural (`MobAnimController`) | Background mobs, scope nhỏ, AI-gen 1 PNG đủ. |
| **Tree, Rock, Water, Grass, Plants** | Procedural (sway / wiggle / ripple) | World decoration, sub-second interactions. |

## Pre-requisites

- **Unity 6 LTS** (6000.4.4f1) — đã có trong repo.
- **Unity 2D Feature Set** (`com.unity.feature.2d` 2.0.1) — đã có trong `Packages/manifest.json`. Bao gồm:
  - `com.unity.2d.animation` (skinning + IK)
  - `com.unity.2d.psdimporter` (multi-layer PSB import)
- **Photoshop / Krita / Procreate** (any DCC support PSB layered export). Hoặc dùng tool free [Photopea](https://www.photopea.com/) → Save As PSD.

## Pilot: One-click Player rig (Editor menu)

> **TL;DR:** đã có sẵn 22 PNG body parts của Player ở `Assets/_Project/Art/Characters/player/{E,N,S}/`
> (do PR #140 extract). Menu `Tools → Wilderness Cultivation → Build Player Bone Rig` build
> Player_Rigged.prefab + 5 AnimationClip + AnimatorController qua 1 click — KHÔNG cần PSB import,
> KHÔNG cần Skinning Editor, KHÔNG cần hand-key animation.

### Output

| Asset | Path | Note |
|---|---|---|
| Prefab | `Assets/_Project/Prefabs/Player_Rigged.prefab` | Root + SpriteRoot + bone tree + `BoneAnimController` |
| Controller | `Assets/_Project/Animations/Player_Rigged/Player.controller` | 5 state FSM với param Speed/Moving/Crouch/Lunge/Squash |
| Clips | `Assets/_Project/Animations/Player_Rigged/{Idle,Walk,Crouch,Lunge,Squash}.anim` | DST-style timing — curves trong `PlayerBoneClipSpecs.cs` |

### Workflow

1. **Mở Unity Editor 6 LTS** với project này.
2. **Menu `Tools → Wilderness Cultivation → Build Player Bone Rig`** → tự build prefab + controller + clips.
   - Idempotent: re-run regenerates (overwrite existing assets) khi tune curve trong `PlayerBoneClipSpecs.cs`.
3. **Drag `Player_Rigged.prefab` vào scene** (vd: MainScene sau Bootstrap, hoặc empty scene).
4. **Hit Play** — character chạy state Idle (subtle breath bob).
5. **Test states qua Animator window:**
   - Window → Animation → Animator (chọn Player_Rigged trong scene).
   - Tab Parameters → tick `Moving` = true → state chuyển Idle → Walk.
   - Tick `Crouch` = true → state chuyển → Crouch.
   - Click Trigger `Lunge` → state Lunge một lần → return Idle.
   - Click Trigger `Squash` → state Squash một lần → return Idle.
6. **Compare side-by-side với Puppet Player:**
   - Run Bootstrap để có Player default (PuppetAnimController) trong scene.
   - Drag Player_Rigged kế bên → so sánh feel idle bob, walk swing, attack snap.

### Tune curve

Curve cho mỗi anim sống trong `Assets/_Project/Scripts/Vfx/PlayerBoneClipSpecs.cs` — pure data,
testable từ EditMode. Edit:

| Method | Tune gì |
|---|---|
| `Idle()` | Torso/head bob amplitude, arm sway angle, breath frequency (qua `len`) |
| `Walk()` | Arm/leg swing degrees, forearm follow-through, knee bend angle, step bob amplitude |
| `Crouch()` | Torso drop Y, knee bend depth, arm forward bend |
| `Lunge()` | Arm/forearm snap angle, anticipation timing, follow-through |
| `Squash()` | Squash compress depth, overshoot bounce |

Sau khi tune → re-run menu → asset overwrite. Test lại trong Editor.

### Test gate

EditMode test `PlayerBoneClipSpecsTests.cs` verify:

- 5 clip với name + loop flag đúng kỳ vọng
- Mỗi binding dùng bone path trong whitelist (tránh typo break Animator binding)
- Walk arm trái + phải đối phase (sign opposite)
- Lunge ArmRight snap value đủ steep (~-90°)
- Idle breath amplitude trong DST range (subtle, 0.01-0.10)

CI lint + test pass = pilot đã ready cho user playtest.

### Sau pilot — feedback loop

User play scene + report:

- "Walk quá cứng" → tăng `armSwingDeg` / `legSwingDeg` trong `PlayerBoneClipSpecs.Walk()`.
- "Idle không thấy thở" → tăng `amp` trong `Idle()` từ 0.04 lên 0.06.
- "Attack chậm quá" → giảm `len` trong `Lunge()` từ 0.4 xuống 0.3.
- "Hit/death missing" → mở rộng spec, add 2 anim mới, update Animator state machine trong `PlayerBoneRigBuilder.cs`.

Devin push update PR riêng cho từng iteration tune.

### Khi nào không dùng pilot này

- Cần **mesh deformation** (head wobble, hair flow, cloth drape) → cần Unity 2D Animation
  Skinning Editor flow ở section "Pipeline" bên dưới (PSB + bone weights + Sprite Skin).
- Cần **N/S direction rig** — pilot chỉ build East direction. Sau review, expand sang N/S là 1 PR riêng.
- Cần **IK constraint** (foot lock, hand-on-weapon) — Skinning Editor IK Solver yêu cầu PSB workflow.

---

## Pipeline (full PSB workflow, 1 character ~2-3 giờ)

> Approach này dùng khi cần mesh deform / IK / multi-direction. Pilot ở trên là sub-set của workflow này (Transform-only animation, sprite-per-part rigid limb).

### Bước 1 — Chuẩn bị PSD

Tạo file PSB (PSD lớn) với từng body part trên 1 layer riêng:

```
PlayerCharacter.psb
├── head           ← layer riêng, 256x256 px
├── torso          ← layer riêng
├── arm_left       ← layer riêng
├── arm_right      ← layer riêng
├── leg_left       ← layer riêng
├── leg_right      ← layer riêng
└── (optional) hair, eyes, weapon_hand
```

**Quy ước layer:**
- Mỗi part vẽ ở "neutral pose" (T-pose hoặc tư thế thẳng đứng) — KHÔNG vẽ pose hành động.
- Layer name không có khoảng trắng (Unity dùng làm sprite name).
- Background trong suốt.
- Pivot point của rig sẽ ở "hông" (giao của torso + leg).

**AI-gen workflow** (nếu dùng SDXL / Midjourney):
1. Prompt template trong `Documentation/ART_STYLE.md` — ra 1 ảnh full character.
2. Photoshop: dùng Magic Wand / Quick Selection để cắt từng part ra layer riêng.
3. Inpainting (Stable Diffusion / Photoshop Generative Fill) để fill phần body bị che (vd: phần arm sau torso).
4. Save As → `.psb` (Photoshop Big format) hoặc `.psd` đa layer.

### Bước 2 — Import vào Unity

1. Drop file `.psb` vào `Assets/_Project/Art/Characters/Player/` (tạo folder nếu chưa có).
2. Click file PSB trong Project view → Inspector → set:
   - **Texture Type:** `Sprite (2D and UI)`
   - **Sprite Mode:** `Multiple` (auto-tách từng layer thành 1 sprite)
   - **Pixels Per Unit:** `64` (theo `ART_STYLE.md`)
   - **Mosaic:** ✓ checked (pack layers vào 1 atlas tự động)
   - **Character Rig:** ✓ checked (quan trọng — auto-create `Sprite Library` + ready cho rig)
   - **Use Layer Grouping:** ✓ checked
3. Click **Apply**.

Unity tạo asset có icon "character" → drag vào scene → ra prefab có hierarchy bones (auto từ layer structure).

### Bước 3 — Rig bones (Skinning Editor)

1. Click PSB asset → Inspector → tìm nút **Sprite Editor** → mở.
2. Click dropdown trên cùng → chọn **Skinning Editor**.
3. Tab **Create Bone**: vẽ bone hierarchy:
   ```
   root (hip pivot)
   ├── spine
   │   ├── neck
   │   │   └── head
   │   ├── shoulder_l → upper_arm_l → forearm_l
   │   └── shoulder_r → upper_arm_r → forearm_r
   ├── thigh_l → shin_l
   └── thigh_r → shin_r
   ```
   Tổng ~10-12 bones cho player. Hero mob đơn giản hơn (~6-8 bones).
4. Tab **Auto Geometry**: chọn All → click **Generate**. Tự tạo mesh + initial weight cho mỗi part.
5. Tab **Auto Weights**: click **Generate Weights**. Auto-bind bones vào mesh.
6. Tab **Weight Brush**: tinh chỉnh các vùng joint (vd: vai/hông) — paint smooth weights để bend mượt khi animate.
7. **Apply** → close Sprite Editor.

**Tip:** Hero mob (Wolf, FoxSpirit) chỉ cần 6 bones (head, body, 4 leg) là đủ. Đừng rig quá chi tiết — animator clip sẽ phức tạp.

### Bước 4 — Tạo prefab + Animator Controller

1. Drag PSB asset vào scene → tạo GameObject với hierarchy bones + `SpriteSkin` + `SpriteRenderer` per part.
2. Add components vào root GameObject:
   - `Rigidbody2D` (Body Type: Dynamic, Gravity Scale: 0, Freeze Rotation: ✓)
   - `CircleCollider2D` (radius khớp body)
   - `Animator` (Controller: chưa có, sẽ tạo bước 5)
   - `BoneAnimController` (component mới từ PR F)
   - `DropShadow` + sprite shadow → tự spawn child shadow ellipse
   - (player only) `PlayerController`, `PlayerStats`, etc.
   - (mob only) `MobBase` subclass (vd: `WolfAI`) + `ReactiveOnHit` + `HitKnockback`
3. Save prefab vào `Assets/_Project/Prefabs/Player.prefab` (override existing) hoặc `Wolf.prefab`.

### Bước 5 — Animator Controller

1. Project view → Right-click `Assets/_Project/Animations/` (tạo folder nếu chưa) → Create → Animator Controller → đặt tên `Player.controller`.
2. Double-click → mở Animator window.
3. **Parameters tab** → add:
   - `Speed` (float)
   - `Moving` (bool)
   - `Crouch` (bool)
   - `Lunge` (trigger)
   - `Squash` (trigger)
   - (optional) `Hurt` (trigger)
4. **Layers tab → Base Layer**: tạo states + transitions:
   ```
   [Idle]  ──Moving=true──→  [Walk]
     ↑ Moving=false              │
     │                           │
     │                           ↓
     ←───Crouch=false──── [Crouch] ←Crouch=true── (any state)
   
   [Any State] ──Lunge trigger──→ [Attack] ──exit time──→ [Idle]
   [Any State] ──Squash trigger──→ [Squash] ──exit time──→ [Idle]
   ```
5. Default state = `Idle`.

### Bước 6 — Tạo animation clips

Mỗi state (Idle, Walk, Crouch, Attack, Squash) cần 1 animation clip. Trong scene với prefab Player chọn:

1. Window → Animation → Animation tab.
2. Chọn root prefab Player.
3. Click **Create** → save vào `Assets/_Project/Animations/Player/Idle.anim`.
4. Animation editor mở. Add Property → chọn từng bone → set keyframe ở 0s và 1s với rotation/position khác nhau (mô phỏng breathing / step).
5. Loop time: ✓ checked cho Idle/Walk/Crouch. Attack/Squash unchecked.
6. Repeat cho Walk, Crouch, Attack, Squash.

**Reference:** Mỗi clip ~0.5-1s. Tổng 5 clip = ~30 phút work nếu artist quen.

### Bước 7 — Wire vào Animator Controller

1. Open Animator (Window → Animator) với Player prefab selected.
2. Drag từng `.anim` file vào graph → tự tạo state.
3. Connect transitions theo Bước 5.
4. Cho mỗi transition, click vào → Inspector → set condition (vd: `Moving = true` cho Idle→Walk).

### Bước 8 — Verify trong scene

1. Mở scene → bấm Play.
2. Move player với WASD → confirm:
   - `Speed` param tăng theo velocity (xem Animator window khi runtime)
   - `Moving` toggle on khi di chuyển
   - State transition Idle ↔ Walk
3. Trigger attack input → `Lunge` trigger fired → state Attack play 1 lần → return Idle.
4. Test crouch (nếu có input): SetCrouch(true) → state Crouch.

## Common pitfalls

- **Layer grouping off:** PSB import tách từng sub-layer thành sprite riêng → quá nhiều mảnh. Bật `Use Layer Grouping`.
- **Pivot misaligned:** root bone không ở hông → character "trượt" khi animate. Sửa qua Sprite Editor → Pivot Override per sprite.
- **Mesh weights cứng:** joint bend vỡ. Tăng smoothing trong Weight Brush + bone influence radius.
- **Animator transition dùng `Has Exit Time`:** với trigger event (Lunge, Squash) thường BỎ Has Exit Time và set transition duration nhỏ (~0.05s) để snappy.
- **`SpriteSkin` thiếu trên child:** sau khi tweak hierarchy, click root → Inspector → Add Component → SpriteSkin → "Create Bones" nếu thiếu.

## Sau khi rig xong

`BoneAnimController` đã có sẵn trong repo (`Assets/_Project/Scripts/Vfx/BoneAnimController.cs`) — implements `IMobAnim` cùng interface với `MobAnimController`. FSM state class (vd: `WolfChase.OnEnter`) gọi `w.Anim.SetCrouch(true)` không cần biết underlying là procedural hay rigged.

**Migration từ procedural sang rigged:**
1. Remove `MobAnimController` component khỏi prefab (Inspector → 3-dot → Remove).
2. Add `BoneAnimController` component (auto-detect Animator + Rigidbody2D).
3. Set parameter names trong inspector match Animator Controller (default match `Speed`/`Moving`/`Crouch`/`Lunge`/`Squash`).
4. Test trong scene — FSM transitions dùng prefab mới không cần code change.

## Roll-out plan đề xuất

| PR | Character | Effort |
|---|---|---|
| F (đang làm) | Scaffolding (interface + BoneAnimController + guide) | 0 art, code only |
| G | Player rig | ~3-4h art + setup |
| H | Wolf rig | ~2-3h |
| I | FoxSpirit rig | ~2-3h |
| (later) | Boss rig | ~4-5h (nhiều bones + special clips) |

Mỗi PR sau F = 1 character. Test trong scene xong → merge → next.

## Tham khảo

- [Unity 2D Animation manual](https://docs.unity3d.com/Packages/com.unity.2d.animation@10.0/manual/index.html)
- [PSDImporter manual](https://docs.unity3d.com/Packages/com.unity.2d.psdimporter@10.0/manual/index.html)
- [GitHub samples — 2D Animation](https://github.com/Unity-Technologies/2d-animation-samples)
