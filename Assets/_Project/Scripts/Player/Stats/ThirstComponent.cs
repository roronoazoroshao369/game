using UnityEngine;

namespace WildernessCultivation.Player.Stats
{
    /// <summary>
    /// Thirst subsystem: gauge [0..maxThirst] + decay rate + Drink API + rain refill.
    /// R1 phase 2 refactor. <see cref="PlayerStats"/> façade giữ <c>Thirst</c> / <c>maxThirst</c>
    /// / <c>thirstDecay</c> / <c>rainThirstRefillPerSec</c> / <c>Drink</c> để consumer không break.
    /// </summary>
    public class ThirstComponent : MonoBehaviour
    {
        [Header("Thirst")]
        public float Thirst = 100f;
        public float maxThirst = 100f;
        [Tooltip("Thirst giảm mỗi giây (~4.1 phút từ full về 0 với mul=1).")]
        public float thirstDecay = 0.4f;

        [Tooltip("Khi trời mưa + đứng ngoài (không trong shelter/nhà) → refill Thirst chậm.")]
        public float rainThirstRefillPerSec = 0.6f;

        /// <summary>True khi Thirst cạn — caller áp dehydrate damage.</summary>
        public bool IsDehydrated => Thirst <= 0f;

        /// <summary>Cộng Thirst khi uống, clamp <= maxThirst.</summary>
        public void Drink(float value)
        {
            Thirst = Mathf.Min(maxThirst, Thirst + value);
        }

        /// <summary>Refill từ mưa. Gọi từ PlayerStats khi weather=Rain/Storm + !sheltered.</summary>
        public void RefillFromRain(float dt)
        {
            Thirst = Mathf.Min(maxThirst, Thirst + rainThirstRefillPerSec * dt);
        }

        /// <summary>Decay theo dt, nhân với multiplier (spirit root / heat).</summary>
        public void Tick(float dt, float decayMul = 1f)
        {
            Thirst = Mathf.Max(0f, Thirst - thirstDecay * decayMul * dt);
        }
    }
}
