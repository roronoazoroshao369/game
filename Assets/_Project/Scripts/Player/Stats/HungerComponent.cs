using UnityEngine;

namespace WildernessCultivation.Player.Stats
{
    /// <summary>
    /// Hunger subsystem: gauge [0..maxHunger] + decay rate + Eat API.
    /// R1 phase 2 refactor. <see cref="PlayerStats"/> façade giữ <c>Hunger</c> / <c>maxHunger</c>
    /// / <c>hungerDecay</c> / <c>Eat</c> để consumer không break.
    /// </summary>
    public class HungerComponent : MonoBehaviour
    {
        [Header("Hunger")]
        public float Hunger = 100f;
        public float maxHunger = 100f;
        [Tooltip("Hunger giảm mỗi giây (~6.6 phút từ full về 0 với mul=1).")]
        public float hungerDecay = 0.25f;

        /// <summary>True khi Hunger cạn — caller áp starve damage.</summary>
        public bool IsStarving => Hunger <= 0f;

        /// <summary>Cộng Hunger khi ăn, clamp <= maxHunger.</summary>
        public void Eat(float value)
        {
            Hunger = Mathf.Min(maxHunger, Hunger + value);
        }

        /// <summary>Decay theo dt, nhân với multiplier (spirit root / status effect).</summary>
        public void Tick(float dt, float decayMul = 1f)
        {
            Hunger = Mathf.Max(0f, Hunger - hungerDecay * decayMul * dt);
        }
    }
}
