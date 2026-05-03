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
> 1. Acceptance test trước khi gen 30 atomic parts.
> 2. Image guidance / `--cref` / IP-Adapter input cho atomic part prompts ở [`PLAYER_ATOMIC_ART_PROMPTS.md`](PLAYER_ATOMIC_ART_PROMPTS.md).
>
> **Concept:** chibi young SEA-Asian male qi-cultivator (age 12–14, chibi cute proportion), cream V-neck wuxia kimono robe HIP-LENGTH với brown cuff trim band ở wrist + gold sash bow knot ở right waist + jade pendant + jade cloud sigil trên ngực + warm-charcoal trousers + brown leather boots với cream toe stitch + ink-black topknot bun + cream silk ribbon trailing + single asymmetric forelock at front.
>
> **Folder output**: `Documentation/assets/style_refs/player_E_v2.png` (sau khi pass).

### §3.1 Master full-body prompt (copy nguyên block)

```
Hand-painted painterly illustration in Don't Starve Together style fused with
Chinese wuxia cultivation aesthetic. Klei-studio sepia-tinted ink outline
(color #1a1408, NOT pure black), CHUNKY VARIABLE-WIDTH brush outline 10-14
pixels at 1024px canvas, slight hand-drawn wobble, slight overshoot at
joint corners. Muddied desaturated wuxia palette, EVERY color saturation
clamped to MAX 30 percent, sepia and ochre tonal overlay across whole image.
Flat 3-color tonal stops per material (light, mid, shadow) with subtle
watercolor wash gradient. Visible brush stroke texture inside fills.

SUBJECT: full-body chibi young male wuxia cultivator boy, age 12-14,
calm curious expression. CHIBI proportion EXACTLY 3.5 to 4 heads tall total
body height — NOT 5 heads, NOT 6 heads, NOT lanky teen, NOT adult. Head
takes 25-28 percent of total body height (cute Webber-leaning chibi).
Shoulders narrow, only 1.0 to 1.2 head-widths wide. Neutral idle pose with
slight forward slouch (curious cultivator pose), arms hanging relaxed at
sides with about 5 degrees gap from torso, hands visible as small mitten
fists at end of sleeves, legs shoulder-width apart with slight knee unlock.

VIEW: pure side profile facing right (East direction). One eye visible.
Single character isolated, transparent background, full body from top of
hair bun to toe of boot in frame.

OUTFIT (every item must be visible and locked exactly):
- Cream wuxia kimono robe, V-neck collar, HIP-LENGTH ONLY — robe ends at
  hip line where leg starts, MUST NOT drape past hip, MUST NOT reach knee,
  MUST NOT reach mid-shin. Sleeves TIGHT TO ARM cylinder shape, NO bell-flow,
  NO flaring sleeves, NO wide opening. Robe palette highlight #e8d8b8,
  mid #c8b094, fold shadow #8a6f47. Subtle watercolor wash gradient on cream.
- Brown cuff trim band wrapping sleeve at wrist, about 8 percent of arm
  length wide. Cuff color #8a6f47 with darker shadow #5a4030.
- Gold sash tied as bow knot at right waist, ribbon ends draping down about
  15 percent of torso height. Sash color MUTED DUSTY GOLD light #a8884a
  shadow #7a5a30. CRITICAL: NOT bright lemon yellow, NOT neon yellow,
  NOT saturated golden, color must read as muddied earthy gold.
- Jade pendant on green-brown silk cord hanging on chest center. Pendant
  color muddied jade #7a9078 with darker shadow #4a5a48.
- Cloud sigil embroidered on left chest of robe, color jade #7a9078,
  curling cloud-pattern about 1/8 of torso area.
- Warm-charcoal trousers (color base #3a3530, shadow #1a1812), tight cylinder
  cut, visible from hip line down to mid-shin where boot starts.
- Brown leather ankle boots, leather color #5a4830 with darker shadow
  #3a2818. Cream-tan toe stitch and ankle strap visible (color #a89878).
  Slightly oversized chibi boot OK but NOT clown-foot.

HAIR:
- Ink-black hair color #2a2418 (warm-dark, NOT pure black, NOT pure #000),
  small subtle highlight #4a4030 sepia gloss only at top of bun.
- Topknot bun on crown of head, tied with cream silk ribbon (color #e8d8b8)
  trailing back about 1.5 head-heights long, ribbon ends fluttering.
- Single asymmetric forelock falling at front of forehead, about 1 head
  width long.

SKIN:
- Warm muddied tan, highlight #c8a884, mid #a08868, sepia shadow #5a4828.
- Optional very subtle round cheek blush #c89878 at 40 percent opacity, only
  at cheek apple area, no full-face blush.

FACE (extreme minimalism):
- ONE small black SOLID DOT pupil only, about 3 to 5 pixels at 1024 canvas,
  color #1a1408. NO iris, NO sclera fill, NO eyelash count, NO eye
  highlight star, NO multi-color eye, NO anime eye shape with rim.
- Single short line mouth, about 1 to 2 pixels thick, color #6a3a28.
- Tiny angle nose suggestion as 5 to 8 pixel brush stroke.
- Single-stroke eyebrow about 10 to 15 pixels above eye, color #2a2418.

BACKGROUND: pure transparent RGBA alpha, NO ground, NO drop shadow under
character, NO ambient particles, NO border frame, NO color background.

COMPOSITION: single character only, isolated, tight alpha bbox with about
5 pixels transparent padding. NO duplicate, NO multiple poses, NO turnaround
sheet, NO front view, NO back view (gen separately later).

REINFORCE: chibi 3.5 to 4 heads tall (NOT 5, NOT 6), HIP-LENGTH robe (NOT
knee, NOT mid-shin), TIGHT sleeves (NO bell-flow), MUTED gold sash (NOT
bright yellow), single dot eye (NOT iris), CHUNKY 10-14px sepia outline
(NOT clean thin vector line).
```

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

### §3.3 Atomic 30-part prompts → see [`PLAYER_ATOMIC_ART_PROMPTS.md`](PLAYER_ATOMIC_ART_PROMPTS.md)

Sau khi master prompt §3.1 PASS 10/10 (xem [§6](#6-acceptance-test-workflow)), dùng output làm `--cref` / IP-Adapter input cho 30 atomic part prompts ở [`PLAYER_ATOMIC_ART_PROMPTS.md`](PLAYER_ATOMIC_ART_PROMPTS.md):

- **§E (East / side profile right)** — 10 parts: `head`, `torso`, `arm_left`, `arm_right`, `forearm_left`, `forearm_right`, `leg_left`, `leg_right`, `shin_left`, `shin_right`
- **§N (North / back view)** — 6 parts: `head`, `torso`, `leg_left`, `leg_right`, `shin_left`, `shin_right` (arms skipped — auto-hidden by `hideArmsInFrontBackView`)
- **§S (South / front view)** — 6 parts: same as §N

→ Total 30 atomic prompts, mỗi prompt self-contained, inherit palette LOCK §3.2 + style §1 + anatomy §2.

Composition rules per part: [`PLAYER_ATOMIC_RULES.md`](PLAYER_ATOMIC_RULES.md).
Visual signature reference: [`PLAYER_DST_REFERENCE.md`](PLAYER_DST_REFERENCE.md).

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

### §4.2 GPT image 2.0 (`gpt-image-1`, OpenAI 2025)

- Aspect: **2:3** (style ref) hoặc role-specific
- Quality: **high**
- Background: **transparent**
- Style: KHÔNG có separate negative field — prepend `"Avoid: ..."` vào cuối prompt (xem [§5](#5-negative-prompts-master))
- Note: GPT-image có xu hướng anime drift mạnh. Nếu first gen vẫn 5-head, thêm câu `"chibi 3.5 head proportion is MANDATORY, regenerate if proportion is wrong"` vào ĐẦU prompt.

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
7. Use player_E_v2.png as --cref / IP-Adapter input
8. Gen 30 atomic parts per PLAYER_ATOMIC_ART_PROMPTS.md §E/§N/§S
9. Save to Assets/_Project/Art/Characters/player/{E,N,S}/{part}.png
10. Run: python3 .agents/scripts/validate_player_art.py
11. Re-bootstrap MainScene: Tools → Wilderness Cultivation → Bootstrap Default Scene
12. Verify rig in Play mode (idle/walk/attack animations)
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
- [`PLAYER_ATOMIC_ART_PROMPTS.md`](PLAYER_ATOMIC_ART_PROMPTS.md) — 30 atomic per-part prompts (E/N/S × 10 parts)
- [`PLAYER_ATOMIC_RULES.md`](PLAYER_ATOMIC_RULES.md) — composition rules (no baked sleeves, pivot convention, …)
- [`ART_STYLE.md`](ART_STYLE.md) — biome palettes (forest / stone highlands / desert)
- [`PUPPET_PIPELINE.md`](PUPPET_PIPELINE.md) — rig hierarchy + animation math
- [`BONE_RIG_GUIDE.md`](BONE_RIG_GUIDE.md) — bone weighting (cho ai dùng Spine/DragonBones)
- `prompts/hero.txt`, `prompts/tileset.txt`, `prompts/tileset_gpt.txt`, `prompts/gpt_workflow.md` — tile/scene workflow
- `.agents/scripts/validate_player_art.py` — mechanical validator (RGBA + bbox + dimensions per part)
- `Assets/_Project/Scripts/Core/PuppetPlaceholderSpec.cs` — `RectFor(role)` source of truth cho anatomy table §2
