# Player Atomic-Symbol Art Rules

Atomic-symbol composition rules cho rig-based puppet animation. **Bắt buộc** đối với
mọi PNG vào `Assets/_Project/Art/Characters/player/{E,N,S}/`. Vi phạm bất kỳ rule nào
sẽ tạo "rời rạc" effect (double-arm, baked sleeves overlapping rig sleeves, paper-doll
gaps) HOẶC drift khỏi DST canon (chibi proportion, anime face, smooth airbrush).

> **Tại sao lại có rule này:** rig overlay separate body-part sprites trên 1 hierarchy.
> Nếu mỗi sprite có nội dung "thừa" (sleeve trong torso, neck trong head, hand trong
> forearm), rig sẽ render trùng nhiều lần → user nhìn 4 cánh tay thay vì 2. Nếu style
> drift khỏi DST canon → game mất identity "Linh Khí Wuxia × DST".

## TL;DR — 7 luật atomic

1. **One part = one anatomical region. No more, no less.**
2. **Each PNG isolated, RGBA transparent BG, alpha bbox tight (≤5px padding).**
3. **No baked-in adjacent parts.** Torso không có shoulders/arms/neck/legs. Head không
   có neck/shoulders. Arm không có shoulder/torso/hand-detail. Forearm không có elbow
   shadow chạy lên arm. Leg không có hip/torso. Shin không có knee shadow chạy lên leg.
4. **Pivot convention.** Top-of-sprite = attach point cho parent joint (head→neck base,
   torso→hip base, arm→shoulder, forearm→elbow, leg→hip, shin→knee). Sprite hangs DOWN
   từ top pivot.
5. **3 directions consistent.** Cùng 1 part ở E/N/S phải cùng style/palette/proportion.
   N = back view (nhìn phía sau), S = front view (nhìn phía trước), E = side view
   (camera nhìn từ phải; W = flipX của E, không cần gen riêng).
6. **DST proportion lock** (§6 below). Mỗi part phải fit vào ~5-head-tall lanky adult
   skeleton. Head ≤ 1/5 body height. Arms long, legs long, shoulders narrow, hands
   oversized mitten-style, boots oversized sole. NO chibi 3-4 head-tall.
7. **DST visual signature lock** (§7 below). Variable-width brush outline 4-12px
   sepia-tinted (NOT pure black uniform). Visible brush strokes inside fills + pencil
   sketch construction lines. Muddied palette saturation ≤30%. Face minimalism (dot eyes,
   line mouth). NO smooth airbrush, NO clean uniform digital outline, NO anime/kawaii.

> **Read [`PLAYER_DST_REFERENCE.md`](PLAYER_DST_REFERENCE.md) FIRST** — full Klei /
> Jeff Agala canon doc với reference characters (Wilson/Maxwell/Webber), proportion
> chart, palette LOCK, signature trait checklist.

---

## §1 Per-part atomic checklist

### head

| DO | DON'T |
|---|---|
| Hộp sọ + tóc + mặt only (E/S) hoặc hộp sọ + tóc only (N) | Cổ, vai, áo collar |
| Bottom edge cut clean ngay dưới cằm (jaw line) | Bottom edge tới collarbone hoặc thấp hơn |
| Hair bun / topknot có thể vượt ra ngoài hộp sọ phía trên | Râu kéo dài xuống cổ |

### torso

| DO | DON'T |
|---|---|
| Trunk only — cổ stub ngay dưới jaw đến hip line | Sleeves bell-flow extending past body width |
| Vai bịt kín ngang — top edge horizontal at shoulder height | Vai có puff hoặc tay áo dạng cánh |
| Hip cut clean — bottom edge horizontal at hip level | Hip có quần / leg fabric kéo xuống |
| Quần áo (kimono robe) bám sát thân trục — narrow profile | Quần áo flow-out, bell shape, hem |
| Sash / belt OK ở waist | Arms / hands / fingers visible anywhere |

> **Critical:** torso width chỉ là chest+belly width, không bao gồm vai mở rộng cho
> cánh tay. Vai = top horizontal edge của torso, không có "shoulder cap". Cánh tay
> riêng sẽ attach ngay tại top corner của torso.

### arm_left / arm_right (upper arm, shoulder → elbow)

| DO | DON'T |
|---|---|
| Cylindrical (≈ rectangular silhouette với rounded corners) | Bell-flow / triangular sleeve |
| Sleeve fits tight to limb — fabric đường viền song song với limb axis | Sleeve flares out, hem hanging |
| Top edge = shoulder cap (rounded), bottom edge = elbow joint (clean cut) | Hand / fingers visible at bottom |
| Width consistent từ top to bottom (chỉ 5-15% taper) | Width tapers >30% top-to-bottom |
| Sleeve color matches torso material | Solid block của sleeve cloth không có arm shape |

### forearm_left / forearm_right (elbow → wrist + hand)

| DO | DON'T |
|---|---|
| Forearm cylinder + visible hand at bottom (5 fingers visible khi neutral pose) | Hand cut off ở wrist |
| Top edge = elbow joint (rounded, fits arm bottom edge) | Continued sleeve flaring |
| Bottom = fingertips OR closed fist (consistent across 3 directions) | Floating hand không attached |
| Sleeve cuff visible ở wrist nếu kimono có cuff | Bell-flow sleeve covering hand |

### leg_left / leg_right (hip → knee, upper leg)

| DO | DON'T |
|---|---|
| Cylindrical thigh + pant fabric tight to limb | Pants flare out, hakama bell shape |
| Top edge = hip joint (clean horizontal cut) | Hip + waist + sash visible |
| Bottom edge = knee joint (rounded) | Shin / foot visible at bottom |
| Width consistent (thigh slightly wider than knee, max 20% taper) | Pants drape past knee |

### shin_left / shin_right (knee → ankle + foot)

| DO | DON'T |
|---|---|
| Shin cylinder + foot/boot at bottom | Foot cut off at ankle |
| Top edge = knee joint (rounded) | Continued thigh / hip visible |
| Bottom = sole of foot/boot (consistent ground line cross 3 dirs) | Floating foot không attached to shin |
| Boot / shoe ankle wrap visible nếu có | Pants fabric draping over boot |

---

## §2 File / folder convention

```
Assets/_Project/Art/Characters/player/
├── E/                        ← side view (right-facing). W = flipX of E auto.
│   ├── head.png              ~210×235 (range 160-250 × 200-270)
│   ├── torso.png             ~130×290 (range 100-150 × 240-340)  ← NEW narrow trunk
│   ├── arm_left.png          ~80×200  (range 60-120 × 170-230)
│   ├── arm_right.png         ~80×200
│   ├── forearm_left.png      ~80×220  (range 50-110 × 180-260)   ← NEW with hand
│   ├── forearm_right.png     ~80×220
│   ├── leg_left.png          ~95×220  (range 70-125 × 190-250)
│   ├── leg_right.png         ~95×220
│   ├── shin_left.png         ~100×210 (range 70-130 × 180-240)
│   └── shin_right.png        ~100×210
├── N/  (back view)           similar dims, head shows back-of-skull only
└── S/  (front view)          similar dims, full face visible
```

> **Width caps are STRICT for torso (max 150).** Current art has torso width 156 (E)
> and 212 (S) because sleeves are baked into the torso PNG. After regen với trunk-only
> prompt, torso width sẽ ~110-140. Validator sẽ FAIL bất kỳ torso > 150 wide.

**RGBA mode required** (transparent BG). PNG-8 indexed colors KHÔNG OK.

**Alpha bbox** phải tight: ≤ 5px transparent padding all sides. Validator script
auto-detect và recommend crop.

---

## §3 Workflow

1. Gen 30 PNGs theo prompt trong [`PLAYER_ATOMIC_ART_PROMPTS.md`](PLAYER_ATOMIC_ART_PROMPTS.md).
2. Save vào `Assets/_Project/Art/Characters/player/{E,N,S}/{part}.png`.
3. Run `python3 .agents/scripts/validate_player_art.py` từ repo root.
4. Read report. Fix any `FAIL:` items (auto-crop / re-gen part / rename).
5. Re-run validator until tất cả `OK`.
6. Re-bootstrap MainScene (`Tools → Wilderness Cultivation → Bootstrap Default Scene`)
   trong Unity.
7. Play. Walk Player ở 3 directions. Verify smooth puppet animation, no rời rạc.

## §4 Common mistakes

- **Sleeves baked into torso** (current player art has this). Fix: regen torso prompt
  EMPHASIZE "trunk only no sleeves no arms". Use negative prompts: `"no sleeves",
  "no arms", "no flowing fabric past body", "narrow profile"`.
- **Bell-flow arm sprites** (current player arm has this). Fix: regen arm prompt
  EMPHASIZE "cylindrical arm with tight sleeve, hand visible at bottom". Negative:
  `"no bell flare", "no flowing cloth", "no triangular shape"`.
- **No hand visible** in forearm. Fix: explicit "5 fingers visible" or "closed fist
  at bottom of forearm".
- **Missing N/S sprites** for arm/forearm. OK to skip — rig auto-hides arm sprites
  in N/S anyway (per `PuppetAnimController.hideArmsInFrontBackView`). But head/torso/
  leg/shin MUST exist for all 3 directions.
- **PNG-8 indexed mode** instead of RGBA. Validator will catch this. Re-export RGBA.

## §5 Reference

- Animation rig logic: `Assets/_Project/Scripts/Vfx/PuppetAnimController.cs`
- Bootstrap pipeline: `Documentation/PUPPET_PIPELINE.md`
- Style lock + palette + ink-wash spec: `Documentation/ART_STYLE.md`
- DST canon visual signature: [`PLAYER_DST_REFERENCE.md`](PLAYER_DST_REFERENCE.md)
- Prompts: [`PLAYER_ATOMIC_ART_PROMPTS.md`](PLAYER_ATOMIC_ART_PROMPTS.md)
- Validator: `.agents/scripts/validate_player_art.py`

---

## §6 DST proportion lock (HARD constraint)

Reference character: **Wilson Percival Higgsbury** (DST default scientist hero).
Lanky adult, **~5 head-tall** total body height. Each atomic part must fit:

| Part | Height (head-units) | Width relative to head |
|---|---|---|
| head | 1.0 H | 1.0 W (head W = head H roughly) |
| torso (trunk only) | ~1.5 H | 1.2 W (narrow shoulders, NOT broad) |
| arm (upper) | ~1.4 H | 0.4 W (narrow lanky) |
| forearm + mitten hand | ~1.5 H | 0.4 W forearm / 0.5 W hand (oversized) |
| leg (upper) | ~1.5 H | 0.45 W |
| shin + boot | ~1.4 H | 0.45 W shin / 0.6 W boot sole (oversized) |

Total stack: head (1.0) + neck-stub (~0.2 baked into top of torso) + torso (1.5) +
leg (1.5) + shin (1.4) ≈ 5.6 H. Arms (1.4 + 1.5 = 2.9 H) hang from shoulder reaching
mid-thigh.

### NO-list (chibi/anime drift markers)

- ✗ Head ≥ 1/3 of body height (chibi big head)
- ✗ Arm shorter than 1.0 H (stubby chibi arm)
- ✗ Leg shorter than 1.0 H (stubby chibi leg)
- ✗ Shoulders wider than 1.5 W (broad superhero shoulders)
- ✗ Hands smaller than forearm-end width (anime slender hands)
- ✗ Boot sole same width as shin (no oversized sole)

---

## §7 DST visual signature lock (HARD constraint)

Klei / Jeff Agala signature traits — apply to EVERY PNG. Without these, output drifts
toward generic chibi/anime cartoon.

### 7.1 Outline

- **Variable width 4-12px** calligraphy ink brush
- Thick (10-12px) on shadow side of limb, thin (4-6px) on highlight side
- **Wobbly hand-drawn quality** — not pixel-perfect digital line
- Slight overshoot at corners (line extends ~3px past intersection)
- **Sepia-tinted ink** `#1a1408` — never pure black `#000000`

### 7.2 Fill

- **Gouache + watercolor wash** — visible brush strokes inside fills
- Flat 3-color stops per material (light / mid / dark) with **wash gradient at edge**
- **Pencil sketch construction lines visible** at edges of fills (~10-20% opacity)
- NO smooth airbrush gradient
- NO solid flat color

### 7.3 Palette LOCK (muddied DST tones, saturation cap 30%)

| Color | Hex | Notes |
|---|---|---|
| Skin highlight | `#c8a884` | warm muddied tan, NOT orange |
| Skin mid | `#a08868` | sepia overlay |
| Skin shadow | `#5a4828` | strong sepia |
| Robe cream highlight | `#e8d8b8` | washed-out, NOT bright cream |
| Robe cream mid | `#c8b094` | warm tan-cream |
| Robe fold | `#8a6f47` | strong sepia fold |
| Sash gold light | `#a8884a` | muted gold, NOT bright |
| Sash gold shadow | `#7a5a30` | dark muted gold |
| Pendant jade light | `#7a9078` | muddied green |
| Pendant jade shadow | `#4a5a48` | dark muddied green |
| Hair ink-black base | `#2a2418` | warm-dark, NOT pure black |
| Hair highlight | `#4a4030` | subtle sepia gloss |
| Trousers base | `#3a3530` | dark olive-charcoal |
| Trousers shadow | `#1a1812` | near-black olive |
| Boot leather base | `#5a4830` | warm dark brown |
| Boot strap | `#a89878` | cream-tan |
| Outline ink | `#1a1408` | sepia-tinted, variable width |

**Saturation check**: every color HSL saturation ≤30%. If hex looks "clean cartoon
bright", desaturate.

### 7.4 Face minimalism (head sprite)

- **Eyes**: small dots `#1a1408` ~3-5px diameter, OR simple curved-line dashes
- **Mouth**: single horizontal line OR small curve, ~1-3px thick
- **Nose**: tiny angle line `<` or `^` shape ~5-8px
- **Eyebrows**: short brush stroke ~10-15px, expressive
- **Cheek blush** (optional): subtle `#c89878` opacity 40%, ~15-20px diameter

### 7.5 NO-list (anime/kawaii drift markers)

- ✗ Detailed almond eye with iris + highlight (anime)
- ✗ Eye sparkles, hearts (kawaii)
- ✗ Smooth airbrush gradient skin (anime cel)
- ✗ Clean uniform digital outline (vector)
- ✗ Saturated bright colors (#ff or #f0 hex peaks)
- ✗ Multiple anime gloss highlight stripes in hair
- ✗ Cute kawaii facial expression (raised brows + open mouth)
- ✗ Pure black `#000000` outline (use sepia `#1a1408`)
