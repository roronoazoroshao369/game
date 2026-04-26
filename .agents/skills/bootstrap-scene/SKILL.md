# Skill: Bootstrap default scene

Mục tiêu: sinh demo scene playable từ Editor wizard.

## Bước

1. Mở Unity Editor (project: `/path/to/game`).
2. Menu: `Tools → Wilderness Cultivation → Bootstrap Default Scene`.
3. Wizard chạy, tự sinh:
   - Sprites placeholder (solid-color)
   - `ItemSO` / `RecipeSO` / `SpiritRootSO` / `StatusEffectSO` / `BiomeSO`
   - `ItemDatabase` asset
   - Prefabs: Player, Tree, Rock, Rabbit, Wolf, FoxSpirit, Campfire, WaterSpring, StorageChest, Workbench, Projectile
   - Scene `Assets/Scenes/MainScene.unity` đã wire:
     - `GameManager` (chứa CraftingSystem + TimeManager + SaveLoadController)
     - `WorldGenerator`
     - `Player` (với Stats / Inventory / RealmSystem / Combat / Dodge / Sleep / Fishing / Interact actions)
     - `MobSpawner` (Rabbit + Wolf + FoxSpirit)
     - Camera follow Player
     - `Campfire` + `StorageChest` + `Workbench` đặt cạnh spawn point
     - UI Canvas:
       - 7 stat bars (HP/Hunger/Thirst/Sanity/Mana/BodyTemp/Encumbrance)
       - VirtualJoystick + Skill row (Combat/Dodge/Magic Treasure)
       - `InventoryUI` + `CraftingUI` + `RealmUI` panels
4. Press Play → demo runnable.

## Phím tắt

- WASD / Joystick: di chuyển
- E: interact (cây, đá, campfire, chest, workbench)
- J: tấn công melee
- Space: dodge (i-frames + cost mana)
- T: thiền (Tụ Linh Quyết)
- B: đột phá realm
- M: open Magic Treasure menu
- F: câu cá (cần water spring)
- I: open Inventory (toggle)
- C: open Crafting (toggle)
- R: open Realm UI (toggle)
- ESC: đóng panel

## Pitfalls

- **Bootstrap chạy 2 lần**: idempotent — sẽ overwrite asset cũ. KHÔNG accumulate. Nếu muốn reset
  từ đầu: xóa folder `Assets/_Generated/` trước khi run.
- **`Library/` folder lần đầu**: Unity cần ~5–10 phút import. Sau đó cached.
- **Wizard fail vì missing meta**: nếu git checkout branch khác tạm thời, nhớ reload Unity Project
  view (Right-click → Reimport All) trước khi run wizard.

## Reference

- `Assets/_Project/Editor/BootstrapWizard.cs`
