# AI Prompts — Chibi Wuxia × Soft-DST Master Catalog (v5)

> **Single source of truth** cho mọi prompt sinh art bằng AI cho `roronoazoroshao369/game`.
> v5 = rewrite hoàn chỉnh sau khi v2 player reference prompt **PASS 10/10 acceptance test** (May 2026).
> Style identity LOCKED = **"Chibi Wuxia × Soft-DST"** — chibi proportion 3.5–4 head + DST signature traits + wuxia outfit. KHÔNG đổi mid-production.
>
> **Trước khi gen art mới, đọc theo thứ tự:**
> 1. [§1 Style lock](#1-style-lock--chibi-wuxia--soft-dst-8-luật) — 8 luật bắt buộc
> 2. [§2 Anatomy spec table](#2-anatomy-spec-table) — aspect ratio + pivot per part (rig depends on this)
> 3. [§3 Player master prompt v2](#3-player-master-prompt-v2-locked) — full-body acceptance test, copy-paste ready
> 4. [§4 Tool-specific settings](#4-tool-specific-settings) — Leonardo / GPT-image / Midjourney / NanoBanana
> 5. [§5 Negative prompts master](#5-negative-prompts-master)
> 6. [§6 Acceptance test workflow](#6-acceptance-test-workflow)
> 7. [§7 Future entities — TODO](#7-future-entities--todo)
> 8. [§8 DST animation feature parity](#8-dst-animation-feature-parity)
> 9. [§9 Cost estimate + iteration tips](#9-cost-estimate--iteration-tips)
>
> **Output rig-ready**: PNG drop vào `Assets/_Project/Art/Characters/{id}/{E|N|S}/{part}.png`. `CharacterArtImporter` tự auto-PPU per role. `BootstrapWizard.BuildPuppetHierarchy` ráp parts vào skeleton. `PuppetAnimController` chạy idle/walk/attack/hit/death out-of-the-box.

---

## §1 Style lock — Chibi Wuxia × Soft-DST 8 luật

Style identity của game = **DST puppet rig + soft-DST signature + wuxia ink-painting**. Mọi prompt PHẢI tuân 8 luật dưới (đã inline sẵn trong §3 master prompt + atomic prompts).

### Base 4 — Soft-DST mechanics (rig-friendly + Klei feel)

1. **Sepia-tinted ink outline `#1a1408`** (NOT pure black `#000`), thickness 8–16px @ 1024 canvas (≈ 1–1.5% cạnh dài). CHUNKY variable-width brush wobble nice-to-have, slight overshoot at joint corners. Outline che chỗ ráp khớp ⇒ rig không thấy seams.
2. **Flat 3-color tonal stops per material** (light / mid / shadow) với subtle watercolor wash gradient ở edges. Visible brush stroke texture inside fills nice-to-have. KHÔNG smooth airbrush gradient. KHÔNG pure flat solid color.
3. **Muddied desaturated palette**, EVERY color saturation clamped to MAX 30%, sepia/ochre tonal overlay across whole image. KHÔNG saturated bright colors (`#f0c020` lemon yellow, `#e48c2e` neon orange).
4. **Chibi proportion 3.5–4 head-tall** (Webber-leaning chibi cute young cultivator). NOT lanky 5-head Wilson. NOT super-deformed >1/3 head ratio. Head ~25–28% of total body height. Shoulders narrow ~1.0–1.2× head width.

### Wuxia signature 4 — game's unique identity

5. **Cream wuxia kimono** với V-neck collar, **HIP-LENGTH ONLY** (robe ends at hip line, MUST NOT drape past hip). Sleeves TIGHT TO ARM cylinder shape, NO bell-flow / NO flaring. Brown cuff trim band wrapping sleeve at wrist (~8% of arm length).
6. **Cinnabar–Jade–Cream tri-color anchor** — every entity contains at least 3 of: cinnabar `#8b3a3a` (red, hostile / blood / cultivator inner energy), jade `#7a9078` (muddied green, life qi / spirit / vegetation), cream `#e8d8b8` (light, skin / cloth / bone). Other palette colors layer on top.
7. **Cultural ornament accent** — every humanoid has wuxia identity full set: jade pendant + cloud sigil on chest, gold sash bow knot at right waist (muted dusty gold `#a8884a`, NOT bright lemon yellow), topknot bun + cream silk ribbon trailing, single asymmetric forelock at front. Mob has at least 1 fantasy hint: glowing eye, qi mist around antler/tail/wing, bone marker.
8. **Face minimalism** — small eye với SINGLE black SOLID DOT pupil `#1a1408` ~3–5px (NOT detailed almond iris, NOT anime eye), single line mouth `#6a3a28` ~1–2px, tiny angle nose suggestion ~5–8px brush stroke, single-stroke eyebrow ~10–15px above eye. NO anime sparkle, NO multi-color iris, NO eye highlight star, NO kawaii expression.

### What this gives you over plain DST

- **Recognizable as YOUR game** — không nhìn như Don't Starve clone (chibi proportion + wuxia outfit identity là signature riêng).
- **Style consistency across entities** — cùng 8 luật ⇒ Wolf đứng cạnh Player nhìn như chung 1 universe.
- **Future-proof for animation richness** — exaggerated anatomy + isolated parts + cultural ornaments cho phép thêm "ribbon flutter", "jade glow pulse", "qi mist trail" sau mà không phải re-author art.

> **Decision history (May 2026):** sau 3 round iteration test strict DST canon prompts (5-head lanky Wilson + variable-width brush + smooth gouache + face minimalism), generic AI gen tools chỉ deliver ~25–30% DST canon adherence do strong anime/chibi training bias. Per user decision, identity locked = **"Chibi Wuxia × Soft-DST"** — chibi proportion accepted, DST signature traits preserved. v2 prompt (§3 dưới) đã PASS 10/10 acceptance test. Không clone DST 100% — đây là identity riêng của game.

---

## §2 Anatomy spec table

Tham chiếu: `Assets/_Project/Scripts/Core/PuppetPlaceholderSpec.cs` `RectFor(role)`. Aspect ratio AI gen ra phải gần placeholder ±10 % nếu không rig render lệch.

| Role | Placeholder W×H (px) | World H (u) | Aspect | Pivot | Recommended canvas |
| --- | --- | --- | --- | --- | --- |
| `head` | 40×40 | 0.625 | 1.00 | center | 1024×1024 |
| `torso` | 52×80 | 1.250 | 0.65 | center | 1024×1536 |
| `arm_left` / `arm_right` | 16×36 | 0.5625 | 0.44 | top-center (vai) | 600×1350 |
| `forearm_left` / `forearm_right` | 14×28 | 0.4375 | 0.50 | top-center (khuỷu) | 540×1080 |
| `leg_left` / `leg_right` | 18×48 | 0.75 | 0.38 | top-center (hông) | 660×1760 |
| `shin_left` / `shin_right` | 16×32 | 0.5 | 0.50 | top-center (gối) | 600×1200 |
| `tail` | 50×18 | 0.281 | 2.78 | left-center (gốc tail) | 1500×540 |
| `wing_left` / `wing_right` | 54×28 | 0.438 | 1.93 | left-center (vai cánh) | 1620×840 |
| `body_seg_1..4` | 38×26 → 26×20 | 0.41 → 0.31 | ~1.5 | left-center | 1140×780 |

**Full-body style ref canvas** (dùng cho §3 master prompt, KHÔNG dùng cho rig — chỉ làm acceptance test + image guidance cho 30 atomic parts):

| Use case | Canvas | Aspect |
| --- | --- | --- |
| Full-body player style ref | 1024×1536 | 2:3 portrait |
| Full-body mob style ref | 1024×1024 | 1:1 square |

---

## §3 Player master prompt v2 (LOCKED)

> **Đây là prompt CANONICAL cho player full-body reference.** PASS 10/10 acceptance test (xem [§6](#6-acceptance-test-workflow)). Dùng làm:
> 1. Acceptance test trước khi gen asset player.
> 2. Image guidance / `--cref` / IP-Adapter input cho final atomic part prompts ở §3.3.
> 3. Base identity lock trước khi dùng [`PLAYER_FULL_ASSET_SOURCE_PROMPT.md`](PLAYER_FULL_ASSET_SOURCE_PROMPT.md) để gen source board đồng bộ.
>
> **Concept:** chibi young SEA-Asian male qi-cultivator (age 12–14, chibi cute proportion), cream V-neck wuxia kimono robe HIP-LENGTH với brown cuff trim band ở wrist + gold sash bow knot ở right waist + jade pendant + jade cloud sigil trên ngực + warm-charcoal trousers + brown leather boots với cream toe stitch + ink-black topknot bun + cream silk ribbon trailing + single asymmetric forelock at front.
>
> **Folder output**: `Documentation/assets/style_refs/player_E_v2.png` (sau khi pass).

### §3.0 GPT image 2.0 prompting framework (template chuẩn)

> **Tại sao framework riêng cho GPT image 2.0?**
> GPT image 2.0 (`gpt-image-2`, OpenAI Apr 2026) là **instruction-following model**, không phải keyword-cloud model như Midjourney/SD/Leonardo. Theo [OpenAI prompting guide](https://developers.openai.com/cookbook/examples/multimodal/image-gen-models-prompting-guide), prompt tốt nhất cho GPT-2 phải:
>
> 1. **Structured + skimmable** — labeled segments với line breaks, KHÔNG one long paragraph.
> 2. **Order: scene/style → subject → key details → constraints → output** — fixed order giúp model hiểu hierarchy.
> 3. **Concrete materials/shapes/textures** — exact hex codes, exact dimensions, exact body regions.
> 4. **Inline constraints "no X"** — GPT không có separate negative field, viết "NO ..." trực tiếp trong prompt.
> 5. **Multi-image input** — pass style ref qua `images.edit()` endpoint cho character consistency (xem [§4.2](#42-gpt-image-20-gpt-image-2-openai-2026)).
> 6. **Iterate, don't overload** — start clean, refine với "change only X, keep everything else the same".

#### Template chuẩn (8 labeled segments)

```
GOAL: [intended use, 1 line — vd "Game sprite asset for atomic puppet rig"]

STYLE:
- Visual medium: [hand-painted painterly illustration, DST × wuxia fusion]
- Outline: [Klei sepia-tinted ink #1a1408, variable-width Wpx-Wpx at Wpx canvas, hand-drawn wobble]
- Palette: [muddied desaturated, saturation cap 30%, sepia/ochre overlay]
- Rendering: [flat 3-color tonal stops, watercolor wash gradient, brush stroke texture]

SUBJECT:
- Character: [chibi young-male wuxia cultivator, age 12-14]
- Proportion: [3.5-4 head tall, head 25-28% of body height, shoulders 1.0-1.2× head width]
- Body region: [ISOLATED part name, cut clean at boundary]

VIEW:
- Direction: [side profile facing right / back view / front view facing camera]
- Camera: [eye-level, full part visible from top to bottom]
- Single subject isolated, NO duplicate, NO turnaround sheet

POSE / ANATOMY:
- [pose details specific to part]
- [anatomical landmarks visible / hidden]

ANATOMICAL ANCHORS (% of canvas Y-axis, top=0% bottom=100%):
- [feature 1]: Y-region X%-Y%
- [feature 2]: Y-region X%-Y%

CLOTHING / MATERIALS (only items present in this body region):
- [item 1 with palette hex]
- [item 2 with palette hex]

PALETTE LOCK (exact hex codes):
- [material]: highlight / mid / shadow

OUTPUT:
- Canvas: [WxH] PNG, transparent RGBA
- Composition: tight alpha bbox ~5px padding, NO ground, NO drop shadow, NO border, NO color background

DO NOT INCLUDE:
- NO [adjacent body parts that should not appear]
- NO [common drift markers: anime, kawaii, 5-head, bell-flow, etc.]
- NO [direction views other than this one]
- NO [bright saturated colors, smooth airbrush, pure black outline, thin vector line]
```

#### Numerical anchors > vague modifiers (CRITICAL for GPT-2)

GPT image 2.0 follows **explicit numbers** much better than fuzzy modifiers. Empirical drift observed when prompt uses vague language:

| ❌ Vague (drift-prone) | ✅ Numerical anchor (locks model) |
|---|---|
| `Cuff trim band ~70% down` | `Brown cuff trim band at Y-region 70-78% from top, occupying 8% of forearm length` |
| `Sleeve covers most of forearm` | `Cream sleeve from Y=0% (elbow cut) to Y=70% (cuff start)` |
| `V-neck collar at top` | `V-neck collar at Y=0-12% from top, opening points downward at center X` |
| `Sash at right waist` | `Gold sash bow knot at Y=68-78%, X=60-85% (right waist), ribbon ends drape Y=78-92%` |
| `Eye in middle of face` | `Eye pupil at Y=48-52% (mid-face), X=55-65% (front-of-skull, profile-right)` |
| `Boot lower portion of shin` | `Brown leather boot from Y=65-100% (lower 35%), trouser fabric Y=0-65%` |

**Rule of thumb:**
- Always specify Y-position as `Y=X%-Y%` of the canvas (top=0%, bottom=100%)
- Always specify dimension as exact px or ratio (e.g. `8% of forearm length`, `~1.2-1.4 head-heights tall`)
- Always specify body landmark using anatomical term + canvas coord (e.g. `wrist at Y=78%, NOT at very bottom`)

Why this matters for `gpt-image-2`: model interprets vague modifiers via training prior (which may differ from your spec). Hard numerical anchors override prior — model places features at exact requested location.

#### Khác biệt vs framework Midjourney/SD/Leonardo

| Aspect | MJ/SD/Leonardo | GPT image 2.0 |
|---|---|---|
| Prompt format | Keyword cloud, comma-sep | Labeled segments, line breaks |
| Negatives | Separate field (`--no` / negative prompt) | Inline `NO X` statements |
| Style reference | `--cref` / IP-Adapter / ControlNet | `images.edit()` multi-image input |
| Iteration | Re-run with new seed | Multi-turn `change only X, keep rest same` |
| Spatial directives | Aspect ratio + composition keyword | **Numerical anchors (Y=X%, dimension ratios)** |
| Token efficiency | Penalize duplicates | Repeat critical constraints OK (instruction-following) |

#### API parameter recommendations

| Use case | `model` | `size` | `quality` | `background` | `output_format` |
|---|---|---|---|---|---|
| Master full-body style ref | `gpt-image-2` | `1024x1536` | `high` | `transparent` | `png` |
| Atomic head | `gpt-image-2` | `1024x1024` | `high` | `transparent` | `png` |
| Atomic torso | `gpt-image-2` | `1024x1536` | `high` | `transparent` | `png` |
| Atomic arm / forearm | `gpt-image-2` | `1024x1536` (square or portrait, model auto-crops) | `medium` | `transparent` | `png` |
| Atomic leg / shin | `gpt-image-2` | `1024x1536` | `medium` | `transparent` | `png` |
| Batch ideation (low-stakes) | `gpt-image-2` `quality=low` or `gpt-image-1-mini` | `1024x1024` | `low` | `transparent` | `png` |

> **Note:** `gpt-image-2` chấp nhận resolution bất kỳ thoả mãn: max edge < 3840px, both edges multiple of 16, ratio long:short ≤ 3:1, total pixels 655K-8.3M. Tuy nhiên với atomic part nhỏ (vd arm 600×1350) → up canvas lên 1024×1536 cho gen, sau đó crop tight bbox về dimensions target trong post-process. `gpt-image-1` / `gpt-image-1.5` chỉ accept fixed sizes: `1024x1024`, `1024x1536`, `1536x1024`.

#### Multi-image reference workflow (character consistency)

Sau khi master §3.1 PASS 10/10, lưu ảnh thành `Documentation/assets/style_refs/player_E_v2.png`. Khi gen 30 atomic parts, dùng `images.edit()` endpoint với style ref:

```python
from openai import OpenAI
client = OpenAI()

with open("Documentation/assets/style_refs/player_E_v2.png", "rb") as ref:
    result = client.images.edit(
        model="gpt-image-2",
        image=[ref],
        prompt=ATOMIC_PART_PROMPT,  # full §3.3/§E/head prompt below
        size="1024x1024",
        background="transparent",
        quality="high",
    )
```

Style ref giúp giữ identity cross-30-PNG (skin tone, hair color, robe wash texture) — quan trọng cho atomic rig vì các part phải nhìn như cùng 1 nhân vật khi ráp lại.

### §3.1 Master full-body prompt (GPT-2 structured template, copy nguyên block)

```
GOAL: Hand-painted full-body character art reference for game sprite asset, side profile facing right (East direction), used as style ref for atomic puppet rig parts.

STYLE:
- Visual medium: hand-painted painterly illustration, Don't Starve Together × Chinese wuxia cultivation fusion
- Outline: Klei-style sepia-tinted ink #1a1408 (NOT pure black), variable-width 10-14px at 1024 canvas, chunky brush with hand-drawn wobble + slight overshoot at joint corners
- Palette: muddied desaturated, EVERY color saturation clamped MAX 30%, sepia/ochre tonal overlay across whole image
- Rendering: flat 3-color tonal stops per material (light/mid/shadow), subtle watercolor wash gradient, visible brush stroke texture inside fills

SUBJECT:
- Character: chibi young-male wuxia cultivator boy, age 12-14, calm curious expression
- Proportion: EXACTLY 3.5-4 heads tall total body height (NOT 5, NOT 6, NOT lanky teen, NOT adult)
- Head ratio: 25-28% of total body height (cute Webber-leaning chibi)
- Build: shoulders narrow 1.0-1.2 head-widths wide, slight forward slouch curious cultivator pose

VIEW:
- Pure side profile facing right (East direction), one eye visible
- Full body from top of hair bun to toe of boot in frame
- Single character isolated, NO duplicate, NO turnaround sheet, NO front view, NO back view (gen separately)

ANATOMICAL ANCHORS (% of canvas height, top=0% bottom=100%):
- Hair bun crown: Y=0-7%
- Forelock front: Y=5-12%
- Eye dot pupil: Y=12-15% (mid-face), X=55-65% (front-of-skull, profile-right side)
- Nose tip: Y=14-17%
- Mouth line: Y=16-19%
- Jaw line: Y=22-25% (bottom of head segment)
- Neck: Y=22-26%
- Shoulder line: Y=26-30%
- Chest center (jade pendant + cloud sigil): Y=32-42%
- Waist (gold sash bow knot): Y=46-52%
- Hip line / robe bottom edge: Y=54-58% (HIP-LENGTH lock)
- Knee: Y=72-76%
- Ankle / boot top: Y=88-92%
- Boot sole / ground line: Y=98-100%

POSE:
- Neutral idle, slight forward slouch
- Arms hanging relaxed at sides, ~5 degree gap from torso
- Hands visible as small mitten fists at Y=58-65% (waist-to-hip side area)
- Legs shoulder-width apart with slight knee unlock

OUTFIT (every item visible, locked exactly):
- Cream wuxia kimono robe, V-neck collar at Y=27-33% (top), HIP-LENGTH ONLY: ends at Y=54-58% where leg starts; NEVER drape past Y=60%, NEVER reach knee (Y=74), NEVER reach mid-shin
- Sleeves TIGHT to arm cylinder shape: NO bell-flow, NO flare, NO wide opening
- Brown cuff trim band at wrist (Y=58-62% on arm), occupying 8% of arm length: #8a6f47 / shadow #5a4030
- Gold sash bow knot at right waist (Y=46-52%, X=60-85%), ribbon ends drape Y=52-60%
- Jade pendant on green-brown silk cord hanging on chest center (Y=32-42%, X=50%)
- Cloud sigil embroidered on chest at Y=33-40%, ~1/8 of torso area
- Charcoal trousers from hip line (Y=58%) to ankle (Y=88%), tight cylinder cut
- Brown leather ankle boots from Y=88-100%, with cream-tan toe stitch + ankle strap (slightly oversized chibi boot OK, NOT clown-foot)

HAIR:
- Ink-black base #2a2418 (warm-dark, NOT pure black, NOT pure #000)
- Subtle highlight #4a4030 sepia gloss only at top of bun
- Topknot bun on crown of head (Y=0-7%), tied with cream silk ribbon trailing back ~1.5 head-heights
- Single asymmetric forelock falling forward at front of forehead (Y=5-12%), ~1 head width long

FACE (extreme minimalism — DST canon):
- ONE small SOLID DOT pupil only, 3-5px at 1024 canvas, color #1a1408, at Y=12-15%
- NO iris, NO sclera fill, NO eyelash, NO highlight star, NO multi-color, NO anime eye shape with rim
- Single short line mouth, 1-2px thick, color #6a3a28, at Y=16-19%
- Tiny angle nose suggestion as 5-8px brush stroke at Y=14-17%
- Single-stroke eyebrow ~10-15px above eye (Y=10-12%), color #2a2418

PALETTE LOCK (exact hex codes):
- Skin: highlight #c8a884 / mid #a08868 / shadow #5a4828 (muddied warm tan with sepia overlay)
- Hair: ink-black #2a2418 + highlight #4a4030
- Ribbon: cream #e8d8b8
- Robe: highlight #e8d8b8 / mid #c8b094 / fold shadow #8a6f47
- Cuff trim: #8a6f47 / shadow #5a4030
- Sash: MUTED DUSTY GOLD #a8884a / shadow #7a5a30 (CRITICAL: NOT bright lemon yellow, NOT neon, must read as muddied earthy gold)
- Jade pendant + cloud sigil: #7a9078 / shadow #4a5a48
- Trousers: #3a3530 / shadow #1a1812
- Boot leather: #5a4830 / shadow #3a2818, strap #a89878
- Lip line: #6a3a28
- Optional cheek blush: #c89878 at 40% opacity, only at apple area
- Outline: sepia #1a1408 variable-width 10-14px

OUTPUT:
- Canvas: 1024x1536 PNG portrait
- Background: transparent RGBA (alpha channel)
- Composition: tight alpha bbox with ~5px transparent padding around subject
- NO ground, NO drop shadow under character, NO ambient particles, NO border frame, NO color background

DO NOT INCLUDE:
- NO front view, NO back view in this image (East side profile only)
- NO duplicate character, NO multiple poses, NO turnaround sheet
- NO 5-head adult proportion, NO 6-head, NO lanky teen
- NO super-deformed (>1/3 head ratio)
- NO bell-flow sleeve, NO flaring sleeve, NO wide sleeve opening
- NO knee-length robe (drape past Y=60%), NO mid-shin robe, NO ankle-length robe (HIP-LENGTH ONLY: end at Y=54-58%)
- NO bright lemon yellow sash, NO neon yellow sash (MUTED dusty gold only)
- NO multi-color anime iris, NO eye sparkle, NO heart eyes, NO kawaii expression
- NO smooth airbrush gradient, NO pure #000 outline, NO clean thin vector line
- NO clown-foot exaggerated boot
- NO floating hair strands disconnected from head
- NO watermark, NO text, NO logo, NO grid, NO border

REINFORCE: chibi 3.5-4 heads (NOT 5+), HIP-LENGTH robe ends at Y=54-58% (NOT longer), TIGHT sleeve (NO bell), MUTED gold (NOT bright), single dot eye (NOT iris), CHUNKY 10-14px sepia outline (NOT thin vector).
```

> **API call (Python):**
> ```python
> from openai import OpenAI
> client = OpenAI()
> result = client.images.generate(
>     model="gpt-image-2",
>     prompt=MASTER_PROMPT,  # paste fenced block above
>     size="1024x1536",
>     quality="high",
>     background="transparent",
>     output_format="png",
> )
> ```

### §3.2 Palette LOCK (single source of truth — inherit in ALL atomic parts)

| Material | Highlight | Mid | Shadow | Notes |
|---|---|---|---|---|
| Skin | `#c8a884` | `#a08868` | `#5a4828` | Muddied warm tan, sepia overlay |
| Hair | `#4a4030` (gloss) | `#2a2418` (base) | — | Ink-black warm-dark, NOT pure `#000` |
| Ribbon (cream) | `#e8d8b8` | — | — | Washed-out cream silk |
| Robe (cream kimono) | `#e8d8b8` | `#c8b094` | `#8a6f47` (fold) | Hip-length, tight sleeve |
| Cuff trim (brown) | `#8a6f47` | — | `#5a4030` | Band ~8% sleeve length |
| Sash (muted gold) | `#a8884a` | — | `#7a5a30` | NOT bright `#f0c020` lemon yellow |
| Jade pendant + cloud sigil | `#7a9078` | — | `#4a5a48` | Muddied green |
| Trousers (warm charcoal) | `#3a3530` (base) | — | `#1a1812` | Dark olive, NOT pure black |
| Boot leather | `#5a4830` (base) | — | `#3a2818` | Warm dark brown |
| Boot strap / toe stitch | `#a89878` | — | — | Cream-tan |
| Outline ink | `#1a1408` | — | — | Sepia-tinted, variable width 8–16px |
| Cheek blush (optional) | `#c89878` @ 40% opacity | — | — | Cheek apple only |

### §3.3 Atomic 30-part prompts (full self-contained, copy-paste ready)

Sau khi master prompt §3.1 PASS 10/10 (xem [§6](#6-acceptance-test-workflow)), dùng output làm `--cref` / IP-Adapter / Image Guidance input cho 30 atomic part prompts dưới đây. Mỗi prompt **self-contained** — copy đúng 1 fenced block, paste vào tool, xong. Inherit palette LOCK §3.2 + style §1 + anatomy §2.

**Output folders**:
- `Assets/_Project/Art/Characters/player/E/{part}.png` — 10 parts (E direction required)
- `Assets/_Project/Art/Characters/player/N/{part}.png` — 6 required + 4 optional (arms auto-hidden by `PuppetAnimController.hideArmsInFrontBackView=true`)
- `Assets/_Project/Art/Characters/player/S/{part}.png` — same as N

**Atomic-symbol composition rules** (đã inline trong từng prompt dưới):
1. ONE part = ONE anatomical region. NO baked-in adjacent parts.
2. Tight alpha bbox (≤5px transparent padding all sides).
3. Pivot convention: top-of-sprite = attach point cho parent joint (head→neck base, torso→hip base, arm→shoulder, forearm→elbow, leg→hip, shin→knee).
4. NO shadow on ground, NO background, NO border, NO watermark, NO text.

---

#### §E — East direction (right-side profile, character facing right)

> 10 parts. W = flipX của E (skip — sprite system tự flip).

##### §E/head.png — target ~210×220, canvas 1024×1024

```
GOAL: Atomic puppet rig sprite — head.png (E direction, right-side profile, character facing right), input ready for game sprite asset pipeline.

STYLE:
- Visual medium: hand-painted painterly illustration, Don't Starve Together × Chinese wuxia cultivation fusion
- Outline: Klei sepia-tinted ink #1a1408 (NOT pure black), variable-width 10-14px at 1024x1024 canvas, chunky brush with hand-drawn wobble + slight overshoot at joints
- Palette: muddied desaturated, saturation cap 30%, sepia/ochre tonal overlay
- Rendering: flat 3-color tonal stops (light/mid/shadow), subtle watercolor wash gradient, visible brush stroke texture

SUBJECT:
- Character context: chibi young-male wuxia cultivator (age 12-14), 3.5-4 head tall proportion (NOT lanky 5-head adult, NOT super-deformed)
- Body region: ISOLATED HUMAN HEAD ONLY (skull + face + hair), right-side profile, character facing right
- Cut boundary: clean horizontal cut at jaw line at Y=95-100% — NO neck below jaw, NO collar, NO shoulders

VIEW:
- Direction: right-side profile, character facing right
- Single isolated part, NO duplicate, NO turnaround, NO other directions in this image
- Pivot: bottom edge of sprite = jaw cut at Y=100% = neck attach point for parent torso

ANATOMICAL ANCHORS (% of sprite height, top=0% bottom=100%):
- Hair bun crown: Y=0-15% (top of skull)
- Forelock front: Y=10-25% (asymmetric, falling toward front-right of skull)
- Eye pupil dot: Y=42-50%, X=58-68% (front-half of skull, profile-right side, ONE eye only)
- Eyebrow stroke: Y=36-42%, X=55-70%
- Nose tip pointing right: Y=48-55%, X=78-92% (right edge of face)
- Mouth line: Y=58-65%, X=70-85%
- Ear: Y=42-58%, X=35-48% (mid-skull side)
- Ribbon trailing back: extends from bun (X=20-40%) leftward to canvas edge (X=0%)
- Jaw line cut: Y=95-100% (clean horizontal bottom)

COMPOSITION:
- Profile-right view: ONE SOLID DOT pupil eye (#1a1408, 3-5px diameter, NO iris, NO sclera, NO eyelash, NO highlight star, NO anime eye)
- Single line mouth (1-2px thick, color #6a3a28)
- Tiny angle nose pointing right (5-8px brush stroke)
- Single-stroke eyebrow (10-15px wide, color #2a2418)
- Hair: topknot bun on crown, cream silk ribbon (#e8d8b8) trailing back ~1.5 head-heights toward canvas left edge
- Single asymmetric forelock falling forward (~1 head-width long, off-center to forehead front)
- Optional subtle cheek blush #c89878 at 40% opacity (apple area Y=50-58%, X=55-72%)

PALETTE LOCK (exact hex codes):
- Skin: highlight #c8a884 / mid #a08868 / shadow #5a4828
- Hair: ink-black #2a2418 + highlight #4a4030 (warm-dark, NOT pure #000)
- Ribbon: cream #e8d8b8
- Lip line: #6a3a28
- Outline: sepia #1a1408 variable-width 10-14px

OUTPUT:
- Canvas: 1024x1024 PNG, transparent RGBA
- Target render dimensions: ~210x220px (model up-renders, post-process crops tight bbox)
- Composition: tight alpha bbox ~5px transparent padding, NO ground, NO drop shadow, NO border, NO color background

DO NOT INCLUDE:
- NO front view in this image
- NO back view in this image
- NO neck below Y=100% jaw cut, NO shoulders, NO collar, NO torso, NO body
- NO multiple eyes (profile = ONE eye), NO detailed almond iris
- NO anime gloss highlight stripes on hair
- NO open mouth surprised expression
- NO floating hair strands disconnected from head
- NO super-deformed >1/3 head ratio, NO 5-head lanky adult
- NO anime eye sparkle, NO multi-color iris, NO eye highlight star, NO kawaii heart eyes
- NO smooth airbrush gradient, NO pure #000 outline, NO clean uniform thin vector line
- NO saturated bright colors, NO lemon yellow, NO neon orange
- NO watermark, NO text, NO grid, NO border, NO blurry edges, NO anatomy errors
```

##### §E/torso.png — target ~120×260, canvas 1024×1536, TRUNK ONLY

```
GOAL: Atomic puppet rig sprite — torso.png (E direction, right-side profile, character facing right), input ready for game sprite asset pipeline.

STYLE:
- Visual medium: hand-painted painterly illustration, Don't Starve Together × Chinese wuxia cultivation fusion
- Outline: Klei sepia-tinted ink #1a1408 (NOT pure black), variable-width 10-14px at 1024x1536 canvas, chunky brush with hand-drawn wobble + slight overshoot at joints
- Palette: muddied desaturated, saturation cap 30%, sepia/ochre tonal overlay
- Rendering: flat 3-color tonal stops (light/mid/shadow), subtle watercolor wash gradient, visible brush stroke texture

SUBJECT:
- Character context: chibi young-male wuxia cultivator (age 12-14), 3.5-4 head tall proportion (NOT lanky 5-head adult, NOT super-deformed)
- Body region: ISOLATED HUMAN TORSO ONLY (TRUNK = chest + belly + back), right-side profile, character facing right
- Cut boundary: clean horizontal cut at shoulder line (Y=0-3%) and hip line (Y=97-100%) — NO neck above, NO arms attached, NO hips/legs below

VIEW:
- Direction: right-side profile, character facing right
- Single isolated part, NO duplicate, NO turnaround, NO other directions in this image
- Pivot: top edge (Y=0%) = shoulder line (arm attach), bottom edge (Y=100%) = hip line (leg attach)

ANATOMICAL ANCHORS (% of sprite height, top=0% bottom=100%):
- Shoulder line: Y=0-3% (top edge cut)
- V-neck collar: Y=2-12%, X=20-50% (collar visible at top edge, opening points downward)
- Chest center (jade pendant cord): Y=18-28%, X=35-55%
- Cloud sigil embroidered on chest: Y=25-38%, X=20-45% (~1/8 of torso area)
- Waist (sash bow knot at right side): Y=70-82%, X=70-95% (right waist visible in side view)
- Sash ribbon ends drape: Y=82-95%
- Hip line: Y=97-100% (bottom edge cut, robe ends here)

COMPOSITION:
- Cylindrical narrow trunk shape, ~1.5 head-heights tall (chibi cute build, NOT broad superhero)
- Width consistent top-to-bottom with slight waist taper at sash ≤15%, NO flaring
- Top edge clean horizontal at shoulder height (where arm attaches separately)
- Bottom edge clean horizontal at hip height (where leg attaches separately)
- NO sleeves, NO arms, NO shoulders extending past body width

CLOTHING / MATERIALS visible on this part:
- Cream V-neck wuxia kimono robe TIGHT TO TRUNK only, NO bell-flow
- Robe ends cleanly at hip line (Y=100%) — MUST NOT drape past hip
- Gold sash bow knot at right side of waist (Y=70-82%, X=70-95%), ribbon ends drape Y=82-95%
- Jade pendant on green-brown silk cord hanging on chest center (Y=18-28%, X=35-55%)
- Cloud sigil embroidered on chest (Y=25-38%, X=20-45%)
- V-neck collar visible at top edge (Y=2-12%, opening points down at center X=30-45%)

PALETTE LOCK (exact hex codes):
- Robe: highlight #e8d8b8 / mid #c8b094 / fold shadow #8a6f47
- Sash: MUTED DUSTY GOLD #a8884a / shadow #7a5a30 (NOT bright lemon yellow)
- Jade pendant + cloud sigil: #7a9078 / shadow #4a5a48
- Outline: sepia #1a1408 variable-width 10-14px

OUTPUT:
- Canvas: 1024x1536 PNG, transparent RGBA
- Target render dimensions: ~120x260px (model up-renders, post-process crops tight bbox)
- Composition: tight alpha bbox ~5px transparent padding, NO ground, NO drop shadow, NO border, NO color background

DO NOT INCLUDE:
- NO front view in this image
- NO back view in this image
- NO sleeves attached, NO arms attached, NO shoulders past body width
- NO bell-flow flaring robe, NO knee-length, NO mid-shin (robe ends at Y=100%)
- NO bright lemon yellow sash, NO neon yellow
- NO neck above shoulder line, NO head, NO hips/legs below hip line
- NO background scene elements
- NO super-deformed >1/3 head ratio, NO 5-head lanky adult
- NO anime eye sparkle, NO multi-color iris, NO eye highlight star, NO kawaii heart eyes
- NO smooth airbrush gradient, NO pure #000 outline, NO clean uniform thin vector line
- NO saturated bright colors, NO lemon yellow, NO neon orange
- NO watermark, NO text, NO grid, NO border, NO blurry edges, NO anatomy errors
```

##### §E/arm_left.png — target ~80×200, canvas 600×1350

```
GOAL: Atomic puppet rig sprite — arm_left.png (E direction, right-side profile context (this arm is the LEFT arm = far-side from camera)), input ready for game sprite asset pipeline.

STYLE:
- Visual medium: hand-painted painterly illustration, Don't Starve Together × Chinese wuxia cultivation fusion
- Outline: Klei sepia-tinted ink #1a1408 (NOT pure black), variable-width 8-12px (scaled for smaller canvas region) at 1024x1536 canvas, chunky brush with hand-drawn wobble + slight overshoot at joints
- Palette: muddied desaturated, saturation cap 30%, sepia/ochre tonal overlay
- Rendering: flat 3-color tonal stops (light/mid/shadow), subtle watercolor wash gradient, visible brush stroke texture

SUBJECT:
- Character context: chibi young-male wuxia cultivator (age 12-14), 3.5-4 head tall proportion (NOT lanky 5-head adult, NOT super-deformed)
- Body region: ISOLATED UPPER ARM ONLY (shoulder cap to elbow joint), right-side profile context (this arm is the LEFT arm = far-side from camera)
- Cut boundary: clean cut at shoulder cap (Y=0-3%) and elbow joint (Y=97-100%) — NO torso, NO forearm, NO hand, NO sleeve below cuff

VIEW:
- Direction: right-side profile context (this arm is the LEFT arm = far-side from camera)
- Single isolated part, NO duplicate, NO turnaround, NO other directions in this image
- Pivot: top edge (Y=0%) = shoulder cap (torso attach), bottom edge (Y=100%) = elbow (forearm attach)

ANATOMICAL ANCHORS (% of sprite height, top=0% bottom=100%):
- Shoulder cap: Y=0-3% (top cut)
- Sleeve covers full upper arm: Y=0-100% (cream kimono fabric, tight cylinder, no cuff visible at this part — cuff belongs to forearm)
- Elbow cut: Y=97-100% (bottom edge)
- Width: ~25-30% of head-width (narrow chibi)
- Length: 1.0-1.2 head-heights total

COMPOSITION:
- Cylindrical limb segment, narrow chibi proportion, vertical orientation
- Sleeve TIGHT to arm cylinder shape, NO bell-flow, NO flare, NO wide opening
- Sleeve covers full upper arm (cream wuxia kimono fabric)
- Width consistent top-to-bottom, NO tapering
- NO hand, NO fingers (those belong to forearm part)

CLOTHING / MATERIALS visible on this part:
- Cream kimono sleeve TIGHT cylinder, full coverage Y=0-100%

PALETTE LOCK (exact hex codes):
- Sleeve cream: highlight #e8d8b8 / mid #c8b094 / fold shadow #8a6f47
- Outline: sepia #1a1408 variable-width 8-12px (scaled for smaller canvas region)

OUTPUT:
- Canvas: 1024x1536 PNG, transparent RGBA
- Target render dimensions: ~80x200px (model up-renders, post-process crops tight bbox)
- Composition: tight alpha bbox ~5px transparent padding, NO ground, NO drop shadow, NO border, NO color background

DO NOT INCLUDE:
- NO front view in this image
- NO back view in this image
- NO forearm, NO hand, NO fingers, NO mitten fist (those are forearm part)
- NO torso attached, NO shoulder past body width
- NO bell-flow flaring sleeve
- NO elbow bend (this is upper arm only — straight cylinder)
- NO cuff trim band visible (cuff is forearm part, NOT upper arm)
- NO super-deformed >1/3 head ratio, NO 5-head lanky adult
- NO anime eye sparkle, NO multi-color iris, NO eye highlight star, NO kawaii heart eyes
- NO smooth airbrush gradient, NO pure #000 outline, NO clean uniform thin vector line
- NO saturated bright colors, NO lemon yellow, NO neon orange
- NO watermark, NO text, NO grid, NO border, NO blurry edges, NO anatomy errors
```

##### §E/arm_right.png — target ~80×200, canvas 600×1350

```
GOAL: Atomic puppet rig sprite — arm_right.png (E direction, right-side profile context (this arm is the RIGHT arm = near-side / camera-side)), input ready for game sprite asset pipeline.

STYLE:
- Visual medium: hand-painted painterly illustration, Don't Starve Together × Chinese wuxia cultivation fusion
- Outline: Klei sepia-tinted ink #1a1408 (NOT pure black), variable-width 8-12px (scaled for smaller canvas region) at 1024x1536 canvas, chunky brush with hand-drawn wobble + slight overshoot at joints
- Palette: muddied desaturated, saturation cap 30%, sepia/ochre tonal overlay
- Rendering: flat 3-color tonal stops (light/mid/shadow), subtle watercolor wash gradient, visible brush stroke texture

SUBJECT:
- Character context: chibi young-male wuxia cultivator (age 12-14), 3.5-4 head tall proportion (NOT lanky 5-head adult, NOT super-deformed)
- Body region: ISOLATED UPPER ARM ONLY (shoulder cap to elbow joint), right-side profile context (this arm is the RIGHT arm = near-side / camera-side)
- Cut boundary: clean cut at shoulder cap (Y=0-3%) and elbow joint (Y=97-100%) — NO torso, NO forearm, NO hand, NO sleeve below cuff

VIEW:
- Direction: right-side profile context (this arm is the RIGHT arm = near-side / camera-side)
- Single isolated part, NO duplicate, NO turnaround, NO other directions in this image
- Pivot: top edge (Y=0%) = shoulder cap, bottom edge (Y=100%) = elbow

ANATOMICAL ANCHORS (% of sprite height, top=0% bottom=100%):
- Shoulder cap: Y=0-3%
- Sleeve full coverage Y=0-100%
- Elbow cut: Y=97-100%
- Width: ~25-30% of head-width
- Length: 1.0-1.2 head-heights

COMPOSITION:
- Cylindrical limb segment, narrow chibi, vertical orientation
- Sleeve TIGHT to arm cylinder, NO bell-flow
- Right arm = near-side from camera in E view (slightly more visible than far-side)
- Sleeve covers full upper arm
- NO hand, NO fingers

CLOTHING / MATERIALS visible on this part:
- Cream kimono sleeve TIGHT cylinder, full coverage Y=0-100%

PALETTE LOCK (exact hex codes):
- Sleeve cream: #e8d8b8 / #c8b094 / #8a6f47
- Outline: sepia #1a1408 variable-width 8-12px (scaled for smaller canvas region)

OUTPUT:
- Canvas: 1024x1536 PNG, transparent RGBA
- Target render dimensions: ~80x200px (model up-renders, post-process crops tight bbox)
- Composition: tight alpha bbox ~5px transparent padding, NO ground, NO drop shadow, NO border, NO color background

DO NOT INCLUDE:
- NO front view in this image
- NO back view in this image
- NO forearm, NO hand, NO fingers, NO mitten fist
- NO torso attached
- NO bell-flow flaring sleeve
- NO elbow bend (upper arm only — straight cylinder)
- NO cuff trim band visible
- NO super-deformed >1/3 head ratio, NO 5-head lanky adult
- NO anime eye sparkle, NO multi-color iris, NO eye highlight star, NO kawaii heart eyes
- NO smooth airbrush gradient, NO pure #000 outline, NO clean uniform thin vector line
- NO saturated bright colors, NO lemon yellow, NO neon orange
- NO watermark, NO text, NO grid, NO border, NO blurry edges, NO anatomy errors
```

##### §E/forearm_left.png — target ~70×220, canvas 540×1080

```
GOAL: Atomic puppet rig sprite — forearm_left.png (E direction, right-side profile context (LEFT forearm = far-side)), input ready for game sprite asset pipeline.

STYLE:
- Visual medium: hand-painted painterly illustration, Don't Starve Together × Chinese wuxia cultivation fusion
- Outline: Klei sepia-tinted ink #1a1408 (NOT pure black), variable-width 8-12px (scaled for smaller canvas region) at 1024x1536 canvas, chunky brush with hand-drawn wobble + slight overshoot at joints
- Palette: muddied desaturated, saturation cap 30%, sepia/ochre tonal overlay
- Rendering: flat 3-color tonal stops (light/mid/shadow), subtle watercolor wash gradient, visible brush stroke texture

SUBJECT:
- Character context: chibi young-male wuxia cultivator (age 12-14), 3.5-4 head tall proportion (NOT lanky 5-head adult, NOT super-deformed)
- Body region: ISOLATED FOREARM + HAND (lower arm from elbow to closed-mitten chibi fist), right-side profile context (LEFT forearm = far-side)
- Cut boundary: clean cut at elbow joint at Y=0-3% (top) — NO upper arm, NO torso, NO bicep

VIEW:
- Direction: right-side profile context (LEFT forearm = far-side)
- Single isolated part, NO duplicate, NO turnaround, NO other directions in this image
- Pivot: top edge (Y=0%) = elbow (upper arm attach), bottom edge (Y=100%) = mitten fist tip (final segment)

ANATOMICAL ANCHORS (% of sprite height, top=0% bottom=100%):
- Elbow cut: Y=0-3% (top edge)
- Cream sleeve fabric: Y=0-70% (covers ~70% of forearm, from elbow to wrist start)
- Brown cuff trim band at wrist: Y=70-78% (occupies 8% of forearm length, NOT thinner, NOT lower)
- Skin-tone closed mitten fist: Y=78-100% (~22% of forearm length, hand visible past cuff)
- Length: 1.2-1.4 head-heights total
- Width: ~25-28% of head-width (slightly narrower than upper arm)

COMPOSITION:
- Lower arm segment with closed-mitten chibi hand at bottom
- Cream kimono sleeve covers Y=0-70%, then brown cuff trim band Y=70-78%, then exposed skin mitten fist Y=78-100%
- Hand readable as small closed fist (single rounded lobe shape, NOT split into two lobes, NOT realistic 5-finger anatomy, NOT open splayed)
- Hand color = exposed skin tone (cuff stops at wrist Y=78%, hand visible from Y=78-100%)
- Brown cuff band thickness = exactly 8% of forearm length (NOT a thin line, NOT thick band)

CLOTHING / MATERIALS visible on this part:
- Cream kimono sleeve: Y=0-70% (covers proximal 70% of forearm)
- Brown cuff trim band wrapping at wrist: Y=70-78% (8% of length)
- Skin-tone closed mitten fist past cuff: Y=78-100%

PALETTE LOCK (exact hex codes):
- Sleeve cream: #e8d8b8 / #c8b094 / #8a6f47
- Cuff trim: #8a6f47 / shadow #5a4030
- Hand skin: highlight #c8a884 / mid #a08868 / shadow #5a4828
- Outline: sepia #1a1408 variable-width 8-12px (scaled for smaller canvas region)

OUTPUT:
- Canvas: 1024x1536 PNG, transparent RGBA
- Target render dimensions: ~70x220px (model up-renders, post-process crops tight bbox)
- Composition: tight alpha bbox ~5px transparent padding, NO ground, NO drop shadow, NO border, NO color background

DO NOT INCLUDE:
- NO front view in this image
- NO back view in this image
- NO upper arm, NO bicep, NO shoulder, NO torso
- NO realistic detailed 5-finger anatomy (mitten fist only, single rounded lobe)
- NO open splayed hand (closed fist preferred)
- NO double-lobe peach-shape mitten (single rounded fist preferred)
- NO bell-flow flaring sleeve
- NO weapon, NO held item (gen separately if needed)
- NO cuff at very bottom edge (cuff is at Y=70-78%, NOT at Y=85-95%)
- NO cuff thinner than 8% of forearm length
- NO super-deformed >1/3 head ratio, NO 5-head lanky adult
- NO anime eye sparkle, NO multi-color iris, NO eye highlight star, NO kawaii heart eyes
- NO smooth airbrush gradient, NO pure #000 outline, NO clean uniform thin vector line
- NO saturated bright colors, NO lemon yellow, NO neon orange
- NO watermark, NO text, NO grid, NO border, NO blurry edges, NO anatomy errors
```

##### §E/forearm_right.png — target ~70×220, canvas 540×1080

```
GOAL: Atomic puppet rig sprite — forearm_right.png (E direction, right-side profile context (RIGHT forearm = near-side)), input ready for game sprite asset pipeline.

STYLE:
- Visual medium: hand-painted painterly illustration, Don't Starve Together × Chinese wuxia cultivation fusion
- Outline: Klei sepia-tinted ink #1a1408 (NOT pure black), variable-width 8-12px (scaled for smaller canvas region) at 1024x1536 canvas, chunky brush with hand-drawn wobble + slight overshoot at joints
- Palette: muddied desaturated, saturation cap 30%, sepia/ochre tonal overlay
- Rendering: flat 3-color tonal stops (light/mid/shadow), subtle watercolor wash gradient, visible brush stroke texture

SUBJECT:
- Character context: chibi young-male wuxia cultivator (age 12-14), 3.5-4 head tall proportion (NOT lanky 5-head adult, NOT super-deformed)
- Body region: ISOLATED FOREARM + HAND (lower arm from elbow to closed-mitten chibi fist), right-side profile context (RIGHT forearm = near-side)
- Cut boundary: clean cut at elbow joint at Y=0-3% (top) — NO upper arm, NO torso, NO bicep

VIEW:
- Direction: right-side profile context (RIGHT forearm = near-side)
- Single isolated part, NO duplicate, NO turnaround, NO other directions in this image
- Pivot: top edge (Y=0%) = elbow, bottom edge (Y=100%) = mitten fist tip

ANATOMICAL ANCHORS (% of sprite height, top=0% bottom=100%):
- Elbow cut: Y=0-3%
- Cream sleeve: Y=0-70%
- Brown cuff trim band: Y=70-78% (8% of length)
- Skin-tone mitten fist: Y=78-100%
- Length: 1.2-1.4 head-heights
- Width: ~25-28% of head-width

COMPOSITION:
- Lower arm + closed-mitten chibi hand
- Cream sleeve Y=0-70%, brown cuff Y=70-78%, mitten fist Y=78-100%
- Hand readable as single rounded fist (NOT 5-finger, NOT double-lobe)
- Right side = near-side from camera in E view
- Brown cuff band thickness = exactly 8% of length

CLOTHING / MATERIALS visible on this part:
- Cream kimono sleeve: Y=0-70%
- Brown cuff trim band: Y=70-78%
- Skin-tone closed mitten fist: Y=78-100%

PALETTE LOCK (exact hex codes):
- Sleeve cream: #e8d8b8 / #c8b094 / #8a6f47
- Cuff trim: #8a6f47 / shadow #5a4030
- Hand skin: highlight #c8a884 / mid #a08868 / shadow #5a4828
- Outline: sepia #1a1408 variable-width 8-12px (scaled for smaller canvas region)

OUTPUT:
- Canvas: 1024x1536 PNG, transparent RGBA
- Target render dimensions: ~70x220px (model up-renders, post-process crops tight bbox)
- Composition: tight alpha bbox ~5px transparent padding, NO ground, NO drop shadow, NO border, NO color background

DO NOT INCLUDE:
- NO front view in this image
- NO back view in this image
- NO upper arm, NO bicep, NO shoulder, NO torso
- NO realistic detailed 5-finger anatomy
- NO open splayed hand
- NO double-lobe peach-shape mitten
- NO bell-flow flaring sleeve
- NO weapon, NO held item
- NO cuff at very bottom edge (Y=70-78% required, NOT Y=85-95%)
- NO super-deformed >1/3 head ratio, NO 5-head lanky adult
- NO anime eye sparkle, NO multi-color iris, NO eye highlight star, NO kawaii heart eyes
- NO smooth airbrush gradient, NO pure #000 outline, NO clean uniform thin vector line
- NO saturated bright colors, NO lemon yellow, NO neon orange
- NO watermark, NO text, NO grid, NO border, NO blurry edges, NO anatomy errors
```

##### §E/leg_left.png — target ~95×225, canvas 660×1760

```
GOAL: Atomic puppet rig sprite — leg_left.png (E direction, right-side profile context (LEFT thigh = far-side)), input ready for game sprite asset pipeline.

STYLE:
- Visual medium: hand-painted painterly illustration, Don't Starve Together × Chinese wuxia cultivation fusion
- Outline: Klei sepia-tinted ink #1a1408 (NOT pure black), variable-width 8-14px (scaled to canvas) at 1024x1536 canvas, chunky brush with hand-drawn wobble + slight overshoot at joints
- Palette: muddied desaturated, saturation cap 30%, sepia/ochre tonal overlay
- Rendering: flat 3-color tonal stops (light/mid/shadow), subtle watercolor wash gradient, visible brush stroke texture

SUBJECT:
- Character context: chibi young-male wuxia cultivator (age 12-14), 3.5-4 head tall proportion (NOT lanky 5-head adult, NOT super-deformed)
- Body region: ISOLATED THIGH ONLY (hip joint to knee joint), right-side profile context (LEFT thigh = far-side)
- Cut boundary: clean cut at hip (Y=0-3%) and knee (Y=97-100%) — NO torso above, NO shin/calf below, NO foot, NO boot

VIEW:
- Direction: right-side profile context (LEFT thigh = far-side)
- Single isolated part, NO duplicate, NO turnaround, NO other directions in this image
- Pivot: top edge (Y=0%) = hip (torso attach), bottom edge (Y=100%) = knee (shin attach)

ANATOMICAL ANCHORS (% of sprite height, top=0% bottom=100%):
- Hip cut: Y=0-3%
- Charcoal trouser fabric: Y=0-100% (full coverage, tight cylinder)
- Knee cut: Y=97-100%
- Length: 1.5 head-heights
- Width: ~25-30% of head-width (narrow chibi)

COMPOSITION:
- Cylindrical thigh segment, narrow chibi proportion, vertical orientation
- Charcoal trousers TIGHT cylinder cut, NO bell-flare, NO hakama puff
- Width consistent top-to-bottom
- NO calf, NO shin, NO foot, NO boot (those belong to shin part)

CLOTHING / MATERIALS visible on this part:
- Charcoal trouser fabric covering full thigh Y=0-100%, tight cylinder

PALETTE LOCK (exact hex codes):
- Trouser charcoal: base #3a3530 / shadow #1a1812
- Outline: sepia #1a1408 variable-width 8-14px (scaled to canvas)

OUTPUT:
- Canvas: 1024x1536 PNG, transparent RGBA
- Target render dimensions: ~95x225px (model up-renders, post-process crops tight bbox)
- Composition: tight alpha bbox ~5px transparent padding, NO ground, NO drop shadow, NO border, NO color background

DO NOT INCLUDE:
- NO front view in this image
- NO back view in this image
- NO shin, NO calf, NO foot, NO boot, NO ankle
- NO torso, NO hip joint protrusion, NO bell-flare hakama
- NO knee bend (thigh only — straight cylinder)
- NO robe overlapping (robe ends at hip, leg starts after)
- NO super-deformed >1/3 head ratio, NO 5-head lanky adult
- NO anime eye sparkle, NO multi-color iris, NO eye highlight star, NO kawaii heart eyes
- NO smooth airbrush gradient, NO pure #000 outline, NO clean uniform thin vector line
- NO saturated bright colors, NO lemon yellow, NO neon orange
- NO watermark, NO text, NO grid, NO border, NO blurry edges, NO anatomy errors
```

##### §E/leg_right.png — target ~95×220, canvas 660×1760

```
GOAL: Atomic puppet rig sprite — leg_right.png (E direction, right-side profile context (RIGHT thigh = near-side)), input ready for game sprite asset pipeline.

STYLE:
- Visual medium: hand-painted painterly illustration, Don't Starve Together × Chinese wuxia cultivation fusion
- Outline: Klei sepia-tinted ink #1a1408 (NOT pure black), variable-width 8-14px (scaled to canvas) at 1024x1536 canvas, chunky brush with hand-drawn wobble + slight overshoot at joints
- Palette: muddied desaturated, saturation cap 30%, sepia/ochre tonal overlay
- Rendering: flat 3-color tonal stops (light/mid/shadow), subtle watercolor wash gradient, visible brush stroke texture

SUBJECT:
- Character context: chibi young-male wuxia cultivator (age 12-14), 3.5-4 head tall proportion (NOT lanky 5-head adult, NOT super-deformed)
- Body region: ISOLATED THIGH ONLY (hip to knee), right-side profile context (RIGHT thigh = near-side)
- Cut boundary: clean cut at hip (Y=0-3%) and knee (Y=97-100%)

VIEW:
- Direction: right-side profile context (RIGHT thigh = near-side)
- Single isolated part, NO duplicate, NO turnaround, NO other directions in this image
- Pivot: top edge (Y=0%) = hip, bottom edge (Y=100%) = knee

ANATOMICAL ANCHORS (% of sprite height, top=0% bottom=100%):
- Hip cut: Y=0-3%
- Trouser fabric: Y=0-100%
- Knee cut: Y=97-100%
- Length: 1.5 head-heights
- Width: ~25-30% of head-width

COMPOSITION:
- Cylindrical thigh, narrow chibi, vertical orientation
- Charcoal trousers TIGHT cylinder, NO bell-flare
- Width consistent top-to-bottom
- NO shin, NO foot below knee

CLOTHING / MATERIALS visible on this part:
- Charcoal trouser fabric Y=0-100%, tight cylinder

PALETTE LOCK (exact hex codes):
- Trouser charcoal: base #3a3530 / shadow #1a1812
- Outline: sepia #1a1408 variable-width 8-14px (scaled to canvas)

OUTPUT:
- Canvas: 1024x1536 PNG, transparent RGBA
- Target render dimensions: ~95x220px (model up-renders, post-process crops tight bbox)
- Composition: tight alpha bbox ~5px transparent padding, NO ground, NO drop shadow, NO border, NO color background

DO NOT INCLUDE:
- NO front view in this image
- NO back view in this image
- NO shin, NO calf, NO foot, NO boot
- NO torso, NO hip protrusion
- NO knee bend
- NO robe overlapping
- NO super-deformed >1/3 head ratio, NO 5-head lanky adult
- NO anime eye sparkle, NO multi-color iris, NO eye highlight star, NO kawaii heart eyes
- NO smooth airbrush gradient, NO pure #000 outline, NO clean uniform thin vector line
- NO saturated bright colors, NO lemon yellow, NO neon orange
- NO watermark, NO text, NO grid, NO border, NO blurry edges, NO anatomy errors
```

##### §E/shin_left.png — target ~95×210, canvas 600×1200

```
GOAL: Atomic puppet rig sprite — shin_left.png (E direction, right-side profile context (LEFT shin = far-side)), input ready for game sprite asset pipeline.

STYLE:
- Visual medium: hand-painted painterly illustration, Don't Starve Together × Chinese wuxia cultivation fusion
- Outline: Klei sepia-tinted ink #1a1408 (NOT pure black), variable-width 8-12px (scaled to canvas) at 1024x1536 canvas, chunky brush with hand-drawn wobble + slight overshoot at joints
- Palette: muddied desaturated, saturation cap 30%, sepia/ochre tonal overlay
- Rendering: flat 3-color tonal stops (light/mid/shadow), subtle watercolor wash gradient, visible brush stroke texture

SUBJECT:
- Character context: chibi young-male wuxia cultivator (age 12-14), 3.5-4 head tall proportion (NOT lanky 5-head adult, NOT super-deformed)
- Body region: ISOLATED LOWER LEG + BOOT (knee to bottom of boot sole), right-side profile context (LEFT shin = far-side)
- Cut boundary: clean cut at knee joint (Y=0-3%) — NO thigh, NO hip, NO torso

VIEW:
- Direction: right-side profile context (LEFT shin = far-side)
- Single isolated part, NO duplicate, NO turnaround, NO other directions in this image
- Pivot: top edge (Y=0%) = knee (thigh attach), bottom edge (Y=100%) = boot sole = ground line

ANATOMICAL ANCHORS (% of sprite height, top=0% bottom=100%):
- Knee cut: Y=0-3% (top edge)
- Charcoal trouser fabric: Y=0-65% (covers shin from knee to ankle)
- Brown leather ankle boot: Y=65-100% (lower 35%)
- Ankle strap visible at top of boot: Y=65-72% (cream-tan #a89878 line)
- Boot toe stitch (front feature, side profile shows toe pointing right): Y=88-95%, X=70-95%
- Boot sole horizontal line: Y=98-100% = ground line
- Length: 1.5 head-heights

COMPOSITION:
- Lower leg cylinder with brown leather ankle boot at bottom
- Charcoal trouser fabric covers Y=0-65% (shin proximal 65%)
- Brown leather boot wraps Y=65-100% (lower 35%)
- Ankle strap at Y=65-72% (top of boot, cream-tan #a89878)
- Cream-tan toe stitch on toe area (front-facing in E view)
- Slightly oversized chibi boot OK but NOT clown-foot exaggeration
- Boot sole horizontal at Y=100% = ground line (sprite bottom edge)
- Side profile shows boot from side, with toe pointing right (E direction)

CLOTHING / MATERIALS visible on this part:
- Charcoal trousers covering shin: Y=0-65%
- Brown leather ankle boot: Y=65-100%
- Cream-tan ankle strap at boot top: Y=65-72%
- Cream-tan toe stitch on toe area

PALETTE LOCK (exact hex codes):
- Trouser charcoal: #3a3530 / shadow #1a1812
- Boot leather: #5a4830 / shadow #3a2818
- Boot strap + toe stitch: #a89878
- Outline: sepia #1a1408 variable-width 8-12px (scaled to canvas)

OUTPUT:
- Canvas: 1024x1536 PNG, transparent RGBA
- Target render dimensions: ~95x210px (model up-renders, post-process crops tight bbox)
- Composition: tight alpha bbox ~5px transparent padding, NO ground, NO drop shadow, NO border, NO color background

DO NOT INCLUDE:
- NO front view in this image
- NO back view in this image
- NO thigh above knee, NO hip, NO torso
- NO barefoot — boot is ALWAYS visible at Y=65-100%
- NO clown-foot exaggerated boot, NO oversized comic boot
- NO high-heel, NO platform sole (chibi flat boot)
- NO laces detail (simple strap only)
- NO trouser extending past Y=65% (trouser MUST end at Y=65%, boot starts there)
- NO super-deformed >1/3 head ratio, NO 5-head lanky adult
- NO anime eye sparkle, NO multi-color iris, NO eye highlight star, NO kawaii heart eyes
- NO smooth airbrush gradient, NO pure #000 outline, NO clean uniform thin vector line
- NO saturated bright colors, NO lemon yellow, NO neon orange
- NO watermark, NO text, NO grid, NO border, NO blurry edges, NO anatomy errors
```

##### §E/shin_right.png — target ~110×210, canvas 600×1200

```
GOAL: Atomic puppet rig sprite — shin_right.png (E direction, right-side profile context (RIGHT shin = near-side)), input ready for game sprite asset pipeline.

STYLE:
- Visual medium: hand-painted painterly illustration, Don't Starve Together × Chinese wuxia cultivation fusion
- Outline: Klei sepia-tinted ink #1a1408 (NOT pure black), variable-width 8-12px (scaled to canvas) at 1024x1536 canvas, chunky brush with hand-drawn wobble + slight overshoot at joints
- Palette: muddied desaturated, saturation cap 30%, sepia/ochre tonal overlay
- Rendering: flat 3-color tonal stops (light/mid/shadow), subtle watercolor wash gradient, visible brush stroke texture

SUBJECT:
- Character context: chibi young-male wuxia cultivator (age 12-14), 3.5-4 head tall proportion (NOT lanky 5-head adult, NOT super-deformed)
- Body region: ISOLATED LOWER LEG + BOOT (knee to sole), right-side profile context (RIGHT shin = near-side)
- Cut boundary: clean cut at knee (Y=0-3%) — NO thigh, NO hip

VIEW:
- Direction: right-side profile context (RIGHT shin = near-side)
- Single isolated part, NO duplicate, NO turnaround, NO other directions in this image
- Pivot: top (Y=0%) = knee, bottom (Y=100%) = boot sole = ground line

ANATOMICAL ANCHORS (% of sprite height, top=0% bottom=100%):
- Knee cut: Y=0-3%
- Charcoal trouser: Y=0-65%
- Brown leather boot: Y=65-100%
- Ankle strap: Y=65-72%
- Boot toe stitch (right-pointing): Y=88-95%, X=70-95%
- Boot sole: Y=98-100% = ground line

COMPOSITION:
- Lower leg + boot, side profile
- Charcoal trouser Y=0-65%, brown boot Y=65-100%
- Cream-tan ankle strap Y=65-72% + toe stitch on toe
- Right side = near-side from camera, slightly larger silhouette than far-side
- Boot sole horizontal Y=100% = ground line

CLOTHING / MATERIALS visible on this part:
- Charcoal trousers Y=0-65%
- Brown leather boot Y=65-100%
- Cream-tan ankle strap Y=65-72% + toe stitch

PALETTE LOCK (exact hex codes):
- Trouser charcoal: #3a3530 / shadow #1a1812
- Boot leather: #5a4830 / shadow #3a2818
- Strap + stitch: #a89878
- Outline: sepia #1a1408 variable-width 8-12px (scaled to canvas)

OUTPUT:
- Canvas: 1024x1536 PNG, transparent RGBA
- Target render dimensions: ~110x210px (model up-renders, post-process crops tight bbox)
- Composition: tight alpha bbox ~5px transparent padding, NO ground, NO drop shadow, NO border, NO color background

DO NOT INCLUDE:
- NO front view in this image
- NO back view in this image
- NO thigh above knee, NO hip, NO torso
- NO barefoot
- NO clown-foot, NO oversized comic boot
- NO high-heel, NO platform sole
- NO trouser extending past Y=65%
- NO super-deformed >1/3 head ratio, NO 5-head lanky adult
- NO anime eye sparkle, NO multi-color iris, NO eye highlight star, NO kawaii heart eyes
- NO smooth airbrush gradient, NO pure #000 outline, NO clean uniform thin vector line
- NO saturated bright colors, NO lemon yellow, NO neon orange
- NO watermark, NO text, NO grid, NO border, NO blurry edges, NO anatomy errors
```

---

#### §N — North direction (back view, character walking away from camera)

> 6 required parts (head, torso, leg×2, shin×2). Arms/forearms auto-hidden by `PuppetAnimController.hideArmsInFrontBackView=true` — gen optional 4 parts only if needed for special poses.

##### §N/head.png — target ~200×220, canvas 1024×1024

```
GOAL: Atomic puppet rig sprite — head.png (N direction, back view, character facing AWAY from camera), input ready for game sprite asset pipeline.

STYLE:
- Visual medium: hand-painted painterly illustration, Don't Starve Together × Chinese wuxia cultivation fusion
- Outline: Klei sepia-tinted ink #1a1408 (NOT pure black), variable-width 10-14px at 1024x1024 canvas, chunky brush with hand-drawn wobble + slight overshoot at joints
- Palette: muddied desaturated, saturation cap 30%, sepia/ochre tonal overlay
- Rendering: flat 3-color tonal stops (light/mid/shadow), subtle watercolor wash gradient, visible brush stroke texture

SUBJECT:
- Character context: chibi young-male wuxia cultivator (age 12-14), 3.5-4 head tall proportion (NOT lanky 5-head adult, NOT super-deformed)
- Body region: ISOLATED HUMAN HEAD ONLY (back-of-head + hair), back view, character facing AWAY from camera
- Cut boundary: clean horizontal cut at back of jaw line at Y=95-100% — NO neck, NO collar, NO shoulders

VIEW:
- Direction: back view, character facing AWAY from camera
- Single isolated part, NO duplicate, NO turnaround, NO other directions in this image
- Pivot: bottom edge (Y=100%) = jaw cut (neck attach for parent torso)

ANATOMICAL ANCHORS (% of sprite height, top=0% bottom=100%):
- Hair bun crown (visible from rear): Y=0-15%, X=35-65% (centered)
- Ribbon trailing down from bun: Y=15-95%, X=35-65% (~1.5 head-heights long)
- Hair covers full skull from crown to nape: Y=15-95%
- Jaw line cut: Y=95-100%

COMPOSITION:
- Back of head view: NO face, NO eyes, NO mouth, NO nose visible
- Topknot bun on crown (Y=0-15%), tied with cream silk ribbon trailing down behind head
- Ribbon ends fluttering visible behind head (Y=15-95%)
- Hair covers full skull from crown down to nape
- Asymmetric forelock NOT visible (back view)
- Smooth back-of-head silhouette without visible hair sprigs

PALETTE LOCK (exact hex codes):
- Hair: ink-black base #2a2418 + highlight #4a4030
- Ribbon: cream #e8d8b8
- Outline: sepia #1a1408 variable-width 10-14px

OUTPUT:
- Canvas: 1024x1024 PNG, transparent RGBA
- Target render dimensions: ~200x220px (model up-renders, post-process crops tight bbox)
- Composition: tight alpha bbox ~5px transparent padding, NO ground, NO drop shadow, NO border, NO color background

DO NOT INCLUDE:
- NO face / eyes / mouth visible (back view shows back-of-head only)
- NO front view in this image
- NO side profile in this image
- NO neck below Y=100% jaw cut, NO collar, NO shoulders
- NO ear visible (back view hides ears)
- NO asymmetric forelock visible
- NO side profile cheek silhouette
- NO super-deformed >1/3 head ratio, NO 5-head lanky adult
- NO anime eye sparkle, NO multi-color iris, NO eye highlight star, NO kawaii heart eyes
- NO smooth airbrush gradient, NO pure #000 outline, NO clean uniform thin vector line
- NO saturated bright colors, NO lemon yellow, NO neon orange
- NO watermark, NO text, NO grid, NO border, NO blurry edges, NO anatomy errors
```

##### §N/torso.png — target ~140×280, canvas 1024×1536, TRUNK ONLY back view

```
GOAL: Atomic puppet rig sprite — torso.png (N direction, back view, character walking away from camera), input ready for game sprite asset pipeline.

STYLE:
- Visual medium: hand-painted painterly illustration, Don't Starve Together × Chinese wuxia cultivation fusion
- Outline: Klei sepia-tinted ink #1a1408 (NOT pure black), variable-width 10-14px at 1024x1536 canvas, chunky brush with hand-drawn wobble + slight overshoot at joints
- Palette: muddied desaturated, saturation cap 30%, sepia/ochre tonal overlay
- Rendering: flat 3-color tonal stops (light/mid/shadow), subtle watercolor wash gradient, visible brush stroke texture

SUBJECT:
- Character context: chibi young-male wuxia cultivator (age 12-14), 3.5-4 head tall proportion (NOT lanky 5-head adult, NOT super-deformed)
- Body region: ISOLATED HUMAN TORSO BACK (TRUNK back side only), back view, character walking away from camera
- Cut boundary: clean cut at shoulder (Y=0-3%) and hip (Y=97-100%) — NO neck, NO arms, NO hips/legs

VIEW:
- Direction: back view, character walking away from camera
- Single isolated part, NO duplicate, NO turnaround, NO other directions in this image
- Pivot: top (Y=0%) = shoulder, bottom (Y=100%) = hip

ANATOMICAL ANCHORS (% of sprite height, top=0% bottom=100%):
- Shoulder line: Y=0-3% (top cut)
- Cream kimono back panel: Y=0-100% (smooth back-side fabric)
- Sash bow ribbon ends visible at right waist (back glimpse): Y=70-90%, X=70-95%
- Hip line: Y=97-100% (bottom cut, robe ends here)

COMPOSITION:
- Back view of trunk, narrow chibi proportion ~1.5 head-heights
- Cream wuxia kimono back panel, smooth fabric
- Sash bow ribbon ends visible at right waist (knot is on right side, even in back view tail of bow drapes visible at Y=70-90%, X=70-95%)
- NO V-neck collar visible (collar is front feature)
- NO jade pendant or cloud sigil visible (those are front-of-chest features)
- Back-side of robe smooth without front detail

CLOTHING / MATERIALS visible on this part:
- Cream kimono back panel TIGHT to trunk Y=0-100%, hip-length
- Sash bow ribbon ends draping at right waist Y=70-90%, X=70-95%

PALETTE LOCK (exact hex codes):
- Robe: highlight #e8d8b8 / mid #c8b094 / fold shadow #8a6f47
- Sash: MUTED DUSTY GOLD #a8884a / shadow #7a5a30
- Outline: sepia #1a1408 variable-width 10-14px

OUTPUT:
- Canvas: 1024x1536 PNG, transparent RGBA
- Target render dimensions: ~140x280px (model up-renders, post-process crops tight bbox)
- Composition: tight alpha bbox ~5px transparent padding, NO ground, NO drop shadow, NO border, NO color background

DO NOT INCLUDE:
- NO face / eyes / mouth visible (back view shows back-of-head only)
- NO front view in this image
- NO side profile in this image
- NO V-neck collar (front feature only)
- NO jade pendant, NO cloud sigil (front features)
- NO face visible, NO eyes
- NO sleeves attached, NO arms
- NO neck above shoulder, NO head
- NO bell-flow robe, NO knee-length, NO mid-shin
- NO super-deformed >1/3 head ratio, NO 5-head lanky adult
- NO anime eye sparkle, NO multi-color iris, NO eye highlight star, NO kawaii heart eyes
- NO smooth airbrush gradient, NO pure #000 outline, NO clean uniform thin vector line
- NO saturated bright colors, NO lemon yellow, NO neon orange
- NO watermark, NO text, NO grid, NO border, NO blurry edges, NO anatomy errors
```

##### §N/arm_left.png + §N/arm_right.png + §N/forearm_left.png + §N/forearm_right.png

> **OPTIONAL** — auto-hidden in N view by `PuppetAnimController.hideArmsInFrontBackView=true`. Skip để tiết kiệm compute. Nếu vẫn muốn gen (cho special poses sau này), dùng SAME prompts như §E/arm_left, §E/arm_right, §E/forearm_left, §E/forearm_right nhưng đổi orientation thành back-view (mitten-style hand shows back of fist instead of palm side). Replace mọi mention "side profile right" → "back view orientation". Negative thêm `NO front view in this image, NO side profile`.

##### §N/leg_left.png — target ~95×225, canvas 660×1760

```
GOAL: Atomic puppet rig sprite — leg_left.png (N direction, back view (LEFT thigh from rear, appears on RIGHT side of image)), input ready for game sprite asset pipeline.

STYLE:
- Visual medium: hand-painted painterly illustration, Don't Starve Together × Chinese wuxia cultivation fusion
- Outline: Klei sepia-tinted ink #1a1408 (NOT pure black), variable-width 8-14px (scaled to canvas) at 1024x1536 canvas, chunky brush with hand-drawn wobble + slight overshoot at joints
- Palette: muddied desaturated, saturation cap 30%, sepia/ochre tonal overlay
- Rendering: flat 3-color tonal stops (light/mid/shadow), subtle watercolor wash gradient, visible brush stroke texture

SUBJECT:
- Character context: chibi young-male wuxia cultivator (age 12-14), 3.5-4 head tall proportion (NOT lanky 5-head adult, NOT super-deformed)
- Body region: ISOLATED THIGH BACK (hip to knee), back view (LEFT thigh from rear, appears on RIGHT side of image)
- Cut boundary: clean cut at hip (Y=0-3%) and knee (Y=97-100%) — NO torso, NO shin, NO foot

VIEW:
- Direction: back view (LEFT thigh from rear, appears on RIGHT side of image)
- Single isolated part, NO duplicate, NO turnaround, NO other directions in this image
- Pivot: top (Y=0%) = hip, bottom (Y=100%) = knee

ANATOMICAL ANCHORS (% of sprite height, top=0% bottom=100%):
- Hip cut: Y=0-3%
- Charcoal trouser back fabric: Y=0-100%
- Knee cut: Y=97-100%
- Length: 1.5 head-heights

COMPOSITION:
- Cylindrical thigh, narrow chibi proportion, charcoal trousers tight cylinder
- Back-of-leg view (no front knee detail)
- NO calf, NO shin, NO foot below knee

CLOTHING / MATERIALS visible on this part:
- Charcoal trouser fabric Y=0-100%, tight cylinder, back-of-thigh smooth

PALETTE LOCK (exact hex codes):
- Trouser charcoal: #3a3530 / shadow #1a1812
- Outline: sepia #1a1408 variable-width 8-14px (scaled to canvas)

OUTPUT:
- Canvas: 1024x1536 PNG, transparent RGBA
- Target render dimensions: ~95x225px (model up-renders, post-process crops tight bbox)
- Composition: tight alpha bbox ~5px transparent padding, NO ground, NO drop shadow, NO border, NO color background

DO NOT INCLUDE:
- NO face / eyes / mouth visible (back view shows back-of-head only)
- NO front view in this image
- NO side profile in this image
- NO shin, NO foot, NO boot
- NO torso, NO hip protrusion
- NO front-of-leg knee detail
- NO bell-flare hakama
- NO super-deformed >1/3 head ratio, NO 5-head lanky adult
- NO anime eye sparkle, NO multi-color iris, NO eye highlight star, NO kawaii heart eyes
- NO smooth airbrush gradient, NO pure #000 outline, NO clean uniform thin vector line
- NO saturated bright colors, NO lemon yellow, NO neon orange
- NO watermark, NO text, NO grid, NO border, NO blurry edges, NO anatomy errors
```

##### §N/leg_right.png — target ~95×225, canvas 660×1760

```
GOAL: Atomic puppet rig sprite — leg_right.png (N direction, back view (RIGHT thigh from rear, appears on LEFT side of image)), input ready for game sprite asset pipeline.

STYLE:
- Visual medium: hand-painted painterly illustration, Don't Starve Together × Chinese wuxia cultivation fusion
- Outline: Klei sepia-tinted ink #1a1408 (NOT pure black), variable-width 8-14px (scaled to canvas) at 1024x1536 canvas, chunky brush with hand-drawn wobble + slight overshoot at joints
- Palette: muddied desaturated, saturation cap 30%, sepia/ochre tonal overlay
- Rendering: flat 3-color tonal stops (light/mid/shadow), subtle watercolor wash gradient, visible brush stroke texture

SUBJECT:
- Character context: chibi young-male wuxia cultivator (age 12-14), 3.5-4 head tall proportion (NOT lanky 5-head adult, NOT super-deformed)
- Body region: ISOLATED THIGH BACK (hip to knee), back view (RIGHT thigh from rear, appears on LEFT side of image)
- Cut boundary: clean cut at hip (Y=0-3%) and knee (Y=97-100%)

VIEW:
- Direction: back view (RIGHT thigh from rear, appears on LEFT side of image)
- Single isolated part, NO duplicate, NO turnaround, NO other directions in this image
- Pivot: top (Y=0%) = hip, bottom (Y=100%) = knee

ANATOMICAL ANCHORS (% of sprite height, top=0% bottom=100%):
- Hip cut: Y=0-3%
- Charcoal trouser: Y=0-100%
- Knee cut: Y=97-100%
- Length: 1.5 head-heights

COMPOSITION:
- Cylindrical thigh, narrow chibi, charcoal trousers tight
- Back-of-leg view
- NO calf, NO shin below knee

CLOTHING / MATERIALS visible on this part:
- Charcoal trouser fabric Y=0-100%, tight cylinder

PALETTE LOCK (exact hex codes):
- Trouser charcoal: #3a3530 / shadow #1a1812
- Outline: sepia #1a1408 variable-width 8-14px (scaled to canvas)

OUTPUT:
- Canvas: 1024x1536 PNG, transparent RGBA
- Target render dimensions: ~95x225px (model up-renders, post-process crops tight bbox)
- Composition: tight alpha bbox ~5px transparent padding, NO ground, NO drop shadow, NO border, NO color background

DO NOT INCLUDE:
- NO face / eyes / mouth visible (back view shows back-of-head only)
- NO front view in this image
- NO side profile in this image
- NO shin, NO foot, NO boot
- NO torso
- NO front-of-leg detail
- NO bell-flare hakama
- NO super-deformed >1/3 head ratio, NO 5-head lanky adult
- NO anime eye sparkle, NO multi-color iris, NO eye highlight star, NO kawaii heart eyes
- NO smooth airbrush gradient, NO pure #000 outline, NO clean uniform thin vector line
- NO saturated bright colors, NO lemon yellow, NO neon orange
- NO watermark, NO text, NO grid, NO border, NO blurry edges, NO anatomy errors
```

##### §N/shin_left.png — target ~95×210, canvas 600×1200

```
GOAL: Atomic puppet rig sprite — shin_left.png (N direction, back view (LEFT shin from rear)), input ready for game sprite asset pipeline.

STYLE:
- Visual medium: hand-painted painterly illustration, Don't Starve Together × Chinese wuxia cultivation fusion
- Outline: Klei sepia-tinted ink #1a1408 (NOT pure black), variable-width 8-12px (scaled to canvas) at 1024x1536 canvas, chunky brush with hand-drawn wobble + slight overshoot at joints
- Palette: muddied desaturated, saturation cap 30%, sepia/ochre tonal overlay
- Rendering: flat 3-color tonal stops (light/mid/shadow), subtle watercolor wash gradient, visible brush stroke texture

SUBJECT:
- Character context: chibi young-male wuxia cultivator (age 12-14), 3.5-4 head tall proportion (NOT lanky 5-head adult, NOT super-deformed)
- Body region: ISOLATED LOWER LEG + BOOT (knee to sole), back view, back view (LEFT shin from rear)
- Cut boundary: clean cut at knee (Y=0-3%) — NO thigh, NO hip

VIEW:
- Direction: back view (LEFT shin from rear)
- Single isolated part, NO duplicate, NO turnaround, NO other directions in this image
- Pivot: top (Y=0%) = knee, bottom (Y=100%) = boot sole = ground

ANATOMICAL ANCHORS (% of sprite height, top=0% bottom=100%):
- Knee cut: Y=0-3%
- Charcoal trouser: Y=0-65%
- Brown leather boot: Y=65-100% (rear view)
- Boot heel visible at back: Y=85-98%, X=40-65% (instead of toe stitch which is front feature)
- Ankle strap (back view): Y=65-72%
- Boot sole: Y=98-100% = ground line

COMPOSITION:
- Lower leg + boot from rear
- Charcoal trouser Y=0-65% covers shin, brown boot Y=65-100% lower 35%
- Boot heel visible at back (instead of toe stitch which is front feature)
- Ankle strap may be visible at top of boot (Y=65-72%)
- Boot sole horizontal at Y=100% = ground line

CLOTHING / MATERIALS visible on this part:
- Charcoal trousers Y=0-65%
- Brown leather boot Y=65-100% from rear
- Boot heel visible at back
- Optional ankle strap Y=65-72%

PALETTE LOCK (exact hex codes):
- Trouser charcoal: #3a3530 / #1a1812
- Boot leather: #5a4830 / #3a2818
- Strap (if visible): #a89878
- Outline: sepia #1a1408 variable-width 8-12px (scaled to canvas)

OUTPUT:
- Canvas: 1024x1536 PNG, transparent RGBA
- Target render dimensions: ~95x210px (model up-renders, post-process crops tight bbox)
- Composition: tight alpha bbox ~5px transparent padding, NO ground, NO drop shadow, NO border, NO color background

DO NOT INCLUDE:
- NO face / eyes / mouth visible (back view shows back-of-head only)
- NO front view in this image
- NO side profile in this image
- NO thigh, NO hip, NO torso
- NO toe stitch (front feature only)
- NO barefoot
- NO clown-foot
- NO super-deformed >1/3 head ratio, NO 5-head lanky adult
- NO anime eye sparkle, NO multi-color iris, NO eye highlight star, NO kawaii heart eyes
- NO smooth airbrush gradient, NO pure #000 outline, NO clean uniform thin vector line
- NO saturated bright colors, NO lemon yellow, NO neon orange
- NO watermark, NO text, NO grid, NO border, NO blurry edges, NO anatomy errors
```

##### §N/shin_right.png — target ~110×210, canvas 600×1200

```
GOAL: Atomic puppet rig sprite — shin_right.png (N direction, back view (RIGHT shin from rear)), input ready for game sprite asset pipeline.

STYLE:
- Visual medium: hand-painted painterly illustration, Don't Starve Together × Chinese wuxia cultivation fusion
- Outline: Klei sepia-tinted ink #1a1408 (NOT pure black), variable-width 8-12px (scaled to canvas) at 1024x1536 canvas, chunky brush with hand-drawn wobble + slight overshoot at joints
- Palette: muddied desaturated, saturation cap 30%, sepia/ochre tonal overlay
- Rendering: flat 3-color tonal stops (light/mid/shadow), subtle watercolor wash gradient, visible brush stroke texture

SUBJECT:
- Character context: chibi young-male wuxia cultivator (age 12-14), 3.5-4 head tall proportion (NOT lanky 5-head adult, NOT super-deformed)
- Body region: ISOLATED LOWER LEG + BOOT, back view, back view (RIGHT shin from rear)
- Cut boundary: clean cut at knee (Y=0-3%) — NO thigh, NO hip

VIEW:
- Direction: back view (RIGHT shin from rear)
- Single isolated part, NO duplicate, NO turnaround, NO other directions in this image
- Pivot: top (Y=0%) = knee, bottom (Y=100%) = boot sole

ANATOMICAL ANCHORS (% of sprite height, top=0% bottom=100%):
- Knee cut: Y=0-3%
- Charcoal trouser: Y=0-65%
- Brown boot: Y=65-100% (rear view)
- Boot heel: Y=85-98%, X=40-65%
- Ankle strap: Y=65-72%
- Boot sole: Y=98-100%

COMPOSITION:
- Lower leg + boot from rear
- Charcoal trouser Y=0-65%, brown boot Y=65-100%
- Boot heel visible at back
- Ankle strap top of boot (Y=65-72%)
- Boot sole = ground line (Y=100%)

CLOTHING / MATERIALS visible on this part:
- Charcoal trousers Y=0-65%
- Brown leather boot Y=65-100%, heel back
- Optional ankle strap Y=65-72%

PALETTE LOCK (exact hex codes):
- Trouser: #3a3530 / #1a1812
- Boot: #5a4830 / #3a2818
- Strap: #a89878
- Outline: sepia #1a1408 variable-width 8-12px (scaled to canvas)

OUTPUT:
- Canvas: 1024x1536 PNG, transparent RGBA
- Target render dimensions: ~110x210px (model up-renders, post-process crops tight bbox)
- Composition: tight alpha bbox ~5px transparent padding, NO ground, NO drop shadow, NO border, NO color background

DO NOT INCLUDE:
- NO face / eyes / mouth visible (back view shows back-of-head only)
- NO front view in this image
- NO side profile in this image
- NO thigh, NO hip
- NO toe stitch (front)
- NO barefoot
- NO clown-foot
- NO super-deformed >1/3 head ratio, NO 5-head lanky adult
- NO anime eye sparkle, NO multi-color iris, NO eye highlight star, NO kawaii heart eyes
- NO smooth airbrush gradient, NO pure #000 outline, NO clean uniform thin vector line
- NO saturated bright colors, NO lemon yellow, NO neon orange
- NO watermark, NO text, NO grid, NO border, NO blurry edges, NO anatomy errors
```

---

#### §S — South direction (front view, character facing camera)

> 6 required parts (head, torso, leg×2, shin×2). Arms/forearms auto-hidden bởi `PuppetAnimController.hideArmsInFrontBackView=true` — optional 4 parts.

##### §S/head.png — target ~210×230, canvas 1024×1024

```
GOAL: Atomic puppet rig sprite — head.png (S direction, front view, character facing camera), input ready for game sprite asset pipeline.

STYLE:
- Visual medium: hand-painted painterly illustration, Don't Starve Together × Chinese wuxia cultivation fusion
- Outline: Klei sepia-tinted ink #1a1408 (NOT pure black), variable-width 10-14px at 1024x1024 canvas, chunky brush with hand-drawn wobble + slight overshoot at joints
- Palette: muddied desaturated, saturation cap 30%, sepia/ochre tonal overlay
- Rendering: flat 3-color tonal stops (light/mid/shadow), subtle watercolor wash gradient, visible brush stroke texture

SUBJECT:
- Character context: chibi young-male wuxia cultivator (age 12-14), 3.5-4 head tall proportion (NOT lanky 5-head adult, NOT super-deformed)
- Body region: ISOLATED HUMAN HEAD ONLY (skull + face + hair), front view, front view, character facing camera
- Cut boundary: clean horizontal cut at jaw line at Y=95-100% — NO neck, NO collar, NO shoulders

VIEW:
- Direction: front view, character facing camera
- Single isolated part, NO duplicate, NO turnaround, NO other directions in this image
- Pivot: bottom (Y=100%) = jaw cut

ANATOMICAL ANCHORS (% of sprite height, top=0% bottom=100%):
- Hair bun crown (top of head): Y=0-15%, X=40-60% (centered)
- Forelock falling forward: Y=10-30%, X=35-65% (asymmetric, off-center to one side)
- Eyebrows (TWO strokes): Y=36-42%, X=30-45% (left) and X=55-70% (right), spaced ~1/3 head width apart
- Eyes (TWO SOLID DOT pupils): Y=42-50%, X=32-42% (left eye) and X=58-68% (right eye), each 3-5px
- Nose tip (center): Y=52-58%, X=46-54%
- Mouth line (center): Y=62-68%, X=42-58%
- Cheek apple blush (optional): Y=50-58%, X=20-35% (left) and X=65-80% (right)
- Ribbon trailing back from bun: visible peeking on either side of head
- Jaw line cut: Y=95-100%

COMPOSITION:
- Front view of head: TWO SOLID DOT pupil eyes (each 3-5px, color #1a1408, NO iris, NO sclera, NO eyelash, NO highlight star)
- Eyes spaced ~1/3 head width apart, symmetric on left/right halves
- Tiny angle nose suggestion in middle (5-8px brush stroke)
- Single line mouth (1-2px below nose, color #6a3a28)
- Two single-stroke eyebrows (10-15px, color #2a2418), one above each eye
- Hair: topknot bun on crown (visible as bun shape on top), cream silk ribbon (#e8d8b8) trailing back behind head (partly visible from front sides)
- Single asymmetric forelock falling forward at front of forehead, ~1 head width long, off-center to one side
- Optional subtle cheek blush #c89878 at 40% opacity on both cheek apples
- Ears NOT visible (hair covers, front view)

PALETTE LOCK (exact hex codes):
- Skin: highlight #c8a884 / mid #a08868 / shadow #5a4828
- Hair: ink-black #2a2418 + highlight #4a4030
- Ribbon: cream #e8d8b8
- Lip line: #6a3a28
- Outline: sepia #1a1408 variable-width 10-14px

OUTPUT:
- Canvas: 1024x1024 PNG, transparent RGBA
- Target render dimensions: ~210x230px (model up-renders, post-process crops tight bbox)
- Composition: tight alpha bbox ~5px transparent padding, NO ground, NO drop shadow, NO border, NO color background

DO NOT INCLUDE:
- NO back view in this image
- NO side profile in this image
- NO neck below Y=100% jaw cut, NO shoulders, NO torso
- NO single eye only (front view = TWO eyes)
- NO anime detailed iris, NO almond eye shape
- NO ear visible (hair covers in front view)
- NO open mouth surprised
- NO super-deformed >1/3 head ratio, NO 5-head lanky adult
- NO anime eye sparkle, NO multi-color iris, NO eye highlight star, NO kawaii heart eyes
- NO smooth airbrush gradient, NO pure #000 outline, NO clean uniform thin vector line
- NO saturated bright colors, NO lemon yellow, NO neon orange
- NO watermark, NO text, NO grid, NO border, NO blurry edges, NO anatomy errors
```

##### §S/torso.png — target ~140×290, canvas 1024×1536, TRUNK ONLY front view

```
GOAL: Atomic puppet rig sprite — torso.png (S direction, front view, character facing camera), input ready for game sprite asset pipeline.

STYLE:
- Visual medium: hand-painted painterly illustration, Don't Starve Together × Chinese wuxia cultivation fusion
- Outline: Klei sepia-tinted ink #1a1408 (NOT pure black), variable-width 10-14px at 1024x1536 canvas, chunky brush with hand-drawn wobble + slight overshoot at joints
- Palette: muddied desaturated, saturation cap 30%, sepia/ochre tonal overlay
- Rendering: flat 3-color tonal stops (light/mid/shadow), subtle watercolor wash gradient, visible brush stroke texture

SUBJECT:
- Character context: chibi young-male wuxia cultivator (age 12-14), 3.5-4 head tall proportion (NOT lanky 5-head adult, NOT super-deformed)
- Body region: ISOLATED HUMAN TORSO FRONT (TRUNK front side only), front view, character facing camera
- Cut boundary: clean cut at shoulder (Y=0-3%) and hip (Y=97-100%) — NO neck, NO arms, NO hips/legs

VIEW:
- Direction: front view, character facing camera
- Single isolated part, NO duplicate, NO turnaround, NO other directions in this image
- Pivot: top (Y=0%) = shoulder, bottom (Y=100%) = hip

ANATOMICAL ANCHORS (% of sprite height, top=0% bottom=100%):
- Shoulder line: Y=0-3% (top cut)
- V-neck collar opening (V-shape pointing down): Y=2-15%, X=35-65% (centered)
- Cloud sigil embroidered on chest: Y=18-32%, X=15-40% (viewer's left side, ~1/8 of torso area)
- Jade pendant on cord (center vertical axis): Y=20-40%, X=45-55%
- Sash bow knot at right waist (in mirrored S front view, knot may appear on viewer's left): Y=68-78%, X=15-40% OR X=60-85%
- Sash ribbon ends drape: Y=78-92%
- Hip line: Y=97-100% (bottom cut, robe ends here)

COMPOSITION:
- Front view of trunk, narrow chibi ~1.5 head-heights
- V-neck collar VISIBLE at top center (Y=2-15%), opening points downward
- Jade pendant on green-brown silk cord hanging on chest center vertical axis (Y=20-40%, X=45-55%)
- Cloud sigil embroidered on chest (Y=18-32%, viewer's left side X=15-40%)
- Gold sash bow knot at right waist (Y=68-78%), ribbon ends drape Y=78-92%
- Cream kimono robe TIGHT to trunk, hip-length (ends at Y=100%)
- NO arms attached, NO sleeves visible

CLOTHING / MATERIALS visible on this part:
- Cream V-neck wuxia kimono front Y=0-100%, tight to trunk, hip-length
- V-neck collar opening at top center (Y=2-15%)
- Jade pendant on cord (Y=20-40%, center)
- Cloud sigil on chest (Y=18-32%, viewer's left)
- Gold sash bow knot at waist (Y=68-78%) with draping ribbon ends (Y=78-92%)

PALETTE LOCK (exact hex codes):
- Robe: highlight #e8d8b8 / mid #c8b094 / fold shadow #8a6f47
- Sash: MUTED DUSTY GOLD #a8884a / shadow #7a5a30
- Jade pendant + cloud sigil: #7a9078 / shadow #4a5a48
- Outline: sepia #1a1408 variable-width 10-14px

OUTPUT:
- Canvas: 1024x1536 PNG, transparent RGBA
- Target render dimensions: ~140x290px (model up-renders, post-process crops tight bbox)
- Composition: tight alpha bbox ~5px transparent padding, NO ground, NO drop shadow, NO border, NO color background

DO NOT INCLUDE:
- NO back view in this image
- NO side profile in this image
- NO sleeves attached, NO arms, NO shoulders past body width
- NO bell-flow robe, NO knee-length (ends at Y=100%), NO mid-shin
- NO bright lemon yellow sash
- NO neck above shoulder line, NO head
- NO two pendants, NO multiple sigils (one of each)
- NO super-deformed >1/3 head ratio, NO 5-head lanky adult
- NO anime eye sparkle, NO multi-color iris, NO eye highlight star, NO kawaii heart eyes
- NO smooth airbrush gradient, NO pure #000 outline, NO clean uniform thin vector line
- NO saturated bright colors, NO lemon yellow, NO neon orange
- NO watermark, NO text, NO grid, NO border, NO blurry edges, NO anatomy errors
```

##### §S/arm_left.png + §S/arm_right.png + §S/forearm_left.png + §S/forearm_right.png

> **OPTIONAL** — auto-hidden in S view by `PuppetAnimController.hideArmsInFrontBackView=true`. Skip để tiết kiệm compute. Nếu cần gen, dùng SAME prompts như §E/arm_left, §E/arm_right, §E/forearm_left, §E/forearm_right nhưng đổi orientation thành front-view (mitten-style hand shows palm-side or front of fist). Replace "side profile right" → "front view orientation". Negative thêm `NO back view in this image, NO side profile`.

##### §S/leg_left.png — target ~95×225, canvas 660×1760

```
GOAL: Atomic puppet rig sprite — leg_left.png (S direction, front view (viewer's RIGHT side of image)), input ready for game sprite asset pipeline.

STYLE:
- Visual medium: hand-painted painterly illustration, Don't Starve Together × Chinese wuxia cultivation fusion
- Outline: Klei sepia-tinted ink #1a1408 (NOT pure black), variable-width 8-14px (scaled to canvas) at 1024x1536 canvas, chunky brush with hand-drawn wobble + slight overshoot at joints
- Palette: muddied desaturated, saturation cap 30%, sepia/ochre tonal overlay
- Rendering: flat 3-color tonal stops (light/mid/shadow), subtle watercolor wash gradient, visible brush stroke texture

SUBJECT:
- Character context: chibi young-male wuxia cultivator (age 12-14), 3.5-4 head tall proportion (NOT lanky 5-head adult, NOT super-deformed)
- Body region: ISOLATED THIGH FRONT (hip to knee), front view (viewer's RIGHT side of image)
- Cut boundary: clean cut at hip (Y=0-3%) and knee (Y=97-100%) — NO torso, NO shin, NO foot

VIEW:
- Direction: front view (viewer's RIGHT side of image)
- Single isolated part, NO duplicate, NO turnaround, NO other directions in this image
- Pivot: top (Y=0%) = hip, bottom (Y=100%) = knee

ANATOMICAL ANCHORS (% of sprite height, top=0% bottom=100%):
- Hip cut: Y=0-3%
- Charcoal trouser front fabric: Y=0-100%
- Knee cut: Y=97-100%
- Length: 1.5 head-heights

COMPOSITION:
- Cylindrical thigh, narrow chibi
- Charcoal trousers tight cylinder, NO bell-flare
- Front-of-thigh view (knee front detail not yet visible — straight cylinder)
- NO shin below knee

CLOTHING / MATERIALS visible on this part:
- Charcoal trouser fabric Y=0-100%, tight cylinder

PALETTE LOCK (exact hex codes):
- Trouser charcoal: #3a3530 / shadow #1a1812
- Outline: sepia #1a1408 variable-width 8-14px (scaled to canvas)

OUTPUT:
- Canvas: 1024x1536 PNG, transparent RGBA
- Target render dimensions: ~95x225px (model up-renders, post-process crops tight bbox)
- Composition: tight alpha bbox ~5px transparent padding, NO ground, NO drop shadow, NO border, NO color background

DO NOT INCLUDE:
- NO back view in this image
- NO side profile in this image
- NO shin, NO foot, NO boot
- NO torso
- NO bell-flare hakama
- NO super-deformed >1/3 head ratio, NO 5-head lanky adult
- NO anime eye sparkle, NO multi-color iris, NO eye highlight star, NO kawaii heart eyes
- NO smooth airbrush gradient, NO pure #000 outline, NO clean uniform thin vector line
- NO saturated bright colors, NO lemon yellow, NO neon orange
- NO watermark, NO text, NO grid, NO border, NO blurry edges, NO anatomy errors
```

##### §S/leg_right.png — target ~95×225, canvas 660×1760

```
GOAL: Atomic puppet rig sprite — leg_right.png (S direction, front view (viewer's LEFT side of image)), input ready for game sprite asset pipeline.

STYLE:
- Visual medium: hand-painted painterly illustration, Don't Starve Together × Chinese wuxia cultivation fusion
- Outline: Klei sepia-tinted ink #1a1408 (NOT pure black), variable-width 8-14px (scaled to canvas) at 1024x1536 canvas, chunky brush with hand-drawn wobble + slight overshoot at joints
- Palette: muddied desaturated, saturation cap 30%, sepia/ochre tonal overlay
- Rendering: flat 3-color tonal stops (light/mid/shadow), subtle watercolor wash gradient, visible brush stroke texture

SUBJECT:
- Character context: chibi young-male wuxia cultivator (age 12-14), 3.5-4 head tall proportion (NOT lanky 5-head adult, NOT super-deformed)
- Body region: ISOLATED THIGH FRONT (hip to knee), front view (viewer's LEFT side of image)
- Cut boundary: clean cut at hip (Y=0-3%) and knee (Y=97-100%)

VIEW:
- Direction: front view (viewer's LEFT side of image)
- Single isolated part, NO duplicate, NO turnaround, NO other directions in this image
- Pivot: top (Y=0%) = hip, bottom (Y=100%) = knee

ANATOMICAL ANCHORS (% of sprite height, top=0% bottom=100%):
- Hip cut: Y=0-3%
- Trouser fabric: Y=0-100%
- Knee cut: Y=97-100%
- Length: 1.5 head-heights

COMPOSITION:
- Cylindrical thigh, narrow chibi
- Charcoal trousers tight cylinder
- Front view, NO shin

CLOTHING / MATERIALS visible on this part:
- Charcoal trouser fabric Y=0-100%, tight cylinder

PALETTE LOCK (exact hex codes):
- Trouser charcoal: #3a3530 / shadow #1a1812
- Outline: sepia #1a1408 variable-width 8-14px (scaled to canvas)

OUTPUT:
- Canvas: 1024x1536 PNG, transparent RGBA
- Target render dimensions: ~95x225px (model up-renders, post-process crops tight bbox)
- Composition: tight alpha bbox ~5px transparent padding, NO ground, NO drop shadow, NO border, NO color background

DO NOT INCLUDE:
- NO back view in this image
- NO side profile in this image
- NO shin, NO foot, NO boot
- NO torso
- NO bell-flare
- NO super-deformed >1/3 head ratio, NO 5-head lanky adult
- NO anime eye sparkle, NO multi-color iris, NO eye highlight star, NO kawaii heart eyes
- NO smooth airbrush gradient, NO pure #000 outline, NO clean uniform thin vector line
- NO saturated bright colors, NO lemon yellow, NO neon orange
- NO watermark, NO text, NO grid, NO border, NO blurry edges, NO anatomy errors
```

##### §S/shin_left.png — target ~95×210, canvas 600×1200

```
GOAL: Atomic puppet rig sprite — shin_left.png (S direction, front view (LEFT shin)), input ready for game sprite asset pipeline.

STYLE:
- Visual medium: hand-painted painterly illustration, Don't Starve Together × Chinese wuxia cultivation fusion
- Outline: Klei sepia-tinted ink #1a1408 (NOT pure black), variable-width 8-12px (scaled to canvas) at 1024x1536 canvas, chunky brush with hand-drawn wobble + slight overshoot at joints
- Palette: muddied desaturated, saturation cap 30%, sepia/ochre tonal overlay
- Rendering: flat 3-color tonal stops (light/mid/shadow), subtle watercolor wash gradient, visible brush stroke texture

SUBJECT:
- Character context: chibi young-male wuxia cultivator (age 12-14), 3.5-4 head tall proportion (NOT lanky 5-head adult, NOT super-deformed)
- Body region: ISOLATED LOWER LEG + BOOT, front view, front view (LEFT shin)
- Cut boundary: clean cut at knee (Y=0-3%) — NO thigh, NO hip

VIEW:
- Direction: front view (LEFT shin)
- Single isolated part, NO duplicate, NO turnaround, NO other directions in this image
- Pivot: top (Y=0%) = knee, bottom (Y=100%) = boot sole = ground

ANATOMICAL ANCHORS (% of sprite height, top=0% bottom=100%):
- Knee cut: Y=0-3%
- Charcoal trouser: Y=0-65%
- Brown leather boot: Y=65-100% (front view)
- Ankle strap (cream-tan): Y=65-72%
- Cream-tan toe stitch on toe area: Y=88-95%, X=35-65% (centered, front-facing)
- Boot sole: Y=98-100% = ground line

COMPOSITION:
- Lower leg + boot, front view
- Charcoal trouser Y=0-65% covers shin from knee down
- Brown leather boot Y=65-100%
- Cream-tan toe stitch VISIBLE on toe area Y=88-95%, X=35-65% (front feature)
- Ankle strap visible at top of boot (Y=65-72%)
- Boot sole horizontal at Y=100% = ground line

CLOTHING / MATERIALS visible on this part:
- Charcoal trousers Y=0-65%
- Brown leather boot Y=65-100%
- Cream-tan toe stitch Y=88-95% + ankle strap Y=65-72%

PALETTE LOCK (exact hex codes):
- Trouser: #3a3530 / shadow #1a1812
- Boot leather: #5a4830 / shadow #3a2818
- Strap + stitch: #a89878
- Outline: sepia #1a1408 variable-width 8-12px (scaled to canvas)

OUTPUT:
- Canvas: 1024x1536 PNG, transparent RGBA
- Target render dimensions: ~95x210px (model up-renders, post-process crops tight bbox)
- Composition: tight alpha bbox ~5px transparent padding, NO ground, NO drop shadow, NO border, NO color background

DO NOT INCLUDE:
- NO back view in this image
- NO side profile in this image
- NO thigh, NO hip, NO torso
- NO barefoot
- NO clown-foot
- NO boot heel from rear (heel is N feature, this is S front)
- NO trouser extending past Y=65%
- NO super-deformed >1/3 head ratio, NO 5-head lanky adult
- NO anime eye sparkle, NO multi-color iris, NO eye highlight star, NO kawaii heart eyes
- NO smooth airbrush gradient, NO pure #000 outline, NO clean uniform thin vector line
- NO saturated bright colors, NO lemon yellow, NO neon orange
- NO watermark, NO text, NO grid, NO border, NO blurry edges, NO anatomy errors
```

##### §S/shin_right.png — target ~110×210, canvas 600×1200

```
GOAL: Atomic puppet rig sprite — shin_right.png (S direction, front view (RIGHT shin)), input ready for game sprite asset pipeline.

STYLE:
- Visual medium: hand-painted painterly illustration, Don't Starve Together × Chinese wuxia cultivation fusion
- Outline: Klei sepia-tinted ink #1a1408 (NOT pure black), variable-width 8-12px (scaled to canvas) at 1024x1536 canvas, chunky brush with hand-drawn wobble + slight overshoot at joints
- Palette: muddied desaturated, saturation cap 30%, sepia/ochre tonal overlay
- Rendering: flat 3-color tonal stops (light/mid/shadow), subtle watercolor wash gradient, visible brush stroke texture

SUBJECT:
- Character context: chibi young-male wuxia cultivator (age 12-14), 3.5-4 head tall proportion (NOT lanky 5-head adult, NOT super-deformed)
- Body region: ISOLATED LOWER LEG + BOOT, front view, front view (RIGHT shin)
- Cut boundary: clean cut at knee (Y=0-3%)

VIEW:
- Direction: front view (RIGHT shin)
- Single isolated part, NO duplicate, NO turnaround, NO other directions in this image
- Pivot: top (Y=0%) = knee, bottom (Y=100%) = boot sole

ANATOMICAL ANCHORS (% of sprite height, top=0% bottom=100%):
- Knee cut: Y=0-3%
- Charcoal trouser: Y=0-65%
- Brown boot: Y=65-100%
- Ankle strap: Y=65-72%
- Toe stitch: Y=88-95%, X=35-65%
- Boot sole: Y=98-100%

COMPOSITION:
- Lower leg + boot, front view
- Charcoal trouser Y=0-65%, brown boot Y=65-100%
- Cream-tan toe stitch on toe Y=88-95%
- Ankle strap top of boot Y=65-72%
- Boot sole = ground line Y=100%

CLOTHING / MATERIALS visible on this part:
- Charcoal trousers Y=0-65%
- Brown leather boot Y=65-100%
- Toe stitch Y=88-95% + ankle strap Y=65-72%

PALETTE LOCK (exact hex codes):
- Trouser: #3a3530 / #1a1812
- Boot: #5a4830 / #3a2818
- Strap + stitch: #a89878
- Outline: sepia #1a1408 variable-width 8-12px (scaled to canvas)

OUTPUT:
- Canvas: 1024x1536 PNG, transparent RGBA
- Target render dimensions: ~110x210px (model up-renders, post-process crops tight bbox)
- Composition: tight alpha bbox ~5px transparent padding, NO ground, NO drop shadow, NO border, NO color background

DO NOT INCLUDE:
- NO back view in this image
- NO side profile in this image
- NO thigh, NO hip
- NO barefoot
- NO clown-foot
- NO rear heel detail
- NO trouser extending past Y=65%
- NO super-deformed >1/3 head ratio, NO 5-head lanky adult
- NO anime eye sparkle, NO multi-color iris, NO eye highlight star, NO kawaii heart eyes
- NO smooth airbrush gradient, NO pure #000 outline, NO clean uniform thin vector line
- NO saturated bright colors, NO lemon yellow, NO neon orange
- NO watermark, NO text, NO grid, NO border, NO blurry edges, NO anatomy errors
```

---

### §3.4 MEGA-PROMPT (1-shot batch tool, GPT image 2.0 structured)

Cho ai dùng Comfy workflow / API script loop / batch tool. 1 preamble cover toàn bộ 30 PNG, structured theo GPT-2 framework với numerical anchors:

```
GOAL: Generate 30 PNG sprites for atomic puppet rig (Chibi Wuxia × Soft-DST cutout animation style). All RGBA transparent BG. Used as game sprite asset for Unity puppet rig system.

STYLE (apply to ALL 30 PNGs):
- Visual medium: hand-painted painterly illustration, Don't Starve Together × Chinese wuxia cultivation fusion
- Outline: Klei sepia-tinted ink #1a1408 (NOT pure black), variable-width 8-14px scaled to canvas, chunky brush with hand-drawn wobble + slight overshoot at joints
- Palette: muddied desaturated, EVERY color saturation cap 30%, sepia/ochre tonal overlay
- Rendering: flat 3-color tonal stops per material (light/mid/shadow), subtle watercolor wash gradient, visible brush stroke texture inside fills

SUBJECT (constant across all 30):
- Character: chibi cute young-male wuxia cultivator, age 12-14 (Webber-leaning chibi)
- Proportion: 3.5-4 head-tall (NOT lanky 5-head Wilson adult, NOT super-deformed >1/3 head)
- Head ratio: ~25-28% of total body height
- Build: shoulders narrow 1.0-1.2× head width, arms reach mid-thigh, hands as small mitten fists at sleeve cuff, slightly oversized chibi boot sole

WUXIA IDENTITY LOCK (every relevant part shows applicable items):
- Cream V-neck wuxia kimono robe, HIP-LENGTH (highlight #e8d8b8 / mid #c8b094 / fold #8a6f47)
- TIGHT sleeves with brown cuff trim band at wrist Y=70-78% on forearm (#8a6f47 / #5a4030, occupying 8% of forearm length)
- Gold sash bow knot at right waist Y=68-78% on torso (MUTED #a8884a / #7a5a30, NOT bright lemon yellow)
- Jade pendant + cloud sigil on chest Y=18-40% on torso (#7a9078 / #4a5a48)
- Ink-black topknot bun Y=0-15% on head (#2a2418 + #4a4030, NOT pure #000) with cream silk ribbon trailing (#e8d8b8) + asymmetric forelock at front
- Warm-charcoal trousers Y=0-65% on shin (#3a3530 / #1a1812)
- Brown leather boots Y=65-100% on shin (#5a4830 / #3a2818) with cream-tan strap and toe stitch (#a89878)
- Skin muddied warm tan (#c8a884 / #a08868 / #5a4828)

FACE MINIMALISM LOCK (head sprites only):
- SOLID DOT pupil eyes #1a1408 (3-5px each at canvas Y=42-50%, NO iris, NO sclera, NO eyelash, NO highlight star)
- Single line mouth #6a3a28 ~1-2px at Y=58-68%
- Tiny angle nose ~5-8px brush stroke at Y=48-58%
- Single-stroke eyebrows ~10-15px color #2a2418 at Y=36-42%
- Optional subtle cheek blush #c89878 @ 40% opacity (apple area only, Y=50-58%)

PARTS LIST (10 body parts × 3 directions = 30 PNG):
- 10 parts per direction: head, torso, arm_left, arm_right, forearm_left, forearm_right, leg_left, leg_right, shin_left, shin_right
- 3 directions: E (right-side profile), N (back view), S (front view)
- W = flipX of E (skip — sprite system auto-flips at runtime)

CRITICAL ATOMIC-SYMBOL COMPOSITION RULES (BREAK = REJECT each PNG):
1. ONE part = ONE anatomical region. NO baked-in adjacent parts.
2. Torso = TRUNK ONLY. NO sleeves, NO arms, NO shoulders flaring, NO neck above Y=0%, NO legs below Y=100%.
3. Arm = upper arm ONLY (shoulder cap Y=0% to elbow Y=100%). Cylindrical, sleeve TIGHT to limb, NO bell-flow, NO hand visible, NO cuff visible (cuff is forearm part).
4. Forearm = lower arm + mitten-style chibi hand. Cream sleeve Y=0-70%, brown cuff trim band Y=70-78% (8% of length), skin-tone mitten fist Y=78-100%. Hand readable as single rounded fist (NOT 5-finger, NOT double-lobe peach shape).
5. Leg = thigh ONLY (hip Y=0% to knee Y=100%). Cylindrical, trousers tight, NO foot, NO shin, NO bell-flare hakama.
6. Shin = lower leg + boot. Trouser Y=0-65%, brown boot Y=65-100%, ankle strap Y=65-72%, sole Y=100% = ground line.
7. Head = skull + face + hair ONLY. Bottom edge Y=100% = clean horizontal cut at jaw line. NO neck visible past jawline.
8. Each PNG: tight alpha bbox ~5px padding, RGBA transparent BG, no shadow, no ground, no background, no border, no watermark.

DIRECTION QUIRKS:
- E (East): right-side profile facing right. ONE dot eye visible at Y=42-50% X=58-68%, ear visible at X=35-48%, single line mouth, asymmetric forelock front.
- N (North): back view. NO face, NO eyes, NO mouth — back-of-head hair Y=0-95% + ribbon trailing only. Boot heel visible at back of shin.
- S (South): front view facing camera. TWO dot eyes visible (X=32-42% and X=58-68%), V-neck collar visible on torso (Y=2-15%), jade pendant + cloud sigil centered on chest. Boot toe stitch visible at front of shin (Y=88-95%, X=35-65%).

OUTPUT FORMAT (all 30 PNGs):
- Canvas: per-part target sizes (head 1024x1024, torso 1024x1536, arm/forearm/leg/shin 1024x1536 with crop tight bbox to target ~600-660 wide post-process)
- Background: transparent RGBA
- File format: PNG with alpha channel
- Tight alpha bbox ~5px padding around subject

DO NOT INCLUDE (apply to ALL 30):
- NO super-deformed (>1/3 head)
- NO 5-head adult lanky proportion
- NO anime sparkle, NO kawaii heart eyes, NO multi-color iris, NO eye highlight star
- NO anime gloss highlight stripes on hair
- NO smooth airbrush gradient, NO clean uniform thin vector line, NO vector cel-shaded
- NO pure black #000 outline (sepia #1a1408 only)
- NO saturated bright colors, NO neon yellow, NO lemon yellow sash
- NO bell-flow flaring sleeves
- NO knee-length robe (drape past Y=58% on torso), NO mid-shin robe, NO ankle-length robe (HIP-LENGTH only)
- NO clown-foot exaggerated boot
- NO floating hands disconnected from arm
- NO realistic detailed 5-finger hand (mitten only), NO double-lobe peach-shape mitten
- NO cuff at very bottom of forearm (cuff at Y=70-78%, NOT Y=85-95%)
- NO trouser extending past Y=65% on shin (trouser MUST end at Y=65%, boot starts there)
- NO shadow on ground, NO ground, NO background, NO border, NO watermark, NO text
- NO extra body parts, NO duplicate limbs, NO anatomy errors

POST-GEN VALIDATION:
After gen, ALWAYS run validator: `python3 .agents/scripts/validate_player_art.py` from repo root.
It checks:
- File naming convention (E/head.png, N/torso.png, etc.)
- Image dimensions vs target table
- RGBA alpha bbox tightness (≤5px padding)
- Composition violations (silhouette only — flag oversized canvases, missing alpha, etc.)

Per-part detailed prompts: see §3.3 above (each part has self-contained fenced block with specific composition + palette + numerical anchors + negative).
Practical asset-side rules: see `DST_RIG_ASSET_GUIDE.md`.
Visual signature: see `PLAYER_DST_REFERENCE.md`.
```

#### Recommended GPT-2 batch script (Python)

```python
import os
from openai import OpenAI

client = OpenAI()
OUT_DIR = "Assets/_Project/Art/Characters/player"
STYLE_REF = "Documentation/assets/style_refs/player_E_v2.png"

# 1. Master full-body (skip if already PASSed)
# 2. 30 atomic parts with style ref
PARTS = [
    ("E", "head"), ("E", "torso"), ("E", "arm_left"), ("E", "arm_right"),
    ("E", "forearm_left"), ("E", "forearm_right"),
    ("E", "leg_left"), ("E", "leg_right"), ("E", "shin_left"), ("E", "shin_right"),
    ("N", "head"), ("N", "torso"),
    ("N", "leg_left"), ("N", "leg_right"), ("N", "shin_left"), ("N", "shin_right"),
    ("S", "head"), ("S", "torso"),
    ("S", "leg_left"), ("S", "leg_right"), ("S", "shin_left"), ("S", "shin_right"),
]

PROMPT_TABLE = {
    ("E", "head"): "<paste §3.3/§E/head.png prompt here>",
    # ... (paste all 22 atomic prompts from §3.3)
}

for direction, part in PARTS:
    out_path = f"{OUT_DIR}/{direction}/{part}.png"
    os.makedirs(os.path.dirname(out_path), exist_ok=True)
    with open(STYLE_REF, "rb") as ref:
        result = client.images.edit(
            model="gpt-image-2",
            image=[ref],
            prompt=PROMPT_TABLE[(direction, part)],
            size="1024x1536" if part != "head" else "1024x1024",
            background="transparent",
            quality="high",
        )
    import base64
    with open(out_path, "wb") as f:
        f.write(base64.b64decode(result.data[0].b64_json))
    print(f"{direction}/{part} done")

print("Run python3 .agents/scripts/validate_player_art.py to verify")
```

---

### §3.5 References cho atomic gen

- Visual signature reference: [`PLAYER_DST_REFERENCE.md`](PLAYER_DST_REFERENCE.md)
- Full asset source-board workflow: [`PLAYER_FULL_ASSET_SOURCE_PROMPT.md`](PLAYER_FULL_ASSET_SOURCE_PROMPT.md)
- Source board → per-part extraction (image-edit / manual crop): [`PLAYER_SOURCE_BOARD_EXTRACTION.md`](PLAYER_SOURCE_BOARD_EXTRACTION.md)
- Practical rig rules: [`DST_RIG_ASSET_GUIDE.md`](DST_RIG_ASSET_GUIDE.md)
- Mechanical validator: `.agents/scripts/validate_player_art.py` (RGBA + bbox + dimensions per part)
- Anatomy spec source of truth: `Assets/_Project/Scripts/Core/PuppetPlaceholderSpec.cs` `RectFor(role)`

---

## §4 Tool-specific settings

### §4.1 Leonardo AI (Phoenix)

- Model: **Leonardo Phoenix**
- Aspect ratio: **2:3** (full-body style ref) hoặc **1:1** / role-specific (atomic parts theo §2 table)
- Image size: **1024×1536** (style ref) hoặc canvas theo §2
- Number of images: **4**
- Prompt Magic: **ON, strength HIGH**
- Style: **Painterly** hoặc trained Element `wilderness_cultivation_painterly_v1` (strength 0.7)
- Image Guidance: attach `Documentation/assets/style_refs/player_E_v2.png` strength **0.4** cho atomic parts (giữ identity, ép isolation per part)

### §4.2 GPT image 2.0 (`gpt-image-2`, OpenAI Apr 2026)

> **Recommended default cho repo này.** Best instruction-following, best for character consistency via multi-image edit endpoint, best for atomic part isolation (precise constraint following).

#### Model selection

| Model | Use case | Notes |
|---|---|---|
| `gpt-image-2` | Primary — high-quality character art, atomic parts | Latest (Apr 2026), supports flexible resolutions, `input_fidelity` always-high |
| `gpt-image-1.5` | Backup — backward compat | Fixed sizes 1024x1024 / 1024x1536 / 1536x1024, supports `input_fidelity="high"` |
| `gpt-image-1` | Legacy — tránh dùng cho new work | Fixed sizes, input_fidelity available |
| `gpt-image-1-mini` | Batch ideation low-stakes | Lower quality, cheaper, faster |

#### API parameters table

| Parameter | Value | Reason |
|---|---|---|
| `model` | `"gpt-image-2"` | Latest, best instruction-following |
| `size` | `"1024x1536"` (master, torso, limbs) / `"1024x1024"` (head) | Tight fit per anatomy |
| `quality` | `"high"` (final) / `"low"` (ideation) | Balance fidelity vs cost/latency |
| `background` | `"transparent"` | Required for puppet rig PNG |
| `output_format` | `"png"` | Required for transparent BG |
| `n` | `1` (default) or `2-4` for variant comparison | Batch ideation |
| `output_compression` | N/A for PNG | Only applies to JPEG/WEBP |

#### Image API — generate from text (no style ref)

```python
from openai import OpenAI
import base64

client = OpenAI()

result = client.images.generate(
    model="gpt-image-2",
    prompt=MASTER_PROMPT_OR_ATOMIC_PROMPT,  # paste §3.1 or §3.3 fenced block
    size="1024x1536",
    quality="high",
    background="transparent",
    output_format="png",
)

with open("output.png", "wb") as f:
    f.write(base64.b64decode(result.data[0].b64_json))
```

#### Image API — edit with style reference (RECOMMENDED for atomic parts)

Pass v2 PASS reference image + atomic part prompt. Model uses ref for identity consistency (skin tone / hair color / robe wash texture) while gen new isolated part.

```python
from openai import OpenAI

client = OpenAI()

with open("Documentation/assets/style_refs/player_E_v2.png", "rb") as ref:
    result = client.images.edit(
        model="gpt-image-2",
        image=[ref],
        prompt=ATOMIC_PROMPT,  # §3.3/§E/head fenced block
        size="1024x1024",
        background="transparent",
        quality="high",
    )
```

#### Responses API — multi-turn iteration (RECOMMENDED for ideation)

Use khi cần iterate "change only X, keep rest same". Auto-tracks image context.

```python
from openai import OpenAI

client = OpenAI()

# Turn 1: initial gen
response = client.responses.create(
    model="gpt-4.1",  # any model that supports image_generation tool
    input=MASTER_PROMPT,
    tools=[{"type": "image_generation", "background": "transparent"}],
)

# Turn 2: refine (model edits previous gen)
response2 = client.responses.create(
    model="gpt-4.1",
    input="Change only the sash color to muddier dusty gold #a8884a, keep everything else exactly the same.",
    previous_response_id=response.id,
    tools=[{"type": "image_generation", "background": "transparent"}],
)
```

#### Iteration tips for GPT-2

- **Bias prompt order**: STYLE first (anchor visual), then SUBJECT, then DETAILS, then CONSTRAINTS, then OUTPUT. GPT-2 weights early sections more.
- **Use numerical anchors** (Y=X%-Y% of canvas, exact px ratios) over vague modifiers ("most of", "~70%"). GPT-2 follows hard numbers much better than fuzzy language.
- **Repeat critical constraints**: GPT-2 follows instructions well — repeating "chibi 3.5 head, NOT 5 head" 2-3 times across STYLE + SUBJECT + REINFORCE sections is fine and helps.
- **Multi-image input** for character consistency: pass v2 PASS as ref image. Model will preserve identity across 30 atomic parts.
- **Surgical iteration**: use `change only X, keep everything else exactly the same` syntax for refinement. Faster than full re-gen.
- **Fail-fast**: if first gen has 5-head proportion or anime drift, do NOT iterate — start fresh with stronger REINFORCE block. GPT-2 can lock to early frame style.
- **Quality tier**: start with `quality="low"` for ideation (faster), promote winning seed to `quality="high"` for final.
- **Anime drift mitigation**: prepend `"Strict instructions: chibi proportion 3.5-4 head, NOT 5 head, NOT anime, NOT kawaii."` to start of prompt if drift observed.

### §4.3 Midjourney v6.1+

- Append `--ar 2:3 --niji 0` (TẮT niji vì niji = anime drift)
- Append `--stylize 250` (lower stylize = follow prompt sát hơn)
- Append `--no anime, kawaii, sparkle, bright yellow, knee-length robe, 5 heads tall`
- Character consistency: `--cref <URL ảnh style ref> --cw 80` (cw = character weight; 80 = strong style match, allow detail variance)

### §4.4 NanoBanana / Imagen 3

- Paste full prompt + negative
- Aspect: **2:3** portrait (style ref)
- Style preset: **Illustration / Painterly** (NOT anime, NOT 3D)

### §4.5 Stable Diffusion (local)

- ControlNet `reference_only` + IPAdapter Plus FaceID
- Style reference weight 0.7–0.85
- Negative embedding: paste §5 master
- Sampler: DPM++ 2M Karras, 30 steps, CFG 7

---

## §5 Negative prompts master

> Paste vào negative field (Leonardo / Midjourney `--no` / SD negative). GPT-image: prepend `"Avoid: ..."` vào cuối prompt vì không có separate field.

```
no anime sparkle, no kawaii heart eyes, no multi-color iris, no detailed
eyelash, no eye highlight star, no smooth airbrush gradient, no pure black
#000 outline, no clean uniform thin vector line outline, no saturated
bright colors, no lemon yellow, no neon orange, no neon yellow, no anime
gloss highlight stripes on hair, no cute open mouth surprised expression,
no smile teeth, no bell-flow sleeves, no flaring sleeves, no wide sleeve
opening, no robe past hip, no knee-length robe, no mid-shin robe, no
ankle-length robe, no long flowing robe drape, no 5 head tall, no 6 head
tall, no lanky teenager, no adult proportion, no superhero proportion,
no super-deformed mega-chibi 1 to 1 head body ratio, no clown-foot oversized
boot, no floating hands, no hands hidden inside sleeve completely, no
shadow on ground, no ground, no background, no environment, no border,
no border frame, no watermark, no signature, no text, no extra body parts,
no duplicate limbs, no twin character, no turnaround sheet, no multiple
views, no front view in this image, no back view in this image, no anatomy
errors, no blurry edges, no anti-alias bleeding past outline.
```

---

## §6 Acceptance test workflow

**Bắt buộc** chạy trước khi gen 30 atomic parts. Nếu master prompt §3.1 không đạt acceptance, KHÔNG đi tiếp — sẽ ra atomic parts inconsistent với rig.

### §6.1 Acceptance checklist (10 boxes — cần ≥ 7/10 PASS)

- [ ] **Proportion** = chibi 3.5–4 head tall (NOT 5+ head, NOT lanky teen, NOT adult)
- [ ] **Robe** = hip-length (NOT past hip / knee / shin / ankle)
- [ ] **Sleeves** tight to arm với cuff trim band (NO bell-flow / NO flaring)
- [ ] **Wuxia outfit full set**: cream V-neck kimono + bow knot sash + jade pendant + cloud sigil + topknot + cream ribbon trailing + asymmetric forelock + brown leather boot + cream toe stitch ALL visible
- [ ] **Sash gold** = muted dusty `#a8884a` family (NOT bright lemon yellow `#f0c020`)
- [ ] **Outline** = sepia chunky variable-width 8–16px (NOT thin clean vector line / NOT pure black)
- [ ] **Saturation** muddied ≤30% (skin reads "muddied tan" not "warm orange", hair reads "warm-dark" not "pure black")
- [ ] **Eye** = single small black SOLID DOT pupil (NOT detailed almond iris / NOT anime / NOT sparkle)
- [ ] **Hand** visible as mitten fist at sleeve cuff (NOT hidden / NOT floating / NOT detailed 5-finger anatomy)
- [ ] **Background** transparent RGBA, NO ground shadow, NO border, NO color BG

**≥ 7/10 PASS** → save `Documentation/assets/style_refs/player_E_v2.png` → proceed gen 30 atomic parts.
**< 7/10 FAIL** → regen với điều chỉnh:
- Fail proportion → tăng emphasis "chibi 3.5 head" lên 5–6 lần trong prompt, thêm ref `Webber from Don't Starve`
- Fail robe length → thêm `"robe MUST end exactly at hip line, like a short jacket, not like a long dress"`
- Fail outline → thêm `"thick painterly brush outline like Don't Starve Together Wilson character"`
- Fail eye anime drift → tăng emphasis `"single solid black dot pupil ONLY, no iris no sclera no eyelash"`
- Fail saturation → tăng emphasis `"muddied desaturated palette saturation cap 30 percent, sepia overlay"` 2x

### §6.2 Workflow end-to-end

```
1. Read §1 (style) + §2 (anatomy) + §3 (master prompt) + §5 (negative)
2. Pick tool (§4) — Leonardo Phoenix recommended for first iteration
3. Paste §3.1 master prompt + §5 negative
4. Generate 4 variations
5. Score each against §6.1 checklist (10 boxes)
6. ≥ 7/10 → save best as Documentation/assets/style_refs/player_E_v2.png
   < 7/10 → regen with §6.1 troubleshooting
7. Optional but recommended: gen 1 source board via `PLAYER_FULL_ASSET_SOURCE_PROMPT.md`
8. Use player_E_v2.png or source board as --cref / IP-Adapter input
9. Gen 30 atomic parts from §3.3 in this file
10. Save to Assets/_Project/Art/Characters/player/{E,N,S}/{part}.png
11. Run: python3 .agents/scripts/validate_player_art.py
12. Re-bootstrap MainScene: Tools → Wilderness Cultivation → Bootstrap Default Scene
13. Verify rig in Play mode (idle/walk/attack animations)
```

---

## §7 Future entities — TODO

Player v2 đã LOCKED (May 2026). Các entity khác (mob / NPC / resource / item / tile / VFX) sẽ regen theo workflow tương tự § 6 SAU KHI mỗi entity có style ref riêng PASS acceptance test.

| Entity | Status | Owner branch |
|---|---|---|
| Player (Cultivation Hero) | ✓ v2 LOCKED — 10/10 PASS | this PR |
| Wolf (Hung Lang) | TODO — regen v2 sau | future PR |
| FoxSpirit (Linh Hồ) | TODO | future PR |
| Rabbit (Linh Thố) | TODO | future PR |
| Boar (Hắc Trư) | TODO | future PR |
| DeerSpirit (Linh Lộc) | TODO | future PR |
| Boss (Hắc Vương) | TODO | future PR |
| Crow (Quạ Đen) | TODO | future PR |
| Bat (Dơi Đêm) | TODO | future PR |
| Snake (Thanh Xà) | TODO | future PR |
| VendorNPC (Lão Tiên Sinh) | TODO | future PR |
| CompanionNPC (Linh Nhi) | TODO | future PR |
| Resources / items / tiles / VFX | TODO | future PR — xem `prompts/tileset.txt` cho tile workflow |

**Khi regen mỗi entity:**

1. Viết master full-body style-ref prompt (template tương tự §3.1 nhưng adapt cho anatomy entity đó).
2. Chạy acceptance checklist §6.1 (adapt cho entity — vd mob skip wuxia outfit checks, thay bằng "fantasy hint" check như glowing eye, qi mist, bone marker).
3. ≥ 7/10 PASS → save `Documentation/assets/style_refs/{entity}_E_v2.png`.
4. Gen 30 atomic parts theo anatomy spec §2.
5. Drop vào `Assets/_Project/Art/{Mobs|Characters}/{entity}/{E,N,S}/{part}.png`.
6. Append entity section vào AI_PROMPTS.md (1 PR riêng per entity — atomic, easy review).

**Tile / item / VFX workflow** đã có sẵn ở `prompts/`:
- `prompts/tileset.txt` — 12 prompt ground tile seamless 64×64 cho 3 biome (Leonardo native tile mode)
- `prompts/tileset_gpt.txt` — same 12 tile, adapted cho GPT-image-1 (cần Photopea seam fix ~70% tile)
- `prompts/gpt_workflow.md` — GPT image 2.0 workflow với seam fix step-by-step
- `prompts/hero.txt` — 6 hero scene cho biome anchor + Element training data

---

## §8 DST animation feature parity

`PuppetAnimController` ráp 30 PNG cho 1 character/mob ⇒ rig chạy được:

| Animation | Out-of-box? | Parts cần | Code state |
| --- | --- | --- | --- |
| **Idle** (subtle bob) | ✓ | base 30 | đang chạy |
| **Walk** (4-frame leg cycle, biped/quadruped) | ✓ | base 30 | đang chạy |
| **Attack** (arm rotation around shoulder) | ✓ | base 30 | đang chạy |
| **Hit flash** (color overlay) | ✓ | base 30 | đang chạy |
| **Death** (collapse via rotation) | ✓ | base 30 | đang chạy |
| **Eat / Channel cast** | ✗ | base 30 + face_eating.png + face_channel.png swap layer | cần code: face-swap component |
| **Sleep** | ✗ | base 30 + body_sleep.png OR full-body 90° rotate + Z particle | cần code: state machine extension |
| **Mining / Chopping / Fishing** | ✗ | base 30 + tool_axe/pickaxe/rod overlay sprites + item-in-hand binding | cần code: ItemHoldComponent |
| **Sit / Crouch** | ✗ | base 30 + leg_sit_left/right.png alt sprites | cần code: alt-sprite swap |
| **Speak / emote** (eyebrow, mouth shape) | ✗ | base 30 + face_*.png swap layer (calm/happy/angry/talk) | cần code: face-swap |

**DST có cả những animation trên** vì Klei Spriter rig support multiple sprite slots per part + state-driven swap. Để đạt parity, repo cần thêm:

1. **Face-swap layer** cho Player/NPC: `face_calm.png`, `face_eating.png`, `face_talking.png`, `face_pain.png`, `face_dead.png` — 5 swap sprites overlay lên `head.png`. Code: thêm `FaceSwapComponent` đính vào head transform với index swap by state.
2. **Item-in-hand layer**: `tool_*.png` overlay sprites bind to `forearm_left`/`forearm_right` tip. Code: `ItemHoldComponent` với offset transform per tool.
3. **Alt-sprite slots** cho leg in sit/crouch states: `leg_sit_left.png` etc. Code: state machine query active sprite per puppet role.
4. **Particle layer** cho channel/cast/sleep/level-up: extend `Vfx/` với prefab particles bind to spawn anchor on character.

**Khi nào cần làm?** Sau khi 12 entity base art set được hoàn thành (12 × ~30 = ~360 PNG), animation richness layer sẽ là PR riêng. Catalog hiện tại chỉ cần cover base 30 PNG/entity vì đó là minimum cho idle/walk/attack/hit/death — đủ cho gameplay loop survival cơ bản.

---

## §9 Cost estimate + iteration tips

### §9.1 Cost (1 entity = 30 PNG, 12 entity total = 360 PNG)

| Tool | Per image | 1 entity (30 PNG) | All 12 entities (360 PNG) |
| --- | --- | --- | --- |
| ChatGPT-Image (DALL·E 3 / gpt-image-1) | $0.04 | $1.20 | ~$14 |
| Midjourney `--cref` (yearly $30/mo, ~1000 imgs) | ~$0.03 | ~$0.90 | ~$11 |
| Leonardo Phoenix (free 150 tokens/day, slow) | $0 | 1 day cap | ~5 days iter |
| NanoBanana / Imagen 3 (Google AI Studio) | $0.04 | $1.20 | ~$14 |
| Stable Diffusion local + ControlNet (8 GB GPU) | $0 + 30s/img | hours | days |

Recommend: **Midjourney `--cref`** for character consistency (best style match across 30 part PNGs), GPT-image fallback cho tricky parts (typically wing/tail với semi-translucent membrane).

### §9.2 Iteration tips

1. **Always gen STYLE-REF master first.** Nếu master §3.1 không đạt 10/10 acceptance, KHÔNG đi tiếp 30 atomic parts — sẽ ra inconsistent. Refine master prompt 3–5 lần để hit checklist §6.1.
2. **Image-to-image + prompt > text-only.** Mỗi atomic part PNG attach `player_E_v2.png` làm guidance. Midjourney `--cref --cw 80` strikes balance (giữ style tight, allow per-part variation).
3. **Lock palette với hex codes inline.** AI tools tôn trọng hex codes hơn text mô tả ("warm grey" → render khác mỗi lần; `#7a7c80` → render đúng).
4. **One PNG at a time.** AI single-prompt KHÔNG gen 30 PNG cùng lúc reliably. Loop từng part.
5. **Trim PNG sau khi gen.** Re-trim mỗi PNG mới với `Image.crop(getbbox())` để placeholder importer auto-PPU không lệch. Validator check ≤5px transparent padding.
6. **Validate trong Unity sau 5 PNG đầu** (head + torso + arm × 2 + leg E direction). Nếu pivot sai (head lơ lửng cách torso, arm offset shoulder), tinh chỉnh prompt "top edge horizontal at <pivot>" rồi re-gen.
7. **Outline thickness scale với canvas.** 1024 canvas → 10–14px (master); 600 canvas → 6–8px (atomic arm/forearm); 540 canvas → 5–7px (atomic shin). AI tools đôi khi outline thin khi canvas nhỏ — gen ở 1024 rồi resize xuống nếu cần.
8. **Common AI failure modes:**
   - "Renders smooth airbrush gradient" → add "FLAT 3-color tonal stops, NO airbrush, NO smooth gradient" 2x trong prompt + 2x trong negative.
   - "Outline too thin" → add "THICK 10-14px sepia ink-wash brushstroke outline like Don't Starve Together" 2x trong prompt.
   - "5-head proportion drift" → add "chibi 3.5 head proportion is MANDATORY" ở ĐẦU prompt + `--no 5 head tall, lanky` trong negative.
   - "Robe drapes past hip" → add `"robe ENDS AT HIP LINE like a short jacket"` 2x trong prompt + `--no knee-length robe, mid-shin robe, ankle-length robe` trong negative.
   - "Eye drifts to anime iris" → add `"single solid black DOT pupil ONLY, no iris, no sclera, no eyelash"` 3x trong prompt.

---

## §10 References

- [`PLAYER_DST_REFERENCE.md`](PLAYER_DST_REFERENCE.md) — visual signature lock + reference image
- [`PLAYER_FULL_ASSET_SOURCE_PROMPT.md`](PLAYER_FULL_ASSET_SOURCE_PROMPT.md) — one GPT prompt to generate a coherent full asset source board before extraction
- [`ART_STYLE.md`](ART_STYLE.md) — biome palettes (forest / stone highlands / desert)
- [`PUPPET_PIPELINE.md`](PUPPET_PIPELINE.md) — rig hierarchy + animation math
- [`DST_RIG_ASSET_GUIDE.md`](DST_RIG_ASSET_GUIDE.md) — practical rules for making assets animate smoothly in the current puppet rig
- [`BONE_RIG_GUIDE.md`](BONE_RIG_GUIDE.md) — alternative Unity 2D bone workflow (not default pipeline)
- `prompts/hero.txt`, `prompts/tileset.txt`, `prompts/tileset_gpt.txt`, `prompts/gpt_workflow.md` — tile/scene workflow
- `.agents/scripts/validate_player_art.py` — mechanical validator (RGBA + bbox + dimensions per part)
- `Assets/_Project/Scripts/Core/PuppetPlaceholderSpec.cs` — `RectFor(role)` source of truth cho anatomy table §2
