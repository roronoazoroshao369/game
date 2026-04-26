# 🏔️ Wilderness Cultivation Chronicle (*Hoang Vực Tu Tiên Ký*)

[![Build Android APK](https://github.com/roronoazoroshao369/game/actions/workflows/build-android.yml/badge.svg)](https://github.com/roronoazoroshao369/game/actions/workflows/build-android.yml)
[![Unity Tests](https://github.com/roronoazoroshao369/game/actions/workflows/test.yml/badge.svg)](https://github.com/roronoazoroshao369/game/actions/workflows/test.yml)

> Game **mobile open-world survival + tu tiên** lấy cảm hứng từ *Don't Starve Together* và *Quỷ Cốc Bát Hoang*.
> Engine: **Unity 2022 LTS** · Nền tảng: **Android** (mở rộng iOS/PC sau).

> ⚠️ Workflow `Build Android` và `Unity Tests` sẽ tự skip (pass-noop) cho đến khi repo có Unity license secret. Xem [GameCI section](#-gameci-auto-build-apk-trên-mỗi-pr) để add secret.

---

## 📂 Cấu trúc dự án

```
game/
├── Assets/_Project/
│   ├── Scripts/
│   │   ├── Core/         GameManager, TimeManager (day/night), SaveSystem, ItemDatabase
│   │   ├── Player/       PlayerController (touch joystick + WASD), PlayerStats, PlayerCombat
│   │   ├── Survival/     (HP/Hunger/Thirst/Sanity/Mana đã tích hợp trong PlayerStats)
│   │   ├── Cultivation/  RealmSystem (12 tier: Phàm Nhân + Luyện Khí 1–9 + Trúc Cơ Sơ/Trung/Hậu), TechniqueSO, MeditationAction, SwordQiSlashSO, FireBallSO, SpiritGatheringArraySO
│   │   ├── Combat/       IDamageable, Projectile, SpiritArray
│   │   ├── Inventory/    ItemSO, Inventory (16 slot), MagicTreasureSO (pháp bảo)
│   │   ├── Crafting/     RecipeSO, CraftingSystem, CraftStationMarker (Campfire/AlchemyFurnace gate qua IStationGate)
│   │   ├── World/        WorldGenerator (multi-biome), BiomeSO, ResourceNode, IInteractable, Campfire, WaterSpring, AlchemyFurnace, BossPortal
│   │   ├── Mobs/         MobBase, RabbitAI, WolfAI, FoxSpiritAI, MobSpawner, BossMobAI (đa pha)
│   │   ├── Player/       PlayerController, PlayerStats (IsWarm), PlayerCombat, InteractAction, SleepAction
│   │   └── UI/           VirtualJoystick, StatBarUI, InventoryUI, CraftingUI, RealmUI, SkillButton, InteractPromptUI
│   ├── Art/              (sprites — bạn tự thêm)
│   ├── Audio/
│   ├── Data/SO/          (ScriptableObject assets — tạo trong Unity Editor)
│   ├── Prefabs/
│   └── Scenes/           Game.unity (bạn tạo)
├── Documentation/        GDD.md, ROADMAP.md, MVP_SCOPE.md
└── README.md
```

---

## ⚡ Quickstart: Bootstrap Default Scene (1 click)

Repo đã có sẵn `Editor/BootstrapWizard.cs`. Sau khi clone xong và mở project trong Unity 2022 LTS:

1. Đợi Unity import xong (lần đầu sẽ generate `Library/`).
2. Menu **Tools → Wilderness Cultivation → Bootstrap Default Scene**.
3. Đợi 5-10s. Sẽ sinh ra:
   - Sprites placeholder (PNG ô vuông màu) tại `Assets/_Project/Sprites/`
   - ScriptableObject assets (Items / Recipes / SpiritRoots / StatusEffects / Biomes / ItemDatabase) tại `Assets/_Project/SOs/`
   - Prefabs (Player / Tree / Rock / Rabbit / Campfire / WaterSpring / Projectile) tại `Assets/_Project/Prefabs/`
   - `Assets/Scenes/MainScene.unity` đã wire sẵn GameManager + WorldGenerator + Player + Camera follow + UI bars
4. Mở `Assets/Scenes/MainScene.unity` → bấm **Play**.

> ⚠️ Sprites là placeholder solid-color — thay bằng art thật khi có. Wizard có thể chạy lại nhiều lần (idempotent, ghi đè asset cũ).

Hệ thống phím cơ bản trong scene mặc định:
- **WASD / arrow** → di chuyển
- **Left Shift** → né (dodge / roll — i-frames trong ~0.25s, tốn 5 Linh Khí)
- **E** → tương tác (lửa trại, suối nước)
- **T** → bật/tắt đuốc
- **J** → đánh (mặc định `PlayerCombat.attackKey`)
- **B** → dùng pháp bảo (cần item đã craft)
- **M** → ngồi thiền (cần xa boss/mob)

> Câu cá (`FishingAction`, phím F), repair workbench, storage chest đã có code nhưng wizard chưa add vào scene mặc định. Tự gắn component + tạo prefab khi cần.

---

## 🚀 Hướng dẫn setup lần đầu (manual / advanced)

### 1. Cài Unity Hub & Unity 2022 LTS
- Tải **Unity Hub**: https://unity.com/download
- Trong Hub → Installs → Install Editor → chọn **Unity 2022.3.x LTS**
- Kèm modules: **Android Build Support** (gồm OpenJDK + Android SDK & NDK Tools)

### 2. Clone repo & mở project
```bash
git clone https://github.com/roronoazoroshao369/game.git
```
Trong Unity Hub → **Add → Add project from disk** → chọn folder `game`.
Unity sẽ tự generate `Library/`, `ProjectSettings/`, `Packages/manifest.json` (đã được .gitignore).

### 3. Cấu hình project lần đầu (trong Unity Editor)

#### a. Packages cần cài (Window → Package Manager)
- `2D Sprite` (com.unity.2d.sprite)
- `2D Tilemap Editor` (com.unity.2d.tilemap)
- `TextMeshPro` (sẽ tự prompt khi mở scene đầu tiên — bấm Import TMP Essentials)
- `Cinemachine` (optional — camera follow)
- `Universal RP` (nếu muốn dùng 2D Lights — optional)

#### b. Build target → Android
File → Build Settings → chọn **Android** → Switch Platform.

#### c. Project Settings (Edit → Project Settings)
- **Player → Other Settings:**
  - Scripting Backend: **IL2CPP**
  - Target Architectures: **ARM64** (bỏ ARMv7 vì Google Play yêu cầu 64-bit)
  - Minimum API Level: **Android 8.0 (API 26)**
  - Target API Level: Automatic (highest installed)
- **Player → Resolution and Presentation:**
  - Default Orientation: Landscape Left (hoặc Auto Rotation tuỳ ý)
- **Quality:** giảm shadow, AA, target FPS 30 cho mobile tầm trung.

### 4. Layers & Tags (Edit → Project Settings → Tags and Layers)
Thêm các Layers:
- `Player`
- `Mob`
- `Resource`
- `CraftStation`

Thêm Tag: `Player`.

---

## 🎮 Tạo scene `Game.unity` (15 phút)

### Bước 1 — Player GameObject
Tạo empty `Player` (Tag: Player, Layer: Player):
- Add `Rigidbody2D` (Body Type: Dynamic, Gravity Scale 0, Freeze rotation Z)
- Add `CircleCollider2D` (radius 0.4)
- Add `SpriteRenderer` (drag tạm 1 sprite hình tròn nào đó)
- Add scripts:
  - `PlayerController`
  - `PlayerStats`
  - `PlayerCombat` (set `hitMask` = Layer Mob + Resource)
  - `Inventory` (class trong namespace `WildernessCultivation.Items`)
  - `CraftingSystem` (set `stationMask` = Layer CraftStation)
  - `RealmSystem`
  - `MeditationAction`
  - `DodgeAction` (né — phím Left Shift mặc định, tốn 5 Linh Khí)

### Bước 2 — World gen
Tạo empty `World`:
- Add `WorldGenerator`
- Set `seed`, `size` (vd 100×100)
- Drag prefab Tree / Rock / GrassBush vào (xem bước 4)
- Drag `Player` vào field `player`

### Bước 3 — Time Manager
Tạo empty `_GameManager`:
- Add `GameManager`
- Add `TimeManager`
- Add `SaveLoadController`
- Drag references (player, timeManager, …)
- Tạo SpriteRenderer fullscreen màu đen với alpha=0 → drag vào `Light2DProxy.fallbackOverlay` để có hiệu ứng ngày/đêm.

### Bước 4 — Tạo data ScriptableObjects
Trong Project: chuột phải `Data/SO/Items/` → **Create → WildernessCultivation → Item**

Tạo các item MVP:

| itemId | displayName | Category | restoreHunger | restoreThirst | restoreHP | maxStack |
|---|---|---|---:|---:|---:|---:|
| `wood` | Gỗ | Material | 0 | 0 | 0 | 99 |
| `stone` | Đá | Material | 0 | 0 | 0 | 99 |
| `grass` | Cỏ | Material | 0 | 0 | 0 | 99 |
| `raw_meat` | Thịt sống | Food | 8 | 0 | -2 | 20 |
| `cooked_meat` | Thịt nướng | Food | 25 | 0 | 5 | 20 |
| `clean_water` | Nước sạch | Drink | 0 | 30 | 0 | 20 |
| `spirit_herb` | Linh thảo | SpiritHerb | 0 | 0 | 0 | 50 |
| `wood_axe` | Rìu gỗ | Tool | — | — | toolPower=10, weaponDamage=12 | 1 |

Tạo recipes (chuột phải → Create → WildernessCultivation → Recipe):
- **Lửa trại** (cần 3 wood + 2 stone, output prefab campfire — implement sau, hoặc output 1 item placeholder)
- **Thịt nướng** (cần 1 raw_meat, requiredStation = Campfire, output 1 cooked_meat)
- **Rìu gỗ** (cần 2 wood + 1 stone, output 1 wood_axe)

Tạo công pháp:
- Chuột phải `Data/SO/Techniques/` → **Create → WildernessCultivation → Techniques → Sword Qi Slash**
- Set range=4, damage=18, manaCost=20.
- Drag asset này vào field `equippedTechnique` của Player.

Tạo `ItemDatabase`:
- Chuột phải `Data/SO/` → **Create → WildernessCultivation → Item Database**
- Drag tất cả ItemSO đã tạo vào list items.

### Bước 5 — Resource Node prefabs
Tạo prefab Tree:
- Empty `Tree`, SpriteRenderer (sprite cây), CircleCollider2D, Layer: Resource
- Add `ResourceNode`: maxHP=30, drops = [{wood, 2-4}, {grass, 0-1}]

Tạo prefab Rock: drops = [{stone, 2-3}]
Tạo prefab GrassBush: drops = [{grass, 1-2}, {spirit_herb, 0-1}]

Drag 3 prefab này vào `WorldGenerator`.

### Bước 6 — Mob prefabs
Tạo prefab Rabbit:
- SpriteRenderer + Rigidbody2D + CircleCollider2D, Layer: Mob
- Add `RabbitAI`: maxHP=10, moveSpeed=2, drops = [{raw_meat, 1-2}]
- Set `playerMask` = Layer Player

Tạo prefab Wolf: maxHP=30, damage=8, drops = [{raw_meat, 2-3}]
Tạo prefab FoxSpirit: maxHP=20, damage=10, drops = [{spirit_herb, 1-2}, {raw_meat, 1}]

Tạo `MobSpawner` GameObject; entries:
- Rabbit: dayCap=8, nightCap=3
- Wolf: dayCap=2, nightCap=4
- FoxSpirit: dayCap=0, nightCap=3

Drag MobSpawner vào field `mobSpawner` của WorldGenerator.

### Bước 7 — UI Canvas
Tạo Canvas (Screen Space - Overlay, Scale Mode: Scale With Screen Size, ref 1080×1920 portrait hoặc 1920×1080 landscape).

Thêm:
- **Joystick:** Image background tròn + Image thumb làm child → add `VirtualJoystick`. Drag vào PlayerController.joystick.
- **Stat bars:** 5 Image (Filled, Horizontal). Add `StatBarUI`, drag references.
- **Inventory panel:** GridLayoutGroup, prefab slot (Image icon + TMP_Text count + Button). Add `InventoryUI`.
- **Crafting panel:** vertical list, prefab recipe button. Add `CraftingUI`.
- **Realm UI:** TMP_Text + Image XP bar + Button breakthrough. Add `RealmUI`.
- **Skill buttons:** 3 Buttons (Attack / Cast / Meditate), mỗi cái add `SkillButton` với action tương ứng.

### Bước 8 — Camera
Camera Main: Orthographic, size=6, follow Player (dùng simple script hoặc Cinemachine 2D).

### Bước 9 — Player actions: Interact + Sleep
Trên `Player` GameObject thêm:
- `InteractAction` — set `interactRadius` ~1.6, `interactKey` = E. Dùng `interactMask` = layer chứa `Campfire` + `WaterSpring` (vd thêm 1 layer mới `Interactable` rồi set lên prefab).
- `SleepAction` — chỉnh `hpPerSec`, `sanityPerSec`, `timeMultiplier` theo balance bạn muốn. Yêu cầu mặc định: ban đêm + đứng trong aura của 1 `Campfire` đang cháy.

### Bước 10 — Lửa trại & Suối nước
**Campfire prefab:**
- Empty `Campfire`, Layer: `Interactable` (hoặc `CraftStation`)
- Add `CraftStationMarker` (set `station` = Campfire) + `Campfire` script
- 1 SpriteRenderer cho ngọn lửa → drag vào `flameRenderer`
- 1 SpriteRenderer phụ (tròn, mềm, mờ) làm aura → drag vào `auraRenderer`
- Set `woodItem` = `Item_wood`, `warmRadius` ~3, `fuelSeconds` ~180.

Tạo `RecipeSO` "Lửa trại" với `requiredStation = None`, output là `ItemSO` placeholder hoặc dùng custom logic place prefab (xem TODO trong `RecipeSO.cs`).

**WaterSpring prefab:**
- Empty `WaterSpring`, Layer: `Interactable`
- CircleCollider2D (isTrigger = true, radius ~0.6)
- Add `WaterSpring` script. Optional: set `cleanWaterItem` = `Item_clean_water` + `dispenseBottleOnDrink = true` để mỗi lần uống cũng nhận 1 bình nước mang đi.
- Drag vào `WorldGenerator.waterSpringPrefab` để procedural rải khắp đồng cỏ (mật độ ~0.005 mặc định = vài chục suối nhỏ trên map 100×100).

### Bước 11 — UI prompt tương tác
Trong Canvas:
- 1 TMP_Text "[E] Uống" gần joystick → add `InteractPromptUI`, drag `interactAction` từ Player. Tự ẩn khi không có target.
- (Optional) 1 Button mobile cho Interact + 1 Button mobile cho Sleep, mỗi cái add `SkillButton` set `action` = `Interact` / `Sleep`.

### Bước 12 — Save/Load
Trên `_GameManager`:
- Add `SaveLoadController`, drag `playerStats`, `realm`, `inventory`, `timeManager`, `worldGenerator`, **và `itemDatabase`** (cần để restore inventory).
- Bật `autoLoadOnStart = true` nếu muốn auto-load.

---

## 🕹️ Điều khiển

### Mobile
- Joystick trái: di chuyển
- Nút Attack (J): đánh thường
- Nút Cast (K): công pháp (Kiếm Khí Trảm)
- Nút Meditate (M): bật/tắt thiền
- Nút Interact (E): tương tác — uống nước tại suối, tiếp gỗ vào lửa trại
- Nút Sleep (Z): ngủ qua đêm khi đang ở cạnh lửa trại đang cháy (hồi HP/SAN, time fast-forward)

### PC (test)
- WASD / Arrow keys: di chuyển
- J: đánh thường
- K: công pháp
- M: thiền
- E: tương tác (hover prompt sẽ hiện ra)
- Z: ngủ (qua `SleepAction`, bind vào `SkillButton.Action.Sleep`)

---

## 📦 Build APK Android

1. File → Build Settings → Add Open Scenes (`Game.unity`)
2. Player Settings → Company Name, Product Name, Bundle ID (vd `com.musa8.hoangvuc`)
3. Keystore: tạo keystore mới (Edit → Project Settings → Player → Publishing Settings → Keystore Manager)
4. Build → chọn folder, xuất `.apk`
5. `adb install game.apk` hoặc copy vào điện thoại cài tay.

---

## 🤖 GameCI (auto-build APK trên mỗi PR)

Repo có sẵn `.github/workflows/build-android.yml` (build APK ARM64 IL2CPP) và `test.yml` (Edit/PlayMode tests).
Workflow CHƯA tự chạy được cho đến khi bạn add Unity license secret. Cách add:

### Personal license (.ulf, miễn phí)
1. Sinh `.alf` request file 1 lần qua [game-ci/unity-request-activation-file](https://github.com/marketplace/actions/unity-request-activation-file) hoặc:
   ```bash
   docker run --rm -v $(pwd):/root/project unityci/editor:2022.3.20f1-android-1.1.0 \
     unity-editor -batchmode -nographics -createManualActivationFile -logFile -
   ```
   File `Unity_v*.alf` xuất ra trong project.
2. Lên https://license.unity3d.com/manual upload `.alf` → tải về `Unity_v*.ulf`.
3. GitHub repo → **Settings → Secrets and variables → Actions → New repository secret**:
   - Name: `UNITY_LICENSE`
   - Value: dán **toàn bộ nội dung file .ulf** (kể cả XML header)

### Pro/Plus license (email + serial)
Add 3 secrets thay vì `UNITY_LICENSE`:
- `UNITY_EMAIL` — email Unity ID
- `UNITY_PASSWORD` — password
- `UNITY_SERIAL` — serial Pro/Plus

Sau khi add, push 1 commit bất kỳ vào PR → workflow `Build Android (IL2CPP, ARM64)` sẽ chạy ~25 phút và upload artifact `WildernessCultivation-<sha>-apk` vào tab Actions.

## 🌲 Biome system

`BiomeSO` (asset trong `Assets/_Project/Data/SO/Biomes/`) chia map thành các vùng theo Perlin noise. 2 biome khởi đầu (theo ROADMAP):

- **Rừng Linh Mộc**: linh khí dày (`spiritEnergyMultiplier = 1.5`), nhiều cây/grass, mob FoxSpirit/Wolf — vùng dễ tu luyện.
- **Hoang Mạc Tử Khí**: linh khí loãng (0.7), `ambientNightSanDamage = 0.4` SAN/giây ban đêm (lửa trại không chống được), drop hiếm hơn — vùng trừng phạt.

Gán array `WorldGenerator.biomes` trong scene để bật. Để trống → fallback prefab cũ (giữ tương thích cho scene Game cũ).

## 🔥 Pháp bảo + Luyện đan

- `MagicTreasureSO`: ItemSO con với `kind` ∈ {HealBurst, ManaBurst, BreakthroughAid, ShieldAura}. Trang bị qua `MagicTreasureAction` (phím **B**), có cooldown + charges.
- `AlchemyFurnace`: `MonoBehaviour` + `CraftStationMarker.station = AlchemyFurnace`. Cần tiếp gỗ (`refuelPerWoodSeconds`) để cháy. Khi tắt thì recipe đan dược (RecipeSO với `requiredStation = AlchemyFurnace`) sẽ KHÔNG craft được — gate qua `IStationGate`.
- Đan dược chỉ là data: tạo ItemSO consumable + RecipeSO trong Editor.

## ❄️ Survival core (Phase Polish)

### Temperature & Weather
- **BodyTemp** trên `PlayerStats` chạy thang [0..100], 50 = thoải mái. Driff về `ComputeAmbientTemperature()`.
  Ambient = `SeasonBaselineTemperature` + biome day/night offset + tổng `LightSource.warmthBonus` đang chứa player + (-5/-10 nếu trời mưa/bão).
- **Hậu quả**: `BodyTemp ≤ 10` → mất HP + SAN; `BodyTemp ≥ 90` → Thirst tụt nhanh + SAN tụt.
- **Mùa**: TimeManager tự đổi mỗi `daysPerSeason` ngày (Xuân→Hạ→Thu→Đông), mỗi mùa baseline khác (30..70).
- **Thời tiết**: Roll mỗi sáng (Clear/Rain/Storm) — mưa refill Thirst nhẹ + decay fire 2x; bão thêm SAN penalty đêm.
- **Dark fear**: Đêm + ngoài tất cả `LightSource` đang phát → SAN tụt nhanh + MobSpawner spawn x2.

### Food spoilage & Equipment durability
- `ItemSO.isPerishable + freshSeconds` — `Inventory.Update` đếm ngược; `slot.IsSpoiled` → ăn vẫn được nhưng restore /2 + trừ SAN.
- `ItemSO.hasDurability + maxDurability + durabilityPerUse` — `Inventory.UseDurability(slot)` hao 1 lần dùng, hết = item biến mất.
  `PlayerCombat.TryMeleeAttack` tự gọi UseDurability trên `equippedWeaponSlotIndex` khi đánh trúng.
- Perishable / durable items KHÔNG stack (mỗi instance giữ tracking riêng).

### Storage chest
- `StorageChest` (require Inventory component) IInteractable mở rương — emit event `OnAnyChestOpened` để UI subscribe & hiển thị grid.

### Farming linh thảo
- `PlantNode` IInteractable: trồng 1 `seedItem` → cần `growDays` + `waterNeeded` lần tưới (consume `waterBucketItem` hoặc đứng cạnh WaterSpring) → harvest `harvestItem`. Sprite stage cho từng giai đoạn.

### Torch (đèn cầm tay)
- `TorchAction` toggle (phím **T** hoặc `SkillButton.ToggleTorch`) — tốn 1 `torchItem` → bật `LightSource` aura quanh player (radius/warmth) + tốn fuel theo thời gian.

### Save/Load mới round-trip
- `bodyTemp`, `seasonIndex`, `weatherIndex`, per-slot `freshRemaining` + `durability`.

## 🍲 Survival depth (Phase Polish 2)

### Cooking nhiều bước
- `RecipeSO.cookTimeSeconds` — recipe có timer; player bấm craft → tiêu liệu ngay, output add sau N giây (StartCoroutine trong `CraftingSystem`).
- `RecipeSO.flavorNote` — string mô tả buff cho UI tooltip.
- Station mới `CraftStation.CookingPot` + `CookingPot` MonoBehaviour: chỉ active khi đặt gần `Campfire.IsLit` ≤ `requiredHeatRadius` (default 1.5m). Recipe stew/canh đặt `requiredStation = CookingPot`.
- Quy ước recipe: Raw → Grilled (Campfire, cookTime ~5s) → Stew (CookingPot, cookTime ~15s, ăn buff lâu).

### Fishing
- `FishingSpot` (gắn vào WaterSpring hoặc prefab nước riêng) có `castRangeFromSpot` + `lootTable` weighted.
- `FishingAction` (gắn player), phím **F**: cần `rodItem` (nên `hasDurability=true`) + đứng trong `castRangeFromSpot` → cast 3–8s (random per spot) → drop random 1 item theo weight + tốn 1 durability rod.
- Nhấn F lần nữa khi đang cast = cancel (không tốn durability).

### Shelter / nhà
- `Shelter` MonoBehaviour có `radius` aura. Đặt prefab vào scene (có thể đặt thêm Campfire bên trong).
- Trong aura: block tác động Rain/Storm (`PlayerStats.UpdateWeatherEffects` skip), block thermal penalty của mưa/bão, cộng `warmthBonus` vào ambient temp.
- Sleep trong shelter: bonus hồi HP/SAN x `sleepRecoveryMultiplier` (default x2). Shelter cũng đủ điều kiện ngủ thay cho Campfire (hữu ích khi shelter có lửa bên trong nhưng khoảng aura riêng).

### Encumbrance
- `ItemSO.weight` (default 1) → `Inventory.TotalWeight`.
- `PlayerController.maxCarryWeight` (≤ → tốc độ bình thường) → `overEncumberedHardCap` (≥ → tốc độ tối thiểu `overweightSpeedMin`). Lerp tuyến tính giữa 2 ngưỡng.
- Quy ước weight: gỗ/đá ~1, thịt sống ~0.5, vũ khí ~3, đan dược ~0.1. Cây cối / tài nguyên nặng ~2.

## 👹 Boss bí cảnh

- `BossPortal` (IInteractable): tốn 1 vật phẩm key (vd Linh Thạch) để mở, spawn `BossMobAI` cách player ~3m.
- `BossMobAI`: 3 phase theo HP threshold (60% / 25%) — phase 2 summon minion, phase 3 bắn volley 8 viên + tốc độ x1.5. Drop `bonusDropItem` (vd 1 pháp bảo).
- **(mới)** Element resistance: `BossMobAI.element` + `sameElementResistance` (cùng linh tố projectile → giảm dame) + `counterElementVulnerability` (tương khắc → tăng dame). Quan hệ tương khắc: Hoả ⇄ Thuỷ, Mộc ⇄ Kim, Thổ ⇄ Mộc.
- **(mới)** Aura status & on-hit status: `auraStatusEffect` (Băng Phách Long → Freeze trong radius), `onHitStatusEffect` (Hắc Lang → Bleeding khi cắn).

### 3 boss preset (gợi ý wiring trong Editor)
| Tên | Realm tier | Linh tố | Aura/On-hit | Drop |
|---|---|---|---|---|
| Hắc Lang | Luyện Khí 9 | Kim | onHit Bleeding, summon sói nhỏ phase 2 | Sói Lang Nha |
| Hoàng Sa Yêu | Trúc Cơ Sơ | Thổ | aura slow (StatusEffect Freeze nhẹ moveSpeed=0.7), kháng Hoả 50% | Hoàng Sa Châu |
| Băng Phách Long | Trúc Cơ Hậu | Thuỷ | aura Freeze (moveSpeed=0.4 + sanity tick), kháng Thuỷ, yếu Hoả x1.5 | Băng Phách Châu (MagicTreasure ManaBurst) |

## 🌟 Linh căn ngũ hành

`SpiritRootSO` (Right-click > Create > WildernessCultivation > Spirit Root). Gắn `SpiritRoot` MonoBehaviour vào Player; điền `candidatePool` để roll random khi nhân vật mới (vd 1 Tạp + 5 Đơn linh căn + 1 Thiên).

PlayerStats / RealmSystem / PlayerCombat / PlayerController tự đọc multiplier:
- `maxHPMultiplier`, `carryWeightMultiplier` (Thổ ↑)
- `freezeThresholdDelta` / `heatThresholdDelta` / `freezeDamageMultiplier` (Hoả/Thuỷ)
- `thirstDecayMultiplier`, `hungerDecayMultiplier`, `sanityDecayMultiplier`
- `weaponDamageMultiplier`, `durabilityWearMultiplier` (Kim ↑ dame, ↓ hao)
- `xpGainMultiplier`, `breakthroughCostMultiplier`, `techniqueAffinityMultiplier`
- `sameElementDamageMultiplier` (FireBall + Hoả căn → cộng)

`TechniqueSO.element` chỉ ra linh tố của công pháp (FireBall = Hoả). `RealmSystem.AddTechniqueXp(amount, element)` áp `techniqueAffinityMultiplier` nếu cùng element. `Projectile.element` được FireBallSO inject xuống projectile để boss áp resistance.

Save/load round-trip: `SaveLoadController.spiritRootCatalog[]` resolve theo `SpiritRootSO.name`.

## 🤢 Status effects

`StatusEffectSO` (Right-click > Create > WildernessCultivation > Status Effect). `StatusEffectManager` gắn cùng GameObject với PlayerStats — tick damage + multiplier moveSpeed/incomingDamage.

Built-in hooks:
- `ItemSO.consumeStatusEffect` + `consumeStatusChance`: ăn raw meat → Sickness, uống nước bẩn → Poison.
- `ItemSO.spoiledStatusEffect`: chồng thêm khi item ĐÃ HỎNG (vd raw meat hỏng → Food Poisoning).
- `Projectile.onHitStatusEffect`: FireBall + Burn SO → áp Burn 4s lên target khi trúng.
- `BossMobAI.auraStatusEffect`: tick mỗi `auraTickInterval` lên player trong `auraRadius`.
- `BossMobAI.onHitStatusEffect`: áp khi boss đập trúng.

UI: gắn `StatusEffectBarUI` lên Canvas với `iconContainer` (HorizontalLayoutGroup) + `iconPrefab` (Image + Text con).

## 📊 HUD mới

`StatBarUI` mở rộng: thêm `bodyTempFill` (đổi màu lạnh/ấm/nóng theo BodyTemp) + `encumbranceFill` (đổi màu khi vượt cap).

`EnvironmentBadgeUI` (mới): badge góc màn hình hiển thị Ngày/Mùa/Weather/Linh căn (text + optional icon).

---

## 🗺️ Roadmap

Xem [`Documentation/ROADMAP.md`](Documentation/ROADMAP.md) cho 5 giai đoạn (Pre-production → Release) và [`Documentation/MVP_SCOPE.md`](Documentation/MVP_SCOPE.md) cho scope MVP cụ thể.

## 📖 Game Design

Xem [`Documentation/GDD.md`](Documentation/GDD.md) cho hệ thống tu tiên (linh căn, cảnh giới, công pháp, đan dược, thiên kiếp), sinh tồn, thế giới mở, art direction.

---

## 🤝 Đóng góp

Solo/hobbyist project. Branch convention: `devin/<timestamp>-<topic>`. PR vào `main`.

## 📜 License

TBD (gợi ý: MIT cho code, CC-BY cho asset).
