# Player Atomic Art Prompts (Chibi Wuxia × Soft-DST)

Atomic-symbol prompts cho player character regen, locked to **"Chibi Wuxia ×
Soft-DST"** identity (after May 2026 iteration: chibi proportion accepted, DST
signature traits preserved — muddied palette + brush outline + atomic
composition). **Đọc theo thứ tự:**

1. [`PLAYER_DST_REFERENCE.md`](PLAYER_DST_REFERENCE.md) — visual signature + identity lock
2. [`PLAYER_ATOMIC_RULES.md`](PLAYER_ATOMIC_RULES.md) — composition rules
3. File này — 30 per-part prompts
4. `.agents/scripts/validate_player_art.py` — mechanical validator
5. [`assets/style_refs/player_E.png`](assets/style_refs/player_E.png) — reference image (attach to AI gen)

> ## Style lock — Chibi Wuxia × Soft-DST
>
> **Aesthetic**: Chibi wuxia hand-painted illustration, soft Don't Starve Together
> aesthetic. Klei-style ink outline (sepia-tinted `#1a1408`, NOT pure black,
> thickness 8-16px at 1024 canvas, slight wobble nice-to-have). Muddied desaturated
> palette **saturation cap 30%**, sepia/ochre tonal overlay. Flat 3-color stops
> per material with subtle wash gradient. Visible brush strokes nice-to-have but
> not mandatory.
>
> **Proportion**: Chibi ~3.5-4 head-tall (cute young cultivator, Webber-leaning).
> NOT pure-DST 5-head Wilson lanky. NOT super-deformed >1/3 head ratio.
>
> **Face**: Small almond eye with single black pupil `#1a1408`, single line mouth,
> tiny angle nose, single-stroke eyebrow. NO anime sparkle, NO multi-color iris,
> NO kawaii expression.
>
> **Wuxia identity** (must be visible in every relevant part):
> - Cream V-neck wuxia kimono robe with brown cuff trim at sleeve wrist
> - Gold sash bow knot at waist (muted gold, NOT bright lemon yellow)
> - Jade pendant + cloud sigil on chest
> - Topknot bun with cream silk ribbon trailing + asymmetric forelock
> - Warm-charcoal trousers, brown leather boots with cream toe / strap
>
> **Palette LOCK** (muddied wuxia, saturation ≤30%):
> - Skin: highlight `#c8a884` / mid `#a08868` / shadow `#5a4828`
> - Robe cream: highlight `#e8d8b8` / mid `#c8b094` / fold `#8a6f47`
> - Cuff trim: `#8a6f47` / shadow `#5a4030`
> - Sash gold: light `#a8884a` / shadow `#7a5a30` (**NOT bright `#f0c020` lemon**)
> - Pendant jade: light `#7a9078` / shadow `#4a5a48`
> - Hair ink-black: base `#2a2418` / highlight `#4a4030` (**NOT pure `#000`**)
> - Trousers: base `#3a3530` / shadow `#1a1812`
> - Boot leather: base `#5a4830` / shadow `#3a2818` / strap `#a89878`
> - Outline ink: `#1a1408` sepia-tinted (**NOT pure black uniform**)
> - Cheek blush (optional): `#c89878` opacity 40%
>
> **Format ALL parts**: transparent BG (RGBA), tight alpha bbox (no padding),
> single character body part isolated, no shadow on ground, no border, no text.

## TL;DR usage

1. Đọc `PLAYER_DST_REFERENCE.md` để hiểu soft-DST + wuxia identity lock.
2. Attach `assets/style_refs/player_E.png` làm image guidance trong AI gen tool.
3. Test gen 1 full-body style ref với §0 prompt — verify ≥7/10 boxes của reference §5 pass.
4. Copy prompt block tương ứng (§E/§N/§S) vào AI gen tool, gen từng part.
5. Save thành `Assets/_Project/Art/Characters/player/{E|N|S}/{part}.png`.
6. Run validator: `python3 .agents/scripts/validate_player_art.py`.
7. Re-bootstrap MainScene: `Tools → Wilderness Cultivation → Bootstrap Default Scene`.

> **Negative prompts ALL parts** (append vào mọi prompt):
> ```
> no anime sparkle, no kawaii heart eyes, no multi-color iris, no smooth
> airbrush gradient, no pure black #000 outline (use sepia #1a1408), no
> saturated bright colors, no lemon yellow, no neon orange, no anime gloss
> highlight stripes, no cute open mouth surprised expression, no bell-flow
> flaring sleeves, no floating hands, no shadow on ground, no ground, no
> background, no border frame, no watermark, no text, no extra body parts,
> no duplicate limbs, no anatomy errors, no blurry edges, no anti-alias
> bleeding past outline.
> ```

---

## §0 — Acceptance test prompt (gen 1 style ref FIRST)

Use this prompt to gen 1 full-body style ref. Verify against
`PLAYER_DST_REFERENCE.md` §5 checklist BEFORE proceeding to 30 atomic parts.

**Tip**: attach [`assets/style_refs/player_E.png`](assets/style_refs/player_E.png)
as image guidance / `--cref` / IP-Adapter input for stronger style transfer.

```
Chibi wuxia hand-painted illustration, soft Don't Starve Together aesthetic.
Klei-style ink outline sepia-tinted #1a1408 (NOT pure black), thickness 8-16px
at 1024 canvas. Muddied desaturated palette saturation cap 30%, sepia/ochre
tonal overlay. Flat 3-color stops per material with subtle wash gradient.

Subject: full-body chibi young male wuxia cultivator, ~3.5-4 head-tall (cute
proportion). Side profile facing right. Standing relaxed neutral pose with
slight slouch, looking forward calmly. Single character isolated.

Outfit: cream V-neck wuxia kimono robe (highlight #e8d8b8 mid #c8b094 fold
#8a6f47), hip-length, sleeves TIGHT TO ARM with brown cuff trim band at wrist
(#8a6f47). Gold sash with bow knot at right waist (muted gold light #a8884a
shadow #7a5a30, NOT bright lemon yellow). Jade pendant + cloud sigil on chest
(jade #7a9078). Warm-charcoal trousers visible mid-shin (#3a3530). Brown
leather boots (#5a4830) with cream toe / ankle strap (#a89878). Hair ink-black
topknot bun (#2a2418, NOT pure black) with cream silk ribbon trailing, single
asymmetric forelock at front. Skin warm muddied tan highlight #c8a884 mid
#a08868 shadow #5a4828.

Face: small almond eye with single black pupil #1a1408, single line mouth,
tiny angle nose, single-stroke eyebrow. NO anime sparkle, NO multi-color iris,
NO kawaii expression. Optional subtle cheek blush #c89878 at 40% opacity.

Background: transparent RGBA, no shadow on ground, no border, no watermark,
no text.

Negative: no anime sparkle, no kawaii heart eyes, no multi-color iris, no
smooth airbrush gradient, no pure black #000 outline, no saturated bright
colors, no lemon yellow, no neon orange, no anime gloss highlight stripes,
no cute open mouth surprised expression, no bell-flow flaring sleeves, no
floating hands, no shadow on ground, no background, no border, no watermark,
no text, no extra limbs, no anatomy errors.
```

After gen, run reference §5 acceptance checklist. ≥7/10 → proceed §E/§N/§S.
<7/10 → adjust prompt with stronger language on failed boxes, re-test.

---

## §E — East direction (side view, character facing right)

### E/head.png (target ~210×220)
```
Chibi wuxia hand-painted illustration, soft Don't Starve Together aesthetic.
Klei-style ink outline sepia-tinted #1a1408 (NOT pure black), thickness 8-16px
at 1024 canvas. Muddied desaturated palette saturation cap 30%, sepia/ochre
tonal overlay. Flat 3-color stops per material with subtle wash gradient.

Subject: ISOLATED HUMAN HEAD ONLY, side profile facing right, neck cut clean
at jaw line. Head ~1/4 of total body height (chibi cute proportion, NOT super-deformed).

Composition: ONLY skull + face + hair. Single profile-right view: 1 dot eye
visible, tiny angle nose pointing right, ear visible. Hair tied in topknot bun
on crown with cream silk ribbon trailing back. Bottom edge = horizontal cut at
jaw line — NO neck visible past jawline, NO collar, NO shoulders.

Anatomy: chibi cute young-male proportion, slight slouch forward (curious
pose). Face: small almond eye with single black pupil #1a1408 ~3-5px, single
line mouth ~1-2px thick, tiny angle nose ~5-8px brush stroke, single-stroke
eyebrow ~10-15px above eye. NO anime sparkle, NO multi-color iris. Skin tone muddied warm tan
highlight #c8a884 mid #a08868 shadow #5a4828. Optional subtle cheek blush
#c89878 at 40% opacity.

Palette LOCK: hair ink-black #2a2418 base + highlight #4a4030, ribbon cream
#e8d8b8, skin #c8a884 / #a08868 / #5a4828, lip line #6a3a28, outline
sepia-ink #1a1408 variable width.

Negative: NO neck below jaw, NO shoulders, NO collar, NO torso, NO body, NO
multiple eyes, NO detailed almond iris, NO anime eye, NO front view, NO back
view, NO super-deformed >1/3 head, NO anime sparkle, NO kawaii heart eyes, NO
multi-color iris, NO smooth airbrush, NO pure black outline, NO saturated colors, no shadow, no ground, no background,
no border, no watermark, no text, no anatomy errors, no blurry edges.
```

### E/torso.png (target ~120×260, narrower than current — TRUNK ONLY)
```
Chibi wuxia hand-painted illustration, soft Don't Starve Together aesthetic.
Klei-style ink outline sepia-tinted #1a1408 (NOT pure black), thickness 8-16px
at 1024 canvas. Muddied desaturated palette saturation cap 30%, sepia/ochre
tonal overlay. Flat 3-color stops per material with subtle wash gradient.

Subject: ISOLATED HUMAN TORSO ONLY (TRUNK), side profile facing right.
Cylindrical narrow trunk shape — chest + belly + back area only. NO sleeves,
NO arms, NO shoulders extending past body width, NO neck, NO hips/legs.

Composition: narrow vertical cylinder profile, ~1.5 head heights tall, chibi
cute build (NOT broad superhero). Top edge = clean horizontal cut at shoulder
height (where arm will attach separately). Bottom edge = clean horizontal cut
at hip height. Width consistent top-to-bottom (slight waist taper at sash OK
≤15%, no flaring at top or bottom).

Clothing: cream wuxia kimono robe TIGHT TO TRUNK only, NO bell-flow, NO flaring
past body width. Gold sash bow knot wrapped at waist (visible on side). Jade
pendant + cloud sigil hanging on chest centered. V-neck collar visible at top.
Robe ends cleanly at hip line — NO drape past hip.

Palette LOCK: robe cream highlight #e8d8b8 / mid #c8b094 / fold #8a6f47, sash
gold light #a8884a / shadow #7a5a30, pendant jade #7a9078 / shadow #4a5a48,
cloud sigil #8a8060, outline sepia-ink #1a1408 variable width.

Visible brush strokes inside cream fill, watercolor wash gradient at edges.
Pencil construction lines faintly visible.

Negative: NO sleeves baked, NO arms, NO hands, NO shoulders flaring out, NO
bell-flow fabric, NO triangular silhouette, NO flowing hem past hip, NO legs,
NO pants, NO feet, NO neck, NO head, NO double layers, NO super-deformed >1/3 head,
NO anime sparkle, NO kawaii heart eyes, NO multi-color iris, NO smooth
airbrush, NO pure black outline, NO saturated colors, no shadow, no ground, no background, no border, no watermark,
no text, no anatomy errors.
```

### E/arm_left.png (target ~80×200)
```
Chibi wuxia hand-painted illustration, soft Don't Starve Together aesthetic.
Klei-style ink outline sepia-tinted #1a1408 (NOT pure black), thickness 8-16px
at 1024 canvas. Muddied desaturated palette saturation cap 30%, sepia/ochre
tonal overlay. Flat 3-color stops per material with subtle wash gradient.

Subject: ISOLATED LEFT UPPER ARM ONLY (shoulder cap to elbow joint), side profile.
Cylindrical chibi limb shape with tight kimono sleeve. NO hand, NO forearm,
NO torso, NO bell-flow.

Composition: narrow cylinder hanging straight down, ~1.4 head heights tall
(chibi cute arm). Top edge = rounded shoulder cap (where arm meets torso).
Bottom edge = clean horizontal cut at elbow joint. Width consistent top-to-bottom
(max 10% taper).

Clothing: kimono sleeve TIGHT TO LIMB matching torso color (cream highlight
#e8d8b8 / mid #c8b094). Outline follows limb axis parallel — sleeve fabric does
NOT flare out at bottom. Sleeve ends cleanly at elbow line.

Palette LOCK: sleeve cream highlight #e8d8b8 / mid #c8b094 / fold #8a6f47,
outline sepia-ink #1a1408 variable width.

Visible brush strokes inside fill, watercolor wash gradient at edges.

Negative: NO bell-flow, NO triangular silhouette, NO flaring sleeve, NO hand,
NO fingers, NO forearm, NO elbow detail past joint, NO shoulder padding past arm
width, NO torso, NO chibi short stubby limb, NO anime style, NO smooth airbrush,
NO clean uniform outline, NO saturated colors, no shadow, no ground, no background,
no border, no watermark, no text, no anatomy errors.
```

### E/arm_right.png (target ~80×200)
```
Subject + composition + palette + negative: SAME as E/arm_left.png but mirror
profile (arm hanging on the right side of body in side view, slight perspective
foreshorten ~5%). Otherwise identical specs.
```

### E/forearm_left.png (target ~70×220)
```
Chibi wuxia hand-painted illustration, soft Don't Starve Together aesthetic.
Klei-style ink outline sepia-tinted #1a1408 (NOT pure black), thickness 8-16px
at 1024 canvas. Muddied desaturated palette saturation cap 30%, sepia/ochre
tonal overlay. Flat 3-color stops per material with subtle wash gradient.

Subject: ISOLATED LEFT FOREARM + HAND ONLY (elbow joint to fingertips),
side profile. Forearm cylinder + visible mitten-style oversized hand at bottom
in relaxed pose.

Composition: cylindrical forearm hanging down from elbow, ~1.5 head heights tall.
Top edge = rounded elbow joint (matches arm_left bottom edge). Sleeve cuff visible
~70% down (where wrist is). **Mitten-style hand** visible at bottom 30% — DST
signature: 4 fingers fused as one shape with thumb separate, OR closed casual fist.
Hand readable size — small fist or mitten OK (~1.0-1.2× forearm-end width).

Clothing: kimono sleeve cuff cream-tan #c8b094 banded at wrist. Forearm has tight
sleeve same cream as upper arm. Hand skin tone matches face highlight #c8a884
mid #a08868 shadow #5a4828.

Palette LOCK: sleeve cream #e8d8b8 / #c8b094 / #8a6f47, cuff #a8884a, skin
#c8a884 / #a08868 / #5a4828, outline sepia-ink #1a1408 variable width.

Visible brush strokes inside fills, pencil construction lines visible.

Negative: NO floating hand (must attach to forearm), NO bell-flow, NO sleeve
covering hand, NO realistic 5-finger detailed hand (DST = mitten style preferred),
NO weapon, NO accessory, NO upper arm, NO torso, NO super-deformed >1/3 head, NO anime sparkle,
NO kawaii heart eyes, NO smooth airbrush, NO pure black outline, no shadow, no ground, no
background, no border, no watermark, no text, no anatomy errors.
```

### E/forearm_right.png (target ~70×220)
```
SAME as E/forearm_left.png but mirror — palm/mitten facing in (toward body),
arm on right side of body in side view. Mitten-style hand visible, otherwise
identical specs.
```

### E/leg_left.png (target ~95×225)
```
Chibi wuxia hand-painted illustration, soft Don't Starve Together aesthetic.
Klei-style ink outline sepia-tinted #1a1408 (NOT pure black), thickness 8-16px
at 1024 canvas. Muddied desaturated palette saturation cap 30%, sepia/ochre
tonal overlay. Flat 3-color stops per material with subtle wash gradient.

Subject: ISOLATED LEFT UPPER LEG ONLY (hip joint to knee joint), side profile.
Cylindrical chibi thigh with tight trousers fabric. NO foot, NO shin, NO hip
detail past joint, NO sash.

Composition: cylindrical thigh hanging down, ~1.5 head heights tall (chibi cute
leg). Top edge = clean horizontal cut at hip line (where leg attaches to torso).
Bottom edge = rounded knee joint. Width consistent top-to-bottom (max 15% taper
at knee).

Clothing: trousers TIGHT to thigh — warm-charcoal #3a3530 fabric. Pants fabric
does NOT flare out at knee. NO drape past knee line.

Palette LOCK: trousers base #3a3530 / shadow #1a1812 / mid-fold #2a2520, outline
sepia-ink #1a1408 variable width.

Visible brush strokes inside fill, pencil construction lines visible.

Negative: NO bell-flare hakama, NO flowing fabric past knee, NO foot, NO shoe,
NO shin, NO hip detail past joint, NO torso, NO sash, NO chibi short stubby
limb, NO anime, NO smooth airbrush, NO clean uniform outline, no shadow, no
ground, no background, no border, no watermark, no text, no anatomy errors.
```

### E/leg_right.png (target ~95×220)
```
SAME as E/leg_left.png but mirror profile, slight perspective foreshorten ~5%
(further leg in side view). Otherwise identical specs.
```

### E/shin_left.png (target ~95×210)
```
Chibi wuxia hand-painted illustration, soft Don't Starve Together aesthetic.
Klei-style ink outline sepia-tinted #1a1408 (NOT pure black), thickness 8-16px
at 1024 canvas. Muddied desaturated palette saturation cap 30%, sepia/ochre
tonal overlay. Flat 3-color stops per material with subtle wash gradient.

Subject: ISOLATED LEFT SHIN + FOOT/BOOT ONLY (knee joint to sole), side profile.
Shin cylinder + ankle wrap + leather boot with oversized sole.

Composition: shin hanging down from knee, ~1.4 head heights tall. Top edge =
rounded knee joint (matches leg_left bottom edge). Bottom 25-30% = leather boot
with strap visible. **Oversized sole** (DST clown-foot proportion ~1.3×) at
bottom = horizontal ground line.

Clothing: shin has tight trousers fabric same warm-charcoal #3a3530 as upper
leg. Boot leather brown #5a4830 / shadow #3a2818 with cream-tan strap #a89878
wrapping ankle. Sole sepia-ink #1a1408.

Palette LOCK: trousers #3a3530 / #1a1812, boot leather #5a4830 / #3a2818, strap
#a89878, sole #1a1408, outline sepia-ink #1a1408 variable width.

Visible brush strokes inside fills, pencil construction lines visible.

Negative: NO floating foot, NO upper leg / thigh, NO knee detail past joint,
NO calf-flare bell-shape pants, NO realistic small shoe (DST = oversized sole),
NO chibi short stubby leg, NO anime, NO smooth airbrush, NO clean uniform outline,
no shadow, no ground, no background, no border, no watermark, no text, no
anatomy errors.
```

### E/shin_right.png (target ~110×210)
```
SAME as E/shin_left.png but mirror profile, slight perspective foreshorten ~10%
(further leg in side view). Otherwise identical specs.
```

---

## §N — North direction (back view, character walking away from camera)

### N/head.png (target ~200×220)
```
Chibi wuxia hand-painted illustration, soft Don't Starve Together aesthetic.
Klei-style ink outline sepia-tinted #1a1408 (NOT pure black), thickness 8-16px
at 1024 canvas. Muddied desaturated palette saturation cap 30%, sepia/ochre
tonal overlay. Flat 3-color stops per material with subtle wash gradient.

Subject: ISOLATED HUMAN HEAD ONLY, back-view (back of skull facing camera).
NO face, NO eyes, NO mouth visible. Just back-of-head ink-black hair tied in
topknot bun with cream silk ribbon trailing visible from rear, neck stub ZERO
past jawline.

Composition: hair has slight gloss highlight on crown (subtle, NOT anime gloss
stripe). Chibi cute head proportion, NOT super-deformed. Bottom edge clean
horizontal cut at jaw line.

Palette LOCK: hair ink-black base #2a2418 + highlight #4a4030, ribbon cream
#e8d8b8, neck-cap-area visible skin #c8a884 / #a08868, outline sepia-ink #1a1408
variable width.

Visible brush strokes in hair fill, pencil construction lines visible.

Negative: NO face, NO eyes, NO mouth, NO front view, NO profile, NO ear, NO
neck below jaw, NO shoulders, NO collar, NO torso, NO super-deformed >1/3 head, NO anime
gloss highlight stripe, NO smooth airbrush, NO clean uniform outline, NO
saturated colors, no shadow, no ground, no background, no border, no watermark,
no text, no anatomy errors.
```

### N/torso.png (target ~140×280, narrower than current — TRUNK ONLY)
```
Chibi wuxia hand-painted illustration, soft Don't Starve Together aesthetic.
Klei-style ink outline sepia-tinted #1a1408 (NOT pure black), thickness 8-16px
at 1024 canvas. Muddied desaturated palette saturation cap 30%, sepia/ochre
tonal overlay. Flat 3-color stops per material with subtle wash gradient.

Subject: ISOLATED HUMAN TORSO ONLY (TRUNK), BACK-VIEW. NO sleeves, NO arms,
NO neck, NO legs. Back of robe + sash knot.

Composition: rectangular trunk silhouette, narrow-medium shoulder cute build, ~1.5
head heights tall. Top edge = clean horizontal cut at shoulder height. Bottom
edge = clean horizontal cut at hip height. Width consistent (slight waist taper
≤15%).

Clothing: cream wuxia kimono robe back panel TIGHT TO TRUNK. Back of gold sash
visible mid-back with bow knot tied at side. Cloud sigil embroidered on lower
back (subtle, muddied). NO bell-flow, NO sleeves visible, NO collar (back of
neck = top edge cut clean).

Palette LOCK: same as E/torso.png — robe cream #e8d8b8 / #c8b094 / #8a6f47,
sash gold #a8884a / #7a5a30, cloud sigil #8a8060, outline sepia-ink #1a1408
variable width.

Visible brush strokes inside fills, pencil construction lines visible.

Negative: NO sleeves baked, NO arms, NO shoulders flaring out, NO front view,
NO profile, NO neck, NO head, NO legs, NO pants, NO bell-shape, NO chibi
proportion, NO anime, NO smooth airbrush, NO clean uniform outline, no shadow,
no ground, no background, no border, no watermark, no text, no anatomy errors.
```

> Arm sprites for N/S can be skipped — `PuppetAnimController.hideArmsInFrontBackView=true`
> auto-hides arm/forearm sprites in N/S views. If you do gen them, follow E/arm rules
> with back-view orientation.

### N/arm_left.png + N/arm_right.png + N/forearm_left.png + N/forearm_right.png
```
OPTIONAL — auto-hidden in N view. Skip to save compute. If you must gen:
Use SAME prompts as E direction but back-view orientation (mitten-style hand
shows back of fist instead of palm side).
```

### N/leg_left.png (target ~95×225)
```
Chibi wuxia hand-painted illustration, soft Don't Starve Together aesthetic.
Klei-style ink outline sepia-tinted #1a1408 (NOT pure black), thickness 8-16px
at 1024 canvas. Muddied desaturated palette saturation cap 30%, sepia/ochre
tonal overlay. Flat 3-color stops per material with subtle wash gradient.

Subject: ISOLATED LEFT UPPER LEG ONLY, back-view (calf-from-behind angle).
NO foot, NO shin, NO hip past joint.

Composition: cylindrical chibi thigh from rear angle, ~1.5 head heights tall.
Top = hip joint clean horizontal cut. Bottom = knee joint rounded. Width
consistent (max 15% taper at knee).

Clothing: trousers tight to thigh, warm-charcoal #3a3530. Slight fabric fold
visible at back of knee (anatomy hint).

Palette LOCK: same as E/leg.

Visible brush strokes inside fill, pencil construction lines visible.

Negative: same as E/leg + NO front view, NO profile, NO foot, NO chibi short
limb, NO anime, NO smooth airbrush, NO clean uniform outline.
```

### N/leg_right.png — mirror of N/leg_left.png

### N/shin_left.png + N/shin_right.png
```
Back-view of shin + boot, otherwise SAME as E/shin specs.
Boot heel visible at back, oversized sole at bottom.
```

---

## §S — South direction (front view, character facing camera)

### S/head.png (target ~210×230)
```
Chibi wuxia hand-painted illustration, soft Don't Starve Together aesthetic.
Klei-style ink outline sepia-tinted #1a1408 (NOT pure black), thickness 8-16px
at 1024 canvas. Muddied desaturated palette saturation cap 30%, sepia/ochre
tonal overlay. Flat 3-color stops per material with subtle wash gradient.

Subject: ISOLATED HUMAN HEAD ONLY, front view facing camera, neck cut clean
at jaw line.

Composition: ONLY skull + face + hair. Front view: 2 dot eyes visible, tiny
angle nose centered, single line mouth small relaxed, ears at sides. Hair tied
in topknot bun on crown with cream silk ribbon. Hair has subtle center part.
Bottom edge horizontal cut at jaw line.

Anatomy: chibi cute young-male proportion. Slight slouch forward (curious
pose). Face: 2 small almond eyes with single black pupils
#1a1408 ~3-5px each, single line mouth ~1-2px thick, tiny angle nose ~5-8px
brush stroke between eyes, expressive brush-stroke eyebrows ~10-15px above eyes.
Skin tone muddied warm tan highlight #c8a884 mid #a08868 shadow #5a4828.
Optional subtle cheek blush #c89878 at 40% opacity.

Palette LOCK: hair ink-black base #2a2418 + highlight #4a4030, ribbon cream
#e8d8b8, skin #c8a884 / #a08868 / #5a4828, lip line #6a3a28, outline sepia-ink
#1a1408 variable width.

Visible brush strokes in fills, pencil construction lines visible.

Negative: NO neck below jaw, NO shoulders, NO collar, NO torso, NO body, NO
profile, NO back view, NO multiple faces, NO detailed almond iris, NO anime
eye sparkle, NO super-deformed >1/3 head, NO kawaii, NO smooth airbrush, NO clean
uniform outline, NO saturated colors, no shadow, no ground, no background,
no border, no watermark, no text, no anatomy errors, no blurry edges.
```

### S/torso.png (target ~140×290, narrower than current — TRUNK ONLY)
```
Chibi wuxia hand-painted illustration, soft Don't Starve Together aesthetic.
Klei-style ink outline sepia-tinted #1a1408 (NOT pure black), thickness 8-16px
at 1024 canvas. Muddied desaturated palette saturation cap 30%, sepia/ochre
tonal overlay. Flat 3-color stops per material with subtle wash gradient.

Subject: ISOLATED HUMAN TORSO ONLY (TRUNK), FRONT-VIEW. NO sleeves, NO arms,
NO neck, NO legs.

Composition: rectangular trunk silhouette TIGHT TO BODY, chibi cute build,
~1.5 head heights tall. Top edge = clean horizontal cut at shoulder height.
Bottom edge = clean horizontal cut at hip height. Width consistent (slight waist
taper at sash ≤15%).

Clothing: cream wuxia kimono robe front panel TIGHT TO TRUNK. V-neck collar
visible at top center. Gold sash with bow knot wrapped at waist (knot visible
front-side). Jade pendant + cloud sigil centered on chest. NO bell-flow sleeves
at top, NO flaring out at hip.

Palette LOCK: same as E/torso.png.

Visible brush strokes inside fills, pencil construction lines visible.

Negative: NO sleeves baked, NO arms, NO hands, NO shoulders flaring past trunk
width, NO bell-flow, NO flowing hem past hip, NO legs, NO pants, NO feet, NO
neck visible above shoulders, NO head, NO triangular silhouette, NO chibi
proportion, NO anime, NO smooth airbrush, NO clean uniform outline, NO
saturated colors, no shadow, no ground, no background, no border, no watermark,
no text, no anatomy errors.
```

### S/arm + S/forearm — auto-hidden in S view, optional
```
Skip to save compute. If gen'd: SAME prompts as E direction but front-view
orientation (mitten-style hand shows palm-side or front of fist).
```

### S/leg_left.png (target ~95×225)
```
Chibi wuxia hand-painted illustration, soft Don't Starve Together aesthetic.
Klei-style ink outline sepia-tinted #1a1408 (NOT pure black), thickness 8-16px
at 1024 canvas. Muddied desaturated palette saturation cap 30%, sepia/ochre
tonal overlay. Flat 3-color stops per material with subtle wash gradient.

Subject: ISOLATED LEFT UPPER LEG ONLY, front view. NO foot, NO shin, NO hip
detail past joint.

Composition: cylindrical chibi thigh front view, ~1.5 head heights tall. Top
= hip joint clean horizontal cut. Bottom = knee joint rounded. Width consistent
(max 15% taper at knee).

Clothing: trousers tight to thigh, warm-charcoal #3a3530. Center fabric crease
line down the leg axis (subtle anatomy hint).

Palette LOCK: same as E/leg.

Visible brush strokes inside fill, pencil construction lines visible.

Negative: same as E/leg + NO back view, NO profile, NO foot, NO chibi short
limb, NO anime, NO smooth airbrush, NO clean uniform outline.
```

### S/leg_right.png — mirror front-view of S/leg_left.png

### S/shin_left.png + S/shin_right.png
```
Front-view shin + boot, otherwise SAME as E/shin specs. Boot toe visible at
front, oversized sole at bottom.
```

---

## §99 Mega-prompt (1-shot batch tool)

If your AI gen tool supports batch in single prompt, use this preamble:

```
Generate 30 PNG sprites for atomic puppet rig (Don't Starve Together cutout
animation style). All RGBA transparent BG. All same chibi wuxia soft-DST aesthetic:
Klei-style ink outline sepia-tinted #1a1408 thickness 8-16px at 1024 canvas,
flat 3-color stops with subtle wash gradient, muddied palette saturation cap 30%
variable width sepia-tinted #1a1408, muddied desaturated palette saturation
cap 30%.

Proportion: ~3.5-4 head-tall chibi cute young-male cultivator (Webber-leaning).
Narrow-medium shoulders. Arms reaching mid-thigh. Long legs. Readable
mitten-style hands. Oversized boot soles. NOT chibi 3-4 head-tall. NOT anime.

Linh Khí Wuxia overlay: cream kimono robe (#e8d8b8 / #c8b094 / #8a6f47), gold
sash bow knot at waist (#a8884a), jade pendant + cloud sigil on chest (#7a9078),
ink-black topknot bun with cream silk ribbon (#2a2418 + #e8d8b8), warm-charcoal
trousers (#3a3530), brown leather boots (#5a4830) with strap (#a89878). Skin
muddied tan #c8a884 / #a08868 / #5a4828.

Face minimalism: dot eyes #1a1408, line mouth, tiny angle nose, expressive
brush eyebrows. NO detailed iris.

10 body parts × 3 directions:
- head, torso, arm_left, arm_right, forearm_left, forearm_right,
  leg_left, leg_right, shin_left, shin_right
- E (right-side profile), N (back view), S (front view)

CRITICAL atomic-symbol rules (BREAK = REJECT):
1. ONE part = ONE anatomical region. No baked-in adjacent parts.
2. Torso = TRUNK ONLY. No sleeves, no arms, no shoulders flaring, no neck,
   no legs.
3. Arm = upper arm ONLY (shoulder to elbow). Cylindrical, sleeve TIGHT to limb,
   NO bell-flow, NO hand.
4. Forearm = lower arm + mitten-style oversized hand. Sleeve cuff at wrist.
5. Leg = thigh ONLY. Cylindrical, trousers tight, NO foot.
6. Shin = lower leg + boot with oversized sole. Sole horizontal at bottom.
7. Head = skull + face + hair ONLY. Bottom = jaw line clean cut. NO neck.
8. Each PNG: tight alpha bbox, no padding, no shadow, no background.

Direction quirks:
- E: right-side profile. 1 dot eye, ear visible. W = flipX of E (skip).
- N: back view. NO face, hair from back, neck cap area only.
- S: front view. 2 dot eyes, line mouth, V-neck collar, jade pendant centered.

Negative: no super-deformed >1/3 head, no anime sparkle, no kawaii heart eyes, no
multi-color iris, no anime gloss highlight stripes, no smooth airbrush, no
clean uniform digital outline, no vector cel-shaded, no saturated bright colors,
no neon, no shadow on ground, no background, no border, no watermark, no text,
no extra body parts, no anatomy errors.

Per-part prompts: see PLAYER_ATOMIC_ART_PROMPTS.md §E, §N, §S.
DST canon reference: see PLAYER_DST_REFERENCE.md.
Validator: python3 .agents/scripts/validate_player_art.py
```

After gen, **ALWAYS run validator** — `python3 .agents/scripts/validate_player_art.py`
from repo root. It will flag any composition violation (alpha bbox, dimensions,
naming) before re-bootstrap.

## References

- DST canon visual signature: [`PLAYER_DST_REFERENCE.md`](PLAYER_DST_REFERENCE.md)
- Composition rules: [`PLAYER_ATOMIC_RULES.md`](PLAYER_ATOMIC_RULES.md)
- Validator: `.agents/scripts/validate_player_art.py`
- Style lock §1: [`AI_PROMPTS.md`](AI_PROMPTS.md)
