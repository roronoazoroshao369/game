# AI Prompts — DST-Style Master Catalog (v2)

> Bộ prompt **mỗi character/mob nằm 1 block riêng** để bạn copy-paste 1 phát vào ChatGPT-Image / Leonardo / Midjourney, gen ra PNG sẵn drop vào `Art/Characters/{id}/`.
>
> **Style lock = Don't Starve Together (Klei) clone với theme asian wuxia cultivation.** DST chính là kiểu pipeline transform-based puppet rig đang có trong repo (head/torso/arm/forearm/leg/shin là PNG riêng, swing quanh khớp). Để rig render đẹp như DST, art phải tuân **4 luật DST** ở §1.
>
> **Workflow chuẩn (lặp cho mỗi entity):**
> 1. Gen STYLE-REF master image (full body T-pose side-view) trước → save làm style reference
> 2. Khi gen từng part, paste **block "Style anchor" + "Negative prompt" + part-specific block** + đính kèm STYLE-REF làm image reference (Midjourney `--cref`, Leonardo "image guidance", ChatGPT-Image "based on this image")
> 3. Drop PNG vào `Art/Characters/{id}/{E|N|S}/{part}.png`. `CharacterArtImporter` (PR #118) tự auto-PPU + trim.
>
> **Tham chiếu code:** `PuppetPlaceholderSpec.RectFor(role)` (size placeholder), `BootstrapWizard.BuildPuppetHierarchy` (offset khớp), `CharacterArtImporter.LoadSpritesFromFolder` (per-role auto-PPU = 64).

---

## Table of Contents

- [§1 Universal style anchor (DST 4 luật)](#1-universal-style-anchor-dst-4-luật)
- [§2 Universal negative prompt](#2-universal-negative-prompt)
- [§3 Universal anatomy rules](#3-universal-anatomy-rules)
- [§4 Per-character / per-mob blocks](#4-per-character--per-mob-blocks)
    - [§4.1 Player — Cultivation Hero](#41-player--cultivation-hero-artcharactersplayer)
    - [§4.2 Wolf — Hung Lang](#42-wolf--hung-lang-artcharacterswolf)
    - [§4.3 FoxSpirit — Linh Hồ](#43-foxspirit--linh-hồ-artcharactersfox_spirit)
    - [§4.4 Rabbit — Linh Thố](#44-rabbit--linh-thố-artcharactersrabbit)
    - [§4.5 Boar — Hắc Trư](#45-boar--hắc-trư-artcharactersboar)
    - [§4.6 DeerSpirit — Linh Lộc](#46-deerspirit--linh-lộc-artcharactersdeer_spirit)
    - [§4.7 Boss — Hắc Vương](#47-boss--hắc-vương-artcharactersboss)
    - [§4.8 Crow — Quạ Đen](#48-crow--quạ-đen-artcharacterscrow)
    - [§4.9 Bat — Dơi Đêm](#49-bat--dơi-đêm-artcharactersbat)
    - [§4.10 Snake — Thanh Xà](#410-snake--thanh-xà-artcharacterssnake)
    - [§4.11 VendorNPC — Lão Tiên Sinh](#411-vendornpc--lão-tiên-sinh-artcharactersvendor_npc)
    - [§4.12 CompanionNPC — Linh Nhi](#412-companionnpc--linh-nhi-artcharacterscompanion_npc)
- [§5 Resources / world objects](#5-resources--world-objects)
- [§6 Item icons](#6-item-icons)
- [§7 Ground tiles](#7-ground-tiles)
- [§8 VFX](#8-vfx)
- [§9 Weather](#9-weather)
- [§10 Environment props](#10-environment-props)
- [§11 Cost estimate](#11-cost-estimate)
- [§12 Iteration tips](#12-iteration-tips)

---

## §1 Universal style anchor (DST 4 luật)

DST signature làm rig "đẹp" thay vì "ghép patch" gồm 4 yếu tố. Mỗi prompt PHẢI bao trọn 4 yếu tố này:

1. **Thick INK BLACK outline** — stroke 16–24 px @ 1024 canvas (≈ 2 % chiều dài cạnh dài). Outline che chỗ ráp khớp ⇒ người chơi không thấy seams.
2. **Gouache flat painted** — fill phẳng, 3–4 tonal stops mỗi surface (light / mid / shadow / outline). KHÔNG smooth airbrush gradient. Brush strokes có thể nhìn thấy nhưng không photo-realistic.
3. **Stylized anatomy cường điệu** — head 1.3–1.5× wider than torso, limbs đơn giản hoá thành ovals / rectangles bo tròn. Torso ngắn (3:5 chiều cao so với toàn body). KHÔNG realistic anatomy.
4. **Neutral T-pose, side-view 90°** — pose tĩnh, không biểu cảm dynamic. Arm thẳng xuống, leg thẳng đứng, head nhìn thẳng (E direction = facing right; N = back-of-skull; S = front-of-face).

**Block paste vào prompt (luôn dùng)**:

```
DST-style hand-painted illustration, Klei Don't Starve Together aesthetic,
gouache flat fills with 3-4 tonal stops per surface, visible brush strokes,
THICK INK BLACK outline 16-24px (2% of canvas dimension), readable silhouette,
stylized exaggerated anatomy with oversized head and simple oval limbs,
asian wuxia cultivation fantasy theme, neutral T-pose, pure side-view 90 degrees,
isolated single body part on FULLY transparent background, no ground, no shadow,
NOT photo-realistic, NOT anime moe, NOT chibi, NOT pixel art, NOT smooth airbrush.
```

---

## §2 Universal negative prompt

Paste vào field "Negative prompt" / "Avoid" / "Exclude" mọi generation:

```
photo-realistic, anime moe, chibi, pixel art, smooth airbrush gradient,
soft shading, ambient occlusion, cast shadow, drop shadow, ground beneath,
floor, dirt, grass, water, multiple subjects, duplicate, mirror, twin,
text, caption, watermark, signature, logo, frame, border, grid lines,
UI elements, lens flare, bokeh, depth of field, volumetric light,
anatomical realism, realistic muscle definition, fine detail wrinkles,
pure black outline (use INK BLACK with slight texture, not solid #000),
overlapping body parts, attached body parts (must be ISOLATED single part).
```

---

## §3 Universal anatomy rules

Tham chiếu code: `PuppetPlaceholderSpec.RectFor(role)`. Mỗi role có placeholder rectangle (px) + world height target (px / 64 PPU). Prompt PHẢI gen art có aspect ratio gần giống placeholder (±10 %), nếu không rig sẽ render lệch.

| Role | Placeholder W×H (px) | World H (u) | Aspect (W:H) | Pivot | Ghi chú |
| --- | --- | --- | --- | --- | --- |
| `head` | 40×40 | 0.625 | 1.00 | center | Head DST cố tình "to" — bạn có thể gen wider (tới 50×40) cho thêm cute |
| `torso` | 52×80 | 1.250 | 0.65 | center | Pose neutral, không pose dynamic |
| `arm_left` / `arm_right` | 16×56 | 0.875 | 0.29 | top center (vai) | Hẹp + dài, hình oval bo tròn 2 đầu |
| `forearm_left` / `forearm_right` | 14×44 | 0.688 | 0.32 | top center (khuỷu) | Nhỏ hơn arm; bottom = bàn tay |
| `leg_left` / `leg_right` | 18×60 | 0.938 | 0.30 | top center (hông) | Đùi → đầu gối |
| `shin_left` / `shin_right` | 16×44 | 0.688 | 0.36 | top center (gối) | Bàn chân nằm bottom |
| `tail` | 50×18 | 0.281 | 2.78 | left center (gốc tail) | Rộng > cao; mob only |
| `wing_left` / `wing_right` | 54×28 | 0.438 | 1.93 | shoulder pivot | Crow/Bat — neutral extended sideways |
| `body_seg_1..4` | 38×26 → 26×20 | 0.41 → 0.31 | ≈1.5 | left center | Snake — taper từ neck → tail |

**Quan trọng:** mỗi part PNG phải có **pivot pixel** ở vị trí sẽ ráp vào parent (vai cho arm, khuỷu cho forearm, hông cho leg, gối cho shin). Trong prompt nói rõ "top edge clean horizontal at {pivot}" và "no body extension above pivot line".

---

## §4 Per-character / per-mob blocks

Mỗi block dưới đây là 1 đơn vị copy-paste hoàn chỉnh: 1 STYLE-REF master (gen trước) + per-part prompts cho 3 directions (E/N/S) + 1 quick-copy bundle. Không cần bay qua bay lại §1/§2/§3 — đã được nhúng sẵn.

---

### §4.1 Player — Cultivation Hero (`Art/Characters/player/`)

**Concept:** young male qi-cultivator monk, white robe with gold sash, calm focused expression, ink-black hair tied in topknot, jade pendant. Bipedal humanoid.

**Palette (lock):** bone cream skin `#e8d5a6` / warm shadow `#b89968` / ink black hair `#1a1a1a` / robe white `#f0e8d0` / gold sash `#d4a64a` / jade pendant `#6b8e62` / dark mid `#8a6f47`.

**Anatomy spec:**
- Head:Torso width ratio = 1.4:1 (DST stylized).
- Limbs = simple oval/rounded rectangle, no muscle definition.
- Neutral T-pose, arm hanging straight, leg straight standing.

#### §4.1.1 STYLE-REF master (gen FIRST, save as `player_style_ref.png`)

```
DST-style hand-painted illustration, Klei Don't Starve Together aesthetic,
gouache flat fills with 3-4 tonal stops, visible brush strokes,
thick INK BLACK outline 16-24px, readable silhouette, stylized exaggerated anatomy
with oversized head and simple oval limbs, asian wuxia cultivation fantasy theme.

Subject: full body T-pose side-view (90 degrees, facing right) of a young Asian male
qi-cultivation monk. Big stylized head 1.4x torso width, calm focused expression,
ink-black hair tied in topknot bun with gold ribbon, smooth jade-pale skin.
Wearing flowing white martial arts robe with gold embroidered sash at waist,
simple oval arms hanging straight at sides, simple straight legs standing,
green jade pendant on chest, robe falls to mid-shin.

Palette ONLY: skin #e8d5a6 light / #b89968 mid / #8a6f47 shadow,
hair #1a1a1a, robe #f0e8d0 light / #b89968 fold,
sash gold #d4a64a, pendant jade #6b8e62, outline ink black #1a1a1a.

Composition: 1024x1536 PNG, isolated single character on FULLY transparent
background, no ground, no shadow, character vertically centered,
T-pose neutral exact (arm 90 deg straight down, leg straight, head facing right).

NEGATIVE: photo-realistic, anime moe, chibi, pixel art, smooth airbrush,
multiple subjects, duplicate, text, watermark, frame, border, ground, shadow,
realistic anatomy, dynamic pose, expression beyond calm, blur, depth of field.
```

#### §4.1.2 East direction parts (`Art/Characters/player/E/`)

Use `player_style_ref.png` as image reference for ALL parts below. Each part = isolated, transparent BG, neutral pose, side-view 90° facing right. Gen tuần tự 10 prompt.

```
─── player/E/head.png ─── (target 512×512, aspect 1.0)
DST-style, gouache flat fills, thick ink black outline 16-24px.
Subject: ISOLATED HEAD ONLY (cut clean at jaw line, NO neck) of young Asian
male cultivator, side-view facing right, calm focused expression, almond
eye, straight nose, ink-black hair tied topknot bun with gold ribbon.
Stylized big head ~1.4x normal proportion (DST chibi-adjacent).
Palette: skin #e8d5a6/#b89968/#8a6f47, hair #1a1a1a, ribbon #d4a64a, outline #1a1a1a.
Composition: 512x512 PNG, transparent BG, head centered, NO neck, NO shoulder, NO body.
NEGATIVE: see §2 + neck, shoulder, body parts below jaw, multiple heads.
```

```
─── player/E/torso.png ─── (target 512×800, aspect 0.65)
DST-style, gouache flat, thick ink black outline 16-24px.
Subject: ISOLATED TORSO ONLY (collar line top → hip bottom, NO head/arms/legs)
of young cultivator, side-view facing right, neutral standing pose, wearing
flowing white robe with gold embroidered sash at waist, green jade pendant on
chest, robe shoulders flat horizontal (puppet rig pivot at top edge).
Palette: robe #f0e8d0/#b89968/#8a6f47, sash gold #d4a64a, pendant jade #6b8e62, outline #1a1a1a.
Composition: 512x800 PNG (vertical), transparent BG, top edge clean horizontal at
collar (puppet pivot), bottom edge at hip line, NO head, NO arms, NO legs.
NEGATIVE: see §2 + arms, hands, legs, head.
```

```
─── player/E/arm_left.png ─── (target 200×700, aspect 0.29)
DST-style, gouache flat, thick ink black outline.
Subject: ISOLATED LEFT ARM ONLY (shoulder top → wrist bottom, NO body NO hand
detail beyond mitten silhouette) of cultivator, side-view facing right, hanging
straight down neutral T-pose, draped white robe sleeve, simple oval limb shape
(NO muscle definition). Top edge horizontal flat at shoulder (puppet pivot).
Palette: sleeve #f0e8d0/#b89968, hand silhouette #e8d5a6 mitten, outline #1a1a1a.
Composition: 200x700 PNG (vertical), transparent BG, top edge clean horizontal
at shoulder, bottom at wrist (NO forearm — that's a separate part).
NEGATIVE: see §2 + body, head, leg, full hand fingers, forearm below elbow.
```

```
─── player/E/arm_right.png ─── (target 200×700, aspect 0.29)
Same as arm_left but mirrored. Easiest workflow: gen arm_left, flip horizontal in
Photopea/GIMP, save as arm_right.png. PuppetAnimController will use sortingOrder
to put arm_right behind torso (back arm in side-view). If you want asymmetric
detail, re-gen with prompt "RIGHT arm" same anatomy.
```

```
─── player/E/forearm_left.png ─── (target 180×550, aspect 0.32)
DST-style, gouache flat, thick ink black outline.
Subject: ISOLATED LEFT FOREARM ONLY (elbow top → fingertip bottom, NO upper arm
NO body) of cultivator, side-view facing right, hanging neutral, white sleeve
cuff, mitten-style hand silhouette (no individual fingers detail). Top edge
horizontal at elbow (puppet pivot).
Palette: sleeve cuff #f0e8d0/#b89968, hand mitten #e8d5a6, outline #1a1a1a.
Composition: 180x550 PNG (vertical), transparent BG, top edge at elbow, bottom
at fingertip, mitten hand (no finger lines).
NEGATIVE: see §2 + upper arm, body, individual fingers, realistic hand.
```

```
─── player/E/forearm_right.png ─── (target 180×550)
Mirror of forearm_left.
```

```
─── player/E/leg_left.png ─── (target 230×750, aspect 0.30)
DST-style, gouache flat, thick ink black outline.
Subject: ISOLATED LEFT LEG ONLY (hip top → knee bottom, NO body NO foot)
of cultivator, side-view facing right, straight standing neutral, white robe
trousers, simple cylindrical limb (DST style). Top edge horizontal at hip
(puppet pivot).
Palette: trousers #f0e8d0/#b89968, outline #1a1a1a.
Composition: 230x750 PNG (vertical), transparent BG, top edge at hip, bottom
at knee (NO shin/foot — that's a separate part).
NEGATIVE: see §2 + body, shin, foot, both legs, realistic muscle.
```

```
─── player/E/leg_right.png ─── (target 230×750)
Mirror of leg_left.
```

```
─── player/E/shin_left.png ─── (target 200×550, aspect 0.36)
DST-style, gouache flat, thick ink black outline.
Subject: ISOLATED LEFT SHIN+FOOT ONLY (knee top → toe bottom, NO upper leg
NO body) of cultivator, side-view facing right, straight neutral pose, white
trouser hem + simple cloth shoe (no laces, mitten-style). Top edge horizontal
at knee (puppet pivot).
Palette: trouser hem #f0e8d0/#b89968, shoe #8a6f47/#1a1a1a, outline #1a1a1a.
Composition: 200x550 PNG (vertical), transparent BG, top edge at knee, bottom
at toe (foot just touching imaginary ground).
NEGATIVE: see §2 + thigh, body, both shins, realistic shoe with sole detail.
```

```
─── player/E/shin_right.png ─── (target 200×550)
Mirror of shin_left.
```

#### §4.1.3 North direction parts (`Art/Characters/player/N/`) — back view

Khi player walk north (away from camera), sprite hiển thị back-of-character. Chỉ 4 parts khác biệt; còn lại tương tự E flip.

```
─── player/N/head.png ─── (target 512×512)
DST-style. ISOLATED HEAD ONLY, BACK VIEW (back of skull facing camera), NO face
visible, just topknot bun and back-of-head ink-black hair, gold ribbon visible
from behind, neck stub at jaw line cut clean.
Palette: hair #1a1a1a, ribbon #d4a64a, neck skin #e8d5a6, outline #1a1a1a.
Composition: 512x512 PNG, transparent BG.
NEGATIVE: see §2 + face, eyes, nose, mouth, profile.
```

```
─── player/N/torso.png ─── (target 512×800)
DST-style. ISOLATED TORSO ONLY, BACK VIEW, robe back panel + sash bow at lower
back, no front pendant visible, shoulders horizontal flat (pivot).
Palette: robe #f0e8d0/#b89968, sash bow #d4a64a, outline #1a1a1a.
Composition: 512x800 PNG, transparent BG, no head, no limbs.
NEGATIVE: see §2 + face, front pendant, arms.
```

```
─── player/N/arm_left.png ─── (target 200×700)
DST-style. ISOLATED LEFT ARM, back view (so this is the arm on viewer's RIGHT
side of back). White sleeve hanging straight, mitten hand. Top horizontal at
shoulder.
Palette: same as E.
NEGATIVE: see §2 + front-view detail.
```

```
─── player/N/arm_right.png ─── Mirror of N/arm_left.
─── player/N/forearm_left.png ─── Same as E/forearm_left (sleeve cuff is symmetric).
─── player/N/forearm_right.png ─── Mirror of N/forearm_left.
─── player/N/leg_left.png ─── Same as E/leg_left (trouser is symmetric).
─── player/N/leg_right.png ─── Mirror of N/leg_left.
─── player/N/shin_left.png ─── Same as E/shin_left (shoe rear view, no toe visible from rear; gen with "back of foot, heel visible").
─── player/N/shin_right.png ─── Mirror of N/shin_left.
```

#### §4.1.4 South direction parts (`Art/Characters/player/S/`) — front view

```
─── player/S/head.png ─── (target 512×512)
DST-style. ISOLATED HEAD ONLY, FRONT VIEW (face directly camera), big head DST-
proportions, two almond eyes (small dots), straight nose, calm closed mouth,
topknot bun at top, gold ribbon side-tied. Cut clean at jaw line.
Palette: skin #e8d5a6/#b89968, hair #1a1a1a, ribbon #d4a64a, outline #1a1a1a.
Composition: 512x512 PNG, transparent BG, head centered, frontal symmetric.
NEGATIVE: see §2 + side profile, asymmetric face, smile, frown.
```

```
─── player/S/torso.png ─── (target 512×800)
DST-style. ISOLATED TORSO, FRONT VIEW, white robe with V-collar, gold sash
horizontal at waist, jade pendant at chest center, shoulders horizontal flat
(pivot).
Palette: robe #f0e8d0/#b89968, sash #d4a64a, pendant #6b8e62, outline #1a1a1a.
Composition: 512x800 PNG, transparent BG, no head, no limbs.
NEGATIVE: see §2 + back-view, side profile.
```

```
─── player/S/arm_left.png ─── (target 200×700) — front view, viewer's left side
─── player/S/arm_right.png ─── Mirror.
─── player/S/forearm_left.png ─── Same E pattern, frontal mitten-hand.
─── player/S/forearm_right.png ─── Mirror.
─── player/S/leg_left.png ─── Front view, trouser straight.
─── player/S/leg_right.png ─── Mirror.
─── player/S/shin_left.png ─── Front view, shoe with toe visible at bottom front.
─── player/S/shin_right.png ─── Mirror.
```

#### §4.1.5 Quick-copy bundle (paste 1 phát gen full set 30 PNG)

> Lưu ý: AI single-prompt thường không gen hết 30 PNG. Bundle này dùng cho Midjourney `--cref` workflow (gen 1 master ref → script-loop từng part). Nếu chỉ gen 1 prompt, dùng STYLE-REF (§4.1.1) thay vì bundle này.

```
You are generating asset pack for "player" character, asian male cultivator,
DST-style hand-painted with thick ink black outline. Output 30 isolated PNGs:

For each direction d in {E (side-right), N (back), S (front)}:
  Generate 10 isolated body parts: head, torso, arm_left, arm_right,
  forearm_left, forearm_right, leg_left, leg_right, shin_left, shin_right.

Style: gouache flat 3-4 tonal stops, ink black outline 16-24px, oversized
head (1.4x torso), simple oval limbs, neutral T-pose, transparent BG.

Palette: skin #e8d5a6/#b89968/#8a6f47, hair #1a1a1a, robe #f0e8d0/#b89968,
sash gold #d4a64a, pendant jade #6b8e62.

Composition per file: see §3 placeholder size table. Each PNG is ISOLATED single
part on transparent BG, top edge at puppet pivot (shoulder/elbow/hip/knee).

NEGATIVE: see §2.
```

---

### §4.2 Wolf — Hung Lang (`Art/Characters/wolf/`)

**Concept:** snarling Hung Lang wolf, slate-grey fur, amber eyes, fangs. Quadruped — but rigged as bipedal-ish puppet (front legs = arm/forearm, hind legs = leg/shin) for procedural anim reuse.

**Palette:** fur slate `#7a7c80` / fur shadow `#5a5d63` / fur highlight `#a3a5a8` / belly cream `#e8d5a6` / eye amber `#d4a64a` / fang bone `#c2c4ba` / outline `#1a1a1a`.

**Anatomy:**
- Side-view: pose like a sit-up wolf (front legs straight down, head forward).
- Head:Body width ratio = 1.2:1 (wolf snout extends head sideways).
- Tail = stub triangular.

#### §4.2.1 STYLE-REF master (`wolf_style_ref.png`)

```
DST-style hand-painted, Klei Don't Starve Together aesthetic, thick ink black
outline 16-24px, gouache flat fills, stylized animal anatomy.

Subject: full body side-view of a snarling Hung Lang wolf, slate-grey fur,
amber glaring eyes, white fangs slightly bared. Standing on all 4 legs but
artist-stylized so puppet rig works: front pair of legs straight down (= arm
+ forearm), hind pair (= leg + shin), short stub tail. Big stylized wolf
head with prominent snout, ears pricked up, neck thick.

Palette ONLY: fur slate #7a7c80 / shadow #5a5d63 / highlight #a3a5a8,
belly cream #e8d5a6, eye amber #d4a64a, fang bone #c2c4ba, outline ink #1a1a1a.

Composition: 1024x1536 PNG, isolated single wolf, transparent BG, no shadow,
T-pose neutral (legs straight, head forward, tail stub straight back).

NEGATIVE: see §2 + cute fluffy, dog-like friendly expression, pose dynamic,
realistic fur texture detail.
```

#### §4.2.2 East direction parts (`Art/Characters/wolf/E/`)

```
─── wolf/E/head.png ─── (target 600×500, aspect 1.2)
DST-style. ISOLATED WOLF HEAD ONLY (snarl, side-view facing right, NO neck/body),
slate-grey fur, big snout extending sideways, ear pricked, amber eye glaring,
fang slightly visible, mouth slightly open showing fang tip.
Palette: fur #7a7c80/#5a5d63/#a3a5a8, eye #d4a64a, fang #c2c4ba, outline #1a1a1a.
Composition: 600x500 PNG, transparent BG.
NEGATIVE: see §2 + neck, body, smile, dog-friendly expression, multiple heads.
```

```
─── wolf/E/torso.png ─── (target 700×900, aspect 0.78)
DST-style. ISOLATED WOLF TORSO ONLY (shoulder line top → hip bottom, NO head
NO legs), slate fur top, cream belly underside (countershading), thick neck
stub at top edge for puppet pivot.
Palette: fur #7a7c80/#5a5d63, belly #e8d5a6, outline #1a1a1a.
Composition: 700x900 PNG, transparent BG, top edge horizontal at shoulder line.
NEGATIVE: see §2 + head, legs, tail.
```

```
─── wolf/E/arm_left.png ─── (front leg upper, 200×700)
DST-style. ISOLATED WOLF FRONT LEFT LEG UPPER (shoulder top → elbow bottom),
slate fur, simple cylinder shape, top horizontal at shoulder pivot.
Palette: fur #7a7c80/#5a5d63, outline #1a1a1a.
Composition: 200x700 PNG, transparent BG.
NEGATIVE: see §2 + body, paw, lower leg.
```

```
─── wolf/E/arm_right.png ─── Mirror of arm_left (front right leg upper).
```

```
─── wolf/E/forearm_left.png ─── (front leg lower + paw, 180×550)
DST-style. ISOLATED WOLF FRONT LEFT LEG LOWER (elbow top → paw bottom),
slate fur, simple paw silhouette no individual claw lines.
Palette: fur #7a7c80, paw pad #1a1a1a, outline #1a1a1a.
Composition: 180x550 PNG, transparent BG.
NEGATIVE: see §2 + upper leg, body, individual claws.
```

```
─── wolf/E/forearm_right.png ─── Mirror.
```

```
─── wolf/E/leg_left.png ─── (hind leg upper, 230×750) — slate fur, top at hip pivot.
─── wolf/E/leg_right.png ─── Mirror.
─── wolf/E/shin_left.png ─── (hind leg lower + paw, 200×550) — slate fur + cream paw.
─── wolf/E/shin_right.png ─── Mirror.
```

```
─── wolf/E/tail.png ─── (target 600×220, aspect 2.7)
DST-style. ISOLATED WOLF TAIL ONLY (stub triangular shape), slate fur top
+ cream tip underside, pivot at LEFT-CENTER (gốc tail = nơi nối với hông).
Palette: fur #7a7c80/#5a5d63, tip cream #e8d5a6, outline #1a1a1a.
Composition: 600x220 PNG (horizontal), transparent BG, pivot at left edge center.
NEGATIVE: see §2 + body, multiple tails, fluffy fox-like.
```

#### §4.2.3 North + South direction (back / front view)

```
─── wolf/N/head.png ─── (back of wolf head, ears pricked up rear-view, slate fur)
─── wolf/N/torso.png ─── (back of wolf body, full slate fur top, no belly)
─── wolf/N/{arm,forearm,leg,shin}_*.png ─── back-view leg silhouettes (mirror E)
─── wolf/N/tail.png ─── tail base from rear (small fan shape)

─── wolf/S/head.png ─── (front-view wolf face, two amber eyes, snout pointing
   camera, fangs visible, "looking at player" snarl)
─── wolf/S/torso.png ─── (front-view chest cream + slate sides)
─── wolf/S/{arm,forearm,leg,shin}_*.png ─── front-view leg silhouettes
─── wolf/S/tail.png ─── SKIP (tail hidden behind body in front view; importer
   fallback to East sprite)
```

Use STYLE-REF + per-part prompts từ §4.2.2 với keyword "back view" / "front view" thay "side-view".

#### §4.2.4 Quick-copy bundle

```
Generate 27 PNGs (3 dirs × 9 parts + tail) for "wolf" entity, slate-grey Hung Lang
DST-style. See §4.2.1 STYLE-REF, §4.2.2 part list, §4.2.3 dir variants.
Palette: fur #7a7c80/#5a5d63/#a3a5a8, belly #e8d5a6, eye amber #d4a64a, fang #c2c4ba,
outline #1a1a1a. Tail = E only, S/N optional.
NEGATIVE: see §2 + dog-friendly, fluffy, cute.
```

---

### §4.3 FoxSpirit — Linh Hồ (`Art/Characters/fox_spirit/`)

**Concept:** Linh Hồ nine-tail fox spirit, orange-cinnabar fur with white-tip tail, glowing jade-blue qi mist, mystical aura, fanged but elegant. Quadruped puppet (same rig as wolf).

**Palette:** fur cinnabar `#a14040` / fur deep `#6b2828` / fur highlight `#d46c5c` / belly white `#e8d5a6` / tail tip cream `#f0e8d0` / qi blue glow `#a8d8ff` / eye golden `#d4a64a` / outline `#1a1a1a`.

**Anatomy:** thinner & taller than wolf, bushy tail (longer than wolf's stub).

#### §4.3.1 STYLE-REF master (`fox_spirit_style_ref.png`)

```
DST-style hand-painted, thick ink black outline 16-24px, gouache flat fills.

Subject: full body side-view of a Linh Hồ fox spirit, cinnabar-orange fur,
big bushy tail with cream-white tip, faint jade-blue qi mist around tail tip
and ear tips, golden glaring eyes, slim elegant build, 4 simple cylindrical
legs, T-pose neutral (legs straight down, tail straight back, head forward).

Palette ONLY: fur cinnabar #a14040 / deep #6b2828 / highlight #d46c5c,
belly cream #e8d5a6, tail tip #f0e8d0, qi mist #a8d8ff (faint glow only),
eye gold #d4a64a, outline ink #1a1a1a.

Composition: 1024x1536 PNG, isolated, transparent BG, no shadow.

NEGATIVE: see §2 + cute kitsune anime, multiple tails (only ONE rendered as
single sprite — multi-tail mystique conveyed via VFX, not art), realistic fur.
```

#### §4.3.2 Per-direction parts

Same as wolf §4.2.2 list (10 body parts × 3 dirs + tail). Replace palette with cinnabar fox palette. Tail prompt:

```
─── fox_spirit/E/tail.png ─── (target 700×260, aspect 2.7 — bigger than wolf)
DST-style. ISOLATED FOX BUSHY TAIL ONLY, cinnabar fur with cream-white tip,
faint jade-blue qi glow on tip (NOT solid, just hint), pivot at LEFT-CENTER
(gốc tail).
Palette: fur #a14040/#6b2828, tip #f0e8d0, qi #a8d8ff, outline #1a1a1a.
Composition: 700x260 PNG, transparent BG, pivot at left edge center, tail
extends to the right (away from body).
NEGATIVE: see §2 + body, nine tails, anime kitsune cute.
```

#### §4.3.3 Quick-copy bundle

Same template as wolf, swap palette + tail dimensions.

---

### §4.4 Rabbit — Linh Thố (`Art/Characters/rabbit/`)

**Concept:** Linh Thố nimble forest rabbit, cream-tan fur, twitchy nose, big floppy ears, cottontail. Quadruped puppet.

**Palette:** fur tan `#c8a878` / fur shadow `#8a7048` / fur highlight `#dfc89a` / belly cream `#f0e8d0` / ear inner pink `#d4a890` / eye black `#1a1a1a` / cottontail white `#f0e8d0` / outline `#1a1a1a`.

#### §4.4.1 STYLE-REF master

```
DST-style hand-painted, thick ink black outline.

Subject: full body side-view of a small forest rabbit (Linh Thố), tan fur,
big floppy ears (ear-tip slightly droop), short oval body, 4 small leg stumps
(front legs shorter than hind legs), cottontail bushy white at rear, twitchy
nose, big black eye. T-pose neutral.

Palette ONLY: fur #c8a878/#8a7048/#dfc89a, belly #f0e8d0, ear inner #d4a890,
eye #1a1a1a, cottontail #f0e8d0, outline #1a1a1a.

Composition: 1024x1024 PNG, isolated, transparent BG.

NEGATIVE: see §2 + Easter cute commercial, anime moe, anthropomorphic.
```

#### §4.4.2 Per-direction parts

10 body parts × 3 dirs + cottontail. Tail prompt:

```
─── rabbit/E/tail.png ─── (target 360×220, smaller cottontail)
DST-style. ISOLATED COTTONTAIL ONLY, fluffy white round shape, faint tan
shadow, pivot LEFT-CENTER.
Palette: cotton #f0e8d0, shadow #8a7048, outline #1a1a1a.
Composition: 360x220 PNG, transparent BG.
NEGATIVE: see §2 + body, multiple cottontails.
```

---

### §4.5 Boar — Hắc Trư (`Art/Characters/boar/`)

**Concept:** wild boar, dark brown bristly coarse fur, ivory tusks, heavy stocky body, stub tail.

**Palette:** fur dark brown `#5a4030` / fur shadow `#2a1f15` / fur highlight `#8b6f47` / tusk ivory `#d4c8a3` / eye small black `#1a1a1a` / nose dark `#1a1a1a` / outline `#1a1a1a`.

#### §4.5.1 STYLE-REF master

```
DST-style hand-painted, thick ink black outline.

Subject: full body side-view of a wild Hắc Trư boar, dark brown bristly fur
with visible bristle ridge along back, two ivory tusks protruding from lower
jaw, small black eye, dark snout, heavy stocky body with short stubby legs,
tiny stub tail. T-pose neutral but slight aggressive stance (head low forward).

Palette ONLY: fur #5a4030/#2a1f15/#8b6f47, tusk #d4c8a3, eye/nose #1a1a1a,
outline ink #1a1a1a.

Composition: 1024x1024 PNG, isolated, transparent BG.

NEGATIVE: see §2 + cute pig, pink, smiling, friendly.
```

#### §4.5.2 Per-direction parts

10 body parts × 3 dirs + stub tail. Bristle ridge visible on torso top.

---

### §4.6 DeerSpirit — Linh Lộc (`Art/Characters/deer_spirit/`)

**Concept:** Linh Lộc forest deer spirit, light brown fur with cream-spotted back, large branched antlers with faint jade qi glow, tall slim build, alert posture.

**Palette:** fur light brown `#a08060` / fur shadow `#6a4830` / fur highlight `#d4b896` / belly cream `#e8d5a6` / antler base `#c4a574` / antler tip `#a08060` / qi jade `#a8c69b` / eye amber `#d4a64a` / outline `#1a1a1a`.

#### §4.6.1 STYLE-REF master

```
DST-style hand-painted, thick ink black outline.

Subject: full body side-view of a Linh Lộc deer spirit, slim graceful build,
4 long thin legs, large branched antlers with faint jade-green qi glow,
gentle amber eye, tan fur with cream-white spots scattered on flank, white
underbelly, white short tail flick. T-pose neutral.

Palette ONLY: fur #a08060/#6a4830/#d4b896, belly #e8d5a6, antler #c4a574/#a08060,
qi #a8c69b, eye #d4a64a, outline #1a1a1a.

Composition: 1024x1536 PNG (taller — antlers extend up), isolated, transparent BG.

NEGATIVE: see §2 + Disney cute, anime moe, baby Bambi.
```

#### §4.6.2 Per-direction parts

10 body parts × 3 dirs + tail flick. Head includes antlers (head PNG vertical = 800×600 to fit antlers).

---

### §4.7 Boss — Hắc Vương (`Art/Characters/boss/`)

**Concept:** Black King cursed overlord, dark crimson and obsidian armor over withered cultivator body, glowing purple death-qi aura, sharp asymmetric silhouette, towering humanoid. Bipedal puppet (same as Player).

**Palette:** armor obsidian `#1a1a20` / armor highlight `#3a3a48` / crimson cloth `#8c1923` / crimson deep `#5a0e15` / death qi purple `#9b6b8b` / death qi deep `#6b3a5b` / eye glow purple `#d4a8e0` / skull bone `#c2c4ba` / outline `#1a1a1a`.

#### §4.7.1 STYLE-REF master

```
DST-style hand-painted, thick ink black outline.

Subject: full body T-pose side-view of Hắc Vương cursed overlord, towering
humanoid 1.5x player height, dark obsidian armor with sharp asymmetric
shoulder pauldrons, crimson cloth hanging from waist (torn), exposed
withered skull-like face under hooded helm, glowing purple eye, death-qi
purple aura wisp around silhouette (faint), heavy long arms ending in
clawed gauntlets, armored greaves on legs.

Palette ONLY: armor #1a1a20/#3a3a48, crimson #8c1923/#5a0e15,
qi purple #9b6b8b/#6b3a5b, eye glow #d4a8e0, skull #c2c4ba, outline #1a1a1a.

Composition: 1024x1536 PNG (taller — boss = 1.5x normal height), isolated,
transparent BG, no shadow.

NEGATIVE: see §2 + heroic noble, cute villain, anime style, realistic gore.
```

#### §4.7.2 Per-direction parts

10 body parts × 3 dirs (NO tail). Same anatomy slot as Player but armored. Each part should retain "ink black outline + obsidian fill" DST signature.

---

### §4.8 Crow — Quạ Đen (`Art/Characters/crow/`)

**Concept:** glossy black corvid scavenger, yellow beak, golden eye. Bipedal flying puppet — REPLACES `arm_left/right + forearm_left/right` with `wing_left/right` (no forearm). Has `leg_left/right + shin_left/right` for landing pose.

**Required parts** (per `PuppetPlaceholderSpec.RolesForCharacter` with `includeWings=true`):
- E direction: `head, torso, leg_left, leg_right, shin_left, shin_right, wing_left, wing_right` (8 parts, NO arm/forearm/tail)

**Palette:** feather glossy black `#08080a` / feather highlight `#3a3a48` / beak yellow `#f4c93a` / eye iris yellow `#f4c93a` / eye pupil `#08080a` / talon dark `#1a1a1a` / outline `#1a1a1a`.

#### §4.8.1 STYLE-REF master

```
DST-style hand-painted, thick ink black outline.

Subject: full body side-view of a glossy black crow, perched neutral pose
(legs straight down, wings extended sideways horizontal — neutral spread for
puppet flap rotation around shoulder), yellow beak, sharp golden iris with
black pupil, tail short fan, 2 small thin legs ending in 3-toe talon
silhouette.

Palette ONLY: feather #08080a/#3a3a48, beak #f4c93a, iris #f4c93a/#08080a,
talon #1a1a1a, outline #1a1a1a.

Composition: 1024x1024 PNG (squarer — wingspan horizontal), isolated, transparent BG.

NEGATIVE: see §2 + cute, raven (different species), anthropomorphic.
```

#### §4.8.2 Per-direction parts

```
─── crow/E/head.png ─── (target 480×400)
DST-style. ISOLATED CROW HEAD ONLY, glossy black feather, yellow beak pointing
right, golden eye visible.
─── crow/E/torso.png ─── (target 600×800)
DST-style. ISOLATED CROW BODY ONLY (no head/wings/legs), glossy black breast
and back, fan tail at rear bottom.
─── crow/E/wing_left.png ─── (target 750×400, aspect 1.88)
DST-style. ISOLATED LEFT WING ONLY (extended horizontal flap-neutral pose),
glossy black feathers with subtle highlight, pivot at LEFT-CENTER (shoulder
attach point), wing extends to the RIGHT.
Palette: feather #08080a/#3a3a48, outline #1a1a1a.
Composition: 750x400 PNG, transparent BG, pivot left-center.
NEGATIVE: see §2 + body, two wings, folded wing.

─── crow/E/wing_right.png ─── Mirror.
─── crow/E/leg_left.png ─── (small thin leg upper, 130×500)
─── crow/E/leg_right.png ─── Mirror.
─── crow/E/shin_left.png ─── (lower leg + 3-toe talon, 130×400)
─── crow/E/shin_right.png ─── Mirror.
```

N/S directions: head front-view = 2 eyes + beak pointing camera; back-view = back-of-head + back of feathers. Wings same as E (horizontal extended).

---

### §4.9 Bat — Dơi Đêm (`Art/Characters/bat/`)

**Concept:** leathery cave-dwelling bat, dark brown fur, translucent membranous wings (4 strut visible), small body, clawed feet for hanging.

**Required parts:** same skeleton as crow (head, torso, leg×2, shin×2, wing×2 — no arm/forearm/tail).

**Palette:** fur dark brown `#3a2a20` / fur highlight `#5a3530` / wing membrane translucent `#5a3530` (60% opacity feel) / wing strut darker `#1a1a1a` / eye small red `#a14040` / fang white `#c2c4ba` / outline `#1a1a1a`.

#### §4.9.1 STYLE-REF master

```
DST-style hand-painted, thick ink black outline.

Subject: full body side-view of a leathery bat, small fuzzy dark brown body,
two big membranous wings extended sideways (neutral flap pose, 4 visible
strut bones each wing, membrane semi-translucent dark brown with subtle
highlight), tiny pointed ears, beady red eye, small fangs visible, tiny
clawed feet hanging.

Palette ONLY: fur #3a2a20/#5a3530, wing membrane #5a3530 (translucent feel),
strut #1a1a1a, eye #a14040, fang #c2c4ba, outline ink #1a1a1a.

Composition: 1024x1024 PNG (squarer — wingspan horizontal), isolated, transparent BG.

NEGATIVE: see §2 + cute halloween, vampire anime, realistic photo.
```

#### §4.9.2 Per-direction parts

Same part list as crow. Wing prompt:

```
─── bat/E/wing_left.png ─── (target 800×450)
DST-style. ISOLATED LEFT BAT WING (extended horizontal), 4 visible strut bones
in dark ink black, membrane semi-translucent dark brown filling between struts,
pivot at LEFT-CENTER (shoulder attach point).
Palette: membrane #5a3530, strut #1a1a1a, outline #1a1a1a.
Composition: 800x450 PNG, transparent BG, pivot left-center.
NEGATIVE: see §2 + folded, body.
```

---

### §4.10 Snake — Thanh Xà (`Art/Characters/snake/`)

**Concept:** Thanh Xà green forest serpent, scaled body in 4 segments tapering from neck to tail, wide hood (cobra-like), forked tongue, no limbs. Body-segment puppet (no arm/leg/tail/wing).

**Required parts** (per `PuppetPlaceholderSpec.RolesForCharacter` with `isSnake=true`):
- E direction: `head, body_seg_1, body_seg_2, body_seg_3, body_seg_4` (5 parts, NO arm/forearm/leg/shin/tail/wing)

**Placeholder sizes** (from `RectFor`):
- head 40×40 → world 0.625u
- body_seg_1 38×26 → world 0.41u (neck — widest)
- body_seg_2 36×26 → world 0.41u
- body_seg_3 32×24 → world 0.38u
- body_seg_4 26×20 → world 0.31u (tail — thinnest)

**Palette:** scale green `#2a4a2a` / scale highlight `#5a8a4a` / scale shadow `#1a2a1a` / belly cream `#a8c69b` / hood inner cream `#c8d4a8` / eye amber-gold `#d4a64a` / tongue red `#8b3a3a` / outline ink `#1a1a1a`.

#### §4.10.1 STYLE-REF master (`snake_style_ref.png`)

```
DST-style hand-painted, thick ink black outline.

Subject: full body top-down/side-hybrid view of a Thanh Xà green serpent,
4 segmented body sections tapering from wide neck to thin tail, cobra-like
flared hood at neck behind head, head with forked red tongue protruding,
amber-gold eye, scale pattern visible (DST-stylized 1-pixel diamond pattern
not realistic). Body laid out STRAIGHT horizontal for puppet rig (each segment
will rotate at junctions in-game).

Palette ONLY: scale #2a4a2a / highlight #5a8a4a / shadow #1a2a1a,
belly cream #a8c69b, hood inner #c8d4a8, eye #d4a64a, tongue #8b3a3a,
outline #1a1a1a.

Composition: 1536x600 PNG (horizontal — body extends right), isolated,
transparent BG, head at left, tail at right, body STRAIGHT.

NEGATIVE: see §2 + coiled body, dynamic curve pose, dragon, multiple heads.
```

#### §4.10.2 Per-direction parts

```
─── snake/E/head.png ─── (target 480×480)
DST-style. ISOLATED SNAKE HEAD ONLY (with hood flare), forked tongue out,
amber eye, side-view facing right, no neck/body.
Palette: see §4.10.
Composition: 480x480 PNG, transparent BG.
NEGATIVE: see §2 + body, dragon.

─── snake/E/body_seg_1.png ─── (target 560×400, aspect 1.40 ≈ placeholder 1.46)
DST-style. ISOLATED NECK SEGMENT (widest segment), scale pattern, belly cream
underside, cylinder oval shape laid horizontal, pivot at LEFT-CENTER (junction
to head).
Composition: 560x400 PNG, transparent BG.

─── snake/E/body_seg_2.png ─── (target 540×400, slightly thinner)
Same as seg_1 but slightly narrower, pivot LEFT-CENTER.

─── snake/E/body_seg_3.png ─── (target 480×360, more taper)
Same pattern, narrower.

─── snake/E/body_seg_4.png ─── (target 400×320, thinnest tail tip)
Same pattern, thinnest. Has tail-pointed end at right (free end).
```

N (back of snake = same as E body silhouette, head from back-of-skull) and S (front of snake head = forked tongue + 2 eyes facing camera) follow same pattern. Body segments same as E (snake body symmetry top/side blurs).

---

### §4.11 VendorNPC — Lão Tiên Sinh (`Art/Characters/vendor_npc/`)

**Concept:** elder merchant cultivator, white long beard, jade-green robe with brown leather satchel, friendly wise expression, walking staff. Bipedal humanoid puppet (same skeleton as Player).

**Palette:** skin elder `#d4b896` / shadow `#8a6f47` / beard white `#f0e8d0` / robe jade `#5a8062` / robe shadow `#3a5a40` / robe highlight `#8aa882` / satchel brown `#8a6f47` / staff wood `#5a4030` / outline `#1a1a1a`.

#### §4.11.1 STYLE-REF master

```
DST-style hand-painted, thick ink black outline.

Subject: full body side-view of an elder merchant cultivator, long white
beard reaching chest, kind smiling expression with eye crinkle, jade-green
flowing robe with wide sleeves, brown leather satchel slung across torso,
holding wooden walking staff in one hand. Slightly hunched (elder posture)
but T-pose neutral for puppet rig (arm holding staff = arm_left, other arm
hanging = arm_right).

Palette ONLY: skin #d4b896/#8a6f47, beard #f0e8d0, robe jade #5a8062/#3a5a40/#8aa882,
satchel #8a6f47, staff #5a4030, outline #1a1a1a.

Composition: 1024x1536 PNG, isolated, transparent BG.

NEGATIVE: see §2 + young, evil, anime style.
```

#### §4.11.2 Per-direction parts

10 body parts × 3 dirs. Note `forearm_left` includes staff hand (so its prompt should specify "hand grasping wooden staff"). Same skeleton as Player §4.1.

---

### §4.12 CompanionNPC — Linh Nhi (`Art/Characters/companion_npc/`)

**Concept:** young female cultivator companion, twin braided hair, light blue/purple robe with silver moon embroidery, calm gentle expression, small spiritual sword on belt. Bipedal humanoid puppet.

**Palette:** skin fair `#f0e0c8` / shadow `#c89970` / hair black `#1a1a1a` / hair tie ribbon silver `#c2c4ba` / robe blue-purple `#6a6a8c` / robe shadow `#3a3a5a` / robe highlight `#9a9ac0` / silver moon `#c2c4ba` / sword sheath `#5a4030` / outline `#1a1a1a`.

#### §4.12.1 STYLE-REF master

```
DST-style hand-painted, thick ink black outline.

Subject: full body side-view of a young female cultivator (early 20s, calm
serene expression), twin braided hair tied with silver ribbons reaching
shoulders, wearing flowing blue-purple robe with silver crescent-moon
embroidery on sleeve and hem, small spiritual sword in lacquered sheath
hanging at left hip, T-pose neutral, simple oval limbs DST-style.

Palette ONLY: skin #f0e0c8/#c89970, hair #1a1a1a, ribbon #c2c4ba,
robe #6a6a8c/#3a3a5a/#9a9ac0, moon embroidery #c2c4ba, sheath #5a4030,
outline ink #1a1a1a.

Composition: 1024x1536 PNG, isolated, transparent BG.

NEGATIVE: see §2 + anime moe, sexualized, chibi big-head extreme, cute commercial.
```

#### §4.12.2 Per-direction parts

10 body parts × 3 dirs. Same skeleton as Player. Sword sheath visible on `torso` (E side-view shows hip silhouette with sword tip protruding).

---

## §5 Resources / world objects

Style: same DST hand-painted gouache + thick ink outline. Top-down 30° angle (resources lay on world tile, not pure side-view). 256×256 PNG transparent BG.

```
─── tree (Cổ Linh Mộc, Art/Resources/tree/) ───
DST-style hand-painted, thick ink black outline. Subject: gnarled forest
spirit tree top-down 30° view, deep moss green leaves canopy, twisted bark
brown trunk visible at base, faint jade-green qi mist in foliage. 256x256 PNG.
Palette: leaves #4a6741/#a8c69b, trunk #b89968/#8a6f47, qi #a8c69b, outline #1a1a1a.

─── rock (Linh Thạch, Art/Resources/rock/) ───
DST-style. Subject: weathered stone boulder top-down 30° view, slate grey with
moss patches on top. 256x256 PNG.
Palette: stone #7a7c80/#5a5d63, moss #4a6741, outline #1a1a1a.

─── mineral_rock (Khoáng Thạch, Art/Resources/rock/ variant) ───
DST-style. Subject: stone boulder with embedded mineral blue crystal vein
top-down 30° view. 256x256 PNG.
Palette: stone #7a7c80/#5a5d63, mineral blue #4d6b8c, outline #1a1a1a.

─── water_spring (Linh Tuyền, Art/Resources/water/) ───
DST-style. Subject: small spirit spring pool top-down 90° overhead, qi-blue
water with concentric ripple, mossy stone rim. 256x256 PNG.
Palette: water #6fb5e0/#a8d8ff, stone rim #7a7c80, outline #1a1a1a.

─── linh_mushroom (Art/Resources/linh_mushroom/) ───
DST-style. Subject: single tall spiritual mushroom top-down 30° view, red cap
with white spots, cream stem. 256x256 PNG.
Palette: cap #a14040/#d46c5c, stem #f0e8d0, spot #f0e8d0, outline #1a1a1a.

─── berry_bush (Linh Quả Mọng, Art/Resources/berry_bush/) ───
DST-style. Subject: low forest bush with cluster of small purple-red berries,
top-down 30°. 256x256 PNG.
Palette: leaves #4a6741, berry #8b3a3a/#a14040, outline #1a1a1a.

─── cactus (Tiên Nhân Chưởng, Art/Resources/cactus/) ───
DST-style. Subject: desert cactus with two side arms (DST cactus shape), green
with white spikes, top-down 30°. 256x256 PNG.
Palette: cactus green #6b8559/#3a5a40, spike #f0e8d0, outline #1a1a1a.

─── death_lily (Tử Linh Hoa, Art/Resources/death_lily/) ───
DST-style. Subject: cursed-desert lily flower with purple-black petals, faint
death-qi purple aura, top-down 30°. 256x256 PNG.
Palette: petal #9b6b8b/#6b3a5b, qi #d4a8e0, stem #2a4a2a, outline #1a1a1a.

─── linh_bamboo (Linh Trúc, Art/Resources/linh_bamboo/) ───
DST-style. Subject: tall jade-green bamboo cluster (3 stalks), faint qi glow
at joints, top-down 30°. 256x256 PNG.
Palette: bamboo #6b8e62/#a8c69b, qi #a8d8ff, outline #1a1a1a.

─── grass_tile (Linh Cỏ, Art/Resources/grass_tile/) ───
DST-style. Subject: small tuft of forest grass, top-down 90°, sage green with
mint highlight. 128x128 PNG (smaller).
Palette: grass #6b8e62/#a8c69b, outline #1a1a1a.

─── campfire (Art/Resources/campfire/) ───
DST-style. Subject: wood logs arranged in star pattern with bright flame on
top, faint orange smoke, top-down 30°. 256x256 PNG.
Palette: wood #5a4030/#8a6f47, flame #d4a64a/#a14040, smoke #c2c4ba, outline #1a1a1a.

─── workbench (Art/Resources/workbench/) ───
DST-style. Subject: wooden crafting workbench with hammer + chisel on top,
top-down 30°. 256x256 PNG.
Palette: wood #5a4030/#8a6f47, tool metal #c2c4ba, outline #1a1a1a.
```

### §5.13 State variants (harvested / depleted)

For each `_harvested` or `_depleted` variant: rerun base prompt với "depleted state, flat tree stump only" / "harvested state, missing fruit, dry leaves" — keep palette desaturated 20%.

---

## §6 Item icons

256×256 transparent BG, DST-style with thick ink outline, frontal isometric icon view.

```
─── materials ───
- wood log: brown log section #5a4030/#8a6f47, pith ring detail
- stone block: grey #7a7c80 block
- linh_stone (linh thạch shard): jade green crystal shard #6b8e62/#a8c69b with qi glow
- iron_ore: dark grey #5a5d63 ore chunk with mineral blue #4d6b8c streak
- bamboo_cane: jade #6b8e62 cylinder with node bands

─── foods & drinks ───
- raw_meat: red #8b3a3a/#a14040 cut meat slab
- cooked_meat: brown #8a6f47 grilled chunk with char marks
- berry: cluster of 3 purple-red #8b3a3a berries
- linh_mushroom_food: red cap #a14040 mushroom prepared
- water_canteen: brown leather flask with water-blue cap glow

─── tools ───
- stone_axe: stone head #7a7c80 + wood handle #5a4030
- iron_pickaxe: metal head #5a5d63 + wood handle
- bone_knife: ivory blade #d4c8a3 + wrapped grip
- fishing_rod: wood pole + string + small hook

─── accessories ───
- jade_pendant: jade green #6b8e62 with gold #d4a64a string
- qi_charm: paper talisman #f0e8d0 with red #8b3a3a sigil ink
- spirit_root_token: stone disc with elemental sigil
```

Mỗi item dùng template:

```
DST-style hand-painted icon, thick ink black outline 16-24px, gouache flat fills,
isolated single object on transparent background, isometric icon view (slight 3/4
angle), 256x256 PNG. Subject: <ITEM>. Palette: <COLORS>. NEGATIVE: see §2.
```

---

## §7 Ground tiles

Seamless tileable 512×512 PNG, hand-painted gouache, top-down 90°.

```
─── forest tile (4 variants) ───
DST-style hand-painted, top-down 90°. Subject: forest floor texture, deep moss
green #4a6741 base with sage #6b8e62 highlight, scattered dry leaf #8a6f47,
small grass tufts. SEAMLESS tileable 512x512 PNG.
NEGATIVE: see §2 + visible tile edges, repetition lines, large objects (resources
go in separate sprite layer).

─── stone_highlands tile (4 variants) ───
Same template. Palette: slate #7a7c80, dry moss #8a9b8c, dirt patch #8b7355.

─── desert tile (4 variants) ───
Same template. Palette: sand base #c4a574, sand highlight #dec594, dirt #8b7355,
faint death-qi purple #9b6b8b dust.
```

Pipeline: `BiomeTileImporter` auto-wires `Art/Tiles/{biomeId}/*.png` → `BiomeSO.groundTileVariants[]`.

---

## §8 VFX

128×128 transparent BG, hand-painted gouache, isolated single particle/effect.

```
─── hit_flash ─── White circular burst, soft edge, 1 frame
─── damage_popup_bg ─── Red ribbon background for damage number
─── blood_splash ─── Red #8b3a3a splatter, 4 droplet shapes
─── dust_poof ─── Beige #c4a574 puff with curled wisp
─── fire_spark ─── Orange #d4a64a + red #a14040 spark cluster
─── smoke_wisp ─── Grey #5a5d63 with white #c2c4ba highlight, vertical wisp
─── mana_glow ─── Sky qi blue #a8d8ff radial soft glow
─── level_up_halo ─── Gold #d4a64a ring with sparkle dots
─── death_decay ─── Purple #9b6b8b dissipating wisp
─── status_buff_icon ─── Green +arrow up (256x256 single frame)
─── status_debuff_icon ─── Red −arrow down (256x256 single frame)
```

Template:

```
DST-style hand-painted VFX, thick outline, isolated single effect on transparent
background, 128x128 PNG. Subject: <EFFECT>. Palette: <COLORS>. NEGATIVE: see §2 +
multiple effects, frame border.
```

---

## §9 Weather

Particle / overlay PNG, transparent BG.

```
─── rain particle (32x32, single droplet, repeat in particle system)
─── snow particle (32x32, hexagonal flake)
─── fog overlay (1024x1024 seamless, white #f0e8d0 wispy clouds, 40% opacity feel)
─── lightning bolt (256x768, jagged white #f0e8d0 with #a8d8ff inner glow)
─── sun ray (256x768, faint gold #d4a64a vertical beam, 30% opacity)
─── sandstorm overlay (1024x1024 seamless, sand #c4a574 swirling wisps, 60% opacity)
```

---

## §10 Environment props

Same template as §5 resources, top-down 30°, 256×256 transparent BG.

```
─── chest (wood + iron banding, closed lid)
─── lantern (red paper with wood frame, glowing inside)
─── shrine (small stone shrine with offering bowl)
─── banner (cloth banner on pole, red-gold cultivation sigil)
─── signpost (wood post + signboard with carved character)
─── barrel (wood barrel with iron hoops)
─── crate (wood crate with rope handle)
─── broken_stele (cracked stone tablet with worn inscription)
─── tent (cloth tent with center pole)
─── altar (raised stone altar with incense brazier)
```

---

## §11 Cost estimate

| Tool | Per image | Full character set (30 PNG) | Full game (12 char × 30 + 50 resources + 20 items + 12 tiles + 11 VFX + 6 weather + 10 props) |
| --- | --- | --- | --- |
| ChatGPT Image 2.0 (DALL·E 3) | $0.04 | $1.20 | ~$18 |
| Midjourney (yearly $30/mo, ~1000 imgs) | ~$0.03 | ~$0.90 | ~$13 |
| Leonardo (free tier 150 tokens/day) | $0 (slow) | 1 day cap | ~5 days iter |
| Stable Diffusion local (GPU) | $0 + GPU time | hours | weeks |

Recommend: dùng Midjourney (`--cref` style ref consistency) + ChatGPT Image cho parts chi tiết khó. Local SD nếu GPU >= 8 GB VRAM và cần iter nhanh.

---

## §12 Iteration tips

1. **Always gen STYLE-REF first.** Nếu STYLE-REF không đạt DST-look (thiếu outline, thiếu gouache flat), KHÔNG đi tiếp các part. Quay lại tinh chỉnh prompt phần outline + gouache cho tới khi master image là "DST-grade".
2. **Image-to-image > text-only.** Khi gen part, đính kèm STYLE-REF làm `--cref` (Midjourney) / "image guidance" (Leonardo) / "based on this image" (ChatGPT). Out put consistency tăng ~5×.
3. **Lock palette với hex codes.** Mỗi prompt phải list hex chính xác. AI tools tôn trọng hex tốt hơn từ ngữ ("warm grey").
4. **One part at a time.** AI single-prompt KHÔNG gen được 30 PNG một lúc. Loop từng part. Nếu dùng API, write script gọi tuần tự với template § kèm part-specific data.
5. **Trim sau khi save.** Repo có `python3 trim_pngs.py` (trong `tools/`, sẽ add khi cần) hoặc dùng PIL: `getbbox()` + crop. PR #118 đã trim user art bằng `scipy.ndimage.label` để loại stray pixel — có thể dùng làm reference.
6. **Test trong Unity ngay sau drop 5 part đầu** (head + torso + arm × 2 + leg E direction). Nếu pivot sai (head lơ lửng cách torso), tinh chỉnh prompt "top edge horizontal at <pivot>" rồi re-gen. Đừng gen full set 30 PNG rồi mới phát hiện pivot lệch.
7. **Outline thickness scale với canvas.** 1024 canvas → outline 16–24px; 512 canvas → 8–12px; 256 canvas → 4–6px. AI tools đôi khi outline-thin khi canvas nhỏ — khuyến khích gen ở 1024 rồi resize down.
8. **DST quirks worth replicating:**
   - Outline shadow cùng tone với base (không pure black mọi nơi).
   - Belly / underside countershading lighter than back.
   - Eyes = small dot (không phải anime large).
   - Hands = mitten silhouette không có finger detail.
   - Feet = simple shoe/paw shape, không có sole detail.
9. **Common AI failure modes:**
   - "Can't generate isolated body part" → re-prompt với "ISOLATED, NO body, NO ground, transparent background" emphasized.
   - "Always gives smooth shading" → add "GOUACHE FLAT FILLS, NO airbrush, NO smooth gradient" trong negative.
   - "Outline too thin" → add "THICK 16-24px ink black stroke outline" 2 lần trong prompt.
   - "Adds drop shadow" → add "no shadow, no ground, no floor" 2 lần trong negative.
10. **Devin Review tip:** Khi PR dùng art mới gen, attach screenshot in-game so with placeholder DST proportion. Nếu ratio lệch placeholder >10% → re-gen với canvas điều chỉnh để match aspect.

---

## References

- [`Documentation/ART_STYLE.md`](ART_STYLE.md) — original style anchor + Leonardo workflow
- [`Documentation/WORLD_MAP_DESIGN.md`](WORLD_MAP_DESIGN.md) — biome palettes
- [`Assets/_Project/Scripts/Core/PuppetPlaceholderSpec.cs`](../Assets/_Project/Scripts/Core/PuppetPlaceholderSpec.cs) — `RectFor(role)` placeholder sizes (source of truth for aspect ratios)
- [`Assets/_Project/Scripts/Core/CharacterArtSpec.cs`](../Assets/_Project/Scripts/Core/CharacterArtSpec.cs) — `PuppetRole` enum + filename constants
- [`Assets/_Project/Editor/CharacterArtImporter.cs`](../Assets/_Project/Editor/CharacterArtImporter.cs) — auto-PPU import (PR #118: per-role)
- [`Assets/_Project/Editor/BootstrapWizard.cs`](../Assets/_Project/Editor/BootstrapWizard.cs) — `BuildPuppetHierarchy` offsets (head +0.45u above torso, arm ±0.18u shoulder, leg ±0.10u hip, etc.)
- [PR #117](https://github.com/roronoazoroshao369/game/pull/117) — initial player art import (illustration-style, DEPRECATED — replaced by DST-style following this catalog)
- [PR #118](https://github.com/roronoazoroshao369/game/pull/118) — render fix (trim PNG + per-role auto-PPU)
