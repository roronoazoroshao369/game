# Refactor history — R1..R7 sprint

> Log các refactor lớn đã merge. Để AI assistant / contributor mới hiểu
> "pattern X từ đâu tới, thay cho pattern cũ nào" + "tại sao code phần này
> lại tổ chức như vậy".
>
> Nguồn: audit 2026-04-29 (`Documentation/` hoặc hỏi maintainer). Plan A = refactor sprint
> 3 tuần trước khi scale feature.

## Timeline tổng

| Sprint | PR | Pattern introduce | Anti-pattern xoá |
|---|---|---|---|
| R3 | [#61](https://github.com/roronoazoroshao369/game/pull/61) | `ServiceLocator` cache | 58× `FindObjectOfType` |
| R2 | [#62](https://github.com/roronoazoroshao369/game/pull/62) | `PlayerStats : IDamageable` | 6× fallback cast trong mob AI |
| R1 p1 | [#63](https://github.com/roronoazoroshao369/game/pull/63) | `WetnessComponent` + `ThermalComponent` + `PermadeathHandler` | God-object `PlayerStats` 82-field |
| R4 | [#64](https://github.com/roronoazoroshao369/game/pull/64) | `GameEvents` + `CombatEvents` hub | UI poll `FindObjectOfType` mỗi frame |
| R5 | [#65](https://github.com/roronoazoroshao369/game/pull/65) | `CharacterBase` abstract (Player + Mob + future NPC) | Không có polymorphic "any character" |
| R1 p2 | [#66](https://github.com/roronoazoroshao369/game/pull/66) | 7 stat component còn lại (Health/Hunger/Thirst/Sanity/Mana/Shield/Invuln) | PlayerStats vẫn 482 LoC sau R1 p1 |
| R6 | [#67](https://github.com/roronoazoroshao369/game/pull/67) | `ISaveable` dispatcher + `SaveRegistry` | `SaveLoadController` monolith 228 LoC |
| R7 | [#68](https://github.com/roronoazoroshao369/game/pull/68) | `StateMachine<T>` + `IState<T>` (Wolf/Rabbit exemplar) | Mob AI if/else trong Update() |

---

## R3 — ServiceLocator cache (PR #61)

**Trước**: 58 `FindObjectOfType<T>()` rải rác, nhiều trong Update() → O(scene scan) per frame trên mobile → perf killer.

**Sau**: `ServiceLocator.Register<T>(this)` trong Awake, `ServiceLocator.Get<T>()` O(1) dictionary lookup. Fake-null safe (Unity destroyed Object return null đúng).

```csharp
// ServiceLocator.cs
public static void Register<T>(T service) where T : Component { services[typeof(T)] = service; }
public static T Get<T>() where T : Component {
    if (services.TryGetValue(typeof(T), out var s) && s != null) return (T)s;
    return null;
}
```

Còn 4× Find trong fallback paths an toàn (edge case khi service không Register, vd BootstrapWizard chưa chạy).

---

## R2 — PlayerStats : IDamageable (PR #62)

**Trước**: `PlayerStats` KHÔNG implement `IDamageable` (lý do legacy). Mỗi mob phải fallback:
```csharp
var dmg = target.GetComponent<IDamageable>() ?? target.GetComponentInParent<IDamageable>();
if (dmg != null) dmg.TakeDamage(damage, gameObject);
else target.GetComponent<PlayerStats>()?.TakeDamage(damage);  // ← fallback lặp 6 lần
```

**Sau**: `PlayerStats : MonoBehaviour, CharacterBase, IDamageable` + overload `TakeDamage(float, GameObject source)`. 6 mob AI (Wolf/Fox/Bat/Boar/Snake/Boss) drop fallback → code gọn hơn, tight coupling biến mất. `source` param reserved cho threat/aggro/log tương lai.

---

## R1 p1 — PlayerStats extract (PR #63)

**Trước**: `PlayerStats.cs` 618 LoC, 82 public field, xử lý 11 concern (HP/Hunger/Thirst/Sanity/Mana/BodyTemp/Wetness/Sickness/Shield/Invulnerable/Awakening). God-object vi phạm SRP.

**Sau**: tách `WetnessComponent` (176 LoC) + `ThermalComponent` (132 LoC) + `PermadeathHandler` (117 LoC). PlayerStats façade còn 482 LoC. 3 component đầu tiên làm exemplar — p1 chấp nhận chậm vì verify pattern trước.

---

## R4 — GameEvents hub (PR #64)

**Trước**: UI/Audio/Quest/VFX dùng `FindObjectOfType<PlayerStats>()` để poll state mỗi frame. 15 event instance rải rác (mỗi system tự quản lý subscriber).

**Sau**: `GameEvents` (Scripts/Core/GameEvents.cs) static hub với 30+ event (PlayerDied, RealmAdvanced, WeatherChanged, ItemConsumed, MobKilled, InventoryChanged, PlayerStatsChanged, …). Publisher fire qua `GameEvents.RaiseXxx(...)`, subscriber `+=` trong OnEnable / `-=` trong OnDisable.

```csharp
// GameEvents.cs
public static event Action OnPlayerDied;
public static void RaisePlayerDied() => OnPlayerDied?.Invoke();
public static void ClearAllSubscribers() { OnPlayerDied = null; OnRealmAdvanced = null; ... }
```

**Coexistence**: PlayerStats vẫn giữ instance event `OnStatsChanged` (backward compat cho 38+ test không break). PlayerStats fire **cả** instance event + GameEvents hub → subscriber mới dùng hub, subscriber cũ vẫn work.

Rule 7 AGENTS: test MUST `GameEvents.ClearAllSubscribers()` trong SetUp/TearDown.

`CombatEvents` tách riêng vì tần suất cao (mỗi hit) — tránh spam subscriber chung.

---

## R5 — CharacterBase abstract (PR #65)

**Trước**: Player + Mob không share parent. Code quan tâm "any character" (damage popup, HP bar generic) phải duplicate logic cho PlayerStats + MobBase.

**Sau**: `CharacterBase : MonoBehaviour, IDamageable` abstract với:
- `public abstract float CurrentHP`, `CurrentMaxHP`
- `public abstract bool IsDead`
- `public virtual Vector3 Position => transform.position`
- `public float HPRatio01`, `public bool IsAlive`

PlayerStats + MobBase inherit CharacterBase. Override `TakeDamage(float, GameObject)`.

Prerequisite cho NPC humanoid (roadmap R5 follow-up) — companion/vendor inherit CharacterBase + reuse subset stat component từ R1.

---

## R1 p2 — 7 stat components (PR #66)

**Trước**: PlayerStats 482 LoC (sau R1 p1), vẫn chứa HP/Hunger/Thirst/Sanity/Mana/Shield/Invulnerability/Sickness inline.

**Sau**: tách 7 component `Scripts/Player/Stats/`:

| Component | LoC | Concern |
|---|---|---|
| `HealthComponent` | ~55 | HP/maxHP/damage/heal/regen/Die |
| `HungerComponent` | ~40 | Hunger decay + Eat |
| `ThirstComponent` | ~40 | Thirst decay + Drink |
| `SanityComponent` | ~50 | Sanity decay + Restore + biome ambient |
| `ManaComponent` | ~45 | Mana regen + Consume |
| `ShieldComponent` | ~35 | Shield value + duration |
| `InvulnerabilityComponent` | ~30 | i-frame timer |

PlayerStats giữ **100% public API** qua façade delegate (HP/maxHP/Eat/Drink/TakeDamage/…). 38+ consumer (UI/SaveLoad/Cultivation/Combat/Test) KHÔNG sửa.

Lazy auto-add pattern: property getter lazy-add component → EditMode test không cần `Boot()` trước.

**Bonus fix**: PR #66 phát hiện `WetnessComponent.cs` thiếu `using WildernessCultivation.Core;` → Build Android fail từ R1 p1 (không ai thấy vì test job skip-noop). Fix 1 dòng → Build pass lần đầu kể từ R1 p1.

---

## R6 — ISaveable dispatcher (PR #67)

**Trước**: `SaveLoadController` 228 LoC monolith. Biết chi tiết schema của Inventory / PlayerStats / RealmSystem / ItemDatabase / SpiritRoot catalog / TimeManager / WorldGenerator. Mỗi field mới → sửa SaveLoadController → vi phạm OCP.

**Sau**: `SaveLoadController` → ~90 LoC **dispatcher thuần**. Enumerate `SaveRegistry` rồi delegate `CaptureState` / `RestoreState`.

```csharp
// ISaveable
public interface ISaveable {
    string SaveKey { get; }
    int Order { get; }
    void CaptureState(SaveData data);
    void RestoreState(SaveData data);
}

// SaveRegistry (static)
public static void RegisterSaveable(ISaveable s);      // OnEnable
public static void UnregisterSaveable(ISaveable s);    // OnDisable
public static void RegisterFixup(object owner, int order, Action<SaveData> action);
public static void UnregisterFixupsFor(object owner);
public static IReadOnlyList<ISaveable> OrderedSaveables();
public static IReadOnlyList<Action<SaveData>> OrderedFixupActions();
public static void ClearAll();   // test SetUp/TearDown
```

**Order schedule** xem `ARCHITECTURE.md` §2.7.

**Backward compat**: `SaveData` / `PlayerSaveData` / `WorldSaveData` / `InventorySlotData` schema KHÔNG đổi. Save cũ load fine.

Rule 8 AGENTS: test MUST `SaveRegistry.ClearAll()` trong SetUp/TearDown.

---

## R7 — StateMachine<T> FSM (PR #68)

**Trước**: Mob AI if/else trong `Update()`. Wolf 41 LoC, Boss 217 LoC. Thêm Block/Parry/Charge/Channel state sẽ rối.

**Sau**: `Scripts/Core/StateMachine.cs` generic FSM ~130 LoC. State singleton instance (no alloc/frame). Reentrant-safe ChangeState (queue + apply sau tick). Exemplar port 2 mob: `WolfAI` (Idle→Chase→Attack→Dead) + `RabbitAI` (Wander↔Flee).

```csharp
public interface IState<TContext> { void OnEnter(TContext); void OnTick(TContext, float); void OnExit(TContext); }
public sealed class StateMachine<TContext> { Init/Tick/ChangeState/Shutdown; }
public sealed class DelegateState<TContext> : IState<TContext> { /* 3-delegate helper */ }
public enum PlayerActivityState { Idle, Moving, Dodging, Attacking, Channeling, Sleeping, Dead }
```

MobBase: `MoveTowards` / `StopMoving` / `TryFindPlayer` promote `protected` → `internal` để state class same-assembly gọi được.

Mob legacy (Fox/Snake/Bat/Boar/Boss/Deer/Crow) vẫn dùng Update if/else — convert dần khi cần state mới. Player FSM out of scope (action component đã clean-separated).

Rule 9 AGENTS: mob mới dùng FSM, xem `.agents/skills/add-mob/`.

---

## Kết quả sprint

**Codebase trước** (audit 2026-04-29):
- 9091 LoC, 91 file, 82 public field trên PlayerStats, 58× FindObjectOfType, 15 event, 4 interface, 2 abstract class.
- Điểm audit: 7/10, với 3 hot spot P0 cần fix trước scale.

**Codebase sau** (post-R7):
- Pattern inventory đủ cho 3-5 năm scale: ISaveable + GameEvents + StateMachine + ServiceLocator + IDamageable + CharacterBase + composable stat components.
- Anti-pattern P0 eliminated.
- Test coverage: 38 test file → 50+ (SaveRegistry 9, StateMachine 8, StatComponents 18, …).
- Docs: AGENTS + ARCHITECTURE + DESIGN_PRINCIPLES + skill index + refactor log.

**Roadmap còn lại** (không trong sprint này):
- R5 follow-up: implement NPC humanoid đầu tiên (vendor hoặc companion) — reuse R1 stat components.
- R8: DI container (VContainer/Zenject) — defer đến Beta, chỉ khi team >3 dev hoặc cần mock heavy.
- R9: Asmdef per module — defer đến codebase >20k LoC hoặc compile chậm.
- R10: Public field encapsulation (`{ get; private set; }`) — defer trừ khi audit bug invariant.
- R11: Boss state class (Phase1/Phase2/Phase3) — khi Boss scope mở rộng.
- Mob legacy → FSM — dần convert khi cần state mới.

---

## Lessons learned

1. **CI gate giả pass**: test job skip-noop vì thiếu `UNITY_LICENSE` → R1 p1 chứa compile bug `WetnessComponent` thiếu `using WildernessCultivation.Core` không ai thấy cho đến R1 p2 fix. Lesson: lint + Build Android gate là cần nhưng không đủ — PHẢI add license secret cho test job thật chạy.

2. **Backward-compat façade**: R1 p2 giữ 100% PlayerStats public API (façade delegate) → 38+ consumer không sửa → PR diff gọn + review nhanh. Rule: refactor monolith trong nhiều PR, giữ API cũ, đổi internal trước.

3. **Coexistence refactor**: R4 GameEvents hub + instance event coexist → không break 38 test. Không cần "big bang rewrite". Rule: introduce pattern mới song song pattern cũ → deprecate dần.

4. **Reentrant-safe FSM**: R7 `ChangeState` trong OnTick queue + apply sau tick → tránh state corruption khi Enter fire transition khác. Chi tiết nhỏ nhưng quan trọng.

5. **Order fixup pattern**: R6 cross-system ordering (`ReapplySpiritRootMaxHP` @30 < `ReapplyAccumulatedBonuses` @50 < clamp HP @60) phải verify qua test regression — dễ bị đảo thứ tự khi merge nhiều PR.
