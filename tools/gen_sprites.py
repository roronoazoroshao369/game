"""Generate pixel-art PNG sprites for the Wilderness Cultivation demo.

Output goes to Assets/_Project/Sprites/<id>.png. The dimensions and ids must
stay in sync with BootstrapWizard.CreateSprites (the editor wizard imports
these PNGs and assigns the resulting Sprites to prefabs / item SOs).

Aesthetic: flat retro 16-bit-ish pixel art. 1-bit outline, 2 fill shades,
optional 1 highlight pixel. Transparent background so sprites composite
cleanly on world tiles. Aliased ("nearest") — never smooth gradients.
"""
from __future__ import annotations

import os
import sys
from PIL import Image, ImageDraw

OUT_DIR = os.path.join(os.path.dirname(os.path.abspath(__file__)), "..",
                       "Assets", "_Project", "Sprites")

# RGBA tuples
T = (0, 0, 0, 0)


def hexc(c, a=255):
    return ((c >> 16) & 0xFF, (c >> 8) & 0xFF, c & 0xFF, a)


def shade(rgb, factor):
    r, g, b, a = rgb
    return (max(0, min(255, int(r * factor))),
            max(0, min(255, int(g * factor))),
            max(0, min(255, int(b * factor))),
            a)


def new_canvas(w, h):
    return Image.new("RGBA", (w, h), T)


def save(img, name):
    os.makedirs(OUT_DIR, exist_ok=True)
    path = os.path.join(OUT_DIR, name + ".png")
    img.save(path, "PNG")
    print(f"  wrote {path} ({img.width}x{img.height})")


def fill_rect(d, x0, y0, x1, y1, c):
    d.rectangle([x0, y0, x1, y1], fill=c)


def stroke_rect(d, x0, y0, x1, y1, c):
    d.rectangle([x0, y0, x1, y1], outline=c)


# ---------- world entities ----------

def draw_player(w, h):
    """32x32 cultivator: blue robe, hair tie, sword on hip."""
    img = new_canvas(w, h)
    d = ImageDraw.Draw(img)
    skin = hexc(0xFFD9B0)
    skin_sh = hexc(0xCCA070)
    hair = hexc(0x2A1A0E)
    robe = hexc(0x3F7AD8)
    robe_sh = shade(robe, 0.65)
    robe_hi = shade(robe, 1.20)
    belt = hexc(0xE6C66E)
    sword = hexc(0xC8C8D0)
    outline = hexc(0x101018)

    # head 12x10 centered around x=16
    fill_rect(d, 11, 4, 20, 13, skin)
    # hair top + side
    fill_rect(d, 10, 3, 21, 6, hair)
    fill_rect(d, 10, 6, 11, 10, hair)
    fill_rect(d, 20, 6, 21, 10, hair)
    # eyes
    fill_rect(d, 13, 9, 14, 9, outline)
    fill_rect(d, 17, 9, 18, 9, outline)
    # face shadow
    fill_rect(d, 11, 12, 20, 13, skin_sh)
    # neck
    fill_rect(d, 14, 14, 17, 14, skin_sh)
    # torso/robe
    fill_rect(d, 9, 15, 22, 25, robe)
    fill_rect(d, 9, 15, 9, 25, robe_sh)
    fill_rect(d, 22, 15, 22, 25, robe_sh)
    # collar V
    fill_rect(d, 14, 15, 17, 17, robe_hi)
    fill_rect(d, 15, 15, 16, 16, skin)
    # belt
    fill_rect(d, 9, 21, 22, 22, belt)
    # arms
    fill_rect(d, 7, 16, 8, 23, robe)
    fill_rect(d, 23, 16, 24, 23, robe)
    fill_rect(d, 7, 23, 8, 24, skin)
    fill_rect(d, 23, 23, 24, 24, skin)
    # legs
    fill_rect(d, 11, 25, 14, 30, robe)
    fill_rect(d, 17, 25, 20, 30, robe)
    # boots
    fill_rect(d, 11, 30, 14, 31, outline)
    fill_rect(d, 17, 30, 20, 31, outline)
    # sword hilt + scabbard at right hip
    fill_rect(d, 24, 19, 25, 25, sword)
    fill_rect(d, 24, 18, 25, 18, belt)
    # outline
    stroke_rect(d, 9, 14, 22, 25, outline)
    stroke_rect(d, 10, 2, 21, 13, outline)
    return img


def draw_tree(w, h):
    """32x48 pine: dark trunk + 3 stacked canopy triangles."""
    img = new_canvas(w, h)
    d = ImageDraw.Draw(img)
    trunk = hexc(0x4A2E18)
    trunk_sh = shade(trunk, 0.7)
    leaf = hexc(0x2A8033)
    leaf_sh = shade(leaf, 0.7)
    leaf_hi = shade(leaf, 1.18)
    outline = hexc(0x0E1A10)

    # trunk
    fill_rect(d, 14, 36, 17, 47, trunk)
    fill_rect(d, 14, 36, 14, 47, trunk_sh)
    fill_rect(d, 17, 36, 17, 47, trunk_sh)
    # canopy: 3 triangles, top->bottom
    layers = [
        (16, 2, 8),   # cy, top_y, half_w (8 wide => 17 wide)
        (16, 14, 12),
        (16, 24, 14),
    ]
    for cx, top, halfw in layers:
        for row in range(top, top + halfw + 1):
            r = row - top
            ww = r + 1
            x0 = cx - ww
            x1 = cx + ww
            if x0 < 0:
                x0 = 0
            if x1 > w - 1:
                x1 = w - 1
            fill_rect(d, x0, row, x1, row, leaf)
            # left edge slightly darker
            fill_rect(d, x0, row, x0, row, leaf_sh)
            fill_rect(d, x1, row, x1, row, leaf_sh)
            # interior highlight for top half
            if r > 1 and r < halfw - 1:
                fill_rect(d, cx - 1, row, cx + 1, row, leaf_hi)
    # bottom outline of each layer
    for cx, top, halfw in layers:
        row = top + halfw
        ww = halfw + 1
        fill_rect(d, cx - ww, row + 1, cx + ww, row + 1, outline)
    return img


def draw_rock(w, h):
    """32x24 boulder: rounded grey blob."""
    img = new_canvas(w, h)
    d = ImageDraw.Draw(img)
    base = hexc(0x8C8C92)
    sh = shade(base, 0.7)
    hi = shade(base, 1.18)
    outline = hexc(0x202028)
    # boulder body
    d.ellipse([2, 6, 30, 23], fill=base)
    # left highlight
    d.ellipse([4, 8, 14, 16], fill=hi)
    # right shadow
    d.ellipse([18, 12, 28, 21], fill=sh)
    # outline
    d.ellipse([2, 6, 30, 23], outline=outline)
    # crack
    fill_rect(d, 13, 14, 14, 18, sh)
    return img


def draw_rabbit(w, h):
    """24x20 white rabbit, ears up, pink nose."""
    img = new_canvas(w, h)
    d = ImageDraw.Draw(img)
    body = hexc(0xF0EDE0)
    body_sh = shade(body, 0.85)
    pink = hexc(0xE08080)
    eye = hexc(0x101018)
    outline = hexc(0x40342A)
    # ears
    fill_rect(d, 6, 1, 8, 7, body)
    fill_rect(d, 15, 1, 17, 7, body)
    fill_rect(d, 7, 3, 7, 6, pink)
    fill_rect(d, 16, 3, 16, 6, pink)
    # head
    fill_rect(d, 5, 6, 18, 13, body)
    # body
    fill_rect(d, 3, 11, 20, 18, body)
    # body shadow under
    fill_rect(d, 3, 17, 20, 18, body_sh)
    # legs
    fill_rect(d, 4, 18, 6, 19, body_sh)
    fill_rect(d, 17, 18, 19, 19, body_sh)
    # tail
    fill_rect(d, 19, 13, 21, 14, body)
    # eyes + nose
    fill_rect(d, 8, 9, 9, 10, eye)
    fill_rect(d, 14, 9, 15, 10, eye)
    fill_rect(d, 11, 11, 12, 11, pink)
    # outline
    stroke_rect(d, 3, 11, 20, 18, outline)
    stroke_rect(d, 5, 6, 18, 13, outline)
    return img


def draw_wolf(w, h):
    """32x24 grey wolf, profile, snout left."""
    img = new_canvas(w, h)
    d = ImageDraw.Draw(img)
    base = hexc(0x6E6760)
    sh = shade(base, 0.7)
    hi = shade(base, 1.2)
    eye = hexc(0xE5E04A)
    fang = hexc(0xF0F0E8)
    outline = hexc(0x1A1812)
    # body
    fill_rect(d, 8, 10, 26, 19, base)
    # back ridge highlight
    fill_rect(d, 9, 10, 25, 11, hi)
    # belly shadow
    fill_rect(d, 8, 18, 26, 19, sh)
    # head (left side)
    fill_rect(d, 1, 8, 9, 16, base)
    # snout
    fill_rect(d, 0, 12, 4, 15, base)
    # ears
    fill_rect(d, 6, 6, 7, 8, base)
    fill_rect(d, 9, 6, 10, 8, base)
    # eye
    fill_rect(d, 5, 11, 6, 11, eye)
    # fangs
    fill_rect(d, 1, 14, 1, 14, fang)
    fill_rect(d, 3, 14, 3, 14, fang)
    # legs
    fill_rect(d, 9, 19, 11, 22, base)
    fill_rect(d, 14, 19, 16, 22, base)
    fill_rect(d, 19, 19, 21, 22, base)
    fill_rect(d, 23, 19, 25, 22, base)
    # paws
    for x in (9, 14, 19, 23):
        fill_rect(d, x, 22, x + 2, 22, sh)
    # tail
    fill_rect(d, 26, 11, 31, 13, base)
    # outline
    stroke_rect(d, 8, 10, 26, 19, outline)
    stroke_rect(d, 1, 8, 9, 16, outline)
    return img


def draw_fox_spirit(w, h):
    """28x24 magenta spirit fox, glowing aura around."""
    img = new_canvas(w, h)
    d = ImageDraw.Draw(img)
    body = hexc(0xD865D6)
    sh = shade(body, 0.7)
    hi = shade(body, 1.2)
    aura = (216, 101, 214, 80)
    eye = hexc(0xFFFFFF)
    outline = hexc(0x40143E)
    # aura halo
    d.ellipse([0, 0, 27, 23], fill=aura)
    # body
    fill_rect(d, 5, 10, 22, 17, body)
    # belly highlight
    fill_rect(d, 5, 10, 22, 11, hi)
    fill_rect(d, 5, 16, 22, 17, sh)
    # head
    fill_rect(d, 1, 9, 8, 15, body)
    # ears (long, fox)
    fill_rect(d, 2, 5, 3, 9, body)
    fill_rect(d, 7, 5, 8, 9, body)
    fill_rect(d, 2, 6, 3, 8, hi)
    # snout
    fill_rect(d, 0, 12, 2, 13, body)
    # eye
    fill_rect(d, 5, 11, 6, 11, eye)
    fill_rect(d, 5, 11, 5, 11, outline)
    # 3 spirit tails
    for i, (x, y) in enumerate([(22, 8), (24, 10), (22, 14)]):
        fill_rect(d, x, y, x + 4, y + 2, body)
        fill_rect(d, x + 3, y, x + 4, y, hi)
    # legs
    fill_rect(d, 7, 17, 9, 19, body)
    fill_rect(d, 18, 17, 20, 19, body)
    # outline
    stroke_rect(d, 5, 10, 22, 17, outline)
    stroke_rect(d, 1, 9, 8, 15, outline)
    return img


def draw_chest(w, h):
    """32x28 wooden chest, gold trim."""
    img = new_canvas(w, h)
    d = ImageDraw.Draw(img)
    wood = hexc(0x7A4A1F)
    wood_sh = shade(wood, 0.7)
    wood_hi = shade(wood, 1.2)
    gold = hexc(0xE8C868)
    gold_sh = shade(gold, 0.7)
    outline = hexc(0x1A0E04)
    # body
    fill_rect(d, 4, 14, 27, 26, wood)
    # plank lines
    for y in (16, 20, 24):
        fill_rect(d, 5, y, 26, y, wood_sh)
    # lid (rounded)
    fill_rect(d, 4, 6, 27, 13, wood)
    fill_rect(d, 5, 5, 26, 5, wood)
    fill_rect(d, 6, 4, 25, 4, wood)
    # lid highlight
    fill_rect(d, 5, 7, 26, 7, wood_hi)
    # gold straps
    fill_rect(d, 4, 9, 27, 10, gold)
    fill_rect(d, 4, 18, 27, 19, gold)
    fill_rect(d, 4, 9, 27, 9, gold_sh)
    # lock
    fill_rect(d, 14, 11, 17, 16, gold)
    fill_rect(d, 15, 13, 16, 14, outline)
    # outline
    stroke_rect(d, 4, 4, 27, 26, outline)
    return img


def draw_workbench(w, h):
    """36x28 carpenter bench: top plank + saw."""
    img = new_canvas(w, h)
    d = ImageDraw.Draw(img)
    wood = hexc(0x6B4218)
    wood_sh = shade(wood, 0.7)
    wood_hi = shade(wood, 1.18)
    saw = hexc(0xB0B0B8)
    saw_sh = shade(saw, 0.6)
    handle = hexc(0x8B5A2B)
    outline = hexc(0x18100A)
    # top
    fill_rect(d, 2, 10, 33, 16, wood)
    fill_rect(d, 2, 10, 33, 11, wood_hi)
    fill_rect(d, 2, 15, 33, 16, wood_sh)
    # legs
    fill_rect(d, 4, 16, 7, 26, wood)
    fill_rect(d, 28, 16, 31, 26, wood)
    # crossbeam
    fill_rect(d, 7, 21, 28, 22, wood_sh)
    # saw lying on top
    fill_rect(d, 8, 6, 24, 9, saw)
    # teeth
    for x in range(8, 24):
        if x % 2 == 0:
            fill_rect(d, x, 9, x, 9, saw_sh)
    # handle
    fill_rect(d, 24, 5, 28, 9, handle)
    fill_rect(d, 25, 6, 27, 8, outline)
    # outline
    stroke_rect(d, 2, 10, 33, 16, outline)
    return img


def draw_campfire(w, h):
    """32x32 fire pit: 3 flame layers + 2 logs + ring of stones."""
    img = new_canvas(w, h)
    d = ImageDraw.Draw(img)
    flame_r = hexc(0xE53A1A)
    flame_o = hexc(0xF59A20)
    flame_y = hexc(0xFFE860)
    log = hexc(0x4A2E14)
    log_hi = shade(log, 1.4)
    stone = hexc(0x8C8C92)
    stone_sh = shade(stone, 0.7)
    outline = hexc(0x1A0A04)
    # stone ring (bottom oval of small stones)
    for cx in (4, 11, 16, 21, 27):
        d.ellipse([cx - 2, 24, cx + 2, 28], fill=stone)
        d.ellipse([cx - 2, 24, cx + 2, 28], outline=stone_sh)
    # logs (X)
    fill_rect(d, 8, 22, 24, 24, log)
    fill_rect(d, 14, 19, 18, 26, log)
    fill_rect(d, 9, 22, 23, 22, log_hi)
    # flame outer red
    pts = [(16, 6), (10, 14), (12, 20), (16, 22), (20, 20), (22, 14)]
    d.polygon(pts, fill=flame_r)
    # flame mid orange
    pts2 = [(16, 9), (12, 16), (14, 20), (16, 21), (18, 20), (20, 16)]
    d.polygon(pts2, fill=flame_o)
    # flame core yellow
    pts3 = [(16, 12), (14, 17), (16, 20), (18, 17)]
    d.polygon(pts3, fill=flame_y)
    return img


def draw_water(w, h):
    """40x40 water tile: 2 wave bands."""
    img = new_canvas(w, h)
    d = ImageDraw.Draw(img)
    base = hexc(0x4FA9DC)
    hi = shade(base, 1.2)
    sh = shade(base, 0.78)
    fill_rect(d, 0, 0, w - 1, h - 1, base)
    # wave bands
    for y in (8, 22, 34):
        for x in range(0, w, 4):
            fill_rect(d, x, y, x + 1, y, hi)
            fill_rect(d, x + 2, y + 1, x + 3, y + 1, sh)
    return img


def draw_ground(w, h):
    """32x32 dirt/grass tile: tan with grass tufts."""
    img = new_canvas(w, h)
    d = ImageDraw.Draw(img)
    base = hexc(0xC4B080)
    hi = shade(base, 1.1)
    sh = shade(base, 0.85)
    grass = hexc(0x6BAF40)
    fill_rect(d, 0, 0, w - 1, h - 1, base)
    # noise dots
    for (x, y) in [(4, 6), (9, 14), (15, 5), (22, 9), (27, 18), (3, 22), (12, 28),
                   (19, 25), (26, 28), (8, 19)]:
        fill_rect(d, x, y, x, y, sh)
    for (x, y) in [(6, 11), (13, 21), (24, 5), (28, 13), (17, 16)]:
        fill_rect(d, x, y, x, y, hi)
    # grass tufts
    for (x, y) in [(5, 10), (18, 20), (25, 27), (10, 6)]:
        fill_rect(d, x, y - 1, x, y, grass)
        fill_rect(d, x - 1, y, x + 1, y, grass)
    return img


def draw_projectile(w, h):
    """16x16 fireball / sword qi: orange star with white core."""
    img = new_canvas(w, h)
    d = ImageDraw.Draw(img)
    outer = hexc(0xE53A1A)
    mid = hexc(0xF59A20)
    core = hexc(0xFFFFE0)
    d.ellipse([1, 1, 14, 14], fill=outer)
    d.ellipse([3, 3, 12, 12], fill=mid)
    d.ellipse([5, 5, 10, 10], fill=core)
    # 4 spike accents
    fill_rect(d, 7, 0, 8, 1, outer)
    fill_rect(d, 7, 14, 8, 15, outer)
    fill_rect(d, 0, 7, 1, 8, outer)
    fill_rect(d, 14, 7, 15, 8, outer)
    return img


# ---------- inventory icons ----------

def icon_frame(w, h):
    """Empty 24x24 icon canvas with subtle grey corners."""
    img = new_canvas(w, h)
    d = ImageDraw.Draw(img)
    return img, d


def draw_icon_stick(w, h):
    img, d = icon_frame(w, h)
    wood = hexc(0x8B5A2B)
    wood_sh = shade(wood, 0.7)
    wood_hi = shade(wood, 1.2)
    # diagonal stick
    for i in range(20):
        fill_rect(d, 2 + i, 20 - i, 4 + i, 22 - i, wood)
        fill_rect(d, 2 + i, 19 - i, 3 + i, 19 - i, wood_hi)
        fill_rect(d, 3 + i, 22 - i, 4 + i, 22 - i, wood_sh)
    # knot
    fill_rect(d, 11, 11, 12, 12, wood_sh)
    return img


def draw_icon_stone(w, h):
    img, d = icon_frame(w, h)
    base = hexc(0x9C9CA2)
    sh = shade(base, 0.7)
    hi = shade(base, 1.2)
    outline = hexc(0x202028)
    d.ellipse([3, 5, 21, 20], fill=base)
    d.ellipse([5, 7, 12, 13], fill=hi)
    d.ellipse([13, 14, 19, 19], fill=sh)
    d.ellipse([3, 5, 21, 20], outline=outline)
    return img


def draw_icon_meat(w, h):
    """Raw meat: red drumstick with white bone."""
    img, d = icon_frame(w, h)
    flesh = hexc(0xC83838)
    flesh_sh = shade(flesh, 0.7)
    flesh_hi = shade(flesh, 1.2)
    bone = hexc(0xF0EAD8)
    bone_sh = shade(bone, 0.78)
    outline = hexc(0x40100A)
    # body blob
    d.ellipse([4, 8, 19, 21], fill=flesh)
    d.ellipse([6, 10, 11, 14], fill=flesh_hi)
    d.ellipse([13, 15, 18, 19], fill=flesh_sh)
    d.ellipse([4, 8, 19, 21], outline=outline)
    # bone protruding top-right
    fill_rect(d, 14, 3, 18, 6, bone)
    fill_rect(d, 13, 5, 14, 9, bone)
    fill_rect(d, 17, 2, 19, 4, bone)
    fill_rect(d, 13, 5, 13, 9, bone_sh)
    return img


def draw_icon_grilled(w, h):
    """Grilled meat: brown cooked drumstick with bone, char marks."""
    img, d = icon_frame(w, h)
    flesh = hexc(0x8B5022)
    flesh_sh = shade(flesh, 0.7)
    flesh_hi = shade(flesh, 1.2)
    bone = hexc(0xF0EAD8)
    char = hexc(0x2A1A0A)
    outline = hexc(0x301508)
    d.ellipse([4, 8, 19, 21], fill=flesh)
    d.ellipse([6, 10, 11, 13], fill=flesh_hi)
    d.ellipse([13, 15, 18, 19], fill=flesh_sh)
    d.ellipse([4, 8, 19, 21], outline=outline)
    # char stripes
    fill_rect(d, 8, 14, 14, 14, char)
    fill_rect(d, 8, 17, 14, 17, char)
    # bone
    fill_rect(d, 14, 3, 18, 6, bone)
    fill_rect(d, 13, 5, 14, 9, bone)
    fill_rect(d, 17, 2, 19, 4, bone)
    return img


def draw_icon_water(w, h):
    """Water flask: blue droplet inside glass outline."""
    img, d = icon_frame(w, h)
    glass = hexc(0xC0E8F8)
    water = hexc(0x4FA9DC)
    water_hi = shade(water, 1.25)
    outline = hexc(0x1A4060)
    cork = hexc(0x8B5A2B)
    # bottle outline
    fill_rect(d, 8, 4, 15, 6, cork)
    fill_rect(d, 7, 6, 16, 7, cork)
    fill_rect(d, 5, 7, 18, 21, glass)
    # water inside
    fill_rect(d, 6, 11, 17, 20, water)
    fill_rect(d, 7, 12, 9, 14, water_hi)
    # ripples
    for y in (14, 17):
        for x in range(6, 18, 3):
            fill_rect(d, x, y, x + 1, y, water_hi)
    # outline
    stroke_rect(d, 5, 7, 18, 21, outline)
    stroke_rect(d, 7, 4, 15, 7, outline)
    return img


def draw_icon_torch(w, h):
    """Torch: stick + flame."""
    img, d = icon_frame(w, h)
    wood = hexc(0x6B3E18)
    wood_sh = shade(wood, 0.7)
    flame_r = hexc(0xE53A1A)
    flame_o = hexc(0xF59A20)
    flame_y = hexc(0xFFE860)
    # handle
    fill_rect(d, 11, 12, 12, 22, wood)
    fill_rect(d, 11, 12, 11, 22, wood_sh)
    # rags
    fill_rect(d, 9, 10, 14, 12, wood_sh)
    # flame
    pts = [(11, 2), (8, 7), (9, 11), (12, 11), (15, 11), (15, 7)]
    d.polygon(pts, fill=flame_r)
    pts2 = [(11, 4), (10, 8), (11, 10), (13, 10), (14, 8)]
    d.polygon(pts2, fill=flame_o)
    fill_rect(d, 11, 6, 12, 8, flame_y)
    return img


def draw_icon_fish(w, h):
    """Blue fish: body + tail + eye."""
    img, d = icon_frame(w, h)
    body = hexc(0x5C9DD8)
    body_sh = shade(body, 0.7)
    body_hi = shade(body, 1.2)
    eye_w = hexc(0xF8F8F8)
    eye_b = hexc(0x101018)
    outline = hexc(0x1A2C40)
    # body ellipse
    d.ellipse([3, 8, 18, 18], fill=body)
    d.ellipse([3, 8, 8, 13], fill=body_hi)
    d.ellipse([12, 14, 18, 18], fill=body_sh)
    # tail
    pts = [(17, 13), (22, 8), (22, 18)]
    d.polygon(pts, fill=body)
    pts2 = [(18, 13), (21, 10), (21, 16)]
    d.polygon(pts2, fill=body_hi)
    # eye
    fill_rect(d, 6, 11, 7, 12, eye_w)
    fill_rect(d, 6, 11, 6, 11, eye_b)
    # outline
    d.ellipse([3, 8, 18, 18], outline=outline)
    return img


def draw_icon_rod(w, h):
    """Fishing rod: bamboo pole + line + hook."""
    img, d = icon_frame(w, h)
    bamboo = hexc(0xC4A050)
    bamboo_sh = shade(bamboo, 0.7)
    bamboo_hi = shade(bamboo, 1.2)
    line = hexc(0xE0E0E0)
    hook = hexc(0xB0B0B8)
    # diagonal bamboo
    for i in range(16):
        x = 3 + i
        y = 19 - i
        fill_rect(d, x, y, x + 2, y, bamboo)
        fill_rect(d, x, y, x, y, bamboo_hi)
        fill_rect(d, x + 2, y, x + 2, y, bamboo_sh)
    # bamboo segment marks
    for s in (5, 10, 15):
        fill_rect(d, s, 19 - s + 2, s + 1, 19 - s + 2, bamboo_sh)
    # line down from tip
    for y in range(4, 14):
        fill_rect(d, 19, y, 19, y, line)
    # hook
    fill_rect(d, 18, 13, 20, 13, hook)
    fill_rect(d, 18, 14, 18, 15, hook)
    fill_rect(d, 19, 16, 20, 16, hook)
    return img


def draw_ui_white(w, h):
    img = Image.new("RGBA", (w, h), (255, 255, 255, 255))
    return img


# ---------- driver ----------

SPRITES = [
    ("player", 32, 32, draw_player),
    ("tree", 32, 48, draw_tree),
    ("rock", 32, 24, draw_rock),
    ("rabbit", 24, 20, draw_rabbit),
    ("wolf", 32, 24, draw_wolf),
    ("fox_spirit", 28, 24, draw_fox_spirit),
    ("chest", 32, 28, draw_chest),
    ("workbench", 36, 28, draw_workbench),
    ("campfire", 32, 32, draw_campfire),
    ("water", 40, 40, draw_water),
    ("ground", 32, 32, draw_ground),
    ("projectile", 16, 16, draw_projectile),
    ("icon_stick", 24, 24, draw_icon_stick),
    ("icon_stone", 24, 24, draw_icon_stone),
    ("icon_meat", 24, 24, draw_icon_meat),
    ("icon_grilled", 24, 24, draw_icon_grilled),
    ("icon_water", 24, 24, draw_icon_water),
    ("icon_torch", 24, 24, draw_icon_torch),
    ("icon_fish", 24, 24, draw_icon_fish),
    ("icon_rod", 24, 24, draw_icon_rod),
    ("ui_white", 4, 4, draw_ui_white),
]


def main():
    print(f"writing {len(SPRITES)} sprites to {OUT_DIR}")
    for name, w, h, fn in SPRITES:
        img = fn(w, h)
        save(img, name)
    print("done.")


if __name__ == "__main__":
    main()
