using UnityEngine;

namespace WildernessCultivation.Player.Stats
{
    /// <summary>
    /// Shield subsystem: absorb damage trước HP, có thời hạn (<c>ShieldEndsAt</c>).
    /// R1 phase 2 refactor. <see cref="PlayerStats"/> façade giữ <c>Shield</c> / <c>ShieldEndsAt</c>
    /// / <c>HasShield</c> / <c>AddShield</c>.
    /// </summary>
    public class ShieldComponent : MonoBehaviour
    {
        [Header("Shield (do pháp bảo cấp tạm thời — không persist save)")]
        [Tooltip("Giá trị chắn còn lại; dame trừ vào shield trước, rồi mới HP.")]
        public float Shield = 0f;
        [Tooltip("Time.time mà shield hết hạn.")]
        public float ShieldEndsAt = 0f;

        /// <summary>True khi có shield còn hiệu lực.</summary>
        public bool HasShield => Shield > 0f && Time.time < ShieldEndsAt;

        /// <summary>
        /// Tạo / cộng dồn shield. Lấy max(durationSec) để không bị shield mới ngắn hơn ghi đè
        /// cũ dài hơn.
        /// </summary>
        public void Add(float amount, float durationSec)
        {
            if (amount <= 0f || durationSec <= 0f) return;
            Shield = Mathf.Max(Shield, 0f) + amount;
            ShieldEndsAt = Mathf.Max(ShieldEndsAt, Time.time + durationSec);
        }

        /// <summary>
        /// Hấp thụ dame. Return phần dame còn lại sau khi shield ăn (caller trừ vào HP).
        /// Expired shield tự reset về 0.
        /// </summary>
        public float Absorb(float dmg)
        {
            if (HasShield)
            {
                float absorbed = Mathf.Min(Shield, dmg);
                Shield -= absorbed;
                return Mathf.Max(0f, dmg - absorbed);
            }
            if (Shield > 0f) Shield = 0f;
            return dmg;
        }
    }
}
