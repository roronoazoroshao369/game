# 🏔️ Wilderness Cultivation Chronicle (*Hoang Vực Tu Tiên Ký*)

> Game **mobile open-world survival + tu tiên** lấy cảm hứng từ *Don't Starve Together* và *Quỷ Cốc Bát Hoang*.
> Engine: **Unity 2022 LTS** · Nền tảng: **Android** (mở rộng iOS/PC sau).

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

## 🚀 Hướng dẫn setup lần đầu

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

## 👹 Boss bí cảnh

- `BossPortal` (IInteractable): tốn 1 vật phẩm key (vd Linh Thạch) để mở, spawn `BossMobAI` cách player ~3m.
- `BossMobAI`: 3 phase theo HP threshold (60% / 25%) — phase 2 summon minion, phase 3 bắn volley 8 viên + tốc độ x1.5. Drop `bonusDropItem` (vd 1 pháp bảo).

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
