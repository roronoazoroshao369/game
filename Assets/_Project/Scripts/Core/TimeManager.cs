using UnityEngine;

namespace WildernessCultivation.Core
{
    /// <summary>
    /// Quản lý chu kỳ ngày-đêm. 1 ngày in-game = dayLengthSeconds giây thực.
    /// Buổi đêm: linh khí đậm hơn (multiplier), quái nguy hiểm hơn.
    /// </summary>
    public class TimeManager : MonoBehaviour
    {
        [Header("Cycle")]
        [Tooltip("Một ngày = bao nhiêu giây thực. Mặc định 8 phút.")]
        public float dayLengthSeconds = 480f;

        [Range(0f, 1f)]
        public float currentTime01;

        public float dayProgress => currentTime01;
        public bool isNight => currentTime01 < 0.25f || currentTime01 > 0.75f;

        [Header("Visual")]
        public Light2DProxy globalLight;

        public System.Action OnDayStart;
        public System.Action OnNightStart;

        bool wasNight;

        void Update()
        {
            currentTime01 += Time.deltaTime / dayLengthSeconds;
            if (currentTime01 >= 1f) currentTime01 -= 1f;

            if (isNight && !wasNight) OnNightStart?.Invoke();
            else if (!isNight && wasNight) OnDayStart?.Invoke();
            wasNight = isNight;

            if (globalLight != null) globalLight.SetIntensity01(GetLightIntensity());
        }

        /// <summary>0 = tối đen, 1 = giữa trưa.</summary>
        public float GetLightIntensity()
        {
            // Cosine-based smooth: peak at 0.5 (noon), trough at 0/1 (midnight)
            float v = Mathf.Cos((currentTime01 - 0.5f) * Mathf.PI * 2f);
            return Mathf.Clamp01((v + 1f) * 0.5f);
        }

        /// <summary>Linh khí đậm hơn vào ban đêm — dùng cho hồi mana khi thiền.</summary>
        public float GetSpiritualEnergyMultiplier()
        {
            return isNight ? 1.5f : 1.0f;
        }
    }

    /// <summary>
    /// Wrapper nhẹ để tránh ràng buộc cứng với URP 2D Light. Project có thể dùng SpriteRenderer color
    /// hoặc Light2D — gắn component này vào object có 1 trong 2.
    /// </summary>
    public class Light2DProxy : MonoBehaviour
    {
        public SpriteRenderer fallbackOverlay;

        public void SetIntensity01(float v)
        {
            if (fallbackOverlay != null)
            {
                var c = fallbackOverlay.color;
                c.a = 1f - v; // overlay tối đen khi đêm
                fallbackOverlay.color = c;
            }
        }
    }
}
