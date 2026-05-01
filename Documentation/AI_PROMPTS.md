# AI Prompts — Master Asset Catalog

> Bộ prompt **đồng bộ** cho TẤT CẢ asset của Wilderness Cultivation Chronicle.
> Mỗi prompt copy-paste vào **GPT Image 2.0** (hoặc Leonardo / Midjourney) → ra PNG sẵn drop.
>
> **Style lock:** painterly hand-painted + asian wuxia cultivation fantasy + top-down (mob/character) hoặc 90° overhead (tile/icon). Đầy đủ rationale: [`Documentation/ART_STYLE.md`](ART_STYLE.md).
>
> **Workflow note:** prompts cho character body part (puppet system) dùng **side view 90°** (không 30° top-down) — vì PuppetAnimController rotate child transforms theo plane phẳng, side-view giữ silhouette ổn định.

## Table of Contents

1. [Style Anchor (universal preamble)](#1-style-anchor-universal-preamble)
2. [Negative Prompt (universal)](#2-negative-prompt-universal)
3. [Puppet Characters](#3-puppet-characters) — §3.0 asset count checklist + Player, Wolf, FoxSpirit (multi-piece) + §3.4 multi-direction NSEW variants + §3.5 L2 elbow/knee (forearm + shin) + §3.6 quick-copy bundles (paste-ready full sets)
4. [Single-Sprite Mobs](#4-single-sprite-mobs) — Rabbit, Boar, Deer Spirit, Crow, Snake, Bat, Boss (Phase 1 + Phase 2 enraged)
5. [Resources / World Objects](#5-resources--world-objects) — tree, rock, water, mushroom, berry, cactus, lily, bamboo, grass tile, mineral, structures (+ §5.13 state variants: harvested / depleted)
6. [Item Icons](#6-item-icons) — 22 inventory icons (materials, foods, tools, accessories)
7. [Ground Tiles](#7-ground-tiles) — 12 seamless prompts inline (forest, highlands, desert × 4 each)
8. [VFX](#8-vfx) — hit flash, damage popup, blood splash, dust poof, fire spark, smoke, mana glow, level-up halo, death decay, status icon
9. [Weather](#9-weather) — rain particle, snow particle, fog overlay, lightning bolt, sun ray, sandstorm
10. [NPC Humanoid](#10-npc-humanoid) — VendorNPC (elder merchant) + CompanionNPC (young female cultivator) full puppet sets
11. [Environment Props](#11-environment-props) — chest, lantern, shrine, banner, signpost, barrel, crate, broken stele, tent, altar
12. [Cost Estimate](#cost-estimate-gpt-image-20)
13. [Iteration / Quality Tips](#iteration--quality-tips)

---

## 1. Style Anchor (universal preamble)

Mọi prompt **PHẢI** include block sau (paste đầu prompt). Nếu tool không nhận block dài, ưu tiên giữ 4 keyword đầu.

```
hand-painted painterly digital illustration, visible brush strokes,
soft cel-shading, asian wuxia cultivation fantasy aesthetic,
limited color palette with 3-4 tonal stops, clean readable silhouette,
1.5 to 2 pixel mid-tone outline (NOT pure black, use dark version of base color),
NOT pixel art, NOT photo-realistic, NOT anime moe style, NOT chibi.
```

### Universal palette (paste khi prompt nhắc accent / mist / qi)

- Primary gold `#d4a64a`, Cinnabar red `#8b3a3a`, Jade green `#6b8e62`, Spirit qi blue `#a8d8ff`, Sky qi mid `#6fb5e0`, Bone cream `#e8d5a6`, Ink black `#1a1a1a` (outline mix), Death qi purple `#9b6b8b`.

### Per-biome palette (chọn khi asset thuộc biome)

- **Forest** (linh mộc): deep moss `#4a6741`, sage `#6b8e62`, mint `#a8c69b`, bark brown `#b89968`, dry leaf `#8a6f47`, mushroom red `#a14040`.
- **Stone Highlands** (đá sơn): slate `#7a7c80`, highlight stone `#a3a5a8`, shadow stone `#5a5d63`, dry moss `#8a9b8c`, bone white `#c2c4ba`, mineral blue `#4d6b8c`.
- **Cursed Desert** (tử khí): sand base `#c4a574`, sand highlight `#dec594`, dirt shadow `#8b7355`, dirt deep `#6b5d40`, death qi purple `#9b6b8b`, bone bleached `#d4d4d4`, cactus green `#6b8559`.

---

## 2. Negative Prompt (universal)

Paste vào "Negative prompt" / "Avoid" field mọi generation:

```
no pixel art, no photo-realistic, no anime moe, no chibi,
no pure black outline, no smooth airbrush gradient,
no drop shadow on transparent background, no text, no watermark,
no signature, no border, single subject only, no duplicate,
no grid lines, no UI elements, no caption, no logo,
no lens flare, no ground beneath subject for body parts.
```

---

## 3. Puppet Characters

> **Pipeline:** Drop PNG vào `Assets/_Project/Art/Characters/{characterId}/{filename}.png` (flat) hoặc `Assets/_Project/Art/Characters/{characterId}/{E|N|S}/{filename}.png` (multi-dir). BootstrapWizard auto-detect layout và build puppet hierarchy phù hợp.
>
> **Tối thiểu để puppet build:** `head.png` + `torso.png` ở East dir (hoặc flat). Nếu thiếu → fallback single-sprite. Limbs / tail optional.
>
> **Pose lock:** "neutral standing — arm thẳng xuống, leg thẳng đứng". PuppetAnimController rotate runtime → KHÔNG vẽ pose dynamic.
>
> **Resolution:** 256×512 (head 256×256 OK), transparent background, isolated single object.
>
> **Pivot tip:** mỗi body part nên có pivot tại khớp nối — vd `arm_left.png` pivot ở **vai** (top center), `leg_left.png` pivot ở **hông** (top center). Sprite Editor có thể chỉnh pivot sau import, nhưng artist gen với clean top-edge khớp giúp default Unity Center pivot vẫn xài được.
>
> **Layout choice (PR J — L3+):**
> - **Flat** = side-only (Vampire Survivors / Soulstone style). Prompts §3.1 / §3.2 / §3.3 = side view facing right (E). Drop vào `{id}/head.png` etc.
> - **Multi-dir** = 4-cardinal NSEW (Don't Starve / Stardew style). Drop side prompts vào `{id}/E/head.png` etc., **plus** N (back) + S (front) variants per §3.4. W = mirror E (free).
>
> **Recommendation:** start với flat → playtest → upgrade multi-dir nếu side-only không đủ "alive". Prompts §3.4 chỉ delta (view change) — palette + style + brush strokes anchor giữ nguyên 100% từ §3.1/3.2/3.3 để đảm bảo cross-dir consistency.

### 3.0 Asset Count Checklist (đọc TRƯỚC khi gen)

> **Q: 1 character / mob đủ là bao nhiêu ảnh?**
> **A:** Tùy character TYPE và TIER chọn — bảng dưới liệt kê chính xác.

| Character | Type | Flat L1 (E-only, side) | Multi-dir L1 (E+N+S) | + L2 elbow/knee | Notes |
|---|---|---|---|---|---|
| **Player** (`Art/Characters/player/`) | bipedal puppet | **6** | **18** | +4 (E only) / +12 (multi-dir) | head + torso + 2 arms + 2 legs |
| **Wolf** (`Art/Characters/wolf/`) | quadruped puppet | **7** | **21** | n/a | + tail (L2 chỉ áp dụng player-shaped puppet) |
| **FoxSpirit** (`Art/Characters/fox_spirit/`) | quadruped puppet | **7** | **21** | n/a | + tail (hero feature ở N back-view) |
| **Rabbit** (`Art/Characters/rabbit/`) | quadruped puppet (small) | **7** | **21** | n/a | + puffy white tail (hero feature) — §3.3.5 master + §3.6.4 bundle (Phase 2A) |
| **Boar** (`Art/Characters/boar/`) | quadruped puppet (heavy) | **7** | **21** | n/a | + bristly stub tail + tusks (head sprite) — §3.3.6 master + §3.6.5 bundle (Phase 2B) |
| **Deer Spirit** (`Art/Characters/deer_spirit/`) | quadruped puppet (graceful) | **7** | **21** | n/a | + white tail flick + antlers (head sprite, hero feature) — §3.3.7 master + §3.6.6 bundle (Phase 2B) |
| **Crow / Snake / Bat** | single-sprite | **1** mỗi mob | n/a | n/a | MobAnimController bob/tilt procedural — chưa upgrade puppet (Phase 3/4) |
| **Boss — Hắc Vương** (`Art/Characters/boss/`) | bipedal puppet (humanoid villain) | **6** | **18** | +4 (E only) / +12 (multi-dir) | head + torso + 2 arms + 2 legs (mirror Player rig, NO tail) — §3.3.8 master + §3.6.7 bundle (Phase 2C) |

**Per-tier breakdown cho Player + Boss (bipedal humanoid):**

| Tier | E (side) | N (back) | S (front) | W | Total | Style ref |
|---|---|---|---|---|---|---|
| Flat L1 | 6 | — | — | mirror E (free) | **6** | Vampire Survivors / Soulstone |
| Multi-dir L1 | 6 | 6 | 6 | mirror E (free) | **18** | Don't Starve / Stardew (DST style) |
| Multi-dir L1 + L2 | 10 | 10 | 10 | mirror E (free) | **30** | Hades / Cuphead polish |

**Per-tier cho Wolf / FoxSpirit / Rabbit / Boar / Deer Spirit (+ tail):**

| Tier | E | N | S | Total |
|---|---|---|---|---|
| Flat L1 | 7 | — | — | **7** |
| Multi-dir L1 | 7 | 7 | 7 | **21** |

> **S/tail skip note (real-world output):** front view (S) cho quadruped tail thường bị che bởi torso → optional, controller fallback East sprite cho slot null. Bundle §3.6.x dùng 14 prompts → 20 PNG (skip `S/tail.png`) thay vì 21 lý thuyết.

> **Q: Đủ rồi sẽ chuyển động mượt mà không?**
> **A:** **Yes — ngay cả Flat L1 (6 PNG) cũng mượt.** PuppetAnimController là **procedural animation** — controller rotate / offset transforms theo time + velocity ở runtime (walk bob, arm swing, leg swing, idle breath, lunge, crouch). Anh **KHÔNG cần gen walk frames** (kiểu sprite sheet 8 frame/cycle). Tăng tier chỉ cải thiện chất lượng visual, không phải fluidity:
> - **Multi-dir (E+N+S)** = nhân vật **xoay theo hướng đi** (DST feel — đi lên thấy lưng, đi xuống thấy mặt) thay vì luôn nhìn ngang.
> - **L2 elbow/knee** = tay / chân **gập tự nhiên** khi attack hoặc crouch (Hades polish) thay vì rigid limb thẳng.
>
> **Single-sprite mob** (Crow/Snake/Bat) chỉ cần 1 PNG — `MobAnimController` dùng `walkBobAmplitude` + `tiltDeg` + lunge keyframe để tạo motion (xem `Scripts/Vfx/MobAnimController.cs`). Không có puppet hierarchy → không cần multi-part PNG. **Phase 3/4** sẽ upgrade nốt (Phase 3 = Crow/Bat wing rig, Phase 4 = Snake serpentine).

**Recommendation budget-conscious:** start tier minimum (Flat L1 = 6 PNG cho Player, 7 cho Wolf/Fox/Rabbit/Boar/Deer, 1 mỗi single-sprite mob = 3 PNG cho 3 mob đơn còn legacy) — playtest, nếu thấy thiếu "alive" mới upgrade Multi-dir. L2 chỉ làm cuối khi đã hài lòng L1.

**Required minimum để puppet build pass:** chỉ cần `head.png` + `torso.png` ở East/flat. Thiếu → fallback single-sprite (puppet không spawn). Limbs / tail optional — controller tự skip slot null.

> **Ngại scroll giữa §3.1 + §3.4 + §3.5?** → §3.6 có **Quick-Copy Bundles** gom đủ prompts theo character. Hiện có: §3.6.1 Player (12 prompts → 18 PNG), §3.6.2 Wolf (14 → 20), §3.6.3 FoxSpirit (14 → 20), §3.6.4 Rabbit (14 → 20), §3.6.5 Boar (14 → 20), §3.6.6 Deer Spirit (14 → 20). Copy tuần tự từ trên xuống là xong, không cần tìm khắp doc.

### 3.1 Player — Cultivation Hero (`Art/Characters/player/`)

> **Concept:** young Asian male cultivator, white robe with gold accent, calm focused expression, ink-black hair tied in topknot, jade pendant. Bipedal humanoid.

```
=== player/head.png ===

hand-painted painterly digital illustration, visible brush strokes,
soft cel-shading, asian wuxia cultivation fantasy aesthetic,
limited color palette, clean readable silhouette, 1.5 to 2 pixel
mid-tone outline (dark cream tone, NOT pure black).

Subject: side view of a young Asian male cultivator HEAD ONLY,
profile facing right, calm focused expression, ink-black hair tied
in a topknot with bone cream silk ribbon, smooth jade-pale skin,
faint qi glow on temple, isolated single body part on fully
transparent background. NO neck visible below jawline (clean cut).

Palette (use ONLY these): bone cream skin #e8d5a6 base, warm shadow
#b89968 mid-tone, ink black #1a1a1a hair, primary gold #d4a64a
ribbon highlight, jade green #6b8e62 faint qi glow.

Composition: 256x256 px, isolated single head on transparent
background, vertically centered, NO body parts below jaw, NO
shoulders, NO neck, NO ground, NO shadow.
```

```
=== player/torso.png ===

hand-painted painterly digital illustration, visible brush strokes,
soft cel-shading, asian wuxia cultivation fantasy aesthetic.

Subject: side view of a young Asian male cultivator TORSO ONLY,
profile facing right, neutral standing pose, wearing flowing white
martial arts robe with gold embroidery on collar and waist sash,
green jade pendant on chest, robe falls to mid-thigh, NO head NO
arms NO legs visible (clean cuts at shoulders, hips, neck).

Palette: bone cream #e8d5a6 robe base, warm shadow #b89968 fold
shadow, primary gold #d4a64a embroidery accent, jade green #6b8e62
pendant, dry leaf #8a6f47 sash highlight.

Composition: 256x384 px (vertical), isolated single torso on fully
transparent background, NO head, NO limbs, NO ground, NO shadow.
Top edge clean horizontal at shoulder line (puppet rig pivot).
```

```
=== player/arm_left.png === / === player/arm_right.png ===

hand-painted painterly, visible brush strokes, asian wuxia.

Subject: side view of a young cultivator's LEFT (or RIGHT) arm,
hanging straight down in neutral pose, wearing flowing white robe
sleeve, hand visible at bottom (hand resting open, fingers slightly
curled), arm length proportional to mid-thigh, NO body, NO head.

Palette: bone cream #e8d5a6 sleeve base, warm shadow #b89968 fold
shadow, jade pale #d4d4ba skin tone for hand, primary gold #d4a64a
narrow trim at cuff.

Composition: 256x384 px (vertical), isolated single arm on fully
transparent background, top edge clean horizontal at shoulder
(pivot point), bottom edge at fingertips, NO body, NO ground.

# NOTE: gen 1 lần rồi flip horizontal trong Photopea/GIMP cho arm_right.
# Hoặc gen 2 lần với prompt "right arm" để có asymmetry tự nhiên.
```

```
=== player/leg_left.png === / === player/leg_right.png ===

hand-painted painterly, visible brush strokes, asian wuxia.

Subject: side view of a young cultivator's LEFT (or RIGHT) leg,
straight standing pose, wearing white robe pant flowing to ankle,
fabric shoe with cloth wrap, NO body, NO foot ground contact (foot
just hovering in neutral pose).

Palette: bone cream #e8d5a6 pant base, warm shadow #b89968 fabric
fold, dry leaf #8a6f47 shoe.

Composition: 256x384 px (vertical), isolated single leg on fully
transparent background, top edge clean horizontal at hip (pivot
point), bottom edge at sole of foot, NO body, NO ground, NO shadow.

# NOTE: same arm pattern — gen 1 lần flip cho phần đối xứng OK.
```

### 3.2 Wolf — Hung Lang (`Art/Characters/wolf/`)

> **Concept:** Đại sói xám mật rừng, ánh mắt vàng săn mồi, lông xù dày. Quadruped — hierarchy adapt: torso = body horizontal, "arms" = front legs, "legs" = back legs.

```
=== wolf/head.png ===

hand-painted painterly, asian wuxia, wilderness creature.

Subject: side view of a fierce gray wolf HEAD ONLY, profile facing
right, snarling fangs partially visible, sharp yellow eyes, ears
back in alert pose, shaggy gray fur with darker neck ruff, NO body,
NO neck below jaw.

Palette: slate gray #7a7c80 fur base, shadow stone #5a5d63 deep
shadow, highlight stone #a3a5a8 fur tip highlight, primary gold
#d4a64a eye color, ink black #1a1a1a snout outline, bone white
#c2c4ba fang.

Composition: 256x256 px, isolated head on transparent background,
NO body, NO ground, NO shadow.
```

```
=== wolf/torso.png ===

hand-painted painterly, asian wuxia, wilderness creature.

Subject: SIDE VIEW of a gray wolf BODY ONLY (no head, no legs, no
tail), oriented HORIZONTAL with head-end on RIGHT, hip-end on LEFT,
shaggy gray fur, lean predator silhouette, ribcage visible through
fur, neutral horizontal pose. Clean cuts at neck (right edge),
hips (left edge), shoulders/hip joints (bottom edge for legs).

Palette: slate gray #7a7c80 fur base, shadow stone #5a5d63 belly
shadow, highlight stone #a3a5a8 back ridge highlight, ink black
#1a1a1a outline at fur edges.

Composition: 384x256 px (HORIZONTAL — wolf body wider than tall),
isolated body on transparent background, NO head, NO legs, NO
tail, NO ground.
```

```
=== wolf/arm_left.png === / === wolf/arm_right.png === (FRONT LEGS)

hand-painted painterly, asian wuxia.

Subject: side view of a gray wolf's FRONT LEG, straight standing
pose, lean muscular leg, gray fur with white sock fading to dark
paw, sharp claws visible at paw tip, NO body, NO foot ground.

Palette: slate gray #7a7c80 fur base, shadow stone #5a5d63 leg
shadow, bone white #c2c4ba paw fur highlight, ink black #1a1a1a
claw.

Composition: 192x320 px (vertical), isolated single front leg on
transparent, top edge clean at shoulder joint (pivot), bottom at
paw, NO body.
```

```
=== wolf/leg_left.png === / === wolf/leg_right.png === (BACK LEGS)

hand-painted painterly, asian wuxia.

Subject: side view of a gray wolf's BACK LEG, standing pose with
slight crouch (powerful hindquarter), strong haunch muscle visible,
gray fur, paw with claws, NO body, NO ground contact.

Palette: same wolf palette — slate gray, shadow stone, highlight
stone, ink black claw.

Composition: 192x320 px (vertical), isolated single back leg, top
edge clean at hip joint (pivot), bottom at paw, NO body.
```

```
=== wolf/tail.png ===

hand-painted painterly, asian wuxia.

Subject: side view of a gray wolf's bushy TAIL, oriented HORIZONTAL,
attaches at LEFT edge (root), tip flowing to RIGHT (relaxed neutral
hang downward at slight angle), shaggy fur with mid-tone shadow,
darker tip, NO body.

Palette: slate gray #7a7c80, shadow stone #5a5d63 deep shadow,
highlight stone #a3a5a8 fur highlight, ink black #1a1a1a tip.

Composition: 256x192 px (HORIZONTAL — tail wider than tall),
isolated tail on transparent, root edge LEFT (pivot point at hip
attachment), tip RIGHT, NO body.
```

### 3.3 FoxSpirit — Linh Hồ (`Art/Characters/fox_spirit/`)

> **Concept:** linh hồ chín đuôi (cửu vĩ), pelt trắng với glow xanh ethereal, ánh mắt xanh quang minh, mist tử khí mờ quanh chân. Supernatural quadruped.

```
=== fox_spirit/head.png ===

hand-painted painterly, asian wuxia cultivation, supernatural ethereal.

Subject: side view of a mystical white nine-tailed fox spirit HEAD
ONLY, profile facing right, glowing pale blue cunning eyes, sharp
ears upright with alert tip, fine white fur with faint blue qi
glow at temple and ear tip, slight smirk showing tiny fang, NO
body, NO neck below jaw.

Palette: bone cream #e8d5a6 fur base, bone bleached #d4d4d4
highlight, spirit qi blue #a8d8ff glow accent on temple and ear
tips, sky qi mid #6fb5e0 eye color, ink black #1a1a1a fine outline,
primary gold #d4a64a faint inner ear gold tone.

Composition: 256x256 px, isolated head on transparent, NO body,
NO ground, ethereal soft edge but still clean silhouette.
```

```
=== fox_spirit/torso.png ===

hand-painted painterly, asian wuxia, supernatural ethereal.

Subject: SIDE VIEW of a white fox spirit BODY ONLY (no head, no
legs, no tail), HORIZONTAL orientation with head-end RIGHT, hip-end
LEFT, lithe slender silhouette, fine snowy fur with faint blue qi
glow trailing along back ridge, ethereal mist wisps at lower belly
dissolving into transparent. Clean cuts at neck/hips/shoulders.

Palette: bone cream #e8d5a6 fur base, bone bleached #d4d4d4
highlight, spirit qi blue #a8d8ff glow trail along spine, sky qi
mid #6fb5e0 belly mist, ink black #1a1a1a outline (very thin —
1px max for ethereal feel).

Composition: 384x256 px (horizontal), isolated body on transparent,
NO head, NO legs, NO tail.
```

```
=== fox_spirit/arm_left.png === / === fox_spirit/arm_right.png === (FRONT LEGS)

hand-painted painterly, supernatural fox spirit.

Subject: side view of a white fox spirit FRONT LEG, slender lithe
pose, fine white fur with faint blue qi outline, delicate paw with
small claws, NO body, NO ground.

Palette: bone cream #e8d5a6, bone bleached #d4d4d4, spirit qi blue
#a8d8ff trim glow.

Composition: 160x288 px (vertical), isolated single front leg on
transparent, top edge clean at shoulder joint, bottom at paw.
```

```
=== fox_spirit/leg_left.png === / === fox_spirit/leg_right.png === (BACK LEGS)

hand-painted painterly, supernatural fox spirit.

Subject: side view of a white fox spirit BACK LEG, slender powerful
haunch with subtle muscle definition, fine white fur with faint
blue qi line at hip, delicate paw, NO body.

Palette: same fox spirit palette.

Composition: 160x288 px (vertical), isolated single back leg, top
edge at hip joint pivot, bottom at paw.
```

```
=== fox_spirit/tail.png ===

hand-painted painterly, supernatural ethereal.

Subject: side view of a magnificent fox spirit TAIL — single tail
fluffy and majestic, oriented horizontal with attachment ROOT on
LEFT and tip flowing to RIGHT in graceful arc, fine white fur with
strong spirit qi blue glow trail along tail length, ethereal mist
wisps fading at tip.

Palette: bone cream #e8d5a6 fur base, bone bleached #d4d4d4, spirit
qi blue #a8d8ff dominant glow accent (stronger than other parts —
tail is hero feature), sky qi mid #6fb5e0 mid-tone, primary gold
#d4a64a inner glow at root.

Composition: 320x256 px (horizontal — wider than head), isolated
tail on transparent, root LEFT pivot, tip RIGHT, NO body.
```

### 3.3.5 Rabbit — Linh Thố Nimble Hopper (`Art/Characters/rabbit/`)

> **Concept:** small woodland rabbit (linh thố) — warm brown/tan fur, cream belly, **puffy white cottontail** (hero feature), long upright alert ears, dark watchful eyes. Quadruped (small) — torso compact, "arms" = short front legs, "legs" = powerful hindquarters built for hopping. NOT a cute mascot, NOT chibi — feral wilderness creature with wary alert posture.
>
> **Scale note:** rabbit ~half the size of wolf (24px placeholder height vs 32px). Sprite resolutions slightly smaller (192×192 head vs 256×256 wolf head) — keeps proportional silhouette in-game.

```
=== rabbit/head.png ===

hand-painted painterly, asian wuxia, wilderness creature.

Subject: side view of a small alert woodland rabbit HEAD ONLY,
profile facing right, long upright ears (slightly back-tilted in
wary listening pose, NOT floppy), warm brown fur with cream cheek,
small dark watchful eye, twitching nose, NO body, NO neck below
jaw.

Palette: tan brown #8b6f47 fur base, warm shadow #5a4a3a deep
shadow, fur highlight #b89968 ear tip and cheek, cream #e8d5a6
inner ear and muzzle, dark nose #2a2a2a eye and nose, bone white
#c2c4ba whisker.

Composition: 192x192 px, isolated head on transparent background,
NO body, NO ground, NO shadow.
```

```
=== rabbit/torso.png ===

hand-painted painterly, asian wuxia, wilderness creature.

Subject: SIDE VIEW of a small rabbit BODY ONLY (no head, no legs,
no tail), oriented HORIZONTAL with head-end on RIGHT, hip-end on
LEFT, compact rounded torso with slight hunched back (hopper
silhouette), warm brown back fur fading to cream belly, neutral
horizontal pose. Clean cuts at neck (right edge), hips (left edge),
shoulders/hip joints (bottom edge for legs).

Palette: tan brown #8b6f47 fur base, warm shadow #5a4a3a belly
shadow, fur highlight #b89968 back ridge highlight, cream #e8d5a6
belly underside, ink black #1a1a1a outline at fur edges.

Composition: 256x192 px (HORIZONTAL — rabbit body wider than tall),
isolated body on transparent background, NO head, NO legs, NO tail,
NO ground.
```

```
=== rabbit/arm_left.png === / === rabbit/arm_right.png === (FRONT LEGS)

hand-painted painterly, asian wuxia.

Subject: side view of a small rabbit's FRONT LEG, straight standing
pose, short slender forelimb (rabbits are hindquarter-dominant —
front legs much shorter than back), warm brown fur fading to cream
paw, small dark claws visible at paw tip, NO body, NO ground.

Palette: tan brown #8b6f47 fur base, warm shadow #5a4a3a leg shadow,
cream #e8d5a6 paw fur highlight, dark nose #2a2a2a claw, ink black
#1a1a1a outline.

Composition: 128x192 px (vertical), isolated single front leg on
transparent, top edge clean at shoulder joint (pivot), bottom at
paw, NO body.
```

```
=== rabbit/leg_left.png === / === rabbit/leg_right.png === (BACK LEGS)

hand-painted painterly, asian wuxia.

Subject: side view of a small rabbit's BACK LEG, standing pose with
distinct crouched fold (powerful hindquarter — hopper anatomy with
long thigh + bent shin folded under body), strong haunch muscle
visible, warm brown fur, paw with small claws, NO body, NO ground
contact.

Palette: same rabbit palette — tan brown, warm shadow, cream paw,
dark claw, ink outline.

Composition: 192x256 px (vertical, taller than front leg — rabbits
have longer back legs), isolated single back leg, top edge clean at
hip joint (pivot), bottom at paw, NO body.
```

```
=== rabbit/tail.png ===

hand-painted painterly, asian wuxia.

Subject: side view of a small rabbit's puffy COTTONTAIL — short
round cloud-like tuft (hero feature, much fluffier than wolf/fox
tail), oriented HORIZONTAL, attaches at LEFT edge (root), tail body
extends slightly to RIGHT (very short — almost as wide as tall),
bright cream-white fur with soft shadow underneath, NO body.

Palette: bone bleached #d4d4d4 fur tip (brightest), cream #e8d5a6
core, warm shadow #5a4a3a underside shadow, ink black #1a1a1a
outline.

Composition: 128x128 px (square — cottontail nearly round), isolated
tail on transparent, root edge LEFT (pivot point at hip attachment),
NO body.
```

### 3.3.6 Boar — Hắc Trư Tusked Charger (`Art/Characters/boar/`)

> **Concept:** wild forest boar (hắc trư) — heavy muscular quadruped, **bristly dark brown/black coarse fur** (NOT smooth like wolf), **curved ivory tusks** (hero feature visible from side), low-slung head, short stiff bristly stub tail, cloven black hooves. Aggressive charger silhouette — neck thick, shoulders hunched. NOT cute farm pig — feral wilderness predator.
>
> **Scale note:** boar ~larger silhouette than wolf (36px placeholder height vs 32px). Body bulkier — torso wider proportions (288×208 vs wolf 256×192). Sprite res same as wolf otherwise.

```
=== boar/head.png ===

hand-painted painterly, asian wuxia, wilderness creature.

Subject: side view of a wild boar HEAD ONLY, profile facing right,
heavy lowered head with thick neck stub, small angry beady eye,
broad flat snout with twin nostrils, **TWO CURVED IVORY TUSKS**
protruding upward from lower jaw (hero feature — clearly visible
from side, slightly yellowed bone tone), short pointed ear pricked
forward (alert/aggressive), bristly coarse fur on forehead and jowl,
NO body, NO neck below jaw cut.

Palette: dark brown #4d3a28 fur base, deep shadow #2a1f15 deep
shadow, fur highlight #8b6f47 forehead ridge highlight, ivory
#d4c8a3 tusk, dark nose #1a1a1a eye and nostril, bone white #c2c4ba
inner ear hint.

Composition: 256x256 px, isolated head on transparent background,
tusks fully visible (do NOT crop), NO body, NO ground, NO shadow.
```

```
=== boar/torso.png ===

hand-painted painterly, asian wuxia, wilderness creature.

Subject: SIDE VIEW of a wild boar BODY ONLY (no head, no legs, no
tail), oriented HORIZONTAL with shoulder-end on RIGHT, hip-end on
LEFT, heavy muscular torso with hunched shoulder hump (boar
characteristic), bristly dark brown fur with coarse texture (NOT
smooth — visible bristle clumps + spine ridge), low-slung belly,
neutral horizontal pose. Clean cuts at neck (right edge), hips
(left edge), shoulders/hip joints (bottom edge for legs).

Palette: dark brown #4d3a28 fur base, deep shadow #2a1f15 belly
shadow, fur highlight #8b6f47 spine ridge bristle highlight, deep
shadow #2a1f15 underside, ink black #1a1a1a outline at fur edges
and bristle clumps.

Composition: 288x208 px (HORIZONTAL — boar body bulkier than wolf),
isolated body on transparent background, NO head, NO legs, NO tail,
NO ground.
```

```
=== boar/arm_left.png === / === boar/arm_right.png === (FRONT LEGS)

hand-painted painterly, asian wuxia.

Subject: side view of a wild boar's FRONT LEG, sturdy planted pose,
short heavy forelimb (boar legs proportionally short for body bulk),
bristly dark brown fur thigh, **black cloven hoof** at bottom (split
hoof tip), no toenail, NO body, NO ground contact.

Palette: dark brown #4d3a28 fur base, deep shadow #2a1f15 leg
shadow, fur highlight #8b6f47 thigh highlight, dark hoof #1a1a1a
cloven hoof tip, ink black #1a1a1a outline.

Composition: 144x224 px (vertical), isolated single front leg on
transparent, top edge clean at shoulder joint (pivot), bottom at
hoof, NO body.
```

```
=== boar/leg_left.png === / === boar/leg_right.png === (BACK LEGS)

hand-painted painterly, asian wuxia.

Subject: side view of a wild boar's BACK LEG, sturdy planted pose,
heavy hind limb with thick haunch muscle (boar charge anatomy —
explosive thigh), bristly fur, **black cloven hoof** at bottom, NO
body, NO ground contact.

Palette: same boar palette — dark brown, deep shadow, fur highlight,
dark hoof, ink outline.

Composition: 160x240 px (vertical, slightly taller than front leg
to hint hindquarter dominance), isolated single back leg, top edge
clean at hip joint (pivot), bottom at hoof, NO body.
```

```
=== boar/tail.png ===

hand-painted painterly, asian wuxia.

Subject: side view of a wild boar's SHORT BRISTLY TAIL — short stiff
stub (~half the length of wolf tail, NOT puffy NOT swishy), straight
horizontal pose with slight twist, bristly dark fur with coarse
clumps at tip, oriented HORIZONTAL, attaches at LEFT edge (root),
tail tip extends to RIGHT, NO body.

Palette: deep shadow #2a1f15 base fur, dark brown #4d3a28 mid-tone,
fur highlight #8b6f47 tip bristle, ink black #1a1a1a outline.

Composition: 128x96 px (short rectangle — tail is stub-length),
isolated tail on transparent, root edge LEFT (pivot point at hip
attachment), NO body.
```

### 3.3.7 Deer Spirit — Linh Lộc Antlered Spirit (`Art/Characters/deer_spirit/`)

> **Concept:** mystical forest deer (linh lộc) — slender graceful quadruped, cream/fawn fur with white belly, **branching ivory antlers with subtle qi-green glow at tips** (hero feature visible from side), large gentle dark eye, long slender legs built for prancing, short white tail flick (raised when alert). NOT a generic doe — wuxia spirit beast with subtle ethereal qi presence. Antler size moderate (4-prong fork), NOT massive elk rack.
>
> **Scale note:** deer ~slimmer silhouette than wolf (28px placeholder height vs 32px). Body slender — torso narrower proportions (240×176 vs wolf 256×192). Legs longer relative to body.

```
=== deer_spirit/head.png ===

hand-painted painterly, asian wuxia, spirit beast.

Subject: side view of a spirit deer HEAD ONLY, profile facing right,
elegant elongated face with delicate muzzle, large gentle dark eye
with subtle qi shimmer, twitching black nose, **PAIR OF BRANCHING
IVORY ANTLERS** rising from forehead (hero feature — 4-prong fork
each side, ivory bone with subtle pale jade green qi glow at tips),
tall pointed ear pricked alertly, slender neck stub, NO body.

Palette: cream fawn #b89968 fur base, warm shadow #6b5237 deep
shadow, fur highlight #d4b896 cheek and forehead highlight, ivory
#c4a574 antler base, jade green #a8c69b faint qi glow at antler
tips, dark nose #1a1a1a eye and nose, white #e8d5a6 inner ear and
muzzle.

Composition: 256x288 px (TALLER than wolf head — antlers add
vertical extent), isolated head on transparent background, antlers
fully visible (do NOT crop tips), NO body, NO ground, NO shadow.
```

```
=== deer_spirit/torso.png ===

hand-painted painterly, asian wuxia, spirit beast.

Subject: SIDE VIEW of a spirit deer BODY ONLY (no head, no legs, no
tail), oriented HORIZONTAL with shoulder-end on RIGHT, hip-end on
LEFT, slender elegant torso (NOT bulky — graceful prancer
proportions), cream fawn fur on back fading to white belly underside
(deer countershading — pale belly is signature visual), neutral
horizontal pose. Clean cuts at neck (right edge), hips (left edge),
shoulders/hip joints (bottom edge for legs).

Palette: cream fawn #b89968 fur base, warm shadow #6b5237 belly
fade shadow, fur highlight #d4b896 spine ridge highlight, white
#e8d5a6 belly underside (countershading), ink black #1a1a1a outline.

Composition: 240x176 px (HORIZONTAL — deer body slimmer than wolf),
isolated body on transparent background, NO head, NO legs, NO tail,
NO ground.
```

```
=== deer_spirit/arm_left.png === / === deer_spirit/arm_right.png === (FRONT LEGS)

hand-painted painterly, asian wuxia.

Subject: side view of a spirit deer's FRONT LEG, slender standing
pose, long graceful forelimb (deer legs are MUCH longer + thinner
than wolf — proportionally elegant), cream fawn fur on thigh fading
to darker shin, **dark cloven hoof** at bottom (small split hoof,
delicate), NO body, NO ground contact.

Palette: cream fawn #b89968 fur base, warm shadow #6b5237 leg
shadow, fur highlight #d4b896 thigh highlight, dark hoof #2a1f15
cloven hoof, ink black #1a1a1a outline.

Composition: 128x256 px (vertical, longer than wolf front leg —
deer legs taller), isolated single front leg on transparent, top
edge clean at shoulder joint (pivot), bottom at hoof, NO body.
```

```
=== deer_spirit/leg_left.png === / === deer_spirit/leg_right.png === (BACK LEGS)

hand-painted painterly, asian wuxia.

Subject: side view of a spirit deer's BACK LEG, slender standing
pose with subtle bend at hock joint (deer hock anatomy — bend looks
"backward" vs human knee), long graceful hind limb, cream fawn fur,
**dark cloven hoof** at bottom, NO body, NO ground contact.

Palette: same deer palette — cream fawn, warm shadow, fur highlight,
dark hoof, ink outline.

Composition: 144x288 px (vertical, longest leg sprite — deer
hindquarter taller than forelimb for prancing), isolated single
back leg, top edge clean at hip joint (pivot), bottom at hoof, NO
body.
```

```
=== deer_spirit/tail.png ===

hand-painted painterly, asian wuxia, spirit beast.

Subject: side view of a spirit deer's SHORT WHITE TAIL FLICK — tiny
upturned puff (deer tail signature — small but bright white,
typically raised when alert), oriented HORIZONTAL with slight upward
angle, attaches at LEFT edge (root), tail tip extends to RIGHT but
also UPWARD, white fur with cream fawn base color underneath, NO
body.

Palette: white #e8d5a6 fur tip (brightest), cream fawn #b89968
base color underneath, warm shadow #6b5237 underside shadow, ink
black #1a1a1a outline.

Composition: 96x128 px (small tail, slightly taller than wide due
to upturned angle), isolated tail on transparent, root edge LEFT
(pivot point at hip attachment), NO body.
```

### 3.3.8 Boss — Hắc Vương (Black King) Cursed Overlord (`Art/Characters/boss/`)

> **Concept:** end-game humanoid villain — Asian wuxia DARK CULTIVATOR overlord, tall imposing male figure (older than Player, ~40s), long ink-black hair flowing past shoulders with single SILVER STREAK at temple (cursed power leak), pale ivory skin with faint scar across left eye, cold imperious expression, **black silk robe with crimson blood-red trim** (signature villain palette — antithesis of Player's white-and-blue robe), wide BLACK CLOAK draping behind shoulders, dark obsidian shoulder pauldrons, blood-red waist sash. Hero feature: faint **CRIMSON QI AURA** edge-glow around silhouette (not strong volumetric — just subtle red rim-light). Carries no weapon (boss summons projectiles via raw qi). Mirror Player's L1 puppet structure: head + torso + 2 arms + 2 legs (NO tail — humanoid). Standing tall, NOT crouched.
>
> **Scale note:** placeholder rendered taller than Player (40px height vs Player 32px). Real PNG composition same per-part dimensions as Player (256×256 head, 256×384 torso/arm/leg) — only in-game scale increased via prefab transform.
>
> **Cross-dir consistency rule (CRITICAL):** Khi gen N + S sau master E, dùng EXACT cùng palette (#1a0a0e black robe, #8c1923 crimson trim, #d8c8a8 ivory skin, #1a1a1a hair với #b8b0a4 silver streak) + cùng silhouette anchor (cloak draping behind, sash around waist, hair below shoulders). Mismatch palette → boss biến hình khi xoay direction → silhouette mismatch lộ rõ.

```
=== boss/head.png ===

hand-painted painterly digital illustration, visible brush strokes,
soft cel-shading, asian wuxia dark cultivation aesthetic,
limited color palette, clean readable silhouette, 1.5 to 2 pixel
mid-tone outline (dark crimson tone, NOT pure black).

Subject: side view of an older Asian male DARK CULTIVATOR
overlord HEAD ONLY, profile facing right, cold imperious
expression with narrow eyes, long ink-black hair flowing past
shoulder line with **SILVER STREAK at temple** (signature cursed-
power visual), pale ivory skin with faint diagonal SCAR across
left brow, faint crimson qi shimmer at temple, isolated single
body part on fully transparent background. NO neck visible below
jawline (clean cut).

Palette (use ONLY these): pale ivory skin #d8c8a8 base, deep
shadow #8a6f47 mid-tone, ink black #1a1a1a hair main, silver
streak #b8b0a4 hair highlight at temple, crimson qi #8c1923 faint
glow on temple, scar shadow #5a3520 brow line.

Composition: 256x256 px, isolated single head on transparent
background, vertically centered, hair must NOT crop at top edge
(allow full silhouette), NO body parts below jaw, NO shoulders,
NO neck, NO ground, NO shadow.
```

```
=== boss/torso.png ===

hand-painted painterly digital illustration, visible brush strokes,
soft cel-shading, asian wuxia dark cultivation aesthetic.

Subject: side view of an older male DARK CULTIVATOR overlord
TORSO ONLY, profile facing right, neutral imperious standing pose,
wearing flowing **BLACK SILK ROBE with crimson blood-red trim** at
collar and waist sash, **wide BLACK CLOAK draping behind shoulders**
(visible in profile as silhouette extending behind torso), dark
obsidian shoulder pauldron at top, blood-red waist sash tied at side,
robe falls to mid-thigh, NO head NO arms NO legs visible (clean
cuts at shoulders, hips, neck).

Palette: black robe #1a0a0e base, deep shadow #0a0408 fold shadow,
crimson trim #8c1923 collar and sash accent, obsidian #2a1a2c
shoulder pauldron, blood-red sash highlight #b82838, silver thread
trim #5a4a48 minimal embroidery.

Composition: 256x384 px (vertical), isolated single torso on fully
transparent background, **CLOAK silhouette CAN extend slightly
beyond standard torso width** (cloak is part of torso), NO head,
NO limbs, NO ground, NO shadow. Top edge clean horizontal at
shoulder line (puppet rig pivot).
```

```
=== boss/arm_left.png === / === boss/arm_right.png ===

hand-painted painterly, visible brush strokes, asian wuxia dark.

Subject: side view of an older male dark cultivator's LEFT (or
RIGHT) arm, hanging straight down in neutral imperious pose,
wearing flowing BLACK SILK ROBE sleeve with CRIMSON CUFF trim,
hand visible at bottom (hand resting open with long fingers
slightly curled — sinister but composed), arm length proportional
to mid-thigh, NO body, NO head.

Palette: black robe sleeve #1a0a0e base, deep shadow #0a0408 fold
shadow, pale ivory #d8c8a8 skin tone for hand, crimson cuff trim
#8c1923 narrow band at wrist, silver thread #5a4a48 minimal
embroidery line.

Composition: 256x384 px (vertical), isolated single arm on fully
transparent background, top edge clean horizontal at shoulder
(pivot point), bottom edge at fingertips, NO body, NO ground.

# NOTE: gen 1 lần rồi flip horizontal trong Photopea/GIMP cho arm_right.
# Hoặc gen 2 lần với prompt "right arm" để có asymmetry tự nhiên.
```

```
=== boss/leg_left.png === / === boss/leg_right.png ===

hand-painted painterly, visible brush strokes, asian wuxia dark.

Subject: side view of an older male dark cultivator's LEFT (or
RIGHT) leg, straight imperious standing pose, wearing BLACK SILK
robe pant flowing to ankle, **dark obsidian boot** with subtle
crimson stitching (not cloth wrap like Player — boss wears stiff
boots), NO body, NO foot ground contact (foot just hovering in
neutral pose).

Palette: black robe pant #1a0a0e base, deep shadow #0a0408 fabric
fold, dark obsidian boot #2a1a2c, crimson stitching #8c1923 narrow
boot accent.

Composition: 256x384 px (vertical), isolated single leg on fully
transparent background, top edge clean horizontal at hip (pivot
point), bottom edge at sole of boot, NO body, NO ground, NO shadow.

# NOTE: same arm pattern — gen 1 lần flip cho phần đối xứng OK.
```

### 3.4 Multi-Direction (L3+) Variants

> **Pipeline:** Drop side prompts §3.1/3.2/3.3 vào `{id}/E/{part}.png`, **plus** N + S variants below. PuppetAnimController auto-swap sprite refs theo velocity angle.
>
> **W (West) = flip(E) free** — không cần W folder, controller mirror East sprite runtime.
>
> **Diagonal velocities (NE/SE/NW/SW)** snap về cardinal nearest qua `ComputeDirectionFromVelocity` (hysteresis 8°). Không cần gen 8-dir.
>
> **Cross-dir consistency rule (CRITICAL):** Khi gen N + S variants, PHẢI dùng SAME palette block + SAME style anchor + SAME brush stroke description từ E prompt — chỉ thay đổi VIEW (subject sentence + composition note). Khác palette giữa dir = character "đổi mặt" khi xoay → silhouette mismatch lộ rõ trên mobile.
>
> **Pivot rule:** Same pivot points across all dirs (vai cho arm, hông cho leg). N + S sprites cùng resolution với E variant.

#### 3.4.1 Player — N (Back View) + S (Front View)

```
=== player/N/head.png === (BACK VIEW)

hand-painted painterly digital illustration, visible brush strokes,
soft cel-shading, asian wuxia cultivation fantasy aesthetic,
limited color palette, clean readable silhouette, 1.5 to 2 pixel
mid-tone outline (dark cream tone, NOT pure black).

Subject: BACK VIEW of a young Asian male cultivator HEAD ONLY,
back of head facing camera, ink-black hair tied in a topknot with
bone cream silk ribbon (topknot prominent at back-top), nape of
neck visible at base of skull, smooth jade-pale skin tone on visible
ear edges and nape, NO face features visible (back of skull only),
isolated single body part on fully transparent background.

Palette (use ONLY these): bone cream skin #e8d5a6 base, warm shadow
#b89968 mid-tone, ink black #1a1a1a hair, primary gold #d4a64a
ribbon highlight, jade green #6b8e62 faint qi glow.

Composition: 256x256 px, isolated single head on transparent
background, vertically centered, NO shoulders, NO neck below skull
base, NO ground, NO shadow.
```

```
=== player/S/head.png === (FRONT VIEW)

hand-painted painterly digital illustration, visible brush strokes,
soft cel-shading, asian wuxia cultivation fantasy aesthetic,
limited color palette, clean readable silhouette, 1.5 to 2 pixel
mid-tone outline (dark cream tone, NOT pure black).

Subject: FRONT VIEW of a young Asian male cultivator HEAD ONLY,
face directly facing camera, calm focused expression, ink-black
hair with center-parting and topknot peak visible above forehead,
bone cream silk ribbon at top, smooth jade-pale skin, dark almond
eyes, faint qi glow on both temples (symmetric), thin dark eyebrows,
neutral closed mouth, isolated single body part on fully transparent
background. NO neck visible below jawline.

Palette: same as E.

Composition: 256x256 px, isolated single head on transparent
background, vertically centered, perfectly symmetric front-facing,
NO shoulders, NO neck, NO ground, NO shadow.
```

```
=== player/N/torso.png === (BACK VIEW)

hand-painted painterly digital illustration, visible brush strokes,
soft cel-shading, asian wuxia cultivation fantasy aesthetic.

Subject: BACK VIEW of a young Asian male cultivator TORSO ONLY,
back of torso facing camera, wearing flowing white martial arts
robe — gold embroidery visible on collar back, waist sash tied at
back with knot + tail trailing down center spine, robe falls to
mid-thigh, NO head NO arms NO legs visible (clean cuts at shoulders,
hips, neck base).

Palette: bone cream #e8d5a6 robe base, warm shadow #b89968 fold
shadow at center spine and side seams, primary gold #d4a64a
embroidery accent, jade green #6b8e62 pendant string-tie at back of
neck (front pendant invisible from back), dry leaf #8a6f47 sash
highlight.

Composition: 256x384 px (vertical), isolated single torso on fully
transparent background, NO head, NO limbs, NO ground, NO shadow.
Top edge clean horizontal at shoulder line (puppet rig pivot).
```

```
=== player/S/torso.png === (FRONT VIEW)

hand-painted painterly, asian wuxia.

Subject: FRONT VIEW of a young Asian male cultivator TORSO ONLY,
chest facing camera, wearing flowing white martial arts robe —
gold embroidery visible at collar V-neck and along chest center
seam, green jade pendant on chest (visible directly center), waist
sash tied in front bow with two tails trailing down, robe falls to
mid-thigh, NO head NO arms NO legs visible (clean cuts at shoulders,
hips, neck base).

Palette: same as E (bone cream, warm shadow, primary gold, jade
green, dry leaf).

Composition: 256x384 px (vertical), isolated torso, perfectly
symmetric front-facing, NO head, NO limbs.
```

```
=== player/N/arm_left.png === / === player/N/arm_right.png === (BACK VIEW)

hand-painted painterly, asian wuxia.

Subject: BACK VIEW of a young cultivator's LEFT (or RIGHT) arm,
hanging straight down in neutral pose, viewed from behind — back
of shoulder + elbow + wrist visible, white robe sleeve drape, hand
visible at bottom showing back of hand (knuckles), arm length
proportional to mid-thigh, NO body, NO head.

Palette: same as E.

Composition: 256x384 px (vertical), isolated single arm on fully
transparent background, top edge clean horizontal at shoulder
(pivot point), bottom edge at fingertips, NO body, NO ground.

# NOTE: Same flip pattern as E — gen 1 lần rồi flip horizontal cho mặt đối xứng.
```

```
=== player/S/arm_left.png === / === player/S/arm_right.png === (FRONT VIEW)

hand-painted painterly, asian wuxia.

Subject: FRONT VIEW of a young cultivator's LEFT (or RIGHT) arm,
hanging straight down at side, viewed from front — front of
shoulder + arm + wrist visible, white robe sleeve drape at front,
palm of hand showing fingers slightly curled inward, NO body, NO
head.

Palette: same as E.

Composition: 256x384 px (vertical), isolated single arm, top edge
at shoulder pivot, bottom at fingertips.
```

```
=== player/N/leg_left.png === / === player/N/leg_right.png === (BACK VIEW)

hand-painted painterly, asian wuxia.

Subject: BACK VIEW of a young cultivator's LEFT (or RIGHT) leg,
straight standing pose viewed from behind, white robe pant flowing
down to ankle (back of leg silhouette), fabric shoe with cloth
wrap visible from behind (heel + ankle wrap), NO body, NO foot
ground contact.

Palette: same as E.

Composition: 256x384 px (vertical), isolated single leg, top edge
at hip pivot, bottom at sole, NO body.
```

```
=== player/S/leg_left.png === / === player/S/leg_right.png === (FRONT VIEW)

hand-painted painterly, asian wuxia.

Subject: FRONT VIEW of a young cultivator's LEFT (or RIGHT) leg,
straight standing pose viewed from front, white robe pant draping
at front (front shin + knee silhouette), fabric shoe toe-end
visible (cloth wrap from front), NO body.

Palette: same as E.

Composition: 256x384 px (vertical), isolated single leg, top at
hip pivot, bottom at toe.
```

#### 3.4.2 Wolf — N (Back View) + S (Front View)

> **Quadruped note:** Wolf "torso" trong puppet rig = body horizontal. Khi xoay sang N (back) hoặc S (front), torso trở thành **rear-end view** (N) hoặc **chest view** (S). Front legs ("arms") + back legs ("legs") cũng đổi: N view = back of leg/paw; S view = front of leg/paw + face camera.

```
=== wolf/N/head.png === (BACK VIEW = back of wolf head)

hand-painted painterly, asian wuxia, wilderness creature.

Subject: BACK VIEW of a fierce gray wolf HEAD ONLY, back of skull
facing camera, ears upright + back of ears with darker gray fur
edge visible, neck ruff prominent shaggy fur fading to body, NO
face features (back of head only), NO body.

Palette: slate gray #7a7c80 fur base, shadow stone #5a5d63 deep
shadow, highlight stone #a3a5a8 fur tip highlight, ink black
#1a1a1a back-of-ear inner edges.

Composition: 256x256 px, isolated head on transparent background,
NO body, NO ground, NO shadow.
```

```
=== wolf/S/head.png === (FRONT VIEW = wolf face camera)

hand-painted painterly, asian wuxia, wilderness creature.

Subject: FRONT VIEW of a fierce gray wolf HEAD ONLY, face directly
facing camera, sharp yellow eyes prominent + symmetric, snout
pointing forward with snarling fangs partially visible, ears alert
and slightly back, shaggy gray fur with darker neck ruff visible
at jaw base, NO body.

Palette: same as E (slate gray, shadow stone, highlight stone,
primary gold eye, ink black snout/fang outline, bone white fang).

Composition: 256x256 px, isolated head, perfectly symmetric front-
facing, NO body, NO shadow.
```

```
=== wolf/N/torso.png === (BACK VIEW = wolf rear/back of body)

hand-painted painterly, asian wuxia, wilderness creature.

Subject: BACK VIEW of gray wolf BODY ONLY (no head, no legs, no
tail), oriented VERTICAL — wolf walking AWAY from camera, back
ridge visible from above, hindquarters at bottom, shoulders at top,
shaggy gray fur with darker dorsal stripe along spine, lean
predator silhouette from above. Clean cuts at neck (top edge),
hips (bottom edge), shoulder/hip joints (side edges for legs).

Palette: slate gray #7a7c80 fur base, shadow stone #5a5d63 spine
shadow groove, highlight stone #a3a5a8 back ridge highlight, ink
black #1a1a1a outline at fur edges.

Composition: 256x384 px (VERTICAL — wolf body taller than wide
when viewed from rear), isolated body on transparent, NO head,
NO legs, NO tail, NO ground.
```

```
=== wolf/S/torso.png === (FRONT VIEW = wolf chest facing camera)

hand-painted painterly, asian wuxia, wilderness creature.

Subject: FRONT VIEW of gray wolf BODY ONLY, oriented VERTICAL —
wolf facing camera, chest + belly visible from front, shoulders at
top widening to ribcage, narrow waist, shaggy fur with paler chest
ruff highlight, NO head, NO legs, NO tail.

Palette: same as E + bone white #c2c4ba chest ruff highlight.

Composition: 256x384 px (vertical), isolated body, perfectly
symmetric front-facing, NO head, NO legs.
```

```
=== wolf/N/arm_left.png === / === wolf/N/arm_right.png === (BACK VIEW front legs)

hand-painted painterly, asian wuxia.

Subject: BACK VIEW of gray wolf FRONT LEG, viewed from behind +
slightly above, lean muscular leg straight standing, gray fur back-
of-leg silhouette, paw with claws partially visible from behind,
NO body.

Palette: same as E.

Composition: 192x320 px (vertical), isolated single front leg, top
edge clean at shoulder pivot, bottom at paw, NO body.
```

```
=== wolf/S/arm_left.png === / === wolf/S/arm_right.png === (FRONT VIEW front legs)

hand-painted painterly, asian wuxia.

Subject: FRONT VIEW of gray wolf FRONT LEG, viewed from front, lean
muscular leg straight standing, gray fur front-of-leg + chest fade,
paw with sharp claws visible at bottom (toes pointing toward camera),
NO body.

Palette: same as E.

Composition: 192x320 px (vertical), isolated single front leg, top
at shoulder, bottom at paw with claws frontal.
```

```
=== wolf/N/leg_left.png === / === wolf/N/leg_right.png === (BACK VIEW back legs)

hand-painted painterly, asian wuxia.

Subject: BACK VIEW of gray wolf BACK LEG, viewed from behind,
strong haunch muscle visible at top, hock + paw visible at bottom,
gray fur, slight crouch in standing pose (powerful hindquarter),
NO body.

Palette: same as E.

Composition: 192x320 px (vertical), isolated single back leg, top
at hip pivot, bottom at paw.
```

```
=== wolf/S/leg_left.png === / === wolf/S/leg_right.png === (FRONT VIEW back legs)

hand-painted painterly, asian wuxia.

Subject: FRONT VIEW of gray wolf BACK LEG, viewed from front, strong
haunch with subtle muscle visible from front angle, hock + paw with
claws facing camera, NO body.

Palette: same as E.

Composition: 192x320 px (vertical), isolated single back leg, top
at hip pivot, bottom at paw.
```

```
=== wolf/N/tail.png === (BACK VIEW tail)

hand-painted painterly, asian wuxia.

Subject: BACK VIEW of gray wolf bushy TAIL, oriented VERTICAL,
attaches at TOP edge (root, hip attachment), tip flowing DOWN at
relaxed neutral hang straight down, shaggy fur viewed from behind
showing dorsal stripe darker line, NO body.

Palette: same as E.

Composition: 192x320 px (VERTICAL — tail straight down from rear
view), isolated tail on transparent, root TOP pivot, tip BOTTOM,
NO body.
```

```
=== wolf/S/tail.png === (FRONT VIEW tail — usually invisible)

# NOTE: Front view của wolf hầu như không thấy tail (body chắn).
# Option 1 (recommended): để tail.png ở S dir RỖNG — controller fallback East sprite cho slot này.
# Option 2 (full coverage): gen tail tip subtly peeking quanh hông trái/phải — nhưng asymmetric khó match cross-dir.

Skip recommended.
```

#### 3.4.3 FoxSpirit — N (Back View) + S (Front View)

> **Same quadruped logic** as Wolf. Fox spirit thanh thoát hơn — back view phải cho thấy spirit qi blue trail nổi rõ trên dorsal ridge (hero feature). Front view nhấn mạnh ethereal mist + glowing eyes symmetric.

```
=== fox_spirit/N/head.png === (BACK VIEW)

hand-painted painterly, asian wuxia cultivation, supernatural ethereal.

Subject: BACK VIEW of a mystical white nine-tailed fox spirit HEAD
ONLY, back of skull facing camera, sharp ears upright with alert
tip + back of ears showing pale gold inner glow at edges, fine white
fur with strong spirit qi blue glow trail starting at back of skull
flowing down into nape, NO face features (back of head only), NO
body.

Palette: bone cream #e8d5a6 fur base, bone bleached #d4d4d4
highlight, spirit qi blue #a8d8ff glow trail dominant at nape,
primary gold #d4a64a inner ear gold tone (visible from back as edge
glow), ink black #1a1a1a fine outline.

Composition: 256x256 px, isolated head, NO body, ethereal soft
edge but clean silhouette.
```

```
=== fox_spirit/S/head.png === (FRONT VIEW)

hand-painted painterly, asian wuxia cultivation, supernatural ethereal.

Subject: FRONT VIEW of mystical white nine-tailed fox spirit HEAD
ONLY, face directly facing camera, glowing pale blue cunning eyes
prominent and symmetric (hero feature), sharp ears upright + slightly
flared, tiny fang visible at slight smirk, fine white fur framing
face with faint blue qi glow at both temples (symmetric), NO body.

Palette: same as E.

Composition: 256x256 px, isolated head, perfectly symmetric front-
facing, NO body, ethereal soft edge.
```

```
=== fox_spirit/N/torso.png === (BACK VIEW)

hand-painted painterly, asian wuxia, supernatural ethereal.

Subject: BACK VIEW of white fox spirit BODY ONLY, oriented VERTICAL
— fox walking away from camera, back ridge prominent with strong
spirit qi blue glow trail along entire spine (hero feature visible
from this angle), lithe slender silhouette, fine snowy fur fading
to ethereal mist at lower hindquarters dissolving into transparent.
Clean cuts at neck (top), hips (bottom), shoulders/hip joints (sides).

Palette: same as E with EMPHASIS on spirit qi blue #a8d8ff dorsal
trail (rendered ~30% stronger than side view since back is dominant
showcase).

Composition: 256x384 px (vertical), isolated body on transparent,
NO head, NO legs, NO tail.
```

```
=== fox_spirit/S/torso.png === (FRONT VIEW)

hand-painted painterly, asian wuxia, supernatural ethereal.

Subject: FRONT VIEW of white fox spirit BODY ONLY, oriented VERTICAL
— fox facing camera, chest + belly visible from front, shoulders at
top, narrow waist, fine snowy fur with paler chest highlight, faint
ethereal mist wisps trailing at lower belly dissolving into
transparent, subtle blue qi glow at chest center (heart area), NO
head, NO legs, NO tail.

Palette: same as E + sky qi mid #6fb5e0 chest mist accent.

Composition: 256x384 px (vertical), isolated body, perfectly
symmetric front-facing, ethereal edge.
```

```
=== fox_spirit/N/arm_left.png === / === fox_spirit/N/arm_right.png === (BACK VIEW front legs)

hand-painted painterly, supernatural fox spirit.

Subject: BACK VIEW of white fox spirit FRONT LEG, viewed from
behind, slender lithe leg straight standing, fine white fur back-
of-leg silhouette with faint blue qi outline at hip attachment,
delicate paw with small claws visible from behind, NO body.

Palette: same as E.

Composition: 160x288 px (vertical), isolated single front leg, top
edge at shoulder joint pivot, bottom at paw.
```

```
=== fox_spirit/S/arm_left.png === / === fox_spirit/S/arm_right.png === (FRONT VIEW front legs)

hand-painted painterly, supernatural fox spirit.

Subject: FRONT VIEW of white fox spirit FRONT LEG, viewed from
front, slender lithe leg, fine white fur with faint blue qi outline
at front of shoulder, delicate paw with small claws facing camera
at bottom, NO body.

Palette: same as E.

Composition: 160x288 px (vertical), isolated single front leg, top
at shoulder, bottom at paw.
```

```
=== fox_spirit/N/leg_left.png === / === fox_spirit/N/leg_right.png === (BACK VIEW back legs)

hand-painted painterly, supernatural fox spirit.

Subject: BACK VIEW of white fox spirit BACK LEG, viewed from behind,
slender powerful haunch with subtle muscle from rear angle, fine
white fur with faint blue qi line at hip, delicate paw at bottom,
NO body.

Palette: same as E.

Composition: 160x288 px (vertical), isolated single back leg, top
at hip pivot, bottom at paw.
```

```
=== fox_spirit/S/leg_left.png === / === fox_spirit/S/leg_right.png === (FRONT VIEW back legs)

hand-painted painterly, supernatural fox spirit.

Subject: FRONT VIEW of white fox spirit BACK LEG, viewed from front,
slender haunch with subtle blue qi glow at hip, delicate paw with
claws visible at bottom facing camera, NO body.

Palette: same as E.

Composition: 160x288 px (vertical), isolated single back leg, top
at hip, bottom at paw.
```

```
=== fox_spirit/N/tail.png === (BACK VIEW tail — hero feature from rear!)

hand-painted painterly, supernatural ethereal.

Subject: BACK VIEW of magnificent fox spirit TAIL — single tail
fluffy and majestic, oriented VERTICAL with attachment ROOT at TOP
(hip back) and tip flowing DOWN with graceful arc to one side, fine
white fur with VERY STRONG spirit qi blue glow trail dominant along
tail length (tail is hero feature from this angle, viewer's eye
naturally drawn here), ethereal mist wisps fading at tip.

Palette: bone cream #e8d5a6 fur base, bone bleached #d4d4d4, spirit
qi blue #a8d8ff dominant glow accent (40% stronger than side view),
sky qi mid #6fb5e0 mid-tone, primary gold #d4a64a inner glow at
root.

Composition: 256x320 px (vertical — tail trailing down from rear
view), isolated tail on transparent, root TOP pivot, tip BOTTOM,
NO body.
```

```
=== fox_spirit/S/tail.png === (FRONT VIEW tail — usually invisible)

# NOTE: Same as wolf/S/tail.png — front view fox usually hidden tail.
# Skip recommended → controller fallback East sprite cho slot này.

Skip recommended.
```

#### 3.4.4 Rabbit — N (Back View) + S (Front View)

> **Cross-dir consistency rule:** Same palette + same brush stroke as §3.3.5 E. Only VIEW changes (subject sentence + composition note).
>
> **Hero feature note:** Rabbit's puffy white cottontail is dominant from N (back view) — viewer sees the fluffy white puff prominently when rabbit hops away. From S (front view) the tail is fully hidden behind body → SKIP `S/tail.png` (controller fallback to E sprite).

```
=== rabbit/N/head.png === (BACK VIEW)

hand-painted painterly, asian wuxia, wilderness creature.

Subject: BACK VIEW of a small alert rabbit HEAD ONLY, back of skull
facing camera, long upright ears prominent (silhouette dominated by
two erect ears with cream inner-ear edge visible from behind),
warm brown fur on back of head, tiny tuft of cream fur at nape, NO
face features (back of head only), NO body.

Palette: same as E (tan brown #8b6f47 fur base, warm shadow #5a4a3a,
fur highlight #b89968 ear back, cream #e8d5a6 inner ear edge).

Composition: 192x192 px, isolated head, NO body, NO ground.
```

```
=== rabbit/S/head.png === (FRONT VIEW)

hand-painted painterly, asian wuxia, wilderness creature.

Subject: FRONT VIEW of small alert rabbit HEAD ONLY, face directly
facing camera, two long upright ears symmetric on top of head, two
small dark watchful eyes prominent and symmetric (hero feature),
twitching nose centered, cream cheek fur framing face, whiskers
spreading symmetrically, NO body.

Palette: same as E.

Composition: 192x192 px, isolated head, perfectly symmetric front-
facing, NO body.
```

```
=== rabbit/N/torso.png === (BACK VIEW)

hand-painted painterly, asian wuxia, wilderness creature.

Subject: BACK VIEW of small rabbit BODY ONLY, oriented VERTICAL —
rabbit hopping away from camera, back ridge prominent with warm
brown fur fading to slightly paler at hindquarter top, compact
rounded silhouette tapering toward hip area at bottom (where tail
attaches), NO head, NO legs, NO tail. Clean cuts at neck (top),
hips (bottom — tail attachment line clean), shoulders/hip joints
(sides for legs).

Palette: same as E (tan brown back fur dominant, warm shadow at
fur folds, fur highlight at back ridge).

Composition: 192x256 px (vertical), isolated body on transparent,
NO head, NO legs, NO tail.
```

```
=== rabbit/S/torso.png === (FRONT VIEW)

hand-painted painterly, asian wuxia, wilderness creature.

Subject: FRONT VIEW of small rabbit BODY ONLY, oriented VERTICAL
— rabbit facing camera, chest + belly visible from front (cream
belly dominant from this angle — hero feature), narrow shoulders at
top, compact rounded torso, NO head, NO legs, NO tail.

Palette: same as E + emphasis on cream #e8d5a6 belly fur (rendered
~20% larger area than side view since belly is dominant from front).

Composition: 192x256 px (vertical), isolated body, perfectly
symmetric front-facing, NO head, NO legs.
```

```
=== rabbit/N/arm_left.png === / === rabbit/N/arm_right.png === (BACK VIEW front legs)

hand-painted painterly, wilderness creature.

Subject: BACK VIEW of small rabbit FRONT LEG, viewed from behind,
short slender forelimb (rabbit hindquarter-dominant), fine warm
brown fur back-of-leg silhouette, small paw with tiny claws visible
from behind at bottom, NO body.

Palette: same as E.

Composition: 128x192 px (vertical), isolated single front leg, top
edge at shoulder joint pivot, bottom at paw.
```

```
=== rabbit/S/arm_left.png === / === rabbit/S/arm_right.png === (FRONT VIEW front legs)

hand-painted painterly, wilderness creature.

Subject: FRONT VIEW of small rabbit FRONT LEG, viewed from front,
short slender forelimb, warm brown fur, small paw with tiny dark
claws facing camera at bottom, NO body.

Palette: same as E.

Composition: 128x192 px (vertical), isolated single front leg, top
at shoulder, bottom at paw.
```

```
=== rabbit/N/leg_left.png === / === rabbit/N/leg_right.png === (BACK VIEW back legs)

hand-painted painterly, wilderness creature.

Subject: BACK VIEW of small rabbit BACK LEG, viewed from behind,
powerful haunch with crouched fold from rear angle (long thigh +
bent shin folded), warm brown fur dominant on back of leg, small
paw at bottom, NO body.

Palette: same as E.

Composition: 192x256 px (vertical, taller than front leg), isolated
single back leg, top at hip pivot, bottom at paw.
```

```
=== rabbit/S/leg_left.png === / === rabbit/S/leg_right.png === (FRONT VIEW back legs)

hand-painted painterly, wilderness creature.

Subject: FRONT VIEW of small rabbit BACK LEG, viewed from front,
powerful crouched haunch (hopper anatomy visible from front —
muscular thigh forward), warm brown fur with cream paw highlight,
small claws facing camera at bottom, NO body.

Palette: same as E.

Composition: 192x256 px (vertical), isolated single back leg, top
at hip, bottom at paw.
```

```
=== rabbit/N/tail.png === (BACK VIEW tail — hero feature from rear!)

hand-painted painterly, wilderness creature.

Subject: BACK VIEW of small rabbit puffy COTTONTAIL — short round
cloud-like tuft (hero feature from rear angle — viewer's eye
naturally drawn to this fluffy white puff when rabbit hops away),
oriented with attachment ROOT at TOP (hip back), tail body extends
slightly DOWN as compact round puff, bright cream-white fur with
soft shadow underneath, NO body.

Palette: bone bleached #d4d4d4 fur tip dominant (brightest area —
40% stronger than side view since cottontail is dominant showcase),
cream #e8d5a6 core, warm shadow #5a4a3a underside shadow.

Composition: 128x128 px (square — cottontail nearly round from any
angle), isolated tail on transparent, root TOP pivot, NO body.
```

```
=== rabbit/S/tail.png === (FRONT VIEW tail — fully hidden)

# NOTE: Same as wolf/S/tail.png and fox_spirit/S/tail.png — front
# view rabbit body fully hides cottontail behind torso.
# Skip recommended → controller fallback East sprite cho slot này.

Skip recommended.
```

#### 3.4.5 Boar — N (Back View) + S (Front View)

> **Cross-dir consistency rule:** Same palette + same brush stroke as §3.3.6 E. Only VIEW changes (subject sentence + composition note).
>
> **Hero feature note:** Boar's signature shoulder hump + spine bristle ridge dominant from N (back view). From S (front view) the **two curved tusks visible face-on** — most menacing angle. Stub tail fully hidden behind body from S → SKIP `S/tail.png`.

```
=== boar/N/head.png === (BACK VIEW)

hand-painted painterly, asian wuxia, wilderness creature.

Subject: BACK VIEW of a wild boar HEAD ONLY, back of skull facing
camera, thick neck stub prominent, short pointed ears pricked
forward (visible from behind as twin pointed silhouette), bristly
dark fur on nape and skull crest, NO face features (back of head
only), NO body.

Palette: same as E (dark brown #4d3a28 fur base, deep shadow
#2a1f15, fur highlight #8b6f47 nape ridge, ink black outline).

Composition: 256x256 px, isolated head, NO body, NO ground.
```

```
=== boar/S/head.png === (FRONT VIEW)

hand-painted painterly, asian wuxia, wilderness creature.

Subject: FRONT VIEW of wild boar HEAD ONLY, face directly facing
camera, **TWO CURVED IVORY TUSKS visible face-on rising upward
symmetric** (hero feature from front — most menacing angle), broad
flat snout centered with twin nostrils, two small angry beady eyes
symmetric (NOT cute — feral aggressive), short pointed ears pricked
forward symmetric, bristly dark fur framing face, NO body.

Palette: same as E.

Composition: 256x256 px, isolated head, perfectly symmetric front-
facing, tusks fully visible (do NOT crop), NO body.
```

```
=== boar/N/torso.png === (BACK VIEW)

hand-painted painterly, asian wuxia, wilderness creature.

Subject: BACK VIEW of wild boar BODY ONLY, oriented VERTICAL — boar
charging away from camera, **prominent shoulder hump + spine bristle
ridge dominant** (hero feature from rear — coarse bristle clumps
along spine), heavy muscular silhouette tapering toward hip area at
bottom, NO head, NO legs, NO tail.

Palette: same as E + emphasis on fur highlight #8b6f47 spine bristle
ridge (rendered ~30% stronger than side view).

Composition: 208x288 px (vertical), isolated body on transparent,
NO head, NO legs, NO tail.
```

```
=== boar/S/torso.png === (FRONT VIEW)

hand-painted painterly, asian wuxia, wilderness creature.

Subject: FRONT VIEW of wild boar BODY ONLY, oriented VERTICAL —
boar facing camera head-on, broad muscular chest dominant (boar
chest plate hero feature from front — wider than wolf/deer), thick
neck stub joining at top, low-slung belly tapering at bottom, NO
head, NO legs, NO tail.

Palette: same as E.

Composition: 208x288 px (vertical), isolated body, perfectly
symmetric front-facing, NO head, NO legs.
```

```
=== boar/N/arm_left.png === / === boar/N/arm_right.png === (BACK VIEW front legs)

hand-painted painterly, wilderness creature.

Subject: BACK VIEW of wild boar FRONT LEG, viewed from behind,
short heavy forelimb (boar legs short for body bulk), bristly dark
brown fur on back of leg, **black cloven hoof** at bottom from rear
angle, NO body.

Palette: same as E.

Composition: 144x224 px (vertical), isolated single front leg, top
edge at shoulder joint pivot, bottom at hoof.
```

```
=== boar/S/arm_left.png === / === boar/S/arm_right.png === (FRONT VIEW front legs)

hand-painted painterly, wilderness creature.

Subject: FRONT VIEW of wild boar FRONT LEG, viewed from front,
short heavy forelimb planted forward, bristly dark fur, **black
cloven hoof** facing camera at bottom, NO body.

Palette: same as E.

Composition: 144x224 px (vertical), isolated single front leg, top
at shoulder, bottom at hoof.
```

```
=== boar/N/leg_left.png === / === boar/N/leg_right.png === (BACK VIEW back legs)

hand-painted painterly, wilderness creature.

Subject: BACK VIEW of wild boar BACK LEG, viewed from behind,
heavy hindquarter with thick haunch muscle from rear angle (charge
muscle hero from rear), bristly fur, **black cloven hoof** at
bottom from rear, NO body.

Palette: same as E.

Composition: 160x240 px (vertical, slightly taller than front leg),
isolated single back leg, top at hip pivot, bottom at hoof.
```

```
=== boar/S/leg_left.png === / === boar/S/leg_right.png === (FRONT VIEW back legs)

hand-painted painterly, wilderness creature.

Subject: FRONT VIEW of wild boar BACK LEG, viewed from front,
heavy haunch visible from front, bristly fur, **black cloven hoof**
facing camera at bottom, NO body.

Palette: same as E.

Composition: 160x240 px (vertical), isolated single back leg, top
at hip, bottom at hoof.
```

```
=== boar/N/tail.png === (BACK VIEW tail — short stub from rear)

hand-painted painterly, wilderness creature.

Subject: BACK VIEW of wild boar SHORT BRISTLY TAIL — tiny stiff
stub viewed from rear (less prominent than wolf/fox tail since
boar tail is bristle stub, not swishy plume), oriented with
attachment ROOT at TOP (hip back), tail extends slightly DOWN as
short coarse bristle clump, dark fur, NO body.

Palette: same as E (deep shadow + dark brown + fur highlight at
bristle tip).

Composition: 96x128 px (small rectangle — tail is stub from any
angle), isolated tail on transparent, root TOP pivot, NO body.
```

```
=== boar/S/tail.png === (FRONT VIEW tail — fully hidden)

# NOTE: Same as wolf/S/tail.png — front view boar body fully hides
# stub tail behind torso. Boar tail is even less visible than wolf
# tail since it's stub-length.
# Skip recommended → controller fallback East sprite cho slot này.

Skip recommended.
```

#### 3.4.6 Deer Spirit — N (Back View) + S (Front View)

> **Cross-dir consistency rule:** Same palette + same brush stroke as §3.3.7 E. Only VIEW changes (subject sentence + composition note).
>
> **Hero feature note:** Deer's antler silhouette dominant from N (back view) — viewer sees branching antler fork from behind framed by ears, plus white tail flick raised when alert. From S (front view) the **antlers visible face-on symmetric** — most regal angle, plus white belly + chest dominant. Tail fully hidden behind body from S → SKIP `S/tail.png`.

```
=== deer_spirit/N/head.png === (BACK VIEW)

hand-painted painterly, asian wuxia, spirit beast.

Subject: BACK VIEW of spirit deer HEAD ONLY, back of skull facing
camera, **PAIR OF BRANCHING IVORY ANTLERS rising prominently from
forehead** (hero feature from rear — 4-prong fork each side, ivory
bone with subtle pale jade green qi glow at tips), tall pointed
ears pricked alertly visible from behind framing antler base, cream
fawn fur on nape, NO face features (back of head only), NO body.

Palette: same as E (cream fawn #b89968 fur base, warm shadow #6b5237,
ivory #c4a574 antler, jade green #a8c69b qi tip glow, fur highlight
#d4b896 nape).

Composition: 256x288 px (TALLER than wolf head — antlers add
vertical extent), isolated head, antlers fully visible (do NOT
crop tips), NO body.
```

```
=== deer_spirit/S/head.png === (FRONT VIEW)

hand-painted painterly, asian wuxia, spirit beast.

Subject: FRONT VIEW of spirit deer HEAD ONLY, face directly facing
camera, **PAIR OF BRANCHING IVORY ANTLERS rising symmetric face-on
visible** (hero feature from front — 4-prong fork each side framing
face like a crown, ivory bone with pale jade green qi glow tips),
two large gentle dark eyes prominent and symmetric (most expressive
angle), twitching black nose centered, tall pointed ears pricked
alertly symmetric, cream fawn cheek fur framing face, NO body.

Palette: same as E.

Composition: 256x288 px (taller — antlers visible), isolated head,
perfectly symmetric front-facing, antlers fully visible, NO body.
```

```
=== deer_spirit/N/torso.png === (BACK VIEW)

hand-painted painterly, asian wuxia, spirit beast.

Subject: BACK VIEW of spirit deer BODY ONLY, oriented VERTICAL —
deer prancing away from camera, slender elegant silhouette with
cream fawn fur on back, narrow shoulders at top, hip area at bottom
where tail attaches, **white belly hint visible on flanks** (deer
countershading shows on flanks even from rear), NO head, NO legs,
NO tail.

Palette: same as E.

Composition: 176x240 px (vertical, slimmer than wolf), isolated body
on transparent, NO head, NO legs, NO tail.
```

```
=== deer_spirit/S/torso.png === (FRONT VIEW)

hand-painted painterly, asian wuxia, spirit beast.

Subject: FRONT VIEW of spirit deer BODY ONLY, oriented VERTICAL —
deer facing camera head-on, **white belly + chest dominant from
front** (hero feature from this angle — countershading creates
striking white-on-fawn silhouette), narrow elegant shoulders at top,
slim chest fading to white belly at bottom, NO head, NO legs, NO
tail.

Palette: same as E + emphasis on white #e8d5a6 belly underside
(rendered ~30% larger area than side view).

Composition: 176x240 px (vertical), isolated body, perfectly
symmetric front-facing, NO head, NO legs.
```

```
=== deer_spirit/N/arm_left.png === / === deer_spirit/N/arm_right.png === (BACK VIEW front legs)

hand-painted painterly, spirit beast.

Subject: BACK VIEW of spirit deer FRONT LEG, viewed from behind,
long graceful slender forelimb (deer legs much taller + thinner
than wolf), cream fawn fur fading to darker shin from rear angle,
**dark cloven hoof** at bottom from behind, NO body.

Palette: same as E.

Composition: 128x256 px (vertical, longer than wolf front leg),
isolated single front leg, top edge at shoulder joint pivot, bottom
at hoof.
```

```
=== deer_spirit/S/arm_left.png === / === deer_spirit/S/arm_right.png === (FRONT VIEW front legs)

hand-painted painterly, spirit beast.

Subject: FRONT VIEW of spirit deer FRONT LEG, viewed from front,
long graceful slender forelimb planted forward, cream fawn fur,
**dark cloven hoof** facing camera at bottom, NO body.

Palette: same as E.

Composition: 128x256 px (vertical), isolated single front leg, top
at shoulder, bottom at hoof.
```

```
=== deer_spirit/N/leg_left.png === / === deer_spirit/N/leg_right.png === (BACK VIEW back legs)

hand-painted painterly, spirit beast.

Subject: BACK VIEW of spirit deer BACK LEG, viewed from behind,
long slender hind limb with subtle bend at hock (deer hock anatomy
— bend looks "backward" vs human knee, visible from rear), cream
fawn fur, **dark cloven hoof** at bottom from rear, NO body.

Palette: same as E.

Composition: 144x288 px (vertical, longest leg sprite), isolated
single back leg, top at hip pivot, bottom at hoof.
```

```
=== deer_spirit/S/leg_left.png === / === deer_spirit/S/leg_right.png === (FRONT VIEW back legs)

hand-painted painterly, spirit beast.

Subject: FRONT VIEW of spirit deer BACK LEG, viewed from front,
long slender hind limb visible from front, cream fawn fur, **dark
cloven hoof** facing camera at bottom, NO body.

Palette: same as E.

Composition: 144x288 px (vertical), isolated single back leg, top
at hip, bottom at hoof.
```

```
=== deer_spirit/N/tail.png === (BACK VIEW tail — white flick raised hero from rear)

hand-painted painterly, spirit beast.

Subject: BACK VIEW of spirit deer SHORT WHITE TAIL FLICK — tiny
upturned puff viewed from rear (hero feature from this angle when
deer alert — bright white flick stands out against fawn back),
oriented with attachment ROOT at TOP (hip back), tail body extends
slightly UPWARD as small white puff, white fur with cream fawn
underneath, NO body.

Palette: white #e8d5a6 fur tip dominant (brightest — 40% stronger
than side view since white flick is signature alert showcase),
cream fawn #b89968 base color, warm shadow #6b5237 underside.

Composition: 96x128 px (small tail upturned), isolated tail on
transparent, root TOP pivot, NO body.
```

```
=== deer_spirit/S/tail.png === (FRONT VIEW tail — fully hidden)

# NOTE: Same as wolf/S/tail.png — front view deer body fully hides
# white tail flick behind torso.
# Skip recommended → controller fallback East sprite cho slot này.

Skip recommended.
```

#### 3.4.7 Boss — N (Back View) + S (Front View)

> **Cross-dir consistency rule (CRITICAL):** giữ EXACT palette E (#1a0a0e black robe, #8c1923 crimson trim, #d8c8a8 ivory skin, #1a1a1a hair với #b8b0a4 silver streak) + cùng silhouette anchor (cloak draping behind, sash around waist). Hero feature **CLOAK** is most dramatic from N back-view (cloak swept across full back), most subdued from S front-view (cloak peeks from behind shoulders). Hair flowing visible from BOTH N and S (long enough to fall past shoulder line in both views).

```
=== boss/N/head.png === (BACK VIEW)

hand-painted painterly, asian wuxia dark cultivation.

Subject: BACK view of an older male dark cultivator overlord HEAD
ONLY, viewer behind subject, **long flowing ink-black hair** falls
past shoulder line covering most of head silhouette (signature back-
view feature), **silver streak at TEMPLE visible from behind** as
single lighter strand on right side of hair mass, NO face features
visible (back of head only), nape ~70% covered by hair.

Palette: ink black #1a1a1a hair main, silver streak #b8b0a4 single
strand right side, pale ivory #d8c8a8 small visible neck nape skin,
deep shadow #5a3520 hair shadow under shoulder.

Composition: 256x256 px, head on transparent, hair MUST NOT crop at
side edges (full hair silhouette flows past shoulder line), NO face,
NO body, NO ground.
```

```
=== boss/N/torso.png === (BACK VIEW)

hand-painted painterly, asian wuxia dark cultivation.

Subject: BACK view of an older male dark cultivator overlord
TORSO ONLY, viewer behind subject, **wide BLACK CLOAK draped across
full back** (hero feature dramatic from N — cloak is the dominant
visual mass), crimson trim visible at collar back-of-neck and at
waist sash tied behind, dark obsidian shoulder pauldrons visible at
top corners, robe seams down center spine, NO head, NO arms, NO
legs (clean cuts).

Palette: black robe + cloak #1a0a0e dominant base, deep shadow
#0a0408 cloak fold shadow, crimson trim #8c1923 collar nape + sash
back-knot, obsidian pauldrons #2a1a2c, blood-red sash #b82838 knot
highlight at lower back.

Composition: 256x384 px (vertical), torso on transparent, **CLOAK
fully visible across BACK** (do NOT crop cloak — extends slightly
beyond standard torso width), top edge clean shoulder line, NO
head, NO limbs, NO ground.
```

```
=== boss/N/arm_left.png === / === boss/N/arm_right.png === (BACK VIEW)

hand-painted painterly, asian wuxia dark.

Subject: BACK view of an older male dark cultivator's LEFT (or
RIGHT) arm, viewer behind subject, hanging straight down, BLACK
SILK robe sleeve visible from behind with **crimson cuff trim** at
wrist, hand silhouette at bottom shows back of hand (knuckles
visible — NOT palm), arm length proportional, NO body, NO head.

Palette: black robe sleeve #1a0a0e base, deep shadow #0a0408 fold
shadow, pale ivory #d8c8a8 skin tone for back-of-hand, crimson cuff
#8c1923 narrow band at wrist.

Composition: 256x384 px (vertical), arm on transparent, top edge
clean shoulder pivot, bottom edge at fingertips/knuckles, NO body.
```

```
=== boss/N/leg_left.png === / === boss/N/leg_right.png === (BACK VIEW)

hand-painted painterly, asian wuxia dark.

Subject: BACK view of an older male dark cultivator's LEFT (or
RIGHT) leg, viewer behind subject, straight imperious standing pose,
BLACK SILK robe pant from behind, dark obsidian boot at bottom
(boot back visible — heel + back seam, NOT toe), crimson stitching
narrow band at boot top, NO body, NO foot ground contact.

Palette: black robe pant #1a0a0e base, deep shadow #0a0408 fabric
fold, dark obsidian #2a1a2c boot, crimson stitching #8c1923 narrow
boot accent.

Composition: 256x384 px (vertical), leg on transparent, top edge
clean hip pivot, bottom edge at boot heel, NO body, NO ground.
```

```
=== boss/S/head.png === (FRONT VIEW — face-on)

hand-painted painterly, asian wuxia dark cultivation.

Subject: FRONT view of an older Asian male DARK CULTIVATOR
overlord HEAD ONLY, face-on toward viewer, cold imperious
expression with narrow eyes (face symmetric front-on), thin lips,
**SCAR diagonal across LEFT eyebrow** (visible from front as
signature feature), pale ivory skin, ink-black hair parted center
flowing past shoulders on BOTH sides, **silver streaks at BOTH
temples** (front view shows streaks symmetric — small detail), faint
crimson qi shimmer at temples.

Palette: pale ivory skin #d8c8a8 base, deep shadow #8a6f47 cheek
shadow, ink black #1a1a1a hair, silver streak #b8b0a4 temple
highlights both sides, crimson qi #8c1923 faint glow temples, scar
shadow #5a3520 brow line on left.

Composition: 256x256 px, head on transparent, vertically centered
+ horizontally centered, hair MUST NOT crop at side edges, NO body
parts below jaw, NO neck, NO shoulders, NO ground.
```

```
=== boss/S/torso.png === (FRONT VIEW — face-on)

hand-painted painterly, asian wuxia dark cultivation.

Subject: FRONT view of an older male DARK CULTIVATOR overlord
TORSO ONLY, face-on toward viewer, neutral imperious standing
pose, BLACK SILK ROBE with **CRIMSON BLOOD-RED collar V-neck trim**
front-and-center, **blood-red waist sash tied with knot front-
center** (knot bow visible face-on), dark obsidian shoulder
pauldrons at top corners, **black cloak peeks from BEHIND shoulders**
as 2 narrow vertical strips (cloak is mostly hidden in front view),
NO head, NO arms, NO legs.

Palette: black robe #1a0a0e front base, deep shadow #0a0408 fold,
crimson collar V #8c1923, blood-red sash highlight #b82838 front-
center knot, obsidian pauldrons #2a1a2c shoulder corners, silver
thread #5a4a48 minimal embroidery on collar.

Composition: 256x384 px (vertical), torso on transparent,
horizontally symmetric body silhouette, **cloak narrow strips
visible behind shoulders**, top edge clean shoulder line, NO head,
NO limbs, NO ground.
```

```
=== boss/S/arm_left.png === / === boss/S/arm_right.png === (FRONT VIEW)

hand-painted painterly, asian wuxia dark.

Subject: FRONT view of an older male dark cultivator's LEFT (or
RIGHT) arm, face-on toward viewer, hanging straight down, BLACK
SILK robe sleeve face-on with crimson cuff trim at wrist, hand
silhouette at bottom shows palm-down face-on (back of hand toward
viewer for natural arm pose), NO body, NO head.

Palette: black robe sleeve #1a0a0e base, deep shadow #0a0408 fold,
pale ivory #d8c8a8 skin tone for back-of-hand, crimson cuff #8c1923
narrow band wrist.

Composition: 256x384 px (vertical), arm on transparent, top edge
clean shoulder pivot, bottom edge at fingertips, NO body.
```

```
=== boss/S/leg_left.png === / === boss/S/leg_right.png === (FRONT VIEW)

hand-painted painterly, asian wuxia dark.

Subject: FRONT view of an older male dark cultivator's LEFT (or
RIGHT) leg, face-on toward viewer, straight imperious standing pose,
BLACK SILK robe pant face-on, **dark obsidian boot toe visible**
at bottom (boot front face — toe + arch + crimson stitching trim),
NO body, NO foot ground contact.

Palette: black robe pant #1a0a0e base, deep shadow #0a0408 fold,
dark obsidian #2a1a2c boot front, crimson stitching #8c1923 narrow
boot accent.

Composition: 256x384 px (vertical), leg on transparent, top edge
clean hip pivot, bottom edge at boot toe, NO body, NO ground.
```

---

### 3.5 L2 Joints — Elbow & Knee (Forearm + Shin)

> **Pipeline (PR K):** sau khi puppet hierarchy có L1 7-joint (head/torso/arms/legs/tail), L2 thêm 4 children: `forearm_left/right.png` (child của arm), `shin_left/right.png` (child của leg). Khi controller bend elbow/knee → forearm/shin xoay quanh elbow/knee pivot tạo bend feedback.
>
> **Tối thiểu để L2 active:** drop forearm + shin PNG vào folder. Thiếu → controller fallback rigid arm/leg (như L1).
>
> **Filename convention:** `forearm_left.png`, `forearm_right.png`, `shin_left.png`, `shin_right.png`. Multi-dir → `{E|N|S}/forearm_left.png` etc.
>
> **Pivot tip:** forearm pivot **TOP-CENTER** = elbow joint (nối cuối arm). Shin pivot **TOP-CENTER** = knee joint (nối cuối leg). Bottom edge = wrist (forearm) hoặc ankle (shin).
>
> **Style anchor lock:** PHẢI dùng EXACT palette + brush style của arm/leg parent của character đó. Forearm = "lower half of arm" (artist tách arm thành upper + lower với clean cut ở elbow). Shin = "lower half of leg" tương tự.

#### 3.5.1 Player L2 — Forearm + Shin

```
=== player/forearm_left.png === / === player/forearm_right.png === (E side)

hand-painted painterly digital illustration, visible brush strokes,
soft cel-shading, asian wuxia cultivation fantasy aesthetic,
limited color palette, clean readable silhouette, 1.5 to 2 pixel
mid-tone outline (NOT pure black).

Subject: side view of a young Asian male cultivator's LEFT (or RIGHT)
FOREARM ONLY (lower half of arm from elbow to fingertips), profile
facing right, hanging straight down in neutral pose, wearing flowing
white robe sleeve cuff, hand visible at bottom (open palm, fingers
slightly curled relaxed), NO upper arm, NO body, NO shoulder.
Clean horizontal cut at top edge (elbow joint pivot) — must align
with bottom cut of arm sprite.

Palette: bone cream #e8d5a6 sleeve base, warm shadow #b89968 fold
shadow, jade pale #d4d4ba skin tone for hand, primary gold #d4a64a
narrow trim at cuff, ink black #1a1a1a outline.

Composition: 192x288 px (vertical), isolated single forearm on fully
transparent background, top edge clean horizontal at elbow (pivot),
bottom edge at fingertips, NO body, NO upper arm, NO ground, NO
shadow.

# NOTE: gen 1 lần flip cho phần đối xứng OK. Multi-dir N → swap with
# §3.5.1.N below. S → §3.5.1.S below. Palette/style 100% identical
# với §3.1 player/arm_left — chỉ khác phần body part được render.
```

```
=== player/shin_left.png === / === player/shin_right.png === (E side)

hand-painted painterly digital illustration, visible brush strokes,
soft cel-shading, asian wuxia cultivation fantasy aesthetic.

Subject: side view of a young Asian male cultivator's LEFT (or RIGHT)
SHIN ONLY (lower half of leg from knee to sole), profile facing right,
straight standing in neutral pose, wearing flowing white robe pant
flowing to ankle, fabric shoe with cloth wrap visible at bottom, NO
upper leg, NO body, NO hip. Clean horizontal cut at top edge (knee
joint pivot) — must align with bottom cut of leg sprite.

Palette: bone cream #e8d5a6 pant base, warm shadow #b89968 fabric
fold, dry leaf #8a6f47 shoe, ink black #1a1a1a outline.

Composition: 192x288 px (vertical), isolated single shin on fully
transparent background, top edge clean horizontal at knee (pivot),
bottom edge at sole of foot, NO body, NO upper leg, NO ground, NO
shadow.
```

```
=== player/N/forearm_left.png === / === player/N/forearm_right.png === (BACK VIEW)

hand-painted painterly, visible brush strokes, asian wuxia cultivation.

Subject: BACK VIEW of a young cultivator's FOREARM viewed from behind,
forearm hanging straight down, white robe sleeve cuff, back of hand
visible at bottom (knuckles facing camera, fingers slightly curled),
NO upper arm, NO body. Symmetric back-of-arm silhouette.

Palette: same as §3.5.1 E (bone cream #e8d5a6, warm shadow #b89968,
jade pale #d4d4ba skin, primary gold #d4a64a cuff trim).

Composition: 192x288 px (vertical), isolated forearm, top edge clean
at elbow pivot, bottom at fingertips, transparent background.
```

```
=== player/N/shin_left.png === / === player/N/shin_right.png === (BACK VIEW)

hand-painted painterly, visible brush strokes, asian wuxia.

Subject: BACK VIEW of a young cultivator's SHIN viewed from behind,
straight standing, white robe pant flowing to ankle, back of fabric
shoe (heel + cloth wrap visible from behind), NO upper leg, NO body.

Palette: same as §3.5.1 E.

Composition: 192x288 px (vertical), isolated shin, top at knee pivot,
bottom at heel, transparent.
```

```
=== player/S/forearm_left.png === / === player/S/forearm_right.png === (FRONT VIEW)

hand-painted painterly, visible brush strokes, asian wuxia.

Subject: FRONT VIEW of a young cultivator's FOREARM facing camera,
forearm hanging straight down, robe sleeve cuff visible front, palm
of hand facing forward at bottom (open palm, fingers slightly curled),
symmetric front silhouette, NO upper arm, NO body.

Palette: same as §3.5.1 E.

Composition: 192x288 px (vertical), isolated forearm, top clean at
elbow, bottom at fingertips, transparent.
```

```
=== player/S/shin_left.png === / === player/S/shin_right.png === (FRONT VIEW)

hand-painted painterly, visible brush strokes, asian wuxia.

Subject: FRONT VIEW of a young cultivator's SHIN facing camera, white
robe pant flowing to ankle, fabric shoe toe visible at bottom (cloth
wrap front + sole tip), NO upper leg, NO body, symmetric front
silhouette.

Palette: same as §3.5.1 E.

Composition: 192x288 px (vertical), isolated shin, top at knee, bottom
at toe, transparent.
```

#### 3.5.2 Wolf L2 — Forearm (Front Lower Leg) + Shin (Hind Lower Leg)

> **Quadruped adaptation:** wolf "forearm" = lower half of front leg (knee → paw). wolf "shin" = lower half of hind leg (hock → paw). Naming theo puppet rig spec — KHÔNG đổi vì BootstrapWizard expect filename consts.

```
=== wolf/forearm_left.png === / === wolf/forearm_right.png === (E side)

hand-painted painterly, asian wuxia, wilderness creature, soft cel-shading.

Subject: side view of a gray wolf's FRONT LOWER LEG ONLY (lower half
from knee to paw), profile facing right, lean predator anatomy with
visible wrist tendon, shaggy gray fur, paw with sharp claws and pad
visible at bottom, NO upper leg, NO body, NO chest. Clean horizontal
cut at top edge (knee pivot).

Palette: slate gray #7a7c80 fur base, shadow stone #5a5d63 deep
shadow, highlight stone #a3a5a8 fur tip highlight, ink black #1a1a1a
claw and outline, bone white #c2c4ba claw highlight.

Composition: 160x240 px (vertical), isolated single front lower leg
on transparent, top edge at knee pivot, bottom at paw pad, NO body,
NO ground.
```

```
=== wolf/shin_left.png === / === wolf/shin_right.png === (E side)

hand-painted painterly, asian wuxia, wilderness creature.

Subject: side view of a gray wolf's HIND LOWER LEG ONLY (lower half
from hock to paw), profile facing right, lean predator hind leg with
prominent hock joint at top, shaggy gray fur, paw with claws and pad
at bottom, NO upper leg/haunch, NO body. Clean horizontal cut at top
(hock pivot).

Palette: slate gray #7a7c80 fur base, shadow stone #5a5d63 deep
shadow, highlight stone #a3a5a8 fur tip, ink black #1a1a1a claw and
outline.

Composition: 160x240 px (vertical), isolated single hind lower leg
on transparent, top at hock, bottom at paw, NO body, NO ground.
```

```
=== wolf/N/forearm_left.png === / === wolf/N/forearm_right.png === (BACK VIEW)

hand-painted painterly, asian wuxia, wilderness creature.

Subject: BACK VIEW of gray wolf FRONT LOWER LEG viewed from behind,
back of leg with shaggy fur, paw rear visible at bottom (back of
pad), NO upper leg, NO body.

Palette: same as §3.5.2 E.

Composition: 160x240 px (vertical), isolated, top at knee pivot,
bottom at paw, transparent.
```

```
=== wolf/N/shin_left.png === / === wolf/N/shin_right.png === (BACK VIEW)

hand-painted painterly, asian wuxia, wilderness creature.

Subject: BACK VIEW of gray wolf HIND LOWER LEG viewed from behind,
prominent hock at top, lean rear silhouette, paw rear visible at
bottom, NO body.

Palette: same as §3.5.2 E.

Composition: 160x240 px (vertical), isolated, top at hock, bottom
at paw, transparent.
```

```
=== wolf/S/forearm_left.png === / === wolf/S/forearm_right.png === (FRONT VIEW)

# NOTE: front-view wolf chest blocks most of front lower legs visibility.
# Skip recommended → controller fallback East mirror cho slot này.
# Nếu MUST gen: front view of front lower leg facing camera, paw front
# visible at bottom (toes facing camera), narrow visible silhouette.

Skip recommended.
```

```
=== wolf/S/shin_left.png === / === wolf/S/shin_right.png === (FRONT VIEW)

# NOTE: front-view wolf body blocks hind lower legs.
# Skip recommended.

Skip recommended.
```

#### 3.5.3 FoxSpirit L2 — Forearm (Front Lower Leg) + Shin (Hind Lower Leg)

```
=== fox_spirit/forearm_left.png === / === fox_spirit/forearm_right.png === (E side)

hand-painted painterly, asian wuxia, supernatural ethereal.

Subject: side view of a slender white fox spirit's FRONT LOWER LEG
ONLY (lower half from knee to paw), profile facing right, lithe and
delicate anatomy, fine snowy white fur fading to faint ethereal mist
at lower paw dissolving to transparent, small claws visible, faint
spirit qi blue glow outline at knee, NO upper leg, NO body. Clean cut
at top (knee pivot).

Palette: bone cream #e8d5a6 fur base, bone bleached #d4d4d4 fur
shadow, spirit qi blue #a8d8ff faint glow accent, ink black #1a1a1a
claw and fine outline.

Composition: 160x240 px (vertical), isolated single front lower leg
on transparent, top at knee, bottom at paw (mist fade OK), NO body,
NO ground.
```

```
=== fox_spirit/shin_left.png === / === fox_spirit/shin_right.png === (E side)

hand-painted painterly, asian wuxia, supernatural ethereal.

Subject: side view of a slender white fox spirit's HIND LOWER LEG
ONLY (lower half from hock to paw), profile facing right, lithe with
prominent hock at top, fine snowy white fur, faint blue qi glow at
hock joint, small claws and ethereal mist fade at paw bottom, NO
upper haunch, NO body.

Palette: bone cream #e8d5a6 fur base, bone bleached #d4d4d4 fur
shadow, spirit qi blue #a8d8ff faint glow at joint, ink black #1a1a1a
fine outline and claw.

Composition: 160x240 px (vertical), isolated single hind lower leg
on transparent, top at hock pivot, bottom at paw, NO body.
```

```
=== fox_spirit/N/forearm_left.png === / === fox_spirit/N/forearm_right.png === (BACK VIEW)

hand-painted painterly, asian wuxia, supernatural ethereal.

Subject: BACK VIEW of fox spirit FRONT LOWER LEG viewed from behind,
fine snowy fur, back of paw visible at bottom, faint blue qi glow at
knee from behind, NO upper leg, NO body.

Palette: same as §3.5.3 E.

Composition: 160x240 px (vertical), isolated, top at knee, bottom at
paw, transparent.
```

```
=== fox_spirit/N/shin_left.png === / === fox_spirit/N/shin_right.png === (BACK VIEW)

hand-painted painterly, asian wuxia, supernatural ethereal.

Subject: BACK VIEW of fox spirit HIND LOWER LEG viewed from behind,
prominent hock from rear angle, fine snowy fur with faint blue qi
glow at joint, paw rear visible at bottom, NO body.

Palette: same as §3.5.3 E.

Composition: 160x240 px (vertical), isolated, top at hock, bottom at
paw, transparent.
```

```
=== fox_spirit/S/forearm_left.png === / === fox_spirit/S/forearm_right.png === (FRONT VIEW)

# NOTE: front-view fox chest blocks most front lower leg visibility.
# Skip recommended → controller fallback East mirror cho slot này.

Skip recommended.
```

```
=== fox_spirit/S/shin_left.png === / === fox_spirit/S/shin_right.png === (FRONT VIEW)

# NOTE: front-view fox body blocks hind lower legs.
# Skip recommended.

Skip recommended.
```

### 3.6 Quick-Copy Bundles (đầy đủ, paste tuần tự là xong)

> Section này gom tất cả prompts cần thiết để gen **full DST set** của 1 character vào MỘT block liên tục — tránh phải scroll giữa §3.1 (E side) + §3.4.1 (N back + S front). Đây là yêu cầu trực tiếp từ user feedback PR #105/#106 follow-up: "tôi cần prompt đầy đủ để copy là dùng".
>
> **Maintenance note:** Prompts trong §3.6.x **LÀ COPY** của master prompts ở §3.1 (E) + §3.4.1 (N, S). Nếu sửa master prompt (palette, composition, pivot rule…) **PHẢI** sync xuống §3.6.x tương ứng. Dễ miss → khi review PR thay đổi §3.1/§3.4.1, kiểm tra §3.6 cùng file diff.
>
> **GPT Image 2.0 lưu ý:** mỗi block = 1 generation request. Bundle 12 prompts → 12 lần gen → 18 PNG output (arm/leg block sinh 2 PNG mỗi cái). Cost ước tính: xem §12 (Cost Estimate).

#### 3.6.1 Player — Full DST Set (12 prompts → 18 PNG)

> **Setup trước khi gen:** tạo folder `Assets/_Project/Art/Characters/player/E/`, `player/N/`, `player/S/` (BootstrapWizard sẽ auto-detect khi có sprite).
>
> **Workflow:** copy từng block bên dưới (theo thứ tự E → N → S) → paste vào GPT Image 2.0 → save output PNG vào path ghi ở dòng đầu block. Style anchor + negative prompt §1/§2 đã inline trong từng block — không cần paste riêng.
>
> **W (West)** = mirror East lúc runtime (PuppetAnimController flipX), KHÔNG cần gen.

##### E (East / side view) — 4 prompts → 6 PNG

```
=== Save to: Assets/_Project/Art/Characters/player/E/head.png ===

hand-painted painterly digital illustration, visible brush strokes,
soft cel-shading, asian wuxia cultivation fantasy aesthetic,
limited color palette, clean readable silhouette, 1.5 to 2 pixel
mid-tone outline (dark cream tone, NOT pure black).

Subject: side view of a young Asian male cultivator HEAD ONLY,
profile facing right, calm focused expression, ink-black hair tied
in a topknot with bone cream silk ribbon, smooth jade-pale skin,
faint qi glow on temple, isolated single body part on fully
transparent background. NO neck visible below jawline (clean cut).

Palette (use ONLY these): bone cream skin #e8d5a6 base, warm shadow
#b89968 mid-tone, ink black #1a1a1a hair, primary gold #d4a64a
ribbon highlight, jade green #6b8e62 faint qi glow.

Composition: 256x256 px, isolated single head on transparent
background, vertically centered, NO body parts below jaw, NO
shoulders, NO neck, NO ground, NO shadow.
```

```
=== Save to: Assets/_Project/Art/Characters/player/E/torso.png ===

hand-painted painterly digital illustration, visible brush strokes,
soft cel-shading, asian wuxia cultivation fantasy aesthetic.

Subject: side view of a young Asian male cultivator TORSO ONLY,
profile facing right, neutral standing pose, wearing flowing white
martial arts robe with gold embroidery on collar and waist sash,
green jade pendant on chest, robe falls to mid-thigh, NO head NO
arms NO legs visible (clean cuts at shoulders, hips, neck).

Palette: bone cream #e8d5a6 robe base, warm shadow #b89968 fold
shadow, primary gold #d4a64a embroidery accent, jade green #6b8e62
pendant, dry leaf #8a6f47 sash highlight.

Composition: 256x384 px (vertical), isolated single torso on fully
transparent background, NO head, NO limbs, NO ground, NO shadow.
Top edge clean horizontal at shoulder line (puppet rig pivot).
```

```
=== Save to: Art/Characters/player/E/arm_left.png  AND  arm_right.png ===

hand-painted painterly, visible brush strokes, asian wuxia.

Subject: side view of a young cultivator's LEFT (or RIGHT) arm,
hanging straight down in neutral pose, wearing flowing white robe
sleeve, hand visible at bottom (hand resting open, fingers slightly
curled), arm length proportional to mid-thigh, NO body, NO head.

Palette: bone cream #e8d5a6 sleeve base, warm shadow #b89968 fold
shadow, jade pale #d4d4ba skin tone for hand, primary gold #d4a64a
narrow trim at cuff.

Composition: 256x384 px (vertical), isolated single arm on fully
transparent background, top edge clean horizontal at shoulder
(pivot point), bottom edge at fingertips, NO body, NO ground.

# NOTE: gen 1 lần (left arm), flip horizontal trong Photopea/GIMP để
# có arm_right.png. Hoặc gen 2 lần với prompt "right arm" cho asymmetry tự nhiên.
```

```
=== Save to: Art/Characters/player/E/leg_left.png  AND  leg_right.png ===

hand-painted painterly, visible brush strokes, asian wuxia.

Subject: side view of a young cultivator's LEFT (or RIGHT) leg,
straight standing pose, wearing white robe pant flowing to ankle,
fabric shoe with cloth wrap, NO body, NO foot ground contact (foot
just hovering in neutral pose).

Palette: bone cream #e8d5a6 pant base, warm shadow #b89968 fabric
fold, dry leaf #8a6f47 shoe.

Composition: 256x384 px (vertical), isolated single leg on fully
transparent background, top edge clean horizontal at hip (pivot
point), bottom edge at sole of foot, NO body, NO ground, NO shadow.

# NOTE: gen 1 lần (left leg), flip horizontal cho leg_right.png.
```

##### N (North / back view) — 4 prompts → 6 PNG

```
=== Save to: Assets/_Project/Art/Characters/player/N/head.png ===

hand-painted painterly digital illustration, visible brush strokes,
soft cel-shading, asian wuxia cultivation fantasy aesthetic,
limited color palette, clean readable silhouette, 1.5 to 2 pixel
mid-tone outline (dark cream tone, NOT pure black).

Subject: BACK VIEW of a young Asian male cultivator HEAD ONLY,
back of head facing camera, ink-black hair tied in a topknot with
bone cream silk ribbon (topknot prominent at back-top), nape of
neck visible at base of skull, smooth jade-pale skin tone on visible
ear edges and nape, NO face features visible (back of skull only),
isolated single body part on fully transparent background.

Palette (use ONLY these): bone cream skin #e8d5a6 base, warm shadow
#b89968 mid-tone, ink black #1a1a1a hair, primary gold #d4a64a
ribbon highlight, jade green #6b8e62 faint qi glow.

Composition: 256x256 px, isolated single head on transparent
background, vertically centered, NO shoulders, NO neck below skull
base, NO ground, NO shadow.
```

```
=== Save to: Assets/_Project/Art/Characters/player/N/torso.png ===

hand-painted painterly digital illustration, visible brush strokes,
soft cel-shading, asian wuxia cultivation fantasy aesthetic.

Subject: BACK VIEW of a young Asian male cultivator TORSO ONLY,
back of torso facing camera, wearing flowing white martial arts
robe — gold embroidery visible on collar back, waist sash tied at
back with knot + tail trailing down center spine, robe falls to
mid-thigh, NO head NO arms NO legs visible (clean cuts at shoulders,
hips, neck base).

Palette: bone cream #e8d5a6 robe base, warm shadow #b89968 fold
shadow at center spine and side seams, primary gold #d4a64a
embroidery accent, jade green #6b8e62 pendant string-tie at back of
neck (front pendant invisible from back), dry leaf #8a6f47 sash
highlight.

Composition: 256x384 px (vertical), isolated single torso on fully
transparent background, NO head, NO limbs, NO ground, NO shadow.
Top edge clean horizontal at shoulder line (puppet rig pivot).
```

```
=== Save to: Art/Characters/player/N/arm_left.png  AND  arm_right.png ===

hand-painted painterly, asian wuxia.

Subject: BACK VIEW of a young cultivator's LEFT (or RIGHT) arm,
hanging straight down in neutral pose, viewed from behind — back
of shoulder + elbow + wrist visible, white robe sleeve drape, hand
visible at bottom showing back of hand (knuckles), arm length
proportional to mid-thigh, NO body, NO head.

Palette: bone cream #e8d5a6 sleeve base, warm shadow #b89968 fold
shadow, jade pale #d4d4ba skin tone for hand, primary gold #d4a64a
narrow trim at cuff.

Composition: 256x384 px (vertical), isolated single arm on fully
transparent background, top edge clean horizontal at shoulder
(pivot point), bottom edge at fingertips, NO body, NO ground.

# NOTE: gen 1 lần flip horizontal cho arm đối xứng.
```

```
=== Save to: Art/Characters/player/N/leg_left.png  AND  leg_right.png ===

hand-painted painterly, asian wuxia.

Subject: BACK VIEW of a young cultivator's LEFT (or RIGHT) leg,
straight standing pose viewed from behind, white robe pant flowing
down to ankle (back of leg silhouette), fabric shoe with cloth
wrap visible from behind (heel + ankle wrap), NO body, NO foot
ground contact.

Palette: bone cream #e8d5a6 pant base, warm shadow #b89968 fabric
fold, dry leaf #8a6f47 shoe.

Composition: 256x384 px (vertical), isolated single leg, top edge
at hip pivot, bottom at sole, NO body.
```

##### S (South / front view) — 4 prompts → 6 PNG

```
=== Save to: Assets/_Project/Art/Characters/player/S/head.png ===

hand-painted painterly digital illustration, visible brush strokes,
soft cel-shading, asian wuxia cultivation fantasy aesthetic,
limited color palette, clean readable silhouette, 1.5 to 2 pixel
mid-tone outline (dark cream tone, NOT pure black).

Subject: FRONT VIEW of a young Asian male cultivator HEAD ONLY,
face directly facing camera, calm focused expression, ink-black
hair with center-parting and topknot peak visible above forehead,
bone cream silk ribbon at top, smooth jade-pale skin, dark almond
eyes, faint qi glow on both temples (symmetric), thin dark eyebrows,
neutral closed mouth, isolated single body part on fully transparent
background. NO neck visible below jawline.

Palette (use ONLY these): bone cream skin #e8d5a6 base, warm shadow
#b89968 mid-tone, ink black #1a1a1a hair, primary gold #d4a64a
ribbon highlight, jade green #6b8e62 faint qi glow.

Composition: 256x256 px, isolated single head on transparent
background, vertically centered, perfectly symmetric front-facing,
NO shoulders, NO neck, NO ground, NO shadow.
```

```
=== Save to: Assets/_Project/Art/Characters/player/S/torso.png ===

hand-painted painterly, asian wuxia.

Subject: FRONT VIEW of a young Asian male cultivator TORSO ONLY,
chest facing camera, wearing flowing white martial arts robe —
gold embroidery visible at collar V-neck and along chest center
seam, green jade pendant on chest (visible directly center), waist
sash tied in front bow with two tails trailing down, robe falls to
mid-thigh, NO head NO arms NO legs visible (clean cuts at shoulders,
hips, neck base).

Palette: bone cream #e8d5a6 robe base, warm shadow #b89968 fold
shadow, primary gold #d4a64a embroidery accent, jade green #6b8e62
pendant, dry leaf #8a6f47 sash highlight.

Composition: 256x384 px (vertical), isolated torso, perfectly
symmetric front-facing, NO head, NO limbs, NO ground, NO shadow.
```

```
=== Save to: Art/Characters/player/S/arm_left.png  AND  arm_right.png ===

hand-painted painterly, asian wuxia.

Subject: FRONT VIEW of a young cultivator's LEFT (or RIGHT) arm,
hanging straight down at side, viewed from front — front of
shoulder + arm + wrist visible, white robe sleeve drape at front,
palm of hand showing fingers slightly curled inward, NO body, NO
head.

Palette: bone cream #e8d5a6 sleeve base, warm shadow #b89968 fold
shadow, jade pale #d4d4ba skin tone for hand, primary gold #d4a64a
narrow trim at cuff.

Composition: 256x384 px (vertical), isolated single arm, top edge
at shoulder pivot, bottom at fingertips.
```

```
=== Save to: Art/Characters/player/S/leg_left.png  AND  leg_right.png ===

hand-painted painterly, asian wuxia.

Subject: FRONT VIEW of a young cultivator's LEFT (or RIGHT) leg,
straight standing pose viewed from front, white robe pant draping
at front (front shin + knee silhouette), fabric shoe toe-end
visible (cloth wrap from front), NO body.

Palette: bone cream #e8d5a6 pant base, warm shadow #b89968 fabric
fold, dry leaf #8a6f47 shoe.

Composition: 256x384 px (vertical), isolated single leg, top at
hip pivot, bottom at toe.
```

##### Negative prompt (paste vào field "Avoid" / "Negative prompt" mọi generation)

```
no pixel art, no photo-realistic, no anime moe, no chibi,
no pure black outline, no smooth airbrush gradient,
no drop shadow on transparent background, no text, no watermark,
no signature, no border, single subject only, no duplicate,
no grid lines, no UI elements, no caption, no logo,
no lens flare, no ground beneath subject for body parts.
```

#### 3.6.2 Wolf — Hung Lang Full DST Set (14 prompts → 20 PNG)

> **Setup:** tạo `Assets/_Project/Art/Characters/wolf/E/`, `wolf/N/`, `wolf/S/`.
>
> **Quadruped pivot map:** torso = body horizontal (E) hoặc vertical (N/S). "arm" = front legs, "leg" = back legs, plus tail. PuppetAnimController hierarchy reuse cùng PuppetRole enum như Player.
>
> **S/tail skip:** front view wolf body hầu như che tail → controller fallback East sprite cho slot này (xem §3.4.2 note line 745). Nên thực tế gen 14 prompts → 20 PNG (không phải 21 như §3.0 table — table tính ideal full coverage).
>
> **W (West):** mirror E lúc runtime, KHÔNG cần gen.

##### E (East / side view) — 5 prompts → 7 PNG

```
=== Save to: Assets/_Project/Art/Characters/wolf/E/head.png ===

hand-painted painterly, asian wuxia, wilderness creature.

Subject: side view of a fierce gray wolf HEAD ONLY, profile facing
right, snarling fangs partially visible, sharp yellow eyes, ears
back in alert pose, shaggy gray fur with darker neck ruff, NO body,
NO neck below jaw.

Palette: slate gray #7a7c80 fur base, shadow stone #5a5d63 deep
shadow, highlight stone #a3a5a8 fur tip highlight, primary gold
#d4a64a eye color, ink black #1a1a1a snout outline, bone white
#c2c4ba fang.

Composition: 256x256 px, isolated head on transparent background,
NO body, NO ground, NO shadow.
```

```
=== Save to: Assets/_Project/Art/Characters/wolf/E/torso.png ===

hand-painted painterly, asian wuxia, wilderness creature.

Subject: SIDE VIEW of a gray wolf BODY ONLY (no head, no legs, no
tail), oriented HORIZONTAL with head-end on RIGHT, hip-end on LEFT,
shaggy gray fur, lean predator silhouette, ribcage visible through
fur, neutral horizontal pose. Clean cuts at neck (right edge),
hips (left edge), shoulders/hip joints (bottom edge for legs).

Palette: slate gray #7a7c80 fur base, shadow stone #5a5d63 belly
shadow, highlight stone #a3a5a8 back ridge highlight, ink black
#1a1a1a outline at fur edges.

Composition: 384x256 px (HORIZONTAL — wolf body wider than tall),
isolated body on transparent background, NO head, NO legs, NO
tail, NO ground.
```

```
=== Save to: Art/Characters/wolf/E/arm_left.png  AND  arm_right.png  (FRONT LEGS) ===

hand-painted painterly, asian wuxia.

Subject: side view of a gray wolf's FRONT LEG, straight standing
pose, lean muscular leg, gray fur with white sock fading to dark
paw, sharp claws visible at paw tip, NO body, NO foot ground.

Palette: slate gray #7a7c80 fur base, shadow stone #5a5d63 leg
shadow, bone white #c2c4ba paw fur highlight, ink black #1a1a1a
claw.

Composition: 192x320 px (vertical), isolated single front leg on
transparent, top edge clean at shoulder joint (pivot), bottom at
paw, NO body.

# NOTE: gen 1 lần flip horizontal cho arm_right.png.
```

```
=== Save to: Art/Characters/wolf/E/leg_left.png  AND  leg_right.png  (BACK LEGS) ===

hand-painted painterly, asian wuxia.

Subject: side view of a gray wolf's BACK LEG, standing pose with
slight crouch (powerful hindquarter), strong haunch muscle visible,
gray fur, paw with claws, NO body, NO ground contact.

Palette: same wolf palette — slate gray, shadow stone, highlight
stone, ink black claw.

Composition: 192x320 px (vertical), isolated single back leg, top
edge clean at hip joint (pivot), bottom at paw, NO body.

# NOTE: gen 1 lần flip horizontal cho leg_right.png.
```

```
=== Save to: Assets/_Project/Art/Characters/wolf/E/tail.png ===

hand-painted painterly, asian wuxia.

Subject: side view of a gray wolf's bushy TAIL, oriented HORIZONTAL,
attaches at LEFT edge (root), tip flowing to RIGHT (relaxed neutral
hang downward at slight angle), shaggy fur with mid-tone shadow,
darker tip, NO body.

Palette: slate gray #7a7c80, shadow stone #5a5d63 deep shadow,
highlight stone #a3a5a8 fur highlight, ink black #1a1a1a tip.

Composition: 256x192 px (HORIZONTAL — tail wider than tall),
isolated tail on transparent, root edge LEFT (pivot point at hip
attachment), tip RIGHT, NO body.
```

##### N (North / back view) — 5 prompts → 7 PNG

```
=== Save to: Assets/_Project/Art/Characters/wolf/N/head.png ===

hand-painted painterly, asian wuxia, wilderness creature.

Subject: BACK VIEW of a fierce gray wolf HEAD ONLY, back of skull
facing camera, ears upright + back of ears with darker gray fur
edge visible, neck ruff prominent shaggy fur fading to body, NO
face features (back of head only), NO body.

Palette: slate gray #7a7c80 fur base, shadow stone #5a5d63 deep
shadow, highlight stone #a3a5a8 fur tip highlight, ink black
#1a1a1a back-of-ear inner edges.

Composition: 256x256 px, isolated head on transparent background,
NO body, NO ground, NO shadow.
```

```
=== Save to: Assets/_Project/Art/Characters/wolf/N/torso.png ===

hand-painted painterly, asian wuxia, wilderness creature.

Subject: BACK VIEW of gray wolf BODY ONLY (no head, no legs, no
tail), oriented VERTICAL — wolf walking AWAY from camera, back
ridge visible from above, hindquarters at bottom, shoulders at top,
shaggy gray fur with darker dorsal stripe along spine, lean
predator silhouette from above. Clean cuts at neck (top edge),
hips (bottom edge), shoulder/hip joints (side edges for legs).

Palette: slate gray #7a7c80 fur base, shadow stone #5a5d63 spine
shadow groove, highlight stone #a3a5a8 back ridge highlight, ink
black #1a1a1a outline at fur edges.

Composition: 256x384 px (VERTICAL — wolf body taller than wide
when viewed from rear), isolated body on transparent, NO head,
NO legs, NO tail, NO ground.
```

```
=== Save to: Art/Characters/wolf/N/arm_left.png  AND  arm_right.png  (BACK VIEW front legs) ===

hand-painted painterly, asian wuxia.

Subject: BACK VIEW of gray wolf FRONT LEG, viewed from behind +
slightly above, lean muscular leg straight standing, gray fur back-
of-leg silhouette, paw with claws partially visible from behind,
NO body.

Palette: slate gray #7a7c80, shadow stone #5a5d63, bone white
#c2c4ba paw fur, ink black #1a1a1a claw.

Composition: 192x320 px (vertical), isolated single front leg, top
edge clean at shoulder pivot, bottom at paw, NO body.

# NOTE: gen 1 lần flip cho arm_right.
```

```
=== Save to: Art/Characters/wolf/N/leg_left.png  AND  leg_right.png  (BACK VIEW back legs) ===

hand-painted painterly, asian wuxia.

Subject: BACK VIEW of gray wolf BACK LEG, viewed from behind,
strong haunch muscle visible at top, hock + paw visible at bottom,
gray fur, slight crouch in standing pose (powerful hindquarter),
NO body.

Palette: same wolf palette as E.

Composition: 192x320 px (vertical), isolated single back leg, top
at hip pivot, bottom at paw.

# NOTE: gen 1 lần flip cho leg_right.
```

```
=== Save to: Assets/_Project/Art/Characters/wolf/N/tail.png ===

hand-painted painterly, asian wuxia.

Subject: BACK VIEW of gray wolf bushy TAIL, oriented VERTICAL,
attaches at TOP edge (root, hip attachment), tip flowing DOWN at
relaxed neutral hang straight down, shaggy fur viewed from behind
showing dorsal stripe darker line, NO body.

Palette: same wolf palette as E.

Composition: 192x320 px (VERTICAL — tail straight down from rear
view), isolated tail on transparent, root TOP pivot, tip BOTTOM,
NO body.
```

##### S (South / front view) — 4 prompts → 6 PNG (S/tail skip — controller fallback E)

```
=== Save to: Assets/_Project/Art/Characters/wolf/S/head.png ===

hand-painted painterly, asian wuxia, wilderness creature.

Subject: FRONT VIEW of a fierce gray wolf HEAD ONLY, face directly
facing camera, sharp yellow eyes prominent + symmetric, snout
pointing forward with snarling fangs partially visible, ears alert
and slightly back, shaggy gray fur with darker neck ruff visible
at jaw base, NO body.

Palette: same as E (slate gray, shadow stone, highlight stone,
primary gold eye, ink black snout/fang outline, bone white fang).

Composition: 256x256 px, isolated head, perfectly symmetric front-
facing, NO body, NO shadow.
```

```
=== Save to: Assets/_Project/Art/Characters/wolf/S/torso.png ===

hand-painted painterly, asian wuxia, wilderness creature.

Subject: FRONT VIEW of gray wolf BODY ONLY, oriented VERTICAL —
wolf facing camera, chest + belly visible from front, shoulders at
top widening to ribcage, narrow waist, shaggy fur with paler chest
ruff highlight, NO head, NO legs, NO tail.

Palette: same as E + bone white #c2c4ba chest ruff highlight.

Composition: 256x384 px (vertical), isolated body, perfectly
symmetric front-facing, NO head, NO legs.
```

```
=== Save to: Art/Characters/wolf/S/arm_left.png  AND  arm_right.png  (FRONT VIEW front legs) ===

hand-painted painterly, asian wuxia.

Subject: FRONT VIEW of gray wolf FRONT LEG, viewed from front, lean
muscular leg straight standing, gray fur front-of-leg + chest fade,
paw with sharp claws visible at bottom (toes pointing toward camera),
NO body.

Palette: same wolf palette as E.

Composition: 192x320 px (vertical), isolated single front leg, top
at shoulder, bottom at paw with claws frontal.

# NOTE: gen 1 lần flip cho arm_right.
```

```
=== Save to: Art/Characters/wolf/S/leg_left.png  AND  leg_right.png  (FRONT VIEW back legs) ===

hand-painted painterly, asian wuxia.

Subject: FRONT VIEW of gray wolf BACK LEG, viewed from front, strong
haunch with subtle muscle visible from front angle, hock + paw with
claws facing camera, NO body.

Palette: same wolf palette as E.

Composition: 192x320 px (vertical), isolated single back leg, top
at hip pivot, bottom at paw.

# NOTE: gen 1 lần flip cho leg_right.
```

> **S/tail SKIP:** front-view wolf body chắn tail → để slot rỗng, controller fallback `wolf/E/tail.png` ở runtime. Nếu insistent gen S/tail, prompt: tail tip subtly peeking quanh hông (asymmetric khó match — không khuyến khích).

##### Negative prompt (paste vào field "Avoid" / "Negative prompt" mọi wolf generation)

```
no pixel art, no photo-realistic, no anime moe, no chibi, no cute
cartoony domestic dog, no fluffy puppy, no pure black outline, no
smooth airbrush gradient, no drop shadow on transparent background,
no text, no watermark, no signature, no border, single subject only,
no duplicate, no grid lines, no UI elements, no caption, no logo,
no lens flare, no ground beneath subject for body parts, no leash,
no collar.
```

#### 3.6.3 FoxSpirit — Linh Hồ Full DST Set (14 prompts → 20 PNG)

> **Setup:** tạo `Assets/_Project/Art/Characters/fox_spirit/E/`, `fox_spirit/N/`, `fox_spirit/S/`.
>
> **Quadruped pivot map:** giống wolf (torso horizontal E / vertical N+S, arms = front legs, legs = back legs, tail).
>
> **Hero feature:** spirit qi blue glow trail along dorsal ridge — **mạnh nhất ở N (back view)** vì user nhìn thẳng xuống dorsal stripe. Tail cũng là hero feature, có glow đậm hơn các parts khác.
>
> **S/tail skip:** giống wolf — front view body chắn tail → controller fallback East.
>
> **W (West):** mirror E lúc runtime.

##### E (East / side view) — 5 prompts → 7 PNG

```
=== Save to: Assets/_Project/Art/Characters/fox_spirit/E/head.png ===

hand-painted painterly, asian wuxia cultivation, supernatural ethereal.

Subject: side view of a mystical white nine-tailed fox spirit HEAD
ONLY, profile facing right, glowing pale blue cunning eyes, sharp
ears upright with alert tip, fine white fur with faint blue qi
glow at temple and ear tip, slight smirk showing tiny fang, NO
body, NO neck below jaw.

Palette: bone cream #e8d5a6 fur base, bone bleached #d4d4d4
highlight, spirit qi blue #a8d8ff glow accent on temple and ear
tips, sky qi mid #6fb5e0 eye color, ink black #1a1a1a fine outline,
primary gold #d4a64a faint inner ear gold tone.

Composition: 256x256 px, isolated head on transparent, NO body,
NO ground, ethereal soft edge but still clean silhouette.
```

```
=== Save to: Assets/_Project/Art/Characters/fox_spirit/E/torso.png ===

hand-painted painterly, asian wuxia, supernatural ethereal.

Subject: SIDE VIEW of a white fox spirit BODY ONLY (no head, no
legs, no tail), HORIZONTAL orientation with head-end RIGHT, hip-end
LEFT, lithe slender silhouette, fine snowy fur with faint blue qi
glow trailing along back ridge, ethereal mist wisps at lower belly
dissolving into transparent. Clean cuts at neck/hips/shoulders.

Palette: bone cream #e8d5a6 fur base, bone bleached #d4d4d4
highlight, spirit qi blue #a8d8ff glow trail along spine, sky qi
mid #6fb5e0 belly mist, ink black #1a1a1a outline (very thin —
1px max for ethereal feel).

Composition: 384x256 px (horizontal), isolated body on transparent,
NO head, NO legs, NO tail.
```

```
=== Save to: Art/Characters/fox_spirit/E/arm_left.png  AND  arm_right.png  (FRONT LEGS) ===

hand-painted painterly, supernatural fox spirit.

Subject: side view of a white fox spirit FRONT LEG, slender lithe
pose, fine white fur with faint blue qi outline, delicate paw with
small claws, NO body, NO ground.

Palette: bone cream #e8d5a6, bone bleached #d4d4d4, spirit qi blue
#a8d8ff trim glow.

Composition: 160x288 px (vertical), isolated single front leg on
transparent, top edge clean at shoulder joint, bottom at paw.

# NOTE: gen 1 lần flip cho arm_right.
```

```
=== Save to: Art/Characters/fox_spirit/E/leg_left.png  AND  leg_right.png  (BACK LEGS) ===

hand-painted painterly, supernatural fox spirit.

Subject: side view of a white fox spirit BACK LEG, slender powerful
haunch with subtle muscle definition, fine white fur with faint
blue qi line at hip, delicate paw, NO body.

Palette: same fox spirit palette.

Composition: 160x288 px (vertical), isolated single back leg, top
edge at hip joint pivot, bottom at paw.

# NOTE: gen 1 lần flip cho leg_right.
```

```
=== Save to: Assets/_Project/Art/Characters/fox_spirit/E/tail.png ===

hand-painted painterly, supernatural ethereal.

Subject: side view of a magnificent fox spirit TAIL — single tail
fluffy and majestic, oriented horizontal with attachment ROOT on
LEFT and tip flowing to RIGHT in graceful arc, fine white fur with
strong spirit qi blue glow trail along tail length, ethereal mist
wisps fading at tip.

Palette: bone cream #e8d5a6 fur base, bone bleached #d4d4d4, spirit
qi blue #a8d8ff dominant glow accent (stronger than other parts —
tail is hero feature), sky qi mid #6fb5e0 mid-tone, primary gold
#d4a64a inner glow at root.

Composition: 320x256 px (horizontal — wider than head), isolated
tail on transparent, root LEFT pivot, tip RIGHT, NO body.
```

##### N (North / back view) — 5 prompts → 7 PNG (hero direction!)

```
=== Save to: Assets/_Project/Art/Characters/fox_spirit/N/head.png ===

hand-painted painterly, asian wuxia cultivation, supernatural ethereal.

Subject: BACK VIEW of a mystical white nine-tailed fox spirit HEAD
ONLY, back of skull facing camera, sharp ears upright with alert
tip + back of ears showing pale gold inner glow at edges, fine white
fur with strong spirit qi blue glow trail starting at back of skull
flowing down into nape, NO face features (back of head only), NO
body.

Palette: bone cream #e8d5a6 fur base, bone bleached #d4d4d4
highlight, spirit qi blue #a8d8ff glow trail dominant at nape,
primary gold #d4a64a inner ear gold tone (visible from back as edge
glow), ink black #1a1a1a fine outline.

Composition: 256x256 px, isolated head, NO body, ethereal soft
edge but clean silhouette.
```

```
=== Save to: Assets/_Project/Art/Characters/fox_spirit/N/torso.png ===

hand-painted painterly, asian wuxia, supernatural ethereal.

Subject: BACK VIEW of white fox spirit BODY ONLY, oriented VERTICAL
— fox walking away from camera, back ridge prominent with strong
spirit qi blue glow trail along entire spine (hero feature visible
from this angle), lithe slender silhouette, fine snowy fur fading
to ethereal mist at lower hindquarters dissolving into transparent.
Clean cuts at neck (top), hips (bottom), shoulders/hip joints (sides).

Palette: same as E with EMPHASIS on spirit qi blue #a8d8ff dorsal
trail (rendered ~30% stronger than side view since back is dominant
showcase).

Composition: 256x384 px (vertical), isolated body on transparent,
NO head, NO legs, NO tail.
```

```
=== Save to: Art/Characters/fox_spirit/N/arm_left.png  AND  arm_right.png  (BACK VIEW front legs) ===

hand-painted painterly, supernatural fox spirit.

Subject: BACK VIEW of white fox spirit FRONT LEG, viewed from
behind, slender lithe leg straight standing, fine white fur back-
of-leg silhouette with faint blue qi outline at hip attachment,
delicate paw with small claws visible from behind, NO body.

Palette: same as E.

Composition: 160x288 px (vertical), isolated single front leg, top
edge at shoulder joint pivot, bottom at paw.

# NOTE: gen 1 lần flip cho arm_right.
```

```
=== Save to: Art/Characters/fox_spirit/N/leg_left.png  AND  leg_right.png  (BACK VIEW back legs) ===

hand-painted painterly, supernatural fox spirit.

Subject: BACK VIEW of white fox spirit BACK LEG, viewed from behind,
slender powerful haunch with subtle muscle from rear angle, fine
white fur with faint blue qi line at hip, delicate paw at bottom,
NO body.

Palette: same as E.

Composition: 160x288 px (vertical), isolated single back leg, top
at hip pivot, bottom at paw.

# NOTE: gen 1 lần flip cho leg_right.
```

```
=== Save to: Assets/_Project/Art/Characters/fox_spirit/N/tail.png ===

hand-painted painterly, supernatural ethereal.

Subject: BACK VIEW of magnificent fox spirit TAIL — single tail
fluffy and majestic, oriented VERTICAL with attachment ROOT at TOP
(hip back) and tip flowing DOWN with graceful arc to one side, fine
white fur with VERY STRONG spirit qi blue glow trail dominant along
tail length (tail is hero feature from this angle, viewer's eye
naturally drawn here), ethereal mist wisps fading at tip.

Palette: bone cream #e8d5a6 fur base, bone bleached #d4d4d4, spirit
qi blue #a8d8ff dominant glow accent (40% stronger than side view),
sky qi mid #6fb5e0 mid-tone, primary gold #d4a64a inner glow at
root.

Composition: 256x320 px (vertical — tail trailing down from rear
view), isolated tail on transparent, root TOP pivot, tip BOTTOM,
NO body.
```

##### S (South / front view) — 4 prompts → 6 PNG (S/tail skip)

```
=== Save to: Assets/_Project/Art/Characters/fox_spirit/S/head.png ===

hand-painted painterly, asian wuxia cultivation, supernatural ethereal.

Subject: FRONT VIEW of mystical white nine-tailed fox spirit HEAD
ONLY, face directly facing camera, glowing pale blue cunning eyes
prominent and symmetric (hero feature), sharp ears upright + slightly
flared, tiny fang visible at slight smirk, fine white fur framing
face with faint blue qi glow at both temples (symmetric), NO body.

Palette: same as E.

Composition: 256x256 px, isolated head, perfectly symmetric front-
facing, NO body, ethereal soft edge.
```

```
=== Save to: Assets/_Project/Art/Characters/fox_spirit/S/torso.png ===

hand-painted painterly, asian wuxia, supernatural ethereal.

Subject: FRONT VIEW of white fox spirit BODY ONLY, oriented VERTICAL
— fox facing camera, chest + belly visible from front, shoulders at
top, narrow waist, fine snowy fur with paler chest highlight, faint
ethereal mist wisps trailing at lower belly dissolving into
transparent, subtle blue qi glow at chest center (heart area), NO
head, NO legs, NO tail.

Palette: same as E + sky qi mid #6fb5e0 chest mist accent.

Composition: 256x384 px (vertical), isolated body, perfectly
symmetric front-facing, ethereal edge.
```

```
=== Save to: Art/Characters/fox_spirit/S/arm_left.png  AND  arm_right.png  (FRONT VIEW front legs) ===

hand-painted painterly, supernatural fox spirit.

Subject: FRONT VIEW of white fox spirit FRONT LEG, viewed from
front, slender lithe leg, fine white fur with faint blue qi outline
at front of shoulder, delicate paw with small claws facing camera
at bottom, NO body.

Palette: same as E.

Composition: 160x288 px (vertical), isolated single front leg, top
at shoulder, bottom at paw.

# NOTE: gen 1 lần flip cho arm_right.
```

```
=== Save to: Art/Characters/fox_spirit/S/leg_left.png  AND  leg_right.png  (FRONT VIEW back legs) ===

hand-painted painterly, supernatural fox spirit.

Subject: FRONT VIEW of white fox spirit BACK LEG, viewed from front,
slender haunch with subtle blue qi glow at hip, delicate paw with
claws visible at bottom facing camera, NO body.

Palette: same as E.

Composition: 160x288 px (vertical), isolated single back leg, top
at hip, bottom at paw.

# NOTE: gen 1 lần flip cho leg_right.
```

> **S/tail SKIP:** giống wolf — front-view body chắn tail. Controller fallback `fox_spirit/E/tail.png` runtime.

##### Negative prompt (paste vào field "Avoid" / "Negative prompt" mọi fox spirit generation)

```
no pixel art, no photo-realistic, no anime moe, no chibi cute
mascot, no domestic pet fox, no fluffy plush toy look, no pure
black outline, no smooth airbrush gradient, no drop shadow on
transparent background, no text, no watermark, no signature, no
border, single subject only, no duplicate, no grid lines, no UI
elements, no caption, no logo, no lens flare except qi glow,
no ground beneath subject for body parts.
```

#### 3.6.4 Rabbit — Linh Thố Full DST Set (14 prompts → 20 PNG)

> **Setup:** tạo `Assets/_Project/Art/Characters/rabbit/E/`, `rabbit/N/`, `rabbit/S/`. BootstrapWizard sẽ build puppet hierarchy với multi-dir sprite swap khi rabbit di chuyển.
>
> **Quadruped pivot map:** giống wolf/fox — torso horizontal E / vertical N+S, arms = front legs, legs = back legs (powerful hindquarter), tail = puffy cottontail.
>
> **Hero feature:** puffy white cottontail — **dominant từ N (back view)** vì viewer thấy fluffy white puff khi rabbit hop away. E (side) tail vẫn visible nhưng nhỏ hơn. S (front view) tail bị che → SKIP.
>
> **S/tail skip:** giống wolf/fox — front-view body chắn tail. Controller fallback `rabbit/E/tail.png` runtime.
>
> **Style anchor:** small nimble feral wilderness rabbit, NOT cute mascot, NOT chibi bunny, NOT floppy-eared domestic pet — alert wary creature with long upright ears + watchful eyes.
>
> **W (West):** mirror E lúc runtime — KHÔNG cần gen W folder.
>
> **Resolution scale:** rabbit ~half wolf size (192px head / 256px torso vs 256/384 wolf) — keeps proportional silhouette in-game vs ~32px placeholder height of wolf.

##### E (East / side view) — 5 prompts → 7 PNG

```
=== Save to: Assets/_Project/Art/Characters/rabbit/E/head.png ===

hand-painted painterly, asian wuxia, wilderness creature.

Subject: side view of a small alert woodland rabbit HEAD ONLY,
profile facing right, long upright ears (slightly back-tilted in
wary listening pose, NOT floppy), warm brown fur with cream cheek,
small dark watchful eye, twitching nose, NO body, NO neck below
jaw.

Palette: tan brown #8b6f47 fur base, warm shadow #5a4a3a deep
shadow, fur highlight #b89968 ear tip and cheek, cream #e8d5a6
inner ear and muzzle, dark nose #2a2a2a eye and nose, bone white
#c2c4ba whisker.

Composition: 192x192 px, isolated head on transparent background,
NO body, NO ground, NO shadow.
```

```
=== Save to: Assets/_Project/Art/Characters/rabbit/E/torso.png ===

hand-painted painterly, asian wuxia, wilderness creature.

Subject: SIDE VIEW of a small rabbit BODY ONLY (no head, no legs,
no tail), oriented HORIZONTAL with head-end on RIGHT, hip-end on
LEFT, compact rounded torso with slight hunched back (hopper
silhouette), warm brown back fur fading to cream belly, neutral
horizontal pose. Clean cuts at neck (right edge), hips (left edge),
shoulders/hip joints (bottom edge for legs).

Palette: tan brown #8b6f47 fur base, warm shadow #5a4a3a belly
shadow, fur highlight #b89968 back ridge highlight, cream #e8d5a6
belly underside, ink black #1a1a1a outline at fur edges.

Composition: 256x192 px (HORIZONTAL — rabbit body wider than tall),
isolated body on transparent, NO head, NO legs, NO tail, NO ground.
```

```
=== Save to: Art/Characters/rabbit/E/arm_left.png  AND  arm_right.png  (FRONT LEGS) ===

hand-painted painterly, asian wuxia.

Subject: side view of a small rabbit's FRONT LEG, straight standing
pose, short slender forelimb (rabbits are hindquarter-dominant —
front legs much shorter than back), warm brown fur fading to cream
paw, small dark claws visible at paw tip, NO body, NO ground.

Palette: tan brown #8b6f47 fur base, warm shadow #5a4a3a leg shadow,
cream #e8d5a6 paw fur highlight, dark nose #2a2a2a claw, ink black
#1a1a1a outline.

Composition: 128x192 px (vertical), isolated single front leg, top
edge clean at shoulder joint (pivot), bottom at paw, NO body.

# NOTE: gen 1 lần rồi flip horizontal cho arm_right (Photopea/GIMP).
# Hoặc gen 2 lần với prompt "right front leg" cho asymmetry tự nhiên.
```

```
=== Save to: Art/Characters/rabbit/E/leg_left.png  AND  leg_right.png  (BACK LEGS) ===

hand-painted painterly, asian wuxia.

Subject: side view of a small rabbit's BACK LEG, standing pose with
distinct crouched fold (powerful hindquarter — hopper anatomy with
long thigh + bent shin folded under body), strong haunch muscle
visible, warm brown fur, paw with small claws, NO body, NO ground
contact.

Palette: same rabbit palette — tan brown, warm shadow, cream paw,
dark claw, ink outline.

Composition: 192x256 px (vertical, taller than front leg — rabbits
have longer back legs), isolated single back leg, top edge clean
at hip joint (pivot), bottom at paw, NO body.

# NOTE: gen 1 lần flip cho leg_right.
```

```
=== Save to: Assets/_Project/Art/Characters/rabbit/E/tail.png ===

hand-painted painterly, asian wuxia.

Subject: side view of a small rabbit's puffy COTTONTAIL — short
round cloud-like tuft (hero feature, much fluffier than wolf/fox
tail), oriented HORIZONTAL, attaches at LEFT edge (root), tail body
extends slightly to RIGHT (very short — almost as wide as tall),
bright cream-white fur with soft shadow underneath, NO body.

Palette: bone bleached #d4d4d4 fur tip (brightest), cream #e8d5a6
core, warm shadow #5a4a3a underside shadow, ink black #1a1a1a
outline.

Composition: 128x128 px (square — cottontail nearly round), isolated
tail on transparent, root edge LEFT (pivot at hip attachment), NO
body.
```

##### N (North / back view) — 5 prompts → 7 PNG

```
=== Save to: Assets/_Project/Art/Characters/rabbit/N/head.png ===

hand-painted painterly, asian wuxia, wilderness creature.

Subject: BACK VIEW of a small alert rabbit HEAD ONLY, back of skull
facing camera, long upright ears prominent (silhouette dominated by
two erect ears with cream inner-ear edge visible from behind), warm
brown fur on back of head, tiny tuft of cream fur at nape, NO face
features (back of head only), NO body.

Palette: same as E (tan brown #8b6f47, warm shadow #5a4a3a, fur
highlight #b89968, cream #e8d5a6 inner ear edge).

Composition: 192x192 px, isolated head, NO body, NO ground.
```

```
=== Save to: Assets/_Project/Art/Characters/rabbit/N/torso.png ===

hand-painted painterly, asian wuxia, wilderness creature.

Subject: BACK VIEW of small rabbit BODY ONLY, oriented VERTICAL —
rabbit hopping away from camera, back ridge prominent with warm
brown fur fading to slightly paler at hindquarter top, compact
rounded silhouette tapering toward hip area at bottom (where tail
attaches), NO head, NO legs, NO tail. Clean cuts at neck (top),
hips (bottom — tail attachment line clean), shoulders/hip joints
(sides for legs).

Palette: same as E (tan brown back fur dominant, warm shadow at
fur folds, fur highlight at back ridge).

Composition: 192x256 px (vertical), isolated body, NO head, NO legs,
NO tail.
```

```
=== Save to: Art/Characters/rabbit/N/arm_left.png  AND  arm_right.png  (BACK VIEW front legs) ===

hand-painted painterly, wilderness creature.

Subject: BACK VIEW of small rabbit FRONT LEG, viewed from behind,
short slender forelimb (rabbit hindquarter-dominant), fine warm
brown fur back-of-leg silhouette, small paw with tiny claws visible
from behind at bottom, NO body.

Palette: same as E.

Composition: 128x192 px (vertical), isolated single front leg, top
at shoulder pivot, bottom at paw.

# NOTE: gen 1 lần flip cho arm_right.
```

```
=== Save to: Art/Characters/rabbit/N/leg_left.png  AND  leg_right.png  (BACK VIEW back legs) ===

hand-painted painterly, wilderness creature.

Subject: BACK VIEW of small rabbit BACK LEG, viewed from behind,
powerful haunch with crouched fold from rear angle (long thigh +
bent shin folded), warm brown fur dominant on back of leg, small
paw at bottom, NO body.

Palette: same as E.

Composition: 192x256 px (vertical, taller than front leg), isolated
single back leg, top at hip pivot, bottom at paw.

# NOTE: gen 1 lần flip cho leg_right.
```

```
=== Save to: Assets/_Project/Art/Characters/rabbit/N/tail.png === (HERO FEATURE)

hand-painted painterly, wilderness creature.

Subject: BACK VIEW of small rabbit puffy COTTONTAIL — short round
cloud-like tuft (hero feature from rear angle — viewer's eye
naturally drawn to this fluffy white puff when rabbit hops away),
oriented with attachment ROOT at TOP (hip back), tail body extends
slightly DOWN as compact round puff, bright cream-white fur with
soft shadow underneath, NO body.

Palette: bone bleached #d4d4d4 fur tip dominant (brightest area —
40% stronger than side view since cottontail is dominant showcase),
cream #e8d5a6 core, warm shadow #5a4a3a underside shadow.

Composition: 128x128 px (square), isolated tail, root TOP pivot,
NO body.
```

##### S (South / front view) — 4 prompts → 6 PNG (skip tail)

```
=== Save to: Assets/_Project/Art/Characters/rabbit/S/head.png ===

hand-painted painterly, asian wuxia, wilderness creature.

Subject: FRONT VIEW of small alert rabbit HEAD ONLY, face directly
facing camera, two long upright ears symmetric on top of head, two
small dark watchful eyes prominent and symmetric (hero feature),
twitching nose centered, cream cheek fur framing face, whiskers
spreading symmetrically, NO body.

Palette: same as E.

Composition: 192x192 px, isolated head, perfectly symmetric front-
facing, NO body.
```

```
=== Save to: Assets/_Project/Art/Characters/rabbit/S/torso.png ===

hand-painted painterly, asian wuxia, wilderness creature.

Subject: FRONT VIEW of small rabbit BODY ONLY, oriented VERTICAL —
rabbit facing camera, chest + belly visible from front (cream belly
dominant from this angle — hero feature), narrow shoulders at top,
compact rounded torso, NO head, NO legs, NO tail.

Palette: same as E + emphasis on cream #e8d5a6 belly fur (rendered
~20% larger area than side view since belly is dominant from front).

Composition: 192x256 px (vertical), isolated body, perfectly
symmetric front-facing, NO head, NO legs.
```

```
=== Save to: Art/Characters/rabbit/S/arm_left.png  AND  arm_right.png  (FRONT VIEW front legs) ===

hand-painted painterly, wilderness creature.

Subject: FRONT VIEW of small rabbit FRONT LEG, viewed from front,
short slender forelimb, warm brown fur, small paw with tiny dark
claws facing camera at bottom, NO body.

Palette: same as E.

Composition: 128x192 px (vertical), isolated single front leg, top
at shoulder, bottom at paw.

# NOTE: gen 1 lần flip cho arm_right.
```

```
=== Save to: Art/Characters/rabbit/S/leg_left.png  AND  leg_right.png  (FRONT VIEW back legs) ===

hand-painted painterly, wilderness creature.

Subject: FRONT VIEW of small rabbit BACK LEG, viewed from front,
powerful crouched haunch (hopper anatomy visible from front —
muscular thigh forward), warm brown fur with cream paw highlight,
small claws facing camera at bottom, NO body.

Palette: same as E.

Composition: 192x256 px (vertical), isolated single back leg, top
at hip, bottom at paw.

# NOTE: gen 1 lần flip cho leg_right.
```

> **S/tail SKIP:** giống wolf/fox — front-view body chắn cottontail. Controller fallback `rabbit/E/tail.png` runtime.

##### Negative prompt (paste vào field "Avoid" / "Negative prompt" mọi rabbit generation)

```
no pixel art, no photo-realistic, no anime moe, no chibi cute
bunny mascot, no floppy-eared domestic pet rabbit, no fluffy plush
toy look, no easter bunny, no cartoon disney rabbit, no pure black
outline, no smooth airbrush gradient, no drop shadow on transparent
background, no text, no watermark, no signature, no border, single
subject only, no duplicate, no grid lines, no UI elements, no
caption, no logo, no lens flare, no ground beneath subject for body
parts, no carrot prop.
```

---

#### 3.6.5 Boar — Hắc Trư Full DST Set (14 prompts → 20 PNG)

> **Setup:** tạo `Assets/_Project/Art/Characters/boar/E/`, `boar/N/`, `boar/S/`. BootstrapWizard sẽ build puppet hierarchy với multi-dir sprite swap khi boar di chuyển.
>
> **Quadruped pivot map:** giống wolf/fox — torso horizontal E / vertical N+S, arms = front legs (heavy short), legs = back legs (heavy haunch), tail = bristly stub.
>
> **Hero feature:** **curved ivory tusks** — visible từ E side (twin tusks rising upward) và **dominant từ S front view** (face-on most menacing angle). N back view shows shoulder hump + bristle ridge along spine.
>
> **S/tail skip:** giống wolf/fox/rabbit — front-view body chắn stub tail (boar tail càng nhỏ hơn nên càng không thấy). Controller fallback `boar/E/tail.png` runtime.
>
> **Style anchor:** wild feral aggressive forest boar, NOT cute farm pig, NOT cartoon Disney boar, NOT chibi — heavy charger silhouette với bristly coarse fur + visible tusks.
>
> **W (West):** mirror E lúc runtime — KHÔNG cần gen W folder.
>
> **Resolution scale:** boar ~larger than wolf (placeholderHeightPx 36 vs 32). Head/torso res same as wolf otherwise (256/288), legs slightly bulkier.

##### E (East / side view) — 5 prompts → 7 PNG

```
=== Save to: Assets/_Project/Art/Characters/boar/E/head.png ===

hand-painted painterly, asian wuxia, wilderness creature.

Subject: side view of a wild boar HEAD ONLY, profile facing right,
heavy lowered head with thick neck stub, small angry beady eye,
broad flat snout with twin nostrils, **TWO CURVED IVORY TUSKS**
protruding upward from lower jaw (hero feature — clearly visible
from side, slightly yellowed bone tone), short pointed ear pricked
forward (alert/aggressive), bristly coarse fur on forehead and jowl,
NO body, NO neck below jaw cut.

Palette: dark brown #4d3a28 fur base, deep shadow #2a1f15 deep
shadow, fur highlight #8b6f47 forehead ridge highlight, ivory
#d4c8a3 tusk, dark nose #1a1a1a eye and nostril, bone white #c2c4ba
inner ear hint.

Composition: 256x256 px, isolated head on transparent background,
tusks fully visible (do NOT crop), NO body, NO ground, NO shadow.
```

```
=== Save to: Assets/_Project/Art/Characters/boar/E/torso.png ===

hand-painted painterly, asian wuxia, wilderness creature.

Subject: SIDE VIEW of a wild boar BODY ONLY (no head, no legs, no
tail), oriented HORIZONTAL with shoulder-end on RIGHT, hip-end on
LEFT, heavy muscular torso with hunched shoulder hump (boar
characteristic), bristly dark brown fur with coarse texture (NOT
smooth — visible bristle clumps + spine ridge), low-slung belly,
neutral horizontal pose. Clean cuts at neck (right edge), hips
(left edge), shoulders/hip joints (bottom edge for legs).

Palette: dark brown #4d3a28 fur base, deep shadow #2a1f15 belly
shadow, fur highlight #8b6f47 spine ridge bristle highlight, deep
shadow #2a1f15 underside, ink black #1a1a1a outline at fur edges
and bristle clumps.

Composition: 288x208 px (HORIZONTAL — boar body bulkier than wolf),
isolated body on transparent, NO head, NO legs, NO tail, NO ground.
```

```
=== Save to: Art/Characters/boar/E/arm_left.png  AND  arm_right.png  (FRONT LEGS) ===

hand-painted painterly, asian wuxia.

Subject: side view of a wild boar's FRONT LEG, sturdy planted pose,
short heavy forelimb (boar legs proportionally short for body bulk),
bristly dark brown fur thigh, **black cloven hoof** at bottom (split
hoof tip), no toenail, NO body, NO ground contact.

Palette: dark brown #4d3a28 fur base, deep shadow #2a1f15 leg
shadow, fur highlight #8b6f47 thigh highlight, dark hoof #1a1a1a
cloven hoof tip, ink black #1a1a1a outline.

Composition: 144x224 px (vertical), isolated single front leg, top
edge clean at shoulder joint (pivot), bottom at hoof, NO body.

# NOTE: gen 1 lần rồi flip horizontal cho arm_right (Photopea/GIMP).
```

```
=== Save to: Art/Characters/boar/E/leg_left.png  AND  leg_right.png  (BACK LEGS) ===

hand-painted painterly, asian wuxia.

Subject: side view of a wild boar's BACK LEG, sturdy planted pose,
heavy hind limb with thick haunch muscle (boar charge anatomy —
explosive thigh), bristly fur, **black cloven hoof** at bottom, NO
body, NO ground contact.

Palette: same boar palette — dark brown, deep shadow, fur highlight,
dark hoof, ink outline.

Composition: 160x240 px (vertical, slightly taller than front leg
to hint hindquarter dominance), isolated single back leg, top edge
clean at hip joint (pivot), bottom at hoof, NO body.

# NOTE: gen 1 lần flip cho leg_right.
```

```
=== Save to: Assets/_Project/Art/Characters/boar/E/tail.png ===

hand-painted painterly, asian wuxia.

Subject: side view of a wild boar's SHORT BRISTLY TAIL — short stiff
stub (~half the length of wolf tail, NOT puffy NOT swishy), straight
horizontal pose with slight twist, bristly dark fur with coarse
clumps at tip, oriented HORIZONTAL, attaches at LEFT edge (root),
tail tip extends to RIGHT, NO body.

Palette: deep shadow #2a1f15 base fur, dark brown #4d3a28 mid-tone,
fur highlight #8b6f47 tip bristle, ink black #1a1a1a outline.

Composition: 128x96 px (short rectangle — tail is stub-length),
isolated tail on transparent, root edge LEFT (pivot point at hip
attachment), NO body.
```

##### N (North / back view) — 5 prompts → 7 PNG

```
=== Save to: Assets/_Project/Art/Characters/boar/N/head.png ===

hand-painted painterly, asian wuxia, wilderness creature.

Subject: BACK VIEW of a wild boar HEAD ONLY, back of skull facing
camera, thick neck stub prominent, short pointed ears pricked
forward (visible from behind as twin pointed silhouette), bristly
dark fur on nape and skull crest, NO face features (back of head
only), NO body.

Palette: same as E (dark brown #4d3a28 fur base, deep shadow
#2a1f15, fur highlight #8b6f47 nape ridge, ink black outline).

Composition: 256x256 px, isolated head, NO body, NO ground.
```

```
=== Save to: Assets/_Project/Art/Characters/boar/N/torso.png ===

hand-painted painterly, asian wuxia, wilderness creature.

Subject: BACK VIEW of wild boar BODY ONLY, oriented VERTICAL — boar
charging away from camera, **prominent shoulder hump + spine bristle
ridge dominant** (hero feature from rear — coarse bristle clumps
along spine), heavy muscular silhouette tapering toward hip area at
bottom, NO head, NO legs, NO tail.

Palette: same as E + emphasis on fur highlight #8b6f47 spine bristle
ridge (rendered ~30% stronger than side view).

Composition: 208x288 px (vertical), isolated body on transparent,
NO head, NO legs, NO tail.
```

```
=== Save to: Art/Characters/boar/N/arm_left.png  AND  arm_right.png  (BACK VIEW front legs) ===

hand-painted painterly, wilderness creature.

Subject: BACK VIEW of wild boar FRONT LEG, viewed from behind,
short heavy forelimb (boar legs short for body bulk), bristly dark
brown fur on back of leg, **black cloven hoof** at bottom from rear
angle, NO body.

Palette: same as E.

Composition: 144x224 px (vertical), isolated single front leg, top
edge at shoulder joint pivot, bottom at hoof.

# NOTE: gen 1 lần flip cho arm_right.
```

```
=== Save to: Art/Characters/boar/N/leg_left.png  AND  leg_right.png  (BACK VIEW back legs) ===

hand-painted painterly, wilderness creature.

Subject: BACK VIEW of wild boar BACK LEG, viewed from behind,
heavy hindquarter with thick haunch muscle from rear angle (charge
muscle hero from rear), bristly fur, **black cloven hoof** at
bottom from rear, NO body.

Palette: same as E.

Composition: 160x240 px (vertical, slightly taller than front leg),
isolated single back leg, top at hip pivot, bottom at hoof.

# NOTE: gen 1 lần flip cho leg_right.
```

```
=== Save to: Assets/_Project/Art/Characters/boar/N/tail.png ===

hand-painted painterly, wilderness creature.

Subject: BACK VIEW of wild boar SHORT BRISTLY TAIL — tiny stiff
stub viewed from rear (less prominent than wolf/fox tail since
boar tail is bristle stub, not swishy plume), oriented with
attachment ROOT at TOP (hip back), tail extends slightly DOWN as
short coarse bristle clump, dark fur, NO body.

Palette: same as E (deep shadow + dark brown + fur highlight at
bristle tip).

Composition: 96x128 px (small rectangle — tail is stub from any
angle), isolated tail on transparent, root TOP pivot, NO body.
```

##### S (South / front view) — 4 prompts → 6 PNG (skip tail)

```
=== Save to: Assets/_Project/Art/Characters/boar/S/head.png === (HERO FEATURE — tusks face-on)

hand-painted painterly, asian wuxia, wilderness creature.

Subject: FRONT VIEW of wild boar HEAD ONLY, face directly facing
camera, **TWO CURVED IVORY TUSKS visible face-on rising upward
symmetric** (hero feature from front — most menacing angle), broad
flat snout centered with twin nostrils, two small angry beady eyes
symmetric (NOT cute — feral aggressive), short pointed ears pricked
forward symmetric, bristly dark fur framing face, NO body.

Palette: same as E.

Composition: 256x256 px, isolated head, perfectly symmetric front-
facing, tusks fully visible (do NOT crop), NO body.
```

```
=== Save to: Assets/_Project/Art/Characters/boar/S/torso.png ===

hand-painted painterly, asian wuxia, wilderness creature.

Subject: FRONT VIEW of wild boar BODY ONLY, oriented VERTICAL —
boar facing camera head-on, broad muscular chest dominant (boar
chest plate hero feature from front — wider than wolf/deer), thick
neck stub joining at top, low-slung belly tapering at bottom, NO
head, NO legs, NO tail.

Palette: same as E.

Composition: 208x288 px (vertical), isolated body, perfectly
symmetric front-facing, NO head, NO legs.
```

```
=== Save to: Art/Characters/boar/S/arm_left.png  AND  arm_right.png  (FRONT VIEW front legs) ===

hand-painted painterly, wilderness creature.

Subject: FRONT VIEW of wild boar FRONT LEG, viewed from front,
short heavy forelimb planted forward, bristly dark fur, **black
cloven hoof** facing camera at bottom, NO body.

Palette: same as E.

Composition: 144x224 px (vertical), isolated single front leg, top
at shoulder, bottom at hoof.

# NOTE: gen 1 lần flip cho arm_right.
```

```
=== Save to: Art/Characters/boar/S/leg_left.png  AND  leg_right.png  (FRONT VIEW back legs) ===

hand-painted painterly, wilderness creature.

Subject: FRONT VIEW of wild boar BACK LEG, viewed from front,
heavy haunch visible from front, bristly fur, **black cloven hoof**
facing camera at bottom, NO body.

Palette: same as E.

Composition: 160x240 px (vertical), isolated single back leg, top
at hip, bottom at hoof.

# NOTE: gen 1 lần flip cho leg_right.
```

> **S/tail SKIP:** giống wolf/fox/rabbit — front-view body chắn stub tail. Controller fallback `boar/E/tail.png` runtime.

##### Negative prompt (paste vào field "Avoid" / "Negative prompt" mọi boar generation)

```
no pixel art, no photo-realistic, no anime moe, no chibi cute
piggy mascot, no farm pig, no peppa pig, no cartoon disney boar,
no fluffy plush toy look, no pink skin, no smooth shaved fur, no
pure black outline, no smooth airbrush gradient, no drop shadow
on transparent background, no text, no watermark, no signature,
no border, single subject only, no duplicate, no grid lines, no
UI elements, no caption, no logo, no lens flare, no ground beneath
subject for body parts.
```

---

#### 3.6.6 Deer Spirit — Linh Lộc Full DST Set (14 prompts → 20 PNG)

> **Setup:** tạo `Assets/_Project/Art/Characters/deer_spirit/E/`, `deer_spirit/N/`, `deer_spirit/S/`. BootstrapWizard sẽ build puppet hierarchy với multi-dir sprite swap khi deer di chuyển.
>
> **Quadruped pivot map:** giống wolf/fox — torso horizontal E / vertical N+S, arms = front legs (long slender), legs = back legs (long slender với hock bend), tail = white flick.
>
> **Hero feature:** **branching ivory antlers với jade green qi glow tips** — visible từ E (side fork visible) và **dominant từ S (face-on symmetric crown framing face)**. N back view shows antler silhouette + white tail flick raised. White belly countershading dominant từ S (front).
>
> **S/tail skip:** giống wolf/fox/rabbit/boar — front-view body chắn white tail flick. Controller fallback `deer_spirit/E/tail.png` runtime.
>
> **Style anchor:** wuxia spirit beast deer (linh lộc) with subtle ethereal qi presence, NOT generic doe, NOT cartoon Bambi, NOT chibi — graceful slender prancer với regal antler crown + gentle expressive eyes.
>
> **W (West):** mirror E lúc runtime — KHÔNG cần gen W folder.
>
> **Resolution scale:** deer ~slimmer than wolf (placeholderHeightPx 28 vs 32). Head taller (288px) để fit antler vertical extent. Torso slimmer (240×176 vs wolf 256×192). Legs longer (256/288px vs wolf 224/256px).

##### E (East / side view) — 5 prompts → 7 PNG

```
=== Save to: Assets/_Project/Art/Characters/deer_spirit/E/head.png === (HERO FEATURE — antlers)

hand-painted painterly, asian wuxia, spirit beast.

Subject: side view of a spirit deer HEAD ONLY, profile facing right,
elegant elongated face with delicate muzzle, large gentle dark eye
with subtle qi shimmer, twitching black nose, **PAIR OF BRANCHING
IVORY ANTLERS** rising from forehead (hero feature — 4-prong fork
each side, ivory bone with subtle pale jade green qi glow at tips),
tall pointed ear pricked alertly, slender neck stub, NO body.

Palette: cream fawn #b89968 fur base, warm shadow #6b5237 deep
shadow, fur highlight #d4b896 cheek and forehead highlight, ivory
#c4a574 antler base, jade green #a8c69b faint qi glow at antler
tips, dark nose #1a1a1a eye and nose, white #e8d5a6 inner ear and
muzzle.

Composition: 256x288 px (TALLER than wolf head — antlers add
vertical extent), isolated head on transparent background, antlers
fully visible (do NOT crop tips), NO body, NO ground, NO shadow.
```

```
=== Save to: Assets/_Project/Art/Characters/deer_spirit/E/torso.png ===

hand-painted painterly, asian wuxia, spirit beast.

Subject: SIDE VIEW of a spirit deer BODY ONLY (no head, no legs, no
tail), oriented HORIZONTAL with shoulder-end on RIGHT, hip-end on
LEFT, slender elegant torso (NOT bulky — graceful prancer
proportions), cream fawn fur on back fading to white belly underside
(deer countershading — pale belly is signature visual), neutral
horizontal pose. Clean cuts at neck (right edge), hips (left edge),
shoulders/hip joints (bottom edge for legs).

Palette: cream fawn #b89968 fur base, warm shadow #6b5237 belly
fade shadow, fur highlight #d4b896 spine ridge highlight, white
#e8d5a6 belly underside (countershading), ink black #1a1a1a outline.

Composition: 240x176 px (HORIZONTAL — deer body slimmer than wolf),
isolated body on transparent, NO head, NO legs, NO tail, NO ground.
```

```
=== Save to: Art/Characters/deer_spirit/E/arm_left.png  AND  arm_right.png  (FRONT LEGS) ===

hand-painted painterly, asian wuxia.

Subject: side view of a spirit deer's FRONT LEG, slender standing
pose, long graceful forelimb (deer legs are MUCH longer + thinner
than wolf — proportionally elegant), cream fawn fur on thigh fading
to darker shin, **dark cloven hoof** at bottom (small split hoof,
delicate), NO body, NO ground contact.

Palette: cream fawn #b89968 fur base, warm shadow #6b5237 leg
shadow, fur highlight #d4b896 thigh highlight, dark hoof #2a1f15
cloven hoof, ink black #1a1a1a outline.

Composition: 128x256 px (vertical, longer than wolf front leg —
deer legs taller), isolated single front leg, top edge clean at
shoulder joint (pivot), bottom at hoof, NO body.

# NOTE: gen 1 lần flip cho arm_right.
```

```
=== Save to: Art/Characters/deer_spirit/E/leg_left.png  AND  leg_right.png  (BACK LEGS) ===

hand-painted painterly, asian wuxia.

Subject: side view of a spirit deer's BACK LEG, slender standing
pose with subtle bend at hock joint (deer hock anatomy — bend looks
"backward" vs human knee), long graceful hind limb, cream fawn fur,
**dark cloven hoof** at bottom, NO body, NO ground contact.

Palette: same deer palette — cream fawn, warm shadow, fur highlight,
dark hoof, ink outline.

Composition: 144x288 px (vertical, longest leg sprite — deer
hindquarter taller than forelimb for prancing), isolated single
back leg, top edge clean at hip joint (pivot), bottom at hoof, NO
body.

# NOTE: gen 1 lần flip cho leg_right.
```

```
=== Save to: Assets/_Project/Art/Characters/deer_spirit/E/tail.png ===

hand-painted painterly, asian wuxia, spirit beast.

Subject: side view of a spirit deer's SHORT WHITE TAIL FLICK — tiny
upturned puff (deer tail signature — small but bright white,
typically raised when alert), oriented HORIZONTAL with slight upward
angle, attaches at LEFT edge (root), tail tip extends to RIGHT but
also UPWARD, white fur with cream fawn base color underneath, NO
body.

Palette: white #e8d5a6 fur tip (brightest), cream fawn #b89968
base color underneath, warm shadow #6b5237 underside shadow, ink
black #1a1a1a outline.

Composition: 96x128 px (small tail, slightly taller than wide due
to upturned angle), isolated tail on transparent, root edge LEFT
(pivot point at hip attachment), NO body.
```

##### N (North / back view) — 5 prompts → 7 PNG

```
=== Save to: Assets/_Project/Art/Characters/deer_spirit/N/head.png ===

hand-painted painterly, asian wuxia, spirit beast.

Subject: BACK VIEW of spirit deer HEAD ONLY, back of skull facing
camera, **PAIR OF BRANCHING IVORY ANTLERS rising prominently from
forehead** (hero feature from rear — 4-prong fork each side, ivory
bone with subtle pale jade green qi glow at tips), tall pointed
ears pricked alertly visible from behind framing antler base, cream
fawn fur on nape, NO face features (back of head only), NO body.

Palette: same as E (cream fawn #b89968 fur base, warm shadow #6b5237,
ivory #c4a574 antler, jade green #a8c69b qi tip glow, fur highlight
#d4b896 nape).

Composition: 256x288 px (TALLER than wolf head — antlers add
vertical extent), isolated head, antlers fully visible (do NOT
crop tips), NO body.
```

```
=== Save to: Assets/_Project/Art/Characters/deer_spirit/N/torso.png ===

hand-painted painterly, asian wuxia, spirit beast.

Subject: BACK VIEW of spirit deer BODY ONLY, oriented VERTICAL —
deer prancing away from camera, slender elegant silhouette with
cream fawn fur on back, narrow shoulders at top, hip area at bottom
where tail attaches, **white belly hint visible on flanks** (deer
countershading shows on flanks even from rear), NO head, NO legs,
NO tail.

Palette: same as E.

Composition: 176x240 px (vertical, slimmer than wolf), isolated body
on transparent, NO head, NO legs, NO tail.
```

```
=== Save to: Art/Characters/deer_spirit/N/arm_left.png  AND  arm_right.png  (BACK VIEW front legs) ===

hand-painted painterly, spirit beast.

Subject: BACK VIEW of spirit deer FRONT LEG, viewed from behind,
long graceful slender forelimb (deer legs much taller + thinner
than wolf), cream fawn fur fading to darker shin from rear angle,
**dark cloven hoof** at bottom from behind, NO body.

Palette: same as E.

Composition: 128x256 px (vertical, longer than wolf front leg),
isolated single front leg, top edge at shoulder joint pivot, bottom
at hoof.

# NOTE: gen 1 lần flip cho arm_right.
```

```
=== Save to: Art/Characters/deer_spirit/N/leg_left.png  AND  leg_right.png  (BACK VIEW back legs) ===

hand-painted painterly, spirit beast.

Subject: BACK VIEW of spirit deer BACK LEG, viewed from behind,
long slender hind limb with subtle bend at hock (deer hock anatomy
— bend looks "backward" vs human knee, visible from rear), cream
fawn fur, **dark cloven hoof** at bottom from rear, NO body.

Palette: same as E.

Composition: 144x288 px (vertical, longest leg sprite), isolated
single back leg, top at hip pivot, bottom at hoof.

# NOTE: gen 1 lần flip cho leg_right.
```

```
=== Save to: Assets/_Project/Art/Characters/deer_spirit/N/tail.png === (HERO FEATURE — white flick raised from rear)

hand-painted painterly, spirit beast.

Subject: BACK VIEW of spirit deer SHORT WHITE TAIL FLICK — tiny
upturned puff viewed from rear (hero feature from this angle when
deer alert — bright white flick stands out against fawn back),
oriented with attachment ROOT at TOP (hip back), tail body extends
slightly UPWARD as small white puff, white fur with cream fawn
underneath, NO body.

Palette: white #e8d5a6 fur tip dominant (brightest — 40% stronger
than side view since white flick is signature alert showcase),
cream fawn #b89968 base color, warm shadow #6b5237 underside.

Composition: 96x128 px (small tail upturned), isolated tail on
transparent, root TOP pivot, NO body.
```

##### S (South / front view) — 4 prompts → 6 PNG (skip tail)

```
=== Save to: Assets/_Project/Art/Characters/deer_spirit/S/head.png === (HERO FEATURE — antlers face-on)

hand-painted painterly, asian wuxia, spirit beast.

Subject: FRONT VIEW of spirit deer HEAD ONLY, face directly facing
camera, **PAIR OF BRANCHING IVORY ANTLERS rising symmetric face-on
visible** (hero feature from front — 4-prong fork each side framing
face like a crown, ivory bone with pale jade green qi glow tips),
two large gentle dark eyes prominent and symmetric (most expressive
angle), twitching black nose centered, tall pointed ears pricked
alertly symmetric, cream fawn cheek fur framing face, NO body.

Palette: same as E.

Composition: 256x288 px (taller — antlers visible), isolated head,
perfectly symmetric front-facing, antlers fully visible, NO body.
```

```
=== Save to: Assets/_Project/Art/Characters/deer_spirit/S/torso.png ===

hand-painted painterly, asian wuxia, spirit beast.

Subject: FRONT VIEW of spirit deer BODY ONLY, oriented VERTICAL —
deer facing camera head-on, **white belly + chest dominant from
front** (hero feature from this angle — countershading creates
striking white-on-fawn silhouette), narrow elegant shoulders at top,
slim chest fading to white belly at bottom, NO head, NO legs, NO
tail.

Palette: same as E + emphasis on white #e8d5a6 belly underside
(rendered ~30% larger area than side view).

Composition: 176x240 px (vertical), isolated body, perfectly
symmetric front-facing, NO head, NO legs.
```

```
=== Save to: Art/Characters/deer_spirit/S/arm_left.png  AND  arm_right.png  (FRONT VIEW front legs) ===

hand-painted painterly, spirit beast.

Subject: FRONT VIEW of spirit deer FRONT LEG, viewed from front,
long graceful slender forelimb planted forward, cream fawn fur,
**dark cloven hoof** facing camera at bottom, NO body.

Palette: same as E.

Composition: 128x256 px (vertical), isolated single front leg, top
at shoulder, bottom at hoof.

# NOTE: gen 1 lần flip cho arm_right.
```

```
=== Save to: Art/Characters/deer_spirit/S/leg_left.png  AND  leg_right.png  (FRONT VIEW back legs) ===

hand-painted painterly, spirit beast.

Subject: FRONT VIEW of spirit deer BACK LEG, viewed from front,
long slender hind limb visible from front, cream fawn fur, **dark
cloven hoof** facing camera at bottom, NO body.

Palette: same as E.

Composition: 144x288 px (vertical), isolated single back leg, top
at hip, bottom at hoof.

# NOTE: gen 1 lần flip cho leg_right.
```

> **S/tail SKIP:** giống wolf/fox/rabbit/boar — front-view body chắn white tail flick. Controller fallback `deer_spirit/E/tail.png` runtime.

##### Negative prompt (paste vào field "Avoid" / "Negative prompt" mọi deer_spirit generation)

```
no pixel art, no photo-realistic, no anime moe, no chibi cute deer
mascot, no bambi cartoon, no rudolph reindeer, no fluffy plush toy
look, no fairy tale unicorn, no pure black outline, no smooth
airbrush gradient, no drop shadow on transparent background, no
text, no watermark, no signature, no border, single subject only,
no duplicate, no grid lines, no UI elements, no caption, no logo,
no lens flare, no ground beneath subject for body parts, no flower
crown decoration.
```

#### 3.6.7 Boss — Hắc Vương Full DST Set (12 prompts → 18 PNG)

> **Concept lock:** end-game humanoid villain (mirror Player rig, NO tail) — older Asian male DARK CULTIVATOR overlord, long ink-black hair with silver streak at temple, pale ivory skin, scar across left brow, **black silk robe with crimson trim + black cloak draping behind**, dark obsidian boots, blood-red waist sash. Hero feature: **CLOAK** dramatic from N back-view (full back coverage), peeks behind shoulders from S front-view. Antithesis of Player's white-and-blue robe — visual threat reads instantly.
>
> **Cross-dir consistency rule:** EXACT cùng palette + cùng silhouette anchor (cloak, sash, hair length past shoulder line) khắp 3 dir E/N/S. Mismatch palette = boss "đổi hình" khi xoay direction → break visual identity.
>
> **Pipeline:** copy block prompt → paste vào GPT Image 2.0 (hoặc Stable Diffusion XL / Midjourney) → save PNG vào path nằm trong header `=== Save to: ... ===` → next block. **12 prompts → 18 PNG output (6 parts × 3 dirs).** W mirror tự động qua `PuppetAnimController.spriteRoot.localScale` runtime — KHÔNG cần gen W.

##### E (East / side view) — 4 prompts → 6 PNG

```
=== Save to: Assets/_Project/Art/Characters/boss/E/head.png ===

hand-painted painterly digital illustration, visible brush strokes,
soft cel-shading, asian wuxia dark cultivation aesthetic,
limited color palette, clean readable silhouette, 1.5 to 2 pixel
mid-tone outline (dark crimson tone, NOT pure black).

Subject: side view of an older Asian male DARK CULTIVATOR
overlord HEAD ONLY, profile facing right, cold imperious
expression with narrow eyes, long ink-black hair flowing past
shoulder line with **SILVER STREAK at temple** (signature cursed-
power visual), pale ivory skin with faint diagonal SCAR across
left brow, faint crimson qi shimmer at temple, isolated single
body part on fully transparent background. NO neck visible below
jawline (clean cut).

Palette (use ONLY these): pale ivory skin #d8c8a8 base, deep
shadow #8a6f47 mid-tone, ink black #1a1a1a hair main, silver
streak #b8b0a4 hair highlight at temple, crimson qi #8c1923 faint
glow on temple, scar shadow #5a3520 brow line.

Composition: 256x256 px, isolated single head on transparent
background, vertically centered, hair must NOT crop at top edge
(allow full silhouette), NO body parts below jaw, NO shoulders,
NO neck, NO ground, NO shadow.
```

```
=== Save to: Assets/_Project/Art/Characters/boss/E/torso.png ===

hand-painted painterly digital illustration, visible brush strokes,
soft cel-shading, asian wuxia dark cultivation aesthetic.

Subject: side view of an older male DARK CULTIVATOR overlord
TORSO ONLY, profile facing right, neutral imperious standing pose,
wearing flowing **BLACK SILK ROBE with crimson blood-red trim** at
collar and waist sash, **wide BLACK CLOAK draping behind shoulders**
(visible in profile as silhouette extending behind torso), dark
obsidian shoulder pauldron at top, blood-red waist sash tied at side,
robe falls to mid-thigh, NO head NO arms NO legs visible (clean
cuts at shoulders, hips, neck).

Palette: black robe #1a0a0e base, deep shadow #0a0408 fold shadow,
crimson trim #8c1923 collar and sash accent, obsidian #2a1a2c
shoulder pauldron, blood-red sash highlight #b82838, silver thread
trim #5a4a48 minimal embroidery.

Composition: 256x384 px (vertical), isolated single torso on fully
transparent background, **CLOAK silhouette CAN extend slightly
beyond standard torso width** (cloak is part of torso), NO head,
NO limbs, NO ground, NO shadow. Top edge clean horizontal at
shoulder line (puppet rig pivot).
```

```
=== Save to: Assets/_Project/Art/Characters/boss/E/arm_left.png ===
=== Then mirror horizontally → Save to: Assets/_Project/Art/Characters/boss/E/arm_right.png ===

hand-painted painterly, visible brush strokes, asian wuxia dark.

Subject: side view of an older male dark cultivator's LEFT arm,
hanging straight down in neutral imperious pose, wearing flowing
BLACK SILK ROBE sleeve with CRIMSON CUFF trim, hand visible at
bottom (hand resting open with long fingers slightly curled —
sinister but composed), arm length proportional to mid-thigh,
NO body, NO head.

Palette: black robe sleeve #1a0a0e base, deep shadow #0a0408 fold
shadow, pale ivory #d8c8a8 skin tone for hand, crimson cuff trim
#8c1923 narrow band at wrist, silver thread #5a4a48 minimal
embroidery line.

Composition: 256x384 px (vertical), isolated single arm on fully
transparent background, top edge clean horizontal at shoulder
(pivot point), bottom edge at fingertips, NO body, NO ground.

# Workflow: gen 1 lần → save vào E/arm_left.png → flip horizontal
# (Photopea/GIMP: Image → Transform → Flip Horizontal) → save copy
# vào E/arm_right.png. Hoặc gen 2 lần với prompt "right arm" để có
# asymmetry tự nhiên.
```

```
=== Save to: Assets/_Project/Art/Characters/boss/E/leg_left.png ===
=== Then mirror horizontally → Save to: Assets/_Project/Art/Characters/boss/E/leg_right.png ===

hand-painted painterly, visible brush strokes, asian wuxia dark.

Subject: side view of an older male dark cultivator's LEFT leg,
straight imperious standing pose, wearing BLACK SILK robe pant
flowing to ankle, **dark obsidian boot** with subtle crimson
stitching (not cloth wrap like Player — boss wears stiff boots),
NO body, NO foot ground contact (foot just hovering in neutral
pose).

Palette: black robe pant #1a0a0e base, deep shadow #0a0408 fabric
fold, dark obsidian boot #2a1a2c, crimson stitching #8c1923 narrow
boot accent.

Composition: 256x384 px (vertical), isolated single leg on fully
transparent background, top edge clean horizontal at hip (pivot
point), bottom edge at sole of boot, NO body, NO ground, NO shadow.

# Workflow: same arm pattern — gen 1 lần flip cho leg_right.png.
```

##### N (North / back view) — 4 prompts → 6 PNG (hero direction!)

> **Why hero direction:** boss CLOAK fully visible across back from N — most dramatic visual reveal. Long flowing hair also dominant from back-view (covers most of head silhouette).

```
=== Save to: Assets/_Project/Art/Characters/boss/N/head.png ===

hand-painted painterly, asian wuxia dark cultivation.

Subject: BACK view of an older male dark cultivator overlord HEAD
ONLY, viewer behind subject, **long flowing ink-black hair** falls
past shoulder line covering most of head silhouette (signature back-
view feature), **silver streak at TEMPLE visible from behind** as
single lighter strand on right side of hair mass, NO face features
visible (back of head only), nape ~70% covered by hair.

Palette: ink black #1a1a1a hair main, silver streak #b8b0a4 single
strand right side, pale ivory #d8c8a8 small visible neck nape skin,
deep shadow #5a3520 hair shadow under shoulder.

Composition: 256x256 px, head on transparent, hair MUST NOT crop at
side edges (full hair silhouette flows past shoulder line), NO face,
NO body, NO ground.
```

```
=== Save to: Assets/_Project/Art/Characters/boss/N/torso.png ===

hand-painted painterly, asian wuxia dark cultivation.

Subject: BACK view of an older male dark cultivator overlord
TORSO ONLY, viewer behind subject, **wide BLACK CLOAK draped across
full back** (hero feature dramatic from N — cloak is the dominant
visual mass), crimson trim visible at collar back-of-neck and at
waist sash tied behind, dark obsidian shoulder pauldrons visible at
top corners, robe seams down center spine, NO head, NO arms, NO
legs (clean cuts).

Palette: black robe + cloak #1a0a0e dominant base, deep shadow
#0a0408 cloak fold shadow, crimson trim #8c1923 collar nape + sash
back-knot, obsidian pauldrons #2a1a2c, blood-red sash #b82838 knot
highlight at lower back.

Composition: 256x384 px (vertical), torso on transparent, **CLOAK
fully visible across BACK** (do NOT crop cloak — extends slightly
beyond standard torso width), top edge clean shoulder line, NO
head, NO limbs, NO ground.
```

```
=== Save to: Assets/_Project/Art/Characters/boss/N/arm_left.png ===
=== Then mirror horizontally → Save to: Assets/_Project/Art/Characters/boss/N/arm_right.png ===

hand-painted painterly, asian wuxia dark.

Subject: BACK view of an older male dark cultivator's LEFT arm,
viewer behind subject, hanging straight down, BLACK SILK robe
sleeve visible from behind with **crimson cuff trim** at wrist,
hand silhouette at bottom shows back of hand (knuckles visible —
NOT palm), arm length proportional, NO body, NO head.

Palette: black robe sleeve #1a0a0e base, deep shadow #0a0408 fold
shadow, pale ivory #d8c8a8 skin tone for back-of-hand, crimson cuff
#8c1923 narrow band at wrist.

Composition: 256x384 px (vertical), arm on transparent, top edge
clean shoulder pivot, bottom edge at fingertips/knuckles, NO body.

# Workflow: gen 1 lần → flip horizontal → arm_right.png.
```

```
=== Save to: Assets/_Project/Art/Characters/boss/N/leg_left.png ===
=== Then mirror horizontally → Save to: Assets/_Project/Art/Characters/boss/N/leg_right.png ===

hand-painted painterly, asian wuxia dark.

Subject: BACK view of an older male dark cultivator's LEFT leg,
viewer behind subject, straight imperious standing pose, BLACK SILK
robe pant from behind, dark obsidian boot at bottom (boot back
visible — heel + back seam, NOT toe), crimson stitching narrow band
at boot top, NO body, NO foot ground contact.

Palette: black robe pant #1a0a0e base, deep shadow #0a0408 fabric
fold, dark obsidian #2a1a2c boot, crimson stitching #8c1923 narrow
boot accent.

Composition: 256x384 px (vertical), leg on transparent, top edge
clean hip pivot, bottom edge at boot heel, NO body, NO ground.
```

##### S (South / front view) — 4 prompts → 6 PNG (face-on imperious confrontation)

> **Boss S front-view is the COMBAT framing**: when player runs UP toward boss, they see boss face-on with scar + cold eyes + cloak peeking from behind shoulders. Most intimidating visual.

```
=== Save to: Assets/_Project/Art/Characters/boss/S/head.png ===

hand-painted painterly, asian wuxia dark cultivation.

Subject: FRONT view of an older Asian male DARK CULTIVATOR
overlord HEAD ONLY, face-on toward viewer, cold imperious
expression with narrow eyes (face symmetric front-on), thin lips,
**SCAR diagonal across LEFT eyebrow** (visible from front as
signature feature), pale ivory skin, ink-black hair parted center
flowing past shoulders on BOTH sides, **silver streaks at BOTH
temples** (front view shows streaks symmetric — small detail), faint
crimson qi shimmer at temples.

Palette: pale ivory skin #d8c8a8 base, deep shadow #8a6f47 cheek
shadow, ink black #1a1a1a hair, silver streak #b8b0a4 temple
highlights both sides, crimson qi #8c1923 faint glow temples, scar
shadow #5a3520 brow line on left.

Composition: 256x256 px, head on transparent, vertically centered
+ horizontally centered, hair MUST NOT crop at side edges, NO body
parts below jaw, NO neck, NO shoulders, NO ground.
```

```
=== Save to: Assets/_Project/Art/Characters/boss/S/torso.png ===

hand-painted painterly, asian wuxia dark cultivation.

Subject: FRONT view of an older male DARK CULTIVATOR overlord
TORSO ONLY, face-on toward viewer, neutral imperious standing
pose, BLACK SILK ROBE with **CRIMSON BLOOD-RED collar V-neck trim**
front-and-center, **blood-red waist sash tied with knot front-
center** (knot bow visible face-on), dark obsidian shoulder
pauldrons at top corners, **black cloak peeks from BEHIND shoulders**
as 2 narrow vertical strips (cloak is mostly hidden in front view),
NO head, NO arms, NO legs.

Palette: black robe #1a0a0e front base, deep shadow #0a0408 fold,
crimson collar V #8c1923, blood-red sash highlight #b82838 front-
center knot, obsidian pauldrons #2a1a2c shoulder corners, silver
thread #5a4a48 minimal embroidery on collar.

Composition: 256x384 px (vertical), torso on transparent,
horizontally symmetric body silhouette, **cloak narrow strips
visible behind shoulders**, top edge clean shoulder line, NO head,
NO limbs, NO ground.
```

```
=== Save to: Assets/_Project/Art/Characters/boss/S/arm_left.png ===
=== Then mirror horizontally → Save to: Assets/_Project/Art/Characters/boss/S/arm_right.png ===

hand-painted painterly, asian wuxia dark.

Subject: FRONT view of an older male dark cultivator's LEFT arm,
face-on toward viewer, hanging straight down, BLACK SILK robe
sleeve face-on with crimson cuff trim at wrist, hand silhouette at
bottom shows palm-down face-on (back of hand toward viewer for
natural arm pose), NO body, NO head.

Palette: black robe sleeve #1a0a0e base, deep shadow #0a0408 fold,
pale ivory #d8c8a8 skin tone for back-of-hand, crimson cuff #8c1923
narrow band wrist.

Composition: 256x384 px (vertical), arm on transparent, top edge
clean shoulder pivot, bottom edge at fingertips, NO body.

# Workflow: gen 1 lần → flip horizontal → arm_right.png.
```

```
=== Save to: Assets/_Project/Art/Characters/boss/S/leg_left.png ===
=== Then mirror horizontally → Save to: Assets/_Project/Art/Characters/boss/S/leg_right.png ===

hand-painted painterly, asian wuxia dark.

Subject: FRONT view of an older male dark cultivator's LEFT leg,
face-on toward viewer, straight imperious standing pose, BLACK SILK
robe pant face-on, **dark obsidian boot toe visible** at bottom
(boot front face — toe + arch + crimson stitching trim), NO body,
NO foot ground contact.

Palette: black robe pant #1a0a0e base, deep shadow #0a0408 fold,
dark obsidian #2a1a2c boot front, crimson stitching #8c1923 narrow
boot accent.

Composition: 256x384 px (vertical), leg on transparent, top edge
clean hip pivot, bottom edge at boot toe, NO body, NO ground.
```

##### Negative prompt (paste vào field "Avoid" / "Negative prompt" mọi boss generation)

```
no pixel art, no photo-realistic, no anime moe, no chibi cute boy,
no shounen hero overlord, no mecha armor, no full plate knight, no
spiky devil horns, no skull mask, no demon claws, no fluffy mascot,
no pure black outline, no smooth airbrush gradient, no drop shadow
on transparent background, no text, no watermark, no signature, no
border, single subject only, no duplicate, no grid lines, no UI
elements, no caption, no logo, no lens flare, no ground beneath
subject for body parts, no weapon (no sword, no staff — boss is
unarmed apex), no bright colors (no green/blue/yellow tunic — boss
is BLACK + CRIMSON only).
```

---

## 4. Single-Sprite Mobs

> **Pipeline:** Drop single PNG `Sprites/{mobId}.png` (legacy gen_sprites.py path) hoặc Art/Mobs/{mobId}/sprite.png (sau khi MobArtImporter merge — chưa có).
>
> **Pose:** side view, facing right, neutral standing.
>
> **Resolution:** 256×256 hoặc 256×192 (depends on aspect).
> **Style:** match puppet characters per ART_STYLE.md.

### 4.1 Rabbit — Linh Thố (Forest mob, peaceful) — **DEPRECATED → §3.3.5 / §3.6.4 puppet**

> **Phase 2A migration:** Rabbit đã upgrade lên multi-piece quadruped puppet (mirror Wolf/Fox pattern). Master prompts ở §3.3.5, quick-copy bundle ở §3.6.4 (14 prompts → 20 PNG). Single-sprite prompt dưới giữ làm reference cho ai chỉ muốn flat fallback (1 PNG drop ở `Art/Characters/rabbit/E/torso.png` — controller fallback skeleton placeholder cho parts còn thiếu).

```
=== rabbit (legacy single-sprite — chỉ dùng nếu skip puppet) ===

hand-painted painterly, asian wuxia, wilderness creature, soft cel-shading,
visible brush strokes.

Subject: side view of a small wild forest rabbit, profile facing right,
neutral upright standing pose with front paws lifted slightly, twitching
nose, long ears half-back in alert mode, soft brown fur with white belly
and tail tuft, big curious black eye. Cute but not chibi — proportional
realistic anatomy.

Palette: bark brown #b89968 fur base, dry leaf #8a6f47 fur shadow, bone
cream #e8d5a6 belly and tail tuft highlight, ink black #1a1a1a eye and
nose, sage green #6b8e62 leaf decoration on side.

Composition: 256x256 px, isolated single rabbit on transparent background,
NO ground, NO shadow, centered.
```

### 4.2 Boar — Lợn Rừng (Forest mob, aggressive) — **DEPRECATED → §3.3.6 / §3.6.5 puppet**

> **Phase 2B migration:** Boar đã upgrade lên multi-piece quadruped puppet (mirror Wolf/Fox pattern). Master prompts ở §3.3.6, quick-copy bundle ở §3.6.5 (14 prompts → 20 PNG). Single-sprite prompt dưới giữ làm reference cho ai chỉ muốn flat fallback (1 PNG drop ở `Art/Characters/boar/E/torso.png` — controller fallback skeleton placeholder cho parts còn thiếu).

```
=== boar ===

hand-painted painterly, asian wuxia, wilderness creature.

Subject: side view of a fierce wild boar, profile facing right, lowered
head with two curved tusks visible, bristly dark brown fur with stiff
mane along spine, muscular boxy body on stout legs, small black eye,
flared nostrils. Threatening posture but stationary stance.

Palette: dry leaf #8a6f47 fur base, dirt shadow #8b7355 deep shadow,
bone cream #e8d5a6 tusk, primary gold #d4a64a small eye highlight, ink
black #1a1a1a outline and hoof.

Composition: 256x192 px (horizontal — boar wider than tall), isolated
boar on transparent, NO ground.
```

### 4.3 Deer Spirit — Linh Lộc (Forest mob, mystical) — **DEPRECATED → §3.3.7 / §3.6.6 puppet**

> **Phase 2B migration:** Deer Spirit đã upgrade lên multi-piece quadruped puppet (mirror Wolf/Fox pattern). Master prompts ở §3.3.7, quick-copy bundle ở §3.6.6 (14 prompts → 20 PNG). Single-sprite prompt dưới giữ làm reference cho ai chỉ muốn flat fallback (1 PNG drop ở `Art/Characters/deer_spirit/E/torso.png` — controller fallback skeleton placeholder cho parts còn thiếu).

```
=== deer_spirit ===

hand-painted painterly, asian wuxia, supernatural ethereal.

Subject: side view of a graceful spirit deer, profile facing right,
slender legs in standing pose, head turned slightly toward viewer with
curious gentle gaze, branching antlers with faint glowing qi runes
inscribed, white spotted dappled coat with jade green undertones, hooves
darker, ethereal mist swirling at hooves dissolving to transparent.

Palette: bone cream #e8d5a6 coat base, sage green #6b8e62 jade undertone
shadow, primary gold #d4a64a antler glow, jade green #6b8e62 rune lines,
spirit qi blue #a8d8ff mist accent at hoof, ink black #1a1a1a outline.

Composition: 256x256 px, isolated deer on transparent, NO ground.
```

### 4.4 Crow — Hắc Vũ (Highlands mob, scavenger)

```
=== crow ===

hand-painted painterly, asian wuxia, wilderness creature.

Subject: side view of a large black crow perched in alert mode, profile
facing right, glossy black feathers with iridescent blue-purple sheen on
back, sharp curved beak, beady yellow eye, tail feather slightly raised,
talons gripping invisible perch.

Palette: ink black #1a1a1a feather base, slate gray #7a7c80 feather
mid-tone, mineral blue #4d6b8c iridescent sheen highlight, primary gold
#d4a64a eye and beak edge, shadow stone #5a5d63 deep shadow.

Composition: 256x256 px, isolated crow on transparent, NO perch, NO
ground, beady eye visible toward right.
```

### 4.5 Snake — Thanh Xà (Forest/Highlands mob, ambush)

```
=== snake ===

hand-painted painterly, asian wuxia, wilderness creature.

Subject: side view of a coiled green snake ready to strike, S-shape body
silhouette, head raised on right side facing forward, long forked tongue
visible, glossy emerald green scales with faint pattern, slit pupils
golden eye, smooth scaled body coiled tightly.

Palette: jade green #6b8e62 scale base, deep moss #4a6741 scale shadow,
mint highlight #a8c69b scale highlight, primary gold #d4a64a eye, ink
black #1a1a1a slit pupil and outline, cinnabar red #8b3a3a tongue.

Composition: 256x192 px (horizontal — coiled snake wider), isolated
snake on transparent, NO ground.
```

### 4.6 Bat — Bức (Cave/Night mob, fast)

```
=== bat ===

hand-painted painterly, asian wuxia, wilderness creature.

Subject: side view of a fruit bat in flight, profile facing right, wings
spread mid-flap, leathery dark brown wings with prominent finger bones,
furry brown body, pointed ears, tiny eyes, sharp small fangs visible.

Palette: dirt shadow #8b7355 wing base, dirt deep #6b5d40 wing shadow,
bark brown #b89968 fur body, ink black #1a1a1a outline, cinnabar red
#8b3a3a tiny inner ear, bone cream #e8d5a6 fang highlight.

Composition: 256x192 px (horizontal — wingspan wider than tall),
isolated bat on transparent, NO ground.
```

### 4.7 Boss Mob — Hắc Vương (Cursed Desert, end-game) — Phase 1 — **DEPRECATED → §3.3.8 / §3.6.7 puppet**

> **Phase 2C migration:** Boss đã upgrade lên multi-piece humanoid puppet (mirror Player rig, NO tail). Master prompts ở §3.3.8, full DST quick-copy bundle ở §3.6.7 (12 prompts → 18 PNG cho E + N + S 3 directions). Single-sprite "spirit boar boss" prompt dưới giữ làm reference cho concept fallback (hoặc nếu user muốn boss biến thân thành dạng quái thú phase 2 — giữ cho biến hình special-case). Default flow: dùng puppet bundle §3.6.7.

```
=== boss_mob ===

hand-painted painterly digital illustration, visible brush strokes,
soft cel-shading, asian wuxia supernatural demonic aesthetic,
limited color palette, 1.5 to 2 pixel mid-tone outline (NOT pure black),
NOT pixel art, NOT photo-realistic, NOT anime moe.

Subject: side view of a massive twisted spirit boar boss in PHASE 1
"stalking presence", hulking distorted form 2x size of normal boar,
profile facing right, lowered head with broken crown of curved black
tusks (some shattered, some intact), shaggy pelt corrupted with faint
death qi purple smoke wisps drifting from spine, glowing red eye sockets
(low intensity baseline state), exposed cracked ribs showing inner
darkness as faint texture, cloven hooves bloody, neutral standing
threatening posture, malevolent presence.

Palette: ink black #1a1a1a body base, death qi purple #9b6b8b corruption
mist (subtle), dirt shadow #8b7355 hide highlight, cinnabar red #8b3a3a
glowing eye and inner ribcage (low saturation phase 1), bone bleached
#d4d4d4 cracked tusk, dirt deep #6b5d40 deep shadow.

Composition: 384x256 px (horizontal — boss bigger than mob), isolated
boss on fully transparent background, NO ground, NO shadow.
```

### 4.8 Boss Mob — Phase 2 Enraged (HP < 30%)

```
=== boss_mob_phase2 ===

hand-painted painterly, visible brush strokes, asian wuxia supernatural
demonic, NOT pixel, NOT photo, NOT anime.

Subject: side view of the SAME massive twisted spirit boar boss in
PHASE 2 "enraged" state, profile facing right, head raised in roar pose
with maw open showing jagged teeth and inner cinnabar glow, broken
crown of black tusks now leaking purple death qi flame from cracks,
shaggy pelt visibly torn with cracked exposed ribcage glowing bright
cinnabar red from within (HP critical visual cue), heavy purple death
qi mist trail erupting from spine and rear (3x stronger than phase 1),
glowing red eye sockets at maximum intensity (full bright cinnabar +
gold flicker), cloven hooves cracked with ember glow leaking from foot
sole, demonic awakened presence.

Palette: ink black #1a1a1a body base, death qi purple #9b6b8b corruption
mist DOMINANT (3x phase 1), cinnabar red #8b3a3a glowing eye/maw/ribcage
(MAXIMUM saturation), primary gold #d4a64a inner ember flicker accent,
dirt shadow #8b7355 hide, bone bleached #d4d4d4 cracked tusk, dirt deep
#6b5d40 deep shadow.

Composition: 384x256 px (horizontal), isolated boss on fully transparent
background, NO ground, NO shadow.

# NOTE: Phase 2 sprite có thể swap-in trên cùng MobBase prefab khi
# HP < threshold. Nếu pipeline chưa hỗ trợ phase swap → ship Phase 1
# cho v1.0, Phase 2 ship cùng PR boss-rework.
```

### 4.9 Boss Mob — Death Decay (post-death corpse)

```
=== boss_mob_dead ===

hand-painted painterly, visible brush strokes, asian wuxia.

Subject: side view of the defeated spirit boar boss CORPSE lying on
side, profile facing right, body slumped flat with eye sockets dim
(no red glow), broken tusks scattered, purple death qi mist
EVAPORATING from body (light wisps fading upward into transparent),
ribcage no longer glowing, hooves limp, fur matted, peaceful death
finality.

Palette: ink black #1a1a1a body, dirt deep #6b5d40 darkest shadow,
death qi purple #9b6b8b mist (FADED 20% intensity, dispersing not
glowing), bone bleached #d4d4d4 tusk fragment, dirt shadow #8b7355
hide.

Composition: 384x192 px (horizontal — body lying lower), isolated
corpse on transparent, NO ground, NO blood pool (game uses VFX overlay).
```

---

## 5. Resources / World Objects

> **Pipeline:** Drop PNG vào `Assets/_Project/Art/Resources/{id}/{anyname}.png`. ResourceArtImporter scan + auto-PPU.
>
> **View angle:** **top-down 30°** (subject thấy mặt trên + một phần mặt trước) — phù hợp với top-down camera của game. KHÁC với puppet body parts (side 90°) và tile (overhead 90°).
>
> **Pose:** static, neutral, single object centered.

### 5.1 Tree — Cổ Linh Mộc (`Art/Resources/tree/`)

```
=== tree ===

hand-painted painterly, asian wuxia, top-down 30 degree perspective angle,
visible brush strokes, soft cel-shading.

Subject: large ancient mystical forest tree (linh mộc cổ thụ), thick
moss-covered bark trunk, broad leafy canopy with layered foliage,
exposed gnarled roots at base, faint qi glow in canopy upper, single
isolated tree.

Palette (forest): bark brown #b89968 trunk base, dry leaf #8a6f47 bark
shadow, deep moss #4a6741 darkest leaves, sage green #6b8e62 mid leaves,
mint highlight #a8c69b sun-lit canopy edge, spirit qi blue #a8d8ff faint
glow, ink black #1a1a1a hint outline.

Composition: 512x768 px (vertical — tree taller than wide), top-down 30°
showing canopy upper + partial trunk + root, isolated single tree on
fully transparent background, NO ground, NO shadow.
```

### 5.2 Rock — Linh Thạch (`Art/Resources/rock/`)

```
=== rock ===

hand-painted painterly, asian wuxia, top-down 30°.

Subject: large mossy boulder rock formation, irregular weathered shape
with deep cracks, dry moss patches on top facing camera, faint mineral
veins glinting on surface, single isolated rock.

Palette (highlands): slate gray #7a7c80 base, shadow stone #5a5d63
deep crack shadow, highlight stone #a3a5a8 sun-lit top, dry moss #8a9b8c
patch, mineral blue #4d6b8c subtle vein, ink black #1a1a1a crack outline.

Composition: 512x384 px, top-down 30°, isolated single rock on
transparent, NO ground beneath.
```

### 5.3 Mineral Rock — Khoáng Thạch (`Art/Resources/rock/` variant — same folder)

```
=== mineral_rock ===

hand-painted painterly, asian wuxia, top-down 30°.

Subject: rock outcrop with prominent glowing mineral ore vein, irregular
boulder form with bright crystalline blue mineral cluster jutting from
crack, single isolated rock with mineral focus point.

Palette: slate gray #7a7c80 stone, mineral blue #4d6b8c bright vein,
spirit qi blue #a8d8ff glowing crystal highlight, primary gold #d4a64a
rare gold flake, ink black #1a1a1a outline.

Composition: 512x384 px, top-down 30°, isolated on transparent.

# NOTE: hiện BootstrapWizard reuse sprites["rock"] cho mineral rock.
# Khi user muốn variant riêng → drop file thứ 2 vào folder rock/, hoặc
# tách folder mineral_rock/ + thêm fallback chain.
```

### 5.4 Water Spring — Linh Tuyền (`Art/Resources/water/`)

```
=== water ===

hand-painted painterly, asian wuxia, top-down view (slightly angled to
show water surface).

Subject: small natural spirit water spring pool, circular pool with
gentle ripples on surface, surrounding small rocks, lotus leaves floating
on surface, faint spirit qi mist rising, glow runes faintly visible
beneath water.

Palette: spirit qi blue #a8d8ff dominant water highlight, sky qi mid
#6fb5e0 water mid-tone, mineral blue #4d6b8c water deep shadow, sage
green #6b8e62 lotus leaves, slate gray #7a7c80 surrounding rocks, primary
gold #d4a64a faint rune glow under water.

Composition: 512x512 px, top-down slightly angled, isolated single pool
on transparent background, NO ground around (cleanly cropped circle).
```

### 5.5 Linh Mushroom — Linh Nấm (`Art/Resources/linh_mushroom/`)

```
=== linh_mushroom ===

hand-painted painterly, asian wuxia, top-down 30°.

Subject: cluster of 3 magical forest mushrooms growing from same base,
tall slender stems with bright red caps spotted with bone cream dots,
faint qi glow under cap, small grass tuft at base, single isolated
cluster.

Palette: mushroom red #a14040 cap dominant, bone cream #e8d5a6 stem
and cap dots, dry leaf #8a6f47 stem shadow, sage green #6b8e62 grass
tuft, spirit qi blue #a8d8ff faint underglow.

Composition: 384x384 px, top-down 30°, isolated cluster on transparent
background, NO ground.
```

### 5.6 Berry Bush — Linh Quả Mọng (`Art/Resources/berry_bush/`)

```
=== berry_bush ===

hand-painted painterly, asian wuxia, top-down 30°.

Subject: small forest berry bush with glossy dark leaves and clusters
of bright red berries hanging visible, dense compact form, branches
visible through gaps, single isolated bush.

Palette: deep moss #4a6741 leaf shadow, sage green #6b8e62 leaf base,
mint highlight #a8c69b leaf edge, cinnabar red #8b3a3a berry, primary
gold #d4a64a small flower hint, bark brown #b89968 visible branch.

Composition: 384x384 px, top-down 30°, isolated bush on transparent.
```

### 5.7 Cactus — Tiên Nhân Chưởng (`Art/Resources/cactus/`)

```
=== cactus ===

hand-painted painterly, asian wuxia, top-down 30°.

Subject: tall barrel cactus desert plant with vertical ribs and visible
spines, one or two short side arms, single small white flower bud at
top, base buried slightly in sand crust, single isolated cactus.

Palette: cactus green #6b8559 body base, deep moss #4a6741 shadow rib,
mint highlight #a8c69b sun-lit ridge, bone cream #e8d5a6 spine and
flower, sand base #c4a574 ground crust hint at root.

Composition: 384x512 px (vertical — tall cactus), top-down 30°,
isolated on transparent.
```

### 5.8 Death Lily — Tử Linh Hoa (`Art/Resources/death_lily/`)

```
=== death_lily ===

hand-painted painterly, asian wuxia, top-down 30°, supernatural.

Subject: cursed dark lily flower in full bloom, dark purple petals with
glowing edge, drooping leaves, death qi purple mist swirling at base
dissolving to transparent, withered stem, single isolated flower.

Palette: death qi purple #9b6b8b petal base, ink black #1a1a1a petal
shadow, bone bleached #d4d4d4 stamen highlight, dirt shadow #8b7355
withered stem, primary gold #d4a64a faint cursed glow on petal edge,
spirit qi blue #a8d8ff toxic mist accent.

Composition: 384x384 px, top-down 30°, isolated flower on transparent.
```

### 5.9 Linh Bamboo — Linh Trúc (`Art/Resources/linh_bamboo/`)

```
=== linh_bamboo ===

hand-painted painterly, asian wuxia, top-down 30°.

Subject: cluster of 3 tall slender bamboo stalks growing close together,
visible nodes along stalk, fan of long thin leaves at top, faint jade
qi shimmer along stalk, single isolated cluster, base anchored.

Palette: cactus green #6b8559 stalk base, deep moss #4a6741 node shadow,
mint highlight #a8c69b stalk highlight, sage green #6b8e62 leaf, primary
gold #d4a64a faint qi accent.

Composition: 384x768 px (very tall vertical), top-down 30°, isolated
cluster on transparent.
```

### 5.10 Grass Tile Decoration — Linh Cỏ (`Art/Resources/grass_tile/` — chưa có folder, gen → tạo)

```
=== grass_tile ===

hand-painted painterly, asian wuxia, top-down 90° (overhead — vì grass
tile decoration paint trên ground tile).

Subject: small patch of tall lush forest grass tuft, mixed grass blade
heights, occasional tiny white wildflower, isolated patch, decoration
overlay (semi-transparent edge OK).

Palette (forest): sage green #6b8e62 base, mint highlight #a8c69b tip,
deep moss #4a6741 shadow at base, bone cream #e8d5a6 small flower dot.

Composition: 256x256 px, overhead 90° view, isolated grass patch on
transparent background, soft edge fade (vì decorate trên tile).
```

### 5.11 Campfire (`Art/Resources/campfire/` — chưa có folder)

```
=== campfire ===

hand-painted painterly, asian wuxia, top-down 30°.

Subject: small camping campfire — circular ring of stones surrounding
burning logs in tent shape, flickering warm flame with sparks, faint
smoke rising fading to transparent, ember glow on stones.

Palette: cinnabar red #8b3a3a flame core, primary gold #d4a64a flame
mid, bone cream #e8d5a6 hot spark, slate gray #7a7c80 surrounding
stones, dry leaf #8a6f47 burnt log, ink black #1a1a1a charred ash.

Composition: 384x384 px, top-down 30°, isolated campfire on transparent.

# NOTE: For animated flame, gen 4 frame variation prompt nhỏ — flame
# shape varies. PR sau (animation) handle.
```

### 5.12 Workbench (`Art/Resources/workbench/` — chưa có folder)

```
=== workbench ===

hand-painted painterly, asian wuxia, top-down 30°.

Subject: rustic wooden crafting workbench, weathered planks with iron
nails visible, hammer and chisel tools resting on top, partially completed
crafted item (vague form), small wood shaving piles, single isolated
station.

Palette: bark brown #b89968 wood base, dry leaf #8a6f47 wood shadow,
slate gray #7a7c80 iron tool, primary gold #d4a64a small accent, ink
black #1a1a1a outline.

Composition: 512x384 px (horizontal — workbench wider), top-down 30°,
isolated on transparent.
```

### 5.13 State Variants — Harvested / Depleted

> **Khi cần:** ResourceNode có thể switch sprite theo respawn/harvested state. Hiện pipeline mặc định
> dùng 1 sprite + tween scale/alpha cho depleted. Nếu user muốn hand-painted depleted state khác
> (vd "tree stump" thay vì "tree shrunk"), gen variant theo prompt dưới đây và đặt cùng folder
> với tên `{id}_depleted.png` hoặc `{id}_stump.png`. Kéo manual vào ResourceNode.depletedSprite trong Inspector.
>
> **Frequency:** chủ yếu dùng cho tree (stump) + rock (rubble). Mushroom/berry/cactus tween đã đủ.

```
=== tree_stump ===

hand-painted painterly digital illustration, visible brush strokes,
soft cel-shading, asian wuxia cultivation fantasy aesthetic,
top-down 30 degree perspective, 1.5 to 2 pixel mid-tone outline.

Subject: harvested ancient mystical tree stump, broad flat cut surface
on top showing inner tree rings concentric pattern, weathered moss-
covered bark on side, exposed gnarled root base, scattered wood chips
and small twig fragments at base, faint residual qi shimmer in core
ring, single isolated stump (canopy completely removed).

Palette (forest): bark brown #b89968 trunk base, dry leaf #8a6f47 bark
shadow, dirt deep #6b5d40 inner ring darkest, sage green #6b8e62 moss
patch, mint highlight #a8c69b moss tip, primary gold #d4a64a faint
core ring shimmer, ink black #1a1a1a fine outline.

Composition: 384x256 px (horizontal — stump wider than tall, no canopy),
top-down 30°, isolated single stump on fully transparent background,
NO ground, NO shadow.
```

```
=== rock_rubble ===

hand-painted painterly, visible brush strokes, asian wuxia, top-down 30°.

Subject: harvested rock rubble pile, irregular small broken stone
fragments scattered in low pile, faint mineral dust drifting, weathered
edges showing recent break exposed lighter surface, single isolated
rubble cluster (boulder reduced to small pieces).

Palette (highlands): slate gray #7a7c80 base, shadow stone #5a5d63
deep crack shadow, highlight stone #a3a5a8 fresh-broken edge, dry moss
#8a9b8c sparse moss residue, mineral blue #4d6b8c minimal trace vein,
ink black #1a1a1a fine outline.

Composition: 384x192 px (horizontal — pile wider than tall), top-down
30°, isolated rubble pile on transparent, NO ground.
```

```
=== mushroom_picked ===

hand-painted painterly, visible brush strokes, asian wuxia, top-down 30°.

Subject: harvested mushroom base — only stem stubs remaining at ground
where mushroom cluster was picked, small grass tuft regrowing at base,
faint qi residual underglow, no caps visible (caps fully harvested),
single isolated cluster base.

Palette: dry leaf #8a6f47 stem stub, bark brown #b89968 stub side
shadow, sage green #6b8e62 grass tuft, mint highlight #a8c69b grass
tip, spirit qi blue #a8d8ff faint underglow.

Composition: 256x192 px (horizontal — flat low cluster), top-down 30°,
isolated on transparent, NO ground.
```

```
=== berry_bush_picked ===

hand-painted painterly, visible brush strokes, asian wuxia, top-down 30°.

Subject: harvested berry bush — same dense leafy bush silhouette as
full version BUT with NO red berries visible (all picked), small
torn-leaf fragments where berries were attached, dense dark green
foliage with sparse remaining flower buds (no fruit), single isolated
bush.

Palette: deep moss #4a6741 leaf shadow dominant, sage green #6b8e62
leaf base, mint highlight #a8c69b leaf edge sparse, primary gold
#d4a64a small flower hint, bark brown #b89968 visible branch (NO red).

Composition: 384x384 px, top-down 30°, isolated bush on transparent.
```

```
=== cactus_picked ===

hand-painted painterly, visible brush strokes, asian wuxia, top-down 30°.

Subject: harvested barrel cactus, same vertical ribbed body silhouette
as full version BUT with prominent fresh cut scar on top where flower
bud was harvested (clean cut showing pale inner flesh), spines intact,
small drop of clear cactus liquid at cut, single isolated cactus.

Palette: cactus green #6b8559 body base, deep moss #4a6741 shadow rib,
mint highlight #a8c69b sun-lit ridge, bone cream #e8d5a6 spine and
inner cut flesh, sand base #c4a574 ground crust hint at root, sky qi
mid #6fb5e0 minimal cactus liquid drop.

Composition: 384x512 px (vertical), top-down 30°, isolated on transparent.
```

```
=== bamboo_picked ===

hand-painted painterly, visible brush strokes, asian wuxia, top-down 30°.

Subject: harvested bamboo cluster — 2 of 3 stalks cut at mid-height
showing clean diagonal cut surface (revealing inner hollow), 1 stalk
intact, fan of leaves on intact stalk only, faint jade qi shimmer at
cut surface, base anchored.

Palette: cactus green #6b8559 stalk base, deep moss #4a6741 node shadow,
mint highlight #a8c69b stalk highlight, sage green #6b8e62 leaf, bone
cream #e8d5a6 inner cut hollow, primary gold #d4a64a faint qi accent.

Composition: 384x768 px (very tall vertical), top-down 30°, isolated
cluster on transparent.
```

---

## 6. Item Icons

> **Pipeline:** Drop PNG vào `Assets/_Project/Art/Items/{icon_id}.png` (sau khi item asset pipeline merge — chưa có).
>
> **View angle:** centered overhead / 3/4 view, transparent background, single object focus.
>
> **Resolution:** 128×128 (per ART_STYLE.md §4.1 item PPU 128).
>
> **Style:** painterly with stronger outline (icon dễ đọc trên UI panel).

### 6.1 Materials

```
=== icon_stick ===
hand-painted painterly icon, asian wuxia inventory item, soft cel-shading.
Subject: single weathered tree branch stick, knot bumps visible, slight
curve, broken end on one side, centered horizontal-diagonal.
Palette: bark brown #b89968 base, dry leaf #8a6f47 shadow, ink black
#1a1a1a outline, mint #a8c69b tiny moss patch hint.
Composition: 128x128 px, isolated stick centered on transparent
background, top-down 3/4 view, NO ground.

=== icon_stone ===
hand-painted painterly icon, asian wuxia.
Subject: small rough gray stone with chipped edges, weathered surface
with crack lines, irregular pebble shape.
Palette: slate gray #7a7c80 base, shadow stone #5a5d63 crack, highlight
stone #a3a5a8 lit edge.
Composition: 128x128 px isolated, transparent.

=== icon_tough_hide ===
hand-painted painterly icon, asian wuxia.
Subject: roll of thick brown leather animal hide, fur side visible at
edge, leather inner side, rolled up cylindrical, leather strap binding.
Palette: bark brown #b89968 leather, dry leaf #8a6f47 fur, dirt shadow
#8b7355 strap.
Composition: 128x128 px isolated, transparent.

=== icon_tusk ===
hand-painted painterly icon, asian wuxia.
Subject: single curved boar tusk, ivory bone color, sharp tip, broad
base, slight curve, centered diagonal.
Palette: bone cream #e8d5a6 base, bone bleached #d4d4d4 highlight, dry
leaf #8a6f47 base shadow, ink black #1a1a1a fine outline.
Composition: 128x128 px isolated, transparent.

=== icon_spirit_antler ===
hand-painted painterly icon, asian wuxia, supernatural.
Subject: branch of spirit deer antler with glowing qi rune lines, ivory
white antler with primary gold rune carving, small spirit qi blue mist
trailing at base, three-prong branching shape.
Palette: bone cream #e8d5a6 antler, primary gold #d4a64a rune, spirit
qi blue #a8d8ff mist.
Composition: 128x128 px isolated, transparent.

=== icon_feather ===
hand-painted painterly icon, asian wuxia.
Subject: single black crow feather, glossy with iridescent blue-purple
sheen on barbs, slightly curved quill, fine detail on barbs.
Palette: ink black #1a1a1a base, mineral blue #4d6b8c iridescent sheen,
slate gray #7a7c80 shadow.
Composition: 128x128 px isolated, transparent.

=== icon_snake_skin ===
hand-painted painterly icon, asian wuxia.
Subject: shed snake skin, jade green scaled pattern, semi-translucent,
crumpled flat with diamond scale visible.
Palette: jade green #6b8e62 base, mint highlight #a8c69b scale edge,
deep moss #4a6741 shadow.
Composition: 128x128 px isolated, transparent.

=== icon_venom_gland ===
hand-painted painterly icon, asian wuxia.
Subject: small organic toxic gland sac, dark purple semi-translucent
membrane, glowing inner toxic liquid, small drip at bottom.
Palette: death qi purple #9b6b8b membrane, ink black #1a1a1a outline,
spirit qi blue #a8d8ff toxic glow inside.
Composition: 128x128 px isolated, transparent.

=== icon_bat_wing ===
hand-painted painterly icon, asian wuxia.
Subject: single bat wing membrane spread, leathery brown with finger
bones visible, hooked claw at top.
Palette: dirt shadow #8b7355 wing, ink black #1a1a1a finger bone, bark
brown #b89968 highlight.
Composition: 128x128 px isolated, transparent.

=== icon_bamboo ===
hand-painted painterly icon, asian wuxia.
Subject: short cut section of bamboo stalk with visible nodes, jade
green scaled segments.
Palette: cactus green #6b8559 base, deep moss #4a6741 node shadow, mint
highlight #a8c69b ridge highlight.
Composition: 128x128 px isolated, transparent.

=== icon_mineral_ore ===
hand-painted painterly icon, asian wuxia.
Subject: chunk of raw mineral ore with glowing crystal cluster jutting,
gray rock matrix with bright blue crystal facets.
Palette: slate gray #7a7c80 rock, mineral blue #4d6b8c crystal, spirit
qi blue #a8d8ff highlight glow, ink black #1a1a1a outline.
Composition: 128x128 px isolated, transparent.

=== icon_death_pollen ===
hand-painted painterly icon, asian wuxia, supernatural.
Subject: small pile of cursed dark purple pollen powder with smoke wisps
rising, faint glow particles drifting upward.
Palette: death qi purple #9b6b8b powder, ink black #1a1a1a deep shadow,
primary gold #d4a64a faint glow particle.
Composition: 128x128 px isolated, transparent.
```

### 6.2 Foods & Drinks

```
=== icon_meat ===
hand-painted painterly icon, asian wuxia.
Subject: raw red meat slab with visible muscle fibers and small bone
end, fresh and uncooked.
Palette: cinnabar red #8b3a3a meat base, bone cream #e8d5a6 fat marbling,
ink black #1a1a1a outline.
Composition: 128x128 px isolated, transparent.

=== icon_grilled ===
hand-painted painterly icon, asian wuxia.
Subject: grilled cooked meat slab on wooden skewer, golden-brown
crispy surface with visible char marks, small steam wisps.
Palette: dirt shadow #8b7355 cooked surface, primary gold #d4a64a
highlight, ink black #1a1a1a char, bone cream #e8d5a6 steam.
Composition: 128x128 px isolated, transparent.

=== icon_water ===
hand-painted painterly icon, asian wuxia.
Subject: small bamboo water flask container with cork stopper, water
visible through gap at top, bamboo segments natural form.
Palette: cactus green #6b8559 bamboo, sky qi mid #6fb5e0 water inside,
dry leaf #8a6f47 cork.
Composition: 128x128 px isolated, transparent.

=== icon_fish ===
hand-painted painterly icon, asian wuxia.
Subject: single fresh fish lying horizontal, scales visible with iridescent
sheen, single eye, fanned tail, small dorsal fin.
Palette: sky qi mid #6fb5e0 fish body, bone cream #e8d5a6 belly, mineral
blue #4d6b8c iridescent sheen, ink black #1a1a1a eye and outline.
Composition: 128x128 px isolated, transparent.

=== icon_linh_mushroom ===
hand-painted painterly icon, asian wuxia.
Subject: single magical red mushroom with bone cream dots, faint qi
glow under cap.
Palette: mushroom red #a14040, bone cream #e8d5a6, spirit qi blue
#a8d8ff faint glow.
Composition: 128x128 px isolated, transparent.

=== icon_berry ===
hand-painted painterly icon, asian wuxia.
Subject: small cluster of 3 bright red spirit berries on green stem
with glossy leaf, juicy droplet on one berry.
Palette: cinnabar red #8b3a3a berry, sage green #6b8e62 leaf, mint
#a8c69b leaf highlight.
Composition: 128x128 px isolated, transparent.

=== icon_cactus_water ===
hand-painted painterly icon, asian wuxia.
Subject: glass-like vial filled with pale green cactus liquid, small
cork, faint sparkle inside.
Palette: cactus green #6b8559 liquid, mint highlight #a8c69b liquid
glow, dry leaf #8a6f47 cork, bone cream #e8d5a6 vial reflection.
Composition: 128x128 px isolated, transparent.

=== icon_spirit_meat ===
hand-painted painterly icon, asian wuxia, supernatural.
Subject: cooked spirit meat with faint golden qi mist rising, dark red
flesh with golden glow on surface.
Palette: cinnabar red #8b3a3a meat, primary gold #d4a64a qi glow, spirit
qi blue #a8d8ff faint mist, ink black #1a1a1a outline.
Composition: 128x128 px isolated, transparent.
```

### 6.3 Tools

```
=== icon_torch ===
hand-painted painterly icon, asian wuxia.
Subject: lit handheld torch — wooden handle wrapped in cloth at top,
bright flickering flame at top with sparks, smoke wisps fading.
Palette: bark brown #b89968 handle, dry leaf #8a6f47 cloth wrap,
cinnabar red #8b3a3a flame core, primary gold #d4a64a flame mid, bone
cream #e8d5a6 spark.
Composition: 128x128 px isolated diagonal, transparent.

=== icon_rod ===
hand-painted painterly icon, asian wuxia.
Subject: simple bamboo fishing rod, slender bamboo pole with small
string and hook at tip, neutral diagonal.
Palette: cactus green #6b8559 bamboo, dry leaf #8a6f47 grip wrap, slate
gray #7a7c80 hook, ink black #1a1a1a string fine line.
Composition: 128x128 px isolated diagonal, transparent.
```

### 6.4 (Optional weapons / armor — placeholder for future PR)

```
=== icon_jade_pendant === (cultivation accessory)
hand-painted painterly icon, asian wuxia.
Subject: carved jade green pendant amulet with mystical rune symbol,
fine silk red cord tassel hanging.
Palette: jade green #6b8e62 jade body, mint highlight #a8c69b polish
shine, primary gold #d4a64a rune, cinnabar red #8b3a3a tassel cord.
Composition: 128x128 px isolated, transparent.

=== icon_qi_pill === (cultivation consumable)
hand-painted painterly icon, asian wuxia.
Subject: small spherical cultivation pill, bone cream surface with
gold rune line embossed, faint qi glow halo.
Palette: bone cream #e8d5a6 base, primary gold #d4a64a rune, spirit qi
blue #a8d8ff glow halo.
Composition: 128x128 px isolated, transparent.
```

---

## 7. Ground Tiles

> **Pipeline:** Drop tile PNG vào `Assets/_Project/Art/Tiles/{biome}/tile_{biome}_{name}_{NN}.png`. BiomeTileImporter auto-wire → BiomeSO.groundTileVariants[].
>
> **Critical:** tile MUST seamless tile (left=right, top=bottom edges blend). Photopea seam-fix có thể cần sau gen — xem [`prompts/gpt_workflow.md`](../prompts/gpt_workflow.md).
>
> **Resolution:** 1024×1024 gen → downscale 64×64 PPU 64.
>
> **View angle:** **straight top-down 90°** (NOT 30° — tile khác character/resource). NO perspective, NO foreshortening.
>
> **Mirror:** [`prompts/tileset_gpt.txt`](../prompts/tileset_gpt.txt) là source canonical. Prompts dưới đây inline cho convenience copy-paste — nếu chỉnh sửa, sync 2 file.

### 7.1 Forest — Rừng Linh Mộc (Perlin 0.0–0.4)

```
=== tile_forest_grass_01 — Basic Grass (Variant A) ===

Generate a seamless tileable ground texture for a 2D top-down survival game.
Style: hand-painted painterly digital illustration with visible brush strokes,
soft cel-shading, asian wuxia cultivation fantasy aesthetic, NOT pixel art,
NOT photo-realistic, NOT anime style.

Subject: lush forest grass with small white and yellow wildflowers,
scattered fallen brown leaves, vibrant living green ground.

Palette (use these hex codes ONLY): deep moss green #4a6741 base shadow,
sage green #6b8e62 mid-tone, mint green highlight #a8c69b for grass tips,
dry leaf brown #8a6f47 patches, small white #ffffff and yellow #ffd966
flower dots. No other colors.

CRITICAL TILEABILITY REQUIREMENTS:
- Image must seamlessly tile in a 2x2 grid placement
- Pattern at LEFT edge MUST match pattern at RIGHT edge (horizontal continuity)
- Pattern at TOP edge MUST match pattern at BOTTOM edge (vertical continuity)
- NO visible borders, NO frame, NO centerpiece composition
- Even distribution of detail across entire image (no focal point)
- Avoid placing any complete object near edges (objects should either span
  edge wrapping or stay fully in interior)
- Brush strokes orientation random/natural, not converging to center

CRITICAL VIEW ANGLE:
- Straight top-down 90 degrees view (looking directly down at ground)
- NOT 30 degree perspective angle
- NO foreshortening, NO depth, NO horizon
- NO isometric projection

Composition: 1024x1024 ground texture only, no characters, no creatures,
no objects above ground level (small flowers and leaves lying flat on
ground only), no UI elements, no text, no watermark, no signature,
no border, no grid lines.

Lighting: even soft ambient light, no harsh directional shadow,
no drop shadow on transparent ground.
```

```
=== tile_forest_grass_02 — Grass with Fallen Leaves ===

Generate a seamless tileable ground texture for a 2D top-down survival game.
Style: hand-painted painterly with visible brush strokes, soft cel-shading,
asian wuxia cultivation aesthetic, NOT pixel art, NOT photo-realistic,
NOT anime.

Subject: forest floor mostly grass with scattered fallen autumn leaves
in browns and ambers, small twigs, hint of moss patches, weathered
woodland feel.

Palette (use these ONLY): deep moss #4a6741 base, sage green #6b8e62,
mint #a8c69b sparse, dry leaf brown #8a6f47 dominant for leaves,
bark brown #b89968 for twigs, lantern gold #d4a64a sparse for autumn
yellow accent. No other colors.

CRITICAL TILEABILITY: image must seamlessly tile 2x2 grid, LEFT edge
matches RIGHT, TOP matches BOTTOM, no border, no centerpiece, even
detail distribution.

VIEW: straight top-down 90 degrees, NO perspective, NO 30 degree angle,
NO isometric.

Composition: 1024x1024 ground only, no characters, no objects above
ground, no text, no watermark.
```

```
=== tile_forest_grass_03 — Moss and Dirt Patches ===

Generate a seamless tileable ground texture for a 2D top-down survival game.
Style: hand-painted painterly with visible brush strokes, soft cel-shading,
asian cultivation aesthetic, NOT pixel, NOT photo, NOT anime.

Subject: forest ground transition between moss patches and exposed dirt,
patchy uneven texture with mixed grass tufts, occasional tiny mushroom,
mossy and earthy mood.

Palette (ONLY these): deep moss #4a6741 dominant base, sage green
#6b8e62 grass mid, mint #a8c69b moss tip, dry leaf #8a6f47 dirt patch,
bark brown #b89968 deep dirt, mushroom red #a14040 occasional small dot
accent.

CRITICAL TILEABILITY: 2x2 seamless grid required, LEFT-RIGHT continuity,
TOP-BOTTOM continuity, no border, no centerpiece, even distribution.

VIEW: straight top-down 90 degrees ONLY, no perspective, no 30 degree.

Composition: 1024x1024, ground only, no characters, no objects.
```

```
=== tile_forest_grass_04 — Dense Grass with Spirit Glow ===

Generate a seamless tileable ground texture for a 2D top-down survival game.
Style: hand-painted painterly with visible brush strokes, soft cel-shading,
asian cultivation aesthetic, NOT pixel, NOT photo, NOT anime.

Subject: dense thick forest grass with subtle qi spirit glow, small
bioluminescent fungus dots emitting soft blue light (very subtle low
saturation glow), mystical living forest floor at dusk feel.

Palette (ONLY these): deep moss #4a6741, sage green #6b8e62 mid, mint
#a8c69b highlight, spirit qi blue #a8d8ff subtle fungus glow accents
(low saturation, NOT bright bloom), dry leaf #8a6f47 minimal, bone cream
#e8d5a6 for fungus stem.

CRITICAL TILEABILITY: 2x2 seamless required, LEFT-RIGHT and TOP-BOTTOM
continuity, no border, no centerpiece, even distribution of fungus dots
spread across image (NOT clustered).

VIEW: straight top-down 90 degrees, no perspective.

Composition: 1024x1024, ground + small fungus only (max 30 pixels each),
no characters, no large objects, no text.

Lighting: dim mystical ambient, subtle glow halo on fungus only, no harsh
bloom, no drop shadow.
```

### 7.2 Stone Highlands — Đá Sơn Cao Nguyên (Perlin 0.4–0.65)

```
=== tile_highlands_stone_01 — Clean Slate Stone ===

Generate a seamless tileable ground texture for a 2D top-down survival game.
Style: hand-painted painterly with visible brush strokes, soft cel-shading,
asian wuxia mountain plateau aesthetic, NOT pixel, NOT photo, NOT anime.

Subject: weathered slate gray stone ground, smooth large stone slab
surface with hairline cracks, very minimal moss, austere mountain plateau,
NO warm tones.

Palette (ONLY these COOL tones): slate gray #7a7c80 base, highlight stone
#a3a5a8 raised ridges, shadow stone #5a5d63 deep cracks, dry moss #8a9b8c
minimal accent, bone white #c2c4ba sparse highlight specks.

CRITICAL TILEABILITY: 2x2 seamless grid, LEFT-RIGHT and TOP-BOTTOM match,
no border, no centerpiece, even crack distribution.

VIEW: straight top-down 90 degrees ONLY.

Composition: 1024x1024 ground only, no characters, no objects.

Lighting: cool ambient, even, no warm color, no drop shadow.
```

```
=== tile_highlands_stone_02 — Cracked Stone with Moss ===

Generate a seamless tileable ground texture for a 2D top-down survival game.
Style: hand-painted painterly, soft cel-shading, asian mountain shrine
aesthetic, NOT pixel, NOT photo.

Subject: weathered cracked stone with significant dry moss patches filling
the cracks, broken stone joint feel, ancient pavement of abandoned shrine.

Palette (ONLY these): slate gray #7a7c80 base, highlight #a3a5a8, shadow
#5a5d63 deep cracks, dry moss #8a9b8c dominant moss filler, mint #a8c69b
moss highlight, bone white #c2c4ba stone highlight.

CRITICAL TILEABILITY: 2x2 seamless, LEFT-RIGHT TOP-BOTTOM match, no border.

VIEW: straight top-down 90 degrees.

Composition: 1024x1024 ground only, no characters, no objects, no text.
```

```
=== tile_highlands_stone_03 — Stone with Bone Fragments ===

Generate a seamless tileable ground texture for a 2D top-down survival game.
Style: hand-painted painterly, soft cel-shading, asian wuxia abandoned
battleground aesthetic, NOT pixel, NOT photo.

Subject: gray stone ground with scattered SMALL bleached bone fragments
and shards lying flat (NOT full skull, NOT skeleton, max ~80 pixels each),
few yellow grass tufts pushing through cracks, eerie feel.

Palette (ONLY these): slate gray #7a7c80, highlight #a3a5a8, shadow #5a5d63
cracks, bone white #c2c4ba dominant for bone shards, dry moss #8a9b8c for
small grass tufts, dirt deep brown #6b5d40 minimal crack dust.

CRITICAL TILEABILITY: 2x2 seamless, LEFT-RIGHT TOP-BOTTOM match, no border,
bone shards distributed evenly NOT clustered.

VIEW: straight top-down 90 degrees, bones lying FLAT on ground (not
standing).

Composition: 1024x1024 ground only, NO full skull, NO skeleton, NO
characters, NO creatures.
```

```
=== tile_highlands_stone_04 — Stone with Mineral Vein ===

Generate a seamless tileable ground texture for a 2D top-down survival game.
Style: hand-painted painterly, soft cel-shading, asian fantasy ore deposit
aesthetic, NOT pixel, NOT photo.

Subject: gray stone ground with thin glowing mineral blue vein running
diagonally across image, occasional small mineral crystal cluster lying
flat in cracks (max ~60 pixels each).

Palette (ONLY these): slate gray #7a7c80, highlight #a3a5a8, shadow #5a5d63,
mineral blue #4d6b8c dominant for vein and crystals, spirit qi blue
#a8d8ff for crystal highlight (subtle glow NOT bloom), bone white #c2c4ba
sparse stone highlight, dry moss #8a9b8c minimal.

CRITICAL TILEABILITY: 2x2 seamless, vein continuous when tiled (entry
edge matches exit edge), no border, no centerpiece.

VIEW: straight top-down 90 degrees, crystals lying FLAT.

Composition: 1024x1024 ground only, no characters, no large objects.

Lighting: cool ambient, subtle vein glow only, no harsh bloom.
```

### 7.3 Cursed Desert — Hoang Mạc Tử Khí (Perlin 0.65–1.0)

```
=== tile_desert_sand_01 — Smooth Sand ===

Generate a seamless tileable ground texture for a 2D top-down survival game.
Style: hand-painted painterly with visible brush strokes, soft cel-shading,
asian desert aesthetic, NOT pixel, NOT photo, NOT anime.

Subject: smooth golden desert sand with subtle wind ripple lines, clean
dune surface, hot sun-baked feel, NO purple, NO green.

Palette (ONLY these WARM tones): sand base #c4a574 dominant, sand highlight
#dec594 ripple tops, dirt shadow #8b7355 ripple bottoms, dirt deep #6b5d40
minimal for deep crack. NO other colors.

CRITICAL TILEABILITY: 2x2 seamless, LEFT-RIGHT TOP-BOTTOM match, no border,
no centerpiece, ripple lines should flow continuously when tiled.

VIEW: straight top-down 90 degrees ONLY, NO 30 degree angle, NO perspective.

Composition: 1024x1024 ground only, NO characters, NO objects, NO text.

Lighting: warm even sunlight ambient, no drop shadow.
```

```
=== tile_desert_sand_02 — Sand with Heavy Ripples ===

Generate a seamless tileable ground texture for a 2D top-down survival game.
Style: hand-painted painterly, soft cel-shading, asian desert post-storm
aesthetic, NOT pixel, NOT photo.

Subject: desert sand with HEAVY wind-carved ripple pattern (deep parallel
grooves running across image), strong texture variation, dynamic dune
surface just after sandstorm, NO purple, NO green.

Palette (ONLY these): sand base #c4a574, sand highlight #dec594 ridge tops,
dirt shadow #8b7355 ripple troughs, dirt deep #6b5d40 deep grooves.

CRITICAL TILEABILITY: 2x2 seamless, ripple lines continuous when tiled
(parallel groove must align edge-to-edge), no border.

VIEW: straight top-down 90 degrees.

Composition: 1024x1024 ground only, no characters, no objects.

Lighting: warm sunlight, high contrast between ridge highlight and trough
shadow.
```

```
=== tile_desert_sand_03 — Sand with Bone Fragments ===

Generate a seamless tileable ground texture for a 2D top-down survival game.
Style: hand-painted painterly, soft cel-shading, asian desert graveyard
aesthetic, NOT pixel, NOT photo.

Subject: golden desert sand with scattered SMALL sun-bleached bone
fragments and shards lying flat (NOT full skull, NOT skeleton, max ~80
pixels each), occasional faint old animal track imprint, hint of dried
out dead grass tuft.

Palette (ONLY these): sand #c4a574, sand highlight #dec594, dirt shadow
#8b7355, dirt deep #6b5d40, bone bleached #d4d4d4 dominant for bone
shards, cactus green #6b8559 minimal dried grass accent.

CRITICAL TILEABILITY: 2x2 seamless, bones distributed evenly NOT clustered,
LEFT-RIGHT TOP-BOTTOM match, no border.

VIEW: straight top-down 90 degrees, bones lying FLAT.

Composition: 1024x1024 ground only, NO full skull, NO skeleton, NO
characters, NO creatures, NO purple.
```

```
=== tile_desert_sand_04 — Deep Desert with Death Qi Cracks ===

Generate a seamless tileable ground texture for a 2D top-down survival game.
Style: hand-painted painterly, soft cel-shading, asian wuxia tử khí (death
qi) endgame zone aesthetic, NOT pixel, NOT photo.

Subject: cracked dry desert sand with darker dried earth showing through
cracks, faint purple death qi haze emanating from cracks (subtle glow
NOT harsh bloom), deep desert tomb area feel for endgame zone.

Palette (ONLY these): sand #c4a574, sand highlight #dec594 reduced (less
sun-lit), dirt shadow #8b7355, dirt deep #6b5d40 dominant for crack network,
death qi purple #9b6b8b for crack haze (low saturation), bone bleached
#d4d4d4 minimal, ink black #1a1a1a deep crack core.

CRITICAL TILEABILITY: 2x2 seamless, crack network continuous when tiled
(crack lines should connect edge-to-edge forming web pattern), no border.

VIEW: straight top-down 90 degrees, no perspective, no 30 degree.

Composition: 1024x1024 ground only, no characters, no large objects.

Lighting: dim purple-tinted ambient (deep desert mood), subtle crack glow,
no harsh bloom, no drop shadow.
```

---

## 8. VFX

> **Pipeline:** drop PNG vào `Assets/_Project/Art/Vfx/{vfx_id}.png`. Hiện ReactiveOnHit / DropShadow / WaterRipple / ProgressiveCrackOverlay / LeafParticle dùng procedural sprites (vd circle alpha cho hit-flash, ellipse cho shadow). User có thể override với hand-painted PNG.
>
> **View angle:** overhead 90° (particle thường spawn at world position, no perspective).
>
> **Resolution:** 128×128 (small effect) hoặc 256×256 (large explosion / boss death).
>
> **Style:** painterly nhưng với strong outline + bright accent — VFX cần đọc nhanh trên BG phức tạp. Alpha blending: subject opaque ở core + transparent fade-edge. Particle frame có thể gen 1 PNG → Unity Animator scale/alpha tween cho loop. Hoặc 4-frame loop strip nếu cần shape change.

### 8.1 Hit Flash — White Burst

```
=== vfx_hit_flash ===

hand-painted painterly digital illustration, visible brush strokes,
soft cel-shading, asian wuxia martial arts impact aesthetic,
NOT pixel art, NOT photo-realistic, NOT anime moe.

Subject: white burst impact flash, irregular star-burst shape with
6-8 jagged spikes radiating outward, bright bone-cream center fading
to transparent edge, faint gold outer rim, painterly brush stroke
visible at edge giving "splatter" feel, single isolated burst.

Palette: bone cream #e8d5a6 center bright core, primary gold #d4a64a
mid-radius rim glow, ink black #1a1a1a thin spike outline (mid-tone
NOT pure black).

Composition: 128x128 px, isolated single burst on fully transparent
background, centered, alpha fades to 0 at edge, NO ground, NO subject
beneath, NO border.

# NOTE: dùng cho ReactiveOnHit child overlay. Alpha blend ADDITIVE.
```

### 8.2 Damage Popup — Red Number Background

```
=== vfx_damage_popup_bg ===

hand-painted painterly, asian wuxia, soft cel-shading.

Subject: small irregular blood-spatter shape acting as background plate
for damage number, painterly red splatter with jagged drips around edge,
faint dark inner shadow, transparent edges, single isolated splatter.

Palette: cinnabar red #8b3a3a base, dirt deep #6b5d40 inner deep shadow,
ink black #1a1a1a outline mid-tone, primary gold #d4a64a thin rim accent.

Composition: 128x96 px (horizontal — fits damage number text), isolated
splatter on transparent, centered, fully opaque core fading to transparent
at edges.

# NOTE: text damage number rendered on top via UI Text component.
# Sprite chỉ là decoration plate.
```

### 8.3 Blood Splash — Mob Death Decoration

```
=== vfx_blood_splash ===

hand-painted painterly, asian wuxia, NOT pixel, NOT photo.

Subject: mob death blood splash decoration, irregular liquid spatter
shape with main central pool + radiating droplets in 4-6 directions,
painterly brush stroke giving "wet" feel, faint dark outline around pool,
single isolated splash.

Palette: cinnabar red #8b3a3a dominant blood color, dirt deep #6b5d40
deep shadow, ink black #1a1a1a fine outline.

Composition: 192x128 px (horizontal — splash wider), isolated on
transparent background, NO ground, alpha fades at outermost droplets.

# NOTE: optional spawn under mob corpse. Dùng theo MobBase death event.
```

### 8.4 Dust Poof — Footstep / Landing

```
=== vfx_dust_poof ===

hand-painted painterly, asian wuxia, soft cel-shading.

Subject: small dust cloud poof, irregular round soft cloud shape with
bone-cream highlight on top + dirt-shadow underside, faint trailing
wisps, painterly puffy edge, single isolated cloud.

Palette: bone cream #e8d5a6 highlight top, dirt shadow #8b7355 underside
mid-tone, dirt deep #6b5d40 deep shadow, transparent edge.

Composition: 128x96 px (horizontal — cloud wider than tall), isolated
on transparent, alpha fades to 0 at outer edge.

# NOTE: spawn at foot when player/mob walks. 4-frame scale tween.
```

### 8.5 Fire Spark — Flame Particle

```
=== vfx_fire_spark ===

hand-painted painterly, asian wuxia, fire VFX.

Subject: small flame spark particle, teardrop shape with broad bottom
narrow tip pointing up, bright cinnabar red core + primary gold outer
glow + bone cream inner highlight at tip, painterly brush flame edge,
single isolated spark.

Palette: cinnabar red #8b3a3a flame core base, primary gold #d4a64a
outer glow, bone cream #e8d5a6 inner tip highlight, dirt deep #6b5d40
darkest base shadow.

Composition: 64x96 px (vertical — flame tall), isolated on fully
transparent background, NO ground.

# NOTE: spawn at campfire / torch / boss attack. Multiple spawn for
# fuller flame look.
```

### 8.6 Smoke Wisp — Ambient

```
=== vfx_smoke_wisp ===

hand-painted painterly, asian wuxia, atmospheric.

Subject: rising smoke wisp particle, irregular vertical curl shape
with broad bottom narrow top dissipating, soft painterly brush stroke,
slate-gray to transparent gradient, single isolated wisp.

Palette: slate gray #7a7c80 base, shadow stone #5a5d63 deepest interior,
highlight stone #a3a5a8 sun-lit edge, transparent fade at top.

Composition: 96x192 px (very tall vertical — smoke rises), isolated on
transparent background, alpha fades fully at top edge.

# NOTE: campfire ambient + boss death decay + burning torch trail.
```

### 8.7 Mana / Qi Glow — Cultivation Channel

```
=== vfx_qi_glow ===

hand-painted painterly, asian wuxia supernatural, soft cel-shading.

Subject: spirit qi mana glow halo, soft round halo with bright spirit qi
blue center + sky qi mid outer ring + primary gold inner ember sparkles,
faint particle dots floating inward (qi gathering motion), single isolated
halo.

Palette: spirit qi blue #a8d8ff bright core dominant, sky qi mid #6fb5e0
mid-ring, primary gold #d4a64a inner spark accent, transparent halo edge.

Composition: 256x256 px, isolated halo on fully transparent background,
NO subject in center (player rendered on top via prefab).

# NOTE: cultivation channeling overlay + meditation marker. Animator
# scale pulse 0.9-1.1 loop.
```

### 8.8 Level-Up Halo — Realm Advance

```
=== vfx_levelup_halo ===

hand-painted painterly, asian wuxia supernatural, breakthrough moment.

Subject: realm advancement halo, large radial sun-burst with primary gold
center + spirit qi blue outer rays + bone cream sparkle dots, dynamic
streaks radiating from center outward (8-12 rays), painterly brush stroke,
single isolated burst.

Palette: primary gold #d4a64a dominant core, spirit qi blue #a8d8ff outer
ray glow, bone cream #e8d5a6 sparkle highlight, sky qi mid #6fb5e0 mid-ring.

Composition: 384x384 px, isolated burst on fully transparent background,
NO subject in center.

# NOTE: spawn khi RealmSystem.OnRealmAdvanced fire. Animator scale tween
# 0.5 → 1.5 + alpha 1 → 0 over 1 second.
```

### 8.9 Death Decay — Mob Vanish

```
=== vfx_death_decay ===

hand-painted painterly, asian wuxia.

Subject: black smoke mist dissipation effect, irregular dark cloud
fading upward, painterly wispy edges, single isolated cloud.

Palette: ink black #1a1a1a base, dirt deep #6b5d40 mid, slate gray
#7a7c80 lighter top, transparent fade.

Composition: 192x256 px (vertical — mist rises), isolated on transparent.

# NOTE: spawn khi mob die before disappear. Animator scale 1 → 1.5 +
# alpha 1 → 0 over 0.8s.
```

### 8.10 Status Effect Icons — Buff/Debuff

```
=== icon_status_poison ===
hand-painted painterly icon, asian wuxia.
Subject: green skull-shape mini icon, drooping toxic drip at chin,
sickly glow halo around skull, single small icon.
Palette: cactus green #6b8559 skull, deep moss #4a6741 shadow, mint
#a8c69b drip highlight, ink black #1a1a1a outline.
Composition: 64x64 px isolated on transparent, centered.

=== icon_status_bleed ===
hand-painted painterly icon, asian wuxia.
Subject: red blood-drop with motion lines mini icon, dripping shape
with falling droplets below, small icon.
Palette: cinnabar red #8b3a3a, dirt deep #6b5d40 shadow, bone cream
#e8d5a6 highlight, ink black #1a1a1a outline.
Composition: 64x64 px isolated on transparent.

=== icon_status_burning ===
hand-painted painterly icon, asian wuxia.
Subject: small flame teardrop mini icon with motion lines, single
flame shape.
Palette: cinnabar red #8b3a3a core, primary gold #d4a64a glow, bone
cream #e8d5a6 tip, ink black #1a1a1a outline.
Composition: 64x64 px isolated on transparent.

=== icon_status_chilled ===
hand-painted painterly icon, asian wuxia.
Subject: pale blue snowflake / frost crystal mini icon with 6 spikes
radiating, single icon.
Palette: spirit qi blue #a8d8ff base, sky qi mid #6fb5e0 mid, bone
cream #e8d5a6 highlight, ink black #1a1a1a outline.
Composition: 64x64 px isolated on transparent.

=== icon_status_blessed ===
hand-painted painterly icon, asian wuxia, supernatural.
Subject: golden halo with cross/star symbol inside, holy blessing
mini icon.
Palette: primary gold #d4a64a base, bone cream #e8d5a6 highlight,
spirit qi blue #a8d8ff faint inner glow, ink black #1a1a1a outline.
Composition: 64x64 px isolated on transparent.
```

---

## 9. Weather

> **Pipeline:** drop PNG vào `Assets/_Project/Art/Weather/{id}.png`. WeatherSystem (in TimeManager) spawns particle prefabs based on weather state. Each weather effect = 1 sprite + ParticleSystem config.
>
> **View angle:** overhead 90° for ground (puddle), or full-screen overlay for fog/rain.
>
> **Resolution:** 128×128 (single particle), 1024×1024 (overlay).

### 9.1 Rain Particle

```
=== weather_rain_drop ===

hand-painted painterly, atmospheric, asian wuxia.

Subject: single rain droplet streak, vertical thin elongated teardrop
shape (long top to short bottom motion blur), spirit qi blue tone with
bone cream highlight on leading edge, transparent at trail end, single
isolated drop.

Palette: spirit qi blue #a8d8ff core, sky qi mid #6fb5e0 mid, bone
cream #e8d5a6 leading edge highlight, transparent fade at trail.

Composition: 32x96 px (very tall vertical — rain streak), isolated on
fully transparent background.

# NOTE: ParticleSystem spawn many drops. 1 PNG → Unity texture rotates +
# scales for variation.
```

### 9.2 Snow Particle

```
=== weather_snow_flake ===

hand-painted painterly, atmospheric, asian wuxia.

Subject: small snowflake / icy fluff particle, irregular fluffy hexagon
shape, painterly soft edge, single isolated flake.

Palette: bone cream #e8d5a6 base, bone bleached #d4d4d4 mid, spirit
qi blue #a8d8ff faint inner glow, transparent edge.

Composition: 64x64 px, isolated on fully transparent background.
```

### 9.3 Fog Overlay — Atmospheric

```
=== weather_fog_overlay ===

hand-painted painterly, atmospheric mist, asian wuxia mountain mist.

Subject: dense fog overlay texture, irregular cloud-like wispy patches
with soft painterly brush, semi-transparent throughout (50-70% opacity),
NO hard edges, NO subject, ground invisible behind fog.

Palette: bone bleached #d4d4d4 core, slate gray #7a7c80 deeper area,
bone cream #e8d5a6 lighter sun-lit edge, transparent throughout (NEVER
fully opaque).

CRITICAL TILEABILITY: 2x2 seamless grid, LEFT-RIGHT TOP-BOTTOM match,
fog patches distributed evenly NOT clustered, no centerpiece.

Composition: 1024x1024 fog texture only, fully tileable, alpha 50-70%
throughout, NO border, NO subject.

# NOTE: BiomeSO.fogColor + WeatherSystem fog overlay. Tween alpha
# in/out for fade.
```

### 9.4 Lightning Bolt — Storm Strike

```
=== weather_lightning_bolt ===

hand-painted painterly, atmospheric storm, asian wuxia.

Subject: jagged vertical lightning bolt strike, sharp zig-zag electric
arc, bright spirit qi blue core with bone cream brightest highlight,
faint primary gold outer glow halo, painterly brush edge, single
isolated bolt from top to bottom of frame.

Palette: bone cream #e8d5a6 brightest core (almost white), spirit qi
blue #a8d8ff core glow, sky qi mid #6fb5e0 outer ring, primary gold
#d4a64a faint warm halo (subtle), ink black #1a1a1a around strike
edge for contrast.

Composition: 192x768 px (very tall vertical — full screen height
lightning), isolated bolt on fully transparent background.

# NOTE: spawn during stormy weather state. Brief 0.2s flash + screen
# tint white pulse via post-processing.
```

### 9.5 Sun Ray — Clear Weather Beam

```
=== weather_sun_ray ===

hand-painted painterly, atmospheric, asian wuxia.

Subject: god-ray sunbeam shaft, soft diagonal ray of light from upper
corner with primary gold tone, transparent fade at edges, painterly
brush soft glow, single isolated ray.

Palette: primary gold #d4a64a core, bone cream #e8d5a6 brightest
highlight, transparent fade at outer edges.

Composition: 384x768 px (diagonal ray), isolated on fully transparent
background.

# NOTE: parallax overlay layer at sky. Multiply blend mode.
```

### 9.6 Sandstorm Overlay — Desert Storm

```
=== weather_sandstorm_overlay ===

hand-painted painterly, atmospheric, desert.

Subject: dense sandstorm overlay texture, fast horizontal sand streaks
with motion blur, semi-transparent throughout, painterly brush sweeping
horizontal direction, NO hard edges, NO subject, ground partially
invisible behind storm.

Palette: sand base #c4a574 core, sand highlight #dec594 lighter, dirt
shadow #8b7355 darker streaks, transparent throughout.

CRITICAL TILEABILITY: 2x2 seamless, LEFT-RIGHT continuous (sand streaks
flow horizontally), no centerpiece.

Composition: 1024x1024 sandstorm texture, fully tileable, alpha 40-65%
throughout.

# NOTE: triggered in Cursed Desert biome at random weather state.
```

---

## 10. NPC Humanoid

> **Pipeline:** R5 NPC pattern. NPC humanoid (Vendor, Companion, Quest Giver) inherit `CharacterBase` + use puppet hierarchy SAME as Player. Drop PNG vào `Assets/_Project/Art/Characters/{npc_id}/{filename}.png` flat hoặc `{E|N|S}/{filename}.png` multi-dir. BootstrapWizard auto-build puppet.
>
> **Naming:** `vendor`, `companion`, `quest_giver_elder`, `villager_male`, `villager_female`, `bandit`, `disciple_male`, `disciple_female`.
>
> **Style:** SAME painterly painterly anchor as Player. Each NPC has distinct palette + costume detail giúp visual identity.

### 10.1 VendorNPC — Lão Tiên Sinh (Elder Merchant Cultivator)

> **Concept:** old male cultivator merchant, gray beard, crimson robe with gold trim, kind wise face, wears straw conical hat, carries bamboo cane, jade pendant.

```
=== vendor/head.png ===

hand-painted painterly digital illustration, visible brush strokes,
soft cel-shading, asian wuxia cultivation fantasy aesthetic,
limited color palette, 1.5 to 2 pixel mid-tone outline (NOT pure black),
NOT pixel art, NOT photo-realistic, NOT anime moe.

Subject: side view of an OLD Asian male cultivator merchant HEAD ONLY,
profile facing right, kind wise weathered face with deep wrinkles,
bushy white-gray beard reaching mid-chest length (extends below jaw cut),
ink-black hair tied in topknot under wide-brimmed straw conical hat
(bamboo woven texture), gentle warm expression, faint qi glow on
forehead, NO shoulders below jaw cut.

Palette: bone cream #e8d5a6 skin base, warm shadow #b89968 wrinkle
shadow, bone bleached #d4d4d4 white beard and hair, dry leaf #8a6f47
straw hat woven texture, primary gold #d4a64a hat trim, jade green
#6b8e62 faint qi glow, ink black #1a1a1a hair.

Composition: 320x320 px (head + extending beard), isolated on fully
transparent background, NO body parts below jaw, NO ground.
```

```
=== vendor/torso.png ===

hand-painted painterly, visible brush strokes, asian wuxia.

Subject: side view of an old male cultivator merchant TORSO ONLY,
profile facing right, neutral standing pose, wearing flowing crimson
red robe with elaborate gold dragon embroidery on chest and waist sash,
bone cream sash tied at waist, large jade pendant on chest, robe falls
to mid-thigh, NO head NO arms NO legs visible (clean cuts at shoulders,
hips, neck).

Palette: cinnabar red #8b3a3a robe base, dirt deep #6b5d40 fold shadow,
primary gold #d4a64a embroidery accent (dragon motif), bone cream
#e8d5a6 sash, jade green #6b8e62 pendant.

Composition: 256x384 px (vertical), isolated single torso on fully
transparent background, NO head, NO limbs, NO ground.
```

```
=== vendor/arm_left.png === / === vendor/arm_right.png ===

hand-painted painterly, asian wuxia.

Subject: side view of old cultivator merchant's LEFT (or RIGHT) arm,
hanging straight down in neutral pose, wearing crimson red robe sleeve
with gold cuff trim, hand visible at bottom (slightly weathered hand,
fingers slightly curled relaxed, optional hint of holding cane handle
on right arm version), NO body, NO head.

Palette: cinnabar red #8b3a3a sleeve base, dirt deep #6b5d40 fold
shadow, jade pale #d4d4ba skin tone for hand, primary gold #d4a64a
cuff trim, bark brown #b89968 cane handle hint (right arm only).

Composition: 256x384 px (vertical), isolated single arm on transparent,
top edge clean horizontal at shoulder pivot, bottom at fingertips,
NO body.
```

```
=== vendor/leg_left.png === / === vendor/leg_right.png ===

hand-painted painterly, asian wuxia.

Subject: side view of old cultivator merchant's LEFT (or RIGHT) leg,
straight standing, wearing crimson red robe pant flowing to ankle,
fabric shoe with cloth wrap, NO body.

Palette: cinnabar red #8b3a3a pant base, dirt deep #6b5d40 fabric fold,
dry leaf #8a6f47 shoe.

Composition: 256x384 px (vertical), isolated single leg on transparent,
top at hip pivot, bottom at sole, NO body.
```

```
=== vendor/forearm_left.png === / === vendor/forearm_right.png ===

hand-painted painterly, asian wuxia.

Subject: side view of old merchant's FOREARM ONLY (lower half from
elbow to fingertips), profile facing right, crimson red sleeve with
gold cuff trim, weathered hand visible at bottom, NO upper arm.

Palette: same as §10.1 vendor/arm.

Composition: 192x288 px (vertical), isolated forearm, top at elbow,
bottom at fingertips, transparent.
```

```
=== vendor/shin_left.png === / === vendor/shin_right.png ===

hand-painted painterly, asian wuxia.

Subject: side view of old merchant's SHIN ONLY (lower half from knee
to sole), crimson red pant flowing to ankle, fabric shoe at bottom,
NO upper leg.

Palette: same as §10.1 vendor/leg.

Composition: 192x288 px (vertical), isolated shin, top at knee, bottom
at sole, transparent.
```

### 10.2 CompanionNPC — Linh Nhi (Young Female Cultivator Companion)

> **Concept:** young Asian female disciple, jade green hairband, sky-blue robe with silver trim, gentle determined face, peach-blossom embroidery on robe.

```
=== companion/head.png ===

hand-painted painterly, visible brush strokes, asian wuxia,
limited color palette, 1.5 to 2 pixel mid-tone outline (NOT pure black),
NOT pixel art, NOT photo-realistic, NOT anime moe.

Subject: side view of a young Asian FEMALE cultivator disciple HEAD
ONLY, profile facing right, gentle determined expression with calm
focused eye, ink-black hair tied in twin loops bun style with jade
green silk hairband, fair smooth skin, faint blue qi glow on temple,
NO neck below jaw.

Palette: bone cream #e8d5a6 skin base, warm shadow #b89968 mid-tone,
ink black #1a1a1a hair, jade green #6b8e62 hairband, spirit qi blue
#a8d8ff faint qi glow, cinnabar red #8b3a3a small lip accent.

Composition: 256x256 px, isolated single head on transparent background,
NO body below jaw, NO ground.
```

```
=== companion/torso.png ===

hand-painted painterly, visible brush strokes, asian wuxia.

Subject: side view of young female cultivator disciple TORSO ONLY,
profile facing right, neutral standing pose, wearing flowing sky-blue
robe with silver trim and pink peach-blossom embroidery on chest,
delicate slim silhouette, jade pendant on chest, robe falls to mid-thigh,
NO head NO arms NO legs visible (clean cuts at shoulders, hips, neck).

Palette: sky qi mid #6fb5e0 robe base, mineral blue #4d6b8c fold shadow,
bone bleached #d4d4d4 silver trim, cinnabar red #8b3a3a peach blossom
embroidery accent, primary gold #d4a64a small ribbon detail, jade
green #6b8e62 pendant.

Composition: 256x384 px (vertical), isolated torso on transparent
background, NO head, NO limbs.
```

```
=== companion/arm_left.png === / === companion/arm_right.png ===

hand-painted painterly, asian wuxia.

Subject: side view of young female cultivator's LEFT (or RIGHT) arm,
hanging straight down in neutral pose, wearing sky-blue robe sleeve
with silver cuff trim, slim hand visible at bottom (fingers slightly
curled relaxed), NO body, NO head.

Palette: sky qi mid #6fb5e0 sleeve base, mineral blue #4d6b8c fold
shadow, jade pale #d4d4ba skin tone for hand, bone bleached #d4d4d4
cuff trim.

Composition: 256x384 px (vertical), isolated single arm on transparent,
top edge at shoulder pivot, bottom at fingertips, NO body.
```

```
=== companion/leg_left.png === / === companion/leg_right.png ===

hand-painted painterly, asian wuxia.

Subject: side view of young female cultivator's LEFT (or RIGHT) leg,
straight standing, wearing sky-blue robe pant flowing to ankle,
delicate fabric shoe with embroidery, NO body.

Palette: sky qi mid #6fb5e0 pant base, mineral blue #4d6b8c fabric
fold, dry leaf #8a6f47 shoe, primary gold #d4a64a small embroidery
accent on shoe.

Composition: 256x384 px (vertical), isolated single leg on transparent,
top at hip pivot, bottom at sole, NO body.
```

```
=== companion/forearm_left.png === / === companion/forearm_right.png ===

hand-painted painterly, asian wuxia.

Subject: side view of young female cultivator's FOREARM ONLY, profile
facing right, sky-blue sleeve with silver cuff, slim hand visible at
bottom, NO upper arm.

Palette: same as §10.2 companion/arm.

Composition: 192x288 px (vertical), isolated forearm, top at elbow,
bottom at fingertips, transparent.
```

```
=== companion/shin_left.png === / === companion/shin_right.png ===

hand-painted painterly, asian wuxia.

Subject: side view of young female cultivator's SHIN ONLY (lower half
from knee to sole), sky-blue pant flowing to ankle, delicate fabric
shoe at bottom, NO upper leg.

Palette: same as §10.2 companion/leg.

Composition: 192x288 px (vertical), isolated shin, top at knee, bottom
at sole, transparent.
```

### 10.3 Quest Giver Elder — Sư Phụ (Sect Master)

> **Concept:** very old male cultivator master, long white beard reaching belt, dark indigo robe with celestial silver embroidery, stern wise expression, wields jade staff. Use SAME puppet structure as Vendor — only color/detail differs. Skip detailed prompt block here — derive from §10.1 with following palette:
>
> - Skin: bone cream #e8d5a6, warm shadow #b89968, deep wrinkle dirt deep #6b5d40
> - Beard: bone bleached #d4d4d4 (near pure white)
> - Robe base: mineral blue #4d6b8c (dark indigo)
> - Embroidery: bone bleached #d4d4d4 (silver star pattern) + spirit qi blue #a8d8ff (celestial constellation)
> - Sash: primary gold #d4a64a
> - Staff (right hand prop): bark brown #b89968 + jade green #6b8e62 jade orb top

### 10.4 Generic Villager — Dân Thường

> Generic NPC for ambient population. Use simpler palette: bone cream skin, brown commoner robe (bark brown #b89968 + dry leaf #8a6f47), no qi glow, no jade pendant. Male + Female variants. Skip detail — derive from §10.1/10.2 minus accent details.

### 10.5 Bandit — Thổ Phỉ (Hostile NPC)

> **Concept:** rough male bandit, scarred face, dark hooded leather armor, wields rusted sword. Hostile encounter NPC. Same puppet structure.
>
> - Skin: warm shadow #b89968 (tanned), dirt deep #6b5d40 scar shadow
> - Hood + armor: ink black #1a1a1a + dirt shadow #8b7355 leather
> - Belt + accents: cinnabar red #8b3a3a (sash), primary gold #d4a64a (rusted buckle)
> - Sword: slate gray #7a7c80 blade with cinnabar red rust streaks
>
> Skip detailed prompt block — derive from §10.1 with palette swap.

---

## 11. Environment Props

> **Pipeline:** drop PNG vào `Assets/_Project/Art/Resources/{prop_id}/{anyname}.png`. Same as §5 resource pipeline. Each prop is a single static decoration sprite.
>
> **View angle:** **top-down 30°** (subject thấy mặt trên + một phần mặt trước) — match §5 resources.
>
> **Resolution:** varies — small props (signpost, lantern) 256×384, medium (chest, barrel) 384×384, large (shrine, altar) 512×512.

### 11.1 Chest — Storage Container

```
=== chest_closed ===

hand-painted painterly digital illustration, visible brush strokes,
soft cel-shading, asian wuxia cultivation fantasy aesthetic,
top-down 30 degree perspective, 1.5 to 2 pixel mid-tone outline,
NOT pixel art, NOT photo-realistic, NOT anime.

Subject: rustic wooden treasure chest with iron bands and large iron
padlock, weathered planks with iron nail rivets visible, slightly
arched lid (closed), painterly bark texture, single isolated chest
on transparent.

Palette: bark brown #b89968 wood base, dry leaf #8a6f47 wood shadow,
slate gray #7a7c80 iron band, shadow stone #5a5d63 iron deep shadow,
primary gold #d4a64a small accent on lock, ink black #1a1a1a outline.

Composition: 384x320 px (slightly horizontal — chest wider than tall),
top-down 30°, isolated chest on fully transparent background, NO ground.
```

```
=== chest_open ===

hand-painted painterly, visible brush strokes, asian wuxia, top-down 30°.

Subject: same wooden treasure chest as §11.1 BUT with lid OPEN tilted
back showing inside (dark interior with faint gold glow from inner
treasure - vague glow no specific item), iron padlock dangling open,
single isolated chest.

Palette: same as chest_closed + primary gold #d4a64a inner glow,
ink black #1a1a1a deep interior shadow.

Composition: 384x384 px (slightly taller — open lid extends up),
top-down 30°, isolated on transparent.

# NOTE: swap to chest_closed when player opens. Optional gen.
```

### 11.2 Lantern — Sect Lantern Post

```
=== lantern_post ===

hand-painted painterly, visible brush strokes, asian wuxia, top-down 30°.

Subject: traditional Asian sect lantern post, vertical wooden pole with
hanging round paper lantern at top emitting warm golden glow, painted
sect symbol on lantern surface (single chinese-style brush rune), faint
qi glow halo around lantern, base anchored on small stone.

Palette: bark brown #b89968 wood pole, dry leaf #8a6f47 pole shadow,
cinnabar red #8b3a3a lantern paper, primary gold #d4a64a glow + sect
rune, slate gray #7a7c80 stone base, ink black #1a1a1a outline.

Composition: 256x512 px (very tall vertical), top-down 30°, isolated
single lantern post on transparent, NO ground.
```

### 11.3 Shrine — Mountain Altar

```
=== shrine_altar ===

hand-painted painterly, visible brush strokes, asian wuxia, top-down 30°.

Subject: small abandoned mountain shrine altar, rectangular stone slab
on stepped base with engraved cultivation rune symbols, weathered with
moss growth, single broken incense burner on top with faint smoke
wisp rising, faint qi glow under runes, single isolated shrine.

Palette (highlands): slate gray #7a7c80 stone base, shadow stone #5a5d63
deep crack, highlight stone #a3a5a8 sun-lit top, dry moss #8a9b8c moss
patch, primary gold #d4a64a faint rune glow, bone cream #e8d5a6 incense
smoke, ink black #1a1a1a engrave outline.

Composition: 512x512 px, top-down 30°, isolated shrine on transparent
background, NO surrounding ground.
```

### 11.4 Banner / Flag — Sect Banner

```
=== sect_banner ===

hand-painted painterly, visible brush strokes, asian wuxia, top-down 30°.

Subject: tall vertical sect banner flag on bamboo pole, crimson red silk
fabric with primary gold sect emblem (single Chinese-style rune symbol),
flowing slightly in wind, base anchored in small stone weight, single
isolated banner.

Palette: cinnabar red #8b3a3a fabric base, dirt deep #6b5d40 deep fold
shadow, primary gold #d4a64a sect emblem + dominant trim, cactus green
#6b8559 bamboo pole, slate gray #7a7c80 stone base, ink black #1a1a1a
outline.

Composition: 256x768 px (very tall vertical), top-down 30°, isolated
on transparent, NO ground.
```

### 11.5 Signpost — Direction Marker

```
=== signpost ===

hand-painted painterly, visible brush strokes, asian wuxia, top-down 30°.

Subject: rustic wooden signpost, single vertical post with horizontal
plank attached at top angled to point direction, painted brush-stroke
chinese-style label on plank, weathered nails, base anchored on
small stone, single isolated signpost.

Palette: bark brown #b89968 wood base, dry leaf #8a6f47 wood shadow,
ink black #1a1a1a painted label + outline, primary gold #d4a64a small
accent rune, slate gray #7a7c80 nail.

Composition: 256x384 px (vertical), top-down 30°, isolated on transparent.
```

### 11.6 Barrel — Storage Barrel

```
=== barrel_wooden ===

hand-painted painterly, visible brush strokes, asian wuxia, top-down 30°.

Subject: rustic wooden barrel, vertical staves with iron bands wrapping
top middle and bottom, lid sealed tight, weathered wood texture,
slightly worn, single isolated barrel.

Palette: bark brown #b89968 wood base, dry leaf #8a6f47 wood shadow,
slate gray #7a7c80 iron band, shadow stone #5a5d63 iron deep shadow,
ink black #1a1a1a outline.

Composition: 256x320 px (slightly tall — barrel taller than wide),
top-down 30°, isolated on transparent.
```

### 11.7 Crate — Wooden Crate

```
=== crate_wooden ===

hand-painted painterly, visible brush strokes, asian wuxia, top-down 30°.

Subject: rectangular wooden crate, slat-board sides with iron corner
brackets, single rope handle on side, weathered wood, sealed lid,
single isolated crate.

Palette: bark brown #b89968 wood base, dry leaf #8a6f47 wood shadow,
slate gray #7a7c80 iron corner, dirt shadow #8b7355 rope handle, ink
black #1a1a1a outline.

Composition: 320x256 px (horizontal — crate wider than tall), top-down
30°, isolated on transparent.
```

### 11.8 Broken Stele — Memorial Stone

```
=== broken_stele ===

hand-painted painterly, visible brush strokes, asian wuxia, top-down 30°.

Subject: ancient broken stone memorial stele, vertical rectangular slab
with engraved chinese-style rune characters, top portion broken off
diagonal showing fresh break, weathered with dry moss patches, faint
death qi residue at break, single isolated stele.

Palette (highlands/desert): slate gray #7a7c80 stone, shadow stone
#5a5d63 deep crack, highlight stone #a3a5a8 sun-lit top, dry moss
#8a9b8c moss patch, ink black #1a1a1a engrave + outline, death qi
purple #9b6b8b faint break residue accent.

Composition: 256x512 px (vertical — broken stele still tall), top-down
30°, isolated on transparent, NO ground.
```

### 11.9 Tent — Camp Shelter

```
=== tent_camp ===

hand-painted painterly, visible brush strokes, asian wuxia, top-down 30°.

Subject: simple traveler's camp tent, triangular fabric shelter on two
wooden poles with rope tie-downs to ground, dark canvas fabric with
slight wind ripple, opening flap visible at front showing dark interior,
single isolated tent.

Palette: dirt shadow #8b7355 fabric base, dirt deep #6b5d40 fabric
shadow, bark brown #b89968 wooden pole, dry leaf #8a6f47 rope, ink
black #1a1a1a interior shadow, slate gray #7a7c80 ground stake.

Composition: 384x256 px (horizontal — tent wider than tall), top-down
30°, isolated on transparent.
```

### 11.10 Ceremonial Altar — Cultivation Rite

```
=== ceremonial_altar ===

hand-painted painterly, visible brush strokes, asian wuxia supernatural,
top-down 30°.

Subject: large ceremonial cultivation altar, octagonal stone platform
with carved rune circle on top emitting strong primary gold + spirit
qi blue glow, four small incense burners at corners with rising smoke
wisps, two ornate stone pillar at back with hanging crimson banner,
mystical ritual feel, single isolated altar.

Palette (highlands): slate gray #7a7c80 stone base, highlight stone
#a3a5a8 sun-lit, shadow stone #5a5d63 deep, primary gold #d4a64a rune
glow dominant, spirit qi blue #a8d8ff inner glow accent, cinnabar red
#8b3a3a banner, bone cream #e8d5a6 incense smoke, ink black #1a1a1a
engrave + outline.

Composition: 768x512 px (large horizontal — altar wider), top-down 30°,
isolated on transparent, NO ground.
```

---

## Iteration / Quality Tips

### Multi-expert review checklist (apply trước khi commit asset)

> Trước khi consider asset "done", check qua 4 lens dưới. Mỗi lens là 1 chuyên gia rà soát góc khác nhau.

**Concept Artist (style consistency):**
- Palette anchor có chính xác? (so với §1.1 hex codes)
- Brush stroke visibility match với existing assets?
- Silhouette readable nhanh ở 64×96 px (zoom out mobile)?
- Cross-asset cohesion: nếu place cạnh tree.png + rabbit.png trong scene, có "thuộc cùng game" không?

**Tech Artist (production-ready):**
- PPU 64 đúng (1024×1024 source → ~16-32 unit world size sau import)?
- Alpha clean: outer pixels TRULY 0% alpha, không có "halo" gray fade?
- Pivot tại joint position (puppet body part) hoặc center (single sprite)?
- Multi-dir consistency: gen với SAME seed (nếu tool support) hoặc SAME prompt template để N + S + E look same character?
- Sprite Editor pivot khớp với rig joint expectation (vd arm pivot top-center = shoulder)?

**Mobile UX (legibility):**
- Character ở 64×96 px viewing distance (player nhìn camera 30° tilted) có distinguishable từ mob khác?
- Item icon ở 48×48 px (inventory slot UI) có readable từng pixel?
- Color contrast (giữa subject và background): nếu subject là cinnabar red và biome forest BG là deep moss green, có đủ contrast?
- Mobile night mode (low brightness): VFX hit flash có "đập vào mắt" không, hay chìm vào BG?

**Cultivation Lore (theme accuracy):**
- Subject có đúng đông phương wuxia visual cue không?
  - Cultivator → robe with sash, topknot or hairband, jade pendant accent, qi glow on temple/forehead
  - Demon spirit → corrupted aesthetic (purple death qi, broken horns/tusks, glowing red eye)
  - Nhân tinh (animal spirit) → ethereal mist, faint blue glow, supernatural anatomy
- Palette match biome lore? (Cursed Desert → death qi purple dominant; Forest → moss green; Highlands → cool slate gray)
- Archetype rõ ràng? (vendor = elder kind merchant; quest giver = stern sect master; bandit = scarred rough)
- Avoid: chibi proportions, generic fantasy (no Asian cue), pure western RPG aesthetic

### Khi gen ra không match style

| Symptom | Fix |
|---|---|
| Output bị anime / chibi | Add `, NOT anime, NOT chibi, realistic proportions for adult` vào subject |
| Output pixel art | Add `, NOT pixel art, smooth painterly brush strokes` |
| Outline đen tuyệt đối | Add `, outline mid-tone NOT pure black, dark version of base color outline` |
| Background không trong suốt | Add `, fully transparent background, alpha channel, isolated subject only` |
| Subject quá phức tạp | Tăng "isolated single object", remove decorative elements khỏi prompt |
| Flicker / inconsistent giữa parts | Use SAME palette block + same style anchor cho tất cả parts của 1 character |

### Cross-part consistency (quan trọng cho puppet)

Khi gen 6-7 part cho 1 character (vd Player), **PHẢI** dùng **SAME style anchor + SAME palette block** cho cả 6-7 prompt. Nếu thay đổi mid-batch (vd thay "white robe" thành "cream robe") → silhouette bị mix.

Recommend gen flow:
1. Gen `head.png` → confirm style + palette match.
2. Lock prompt template + palette.
3. Replace chỉ phần "subject body part" cho từng part còn lại.
4. Gen tất cả parts trong 1 session (model context giữ consistent).

### Pivot validation post-gen

Sau khi drop PNG vào folder và Bootstrap → mở Player.prefab trong Unity Editor:
- Click child Transform (`SpriteRoot/ArmLeft` etc.)
- Position default = (`-0.18, 0.05, 0`) (auto từ BootstrapWizard)
- Nếu sprite render lệch joint → adjust `Position` của child Transform cho đến khi pivot khớp khớp xương vai.
- Save prefab.

### Re-generation cycle

- PR G/H/I không lock asset trong git. PNG trong `Art/Characters/{id}/` — user có thể replace anytime + re-Bootstrap. Tuning trong `PuppetAnimController` Inspector preserved nếu prefab existing (chỉ refresh sprite refs).
- Old PNG lưu local backup trước khi replace (tránh lose work nếu bản mới không tốt hơn).

---

## Cost Estimate (GPT Image 2.0)

> **Pricing model:** GPT Image 2.0 ~$0.02/image (1024×1024 standard). Iteration thông thường 2-4 var/asset
> để chọn best output. Cost dưới đây là baseline (1 var) + realistic budget (3-4 var iteration).

### Phase 1 — Core MVP (side-only, no L2, no NPC, no VFX/weather)

| Group | Asset count | Image gen (4 var) | Baseline cost |
|---|---|---|---|
| Player puppet E (6 parts: head, torso, 2 arm, 2 leg) | 6 | 24 | $0.48 |
| Wolf puppet E (7 parts incl. tail) | 7 | 28 | $0.56 |
| FoxSpirit puppet E | 7 | 28 | $0.56 |
| Rabbit puppet E (Phase 2A — 7 parts incl. cottontail) | 7 | 28 | $0.56 |
| Boar puppet E (Phase 2B — 7 parts incl. bristly stub tail) | 7 | 28 | $0.56 |
| Deer Spirit puppet E (Phase 2B — 7 parts incl. white tail flick) | 7 | 28 | $0.56 |
| Boss puppet E (Phase 2C — 6 parts humanoid mirror Player rig, NO tail) | 6 | 24 | $0.48 |
| Single-sprite mobs (Crow, Snake, Bat) | 3 | 12 | $0.24 |
| Resources (12 nodes) | 12 | 48 | $0.96 |
| Item icons (22) | 22 | 88 | $1.76 |
| Tiles (12 seamless: 3 biome × 4 var) | 12 | 48 | $0.96 |
| **Phase 1 subtotal** | **96** | **~384** | **~$7.68** |

### Phase 2 — Multi-direction (L3 NSEW)

> Add-on cho 7 puppet character (Player + Wolf + Fox + Rabbit + Boar + Deer Spirit + Boss). Mob single-sprite + resources + tiles KHÔNG cần.

| Group | Asset count | Image gen (4 var) | Baseline cost |
|---|---|---|---|
| Player N + S (6 × 2) | 12 | 48 | $0.96 |
| Wolf N + S (7 × 2, skip S/tail → 13) | 13 | 52 | $1.04 |
| FoxSpirit N + S (skip S/tail → 13) | 13 | 52 | $1.04 |
| Rabbit N + S (Phase 2A — skip S/tail → 13) | 13 | 52 | $1.04 |
| Boar N + S (Phase 2B — skip S/tail → 13) | 13 | 52 | $1.04 |
| Deer Spirit N + S (Phase 2B — skip S/tail → 13) | 13 | 52 | $1.04 |
| Boss N + S (Phase 2C — humanoid 6 × 2, NO tail skip needed) | 12 | 48 | $0.96 |
| **Phase 2 subtotal** | **89** | **356** | **~$7.12** |

### Phase 3 — L2 elbow/knee (forearm + shin)

> Add-on cho 3 puppet character. 4 parts/dir × 3 dirs (E + N + S, Wolf/Fox skip S forearm/shin).

| Group | Asset count | Image gen (4 var) | Baseline cost |
|---|---|---|---|
| Player forearm + shin × 3 dir (4 × 3) | 12 | 48 | $0.96 |
| Wolf forearm + shin × 2 dir (E + N, skip S) | 8 | 32 | $0.64 |
| FoxSpirit forearm + shin × 2 dir (E + N, skip S) | 8 | 32 | $0.64 |
| **Phase 3 subtotal** | **28** | **112** | **~$2.24** |

### Phase 4 — Boss extension + Resource state variants

| Group | Asset count | Image gen (4 var) | Baseline cost |
|---|---|---|---|
| Boss Phase 2 enraged + death corpse | 2 | 8 | $0.16 |
| Resource depleted variants (tree stump, rock rubble, mushroom/berry/cactus/bamboo picked) | 6 | 24 | $0.48 |
| **Phase 4 subtotal** | **8** | **32** | **~$0.64** |

### Phase 5 — VFX

| Group | Asset count | Image gen (4 var) | Baseline cost |
|---|---|---|---|
| Combat VFX (hit flash, damage popup, blood splash, dust poof) | 4 | 16 | $0.32 |
| Effect VFX (fire spark, smoke wisp, qi glow, levelup halo, death decay) | 5 | 20 | $0.40 |
| Status icons (poison, bleed, burning, chilled, blessed) | 5 | 20 | $0.40 |
| **Phase 5 subtotal** | **14** | **56** | **~$1.12** |

### Phase 6 — Weather

| Group | Asset count | Image gen (4 var) | Baseline cost |
|---|---|---|---|
| Weather props (rain drop, snow flake, fog overlay, lightning bolt, sun ray, sandstorm overlay) | 6 | 24 | $0.48 |
| **Phase 6 subtotal** | **6** | **24** | **~$0.48** |

### Phase 7 — NPC humanoid (full puppet sets)

| Group | Asset count | Image gen (4 var) | Baseline cost |
|---|---|---|---|
| VendorNPC (head, torso, 2 arm, 2 leg, 2 forearm, 2 shin = 11 parts E only) | 11 | 44 | $0.88 |
| CompanionNPC (11 parts E only) | 11 | 44 | $0.88 |
| Quest Giver Elder + Villager M/F + Bandit (11 × 4 = 44, optional) | 44 | 176 | $3.52 |
| **Phase 7 subtotal (Vendor + Companion only)** | **22** | **88** | **~$1.76** |
| Phase 7 with all NPC variants | 66 | 264 | $5.28 |

### Phase 8 — Environment props

| Group | Asset count | Image gen (4 var) | Baseline cost |
|---|---|---|---|
| Props (chest closed/open, lantern, shrine, banner, signpost, barrel, crate, broken stele, tent, ceremonial altar) | 10 | 40 | $0.80 |
| **Phase 8 subtotal** | **10** | **40** | **~$0.80** |

### Total Coverage Summary

| Scope | Asset count | Baseline cost | Realistic w/ 3× iter |
|---|---|---|---|
| **Phase 1 only (MVP side-only)** | 96 | ~$7.68 | ~$20-25 |
| Phase 1 + 2 (multi-dir) | 185 | ~$14.80 | ~$40-55 |
| Phase 1 + 2 + 3 (L3+ full puppet) | 213 | ~$17.04 | ~$45-65 |
| Phase 1-6 (combat + VFX + weather) | 241 | ~$19.28 | ~$55-80 |
| **Phase 1-8 (FULL game asset coverage incl. NPC + environment)** | **273** (all NPC variants: **317**) | **~$21.84** (with all NPC: **~$25.36**) | **~$70-130** |

> **Recommendation:** ship Phase 1-3 first (MVP playtest), Phase 4-5 với balance pass, Phase 6-8 polish pass.
> Tổng prompt count trong file này: ~310 (counting each direction + state variant + NPC variant separately).

---

## Cross-references

- [`Documentation/ART_STYLE.md`](ART_STYLE.md) — full style bible (palette, lighting, sprite specs)
- [`prompts/README.md`](../prompts/README.md) — Leonardo workflow (Element training)
- [`prompts/gpt_workflow.md`](../prompts/gpt_workflow.md) — GPT image 2.0 specific tips
- [`prompts/tileset_gpt.txt`](../prompts/tileset_gpt.txt) — 12 ground tile prompts
- [`Assets/_Project/Art/Characters/README.md`](../Assets/_Project/Art/Characters/README.md) — puppet pipeline guide
- [`Assets/_Project/Art/Resources/README.md`](../Assets/_Project/Art/Resources/README.md) — resource pipeline
