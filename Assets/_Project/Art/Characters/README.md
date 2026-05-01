# Character Art (Puppet)

Drop body-part PNGs để build puppet character (multi-piece animation).

## Folders

```
Art/Characters/
├── player/         ← Player character
├── wolf/           ← (PR H) Wolf
├── fox_spirit/     ← (PR I) FoxSpirit
└── boss/           ← (later) Boss
```

## Required filenames per character

Tối thiểu (puppet không build nếu thiếu 1 trong 2):

- `head.png`
- `torso.png`

Optional:

- `arm_left.png`, `arm_right.png`
- `leg_left.png`, `leg_right.png`
- `tail.png` (cho mob có tail)

PuppetAnimController bỏ qua part nào folder không có (animator nullsafe).

## File requirements

- **Format:** PNG (alpha = trong suốt). JPG cũng accept nhưng mất alpha.
- **Resolution:** flexible — auto-PPU formula trong `ResourceArtSpec.ComputeAutoPPU` giữ world size đồng nhất với placeholder.
- **Pose:** "neutral" — vẽ part ở tư thế trung lập (arm thẳng xuống, leg thẳng đứng). PuppetAnimController sẽ rotate runtime.
- **Pivot:** mỗi PNG nên có pivot ở khớp nối — vd `arm_left.png` pivot ở **vai** (top center), không ở giữa cánh tay. Sprite Editor có thể chỉnh pivot sau import. Default pivot Unity = center (chấp nhận được nhưng motion sẽ centered thay vì swing-from-shoulder).

## GPT Image 2.0 prompt template

```
"Side view, neutral standing pose, [character] [body part], stylized fantasy
art, transparent background, isolated single object, no shadow, no ground,
clean silhouette, 256x512 px"
```

Per part. Examples:

**Player** (cultivation hero, bipedal):
- head: "Side view, neutral, anime cultivation hero head with short black hair, profile facing right, transparent background..."
- torso: "Side view, neutral, white martial arts robe torso (no head, no limbs), embroidery details, transparent background..."
- arms/legs: "Side view, neutral, white robe sleeve straight down, transparent background..."

**Wolf** (quadruped, dark fur). Folder `wolf/`. Tunings: 4.5Hz step, 32° arm swing, 28° leg, tail 16°.
- head: "Side view, neutral, fierce gray wolf head facing right, snarling fangs, yellow eyes, transparent background..."
- torso: "Side view, neutral, gray wolf body (no head, no limbs, no tail), shaggy fur, transparent background..."
- legs: "Side view, neutral, gray wolf foreleg/hindleg straight down, dark fur, transparent background..."
- tail: "Side view, neutral, gray wolf tail straight horizontal, bushy, transparent background..."

**FoxSpirit** (lithe, supernatural). Folder `fox_spirit/`. Tunings: 5.5Hz step, 35° arm, 26° leg, tail 24° + 2.2Hz (active sway).
- head: "Side view, neutral, white nine-tailed fox spirit head facing right, glowing blue eyes, ethereal aura, transparent background..."
- torso: "Side view, neutral, white fox spirit body (no head, no limbs, no tail), ethereal smoke wisps, transparent background..."
- tail: "Side view, neutral, white fox spirit tail with mystical glow, transparent background..."

## Pipeline (auto)

1. Drop PNGs vào `Art/Characters/{characterId}/`.
2. Run `Tools → Wilderness Cultivation → Bootstrap Default Scene` (hoặc
   `Tools → Wilderness Cultivation → Import Character Art` để chỉ scan).
3. CharacterArtImporter detect filename → role mapping (head, torso, etc.).
4. BootstrapWizard.BuildPlayerPrefab → puppet hierarchy + PuppetAnimController.
5. Default body-part offsets in `BuildPuppetHierarchy` ước lượng character
   ~1.5u tall. Tinh chỉnh trong Inspector của child Transform sau Bootstrap.

## Fallback chain

```
Art/Characters/{id}/{head,torso,...}.png  →  puppet hierarchy
       ↓ (any required part missing)
sprites["{id}"] (single PNG từ gen_sprites.py / Sprites/)  →  single SpriteRenderer
       ↓ (also missing)
procedural color rect (placeholder)
```

## Tinh chỉnh per-part

Sau Bootstrap, mở prefab `Assets/_Project/Prefabs/Player.prefab` (hoặc Wolf, etc.):
- Click child Transform (Head, Torso, ArmLeft, ...) → edit Position/Rotation theo pivot PNG của user.
- Click `PuppetAnimController` → tinh chỉnh `walkFrequency`, `armSwingDeg`, `legSwingDeg`, `torsoBobAmplitude`.

## Notes

- Tail sprite folder có thể skip nếu character không có tail (vd Player).
- Optional parts (arms/legs) có thể skip — character vẫn build (chỉ idle bob torso/head).
- Mỗi character `{id}` folder INDEPENDENT — không stack PNG giữa các character.
