"""Atlas packing + frame_metadata.json writer.

Pure Python module — no Blender deps. Tested via tests/test_frame_packer.py.

Layout strategy: row-major pack rendered cells (one per anim/direction/frame)
into a Power-of-Two RGBA atlas. Schema is List-friendly so Unity JsonUtility
can deserialize without Newtonsoft (xem schema docstring §SCHEMA below).

§SCHEMA (frame_metadata.json):
    {
      "version": 1,
      "characterId": "<id>",
      "cell": <px>,                 // square cell size
      "atlasWidth": <px>,
      "atlasHeight": <px>,
      "framesPerAnim": <int>,
      "frameRate": <int>,           // playback fps in Unity
      "anims": [
        {
          "name": "idle",
          "loop": true,
          "directions": [
            {
              "dir": "S",            // S / E / N / W
              "frames": [
                { "x": <px>, "y": <px>, "w": <px>, "h": <px> },
                ...
              ]
            }, ...
          ]
        }, ...
      ]
    }

Pivot convention: Unity sprite pivot computed importer-side (foot anchor,
default 0.5 / 0.25). Atlas Y-axis follows PIL/Unity convention (top-left
origin, y increases downward) — Unity TextureImporter expects bottom-left,
the importer flips on read. The packer writes top-left.
"""
from __future__ import annotations

import json
import math
import os
from dataclasses import dataclass, field
from typing import Dict, Iterable, List, Optional, Sequence

POT_SIZES = (256, 512, 1024, 2048, 4096, 8192)
DEFAULT_DIRECTIONS = ("S", "E", "N", "W")
DEFAULT_LOOP_NAMES = frozenset({"idle", "walk", "run", "meditation", "victory"})


@dataclass
class FrameRect:
    """Rect for one rendered frame (top-left origin, pixels)."""

    x: int
    y: int
    w: int
    h: int

    def to_dict(self) -> Dict[str, int]:
        return {"x": self.x, "y": self.y, "w": self.w, "h": self.h}


@dataclass
class DirectionPack:
    direction: str
    frames: List[FrameRect] = field(default_factory=list)

    def to_dict(self) -> Dict[str, object]:
        return {
            "dir": self.direction,
            "frames": [f.to_dict() for f in self.frames],
        }


@dataclass
class AnimPack:
    name: str
    loop: bool
    directions: List[DirectionPack] = field(default_factory=list)

    def to_dict(self) -> Dict[str, object]:
        return {
            "name": self.name,
            "loop": self.loop,
            "directions": [d.to_dict() for d in self.directions],
        }


@dataclass
class AtlasMetadata:
    version: int
    characterId: str
    cell: int
    atlasWidth: int
    atlasHeight: int
    framesPerAnim: int
    frameRate: int
    anims: List[AnimPack] = field(default_factory=list)

    def to_dict(self) -> Dict[str, object]:
        return {
            "version": self.version,
            "characterId": self.characterId,
            "cell": self.cell,
            "atlasWidth": self.atlasWidth,
            "atlasHeight": self.atlasHeight,
            "framesPerAnim": self.framesPerAnim,
            "frameRate": self.frameRate,
            "anims": [a.to_dict() for a in self.anims],
        }


def smallest_pot(side_min: int) -> int:
    """Return smallest PoT ≥ side_min from POT_SIZES."""
    for s in POT_SIZES:
        if s >= side_min:
            return s
    raise ValueError(
        f"Required atlas side {side_min}px exceeds max supported {POT_SIZES[-1]}px"
    )


def required_atlas_size(num_cells: int, cell: int) -> int:
    """Smallest PoT square fitting `num_cells` cells of size `cell`."""
    if num_cells <= 0 or cell <= 0:
        raise ValueError("num_cells and cell must be positive")
    cells_per_side = math.ceil(math.sqrt(num_cells))
    side_px = cells_per_side * cell
    return smallest_pot(side_px)


def is_loop_anim(name: str, override: Optional[Sequence[str]] = None) -> bool:
    s = name.strip().lower()
    if override is not None:
        return s in {n.lower() for n in override}
    return s in DEFAULT_LOOP_NAMES


def pack_layout(
    character_id: str,
    anim_names: Sequence[str],
    directions: Sequence[str],
    frames_per_anim: int,
    cell: int,
    frame_rate: int = 12,
    loop_overrides: Optional[Sequence[str]] = None,
) -> AtlasMetadata:
    """Compute atlas size + per-cell rects without rendering anything.

    Cells are packed row-major in the order:
      anim[0].dir[0].frame[0..N], anim[0].dir[1].frame[0..N], ..., anim[K].dir[D].frame[N]

    Returns metadata with x/y rects filled. Caller is responsible for actually
    pasting PIL Images at the rects (xem build_atlas_from_renders).
    """
    if not anim_names:
        raise ValueError("anim_names must be non-empty")
    if not directions:
        raise ValueError("directions must be non-empty")
    if frames_per_anim <= 0:
        raise ValueError("frames_per_anim must be positive")

    total_cells = len(anim_names) * len(directions) * frames_per_anim
    side = required_atlas_size(total_cells, cell)
    cells_per_row = side // cell

    meta = AtlasMetadata(
        version=1,
        characterId=character_id,
        cell=cell,
        atlasWidth=side,
        atlasHeight=side,
        framesPerAnim=frames_per_anim,
        frameRate=frame_rate,
        anims=[],
    )

    cell_idx = 0
    for anim_name in anim_names:
        ap = AnimPack(name=anim_name, loop=is_loop_anim(anim_name, loop_overrides))
        for dir_name in directions:
            dp = DirectionPack(direction=dir_name)
            for _ in range(frames_per_anim):
                row = cell_idx // cells_per_row
                col = cell_idx % cells_per_row
                dp.frames.append(
                    FrameRect(x=col * cell, y=row * cell, w=cell, h=cell)
                )
                cell_idx += 1
            ap.directions.append(dp)
        meta.anims.append(ap)

    return meta


def write_metadata(meta: AtlasMetadata, out_path: str) -> None:
    os.makedirs(os.path.dirname(out_path), exist_ok=True)
    with open(out_path, "w", encoding="utf-8") as f:
        json.dump(meta.to_dict(), f, indent=2)
        f.write("\n")


def build_atlas_from_renders(
    meta: AtlasMetadata,
    render_root: str,
    atlas_out_path: str,
    metadata_out_path: str,
) -> None:
    """Compose atlas PNG by pasting pre-rendered cell PNGs at meta rects.

    Expected render layout under `render_root`:
        {anim_name}/{dir}/{frame_idx:02d}.png

    Missing frames raise FileNotFoundError — fail loud (don't silently skip).
    """
    try:
        from PIL import Image  # type: ignore
    except ImportError as e:
        raise ImportError(
            "PIL/Pillow required: pip install Pillow"
        ) from e

    atlas = Image.new("RGBA", (meta.atlasWidth, meta.atlasHeight), (0, 0, 0, 0))

    for anim in meta.anims:
        for dpack in anim.directions:
            for frame_idx, rect in enumerate(dpack.frames):
                src = os.path.join(
                    render_root,
                    anim.name,
                    dpack.direction,
                    f"{frame_idx:02d}.png",
                )
                if not os.path.isfile(src):
                    raise FileNotFoundError(
                        f"Missing render: {src} (anim={anim.name}, dir={dpack.direction}, idx={frame_idx})"
                    )
                cell_img = Image.open(src).convert("RGBA")
                if cell_img.size != (meta.cell, meta.cell):
                    cell_img = cell_img.resize((meta.cell, meta.cell), Image.LANCZOS)
                atlas.paste(cell_img, (rect.x, rect.y), cell_img)

    os.makedirs(os.path.dirname(atlas_out_path), exist_ok=True)
    atlas.save(atlas_out_path, "PNG", optimize=True)
    write_metadata(meta, metadata_out_path)


def assert_anim_count(
    meta: AtlasMetadata, expected_anims: Iterable[str]
) -> None:
    """Sanity check: meta has exactly the expected anim names (order-insensitive)."""
    have = {a.name for a in meta.anims}
    want = set(expected_anims)
    missing = want - have
    extra = have - want
    if missing or extra:
        raise AssertionError(
            f"Anim mismatch — missing={sorted(missing)}, extra={sorted(extra)}"
        )
