# GPT image 2.0 (`gpt-image-1`) workflow

> Adapted workflow khi dùng GPT image 2.0 (model `gpt-image-1` mới nhất, ra cuối 2025) thay Leonardo. Trade-offs documented dưới.
>
> ⚠️ **Khác `dall-e-3` cũ**: GPT image 2.0 mạnh hơn nhiều ở instruction following, palette adherence (hex codes), tileability hint, native transparent background. ChatGPT consumer UI vẫn chỉ 1 ảnh / request — muốn variation phải regen.

## Khi nào dùng GPT vs Leonardo

| Asset type | Leonardo | GPT image 2.0 | Recommend |
|---|---|---|---|
| Hero image / illustration | ⭐⭐⭐⭐ | ⭐⭐⭐⭐⭐ | **GPT** (style + chi tiết tốt hơn) |
| Item icon (128×128) | ⭐⭐⭐⭐ | ⭐⭐⭐⭐⭐ | **GPT** (transparent BG native) |
| Mob/character sprite | ⭐⭐⭐⭐ | ⭐⭐⭐⭐⭐ | **GPT** (transparent BG native) |
| NPC sprite | ⭐⭐⭐⭐ | ⭐⭐⭐⭐⭐ | **GPT** |
| Decoration (flower, bone…) | ⭐⭐⭐⭐ | ⭐⭐⭐⭐⭐ | **GPT** |
| **Tile texture seamless** | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐ | **GPT** OK (gần seamless ~80-90%, retouch 1-2 phút) |
| UI panel / button | ⭐⭐⭐ | ⭐⭐⭐⭐⭐ | **GPT** (text + clean line tốt nhất) |
| VFX particle frame | ⭐⭐⭐⭐ | ⭐⭐⭐⭐ | Tie |

## Trade-offs khi dùng GPT image 2.0 cho asset pipeline

### Tốt hơn Leonardo

- Hiểu prompt phức tạp + Asian aesthetic chi tiết hơn (training data lớn hơn)
- Render text, tablet/scroll calligraphy đẹp hơn
- Composition hero scene cinematic hơn
- **Native transparent background** (PNG alpha) — perfect cho item icon / mob sprite / decoration không cần Magic Eraser sau
- **Palette adherence chính xác** — paste `#4a6741` GPT render gần đúng màu hex hơn DALL-E 3
- Đã trả $20/tháng → no extra cost

### Tệ hơn Leonardo

- ⚠️ KHÔNG có "Tile" / "Tileable" toggle native — tile gần seamless ~80-90% nhưng cần Photopea retouch 1-2 phút (vs 0 phút Leonardo)
- ❌ KHÔNG có Element/LoRA training → mỗi prompt phải paste full style anchor (~15 dòng) để giữ consistency
- ❌ KHÔNG có Image Guidance upload reference (Leonardo có Style Reference)
- ⚠️ Style drift nhẹ giữa các generation (GPT có chút random hơn LoRA-locked) — vẫn acceptable
- ⚠️ **1 ảnh / request** (ChatGPT consumer UI) — muốn 4 variation phải request 4 lần (Leonardo: 4 / 1 request)
- ⚠️ Output format mặc định 1024×1024 PNG hoặc 1024×1792 (portrait) / 1792×1024 (landscape) — cần downscale

## Workflow tổng (adapted cho GPT)

### Step 1 — Hero anchor (visual reference, KHÔNG train)

Vì không train Element được, hero image chỉ dùng làm **reference visual** cho bạn đối chiếu mắt người (style nhất quán giữa các batch). Generate 6 hero theo `prompts/hero.txt` (vẫn dùng được, GPT chỉ cần phrasing đầy đủ).

Save về `Documentation/assets/hero_*.png` (không bundle vào APK).

### Step 2 — Tile generation (Photopea retouch ngắn)

Generate 12 tile theo `prompts/tileset_gpt.txt` (file mới — adapted cho GPT, có rule seamless mạnh hơn):

```
Bạn → ChatGPT (GPT image 2.0):
[Paste prompt từ tileset_gpt.txt]

GPT → 1 ảnh PNG 1024×1024 / request
```

→ Muốn 4 variation per tile prompt: regen 4 lần (gõ "another variation" / "try again" sau lần đầu). Pick 1-2 best.

→ Verify seamless trong Photopea (Step 4). ~30% tile sẽ seamless luôn (skip Step 3). ~70% tile cần Photopea retouch 1-2 phút.

### Step 3 — Seam fix với Photopea (CHỈ KHI cần)

GPT image 2.0 cho tile gần seamless ~80-90%. Verify trước (Step 4); nếu seam visible thì fix:

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

| | Leonardo | GPT image 2.0 |
|---|---|---|
| Subscription | $10/tháng (Apprentice) | $20/tháng (Plus, đã trả) |
| Images per request | 4 | 1 (regen 4 lần cho 4 variation) |
| Time per tile (gen 4 var) | ~30s tot (4 var / 1 request) | ~30s × 4 request = ~2 phút |
| Time per tile (retouch seam) | 0 phút (tile native) | **~1-2 phút Photopea** (chỉ ~70% tile cần) |
| Total per tile | ~30s | ~3-4 phút |
| **12 tiles total** | ~6 phút | **~36-48 phút** |
| 6 hero (no retouch, 4 var/hero) | ~3 phút | ~12 phút |
| **Grand total Phase 1+2** | ~9 phút | ~48-60 phút |

→ GPT image 2.0 chậm hơn nhiều so với Leonardo cho tile do **1 ảnh / request** (regen 4 lần) + retouch step. Hero hơi chậm hơn cũng lý do đó.

→ Cho phần còn lại của asset pack (icon, mob, decoration, NPC, UI), GPT vẫn nhanh hơn Leonardo per-asset ở bước post-process nhờ native transparent BG (skip Magic Eraser step), bù lại 4 request gen.

## Recommendations

1. **Dùng GPT image 2.0 cho TẤT CẢ asset** — Leonardo không còn cần thiết. Tile nhanh hơn nhiều so với DALL-E 3 cũ; icon/sprite/decoration thậm chí tiện hơn Leonardo nhờ transparent BG native.
2. **Có thể huỷ Leonardo subscription** — tiết kiệm $10/tháng. GPT Plus $20 đủ cho cả pipeline.
3. **Hero image vừa tạo** ("Mystical glade with a lone wanderer"): giữ lại, dùng làm visual reference khi prompt tile + decoration để mắt bạn đối chiếu style nhất quán.
4. **Future asset prompts (PR sau)**: mob/icon/decoration/NPC/UI — sẽ viết tận dụng native transparent BG, không cần adapter file riêng cho Leonardo.

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
- [`../Documentation/art/ART_STYLE.md`](../Documentation/art/ART_STYLE.md) — style bible
