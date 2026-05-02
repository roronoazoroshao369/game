#!/usr/bin/env python3
"""
Mask "hand" portion off arm PNGs + "foot" portion off leg PNGs (Player puppet rig).

Why: AI gen-art (PR #122 v3 sheet) included full arm-with-hand silhouette in BOTH
arm.png and forearm.png (and same for leg.png + shin.png). When BootstrapWizard.BuildPuppetHierarchy
chains them (forearm child of arm at elbow joint), 2 hand silhouettes render at different
y levels = visible doubled-hand artifact in Unity.

Fix: arm.png keeps sleeve+cuff only (mask hand at bottom). forearm.png keeps sleeve+hand
(it's the END of the arm chain). Same for leg/shin: leg keeps trouser only, shin keeps
trouser+shoe.

Usage: python3 tools/mask_arm_leg_terminals.py [--dry-run]

Operates on Assets/_Project/Art/Characters/player/{E,N,S}/{arm_left,arm_right,leg_left,leg_right}.png
Outputs alpha-feathered crop in-place. Idempotent: re-running on already-masked PNG produces
same output (since detection is based on alpha cumulative profile).
"""
import argparse
import sys
from pathlib import Path

import numpy as np
from PIL import Image

ROOT = Path(__file__).resolve().parents[1] / "Assets" / "_Project" / "Art" / "Characters" / "player"

# What fraction of bottom to crop. Tuned per part type:
# - arm: ~25% bottom = cuff transition + hand (forearm has its own hand)
# - leg: ~18% bottom = shoe (shin has its own shoe)
# Use cumulative-alpha "elbow" detection within this range to find the actual cuff boundary,
# fall back to fixed fraction if profile is uniform.
PARTS_TO_MASK = {
    # part_basename: (search_range_pct, default_crop_pct, feather_px)
    "arm_left":   (0.30, 0.22, 6),
    "arm_right":  (0.30, 0.22, 6),
    "leg_left":   (0.25, 0.18, 6),
    "leg_right":  (0.25, 0.18, 6),
}

DIRECTIONS = ["E", "N", "S"]


def find_terminal_boundary(alpha: np.ndarray, search_pct: float, default_pct: float) -> int:
    """
    Find Y row where the part's "terminal" (hand/foot) starts. Detection: walk from bottom
    upward, find first row where width INCREASES significantly compared to the row above
    (= cuff bulge → hand widening, or trouser → shoe).

    If no clear bulge in `search_pct` band, fall back to `default_pct` from bottom.
    """
    h = alpha.shape[0]
    widths = (alpha > 30).sum(axis=1)

    search_start = int(h * (1.0 - search_pct))  # top of search band
    search_end = h - 1  # near bottom

    # Find peak width within search range — that's the cuff/shoe widening point.
    # Boundary = a few rows ABOVE peak (where width first started increasing from sleeve baseline).
    if search_end <= search_start:
        return int(h * (1.0 - default_pct))

    band = widths[search_start:search_end]
    if band.max() == 0:
        return int(h * (1.0 - default_pct))

    # Reverse: find the row where width starts SIGNIFICANTLY decreasing as we go down past peak.
    # In practice: the "narrow neck" between sleeve & cuff (or trouser & shoe) is where we want
    # to cut. Look for local minimum just above peak.
    peak_local_idx = int(np.argmax(band))
    peak_global = search_start + peak_local_idx

    # Search a small window above peak for the narrowest point ("neck" before bulge).
    upward_band_size = max(20, int(h * 0.08))
    look_start = max(search_start, peak_global - upward_band_size)
    if peak_global <= look_start:
        return int(h * (1.0 - default_pct))

    upward = widths[look_start:peak_global]
    if len(upward) == 0:
        return int(h * (1.0 - default_pct))

    neck_local_idx = int(np.argmin(upward))
    neck_global = look_start + neck_local_idx

    # Sanity: neck must be within the search band, not above sleeve waistline.
    if neck_global < int(h * (1.0 - search_pct - 0.05)):
        return int(h * (1.0 - default_pct))

    return neck_global


def apply_mask(img: Image.Image, cut_y: int, feather_px: int) -> Image.Image:
    """
    Alpha-zero everything below cut_y, with a smooth `feather_px` ramp from full alpha at
    cut_y - feather to zero at cut_y. Preserves rgba structure.
    """
    arr = np.array(img).copy()
    if arr.shape[2] != 4:
        # Add alpha if missing.
        rgb = arr[:, :, :3]
        a = np.full(arr.shape[:2], 255, dtype=np.uint8)
        arr = np.dstack([rgb, a])

    h = arr.shape[0]
    cut_y = max(1, min(h - 1, cut_y))

    feather = max(1, feather_px)
    ramp_top = max(0, cut_y - feather)

    # Below cut_y: alpha = 0.
    arr[cut_y:, :, 3] = 0
    # Feather: linearly decrease alpha from row ramp_top → cut_y.
    if cut_y > ramp_top:
        for y in range(ramp_top, cut_y):
            t = 1.0 - (y - ramp_top) / max(1, cut_y - ramp_top)
            arr[y, :, 3] = (arr[y, :, 3].astype(np.float32) * t).astype(np.uint8)

    return Image.fromarray(arr, "RGBA")


def trim_alpha_bbox(img: Image.Image, margin: int = 4) -> Image.Image:
    """Re-tighten bbox after masking — mask might have left transparent rows at bottom."""
    arr = np.array(img)
    if arr.shape[2] != 4:
        return img
    alpha = arr[:, :, 3]
    rows = np.where(alpha.sum(axis=1) > 50)[0]
    cols = np.where(alpha.sum(axis=0) > 50)[0]
    if len(rows) == 0 or len(cols) == 0:
        return img
    y0, y1 = int(rows.min()), int(rows.max())
    x0, x1 = int(cols.min()), int(cols.max())
    y0 = max(0, y0 - margin)
    y1 = min(arr.shape[0] - 1, y1 + margin)
    x0 = max(0, x0 - margin)
    x1 = min(arr.shape[1] - 1, x1 + margin)
    return img.crop((x0, y0, x1 + 1, y1 + 1))


def process_one(path: Path, search_pct: float, default_pct: float, feather_px: int, dry_run: bool):
    img = Image.open(path).convert("RGBA")
    arr = np.array(img)
    alpha = arr[:, :, 3]
    h = alpha.shape[0]
    boundary = find_terminal_boundary(alpha, search_pct, default_pct)
    pct_from_top = boundary / h * 100.0
    print(f"  {path.relative_to(ROOT.parent.parent.parent)}: h={h}, cut at y={boundary} ({pct_from_top:.1f}% from top, "
          f"removing bottom {h - boundary}px / {(h - boundary) / h * 100:.1f}%)")

    if dry_run:
        return

    masked = apply_mask(img, boundary, feather_px)
    trimmed = trim_alpha_bbox(masked, margin=4)
    trimmed.save(path, "PNG", optimize=True)


def main():
    ap = argparse.ArgumentParser(description=__doc__)
    ap.add_argument("--dry-run", action="store_true", help="Print actions without writing files")
    args = ap.parse_args()

    if not ROOT.exists():
        print(f"ERROR: {ROOT} not found — script must be run from repo root", file=sys.stderr)
        return 1

    total = 0
    for direction in DIRECTIONS:
        dir_path = ROOT / direction
        if not dir_path.exists():
            print(f"WARN: {dir_path} missing, skipping", file=sys.stderr)
            continue
        print(f"--- {direction} ---")
        for part_name, (search_pct, default_pct, feather_px) in PARTS_TO_MASK.items():
            png = dir_path / f"{part_name}.png"
            if not png.exists():
                print(f"  SKIP missing {png.name}", file=sys.stderr)
                continue
            process_one(png, search_pct, default_pct, feather_px, args.dry_run)
            total += 1

    suffix = " (dry-run)" if args.dry_run else ""
    print(f"\nProcessed {total} PNG(s){suffix}.")
    return 0


if __name__ == "__main__":
    sys.exit(main())
