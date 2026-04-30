using UnityEngine;
using WildernessCultivation.Core;
using WildernessCultivation.Cultivation;
using WildernessCultivation.World;

namespace WildernessCultivation.Player.Stats
{
    /// <summary>
    /// Thermal subsystem: BodyTemp [0..100] + ambient compute (mùa + biome day/night offset
    /// + lightSource warmth + shelter bonus). Drift về ambient mỗi tick; freeze khi quá lạnh
    /// (HP+SAN damage), heat khi quá nóng (Thirst+SAN penalty).
    ///
    /// Tách khỏi <see cref="PlayerStats"/> (R1 refactor). PlayerStats giữ properties façade
    /// (<see cref="PlayerStats.BodyTemp"/>, <see cref="PlayerStats.ComputeAmbientTemperature"/>,
    /// <see cref="PlayerStats.EffectiveFreezeThreshold"/>, …) cho consumer + test cũ.
    /// </summary>
    public class ThermalComponent : MonoBehaviour
    {
        [Header("Body Temperature [0..100]; 50 = thoải mái")]
        public float BodyTemp = 50f;
        public float comfortMin = 30f;
        public float comfortMax = 70f;
        [Tooltip("Tốc độ kéo BodyTemp về ambient temperature (đơn vị/giây).")]
        public float thermalDriftRate = 4f;

        [Header("Cold (BodyTemp <= freezeThreshold)")]
        public float freezeThreshold = 10f;
        public float freezeDamagePerSec = 1.5f;

        [Header("Heat (BodyTemp >= heatThreshold)")]
        public float heatThreshold = 90f;
        public float heatThirstMult = 2.5f;
        public float heatSanityPenaltyPerSec = 0.5f;

        PlayerStats stats;
        SpiritRoot spiritRoot;
        TimeManager timeManager;

        void Awake()
        {
            ResolveDeps();
        }

        void Start()
        {
            timeManager = GameManager.Instance != null
                ? GameManager.Instance.timeManager
                : ServiceLocator.Get<TimeManager>();
        }

        // Lazy resolve — EditMode AddComponent KHÔNG fire Awake; ThermalComponent có thể
        // được PlayerStats.Awake add ra runtime, sub-Awake không chạy.
        void ResolveDeps()
        {
            if (stats == null) stats = GetComponent<PlayerStats>();
            if (spiritRoot == null) spiritRoot = GetComponent<SpiritRoot>();
        }

        /// <summary>Freeze threshold đã cộng spirit root delta (UI nên đọc giá trị này thay vì raw).</summary>
        public float EffectiveFreezeThreshold
            => freezeThreshold + (spiritRoot != null && spiritRoot.Current != null ? spiritRoot.Current.freezeThresholdDelta : 0f);

        /// <summary>Heat threshold đã cộng spirit root delta.</summary>
        public float EffectiveHeatThreshold
            => heatThreshold + (spiritRoot != null && spiritRoot.Current != null ? spiritRoot.Current.heatThresholdDelta : 0f);

        /// <summary>Nhiệt độ ambient hiện tại tại vị trí (mùa + biome day/night offset + warmth).</summary>
        public float ComputeAmbientTemperature()
        {
            float t = 50f;
            if (timeManager != null)
            {
                t = timeManager.SeasonBaselineTemperature;
                float day01 = timeManager.GetLightIntensity(); // 0=midnight, 1=noon
                if (WorldGenerator.Instance != null)
                {
                    var biome = WorldGenerator.Instance.BiomeAt(transform.position);
                    if (biome != null)
                    {
                        // Lerp giữa nightOffset và dayOffset theo day01
                        float biomeOff = Mathf.Lerp(biome.temperatureNightOffset, biome.temperatureDayOffset, day01);
                        t += biomeOff;
                    }
                }
                // Mưa làm lạnh thêm — bị block nếu trong shelter
                bool sheltered = Shelter.IsSheltered(transform.position);
                if (!sheltered)
                {
                    if (timeManager.currentWeather == Weather.Rain) t -= 5f;
                    if (timeManager.currentWeather == Weather.Storm) t -= 10f;
                }
            }
            t += LightSource.TotalWarmthAt(transform.position);
            var shelter = Shelter.NearestSheltering(transform.position);
            if (shelter != null) t += shelter.warmthBonus;
            return t;
        }

        /// <summary>Drift BodyTemp về ambient + apply freeze/heat damage. Gọi từ PlayerStats.Update.</summary>
        public void Tick(float dt)
        {
            ResolveDeps();
            if (stats == null) return;

            float ambient = ComputeAmbientTemperature();

            // Wetness boost cold drift: nếu ambient < BodyTemp, drift về lạnh nhanh hơn theo tier ướt.
            float driftRate = thermalDriftRate;
            if (ambient < BodyTemp && stats.wetness != null)
            {
                driftRate *= stats.wetness.ColdDriftMultiplier();
            }

            BodyTemp = Mathf.Lerp(BodyTemp, ambient, Mathf.Clamp01(driftRate * dt / 100f));

            float effFreezeT = EffectiveFreezeThreshold;
            float effHeatT = EffectiveHeatThreshold;
            float freezeMul = spiritRoot != null ? spiritRoot.FreezeDamageMul : 1f;

            if (BodyTemp <= effFreezeT)
            {
                stats.HP = Mathf.Max(0f, stats.HP - freezeDamagePerSec * freezeMul * dt);
                stats.Sanity = Mathf.Max(0f, stats.Sanity - 0.3f * dt);
            }
            else if (BodyTemp >= effHeatT)
            {
                stats.Thirst = Mathf.Max(0f, stats.Thirst - stats.thirstDecay * (heatThirstMult - 1f) * dt);
                stats.Sanity = Mathf.Max(0f, stats.Sanity - heatSanityPenaltyPerSec * dt);
            }
        }
    }
}
