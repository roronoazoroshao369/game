#!/usr/bin/env python3
"""Slice a 3-row × 10-col player puppet sheet into 30 isolated transparent PNGs.

Input sheet layout:
  Row 0 (top)    = E (side-view 90° facing right)
  Row 1 (middle) = N (back-view)
  Row 2 (bottom) = S (front-view facing camera)
  Col 0..9       = head, torso, arm_left, arm_right, forearm_left, forearm_right,
                   leg_left, leg_right, shin_left, shin_right

Pipeline:
  1. Treat near-white BG (RGB > 220 all channels) as transparent.
  2. Connected components on FG mask.
  3. Cluster components by Y-centroid into 3 rows (k=3).
  4. Within each row, sort by X-centroid and merge close-together components
     into "part groups" (a group = one body part; multiple components within a
     part are bun+ribbon, sleeve+hand split, etc.).
  5. For each part group: tight crop bbox + 4px AA margin, write transparent PNG.
"""
import sys
from pathlib import Path

import numpy as np
from PIL import Image
from scipy import ndimage


PART_NAMES = [
    "head", "torso",
    "arm_left", "arm_right",
    "forearm_left", "forearm_right",
    "leg_left", "leg_right",
    "shin_left", "shin_right",
]
DIR_NAMES = ["E", "N", "S"]

BG_THRESHOLD = 220       # RGB all > 220 → background
MIN_COMPONENT_PX = 200   # drop tiny stray pixel groups
MARGIN_PX = 4            # AA margin around tight bbox
GROUP_X_GAP_PX = 30      # if X gap between components < this, same part group


def make_alpha(arr: np.ndarray) -> np.ndarray:
    """Return alpha channel where near-white BG → 0, anything else → 255."""
    rgb = arr[..., :3]
    bg = (rgb[..., 0] > BG_THRESHOLD) & (rgb[..., 1] > BG_THRESHOLD) & (rgb[..., 2] > BG_THRESHOLD)
    return (~bg).astype(np.uint8) * 255


def label_components(alpha: np.ndarray):
    fg = alpha > 0
    labeled, n = ndimage.label(fg)
    sizes = ndimage.sum(fg, labeled, range(1, n + 1))
    keep_labels = [i for i in range(1, n + 1) if sizes[i - 1] >= MIN_COMPONENT_PX]
    comps = []
    for lbl in keep_labels:
        ys, xs = np.where(labeled == lbl)
        comps.append({
            "label": lbl,
            "y_min": int(ys.min()),
            "y_max": int(ys.max()),
            "x_min": int(xs.min()),
            "x_max": int(xs.max()),
            "y_c": float(ys.mean()),
            "x_c": float(xs.mean()),
            "size": int(len(ys)),
        })
    return labeled, comps


def cluster_rows(comps, k=3):
    """K-means on Y-centroid to assign each component to row 0/1/2."""
    if not comps:
        return []
    ys = np.array([c["y_c"] for c in comps])
    y_min, y_max = ys.min(), ys.max()
    centers = np.linspace(y_min, y_max, k)
    for _ in range(50):
        assignments = np.argmin(np.abs(ys[:, None] - centers[None, :]), axis=1)
        new_centers = np.array([
            ys[assignments == i].mean() if (assignments == i).any() else centers[i]
            for i in range(k)
        ])
        if np.allclose(new_centers, centers):
            break
        centers = new_centers
    rows = [[] for _ in range(k)]
    for c, a in zip(comps, assignments):
        rows[a].append(c)
    order = np.argsort(centers)
    return [rows[i] for i in order]


def group_parts_in_row(row_comps, target_count=10, gap_px=GROUP_X_GAP_PX):
    """Group components in a row into part groups.

    Strategy: if row already has exactly `target_count` components, each component
    IS a part (no grouping). Otherwise fall back to X-proximity merging: walk
    left→right; if current component's x_min - prev group's x_max <= gap_px,
    merge into prev group; else start new group.
    """
    if not row_comps:
        return []
    sorted_comps = sorted(row_comps, key=lambda c: c["x_c"])
    if len(sorted_comps) == target_count:
        return [[c] for c in sorted_comps]
    groups = [[sorted_comps[0]]]
    for c in sorted_comps[1:]:
        prev_x_max = max(g["x_max"] for g in groups[-1])
        if c["x_min"] - prev_x_max <= gap_px:
            groups[-1].append(c)
        else:
            groups.append([c])
    return groups


def crop_part(arr: np.ndarray, alpha: np.ndarray, group, margin=MARGIN_PX):
    """Tight-crop a part group + margin and return RGBA Image with proper alpha."""
    y_min = min(c["y_min"] for c in group) - margin
    y_max = max(c["y_max"] for c in group) + margin + 1
    x_min = min(c["x_min"] for c in group) - margin
    x_max = max(c["x_max"] for c in group) + margin + 1
    H, W = arr.shape[:2]
    y_min = max(0, y_min)
    y_max = min(H, y_max)
    x_min = max(0, x_min)
    x_max = min(W, x_max)
    rgba = arr[y_min:y_max, x_min:x_max].copy()
    rgba[..., 3] = alpha[y_min:y_max, x_min:x_max]
    return Image.fromarray(rgba, "RGBA"), (x_min, y_min, x_max, y_max)


def main():
    if len(sys.argv) != 3:
        print("usage: slice_player_sheet.py <input.png> <output_root>")
        sys.exit(1)
    in_path = Path(sys.argv[1])
    out_root = Path(sys.argv[2])

    img = Image.open(in_path).convert("RGBA")
    arr = np.array(img)
    H, W = arr.shape[:2]
    print(f"Loaded sheet {in_path} ({W}x{H})")

    alpha = make_alpha(arr)
    fg_pct = (alpha > 0).mean() * 100
    print(f"Foreground coverage: {fg_pct:.1f}%")

    labeled, comps = label_components(alpha)
    print(f"Connected components (size >= {MIN_COMPONENT_PX}): {len(comps)}")

    rows = cluster_rows(comps, k=3)
    print(f"Row sizes: {[len(r) for r in rows]}")

    summary = []
    for row_idx, row_comps in enumerate(rows):
        groups = group_parts_in_row(row_comps, target_count=len(PART_NAMES))
        print(f"\nRow {row_idx} ({DIR_NAMES[row_idx]}): {len(groups)} part groups")
        if len(groups) != len(PART_NAMES):
            print(f"  WARNING: expected {len(PART_NAMES)} parts, got {len(groups)}")
            for i, g in enumerate(groups):
                xc = sum(c["x_c"] for c in g) / len(g)
                print(f"    group {i}: {len(g)} comps, x_c={xc:.0f}, "
                      f"x_min={min(c['x_min'] for c in g)}, x_max={max(c['x_max'] for c in g)}")
            sys.exit(2)

        out_dir = out_root / DIR_NAMES[row_idx]
        out_dir.mkdir(parents=True, exist_ok=True)

        for col_idx, group in enumerate(groups):
            part_name = PART_NAMES[col_idx]
            crop_img, bbox = crop_part(arr, alpha, group)
            out_path = out_dir / f"{part_name}.png"
            crop_img.save(out_path)
            x0, y0, x1, y1 = bbox
            summary.append((DIR_NAMES[row_idx], part_name,
                            crop_img.size[0], crop_img.size[1], x0, y0, x1, y1))
            print(f"  {DIR_NAMES[row_idx]}/{part_name}.png  size={crop_img.size}  bbox=({x0},{y0})-({x1},{y1})")

    print(f"\nDone. Saved {len(summary)} PNG files to {out_root}.")


if __name__ == "__main__":
    main()
