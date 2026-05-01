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
3. [Puppet Characters](#3-puppet-characters) — Player, Wolf, FoxSpirit (multi-piece)
4. [Single-Sprite Mobs](#4-single-sprite-mobs) — Rabbit, Boar, Deer Spirit, Crow, Snake, Bat, Boss
5. [Resources / World Objects](#5-resources--world-objects) — tree, rock, water, mushroom, berry, cactus, lily, bamboo, grass tile, mineral, structures
6. [Item Icons](#6-item-icons) — 22 inventory icons
7. [Ground Tiles](#7-ground-tiles) — link to `prompts/tileset_gpt.txt`

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

> **Pipeline:** Drop PNG vào `Assets/_Project/Art/Characters/{characterId}/{filename}.png`. BootstrapWizard tự build puppet hierarchy.
>
> **Tối thiểu để puppet build:** `head.png` + `torso.png`. Nếu thiếu → fallback single-sprite. Limbs / tail optional.
>
> **Pose lock:** "side view 90°, facing right, neutral standing — arm thẳng xuống, leg thẳng đứng". PuppetAnimController rotate runtime → KHÔNG vẽ pose dynamic.
>
> **Resolution:** 256×512 (head 256×256 OK), transparent background, isolated single object.
>
> **Pivot tip:** mỗi body part nên có pivot tại khớp nối — vd `arm_left.png` pivot ở **vai** (top center), `leg_left.png` pivot ở **hông** (top center). Sprite Editor có thể chỉnh pivot sau import, nhưng artist gen với clean top-edge khớp giúp default Unity Center pivot vẫn xài được.

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

---

## 4. Single-Sprite Mobs

> **Pipeline:** Drop single PNG `Sprites/{mobId}.png` (legacy gen_sprites.py path) hoặc Art/Mobs/{mobId}/sprite.png (sau khi MobArtImporter merge — chưa có).
>
> **Pose:** side view, facing right, neutral standing.
>
> **Resolution:** 256×256 hoặc 256×192 (depends on aspect).
> **Style:** match puppet characters per ART_STYLE.md.

### 4.1 Rabbit — Linh Thố (Forest mob, peaceful)

```
=== rabbit ===

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

### 4.2 Boar — Lợn Rừng (Forest mob, aggressive)

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

### 4.3 Deer Spirit — Linh Lộc (Forest mob, mystical)

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

### 4.7 Boss Mob — Hắc Vương (Cursed Desert, end-game)

```
=== boss_mob ===

hand-painted painterly, asian wuxia, supernatural demonic.

Subject: side view of a massive twisted spirit boar boss, hulking
distorted form 2x size of normal boar, profile facing right, broken
crown of curved black tusks (some shattered), pelt corrupted with death
qi purple smoke wisps, glowing red eye sockets, exposed cracked ribs
showing inner darkness, hooves cloven and bloody, malevolent presence.

Palette: ink black #1a1a1a body base, death qi purple #9b6b8b corruption
mist, dirt shadow #8b7355 hide highlight, cinnabar red #8b3a3a glowing
eye and inner ribcage, bone bleached #d4d4d4 cracked tusk, dirt deep
#6b5d40 deep shadow.

Composition: 384x256 px (horizontal — boss bigger), isolated boss on
transparent, NO ground.
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

> **Đã có sẵn:** [`prompts/tileset_gpt.txt`](../prompts/tileset_gpt.txt) — 12 prompt seamless 64×64 cho 3 biome (forest grass, stone highlands, desert sand).
>
> **Pipeline:** Drop tile PNG vào `Assets/_Project/Art/Tiles/{biome}/tile_{biome}_{name}_{NN}.png`. BiomeTileImporter auto-wire → BiomeSO.groundTileVariants[].
>
> **Critical:** tile MUST seamless tile (left=right, top=bottom edges blend). Photopea seam-fix có thể cần sau gen — xem [`prompts/gpt_workflow.md`](../prompts/gpt_workflow.md).
>
> **Resolution:** 1024×1024 gen → downscale 64×64 PPU 64.

→ **Mở `prompts/tileset_gpt.txt`** để copy 12 tile prompts đầy đủ. Không duplicate vào file này (single source of truth).

---

## Iteration / Quality Tips

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

| Phase | Asset count | Image gen | Cost (~$0.02/image) |
|---|---|---|---|
| Player puppet | 6 (head, torso, 2 arm, 2 leg) | 4 var × 6 = 24 | $0.48 |
| Wolf puppet | 7 (+tail) | 4 × 7 = 28 | $0.56 |
| FoxSpirit puppet | 7 | 4 × 7 = 28 | $0.56 |
| Single-sprite mobs | 7 | 4 × 7 = 28 | $0.56 |
| Resources | 12 | 4 × 12 = 48 | $0.96 |
| Item icons | 22 | 4 × 22 = 88 | $1.76 |
| Tiles | 12 | 4 × 12 = 48 | $0.96 |
| **Total full asset set** | **73 unique** | **~292** | **~$5.84** |

Realistic with iteration (gen 2-3 lần per asset trung bình): **~$15-20**.

---

## Cross-references

- [`Documentation/ART_STYLE.md`](ART_STYLE.md) — full style bible (palette, lighting, sprite specs)
- [`prompts/README.md`](../prompts/README.md) — Leonardo workflow (Element training)
- [`prompts/gpt_workflow.md`](../prompts/gpt_workflow.md) — GPT image 2.0 specific tips
- [`prompts/tileset_gpt.txt`](../prompts/tileset_gpt.txt) — 12 ground tile prompts
- [`Assets/_Project/Art/Characters/README.md`](../Assets/_Project/Art/Characters/README.md) — puppet pipeline guide
- [`Assets/_Project/Art/Resources/README.md`](../Assets/_Project/Art/Resources/README.md) — resource pipeline
