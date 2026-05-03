---
name: art-style
audience: both
status: active — locked
scope: Style bible: palette HEX, line weight, naming convention. Đọc trước khi gen art.
depends-on: []
---
# Art Style Bible

> **Lock document** cho mọi asset visual của Wilderness Cultivation Chronicle.
> Mục tiêu: 1 nguồn duy nhất để palette, lighting, sprite spec không drift qua 150-170 asset
> được sinh bằng AI (Leonardo / GPT-4o image / Image 2.0).
>
> **Đã quyết:**
> - Style: **Hand-painted painterly** (Don't Starve / Hades / Black Myth Wukong concept art vibe)
> - Theme: **Asian wuxia / cultivation fantasy** (tu tiên VN — không Japanese-otaku)
> - View: **Top-down ~30°** (camera hơi nghiêng, không true 90° overhead)
> - Tool chủ lực: **Leonardo AI** (custom Element / Style Reference)
>
> **Cập nhật khi:** đổi palette, đổi style direction, thêm biome mới có sub-palette riêng.
> KHÔNG đổi mid-production — sẽ break consistency với asset đã sinh.

---

## 1. Style anchor

### 1.1 Reference

| Game / Concept | Học gì |
|---|---|
| **Don't Starve** | Outline 1.5-2px màu đậm, palette giới hạn, mood foreboding nhưng vẫn charm |
| **Hades** | Hand-painted brush stroke nhìn rõ, character readable trên BG phức tạp, color pop |
| **Black Myth Wukong (concept art)** | Asian fantasy, mountain mist, jade/cinnabar palette |
| **Genshin Impact (key art)** | Cultivation aesthetic, qi mist, robe flow |
| **Sky: Children of Light** | Soft glow, ambient particle, atmospheric |

### 1.2 Style description (paste này vào mọi Leonardo prompt)

```
hand-painted painterly digital illustration, visible brush strokes,
soft cel-shading, top-down perspective angled 30 degrees,
asian wuxia cultivation fantasy aesthetic, atmospheric ambient lighting,
limited color palette with strong color identity per region,
clean readable silhouette, 1.5-2 pixel mid-tone outline
```

### 1.3 KHÔNG làm

- KHÔNG pixel art (đã chốt painterly).
- KHÔNG photo-realistic (asset cost cao + không hợp mobile).
- KHÔNG anime moe / chibi (tone game serious — không kawaii).
- KHÔNG gradient mềm quá 4 stop — giữ 3-4 stop tonal max để brush stroke còn nhìn ra.
- KHÔNG drop shadow đen tuyệt đối — dùng tone shadow của ground (vd shadow trên cỏ = green-brown đậm, không pure black).
- KHÔNG mix style giữa biome — desert vẫn cùng painterly với forest, chỉ palette khác.

---

## 2. Palette

### 2.1 Universal palette (UI + character + global)

| Role | Color | Hex | Use |
|---|---|---|---|
| Primary gold | ![](https://placehold.co/16x16/d4a64a/d4a64a) | `#d4a64a` | UI accent, button highlight, sect lantern |
| Cinnabar red | ![](https://placehold.co/16x16/8b3a3a/8b3a3a) | `#8b3a3a` | Damage hit, fire, lantern |
| Jade green | ![](https://placehold.co/16x16/6b8e62/6b8e62) | `#6b8e62` | Healing, plant, grass tone |
| Spirit qi blue | ![](https://placehold.co/16x16/a8d8ff/a8d8ff) | `#a8d8ff` | Qi mist, mana, meditation glow |
| Sky qi mid | ![](https://placehold.co/16x16/6fb5e0/6fb5e0) | `#6fb5e0` | Spirit pool, water highlight |
| Bone cream | ![](https://placehold.co/16x16/e8d5a6/e8d5a6) | `#e8d5a6` | Paper, robe, light highlight |
| Ink black | ![](https://placehold.co/16x16/1a1a1a/1a1a1a) | `#1a1a1a` | Outline mid-tone (NEVER pure `#000`) |
| Death qi purple | ![](https://placehold.co/16x16/9b6b8b/9b6b8b) | `#9b6b8b` | Tử khí mist, poison, demonic |

### 2.2 Forest sub-palette — "Rừng Linh Mộc"

> **Mood**: linh khí dồi dào, sương sớm, ánh nắng xuyên tán lá. Cảm giác "nhà".

| Role | Hex | Use |
|---|---|---|
| Deep moss | `#4a6741` | Base ground, deep shadow |
| Sage mid | `#6b8e62` | Mid-tone grass |
| Mint highlight | `#a8c69b` | Highlight grass, leaf glow |
| Bark brown | `#b89968` | Tree trunk, log |
| Dry leaf | `#8a6f47` | Fallen leaf, dirt patch |
| Mushroom red | `#a14040` | LinhMushroom cap |
| Lantern gold | `#d4a64a` | Old shrine lantern (universal accent) |

**Vibe**: warm earth tones + jade green + occasional gold accent. Avoid blue except for water and qi mist.

### 2.3 Stone Highlands sub-palette — "Đá Sơn Cao Nguyên"

> **Mood**: cao nguyên đá xám, cây thông gầy, sương lạnh đêm. Transition zone.

| Role | Hex | Use |
|---|---|---|
| Slate gray | `#7a7c80` | Base stone ground |
| Highlight stone | `#a3a5a8` | Top-lit stone |
| Shadow stone | `#5a5d63` | Deep crack, shadow |
| Dry moss | `#8a9b8c` | Moss patch on stone |
| Bone white | `#c2c4ba` | Fallen bones, old structure |
| Mineral blue | `#4d6b8c` | Mineral ore vein |
| Highland flower purple | `#8a6ba1` | Wildflower accent |

**Vibe**: cool gray + dusty green + cold blue. Outline darker (`#3a3d42`) để tăng contrast.

### 2.4 Desert sub-palette — "Hoang Mạc Tử Khí"

> **Mood**: hoang vu, sấm tử khí tím xa xa, tử lily hé nở dưới trăng. Endgame.

| Role | Hex | Use |
|---|---|---|
| Sand base | `#c4a574` | Base sand ground |
| Sand highlight | `#dec594` | Sun-lit ridge |
| Dirt shadow | `#8b7355` | Deep shadow, low ground |
| Dirt deep | `#6b5d40` | Crack, footprint |
| Death qi purple | `#9b6b8b` | Tử khí mist, DeathLily |
| Bone bleached | `#d4d4d4` | Skull, bone decoration |
| Cactus green | `#6b8559` | Cactus body |

**Vibe**: warm sand + hostile purple accent. Day = harsh sunlit; Night = purple-black với glow accent từ DeathLily / qi mist.

### 2.5 Palette swatch image (cần generate)

Sau khi finalize, tạo 1 file `Documentation/assets/palette_swatch.png` (lưu trong repo) là 1 image PNG có 4 dải màu (universal + 3 biome) — dùng làm Leonardo Image Guidance reference.

---

## 3. Lighting / shadow / outline

### 3.1 Lighting direction

- **Key light**: top-left, ấm vàng (#fff1d6)
- **Fill light**: bottom-right, mát xanh (#a8c5dc), ~40% strength
- **Ambient**: tone của biome (forest = warm green ambient, desert = harsh white ambient ban ngày + cold purple ambient ban đêm)
- **Top-down 30° angle**: subject thấy mặt trên + một phần mặt trước. Không pure overhead — mất chiều sâu.

### 3.2 Shadow rule

- **Drop shadow trên ground**: 50% opacity, offset 2px down-right, color = ground shadow tone (NEVER pure black)
  - Forest ground: shadow color `#3a4a32` (deep moss tone)
  - Stone ground: shadow color `#3a3d42` (slate dark)
  - Sand ground: shadow color `#5b4a30` (dirt deep)
- **Self-shadow trên subject**: 30-40% darker version of base color, soft cel edge (2-3 pixel transition).

### 3.3 Outline rule

- **Outline thickness**: 1.5-2 pixel ở 1 PPU = 64
- **Outline color**: NEVER pure `#000`. Use 30-40% darker shade of base color.
  - Subject xanh → outline xanh đậm
  - Subject vàng → outline nâu đậm
  - Subject xám → outline `#3a3d42`
- **Outline behavior**: stronger cho character/mob (dễ đọc trên BG), softer cho background (tile, decoration).
  - Mob/NPC: 2px outline, `#1a1a1a` mix với base color (vd 60% base + 40% black).
  - Resource node (tree, rock, plant): 1.5px outline, base shadow color.
  - Tile: 0-1px outline (mostly seamless, không cần outline rõ).
  - Decoration (flower, bone): 1px outline, very soft.

### 3.4 Brush stroke

- Visible brush stroke trên surface lớn (ground tile, tree bark, mob fur).
- Stroke direction follow form (vd lá cỏ stroke dọc, đá stroke ngang theo crack).
- KHÔNG smooth airbrush gradient — mất painterly feel.

---

## 4. Sprite specs

### 4.1 Pixels Per Unit (PPU) per category

| Category | PPU | Sprite size | Reason |
|---|---|---|---|
| Tile (ground) | 64 | 64×64 | 1 tile = 1 unit Unity, seamless |
| Resource node (tree, rock, mushroom) | 64 | 96×96 (tall) hoặc 64×64 | Stand on tile, scale ~1.5× |
| Mob | 64 | 96×96 (tall) hoặc 64×64 | Scale ngang character |
| Player | 64 | 96×96 | Center-pivot, scale 1.5× |
| NPC | 64 | 96×96 | |
| Decoration | 64 | 32×32 đến 64×64 | Small ambient |
| Item icon | 128 | 128×128 | Show in inventory với high-res |
| UI (button, panel) | 64 | varies | Atlas |
| VFX particle frame | 64 | 64×64 | 8-frame loop |

### 4.2 Frame count

| Type | Frames | Total per entity |
|---|---|---|
| Static (tile, decoration, item icon) | 1 | 1 |
| Resource node stage | 3 (full, harvested, respawning) | 3 |
| Mob walk | 4-frame loop | 4 |
| Mob attack | 4-6 frame | 4-6 |
| Mob death | 1 (decay frame) | 1 |
| Player walk per direction | 4 | 16 (4 dir × 4) |
| Player idle | 2-frame breathing loop | 2 |
| Player attack swing | 4 | 4 |
| VFX (fire, water, magic) | 8-frame loop | 8 |

### 4.3 Mobile budget

- **Atlas total per category**: < 4 MB compressed (ASTC 6×6 cho mob, ASTC 8×8 cho tile background)
- **Per-sprite max raw**: 256 KB PNG (sẽ shrink xuống ~30 KB sau ASTC)
- **Total atlas**: < 30 MB — đủ chứa toàn bộ ~150 asset
- **APK target**: < 150 MB (per MVP_SCOPE.md)

### 4.4 Naming convention

```
Assets/_Project/Art/
├── Tiles/
│   ├── forest/
│   │   ├── tile_forest_grass_01.png
│   │   ├── tile_forest_grass_02.png
│   │   ├── tile_forest_grass_03.png
│   │   └── tile_forest_grass_04.png
│   ├── stone_highlands/
│   │   ├── tile_highlands_stone_01.png
│   │   ├── tile_highlands_stone_02.png
│   │   └── tile_highlands_stone_03.png
│   └── desert/
│       ├── tile_desert_sand_01.png
│       ├── tile_desert_sand_02.png
│       ├── tile_desert_sand_03.png
│       └── tile_desert_sand_04.png
├── Mobs/
│   ├── rabbit/
│   │   ├── rabbit_walk_sheet.png
│   │   ├── rabbit_flee_sheet.png
│   │   └── rabbit_death.png
│   └── ...
├── Resources/
│   ├── forest/
│   │   ├── tree_01.png
│   │   ├── linh_mushroom_01.png
│   │   └── ...
│   └── ...
├── Decorations/
│   └── forest/
│       ├── flower_white_01.png
│       └── ...
├── Items/
│   ├── icon_stick.png
│   ├── icon_stone.png
│   └── ...
└── UI/
    └── ...
```

### 4.5 Unity import settings (preset)

| Setting | Value |
|---|---|
| `Texture Type` | Sprite (2D and UI) |
| `Sprite Mode` | Single (per-tile) hoặc Multiple (sheet) |
| `Pixels Per Unit` | 64 (theo §4.1) |
| `Filter Mode` | Bilinear (painterly cần soft) — KHÔNG Point |
| `Compression` | None cho UI / icon, ASTC 6×6 cho mob, ASTC 8×8 cho tile |
| `Generate Mip Maps` | Off (2D không cần) |
| `Wrap Mode` | Clamp |
| `Max Size` | 256 cho tile, 512 cho mob, 1024 cho atlas |

---

## 5. Leonardo AI workflow

### 5.1 Setup Element / Style Reference (1 lần đầu)

**Mục tiêu**: train Leonardo "nhớ" style hand-painted painterly + Asian wuxia → mọi prompt sau ra style nhất quán.

**Bước**:

1. Generate 3 hero image bằng Leonardo Phoenix model (Leonardo's flagship cho painterly):
   - "Forest scene at dawn — ancient trees, qi mist, cultivator standing, top-down 30° angle, hand-painted painterly digital art"
   - "Stone highland with cloud sea — mountain shrine, lone pine tree, top-down 30° angle, hand-painted painterly"
   - "Desert ruins under purple moonlit sky — bone, broken stele, death qi mist, top-down 30° angle, hand-painted painterly"
2. Pick 4 best variations across 3 hero (12 total). Save as PNG.
3. Upload 12 PNG vào Leonardo → "Training" → "Train Element" (custom LoRA).
   - Element name: `wilderness_cultivation_painterly_v1`
   - Token cost: ~500 token (Apprentice plan đủ).
   - Time: ~30 phút.
4. After training, mọi prompt sau **must** include Element selector. Trong UI: bật "Use Element" → chọn `wilderness_cultivation_painterly_v1`, strength 0.6-0.8.

### 5.2 Prompt template structure

Mỗi prompt PHẢI có 5 phần theo thứ tự:

```
[STYLE ANCHOR]
hand-painted painterly digital illustration, visible brush strokes,
soft cel-shading, top-down perspective angled 30 degrees,
asian wuxia cultivation fantasy aesthetic

[SUBJECT]
{thing being drawn — tile, mob, decoration, etc.}

[BIOME PALETTE]
{palette hex codes from §2 sub-palette}

[TECHNICAL]
{size 64x64 / 96x96, seamless tileable / single subject transparent BG, etc.}

[NEGATIVE]
no pixel art, no photo-realistic, no anime moe, no pure black outline,
no smooth airbrush gradient, no drop shadow on transparent BG
(when applicable), no text, no watermark, single object only
(when subject is one item)
```

### 5.3 Recommended Leonardo settings

| Setting | Value | Note |
|---|---|---|
| Model | **Leonardo Phoenix** | Best cho painterly (default) |
| Style | "Painterly" hoặc custom Element | |
| Image Guidance | 1 hero image (after Element trained, optional) | Strength 0.5 |
| Element | `wilderness_cultivation_painterly_v1` | Strength 0.7 |
| Aspect ratio | 1:1 cho tile, 2:3 cho character (tall) | |
| Size | 1024×1024 (downscale sau) | Max quality |
| Number of images | 4 per generation | Pick best |
| Prompt Magic | On | Better adherence to prompt |
| Negative prompt | (paste từ §5.2 negative section) | |

### 5.4 Iteration workflow

1. Generate 4 variant với prompt template.
2. Pick best 1-2 → save raw PNG.
3. Mở Photopea (free, https://photopea.com) → crop về size đúng (vd 64×64), remove BG nếu cần (Subject auto-select), align center.
4. Export PNG → drop vào `Assets/_Project/Art/Tiles/{biome}/` theo naming §4.4.
5. Unity tự refresh, BootstrapWizard sẽ pick up (sau khi PR Unity import pipeline merge).

### 5.5 Khi nào regenerate

Regenerate nếu output có:
- Mismatch palette (vd forest tile có sand color)
- Pixel-art look (Element strength quá thấp)
- Drop shadow đen sì (negative prompt thiếu)
- Multiple subject (negative thiếu "single object only")
- Photo-realistic (Element strength quá cao + style anchor sai)

---

## 6. Hero image checklist (sinh trước khi batch)

Trước khi sinh batch tile/mob/decoration, sinh 6 hero image làm visual anchor + Element training data.

| # | Title | Prompt brief |
|---|---|---|
| 1 | Forest dawn | Ancient trees + qi mist + cultivator + top-down 30°, forest palette |
| 2 | Forest spirit pool | Hidden glade + glowing pool + lantern + cultivator meditating |
| 3 | Stone highlands cloud sea | Mountain shrine + pine tree + cloud sea below, highlands palette |
| 4 | Stone shrine ruin | Old altar + broken pillar + spirit ribbon, highlands palette |
| 5 | Desert dusk | Endless sand + setting sun + cultivator silhouette, desert palette |
| 6 | Desert tomb | Broken stele + bone + tử khí mist + DeathLily glowing, desert night |

Save 6 hero ở `Documentation/assets/hero_*.png` (cần thư mục mới — sinh sau merge).

---

## 7. Sample prompt structure (preview)

Full prompts sẽ ở `prompts/tileset.txt` PR sau. Đây là 1 sample để hình dung:

```
=== Forest Tile 01 (basic grass) ===
hand-painted painterly digital illustration, visible brush strokes,
soft cel-shading, top-down perspective angled 30 degrees,
asian wuxia cultivation fantasy aesthetic.

Subject: seamless tileable ground texture, lush forest grass with
small wildflowers, scattered fallen leaves.

Palette: deep moss #4a6741 base, sage green #6b8e62 mid-tone,
mint green #a8c69b highlights, bark brown #b89968 leaf accent,
dry leaf #8a6f47 patches.

Technical: 64x64 pixel seamless tileable texture, edges must blend
seamlessly when tiled (left-right and top-bottom continuity),
single ground plane no objects above ground level, brush stroke
visible direction follows blade-of-grass vertical, soft cel-shading
with 1px shadow gradient, no drop shadow.

Negative: no pixel art, no photo-realistic, no anime, no pure black
outline, no smooth airbrush, no characters, no objects above ground,
no text, no watermark, no hard tile boundary lines.
```

→ 12 tile variants (3 biome × 4 variants) sẽ có cùng template, chỉ thay [SUBJECT] + [PALETTE] phần.

---

## 8. DON'T list (consolidated)

1. KHÔNG mix pixel art với painterly trong cùng pack.
2. KHÔNG drop shadow đen pure (`#000`) — luôn dùng tone shadow của ground.
3. KHÔNG outline pure `#000` — luôn 30-40% darker shade của base color.
4. KHÔNG gradient airbrush smooth quá 4 stop tonal.
5. KHÔNG character anime moe / chibi.
6. KHÔNG generate sprite có drop shadow built-in trên transparent BG (Unity tự render shadow runtime nếu cần).
7. KHÔNG lưu PNG > 1024×1024 vào repo (downscale trước khi commit).
8. KHÔNG commit PSD / Photopea project file vào repo (chỉ PNG final).
9. KHÔNG dùng GPT-4o image cho tileset (không có seamless mode).
10. KHÔNG đổi style anchor mid-production (sẽ break consistency với asset đã sinh trước đó).

---

## 9. Cross-references

- **World design**: [`WORLD_MAP_DESIGN.md`](../design/WORLD_MAP_DESIGN.md) — palette + tile variants gợi ý per biome.
- **Architecture**: [`../ARCHITECTURE.md`](../../ARCHITECTURE.md) §1 — folder layout (Art/ folder convention).
- **MVP scope**: [`MVP_SCOPE.md`](../design/MVP_SCOPE.md) — APK budget < 150 MB constraint.
- **Code**: `Scripts/World/BiomeSO.cs` (`groundTileVariants[]` field từ PR #72).

---

## 10. Approval log

| Date | Decision | By |
|---|---|---|
| 2026-04-30 | Style locked: hand-painted painterly + Asian wuxia | musa4 |
| 2026-04-30 | Tool: Leonardo AI primary | musa4 |
| 2026-04-30 | Workflow: ART_STYLE.md trước → prompt batch → import pipeline | musa4 |
