---
name: player-dst-reference
audience: both
status: active — Player identity locked
scope: Visual signature reference for player art regen. Đọc trước AI_PROMPTS.md §3.
depends-on:
  - ART_STYLE.md
---
# Player Soft-DST + Wuxia Reference Lock

Visual signature reference for player art regen. Read this BEFORE
[`AI_PROMPTS.md`](AI_PROMPTS.md) §3 — đây là single source of truth cho player prompt workflow.

Nếu mục tiêu là rig-ready asset nhìn mượt trong procedural puppet hiện tại, đọc thêm
[`DST_RIG_ASSET_GUIDE.md`](../pipelines/DST_RIG_ASSET_GUIDE.md) để biết 7 nguyên tắc practical về silhouette,
pivot boundary, overlap, near/far depth, và front/back ownership.

> **Identity decision (May 2026):** After 3 iteration rounds testing strict DST
> canon prompts (5 head-tall lanky Wilson proportion + variable-width brush
> outline + smooth gouache fills + face minimalism), generic AI gen tools could
> only deliver ~25-30% DST canon adherence due to strong anime/chibi training
> bias. Per user decision, identity locked to **"Chibi Wuxia × Soft-DST"** —
> chibi proportion accepted, but DST signature traits (muddied palette, brush
> outline, atomic-rig composition) preserved. This is the game's distinct visual
> identity, NOT pure DST clone.

> **Reference image** — see [`Documentation/assets/style_refs/player_E.png`](../assets/style_refs/player_E.png).
> Use this as image guidance / `--cref` / IP-Adapter input when generating the
> 30 atomic body parts. The 30 parts must match this reference's: chibi
> proportion, ink outline, hair silhouette (topknot bun + cream ribbon), wuxia
> outfit (cream kimono + brown cuff + jade pendant + sash + leather boot).

## §1 Identity = "Chibi Wuxia × Soft-DST"

What we **keep** from DST canon:
- Hand-painted aesthetic (NOT pixel art / NOT 3D / NOT vector cel-shaded)
- Bold outline silhouette readable at distance
- Atomic body part decomposition for puppet rig
- Muddied palette overlay (≤30% saturation cap)
- Klei-style ink outline tinted toward sepia-ink (not pure black)
- No anime sparkle / no kawaii expression / no detailed iris drawing

What we **adapt** to chibi:
- Proportion ~3.5-4 head-tall (Webber-leaning chibi, NOT Wilson lanky 5-head)
- Face has small detailed eye OK (almond shape with small black pupil) — but
  NO highlight star, NO eyelash flutter, NO multi-color iris
- Hands can be smaller than DST mitten if the gen tool resists oversize
- Smooth flat fill OK — visible brush strokes nice-to-have but not mandatory

What we **lock** from wuxia identity:
- Cream wuxia kimono robe (V-neck collar, hip-length, NO bell-flow sleeve)
- Gold sash bow knot at waist (color muted, NOT bright lemon yellow)
- Jade pendant + cloud sigil on chest
- Topknot bun with cream silk ribbon trailing
- Single asymmetric forelock at front
- Brown leather boots with cream toe / strap
- Warm-charcoal trousers visible mid-shin

## §2 Klei signature traits (preserved from DST canon)

### 2.1 Outline

- **Calligraphy ink, sepia-tinted `#1a1408`** (NOT pure black `#000000`)
- Thickness 8-16px at 1024 canvas. Slight wobble OK. Variable-width nice-to-have
  but uniform 12px also OK if AI tool resists variable-width.
- Slight overshoot at corners (~3px past intersection)

### 2.2 Fill

- Flat 3-color stops per material (light / mid / shadow) with subtle wash gradient
- Visible brush strokes inside fills nice-to-have
- NO smooth airbrush gradient
- NO pure flat solid color (must have at least light/mid/shadow tonal stops)

### 2.3 Palette LOCK (muddied wuxia, saturation ≤30%)

| Color | Hex | Notes |
|---|---|---|
| Skin highlight | `#c8a884` | warm muddied tan |
| Skin mid | `#a08868` | sepia overlay |
| Skin shadow | `#5a4828` | strong sepia |
| Robe cream highlight | `#e8d8b8` | washed-out, NOT bright cream |
| Robe cream mid | `#c8b094` | warm tan-cream |
| Robe fold | `#8a6f47` | strong sepia fold |
| Sash gold light | `#a8884a` | **muted gold, NOT bright lemon yellow** |
| Sash gold shadow | `#7a5a30` | dark muted gold |
| Cuff trim brown | `#8a6f47` / `#5a4030` | warm brown band at wrist |
| Pendant jade light | `#7a9078` | muddied green |
| Pendant jade shadow | `#4a5a48` | dark muddied green |
| Hair ink-black base | `#2a2418` | warm-dark, NOT pure `#000` |
| Hair highlight | `#4a4030` | subtle sepia gloss |
| Trousers base | `#3a3530` | dark olive-charcoal |
| Trousers shadow | `#1a1812` | near-black olive |
| Boot leather base | `#5a4830` | warm dark brown |
| Boot strap | `#a89878` | cream-tan |
| Outline ink | `#1a1408` | sepia-tinted |
| Cheek blush (optional) | `#c89878` opacity 40% | small round patch |

**Saturation check**: every color HSL saturation ≤30%. If a color looks "too clean
cartoon bright" (e.g. lemon yellow `#f0c020`, neon orange `#e48c2e`), desaturate.

### 2.4 Face

Allowed:
- Small almond eye with single black pupil (`#1a1408`)
- Single line mouth (`#6a3a28` thin)
- Tiny angle nose suggestion
- Expressive single-stroke eyebrow

NOT allowed:
- Multi-color iris with rim highlight (anime)
- Sparkle / star / heart in eye (kawaii)
- Detailed eyelash count >2 (anime)
- Smile teeth visible (cartoon)
- Open mouth surprised expression (kawaii)
- Uniform white sclera filling whole eye area (anime)

### 2.5 Anatomy stylization (chibi-OK)

- Proportion: ~3.5-4 head-tall (Webber-leaning chibi)
- Head: ~25-30% of body height (NOT 1/5 Wilson lanky, but NOT >1/3 super-deformed)
- Shoulders: narrow (~1.0-1.2× head width)
- Hands: visible at end of forearm, mitten-style preferred but small fist OK
- Boots: visible sole, slightly oversized OK but not required clown-foot

### 2.6 Pose at idle (atomic neutral)

Neutral pose for atomic body part gen:
- Slight slouch (curious cultivator pose)
- Arms hang relaxed at sides, slight gap from torso (~5° away from vertical)
- Hands relaxed fist or open mitten
- Legs shoulder-width apart, slight knee unlock
- Head straight or tilted ~5° forward

### 2.7 NO-list (drift markers — reject if present)

- ✗ Smooth airbrush gradients (anime cel)
- ✗ Sparkles / hearts / stars in eyes (kawaii)
- ✗ Multi-color iris with rim highlight (anime)
- ✗ Saturated bright colors (`#f0c020` lemon yellow, `#e48c2e` neon orange)
- ✗ Pure black `#000000` outline (use sepia `#1a1408`)
- ✗ Uniform vector clean digital outline at 0px wobble (use slight hand-drawn quality)
- ✗ Hair with multiple anime gloss highlight stripes
- ✗ Cute kawaii expression (raised brows + open mouth + pink blush all over)
- ✗ Bell-flow flaring kimono sleeves (atomic-rig kills this)
- ✗ Floating hands/feet not attached to limbs

## §3 Wuxia identity layer (mandatory, ~80%+ adherence target)

These wuxia signature traits MUST be preserved in every generated part:

1. **Cream wuxia kimono** with V-neck collar, hip-length (NOT long-flowing past hip)
2. **Gold sash with bow knot** tied at right waist, ribbon ends draping ~15% of
   torso height
3. **Brown cuff trim** band at sleeve wrist (~8% of arm length)
4. **Jade pendant + cloud sigil** on chest (upper torso, readable)
5. **Topknot bun on crown** with cream silk ribbon trailing
6. **Single asymmetric forelock** at front
7. **Warm-charcoal trousers** visible mid-shin
8. **Brown leather boots** with cream toe stitch / ankle strap

## §4 Sample soft-DST chibi-wuxia preamble (use for all 30 parts)

```
Chibi wuxia hand-painted illustration, soft Don't Starve Together aesthetic.
Klei-style ink outline (sepia-tinted #1a1408, NOT pure black, thickness 8-16px
at 1024 canvas). Muddied desaturated palette saturation cap 30%, sepia/ochre
tonal overlay. Flat 3-color stops per material with subtle wash gradient.

Chibi proportion ~3.5-4 head-tall (cute young cultivator, NOT lanky adult).
Wuxia identity: cream V-neck kimono robe (highlight #e8d8b8 mid #c8b094 fold
#8a6f47), brown cuff trim (#8a6f47) at sleeve wrist, gold sash bow knot at
waist (light #a8884a shadow #7a5a30, NOT bright lemon yellow), jade pendant
+ cloud sigil on chest (#7a9078), topknot bun ink-black hair (#2a2418, NOT
pure black) with cream silk ribbon trailing, single asymmetric forelock,
warm-charcoal trousers (#3a3530), brown leather boots (#5a4830) with cream
toe / strap (#a89878). Skin warm muddied tan highlight #c8a884 mid #a08868
shadow #5a4828.

Face: small almond eye with single black pupil #1a1408, single line mouth,
tiny angle nose. NO anime sparkle, NO multi-color iris, NO kawaii expression.

Background: transparent RGBA, no shadow on ground, no border, no watermark,
no text.

Negative: no anime sparkle, no kawaii heart eyes, no multi-color iris, no
smooth airbrush gradient, no pure black #000 outline, no saturated bright
colors, no lemon yellow, no neon orange, no anime gloss highlight stripes,
no cute open mouth surprised expression, no bell-flow flaring sleeves, no
floating hands, no shadow on ground, no background, no border, no watermark,
no text, no extra body parts.
```

## §5 Acceptance test for re-generated style ref

Before generating all 30 atomic parts, gen 1 NEW style ref (full body, side
view) using §4 preamble. Compare against this checklist:

- [ ] Wuxia outfit: cream kimono + bow sash + jade pendant + cloud sigil +
      topknot + forelock + cuff trim + leather boot ALL visible
- [ ] Saturation ≤30%: skin reads "muddied tan" (NOT "warm orange"), sash
      reads "muted gold" (NOT "bright yellow"), hair reads "warm-dark"
      (NOT "pure black")
- [ ] Outline sepia-tinted (NOT pure `#000`), thickness 8-16px
- [ ] Eye is small almond with single black pupil (NOT detailed iris with
      sparkle / multiple colors)
- [ ] Sleeve TIGHT to arm, NOT bell-flow / flaring
- [ ] Hand visible at end of arm (mitten or small fist OK)
- [ ] No anime/kawaii markers (no sparkle, no heart eyes, no smooth airbrush)
- [ ] Cream `#e8d8b8` family (washed-out), NOT `#f0e8d0` family (bright)
- [ ] Sash `#a8884a` family (muted gold), NOT `#f0c020` family (lemon yellow)
- [ ] Atomic-rig friendly: silhouette decomposable into head / torso-trunk /
      arms / forearms / legs / shins (no body parts fused)

If ≥7/10 boxes check → proceed gen 30 atomic parts.
If <7/10 boxes → adjust prompt with stronger language emphasis on the failed
boxes + re-test.

## §6 References

- Prompt workflow + atomic prompts: [`AI_PROMPTS.md`](AI_PROMPTS.md)
- Full asset source-board prompt: [`PLAYER_FULL_ASSET_SOURCE_PROMPT.md`](PLAYER_FULL_ASSET_SOURCE_PROMPT.md)
- Validator: `.agents/scripts/validate_player_art.py`
- Style ref image: [`assets/style_refs/player_E.png`](../assets/style_refs/player_E.png)
- Practical rig rules: [`DST_RIG_ASSET_GUIDE.md`](../pipelines/DST_RIG_ASSET_GUIDE.md)
