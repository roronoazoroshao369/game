# AI prompts cho asset pipeline

Bộ prompt sẵn để sinh asset cho Wilderness Cultivation Chronicle.
Mỗi prompt follow template 5-part trong [`../Documentation/ART_STYLE.md`](../Documentation/ART_STYLE.md) §5.2.

## Tool support

| Tool | File mặc định | Status |
|---|---|---|
| **Leonardo AI** | `hero.txt`, `tileset.txt` | Native seamless + Element training |
| **GPT-4o image** (DALL-E 3, "Image 2.0") | `tileset_gpt.txt`, dùng chung `hero.txt` | Cần seam fix Photopea cho tile — xem [`gpt_workflow.md`](gpt_workflow.md) |

→ Nếu dùng GPT-4o thay Leonardo, đọc [`gpt_workflow.md`](gpt_workflow.md) trước.

## Workflow

1. **Tuần 1 — Hero anchor (Element training)**: Generate 6 hero image bằng `hero.txt` (raw prompt, chưa có Element). Pick 4 best variation per hero = 24 PNG → train Leonardo Element `wilderness_cultivation_painterly_v1`.
2. **Tuần 2 — Tile batch**: Use `tileset.txt` với Element vừa train. Generate 4 variation per prompt → pick 1-2 best = 12-24 PNG ground tile.
3. **Tuần 3+ — Mob / decoration / item batch**: Add `mob.txt`, `decoration.txt`, `item_icon.txt` files (PR sau).

## File index

| File | Purpose | Count |
|---|---|---|
| `hero.txt` | 6 hero scene (Leonardo + GPT đều dùng được) | 6 prompt |
| `tileset.txt` | Ground tile seamless 64×64 cho 3 biome (Leonardo native tile mode) | 12 prompt (3 biome × 4 variants) |
| `tileset_gpt.txt` | Cùng 12 tile, adapted cho GPT-4o (rule seamless mạnh + cần Photopea seam fix) | 12 prompt |
| `gpt_workflow.md` | GPT-4o workflow: tool comparison, Photopea seam fix step-by-step, recommendations | doc |
| (future) `mob.txt` | 9 mob walk/attack/death sheet | ~30 prompt |
| (future) `decoration.txt` | Visual ambient (flower, bone, lantern…) | ~20 prompt |
| (future) `item_icon.txt` | 22 item icon 128×128 | ~22 prompt |
| (future) `npc.txt` | Vendor + Companion sprite sheet | ~10 prompt |

## Recommended Leonardo settings (for ALL prompts)

- **Model**: Leonardo Phoenix
- **Style**: Painterly (or custom Element after trained)
- **Aspect ratio**: 1:1 (tile, icon) hoặc 2:3 (mob, character)
- **Image size**: 1024×1024 (downscale sau khi pick best)
- **Number of images per generation**: 4
- **Prompt Magic**: ON
- **Element**: `wilderness_cultivation_painterly_v1` (sau khi trained), strength 0.7

## Negative prompt (paste vào mọi generation)

```
no pixel art, no photo-realistic, no anime moe style, no chibi,
no pure black outline, no smooth airbrush gradient,
no drop shadow on transparent background, no text, no watermark,
no signature, no border, single subject only, no duplicate,
no grid lines, no ui elements
```

## Naming output PNG

Khi save từ Leonardo về local, follow naming convention `Documentation/ART_STYLE.md` §4.4:

```
hero_forest_dawn.png
hero_forest_pool.png
hero_highlands_cloud_sea.png
hero_highlands_shrine.png
hero_desert_dusk.png
hero_desert_tomb.png

tile_forest_grass_01.png
tile_forest_grass_02.png
tile_forest_grass_03.png
tile_forest_grass_04.png

tile_highlands_stone_01.png
tile_highlands_stone_02.png
tile_highlands_stone_03.png
tile_highlands_stone_04.png

tile_desert_sand_01.png
tile_desert_sand_02.png
tile_desert_sand_03.png
tile_desert_sand_04.png
```

Drop hero PNG vào `Documentation/assets/` (Documentation-only, không bundle vào APK).
Drop tile PNG vào `Assets/_Project/Art/Tiles/{biome}/` (sẽ auto-detect bởi BootstrapWizard sau PR import pipeline).

## Workflow per generation (step-by-step)

1. Mở Leonardo AI: https://app.leonardo.ai
2. Chọn "Image Generation"
3. Settings panel:
   - Model: Phoenix
   - Aspect: 1:1 (tile) / 2:3 (subject)
   - Image size: 1024×1024
   - Number of images: 4
   - Prompt Magic: ON
4. (Sau khi train Element) Bật "Use Element" → chọn `wilderness_cultivation_painterly_v1` strength 0.7
5. Paste prompt từ file (vd `tileset.txt` line "=== Forest Tile 01 ===")
6. Paste negative prompt từ README này
7. Click Generate — đợi ~30-60s
8. Pick 1-2 best variation
9. Click "Download" → save PNG về local
10. Crop trong Photopea (https://photopea.com): 1024×1024 → 512×512 → 64×64 (3-step downscale giữ chất lượng)
11. (For tile) verify seamless: paste 4× cùng tile thành 2×2 grid → check edge có blend không. Nếu có hard line → regenerate với negative thêm "no hard tile boundary".
12. Save về folder đúng theo naming convention.
13. Commit batch (vd 4 forest tile/lần) vào branch riêng → PR.

## Cost estimate

| Phase | Token | Cost (Apprentice $10) |
|---|---|---|
| Hero × 6 (4 var/each = 24 image) | ~480 token | $0.56 |
| Element training (12-24 image) | ~500 token | $0.59 |
| Tile × 12 (4 var/each = 48 image) | ~960 token | $1.13 |
| **Phase 1+2 total** | **~1940 token** | **~$2.30** |

Apprentice plan ($10/tháng = 8500 token) đủ cho cả Phase 1+2+3.

## Iteration tips

- Nếu output không match style → tăng Element strength 0.7 → 0.85
- Nếu output bị stiff / không painterly → giảm Element strength 0.7 → 0.5, tăng Prompt Magic
- Nếu output có character không mong muốn → thêm negative "no character, no person, no figure"
- Nếu palette sai → paste hex codes ngay trong prompt (dùng template trong file)
- Nếu tile không seamless → switch sang Leonardo "Texture / Pattern" mode (nếu có) hoặc thêm "edge-to-edge tileable, blend at borders" vào positive prompt

## Tham khảo

- [`../Documentation/ART_STYLE.md`](../Documentation/ART_STYLE.md) — full style bible
- [`../Documentation/WORLD_MAP_DESIGN.md`](../Documentation/WORLD_MAP_DESIGN.md) — biome palette & decoration list
- Leonardo docs: https://docs.leonardo.ai/
