using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using WildernessCultivation.Combat;
using WildernessCultivation.Core;
using WildernessCultivation.Cultivation;
using WildernessCultivation.Items;
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

        [Header("Wetness [0..100] — đứng mưa làm ướt, lửa/nắng/shelter làm khô")]
        [Tooltip("Ướt hiện tại. Tier: <20 Dry, 20..50 Damp, 50..80 Wet, >=80 Drenched.")]
        [Range(0f, 100f)] public float Wetness = 0f;
        public float maxWetness = 100f;
        [Tooltip("Tốc độ ướt thêm khi đứng mưa (đơn vị/giây).")]
        public float wetnessRainPerSec = 4f;
        [Tooltip("Storm: ướt nhanh hơn rain (multiplier).")]
        public float wetnessStormMultiplier = 2f;
        [Tooltip("Tốc độ khô khi không bị mưa (đơn vị/giây). Lửa/nắng/shelter cộng thêm.")]
        public float wetnessDryBasePerSec = 0.5f;
        [Tooltip("Bonus tốc độ khô khi gần lửa trại (cộng vào dry rate).")]
        public float wetnessDryFireBonus = 4f;
        [Tooltip("Bonus tốc độ khô khi trong shelter.")]
        public float wetnessDryShelterBonus = 2f;
        [Tooltip("Bonus tốc độ khô vào ban ngày (nắng), pro-rate theo light intensity.")]
        public float wetnessDryDayBonus = 1.5f;

        [Header("Wetness tier coupling")]
        [Tooltip("Damp (>=20): multiplier thermalDriftRate khi ambient lạnh hơn BodyTemp.")]
        public float dampColdDriftMultiplier = 1.2f;
        [Tooltip("Wet (>=50): multiplier thermalDriftRate khi ambient lạnh.")]
        public float wetColdDriftMultiplier = 1.4f;
        [Tooltip("Drenched (>=80): multiplier thermalDriftRate khi ambient lạnh.")]
        public float drenchedColdDriftMultiplier = 1.7f;
        [Tooltip("Wet (>=50): trừ SAN / giây.")]
        public float wetSanityPenaltyPerSec = 0.2f;
        [Tooltip("Drenched (>=80): trừ SAN / giây (chồng lên wet penalty).")]
        public float drenchedSanityPenaltyPerSec = 0.3f;

        [Header("Wetness sickness chain")]
        [Tooltip("Khi Drenched (>=80) + BodyTemp dưới ngưỡng này → roll Sickness. -1 = dùng comfortMin.")]
        public float sicknessColdThreshold = -1f;
        [Tooltip("Xác suất / giây apply Sickness khi điều kiện đạt. 0 = tắt chain.")]
        public float sicknessChancePerSec = 0.02f;
        [Tooltip("Sickness status apply khi điều kiện đạt. Null = không apply.")]
        public StatusEffectSO sicknessEffect;
        [Tooltip("Cooldown sau khi apply Sickness — tránh spam reapply mỗi frame. Giây.")]
        public float sicknessApplyCooldownSec = 30f;
        float nextSicknessAllowedAt;

        public event Action OnDeath;
        public event Action OnStatsChanged;

        public bool IsDead => HP <= 0f;

        [Header("Tu tiên gating")]
        [Tooltip("True = đã khai mở tu tiên. Mặc định Thường Nhân (false). Set qua AwakeningSystem.")]
        public bool IsAwakened = false;

        [Tooltip("Pity counter — số lần roll Phàm liên tiếp. AwakeningSystem cộng dồn mỗi fail, reset 0 khi success bất kỳ grade. Per-run (save với main save → wipe khi permadeath).")]
        public int phamFailStreak = 0;

        [Header("Permadeath")]
        [Tooltip("True = khi HP về 0 sẽ wipe save slot, dump inventory thành tombstone, reload scene. False = chỉ raise OnDeath (cho test / tutorial mode).")]
        public bool permadeathEnabled = true;
        [Tooltip("Giây delay trước khi reload scene sau khi chết (cho tử vong overlay).")]
        public float deathReloadDelay = 1.5f;

        /// <summary>True nếu player đang trong aura của 1 <see cref="Campfire"/> đang cháy.</summary>
        public bool IsWarm => Campfire.FindWarmthAt(transform.position) != null;

        [Header("Shield (do pháp bảo cấp tạm thời — không persist save)")]
        [Tooltip("Giá trị chắn còn lại; dame trừ vào shield trước, rồi mới HP.")]
        public float Shield;
        [Tooltip("Time.time mà shield hết hạn.")]
        public float ShieldEndsAt;
        public bool HasShield => Shield > 0f && Time.time < ShieldEndsAt;

        [Header("Invulnerability (i-frames cho dodge / hồi sinh)")]
        [Tooltip("Time.time mà i-frames hết hạn. Trong khoảng này TakeDamage bỏ qua.")]
        public float InvulnerableUntil;
        public bool IsInvulnerable => Time.time < InvulnerableUntil;

        /// <summary>Set i-frames trong duration giây tính từ thời điểm gọi.</summary>
        public void SetInvulnerable(float duration)
        {
            float end = Time.time + Mathf.Max(0f, duration);
            if (end > InvulnerableUntil) InvulnerableUntil = end;
        }

        /// <summary>Freeze threshold đã cộng spirit root delta (UI nên đọc giá trị này thay vì raw).</summary>
        public float EffectiveFreezeThreshold
            => freezeThreshold + (spiritRoot != null && spiritRoot.Current != null ? spiritRoot.Current.freezeThresholdDelta : 0f);

        /// <summary>Heat threshold đã cộng spirit root delta (UI nên đọc giá trị này thay vì raw).</summary>
        public float EffectiveHeatThreshold
            => heatThreshold + (spiritRoot != null && spiritRoot.Current != null ? spiritRoot.Current.heatThresholdDelta : 0f);

        TimeManager timeManager;
        SpiritRoot spiritRoot;
        StatusEffectManager statusManager;
        bool maxHPApplied;
        float baseMaxHP;
        float baseMaxMana;

        void Awake()
        {
            // Init component refs + cache base maxHP ở Awake để ReapplySpiritRootMaxHP gọi từ
            // SaveLoadController.Start (có thể chạy trước PlayerStats.Start) vẫn có dữ liệu hợp lệ.
            spiritRoot = GetComponent<SpiritRoot>();
            statusManager = GetComponent<StatusEffectManager>();
            baseMaxHP = maxHP;
            baseMaxMana = maxMana;
            ServiceLocator.Register<PlayerStats>(this);
        }

        void OnDestroy() => ServiceLocator.Unregister<PlayerStats>(this);

        void Start()
        {
            timeManager = GameManager.Instance != null ? GameManager.Instance.timeManager : ServiceLocator.Get<TimeManager>();
            ApplySpiritRootMaxHP();
        }

        /// <summary>Áp linh căn lên maxHP. Chỉ scale maxHP — không scale HP để tránh
        /// xung đột thứ tự Start với SaveLoadController (SaveLoadController sẽ set HP sau theo save data).</summary>
        public void ApplySpiritRootMaxHP()
        {
            if (maxHPApplied || spiritRoot == null) return;
            float mul = spiritRoot.MaxHPMul;
            if (Mathf.Abs(mul - 1f) > 0.001f)
            {
                float oldMax = maxHP;
                maxHP *= mul;
                // Chỉ scale HP theo nếu nhân vật đang ở full health (fresh game / chưa bị thương).
                // Trường hợp save load đã set HP < oldMax → giữ nguyên, SaveLoadController sẽ phụ trách.
                if (HP >= oldMax - 0.01f) HP = maxHP;
                else HP = Mathf.Min(HP, maxHP);
            }
            maxHPApplied = true;
        }

        /// <summary>Reset & re-apply linh căn lên maxHP. Gọi từ SaveLoadController sau khi SetSpiritRoot,
        /// hoặc khi player đổi linh căn runtime (hiếm gặp).</summary>
        public void ReapplySpiritRootMaxHP()
        {
            maxHPApplied = false;
            if (baseMaxHP > 0f) maxHP = baseMaxHP;
            // Reset maxMana về base — RealmSystem.ReapplyAccumulatedBonuses sẽ cộng lại bonus tier
            // tránh stack double khi LoadAndApply được gọi nhiều lần trong cùng scene.
            if (baseMaxMana > 0f) maxMana = baseMaxMana;
            ApplySpiritRootMaxHP();
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

            UpdateWetness(dt);
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
            // I-frames: bỏ qua mọi dame ngoài (melee/projectile/env). Tick status (TakeDamageRaw)
            // vẫn vào để tránh dodge cancel poison/burn đang stack.
            if (IsInvulnerable) return;
            // Status effect modifier (Burn x1.2…) chỉ áp cho dame ngoài (melee/projectile/env).
            if (statusManager != null) dmg *= statusManager.IncomingDamageMultiplier;
            TakeDamageRaw(dmg);
        }

        /// <summary>Nhận dame KHÔNG nhân IncomingDamageMultiplier (dùng cho tick status để tránh tự khuếch đại).</summary>
        public void TakeDamageRaw(float dmg)
        {
            if (IsDead) return;
            float incoming = dmg;
            if (HasShield)
            {
                float absorbed = Mathf.Min(Shield, dmg);
                Shield -= absorbed;
                dmg -= absorbed;
            }
            else if (Shield > 0f)
            {
                Shield = 0f;
            }
            if (dmg > 0f) HP = Mathf.Max(0f, HP - dmg);
            OnStatsChanged?.Invoke();
            // Bắn event juice (camera shake, damage numbers) — bao gồm cả damage bị shield ăn,
            // để player vẫn thấy phản hồi visible khi đỡ đòn.
            if (incoming > 0f) CombatEvents.RaiseDamage(transform.position, incoming, false);
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
                    if (timeManager.currentWeather == Weather.Rain) t -= 5f;
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

            // Wetness boost cold drift: nếu ambient < BodyTemp, drift về lạnh nhanh hơn theo tier ướt.
            float driftRate = thermalDriftRate;
            if (ambient < BodyTemp)
            {
                float coldMul = WetnessColdDriftMultiplier();
                driftRate *= coldMul;
            }

            BodyTemp = Mathf.Lerp(BodyTemp, ambient, Mathf.Clamp01(driftRate * dt / 100f));

            float effFreezeT = EffectiveFreezeThreshold;
            float effHeatT = EffectiveHeatThreshold;
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

        /// <summary>
        /// Update Wetness: rain/storm cộng, base/fire/shelter/sun trừ. Apply tier SAN penalty
        /// và roll Sickness chain khi Drenched + cold.
        /// </summary>
        void UpdateWetness(float dt)
        {
            if (timeManager == null) return;
            bool sheltered = Shelter.IsSheltered(transform.position);
            float dayLight = timeManager.GetLightIntensity();
            TickWetness(dt, timeManager.currentWeather, sheltered, IsWarm, dayLight, applySanityPenalty: true, applySicknessRoll: true);
        }

        /// <summary>
        /// Pure tick — testable (không phụ thuộc timeManager / Shelter static / Campfire static).
        /// Caller cung cấp weather + sheltered + warm + dayLight (0..1).
        /// applySanityPenalty: trừ SAN theo tier hiện tại.
        /// applySicknessRoll: thử apply Sickness status nếu Drenched + cold.
        /// </summary>
        public void TickWetness(float dt, Weather weather, bool sheltered, bool warm, float dayLight,
            bool applySanityPenalty = true, bool applySicknessRoll = true)
        {
            // Sources: rain/storm chỉ tăng Wetness nếu KHÔNG sheltered.
            float gain = 0f;
            if (!sheltered)
            {
                if (weather == Weather.Rain)
                    gain = wetnessRainPerSec;
                else if (weather == Weather.Storm)
                    gain = wetnessRainPerSec * Mathf.Max(1f, wetnessStormMultiplier);
            }

            // Sinks: base + fire + shelter + nắng (chỉ khi không bị mưa).
            float dry = wetnessDryBasePerSec;
            if (warm) dry += wetnessDryFireBonus;
            if (sheltered) dry += wetnessDryShelterBonus;
            if (gain <= 0f)
            {
                float dayBonus = wetnessDryDayBonus * Mathf.Clamp01(dayLight);
                dry += Mathf.Max(0f, dayBonus);
            }

            float net = gain - dry;
            Wetness = Mathf.Clamp(Wetness + net * dt, 0f, maxWetness);

            var tier = WetnessTierOf(Wetness);

            if (applySanityPenalty)
            {
                // Tier SAN penalty stack: Drenched cũng dính Wet penalty.
                if (tier >= WetnessTier.Wet)
                    Sanity = Mathf.Max(0f, Sanity - wetSanityPenaltyPerSec * dt);
                if (tier == WetnessTier.Drenched)
                    Sanity = Mathf.Max(0f, Sanity - drenchedSanityPenaltyPerSec * dt);
            }

            if (applySicknessRoll)
                TryRollSickness(dt, tier);
        }

        void TryRollSickness(float dt, WetnessTier tier)
        {
            if (tier != WetnessTier.Drenched) return;
            if (sicknessEffect == null || sicknessChancePerSec <= 0f) return;
            float coldT = sicknessColdThreshold >= 0f ? sicknessColdThreshold : comfortMin;
            if (BodyTemp >= coldT) return;
            if (Time.time < nextSicknessAllowedAt) return;

            // > thay >= để: (a) cap saturation 100% khi chance*dt >= 1 (lag spike), (b) tránh
            // edge case Random.value = 1.0 (Unity docs: inclusive [0..1]) làm test với chance=1
            // không deterministic (~1/8M flake).
            float roll = UnityEngine.Random.value;
            if (roll > sicknessChancePerSec * dt) return;

            if (statusManager == null) statusManager = GetComponent<StatusEffectManager>();
            if (statusManager == null) return;

            statusManager.Apply(sicknessEffect);
            nextSicknessAllowedAt = Time.time + Mathf.Max(0f, sicknessApplyCooldownSec);
            Debug.Log($"[Wetness] Drenched + cold → áp Sickness ({sicknessEffect.displayName}).");
        }

        /// <summary>Multiplier vào thermalDriftRate khi ambient lạnh hơn BodyTemp, theo tier ướt.</summary>
        public float WetnessColdDriftMultiplier()
        {
            return WetnessTierOf(Wetness) switch
            {
                WetnessTier.Damp => dampColdDriftMultiplier,
                WetnessTier.Wet => wetColdDriftMultiplier,
                WetnessTier.Drenched => drenchedColdDriftMultiplier,
                _ => 1f,
            };
        }

        /// <summary>Tier hiện tại của Wetness.</summary>
        public WetnessTier CurrentWetnessTier => WetnessTierOf(Wetness);

        /// <summary>Map giá trị wetness → tier.</summary>
        public static WetnessTier WetnessTierOf(float wetness)
        {
            if (wetness >= 80f) return WetnessTier.Drenched;
            if (wetness >= 50f) return WetnessTier.Wet;
            if (wetness >= 20f) return WetnessTier.Damp;
            return WetnessTier.Dry;
        }

        /// <summary>Cộng wetness (vd splash khi uống nước, lội vũng). Clamp 0..max.</summary>
        public void AddWetness(float amount)
        {
            Wetness = Mathf.Clamp(Wetness + amount, 0f, maxWetness);
            OnStatsChanged?.Invoke();
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

        /// <summary>Trừ Sanity (clamp >= 0), fire OnStatsChanged. Dùng cho environmental
        /// SAN drain (Death Lily harvest, ambient night fear) — không phải combat.</summary>
        public void DamageSanity(float amount)
        {
            if (amount <= 0f) return;
            Sanity = Mathf.Max(0f, Sanity - amount);
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
            // Permadeath chỉ chạy auto trong PlayMode — tránh EditMode tests vô tình
            // ghi vào persistentDataPath / reload scene khi assert HP=0. Test có thể
            // gọi <see cref="ExecutePermadeath"/> trực tiếp.
            if (permadeathEnabled && Application.isPlaying) ExecutePermadeath();
        }

        /// <summary>Permadeath sequence: dump inventory → tombstone, record meta stats,
        /// xoá save slot, reload scene với seed mới. Public để test gọi trực tiếp.</summary>
        public void ExecutePermadeath()
        {
            int days = 0;
            int worldSeed = 0;
            var tm = timeManager != null ? timeManager
                   : (GameManager.Instance != null ? GameManager.Instance.timeManager : null);
            if (tm != null) days = tm.daysSurvived;
            var wg = WorldGenerator.Instance;
            if (wg != null) worldSeed = wg.seed;

            int realmTier = 0;
            var realm = GetComponent<RealmSystem>();
            if (realm != null) realmTier = realm.currentTier;

            var inv = GetComponent<Inventory>();
            var entry = new TombstoneData
            {
                worldSeed = worldSeed,
                position = transform.position,
                daySurvived = days,
                previousLifeRealmTier = realmTier,
                previousLifeWasAwakened = IsAwakened,
                items = SnapshotInventoryItems(inv),
            };
            try { Graveyard.Append(entry); }
            catch (Exception e) { Debug.LogError($"[Permadeath] Graveyard.Append thất bại: {e}"); }

            try { MetaStats.RecordDeath(days, realmTier, IsAwakened); }
            catch (Exception e) { Debug.LogError($"[Permadeath] MetaStats.RecordDeath thất bại: {e}"); }

            try { SaveSystem.Delete(); }
            catch (Exception e) { Debug.LogError($"[Permadeath] SaveSystem.Delete thất bại: {e}"); }

            // Reseed world cho run kế tiếp.
            if (wg != null) wg.seed = UnityEngine.Random.Range(1, int.MaxValue);

            if (Application.isPlaying)
            {
                if (deathReloadDelay > 0f) Invoke(nameof(ReloadActiveScene), deathReloadDelay);
                else ReloadActiveScene();
            }
        }

        static List<InventorySlotData> SnapshotInventoryItems(Inventory inv)
        {
            var list = new List<InventorySlotData>();
            if (inv == null) return list;
            foreach (var s in inv.Slots)
            {
                if (s == null || s.IsEmpty || s.item == null) continue;
                list.Add(new InventorySlotData
                {
                    itemId = s.item.itemId,
                    count = s.count,
                    freshRemaining = s.IsPerishable ? s.freshRemaining : -1f,
                    durability = s.IsDurable ? s.durability : -1f,
                });
            }
            return list;
        }

        void ReloadActiveScene()
        {
            GameManager.ResetInstanceForSceneReload();
            var active = SceneManager.GetActiveScene();
            SceneManager.LoadScene(active.buildIndex >= 0 ? active.buildIndex : 0);
        }
    }

    /// <summary>Tier ướt — quyết định cold-drift multiplier + SAN penalty + sickness chain.</summary>
    public enum WetnessTier { Dry, Damp, Wet, Drenched }
}
