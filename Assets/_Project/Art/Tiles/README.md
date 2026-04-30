# Real biome tilesets

Drop PNG sprite vào folder con tương ứng — `BootstrapWizard` sẽ auto-detect khi chạy
`Tools → Wilderness Cultivation → Bootstrap Default Scene` (hoặc menu rời
`Tools → Wilderness Cultivation → Import Biome Tiles`).

## Layout

```
Assets/_Project/Art/Tiles/
├── forest/                   ← rừng linh mộc (Perlin 0.0-0.4)
│   ├── tile_forest_grass_01.png
│   ├── tile_forest_grass_02.png
│   ├── tile_forest_grass_03.png
│   └── tile_forest_grass_04.png
├── stone_highlands/          ← đá sơn cao nguyên (Perlin 0.4-0.65)
│   ├── tile_highlands_stone_01.png
│   ├── tile_highlands_stone_02.png
│   ├── tile_highlands_stone_03.png
│   └── tile_highlands_stone_04.png
└── desert/                   ← hoang mạc tử khí (Perlin 0.65-1.0)
    ├── tile_desert_sand_01.png
    ├── tile_desert_sand_02.png
    ├── tile_desert_sand_03.png
    └── tile_desert_sand_04.png
```

## Naming

- Folder name PHẢI khớp `BiomeSO.biomeId` (xem `BootstrapWizard.CreateBiomes()`).
- File name PNG free-form (importer scan `*.png`), gợi ý theo convention
  `tile_{biome}_{type}_{nn}.png` để dễ đọc.

## Sau khi drop sprite

1. Trong Unity Editor: `Tools → Wilderness Cultivation → Import Biome Tiles`
   - Auto-apply import settings (Sprite mode Single, PPU=64, Bilinear, Compression None).
   - Tạo `Tile` asset ở `Assets/_Project/SOs/Tiles/Tile_{biomeId}_{filename}.asset` cho mỗi PNG.
   - Wire `BiomeSO.groundTileVariants[]` cho biome tương ứng.
2. Hoặc chạy lại Bootstrap full: `Tools → Wilderness Cultivation → Bootstrap Default Scene`.
3. Mở `Assets/Scenes/MainScene.unity` → Play → world map render với tile thật
   (variant pick deterministic per cell qua `WorldGenerator.PickGroundTile`).

## Fallback

Nếu folder con empty (0 PNG):
- Biome đó dùng `BiomeSO.groundTile = ground_default` (sprite placeholder solid color).
- Game vẫn run được, chỉ là chưa có tile thật.

## Spec cho sprite

- **Format**: PNG, RGBA 8-bit.
- **Size**: 64×64 (raw); hoặc 128×128 / 256×256 nếu source lớn — Unity import sẽ downscale theo PPU.
- **Tileable**: PNG phải seamless ở 4 cạnh (left↔right, top↔bottom). Verify bằng cách paste
  4× thành 2×2 grid → check không có hard line ở edge.
- **Top-down 90° view** (KHÔNG 30° như character / hero).
- **Không drop shadow built-in** trên transparent BG.
- Chi tiết style: xem [`../../Documentation/ART_STYLE.md`](../../../Documentation/ART_STYLE.md).
- Prompt sẵn cho Leonardo: xem [`../../prompts/tileset.txt`](../../../prompts/tileset.txt).

## Subfolder chưa generate?

Folder không tồn tại không gây crash; importer sẽ skip biome đó. Tạo sub-folder
`forest/` / `stone_highlands/` / `desert/` khi muốn drop sprite của biome đó vào.
