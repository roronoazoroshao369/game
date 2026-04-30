"""Unit tests for tools/seam_fix.py.

Run from repo root:
    python3 -m pytest tools/test_seam_fix.py -v
or:
    python3 tools/test_seam_fix.py
"""
import os
import subprocess
import sys
import tempfile
import unittest

import numpy as np
from PIL import Image

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from seam_fix import feather_1d  # noqa: E402

SCRIPT = os.path.join(os.path.dirname(os.path.abspath(__file__)), "seam_fix.py")


class FeatherTests(unittest.TestCase):
    def test_band_zero_returns_all_ones(self):
        m = feather_1d(1024, 0)
        self.assertTrue(np.allclose(m, 1.0))

    def test_band_negative_returns_all_ones(self):
        m = feather_1d(1024, -5)
        self.assertTrue(np.allclose(m, 1.0))

    def test_band_clamped_to_half_n(self):
        # Regression: when band > n // 2 the two ramps used to overlap and
        # produced a ~0.75 jump in the middle of the mask.
        m = feather_1d(128, 96)
        max_jump = float(np.max(np.abs(np.diff(m))))
        self.assertLess(max_jump, 0.05)

    def test_smoothstep_endpoints(self):
        m = feather_1d(1024, 96)
        self.assertEqual(m[0], 0.0)
        self.assertAlmostEqual(float(m[95]), 1.0, places=5)
        self.assertEqual(m[-1], 0.0)
        self.assertAlmostEqual(float(m[-96]), 1.0, places=5)

    def test_mask_is_symmetric(self):
        m = feather_1d(1024, 96)
        self.assertTrue(np.allclose(m, m[::-1]))

    def test_interior_is_one(self):
        m = feather_1d(1024, 96)
        self.assertTrue(np.allclose(m[96:-96], 1.0))


class DownscalePipelineTests(unittest.TestCase):
    """Verify CLI end-to-end. Creates a tiny synthetic RGBA tile, runs script,
    asserts output dimensions match the requested final_size."""

    def _run(self, src_size: int, final_size: int) -> tuple:
        with tempfile.TemporaryDirectory() as tmp:
            src = os.path.join(tmp, "src.png")
            dst = os.path.join(tmp, "dst.png")
            # Random RGBA noise (deterministic seed) so seam-fix has high-freq
            # texture similar to grass tiles.
            rng = np.random.default_rng(42)
            arr = rng.integers(0, 255, size=(src_size, src_size, 4), dtype=np.uint8)
            arr[..., 3] = 255  # opaque
            Image.fromarray(arr, mode="RGBA").save(src)

            subprocess.run(
                [sys.executable, SCRIPT, src, dst, "16", str(final_size)],
                check=True, capture_output=True, text=True,
            )
            with Image.open(dst) as im:
                return im.size

    def test_downscale_to_64(self):
        self.assertEqual(self._run(1024, 64), (64, 64))

    def test_downscale_to_256(self):
        # Regression: previously hardcoded steps (256, 128, 256) shrunk to
        # 128 then skipped final 256 step.
        self.assertEqual(self._run(1024, 256), (256, 256))

    def test_downscale_to_128(self):
        self.assertEqual(self._run(1024, 128), (128, 128))

    def test_skip_downscale_when_final_size_zero(self):
        self.assertEqual(self._run(512, 0), (512, 512))

    def test_no_upscale_when_input_smaller_than_final(self):
        # final_size=128 on 64-input should not upscale to 128.
        self.assertEqual(self._run(64, 128), (64, 64))


if __name__ == "__main__":
    unittest.main(verbosity=2)
