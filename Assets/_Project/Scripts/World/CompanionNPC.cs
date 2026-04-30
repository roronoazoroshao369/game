using UnityEngine;
using WildernessCultivation.Combat;
using WildernessCultivation.Core;
using WildernessCultivation.Player.Stats;
using WildernessCultivation.World.States;

namespace WildernessCultivation.World
{
    /// <summary>
    /// Companion mode — exposed for save + UI. <see cref="Follow"/> = đi theo player;
    /// <see cref="Stay"/> = đứng nguyên tại vị trí Interact toggle.
    /// </summary>
    public enum CompanionMode
    {
        Follow,
        Stay,
    }

    /// <summary>
    /// NPC humanoid exemplar thứ 2 — Companion (R5 follow-up tiếp theo VendorNPC).
    /// Chứng minh multi-component composition + FSM pattern cho NPC có behavior tick:
    /// <list type="number">
    /// <item><b>Multi-component composition</b> (DESIGN_PRINCIPLES rule 1 + 6): reuse 3 component
    /// từ R1 — <see cref="HealthComponent"/> + <see cref="HungerComponent"/> +
    /// <see cref="InvulnerabilityComponent"/>. Companion cần hunger (khác vendor) nhưng
    /// KHÔNG cần Wetness/Thermal/Sanity của Player.</item>
    /// <item><b>FSM qua <see cref="StateMachine{TContext}"/></b> (R7): <see cref="CompanionStates"/>
    /// Idle / Follow / Dead. State singleton, no alloc.</item>
    /// <item><b>IInteractable toggle</b>: Interact flip <see cref="mode"/> giữa Follow/Stay,
    /// fire <see cref="GameEvents.OnCompanionModeChanged"/>.</item>
    /// <item><b>Passive damage recipient</b>: mob aggro IDamageable → CharacterBase →
    /// CompanionNPC.TakeDamage. Companion KHÔNG tự attack mob (scope MVP, combat companion
    /// future work).</item>
    /// <item><b>Starvation damage</b>: hunger decay → khi IsStarving → TickStarvation trừ HP
    /// (reuse <see cref="HealthComponent.TickStarvation"/>).</item>
    /// </list>
    /// </summary>
    public class CompanionNPC : CharacterBase, IInteractable, ISaveable
    {
        [Header("Identity")]
        [Tooltip("ID ổn định cho save lookup. Unique per companion instance.")]
        public string companionId = "companion_generic";
        [Tooltip("Tên hiển thị trên UI prompt + HUD.")]
        public string displayName = "Bạn Đồng Hành";

        [Header("Health")]
        public float maxHP = 60f;
        [Tooltip("Bất tử (không nhận damage từ TakeDamage). Mặc định false — companion có thể chết.")]
        public bool invulnerable = false;

        [Header("Hunger")]
        public float maxHunger = 100f;
        [Tooltip("Multiplier cho hunger decay. 1 = decay như player base.")]
        public float hungerDecayMul = 1f;

        [Header("Follow behavior")]
        [Tooltip("Target companion đi theo (player transform). Assign qua Bootstrap hoặc SetFollowTarget.")]
        public Transform followTarget;
        [Tooltip("Ngoài cự ly này companion bắt đầu follow.")]
        public float followDistance = 3f;
        [Tooltip("Gần cự ly này companion dừng (tránh jitter vs follow distance).")]
        public float stopDistance = 1.5f;
        [Tooltip("Tốc độ di chuyển (units/sec).")]
        public float moveSpeed = 2.2f;

        [Header("State")]
        [Tooltip("Mode hiện tại. Interact toggle Follow ↔ Stay. Save/restore qua ISaveable.")]
        public CompanionMode mode = CompanionMode.Follow;

        // Pure components (composition from R1).
        HealthComponent health;
        HungerComponent hunger;
        InvulnerabilityComponent invuln;

        public readonly StateMachine<CompanionNPC> Fsm = new();

        // ===== CharacterBase =====
        public override float CurrentHP => health != null ? health.HP : 0f;
        public override float CurrentMaxHP => health != null ? health.maxHP : 0f;
        public override bool IsDead => health != null && health.IsDead;

        /// <summary>Expose Hunger cho UI / test. Delegate sang <see cref="HungerComponent"/>.</summary>
        public float Hunger => hunger != null ? hunger.Hunger : 0f;
        public float MaxHunger => hunger != null ? hunger.maxHunger : 0f;
        public bool IsStarving => hunger != null && hunger.IsStarving;

        // ===== IInteractable =====
        public string InteractLabel => mode == CompanionMode.Follow
            ? $"Nói {displayName} ở lại"
            : $"Gọi {displayName} đi theo";
        public bool CanInteract(GameObject actor) => actor != null && !IsDead;

        // ===== ISaveable =====
        public string SaveKey => $"World/Companion/{companionId}";
        public int Order => 70; // Sau Inventory (60), cùng order với VendorNPC.

        void Awake()
        {
            health = gameObject.GetComponent<HealthComponent>() ?? gameObject.AddComponent<HealthComponent>();
            hunger = gameObject.GetComponent<HungerComponent>() ?? gameObject.AddComponent<HungerComponent>();
            invuln = gameObject.GetComponent<InvulnerabilityComponent>() ?? gameObject.AddComponent<InvulnerabilityComponent>();

            health.maxHP = maxHP;
            health.HP = maxHP;
            hunger.maxHunger = maxHunger;
            hunger.Hunger = maxHunger;

            if (invulnerable) invuln.InvulnerableUntil = float.MaxValue;

            Fsm.Init(this, CompanionStates.Idle);
        }

        void OnEnable()
        {
            SaveRegistry.RegisterSaveable(this);
        }

        void OnDisable()
        {
            SaveRegistry.UnregisterSaveable(this);
        }

        void Update()
        {
            float dt = Time.deltaTime;
            TickSurvival(dt);
            Fsm.Tick(dt);
        }

        /// <summary>Gọi mỗi frame (Update hoặc test manual) để hunger decay + starvation damage.
        /// Tách pure để EditMode test gọi trực tiếp không cần Update loop.</summary>
        public void TickSurvival(float dt)
        {
            if (IsDead || hunger == null || health == null) return;
            hunger.Tick(dt, hungerDecayMul);
            health.TickStarvation(dt, hunger.IsStarving, dehydrated: false);
        }

        public override void TakeDamage(float amount, GameObject source)
        {
            if (health == null || health.IsDead) return;
            if (invuln != null && invuln.IsInvulnerable) return;
            health.TakeRaw(amount);
            if (health.IsDead) Fsm.ChangeState(CompanionStates.Dead);
        }

        /// <summary>Player nhặt companion = assign followTarget + mode Follow. Gọi từ bootstrap
        /// hoặc quest script.</summary>
        public void SetFollowTarget(Transform target)
        {
            followTarget = target;
        }

        /// <summary>Cho ăn — delegate HungerComponent.Eat. Hook cho future "feed command" interaction.</summary>
        public void Eat(float amount)
        {
            hunger?.Eat(amount);
        }

        public bool Interact(GameObject actor)
        {
            if (!CanInteract(actor)) return false;
            mode = mode == CompanionMode.Follow ? CompanionMode.Stay : CompanionMode.Follow;
            GameEvents.RaiseCompanionModeChanged(this);
            return true;
        }

        /// <summary>Helper cho state class: khoảng cách tới followTarget. Infinity khi không target.</summary>
        public float DistanceToTarget()
        {
            if (followTarget == null) return float.PositiveInfinity;
            return Vector2.Distance(followTarget.position, transform.position);
        }

        /// <summary>Helper cho state class: di chuyển 1 step về phía target (transform-based, không
        /// dùng Rigidbody2D → EditMode-testable mà không cần Physics2D).</summary>
        public void MoveTowardTargetStep(float dt)
        {
            if (followTarget == null) return;
            transform.position = Vector3.MoveTowards(
                transform.position,
                followTarget.position,
                moveSpeed * Mathf.Max(0f, dt));
        }

        public void CaptureState(SaveData data)
        {
            if (data == null) return;
            data.companions ??= new System.Collections.Generic.List<CompanionSaveData>();
            var existing = data.companions.Find(c => c != null && c.companionId == companionId);
            var entry = existing ?? new CompanionSaveData { companionId = companionId };
            entry.position = transform.position;
            entry.hp = CurrentHP;
            entry.hunger = Hunger;
            entry.mode = (int)mode;
            if (existing == null) data.companions.Add(entry);
        }

        public void RestoreState(SaveData data)
        {
            if (data == null || data.companions == null) return;
            var entry = data.companions.Find(c => c != null && c.companionId == companionId);
            if (entry == null) return;
            transform.position = entry.position;
            if (health != null) health.HP = Mathf.Clamp(entry.hp, 0f, health.maxHP);
            if (hunger != null) hunger.Hunger = Mathf.Clamp(entry.hunger, 0f, hunger.maxHunger);
            mode = entry.mode == (int)CompanionMode.Stay ? CompanionMode.Stay : CompanionMode.Follow;
            if (IsDead) Fsm.ChangeState(CompanionStates.Dead);
        }
    }
}
