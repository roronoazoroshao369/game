# GPT-4o image (DALL-E 3 / "Image 2.0") workflow

> Adapted workflow khi dùng GPT-4o image thay Leonardo. Trade-offs documented dưới.

## Khi nào dùng GPT vs Leonardo

| Asset type | Leonardo | GPT-4o | Recommend |
|---|---|---|---|
| Hero image / illustration | ⭐⭐⭐⭐ | ⭐⭐⭐⭐⭐ | **GPT** (style + chi tiết tốt hơn) |
| Item icon (128×128) | ⭐⭐⭐⭐ | ⭐⭐⭐⭐ | Tie — chọn theo cost |
| Mob/character sprite | ⭐⭐⭐⭐ | ⭐⭐⭐⭐ | Tie |
| NPC sprite | ⭐⭐⭐⭐ | ⭐⭐⭐⭐ | Tie |
| Decoration (flower, bone…) | ⭐⭐⭐⭐ | ⭐⭐⭐⭐ | Tie |
| **Tile texture seamless** | ⭐⭐⭐⭐⭐ | ⭐⭐ | **Leonardo** nếu được; **GPT + retouch** nếu phải |
| UI panel / button | ⭐⭐⭐ | ⭐⭐⭐⭐⭐ | **GPT** (chữ + clean line tốt hơn) |
| VFX particle frame | ⭐⭐⭐⭐ | ⭐⭐⭐ | **Leonardo** |

## Trade-offs khi dùng GPT-4o cho asset pipeline

### Tốt hơn Leonardo

- Hiểu prompt phức tạp + Asian aesthetic chi tiết hơn (training data lớn hơn)
- Render text, tablet/scroll calligraphy đẹp hơn
- Composition hero scene cinematic hơn
- Đã trả $20/tháng → no extra cost

### Tệ hơn Leonardo

- ❌ KHÔNG có "Tile" / "Tileable" mode → tile sẽ có seam line ở 4 cạnh, phải retouch thủ công
- ❌ KHÔNG có Element/LoRA training → mỗi prompt phải paste full style anchor (~15 dòng) để giữ consistency
- ❌ KHÔNG có Image Guidance (upload reference) — chỉ có DALL-E 2 hay GPT-4o variation, không stylize
- ⚠️ Style drift cao hơn giữa các generation (GPT có chút random hơn LoRA-locked)
- ⚠️ Output format mặc định 1024×1024 PNG hoặc 1024×1792 (portrait) / 1792×1024 (landscape) — cần downscale

## Workflow tổng (adapted cho GPT)

### Step 1 — Hero anchor (visual reference, KHÔNG train)

Vì không train Element được, hero image chỉ dùng làm **reference visual** cho bạn đối chiếu mắt người (style nhất quán giữa các batch). Generate 6 hero theo `prompts/hero.txt` (vẫn dùng được, GPT chỉ cần phrasing đầy đủ).

Save về `Documentation/assets/hero_*.png` (không bundle vào APK).

### Step 2 — Tile generation (cần extra retouch)

Generate 12 tile theo `prompts/tileset_gpt.txt` (file mới — adapted cho GPT, có rule seamless mạnh hơn):

```
Bạn → ChatGPT (GPT-4o, có image generation):
[Paste prompt từ tileset_gpt.txt]

GPT → 1 ảnh PNG 1024×1024 (DALL-E 3, không có 4-variation như Leonardo)
```

⚠️ **GPT-4o image chỉ generate 1 ảnh / request** (DALL-E 3 default). Muốn 4 variation phải request 4 lần.

### Step 3 — Seam fix với Photopea (BẮT BUỘC cho tile)

GPT cho tile gần seamless ~70% — vẫn có seam line nhẹ ở 4 cạnh khi tile lặp. Fix:

1. Mở https://photopea.com (free, web-based, không cần install)
2. File → Open → tile PNG vừa download
3. Filter → Other → **Offset**
   - Horizontal: 512 (= half of 1024)
   - Vertical: 512
   - Wrap Around: ON
   - Click OK → ảnh shift 50% → seam edges hiện ra ở giữa
4. Dùng **Spot Healing Brush Tool** (J) hoặc **Clone Stamp** (S) → quét che seam ở giữa
5. Filter → Other → Offset 512×512 lại → ảnh quay về bình thường (giờ đã seamless 4 cạnh)
6. Image → Image Size → 64×64 (PPU=64) — downscale 3-step: 1024 → 256 → 128 → 64 (giữ chất lượng)
7. File → Export As → PNG → save với naming `tile_forest_grass_01.png`

### Step 4 — Verify seamless

Trong Photopea:
- File → New → 128×128
- Edit → Define Pattern (chọn tile vừa fix)
- Layer → Fill → Pattern → check 2×2 grid không có line ở edge

Nếu vẫn có line → quay lại Step 3 retouch lại.

### Step 5 — Drop vào Unity

Theo workflow PR #76:
1. PNG → `Assets/_Project/Art/Tiles/{forest|stone_highlands|desert}/`
2. Unity Editor → `Tools → Wilderness Cultivation → Import Biome Tiles`
3. Verify scene play

## Cost & timeline

| | Leonardo | GPT-4o |
|---|---|---|
| Subscription | $10/tháng (Apprentice) | $20/tháng (Plus, đã trả) |
| Time per tile (gen) | ~30s × 4 var = 2 phút | ~30s × 4 = 2 phút |
| Time per tile (retouch seam) | 0 phút (tile native) | **~5 phút Photopea** |
| Total per tile | ~2 phút | ~7 phút |
| **12 tiles total** | ~24 phút | **~84 phút (1.5 giờ)** |
| 6 hero (no retouch) | ~12 phút | ~12 phút |
| **Grand total Phase 1+2** | ~36 phút | ~96 phút |

→ GPT mất gấp ~3× thời gian cho tile. Hero không thay đổi.

## Recommendations

1. **Dùng GPT cho hero / icon / character / NPC / decoration / UI** (90% asset pack) — quality tốt hơn, không cần retouch.
2. **Cho tile** — chấp nhận retouch 5 phút/tile (12 × 5 = 1 giờ), hoặc:
   - Switch lại Leonardo SAU khi fix lỗi
   - Hoặc Midjourney `--tile` flag ($10/tháng)
   - Hoặc hand-paint tile trong Photopea (dùng palette từ ART_STYLE.md §2)
3. **Hero image vừa tạo** ("Mystical glade with a lone wanderer"): giữ lại, dùng làm visual reference khi prompt tile + decoration để mắt bạn đối chiếu style nhất quán.

## File index (GPT-adapted prompts)

| File | Purpose | Status |
|---|---|---|
| `prompts/hero.txt` | 6 hero scene — vẫn dùng được cho cả Leonardo + GPT (chỉ paste vào ChatGPT) | ✅ existing |
| `prompts/tileset.txt` | 12 tile — viết cho Leonardo, GPT chạy được nhưng ít rule seamless | ✅ existing |
| `prompts/tileset_gpt.txt` | 12 tile — adapted cho GPT với rule seamless mạnh + retouch instruction | ✅ this PR |
| (future) `prompts/icon_gpt.txt` | 22 item icon adapted cho GPT | ⏳ future |
| (future) `prompts/mob_gpt.txt` | 9 mob sprite adapted cho GPT | ⏳ future |

## Cross-reference

- [`README.md`](README.md) — Leonardo workflow gốc
- [`tileset.txt`](tileset.txt) — Leonardo tile prompts
- [`tileset_gpt.txt`](tileset_gpt.txt) — GPT-adapted tile prompts (this PR)
- [`../Documentation/ART_STYLE.md`](../Documentation/ART_STYLE.md) — style bible
