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
    /// Façade cho player stat subsystems. State + logic chi tiết nằm trên các MonoBehaviour
    /// component con (<see cref="HealthComponent"/>, <see cref="HungerComponent"/>, …).
    /// PlayerStats chỉ giữ:
    /// 1. Public properties + methods (façade API) — consumer + test cũ không break.
    /// 2. Orchestrator <see cref="Update"/> gọi Tick từng component đúng thứ tự.
    /// 3. Cross-concern lifecycle: spirit root scale maxHP, GameEvents bridge, Die + Permadeath.
    ///
    /// R1 phase 1 tách: <see cref="WetnessComponent"/>, <see cref="ThermalComponent"/>,
    /// <see cref="PermadeathHandler"/>.
    /// R1 phase 2 tách: <see cref="HealthComponent"/>, <see cref="HungerComponent"/>,
    /// <see cref="ThirstComponent"/>, <see cref="SanityComponent"/>, <see cref="ManaComponent"/>,
    /// <see cref="ShieldComponent"/>, <see cref="InvulnerabilityComponent"/>.
    ///
    /// Lazy auto-add: bất kỳ property nào chạm tới component sẽ auto-add component nếu prefab
    /// chưa có — test chỉ cần <c>go.AddComponent&lt;PlayerStats&gt;()</c> mà không cần wiring.
    /// Field initializer trên mỗi component cung cấp default values giống PlayerStats cũ.
    /// </summary>
    public class PlayerStats : CharacterBase, ISaveable
    {
        public event Action OnDeath;
        public event Action OnStatsChanged;

        // ===== ISaveable =====

        /// <summary>R6 ISaveable: order 30 — sau RealmSystem(10)+SpiritRoot(20), trước Inventory(60).</summary>
        public string SaveKey => "Player/Vitals";
        public int Order => 30;

        // ===== Subsystem components (auto-added in Awake + lazy on property access) =====

        [Header("Core stat components (auto-add)")]
        public HealthComponent health;
        public HungerComponent hunger;
        public ThirstComponent thirst;
        public SanityComponent sanity;
        public ManaComponent mana;
        public ShieldComponent shieldComp;
        public InvulnerabilityComponent invuln;

        [Header("Environmental stat components (auto-add)")]
        public WetnessComponent wetness;
        public ThermalComponent thermal;
        public PermadeathHandler permadeath;

        [Header("Tu tiên gating")]
        [Tooltip("True = đã khai mở tu tiên. Mặc định Thường Nhân (false). Set qua AwakeningSystem.")]
        public bool IsAwakened = false;

        [Tooltip("Pity counter — số lần roll Phàm liên tiếp. AwakeningSystem cộng dồn mỗi fail, reset 0 khi success bất kỳ grade. Per-run.")]
        public int phamFailStreak = 0;

        TimeManager timeManager;
        SpiritRoot spiritRoot;
        StatusEffectManager statusManager;
        Inventory playerInventory;
        bool maxHPApplied;
        float baseMaxHP;
        float baseMaxMana;

        // ===== Awake / lifecycle =====

        void Awake()
        {
            spiritRoot = GetComponent<SpiritRoot>();
            statusManager = GetComponent<StatusEffectManager>();

            EnsureComponent(ref health);
            EnsureComponent(ref hunger);
            EnsureComponent(ref thirst);
            EnsureComponent(ref sanity);
            EnsureComponent(ref mana);
            EnsureComponent(ref shieldComp);
            EnsureComponent(ref invuln);
            EnsureComponent(ref wetness);
            EnsureComponent(ref thermal);
            EnsureComponent(ref permadeath);

            baseMaxHP = health.maxHP;
            baseMaxMana = mana.maxMana;

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

        void OnEnable()
        {
            // R6: register với SaveRegistry. Cross-system fixup cần chạy SAU khi
            // SpiritRoot.SetSpiritRoot (order 20) áp catalog + trước RealmSystem
            // ReapplyAccumulatedBonuses (50) add tier bonus + cuối cùng clamp HP/Mana (60).
            SaveRegistry.RegisterSaveable(this);
            SaveRegistry.RegisterFixup(this, 30, _ => ReapplySpiritRootMaxHP());
            SaveRegistry.RegisterFixup(this, 60, data =>
            {
                if (data?.player == null) return;
                HP = Mathf.Min(data.player.hp, maxHP);
                Mana = Mathf.Min(data.player.mana, maxMana);
            });
        }

        void OnDisable()
        {
            SaveRegistry.UnregisterSaveable(this);
            SaveRegistry.UnregisterFixupsFor(this);
        }

        public void CaptureState(SaveData data)
        {
            if (data == null) return;
            data.player ??= new PlayerSaveData();
            data.player.position = transform.position;
            data.player.hp = HP;
            data.player.hunger = Hunger;
            data.player.thirst = Thirst;
            data.player.sanity = Sanity;
            data.player.mana = Mana;
            data.player.bodyTemp = BodyTemp;
            data.player.isAwakened = IsAwakened;
            data.player.phamFailStreak = phamFailStreak;
            data.player.wetness = Wetness;
        }

        public void RestoreState(SaveData data)
        {
            if (data?.player == null) return;
            transform.position = data.player.position;
            // HP/Mana KHÔNG clamp ở đây — clamp phase chạy sau RealmSystem.ReapplyAccumulatedBonuses
            // (fixup order 60) khi maxHP/maxMana đã có đủ spirit root scale + tier bonus.
            HP = data.player.hp;
            Hunger = data.player.hunger;
            Thirst = data.player.thirst;
            Sanity = data.player.sanity;
            Mana = data.player.mana;
            BodyTemp = data.player.bodyTemp <= 0f ? 50f : data.player.bodyTemp;
            IsAwakened = data.player.isAwakened;
            phamFailStreak = data.player.phamFailStreak;
            Wetness = Mathf.Clamp(data.player.wetness, 0f, maxWetness);
        }

        void BridgeInventoryToGameEvents() => GameEvents.RaisePlayerInventoryChanged();

        void Start()
        {
            timeManager = GameManager.Instance != null ? GameManager.Instance.timeManager : ServiceLocator.Get<TimeManager>();
            ApplySpiritRootMaxHP();
        }

        /// <summary>
        /// Resolve existing component hoặc add mới. Safe gọi từ Awake + từ property getter
        /// (EditMode AddComponent không tự fire Awake → property access đầu tiên lazy-resolve).
        /// </summary>
        T EnsureComponent<T>(ref T cache) where T : MonoBehaviour
        {
            if (cache != null) return cache;
            cache = GetComponent<T>();
            if (cache == null) cache = gameObject.AddComponent<T>();
            return cache;
        }

        // ===== Façade: Health =====

        public float HP
        {
            get => EnsureComponent(ref health).HP;
            set => EnsureComponent(ref health).HP = value;
        }
        public float maxHP
        {
            get => EnsureComponent(ref health).maxHP;
            set => EnsureComponent(ref health).maxHP = value;
        }
        public float starveDamagePerSec
        {
            get => EnsureComponent(ref health).starveDamagePerSec;
            set => EnsureComponent(ref health).starveDamagePerSec = value;
        }
        public float dehydrateDamagePerSec
        {
            get => EnsureComponent(ref health).dehydrateDamagePerSec;
            set => EnsureComponent(ref health).dehydrateDamagePerSec = value;
        }

        public override bool IsDead => HP <= 0f;
        public override float CurrentHP => HP;
        public override float CurrentMaxHP => maxHP;

        // ===== Façade: Hunger =====

        public float Hunger
        {
            get => EnsureComponent(ref hunger).Hunger;
            set => EnsureComponent(ref hunger).Hunger = value;
        }
        public float maxHunger
        {
            get => EnsureComponent(ref hunger).maxHunger;
            set => EnsureComponent(ref hunger).maxHunger = value;
        }
        public float hungerDecay
        {
            get => EnsureComponent(ref hunger).hungerDecay;
            set => EnsureComponent(ref hunger).hungerDecay = value;
        }

        // ===== Façade: Thirst =====

        public float Thirst
        {
            get => EnsureComponent(ref thirst).Thirst;
            set => EnsureComponent(ref thirst).Thirst = value;
        }
        public float maxThirst
        {
            get => EnsureComponent(ref thirst).maxThirst;
            set => EnsureComponent(ref thirst).maxThirst = value;
        }
        public float thirstDecay
        {
            get => EnsureComponent(ref thirst).thirstDecay;
            set => EnsureComponent(ref thirst).thirstDecay = value;
        }
        public float rainThirstRefillPerSec
        {
            get => EnsureComponent(ref thirst).rainThirstRefillPerSec;
            set => EnsureComponent(ref thirst).rainThirstRefillPerSec = value;
        }

        // ===== Façade: Sanity =====

        public float Sanity
        {
            get => EnsureComponent(ref sanity).Sanity;
            set => EnsureComponent(ref sanity).Sanity = value;
        }
        public float maxSanity
        {
            get => EnsureComponent(ref sanity).maxSanity;
            set => EnsureComponent(ref sanity).maxSanity = value;
        }
        public float sanityNightDecay
        {
            get => EnsureComponent(ref sanity).sanityNightDecay;
            set => EnsureComponent(ref sanity).sanityNightDecay = value;
        }
        public float stormSanityPenaltyPerSec
        {
            get => EnsureComponent(ref sanity).stormSanityPenaltyPerSec;
            set => EnsureComponent(ref sanity).stormSanityPenaltyPerSec = value;
        }
        public float darknessSanityPenaltyPerSec
        {
            get => EnsureComponent(ref sanity).darknessSanityPenaltyPerSec;
            set => EnsureComponent(ref sanity).darknessSanityPenaltyPerSec = value;
        }

        // ===== Façade: Mana =====

        public float Mana
        {
            get => EnsureComponent(ref mana).Mana;
            set => EnsureComponent(ref mana).Mana = value;
        }
        public float maxMana
        {
            get => EnsureComponent(ref mana).maxMana;
            set => EnsureComponent(ref mana).maxMana = value;
        }
        public float manaRegenIdle
        {
            get => EnsureComponent(ref mana).manaRegenIdle;
            set => EnsureComponent(ref mana).manaRegenIdle = value;
        }

        // ===== Façade: Shield =====

        public float Shield
        {
            get => EnsureComponent(ref shieldComp).Shield;
            set => EnsureComponent(ref shieldComp).Shield = value;
        }
        public float ShieldEndsAt
        {
            get => EnsureComponent(ref shieldComp).ShieldEndsAt;
            set => EnsureComponent(ref shieldComp).ShieldEndsAt = value;
        }
        public bool HasShield => EnsureComponent(ref shieldComp).HasShield;

        // ===== Façade: Invulnerability =====

        public float InvulnerableUntil
        {
            get => EnsureComponent(ref invuln).InvulnerableUntil;
            set => EnsureComponent(ref invuln).InvulnerableUntil = value;
        }
        public bool IsInvulnerable => EnsureComponent(ref invuln).IsInvulnerable;

        /// <summary>True nếu player đang trong aura của 1 <see cref="Campfire"/> đang cháy.</summary>
        public bool IsWarm => Campfire.FindWarmthAt(transform.position) != null;

        // ===== Spirit root maxHP =====

        /// <summary>Áp linh căn lên maxHP. Chỉ scale maxHP — không scale HP để tránh
        /// xung đột thứ tự Start với SaveLoadController.</summary>
        public void ApplySpiritRootMaxHP()
        {
            if (maxHPApplied || spiritRoot == null) return;
            float mul = spiritRoot.MaxHPMul;
            if (Mathf.Abs(mul - 1f) > 0.001f)
            {
                float oldMax = health.maxHP;
                health.maxHP *= mul;
                if (health.HP >= oldMax - 0.01f) health.HP = health.maxHP;
                else health.HP = Mathf.Min(health.HP, health.maxHP);
            }
            maxHPApplied = true;
        }

        /// <summary>Reset & re-apply linh căn lên maxHP. Gọi từ SaveLoadController.</summary>
        public void ReapplySpiritRootMaxHP()
        {
            maxHPApplied = false;
            if (baseMaxHP > 0f) health.maxHP = baseMaxHP;
            if (baseMaxMana > 0f) mana.maxMana = baseMaxMana;
            ApplySpiritRootMaxHP();
        }

        // ===== Fire instance + global event =====

        // R4: GameEvents là entry point cho UI/audio mới — instance event giữ cho code cũ +
        // test subscribe trực tiếp. Mọi field mutation gọi method này thay vì OnStatsChanged?.Invoke().
        void RaiseStatsChanged()
        {
            OnStatsChanged?.Invoke();
            GameEvents.RaisePlayerStatsChanged();
        }

        // ===== Update orchestrator =====

        void Update()
        {
            if (IsDead) return;
            float dt = Time.deltaTime;

            float hungerMul = spiritRoot != null ? spiritRoot.HungerDecayMul : 1f;
            float thirstMul = spiritRoot != null ? spiritRoot.ThirstDecayMul : 1f;
            float sanityMul = spiritRoot != null ? spiritRoot.SanityDecayMul : 1f;

            hunger.Tick(dt, hungerMul);
            thirst.Tick(dt, thirstMul);

            if (timeManager != null && timeManager.isNight && !IsWarm)
                sanity.TickNightDecay(dt, sanityMul);

            // Biome ambient SAN damage (vd Hoang Mạc Tử Khí về đêm). Lửa trại không chống được.
            if (timeManager != null && timeManager.isNight && WorldGenerator.Instance != null)
            {
                var biome = WorldGenerator.Instance.BiomeAt(transform.position);
                if (biome != null && biome.ambientNightSanDamage > 0f)
                    sanity.Damage(biome.ambientNightSanDamage * dt);
            }

            UpdateWetness(dt);
            if (thermal != null) thermal.Tick(dt);
            UpdateWeatherEffects(dt);
            UpdateDarkness(dt);

            health.TickStarvation(dt, hunger.IsStarving, thirst.IsDehydrated);
            mana.TickRegen(dt);

            RaiseStatsChanged();

            if (health.IsDead) Die();
        }

        // ===== Damage / heal API =====

        public void TakeDamage(float dmg)
        {
            // I-frames: bỏ qua mọi dame ngoài (melee/projectile/env). Tick status (TakeDamageRaw)
            // vẫn vào để tránh dodge cancel poison/burn đang stack.
            if (invuln != null && invuln.IsInvulnerable) return;
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
        public override void TakeDamage(float amount, GameObject source) => TakeDamage(amount);

        /// <summary>Nhận dame KHÔNG nhân IncomingDamageMultiplier (dùng cho tick status).</summary>
        public void TakeDamageRaw(float dmg)
        {
            EnsureComponent(ref health);
            EnsureComponent(ref shieldComp);
            if (health.IsDead) return;
            float incoming = dmg;
            float remaining = shieldComp.Absorb(dmg);
            if (remaining > 0f) health.TakeRaw(remaining);
            RaiseStatsChanged();
            // Bắn event juice (camera shake, damage numbers) — bao gồm cả damage bị shield ăn.
            if (incoming > 0f) CombatEvents.RaiseDamage(transform.position, incoming, false);
            if (health.IsDead) Die();
        }

        /// <summary>Tạo / cộng dồn shield. Lấy max(durationSec) để không bị shield mới ngắn hơn ghi đè cũ dài hơn.</summary>
        public void AddShield(float amount, float durationSec)
        {
            EnsureComponent(ref shieldComp).Add(amount, durationSec);
            if (amount > 0f && durationSec > 0f) RaiseStatsChanged();
        }

        /// <summary>Set i-frames trong duration giây tính từ thời điểm gọi.</summary>
        public void SetInvulnerable(float duration)
        {
            EnsureComponent(ref invuln).Set(duration);
        }

        public void Heal(float amount)
        {
            EnsureComponent(ref health).Heal(amount);
            RaiseStatsChanged();
        }

        public void Eat(float foodValue)
        {
            EnsureComponent(ref hunger).Eat(foodValue);
            RaiseStatsChanged();
        }

        public void Drink(float waterValue)
        {
            EnsureComponent(ref thirst).Drink(waterValue);
            RaiseStatsChanged();
        }

        public void RestoreSanity(float amount)
        {
            EnsureComponent(ref sanity).Restore(amount);
            RaiseStatsChanged();
        }

        /// <summary>Trừ Sanity (clamp >= 0), fire OnStatsChanged. Dùng cho environmental SAN drain.</summary>
        public void DamageSanity(float amount)
        {
            if (amount <= 0f) return;
            EnsureComponent(ref sanity).Damage(amount);
            RaiseStatsChanged();
        }

        public bool TryConsumeMana(float cost)
        {
            if (!EnsureComponent(ref mana).TryConsume(cost)) return false;
            RaiseStatsChanged();
            return true;
        }

        public void AddMana(float amount)
        {
            EnsureComponent(ref mana).Add(amount);
            RaiseStatsChanged();
        }

        // ===== Wetness façade (delegate to WetnessComponent) =====

        /// <summary>Wetness gauge [0..maxWetness]. R1: storage trên WetnessComponent.</summary>
        public float Wetness
        {
            get => EnsureComponent(ref wetness).Wetness;
            set => EnsureComponent(ref wetness).Wetness = value;
        }
        public float maxWetness
        {
            get => EnsureComponent(ref wetness).maxWetness;
            set => EnsureComponent(ref wetness).maxWetness = value;
        }
        public float wetnessRainPerSec => EnsureComponent(ref wetness).wetnessRainPerSec;
        public float wetnessStormMultiplier => EnsureComponent(ref wetness).wetnessStormMultiplier;
        public float wetnessDryBasePerSec => EnsureComponent(ref wetness).wetnessDryBasePerSec;
        public float wetnessDryFireBonus => EnsureComponent(ref wetness).wetnessDryFireBonus;
        public float wetnessDryShelterBonus => EnsureComponent(ref wetness).wetnessDryShelterBonus;
        public float wetnessDryDayBonus => EnsureComponent(ref wetness).wetnessDryDayBonus;
        public float dampColdDriftMultiplier => EnsureComponent(ref wetness).dampColdDriftMultiplier;
        public float wetColdDriftMultiplier => EnsureComponent(ref wetness).wetColdDriftMultiplier;
        public float drenchedColdDriftMultiplier => EnsureComponent(ref wetness).drenchedColdDriftMultiplier;
        public float wetSanityPenaltyPerSec => EnsureComponent(ref wetness).wetSanityPenaltyPerSec;
        public float drenchedSanityPenaltyPerSec => EnsureComponent(ref wetness).drenchedSanityPenaltyPerSec;
        public WetnessTier CurrentWetnessTier => EnsureComponent(ref wetness).CurrentTier;

        // Sickness chain knobs (test mutate qua façade) — set-through tới WetnessComponent.
        public float sicknessChancePerSec
        {
            get => EnsureComponent(ref wetness).sicknessChancePerSec;
            set => EnsureComponent(ref wetness).sicknessChancePerSec = value;
        }
        public StatusEffectSO sicknessEffect
        {
            get => EnsureComponent(ref wetness).sicknessEffect;
            set => EnsureComponent(ref wetness).sicknessEffect = value;
        }
        public float sicknessColdThreshold
        {
            get => EnsureComponent(ref wetness).sicknessColdThreshold;
            set => EnsureComponent(ref wetness).sicknessColdThreshold = value;
        }
        public float sicknessApplyCooldownSec
        {
            get => EnsureComponent(ref wetness).sicknessApplyCooldownSec;
            set => EnsureComponent(ref wetness).sicknessApplyCooldownSec = value;
        }

        /// <summary>Map giá trị wetness → tier (static façade).</summary>
        public static WetnessTier WetnessTierOf(float wetness) => WetnessComponent.TierOf(wetness);

        /// <summary>Multiplier vào thermalDriftRate khi ambient lạnh hơn BodyTemp, theo tier ướt.</summary>
        public float WetnessColdDriftMultiplier() => EnsureComponent(ref wetness).ColdDriftMultiplier();

        /// <summary>Cộng wetness (vd splash khi uống nước, lội vũng). Clamp 0..max.</summary>
        public void AddWetness(float amount)
        {
            EnsureComponent(ref wetness).Add(amount);
            RaiseStatsChanged();
        }

        /// <summary>
        /// Pure tick — testable façade. Forwards to <see cref="WetnessComponent.Tick"/>.
        /// </summary>
        public void TickWetness(float dt, Weather weather, bool sheltered, bool warm, float dayLight,
            bool applySanityPenalty = true, bool applySicknessRoll = true)
        {
            EnsureComponent(ref wetness).Tick(dt, weather, sheltered, warm, dayLight, applySanityPenalty, applySicknessRoll);
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
            get => EnsureComponent(ref thermal).BodyTemp;
            set => EnsureComponent(ref thermal).BodyTemp = value;
        }
        public float comfortMin => EnsureComponent(ref thermal).comfortMin;
        public float comfortMax => EnsureComponent(ref thermal).comfortMax;
        public float thermalDriftRate => EnsureComponent(ref thermal).thermalDriftRate;
        public float freezeThreshold => EnsureComponent(ref thermal).freezeThreshold;
        public float freezeDamagePerSec => EnsureComponent(ref thermal).freezeDamagePerSec;
        public float heatThreshold => EnsureComponent(ref thermal).heatThreshold;
        public float heatThirstMult => EnsureComponent(ref thermal).heatThirstMult;
        public float heatSanityPenaltyPerSec => EnsureComponent(ref thermal).heatSanityPenaltyPerSec;
        public float EffectiveFreezeThreshold => EnsureComponent(ref thermal).EffectiveFreezeThreshold;
        public float EffectiveHeatThreshold => EnsureComponent(ref thermal).EffectiveHeatThreshold;
        public float ComputeAmbientTemperature() => EnsureComponent(ref thermal).ComputeAmbientTemperature();

        // ===== Permadeath façade =====

        public bool permadeathEnabled
        {
            get => EnsureComponent(ref permadeath).permadeathEnabled;
            set => EnsureComponent(ref permadeath).permadeathEnabled = value;
        }
        public float deathReloadDelay
        {
            get => EnsureComponent(ref permadeath).deathReloadDelay;
            set => EnsureComponent(ref permadeath).deathReloadDelay = value;
        }

        /// <summary>Permadeath sequence façade. Forwards to <see cref="PermadeathHandler.Execute"/>.</summary>
        public void ExecutePermadeath()
        {
            EnsureComponent(ref permadeath).Execute();
        }

        // ===== Weather / darkness ticks =====

        void UpdateWeatherEffects(float dt)
        {
            if (timeManager == null) return;
            if (Shelter.IsSheltered(transform.position)) return;
            switch (timeManager.currentWeather)
            {
                case Weather.Rain:
                    thirst.RefillFromRain(dt);
                    break;
                case Weather.Storm:
                    thirst.RefillFromRain(dt);
                    if (timeManager.isNight)
                        sanity.ApplyStormPenalty(dt);
                    break;
            }
        }

        void UpdateDarkness(float dt)
        {
            if (timeManager == null || !timeManager.isNight) return;
            if (LightSource.AnyLightAt(transform.position)) return;
            // Đêm + ngoài tất cả nguồn sáng → "deep dark"
            sanity.ApplyDarknessPenalty(dt);
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
