# Asset preview folder

Lưu PNG raw + intermediate processing để verify chất lượng trước khi đưa vào
`Assets/_Project/Art/Tiles/{biome}/`. KHÔNG bundle vào APK.

## Convention naming

- `tile_{biome}_{type}_{NN}.png` — RAW từ GPT image 2.0 (1024×1024 hoặc 1254×1254)
- `tile_{biome}_{type}_{NN}_seamless_{size}.png` — đã pass seam-fix pipeline, chưa downscale

PNG ở `Assets/_Project/Art/Tiles/{biome}/` là final 64×64 (PPU=64) Unity dùng.

## Seam-fix pipeline (tự động trên VM)

Script: `tools/seam_fix.py` (numpy + Pillow).

Algorithm: dùng torus-shift + smoothstep feather để blend 4 cạnh. Không cần
inpaint AI (GPT image 2.0 đã cho high-frequency texture phù hợp với phương pháp này).

```
python3 tools/seam_fix.py <src.png> <dst.png> [feather_band_px=96]
```

Output sau cùng đi qua 3-step Lanczos downscale 1024 → 256 → 128 → 64 (giữ chi tiết).

## Ví dụ

| File | Status |
|---|---|
| `tile_forest_grass_01.png` | RAW từ GPT image 2.0 (https://chatgpt.com/s/m_69f35c37b41c81918814e0aab1927e9b) |
| `tile_forest_grass_01_seamless_1024.png` | Đã pass seam-fix (preview chất lượng) |
| `Assets/_Project/Art/Tiles/forest/tile_forest_grass_01.png` | 64×64 final, Unity-ready |
