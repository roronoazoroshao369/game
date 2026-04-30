"""Seam fix + 3-step Lanczos downscale tile to PPU=64 size, deterministic.

Strategy: instead of inpaint (no GPU model here), use torus-shift + smoothstep
feather blend on edges via numpy: blend left/right edges with horizontally
shifted copy + top/bottom edges with vertically shifted copy. This guarantees
seamless 4-edge wrap with minimal visual artifact since GPT image 2.0 ground
texture (grass / stone / sand) is high-frequency (no large objects spanning
edges). Then 3-step Lanczos downscale src -> 256 -> 128 -> 64 (preserve detail).

Usage:
    python3 tools/seam_fix.py <src.png> <dst.png> [feather_band_px=96] [final_size=64]

If final_size <= 0 the script keeps the seamless full-size image (skip downscale).
"""
import sys

import numpy as np
from PIL import Image


def feather_1d(n: int, band: int) -> np.ndarray:
    m = np.ones(n, dtype=np.float32)
    if band > 0:
        x = np.arange(band, dtype=np.float32)
        t = x / max(band - 1, 1)
        m[:band] = t * t * (3 - 2 * t)  # smoothstep
        m[-band:] = m[:band][::-1]
    return m


def main():
    src_path = sys.argv[1]
    dst_path = sys.argv[2]
    band = int(sys.argv[3]) if len(sys.argv) > 3 else 96
    final_size = int(sys.argv[4]) if len(sys.argv) > 4 else 64

    img = Image.open(src_path).convert("RGBA")
    arr = np.array(img, dtype=np.float32)
    h, w, _ = arr.shape
    assert h == w, f"expected square tile, got {w}x{h}"

    # Horizontal seam fix: blend with W/2 shifted copy.
    shifted_h = np.roll(arr, w // 2, axis=1)
    mask_h = feather_1d(w, band)[None, :, None]
    arr_h = arr * mask_h + shifted_h * (1 - mask_h)

    # Vertical seam fix on the horizontally-fixed result.
    shifted_v = np.roll(arr_h, h // 2, axis=0)
    mask_v = feather_1d(h, band)[:, None, None]
    arr_hv = arr_h * mask_v + shifted_v * (1 - mask_v)

    result = np.clip(arr_hv, 0, 255).astype(np.uint8)
    out = Image.fromarray(result, mode="RGBA")

    # 3-step Lanczos downscale src -> 256 -> 128 -> final_size to preserve detail.
    if final_size > 0:
        for target in (256, 128, final_size):
            if out.width > target:
                out = out.resize((target, target), Image.LANCZOS)

    out.save(dst_path)
    print(f"Wrote {dst_path} ({out.size})")


if __name__ == "__main__":
    main()
