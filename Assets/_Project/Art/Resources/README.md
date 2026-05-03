# Resource sprites (tree / rock / grass / herb / flora / water / structures)

Drop PNG vào folder con tương ứng — `BootstrapWizard` sẽ auto-detect khi chạy
`Tools → Wilderness Cultivation → Bootstrap Default Scene` (hoặc menu rời
`Tools → Wilderness Cultivation → Import Resource Art`).

## Layout

```
Assets/_Project/Art/Resources/
├── tree/                     ← cây (forest biome) — share cho mọi biome có treePrefab
│   └── tree_pine_01.png
├── rock/                     ← đá (rock + mineral_rock dùng chung sprite này)
├── water/                    ← nước cell + water_spring (linh tuyền) dùng chung
├── linh_mushroom/            ← nấm linh (forest flora)
├── berry_bush/               ← bụi mâm xôi (forest flora)
├── cactus/                   ← xương rồng (desert flora)
├── death_lily/               ← tử bách hoa (highlands flora)
└── linh_bamboo/              ← linh trúc (forest flora)
```

> mineral_rock + water_spring chia sẻ sprite với rock + water → drop sprite "rock" /
> "water" là cả hai prefab cùng cập nhật. Nếu sau này muốn art riêng cho từng biến thể
> → mở `BootstrapWizard.CreateSprites` thêm sprite id mới rồi tạo folder tương ứng.

## Naming

- Folder name PHẢI khớp sprite id trong `BootstrapWizard.CreateSprites()` (vd `tree`, `rock`, …).
- File name PNG free-form — importer scan `*.png` rồi sort alphabetical, **chọn file đầu tiên**.
  Ví dụ `tree_pine_01.png` thắng `tree_pine_99.png`. Nếu muốn variant nào → đặt số nhỏ hơn ở đầu.

## Sau khi drop sprite

1. Trong Unity Editor: `Tools → Wilderness Cultivation → Import Resource Art`
   - Auto-apply import settings (Sprite mode Single, Bilinear, Compression None, alphaIsTransparency).
   - PPU auto-tính theo placeholder size (`tree` placeholder 32×48 → PPU sao cho world height = 1.5
     unit; user PNG 64×96 → PPU=64, user PNG 1024×1536 → PPU=1024).
2. Hoặc chạy lại Bootstrap full: `Tools → Wilderness Cultivation → Bootstrap Default Scene`.
3. Mở `Assets/Scenes/MainScene.unity` → Play → resource render với sprite thật.

## Fallback

Nếu folder con empty (0 PNG):
- BootstrapWizard fallback dùng `Sprites/{id}.png` nếu có (output `tools/gen_sprites.py`).
- Nếu cả 2 đều thiếu → procedural solid-color placeholder (tree xanh, rock xám, …).
- Game vẫn run được, chỉ là chưa có art thật.

## Spec cho sprite

- **Format**: PNG, RGBA 8-bit, transparent BG.
- **Size**: tối thiểu khớp tỉ lệ placeholder (`tree` 32×48 → 2:3 ratio); upscale 2× / 4× / 8× / 16× OK
  (importer auto-tính PPU). Khuyến nghị 64×96 (2×) hoặc 128×192 (4×) cho mobile.
- **Top-down 90° view** (KHÔNG perspective) cho world prop. Character / mob có thể ¾ view.
- **No drop shadow built-in** (engine có DropShadow VFX riêng).
- Style: xem [`../../Documentation/art/ART_STYLE.md`](../../../Documentation/art/ART_STYLE.md).

## Placeholder size reference (BootstrapWizard.CreateSprites)

| Resource | Placeholder px | Aspect ratio | World units |
|---|---|---|---|
| tree | 32×48 | 2:3 | 1.0×1.5 |
| rock | 32×24 | 4:3 | 1.0×0.75 |
| water | 40×40 | 1:1 | 1.25×1.25 |
| linh_mushroom | 24×24 | 1:1 | 0.75×0.75 |
| berry_bush | 28×22 | ~5:4 | 0.875×0.6875 |
| cactus | 24×32 | 3:4 | 0.75×1.0 |
| death_lily | 24×28 | 6:7 | 0.75×0.875 |
| linh_bamboo | 20×40 | 1:2 | 0.625×1.25 |
