using UnityEngine;

namespace WildernessCultivation.Player.Stats
{
    /// <summary>
    /// Sanity subsystem: gauge [0..maxSanity] + night decay + storm / darkness penalties
    /// + Restore / Damage API.
    ///
    /// R1 phase 2 refactor. <see cref="PlayerStats"/> façade giữ <c>Sanity</c> / <c>maxSanity</c>
    /// / <c>sanityNightDecay</c> / <c>stormSanityPenaltyPerSec</c> / <c>darknessSanityPenaltyPerSec</c>
    /// / <c>RestoreSanity</c> / <c>DamageSanity</c>.
    /// </summary>
    public class SanityComponent : MonoBehaviour
    {
        [Header("Sanity")]
        public float Sanity = 100f;
        public float maxSanity = 100f;

        [Header("Decay (per second)")]
        [Tooltip("Sanity giảm mỗi giây vào đêm + ngoài trời + xa lửa.")]
        public float sanityNightDecay = 0.6f;
        [Tooltip("Khi bão đêm → trừ SAN bonus.")]
        public float stormSanityPenaltyPerSec = 0.4f;
        [Tooltip("Khi đứng ngoài aura sáng vào đêm sâu (deep dark) → trừ SAN bonus.")]
        public float darknessSanityPenaltyPerSec = 0.8f;

        /// <summary>Hồi Sanity, clamp <= maxSanity.</summary>
        public void Restore(float amount)
        {
            Sanity = Mathf.Min(maxSanity, Sanity + amount);
        }

        /// <summary>Trừ Sanity (clamp >= 0). Dùng cho environmental SAN drain.</summary>
        public void Damage(float amount)
        {
            if (amount <= 0f) return;
            Sanity = Mathf.Max(0f, Sanity - amount);
        }

        /// <summary>Night decay (ngoài lửa + đêm). Gọi từ PlayerStats.Update.</summary>
        public void TickNightDecay(float dt, float decayMul = 1f)
        {
            Sanity = Mathf.Max(0f, Sanity - sanityNightDecay * decayMul * dt);
        }

        /// <summary>Storm penalty (ngoài shelter + đêm). Gọi từ PlayerStats.Update.</summary>
        public void ApplyStormPenalty(float dt)
        {
            Damage(stormSanityPenaltyPerSec * dt);
        }

        /// <summary>Darkness penalty (đêm + ngoài mọi nguồn sáng). Gọi từ PlayerStats.Update.</summary>
        public void ApplyDarknessPenalty(float dt)
        {
            Damage(darknessSanityPenaltyPerSec * dt);
        }
    }
}
