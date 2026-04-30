# Skills index — `.agents/skills/`

> Procedures "làm X như thế nào" cho AI assistant / contributor. Mỗi skill là
> 1 file `SKILL.md` đủ tự-chứa, không phụ thuộc skill khác.
>
> Đọc trước khi code: `AGENTS.md` (rules) → `DESIGN_PRINCIPLES.md` (DO/DON'T) →
> `ARCHITECTURE.md` (pattern inventory) → skill cụ thể.

## Skills hiện có

| Skill | Khi dùng | Prerequisites |
|---|---|---|
| [`add-edit-mode-test/`](add-edit-mode-test/SKILL.md) | Thêm test pure logic (không Physics2D, không coroutine) | — |
| [`add-play-mode-test/`](add-play-mode-test/SKILL.md) | Thêm test cần `Time.deltaTime`, Physics2D, coroutine | — |
| [`add-mob/`](add-mob/SKILL.md) | Thêm quái mới — FSM (R7, khuyến nghị) hoặc legacy Update | `IDamageable`, `StateMachine<T>`, `CharacterBase` |
| [`add-npc/`](add-npc/SKILL.md) | Thêm NPC humanoid (vendor/companion/quest giver) reuse R1 stat components. Exemplars: `VendorNPC` (stationary) + `CompanionNPC` (FSM follow) | `CharacterBase`, `IInteractable`, `ISaveable`, `StateMachine<T>`, R1 components |
| [`add-recipe/`](add-recipe/SKILL.md) | Thêm RecipeSO + ingredient + craft station | `RecipeSO`, `IStationGate` |
| [`save-load-pattern/`](save-load-pattern/SKILL.md) | Thêm field vào save/load không break legacy | `ISaveable` (R6), `SaveRegistry` |
| [`bootstrap-scene/`](bootstrap-scene/SKILL.md) | Sinh demo scene playable từ Editor wizard | `BootstrapWizard` |
| [`lint-locally/`](lint-locally/SKILL.md) | Chạy `dotnet format` + whitespace check local trước push | `.editorconfig` |

## Skills chưa có (TODO — add khi cần)

| Skill đề xuất | Khi dùng | Owner file sẽ là |
|---|---|---|
| `add-stat-component` | Thêm component `Scripts/Player/Stats/` mới (vd FatigueComponent) | PlayerStats façade + test |
| `add-isaveable-system` | System mới cần persistence (implement `ISaveable` + Order + Fixup) | `Scripts/Core/ISaveable.cs` + SaveRegistry |
| `add-fsm-state` | Thêm state mới cho mob FSM (vd WolfFleeState khi HP thấp) | `Scripts/Mobs/States/` |
| `add-game-event` | Thêm domain event vào `GameEvents` hub | `Scripts/Core/GameEvents.cs` |
| `add-ui-panel` | Thêm UI panel subscribe `GameEvents` | `Scripts/UI/` |
| `refactor-mob-to-fsm` | Convert mob legacy (Fox/Snake/Bat/…) sang FSM | MobBase + States/ |
| `add-technique` | Thêm công pháp mới (TechniqueSO subclass) | `Scripts/Cultivation/` |
| `add-combat-companion` | Companion combat (AI attack hostile mobs to assist player) — sau khi PR B2 merge | Extend `CompanionNPC` với combat state + aggro logic |

## Workflow khi add feature mới

```
1. Đọc AGENTS.md — có invariant nào liên quan feature mình làm không?
2. Đọc DESIGN_PRINCIPLES.md — pattern nào áp dụng (1-10)?
3. Đọc ARCHITECTURE.md — chỗ nào trong module map? Pattern nào đã có?
4. Tìm skill phù hợp trong bảng trên.
   → Nếu có skill: theo skill bước-bước.
   → Nếu không: xem "Skills chưa có" → đề xuất skill + code theo exemplar
     gần nhất trong repo.
5. Code + test + lint.
6. PR tách theo scope (rule 10 DESIGN_PRINCIPLES).
7. Reply Devin Review + fix CI red.
8. Merge khi lint + Build Android + test đều pass (hoặc skip-noop
   do thiếu UNITY_LICENSE — xem AGENTS.md CI gate section).
```

## Skill format convention

Mỗi `SKILL.md`:
1. **Tiêu đề + mục tiêu** (1-2 dòng).
2. **Bước thực hiện** (numbered list, ~5-10 bước).
3. **Reference implementations** (link file cụ thể trong repo).
4. **Pitfalls** (gotcha đã gặp trong PR trước).
5. **Test checklist** (nếu skill liên quan feature — bắt buộc có test).

Giữ skill **< 100 dòng**. Detail sâu hơn vào `ARCHITECTURE.md` hoặc `DESIGN_PRINCIPLES.md`.
