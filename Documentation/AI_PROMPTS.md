# AI Prompts — Linh Khí Wuxia × DST Master Catalog (v3)

> **Mỗi PNG = 1 prompt block self-contained.** Copy đúng 1 fenced code block, paste vào tool, xong. Không cần ghép §1 + §2 + part — mọi thông tin (style anchor + palette + negative + composition + tool tip) đã inline trong từng block.
>
> **Mỗi entity có 1 MEGA-PROMPT** ở cuối section: 1 prompt → 30 PNG (cho ai dùng batch tool / script multi-output).
>
> **Style lock = "Linh Khí Wuxia" — game's distinctive identity** = DST 4 luật cơ bản + 4 luật signature riêng cho game này (ink-wash outline, tri-color anchor, qi-glow rim, cultural ornament). Xem [§1](#1-linh-khí-wuxia--dst-8-luật).
>
> **Output rig-ready:** PNG drop vào `Art/Characters/{id}/{E|N|S}/{part}.png`. `CharacterArtImporter` tự auto-PPU per role (PR #118). `BootstrapWizard.BuildPuppetHierarchy` ráp parts vào skeleton. `PuppetAnimController` chạy idle/walk/attack/hit/death out-of-the-box. Animations cao cấp DST (eat/sleep/mine/channel/sit) cần thêm code — xem [§5](#5-dst-animation-feature-parity).

---

## Table of Contents

- [§1 Linh Khí Wuxia × DST 8 luật](#1-linh-khí-wuxia--dst-8-luật)
- [§2 Anatomy spec table](#2-anatomy-spec-table)
- [§3 Per-entity blocks](#3-per-entity-blocks)
    - [§3.1 Player — Cultivation Hero](#31-player--cultivation-hero)
    - [§3.2 Wolf — Hung Lang](#32-wolf--hung-lang)
    - [§3.3 FoxSpirit — Linh Hồ](#33-foxspirit--linh-hồ)
    - [§3.4 Rabbit — Linh Thố](#34-rabbit--linh-thố)
    - [§3.5 Boar — Hắc Trư](#35-boar--hắc-trư)
    - [§3.6 DeerSpirit — Linh Lộc](#36-deerspirit--linh-lộc)
    - [§3.7 Boss — Hắc Vương](#37-boss--hắc-vương)
    - [§3.8 Crow — Quạ Đen](#38-crow--quạ-đen)
    - [§3.9 Bat — Dơi Đêm](#39-bat--dơi-đêm)
    - [§3.10 Snake — Thanh Xà](#310-snake--thanh-xà)
    - [§3.11 VendorNPC — Lão Tiên Sinh](#311-vendornpc--lão-tiên-sinh)
    - [§3.12 CompanionNPC — Linh Nhi](#312-companionnpc--linh-nhi)
- [§4 Resources / items / tiles / VFX / weather / props](#4-resources--items--tiles--vfx--weather--props)
- [§5 DST animation feature parity](#5-dst-animation-feature-parity)
- [§6 Cost estimate + iteration tips](#6-cost-estimate--iteration-tips)

---

## §1 Linh Khí Wuxia × DST 8 luật

Style identity của game = **DST puppet rig + Asian Wuxia ink-painting**. Mọi prompt PHẢI tuân 8 luật dưới (đã inline sẵn trong từng PNG block).

### Base 4 — DST mechanics (giữ rig "đẹp" thay vì "ghép patch")

1. **Thick INK BLACK outline 16–24px** @ 1024 canvas (≈ 2 % cạnh dài). Outline che chỗ ráp khớp ⇒ không thấy seams.
2. **Gouache flat painted** — 3–4 tonal stops mỗi surface (light / mid / shadow / outline). KHÔNG smooth airbrush gradient.
3. **Stylized anatomy cường điệu** — head 1.3–1.5× wider than torso, limbs đơn giản hoá thành ovals / rectangles bo tròn.
4. **Neutral T-pose, side-view 90°** (E direction). Pose tĩnh — arm thẳng xuống, leg thẳng đứng, head nhìn thẳng.

### Linh Khí Wuxia signature 4 — game's unique identity

5. **Ink-wash brushstroke outline** — outline NOT pure `#000` solid. Slight calligraphy ink-wash texture (Asian sumi-e feel), thickness varies subtly along stroke (1.5–2.5× variance), edge slightly feathered. Tone is `#1a1a1a` warm-tinted, NOT cold blue-black.
6. **Cinnabar–Jade–Cream tri-color anchor** — every entity must contain at least 3 of: cinnabar `#8b3a3a` (red, hostile / blood / cultivator inner energy), jade `#6b8e62` (green, life qi / spirit / vegetation), cream `#e8d5a6` (light, skin / cloth / bone). Other palette colors layer on top.
7. **Qi-glow rim light** — faint colored rim on silhouette edge (5 % intensity, NOT flashy). Color depends on entity:
    - Hero / NPC ally: jade `#a8c69b` faint
    - Hostile mob: cinnabar `#a14040` faint
    - Boss / cursed: death qi purple `#9b6b8b` faint
    - Spirit / mystical: spirit qi blue `#a8d8ff` faint
8. **Cultural ornament accent** — every humanoid has at least 1 wuxia detail: jade pendant, silk ribbon, calligraphy talisman, cloud sigil on robe, qi-flow embroidery. Mob has at least 1 fantasy hint: glowing eye, qi mist around antler/tail/wing, bone marker.

### What this gives you over plain DST

- **Recognizable as YOUR game** — không nhìn như Don't Starve clone (cinnabar-jade-cream palette + ink-wash + qi-glow là signature riêng).
- **Style consistency across entities** — cùng 8 luật ⇒ Wolf đứng cạnh Player nhìn như chung 1 universe.
- **Future-proof for animation richness** — exaggerated anatomy + isolated parts + cultural ornaments cho phép thêm "ribbon flutter", "jade glow pulse", "qi mist trail" sau mà không phải re-author art.

---

## §2 Anatomy spec table

Tham chiếu: `Assets/_Project/Scripts/Core/PuppetPlaceholderSpec.cs` `RectFor(role)`. Aspect ratio AI gen ra phải gần placeholder ±10 % nếu không rig render lệch.

| Role | Placeholder W×H (px) | World H (u) | Aspect | Pivot | Recommended canvas |
| --- | --- | --- | --- | --- | --- |
| `head` | 40×40 | 0.625 | 1.00 | center | 1024×1024 |
| `torso` | 52×80 | 1.250 | 0.65 | center | 1024×1536 |
| `arm_left` / `arm_right` | 16×56 | 0.875 | 0.29 | top-center (vai) | 600×1800 |
| `forearm_left` / `forearm_right` | 14×44 | 0.688 | 0.32 | top-center (khuỷu) | 540×1620 |
| `leg_left` / `leg_right` | 18×60 | 0.938 | 0.30 | top-center (hông) | 660×1980 |
| `shin_left` / `shin_right` | 16×44 | 0.688 | 0.36 | top-center (gối) | 600×1620 |
| `tail` | 50×18 | 0.281 | 2.78 | left-center (gốc tail) | 1500×540 |
| `wing_left` / `wing_right` | 54×28 | 0.438 | 1.93 | left-center (vai cánh) | 1620×840 |
| `body_seg_1..4` | 38×26 → 26×20 | 0.41 → 0.31 | ~1.5 | left-center | 1140×780 |

**Tool tip glossary** (paste vào prompt từng tool):

- **Midjourney**: `--cref <reference_url> --cw 80 --stylize 100 --ar W:H` (cw = character weight; 80 = strong style match, allow detail variance).
- **Leonardo AI**: upload reference image, set "Image Guidance: Character" with weight 0.85; use Phoenix model + Style Reference v2.
- **ChatGPT-Image (DALL·E 3)**: attach reference, write "Match this image's line weight, palette, gouache flat-fill, and outline texture EXACTLY. Generate the part described below as a NEW isolated PNG."
- **Stable Diffusion (local)**: ControlNet reference_only + IPAdapter Plus FaceID style reference, weight 0.7–0.85.

---

## §3 Per-entity blocks

Mỗi entity có 4 phần (mỗi part PNG là 1 fenced block tự chứa, copy 1 phát = xong):

1. **Concept + palette LOCK** — context ngắn để hiểu entity.
2. **STYLE-REF master prompt** — gen FIRST, save as `{entity}_style_ref.png` để dùng làm image guidance cho 30 part PNGs.
3. **30 self-contained part prompts** (E direction 10 + N direction 10 + S direction 10) — mỗi block đầy đủ 8 luật + palette + composition + negative + tool tip.
4. **MEGA-PROMPT** — 1 prompt yêu cầu AI gen toàn bộ 30 PNG cho entity đó (cho batch tool / script).

---

### §3.1 Player — Cultivation Hero

**Folder:** `Art/Characters/player/`

**Concept:** young male qi-cultivation monk, white robe with gold sash, calm focused expression, ink-black hair tied in topknot bun with cream ribbon trail, jade pendant at chest. Bipedal humanoid puppet (10 parts × 3 dirs = 30 PNG).

**Palette LOCK:**
- Skin: light `#e8d5a6` / mid `#b89968` / shadow `#8a6f47`
- Hair: ink black `#1a1a1a` + gloss highlight `#3a3030`
- Robe: cream `#f0e8d0` / fold `#b89968` / deep fold `#8a6f47`
- Sash: gold `#d4a64a` / shadow `#a08038`
- Pendant: jade `#6b8e62` / glow `#a8c69b`
- Ribbon: cream `#f0e8d0`
- Shoe: brown `#5a4030` / shadow `#3a2820`
- Outline: ink `#1a1a1a` (ink-wash texture)
- Qi-glow rim (Linh Khí signature): jade `#a8c69b` 5 % intensity

#### §3.1.0 STYLE-REF master (gen FIRST, save as `player_style_ref.png`)

```text
Linh Khí Wuxia × DST hand-painted illustration master reference. Klei Don't Starve
Together visual language adapted for Asian wuxia cultivation theme. Ink-wash thick
black outline 16-24px (calligraphy sumi-e texture, NOT pure #000, warm-tinted
#1a1a1a, slight thickness variance and edge feathering). Gouache flat fills with
3-4 tonal stops per surface, NO airbrush gradient, NO soft shading, visible brush
strokes. Stylized exaggerated anatomy: head 1.4x torso width.

Subject: full body T-pose side-view 90° facing right of a young male qi-cultivation
monk. Calm focused expression, almond eye, straight nose, ink-black hair tied in
topknot bun with cream silk ribbon trailing, asymmetric forelock strand falling
forward. Wearing flowing white martial arts robe with gold embroidered sash at
waist, jade pendant on chest, simple oval arms hanging at 90° straight down with
clear space between arm and torso silhouette, simple straight legs slightly spread
(hip-width apart) standing neutral, brown cloth shoes. Robe falls to mid-shin.
Cultural ornament: jade-green cloud sigil embroidered on robe hem.

Palette LOCK: skin #e8d5a6/#b89968/#8a6f47, hair #1a1a1a + gloss #3a3030, robe
#f0e8d0/#b89968/#8a6f47, sash gold #d4a64a/#a08038, pendant jade #6b8e62 + glow
#a8c69b, ribbon #f0e8d0, shoe #5a4030/#3a2820, outline ink #1a1a1a textured.
Linh Khí signature: jade #a8c69b 5% rim glow on silhouette.

Composition: 1024x1536 PNG, isolated single character on FULLY transparent
background, no ground, no shadow, no floor, character vertically centered, T-pose
exact (arms hanging straight at sides with clear gap from torso, legs slightly
spread hip-width, head facing right, shoulders horizontal flat).

Negative: photo-realistic, anime moe, chibi extreme proportions, pixel art, smooth
airbrush, soft shading, ambient occlusion, drop shadow, ground, floor, multiple
subjects, duplicate, mirror, text, caption, watermark, signature, frame, border,
realistic anatomy, dynamic action pose, blur, depth of field, expression beyond
calm, pure-black solid outline, anime sparkle eye, sexualized.

Output: 1024x1536 PNG, transparent BG.
```

#### §3.1.1 East direction (E = side-view 90° facing right)

Each block dưới đây là copy-paste-ready: paste 1 block, attach `player_style_ref.png` làm image guidance, gen 1 PNG, lưu vào `Art/Characters/player/E/{filename}.png`.

```text
=== player/E/head.png === (1/30)

Linh Khí Wuxia × DST hand-painted. Ink-wash thick black outline 16-24px (sumi-e
texture, NOT pure #000, warm #1a1a1a, slight thickness variance), gouache flat
3-4 tonal stops, NO airbrush gradient, visible brush strokes, stylized anatomy.

Subject: ISOLATED HEAD ONLY of young male qi-cultivation monk, side-view 90° facing
right, calm focused expression, almond eye, straight nose, ink-black hair tied in
topknot bun + cream silk ribbon trailing behind, asymmetric forelock strand falling
forward over forehead. Cut clean at jaw line — NO neck, NO shoulders, NO body.
Head ~1.4x torso width (DST chibi-adjacent stylization).

Palette LOCK: skin #e8d5a6/#b89968/#8a6f47, hair #1a1a1a + gloss highlight #3a3030,
ribbon cream #f0e8d0, faint jade #6b8e62 qi rim 5% on silhouette edge, outline ink
#1a1a1a calligraphy-textured.

Composition: 1024x1024 PNG, transparent BG, head centered, jaw line at vertical
70%, topknot bun fits in upper 25%, NO ground, NO shadow.

Negative: photo-realistic, anime moe, chibi extreme, smooth airbrush, soft shading,
drop shadow, ground, multiple subjects, duplicate, text, watermark, frame, neck
below jaw, shoulder, body, finger detail, realistic anatomy, dynamic pose, sparkle
eye, pure-black solid outline, anime moe.

Tool: attach player_style_ref.png. Midjourney --cref <ref_url> --cw 80 --ar 1:1
--stylize 100. Leonardo "Image Guidance: Character" weight 0.85, Phoenix model.
ChatGPT/DALL-E "Match this image's line weight, palette, gouache flat-fill, and
ink-wash outline texture EXACTLY. Generate the new isolated head PNG described."
```

```text
=== player/E/torso.png === (2/30)

Linh Khí Wuxia × DST hand-painted. Ink-wash thick black outline 16-24px, gouache
flat 3-4 tonal stops, NO airbrush, visible brush strokes, stylized anatomy.

Subject: ISOLATED TORSO ONLY of young male cultivator, side-view 90° facing right,
neutral standing pose. Wearing flowing white martial arts robe with gold embroidered
sash at waist, jade pendant on chest, jade-green cloud sigil on robe hem (Linh Khí
cultural ornament). Top edge clean horizontal at collar/shoulder line (puppet rig
pivot), bottom edge at hip line. NO head, NO arms, NO legs visible.

Palette LOCK: robe #f0e8d0/#b89968/#8a6f47, sash gold #d4a64a/#a08038, pendant jade
#6b8e62 + glow #a8c69b, sigil jade #a8c69b, outline ink #1a1a1a textured. Jade qi
rim 5%.

Composition: 1024x1536 PNG (vertical), transparent BG, torso centered, top edge
horizontal at shoulder (no neck/head extending up), bottom at hip (no leg extending
down), NO ground.

Negative: photo-realistic, anime moe, chibi extreme, smooth airbrush, drop shadow,
ground, multiple subjects, text, watermark, frame, head, neck, arms, hands, legs,
finger detail, realistic anatomy, dynamic pose, pure-black outline.

Tool: attach player_style_ref.png. Midjourney --cref <ref_url> --cw 80 --ar 2:3
--stylize 100. Leonardo "Image Guidance: Character" 0.85. ChatGPT "match line/
palette/outline of reference exactly, isolate torso only".
```

```text
=== player/E/arm_left.png === (3/30)

Linh Khí Wuxia × DST hand-painted. Ink-wash thick outline 16-24px, gouache flat
3-4 stops, NO airbrush, visible brush strokes.

Subject: ISOLATED LEFT ARM ONLY of cultivator, side-view 90° facing right, hanging
straight down at 90° in neutral T-pose. Draped white robe sleeve, simple oval limb
silhouette (DST style — NO muscle definition), mitten-hand silhouette at bottom
(no individual finger lines). Top edge clean horizontal at shoulder pivot, bottom
at wrist (NO forearm — separate part). Cuff visible at wrist with subtle gold trim.

Palette LOCK: sleeve #f0e8d0/#b89968/#8a6f47, hand mitten skin #e8d5a6/#b89968,
gold cuff trim #d4a64a, outline ink #1a1a1a textured. Jade qi rim 5%.

Composition: 600x1800 PNG (tall narrow), transparent BG, arm centered, top edge
horizontal at shoulder pivot (no body above), bottom at wrist (NO forearm/hand
fingers detail).

Negative: photo-realistic, anime moe, smooth airbrush, drop shadow, ground, body,
torso, head, leg, both arms, individual finger lines, realistic muscle, dynamic
pose, attached body, text, watermark, frame, pure-black outline.

Tool: attach player_style_ref.png. Midjourney --cref <ref_url> --cw 80 --ar 1:3
--stylize 100. Leonardo "Image Guidance: Character" 0.85. ChatGPT "isolate left
arm only, match reference line/palette".
```

```text
=== player/E/arm_right.png === (4/30)

Linh Khí Wuxia × DST hand-painted. Ink-wash thick outline 16-24px, gouache flat
3-4 stops, NO airbrush.

SHORTCUT: open arm_left.png in Photopea/GIMP/Krita → Image → Transform → Flip
Horizontal → Save As arm_right.png. Done in 5 seconds.

OR generate fresh (recommended for slight asymmetric variation that reads more
hand-painted-feel in animation):

Subject: ISOLATED RIGHT ARM ONLY of cultivator, side-view 90° facing right (so this
is the BACK arm — hidden behind torso in scene; rendered with sortingOrder behind
torso). Hanging straight down 90° neutral T-pose, draped white robe sleeve, mitten
hand at bottom, gold cuff trim. Top edge horizontal at shoulder pivot, bottom at
wrist.

Palette LOCK: same as arm_left — sleeve #f0e8d0/#b89968/#8a6f47, hand #e8d5a6/
#b89968, cuff #d4a64a, outline #1a1a1a textured. Jade qi rim 5%.

Composition: 600x1800 PNG, transparent BG, arm centered, top horizontal at shoulder.

Negative: same as arm_left.

Tool: same as arm_left.
```

```text
=== player/E/forearm_left.png === (5/30)

Linh Khí Wuxia × DST hand-painted. Ink-wash thick outline 16-24px, gouache flat
3-4 stops, NO airbrush, stylized anatomy.

Subject: ISOLATED LEFT FOREARM ONLY of cultivator, side-view 90° facing right.
From elbow top → fingertip bottom. White robe sleeve cuff visible at top, mitten-
style hand silhouette at bottom (no individual fingers, just rounded shape with
slight thumb hint). Top edge clean horizontal at elbow pivot. Limb is simple oval
narrowing slightly to wrist then bulging to mitten hand.

Palette LOCK: sleeve cuff #f0e8d0/#b89968, gold cuff trim #d4a64a thin band, hand
mitten #e8d5a6/#b89968 with subtle shadow at thumb crease, outline ink #1a1a1a
textured. Jade qi rim 5%.

Composition: 540x1620 PNG (tall narrow), transparent BG, forearm centered, top
edge horizontal at elbow pivot, bottom at fingertip.

Negative: photo-realistic, anime moe, smooth airbrush, drop shadow, ground, upper
arm, body, individual finger lines, realistic hand, claw, weapon in hand, text,
watermark, frame, pure-black outline.

Tool: attach player_style_ref.png. Midjourney --cref <ref_url> --cw 80 --ar 1:3
--stylize 100. Leonardo "Image Guidance: Character" 0.85. ChatGPT "isolate forearm
only, mitten hand, match reference exactly".
```

```text
=== player/E/forearm_right.png === (6/30)

SHORTCUT: flip forearm_left.png horizontally in Photopea/GIMP. Done.

OR generate fresh: same prompt as forearm_left.png, replace "LEFT" → "RIGHT". This
will be the back-side forearm rendered with sortingOrder behind torso in scene.

Palette + composition + negative + tool: same as forearm_left.
```

```text
=== player/E/leg_left.png === (7/30)

Linh Khí Wuxia × DST hand-painted. Ink-wash thick outline 16-24px, gouache flat
3-4 stops, NO airbrush, stylized anatomy.

Subject: ISOLATED LEFT LEG (UPPER, hip → knee) ONLY of cultivator, side-view 90°
facing right. White robe trouser pant flowing straight down, simple cylindrical
shape (DST style — NO realistic thigh muscle), thigh narrowing slightly to knee.
Top edge clean horizontal at hip pivot, bottom at knee. NO body, NO shin/foot.

Palette LOCK: trouser #f0e8d0/#b89968/#8a6f47, optional jade #6b8e62 ankle-band
hint near knee, outline ink #1a1a1a textured. Jade qi rim 5%.

Composition: 660x1980 PNG (tall narrow), transparent BG, leg centered, top edge
horizontal at hip pivot, bottom at knee (NO shin/foot — separate part).

Negative: photo-realistic, anime moe, smooth airbrush, drop shadow, ground, body,
shin, foot, both legs, realistic muscle, dynamic pose, text, watermark, frame,
pure-black outline.

Tool: attach player_style_ref.png. Midjourney --cref <ref_url> --cw 80 --ar 1:3
--stylize 100. Leonardo "Image Guidance: Character" 0.85. ChatGPT "isolate upper
left leg only, match reference exactly".
```

```text
=== player/E/leg_right.png === (8/30)

SHORTCUT: flip leg_left.png horizontally in Photopea/GIMP. Done.

OR generate fresh: same prompt as leg_left.png, replace "LEFT" → "RIGHT". Back-side
leg rendered with sortingOrder behind torso.

Palette + composition + negative + tool: same as leg_left.
```

```text
=== player/E/shin_left.png === (9/30)

Linh Khí Wuxia × DST hand-painted. Ink-wash thick outline 16-24px, gouache flat
3-4 stops, NO airbrush, stylized anatomy.

Subject: ISOLATED LEFT SHIN+FOOT ONLY of cultivator, side-view 90° facing right.
From knee top → toe bottom. White trouser hem at top with subtle gold trim band,
simple cylindrical shin narrowing slightly, brown cloth-wrapped shoe silhouette
at bottom (DST style — no laces, no sole detail, mitten foot shape with slight
toe-bulge). Top edge clean horizontal at knee pivot.

Palette LOCK: trouser hem #f0e8d0/#b89968, gold trim #d4a64a thin band, shoe
brown #5a4030/#3a2820, outline ink #1a1a1a textured. Jade qi rim 5%.

Composition: 600x1620 PNG (tall narrow), transparent BG, shin+foot centered, top
edge horizontal at knee pivot, bottom at toe (foot just touching imaginary ground).

Negative: photo-realistic, anime moe, smooth airbrush, drop shadow, ground, thigh,
body, both shins, realistic shoe sole, lace detail, individual toes, dynamic pose,
text, watermark, frame, pure-black outline.

Tool: attach player_style_ref.png. Midjourney --cref <ref_url> --cw 80 --ar 1:3
--stylize 100. Leonardo "Image Guidance: Character" 0.85. ChatGPT "isolate shin+
foot only, mitten shoe, match reference exactly".
```

```text
=== player/E/shin_right.png === (10/30)

SHORTCUT: flip shin_left.png horizontally in Photopea/GIMP. Done.

OR generate fresh: same prompt as shin_left.png, replace "LEFT" → "RIGHT". Back-
side shin rendered with sortingOrder behind torso.

Palette + composition + negative + tool: same as shin_left.
```

#### §3.1.2 North direction (N = back-view, walking away from camera)

```text
=== player/N/head.png === (11/30)

Linh Khí Wuxia × DST hand-painted. Ink-wash thick outline 16-24px, gouache flat
3-4 stops, NO airbrush, stylized anatomy.

Subject: ISOLATED HEAD ONLY, BACK-VIEW (back of skull facing camera). NO face,
NO eyes, NO mouth visible. Just back-of-head ink-black hair tied in topknot bun
with cream silk ribbon trailing visible from rear, neck stub at jaw cut clean.
Hair has slight gloss highlight on crown, asymmetric forelock NOT visible from
back.

Palette LOCK: hair #1a1a1a + gloss highlight #3a3030, ribbon cream #f0e8d0, neck
skin stub #e8d5a6/#b89968, outline ink #1a1a1a textured. Jade qi rim 5%.

Composition: 1024x1024 PNG, transparent BG, head centered, jaw at vertical 70%,
topknot bun upper 25%.

Negative: face, eyes, nose, mouth, profile, side-view, photo-realistic, anime moe,
chibi extreme, smooth airbrush, drop shadow, ground, multiple subjects, text,
watermark, frame, body, dynamic pose, pure-black outline.

Tool: attach player_style_ref.png. Midjourney --cref <ref_url> --cw 80 --ar 1:1
--stylize 100 + prompt "back of head". Leonardo "Image Guidance: Character" 0.85.
ChatGPT "back-of-head view, no face visible, match reference style".
```

```text
=== player/N/torso.png === (12/30)

Linh Khí Wuxia × DST hand-painted. Ink-wash thick outline 16-24px, gouache flat
3-4 stops.

Subject: ISOLATED TORSO ONLY, BACK-VIEW. Robe back panel visible, gold sash bow
at lower back (decorative knot), cloud sigil embroidered on lower back hem,
shoulders horizontal flat (puppet pivot). NO front pendant, NO arms, NO head.

Palette LOCK: robe back #f0e8d0/#b89968/#8a6f47, sash bow gold #d4a64a/#a08038,
sigil jade #a8c69b, outline ink #1a1a1a textured. Jade qi rim 5%.

Composition: 1024x1536 PNG, transparent BG, torso centered, top horizontal at
shoulder, bottom at hip.

Negative: face, side-view, front pendant, arms, hands, legs, head, photo-realistic,
anime moe, smooth airbrush, drop shadow, ground, multiple subjects, text, frame,
dynamic pose, pure-black outline.

Tool: attach player_style_ref.png. Midjourney --cref --cw 80 --ar 2:3 --stylize 100
+ prompt "back view of robe". Leonardo "Image Guidance: Character" 0.85. ChatGPT
"back-view torso, sash bow visible, match reference".
```

```text
=== player/N/arm_left.png === (13/30)

Linh Khí Wuxia × DST hand-painted. Ink-wash thick outline 16-24px, gouache flat,
NO airbrush.

Subject: ISOLATED LEFT ARM (back-view), hanging straight down 90°. From back the
left arm is on viewer's RIGHT side. Draped white robe sleeve, mitten hand at
bottom, gold cuff trim. Top horizontal at shoulder pivot.

Palette LOCK: sleeve #f0e8d0/#b89968/#8a6f47, hand #e8d5a6/#b89968, cuff #d4a64a,
outline #1a1a1a textured. Jade qi rim 5%.

Composition: 600x1800 PNG, transparent BG, arm centered, top horizontal at shoulder.

Negative: side-view, front, body, torso, head, both arms, finger detail, realistic
muscle, dynamic pose, drop shadow, text, frame, pure-black outline.

Tool: attach player_style_ref.png. --cref --cw 80 --ar 1:3.
```

```text
=== player/N/arm_right.png === (14/30)

SHORTCUT: flip N/arm_left.png horizontally. Done.

OR fresh: same prompt as N/arm_left, "RIGHT" instead of "LEFT". From back this is
viewer's LEFT side.
```

```text
=== player/N/forearm_left.png === (15/30)

Linh Khí Wuxia × DST hand-painted. Same as E/forearm_left but back-view.

Subject: ISOLATED LEFT FOREARM (back-view), elbow → mitten hand bottom. White
sleeve cuff with gold trim, top horizontal at elbow pivot.

Palette + composition + negative + tool: same as E/forearm_left, plus negative
"side-view, front" added.
```

```text
=== player/N/forearm_right.png === (16/30)

SHORTCUT: flip N/forearm_left.png horizontally.
```

```text
=== player/N/leg_left.png === (17/30)

Linh Khí Wuxia × DST hand-painted. Same as E/leg_left but back-view.

Subject: ISOLATED LEFT LEG (UPPER, hip → knee), back-view. White robe trouser back.
Top horizontal at hip pivot.

Palette + composition + negative + tool: same as E/leg_left, plus negative "side-
view, front" added.
```

```text
=== player/N/leg_right.png === (18/30)

SHORTCUT: flip N/leg_left.png horizontally.
```

```text
=== player/N/shin_left.png === (19/30)

Linh Khí Wuxia × DST hand-painted. Same as E/shin_left but back-view.

Subject: ISOLATED LEFT SHIN+FOOT, back-view. Trouser hem + heel-of-shoe visible
(no toe in back-view). Top horizontal at knee pivot.

Palette + composition + negative + tool: same as E/shin_left, plus negative "side-
view, front, toe visible".
```

```text
=== player/N/shin_right.png === (20/30)

SHORTCUT: flip N/shin_left.png horizontally.
```

#### §3.1.3 South direction (S = front-view, walking toward camera)

```text
=== player/S/head.png === (21/30)

Linh Khí Wuxia × DST hand-painted. Ink-wash thick outline 16-24px, gouache flat
3-4 stops, NO airbrush, stylized anatomy.

Subject: ISOLATED HEAD ONLY, FRONT-VIEW (face directly at camera). Big stylized
head 1.4x torso width (DST chibi-adjacent), two small almond eyes (dot-style not
anime sparkle), straight nose tip, calm closed mouth (small line), ink-black
topknot bun visible at top of head, cream ribbon side-tied, asymmetric forelock
strand falling forward over forehead. Cut clean at jaw line.

Palette LOCK: skin #e8d5a6/#b89968/#8a6f47, hair #1a1a1a + gloss #3a3030, ribbon
#f0e8d0, outline ink #1a1a1a textured. Jade qi rim 5%.

Composition: 1024x1024 PNG, transparent BG, head centered, jaw at 70% vertical,
frontal symmetric (left-right mirror).

Negative: side profile, asymmetric face, smile, frown, anime sparkle eye, anime
moe, chibi extreme, smooth airbrush, drop shadow, ground, multiple subjects, text,
watermark, frame, neck below jaw, body, dynamic pose, pure-black outline.

Tool: attach player_style_ref.png. Midjourney --cref --cw 80 --ar 1:1 --stylize 100
+ prompt "frontal face view". Leonardo "Image Guidance: Character" 0.85. ChatGPT
"front-view face, two eyes visible, match reference style".
```

```text
=== player/S/torso.png === (22/30)

Linh Khí Wuxia × DST hand-painted. Ink-wash thick outline 16-24px, gouache flat.

Subject: ISOLATED TORSO ONLY, FRONT-VIEW. White robe with V-collar opening, gold
sash horizontal across waist, jade pendant centered on chest hanging from cord,
jade-green cloud sigil embroidered on robe hem center. Shoulders horizontal flat
(puppet pivot). NO head, NO arms, NO legs.

Palette LOCK: robe #f0e8d0/#b89968/#8a6f47, sash gold #d4a64a/#a08038, pendant
jade #6b8e62 + glow #a8c69b, sigil #a8c69b, outline ink #1a1a1a textured. Jade qi
rim 5%.

Composition: 1024x1536 PNG, transparent BG, torso centered frontal symmetric, top
horizontal at shoulder, bottom at hip.

Negative: side profile, back-view, head, arms, legs, anime moe, smooth airbrush,
drop shadow, ground, multiple subjects, text, watermark, frame, dynamic pose,
pure-black outline.

Tool: attach player_style_ref.png. --cref --cw 80 --ar 2:3 --stylize 100 + prompt
"frontal torso view, V-collar". Leonardo "Image Guidance: Character" 0.85.
```

```text
=== player/S/arm_left.png === (23/30)

Linh Khí Wuxia × DST hand-painted. Same as E/arm_left but front-view.

Subject: ISOLATED LEFT ARM (front-view, viewer's left side), hanging straight down
90°, draped sleeve, mitten hand, gold cuff. Top horizontal at shoulder.

Palette + composition + negative + tool: same as E/arm_left, plus negative "side-
profile, back-view".
```

```text
=== player/S/arm_right.png === (24/30)

SHORTCUT: flip S/arm_left.png horizontally.
```

```text
=== player/S/forearm_left.png === (25/30)

Same as E/forearm_left, front-view. Negative add "side-profile, back-view".
```

```text
=== player/S/forearm_right.png === (26/30)

SHORTCUT: flip S/forearm_left.png horizontally.
```

```text
=== player/S/leg_left.png === (27/30)

Same as E/leg_left, front-view. Trouser straight, frontal. Negative add "side-
profile, back-view".
```

```text
=== player/S/leg_right.png === (28/30)

SHORTCUT: flip S/leg_left.png horizontally.
```

```text
=== player/S/shin_left.png === (29/30)

Same as E/shin_left, front-view. Shoe with toe visible at bottom front, frontal
view of trouser hem. Negative add "side-profile, back-view, heel only".
```

```text
=== player/S/shin_right.png === (30/30)

SHORTCUT: flip S/shin_left.png horizontally.
```

#### §3.1.4 MEGA-PROMPT (1 prompt → 30 PNG, for batch tools / scripts)

```text
You are generating a Linh Khí Wuxia × DST asset pack for the "player" character —
a young male qi-cultivation monk. Output 30 individual PNG files following these
strict rules.

GLOBAL STYLE (apply to ALL 30 PNGs):
- Linh Khí Wuxia × DST hand-painted (Klei Don't Starve language + Asian wuxia
  cultivation theme). Ink-wash thick black outline 16-24px (calligraphy sumi-e
  texture, NOT pure #000, warm #1a1a1a tinted, slight thickness variance and edge
  feathering). Gouache flat fills 3-4 tonal stops per surface, NO airbrush, NO soft
  shading, visible brush strokes. Stylized exaggerated anatomy: head 1.4x torso
  width, simple oval limbs, mitten hands and feet (no finger/toe detail).
- PALETTE LOCK (use ONLY these): skin #e8d5a6/#b89968/#8a6f47, hair #1a1a1a +
  gloss #3a3030, robe #f0e8d0/#b89968/#8a6f47, sash gold #d4a64a/#a08038, pendant
  jade #6b8e62 + glow #a8c69b, sigil jade #a8c69b, ribbon cream #f0e8d0, shoe
  brown #5a4030/#3a2820, outline ink #1a1a1a textured. Linh Khí signature: jade
  #a8c69b 5% rim glow on every silhouette.
- Each PNG is ISOLATED single body part on FULLY transparent background, no ground,
  no shadow, no floor, no other subject.
- Each PNG has clean horizontal top edge at the puppet pivot point (shoulder for
  arm, elbow for forearm, hip for leg, knee for shin, jaw for head, collar for
  torso).

FILES TO GENERATE (30 total):

E direction (side-view 90° facing right):
 1. player/E/head.png — 1024x1024 — head only, jaw cut, topknot bun, cream ribbon,
    asymmetric forelock falling forward, side-view profile.
 2. player/E/torso.png — 1024x1536 — torso shoulder→hip, robe with gold sash, jade
    pendant, cloud sigil hem.
 3. player/E/arm_left.png — 600x1800 — arm shoulder→wrist, sleeve hanging straight,
    mitten hand, gold cuff trim.
 4. player/E/arm_right.png — 600x1800 — mirror of #3.
 5. player/E/forearm_left.png — 540x1620 — elbow→fingertip, sleeve cuff, mitten.
 6. player/E/forearm_right.png — 540x1620 — mirror of #5.
 7. player/E/leg_left.png — 660x1980 — hip→knee, trouser straight.
 8. player/E/leg_right.png — 660x1980 — mirror of #7.
 9. player/E/shin_left.png — 600x1620 — knee→toe, trouser hem + cloth shoe.
10. player/E/shin_right.png — 600x1620 — mirror of #9.

N direction (back-view, walking away):
11. player/N/head.png — 1024x1024 — back of skull, NO face, topknot + ribbon rear.
12. player/N/torso.png — 1024x1536 — robe back panel + sash bow at lower back +
    cloud sigil hem.
13. player/N/arm_left.png — 600x1800 — back-view sleeve hanging.
14. player/N/arm_right.png — 600x1800 — mirror of #13.
15. player/N/forearm_left.png — 540x1620 — back-view forearm.
16. player/N/forearm_right.png — 540x1620 — mirror of #15.
17. player/N/leg_left.png — 660x1980 — back-view trouser leg.
18. player/N/leg_right.png — 660x1980 — mirror of #17.
19. player/N/shin_left.png — 600x1620 — back-view, heel of shoe visible.
20. player/N/shin_right.png — 600x1620 — mirror of #19.

S direction (front-view, walking toward camera):
21. player/S/head.png — 1024x1024 — frontal face, two almond eyes, calm mouth,
    topknot at top, asymmetric forelock.
22. player/S/torso.png — 1024x1536 — V-collar robe, gold sash, jade pendant
    centered on chest, sigil hem.
23. player/S/arm_left.png — 600x1800 — front-view, viewer's left.
24. player/S/arm_right.png — 600x1800 — mirror of #23.
25. player/S/forearm_left.png — 540x1620 — front-view forearm.
26. player/S/forearm_right.png — 540x1620 — mirror of #25.
27. player/S/leg_left.png — 660x1980 — front-view trouser.
28. player/S/leg_right.png — 660x1980 — mirror of #27.
29. player/S/shin_left.png — 600x1620 — front-view shoe, toe visible.
30. player/S/shin_right.png — 600x1620 — mirror of #29.

GLOBAL NEGATIVE: photo-realistic, anime moe, chibi extreme proportions, pixel art,
smooth airbrush gradient, soft shading, ambient occlusion, drop shadow, cast shadow,
ground, floor, dirt, grass, water, multiple subjects per file, duplicate, mirror
artifact, text, caption, watermark, signature, logo, frame, border, grid lines,
UI elements, lens flare, depth of field, realistic anatomy, dynamic action pose,
expression beyond calm, pure-black solid outline (#000), anime sparkle eye,
sexualized, attached body parts (must be ISOLATED single part per file).

OUTPUT: 30 separate PNG files, each transparent background, exact filenames as
listed above. Maintain identical character identity across all 30 (same face shape
in S/head.png as in player_style_ref.png, same fold pattern on robe in E/torso.png
as in N/torso.png, etc.).
```

---

### §3.2 Wolf — Hung Lang

**Folder:** `Art/Characters/wolf/`

**Concept:** snarling Hung Lang grey wolf with cinnabar inner-eye glow, slate fur, exposed white fangs. Quadruped puppet rigged as bipedal (front legs = arm+forearm, hind legs = leg+shin) — 11 parts × 3 dirs = 33 PNG (extra: tail E only, optional N/S).

**Palette LOCK:**
- Fur: slate `#7a7c80` / shadow `#5a5d63` / highlight `#a3a5a8`
- Belly: cream `#e8d5a6` (countershading)
- Eye: cinnabar glow `#a14040` / iris amber `#d4a64a`
- Fang: bone `#c2c4ba`
- Paw pad: ink `#1a1a1a`
- Outline: ink `#1a1a1a` textured
- Qi-glow rim (Linh Khí signature for hostile mob): cinnabar `#a14040` 5 %

#### §3.2.0 STYLE-REF master (gen FIRST, save as `wolf_style_ref.png`)

```text
Linh Khí Wuxia × DST hand-painted master reference for "Wolf" hostile mob.
Klei Don't Starve language + Asian wuxia. Ink-wash thick black outline 16-24px
(sumi-e calligraphy texture, NOT pure #000, warm #1a1a1a, slight variance).
Gouache flat 3-4 tonal stops per surface, NO airbrush, visible brush strokes.

Subject: full body side-view 90° of a snarling Hung Lang grey wolf, slate-grey
fur with cinnabar inner-eye glow, white fangs slightly bared in snarl. Standing
on all 4 legs but stylized for puppet rig: front pair of legs straight down (=
arm + forearm slot), hind pair (= leg + shin slot), short stub triangular tail.
Big stylized wolf head with prominent snout, ears pricked up, neck thick. Belly
underside cream countershading.

Palette LOCK: fur #7a7c80/#5a5d63/#a3a5a8, belly #e8d5a6, eye glow cinnabar
#a14040 + iris amber #d4a64a, fang bone #c2c4ba, paw pad #1a1a1a, outline ink
#1a1a1a textured. Linh Khí signature: cinnabar #a14040 5% rim glow on silhouette
(hostile mob signature).

Composition: 1024x1536 PNG, isolated single wolf, transparent BG, no shadow,
T-pose neutral (legs straight, head forward, tail stub straight back, ears up).

Negative: cute fluffy puppy, dog-friendly expression, anime moe, chibi extreme,
smooth airbrush, drop shadow, ground, multiple subjects, text, watermark, frame,
realistic fur texture detail, photo-realistic, dynamic pose, pure-black outline.

Output: 1024x1536 PNG, transparent BG.
```

#### §3.2.1 East direction (E)

```text
=== wolf/E/head.png === (1/33)

Linh Khí Wuxia × DST hand-painted. Ink-wash thick black outline 16-24px (sumi-e
texture, warm #1a1a1a, NOT pure #000). Gouache flat 3-4 stops, NO airbrush,
visible brush strokes, stylized animal anatomy.

Subject: ISOLATED WOLF HEAD ONLY, side-view 90° facing right, snarl expression.
Slate-grey fur, big snout extending sideways to right, ear pricked up, amber-iris
eye with cinnabar inner glow, white fang slightly visible at mouth, mouth slightly
open showing fang tip. Neck stub at bottom cut clean (NO body). Linh Khí cinnabar
qi rim 5% on silhouette.

Palette LOCK: fur #7a7c80/#5a5d63/#a3a5a8, eye glow #a14040 + iris #d4a64a, fang
#c2c4ba, outline ink #1a1a1a textured.

Composition: 1024x1024 PNG (head W ≈ 1.2x H), transparent BG, head centered, neck
stub at bottom 70%, snout extends right.

Negative: neck below stub, body, smile, dog-friendly, multiple heads, photo-
realistic, anime moe, smooth airbrush, drop shadow, ground, text, watermark,
frame, realistic fur, pure-black outline.

Tool: attach wolf_style_ref.png. Midjourney --cref --cw 80 --ar 1:1 --stylize 100.
Leonardo "Image Guidance: Character" 0.85. ChatGPT "snarling wolf head only,
match reference exactly".
```

```text
=== wolf/E/torso.png === (2/33)

Linh Khí Wuxia × DST hand-painted. Ink-wash thick outline 16-24px, gouache flat,
NO airbrush.

Subject: ISOLATED WOLF TORSO ONLY (shoulder line top → hip bottom), side-view 90°
facing right. Slate fur top with cream belly countershading underside. Thick neck
stub at top edge for puppet pivot, hip end at bottom. NO head, NO legs, NO tail.

Palette LOCK: fur #7a7c80/#5a5d63/#a3a5a8, belly #e8d5a6, outline #1a1a1a textured.
Cinnabar qi rim 5%.

Composition: 1024x1280 PNG, transparent BG, torso centered, top horizontal at
shoulder/neck-stub, bottom at hip.

Negative: head, legs, tail, dog-friendly, anime moe, smooth airbrush, drop shadow,
ground, multiple subjects, text, watermark, frame, realistic fur, pure-black
outline.

Tool: attach wolf_style_ref.png. --cref --cw 80 --ar 4:5 --stylize 100. Leonardo
"Image Guidance: Character" 0.85.
```

```text
=== wolf/E/arm_left.png === (3/33)  (= front leg upper, shoulder→elbow)

Linh Khí Wuxia × DST hand-painted. Ink-wash thick outline 16-24px, gouache flat.

Subject: ISOLATED WOLF FRONT LEFT LEG UPPER (shoulder pivot top → elbow bottom),
side-view facing right. Slate fur, simple cylinder shape (DST style — NO realistic
muscle), top edge clean horizontal at shoulder pivot. NO body, NO lower leg, NO
paw.

Palette LOCK: fur #7a7c80/#5a5d63, outline ink #1a1a1a textured. Cinnabar qi rim 5%.

Composition: 600x1800 PNG, transparent BG, leg centered, top horizontal at shoulder.

Negative: body, paw, lower leg, both legs, realistic muscle, anime moe, smooth
airbrush, drop shadow, ground, text, frame, pure-black outline.

Tool: attach wolf_style_ref.png. --cref --cw 80 --ar 1:3.
```

```text
=== wolf/E/arm_right.png === (4/33)
SHORTCUT: flip arm_left.png horizontally. Or re-gen with "RIGHT" replacing "LEFT".
```

```text
=== wolf/E/forearm_left.png === (5/33)  (= front leg lower + paw)

Linh Khí Wuxia × DST hand-painted. Ink-wash thick outline 16-24px, gouache flat.

Subject: ISOLATED WOLF FRONT LEFT LEG LOWER (elbow top → paw bottom), side-view
facing right. Slate fur, simple paw silhouette at bottom (NO individual claw lines,
just rounded shape with subtle pad indication). Top edge clean horizontal at elbow
pivot.

Palette LOCK: fur #7a7c80/#5a5d63, paw pad #1a1a1a hint, outline #1a1a1a textured.
Cinnabar qi rim 5%.

Composition: 540x1620 PNG, transparent BG, lower leg centered.

Negative: upper leg, body, individual claws, realistic paw detail, anime moe,
smooth airbrush, drop shadow, ground, text, frame, pure-black outline.

Tool: attach wolf_style_ref.png. --cref --cw 80 --ar 1:3.
```

```text
=== wolf/E/forearm_right.png === (6/33)
SHORTCUT: flip forearm_left.png horizontally.
```

```text
=== wolf/E/leg_left.png === (7/33)  (= hind leg upper, hip→knee)

Linh Khí Wuxia × DST. Same template as wolf/E/arm_left but for hind leg upper
(hip pivot top → knee bottom). Slate fur, simple cylinder, slight thigh bulge
near hip top (hind leg slightly thicker than front).

Palette + composition + negative + tool: same as wolf/E/arm_left, change ar to
1:3 = 660x1980 PNG. "Hind leg upper" replaces "front leg upper".
```

```text
=== wolf/E/leg_right.png === (8/33)
SHORTCUT: flip leg_left.png horizontally.
```

```text
=== wolf/E/shin_left.png === (9/33)  (= hind leg lower + paw)

Linh Khí Wuxia × DST. Same template as wolf/E/forearm_left but for hind leg lower
(knee top → paw bottom). Slate fur, paw at bottom.

Palette + composition + negative + tool: same as wolf/E/forearm_left, ar 1:3 =
600x1620 PNG.
```

```text
=== wolf/E/shin_right.png === (10/33)
SHORTCUT: flip shin_left.png horizontally.
```

```text
=== wolf/E/tail.png === (11/33)

Linh Khí Wuxia × DST hand-painted. Ink-wash thick outline 16-24px, gouache flat.

Subject: ISOLATED WOLF TAIL ONLY, stub triangular shape, side-view, slate fur top
+ cream tip underside (countershading). Pivot at LEFT-CENTER (gốc tail = nơi nối
hông, will rotate around this point in animation). Tail extends to the RIGHT.

Palette LOCK: fur #7a7c80/#5a5d63, tip cream #e8d5a6, outline #1a1a1a textured.
Cinnabar qi rim 5%.

Composition: 1500x540 PNG (horizontal), transparent BG, tail extending right from
left edge, pivot at left-center.

Negative: body, multiple tails, fluffy fox bushy tail, anime moe, smooth airbrush,
drop shadow, ground, text, frame, pure-black outline.

Tool: attach wolf_style_ref.png. --cref --cw 80 --ar 5:2.
```

#### §3.2.2 North direction (N = back-view)

```text
=== wolf/N/head.png === (12/33)

Linh Khí Wuxia × DST. Same as wolf/E/head but BACK-VIEW (back of wolf head, ears
pricked rear, NO snout/face/eye visible from this angle, just back of skull
with ear backs and neck fur).

Palette + composition + negative + tool: same as wolf/E/head, plus negative
"face, snout, eye, fang, side-profile".
```

```text
=== wolf/N/torso.png === (13/33)
Same as E/torso, back-view — full slate fur top covering, NO belly visible (top-
down rear view of body). Negative: "belly cream, side-profile".
```

```text
=== wolf/N/arm_left.png === (14/33)
SHORTCUT or re-gen wolf/E/arm_left back-view.
```

```text
=== wolf/N/arm_right.png === (15/33) — flip N/arm_left.
=== wolf/N/forearm_left.png === (16/33) — same as E, back-view.
=== wolf/N/forearm_right.png === (17/33) — flip N/forearm_left.
=== wolf/N/leg_left.png === (18/33) — same as E, back-view.
=== wolf/N/leg_right.png === (19/33) — flip N/leg_left.
=== wolf/N/shin_left.png === (20/33) — same as E, back-view.
=== wolf/N/shin_right.png === (21/33) — flip N/shin_left.
=== wolf/N/tail.png === (22/33) — same as E/tail but back-view (small fan triangle
from rear, slate fur, cream tip). Pivot left-center.
```

#### §3.2.3 South direction (S = front-view)

```text
=== wolf/S/head.png === (23/33)

Linh Khí Wuxia × DST. ISOLATED WOLF HEAD, FRONT-VIEW (snout pointing camera, two
amber+cinnabar eyes visible, fangs bared, "looking at player" snarl). Slate fur,
ears up.

Palette + composition + negative + tool: same as wolf/E/head, plus negative
"side-profile, single eye visible".
```

```text
=== wolf/S/torso.png === (24/33)
Front-view — chest cream + slate sides flanking. Negative: "side-profile, back-view".
```

```text
=== wolf/S/arm_left.png === (25/33)  — front-view, viewer's left.
=== wolf/S/arm_right.png === (26/33) — flip S/arm_left.
=== wolf/S/forearm_left.png === (27/33) — front-view forearm.
=== wolf/S/forearm_right.png === (28/33) — flip S/forearm_left.
=== wolf/S/leg_left.png === (29/33) — front-view hind leg upper.
=== wolf/S/leg_right.png === (30/33) — flip S/leg_left.
=== wolf/S/shin_left.png === (31/33) — front-view hind leg lower + paw.
=== wolf/S/shin_right.png === (32/33) — flip S/shin_left.
=== wolf/S/tail.png === (33/33) — SKIP recommended (tail hidden behind body in
front-view; CharacterArtImporter falls back to E/tail.png).
```

#### §3.2.4 MEGA-PROMPT

```text
You are generating a Linh Khí Wuxia × DST asset pack for "wolf" hostile mob —
a snarling Hung Lang grey wolf. Output 33 individual PNG files (or 32 if S/tail
skipped).

GLOBAL STYLE: Linh Khí Wuxia × DST hand-painted, Klei Don't Starve language +
Asian wuxia. Ink-wash thick black outline 16-24px (sumi-e calligraphy, NOT pure
#000, warm #1a1a1a). Gouache flat 3-4 stops, NO airbrush, visible brush strokes.
Stylized animal anatomy — DST proportion (head 1.2x body width, simple cylinder
limbs, paw silhouettes without claw detail).

PALETTE LOCK: fur #7a7c80/#5a5d63/#a3a5a8, belly #e8d5a6, eye glow cinnabar
#a14040 + iris amber #d4a64a, fang bone #c2c4ba, paw pad #1a1a1a, outline ink
#1a1a1a textured. Linh Khí signature: cinnabar #a14040 5% rim glow on silhouette
(hostile mob signature).

ISOLATION: each PNG single body part on FULLY transparent background, no ground,
no shadow, no other subject. Top edge horizontal at puppet pivot.

FILES (33 total):
E (side-view 90° facing right): head 1024x1024, torso 1024x1280, arm_left 600x1800,
arm_right (mirror), forearm_left 540x1620, forearm_right (mirror), leg_left
660x1980, leg_right (mirror), shin_left 600x1620, shin_right (mirror), tail
1500x540.
N (back-view): same 11 parts but back-of-skull head, full-fur back torso, back-
view limbs, back-view tail.
S (front-view): same 11 parts but frontal snout head with 2 eyes, chest+flank
torso, front-view limbs, S/tail SKIP (hidden behind body).

GLOBAL NEGATIVE: cute fluffy puppy, dog-friendly expression, anime moe, chibi
extreme, photo-realistic, pixel art, smooth airbrush, soft shading, drop shadow,
ground, floor, multiple subjects per file, duplicate, text, watermark, signature,
frame, border, realistic fur texture, dynamic pose, pure-black solid outline,
attached body parts.

Maintain identical wolf identity across all 33: same fur tone, same eye glow,
same fang shape, same proportional anatomy.
```

---

### §3.3 FoxSpirit — Linh Hồ

**Folder:** `Art/Characters/fox_spirit/`

**Concept:** Linh Hồ nine-tail fox spirit, cinnabar-orange fur with cream belly + cream-white tail tip, faint jade-blue spirit qi mist around tail tip + ear tips, golden glaring eyes. Quadruped puppet (same skeleton as Wolf, 11 parts × 3 dirs).

**Palette LOCK:**
- Fur: cinnabar `#a14040` / deep `#6b2828` / highlight `#d46c5c`
- Belly: cream `#e8d5a6`
- Tail tip: cream `#f0e8d0`
- Eye: gold `#d4a64a` / glow `#f4d984`
- Spirit qi mist: spirit blue `#a8d8ff` (signature for spirit/mystical entity)
- Outline: ink `#1a1a1a` textured

#### §3.3.0 STYLE-REF master (`fox_spirit_style_ref.png`)

```text
Linh Khí Wuxia × DST hand-painted master reference for "FoxSpirit" mystical mob.
Ink-wash thick black outline 16-24px, gouache flat 3-4 stops, NO airbrush.

Subject: full body side-view 90° of a Linh Hồ spirit fox, slim elegant build,
cinnabar-orange fur, big bushy tail with cream-white tip + faint spirit-blue qi
mist on tail tip + ear tips (5% glow, NOT flashy), golden glaring eyes, cream
belly countershading, 4 simple cylindrical legs, T-pose neutral (legs straight
down, tail straight back, head forward, ears pricked).

Palette LOCK: fur #a14040/#6b2828/#d46c5c, belly #e8d5a6, tail tip #f0e8d0, qi
mist #a8d8ff (faint hint only), eye gold #d4a64a + glow #f4d984, outline ink
#1a1a1a textured. Spirit qi rim 5% (mystical entity signature).

Composition: 1024x1536 PNG, isolated single fox, transparent BG.

Negative: cute kitsune anime, multiple tails (only ONE rendered as single sprite —
9-tail mystique conveyed via VFX, not art), realistic fur texture, photo-realistic,
chibi extreme, anime moe, smooth airbrush, drop shadow, ground, text, watermark,
frame, dynamic pose, pure-black outline.
```

#### §3.3.1 East direction parts

Same template as Wolf §3.2.1 (11 parts: head, torso, arm×2, forearm×2, leg×2, shin×2, tail), substitute palette + spirit-blue qi rim.

```text
=== fox_spirit/E/head.png === (1/33)

Linh Khí Wuxia × DST. Ink-wash thick outline 16-24px, gouache flat.

Subject: ISOLATED FOX HEAD ONLY, side-view 90° facing right, slim graceful muzzle,
ear pricked up with faint spirit-blue qi mist on ear tip, golden eye glaring,
small white fang hint at mouth. Cinnabar fur with cream cheek/jaw underside.

Palette LOCK: fur #a14040/#6b2828/#d46c5c, cheek cream #e8d5a6, eye gold #d4a64a +
glow #f4d984, ear-tip qi #a8d8ff faint, outline #1a1a1a textured.

Composition: 1024x1024 PNG, transparent BG, head centered, snout right.

Negative: cute anime kitsune, multiple eyes/heads, realistic fur, anime moe, smooth
airbrush, drop shadow, ground, text, frame, body, pure-black outline.

Tool: attach fox_spirit_style_ref.png. --cref --cw 80 --ar 1:1.
```

```text
=== fox_spirit/E/torso.png === (2/33)
Same template as wolf/E/torso, palette swap to fox cinnabar + cream belly.
Composition 1024x1280. Spirit qi rim 5%.
```

```text
=== fox_spirit/E/arm_left.png === (3/33)  (= front leg upper)
Same template as wolf/E/arm_left, palette swap. 600x1800.
=== fox_spirit/E/arm_right.png === (4/33) — flip arm_left.
=== fox_spirit/E/forearm_left.png === (5/33)  (= front leg lower + paw)
Same template as wolf/E/forearm_left, palette swap. Paw cream-white toe hint at
bottom (fox often has white-tipped paws). 540x1620.
=== fox_spirit/E/forearm_right.png === (6/33) — flip.
=== fox_spirit/E/leg_left.png === (7/33)  (= hind leg upper) — 660x1980.
=== fox_spirit/E/leg_right.png === (8/33) — flip.
=== fox_spirit/E/shin_left.png === (9/33)  (= hind leg lower + paw) — 600x1620.
=== fox_spirit/E/shin_right.png === (10/33) — flip.
```

```text
=== fox_spirit/E/tail.png === (11/33)

Linh Khí Wuxia × DST. Ink-wash thick outline 16-24px, gouache flat.

Subject: ISOLATED FOX BUSHY TAIL ONLY (longer + thicker than wolf stub), cinnabar
fur with cream-white tip + faint spirit-blue qi mist swirling around tip (5% glow,
NOT flashy). Pivot at LEFT-CENTER (gốc tail). Tail extends to RIGHT, slight curve.

Palette LOCK: fur #a14040/#6b2828/#d46c5c, tip #f0e8d0, qi mist #a8d8ff, outline
#1a1a1a textured.

Composition: 1700x600 PNG (longer than wolf), transparent BG, pivot left-center.

Negative: body, nine tails, anime kitsune cute, realistic fur, smooth airbrush,
drop shadow, ground, text, frame, pure-black outline.

Tool: attach fox_spirit_style_ref.png. --cref --cw 80 --ar 17:6.
```

#### §3.3.2 N + S directions

Same skeleton as wolf §3.2.2 + §3.2.3, substitute palette. N/head = back-view, N/tail = back-view (small fan tail). S/head = front-view (snout to camera, golden glaring eyes), S/tail = SKIP.

#### §3.3.3 MEGA-PROMPT

```text
Generate Linh Khí Wuxia × DST asset pack for "fox_spirit" mystical mob — a Linh
Hồ spirit fox. Output 33 PNGs (or 32 if S/tail skipped).

GLOBAL STYLE: same as wolf MEGA-PROMPT (§3.2.4) — ink-wash thick outline, gouache
flat, stylized animal anatomy.

PALETTE LOCK (different from wolf): fur #a14040/#6b2828/#d46c5c, belly #e8d5a6,
tail tip #f0e8d0, qi mist #a8d8ff (faint 5% on tail tip + ear tips), eye gold
#d4a64a + glow #f4d984, outline ink #1a1a1a textured. Spirit qi rim 5% (mystical
entity signature).

FILES: same 33-file structure as wolf (E/N/S × 11 parts) but slimmer/taller fox
proportions, longer bushy tail with qi-mist tip.

GLOBAL NEGATIVE: cute kitsune anime, multiple tails (mystique via VFX), wolf-like
proportions, realistic fur, photo-realistic, anime moe, chibi extreme, smooth
airbrush, drop shadow, ground, multiple subjects, text, watermark, frame, dynamic
pose, pure-black outline.

Maintain identical fox identity across all 33: same cinnabar fur tone, same qi-
mist on tail/ears, same golden eye, same proportional anatomy.
```

---

### §3.4 Rabbit — Linh Thố

**Folder:** `Art/Characters/rabbit/`

**Concept:** Linh Thố nimble forest rabbit, cream-tan fur, big floppy ears, twitchy nose, fluffy cottontail. Peaceful prey mob. Quadruped puppet (small body, 11 parts × 3 dirs).

**Palette LOCK:** fur tan `#c8a878` / shadow `#8a7048` / highlight `#dfc89a` / belly cream `#f0e8d0` / ear inner pink `#d4a890` / eye black dot `#1a1a1a` / cottontail white `#f0e8d0` / outline `#1a1a1a` textured. Jade qi rim 5 % (peaceful/spirit entity signature, like Player).

#### §3.4.0 STYLE-REF (`rabbit_style_ref.png`)

```text
Linh Khí Wuxia × DST hand-painted master reference for "Rabbit" peaceful mob.
Ink-wash thick black outline 16-24px, gouache flat 3-4 stops, NO airbrush.

Subject: full body side-view 90° of a small forest rabbit (Linh Thố), tan fur,
big floppy ears slightly drooping, short oval body, 4 small leg stumps (front
shorter than hind), fluffy white cottontail at rear, twitchy small nose, big
black-dot eye, cream belly. T-pose neutral.

Palette LOCK: fur #c8a878/#8a7048/#dfc89a, belly #f0e8d0, ear inner pink #d4a890,
eye #1a1a1a, cottontail #f0e8d0, outline #1a1a1a textured. Jade qi rim 5%.

Composition: 1024x1024 PNG, isolated, transparent BG.

Negative: Easter cute commercial mascot, anime moe, anthropomorphic, chibi extreme,
photo-realistic, smooth airbrush, drop shadow, ground, multiple subjects, text,
watermark, frame, dynamic pose, pure-black outline.
```

#### §3.4.1–§3.4.3 E/N/S parts (11 parts × 3 dirs = 33 PNG)

Same skeleton as wolf. Cottontail prompt:

```text
=== rabbit/E/tail.png === (11/33)

Linh Khí Wuxia × DST. Ink-wash thick outline 16-24px, gouache flat.

Subject: ISOLATED FLUFFY COTTONTAIL ONLY, white round pom-pom shape with subtle
tan shadow underside, side-view, pivot at LEFT-CENTER (small tail attaches to
rabbit rump). Slightly fluffy edge texture (DST style — soft ink stroke gives
fluff hint, NO realistic fur).

Palette LOCK: cotton #f0e8d0, shadow tan #8a7048 hint, outline #1a1a1a textured.
Jade qi rim 5%.

Composition: 540x540 PNG (small), transparent BG, pivot left-center.

Negative: body, multiple cottontails, fluffy fox bushy tail, anime moe, smooth
airbrush, drop shadow, ground, text, frame, pure-black outline.

Tool: attach rabbit_style_ref.png. --cref --cw 80 --ar 1:1.
```

Other 32 parts: same template as wolf §3.2.1–§3.2.3 with palette swap to rabbit + smaller canvas (rabbit ~70 % wolf size). Limbs are short stumps, not long legs.

#### §3.4.4 MEGA-PROMPT

```text
Generate Linh Khí Wuxia × DST asset pack for "rabbit" peaceful mob — a Linh Thố
forest rabbit. Output 33 PNGs.

GLOBAL STYLE: same as wolf MEGA-PROMPT.

PALETTE LOCK: fur #c8a878/#8a7048/#dfc89a, belly #f0e8d0, ear inner pink #d4a890,
eye #1a1a1a, cottontail #f0e8d0, outline #1a1a1a textured. Jade qi rim 5%
(peaceful/spirit entity signature).

ANATOMY: small rabbit body (~70% wolf scale), big floppy ears, short stub legs
(front shorter than hind), fluffy cottontail. Use smaller canvas — head 720x720,
torso 720x900, limbs proportionally reduced.

FILES: same 33-file structure as wolf, smaller canvas per part. Tail = cottontail
540x540.

GLOBAL NEGATIVE: Easter mascot, anime moe, anthropomorphic, chibi extreme, photo-
realistic, smooth airbrush, drop shadow, ground, text, watermark, frame, dynamic
pose, pure-black outline.

Maintain identical rabbit identity across all 33: same tan fur, same floppy ears,
same fluffy cottontail.
```

---

### §3.5 Boar — Hắc Trư

**Folder:** `Art/Characters/boar/`

**Concept:** wild Hắc Trư boar, dark brown bristly coarse fur with bristle-ridge spine, ivory tusks protruding from lower jaw, heavy stocky body, stub tail, aggressive head-low stance. Aggressive mob.

**Palette LOCK:** fur dark brown `#5a4030` / shadow `#2a1f15` / highlight `#8b6f47` (bristle ridge) / tusk ivory `#d4c8a3` / eye small black `#1a1a1a` / nose dark `#1a1a1a` / outline `#1a1a1a` textured. Cinnabar qi rim 5 % (hostile mob).

#### §3.5.0 STYLE-REF (`boar_style_ref.png`)

```text
Linh Khí Wuxia × DST hand-painted master reference for "Boar" aggressive mob.
Ink-wash thick black outline 16-24px, gouache flat 3-4 stops, NO airbrush.

Subject: full body side-view 90° of a wild Hắc Trư boar, dark brown bristly fur
with prominent bristle-ridge spine running from neck to rump (highlighted lighter
brown), two ivory tusks protruding upward from lower jaw, small black eye, dark
snout, heavy stocky body with short stubby legs, tiny stub tail. T-pose with
slight aggressive stance — head low forward, shoulders hunched.

Palette LOCK: fur #5a4030/#2a1f15/#8b6f47, tusk #d4c8a3, eye/nose #1a1a1a, outline
ink #1a1a1a textured. Cinnabar qi rim 5% (hostile mob).

Composition: 1024x1024 PNG, isolated, transparent BG.

Negative: cute pig pink, smiling, friendly, anime moe, chibi extreme, photo-
realistic, smooth airbrush, drop shadow, ground, multiple subjects, text, watermark,
frame, dynamic pose, pure-black outline.
```

#### §3.5.1–§3.5.3 E/N/S parts

Same template as wolf 11 parts × 3 dirs. Bristle-ridge highlighted on torso top edge. Stub tail very small. Limbs short and thick.

#### §3.5.4 MEGA-PROMPT

```text
Generate Linh Khí Wuxia × DST asset pack for "boar" aggressive mob — Hắc Trư.
Output 33 PNGs.

GLOBAL STYLE: same as wolf MEGA-PROMPT.

PALETTE LOCK: fur #5a4030/#2a1f15/#8b6f47 (bristle ridge highlight), tusk #d4c8a3,
eye/nose #1a1a1a, outline #1a1a1a textured. Cinnabar qi rim 5% (hostile mob).

ANATOMY: stocky body, short thick legs, prominent bristle-ridge spine on torso
top, ivory tusks on head, stub tail.

FILES: same 33-file structure. Head 1024x900 (wider than tall — broad snout +
tusks). Torso 1024x1100 (stockier than wolf).

GLOBAL NEGATIVE: cute pig pink, friendly smile, anime moe, photo-realistic, smooth
airbrush, drop shadow, ground, multiple subjects, text, watermark, frame, dynamic
pose, pure-black outline.

Maintain identical boar identity across all 33: same dark fur, same bristle ridge,
same ivory tusks, same stocky proportions.
```

---

### §3.6 DeerSpirit — Linh Lộc

**Folder:** `Art/Characters/deer_spirit/`

**Concept:** Linh Lộc forest deer spirit, slim graceful build with light brown fur + cream-white spotted flank, large branched antlers with faint jade-green qi glow, alert posture, white belly + tail flick. Mystical peaceful mob.

**Palette LOCK:** fur tan `#a08060` / shadow `#6a4830` / highlight `#d4b896` / spot cream `#e8d5a6` / belly cream `#e8d5a6` / antler base `#c4a574` / antler tip `#a08060` / qi jade `#a8c69b` / eye amber `#d4a64a` / outline `#1a1a1a` textured. Jade qi rim 5 % (spirit mystical signature).

#### §3.6.0 STYLE-REF (`deer_spirit_style_ref.png`)

```text
Linh Khí Wuxia × DST hand-painted master reference for "DeerSpirit" mystical mob.

Subject: full body side-view of a Linh Lộc spirit deer, slim graceful build, 4
long thin legs, large branched antlers (3-4 prong) with faint jade-green qi glow
at antler tips (5%), gentle amber eye, tan fur with cream-white spots scattered
on flank, white belly underside, white short tail flick at rear. T-pose neutral.

Palette LOCK: fur #a08060/#6a4830/#d4b896, spots/belly #e8d5a6, antler #c4a574/
#a08060, qi jade #a8c69b (5% glow on antler tips), eye amber #d4a64a, outline
#1a1a1a textured. Jade qi rim 5%.

Composition: 1024x1536 PNG (taller — antlers extend up), isolated, transparent BG.

Negative: Disney baby Bambi cute, anime moe, chibi extreme, photo-realistic,
realistic antler bone detail, smooth airbrush, drop shadow, ground, multiple
subjects, text, watermark, frame, dynamic pose, pure-black outline.
```

#### §3.6.1–§3.6.3 E/N/S parts

11 parts × 3 dirs. Head canvas 1024x1100 (taller to fit antlers). Limbs long+thin. Tail = small cream flick (tail.png 540x300).

#### §3.6.4 MEGA-PROMPT

```text
Generate Linh Khí Wuxia × DST asset pack for "deer_spirit" mystical mob — Linh
Lộc. Output 33 PNGs.

GLOBAL STYLE + PALETTE: see above.

ANATOMY: slim graceful, long thin legs, branched antlers (3-4 prong) with qi-glow
tips, cream-spotted flank, small white tail flick.

FILES: same 33 structure. Head 1024x1100 (antler extension). Tail 540x300 (small).

GLOBAL NEGATIVE: Disney Bambi, anime moe, chibi, photo-realistic, smooth airbrush,
drop shadow, ground, text, frame, dynamic pose, pure-black outline.

Maintain deer identity across all 33.
```

---

### §3.7 Boss — Hắc Vương

**Folder:** `Art/Characters/boss/`

**Concept:** Black King cursed overlord, towering humanoid (1.5× normal height), dark obsidian armor over withered cultivator body, glowing purple death-qi aura, sharp asymmetric silhouette with shoulder pauldrons, crimson torn cloth at waist, exposed skull-like face under hooded helm, clawed gauntlets. Bipedal puppet (10 parts × 3 dirs).

**Palette LOCK:**
- Armor: obsidian `#1a1a20` / highlight `#3a3a48`
- Crimson cloth: `#8c1923` / deep `#5a0e15`
- Death qi: purple `#9b6b8b` / deep `#6b3a5b`
- Eye glow: purple `#d4a8e0`
- Skull bone: `#c2c4ba`
- Outline: ink `#1a1a1a` textured
- Qi-glow rim (Linh Khí signature for cursed/boss): death qi purple `#9b6b8b` 5 %

#### §3.7.0 STYLE-REF (`boss_style_ref.png`)

```text
Linh Khí Wuxia × DST hand-painted master reference for "Boss" cursed overlord.

Subject: full body T-pose side-view 90° of Hắc Vương cursed overlord. Towering
humanoid 1.5x player height, dark obsidian armor with sharp asymmetric shoulder
pauldrons (left larger than right for menace), crimson torn cloth hanging from
waist, exposed withered skull-like face under hooded helm with one glowing purple
eye visible, faint death-qi purple aura wisp around silhouette (5% glow), heavy
long arms ending in clawed gauntlets (3 talon hint, NOT realistic), armored
greaves on legs.

Palette LOCK: armor #1a1a20/#3a3a48, crimson #8c1923/#5a0e15, qi #9b6b8b/#6b3a5b,
eye glow #d4a8e0, skull #c2c4ba, outline ink #1a1a1a textured. Death qi rim 5%.

Composition: 1024x1536 PNG (taller — boss = 1.5x normal), isolated, transparent BG.

Negative: heroic noble, cute villain, anime style, realistic gore, photo-realistic,
chibi extreme, smooth airbrush, drop shadow, ground, multiple subjects, text,
watermark, frame, dynamic pose, pure-black outline.
```

#### §3.7.1–§3.7.3 E/N/S parts (10 parts × 3 dirs = 30 PNG)

Same humanoid skeleton as Player but armored + cursed. Each part should have ink-black outline + obsidian fill + crimson cloth accent + faint death-qi rim. Helm hooded silhouette on head, asymmetric pauldron on left arm (bigger), greaves on shins.

#### §3.7.4 MEGA-PROMPT

```text
Generate Linh Khí Wuxia × DST asset pack for "boss" — Hắc Vương cursed overlord.
Output 30 PNGs (no tail, no wings).

GLOBAL STYLE + PALETTE: see above.

ANATOMY: towering humanoid 1.5x player height, sharp asymmetric pauldron (left
bigger), heavy long arms with clawed gauntlets, armored greaves, hooded helm
with glowing eye.

FILES: same 30-file structure as Player (E/N/S × 10 parts). Bigger canvas — head
1024x1024, torso 1024x1700 (taller), arm 660x1980 (longer + thicker), forearm
600x1800, leg 720x2100, shin 660x1800.

GLOBAL NEGATIVE: heroic noble, cute villain, anime, realistic gore, smooth airbrush,
drop shadow, ground, text, frame, dynamic pose, pure-black outline.
```

---

### §3.8 Crow — Quạ Đen

**Folder:** `Art/Characters/crow/`

**Concept:** glossy black corvid scavenger with yellow beak + golden iris eye + black pupil, sharp tail fan, 3-toe talon. Bipedal flying puppet — REPLACES `arm/forearm` slots with `wing` (no forearm). Has `leg+shin` for landing. 8 parts × 3 dirs = 24 PNG.

**Required parts (per `PuppetPlaceholderSpec.RolesForCharacter` with `includeWings=true`):** head, torso, leg×2, shin×2, wing×2 (NO arm, NO forearm, NO tail).

**Palette LOCK:** feather glossy black `#08080a` / highlight `#3a3a48` / beak yellow `#f4c93a` / iris yellow `#f4c93a` / pupil black `#08080a` / talon dark `#1a1a1a` / outline `#1a1a1a` textured. Cinnabar qi rim 5 % (hostile/scavenger mob).

#### §3.8.0 STYLE-REF (`crow_style_ref.png`)

```text
Linh Khí Wuxia × DST hand-painted master reference for "Crow" scavenger mob.

Subject: full body side-view 90° of a glossy black crow, perched neutral pose
(legs straight down, wings extended sideways horizontal — neutral spread for
puppet flap rotation around shoulder), yellow beak pointing right, sharp golden
iris with black pupil, tail short fan at rear, 2 small thin legs ending in 3-toe
talon silhouette (DST style — no individual claw detail, just rounded talon shape).

Palette LOCK: feather #08080a/#3a3a48 (subtle blue-black glossy highlight), beak
#f4c93a, iris #f4c93a + pupil #08080a, talon #1a1a1a, outline #1a1a1a textured.
Cinnabar qi rim 5%.

Composition: 1024x1024 PNG (square — wingspan horizontal), isolated, transparent BG.

Negative: cute friendly bird, raven (different species), anthropomorphic, anime
moe, chibi, photo-realistic, smooth airbrush, drop shadow, ground, text, watermark,
frame, dynamic pose, pure-black outline.
```

#### §3.8.1 E direction parts

```text
=== crow/E/head.png === (1/24)
Linh Khí Wuxia × DST. ISOLATED CROW HEAD ONLY, side-view 90° facing right, glossy
black feather, yellow beak pointing right, golden iris with black pupil, neck stub
at bottom cut clean. 1024x900 PNG (slightly wider for beak). Palette as above.
Cinnabar qi rim 5%. Tool: --cref crow_style_ref.png --cw 80 --ar 10:9.
```

```text
=== crow/E/torso.png === (2/24)
ISOLATED CROW BODY ONLY (NO head, NO wings, NO legs). Glossy black breast + back,
fan tail at rear bottom (small fan visible). Top horizontal at shoulder pivot.
1024x1280 PNG. Palette + tool as above.
```

```text
=== crow/E/wing_left.png === (3/24)

Linh Khí Wuxia × DST hand-painted. Ink-wash thick outline 16-24px, gouache flat.

Subject: ISOLATED LEFT WING ONLY, extended HORIZONTAL flap-neutral pose (NOT
folded — extended sideways for puppet rotation around shoulder). Glossy black
feathers with subtle blue-black highlight on primaries (visible feather lines but
NOT photorealistic detail), pivot at LEFT-CENTER (shoulder attach point), wing
extends to the RIGHT.

Palette LOCK: feather #08080a/#3a3a48, outline #1a1a1a textured. Cinnabar qi rim 5%.

Composition: 1620x840 PNG (horizontal), transparent BG, pivot left-center.

Negative: folded wing, body, two wings, anime moe, smooth airbrush, drop shadow,
ground, multiple subjects, text, frame, realistic feather texture, dynamic pose,
pure-black outline.

Tool: attach crow_style_ref.png. --cref --cw 80 --ar 27:14.
```

```text
=== crow/E/wing_right.png === (4/24) — flip wing_left horizontally.
=== crow/E/leg_left.png === (5/24) — small thin leg upper, body→knee, glossy black
feather upper + bare-skin lower hint, 380x1500 PNG. Top horizontal at hip pivot.
=== crow/E/leg_right.png === (6/24) — flip leg_left.
=== crow/E/shin_left.png === (7/24) — lower leg + 3-toe talon silhouette, 380x1200
PNG. Top horizontal at knee pivot.
=== crow/E/shin_right.png === (8/24) — flip shin_left.
```

#### §3.8.2 N + S directions

```text
=== crow/N/head.png === (9/24) — back of crow head, glossy feather rear, NO beak/eye.
=== crow/N/torso.png === (10/24) — back-view, full glossy black, fan tail visible.
=== crow/N/wing_left.png === (11/24) — back-view wing extended.
=== crow/N/wing_right.png === (12/24) — flip.
=== crow/N/leg_left.png === (13/24) — back-view leg upper.
=== crow/N/leg_right.png === (14/24) — flip.
=== crow/N/shin_left.png === (15/24) — back-view talon (3 toes from rear).
=== crow/N/shin_right.png === (16/24) — flip.

=== crow/S/head.png === (17/24) — front-view, beak pointing camera, two golden eyes.
=== crow/S/torso.png === (18/24) — chest front-view, fan tail behind.
=== crow/S/wing_left.png === (19/24) — front-view wing extended (viewer's left).
=== crow/S/wing_right.png === (20/24) — flip.
=== crow/S/leg_left.png === (21/24) — front-view leg.
=== crow/S/leg_right.png === (22/24) — flip.
=== crow/S/shin_left.png === (23/24) — front-view talon (3 toes facing camera).
=== crow/S/shin_right.png === (24/24) — flip.
```

#### §3.8.3 MEGA-PROMPT

```text
Generate Linh Khí Wuxia × DST asset pack for "crow" scavenger mob. Output 24 PNGs.

GLOBAL STYLE + PALETTE: see crow_style_ref.

ANATOMY: glossy black corvid, yellow beak, golden iris with black pupil, fan tail,
3-toe talon, wings extended horizontal (puppet rotation pivot at shoulder).

FILES: 8 parts × 3 dirs = 24. Head 1024x900, torso 1024x1280, wing 1620x840 each,
leg 380x1500, shin 380x1200.

GLOBAL NEGATIVE: cute friendly bird, raven (different species), anthropomorphic,
anime moe, chibi, photo-realistic, smooth airbrush, drop shadow, ground, text,
watermark, frame, dynamic pose, pure-black outline, folded wing.

Maintain crow identity across all 24.
```

---

### §3.9 Bat — Dơi Đêm

**Folder:** `Art/Characters/bat/`

**Concept:** leathery cave-dwelling bat, dark brown fur small body, translucent membranous wings (4 strut bones visible), tiny pointed ears, beady red eye, small fangs, clawed feet. Hostile night mob. Bipedal flying (same skeleton as crow). 8 parts × 3 dirs = 24 PNG.

**Palette LOCK:** fur dark brown `#3a2a20` / highlight `#5a3530` / wing membrane translucent dark brown `#5a3530` (60 % opacity) / wing strut darker `#1a1a1a` / eye small red `#a14040` / fang white `#c2c4ba` / outline `#1a1a1a` textured. Cinnabar qi rim 5 % (hostile mob).

#### §3.9.0 STYLE-REF (`bat_style_ref.png`)

```text
Linh Khí Wuxia × DST hand-painted master reference for "Bat" hostile night mob.

Subject: full body side-view 90° of a leathery bat, small fuzzy dark brown body,
two big membranous wings extended sideways (neutral flap pose, 4 visible strut
bones in dark ink black each wing, membrane semi-translucent dark brown 60% with
subtle highlight, wing edges scalloped between struts), tiny pointed ears, beady
red eye, small fangs visible at mouth, tiny clawed feet hanging.

Palette LOCK: fur #3a2a20/#5a3530, wing membrane #5a3530 (translucent feel), strut
#1a1a1a, eye #a14040, fang #c2c4ba, outline ink #1a1a1a textured. Cinnabar qi rim 5%.

Composition: 1024x1024 PNG (squarer — wingspan horizontal), isolated, transparent BG.

Negative: cute halloween, vampire anime, realistic photo, anthropomorphic, smooth
airbrush, drop shadow, ground, multiple subjects, text, watermark, frame, dynamic
pose, pure-black outline.
```

#### §3.9.1 E parts

```text
=== bat/E/head.png === (1/24) — small bat head, side-view, pointed ear up, red dot
eye, fang at mouth. 800x800 PNG.
=== bat/E/torso.png === (2/24) — small fuzzy body, no head/wings/legs. 800x1000 PNG.
=== bat/E/wing_left.png === (3/24)

Linh Khí Wuxia × DST. ISOLATED LEFT BAT WING (extended horizontal), 4 visible strut
bones rendering as dark ink black radiating from shoulder pivot like umbrella ribs,
membrane semi-translucent dark brown #5a3530 (60% opacity feel) filling between
struts, wing edge scalloped between strut endpoints. Pivot at LEFT-CENTER (shoulder
attach point).

Palette LOCK: membrane #5a3530, strut #1a1a1a, outline #1a1a1a textured.

Composition: 1620x840 PNG, transparent BG, pivot left-center.

Negative: folded, body, anime moe, smooth airbrush, drop shadow, ground, text, frame,
realistic photo, pure-black outline.

Tool: attach bat_style_ref.png. --cref --cw 80 --ar 27:14.
```

```text
=== bat/E/wing_right.png === (4/24) — flip wing_left.
=== bat/E/leg_left.png === (5/24) — tiny clawed leg upper, 280x900 PNG.
=== bat/E/leg_right.png === (6/24) — flip.
=== bat/E/shin_left.png === (7/24) — tiny clawed foot lower, 280x720 PNG.
=== bat/E/shin_right.png === (8/24) — flip.
```

#### §3.9.2 N + S parts

Same structure as crow N/S. Head back-view (no face), front-view (red eyes + fangs camera). Wings same.

#### §3.9.3 MEGA-PROMPT

```text
Generate Linh Khí Wuxia × DST asset pack for "bat" hostile night mob — Dơi Đêm.
Output 24 PNGs.

GLOBAL STYLE + PALETTE: see bat_style_ref.

ANATOMY: small fuzzy body, big membranous wings (4 strut bones each), tiny pointed
ears, red eye, fangs, clawed feet.

FILES: 8 parts × 3 dirs = 24. Head 800x800, torso 800x1000, wing 1620x840 each,
leg 280x900, shin 280x720.

GLOBAL NEGATIVE: cute halloween, vampire anime, anthropomorphic, smooth airbrush,
drop shadow, ground, text, watermark, frame, realistic photo, dynamic pose, pure-
black outline.

Maintain bat identity across all 24.
```

---

### §3.10 Snake — Thanh Xà

**Folder:** `Art/Characters/snake/`

**Concept:** Thanh Xà green forest serpent, scaled body in 4 segments tapering from wide neck to thin tail, cobra-like flared hood at neck, forked red tongue protruding, amber-gold eye. NO limbs. Body-segment puppet (head + 4 body segs). 5 parts × 3 dirs = 15 PNG.

**Required parts (per `PuppetPlaceholderSpec.RolesForCharacter` with `isSnake=true`):** head, body_seg_1 (widest, neck), body_seg_2, body_seg_3, body_seg_4 (thinnest, tail).

**Palette LOCK:** scale green `#2a4a2a` / highlight `#5a8a4a` / shadow `#1a2a1a` / belly cream `#a8c69b` / hood inner cream `#c8d4a8` / eye amber-gold `#d4a64a` / tongue red `#8b3a3a` / outline `#1a1a1a` textured. Jade qi rim 5 % (mystical forest entity).

#### §3.10.0 STYLE-REF (`snake_style_ref.png`)

```text
Linh Khí Wuxia × DST hand-painted master reference for "Snake" forest serpent —
Thanh Xà.

Subject: full body top-down/side-hybrid view of a Thanh Xà green serpent, 4
segmented body sections tapering from wide neck (segment 1) to thin tail
(segment 4), cobra-like flared hood at neck behind head, head with forked red
tongue protruding, amber-gold eye, scale pattern visible (DST-stylized 1-pixel
diamond pattern, NOT realistic), cream belly underside. Body laid STRAIGHT
horizontal for puppet rig (each segment will rotate at junctions in-game).

Palette LOCK: scale #2a4a2a/#5a8a4a/#1a2a1a, belly cream #a8c69b, hood inner
#c8d4a8, eye #d4a64a, tongue #8b3a3a, outline ink #1a1a1a textured. Jade qi rim 5%.

Composition: 1536x600 PNG (horizontal — body extends right), isolated, transparent
BG, head at LEFT, tail at RIGHT, body STRAIGHT.

Negative: coiled body, dynamic curve pose, dragon, multiple heads, anime moe,
chibi, photo-realistic, smooth airbrush, drop shadow, ground, text, watermark,
frame, pure-black outline.
```

#### §3.10.1 E parts

```text
=== snake/E/head.png === (1/15)

Linh Khí Wuxia × DST. ISOLATED SNAKE HEAD ONLY (with cobra hood flare), side-view
facing right, forked red tongue protruding, amber-gold eye, NO neck/body. Scale
pattern on head + hood, belly cream underside.

Palette LOCK: scale #2a4a2a/#5a8a4a/#1a2a1a, hood inner #c8d4a8, eye #d4a64a,
tongue #8b3a3a, outline #1a1a1a textured. Jade qi rim 5%.

Composition: 720x720 PNG, transparent BG.

Negative: body, dragon, multiple heads, anime moe, smooth airbrush, drop shadow,
ground, text, frame, pure-black outline.

Tool: attach snake_style_ref.png. --cref --cw 80 --ar 1:1.
```

```text
=== snake/E/body_seg_1.png === (2/15)  (= neck, widest segment)

Linh Khí Wuxia × DST. ISOLATED NECK SEGMENT (segment 1, widest, ~38px placeholder
width), side-view, scale pattern on top, belly cream underside, cylinder oval
shape laid horizontal. Pivot at LEFT-CENTER (junction to head). Right end is
junction to body_seg_2.

Palette LOCK: scale #2a4a2a/#5a8a4a/#1a2a1a, belly cream #a8c69b, outline #1a1a1a
textured.

Composition: 1140x780 PNG (horizontal, ~38:26 aspect), transparent BG, pivot
left-center.

Negative: head, tail, multiple segments, body curve, anime moe, smooth airbrush,
drop shadow, ground, text, frame, pure-black outline.

Tool: attach snake_style_ref.png. --cref --cw 80 --ar 19:13.
```

```text
=== snake/E/body_seg_2.png === (3/15) — same as seg_1 but slightly narrower (36px
placeholder), 1080x780 PNG, ~36:26 aspect.
=== snake/E/body_seg_3.png === (4/15) — narrower still (32x24 placeholder), 960x720
PNG, ~32:24 aspect.
=== snake/E/body_seg_4.png === (5/15) — thinnest tail tip (26x20 placeholder),
780x600 PNG, ~26:20 aspect. Has tail-pointed end at RIGHT (free end).
```

#### §3.10.2 N + S parts

```text
=== snake/N/head.png === (6/15) — back-of-head, hood spread, NO eye/tongue/face
visible from rear.
=== snake/N/body_seg_1..4 === (7-10/15) — top-down rear-view scale patterns, no
belly visible. (Snake body symmetry blurs side/top — N variant is mostly same as
E with belly hidden by rotation.)

=== snake/S/head.png === (11/15) — front-view of snake head, forked tongue + 2
eyes facing camera, hood flare visible.
=== snake/S/body_seg_1..4 === (12-15/15) — front-view scale + belly center, slight
foreshortening hint.
```

#### §3.10.3 MEGA-PROMPT

```text
Generate Linh Khí Wuxia × DST asset pack for "snake" forest serpent — Thanh Xà.
Output 15 PNGs.

GLOBAL STYLE + PALETTE: see snake_style_ref.

ANATOMY: green scaled serpent, 4 segmented body sections tapering from wide neck
to thin tail, cobra-like hood flare, forked tongue, amber eye, cream belly.

FILES: 5 parts × 3 dirs = 15. Head 720x720, body_seg_1 1140x780, seg_2 1080x780,
seg_3 960x720, seg_4 780x600.

GLOBAL NEGATIVE: coiled body, dynamic curve pose, dragon, multiple heads, anime
moe, chibi, photo-realistic, smooth airbrush, drop shadow, ground, text, watermark,
frame, pure-black outline.

Maintain snake identity across all 15: same scale pattern, same eye, same hood.
```

---

### §3.11 VendorNPC — Lão Tiên Sinh

**Folder:** `Art/Characters/vendor_npc/`

**Concept:** elder merchant cultivator, long white beard reaching chest, jade-green flowing robe with brown leather satchel slung across torso, walking staff in one hand, slight hunched stance (elder posture), kind smiling expression with eye crinkle. Bipedal humanoid puppet (same skeleton as Player, 10 parts × 3 dirs = 30 PNG).

**Palette LOCK:**
- Skin: elder `#d4b896` / shadow `#8a6f47`
- Beard + brow: white `#f0e8d0`
- Robe: jade `#5a8062` / shadow `#3a5a40` / highlight `#8aa882`
- Satchel: brown `#8a6f47` / strap `#5a4030`
- Staff: wood `#5a4030` / dark `#3a2820`
- Outline: ink `#1a1a1a` textured
- Qi-glow rim: jade `#a8c69b` 5 % (NPC ally signature)

#### §3.11.0 STYLE-REF (`vendor_npc_style_ref.png`)

```text
Linh Khí Wuxia × DST hand-painted master reference for "VendorNPC" — Lão Tiên Sinh
elder merchant.

Subject: full body side-view of an elder merchant cultivator, long white beard
reaching chest, kind smiling expression with eye crinkle, jade-green flowing robe
with wide sleeves, brown leather satchel slung diagonal across torso, holding
wooden walking staff in left hand. Slightly hunched (elder posture) but T-pose
neutral for puppet rig (left arm holding staff = arm_left + forearm_left, other
arm hanging = arm_right + forearm_right).

Palette LOCK: skin #d4b896/#8a6f47, beard #f0e8d0, robe jade #5a8062/#3a5a40/
#8aa882, satchel #8a6f47/strap #5a4030, staff #5a4030/#3a2820, outline #1a1a1a
textured. Jade qi rim 5%.

Composition: 1024x1536 PNG, isolated, transparent BG.

Negative: young, evil, anime style, photo-realistic, chibi extreme, smooth
airbrush, drop shadow, ground, multiple subjects, text, watermark, frame, dynamic
pose, pure-black outline.
```

#### §3.11.1–§3.11.3 E/N/S parts (10 × 3 = 30)

Same skeleton as Player. `forearm_left.png` should include "right hand grasping wooden staff vertical" — staff extends below mitten hand. Other parts standard humanoid template with vendor palette swap.

#### §3.11.4 MEGA-PROMPT

```text
Generate Linh Khí Wuxia × DST asset pack for "vendor_npc" — Lão Tiên Sinh elder
merchant. Output 30 PNGs.

GLOBAL STYLE + PALETTE: see vendor_npc_style_ref.

ANATOMY: elder humanoid (Player skeleton), long beard, jade robe, satchel diagonal,
staff in left hand (forearm_left includes staff).

FILES: same 30-file structure as Player (E/N/S × 10 parts).

GLOBAL NEGATIVE: young, evil, anime style, smooth airbrush, drop shadow, ground,
text, watermark, frame, dynamic pose, pure-black outline.

Maintain vendor identity across all 30.
```

---

### §3.12 CompanionNPC — Linh Nhi

**Folder:** `Art/Characters/companion_npc/`

**Concept:** young female cultivator companion, twin braided hair tied with silver ribbons reaching shoulders, light blue-purple robe with silver crescent-moon embroidery on sleeve and hem, calm gentle expression, small spiritual sword in lacquered sheath at left hip. Bipedal humanoid puppet (10 parts × 3 dirs = 30 PNG).

**Palette LOCK:**
- Skin: fair `#f0e0c8` / shadow `#c89970`
- Hair: black `#1a1a1a` + gloss `#3a3030`
- Ribbon: silver `#c2c4ba`
- Robe: blue-purple `#6a6a8c` / shadow `#3a3a5a` / highlight `#9a9ac0`
- Moon embroidery: silver `#c2c4ba`
- Sword sheath: lacquered `#5a4030` / dark `#3a2820`
- Outline: ink `#1a1a1a` textured
- Qi-glow rim: spirit blue `#a8d8ff` 5 % (NPC ally + spiritual signature)

#### §3.12.0 STYLE-REF (`companion_npc_style_ref.png`)

```text
Linh Khí Wuxia × DST hand-painted master reference for "CompanionNPC" — Linh Nhi
young female cultivator.

Subject: full body side-view of a young female cultivator (early 20s, calm serene
expression), twin braided hair tied with silver ribbons reaching shoulders,
wearing flowing blue-purple robe with silver crescent-moon embroidery on sleeve
end and hem, small spiritual sword in lacquered sheath hanging at left hip,
T-pose neutral, simple oval limbs DST-style.

Palette LOCK: skin #f0e0c8/#c89970, hair #1a1a1a + gloss #3a3030, ribbon #c2c4ba,
robe #6a6a8c/#3a3a5a/#9a9ac0, moon embroidery #c2c4ba, sheath #5a4030/#3a2820,
outline ink #1a1a1a textured. Spirit blue qi rim 5%.

Composition: 1024x1536 PNG, isolated, transparent BG.

Negative: anime moe, sexualized, chibi big-head extreme, cute commercial, anime
sparkle eye, photo-realistic, smooth airbrush, drop shadow, ground, multiple
subjects, text, watermark, frame, dynamic pose, pure-black outline.
```

#### §3.12.1–§3.12.3 E/N/S parts (10 × 3 = 30)

Same humanoid skeleton as Player. `torso.png` E side-view shows hip silhouette with sword sheath tip protruding (lacquered + dark accent). Other parts standard with companion palette swap.

#### §3.12.4 MEGA-PROMPT

```text
Generate Linh Khí Wuxia × DST asset pack for "companion_npc" — Linh Nhi young
female cultivator. Output 30 PNGs.

GLOBAL STYLE + PALETTE: see companion_npc_style_ref.

ANATOMY: young female humanoid (Player skeleton), twin braids, blue-purple robe,
moon embroidery, sword sheath at left hip (visible on torso side-view).

FILES: same 30-file structure as Player (E/N/S × 10 parts).

GLOBAL NEGATIVE: anime moe, sexualized, chibi big-head extreme, cute commercial,
anime sparkle eye, smooth airbrush, drop shadow, ground, text, watermark, frame,
dynamic pose, pure-black outline.

Maintain Linh Nhi identity across all 30.
```

---

## §4 Resources / items / tiles / VFX / weather / props

Mỗi block là 1 self-contained prompt PNG. Style anchor inline mỗi block.

### §4.1 Resources / world objects (top-down 30°, 256×256 PNG transparent)

```text
=== Art/Resources/tree/tree.png ===

Linh Khí Wuxia × DST hand-painted. Ink-wash thick black outline 8-12px (smaller
canvas), gouache flat 3-4 stops, NO airbrush.

Subject: ISOLATED gnarled forest spirit tree (Cổ Linh Mộc), top-down 30° view,
deep moss green leaves canopy + twisted bark brown trunk visible at base, faint
jade-green qi mist hint (5%) in foliage. Single tree centered.

Palette LOCK: leaves #4a6741/#a8c69b, trunk #b89968/#8a6f47, qi #a8c69b, outline
#1a1a1a textured.

Composition: 256x256 PNG, transparent BG, no shadow on ground.

Negative: forest scene, multiple trees, photo-realistic, anime moe, smooth
airbrush, drop shadow, ground, text, watermark, frame, pure-black outline.

Tool: --ar 1:1 --stylize 80.
```

```text
=== Art/Resources/rock/rock.png ===
Subject: ISOLATED weathered stone boulder, top-down 30°, slate grey with moss
patches on top. Palette: stone #7a7c80/#5a5d63, moss #4a6741, outline #1a1a1a.
Composition: 256x256 PNG. Negative: multiple rocks, ground, photo-realistic,
anime, smooth airbrush. Tool: --ar 1:1.
```

```text
=== Art/Resources/rock/mineral_rock.png ===
Subject: ISOLATED stone boulder with embedded mineral blue crystal vein, top-down
30°. Palette: stone #7a7c80/#5a5d63, mineral blue #4d6b8c/#a8d8ff, outline
#1a1a1a. Composition: 256x256 PNG. Negative: multiple, ground, photo-realistic.
```

```text
=== Art/Resources/water/water_spring.png ===
Subject: ISOLATED small spirit spring pool top-down 90° overhead, qi-blue water
with concentric ripple, mossy stone rim. Palette: water #6fb5e0/#a8d8ff, stone
rim #7a7c80, outline #1a1a1a. Composition: 256x256 PNG.
```

```text
=== Art/Resources/linh_mushroom/linh_mushroom.png ===
Subject: ISOLATED single tall spiritual mushroom top-down 30°, red cap with white
spots, cream stem. Palette: cap #a14040/#d46c5c, stem #f0e8d0, spot #f0e8d0,
outline #1a1a1a. Composition: 256x256 PNG.
```

```text
=== Art/Resources/berry_bush/berry_bush.png ===
Subject: ISOLATED low forest bush with cluster of small purple-red berries, top-
down 30°. Palette: leaves #4a6741, berry #8b3a3a/#a14040, outline #1a1a1a.
Composition: 256x256 PNG.
```

```text
=== Art/Resources/cactus/cactus.png ===
Subject: ISOLATED desert cactus with two side arms, green with white spikes, top-
down 30°. Palette: cactus #6b8559/#3a5a40, spike #f0e8d0, outline #1a1a1a.
Composition: 256x256 PNG.
```

```text
=== Art/Resources/death_lily/death_lily.png ===
Subject: ISOLATED cursed-desert lily flower with purple-black petals, faint
death-qi purple aura, top-down 30°. Palette: petal #9b6b8b/#6b3a5b, qi #d4a8e0,
stem #2a4a2a, outline #1a1a1a. Composition: 256x256 PNG.
```

```text
=== Art/Resources/linh_bamboo/linh_bamboo.png ===
Subject: ISOLATED tall jade-green bamboo cluster (3 stalks), faint qi glow at
joints, top-down 30°. Palette: bamboo #6b8e62/#a8c69b, qi #a8d8ff, outline
#1a1a1a. Composition: 256x256 PNG.
```

```text
=== Art/Resources/grass_tile/grass_tile.png ===
Subject: ISOLATED small tuft of forest grass, top-down 90°. Palette: grass
#6b8e62/#a8c69b, outline #1a1a1a. Composition: 128x128 PNG.
```

```text
=== Art/Resources/campfire/campfire.png ===
Subject: ISOLATED wood logs in star pattern with bright flame on top, faint orange
smoke. Palette: wood #5a4030/#8a6f47, flame #d4a64a/#a14040, smoke #c2c4ba,
outline #1a1a1a. Composition: 256x256 PNG.
```

```text
=== Art/Resources/workbench/workbench.png ===
Subject: ISOLATED wooden crafting workbench with hammer + chisel on top, top-down
30°. Palette: wood #5a4030/#8a6f47, tool metal #c2c4ba, outline #1a1a1a.
Composition: 256x256 PNG.
```

### §4.2 Item icons (256×256 transparent BG, frontal isometric)

```text
=== Art/Icons/wood_log.png ===
Linh Khí Wuxia × DST icon. Subject: ISOLATED wood log section, frontal isometric
3/4 angle. Palette: bark #8a6f47, pith #b89968, outline #1a1a1a textured.
Composition: 256x256 PNG. Negative: ground, multiple, photo-realistic.
```

```text
=== Art/Icons/stone_block.png ===
Subject: ISOLATED grey stone cube. Palette: stone #7a7c80/#5a5d63, outline #1a1a1a.
256x256 PNG.
```

```text
=== Art/Icons/linh_stone.png ===
Subject: ISOLATED jade-green crystal shard with qi glow. Palette: jade #6b8e62/
#a8c69b, glow #a8d8ff, outline #1a1a1a. 256x256 PNG.
```

```text
=== Art/Icons/iron_ore.png ===
Subject: ISOLATED dark grey ore chunk with mineral blue streak. Palette: ore
#5a5d63, streak #4d6b8c, outline #1a1a1a. 256x256 PNG.
```

```text
=== Art/Icons/bamboo_cane.png ===
Subject: ISOLATED jade-green bamboo cylinder with node bands. Palette: bamboo
#6b8e62/#a8c69b, outline #1a1a1a. 256x256 PNG.
```

```text
=== Art/Icons/raw_meat.png ===
Subject: ISOLATED red cut meat slab with marbling. Palette: meat #8b3a3a/#a14040,
outline #1a1a1a. 256x256 PNG.
```

```text
=== Art/Icons/cooked_meat.png ===
Subject: ISOLATED brown grilled chunk with char marks. Palette: meat #8a6f47/
#5a4030, char #1a1a1a, outline #1a1a1a. 256x256 PNG.
```

```text
=== Art/Icons/berry.png ===
Subject: ISOLATED cluster of 3 purple-red berries. Palette: berry #8b3a3a/#a14040,
leaf #4a6741, outline #1a1a1a. 256x256 PNG.
```

```text
=== Art/Icons/linh_mushroom_food.png ===
Subject: ISOLATED red-cap mushroom prepared. Palette: cap #a14040, stem #f0e8d0,
outline #1a1a1a. 256x256 PNG.
```

```text
=== Art/Icons/water_canteen.png ===
Subject: ISOLATED brown leather flask with water-blue cap glow. Palette: leather
#8a6f47/#5a4030, cap blue #6fb5e0, outline #1a1a1a. 256x256 PNG.
```

```text
=== Art/Icons/stone_axe.png ===
Subject: ISOLATED axe with stone head + wood handle. Palette: stone #7a7c80, wood
#5a4030, outline #1a1a1a. 256x256 PNG.
```

```text
=== Art/Icons/iron_pickaxe.png ===
Subject: ISOLATED pickaxe with metal head + wood handle. Palette: metal #5a5d63,
wood #5a4030, outline #1a1a1a. 256x256 PNG.
```

```text
=== Art/Icons/bone_knife.png ===
Subject: ISOLATED knife with ivory blade + wrapped grip. Palette: blade #d4c8a3,
grip #5a4030, outline #1a1a1a. 256x256 PNG.
```

```text
=== Art/Icons/fishing_rod.png ===
Subject: ISOLATED wood pole + string + small hook. Palette: wood #5a4030, string
#f0e8d0, hook #c2c4ba, outline #1a1a1a. 256x256 PNG.
```

```text
=== Art/Icons/jade_pendant.png ===
Subject: ISOLATED jade-green pendant on gold string. Palette: jade #6b8e62/#a8c69b,
string #d4a64a, outline #1a1a1a. 256x256 PNG.
```

```text
=== Art/Icons/qi_charm.png ===
Subject: ISOLATED paper talisman with red sigil ink. Palette: paper #f0e8d0,
sigil #8b3a3a, outline #1a1a1a. 256x256 PNG.
```

```text
=== Art/Icons/spirit_root_token.png ===
Subject: ISOLATED stone disc with elemental sigil engraved. Palette: stone
#7a7c80/#a3a5a8, sigil engrave #1a1a1a, outline #1a1a1a. 256x256 PNG.
```

### §4.3 Ground tiles (seamless 512×512 PNG, top-down 90°)

```text
=== Art/Tiles/forest/forest_01.png === (4 variants forest_01..04)

Linh Khí Wuxia × DST hand-painted ground tile. Top-down 90° flat orthographic
view. SEAMLESS tileable (edges must match opposite edges).

Subject: forest floor texture, deep moss green base with sage highlight, scattered
dry leaf, small grass tufts. Palette: moss #4a6741, sage #6b8e62, leaf #8a6f47,
outline (subtle, not as thick as character) #1a1a1a hint.

Composition: 512x512 PNG, edges seamless tileable.

Negative: visible tile edges, repetition lines, large objects, characters,
photographic, smooth airbrush, watermark, frame.

Tool: --ar 1:1 --stylize 80 + "seamless tileable, edges match top-bottom and left-
right".
```

```text
=== Art/Tiles/stone_highlands/stone_01.png === (4 variants)
Same template, palette: slate #7a7c80, dry moss #8a9b8c, dirt patch #8b7355.
```

```text
=== Art/Tiles/desert/desert_01.png === (4 variants)
Same template, palette: sand #c4a574, sand highlight #dec594, dirt #8b7355, faint
death-qi purple #9b6b8b dust hint.
```

### §4.4 VFX (128×128 transparent PNG, isolated single particle)

```text
=== Art/Vfx/hit_flash.png ===
Subject: ISOLATED white circular burst, soft edge, single frame VFX. Palette:
white #f0e8d0, glow #c2c4ba, outline subtle #1a1a1a. 128x128 PNG.

=== Art/Vfx/damage_popup_bg.png ===
Subject: ISOLATED red ribbon background banner for damage number. Palette: ribbon
red #8b3a3a/#5a0e15, outline #1a1a1a. 128x128 PNG.

=== Art/Vfx/blood_splash.png ===
Subject: ISOLATED red splatter, 4 droplet shapes radiating. Palette: blood
#8b3a3a/#5a0e15, outline #1a1a1a. 128x128 PNG.

=== Art/Vfx/dust_poof.png ===
Subject: ISOLATED beige puff with curled wisp. Palette: dust #c4a574/#dec594,
outline subtle. 128x128 PNG.

=== Art/Vfx/fire_spark.png ===
Subject: ISOLATED orange + red spark cluster. Palette: spark #d4a64a/#a14040,
outline subtle. 128x128 PNG.

=== Art/Vfx/smoke_wisp.png ===
Subject: ISOLATED grey vertical wisp with white highlight. Palette: smoke
#5a5d63/#c2c4ba, outline subtle. 128x128 PNG.

=== Art/Vfx/mana_glow.png ===
Subject: ISOLATED sky-qi blue radial soft glow. Palette: glow #a8d8ff/#6fb5e0,
outline subtle. 128x128 PNG.

=== Art/Vfx/level_up_halo.png ===
Subject: ISOLATED gold ring with sparkle dots. Palette: halo #d4a64a, sparkle
#f0e8d0, outline #1a1a1a. 128x128 PNG.

=== Art/Vfx/death_decay.png ===
Subject: ISOLATED purple dissipating wisp. Palette: decay #9b6b8b/#6b3a5b, outline
subtle. 128x128 PNG.

=== Art/Vfx/status_buff_icon.png ===
Subject: ISOLATED green up-arrow icon. Palette: green #6b8e62/#a8c69b, outline
#1a1a1a. 256x256 PNG.

=== Art/Vfx/status_debuff_icon.png ===
Subject: ISOLATED red down-arrow icon. Palette: red #8b3a3a/#a14040, outline
#1a1a1a. 256x256 PNG.
```

### §4.5 Weather (transparent PNG)

```text
=== Art/Weather/rain_drop.png === — 32x32 single droplet, water blue.
=== Art/Weather/snow_flake.png === — 32x32 hexagonal flake, white.
=== Art/Weather/fog_overlay.png === — 1024x1024 seamless wispy clouds, white 40%
opacity.
=== Art/Weather/lightning_bolt.png === — 256x768 jagged white bolt with blue glow.
=== Art/Weather/sun_ray.png === — 256x768 faint gold vertical beam, 30% opacity.
=== Art/Weather/sandstorm_overlay.png === — 1024x1024 seamless sand wisps, 60%
opacity.
```

### §4.6 Environment props (256×256 transparent PNG, top-down 30°)

```text
=== Art/Props/chest.png === — wood + iron banding, closed lid.
=== Art/Props/lantern.png === — red paper with wood frame, glow inside.
=== Art/Props/shrine.png === — small stone shrine with offering bowl.
=== Art/Props/banner.png === — cloth banner on pole, red-gold cultivation sigil.
=== Art/Props/signpost.png === — wood post + signboard with carved character.
=== Art/Props/barrel.png === — wood barrel with iron hoops.
=== Art/Props/crate.png === — wood crate with rope handle.
=== Art/Props/broken_stele.png === — cracked stone tablet with worn inscription.
=== Art/Props/tent.png === — cloth tent with center pole.
=== Art/Props/altar.png === — raised stone altar with incense brazier.
```

Mỗi prop block dùng template:

```text
Linh Khí Wuxia × DST hand-painted. Top-down 30° isometric view. Subject: ISOLATED
<PROP>. Palette: <COLORS>. Composition: 256x256 PNG transparent BG, no ground.
Negative: multiple, ground, photo-realistic, anime, smooth airbrush, text, frame.
Tool: --ar 1:1 --stylize 80.
```

---

## §5 DST animation feature parity

Hiện tại `PuppetAnimController` ráp 30 PNG cho 1 character/mob ⇒ rig chạy được:

| Animation | Out-of-box? | Parts cần | Code state |
| --- | --- | --- | --- |
| **Idle** (subtle bob) | ✅ | base 30 | đang chạy |
| **Walk** (4-frame leg cycle, biped/quadruped) | ✅ | base 30 | đang chạy |
| **Attack** (arm rotation around shoulder) | ✅ | base 30 | đang chạy |
| **Hit flash** (color overlay) | ✅ | base 30 | đang chạy |
| **Death** (collapse via rotation) | ✅ | base 30 | đang chạy |
| **Eat / Channel cast** | ❌ | base 30 + face_eating.png + face_channel.png swap layer | cần code: face-swap component |
| **Sleep** | ❌ | base 30 + body_sleep.png OR full-body 90° rotate + Z particle | cần code: state machine extension |
| **Mining / Chopping / Fishing** | ❌ | base 30 + tool_axe/pickaxe/rod overlay sprites + item-in-hand binding | cần code: ItemHoldComponent |
| **Sit / Crouch** | ❌ | base 30 + leg_sit_left/right.png alt sprites | cần code: alt-sprite swap |
| **Speak / emote** (eyebrow, mouth shape) | ❌ | base 30 + face_*.png swap layer (calm/happy/angry/talk) | cần code: face-swap |

**DST có cả những animation trên** vì Klei Spriter rig support multiple sprite slots per part + state-driven swap. Để đạt parity, repo cần thêm:

1. **Face-swap layer** cho Player/NPC: `face_calm.png`, `face_eating.png`, `face_talking.png`, `face_pain.png`, `face_dead.png` — 5 swap sprites overlay lên `head.png`. Code: thêm `FaceSwapComponent` đính vào head transform với index swap by state.
2. **Item-in-hand layer**: `tool_*.png` overlay sprites bind to `forearm_left`/`forearm_right` tip. Code: `ItemHoldComponent` với offset transform per tool.
3. **Alt-sprite slots** cho leg in sit/crouch states: `leg_sit_left.png` etc. Code: state machine query active sprite per puppet role.
4. **Particle layer** cho channel/cast/sleep/level-up: extend `Vfx/` với prefab particles bind to spawn anchor on character.

**Khi nào cần làm?** Sau khi 12 entity base art set được hoàn thành (12 × ~30 = ~360 PNG), animation richness layer sẽ là PR riêng. Catalog hiện tại chỉ cần cover base 30 PNG/entity vì đó là minimum cho idle/walk/attack/hit/death — đủ cho gameplay loop survival cơ bản.

Catalog trên KHÔNG include face-swap / item / alt-sprite prompts. Khi cần add những asset đó, sẽ extend catalog với section §3.x.5 (`Player face-swap pack`, `Player item-hold pack`, …).

---

## §6 Cost estimate + iteration tips

### Cost (1 entity = 30 PNG, 12 entity total = 360 PNG)

| Tool | Per image | 1 entity (30 PNG) | All 12 entities (360 PNG) |
| --- | --- | --- | --- |
| ChatGPT-Image (DALL·E 3) | $0.04 | $1.20 | ~$14 |
| Midjourney `--cref` (yearly $30/mo, ~1000 imgs) | ~$0.03 | ~$0.90 | ~$11 |
| Leonardo Phoenix (free 150 tokens/day, slow) | $0 | 1 day cap | ~5 days iter |
| Stable Diffusion local + ControlNet (8 GB GPU) | $0 + 30s/img | hours | days |

Recommend: **Midjourney `--cref`** for character consistency (best style match across 30 part PNGs), ChatGPT-Image fallback for tricky parts AI fails on (typically wing/tail with semi-translucent membrane).

### Iteration tips

1. **Always gen STYLE-REF master first.** Nếu STYLE-REF không đạt 8 luật, KHÔNG đi tiếp 30 part PNGs — sẽ ra inconsistent. Refine STYLE-REF 3–5 lần để hit DST + Linh Khí signature đúng.
2. **Image-to-image + prompt > text-only.** Mỗi part PNG attach `{entity}_style_ref.png` làm guidance. Midjourney `--cref --cw 80` strikes balance (giữ style tight, allow per-part variation).
3. **Lock palette với hex codes inline.** AI tools tôn trọng hex codes hơn text mô tả ("warm grey" → render khác mỗi lần; `#7a7c80` → render đúng).
4. **One PNG at a time.** AI single-prompt KHÔNG gen 30 PNG cùng lúc reliably. Loop từng part. MEGA-PROMPT chỉ dùng cho batch tool / API script.
5. **Trim PNG sau khi gen.** PR #118 đã trim PNG bbox + scipy stray-pixel removal. Re-trim mỗi PNG mới để placeholder importer auto-PPU không lệch. Script PIL: `Image.crop(getbbox())`.
6. **Validate trong Unity sau 5 PNG đầu** (head + torso + arm × 2 + leg E direction). Nếu pivot sai (head lơ lửng cách torso, arm offset shoulder), tinh chỉnh prompt "top edge horizontal at <pivot>" rồi re-gen.
7. **Outline thickness scale với canvas.** 1024 canvas → 16–24px; 512 canvas → 8–12px; 256 canvas → 4–6px. AI tools đôi khi outline thin khi canvas nhỏ — gen ở 1024 rồi resize xuống.
8. **Linh Khí signature checks** (8 luật):
   - Outline KHÔNG pure `#000` (hand-drawn ink-wash texture)?
   - Cinnabar/Jade/Cream tri-color anchor present?
   - Faint qi rim (5%) on silhouette? (jade for hero/NPC ally, cinnabar for hostile, purple for boss/cursed, blue for spirit/mystical)
   - Cultural ornament (jade pendant / cloud sigil / ribbon / talisman / etc.)?
9. **Common AI failure modes:**
   - "Renders smooth airbrush gradient" → add "GOUACHE FLAT FILLS, NO airbrush, NO smooth gradient" 2x in negative.
   - "Outline too thin" → add "THICK 16-24px ink-wash brushstroke outline" 2x in prompt.
   - "Adds drop shadow" → add "no shadow, no ground, no floor" 2x in negative.
   - "Cannot isolate body part" → re-prompt "ISOLATED, NO body, NO ground, transparent BG, single subject only" emphasized.
   - "Pure black solid outline" → emphasize "ink-wash calligraphy texture, NOT pure #000, warm #1a1a1a tinted, slight thickness variance".
10. **Devin Review tip:** PR dùng art mới — attach screenshot in-game so với placeholder. Nếu aspect ratio lệch placeholder >10% → re-gen với canvas điều chỉnh để match `PuppetPlaceholderSpec.RectFor(role)`.

---

## References

- [`Documentation/ART_STYLE.md`](ART_STYLE.md) — original style anchor + Leonardo workflow
- [`Documentation/WORLD_MAP_DESIGN.md`](WORLD_MAP_DESIGN.md) — biome palettes
- [`Assets/_Project/Scripts/Core/PuppetPlaceholderSpec.cs`](../Assets/_Project/Scripts/Core/PuppetPlaceholderSpec.cs) — `RectFor(role)` + `PuppetPlaceholderPPU` constants
- [`Assets/_Project/Scripts/Core/CharacterArtSpec.cs`](../Assets/_Project/Scripts/Core/CharacterArtSpec.cs) — `PuppetRole` enum + filename constants
- [`Assets/_Project/Editor/CharacterArtImporter.cs`](../Assets/_Project/Editor/CharacterArtImporter.cs) — auto-PPU import (PR #118: per-role)
- [`Assets/_Project/Editor/BootstrapWizard.cs`](../Assets/_Project/Editor/BootstrapWizard.cs) — `BuildPuppetHierarchy` offsets (head +0.45u above torso, arm ±0.18u shoulder, leg ±0.10u hip)
- [PR #117](https://github.com/roronoazoroshao369/game/pull/117) — initial player art import (illustration-style, DEPRECATED — replaced by Linh Khí Wuxia × DST style following this catalog)
- [PR #118](https://github.com/roronoazoroshao369/game/pull/118) — render fix (trim PNG + per-role auto-PPU)
- [PR #119](https://github.com/roronoazoroshao369/game/pull/119) — DST per-entity catalog v2 (replaced by this v3 with per-PNG self-contained blocks + Linh Khí signature)
