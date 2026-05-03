"""Compose extracted player PNGs into a full puppet preview, mirroring Unity's
BuildPuppetHierarchy + auto-PPU import logic. Generates a single PNG showing
all 4 directions side-by-side so we can iterate on extraction without Unity.

Run from repo root:
    python3 .agents/scripts/preview_player_rig.py
Output: /tmp/player_rig_preview.png

Rig math (mirrors Assets/_Project/Editor/BootstrapWizard.cs):
  PPU per part = tex.height * 64 / placeholder_height_px   (auto-normalize world h)
  shoulder pos = (±shoulderX, +shoulderY)  — within torso
  hip pos      = (±hipX,      -hipY)       — within torso
  arm pivot    = (0.5, 1.0) top-center      → hangs down from shoulder
  forearm pos  = (0, -armLen + elbowOverlap*armLen) within arm — child of arm
  leg pivot    = (0.5, 1.0) top-center      → hangs down from hip
  shin pos     = (0, -legLen + kneeOverlap*legLen) within leg — child of leg
  head pivot   = (0.5, 0.0) bottom-center   → grows up from torso top
"""
from __future__ import annotations
from pathlib import Path
from PIL import Image
import numpy as np

ART_ROOT = Path("Assets/_Project/Art/Characters/player")

# ---- Placeholder spec (mirrors PuppetPlaceholderSpec.cs RectFor) ----
PLACEHOLDER_PX = {
    "head":     (40, 40),
    "torso":    (52, 80),
    "arm":      (16, 36),
    "forearm":  (14, 28),
    "leg":      (18, 48),
    "shin":     (16, 32),
}
PPU_PLACEHOLDER = 64.0

# ---- CharacterRigSpec defaults (mirrors fallback in BootstrapWizard) ----
DEFAULT_RIG = {
    "shoulderY": 0.55,
    "shoulderX": 0.30,
    "hipY":      0.55,
    "hipX":      0.13,
    "elbowOverlap": 0.06,
    "kneeOverlap":  0.10,
    "farLimbScale": 0.92,
}

PIVOT = {
    "head":     (0.5, 0.0),  # bottom-center
    "torso":    (0.5, 0.5),  # center
    "arm":      (0.5, 1.0),  # top-center
    "forearm":  (0.5, 1.0),
    "leg":      (0.5, 1.0),
    "shin":     (0.5, 1.0),
}


def world_h_for(role: str) -> float:
    """World height in units = placeholder_h_px / 64."""
    return PLACEHOLDER_PX[role][1] / PPU_PLACEHOLDER


def role_for_filename(fname: str) -> str:
    """Map filename to role key for placeholder lookup."""
    if fname.startswith("head"):     return "head"
    if fname.startswith("torso"):    return "torso"
    if fname.startswith("forearm"):  return "forearm"
    if fname.startswith("arm"):      return "arm"
    if fname.startswith("shin"):     return "shin"
    if fname.startswith("leg"):      return "leg"
    raise ValueError(f"unknown role for {fname}")


def load_part(direction: str, fname: str) -> tuple[Image.Image, float, float]:
    """Load PNG, compute its world (w, h) using auto-PPU normalize math."""
    role = role_for_filename(fname)
    placeholder_h = PLACEHOLDER_PX[role][1]
    img = Image.open(ART_ROOT / direction / fname).convert("RGBA")
    w_px, h_px = img.size
    # ppu = h_px * 64 / placeholder_h_px → world_h = h_px/ppu = placeholder_h/64
    world_h = placeholder_h / PPU_PLACEHOLDER  # always == placeholder world h
    world_w = w_px / h_px * world_h
    return img, world_w, world_h


def world_to_canvas(x: float, y: float, canvas_w: int, canvas_h: int,
                    units_per_px: float, origin_x: int, origin_y: int) -> tuple[int, int]:
    """Convert world (x, y) to canvas pixel coords. y=0 is character middle.
       Y up in world → up on canvas (smaller pixel y)."""
    cx = int(round(origin_x + x / units_per_px))
    cy = int(round(origin_y - y / units_per_px))
    return cx, cy


def paste_part_with_pivot(canvas: Image.Image, part_img: Image.Image,
                           world_pos: tuple[float, float], pivot_norm: tuple[float, float],
                           world_w: float, world_h: float,
                           units_per_px: float, origin_x: int, origin_y: int,
                           scale: float = 1.0, flip_x: bool = False,
                           order: int = 0, parts_buffer: list = None) -> None:
    """Paste a sprite onto canvas with pivot-aware positioning.
    pivot_norm: (px, py) in [0..1] of sprite. py=0 is BOTTOM, py=1 is TOP (Unity convention).
    """
    if flip_x:
        part_img = part_img.transpose(Image.FLIP_LEFT_RIGHT)
    sw_px, sh_px = part_img.size
    target_w_px = max(1, int(round(world_w * scale / units_per_px)))
    target_h_px = max(1, int(round(world_h * scale / units_per_px)))
    if (target_w_px, target_h_px) != (sw_px, sh_px):
        part_img = part_img.resize((target_w_px, target_h_px), Image.LANCZOS)
        sw_px, sh_px = target_w_px, target_h_px

    # Pivot: in Unity, py=0 is bottom, py=1 is top. PIL y=0 is top.
    # So pivot pixel position in PIL: (px*sw, (1-py)*sh)
    px_norm, py_norm = pivot_norm
    pivot_pil_x = int(round(px_norm * sw_px))
    pivot_pil_y = int(round((1.0 - py_norm) * sh_px))

    # World position → canvas coords
    cx, cy = world_to_canvas(world_pos[0], world_pos[1],
                             canvas.size[0], canvas.size[1],
                             units_per_px, origin_x, origin_y)
    paste_x = cx - pivot_pil_x
    paste_y = cy - pivot_pil_y

    if parts_buffer is None:
        canvas.paste(part_img, (paste_x, paste_y), part_img)
    else:
        parts_buffer.append((order, paste_x, paste_y, part_img))


def compose_direction(direction: str, hide_arms: bool, flip_x: bool = False,
                      rig: dict = None) -> Image.Image:
    """Compose a single character direction into a 256x320 canvas."""
    if rig is None:
        rig = DEFAULT_RIG
    canvas_w, canvas_h = 256, 320
    canvas = Image.new("RGBA", (canvas_w, canvas_h), (40, 50, 40, 255))  # forest tint
    parts: list[tuple[int, int, int, Image.Image]] = []  # (order, x, y, img)

    units_per_px = 5.0 / canvas_h  # ~5u tall character fills canvas
    origin_x = canvas_w // 2
    origin_y = int(canvas_h * 0.55)  # character center slightly below mid

    torso_h = world_h_for("torso")
    arm_h = world_h_for("arm")
    forearm_h = world_h_for("forearm")
    leg_h = world_h_for("leg")
    shin_h = world_h_for("shin")

    shoulderY = rig["shoulderY"] - torso_h * 0.0
    shoulderX = rig["shoulderX"]
    hipY = -rig["hipY"]
    hipX = rig["hipX"]
    elbow_overlap = rig["elbowOverlap"]
    knee_overlap = rig["kneeOverlap"]
    far_scale = rig["farLimbScale"]

    # ---- Far-side limbs render BEHIND torso (lower order) ----
    # In E side, "left" = far-side (assuming character faces right). For W (flip_x),
    # the visual far-side flips. Here we follow asset naming — _left.png is far-side.
    # Hide arms in S/N (front/back baked into torso silhouette).
    far_arm_label = "arm_left.png" if direction == "E" else None
    far_forearm_label = "forearm_left.png" if direction == "E" else None
    near_arm_label = "arm_right.png" if direction == "E" else None
    near_forearm_label = "forearm_right.png" if direction == "E" else None
    far_leg_label = "leg_left.png"
    far_shin_label = "shin_left.png"
    near_leg_label = "leg_right.png"
    near_shin_label = "shin_right.png"

    # ---- FAR LEG + SHIN (behind torso) ----
    leg_img, leg_w, _ = load_part(direction, far_leg_label)
    paste_part_with_pivot(canvas, leg_img, (-hipX, hipY),
                          PIVOT["leg"], leg_w, leg_h,
                          units_per_px, origin_x, origin_y,
                          scale=far_scale, flip_x=flip_x,
                          order=1, parts_buffer=parts)
    # Shin is child of leg: world pos = hip + (0, -leg_h + knee_overlap*leg_h)
    shin_world_y = hipY - leg_h * (1.0 - knee_overlap) * far_scale
    shin_img, shin_w, _ = load_part(direction, far_shin_label)
    paste_part_with_pivot(canvas, shin_img, (-hipX, shin_world_y),
                          PIVOT["shin"], shin_w, shin_h,
                          units_per_px, origin_x, origin_y,
                          scale=far_scale, flip_x=flip_x,
                          order=2, parts_buffer=parts)

    # ---- FAR ARM (E only) ----
    if direction == "E" and not hide_arms and far_arm_label:
        arm_img, arm_w, _ = load_part(direction, far_arm_label)
        paste_part_with_pivot(canvas, arm_img, (-shoulderX, rig["shoulderY"]),
                              PIVOT["arm"], arm_w, arm_h,
                              units_per_px, origin_x, origin_y,
                              scale=far_scale, flip_x=flip_x,
                              order=3, parts_buffer=parts)
        forearm_world_y = rig["shoulderY"] - arm_h * (1.0 - elbow_overlap) * far_scale
        fa_img, fa_w, _ = load_part(direction, far_forearm_label)
        paste_part_with_pivot(canvas, fa_img, (-shoulderX, forearm_world_y),
                              PIVOT["forearm"], fa_w, forearm_h,
                              units_per_px, origin_x, origin_y,
                              scale=far_scale, flip_x=flip_x,
                              order=4, parts_buffer=parts)

    # ---- TORSO (sortingOrder middle) ----
    torso_img, torso_w, _ = load_part(direction, "torso.png")
    paste_part_with_pivot(canvas, torso_img, (0, 0),
                          PIVOT["torso"], torso_w, torso_h,
                          units_per_px, origin_x, origin_y,
                          flip_x=flip_x, order=10, parts_buffer=parts)

    # ---- HEAD (above torso) ----
    head_img, head_w, head_h = load_part(direction, "head.png")
    head_h = world_h_for("head")
    paste_part_with_pivot(canvas, head_img, (0, torso_h * 0.5),
                          PIVOT["head"], head_w, head_h,
                          units_per_px, origin_x, origin_y,
                          flip_x=flip_x, order=11, parts_buffer=parts)

    # ---- NEAR LEG + SHIN (in front of far leg) ----
    leg_img, leg_w, _ = load_part(direction, near_leg_label)
    paste_part_with_pivot(canvas, leg_img, (hipX, hipY),
                          PIVOT["leg"], leg_w, leg_h,
                          units_per_px, origin_x, origin_y,
                          flip_x=flip_x, order=12, parts_buffer=parts)
    shin_world_y = hipY - leg_h * (1.0 - knee_overlap)
    shin_img, shin_w, _ = load_part(direction, near_shin_label)
    paste_part_with_pivot(canvas, shin_img, (hipX, shin_world_y),
                          PIVOT["shin"], shin_w, shin_h,
                          units_per_px, origin_x, origin_y,
                          flip_x=flip_x, order=13, parts_buffer=parts)

    # ---- NEAR ARM (E only) ----
    if direction == "E" and not hide_arms and near_arm_label:
        arm_img, arm_w, _ = load_part(direction, near_arm_label)
        paste_part_with_pivot(canvas, arm_img, (shoulderX, rig["shoulderY"]),
                              PIVOT["arm"], arm_w, arm_h,
                              units_per_px, origin_x, origin_y,
                              flip_x=flip_x,
                              order=14, parts_buffer=parts)
        forearm_world_y = rig["shoulderY"] - arm_h * (1.0 - elbow_overlap)
        fa_img, fa_w, _ = load_part(direction, near_forearm_label)
        paste_part_with_pivot(canvas, fa_img, (shoulderX, forearm_world_y),
                              PIVOT["forearm"], fa_w, forearm_h,
                              units_per_px, origin_x, origin_y,
                              flip_x=flip_x,
                              order=15, parts_buffer=parts)

    # Sort by order and paste in sequence
    parts.sort(key=lambda p: p[0])
    for _, x, y, img in parts:
        canvas.alpha_composite(img, (x, y))

    return canvas


def main() -> None:
    directions = [
        ("E", False, False),
        ("S", True, False),
        ("N", True, False),
        ("E_flip_W", False, True),  # W = E flipped
    ]
    panels = []
    for label, hide_arms, flip_x in directions:
        d = "E" if label.startswith("E") else label
        c = compose_direction(d, hide_arms=hide_arms, flip_x=flip_x)
        panels.append((label, c))

    sheet_w = sum(p[1].size[0] for p in panels) + 8 * (len(panels) + 1)
    sheet_h = panels[0][1].size[1] + 32
    sheet = Image.new("RGB", (sheet_w, sheet_h), (60, 70, 60))
    x = 8
    for label, c in panels:
        sheet.paste(c, (x, 24), c)
        x += c.size[0] + 8

    out = Path("/tmp/player_rig_preview.png")
    sheet.save(out)
    print(f"saved {out} ({sheet.size})")


if __name__ == "__main__":
    main()
