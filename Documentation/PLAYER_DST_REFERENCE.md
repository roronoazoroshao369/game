# Player DST-Canon Reference Lock

100% DST adherence reference for player art regen. Read this before
[`PLAYER_ATOMIC_ART_PROMPTS.md`](PLAYER_ATOMIC_ART_PROMPTS.md) — these are the
visual signature traits AI must match. Without these, output drifts toward generic
chibi/anime cartoon and loses Klei DST identity.

> **Klei art studio reference** — Don't Starve Together (2016+) art directed by
> **Jeff Agala**. Tim Burton-meets-Klei aesthetic: gothic but warm, hand-painted
> watercolor + ink wash, characters look "drawn in a journal".

## §1 Canon characters & target proportion

| Character | Role | Proportion | Closest match for our cultivator |
|---|---|---|---|
| **Wilson Percival Higgsbury** | Default scientist hero | ~5.5 head-tall lanky adult, narrow shoulders, long limbs | ✓ **Primary reference** — adult young scientist→young adult cultivator |
| **Maxwell** | Antagonist / unlockable | ~6 head-tall taller lanky | partial — too tall for early cultivator |
| **Webber** | Spider boy | ~4 head-tall youthful chibi-leaning | partial — too short, too child |
| **Wendy** | Solemn twin | ~4.5 head-tall pre-teen | partial |

**Lock for Player:** ~5 head-tall young-adult (between Wilson and Webber).
NOT ~3-4 head-tall chibi (current style ref is too chibi). Long arms reaching
mid-thigh, long legs (hip→knee = knee→ankle ≈ 1:1).

```
Head height : Total body height = 1 : 5

[head]              ←  H
[neck+shoulders]    ←  ½H
[torso]             ←  1.5H
[hip+upper-leg]     ←  1.5H
[shin+foot]         ←  1.5H
                    ----
                    5H total
```

## §2 Klei signature visual traits (mandatory)

### 2.1 Outline

- **Calligraphy ink brush, variable width 4-12px**
  - Thick (10-12px) on shadow side of limb
  - Thin (4-6px) on highlight side
- **Wobbly, hand-drawn quality** — not pixel-perfect digital line
- Slight overshoot at corners (line extends ~3px past intersection)
- **Sepia-tinted ink color** `#1a1408` — never pure black `#000000`

### 2.2 Fill

- **Gouache + watercolor wash** — visible brush strokes inside fills
- Flat 3-color stops per material (light / mid / dark) with **wash gradient at edge**
  (lighter color bleeds outward toward outline before edge transition)
- **Pencil sketch construction lines visible** at edges of fills (~10-20% opacity)
  — Klei signature, looks like the artist forgot to erase the underdrawing
- NO smooth airbrush gradient
- NO solid flat color

### 2.3 Palette (muddied desaturated)

Klei palette anchor — every color sits in this tonal range:

| Color | DST canon | hex | Notes |
|---|---|---|---|
| Highlight cream | Wilson shirt | `#e8d8b8` | NOT `#f0e8d0` (that's brighter than DST) |
| Mid cream | Wilson shirt mid | `#c8b094` | warm tan-cream, sepia overlay |
| Deep fold | shadow cream | `#8a6f47` | strong sepia |
| Skin highlight | Wilson face | `#c8a884` | NOT `#d4a880` orange |
| Skin mid | Wilson cheek | `#a08868` | muddied warm tan |
| Skin shadow | jaw under | `#5a4828` | sepia shadow |
| Robe gold | Wilson bow tie | `#a8884a` | NOT `#d4a64a` (too bright) |
| Trousers black-brown | Wilson pants | `#3a3530` | dark olive-charcoal |
| Outline ink | all outlines | `#1a1408` | sepia-tinted black |
| Ground shadow | drop shadow | `#3a3025` opacity 60% | NOT `#000000` |

**Saturation cap: 30%.** Apply HSL filter `Saturation -30%` mental check on every
color before locking. If a color looks "too clean" or "too cartoon bright", desaturate.

### 2.4 Face

DST faces are **MINIMALIST** — character identity comes from body silhouette + hair,
NOT face detail.

- **Eyes**: small dots `#1a1408` ~3-5px diameter, OR simple curved-line dashes,
  OR outlined oval with tiny pupil. NEVER full almond eye with iris detail.
- **Mouth**: single horizontal line OR small curve, ~1-3px thick.
- **Nose**: tiny angle line `<` or `^` shape ~5-8px, suggested not detailed.
- **Eyebrows**: short brush stroke ~10-15px, expressive (worried/curious/neutral).
- **Cheek blush**: optional subtle round patch `#d8a888` opacity 40%, ~15-20px diameter.

### 2.5 Anatomy stylization

- **Hands**: oversized relative to forearm (~1.3× wrist width). Often **mitten-like**
  (4 fingers fused as one shape, thumb separate). Wilson's hands are giant gloves.
- **Feet/boots**: oversized soles (clown-foot proportion ×1.3). Long toe direction.
- **Neck**: thin (~½ head width), short (~1/3 head height).
- **Shoulders**: NARROW for lanky build, only ~1.2× head width.
- **Torso**: long rectangular trunk, cinched at waist by sash.
- **Limbs**: slender cylindrical, slight narrowing at joints (elbow/knee).

### 2.6 Pose at idle (atomic neutral)

For atomic body part gen, neutral pose = **slight slouch**:
- Arms hang relaxed at sides, slight gap from torso (~5° away from vertical)
- Hands open relaxed (mitten silhouette readable) OR closed in casual fist
- Legs shoulder-width apart OR closer (lanky stance), slight knee unlock
- Head tilted ~5° forward (curious / pondering) — Wilson signature pose

### 2.7 NO-list (chibi/anime drift markers — reject if present)

- ✗ Big head ≥ 1/3 body height (chibi proportion)
- ✗ Small body with stubby limbs (chibi)
- ✗ Smooth airbrush gradients (anime cel)
- ✗ Detailed iris with highlight in eyes (anime)
- ✗ Sparkles, magical effect on character (anime)
- ✗ Saturated bright colors (#ff or #f0 hex peaks) (anime / kawaii)
- ✗ Clean uniform digital outline (vector / cell-shaded)
- ✗ Cute facial expression with raised eyebrows + open mouth (kawaii)
- ✗ Heart shapes / sparkles in eyes
- ✗ Hair with multiple highlight stripes (anime gloss)
- ✗ Skin shading via cel-shading hard edge (anime)

## §3 Wuxia layer (added on top of DST canon)

Game identity = "Linh Khí Wuxia × DST" lock — DST aesthetic + 4 wuxia signature
traits:

1. **Cream wuxia kimono** instead of Wilson's white shirt + black pants
2. **Gold sash with bow knot at waist** instead of red bow tie at neck
3. **Jade pendant + cloud sigil** on chest (cultivation essence symbol)
4. **Topknot bun with cream silk ribbon** instead of mad-scientist hair sweep

These wuxia elements MUST follow DST stylization rules above (muddied palette,
brush outline, minimalist detail). Don't render the kimono with detailed embroidery
threads — keep it gouache flat with simple silhouette.

## §4 Sample DST-canon prompt block (use as preamble for all 30 parts)

```
Don't Starve Together cutout art, by Jeff Agala / Klei Entertainment.
Hand-painted gouache + watercolor wash. Visible brush strokes inside fills.
Pencil sketch construction lines peeking through outline (~15% opacity).
Calligraphy ink outline with variable width 4-12px (thick on shadow side,
thin on highlight). Sepia-tinted black outline #1a1408 (NOT pure black).
Muddied desaturated palette, saturation cap 30%, sepia/ochre tonal overlay.
Adult young-male proportion ~5 head-tall (Wilson reference, lanky narrow
shoulders, long limbs). NOT chibi, NOT anime, NOT kawaii.
Minimalist face: dot eyes, line mouth, tiny angle nose, expressive brush-stroke
eyebrows. NO detailed iris. NO sparkles.
Oversized mitten-style hands. Oversized boot soles.
Clean transparent background. Tight alpha bbox no padding.
Linh Khí Wuxia overlay: cream kimono robe, gold sash bow knot at waist,
jade pendant + cloud sigil on chest, topknot bun with cream silk ribbon.
Negative: no chibi, no big head, no anime style, no kawaii, no detailed eyes,
no clean digital outline, no smooth flat fills, no airbrush, no saturated
colors, no Chinese cartoon, no manga, no smooth gradient skin, no shadow on
ground, no border frame, no watermark, no text, no extra body parts.
```

## §5 Acceptance test for re-generated style ref

Before generating all 30 atomic parts, gen 1 NEW style ref (full body, side view)
using §4 preamble + this addition:
```
Side profile facing right. Full body. Standing relaxed neutral pose with slight
slouch. Looking forward with curious expression. Single character isolated.
```

Compare new ref against this checklist:

- [ ] Proportion: head height fits ~5× into total body height (count squares)
- [ ] Outline visibly variable-width brush, not uniform digital line
- [ ] Visible brush stroke marks inside cream robe fill (not smooth flat)
- [ ] Pencil construction lines faintly visible at outline edges
- [ ] Palette saturation ≤30% (eyeball: skin should look "muddied tan", not "warm orange")
- [ ] Eyes are dots/dashes, not detailed almond+iris
- [ ] Hands are mitten-style oversized, not slender realistic
- [ ] Cream robe color reads `#e8d8b8` family, not `#f0e8d0` family (washed-out feel)
- [ ] Gold sash reads `#a8884a` family, not `#d4a64a` (muted gold not bright gold)
- [ ] No anime/kawaii markers (no sparkle, no cute expression, no smooth gradients)

If ≥8 boxes check → proceed gen 30 atomic parts.
If <8 boxes → adjust prompt with stronger DST language + re-test ref.

## §6 References

- Atomic prompts: [`PLAYER_ATOMIC_ART_PROMPTS.md`](PLAYER_ATOMIC_ART_PROMPTS.md)
- Composition rules: [`PLAYER_ATOMIC_RULES.md`](PLAYER_ATOMIC_RULES.md)
- Validator: `.agents/scripts/validate_player_art.py`
- Style lock §1: [`AI_PROMPTS.md`](AI_PROMPTS.md#§1-linh-khí-wuxia--dst-8-luật)
- DST visual reference: search "Don't Starve Together character lineup",
  "Wilson DST sprite", "Webber DST sprite", "Klei art Jeff Agala"
