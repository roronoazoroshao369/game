# Player Atomic Art Prompts (regen — fix rời rạc)

Atomic-symbol prompts cho player character regen. **Đọc trước:**
[`PLAYER_ATOMIC_RULES.md`](PLAYER_ATOMIC_RULES.md) — composition rules.

> **Style lock** = Linh Khí Wuxia × DST hand-painted. Ink-wash thick outline 16-24px,
> gouache flat 3-4 stops, NO airbrush, stylized anatomy. Palette: cream
> robe (#f0e8d0 / #d8c8a8 / #b89878 stops), gold sash (#c8a060), jade pendant
> (#7ba089), ink-black hair (#1a1a1a + gloss highlight #3a3030).
>
> **Format ALL parts:** transparent BG (RGBA), tight alpha bbox (no padding),
> single character body part isolated, no shadow on ground.

## TL;DR usage

1. Copy prompt block tương ứng vào AI gen tool.
2. Generate at the recommended dimensions (≈ ±30px tolerance).
3. Save thành `Assets/_Project/Art/Characters/player/{E|N|S}/{part}.png`.
4. Run validator: `python3 .agents/scripts/validate_player_art.py`.

> Negative prompts ALL parts (append vào mọi prompt):
> `no shadow, no ground, no background, no border frame, no watermark, no text,
> no airbrush, no glow effects beyond outline, no extra body parts, no duplicate
> limbs, no anatomy errors, no blurry edges, no anti-alias bleeding past outline.`

---

## §E — East direction (side view, character facing right)

### E/head.png (target ~223×233)
```
Linh Khí Wuxia × DST hand-painted. Ink-wash thick outline 16-24px, gouache flat
3-4 stops, NO airbrush.

Subject: ISOLATED HUMAN HEAD ONLY, side profile facing right, neck cut clean at jaw.

Composition: ONLY skull + face + hair. Single profile-right view: 1 eye visible,
nose pointing right, ear visible. Hair tied in topknot bun on crown with cream silk
ribbon. Bottom edge of sprite = horizontal cut at jaw line — NO neck visible past
jawline, NO collar, NO shoulders.

Anatomy: stylized stocky chibi-realism (DST proportion). Eye almond shape, slight
upturn. Mouth small relaxed line. Skin tone warm tan #d8b89c.

Palette LOCK: hair ink-black #1a1a1a + gloss highlight #3a3030, ribbon cream
#f0e8d0, skin #d8b89c with shadow #b09078 + highlight #f0d8b8, lip #a06848.

Negative: no neck below jaw, no shoulders, no collar, no torso, no body, no
multiple eyes/faces, no profile-from-other-side, no front view, no shadow,
no ground, no background, no border, no watermark, no text, no airbrush,
no extra hair past topknot crown, no anatomy errors, no blurry edges.
```

### E/torso.png (target ~140×260, narrower than current)
```
Linh Khí Wuxia × DST hand-painted. Ink-wash thick outline 16-24px, gouache flat
3-4 stops, NO airbrush, stylized anatomy.

Subject: ISOLATED HUMAN TORSO ONLY (TRUNK), side profile facing right. Cylindrical
trunk shape — chest + belly area only. NO sleeves, NO arms, NO shoulders extending
past body width, NO neck, NO hips/legs.

Composition: narrow cylinder profile. Top edge = clean horizontal cut at shoulder
height (where arm will attach separately). Bottom edge = clean horizontal cut at
hip height. Width consistent top-to-bottom (slight taper at waist OK ≤15%).

Clothing: cream wuxia kimono robe TIGHT TO TRUNK only, NO bell-flow, NO flaring
past body width. Gold sash #c8a060 wrapped at waist (tied side-knot visible).
Jade pendant #7ba089 hanging on chest centered. Cloud sigil #b8a070 embroidered
on robe hem at bottom edge. V-neck collar visible at top.

Palette LOCK: robe cream #f0e8d0 / shadow #d8c8a8 / mid #b89878, sash gold #c8a060
/ shadow #a07840, pendant jade #7ba089 / shadow #5a7a6a, outline ink #1a1a1a.

Negative: NO sleeves, NO arms, NO hands, NO shoulders flaring out, NO bell-flow
fabric, NO flowing hem past hip, NO legs, NO pants, NO feet, NO neck, NO head,
NO triangular silhouette, NO double layers, no shadow, no ground, no background,
no border, no watermark, no text, no airbrush, no anatomy errors.
```

### E/arm_left.png (target ~80×200)
```
Linh Khí Wuxia × DST hand-painted. Ink-wash thick outline 16-24px, gouache flat
3-4 stops, NO airbrush.

Subject: ISOLATED LEFT UPPER ARM ONLY (shoulder cap to elbow joint), side profile.
Cylindrical limb shape with tight kimono sleeve. NO hand, NO forearm, NO torso,
NO bell-flow.

Composition: narrow cylinder hanging straight down. Top edge = rounded shoulder
cap (where arm meets torso). Bottom edge = clean horizontal cut at elbow joint.
Width consistent top-to-bottom (max 10% taper).

Clothing: kimono sleeve TIGHT TO LIMB matching torso color (cream #f0e8d0). Outline
follows limb axis parallel — sleeve fabric does NOT flare out at bottom. Sleeve
ends cleanly at elbow line.

Palette LOCK: sleeve cream #f0e8d0 / shadow #d8c8a8 / mid #b89878, outline ink
#1a1a1a.

Negative: NO bell-flow, NO triangular silhouette, NO flaring sleeve, NO hand,
NO fingers, NO forearm, NO elbow detail past joint, NO shoulder padding past arm
width, NO torso, no shadow, no ground, no background, no border, no watermark,
no text, no airbrush, no anatomy errors.
```

### E/arm_right.png (target ~80×200)
```
Subject + composition + palette + negative: SAME as arm_left.png but mirror profile
(arm hanging on the right side of body in side view, slight perspective foreshorten
~5%). Otherwise identical specs.
```

### E/forearm_left.png (target ~70×220)
```
Linh Khí Wuxia × DST hand-painted. Ink-wash thick outline 16-24px, gouache flat
3-4 stops, NO airbrush.

Subject: ISOLATED LEFT FOREARM + HAND ONLY (elbow joint to fingertips), side profile.
Forearm cylinder + visible 5-finger hand at bottom in relaxed open palm pose.

Composition: cylindrical forearm hanging down from elbow. Top edge = rounded elbow
joint (matches arm_left bottom edge). Sleeve cuff visible ~70% down (where wrist is).
Hand visible at bottom 25% — fingers slightly curled in relaxed pose, palm facing
in (toward body). 5 distinct fingers visible.

Clothing: kimono sleeve cuff #c8a060 tied at wrist. Forearm has tight sleeve same
cream as upper arm. Hand skin tone matches face #d8b89c.

Palette LOCK: sleeve cream #f0e8d0 / shadow #d8c8a8, cuff gold #c8a060 / shadow
#a07840, skin #d8b89c with shadow #b09078, outline ink #1a1a1a.

Negative: NO floating hand (must attach to forearm), NO bell-flow, NO sleeve
covering hand, NO fist (open palm relaxed), NO weapon, NO accessory, NO upper arm,
NO torso, no shadow, no ground, no background, no border, no watermark, no text,
no airbrush, no anatomy errors, no extra fingers (exactly 5).
```

### E/forearm_right.png (target ~70×220)
```
SAME as forearm_left.png but mirror — palm facing in (toward body), arm on right
side of body in side view. 5 fingers visible, otherwise identical specs.
```

### E/leg_left.png (target ~95×225)
```
Linh Khí Wuxia × DST hand-painted. Ink-wash thick outline 16-24px, gouache flat
3-4 stops, NO airbrush.

Subject: ISOLATED LEFT UPPER LEG ONLY (hip joint to knee joint), side profile.
Cylindrical thigh with tight kimono pant fabric. NO foot, NO shin, NO hip detail
past joint, NO sash.

Composition: cylindrical thigh hanging down. Top edge = clean horizontal cut at
hip line (where leg attaches to torso). Bottom edge = rounded knee joint. Width
consistent top-to-bottom (max 15% taper at knee).

Clothing: kimono pants (hakama) TIGHT to thigh — cream-tan #c8a878 fabric. Pants
fabric does NOT flare out at knee. NO drape past knee line.

Palette LOCK: pants tan #c8a878 / shadow #a08858 / mid #807040, outline ink #1a1a1a.

Negative: NO bell-flare hakama, NO flowing fabric past knee, NO foot, NO shoe,
NO shin, NO hip detail past joint, NO torso, NO sash, no shadow, no ground, no
background, no border, no watermark, no text, no airbrush, no anatomy errors.
```

### E/leg_right.png (target ~95×220)
```
SAME as leg_left.png but mirror profile. Otherwise identical specs.
```

### E/shin_left.png (target ~95×210)
```
Linh Khí Wuxia × DST hand-painted. Ink-wash thick outline 16-24px, gouache flat
3-4 stops, NO airbrush.

Subject: ISOLATED LEFT SHIN + FOOT/BOOT ONLY (knee joint to sole), side profile.
Shin cylinder + ankle wrap + boot.

Composition: shin hanging down from knee. Top edge = rounded knee joint (matches
leg_left bottom edge). Bottom 20-25% = leather boot with ankle wrap visible. Sole
of boot at bottom = horizontal ground line.

Clothing: shin has tight kimono pant fabric same tan #c8a878 as upper leg above.
Boot leather brown #6a4830 with cream straps #d8c0a0 wrapping ankle. Sole black
#1a1a1a.

Palette LOCK: pants tan #c8a878, boot leather #6a4830 / shadow #4a2818, strap
cream #d8c0a0, sole #1a1a1a, outline ink #1a1a1a.

Negative: NO floating foot, NO upper leg / thigh, NO knee detail past joint,
NO calf-flare bell-shape pants, no shadow, no ground, no background, no border,
no watermark, no text, no airbrush, no anatomy errors.
```

### E/shin_right.png (target ~110×210)
```
SAME as shin_left.png but mirror profile, slight perspective foreshorten ~10%
(further leg in side view). Otherwise identical specs.
```

---

## §N — North direction (back view, character walking away from camera)

### N/head.png (target ~210×230)
```
Linh Khí Wuxia × DST hand-painted. Ink-wash thick outline 16-24px, gouache flat
3-4 stops, NO airbrush.

Subject: ISOLATED HUMAN HEAD ONLY, back-view (back of skull facing camera). NO face,
NO eyes, NO mouth visible. Just back-of-head ink-black hair tied in topknot bun
with cream silk ribbon trailing visible from rear, neck stub ZERO past jawline.

Composition: hair has slight gloss highlight on crown, asymmetric forelock NOT
visible from back. Bottom edge clean horizontal cut at jaw line.

Palette: hair ink-black #1a1a1a + gloss highlight #3a3030, ribbon cream #f0e8d0,
neck-cap-area visible skin #d8b89c with shadow #b09078, outline #1a1a1a.

Negative: NO face, NO eyes, NO mouth, NO front view, NO profile, NO ear, NO neck
below jaw, NO shoulders, NO collar, NO torso, no shadow, no ground, no background,
no border, no watermark, no text, no airbrush.
```

### N/torso.png (target ~190×305)
```
Linh Khí Wuxia × DST hand-painted. Ink-wash thick outline 16-24px, gouache flat
3-4 stops, NO airbrush.

Subject: ISOLATED HUMAN TORSO ONLY (TRUNK), BACK-VIEW. NO sleeves, NO arms, NO neck,
NO legs. Back of robe + sash knot.

Composition: rectangular trunk silhouette. Top edge = clean horizontal cut at
shoulder height. Bottom edge = clean horizontal cut at hip height. Width consistent.

Clothing: cream wuxia kimono robe back panel TIGHT TO TRUNK. Back of gold sash
#c8a060 visible mid-back with knot tied at side. Cloud sigil #b8a070 embroidered
on lower back. NO bell-flow, NO sleeves visible, NO collar (back of neck = top
edge cut clean).

Palette LOCK: same as E/torso.png.

Negative: NO sleeves baked, NO arms, NO shoulders flaring out, NO front view, NO
profile, NO neck, NO head, NO legs, NO pants, NO bell-shape, no shadow, no ground,
no background, no border, no watermark, no text, no airbrush.
```

> Arm sprites for N/S can be skipped — `PuppetAnimController.hideArmsInFrontBackView=true`
> auto-hides arm/forearm sprites in N/S views. If you do gen them, follow E/arm rules.

### N/arm_left.png + N/arm_right.png + N/forearm_left.png + N/forearm_right.png
```
OPTIONAL — auto-hidden in N view. Skip to save compute. If you must gen:
Use SAME prompts as E direction but back-view orientation.
```

### N/leg_left.png (target ~95×225)
```
Linh Khí Wuxia × DST hand-painted. Ink-wash thick outline 16-24px, gouache flat
3-4 stops, NO airbrush.

Subject: ISOLATED LEFT UPPER LEG ONLY, back-view (calf-from-behind angle). NO foot,
NO shin, NO hip past joint.

Composition: cylindrical thigh from rear angle. Top = hip joint clean cut. Bottom
= knee joint rounded. Width consistent.

Clothing: kimono pants tight to thigh, tan #c8a878. Slight fabric fold visible at
back of knee (anatomy hint).

Palette LOCK: same as E/leg.

Negative: same as E/leg + NO front view, NO profile, NO foot.
```

### N/leg_right.png — mirror of N/leg_left.png

### N/shin_left.png + N/shin_right.png — back view of shin + boot, otherwise same as E

---

## §S — South direction (front view, character facing camera)

### S/head.png (target ~225×245)
```
Linh Khí Wuxia × DST hand-painted. Ink-wash thick outline 16-24px, gouache flat
3-4 stops, NO airbrush.

Subject: ISOLATED HUMAN HEAD ONLY, front view facing camera, neck cut clean at jaw.

Composition: ONLY skull + face + hair. Front view: 2 eyes visible, nose centered,
mouth small relaxed, ears at sides. Hair tied in topknot bun on crown with cream
silk ribbon. Hair has center part. Bottom edge horizontal cut at jaw line.

Anatomy: stylized stocky chibi-realism (DST proportion). Eye almond shape, slight
upturn. Mouth small relaxed line. Skin tone warm tan #d8b89c.

Palette LOCK: hair ink-black #1a1a1a + gloss highlight #3a3030, ribbon cream
#f0e8d0, skin #d8b89c with shadow #b09078 + highlight #f0d8b8, eye iris ink-black
#1a1a1a + sclera off-white #f0e8d0, lip #a06848.

Negative: NO neck below jaw, NO shoulders, NO collar, NO torso, NO body, NO
profile, NO back view, NO multiple faces, no shadow, no ground, no background,
no border, no watermark, no text, no airbrush, no anatomy errors.
```

### S/torso.png (target ~190×310)
```
Linh Khí Wuxia × DST hand-painted. Ink-wash thick outline 16-24px, gouache flat
3-4 stops, NO airbrush.

Subject: ISOLATED HUMAN TORSO ONLY (TRUNK), FRONT-VIEW. NO sleeves, NO arms, NO neck,
NO legs.

Composition: rectangular trunk silhouette TIGHT TO BODY. Top edge = clean horizontal
cut at shoulder height. Bottom edge = clean horizontal cut at hip height. Width
consistent (slight waist taper ≤15%).

Clothing: cream wuxia kimono robe front panel TIGHT TO TRUNK. V-neck collar visible
at top center. Gold sash #c8a060 wrapped at waist with knot tied side. Jade pendant
#7ba089 centered on chest. Cloud sigil #b8a070 embroidered on lower hem at hip.
NO bell-flow sleeves at top, NO flaring out at hip.

Palette LOCK: same as E/torso.png.

Negative: NO sleeves baked, NO arms, NO hands, NO shoulders flaring past trunk
width, NO bell-flow, NO flowing hem past hip, NO legs, NO pants, NO feet, NO
neck visible above shoulders, NO head, NO triangular silhouette, no shadow,
no ground, no background, no border, no watermark, no text, no airbrush.
```

### S/arm + S/forearm — auto-hidden in S view, optional. Use E prompts if gen'd.

### S/leg_left.png (target ~95×225)
```
Linh Khí Wuxia × DST hand-painted. Ink-wash thick outline 16-24px, gouache flat
3-4 stops, NO airbrush.

Subject: ISOLATED LEFT UPPER LEG ONLY, front view. NO foot, NO shin, NO hip detail
past joint.

Composition: cylindrical thigh front view. Top = hip joint clean horizontal cut.
Bottom = knee joint rounded. Width consistent (max 15% taper at knee).

Clothing: kimono pants tight to thigh, tan #c8a878. Center fabric crease line down
the leg axis (anatomical realistic).

Palette LOCK: same as E/leg.

Negative: same as E/leg + NO back view, NO profile, NO foot.
```

### S/leg_right.png — mirror front-view of S/leg_left.png

### S/shin_left.png + S/shin_right.png — front view shin + boot, same scheme

---

## §99 Mega-prompt (1-shot batch)

If your AI gen tool supports batch in single prompt, use this preamble:

```
Generate 30 PNG sprites for atomic puppet rig (DST-style cutout animation).
All RGBA transparent BG. All same Linh Khí Wuxia × DST style: ink-wash 16-24px
outline, gouache flat 3-4 stops, NO airbrush. Palette: cream robe #f0e8d0,
gold sash #c8a060, jade pendant #7ba089, ink-black hair #1a1a1a, skin #d8b89c.

10 body parts × 3 directions:
- head, torso, arm_left, arm_right, forearm_left, forearm_right,
  leg_left, leg_right, shin_left, shin_right
- E (right-side profile), N (back view), S (front view)

CRITICAL atomic-symbol rules (BREAK = REJECT):
1. ONE part = ONE anatomical region. No baked-in adjacent parts.
2. Torso = TRUNK ONLY. No sleeves, no arms, no shoulders flaring, no neck, no legs.
3. Arm = upper arm ONLY (shoulder to elbow). Cylindrical, sleeve TIGHT to limb,
   NO bell-flow, NO hand.
4. Forearm = lower arm + hand. 5 fingers visible at bottom. Sleeve cuff at wrist.
5. Leg = thigh ONLY. Cylindrical, pants tight, NO foot.
6. Shin = lower leg + boot. Sole at bottom horizontal.
7. Head = skull + face + hair ONLY. Bottom = jaw line clean cut. NO neck.
8. Each PNG: tight alpha bbox, no padding, no shadow, no background.

Direction quirks:
- E: right-side profile. 1 eye, ear visible, profile-right. W = flipX of E (skip).
- N: back view. NO face, hair from back, neck cap area only.
- S: front view. 2 eyes, full face, V-neck collar, jade pendant centered.

Per-part prompts: see PLAYER_ATOMIC_ART_PROMPTS.md §E, §N, §S.
Validator: python3 .agents/scripts/validate_player_art.py
```

After gen, **ALWAYS run validator** — `python3 .agents/scripts/validate_player_art.py`
from repo root. It will flag any composition violation (alpha bbox, dimensions,
naming) before re-bootstrap.
