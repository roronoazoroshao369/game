# Player Atomic Art Prompts (DST canon — fix rời rạc)

Atomic-symbol prompts cho player character regen, locked to **100% DST canon
adherence**. **Đọc theo thứ tự:**

1. [`PLAYER_DST_REFERENCE.md`](PLAYER_DST_REFERENCE.md) — DST canon visual signature lock
2. [`PLAYER_ATOMIC_RULES.md`](PLAYER_ATOMIC_RULES.md) — composition rules
3. File này — 30 per-part prompts
4. `.agents/scripts/validate_player_art.py` — mechanical validator

> ## Style lock — DST canon (Klei / Jeff Agala) + Wuxia overlay
>
> **Aesthetic**: Don't Starve Together cutout art. Hand-painted gouache + watercolor
> wash. Visible brush strokes inside fills. Pencil sketch construction lines peeking
> through outline (~15% opacity). Calligraphy ink outline with **variable width
> 4-12px** (thick on shadow side, thin on highlight). Sepia-tinted black outline
> `#1a1408` (NOT pure black). Muddied desaturated palette, **saturation cap 30%**,
> sepia/ochre tonal overlay.
>
> **Proportion lock**: Adult young-male ~5 head-tall (Wilson reference). Lanky
> narrow shoulders, long limbs, oversized mitten hands, oversized boot soles.
> NOT chibi 3-4 head-tall. NOT anime. NOT kawaii.
>
> **Face minimalism**: dot eyes, line mouth, tiny angle nose, expressive brush-stroke
> eyebrows. NO detailed iris with highlight. NO anime eye sparkles.
>
> **Wuxia overlay** (added on top of DST canon):
> - Cream kimono robe instead of Wilson's white shirt + black pants
> - Gold sash bow knot at waist instead of red bow tie at neck
> - Jade pendant + cloud sigil on chest
> - Topknot bun with cream silk ribbon instead of mad-scientist hair
>
> **Palette LOCK** (muddied DST tones, saturation ≤30%):
> - Skin: highlight `#c8a884` / mid `#a08868` / shadow `#5a4828`
> - Robe cream: highlight `#e8d8b8` / mid `#c8b094` / fold `#8a6f47`
> - Sash gold: light `#a8884a` / shadow `#7a5a30`
> - Pendant jade: light `#7a9078` / shadow `#4a5a48`
> - Hair ink-black: base `#2a2418` / highlight `#4a4030`
> - Trousers: base `#3a3530` / shadow `#1a1812`
> - Boot leather: base `#5a4830` / shadow `#3a2818` / strap `#a89878`
> - Outline ink: variable-width brush `#1a1408` (sepia-tinted, NOT pure black)
> - Cheek blush (optional): `#c89878` opacity 40%
>
> **Format ALL parts**: transparent BG (RGBA), tight alpha bbox (no padding),
> single character body part isolated, no shadow on ground, no border, no text.

## TL;DR usage

1. Đọc `PLAYER_DST_REFERENCE.md` để hiểu Klei aesthetic markers.
2. Test gen 1 full-body style ref với §0 prompt — verify ≥8/10 boxes của reference §5 pass.
3. Copy prompt block tương ứng (§E/§N/§S) vào AI gen tool, gen từng part.
4. Save thành `Assets/_Project/Art/Characters/player/{E|N|S}/{part}.png`.
5. Run validator: `python3 .agents/scripts/validate_player_art.py`.
6. Re-bootstrap MainScene: `Tools → Wilderness Cultivation → Bootstrap Default Scene`.

> **Negative prompts ALL parts** (append vào mọi prompt):
> ```
> no chibi proportion, no big head, no anime style, no kawaii, no manga,
> no Chinese cartoon, no detailed eyes with iris, no eye sparkle, no smooth
> airbrush gradient, no clean uniform digital outline, no vector style, no
> cell-shaded hard edge shadow, no saturated bright colors, no neon, no
> shadow on ground, no ground, no background, no border frame, no watermark,
> no text, no extra body parts, no duplicate limbs, no anatomy errors, no
> blurry edges, no anti-alias bleeding past outline.
> ```

---

## §0 — Acceptance test prompt (gen 1 style ref FIRST)

Use this prompt to gen 1 full-body style ref. Verify against
`PLAYER_DST_REFERENCE.md` §5 checklist BEFORE proceeding to 30 atomic parts.

```
Don't Starve Together cutout art, by Jeff Agala / Klei Entertainment style.
Hand-painted gouache + watercolor wash, visible brush strokes inside fills.
Pencil sketch construction lines peeking through outline at ~15% opacity.
Calligraphy ink outline variable width 4-12px (thick shadow / thin highlight),
sepia-tinted black #1a1408 (NOT pure black). Muddied desaturated palette,
saturation cap 30%, sepia/ochre tonal overlay throughout.

Subject: full-body young-adult male wuxia cultivator. ~5 head-tall lanky
proportion (Wilson DST reference: narrow shoulders, long thin arms reaching
mid-thigh, long legs, oversized mitten-style hands, oversized boot soles).
Side profile facing right. Standing relaxed neutral pose with slight slouch.
Looking forward with curious expression. Single character isolated.

Outfit (Linh Khí Wuxia overlay): cream wuxia kimono robe (highlight #e8d8b8
mid #c8b094 fold #8a6f47), short above hip line, sleeves TIGHT TO ARM (NOT
bell-flow). Gold sash with bow knot at waist (light #a8884a shadow #7a5a30).
Jade pendant + cloud sigil on chest (jade #7a9078). Trousers warm-charcoal
(#3a3530). Brown leather boots (#5a4830) with strap (#a89878). Hair ink-black
topknot bun (#2a2418) with cream silk ribbon trailing. Skin highlight #c8a884
mid #a08868 shadow #5a4828.

Face minimalism: small dot eyes #1a1408, single line mouth, tiny angle nose,
expressive brush-stroke eyebrows. NO detailed iris. Optional subtle cheek
blush #c89878 at 40% opacity.

Background: transparent RGBA, no shadow on ground, no border, no watermark.

Negative: no chibi proportion, no big head, no anime style, no kawaii, no
manga, no Chinese cartoon, no detailed iris eyes, no eye sparkle, no smooth
airbrush, no clean uniform digital outline, no vector cel-shaded, no saturated
bright colors, no neon, no shadow on ground, no background, no border, no
watermark, no text, no extra limbs, no anatomy errors.
```

After gen, run reference §5 acceptance checklist. ≥8/10 → proceed §E/§N/§S.
<8/10 → adjust prompt with stronger DST language, re-test.

---

## §E — East direction (side view, character facing right)

### E/head.png (target ~210×220)
```
Don't Starve Together cutout art, Klei / Jeff Agala style. Hand-painted gouache
+ watercolor wash, visible brush strokes, pencil sketch lines ~15% opacity.
Calligraphy ink outline 4-12px variable width (thick shadow / thin highlight),
sepia-tinted black #1a1408. Muddied desaturated palette, saturation cap 30%.

Subject: ISOLATED HUMAN HEAD ONLY, side profile facing right, neck cut clean
at jaw line. ~1/5 of total body height (lanky adult proportion, NOT chibi big head).

Composition: ONLY skull + face + hair. Single profile-right view: 1 dot eye
visible, tiny angle nose pointing right, ear visible. Hair tied in topknot bun
on crown with cream silk ribbon trailing back. Bottom edge = horizontal cut at
jaw line — NO neck visible past jawline, NO collar, NO shoulders.

Anatomy: lanky adult young-male DST proportion, narrow head, slight slouch
forward (curious Wilson pose). Face minimalism: single small dot eye #1a1408
~3-5px, single line mouth ~1-2px thick, tiny angle nose ~5-8px brush stroke,
expressive brush-stroke eyebrow ~10-15px above eye. Skin tone muddied warm tan
highlight #c8a884 mid #a08868 shadow #5a4828. Optional subtle cheek blush
#c89878 at 40% opacity.

Palette LOCK: hair ink-black #2a2418 base + highlight #4a4030, ribbon cream
#e8d8b8, skin #c8a884 / #a08868 / #5a4828, lip line #6a3a28, outline
sepia-ink #1a1408 variable width.

Negative: NO neck below jaw, NO shoulders, NO collar, NO torso, NO body, NO
multiple eyes, NO detailed almond iris, NO anime eye, NO front view, NO back
view, NO chibi big head, NO anime style, NO kawaii, NO smooth airbrush, NO
clean uniform outline, NO saturated colors, no shadow, no ground, no background,
no border, no watermark, no text, no anatomy errors, no blurry edges.
```

### E/torso.png (target ~120×260, narrower than current — TRUNK ONLY)
```
Don't Starve Together cutout art, Klei / Jeff Agala style. Hand-painted gouache
+ watercolor wash, visible brush strokes, pencil sketch lines ~15% opacity.
Calligraphy ink outline 4-12px variable width, sepia-tinted black #1a1408.
Muddied desaturated palette, saturation cap 30%.

Subject: ISOLATED HUMAN TORSO ONLY (TRUNK), side profile facing right.
Cylindrical narrow trunk shape — chest + belly + back area only. NO sleeves,
NO arms, NO shoulders extending past body width, NO neck, NO hips/legs.

Composition: narrow vertical cylinder profile, ~1.5 head heights tall, narrow
shoulder lanky build (NOT broad). Top edge = clean horizontal cut at shoulder
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
NO pants, NO feet, NO neck, NO head, NO double layers, NO chibi proportion,
NO anime style, NO kawaii, NO smooth airbrush, NO saturated colors, NO clean
uniform outline, no shadow, no ground, no background, no border, no watermark,
no text, no anatomy errors.
```

### E/arm_left.png (target ~80×200)
```
Don't Starve Together cutout art, Klei / Jeff Agala style. Hand-painted gouache
+ watercolor wash, visible brush strokes, pencil sketch lines ~15% opacity.
Calligraphy ink outline 4-12px variable width, sepia-tinted black #1a1408.
Muddied desaturated palette, saturation cap 30%.

Subject: ISOLATED LEFT UPPER ARM ONLY (shoulder cap to elbow joint), side profile.
Cylindrical lanky limb shape with tight kimono sleeve. NO hand, NO forearm,
NO torso, NO bell-flow.

Composition: narrow cylinder hanging straight down, ~1.4 head heights tall
(long lanky arm). Top edge = rounded shoulder cap (where arm meets torso).
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
Don't Starve Together cutout art, Klei / Jeff Agala style. Hand-painted gouache
+ watercolor wash, visible brush strokes, pencil sketch lines ~15% opacity.
Calligraphy ink outline 4-12px variable width, sepia-tinted black #1a1408.
Muddied desaturated palette, saturation cap 30%.

Subject: ISOLATED LEFT FOREARM + HAND ONLY (elbow joint to fingertips),
side profile. Forearm cylinder + visible mitten-style oversized hand at bottom
in relaxed pose.

Composition: cylindrical forearm hanging down from elbow, ~1.5 head heights tall.
Top edge = rounded elbow joint (matches arm_left bottom edge). Sleeve cuff visible
~70% down (where wrist is). **Mitten-style hand** visible at bottom 30% — DST
signature: 4 fingers fused as one shape with thumb separate, OR closed casual fist.
Hand oversized ~1.3× forearm-end width (Wilson DST proportion).

Clothing: kimono sleeve cuff cream-tan #c8b094 banded at wrist. Forearm has tight
sleeve same cream as upper arm. Hand skin tone matches face highlight #c8a884
mid #a08868 shadow #5a4828.

Palette LOCK: sleeve cream #e8d8b8 / #c8b094 / #8a6f47, cuff #a8884a, skin
#c8a884 / #a08868 / #5a4828, outline sepia-ink #1a1408 variable width.

Visible brush strokes inside fills, pencil construction lines visible.

Negative: NO floating hand (must attach to forearm), NO bell-flow, NO sleeve
covering hand, NO realistic 5-finger detailed hand (DST = mitten style preferred),
NO weapon, NO accessory, NO upper arm, NO torso, NO chibi proportion, NO anime
style, NO smooth airbrush, NO clean uniform outline, no shadow, no ground, no
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
Don't Starve Together cutout art, Klei / Jeff Agala style. Hand-painted gouache
+ watercolor wash, visible brush strokes, pencil sketch lines ~15% opacity.
Calligraphy ink outline 4-12px variable width, sepia-tinted black #1a1408.
Muddied desaturated palette, saturation cap 30%.

Subject: ISOLATED LEFT UPPER LEG ONLY (hip joint to knee joint), side profile.
Cylindrical lanky thigh with tight trousers fabric. NO foot, NO shin, NO hip
detail past joint, NO sash.

Composition: cylindrical thigh hanging down, ~1.5 head heights tall (long lanky
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
Don't Starve Together cutout art, Klei / Jeff Agala style. Hand-painted gouache
+ watercolor wash, visible brush strokes, pencil sketch lines ~15% opacity.
Calligraphy ink outline 4-12px variable width, sepia-tinted black #1a1408.
Muddied desaturated palette, saturation cap 30%.

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
Don't Starve Together cutout art, Klei / Jeff Agala style. Hand-painted gouache
+ watercolor wash, visible brush strokes, pencil sketch lines ~15% opacity.
Calligraphy ink outline 4-12px variable width, sepia-tinted black #1a1408.
Muddied desaturated palette, saturation cap 30%.

Subject: ISOLATED HUMAN HEAD ONLY, back-view (back of skull facing camera).
NO face, NO eyes, NO mouth visible. Just back-of-head ink-black hair tied in
topknot bun with cream silk ribbon trailing visible from rear, neck stub ZERO
past jawline.

Composition: hair has slight gloss highlight on crown (subtle, NOT anime gloss
stripe). Lanky adult head proportion, NOT chibi big head. Bottom edge clean
horizontal cut at jaw line.

Palette LOCK: hair ink-black base #2a2418 + highlight #4a4030, ribbon cream
#e8d8b8, neck-cap-area visible skin #c8a884 / #a08868, outline sepia-ink #1a1408
variable width.

Visible brush strokes in hair fill, pencil construction lines visible.

Negative: NO face, NO eyes, NO mouth, NO front view, NO profile, NO ear, NO
neck below jaw, NO shoulders, NO collar, NO torso, NO chibi big head, NO anime
gloss highlight stripe, NO smooth airbrush, NO clean uniform outline, NO
saturated colors, no shadow, no ground, no background, no border, no watermark,
no text, no anatomy errors.
```

### N/torso.png (target ~140×280, narrower than current — TRUNK ONLY)
```
Don't Starve Together cutout art, Klei / Jeff Agala style. Hand-painted gouache
+ watercolor wash, visible brush strokes, pencil sketch lines ~15% opacity.
Calligraphy ink outline 4-12px variable width, sepia-tinted black #1a1408.
Muddied desaturated palette, saturation cap 30%.

Subject: ISOLATED HUMAN TORSO ONLY (TRUNK), BACK-VIEW. NO sleeves, NO arms,
NO neck, NO legs. Back of robe + sash knot.

Composition: rectangular trunk silhouette, narrow shoulder lanky build, ~1.5
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
Don't Starve Together cutout art, Klei / Jeff Agala style. Hand-painted gouache
+ watercolor wash, visible brush strokes, pencil sketch lines ~15% opacity.
Calligraphy ink outline 4-12px variable width, sepia-tinted black #1a1408.
Muddied desaturated palette, saturation cap 30%.

Subject: ISOLATED LEFT UPPER LEG ONLY, back-view (calf-from-behind angle).
NO foot, NO shin, NO hip past joint.

Composition: cylindrical lanky thigh from rear angle, ~1.5 head heights tall.
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
Don't Starve Together cutout art, Klei / Jeff Agala style. Hand-painted gouache
+ watercolor wash, visible brush strokes, pencil sketch lines ~15% opacity.
Calligraphy ink outline 4-12px variable width, sepia-tinted black #1a1408.
Muddied desaturated palette, saturation cap 30%.

Subject: ISOLATED HUMAN HEAD ONLY, front view facing camera, neck cut clean
at jaw line.

Composition: ONLY skull + face + hair. Front view: 2 dot eyes visible, tiny
angle nose centered, single line mouth small relaxed, ears at sides. Hair tied
in topknot bun on crown with cream silk ribbon. Hair has subtle center part.
Bottom edge horizontal cut at jaw line.

Anatomy: lanky adult young-male DST proportion, narrow head, NOT chibi big head.
Slight slouch forward (curious Wilson pose). Face minimalism: 2 small dot eyes
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
eye sparkle, NO chibi big head, NO kawaii, NO smooth airbrush, NO clean
uniform outline, NO saturated colors, no shadow, no ground, no background,
no border, no watermark, no text, no anatomy errors, no blurry edges.
```

### S/torso.png (target ~140×290, narrower than current — TRUNK ONLY)
```
Don't Starve Together cutout art, Klei / Jeff Agala style. Hand-painted gouache
+ watercolor wash, visible brush strokes, pencil sketch lines ~15% opacity.
Calligraphy ink outline 4-12px variable width, sepia-tinted black #1a1408.
Muddied desaturated palette, saturation cap 30%.

Subject: ISOLATED HUMAN TORSO ONLY (TRUNK), FRONT-VIEW. NO sleeves, NO arms,
NO neck, NO legs.

Composition: rectangular trunk silhouette TIGHT TO BODY, narrow shoulder lanky
build, ~1.5 head heights tall. Top edge = clean horizontal cut at shoulder height.
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
Don't Starve Together cutout art, Klei / Jeff Agala style. Hand-painted gouache
+ watercolor wash, visible brush strokes, pencil sketch lines ~15% opacity.
Calligraphy ink outline 4-12px variable width, sepia-tinted black #1a1408.
Muddied desaturated palette, saturation cap 30%.

Subject: ISOLATED LEFT UPPER LEG ONLY, front view. NO foot, NO shin, NO hip
detail past joint.

Composition: cylindrical lanky thigh front view, ~1.5 head heights tall. Top
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
animation style). All RGBA transparent BG. All same Klei / Jeff Agala aesthetic:
hand-painted gouache + watercolor wash, visible brush strokes inside fills,
pencil sketch construction lines ~15% opacity, calligraphy ink outline 4-12px
variable width sepia-tinted #1a1408, muddied desaturated palette saturation
cap 30%.

Proportion lock: ~5 head-tall lanky adult young-male (Wilson DST reference).
Narrow shoulders. Long thin arms reaching mid-thigh. Long legs. Oversized
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

Negative: no chibi proportion, no big head, no anime, no kawaii, no manga, no
Chinese cartoon, no detailed iris, no anime eye sparkle, no smooth airbrush, no
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
