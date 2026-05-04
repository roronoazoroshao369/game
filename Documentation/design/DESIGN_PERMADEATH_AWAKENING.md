---
name: design-permadeath-awakening
audience: both
status: proposal
scope: Permadeath + soul awakening gate. Cần user confirm trước khi code.
depends-on:
  - GDD.md
  - MVP_SCOPE.md
---
# Design — Permadeath + Awakening Gate (v1)

> Status: Proposal. Cần user confirm trước khi code.
> Liên quan: `PlayerStats`, `RealmSystem`, `MeditationAction`, `SaveSystem`,
> `SaveLoadController`, `TutorialHUD`, `WorldGenerator`, `TimeManager`.

## 1. Tóm tắt mục tiêu

- Game start = **survival** (giống DST). Chết = mất hết, chơi lại từ đầu.
- **Tu tiên không default-on**. Mặc định player là **Thường Nhân**, KHÔNG thiền,
  KHÔNG đột phá được. Phải khai mở (awakening) qua kì ngộ.

## 2. Permadeath rules (đã chốt từ Q1-Q3)

| # | Rule | Decision |
|---|------|----------|
| Q1 | Permadeath strictness | **Hard** — wipe save slot, world seed mới |
| Q2 | Death drop | Tất cả inventory → **Mộ Phần** tại vị trí chết, run sau pickup |
| Q3 | Min survival để qualify awakening | **7 ngày** survived |

### 2.1 Death flow (`PlayerStats.Die()`)

```
1. Snapshot inventory + position + worldSeed + daysSurvived → graveyard.json
2. SaveSystem.Delete()         — xoá save slot
3. WorldGenerator.seed         — random new seed cho run sau
4. Show "Bạn đã chết" overlay → reload scene
```

### 2.2 Tombstone (`Mộ Phần`)

**Persistence file**: `graveyard.json` ở `Application.persistentDataPath`.
**KHÔNG bị xoá khi `SaveSystem.Delete()`** — đây là cross-run state.
**Cap**: 10 tombstone tối đa cùng tồn tại; entry cũ nhất bị FIFO drop khi vượt cap.

```json
{
  "tombstones": [
    {
      "id": "tomb_<timestamp>",
      "worldSeed": 12345,
      "position": [x, y, z],
      "daySurvived": 12,
      "items": [{ "itemId": "wood", "count": 5, "freshRemaining": -1, "durability": -1 }],
      "previousLifeRealmTier": 0,
      "previousLifeWasAwakened": false
    }
  ]
}
```

**Spawn rule trong run mới (world seed mới):**
- Mỗi run mới, `WorldGenerator` đọc graveyard list.
- Mỗi tombstone spawn tại 1 vị trí **random** trong world (deterministic theo
  new world seed + tombstone id để re-load idempotent).
- Visual: tombstone dùng sprite tone xám/đen rõ rệt → nhìn thấy trên minimap.
- Player E-interact → drain items vào inventory.
- Empty tombstone tự destroy.

### 2.3 Save scope

- `save_slot_0.json` (run hiện tại) — **xoá khi chết**.
- `graveyard.json` (lịch sử các đời) — **persist mãi**.
- `meta.json` (settings: volume, awakening unlocks lifetime stats) — **persist mãi**.

## 3. Awakening (khai mở tu tiên)

### 3.1 State mới: `IsAwakened`

Thêm vào `PlayerStats` field `bool IsAwakened`. Mặc định `false`.
- `MeditationAction.Update` early-return nếu `!IsAwakened`.
- `RealmSystem.TryBreakthrough` early-return false nếu `!IsAwakened`.
- `RealmUI` hiển thị "Thường Nhân" thay vì "Phàm Nhân (Luyện Khí 0)" khi
  chưa awaken.

### 3.2 Điều kiện khai mở (cần & đủ)

**Điều kiện cần (gating):**
- `daysSurvived >= 7`.
- HP > 50% maxHP (không khai mở trong trạng thái thoi thóp).
- Sanity > 50 (tâm trí phải tỉnh táo).

**Điều kiện đủ (kì ngộ — chọn 1 trong các trigger):**

| Trigger | Mô tả | Spawn rate |
|---|---|---|
| Linh Tuyền (Spirit Spring) | World event spawn 1 spring đặc biệt sau day 7. Player drink → roll awaken. | 1 spring per world, hidden tile |
| Linh Quả (Spirit Fruit) | Drop từ FoxSpirit AI khi giết sau day 7 (10% drop). Eat → roll awaken. | 10% per FoxSpirit kill |
| Awakening Altar | Static prop spawn random trong "Linh-rich" biome sau day 7. Player meditate cạnh (giả meditate, force-allowed) → roll awaken. | 1 altar per world |

> **MVP cut**: chỉ implement **Linh Tuyền** + **Linh Quả** ở phase 1. Altar để
> phase 2 vì cần biome system phức tạp hơn.

### 3.3 Awakening roll (Q4 — confirmed)

Khi player trigger awakening:

| Outcome | Probability | Effect |
|---|---|---|
| **Phàm căn (fail)** | 50% | Không thành. Vẫn là Thường Nhân. Trigger consumed (Linh Quả/Tuyền hết). Phải tìm kì ngộ khác. |
| **Tạp linh căn** | 35% | Awaken thành công, weak root (`SpiritRootGrade.Tap`). |
| **Đơn linh căn** | 13% | Awaken, single-element root (`SpiritRootGrade.Don`), ngẫu nhiên Kim/Mộc/Thuỷ/Hoả/Thổ. |
| **Thiên linh căn** | 2% | Awaken, heavenly root (`SpiritRootGrade.Thien`). UI flash + SFX hoành tráng. |

**Tổng fail rate: 50%.** Điều chỉnh được qua `AwakeningConfigSO`.

### 3.4 Awakening event flow

```
1. Player drink Linh Tuyền / eat Linh Quả với conditions cần thỏa mãn
2. AwakeningSystem.TryAwaken() roll outcome
3. Fail → toast "Linh khí tản mất, ngươi chưa đủ duyên..."
4. Success → 
   - PlayerStats.IsAwakened = true
   - SpiritRoot.SetSpiritRoot(rolledRoot)
   - PlayerStats.ReapplySpiritRootMaxHP()
   - RealmSystem.currentTier = 0 ("Phàm Nhân" → giờ là entry point Luyện Khí)
   - UI flash + breakthrough SFX
   - Toast "Khai mở [grade] [element] linh căn!"
```

## 4. File changes (planned)

### New files
- `Scripts/Cultivation/AwakeningSystem.cs` — orchestrator: kiểm tra conditions, roll outcome.
- `Scripts/Cultivation/AwakeningConfigSO.cs` — ScriptableObject tuning (probabilities, min days).
- `Scripts/Core/Graveyard.cs` — persistence cho tombstone list (separate file).
- `Scripts/World/Tombstone.cs` — `IInteractable` prop, drain items khi interact.
- `Scripts/World/SpiritSpring.cs` — `IInteractable` prop, drink → trigger awaken.
- `Scripts/Items/SpiritFruitItem` (data: thêm `ItemSO` asset trigger, không phải class mới — hook qua `SpiritFruitConsumeAction` component).
- `Tests/EditMode/AwakeningSystemTests.cs`
- `Tests/EditMode/GraveyardTests.cs`
- `Tests/EditMode/TombstoneTests.cs`

### Modified
- `PlayerStats.cs` — thêm `IsAwakened`, modify `Die()` để dump tombstone + delete save + reseed.
- `RealmSystem.cs` — gate `TryBreakthrough` + `AddCultivationXp` qua `IsAwakened` (no-op nếu chưa khai mở).
- `MeditationAction.cs` — gate `StartMeditation()` qua `IsAwakened`.
- `SaveSystem.cs` — `PlayerSaveData.isAwakened` field; `Delete()` KHÔNG đụng `graveyard.json`.
- `SaveLoadController.cs` — round-trip `IsAwakened`.
- `TutorialHUD.cs` — welcome message phản ánh "Thường Nhân"; objective list điều chỉnh.
- `RealmUI.cs` — hiển thị "Thường Nhân" pre-awaken.
- `WorldGenerator.cs` — spawn tombstones từ Graveyard; spawn SpiritSpring after day 7.
- `Mobs/FoxSpiritAI.cs` — drop Linh Quả 10% sau day 7.

## 5. Test plan (EditMode + PlayMode)

- `Awaken_BlockedBeforeDay7` — daysSurvived=5 → TryAwaken returns NotEligible.
- `Awaken_BlockedWhenLowHP` — HP=20% → NotEligible.
- `Awaken_RollDistribution` — fix Random seed, 10000 rolls match probability ±2%.
- `Awaken_SuccessSetsRootAndRealmEntry` — IsAwakened=true, SpiritRoot.Current set, currentTier=0.
- `Awaken_FailConsumesTrigger` — fail outcome still consumes Linh Quả/Tuyền.
- `Meditation_BlockedWhenNotAwakened` — StartMeditation no-op.
- `Breakthrough_BlockedWhenNotAwakened` — TryBreakthrough returns false.
- `Death_DumpsInventoryToGraveyard` — die → graveyard.json has new entry with all items.
- `Death_DeletesSaveSlot_KeepsGraveyard` — save_slot_0.json gone, graveyard.json intact.
- `Death_GeneratesNewSeed` — WorldGenerator.seed differs after death.
- `Tombstone_InteractTransfersItems` — interact drains items into player inventory.
- `Tombstone_EmptyDestroys` — sau khi rỗng, tombstone gone.
- `Graveyard_PersistsAcrossSessions` — write → reload static state → readable.

## 6. Open questions cần user confirm

- ✅ Q1-Q3: locked.
- ❓ **Q4**: proposed 30% fail / 50% Tạp / 18% Đơn / 2% Thiên — OK?
- ❓ Awakening triggers: chỉ Linh Tuyền + Linh Quả MVP (cắt Altar) — OK?
- ❓ Tombstone spawn: cố định gần spawn point (deterministic theo new seed) thay vì vị trí chết cũ — OK?
- ❓ Số tombstone tối đa hiển thị 1 lúc trong world: 5 (cap để không clutter) — OK?
- ❓ Lifetime stat tracking: lưu "tổng số đời", "best daysSurvived", "đã từng đột phá Trúc Cơ" trong `meta.json`? Nice-to-have, có thể defer.
