# AGENTS.md — Rules cho AI assistants & contributors

> File này AI assistant (Devin, Claude, Cursor, Copilot…) PHẢI đọc trước khi sửa repo.
> Mục đích: giữ "AI-safe vibe code" — mỗi PR phải qua test gate + lint gate, KHÔNG side-effect.
>
> **Docs tuần tự**:
> 1. **AGENTS.md** (file này) — HARD constraints + rules + CI gates
> 2. [`DESIGN_PRINCIPLES.md`](DESIGN_PRINCIPLES.md) — 10 DO / DON'T với code example
> 3. [`ARCHITECTURE.md`](ARCHITECTURE.md) — module map, pattern inventory, data flow, save pipeline
> 4. [`REFACTOR_HISTORY.md`](REFACTOR_HISTORY.md) — R1..R7 timeline (pattern từ đâu tới)
> 5. [`Documentation/WORLD_MAP_DESIGN.md`](Documentation/WORLD_MAP_DESIGN.md) — 3 biome chi tiết + asset checklist
> 6. [`.agents/skills/README.md`](.agents/skills/README.md) — procedures "làm X như thế nào"

## Project overview

**Wilderness Cultivation Chronicle** — game mobile open-world survival + tu tiên (cultivation),
Unity **6 LTS (6000.4.4f1)**, target **Android (IL2CPP, ARM64)**.

- Engine: Unity 6 LTS · Render: Universal RP · Physics: 2D
- Code chính: `Assets/_Project/Scripts/` (asmdef: `WildernessCultivation`)
- Tests: `Assets/_Project/Tests/EditMode/` + `Assets/_Project/Tests/PlayMode/`
- Bootstrap demo scene: `Tools → Wilderness Cultivation → Bootstrap Default Scene` (Editor wizard)
- CI: GitHub Actions (`build-android.yml`, `test.yml`, `lint.yml`) qua GameCI Docker

## Architecture rules (HARD constraints)

1. **Namespaces theo folder.** `Scripts/Mobs/WolfAI.cs` → `namespace WildernessCultivation.Mobs`.
   File mới phải dùng `using` đúng namespace, KHÔNG flatten vào root.
2. **`PlayerStats` IMPLEMENT `IDamageable`** (R2). Mob/projectile/env damage qua interface — KHÔNG fallback `GetComponent<PlayerStats>()`:
   ```csharp
   var dmg = target.GetComponent<IDamageable>() ?? target.GetComponentInParent<IDamageable>();
   if (dmg != null) dmg.TakeDamage(damage, gameObject);
   ```
   `PlayerStats.TakeDamage(float, GameObject)` overload gọi nội bộ `TakeDamage(float)` (giữ i-frame + status modifier). `source` reserved cho threat/aggro/log tương lai.
3. **Inventory broken items invariant.** `CountOf` + `TryConsume` SKIP slot có `IsBroken==true`.
   Recipe KHÔNG consume món đã hỏng (player phải sửa qua Workbench).
4. **Workbench atomic repair.** Pre-check material → repair → consume material. KHÔNG consume trước.
5. **Save round-trip per-slot.** `Inventory.RestoreState` (R6 ISaveable) phải snapshot count mỗi slot
   trước `Add()` để định vị slot vừa được Add (perishable/durable không stack). Đừng scan-from-zero
   sẽ ghi đè freshness/durability slot trước.
6. **Unity meta files commit chung với asset.** Nếu thêm file `.cs` / `.asset` mới, MUST commit `.meta`
   tương ứng (Unity tự sinh khi mở Editor).
7. **Domain events qua `GameEvents` static hub** (R4). UI / audio / quest / achievement subscribe
   `GameEvents.OnPlayerDied`, `OnRealmAdvanced`, `OnPlayerStatsChanged`, `OnWeatherChanged`, …
   thay vì giữ ref tới `PlayerStats` / `RealmSystem` / `TimeManager`. Publisher fire qua
   `GameEvents.RaiseXxx(...)` (không bao giờ `OnXxx?.Invoke()` trực tiếp ngoài hub).
   Subscriber **PHẢI** subscribe trong `OnEnable` + unsubscribe trong `OnDisable`.
   Test PHẢI gọi `GameEvents.ClearAllSubscribers()` trong `SetUp`/`TearDown` để tránh leak.
   Damage / hit feedback giữ ở `CombatEvents` (concern tần suất cao — mỗi hit).
8. **Save qua `ISaveable` dispatcher** (R6). `SaveLoadController` chỉ enumerate `SaveRegistry` rồi
   gọi `CaptureState` / `RestoreState` trên từng system. Mỗi system tự own slice của mình
   (PlayerStats=vitals, RealmSystem=tier/xp, Inventory=slots, TimeManager=time, WorldGenerator=seed).
   Post-restore cross-system ordering (ReapplySpiritRootMaxHP @30 → ReapplyAccumulatedBonuses @50 → clamp HP @60)
   qua `SaveRegistry.RegisterFixup(owner, order, action)`. Register OnEnable / Unregister OnDisable.
   Test PHẢI gọi `SaveRegistry.ClearAll()` trong `SetUp`/`TearDown` (tương tự rule 7).
   Chi tiết schedule xem `ARCHITECTURE.md` §2.7.
9. **Mob AI qua FSM** (R7). Mob mới nên dùng `StateMachine<T>` + `IState<T>` (`Scripts/Core/StateMachine.cs`).
   Exemplar: `WolfAI` (chase/attack) + `RabbitAI` (wander/flee). State class ở `Scripts/Mobs/States/`,
   singleton `IState<T>` per state (no alloc per frame), state mutable sống trên mob context.
   Transition safe trong OnEnter/OnTick/OnExit (queue + apply sau tick). Xem `.agents/skills/add-mob/`.
10. **NPC humanoid qua composition** (R5 follow-up). NPC mới (vendor / companion / quest giver /
    villager) inherit `CharacterBase` + auto-add pure stat component từ R1 (`HealthComponent`,
    `HungerComponent`, `InvulnerabilityComponent`…) theo role — KHÔNG kéo `PlayerStats` façade
    (tránh kéo Wetness/Thermal/Sanity không cần). Event hub fire qua `GameEvents.OnXxx` với
    arg `object` (subscriber cast — tránh circular namespace). Exemplar:
    - `VendorNPC` — stationary, barter, `TryExecuteTrade` atomic + rollback on full inventory.
    - `CompanionNPC` — follow player FSM (R7) ở `Scripts/World/States/CompanionStates.cs`
      (Idle / Follow / Dead) + hunger decay via `TickSurvival` → starvation damage qua
      `HealthComponent.TickStarvation`. Interact toggle Follow ↔ Stay mode.
    Xem `.agents/skills/add-npc/`.

## Code conventions

- **Indent:** 4 space (KHÔNG tab). LF line endings. UTF-8. Trim trailing whitespace. Final newline.
  → enforced bởi `.editorconfig` + CI lint gate (`.github/workflows/lint.yml`).
- **Brace:** Allman style (`csharp_new_line_before_open_brace = all`).
- **Naming:** `PascalCase` cho type/method/property, `camelCase` cho local/field. KHÔNG `_PascalCase` / `m_camelCase`.
- **Comments:** Bias terse. Comment tiếng Việt OK (codebase hiện có).
- **Imports:** Top of file, KHÔNG nested. Group System / Unity / project namespaces.
- **Tránh:** `Any`, reflection trừ khi cần. Type-cast tường minh.

## Testing — bắt buộc

**Mỗi feature mới hoặc bug fix PHẢI đi kèm test.**

- **EditMode** (synchronous, không Physics2D / coroutine): `Assets/_Project/Tests/EditMode/`
  - asmdef: `WildernessCultivation.Tests.EditMode.asmdef` (`includePlatforms: ["Editor"]`)
  - SetUp/TearDown dùng `Object.DestroyImmediate()`.
- **PlayMode** (coroutine + Physics2D + `Time.deltaTime`): `Assets/_Project/Tests/PlayMode/`
  - asmdef: `WildernessCultivation.Tests.PlayMode.asmdef` (`includePlatforms: []`)
  - SetUp/TearDown dùng `Object.Destroy()` (yield-compatible).
  - `[UnityTest] IEnumerator TestName()` không phải `[Test] void`.

**PlayMode timing trap:**
```csharp
// SAI: WaitForSecondsRealtime(0.5f) với dayLengthSeconds=0.3 sẽ wrap 2 lần thay vì 1.
// SAI: Assert.Less(currentTime01, 0.5f) — frame jump ±16ms làm window 9ms quá hẹp → flaky.
// ĐÚNG: bump dayLengthSeconds (>= 1.0) để per-frame delta nhỏ; assert state change, không exact value.
```
Xem `Assets/_Project/Tests/PlayMode/TimeManagerPlayTests.cs`.

**Test-integration invariant** (R4 + R6):
- Subscribe GameEvents/SaveRegistry trong component OnEnable → `ClearAllSubscribers()` / `ClearAll()` trong test SetUp/TearDown để tránh leak cross-test.

## CI gates — PR phải PASS hết

1. **Build Android (IL2CPP, ARM64)** — compile clean Unity 6000.4.4f1 + Android module.
2. **Run EditMode + PlayMode tests** — pass tất cả case.
3. **Lint / format** — `.editorconfig` whitespace + `dotnet format whitespace --verify`.
4. **Devin Review** — auto bot review, address findings trước khi merge.

⚠️ Hiện tại repo chưa có `UNITY_LICENSE` secret → unity-test-runner skip-noop.
Tests vẫn compile-pass nhưng KHÔNG execute trong CI. Để gate thật, user add secret theo
`README.md` section "GameCI".

## File layout — biết đặt code vào đâu

Chi tiết đầy đủ xem [`ARCHITECTURE.md`](ARCHITECTURE.md) §1. Tóm tắt:

- `Scripts/Core/` — infra: `GameManager`, `TimeManager`, `SaveSystem`, `SaveLoadController` (R6 dispatcher), `GameEvents` + `CombatEvents`, `ServiceLocator`, `ISaveable` + `SaveRegistry`, `StateMachine` + `IState`, `CharacterBase`
- `Scripts/Player/` — `PlayerController`, `PlayerStats` (façade), action components
- `Scripts/Player/Stats/` — R1 pure subsystem components (Health/Hunger/Thirst/Sanity/Mana/Shield/Invuln/Wetness/Thermal/Permadeath)
- `Scripts/Cultivation/` — `RealmSystem`, `SpiritRoot`, TechniqueSO
- `Scripts/Inventory/` + `Scripts/Items/` — `Inventory`, `InventorySlot`, `ItemSO`, `ItemDatabase`
- `Scripts/Crafting/` — `CraftingSystem`, `RecipeSO`, `CraftStationMarker`
- `Scripts/Mobs/` — `MobBase`, `WolfAI` (FSM), `RabbitAI` (FSM), `FoxSpiritAI` / `SnakeAI` / `BossMobAI` (legacy)
- `Scripts/Mobs/States/` — R7 FSM state classes per mob
- `Scripts/Combat/` — `IDamageable`, `Projectile`, `CombatEvents`
- `Scripts/World/` — `WorldGenerator`, `BiomeSO`, `ResourceNode`, `Workbench`, `Campfire`, weather, `VendorNPC` + `TradeOffer` (R5), `CompanionNPC` + `States/CompanionStates` (R5)
- `Scripts/UI/`, `Scripts/Audio/`, `Scripts/Camera/`, `Scripts/Vfx/` — presentation layer
- `Editor/` — `BootstrapWizard`, SO generators

## Skills — procedures sẵn

Xem [`.agents/skills/README.md`](.agents/skills/README.md) cho index đầy đủ.

Skills hiện có: `add-edit-mode-test`, `add-play-mode-test`, `add-mob`, `add-npc` (R5), `add-recipe`, `save-load-pattern`, `bootstrap-scene`, `lint-locally`.

## DO

- Đọc system bạn sửa + test hiện có TRƯỚC khi viết code.
- Mỗi PR = 1 hạng mục (test, fix, feature). Tránh PR đa-mục đích (DESIGN_PRINCIPLES rule 10).
- Sửa production code khi Devin Review tìm bug thật (xem PR #23 SaveLoad, PR #24 Wolf/Fox).
- Test đi kèm phải verify invariant cụ thể, không phải "compile pass" (DESIGN_PRINCIPLES rule 9).
- Reply Devin Review inline khi đã fix (`git_comment` với `in_reply_to`).
- Composition over inheritance cho stat/ability (DESIGN_PRINCIPLES rule 1).
- Pure component (no external deps) — reuse NPC humanoid (DESIGN_PRINCIPLES rule 6).
- Zero alloc/frame trong hot path (DESIGN_PRINCIPLES rule 7).

## DON'T

- KHÔNG modify `.meta` files thủ công (Unity manage).
- KHÔNG modify generated `Assembly-CSharp.csproj` (Unity tự sinh).
- KHÔNG bỏ qua test gate "vì test chỉ skip-noop trên CI" — test logic PHẢI đúng.
- KHÔNG hardcode timing trong PlayMode tests; dùng `dayLengthSeconds` configurable.
- KHÔNG add feature mới mà thiếu test.
- KHÔNG force-push lên `main`.
- KHÔNG commit `Library/`, `Temp/`, `Logs/` (đã `.gitignore`).
- KHÔNG bỏ qua finding 🔴 critical của Devin Review.
- KHÔNG cast concrete thay vì interface (DESIGN_PRINCIPLES rule 3).
- KHÔNG `FindObjectOfType` trong Update — dùng `ServiceLocator.Get<T>()` cache.
- KHÔNG rename / xoá field trong `SaveData` struct (backward compat — DESIGN_PRINCIPLES rule 8).

## References

- [ARCHITECTURE.md](ARCHITECTURE.md) — module map + pattern inventory + save pipeline
- [DESIGN_PRINCIPLES.md](DESIGN_PRINCIPLES.md) — 10 DO/DON'T với code example
- [REFACTOR_HISTORY.md](REFACTOR_HISTORY.md) — R1..R7 log (patterns introduced)
- [.agents/skills/README.md](.agents/skills/README.md) — skill index
- [README.md](README.md) — project setup
- [Documentation/MVP_SCOPE.md](Documentation/MVP_SCOPE.md) — feature scope
- [Documentation/ROADMAP.md](Documentation/ROADMAP.md) — long-term roadmap
- `.editorconfig` — format rules
- `.github/workflows/` — CI definitions
