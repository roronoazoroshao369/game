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

        [Tooltip("Số ngày in-game đã sống sót (tăng mỗi khi currentTime01 wrap qua 1).")]
        public int daysSurvived;

        public float dayProgress => currentTime01;
        public bool isNight => currentTime01 < 0.25f || currentTime01 > 0.75f;

        [Header("Visual")]
        public Light2DProxy globalLight;

        public System.Action OnDayStart;
        public System.Action OnNightStart;
        public System.Action<Season> OnSeasonChanged;
        public System.Action<Weather> OnWeatherChanged;

        [Header("Season / Weather")]
        [Tooltip("Số ngày 1 mùa kéo dài. 4 mùa luân phiên Xuân → Hạ → Thu → Đông.")]
        public int daysPerSeason = 5;
        public Season currentSeason = Season.Spring;
        public Weather currentWeather = Weather.Clear;
        [Range(0f, 1f)]
        [Tooltip("Xác suất ROLL ra Rain mỗi sáng. Storm = rain^2 (rain phải pass thì mới roll storm).")]
        public float rainChanceBase = 0.25f;

        bool wasNight;

        void Update()
        {
            currentTime01 += Time.deltaTime / dayLengthSeconds;
            if (currentTime01 >= 1f)
            {
                currentTime01 -= 1f;
                daysSurvived++;
                RollWeather();
            }

            int seasonIdx = Mathf.Max(1, daysPerSeason) > 0 ? (daysSurvived / Mathf.Max(1, daysPerSeason)) % 4 : 0;
            var newSeason = (Season)seasonIdx;
            if (newSeason != currentSeason)
            {
                currentSeason = newSeason;
                OnSeasonChanged?.Invoke(currentSeason);
                Debug.Log($"[Time] Sang mùa {currentSeason}.");
            }

            if (isNight && !wasNight) OnNightStart?.Invoke();
            else if (!isNight && wasNight) OnDayStart?.Invoke();
            wasNight = isNight;

            if (globalLight != null) globalLight.SetIntensity01(GetLightIntensity());
        }

        void RollWeather()
        {
            // Mưa nhiều hơn vào Xuân/Thu, ít hơn vào Hạ/Đông
            float rainChance = currentSeason switch
            {
                Season.Spring => rainChanceBase * 1.5f,
                Season.Autumn => rainChanceBase * 1.3f,
                Season.Summer => rainChanceBase * 0.7f,
                Season.Winter => rainChanceBase * 0.6f,
                _             => rainChanceBase,
            };
            float roll = Random.value;
            Weather next;
            if (roll < rainChance * rainChance) next = Weather.Storm;
            else if (roll < rainChance) next = Weather.Rain;
            else next = Weather.Clear;

            if (next != currentWeather)
            {
                currentWeather = next;
                OnWeatherChanged?.Invoke(currentWeather);
                Debug.Log($"[Time] Thời tiết hôm nay: {currentWeather}.");
            }
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

        /// <summary>
        /// Nhiệt độ baseline của mùa hiện tại. Range tham chiếu 0..100; 50 = neutral.
        /// </summary>
        public float SeasonBaselineTemperature => currentSeason switch
        {
            Season.Spring => 55f,
            Season.Summer => 70f,
            Season.Autumn => 50f,
            Season.Winter => 30f,
            _             => 50f,
        };
    }

    public enum Season { Spring, Summer, Autumn, Winter }
    public enum Weather { Clear, Rain, Storm }

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
