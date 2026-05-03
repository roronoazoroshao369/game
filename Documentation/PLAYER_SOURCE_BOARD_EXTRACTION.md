# Player Source Board → Per-Part Extraction (Codex CLI / GPT image 2.0)

Workflow để biến `Documentation/assets/style_refs/player_source_board_v1.png` thành **30 PNG rig-ready** mà không bị drift / paper-doll.

> **Tiền đề:** bạn đã có 1 source board PASS acceptance test (xem `PLAYER_FULL_ASSET_SOURCE_PROMPT.md`).
> Nếu chưa, gen board trước, sau đó quay lại file này.

---

## 1. Tại sao image-edit từ board > gen text-only

| | Text-only atomic | Image-edit từ board |
|---|---|---|
| Identity lock | KHÔNG (mỗi part drift) | ✓ (board = ground truth) |
| Palette consistency | Phải lặp HEX trong từng prompt | ✓ inherit từ board |
| Outline / wash texture | Model tự nghĩ | ✓ match board |
| Proportion match | Khó kiểm soát | ✓ board đã lock |
| Cuff / sash / pendant placement | Drift | ✓ inherit từ board |
| Workload prompt | Verbose 60-80 dòng | Ngắn (~20 dòng) |

→ Tất cả prompt extraction trong file này **đều cần attach board làm reference image** trong Codex CLI / ChatGPT API.

---

## 2. Cách paste vào Codex CLI

```bash
# Trong terminal, mở Codex CLI:
codex

# Trong session, attach board + paste prompt:
> @Documentation/assets/style_refs/player_source_board_v1.png
> [PASTE EXTRACTION PROMPT TỪ §4 BÊN DƯỚI]
```

Hoặc nếu bạn dùng OpenAI Python API:

```python
from openai import OpenAI
client = OpenAI()

with open("Documentation/assets/style_refs/player_source_board_v1.png", "rb") as ref:
    result = client.images.edit(
        model="gpt-image-2",
        image=[ref],
        prompt=EXTRACTION_PROMPT,         # paste 1 atomic block từ §4
        size="1024x1536",
        background="transparent",
        quality="high",
    )

with open("Assets/_Project/Art/Characters/player/E/forearm_left.png", "wb") as f:
    import base64
    f.write(base64.b64decode(result.data[0].b64_json))
```

---

## 3. Universal extraction preamble (PHẢI prepend trước mỗi atomic prompt)

```text
TASK: Extract one isolated character body part from the attached source board reference image. The reference IS the canonical character identity — match its palette, outline language, proportion, costume details, and brush wash texture EXACTLY.

DO NOT redesign the character. DO NOT invent new outfit elements. DO NOT change palette. The extracted part must look like it came directly from this character.

OUTPUT REQUIREMENTS:
- Single isolated body part on transparent background (RGBA, alpha channel)
- Tight bounding box, ~5px transparent padding only
- NO collage, NO duplicate, NO turnaround, NO labels, NO ground shadow, NO color background
- Same outline style, same painterly wash, same sepia tone as the reference board

PIVOT (top-of-sprite = attach point for parent joint):
- head: pivot at bottom-center (neck attach to torso)
- torso: pivot at center
- arm / forearm / leg / shin: pivot at top-center (hangs from parent joint)
```

---

## 4. Per-part extraction prompts

Mỗi block dưới đây = 1 PNG. Paste preamble §3 → paste atomic block → attach board → run.

> Atomic spec chi tiết (palette HEX, exact dimensions, anatomical anchors): xem `AI_PROMPTS.md` §3.3.
> File này tập trung vào **extraction framing** thay vì full generation framing.

### §E — East direction (right-side profile, full 10 parts)

#### `Assets/_Project/Art/Characters/player/E/head.png`

```text
PART: head — east direction.
VIEW: pure right-side profile, ONE eye visible only, calm expression.
ANATOMY: top of bun → bottom of jawline. NO neck visible past jaw, NO torso, NO shoulders.
INCLUDE: ink-black topknot bun, cream silk ribbon trailing back, single asymmetric forelock at front, one visible ear, jaw line.
OUTPUT TARGET: ~190x230px tight bbox, canvas 1024x1536, transparent RGBA.
EXTRACT FROM: head close-up callout in lower-left of board, AND head of E full-body figure (cross-reference both).
```

#### `Assets/_Project/Art/Characters/player/E/torso.png`

```text
PART: torso — east direction.
VIEW: right-side profile trunk only.
ANATOMY: from collarbone base (below neck) to hip line (above thigh). TRUNK ONLY.
INCLUDE: V-neck collar, jade pendant on chest, jade cloud sigil chest patch, gold sash bow knot at right waist, robe hip-length hem.
EXCLUDE: NO arms, NO sleeves past shoulder line, NO head, NO neck, NO legs.
OUTPUT TARGET: ~120x290px tight bbox, canvas 1024x1536, transparent RGBA.
EXTRACT FROM: torso callout in lower-mid of board (sleeveless trunk-only), AND torso of E full-body figure.
```

#### `Assets/_Project/Art/Characters/player/E/arm_left.png` (far-side arm)

```text
PART: upper arm — east direction, LEFT arm = far-side (behind torso in profile).
VIEW: vertical cylinder, top = shoulder, bottom = elbow.
ANATOMY: shoulder joint to elbow only. NO forearm, NO hand, NO torso.
INCLUDE: cream tight kimono sleeve from shoulder to elbow.
EXCLUDE: NO bell-flow, NO cuff (cuff is on forearm), NO mitten fist.
OCCLUSION: this is the FAR arm, slightly desaturated/muted relative to right arm to imply depth (rig will also scale 0.92x).
OUTPUT TARGET: ~70x180px tight bbox, canvas 1024x1536, transparent RGBA.
EXTRACT FROM: shoulder-to-elbow region of E full-body figure (LEFT/back arm).
```

#### `Assets/_Project/Art/Characters/player/E/arm_right.png` (near-side arm)

```text
PART: upper arm — east direction, RIGHT arm = near-side (in front of torso).
VIEW: vertical cylinder, top = shoulder, bottom = elbow.
ANATOMY: shoulder joint to elbow only.
INCLUDE: cream tight kimono sleeve from shoulder to elbow.
EXCLUDE: NO bell-flow, NO cuff (cuff is on forearm), NO mitten fist.
OUTPUT TARGET: ~70x180px tight bbox, canvas 1024x1536, transparent RGBA.
EXTRACT FROM: shoulder-to-elbow region of E full-body figure (RIGHT/front arm), at full saturation.
```

#### `Assets/_Project/Art/Characters/player/E/forearm_left.png`

```text
PART: forearm + closed mitten fist — east direction, LEFT (far-side).
ANATOMY: elbow → wrist → mitten fist. Top = elbow attach.
INCLUDE: cream sleeve covering ~70% of forearm length (from top), brown cuff trim band at Y=70-78% from top, exposed mitten fist Y=78-100%.
EXCLUDE: NO upper arm, NO torso, NO realistic 5-finger anatomy, NO open splayed hand.
OCCLUSION: muted desaturation for far-side depth.
OUTPUT TARGET: ~70x220px tight bbox, canvas 1024x1536, transparent RGBA.
EXTRACT FROM: forearm + fist callout in lower-left of board, AND forearm of E full-body LEFT arm.
```

#### `Assets/_Project/Art/Characters/player/E/forearm_right.png`

```text
PART: forearm + closed mitten fist — east direction, RIGHT (near-side).
Same anatomy as forearm_left but full saturation, no occlusion mute.
EXTRACT FROM: forearm callout in lower-right of board, AND forearm of E full-body RIGHT arm.
```

#### `Assets/_Project/Art/Characters/player/E/leg_left.png` (far-side thigh)

```text
PART: thigh — east direction, LEFT leg = far-side.
ANATOMY: hip → knee. Vertical column.
INCLUDE: warm-charcoal trouser fabric from hip to knee.
EXCLUDE: NO shin, NO boot, NO torso, NO hip belt.
OCCLUSION: muted desaturation for far-side.
OUTPUT TARGET: ~80x190px tight bbox, canvas 1024x1536, transparent RGBA.
EXTRACT FROM: thigh callout in lower-mid of board, AND thigh of E full-body LEFT leg.
```

#### `Assets/_Project/Art/Characters/player/E/leg_right.png`

```text
PART: thigh — east direction, RIGHT leg = near-side.
Same anatomy as leg_left but full saturation, no occlusion mute.
EXTRACT FROM: thigh of E full-body RIGHT leg, full saturation.
```

#### `Assets/_Project/Art/Characters/player/E/shin_left.png` (far-side)

```text
PART: shin + boot — east direction, LEFT (far-side).
ANATOMY: knee → ankle → boot sole. Top = knee attach.
INCLUDE: dark trouser fabric Y=0-65%, brown leather boot Y=65-100%, cream-tan ankle strap at Y=65-72%, boot sole as ground line at Y=100%.
EXCLUDE: NO thigh, NO knee cap detail, NO realistic 5-toe.
OCCLUSION: muted desaturation for far-side.
OUTPUT TARGET: ~80x230px tight bbox, canvas 1024x1536, transparent RGBA.
EXTRACT FROM: shin + boot callout in lower-right of board, AND shin of E full-body LEFT leg.
```

#### `Assets/_Project/Art/Characters/player/E/shin_right.png`

```text
PART: shin + boot — east direction, RIGHT (near-side).
Same anatomy as shin_left but full saturation.
EXTRACT FROM: shin of E full-body RIGHT leg, full saturation.
```

---

### §S — South direction (front view, 6 required + 4 optional)

> Front view = arms auto-hidden by `PuppetAnimController.hideArmsInFrontBackView=true`.
> Required: head, torso, leg_left, leg_right, shin_left, shin_right.
> Optional (can skip): arm_left, arm_right, forearm_left, forearm_right.

#### `Assets/_Project/Art/Characters/player/S/head.png`

```text
PART: head — south direction (front view, facing camera).
VIEW: face directly forward, two simple dot eyes visible, calm expression.
INCLUDE: ink-black topknot bun (visible top), single asymmetric forelock at front, both ears symmetric, two dot eyes, simple nose dot, calm small mouth.
EXCLUDE: NO neck past jaw, NO body.
OUTPUT TARGET: ~190x230px tight bbox.
EXTRACT FROM: head of S full-body figure (center top of board).
```

#### `Assets/_Project/Art/Characters/player/S/torso.png`

```text
PART: torso — south direction (front view).
VIEW: trunk facing camera, NO arms visible.
INCLUDE: V-neck collar centered, jade pendant centered on chest, jade cloud sigil chest patch (LEFT chest from viewer perspective), gold sash bow knot at right waist (viewer's left = wearer's right), robe hip-length hem.
ARMS: NO sleeves visible — front view torso silhouette includes only the trunk, since rig hides arm sprites in front/back direction.
OUTPUT TARGET: ~120x290px tight bbox.
EXTRACT FROM: torso of S full-body figure (front view), inferring trunk-only silhouette.
```

#### `Assets/_Project/Art/Characters/player/S/leg_left.png`

```text
PART: thigh — south direction (front), LEFT leg (viewer's right = wearer's left).
ANATOMY: hip → knee, vertical column, warm-charcoal trousers.
EXCLUDE: NO shin, NO boot.
EXTRACT FROM: LEFT thigh of S full-body figure.
```

#### `Assets/_Project/Art/Characters/player/S/leg_right.png`

```text
PART: thigh — south direction (front), RIGHT leg.
Same as leg_left but mirrored to right side.
EXTRACT FROM: RIGHT thigh of S full-body figure.
```

#### `Assets/_Project/Art/Characters/player/S/shin_left.png`

```text
PART: shin + boot — south direction (front), LEFT.
ANATOMY: knee → ankle → boot sole. Front-view boot shows toe + ankle strap centered.
EXTRACT FROM: LEFT shin/boot of S full-body figure.
```

#### `Assets/_Project/Art/Characters/player/S/shin_right.png`

```text
PART: shin + boot — south direction (front), RIGHT.
Same as shin_left mirrored.
EXTRACT FROM: RIGHT shin/boot of S full-body figure.
```

---

### §N — North direction (back view, 6 required + 4 optional)

> Back view = arms auto-hidden, same as S.
> Required: head, torso, leg×2, shin×2.

#### `Assets/_Project/Art/Characters/player/N/head.png`

```text
PART: head — north direction (back view, facing away from camera).
VIEW: back of head, NO face visible.
INCLUDE: ink-black topknot bun centered at top, cream silk ribbon trailing down back of head, hair flowing down to nape line.
EXCLUDE: NO face features, NO ears past silhouette edge, NO neck past hair line.
OUTPUT TARGET: ~190x230px tight bbox.
EXTRACT FROM: head of N full-body figure (top right of board).
```

#### `Assets/_Project/Art/Characters/player/N/torso.png`

```text
PART: torso — north direction (back view), trunk only.
INCLUDE: back of cream kimono robe, seam line down center spine if visible, gold sash bow knot visible from back at right side (wearer's right = viewer's left), robe hip-length hem.
EXCLUDE: NO arms visible (rig hides), NO V-neck (V-neck is front).
OUTPUT TARGET: ~120x290px tight bbox.
EXTRACT FROM: torso of N full-body figure (back view).
```

#### `Assets/_Project/Art/Characters/player/N/leg_left.png`

```text
PART: thigh — north direction (back), LEFT leg.
Mirror logic of S/leg_left from back perspective.
EXTRACT FROM: LEFT thigh of N full-body figure.
```

#### `Assets/_Project/Art/Characters/player/N/leg_right.png`

```text
PART: thigh — north direction (back), RIGHT leg.
EXTRACT FROM: RIGHT thigh of N full-body figure.
```

#### `Assets/_Project/Art/Characters/player/N/shin_left.png`

```text
PART: shin + boot — north direction (back), LEFT.
Boot back shows heel + ankle strap from behind.
EXTRACT FROM: LEFT shin/boot of N full-body figure.
```

#### `Assets/_Project/Art/Characters/player/N/shin_right.png`

```text
PART: shin + boot — north direction (back), RIGHT.
EXTRACT FROM: RIGHT shin/boot of N full-body figure.
```

---

## 5. Manual crop alternative (zero-cost backup)

Nếu Codex CLI extraction drift hoặc bạn muốn tiết kiệm credit, dùng **manual crop**:

```bash
python3 - <<'EOF'
from PIL import Image
board = Image.open("Documentation/assets/style_refs/player_source_board_v1.png").convert("RGBA")
# Vd: crop forearm_right callout (bbox tay phải bottom)
# Open image trong viewer, đo bbox bằng pixel, cập nhật:
crop = board.crop((760, 1090, 920, 1300))   # left, top, right, bottom
crop.save("Assets/_Project/Art/Characters/player/E/forearm_right.png")
EOF
```

Sau khi crop:
1. Mở part trong GIMP / Photoshop / Photopea
2. Magic-wand background parchment color → delete → transparent
3. Tighten alpha bbox to ~5px padding
4. Save PNG RGBA
5. Run validator

→ **Manual crop tốt cho parts có callout sạch trong board** (head, torso, forearm, thigh, shin+boot).
→ Cần image-edit cho parts thiếu callout (upper arm, N/S body parts từ full-body figure).

---

## 6. Validation + bootstrap

Sau khi gen / crop xong N parts:

```bash
# 1. Verify dimensions + alpha
python3 .agents/scripts/validate_player_art.py

# 2. Re-bootstrap MainScene trong Unity Editor
# Tools → Wilderness Cultivation → Bootstrap Default Scene

# 3. Play mode → idle/walk/attack animation phải mượt, không paper-doll
```

Nếu rig còn rời rạc → đọc `DST_RIG_ASSET_GUIDE.md` 7 nguyên tắc trước khi regen.

---

## 7. Recommended order

1. **E direction first (10 parts)** — full articulation, validate rig đẹp side view
2. **S direction (6 required)** — front view trong town / dialog
3. **N direction (6 required)** — back view khi player walk away from camera
4. **(skip optional 8 N/S arms)** — auto-hidden bởi rig
5. Total: **22 PNG required** để rig hoạt động full 4-direction (W = flip E)

---

## 8. References

- Source board prompt: [`PLAYER_FULL_ASSET_SOURCE_PROMPT.md`](PLAYER_FULL_ASSET_SOURCE_PROMPT.md)
- Full atomic spec (palette HEX, exact bbox): [`AI_PROMPTS.md`](AI_PROMPTS.md) §3.3
- Asset-side rig rules: [`DST_RIG_ASSET_GUIDE.md`](DST_RIG_ASSET_GUIDE.md)
- Validator: `.agents/scripts/validate_player_art.py`
- Pivot spec: `Assets/_Project/Scripts/Core/PuppetPlaceholderSpec.cs`
- Rig spec: `Assets/_Project/Scripts/Core/CharacterRigSpec.cs`
