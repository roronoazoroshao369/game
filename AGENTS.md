# AGENTS.md — Rules cho AI assistants & contributors

> File này AI assistant (Devin, Claude, Cursor, Copilot…) PHẢI đọc trước khi sửa repo.
> Mục đích: giữ "AI-safe vibe code" — mỗi PR phải qua test gate + lint gate, KHÔNG side-effect.

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
   File mới phải dùng `using` đúng namespace, KHÔNG flatten.
2. **`PlayerStats` IMPLEMENT `IDamageable`.** Mob/projectile/env damage player qua interface — KHÔNG cần fallback `GetComponent<PlayerStats>()`:
   ```csharp
   var dmg = target.GetComponent<IDamageable>() ?? target.GetComponentInParent<IDamageable>();
   if (dmg != null) dmg.TakeDamage(damage, gameObject);
   ```
   `PlayerStats.TakeDamage(float, GameObject)` overload nội bộ gọi lại `TakeDamage(float)` (giữ i-frame + status modifier). `source` reserved cho threat/aggro/log sau này. Xem `BossMobAI.cs`, `WolfAI.cs`, `FoxSpiritAI.cs` cho pattern hiện hành.
3. **Inventory broken items invariant.** `CountOf` + `TryConsume` SKIP slot có `IsBroken==true`.
   Recipe không được consume món đã hỏng (player muốn sửa qua Workbench).
4. **Workbench atomic repair.** Pre-check material → repair → consume material. KHÔNG consume trước.
5. **Save round-trip per-slot.** `SaveLoadController.RestoreInventory` phải snapshot count mỗi slot
   trước `Add()` để định vị slot vừa được Add (perishable/durable không stack). Đừng scan-from-zero
   sẽ ghi đè freshness/durability slot trước.
6. **Unity meta files commit chung với asset.** Nếu thêm file `.cs`/`.asset` mới, MUST commit `.meta`
   tương ứng (Unity tự sinh khi mở Editor).

## Code conventions

- **Indent:** 4 space (KHÔNG tab). LF line endings. UTF-8. Trim trailing whitespace. Final newline.
  → enforced bởi `.editorconfig` + CI lint gate (`.github/workflows/lint.yml`).
- **Brace:** Allman style (`csharp_new_line_before_open_brace = all`).
- **Naming:** `PascalCase` cho type/method/property, `camelCase` cho local/field, `_PascalCase`
  hoặc `m_camelCase` KHÔNG dùng — chỉ field public/serialized name `camelCase`.
- **Comments:** Bias terse. Comment block tiếng Việt OK (xem code base hiện có).
- **Imports:** Top of file, KHÔNG nested. Group System / Unity / project namespaces.
- **Tránh:** `Any`, `getattr`/reflection trừ khi cần thiết. Type-cast tường minh.

## Testing — bắt buộc

**Mỗi feature mới hoặc bug fix PHẢI đi kèm test.**

- **EditMode** (synchronous, không Physics2D / coroutine): `Assets/_Project/Tests/EditMode/`
  - asmdef: `WildernessCultivation.Tests.EditMode.asmdef` (`includePlatforms: ["Editor"]`)
  - SetUp/TearDown dùng `Object.DestroyImmediate()`.
- **PlayMode** (coroutine + Physics2D + `Time.deltaTime`): `Assets/_Project/Tests/PlayMode/`
  - asmdef: `WildernessCultivation.Tests.PlayMode.asmdef` (`includePlatforms: []`)
  - SetUp/TearDown dùng `Object.Destroy()` (yield-compatible).
  - `[UnityTest] IEnumerator TestName()` không phải `[Test] void`.

**Pattern PlayMode timing trap:**
```csharp
// SAI: WaitForSecondsRealtime(0.5f) với dayLengthSeconds=0.3 sẽ wrap 2 lần thay vì 1.
// SAI: Assert.Less(currentTime01, 0.5f) — frame jump ±16ms làm window 9ms quá hẹp → flaky.
// ĐÚNG: bump dayLengthSeconds (>= 1.0) để per-frame delta nhỏ; assert state change, không assert exact value.
```

Xem `Assets/_Project/Tests/PlayMode/TimeManagerPlayTests.cs` cho ví dụ.

## CI gates — PR phải PASS hết

1. **Build Android (IL2CPP, ARM64)** — compile clean Unity 6000.4.4f1 + Android module.
2. **Run EditMode + PlayMode tests** — pass tất cả case.
3. **Lint / format** — `.editorconfig` whitespace + `dotnet format whitespace --verify`.
4. **Devin Review** — auto bot review, address findings trước khi merge.

⚠️ Hiện tại repo chưa có `UNITY_LICENSE` secret → unity-test-runner skip-noop.
Tests vẫn compile-pass nhưng KHÔNG execute trong CI. Để gate thật, user cần add secret theo
`README.md` section "GameCI".

## File layout — biết đặt code vào đâu

- `Scripts/Core/` — `GameManager`, `TimeManager`, `SaveLoadController`, `SaveSystem`
- `Scripts/Player/` — `PlayerController`, `PlayerStats` (façade), action components (Dodge/Sleep/Fishing/…)
- `Scripts/Player/Stats/` — `WetnessComponent`, `ThermalComponent`, `PermadeathHandler` (subsystem extracts từ R1; PlayerStats tự auto-add trong Awake)
- `Scripts/Cultivation/` — `RealmSystem`, `SpiritRoot`, công pháp, breakthrough
- `Scripts/Inventory/` — `Inventory`, `InventorySlot`
- `Scripts/Crafting/` — `CraftingSystem`, `RecipeSO`, `CraftStationMarker`
- `Scripts/Mobs/` — `MobBase`, `WolfAI`, `FoxSpiritAI`, `RabbitAI`, `BossMobAI`
- `Scripts/Combat/` — `IDamageable`, `Projectile`
- `Scripts/UI/` — `*UI` panels, `VirtualJoystick`, `SkillButton`
- `Scripts/World/` — `WorldGenerator`, `BiomeSO`, `ResourceNode`, `Workbench`, `Campfire`,
  `StorageChest`, `Shelter`, weather
- `Scripts/Items/` — `ItemSO`, `ItemDatabase`
- `Editor/` — `BootstrapWizard`, build-time SO generators

## Skills (.agents/skills/) — procedures sẵn

- `add-edit-mode-test/` — viết EditMode test cho 1 system mới
- `add-play-mode-test/` — viết PlayMode test (Physics2D + coroutine)
- `add-mob/` — thêm MobAI mới (Wolf/FoxSpirit pattern)
- `add-recipe/` — thêm RecipeSO + ingredient + station
- `lint-locally/` — chạy `dotnet format whitespace --verify` local

## DO

- Đọc system bạn sửa + test hiện có TRƯỚC khi viết code.
- Mỗi PR = 1 hạng mục (test, fix, feature). Tránh PR đa-mục đích.
- Sửa production code khi Devin Review tìm bug thật (xem `SaveLoadController` PR #23,
  `WolfAI`/`FoxSpiritAI` PR #24).
- Test đi kèm phải verify invariant cụ thể, không phải "compile pass".
- Reply Devin Review inline khi đã fix (`git_comment` với `in_reply_to`).

## DON'T

- KHÔNG modify `.meta` files thủ công (Unity manage).
- KHÔNG modify generated `Assembly-CSharp.csproj` (Unity tự sinh).
- KHÔNG bỏ qua test gate "vì test chỉ skip-noop trên CI" — test logic phải đúng.
- KHÔNG hardcode timing trong PlayMode tests; dùng `dayLengthSeconds` configurable.
- KHÔNG add new feat mà thiếu test.
- KHÔNG force-push lên `main`.
- KHÔNG commit `Library/`, `Temp/`, `Logs/` (đã `.gitignore`).
- KHÔNG bỏ qua finding 🔴 critical của Devin Review.

## References

- README: project overview + setup
- Documentation/MVP_SCOPE.md: feature scope
- .editorconfig: format rules
- .github/workflows/: CI definitions
