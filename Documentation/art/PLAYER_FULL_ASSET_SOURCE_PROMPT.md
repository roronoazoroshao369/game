---
name: player-full-asset-source-prompt
audience: both
status: active
scope: 1 prompt GPT image gen full asset source board cho player.
depends-on:
  - AI_PROMPTS.md
  - PLAYER_DST_REFERENCE.md
---
# Player Full Asset Source Prompt (GPT image 2.0)

Một prompt duy nhất để GPT sinh ra **full asset source board** cho player theo hướng gần DST nhất, sau đó dùng board này làm nguồn để hoàn thiện character rig-ready.

> **Đây KHÔNG phải final 30-part sprite sheet.**
> Mục tiêu của file này là tạo ra **1 source board đồng bộ** để:
> 1. lock form / silhouette / palette / costume / direction
> 2. làm style reference cho `images.edit()`
> 3. làm nguồn để crop, paint-over, hoặc tách tiếp thành `head / torso / arm / forearm / leg / shin`
>
> Nếu cần final atomic prompts rig-ready, dùng `Documentation/AI_PROMPTS.md` §3.3.

---

## Khi nào dùng file này

Dùng file này khi bạn muốn tránh lỗi lớn của workflow text-only atomic generation:
- mỗi part tự drift thành một kiểu volume khác nhau
- arm / forearm / leg / shin không cùng một hệ form
- batch ráp lại bị paper-doll / rời rạc

### Thứ tự khuyên dùng

1. Gen **1 source board** bằng prompt dưới
2. Chọn board tốt nhất, lưu làm `Documentation/assets/style_refs/player_source_board_v1.png`
3. Dùng board đó làm ref cho:
   - crop / paint-over tay
   - image-edit từng part khó
   - final rig-ready extraction
4. Chỉ sau đó mới hoàn thiện các file PNG cho rig

---

## Output target

GPT nên sinh ra **1 production sheet** chứa:

- **1 full-body East view** (main hero view)
- **1 full-body South view**
- **1 full-body North view**
- **6 clean callouts / source pieces**:
  - head close-up
  - torso close-up
  - upper arm
  - forearm + fist
  - thigh
  - shin + boot

### Tại sao layout này tốt hơn 1 shot 30 part

- vẫn giữ được **global character consistency**
- nhưng không ép model phải solve cùng lúc 30 pivot-perfect parts
- đủ material để mình cleanup / tách part sau
- gần cách production sheet / turnaround board của artist hơn

---

## Master prompt (copy nguyên block)

```text
GOAL: Create one high-resolution CHARACTER SOURCE BOARD for a 2D procedural puppet game character. This board is NOT the final rig-ready sprite sheet. It is a production source board used to extract, repaint, and finalize rig-ready parts later.

STYLE:
- Visual medium: hand-painted painterly illustration, Don't Starve Together × Chinese wuxia cultivation fusion
- Outline: Klei-style sepia-tinted ink #1a1408 (NOT pure black), variable-width 10-14px at 2048 canvas, chunky brush with hand-drawn wobble and slight overshoot at corners
- Palette: muddied desaturated, every color saturation clamped to MAX 30%, sepia/ochre tonal overlay across whole image
- Rendering: flat 3-color tonal stops per material (light / mid / shadow), subtle watercolor wash gradient, visible brush stroke texture

SUBJECT:
- Character: chibi young male wuxia cultivator, age 12-14, calm expression
- Proportion: EXACTLY 3.5-4 heads tall total body height (NOT 5-head adult, NOT lanky teen, NOT super-deformed >1/3 head)
- Build: narrow shoulders 1.0-1.2 head-widths, readable silhouette, slight forward slouch
- This character must feel rig-friendly, cutout-friendly, and readable in motion like DST

BOARD LAYOUT:
- One single source board image, portrait orientation, clean structured layout, no text labels
- Main figure = full-body EAST view (side profile facing right), largest element on board, occupying about 45-55% of board height
- Secondary figure = full-body SOUTH view (front view), smaller than East, occupying about 28-35% of board height
- Secondary figure = full-body NORTH view (back view), smaller than East, occupying about 28-35% of board height
- Also include 6 isolated clean source callouts on the same board, clearly separated from the three full-body figures:
  1. head close-up
  2. torso close-up (TRUNK ONLY)
  3. upper arm only
  4. forearm + closed mitten fist
  5. thigh only
  6. shin + boot
- All figures and callouts must look like the SAME exact character, same palette, same costume, same outline language
- Keep all elements separated with generous empty space, no overlap, no collage clutter

EAST VIEW (main figure):
- Pure side profile facing right
- One eye visible only
- Main approval silhouette for the character
- Full outfit visible and readable from bun to boot

SOUTH VIEW (secondary):
- Front view facing camera
- Two simple dot eyes visible
- Torso silhouette must read clearly even if front/back rig later hides arm sprites

NORTH VIEW (secondary):
- Back view facing away from camera
- No face visible
- Bun, ribbon, back-of-robe, and back silhouette readable

OUTFIT LOCK:
- Cream wuxia kimono robe, V-neck collar, HIP-LENGTH ONLY
- Robe hem ends at hip, NEVER knee-length, NEVER mid-shin
- Sleeves TIGHT to arm cylinder shape, NO bell-flow, NO flare
- Brown cuff trim band at wrist, occupying about 8% of forearm length
- Muted dusty gold sash bow knot at right waist, NOT bright lemon yellow
- Jade pendant on chest and jade cloud sigil on chest
- Warm-charcoal trousers
- Brown leather ankle boots with cream-tan strap and toe stitch
- Ink-black topknot bun with cream silk ribbon trailing and one asymmetric forelock

RIG-FRIENDLY SOURCE RULES:
- This board must be designed for later cutout extraction into head / torso / arm / forearm / leg / shin
- Keep silhouettes simple and readable
- Show how upper arm, forearm, thigh, and shin should look as clean isolated masses
- Torso callout MUST be trunk only, not a full illustration with double arms baked in
- Forearm callout MUST clearly show sleeve -> cuff -> fist sequence
- Shin callout MUST clearly show trouser -> boot -> sole sequence
- The board must help a human or AI editor finish rig-ready parts later
- Prioritize consistency and usable source information over flashy concept-art posing

FACE:
- Small SOLID DOT pupil(s) only, color #1a1408, no iris, no sclera fill, no eyelash, no highlight star
- Tiny nose suggestion, single short line mouth, simple eyebrow strokes
- No anime sparkle, no heart eyes, no kawaii expression

PALETTE LOCK (exact hex codes):
- Skin: #c8a884 / #a08868 / #5a4828
- Hair: #2a2418 + #4a4030
- Ribbon: #e8d8b8
- Robe: #e8d8b8 / #c8b094 / #8a6f47
- Cuff trim: #8a6f47 / #5a4030
- Sash: #a8884a / #7a5a30
- Jade pendant + cloud sigil: #7a9078 / #4a5a48
- Trousers: #3a3530 / #1a1812
- Boots: #5a4830 / #3a2818 / strap #a89878
- Outline: #1a1408

BACKGROUND / OUTPUT:
- One single clean production board image
- Portrait canvas 2048x3072 PNG
- Plain desaturated warm gray background or transparent background, but keep it clean and non-distracting
- No environment scene, no floor, no props, no atmospheric FX, no text labels, no arrows, no watermark

DO NOT INCLUDE:
- NO final 30-part exploded sprite sheet
- NO random collage layout
- NO dramatic action pose
- NO bell sleeves
- NO knee-length robe
- NO bright yellow sash
- NO anime eyes
- NO smooth airbrush gradient
- NO pure black outline
- NO duplicate inconsistent limbs
- NO giant boots clown-foot style
- NO fused unreadable shapes that cannot be extracted later

REINFORCE:
This is a controlled CHARACTER SOURCE BOARD for later rig extraction. It must feel like one coherent art pack for the same character, not a set of unrelated illustrations.
```

---

## Recommended GPT workflow

### Pass 1 — Generate source board

Dùng prompt trên để gen 2-4 variants. Chọn board có:
- form ổn nhất
- E view đẹp nhất
- callouts usable nhất
- cuff / boot / sash / head silhouette rõ nhất

### Pass 2 — Save best board as source ref

Lưu best image thành:

```text
Documentation/assets/style_refs/player_source_board_v1.png
```

### Pass 3 — Use the board to finish final rig parts

Từ source board này, làm 1 trong 3 hướng:

1. **Manual extract + cleanup**
   - crop từng callout
   - paint-over seam/joint
   - xuất final PNG

2. **Image-edit per part**
   - feed `player_source_board_v1.png` vào `images.edit()`
   - yêu cầu GPT chỉ tạo `torso.png`, `forearm_left.png`, etc.
   - consistency sẽ cao hơn text-only atomic prompts

3. **Hybrid**
   - part dễ thì crop từ source board
   - part khó như far arm / hidden leg / N/S torso thì image-edit tiếp

---

## Acceptance checklist cho source board

- [ ] East / South / North đều đọc ra cùng một nhân vật
- [ ] Head / torso / arm / forearm / leg / shin callouts cùng một hệ volume
- [ ] Torso callout đủ sạch để dùng làm trunk source
- [ ] Forearm callout đọc rõ sleeve → cuff → fist
- [ ] Shin callout đọc rõ trouser → boot → sole
- [ ] No bell sleeve, no long robe, no anime eye drift
- [ ] Outline / palette / texture nhất quán giữa full-body views và callouts
- [ ] Board nhìn như production source, không như random concept collage

Nếu fail 2-3 box lớn, regen board. Đừng cố nhảy thẳng sang final parts.

---

## Relationship với các docs khác

- `Documentation/AI_PROMPTS.md` — master catalog + atomic prompts rig-ready (§3.3)
- `Documentation/PLAYER_SOURCE_BOARD_EXTRACTION.md` — bước tiếp theo: extract 22 PNG rig-ready từ board (image-edit + manual crop workflow)
- `Documentation/PLAYER_DST_REFERENCE.md` — identity lock
- `Documentation/DST_RIG_ASSET_GUIDE.md` — 7 nguyên tắc practical để asset animate mượt trong rig hiện tại
- `Documentation/PUPPET_PIPELINE.md` — cách drop asset vào repo rồi bootstrap rig
- `Documentation/assets/style_refs/player_source_board_v1.png` — board đã PASS, dùng làm reference image cho extraction
