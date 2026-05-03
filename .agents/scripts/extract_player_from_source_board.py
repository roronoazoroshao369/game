"""Extract 22 rig-ready player PNGs from source board v1 via PIL crop + parchment bg removal.

Source: Documentation/assets/style_refs/player_source_board_v1.png (1024x1536)
Output: Assets/_Project/Art/Characters/player/{E,N,S}/*.png (E=10, N=6, S=6)

Run from repo root:
    python3 .agents/scripts/extract_player_from_source_board.py

Pipeline per part:
  1. Crop bbox from source board.
  2. Color-key remove parchment bg (RGB ~199,187,170) via soft feather (alpha lerp).
  3. Tight-crop to alpha bbox + 5px padding (keeps content flush to sprite bounds).
  4. Apply optional occlusion_mute (fade RGB toward bg) to far-side limbs in side view.
  5. Resize Lanczos to fit target h (preserving natural aspect — NO aspect padding).
     Why: aspect padding inserts transparent strips above/below content, which causes
     visible gaps in the rig (e.g. between sash and trouser top, between trouser and
     boot). With auto-PPU normalize, world height stays equal to placeholder regardless
     of source pixel dims — so squatter source art just renders as a chunkier (wider)
     character without joint dislocation. World width adapts to natural aspect.
  6. Save PNG.

Validate result: python3 .agents/scripts/validate_player_art.py
"""
from __future__ import annotations
from pathlib import Path
import numpy as np
from PIL import Image, ImageFilter

BOARD = Path("Documentation/assets/style_refs/player_source_board_v1.png")
ART_ROOT = Path("Assets/_Project/Art/Characters/player")

# Parchment bg color sampled at corners ≈ (199, 187, 170)
BG_RGB = np.array([199, 187, 170], dtype=np.float32)

# Soft color-key: pixels within `soft_inner` distance of bg → fully transparent;
# pixels beyond `soft_outer` → fully opaque; in-between → linear feather.
SOFT_INNER = 18.0
SOFT_OUTER = 50.0
PADDING = 2  # tight bbox padding (small to avoid bleed of next callout content)

# Alpha threshold: pixels with alpha below this are dropped before tight-crop, to
# avoid sparse low-opacity ghost pixels from soft color-key edges extending the bbox
# into adjacent callout territory (e.g. C2 torso bbox accidentally including the top
# of C5 thigh callout below it).
ALPHA_HARD_THRESHOLD = 96


def remove_bg(img_rgb: Image.Image) -> Image.Image:
    """Color-key remove parchment background → transparent RGBA."""
    arr = np.array(img_rgb.convert("RGB")).astype(np.float32)
    diff = np.sqrt(((arr - BG_RGB) ** 2).sum(axis=-1))
    alpha = np.clip((diff - SOFT_INNER) / (SOFT_OUTER - SOFT_INNER), 0.0, 1.0)
    alpha8 = (alpha * 255.0).astype(np.uint8)
    rgba = np.dstack([arr.astype(np.uint8), alpha8])
    return Image.fromarray(rgba, "RGBA")


def hard_threshold_alpha(img: Image.Image, threshold: int = ALPHA_HARD_THRESHOLD) -> Image.Image:
    """Drop pixels with alpha below threshold to avoid sparse ghost halos extending
    bbox into adjacent callout content. Pixels above threshold keep their soft alpha."""
    arr = np.array(img)
    mask = arr[..., 3] < threshold
    arr[mask, 3] = 0
    return Image.fromarray(arr, "RGBA")


def tight_crop(img: Image.Image, padding: int = PADDING) -> Image.Image:
    """Crop to alpha bbox + uniform padding."""
    alpha = img.split()[-1]
    bbox = alpha.getbbox()
    if bbox is None:
        return img
    l, t, r, b = bbox
    w, h = img.size
    l = max(0, l - padding)
    t = max(0, t - padding)
    r = min(w, r + padding)
    b = min(h, b + padding)
    return img.crop((l, t, r, b))


def pad_to_aspect(img: Image.Image, min_aspect: float, max_aspect: float,
                  vertical_anchor: str = "center") -> Image.Image:
    """Pad with transparent pixels (vertical or horizontal) so w/h fits target aspect range.

    vertical_anchor:
      - "top"    → pad below content (pivot is at top: arm/forearm/leg/shin).
      - "bottom" → pad above content (pivot is at bottom: head).
      - "center" → symmetric padding (pivot is mid: torso).

    Horizontal padding is always centered (X pivot is always 0.5).
    """
    w, h = img.size
    aspect = w / h
    if min_aspect <= aspect <= max_aspect:
        return img
    if aspect > max_aspect:
        # Too wide → add vertical padding to make taller.
        target_aspect = max_aspect
        new_h = int(round(w / target_aspect))
        new_w = w
        canvas = Image.new("RGBA", (new_w, new_h), (0, 0, 0, 0))
        if vertical_anchor == "top":
            offset_y = 0  # content flush with top, padding below
        elif vertical_anchor == "bottom":
            offset_y = new_h - h  # content flush with bottom, padding above
        else:
            offset_y = (new_h - h) // 2
        canvas.paste(img, ((new_w - w) // 2, offset_y), img)
        return canvas
    # aspect < min_aspect — too tall → add horizontal padding (always centered).
    target_aspect = min_aspect
    new_w = int(round(h * target_aspect))
    new_h = h
    canvas = Image.new("RGBA", (new_w, new_h), (0, 0, 0, 0))
    canvas.paste(img, ((new_w - w) // 2, (new_h - h) // 2), img)
    return canvas


def fit_to_target_h(img: Image.Image, target_h: int) -> Image.Image:
    """Resize so sprite is exactly `target_h` pixels tall, preserving natural aspect.
    Auto-PPU import then normalizes world height to placeholder. World width adapts
    to the natural aspect of source content. NO transparent padding inserted.
    """
    w, h = img.size
    aspect = w / h
    target_w = max(1, int(round(target_h * aspect)))
    return img.resize((target_w, target_h), Image.LANCZOS)


# Target sprite height per role. Width is derived from natural source aspect.
# Height is what determines auto-PPU world-h normalize, so this is the canonical
# spec. All E/N/S sprites of the same role render at the same world height.
TARGET_H = {
    "head":    220,
    "torso":   280,
    "arm":     200,
    "forearm": 200,
    "leg":     200,
    "shin":    200,
}


def target_h_for(out_path: Path) -> int:
    name = out_path.stem  # "head", "torso", "arm_left", ...
    for key in TARGET_H:
        if name.startswith(key):
            return TARGET_H[key]
    raise ValueError(f"unknown role for {out_path}")


def extract(board: Image.Image, bbox: tuple[int, int, int, int],
            out_path: Path, occlusion_mute: float = 0.0) -> None:
    """Crop bbox from board, key out bg, hard-threshold halo, tight-crop, resize, save.

    occlusion_mute: 0..1, fade RGB toward parchment color to imply far-side depth.
    """
    out_path.parent.mkdir(parents=True, exist_ok=True)
    crop = board.crop(bbox)
    rgba = remove_bg(crop)
    rgba = hard_threshold_alpha(rgba, ALPHA_HARD_THRESHOLD)
    rgba = tight_crop(rgba, padding=PADDING)
    if occlusion_mute > 0.0:
        arr = np.array(rgba).astype(np.float32)
        rgb = arr[..., :3]
        muted = rgb * (1.0 - occlusion_mute) + BG_RGB * occlusion_mute
        arr[..., :3] = muted
        rgba = Image.fromarray(arr.astype(np.uint8), "RGBA")
    rgba = fit_to_target_h(rgba, target_h_for(out_path))
    rgba.save(out_path, "PNG")
    print(f"  -> {out_path}  ({rgba.size[0]}x{rgba.size[1]})")


def main() -> None:
    if not BOARD.exists():
        raise SystemExit(f"missing source board: {BOARD}")
    board = Image.open(BOARD).convert("RGB")
    print(f"board: {BOARD} ({board.size[0]}x{board.size[1]})")

    # Crop bboxes (left, top, right, bottom) on board pixel coords.
    # Tuned by visual inspection of player_source_board_v1.png 1024x1536.

    # ---- E direction (10 parts) ----
    # E full-body figure occupies left column ~80-460 x 40-960
    # Anatomical breakdown of E full-body:
    #   head:        x~190-380, y~50-300
    #   torso:       x~150-410, y~340-650
    #   upper arm:   x~270-330, y~430-580 (right/near arm visible in front)
    #   forearm:     x~270-360, y~580-720
    #   thigh:       x~210-370, y~660-820
    #   shin+boot:   x~200-410, y~820-960
    # Callouts (cleaner, prefer):
    #   C1 head:     x~50-360, y~960-1260
    #   C2 torso:    x~410-720, y~960-1300 (sleeveless, V-neck front — best for S)
    #   C3 upper arm:x~830-960, y~970-1240
    #   C4 forearm:  x~50-310, y~1280-1480
    #   C5 thigh:    x~440-660, y~1290-1500
    #   C6 shin+boot:x~770-980, y~1290-1530

    e = ART_ROOT / "E"
    print("[E direction]")
    # Head: callout C1 (high-detail side profile)
    extract(board, (50, 960, 360, 1260), e / "head.png")
    # Torso: E full-body figure side profile (NOT C2 callout — C2 is sleeveless front-view
    # which has wrong silhouette + V-neck transparency for side rendering, and was bleeding
    # into C5 thigh callout below it).
    extract(board, (170, 340, 305, 660), e / "torso.png")
    # Arm right (near-side, full saturation): callout C3
    extract(board, (830, 970, 960, 1230), e / "arm_right.png")
    # Arm left (far-side): C3 muted
    extract(board, (830, 970, 960, 1230), e / "arm_left.png", occlusion_mute=0.18)
    # Forearm right: callout C4 tight-cropped from cuff-down to fist (forearm + hand only;
    # full C4 callout is the WHOLE arm including sleeve which would double the upper arm).
    extract(board, (50, 1370, 310, 1480), e / "forearm_right.png")
    # Forearm left (far-side muted)
    extract(board, (50, 1370, 310, 1480), e / "forearm_left.png", occlusion_mute=0.18)
    # Leg right (thigh): callout C5
    extract(board, (440, 1280, 670, 1510), e / "leg_right.png")
    # Leg left (far-side muted)
    extract(board, (440, 1280, 670, 1510), e / "leg_left.png", occlusion_mute=0.18)
    # Shin right + boot: callout C6
    extract(board, (770, 1280, 990, 1530), e / "shin_right.png")
    # Shin left (far-side muted)
    extract(board, (770, 1280, 990, 1530), e / "shin_left.png", occlusion_mute=0.18)

    # ---- S direction (front view, 6 required) ----
    s = ART_ROOT / "S"
    print("[S direction]")
    extract(board, (490, 180, 720, 390), s / "head.png")
    extract(board, (530, 410, 690, 690), s / "torso.png")
    extract(board, (485, 690, 600, 810), s / "leg_left.png")
    extract(board, (605, 690, 720, 810), s / "leg_right.png")
    extract(board, (480, 800, 600, 960), s / "shin_left.png")
    extract(board, (605, 800, 730, 960), s / "shin_right.png")

    # ---- N direction (back view, 6 required) ----
    n = ART_ROOT / "N"
    print("[N direction]")
    extract(board, (760, 180, 970, 380), n / "head.png")
    extract(board, (790, 400, 950, 690), n / "torso.png")
    extract(board, (760, 690, 870, 810), n / "leg_left.png")
    extract(board, (875, 690, 985, 810), n / "leg_right.png")
    extract(board, (755, 800, 875, 960), n / "shin_left.png")
    extract(board, (875, 800, 990, 960), n / "shin_right.png")

    print("\nDone. 22 PNGs extracted.")


if __name__ == "__main__":
    main()
