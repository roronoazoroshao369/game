# Architecture — Wilderness Cultivation Chronicle

> Overview kiến trúc code để AI assistant / contributor mới nắm đủ context
> trước khi đọc `DESIGN_PRINCIPLES.md` (quy tắc) hoặc đụng vào `AGENTS.md` (invariant).
>
> Cập nhật: sau refactor sprint R1..R7. Xem `REFACTOR_HISTORY.md` cho timeline.

## 1. High-level module map

```
Assets/_Project/
├── Scripts/                      ← 1 asmdef (WildernessCultivation)
│   ├── Core/                     — infra: GameManager, TimeManager, SaveSystem,
│   │                                GameEvents, ServiceLocator, ISaveable,
│   │                                StateMachine, CharacterBase
│   ├── Player/                   — PlayerController + action components
│   │   └── Stats/                — R1 subsystem components (Health/Hunger/…)
│   ├── Cultivation/              — RealmSystem, SpiritRoot, công pháp
│   ├── Inventory/ · Items/       — Inventory, InventorySlot, ItemSO, ItemDatabase
│   ├── Crafting/                 — RecipeSO, CraftingSystem, CraftStationMarker
│   ├── Mobs/                     — MobBase + AI subclass (Wolf/Fox/Rabbit/…)
│   │   └── States/               — R7 FSM state classes per mob
│   ├── Combat/                   — IDamageable, Projectile, CombatEvents
│   ├── World/                    — WorldGenerator, BiomeSO, ResourceNode, Weather,
│   │                                Workbench, Campfire, StorageChest, Shelter
│   ├── UI/ · Audio/ · Camera/ · Vfx/ — cross-cutting presentation
│   └── WildernessCultivation.asmdef
└── Tests/
    ├── EditMode/                 ← 1 asmdef (WildernessCultivation.Tests.EditMode, Editor only)
    └── PlayMode/                 ← 1 asmdef (WildernessCultivation.Tests.PlayMode, all platforms)
```

**Rule**: folder = namespace = module boundary. `Scripts/Mobs/WolfAI.cs` → `namespace WildernessCultivation.Mobs`.
File mới PHẢI khai báo đúng namespace theo folder. Không flatten vào root namespace.

Hiện tại 1 asmdef duy nhất (`WildernessCultivation`) → internal access qua toàn code game.
Khi scale lên nhiều module (Beta+), xem R9 trong roadmap audit — split asmdef per folder.

## 2. Pattern inventory

### 2.1 Component composition (Player)

Player = GameObject + N component. Mỗi ability là MonoBehaviour riêng, enable/disable độc lập.

```
PlayerObject
├── PlayerController              — input + movement
├── PlayerStats                   — façade delegate → 10 stat components
├── PlayerCombat                  — melee + cast
├── DodgeAction / FishingAction / SleepAction / TorchAction / …
├── Stats/HealthComponent         — HP, maxHP, damage, heal, events
├── Stats/HungerComponent         — decay + eat
├── Stats/ThirstComponent
├── Stats/SanityComponent
├── Stats/ManaComponent
├── Stats/ShieldComponent
├── Stats/InvulnerabilityComponent
├── Stats/WetnessComponent
├── Stats/ThermalComponent
└── Stats/PermadeathHandler
```

**Lợi ích**: test component riêng không cần full Player bootstrap. NPC humanoid (roadmap R5 follow-up) reuse subset — vendor chỉ cần Health + Invuln; companion cần Health + Hunger + Invuln.

Stat component là **pure** (no external deps) — chỉ đụng `Time.deltaTime` + field nội bộ. PlayerStats façade lazy-auto-add trên property access (EditMode test không cần Boot).

### 2.2 Inheritance (Mob + Player share `CharacterBase`)

```
CharacterBase : MonoBehaviour, IDamageable           ← abstract (R5)
├── PlayerStats                                       — input-driven
└── MobBase                                           — AI-driven abstract
    ├── WolfAI (+ WolfStates FSM)                     — R7 FSM exemplar
    ├── RabbitAI (+ RabbitStates FSM)                 — R7 FSM exemplar
    ├── FoxSpiritAI / SnakeAI / BoarAI / BatAI /      — legacy Update if/else
    │   CrowAI / DeerSpiritAI
    └── BossMobAI                                     — legacy đa phase
```

`CharacterBase` expose abstract `CurrentHP` / `CurrentMaxHP` / `IsDead` + virtual `Position`,
`HPRatio01`, `IsAlive`. Dùng khi code quan tâm polymorphic "any character" (damage popup,
HP bar generic, target selection).

Subclass riêng override `TakeDamage(float, GameObject)` để inject side-effect (aggro, loot, xp).

### 2.3 ScriptableObject data (Strategy pattern)

```
Assets/_Project/Data/
├── Items/            — 23 ItemSO instance
├── Recipes/          — 3 RecipeSO (seed, crafting stations fallback)
├── SpiritRoots/      — 6 SpiritRootSO (linh căn 5 elements + null)
├── Biomes/           — 3 BiomeSO (forest/grass/rock)
├── StatusEffects/    — 6 StatusEffectSO
└── Techniques/       — TechniqueSO (abstract) → FireBall/SpiritGathering/SwordQi SO
```

**Designer edit từ Inspector**, code KHÔNG đổi. TechniqueSO là pattern Strategy hoàn chỉnh — mỗi technique SO có logic cast riêng, `PlayerCombat.Cast(TechniqueSO)` polymorphic dispatch.

### 2.4 Interfaces (cross-cutting)

| Interface | Ý nghĩa | Implementers |
|---|---|---|
| `IDamageable` | Anything takes damage | PlayerStats, MobBase, ResourceNode |
| `IInteractable` | Player E-key interact | ResourceNode, Workbench, Campfire, StorageChest, Shelter, CultivationMat |
| `IStationGate` | Crafting station gating | Workbench, Campfire |
| `RealmSystemHook` | Realm tier observer | UI indicators, spirit root auto-check |
| `ISaveable` | Save/load participant (R6) | WorldGenerator, TimeManager, RealmSystem, PlayerStats, Inventory |
| `IState<T>` | FSM state (R7) | WolfStates.Idle/Chase/Attack/Dead, RabbitStates.Wander/Flee |

### 2.5 Service Locator + Singleton (R3)

```csharp
ServiceLocator.Register<T>(this);     // in Awake() / OnEnable
ServiceLocator.Unregister<T>(this);   // in OnDestroy / OnDisable
var stats = ServiceLocator.Get<PlayerStats>();   // O(1) cached
```

Registry-backed (không phải reflection). Fake-null safe (Unity destroyed object return null đúng).
3 Singleton (`GameManager`, `WorldGenerator`, `AudioManager`) giữ nguyên vì bootstrap order phức tạp.

Rule: mỗi file tối đa **1 lần** `FindObjectOfType` (và chỉ trong `Start()`). 99% trường hợp dùng `ServiceLocator.Get<T>()`.

### 2.6 Domain events (R4)

```csharp
// publisher:
GameEvents.RaisePlayerStatsChanged();
GameEvents.RaiseRealmAdvanced(newTier);
GameEvents.RaiseWeatherChanged(weather);

// subscriber — OnEnable/OnDisable lifecycle:
void OnEnable()  { GameEvents.OnRealmAdvanced += HandleRealmAdvance; }
void OnDisable() { GameEvents.OnRealmAdvanced -= HandleRealmAdvance; }
```

`GameEvents` (Scripts/Core) là **static hub** cho 30+ event cross-cutting (PlayerDied, RealmAdvanced, WeatherChanged, ItemConsumed, MobKilled, InventoryChanged, …). UI / audio / quest / VFX subscribe thay vì poll `FindObjectOfType` mỗi frame.

`CombatEvents` tách ra vì tần suất cao (mỗi hit) — tránh spam GameEvents subscriber chung.

Test **PHẢI** gọi `GameEvents.ClearAllSubscribers()` trong `SetUp`/`TearDown` (rule 7 AGENTS.md).

### 2.7 Save/load — ISaveable dispatcher (R6)

**Trước R6**: `SaveLoadController` monolith biết chi tiết schema của Inventory/PlayerStats/RealmSystem/… (228 LoC, vi phạm OCP).

**Sau R6**: `SaveLoadController` là **dispatcher thuần**. Mỗi system implement `ISaveable`, tự serialize slice của nó vào `SaveData`.

```csharp
public interface ISaveable
{
    string SaveKey { get; }        // diagnostic "Player/Vitals", "World/Seed", …
    int Order { get; }             // thấp → trước
    void CaptureState(SaveData data);
    void RestoreState(SaveData data);
}
```

**Save pipeline**:
```
SaveLoadController.Save()
    ↓
skip if playerStats.IsDead (permadeath race guard)
    ↓
ForwardLegacyRefs()  ← bridge BootstrapWizard-wired itemDatabase/spiritRootCatalog
    ↓
foreach s in SaveRegistry.OrderedSaveables()    ← sorted theo Order ascending
    s.CaptureState(data)
    ↓
SaveSystem.Save(data)   ← JSON serialize to PlayerPrefs / file
```

**Load pipeline**:
```
SaveLoadController.LoadAndApply()
    ↓
SaveSystem.TryLoad(out data)  ← return nếu không có save
    ↓
ForwardLegacyRefs()
    ↓
foreach s in SaveRegistry.OrderedSaveables()
    s.RestoreState(data)  ← đọc state cơ bản
    ↓
foreach f in SaveRegistry.OrderedFixupActions()  ← cross-system ordering
    f(data)
```

**Order table** (thấp → trước):

| Order | Phase | Actor | Action |
|---|---|---|---|
| 0 | Capture/Restore | WorldGenerator | seed |
| 5 | Capture/Restore | TimeManager | time / season / weather |
| 10 | Capture/Restore | RealmSystem | tier / xp / spiritRoot name |
| 20 | Fixup | SpiritRoot | catalog lookup: name → SO → SetSpiritRoot |
| 30 | Capture/Restore | PlayerStats | vitals (HP/Hunger/Thirst/Sanity/Mana/…) |
| 30 | Fixup | PlayerStats | ReapplySpiritRootMaxHP |
| 40 | Fixup | PlayerCombat | ResetMeleeDamageToBase (tránh double-stack tier bonus) |
| 50 | Fixup | RealmSystem | ReapplyAccumulatedBonuses (tier 1..currentTier maxHP/damage) |
| 60 | Capture/Restore | Inventory | slots (per-slot freshness/durability) |
| 60 | Fixup | PlayerStats | clamp HP/Mana to final maxHP/maxMana |

**Lifecycle**: implementer register OnEnable, unregister OnDisable. Test `SaveRegistry.ClearAll()` trong SetUp/TearDown (rule 8 AGENTS.md).

**Backward compat**: `SaveData` / `PlayerSaveData` / `WorldSaveData` / `InventorySlotData` schema KHÔNG đổi — save cũ load fine.

### 2.8 Mob AI — StateMachine<T> FSM (R7)

Mob mới dùng `StateMachine<T>` (Scripts/Core/StateMachine.cs):

```csharp
public class WolfAI : MobBase
{
    internal readonly StateMachine<WolfAI> Fsm = new();

    protected override void Awake()
    {
        base.Awake();
        Fsm.Init(this, WolfStates.Idle);
    }

    void Update()
    {
        if (!ShouldTickAI()) return;      // LOD gate
        Fsm.Tick(Time.deltaTime);
    }

    protected override void Die(GameObject killer)
    {
        Fsm.Shutdown();
        base.Die(killer);
    }
}
```

State class singleton (no alloc/frame):
```csharp
public static class WolfStates
{
    public static readonly IState<WolfAI> Idle = new WolfIdle();
    public static readonly IState<WolfAI> Chase = new WolfChase();
    public static readonly IState<WolfAI> Attack = new WolfAttack();
    public static readonly IState<WolfAI> Dead = new WolfDead();
}

sealed class WolfChase : IState<WolfAI>
{
    public void OnEnter(WolfAI w) { }
    public void OnTick(WolfAI w, float dt)
    {
        if (w.IsDead) { w.Fsm.ChangeState(WolfStates.Dead); return; }
        if (w.target == null) { w.Fsm.ChangeState(WolfStates.Idle); return; }
        float dist = Vector2.Distance(w.target.position, w.transform.position);
        if (dist <= w.attackRange) { w.Fsm.ChangeState(WolfStates.Attack); return; }
        w.MoveTowards(w.target.position);
    }
    public void OnExit(WolfAI w) { }
}
```

Reentrant-safe: `ChangeState` trong OnTick/OnEnter/OnExit được queue + apply sau tick hiện tại.

**Legacy mob** (Fox/Snake/Bat/Boar/Boss/Deer/Crow) vẫn dùng Update if/else — convert sang FSM khi cần thêm state (Boss Phase1/Phase2, Fox Summon, Snake Venomized…).

### 2.9 Perf-aware LOD (mob)

`MobBase.ShouldTickAI()`:
- Nếu `lodFarDistance > 0` và player cách > `lodFarDistance` → `rb.simulated = false` + tick `1/lodSlowFrameMod` frame (default 8 → giảm 87% CPU).
- Instance ID offset để mob xa lệch pha nhau (tránh spike cùng frame).

## 3. Data flow (runtime)

### Input → gameplay
```
[Joystick / Button]  UI/VirtualJoystick + UI/SkillButton
       ↓
PlayerController.Update() reads joystick → rb.velocity
       ↓
PlayerCombat.Cast(technique) → TechniqueSO.Execute(owner)
       ↓
Projectile / melee hit → target.GetComponent<IDamageable>().TakeDamage(amt, source)
       ↓
IDamageable impl (PlayerStats or MobBase) mutate state
       ↓
GameEvents.RaisePlayerStatsChanged() / CombatEvents.RaiseDamage(pos, amt, isCrit)
       ↓
UI / Audio / Vfx / Quest subscriber react
```

### Tick loop (per frame)
```
GameManager.Update()
    → TimeManager.Advance(dt)          ← season/weather/day-night
        → if day↔night flip → GameEvents.RaiseWeatherChanged(...)
    → PlayerStats components tick      ← decay hunger/thirst/sanity; regen mana
        → threshold cross → GameEvents.RaiseX...
    → Mob AI Update()                  ← FSM tick (new) or if/else (legacy)
```

### Save/load
Xem §2.7. Trigger:
- Auto-save qua `SaveLoadController.autosaveInterval` (skip nếu player dead).
- Manual save qua PauseMenu.
- Load khi scene start nếu `autoLoadOnStart`.

## 4. Scene structure

1 scene (`Main`) + `BootstrapWizard` sinh runtime GO từ Editor menu. Bootstrap wire:
- Player + inventory + realm + save controller
- Mob spawner (spawn distance, density)
- Resource nodes (tree/rock/bush/spawn via BiomeSO)
- Crafting stations
- World (WorldGenerator + TimeManager + weather)
- UI canvas (HUD + pause + inventory + skill buttons)

Mỗi lần add system mới cần scene presence → update `BootstrapWizard` trong `Editor/`.

## 5. CI pipeline

`.github/workflows/`:

| Workflow | Job | Gate |
|---|---|---|
| `lint.yml` | Whitespace + EOL + final newline | grep-based, fail nếu tab/trailing/CRLF |
| `lint.yml` | `dotnet format whitespace --verify` | .editorconfig check |
| `build.yml` | Build Android (IL2CPP, ARM64) | Unity 6000.4.4f1 + Android module |
| `test.yml` | Run EditMode + PlayMode tests | requires `UNITY_LICENSE` secret (hiện skip-noop) |

Build cache qua GameCI docker. PR phải pass lint + build; test gate sẽ active khi license setup.

## 6. Cross-doc map

- `AGENTS.md` — **rules** (invariant + HARD constraints + conventions). Đọc trước khi code.
- `DESIGN_PRINCIPLES.md` — **principles** (DO/DON'T với code example). Đọc trước khi design feature mới.
- `REFACTOR_HISTORY.md` — **timeline** R1..R7 (pattern nào introduce ở PR nào, thay cho pattern gì).
- `.agents/skills/README.md` — **procedures** (how-to: add mob, recipe, test, …).
- `Documentation/` — gameplay/design docs (MVP_SCOPE, ROADMAP, GDD).

Khi add feature mới: AGENTS → DESIGN_PRINCIPLES → ARCHITECTURE (chỗ này) → skill phù hợp → code.
