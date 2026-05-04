"""Unit tests for tools/blender_sprite_render/frame_packer.

Run from repo root:
    python3 -m pytest tools/blender_sprite_render/tests/test_frame_packer.py -v
or:
    python3 tools/blender_sprite_render/tests/test_frame_packer.py
"""
import json
import os
import sys
import tempfile
import unittest

sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
from frame_packer import (  # noqa: E402
    AtlasMetadata,
    DEFAULT_DIRECTIONS,
    is_loop_anim,
    pack_layout,
    required_atlas_size,
    smallest_pot,
    write_metadata,
)


class PotSizingTests(unittest.TestCase):
    def test_smallest_pot_returns_exact_match(self):
        self.assertEqual(smallest_pot(512), 512)

    def test_smallest_pot_rounds_up(self):
        self.assertEqual(smallest_pot(257), 512)
        self.assertEqual(smallest_pot(2049), 4096)

    def test_smallest_pot_overflow_raises(self):
        with self.assertRaises(ValueError):
            smallest_pot(8193 * 2)

    def test_required_atlas_size_minimal_case(self):
        # 4 cell @ 256 = 2x2 grid = 512 atlas
        self.assertEqual(required_atlas_size(4, 256), 512)

    def test_required_atlas_size_default_pipeline_case(self):
        # 10 anim × 4 dir × 12 frame = 480 cells @ 256
        # ceil(sqrt(480)) = 22 cells/side → 22*256 = 5632 → smallest PoT = 8192
        self.assertEqual(required_atlas_size(480, 256), 8192)

    def test_required_atlas_size_smaller_cell(self):
        # 480 cells @ 128 = 22 cells/side × 128 = 2816 → smallest PoT = 4096
        self.assertEqual(required_atlas_size(480, 128), 4096)

    def test_required_atlas_size_zero_raises(self):
        with self.assertRaises(ValueError):
            required_atlas_size(0, 256)
        with self.assertRaises(ValueError):
            required_atlas_size(10, 0)


class LoopHeuristicTests(unittest.TestCase):
    def test_default_loop_names(self):
        for name in ("idle", "walk", "run", "meditation", "victory"):
            self.assertTrue(is_loop_anim(name), f"{name} should loop by default")

    def test_default_oneshot_names(self):
        for name in ("attack", "hit", "dead", "jump", "cast"):
            self.assertFalse(is_loop_anim(name), f"{name} should NOT loop by default")

    def test_case_insensitive(self):
        self.assertTrue(is_loop_anim("Idle"))
        self.assertTrue(is_loop_anim("WALK"))

    def test_override_replaces_defaults(self):
        # Override = explicit allow-list. "idle" not in override → not looping.
        self.assertFalse(is_loop_anim("idle", override=("attack",)))
        self.assertTrue(is_loop_anim("attack", override=("attack",)))


class PackLayoutTests(unittest.TestCase):
    def setUp(self):
        self.meta = pack_layout(
            character_id="player",
            anim_names=("idle", "walk", "attack"),
            directions=DEFAULT_DIRECTIONS,
            frames_per_anim=12,
            cell=256,
        )

    def test_metadata_top_level_fields(self):
        self.assertEqual(self.meta.version, 1)
        self.assertEqual(self.meta.characterId, "player")
        self.assertEqual(self.meta.cell, 256)
        self.assertEqual(self.meta.framesPerAnim, 12)
        self.assertEqual(self.meta.frameRate, 12)

    def test_atlas_size_is_power_of_two(self):
        # 3 anim × 4 dir × 12 = 144 cells × 256 = ceil(sqrt(144))=12 cells side
        # 12 × 256 = 3072 → smallest PoT = 4096
        self.assertEqual(self.meta.atlasWidth, 4096)
        self.assertEqual(self.meta.atlasHeight, 4096)
        # Both sides must be PoT
        self.assertTrue((self.meta.atlasWidth & (self.meta.atlasWidth - 1)) == 0)

    def test_anim_count_matches_input(self):
        self.assertEqual(len(self.meta.anims), 3)
        self.assertEqual([a.name for a in self.meta.anims], ["idle", "walk", "attack"])

    def test_each_anim_has_all_directions(self):
        for anim in self.meta.anims:
            self.assertEqual(
                [d.direction for d in anim.directions],
                list(DEFAULT_DIRECTIONS),
            )

    def test_each_direction_has_frames_per_anim_frames(self):
        for anim in self.meta.anims:
            for dpack in anim.directions:
                self.assertEqual(len(dpack.frames), 12)

    def test_loop_flag_per_anim(self):
        flags = {a.name: a.loop for a in self.meta.anims}
        self.assertTrue(flags["idle"])
        self.assertTrue(flags["walk"])
        self.assertFalse(flags["attack"])

    def test_frame_rects_no_overlap_and_inside_atlas(self):
        seen = set()
        for anim in self.meta.anims:
            for dpack in anim.directions:
                for f in dpack.frames:
                    self.assertGreaterEqual(f.x, 0)
                    self.assertGreaterEqual(f.y, 0)
                    self.assertLessEqual(f.x + f.w, self.meta.atlasWidth)
                    self.assertLessEqual(f.y + f.h, self.meta.atlasHeight)
                    self.assertEqual(f.w, 256)
                    self.assertEqual(f.h, 256)
                    key = (f.x, f.y)
                    self.assertNotIn(key, seen, f"Cell {key} packed twice")
                    seen.add(key)
        self.assertEqual(len(seen), 3 * 4 * 12)

    def test_first_cell_at_origin(self):
        first = self.meta.anims[0].directions[0].frames[0]
        self.assertEqual((first.x, first.y), (0, 0))

    def test_pack_layout_rejects_empty_anims(self):
        with self.assertRaises(ValueError):
            pack_layout(
                character_id="x", anim_names=(),
                directions=DEFAULT_DIRECTIONS,
                frames_per_anim=12, cell=256,
            )

    def test_pack_layout_rejects_empty_directions(self):
        with self.assertRaises(ValueError):
            pack_layout(
                character_id="x", anim_names=("idle",),
                directions=(),
                frames_per_anim=12, cell=256,
            )

    def test_pack_layout_rejects_zero_frames(self):
        with self.assertRaises(ValueError):
            pack_layout(
                character_id="x", anim_names=("idle",),
                directions=DEFAULT_DIRECTIONS,
                frames_per_anim=0, cell=256,
            )


class JsonSchemaTests(unittest.TestCase):
    """Verify schema produced is parseable by Unity JsonUtility convention.

    JsonUtility constraints: no Dict, lists OK, primitives OK, public fields only.
    """

    def setUp(self):
        self.meta = pack_layout(
            character_id="player",
            anim_names=("idle", "walk"),
            directions=DEFAULT_DIRECTIONS,
            frames_per_anim=4,
            cell=128,
            frame_rate=10,
        )

    def test_to_dict_round_trip_via_json(self):
        d = self.meta.to_dict()
        s = json.dumps(d)
        rt = json.loads(s)
        self.assertEqual(rt["version"], 1)
        self.assertEqual(rt["characterId"], "player")
        self.assertEqual(rt["cell"], 128)
        self.assertEqual(rt["framesPerAnim"], 4)
        self.assertEqual(rt["frameRate"], 10)
        self.assertIsInstance(rt["anims"], list)
        self.assertEqual(len(rt["anims"]), 2)

    def test_schema_keys_match_unity_dto(self):
        # These must match field names in BakedSpriteCharacterMetadata.cs
        d = self.meta.to_dict()
        self.assertEqual(
            set(d.keys()),
            {"version", "characterId", "cell", "atlasWidth", "atlasHeight",
             "framesPerAnim", "frameRate", "anims"},
        )
        anim = d["anims"][0]
        self.assertEqual(set(anim.keys()), {"name", "loop", "directions"})
        dpack = anim["directions"][0]
        self.assertEqual(set(dpack.keys()), {"dir", "frames"})
        rect = dpack["frames"][0]
        self.assertEqual(set(rect.keys()), {"x", "y", "w", "h"})

    def test_write_metadata_creates_file(self):
        with tempfile.TemporaryDirectory() as tmp:
            out = os.path.join(tmp, "subdir", "frame_metadata.json")
            write_metadata(self.meta, out)
            self.assertTrue(os.path.isfile(out))
            with open(out) as f:
                rt = json.load(f)
            self.assertEqual(rt["characterId"], "player")
            # Final newline (lint convention in repo)
            with open(out) as f:
                self.assertTrue(f.read().endswith("\n"))


if __name__ == "__main__":
    unittest.main(verbosity=2)
