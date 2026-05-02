"""
Trim alpha bbox of player puppet PNGs.

Each part PNG has small transparent margins (4-5 px) from prior slicing. With sprite
pivot at top-center / bottom-center / etc., these margins translate to visible gaps at
joint connections (e.g. shin-top has 4px transparent before shoe content → 4px gap
between leg-bottom and shin-shoe-top).

This script crops each PNG tight to its alpha bbox so:
- top-pivot (0.5, 1.0) → visible content starts at sprite top edge
- bottom-pivot (0.5, 0.0) → visible content ends at sprite bottom edge
- center pivot (0.5, 0.5) → visible content is centered

Usage:
    python3 tools/trim_alpha_bbox.py
"""
from pathlib import Path
from PIL import Image

ROOT = Path(__file__).resolve().parent.parent
ART = ROOT / "Assets" / "_Project" / "Art" / "Characters" / "player"
DIRECTIONS = ["E", "N", "S"]
PARTS = [
    "head", "torso",
    "arm_left", "arm_right",
    "forearm_left", "forearm_right",
    "leg_left", "leg_right",
    "shin_left", "shin_right",
]


def trim_one(path: Path) -> tuple[bool, str]:
    img = Image.open(path).convert("RGBA")
    bbox = img.getbbox()
    if bbox is None:
        return False, f"empty alpha — skipped"
    left, top, right, bot = bbox
    w, h = img.size
    margins = (left, top, w - right, h - bot)
    if all(m == 0 for m in margins):
        return False, f"already tight ({w}x{h})"
    cropped = img.crop(bbox)
    cropped.save(path, optimize=True)
    return True, f"{w}x{h} → {cropped.size[0]}x{cropped.size[1]}  trimmed L{margins[0]} T{margins[1]} R{margins[2]} B{margins[3]}"


def main():
    n_modified = 0
    for direction in DIRECTIONS:
        for part in PARTS:
            path = ART / direction / f"{part}.png"
            if not path.exists():
                print(f"  MISSING  {path.relative_to(ROOT)}")
                continue
            modified, msg = trim_one(path)
            tag = "TRIM" if modified else "skip"
            print(f"  [{tag}]  {direction}/{part:18s}  {msg}")
            if modified:
                n_modified += 1
    print(f"\nTotal: {n_modified} files trimmed")


if __name__ == "__main__":
    main()
