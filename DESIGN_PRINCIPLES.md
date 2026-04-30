# Design Principles — Wilderness Cultivation Chronicle

> 10 nguyên tắc DO / DON'T kèm code example. Đọc trước khi design feature mới
> hoặc sửa pattern hiện có. Bổ sung cho `AGENTS.md` (HARD invariant) +
> `ARCHITECTURE.md` (pattern inventory).

Mục đích: giữ codebase scale được 3-5 năm + 30+ feature mới mà không rot.

---

## 1. Composition over inheritance cho stat / ability

**Lý do**: NPC humanoid (vendor/companion) cần subset component giống player. Inheritance
buộc NPC kéo theo toàn bộ stat của player (Wetness, Thermal, …) kể cả khi không cần.

```csharp
// DO — NPC vendor chỉ cần Health + Invuln
var vendor = go.AddComponent<HealthComponent>();
go.AddComponent<InvulnerabilityComponent>();
// KHÔNG add Wetness, Thermal, Hunger nếu vendor không cần.

// DON'T — kéo PlayerStats façade (10 component auto-add) khi vendor chỉ cần 2
go.AddComponent<PlayerStats>();  // ← drag Wetness + Thermal + Hunger vào vendor GO
```

Exception: `CharacterBase` inheritance (R5) OK vì chỉ expose abstract polymorphic view
(`CurrentHP`, `IsDead`) — không kéo field cụ thể.

---

## 2. Event hub cho cross-cutting, ref trực tiếp cho hot path

Chia làm 2 lớp:

| Lớp | Ví dụ | Cách giao tiếp |
|---|---|---|
| **Domain cross-cutting** | UI react PlayerDied, Audio play RealmBreakthrough, Quest tick MobKilled | `GameEvents` hub subscribe/raise |
| **Hot-path gameplay** | Mob AI tick target, PlayerCombat hit detect | Direct ref + `ServiceLocator.Get<T>` cache |

```csharp
// DO — UI subscribe GameEvents (update HP bar khi player take damage)
void OnEnable()  { GameEvents.OnPlayerStatsChanged += RefreshHPBar; }
void OnDisable() { GameEvents.OnPlayerStatsChanged -= RefreshHPBar; }

// DON'T — UI poll PlayerStats mỗi frame (58x FindObjectOfType anti-pattern cũ)
void Update() { var stats = FindObjectOfType<PlayerStats>(); hpBar.value = stats.HP / stats.maxHP; }
```

```csharp
// DO — Mob AI cache player ref (hot path, mỗi frame)
static Transform s_cachedPlayer;
Transform GetPlayerCached() { return s_cachedPlayer ??= ServiceLocator.Get<PlayerStats>()?.transform; }

// DON'T — fire GameEvents mỗi frame (10 wolves × 60fps = 600 event/s spam)
void Update() { GameEvents.RaiseMobSearchedForPlayer(this); }  // ← tắc subscriber list
```

Rule of thumb: event tần suất > 10/s → dùng direct ref. Event tần suất < 1/s → dùng GameEvents.

---

## 3. Interface cho polymorphic dispatch, không cast concrete

**Lý do**: mob biết `IDamageable` là đủ để đánh player/mob/resource. Biết `PlayerStats` cụ thể là tight coupling — khi đổi Player type (NPC humanoid damage mob hoặc mob damage NPC), phải add case mới.

```csharp
// DO — mob code chỉ biết IDamageable
var dmg = target.GetComponent<IDamageable>() ?? target.GetComponentInParent<IDamageable>();
if (dmg != null) dmg.TakeDamage(damage, gameObject);

// DON'T — fallback cast concrete (pre-R2 pattern, đã xoá)
if (dmg != null) dmg.TakeDamage(damage, gameObject);
else target.GetComponent<PlayerStats>()?.TakeDamage(damage);
```

Áp dụng tương tự:
- Save/load → `ISaveable` (không cast `Inventory` cụ thể)
- Interact → `IInteractable` (không cast `Workbench`)
- Craft gate → `IStationGate`
- FSM state → `IState<T>`

---

## 4. Lifecycle register/unregister PHẢI symmetric

Subscribe OnEnable → unsubscribe OnDisable. Register ServiceLocator Awake → unregister OnDestroy. Register SaveRegistry OnEnable → unregister OnDisable.

```csharp
// DO — symmetric lifecycle
void OnEnable()  { SaveRegistry.RegisterSaveable(this); GameEvents.OnPlayerDied += Handle; }
void OnDisable() { SaveRegistry.UnregisterSaveable(this); GameEvents.OnPlayerDied -= Handle; }

// DON'T — subscribe trong Awake + unsubscribe OnDestroy (Unity disable/enable cycle leak)
void Awake()     { GameEvents.OnPlayerDied += Handle; }      // ← subscribe 1 lần
void OnDestroy() { GameEvents.OnPlayerDied -= Handle; }      // ← unsubscribe 1 lần, OK
// NHƯNG nếu GO được pool + disable/re-enable → subscribe 2 lần, handle fire 2 lần.
```

Test integration: PHẢI gọi `ClearAllSubscribers()` / `SaveRegistry.ClearAll()` trong SetUp/TearDown (rule 7+8 AGENTS).

---

## 5. ScriptableObject cho data, MonoBehaviour cho behavior

ScriptableObject = pure data + immutable logic. MonoBehaviour = scene-bound state.

```csharp
// DO — Item data = ScriptableObject (instance / asset)
[CreateAssetMenu] public class ItemSO : ScriptableObject { public string id; public int maxStack; ... }

// DO — Inventory = MonoBehaviour (scene-bound state)
public class Inventory : MonoBehaviour, ISaveable { List<InventorySlot> slots; ... }

// DON'T — item runtime state (count, durability) trên SO (shared global mutation bug)
public class ItemSO : ScriptableObject { public int count; }  // ← mọi Inventory share count!
```

Technique (spell): logic cast sống trên SO, player chỉ `currentTech.Execute(owner)`.

```csharp
public abstract class TechniqueSO : ScriptableObject {
    public abstract void Execute(PlayerCombat owner);   // ← override per SO subclass
}
public class FireBallSO : TechniqueSO {
    public override void Execute(PlayerCombat owner) { /* spawn projectile */ }
}
```

---

## 6. Pure component cho reusability (R1 pattern)

Stat component (Health/Hunger/…) KHÔNG depend external — chỉ dùng `Time.deltaTime` + internal field. Lý do: NPC humanoid reuse subset.

```csharp
// DO — pure component (HealthComponent)
public class HealthComponent : MonoBehaviour {
    public float maxHP = 100f;
    public float HP;
    public event Action<float> OnHPChanged;
    public void Damage(float amt) { HP = Mathf.Max(0, HP - amt); OnHPChanged?.Invoke(HP); }
}

// DON'T — depend PlayerStats façade (không reuse cho NPC được)
public class HealthComponent : MonoBehaviour {
    PlayerStats stats;
    void Damage(float amt) { stats.TakeDamage(amt); }  // ← NPC không có PlayerStats
}
```

Invariant: stat component dùng được standalone (add vào GO bất kỳ → hoạt động).

---

## 7. Zero alloc / frame trong hot path

Update loop trên 200+ GO (mob + projectile + particle + player + UI) — alloc 1 lambda/frame × 200 = GC spike mỗi frame.

```csharp
// DO — FSM state singleton (R7, no alloc per frame)
public static readonly IState<WolfAI> Idle = new WolfIdle();
Fsm.Init(this, WolfStates.Idle);

// DON'T — new state instance mỗi transition
Fsm.ChangeState(new WolfChase());  // ← 60 alloc/s per wolf × 10 wolves = 600 alloc/s

// DO — cache Vector2 difference
Vector2 diff = target - pos;
float sqrDist = diff.sqrMagnitude;

// DON'T — box value type / closure lambda trong hot path
mobs.FindAll(m => Vector2.Distance(m.pos, player) < 5f);  // ← lambda capture player, alloc
```

Khi cần lambda (event subscribe): 1 lần trong `OnEnable`, không tạo mới mỗi frame.

---

## 8. Save backward compat (schema stability)

`SaveData` struct = API public. Field mới: add optional field + default value. Xoá / rename field = break saves.

```csharp
// DO — add field mới với default
[Serializable] public class PlayerSaveData {
    public float hp;
    public float mana;
    public float wetness;        // ← new R1, default 0 OK cho save cũ
}

// DON'T — rename / xoá field
// public float hunger;   ← xoá = save cũ load NPE
// public float food;     ← rename, JSON unmarshal fail silently → player lose stat
```

Nếu buộc phải break schema: add `saveVersion` int, migrate trong `SaveSystem.TryLoad`. Xem `save-load-pattern` skill.

---

## 9. Test per invariant, không per line

Test verify **behavior** cụ thể: "broken items skip consume", "fixup 30 runs before 50", "save round-trip preserves freshness". Không test "compile passes" hay getter return field value trivial.

```csharp
// DO — invariant-based test
[Test]
public void TryConsume_SkipsBrokenItems_AndConsumesNextAvailable() {
    inv.Add(knife, 1); var slot = inv.slots[0]; slot.durability = 0; // break it
    inv.Add(knife, 1);
    Assert.IsTrue(inv.TryConsume(knife, 1));
    Assert.AreEqual(0, slot.durability, "broken slot untouched");    // ← invariant
    Assert.AreEqual(0, inv.slots[1].count, "available consumed");
}

// DON'T — trivial getter test
[Test]
public void MaxHP_Returns100() { Assert.AreEqual(100, stats.maxHP); }  // ← zero signal
```

Tests xem `Assets/_Project/Tests/EditMode/` cho EditMode pattern, PlayMode cho coroutine/Physics2D.

---

## 10. Một PR = một hạng mục

Feature / refactor / test / fix: tách PR. Lý do: review nhỏ → merge nhanh → bisect dễ khi có bug regression. Audit roadmap R1..R7 tách 8 PR, mỗi PR isolate, tất cả merge clean.

```
// DO — PR lineage
#63 R1 p1 — extract Wetness/Thermal/Permadeath
#66 R1 p2 — extract 7 stat components còn lại
#67 R6    — ISaveable dispatcher

// DON'T — "refactor + new feature" gộp
#99 R6 ISaveable + add NPC + refactor UI — 2000 LoC diff, review flaky
```

Exception: refactor nội bộ của 1 feature mới (chưa user-visible) OK gộp.

---

## Anti-patterns đã eliminated (tham chiếu)

Xem `REFACTOR_HISTORY.md` cho timeline chi tiết.

| Anti-pattern | Fixed by | Rule now |
|---|---|---|
| 58 `FindObjectOfType` trong Update | R3 | `ServiceLocator.Get<T>()` cache |
| `PlayerStats` fallback cast (6 mob) | R2 | `IDamageable` interface |
| God-object `PlayerStats` 618 LoC 82 field | R1 p1 + p2 | 10 pure component family |
| `SaveLoadController` 228 LoC monolith | R6 | `ISaveable` dispatcher |
| Mob AI nested if/else Update() | R7 | `StateMachine<T>` FSM |
| UI poll PlayerStats mỗi frame | R4 | `GameEvents` hub subscribe |

---

## Khi design feature mới

Checklist trước khi code:
1. [ ] Đọc `AGENTS.md` — invariant nào liên quan?
2. [ ] Đọc `ARCHITECTURE.md` §2 — pattern nào áp dụng?
3. [ ] Đọc nguyên tắc này — DO/DON'T nào áp dụng?
4. [ ] Có skill sẵn trong `.agents/skills/` không?
5. [ ] Test invariant gì? (rule 9)
6. [ ] Save schema đổi không? (rule 8)
7. [ ] Event cross-cutting nào cần fire? (rule 2)
