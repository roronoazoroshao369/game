using System;
using UnityEngine;
using WildernessCultivation.Core;
using WildernessCultivation.Cultivation;
using WildernessCultivation.Player.Status;
using WildernessCultivation.World;

namespace WildernessCultivation.Player
{
    /// <summary>
    /// 5 chỉ số sinh tồn cốt lõi: HP, Đói, Khát, SAN (tinh thần), Linh Khí (mana).
    /// Tự decay theo thời gian; HP chảy máu nếu đói/khát = 0.
    /// </summary>
    public class PlayerStats : MonoBehaviour
    {
        [Header("Max values")]
        public float maxHP = 100f;
        public float maxHunger = 100f;
        public float maxThirst = 100f;
        public float maxSanity = 100f;
        public float maxMana = 50f;

        [Header("Current values")]
        public float HP = 100f;
        public float Hunger = 100f;
        public float Thirst = 100f;
        public float Sanity = 100f;
        public float Mana = 50f;

        [Header("Decay (per second)")]
        public float hungerDecay = 0.25f;     // ~6.6 phút từ full về 0
        public float thirstDecay = 0.4f;      // ~4.1 phút (khát đến nhanh hơn)
        public float sanityNightDecay = 0.6f; // chỉ giảm khi đêm + ngoài trời + xa lửa
        public float manaRegenIdle = 0.5f;    // hồi mana chậm khi không thiền

        [Header("Damage rates when starving/dehydrated")]
        public float starveDamagePerSec = 1.5f;
        public float dehydrateDamagePerSec = 2.5f;

        [Header("Body Temperature [0..100]; 50 = thoải mái")]
        public float BodyTemp = 50f;
        public float comfortMin = 30f;
        public float comfortMax = 70f;
        [Tooltip("Tốc độ kéo BodyTemp về ambient temperature (đơn vị/giây).")]
        public float thermalDriftRate = 4f;
        [Tooltip("Damage HP / giây khi BodyTemp < freezeThreshold.")]
        public float freezeThreshold = 10f;
        public float freezeDamagePerSec = 1.5f;
        [Tooltip("Trên ngưỡng này → mất Thirst nhanh + tụt SAN nhẹ.")]
        public float heatThreshold = 90f;
        public float heatThirstMult = 2.5f;
        public float heatSanityPenaltyPerSec = 0.5f;

        [Header("Weather effects")]
        [Tooltip("Khi trời mưa + đứng ngoài (không trong shelter/nhà) → refill Thirst chậm.")]
        public float rainThirstRefillPerSec = 0.6f;
        [Tooltip("Khi bão đêm → trừ SAN bonus.")]
        public float stormSanityPenaltyPerSec = 0.4f;
        [Tooltip("Khi đứng ngoài aura sáng vào đêm sâu (deep dark) → trừ SAN bonus.")]
        public float darknessSanityPenaltyPerSec = 0.8f;

        public event Action OnDeath;
        public event Action OnStatsChanged;

        public bool IsDead => HP <= 0f;

        /// <summary>True nếu player đang trong aura của 1 <see cref="Campfire"/> đang cháy.</summary>
        public bool IsWarm => Campfire.FindWarmthAt(transform.position) != null;

        [Header("Shield (do pháp bảo cấp tạm thời — không persist save)")]
        [Tooltip("Giá trị chắn còn lại; dame trừ vào shield trước, rồi mới HP.")]
        public float Shield;
        [Tooltip("Time.time mà shield hết hạn.")]
        public float ShieldEndsAt;
        public bool HasShield => Shield > 0f && Time.time < ShieldEndsAt;

        TimeManager timeManager;
        SpiritRoot spiritRoot;
        StatusEffectManager statusManager;
        bool maxHPApplied;

        void Start()
        {
            timeManager = GameManager.Instance != null ? GameManager.Instance.timeManager : FindObjectOfType<TimeManager>();
            spiritRoot = GetComponent<SpiritRoot>();
            statusManager = GetComponent<StatusEffectManager>();
            ApplySpiritRootMaxHP();
        }

        void ApplySpiritRootMaxHP()
        {
            if (maxHPApplied || spiritRoot == null) return;
            float mul = spiritRoot.MaxHPMul;
            if (Mathf.Abs(mul - 1f) > 0.001f)
            {
                float oldMax = maxHP;
                maxHP *= mul;
                HP = Mathf.Min(maxHP, HP * (maxHP / Mathf.Max(0.001f, oldMax)));
            }
            maxHPApplied = true;
        }

        void Update()
        {
            if (IsDead) return;
            float dt = Time.deltaTime;

            float hungerMul = spiritRoot != null ? spiritRoot.HungerDecayMul : 1f;
            float thirstMul = spiritRoot != null ? spiritRoot.ThirstDecayMul : 1f;
            float sanityMul = spiritRoot != null ? spiritRoot.SanityDecayMul : 1f;

            Hunger = Mathf.Max(0f, Hunger - hungerDecay * hungerMul * dt);
            Thirst = Mathf.Max(0f, Thirst - thirstDecay * thirstMul * dt);

            if (timeManager != null && timeManager.isNight && !IsWarm)
                Sanity = Mathf.Max(0f, Sanity - sanityNightDecay * sanityMul * dt);

            // Biome ambient SAN damage (vd Hoang Mạc Tử Khí về đêm). Lửa trại không chống được.
            if (timeManager != null && timeManager.isNight && WorldGenerator.Instance != null)
            {
                var biome = WorldGenerator.Instance.BiomeAt(transform.position);
                if (biome != null && biome.ambientNightSanDamage > 0f)
                    Sanity = Mathf.Max(0f, Sanity - biome.ambientNightSanDamage * dt);
            }

            UpdateThermal(dt);
            UpdateWeatherEffects(dt);
            UpdateDarkness(dt);

            if (Hunger <= 0f) HP = Mathf.Max(0f, HP - starveDamagePerSec * dt);
            if (Thirst <= 0f) HP = Mathf.Max(0f, HP - dehydrateDamagePerSec * dt);

            Mana = Mathf.Min(maxMana, Mana + manaRegenIdle * dt);

            OnStatsChanged?.Invoke();

            if (HP <= 0f) Die();
        }

        public void TakeDamage(float dmg)
        {
            if (IsDead) return;
            // Status effect modifier (Burn x1.2…)
            if (statusManager != null) dmg *= statusManager.IncomingDamageMultiplier;
            if (HasShield)
            {
                float absorbed = Mathf.Min(Shield, dmg);
                Shield -= absorbed;
                dmg -= absorbed;
            }
            else if (Shield > 0f)
            {
                // Shield đã hết hạn → reset
                Shield = 0f;
            }
            if (dmg > 0f) HP = Mathf.Max(0f, HP - dmg);
            OnStatsChanged?.Invoke();
            if (HP <= 0f) Die();
        }

        /// <summary>Nhiệt độ ambient hiện tại tại vị trí player (mùa + biome day/night offset + lightSource warmth).</summary>
        public float ComputeAmbientTemperature()
        {
            float t = 50f;
            if (timeManager != null)
            {
                t = timeManager.SeasonBaselineTemperature;
                // Biên độ ngày/đêm: theo dayProgress (cosine-based, peak noon)
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
                    if (timeManager.currentWeather == Weather.Rain)  t -= 5f;
                    if (timeManager.currentWeather == Weather.Storm) t -= 10f;
                }
            }
            t += LightSource.TotalWarmthAt(transform.position);
            var shelter = Shelter.NearestSheltering(transform.position);
            if (shelter != null) t += shelter.warmthBonus;
            return t;
        }

        void UpdateThermal(float dt)
        {
            float ambient = ComputeAmbientTemperature();
            BodyTemp = Mathf.Lerp(BodyTemp, ambient, Mathf.Clamp01(thermalDriftRate * dt / 100f));

            float effFreezeT = freezeThreshold + (spiritRoot != null && spiritRoot.Current != null ? spiritRoot.Current.freezeThresholdDelta : 0f);
            float effHeatT = heatThreshold + (spiritRoot != null && spiritRoot.Current != null ? spiritRoot.Current.heatThresholdDelta : 0f);
            float freezeMul = spiritRoot != null ? spiritRoot.FreezeDamageMul : 1f;

            if (BodyTemp <= effFreezeT)
            {
                HP = Mathf.Max(0f, HP - freezeDamagePerSec * freezeMul * dt);
                Sanity = Mathf.Max(0f, Sanity - 0.3f * dt);
            }
            else if (BodyTemp >= effHeatT)
            {
                Thirst = Mathf.Max(0f, Thirst - thirstDecay * (heatThirstMult - 1f) * dt);
                Sanity = Mathf.Max(0f, Sanity - heatSanityPenaltyPerSec * dt);
            }
        }

        void UpdateWeatherEffects(float dt)
        {
            if (timeManager == null) return;
            // Trong shelter → không dính mưa/bão
            if (Shelter.IsSheltered(transform.position)) return;
            switch (timeManager.currentWeather)
            {
                case Weather.Rain:
                    Thirst = Mathf.Min(maxThirst, Thirst + rainThirstRefillPerSec * dt);
                    break;
                case Weather.Storm:
                    Thirst = Mathf.Min(maxThirst, Thirst + rainThirstRefillPerSec * dt);
                    if (timeManager.isNight)
                        Sanity = Mathf.Max(0f, Sanity - stormSanityPenaltyPerSec * dt);
                    break;
            }
        }

        void UpdateDarkness(float dt)
        {
            if (timeManager == null || !timeManager.isNight) return;
            if (LightSource.AnyLightAt(transform.position)) return;
            // Đêm + ngoài tất cả nguồn sáng → "deep dark"
            Sanity = Mathf.Max(0f, Sanity - darknessSanityPenaltyPerSec * dt);
        }

        /// <summary>Tạo / cộng dồn shield. Lấy max(durationSec) để không bị shield mới ngắn hơn ghi đè shield cũ dài hơn.</summary>
        public void AddShield(float amount, float durationSec)
        {
            if (amount <= 0f || durationSec <= 0f) return;
            Shield = Mathf.Max(Shield, 0f) + amount;
            ShieldEndsAt = Mathf.Max(ShieldEndsAt, Time.time + durationSec);
            OnStatsChanged?.Invoke();
        }

        public void Heal(float amount)
        {
            HP = Mathf.Min(maxHP, HP + amount);
            OnStatsChanged?.Invoke();
        }

        public void Eat(float foodValue)
        {
            Hunger = Mathf.Min(maxHunger, Hunger + foodValue);
            OnStatsChanged?.Invoke();
        }

        public void Drink(float waterValue)
        {
            Thirst = Mathf.Min(maxThirst, Thirst + waterValue);
            OnStatsChanged?.Invoke();
        }

        public void RestoreSanity(float amount)
        {
            Sanity = Mathf.Min(maxSanity, Sanity + amount);
            OnStatsChanged?.Invoke();
        }

        public bool TryConsumeMana(float cost)
        {
            if (Mana < cost) return false;
            Mana -= cost;
            OnStatsChanged?.Invoke();
            return true;
        }

        public void AddMana(float amount)
        {
            Mana = Mathf.Min(maxMana, Mana + amount);
            OnStatsChanged?.Invoke();
        }

        void Die()
        {
            OnDeath?.Invoke();
            Debug.Log("[Player] Died.");
        }
    }
}
