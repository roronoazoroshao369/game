#!/usr/bin/env python3
"""Validator for atomic-symbol player art (PR fix rời rạc).

Usage from repo root:
    python3 .agents/scripts/validate_player_art.py

Checks each PNG in Assets/_Project/Art/Characters/player/{E,N,S}/:
1. Filename matches expected pattern.
2. PNG is RGBA mode (transparent BG support).
3. Alpha bbox is tight (≤5px transparent padding all sides).
4. Width/height in expected range per part (catches "torso has sleeves baked"
   when width is too wide for trunk-only).
5. Suggests auto-crop or re-gen.

Exits non-zero if any FAIL — useful in CI later.

Reference: Documentation/AI_PROMPTS.md §3 (player prompts + composition rules).
"""

from __future__ import annotations

import sys
from pathlib import Path

try:
    from PIL import Image
except ImportError:
    print("ERROR: Pillow not installed. Run: pip install Pillow")
    sys.exit(1)

ART_ROOT = Path("Assets/_Project/Art/Characters/player")

# Expected dimensions per part. (min_w, max_w, min_h, max_h).
# Tolerance ±30 from target dims documented in AI_PROMPTS.md §3.
# Tighter widths than the current "kimono-baked" art forces narrow trunk gen.
EXPECTED_DIMS = {
    "head":           (160, 250, 200, 270),
    "torso":          (100, 150, 240, 340),  # NEW: max width 150 forces trunk-only,
                                              # flags torsos with sleeves baked.
    "arm_left":       (60,  120, 170, 230),
    "arm_right":      (60,  120, 170, 230),
    "forearm_left":   (50,  110, 180, 260),  # NEW: must include hand → taller +
                                              # slightly wider at bottom (hand width).
    "forearm_right":  (50,  110, 180, 260),
    "leg_left":       (70,  125, 190, 250),
    "leg_right":      (70,  125, 190, 250),
    "shin_left":      (70,  130, 180, 240),
    "shin_right":     (70,  130, 180, 240),
}

# N/S can skip arm/forearm (auto-hidden by hideArmsInFrontBackView).
OPTIONAL_PARTS_BY_DIR = {
    "N": {"arm_left", "arm_right", "forearm_left", "forearm_right"},
    "S": {"arm_left", "arm_right", "forearm_left", "forearm_right"},
    "E": set(),
}

REQUIRED_PARTS = set(EXPECTED_DIMS.keys())
DIRS = ("E", "N", "S")
MAX_BBOX_PADDING = 5  # px


class Issue:
    def __init__(self, path: Path, level: str, message: str, fix: str = ""):
        self.path = path
        self.level = level  # "FAIL" | "WARN"
        self.message = message
        self.fix = fix


def check_file(direction: str, part: str, path: Path) -> list[Issue]:
    issues: list[Issue] = []
    if not path.exists():
        if part in OPTIONAL_PARTS_BY_DIR.get(direction, set()):
            return []  # OK to skip
        issues.append(Issue(path, "FAIL", "missing required file",
                            f"gen part `{part}` for direction {direction}"))
        return issues

    try:
        im = Image.open(path)
    except Exception as exc:
        issues.append(Issue(path, "FAIL", f"cannot open: {exc}", "re-export PNG"))
        return issues

    # 1. Mode RGBA.
    if im.mode != "RGBA":
        issues.append(Issue(path, "FAIL", f"mode={im.mode}, expected RGBA",
                            "re-export PNG with transparency (Photoshop: Save As "
                            "PNG-32 / Krita: Export PNG with alpha)"))
        return issues  # subsequent checks need RGBA

    # 2. Dimensions.
    w, h = im.size
    min_w, max_w, min_h, max_h = EXPECTED_DIMS[part]
    if w < min_w or w > max_w:
        msg = f"width {w} outside expected [{min_w}, {max_w}]"
        if w > max_w and part == "torso":
            msg += " — torso likely has sleeves baked (atomic violation)"
            fix = "regen torso with prompt emphasis 'TRUNK ONLY, no sleeves'"
        elif w > max_w:
            msg += f" — sprite too wide for {part}"
            fix = f"crop or regen narrower"
        else:
            fix = f"regen with min width ≥ {min_w}"
        issues.append(Issue(path, "FAIL", msg, fix))
    if h < min_h or h > max_h:
        msg = f"height {h} outside expected [{min_h}, {max_h}]"
        if h < min_h and part.startswith("forearm"):
            msg += " — forearm too short, likely missing hand"
            fix = "regen forearm with hand visible at bottom (5 fingers)"
        elif h < min_h and part.startswith("shin"):
            msg += " — shin too short, likely missing foot/boot"
            fix = "regen shin with boot at bottom"
        else:
            fix = f"regen with height in [{min_h}, {max_h}]"
        issues.append(Issue(path, "FAIL", msg, fix))

    # 3. Alpha bbox tight.
    alpha = im.split()[-1]
    bbox = alpha.getbbox()
    if bbox is None:
        issues.append(Issue(path, "FAIL", "fully transparent — empty PNG",
                            "regen, ensure subject visible"))
        return issues
    left, top, right, bottom = bbox
    pad_l, pad_t = left, top
    pad_r, pad_b = w - right, h - bottom
    max_pad = max(pad_l, pad_t, pad_r, pad_b)
    if max_pad > MAX_BBOX_PADDING:
        msg = (f"alpha bbox padding too large: L={pad_l} T={pad_t} R={pad_r} "
               f"B={pad_b} (max allowed {MAX_BBOX_PADDING})")
        fix = (f"auto-crop to bbox ({right - left}×{bottom - top}). "
               f"Run: python3 -c \"from PIL import Image; "
               f"i=Image.open('{path}'); "
               f"i.crop(i.getbbox()).save('{path}')\"")
        issues.append(Issue(path, "WARN", msg, fix))

    # 4. Alpha rate sanity (catches solid-BG export).
    alpha_pixels = list(alpha.getdata())
    if not alpha_pixels:
        return issues
    transparent = sum(1 for a in alpha_pixels if a < 16)
    transparent_rate = transparent / len(alpha_pixels)
    if transparent_rate < 0.05:
        issues.append(Issue(
            path, "FAIL",
            f"only {transparent_rate*100:.1f}% transparent pixels — "
            f"likely has solid background baked in",
            "remove BG (Photoshop: Magic Wand BG → Delete; Krita: Color Select → "
            "Cut; or re-gen with negative='no background, no border'"))

    return issues


def main() -> int:
    if not ART_ROOT.exists():
        print(f"ERROR: {ART_ROOT} not found. Run from repo root.")
        return 2

    all_issues: list[Issue] = []
    by_dir_summary: dict[str, dict[str, int]] = {}

    for direction in DIRS:
        dir_path = ART_ROOT / direction
        if not dir_path.exists():
            print(f"FAIL: directory missing: {dir_path}")
            all_issues.append(Issue(dir_path, "FAIL", "directory missing",
                                    f"mkdir + gen all parts for direction {direction}"))
            continue
        by_dir_summary[direction] = {"OK": 0, "WARN": 0, "FAIL": 0}
        for part in sorted(REQUIRED_PARTS):
            file_path = dir_path / f"{part}.png"
            issues = check_file(direction, part, file_path)
            if not issues:
                if file_path.exists():
                    by_dir_summary[direction]["OK"] += 1
            else:
                worst = "WARN"
                for issue in issues:
                    if issue.level == "FAIL":
                        worst = "FAIL"
                by_dir_summary[direction][worst] += 1
                all_issues.extend(issues)

    # Print summary table.
    print("=" * 70)
    print(f"{'Player atomic art validator':^70}")
    print("=" * 70)
    print(f"{'Dir':<5} {'OK':>5} {'WARN':>5} {'FAIL':>5}")
    print("-" * 24)
    for direction in DIRS:
        s = by_dir_summary.get(direction, {"OK": 0, "WARN": 0, "FAIL": 0})
        print(f"{direction:<5} {s['OK']:>5} {s['WARN']:>5} {s['FAIL']:>5}")
    print()

    # Print per-issue detail.
    if all_issues:
        print("Issues:")
        print("-" * 70)
        for issue in all_issues:
            rel = issue.path.relative_to(Path.cwd()) if issue.path.is_absolute() else issue.path
            print(f"  {issue.level}: {rel}")
            print(f"    {issue.message}")
            if issue.fix:
                print(f"    fix: {issue.fix}")
            print()
    else:
        print("ALL OK. Re-bootstrap MainScene in Unity to load new art.")

    fail_count = sum(1 for i in all_issues if i.level == "FAIL")
    return 1 if fail_count else 0


if __name__ == "__main__":
    sys.exit(main())
