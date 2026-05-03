"""Extract 22 rig-ready player PNGs from source board v1 via PIL crop + parchment bg removal.

Source: Documentation/assets/style_refs/player_source_board_v1.png (1024x1536)
Output: Assets/_Project/Art/Characters/player/{E,N,S}/*.png (E=10, N=6, S=6)

Run from repo root:
    python3 .agents/scripts/extract_player_from_source_board.py

Pipeline per part:
  1. Crop bbox from source board.
  2. Color-key remove parchment bg (RGB ~199,187,170) via soft feather (alpha lerp).
  3. Tight-crop to alpha bbox + 5px padding.
  4. Apply optional occlusion_mute (fade RGB toward bg) to far-side limbs in side view.
  5. Pad to target aspect range (so subsequent resize lands within validator dim spec).
  6. Resize Lanczos to comfortable mid-point of validator dim range.
  7. Save PNG.

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
PADDING = 5  # tight bbox padding


def remove_bg(img_rgb: Image.Image) -> Image.Image:
    """Color-key remove parchment background → transparent RGBA."""
    arr = np.array(img_rgb.convert("RGB")).astype(np.float32)
    diff = np.sqrt(((arr - BG_RGB) ** 2).sum(axis=-1))
    alpha = np.clip((diff - SOFT_INNER) / (SOFT_OUTER - SOFT_INNER), 0.0, 1.0)
    alpha8 = (alpha * 255.0).astype(np.uint8)
    rgba = np.dstack([arr.astype(np.uint8), alpha8])
    return Image.fromarray(rgba, "RGBA")


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


def fit_to_dims(img: Image.Image, max_w: int, max_h: int, min_w: int, min_h: int) -> Image.Image:
    """Resize so dims fit within (min..max) range. Preserves aspect.

    Caller must pre-pad to a feasible aspect range via `pad_to_aspect` first.
    """
    w, h = img.size
    # Compute h range satisfying both width-derived and height-derived constraints.
    aspect = w / h
    h_lo = max(min_h, int(round(min_w / aspect)))
    h_hi = min(max_h, int(round(max_w / aspect)))
    if h_lo > h_hi:
        # No feasible solution — clamp to nearest.
        target_h = max(min_h, min(max_h, h))
    else:
        target_h = (h_lo + h_hi) // 2
    target_w = int(round(target_h * aspect))
    target_w = max(min_w, min(max_w, target_w))
    return img.resize((target_w, target_h), Image.LANCZOS)


def extract(board: Image.Image, bbox: tuple[int, int, int, int],
            target_w: tuple[int, int], target_h: tuple[int, int],
            out_path: Path, occlusion_mute: float = 0.0,
            vertical_anchor: str = "center") -> None:
    """Crop bbox from board, key out bg, tight-crop, resize, save.

    occlusion_mute: 0..1, fade RGB toward parchment color to imply far-side depth.
    vertical_anchor: "top" (arm/forearm/leg/shin), "bottom" (head), "center" (torso).
        Determines on which side transparent padding goes when extending aspect range,
        so pivot stays anchored on actual content edge.
    """
    out_path.parent.mkdir(parents=True, exist_ok=True)
    crop = board.crop(bbox)
    rgba = remove_bg(crop)
    rgba = tight_crop(rgba, padding=PADDING)
    if occlusion_mute > 0.0:
        arr = np.array(rgba).astype(np.float32)
        rgb = arr[..., :3]
        muted = rgb * (1.0 - occlusion_mute) + BG_RGB * occlusion_mute
        arr[..., :3] = muted
        rgba = Image.fromarray(arr.astype(np.uint8), "RGBA")
    min_w, max_w = target_w
    min_h, max_h = target_h
    # Pad to feasible aspect first so resize lands within target dim range.
    # Anchor controls which side gets transparent padding so pivot stays on content.
    min_aspect = min_w / max_h
    max_aspect = max_w / min_h
    rgba = pad_to_aspect(rgba, min_aspect, max_aspect, vertical_anchor=vertical_anchor)
    rgba = fit_to_dims(rgba, max_w, max_h, min_w, min_h)
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

    # Pivot anchors per part type (matches PuppetPlaceholderSpec):
    #   head: (0.5, 0)   bottom-center → vertical_anchor="bottom" (pad above)
    #   torso: (0.5, 0.5) center        → vertical_anchor="center"
    #   arm/forearm/leg/shin: (0.5, 1) top-center → vertical_anchor="top" (pad below)
    HEAD_ANCHOR, TORSO_ANCHOR, LIMB_ANCHOR = "bottom", "center", "top"

    e = ART_ROOT / "E"
    print("[E direction]")
    # Head: callout C1 (high-detail side profile)
    extract(board, (50, 960, 360, 1260), (160, 250), (200, 270), e / "head.png",
            vertical_anchor=HEAD_ANCHOR)
    # Torso: callout C2 (sleeveless trunk, V-neck visible — works for E side too via narrow crop)
    extract(board, (430, 980, 700, 1280), (100, 150), (240, 340), e / "torso.png",
            vertical_anchor=TORSO_ANCHOR)
    # Arm right (near-side, full saturation): callout C3
    extract(board, (830, 970, 960, 1230), (60, 120), (170, 230), e / "arm_right.png",
            vertical_anchor=LIMB_ANCHOR)
    # Arm left (far-side): C3 muted
    extract(board, (830, 970, 960, 1230), (60, 120), (170, 230), e / "arm_left.png",
            occlusion_mute=0.18, vertical_anchor=LIMB_ANCHOR)
    # Forearm right: callout C4
    extract(board, (50, 1280, 310, 1480), (50, 110), (180, 260), e / "forearm_right.png",
            vertical_anchor=LIMB_ANCHOR)
    # Forearm left (far-side muted)
    extract(board, (50, 1280, 310, 1480), (50, 110), (180, 260), e / "forearm_left.png",
            occlusion_mute=0.18, vertical_anchor=LIMB_ANCHOR)
    # Leg right (thigh): callout C5
    extract(board, (440, 1280, 670, 1510), (70, 125), (190, 250), e / "leg_right.png",
            vertical_anchor=LIMB_ANCHOR)
    # Leg left (far-side muted)
    extract(board, (440, 1280, 670, 1510), (70, 125), (190, 250), e / "leg_left.png",
            occlusion_mute=0.18, vertical_anchor=LIMB_ANCHOR)
    # Shin right + boot: callout C6
    extract(board, (770, 1280, 990, 1530), (70, 130), (180, 240), e / "shin_right.png",
            vertical_anchor=LIMB_ANCHOR)
    # Shin left (far-side muted)
    extract(board, (770, 1280, 990, 1530), (70, 130), (180, 240), e / "shin_left.png",
            occlusion_mute=0.18, vertical_anchor=LIMB_ANCHOR)

    # ---- S direction (front view, 6 required) ----
    s = ART_ROOT / "S"
    print("[S direction]")
    extract(board, (490, 180, 720, 390), (160, 250), (200, 270), s / "head.png",
            vertical_anchor=HEAD_ANCHOR)
    extract(board, (530, 410, 690, 690), (100, 150), (240, 340), s / "torso.png",
            vertical_anchor=TORSO_ANCHOR)
    extract(board, (485, 690, 600, 810), (70, 125), (190, 250), s / "leg_left.png",
            vertical_anchor=LIMB_ANCHOR)
    extract(board, (605, 690, 720, 810), (70, 125), (190, 250), s / "leg_right.png",
            vertical_anchor=LIMB_ANCHOR)
    extract(board, (480, 800, 600, 960), (70, 130), (180, 240), s / "shin_left.png",
            vertical_anchor=LIMB_ANCHOR)
    extract(board, (605, 800, 730, 960), (70, 130), (180, 240), s / "shin_right.png",
            vertical_anchor=LIMB_ANCHOR)

    # ---- N direction (back view, 6 required) ----
    n = ART_ROOT / "N"
    print("[N direction]")
    extract(board, (760, 180, 970, 380), (160, 250), (200, 270), n / "head.png",
            vertical_anchor=HEAD_ANCHOR)
    extract(board, (790, 400, 950, 690), (100, 150), (240, 340), n / "torso.png",
            vertical_anchor=TORSO_ANCHOR)
    extract(board, (760, 690, 870, 810), (70, 125), (190, 250), n / "leg_left.png",
            vertical_anchor=LIMB_ANCHOR)
    extract(board, (875, 690, 985, 810), (70, 125), (190, 250), n / "leg_right.png",
            vertical_anchor=LIMB_ANCHOR)
    extract(board, (755, 800, 875, 960), (70, 130), (180, 240), n / "shin_left.png",
            vertical_anchor=LIMB_ANCHOR)
    extract(board, (875, 800, 990, 960), (70, 130), (180, 240), n / "shin_right.png",
            vertical_anchor=LIMB_ANCHOR)

    print("\nDone. 22 PNGs extracted.")


if __name__ == "__main__":
    main()
