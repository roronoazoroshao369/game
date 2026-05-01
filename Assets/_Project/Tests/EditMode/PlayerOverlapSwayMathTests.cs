using NUnit.Framework;
using UnityEngine;
using WildernessCultivation.Vfx;

namespace WildernessCultivation.Tests.EditMode
{
    /// <summary>
    /// Pure math tests cho <see cref="PlayerOverlapSway.ComputeBendAngleDeg"/>.
    /// Direction: closer = stronger bend; sign flip theo X bên player.
    /// </summary>
    public class PlayerOverlapSwayMathTests
    {
        const float Range = 0.6f;
        const float MaxBend = 12f;

        [Test]
        public void ComputeBendAngle_PlayerExactlyOnTop_ReturnsMaxBend()
        {
            // self - player = (0, 0) → distance 0 → strength = 1 → max bend.
            // Sign defaults to +1 (selfMinusPlayer.x = 0 → >= 0 → positive).
            float a = PlayerOverlapSway.ComputeBendAngleDeg(Vector2.zero, MaxBend, Range);
            Assert.AreEqual(MaxBend, a, 0.001f);
        }

        [Test]
        public void ComputeBendAngle_PlayerAtRangeEdge_ReturnsZero()
        {
            // Distance = range → strength = 1 - 1 = 0 → bend = 0.
            float a = PlayerOverlapSway.ComputeBendAngleDeg(new Vector2(Range, 0), MaxBend, Range);
            Assert.AreEqual(0f, a, 0.001f);
        }

        [Test]
        public void ComputeBendAngle_PlayerOutsideRange_ReturnsZero()
        {
            // strength clamped 0..1 → distance > range → 0.
            float a = PlayerOverlapSway.ComputeBendAngleDeg(new Vector2(Range * 2f, 0), MaxBend, Range);
            Assert.AreEqual(0f, a, 0.001f);
        }

        [Test]
        public void ComputeBendAngle_PlayerLeft_BendsRight()
        {
            // self - player = (positive X, 0) → player ở trái → bend +Z (positive sign).
            float a = PlayerOverlapSway.ComputeBendAngleDeg(new Vector2(0.3f, 0), MaxBend, Range);
            Assert.Greater(a, 0f);
        }

        [Test]
        public void ComputeBendAngle_PlayerRight_BendsLeft()
        {
            // self - player = (negative X, 0) → player ở phải → bend -Z (negative sign).
            float a = PlayerOverlapSway.ComputeBendAngleDeg(new Vector2(-0.3f, 0), MaxBend, Range);
            Assert.Less(a, 0f);
        }

        [Test]
        public void ComputeBendAngle_HalfDistance_ReturnsHalfMaxBend()
        {
            // Distance = range/2 → strength = 1 - 0.5 = 0.5 → bend = 0.5 * MaxBend.
            float a = PlayerOverlapSway.ComputeBendAngleDeg(new Vector2(Range * 0.5f, 0), MaxBend, Range);
            Assert.AreEqual(MaxBend * 0.5f, a, 0.001f);
        }

        [Test]
        public void ComputeBendAngle_ZeroRange_ReturnsZero()
        {
            // Defensive: range 0 → div-zero risk → return 0.
            float a = PlayerOverlapSway.ComputeBendAngleDeg(new Vector2(0.1f, 0), MaxBend, 0f);
            Assert.AreEqual(0f, a, 0.001f);
        }
    }
}
