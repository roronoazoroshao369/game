using System;
using UnityEngine;
using WildernessCultivation.Combat;
using WildernessCultivation.Core;
using WildernessCultivation.Cultivation;
using WildernessCultivation.Items;
using WildernessCultivation.Player.Stats;
using WildernessCultivation.Player.Status;
using WildernessCultivation.World;

namespace WildernessCultivation.Player
{
    /// <summary>
    /// 5 chỉ số sinh tồn cốt lõi (HP, Đói, Khát, SAN, Linh Khí) + orchestrator cho subsystem
    /// components: <see cref="WetnessComponent"/>, <see cref="ThermalComponent"/>,
    /// <see cref="PermadeathHandler"/>.
    ///
    /// R1 refactor: tách 3 subsystem có logic riêng (Wetness 100+ LoC, Thermal 60+ LoC,
    /// Permadeath 80+ LoC) ra component MonoBehaviour. PlayerStats giữ public surface
    /// (properties + methods façade) để consumer + 100+ test cũ không break.
    ///
    /// Awake auto-add component nếu prefab chưa có — test chỉ cần
    /// <c>go.AddComponent&lt;PlayerStats&gt;()</c> là full subsystem khả dụng.
    /// </summary>
    public class PlayerStats : MonoBehaviour, IDamageable
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

        [Header("Weather effects")]
        [Tooltip("Khi trời mưa + đứng ngoài (không trong shelter/nhà) → refill Thirst chậm.")]
        public float rainThirstRefillPerSec = 0.6f;
        [Tooltip("Khi bão đêm → trừ SAN bonus.")]
        public float stormSanityPenaltyPerSec = 0.4f;
        [Tooltip("Khi đứng ngoài aura sáng vào đêm sâu (deep dark) → trừ SAN bonus.")]
        public float darknessSanityPenaltyPerSec = 0.8f;

        public event Action OnDeath;
        public event Action OnStatsChanged;

        // Fire instance + global hub. R4: GameEvents là entry point cho UI/audio mới — instance
        // event giữ cho code cũ + test subscribe trực tiếp. Mọi field mutation gọi method này
        // thay vì OnStatsChanged?.Invoke() trực tiếp.
        void RaiseStatsChanged()
        {
            OnStatsChanged?.Invoke();
            GameEvents.RaisePlayerStatsChanged();
        }

        public bool IsDead => HP <= 0f;

        [Header("Tu tiên gating")]
        [Tooltip("True = đã khai mở tu tiên. Mặc định Thường Nhân (false). Set qua AwakeningSystem.")]
        public bool IsAwakened = false;

        [Tooltip("Pity counter — số lần roll Phàm liên tiếp. AwakeningSystem cộng dồn mỗi fail, reset 0 khi success bất kỳ grade. Per-run.")]
        public int phamFailStreak = 0;

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

        // ===== Subsystem components (auto-added in Awake) =====

        [Header("Subsystem components (auto-add)")]
        [Tooltip("Wetness subsystem (gauge + tier + sickness chain). Auto-add nếu null.")]
        public WetnessComponent wetness;
        [Tooltip("Thermal subsystem (BodyTemp + ambient + freeze/heat damage). Auto-add nếu null.")]
        public ThermalComponent thermal;
        [Tooltip("Permadeath subsystem (tombstone + save wipe + reload). Auto-add nếu null.")]
        public PermadeathHandler permadeath;

        TimeManager timeManager;
        SpiritRoot spiritRoot;
        StatusEffectManager statusManager;
        bool maxHPApplied;
        float baseMaxHP;
        float baseMaxMana;

        void Awake()
        {
            spiritRoot = GetComponent<SpiritRoot>();
            statusManager = GetComponent<StatusEffectManager>();
            baseMaxHP = maxHP;
            baseMaxMana = maxMana;

            if (wetness == null) wetness = GetComponent<WetnessComponent>() ?? gameObject.AddComponent<WetnessComponent>();
            if (thermal == null) thermal = GetComponent<ThermalComponent>() ?? gameObject.AddComponent<ThermalComponent>();
            if (permadeath == null) permadeath = GetComponent<PermadeathHandler>() ?? gameObject.AddComponent<PermadeathHandler>();

            // R4 GameEvents bridge: Inventory KHÔNG biết nó thuộc player hay chest, nên
            // PlayerStats (marker singleton) hook OnInventoryChanged → GameEvents.RaisePlayerInventoryChanged.
            // Chest inventory không có PlayerStats kế bên ⇒ không bắc cầu.
            playerInventory = GetComponent<Inventory>();
            if (playerInventory != null) playerInventory.OnInventoryChanged += BridgeInventoryToGameEvents;

            ServiceLocator.Register<PlayerStats>(this);
        }

        void OnDestroy()
        {
            if (playerInventory != null) playerInventory.OnInventoryChanged -= BridgeInventoryToGameEvents;
            ServiceLocator.Unregister<PlayerStats>(this);
        }

        Inventory playerInventory;
        void BridgeInventoryToGameEvents() => GameEvents.RaisePlayerInventoryChanged();

        void Start()
        {
            timeManager = GameManager.Instance != null ? GameManager.Instance.timeManager : ServiceLocator.Get<TimeManager>();
            ApplySpiritRootMaxHP();
        }

        // ===== Spirit root maxHP =====

        /// <summary>Áp linh căn lên maxHP. Chỉ scale maxHP — không scale HP để tránh
        /// xung đột thứ tự Start với SaveLoadController.</summary>
        public void ApplySpiritRootMaxHP()
        {
            if (maxHPApplied || spiritRoot == null) return;
            float mul = spiritRoot.MaxHPMul;
            if (Mathf.Abs(mul - 1f) > 0.001f)
            {
                float oldMax = maxHP;
                maxHP *= mul;
                if (HP >= oldMax - 0.01f) HP = maxHP;
                else HP = Mathf.Min(HP, maxHP);
            }
            maxHPApplied = true;
        }

        /// <summary>Reset & re-apply linh căn lên maxHP. Gọi từ SaveLoadController.</summary>
        public void ReapplySpiritRootMaxHP()
        {
            maxHPApplied = false;
            if (baseMaxHP > 0f) maxHP = baseMaxHP;
            if (baseMaxMana > 0f) maxMana = baseMaxMana;
            ApplySpiritRootMaxHP();
        }

        // ===== Update orchestrator =====

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
            if (thermal != null) thermal.Tick(dt);
            UpdateWeatherEffects(dt);
            UpdateDarkness(dt);

            if (Hunger <= 0f) HP = Mathf.Max(0f, HP - starveDamagePerSec * dt);
            if (Thirst <= 0f) HP = Mathf.Max(0f, HP - dehydrateDamagePerSec * dt);

            Mana = Mathf.Min(maxMana, Mana + manaRegenIdle * dt);

            RaiseStatsChanged();

            if (HP <= 0f) Die();
        }

        // ===== Damage / heal API =====

        public void TakeDamage(float dmg)
        {
            // I-frames: bỏ qua mọi dame ngoài (melee/projectile/env). Tick status (TakeDamageRaw)
            // vẫn vào để tránh dodge cancel poison/burn đang stack.
            if (IsInvulnerable) return;
            // Status effect modifier (Burn x1.2…) chỉ áp cho dame ngoài.
            if (statusManager != null) dmg *= statusManager.IncomingDamageMultiplier;
            TakeDamageRaw(dmg);
        }

        /// <summary>
        /// IDamageable entry — mob/projectile/env gọi qua interface, đỡ phải fallback
        /// <c>GetComponent&lt;PlayerStats&gt;()?.TakeDamage(...)</c>. Giữ nguyên i-frame +
        /// status modifier như overload <see cref="TakeDamage(float)"/>; <paramref name="source"/>
        /// reserved cho threat/aggro/log sau này.
        /// </summary>
        public void TakeDamage(float amount, GameObject source) => TakeDamage(amount);

        /// <summary>Nhận dame KHÔNG nhân IncomingDamageMultiplier (dùng cho tick status).</summary>
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
            RaiseStatsChanged();
            // Bắn event juice (camera shake, damage numbers) — bao gồm cả damage bị shield ăn.
            if (incoming > 0f) CombatEvents.RaiseDamage(transform.position, incoming, false);
            if (HP <= 0f) Die();
        }

        /// <summary>Tạo / cộng dồn shield. Lấy max(durationSec) để không bị shield mới ngắn hơn ghi đè cũ dài hơn.</summary>
        public void AddShield(float amount, float durationSec)
        {
            if (amount <= 0f || durationSec <= 0f) return;
            Shield = Mathf.Max(Shield, 0f) + amount;
            ShieldEndsAt = Mathf.Max(ShieldEndsAt, Time.time + durationSec);
            RaiseStatsChanged();
        }

        /// <summary>Set i-frames trong duration giây tính từ thời điểm gọi.</summary>
        public void SetInvulnerable(float duration)
        {
            float end = Time.time + Mathf.Max(0f, duration);
            if (end > InvulnerableUntil) InvulnerableUntil = end;
        }

        public void Heal(float amount)
        {
            HP = Mathf.Min(maxHP, HP + amount);
            RaiseStatsChanged();
        }

        public void Eat(float foodValue)
        {
            Hunger = Mathf.Min(maxHunger, Hunger + foodValue);
            RaiseStatsChanged();
        }

        public void Drink(float waterValue)
        {
            Thirst = Mathf.Min(maxThirst, Thirst + waterValue);
            RaiseStatsChanged();
        }

        public void RestoreSanity(float amount)
        {
            Sanity = Mathf.Min(maxSanity, Sanity + amount);
            RaiseStatsChanged();
        }

        /// <summary>Trừ Sanity (clamp >= 0), fire OnStatsChanged. Dùng cho environmental SAN drain.</summary>
        public void DamageSanity(float amount)
        {
            if (amount <= 0f) return;
            Sanity = Mathf.Max(0f, Sanity - amount);
            RaiseStatsChanged();
        }

        public bool TryConsumeMana(float cost)
        {
            if (Mana < cost) return false;
            Mana -= cost;
            RaiseStatsChanged();
            return true;
        }

        public void AddMana(float amount)
        {
            Mana = Mathf.Min(maxMana, Mana + amount);
            RaiseStatsChanged();
        }

        // ===== Wetness façade (delegate to WetnessComponent) =====

        /// <summary>Wetness gauge [0..maxWetness]. R1: storage trên WetnessComponent.</summary>
        public float Wetness
        {
            get => wetness != null ? wetness.Wetness : 0f;
            set { if (wetness != null) wetness.Wetness = value; }
        }
        public float maxWetness
        {
            get => wetness != null ? wetness.maxWetness : 100f;
            set { if (wetness != null) wetness.maxWetness = value; }
        }
        public float wetnessRainPerSec => wetness != null ? wetness.wetnessRainPerSec : 0f;
        public float wetnessStormMultiplier => wetness != null ? wetness.wetnessStormMultiplier : 1f;
        public float wetnessDryBasePerSec => wetness != null ? wetness.wetnessDryBasePerSec : 0f;
        public float wetnessDryFireBonus => wetness != null ? wetness.wetnessDryFireBonus : 0f;
        public float wetnessDryShelterBonus => wetness != null ? wetness.wetnessDryShelterBonus : 0f;
        public float wetnessDryDayBonus => wetness != null ? wetness.wetnessDryDayBonus : 0f;
        public float dampColdDriftMultiplier => wetness != null ? wetness.dampColdDriftMultiplier : 1f;
        public float wetColdDriftMultiplier => wetness != null ? wetness.wetColdDriftMultiplier : 1f;
        public float drenchedColdDriftMultiplier => wetness != null ? wetness.drenchedColdDriftMultiplier : 1f;
        public float wetSanityPenaltyPerSec => wetness != null ? wetness.wetSanityPenaltyPerSec : 0f;
        public float drenchedSanityPenaltyPerSec => wetness != null ? wetness.drenchedSanityPenaltyPerSec : 0f;
        public WetnessTier CurrentWetnessTier => wetness != null ? wetness.CurrentTier : WetnessTier.Dry;

        // Sickness chain knobs (test mutate qua façade) — set-through tới WetnessComponent.
        public float sicknessChancePerSec
        {
            get => wetness != null ? wetness.sicknessChancePerSec : 0f;
            set { if (wetness != null) wetness.sicknessChancePerSec = value; }
        }
        public StatusEffectSO sicknessEffect
        {
            get => wetness != null ? wetness.sicknessEffect : null;
            set { if (wetness != null) wetness.sicknessEffect = value; }
        }
        public float sicknessColdThreshold
        {
            get => wetness != null ? wetness.sicknessColdThreshold : -1f;
            set { if (wetness != null) wetness.sicknessColdThreshold = value; }
        }
        public float sicknessApplyCooldownSec
        {
            get => wetness != null ? wetness.sicknessApplyCooldownSec : 0f;
            set { if (wetness != null) wetness.sicknessApplyCooldownSec = value; }
        }

        /// <summary>Map giá trị wetness → tier (static façade).</summary>
        public static WetnessTier WetnessTierOf(float wetness) => WetnessComponent.TierOf(wetness);

        /// <summary>Multiplier vào thermalDriftRate khi ambient lạnh hơn BodyTemp, theo tier ướt.</summary>
        public float WetnessColdDriftMultiplier() => wetness != null ? wetness.ColdDriftMultiplier() : 1f;

        /// <summary>Cộng wetness (vd splash khi uống nước, lội vũng). Clamp 0..max.</summary>
        public void AddWetness(float amount)
        {
            if (wetness == null) return;
            wetness.Add(amount);
            RaiseStatsChanged();
        }

        /// <summary>
        /// Pure tick — testable façade. Forwards to <see cref="WetnessComponent.Tick"/>.
        /// </summary>
        public void TickWetness(float dt, Weather weather, bool sheltered, bool warm, float dayLight,
            bool applySanityPenalty = true, bool applySicknessRoll = true)
        {
            if (wetness == null) return;
            wetness.Tick(dt, weather, sheltered, warm, dayLight, applySanityPenalty, applySicknessRoll);
        }

        void UpdateWetness(float dt)
        {
            if (timeManager == null || wetness == null) return;
            bool sheltered = Shelter.IsSheltered(transform.position);
            float dayLight = timeManager.GetLightIntensity();
            wetness.Tick(dt, timeManager.currentWeather, sheltered, IsWarm, dayLight,
                applySanityPenalty: true, applySicknessRoll: true);
        }

        // ===== Thermal façade (delegate to ThermalComponent) =====

        public float BodyTemp
        {
            get => thermal != null ? thermal.BodyTemp : 50f;
            set { if (thermal != null) thermal.BodyTemp = value; }
        }
        public float comfortMin => thermal != null ? thermal.comfortMin : 30f;
        public float comfortMax => thermal != null ? thermal.comfortMax : 70f;
        public float thermalDriftRate => thermal != null ? thermal.thermalDriftRate : 4f;
        public float freezeThreshold => thermal != null ? thermal.freezeThreshold : 10f;
        public float freezeDamagePerSec => thermal != null ? thermal.freezeDamagePerSec : 1.5f;
        public float heatThreshold => thermal != null ? thermal.heatThreshold : 90f;
        public float heatThirstMult => thermal != null ? thermal.heatThirstMult : 2.5f;
        public float heatSanityPenaltyPerSec => thermal != null ? thermal.heatSanityPenaltyPerSec : 0.5f;
        public float EffectiveFreezeThreshold => thermal != null ? thermal.EffectiveFreezeThreshold : 10f;
        public float EffectiveHeatThreshold => thermal != null ? thermal.EffectiveHeatThreshold : 90f;
        public float ComputeAmbientTemperature() => thermal != null ? thermal.ComputeAmbientTemperature() : 50f;

        // ===== Permadeath façade =====

        public bool permadeathEnabled
        {
            get => permadeath != null && permadeath.permadeathEnabled;
            set { if (permadeath != null) permadeath.permadeathEnabled = value; }
        }
        public float deathReloadDelay
        {
            get => permadeath != null ? permadeath.deathReloadDelay : 0f;
            set { if (permadeath != null) permadeath.deathReloadDelay = value; }
        }

        /// <summary>Permadeath sequence façade. Forwards to <see cref="PermadeathHandler.Execute"/>.</summary>
        public void ExecutePermadeath()
        {
            if (permadeath != null) permadeath.Execute();
        }

        // ===== Weather / darkness ticks (small enough to keep on PlayerStats) =====

        void UpdateWeatherEffects(float dt)
        {
            if (timeManager == null) return;
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

        // ===== Death =====

        void Die()
        {
            OnDeath?.Invoke();
            GameEvents.RaisePlayerDied();
            Debug.Log("[Player] Died.");
            // Permadeath chỉ chạy auto trong PlayMode — tránh EditMode tests vô tình
            // ghi vào persistentDataPath / reload scene khi assert HP=0. Test gọi
            // ExecutePermadeath trực tiếp nếu cần.
            if (permadeathEnabled && Application.isPlaying) ExecutePermadeath();
        }
    }

    /// <summary>Tier ướt — quyết định cold-drift multiplier + SAN penalty + sickness chain.</summary>
    public enum WetnessTier { Dry, Damp, Wet, Drenched }
}
