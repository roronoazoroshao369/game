---
name: world-map-design
audience: both
status: active — post-MVP target
scope: 3 biome chi tiết + mob spawn table + asset checklist. MVP chỉ dùng 1 biome.
depends-on:
  - GDD.md
  - ../art/ART_STYLE.md
---
# World Map Design

> Tài liệu thiết kế thế giới cho Wilderness Cultivation Chronicle.
> Định nghĩa 3 biome, mob spawn table, resource distribution, asset checklist.
>
> **Quan hệ với code:**
> - `BiomeSO` (Scripts/World) — schema biome (PR #72: thêm `groundTileVariants` + `decorations`)
> - `WorldGenerator` (Scripts/World) — Perlin biome selection + per-tile spawn pass
> - `BootstrapWizard.CreateBiomes()` (Editor/) — instantiate 3 biome SO mặc định
> - `MobSpawner` (Scripts/Mobs) — global cap day/night (chưa biome-aware, defer)
>
> **Cập nhật khi:**
> - Thêm biome mới → update bảng macro + Perlin range
> - Thêm mob/plant/decoration → update biome's resource & spawn table
> - Đổi balance (density, ambient damage, temperature) → sync với BiomeSO asset

---

## 1. Macro layout

Map procedural 100×100 tiles (`WorldGenerator.size`), Perlin "biome map" chia 3 biome theo
`selectionRange` không overlap, không gap. Player spawn giữa map (50, 50). Mob spawn theo cap
global của `MobSpawner` (chưa filter theo biome — xem Future work).

```
Perlin value (biomeNoiseScale = 0.025):
0.00 ─────────────── 0.40 ───────── 0.65 ────────────── 1.00
│                       │                │                  │
│   forest              │ stone_highlands│   desert         │
│   (Rừng Linh Mộc)     │ (Đá Sơn        │   (Hoang Mạc     │
│                       │  Cao Nguyên)   │   Tử Khí)        │
└───────────────────────┴────────────────┴──────────────────┘
        ~40% area              ~25%             ~35%
```

### Biome distribution principles

- **Forest = start zone** (40% area, friendly mobs, abundant grass/wood, +20% spirit khi thiền).
- **Stone highlands = mid zone** (25%, ore + mineral cao, bắt đầu xuất hiện đêm SAN dmg nhẹ).
- **Desert = end zone** (35%, mob nguy hiểm + linh quái, thân nhiệt khắc nghiệt, đêm SAN dmg cao).

Player nên di chuyển từ forest (sống sót) → highlands (luyện) → desert (đột phá / boss). World gen
KHÔNG enforce vật lý zone wall — Perlin map tự nhiên rải biome theo blob, có thể có pocket biome
nhỏ trong vùng khác (vd 1 mảnh desert nhỏ giữa forest).

### Spawn safety (rule code)

- `WorldGenerator.Spawn()` không spawn resource trong bán kính 4 ô quanh tâm map (player spawn point).
- `DeterministicPositionFor()` cho tombstone / spirit spring push ra >= 6 ô khỏi tâm.

---

## 2. Biome chi tiết

### 2.1 Forest — "Rừng Linh Mộc"

| Field (BiomeSO) | Value | Ghi chú |
|---|---|---|
| `biomeId` | `forest` | |
| `displayName` | Rừng Linh Mộc | |
| `selectionRange` | (0.00, 0.40) | ~40% area |
| `treeDensity` | 0.18 | dày — đây là rừng |
| `rockDensity` | 0.05 | thưa |
| `grassDensity` | 0.20 (default) | bụi cỏ phổ biến |
| `waterDensity` | 0.008 | suối nhỏ |
| `temperatureDayOffset` | 0 | dễ chịu |
| `temperatureNightOffset` | 0 | mát mẻ |
| `spiritEnergyMultiplier` | **1.2** | thiền nhanh hơn 20% |
| `ambientNightSanDamage` | 0 | an toàn ban đêm |

#### Mood + palette (Asset Bible)

> **Mood**: linh khí dồi dào, sương sớm, ánh nắng xuyên tán lá. Cảm giác "nhà".
> **Palette**: `#4a6741` deep moss · `#6b8e62` sage · `#a8c69b` mint highlight · `#b89968` bark · `#e8d5a6` light tree leaf glow.
> **Lighting**: ambient ấm, golden hour bias.

#### Tile variants (mục tiêu sau asset)

- `groundTile`: cỏ xanh đậm cơ bản (pixel 1 PPU = 64)
- `groundTileVariants[]` (3-4 variants để phá pattern):
  1. cỏ xanh thường
  2. cỏ điểm hoa nhỏ trắng/vàng
  3. cỏ pha lá rụng nâu
  4. cỏ ẩm rêu xanh sáng

#### Resource nodes (extraNodes)

| Prefab | Density | Drop | Notes |
|---|---|---|---|
| `LinhMushroom` (Linh Nấm) | 0.020 | `linh_mushroom` × 1-2 | hồi 5 hunger + 5 mana |
| `BerryBush` (Linh Quả Mọng) | 0.025 | `berry` × 1-3 | hồi 3 hunger + 2 thirst |
| `LinhBamboo` (Linh Trúc) | 0.015 | `bamboo` × 1, `stick` × 1-2 | crafting material |
| `Tree` (legacy resource) | 0.18 | `stick` × 2-4 | universal |

#### Decorations gợi ý (chưa có trong code, chuẩn bị slot)

- Hoa cỏ dại trắng/vàng nhỏ (density 0.05)
- Khúc gỗ mục nâu (density 0.02)
- Tổ chim trên đất (density 0.01)
- Bụi cây thấp xanh đậm (density 0.04)
- Đèn lồng giấy đỏ rách (density 0.005, lore: tu sĩ trước đây cắm trại)

#### Mob spawn (theo `MobSpawner` global; chưa biome-aware)

| Mob | Day cap | Night cap | Loot | Behavior |
|---|---|---|---|---|
| Rabbit (Thỏ) | 5 | 0 | `raw_meat` | Passive, FSM Wander/Flee (R7 exemplar) |
| Wolf (Sói) | 2 | 3 | `raw_meat` | Aggressive, FSM Patrol/Chase/Attack (R7 exemplar) |
| FoxSpirit (Yêu Hồ) | 0 | 1 | `raw_meat` (drop hiếm linh dược) | Night-only, ranged |
| Crow (Quạ) | 1 | 1 | `feather` | Flying, swoop attack |

#### Lore hook

Đây là cố hương bị bỏ hoang. Nhiều shrine cũ + đèn lồng = di tích sect tu sĩ trước đây ẩn cư.
Linh khí cao nhất biome này (1.2× multiplier) — phù hợp để Luyện Khí 1-3 tầng đầu.

---

### 2.2 Stone Highlands — "Đá Sơn Cao Nguyên"

| Field (BiomeSO) | Value | Ghi chú |
|---|---|---|
| `biomeId` | `stone_highlands` | |
| `displayName` | Đá Sơn Cao Nguyên | |
| `selectionRange` | (0.40, 0.65) | ~25% area |
| `treeDensity` | 0.04 | thưa thớt — chỉ thông gầy |
| `rockDensity` | **0.20** | dày — biome chủ đạo |
| `grassDensity` | 0.20 | cỏ ngắn cứng |
| `waterDensity` | 0.003 | hiếm |
| `temperatureDayOffset` | -3 | mát |
| `temperatureNightOffset` | **-8** | lạnh — cần lửa |
| `spiritEnergyMultiplier` | 1.0 | trung bình |
| `ambientNightSanDamage` | **0.3 / s** | bắt đầu ám ảnh |

#### Mood + palette

> **Mood**: cao nguyên đá xám, cây thông gầy, sương lạnh đêm. Cảm giác "transition zone".
> **Palette**: `#7a7c80` slate gray · `#a3a5a8` highlight · `#5a5d63` shadow · `#8a9b8c` dry moss · `#c2c4ba` bone white.
> **Lighting**: ambient nguội, contrast cao, đêm xanh tím.

#### Tile variants

- `groundTile`: đá xám cracked
- `groundTileVariants[]` (3 variants):
  1. đá xám sạch
  2. đá xám có vết nứt
  3. đá xám phủ rêu nhẹ

#### Resource nodes

| Prefab | Density | Drop | Notes |
|---|---|---|---|
| `MineralRock` | 0.030 | `mineral_ore` × 1-2 | crafting cao cấp |
| `LinhBamboo` | 0.010 | `bamboo` × 1, `stick` × 1-2 | hiếm hơn forest |
| `Rock` (legacy) | 0.20 | `stone` × 1-3 | universal |

**Optional Perlin band** (sau khi PR #72 merged):
- `MineralRock` set `perlinMin=0.55, perlinMax=0.65` — chỉ ở phần cao của highlands (gần ranh giới desert) → tạo cảm giác "ore vein đỉnh núi".

#### Decorations gợi ý

- Xương khô trắng (density 0.01) — lore: tu sĩ trước thất bại
- Tháp đổ đá xám (density 0.005)
- Hoa cỏ dại tím nhỏ (density 0.03)
- Cột đá khắc cổ ngữ (density 0.003) — lore hook
- Đèn lồng giấy đỏ tắt (density 0.005)

#### Mob spawn

| Mob | Day cap | Night cap | Loot | Behavior |
|---|---|---|---|---|
| Boar (Heo Rừng) | 1 | 1 | `raw_meat`, `tough_hide`, `tusk` | Charge attack, R7-pending |
| DeerSpirit (Linh Lộc) | 1 | 0 | `spirit_meat`, `spirit_antler` | Flee + thỉnh thoảng counter |
| Snake (Rắn) | 1 | 2 | `snake_skin`, `venom_gland` | Bite + Poison status, ambush |
| Bat (Bức) | 0 | 2 | `bat_wing` | Night flying, Bleed status |

#### Lore hook

Đỉnh cao nguyên có nhiều cột đá khắc — đây là di tích thiên đàn sect cổ. Mob đa dạng, drop cao.
Phù hợp Luyện Khí 4-6.

---

### 2.3 Desert — "Hoang Mạc Tử Khí"

| Field (BiomeSO) | Value | Ghi chú |
|---|---|---|
| `biomeId` | `desert` | |
| `displayName` | Hoang Mạc Tử Khí | |
| `selectionRange` | (0.65, 1.00) | ~35% area |
| `treeDensity` | 0.02 | rất thưa |
| `rockDensity` | 0.10 | trung bình |
| `grassDensity` | 0.20 | cỏ khô |
| `waterDensity` | 0.001 | cực hiếm — survival hard |
| `temperatureDayOffset` | **+25** | giữa trưa cực nóng |
| `temperatureNightOffset` | **-15** | đêm cực lạnh |
| `spiritEnergyMultiplier` | **0.8** | thiền chậm 20% |
| `ambientNightSanDamage` | **1.0 / s** | tử khí dày — đêm nguy hiểm |

#### Mood + palette

> **Mood**: hoang vu, sấm tử khí tím xa xa, tử lily hé nở dưới trăng. Cảm giác "endgame".
> **Palette**: `#c4a574` sand · `#8b7355` dirt · `#6b5d40` shadow · `#9b6b8b` purple death qi · `#d4d4d4` bone bleached.
> **Lighting**: ambient gắt ban ngày (nắng cháy), ban đêm tím tối.

#### Tile variants

- `groundTile`: cát vàng sạch
- `groundTileVariants[]` (4 variants):
  1. cát vàng sạch
  2. cát có gợn sóng
  3. cát có vết chân thú
  4. cát rải xương vụn nhỏ

#### Resource nodes

| Prefab | Density | Drop | Notes |
|---|---|---|---|
| `Cactus` (Tiên Nhân Chưởng) | 0.020 | `cactus_water` × 1-2 | hồi 8 thirst — quan trọng cho survival |
| `DeathLily` (Tử Lily) | 0.005 | `death_pollen` × 1 | crafting độc dược / antidote |
| `Rock` (legacy) | 0.10 | `stone` × 1-3 | |

**Optional Perlin band**:
- `DeathLily` set `perlinMin=0.85, perlinMax=1.0` — chỉ ở phần "deep desert" → endgame hook.

#### Decorations gợi ý

- Xương sườn lớn (density 0.01)
- Cát hiệu xoáy (density 0.03)
- Đầu lâu nhỏ trên cát (density 0.005)
- Tử khí mist halo tím (particle, density 0.02)
- Cột đá khắc bị cát phủ một nửa (density 0.003)

#### Mob spawn

| Mob | Day cap | Night cap | Loot | Behavior |
|---|---|---|---|---|
| Snake (Rắn Cát) | 2 | 2 | `snake_skin`, `venom_gland` | Ambush + Poison |
| Crow (Quạ Tử) | 1 | 2 | `feather` | Flying scavenger |
| FoxSpirit (Yêu Hồ Hắc) | 0 | 2 | `raw_meat` | Night-only, ranged, drop hiếm linh dược |
| **BossMob** | 0 | 0 (spawn manual / quest) | (drop unique) | Phase1/Phase2 patterns, R7 follow-up |

#### Lore hook

Đây là vùng linh khí bị nhiễm tử khí từ trận chiến cổ xưa giữa các đại sect. Boss ở trung tâm
desert là tâm điểm trận chiến. Phù hợp Luyện Khí 7-9 + boss kill = đột phá Trúc Cơ.

---

## 3. Resource economy

### 3.1 Universal resources (mọi biome)

| Item | Source | Use |
|---|---|---|
| `stick` | Tree, LinhBamboo | Torch, Fishing Rod, Campfire |
| `stone` | Rock, MineralRock | Workbench, weapons |
| `water` | Water spring, Cactus (cactus_water) | Drink, recipes |
| `raw_meat` | Rabbit/Wolf/FoxSpirit/Boar/etc. | Food (sống/chín) |

### 3.2 Biome-locked resources

| Item | Biome | Source | Notes |
|---|---|---|---|
| `linh_mushroom` | forest | LinhMushroom | hunger + mana |
| `berry` | forest | BerryBush | hunger + thirst |
| `bamboo` | forest, stone_highlands | LinhBamboo | crafting |
| `mineral_ore` | stone_highlands | MineralRock | weapon upgrade |
| `tough_hide` | stone_highlands | Boar | armor crafting |
| `tusk` | stone_highlands | Boar | weapon ingredient |
| `spirit_antler` | stone_highlands | DeerSpirit | cao cấp crafting |
| `spirit_meat` | stone_highlands | DeerSpirit | premium food |
| `snake_skin` | stone_highlands, desert | Snake | armor |
| `venom_gland` | stone_highlands, desert | Snake | poison weapon |
| `bat_wing` | stone_highlands (night), desert (night) | Bat | crafting |
| `feather` | mọi biome (Crow) | Crow | arrow/decoration |
| `cactus_water` | desert | Cactus | thirst critical |
| `death_pollen` | desert | DeathLily | antidote / advanced potion |

### 3.3 Progression curve

Player path (gợi ý):
1. **Forest** (HP 100, no gear) → harvest stick + berry + mushroom + rabbit → craft Torch + Fishing Rod + Grilled Meat
2. **Stone Highlands** (Luyện Khí 3+) → harvest mineral_ore + tough_hide → craft armor + iron weapon
3. **Desert** (Luyện Khí 6+) → harvest cactus_water (survival) + death_pollen → craft poison resist + antidote → fight Boss

---

## 4. Asset checklist (sẵn sàng cho Leonardo prompt)

Mỗi cell dưới = 1 asset cần sinh. Tổng ~50-70 unique sprite cho 3 biome.

### Forest

| Loại | Count | Spec |
|---|---|---|
| Ground tile variants | 4 | 64×64, seamless, palette forest |
| Tree prefab sprite | 2-3 (variation lớn nhỏ) | 96×96 |
| Rock prefab sprite | 1-2 | 64×64 |
| Grass bush sprite | 1 | 48×48 |
| Water spring sprite | 1 | 64×64, animated 2-frame ripple |
| LinhMushroom (đỏ glowing) | 1 | 48×48 |
| BerryBush | 1 | 64×64 |
| LinhBamboo | 1 | 96×96 (cao) |
| Decoration: hoa, log, tổ chim, lantern | 5 | 32-48×32-48 |

### Stone Highlands

| Loại | Count | Spec |
|---|---|---|
| Ground tile variants | 3 | 64×64, đá xám |
| Tree (thông gầy) | 1 | 96×96 |
| Rock (đá xám lớn) | 2 | 64×64 |
| MineralRock (đá có vein khoáng) | 1 | 64×64 |
| LinhBamboo (variation) | 1 | reuse forest |
| Decoration: bone, tower ruin, flower, stone column, lantern | 5 | 32-64 |

### Desert

| Loại | Count | Spec |
|---|---|---|
| Ground tile variants | 4 | 64×64, cát vàng |
| Cactus (3 loại: nhỏ/trung/lớn) | 3 | 48-96 |
| Rock (đá sa thạch) | 1 | 64×64 |
| DeathLily (hoa tím tử khí) | 1 | 48×48, glow |
| Decoration: bone, skull, sand swirl, mist particle, ruin | 5 | 32-64 |

### Mob sprites (universal — không tied biome)

| Mob | Frame count | Total sprite |
|---|---|---|
| Rabbit | 4-frame walk + 4-frame flee + death | 9 |
| Wolf | 4 walk + 4 chase + 4 attack + death | 13 |
| FoxSpirit | 4 walk + 4 attack (range) + death | 9 |
| Boar | 4 walk + 4 charge + death | 9 |
| DeerSpirit | 4 walk + 4 flee + death | 9 |
| Crow | 4 fly + 4 swoop + death | 9 |
| Snake | 4 slither + 4 strike + death | 9 |
| Bat | 4 fly + 4 dive + death | 9 |
| BossMob | 4 walk + 4 phase1 attack + 4 phase2 attack + death | 13 |

**Total mob sprites**: ~89 frames (có thể giảm bằng cách reuse pose).

### NPC sprites

| NPC | Frame count |
|---|---|
| VendorNPC (stationary) | idle 2-frame + interact 2-frame = 4 |
| CompanionNPC | 4 walk × 4 direction + idle 2 + dead 1 = 19 |

### UI / Icon

- Item icons: 22 × 64×64 = 22 sprite
- Skill icons: 4 × 64×64 = 4 sprite
- HUD elements (status bar, joystick, button): ~10 sprite

### Total estimate

- Tile + decoration: ~30 sprite
- Resource node: ~15 sprite
- Mob: ~89 frame (= ~13 unique entity)
- NPC: ~23 frame (= 2 entity)
- Item icon: 22 sprite
- UI: ~14 sprite
- **Grand total: ~150-170 sprite/frame**

Với Leonardo Apprentice plan ($10/tháng = 8500 token = ~500 generation, mỗi generation ra 4 variant), 1 batch ngày làm được ~30 asset → **5-7 ngày làm xong full pack**.

---

## 5. Future biome ideas (defer roadmap)

Khi expand thế giới (post-MVP), gợi ý thêm 3 biome:

| Biome | Theme | Selection range (cần re-balance) | Nét đặc trưng |
|---|---|---|---|
| **Đáy Hồ Linh Tuyền** | water + spirit fountain | sub-area trong forest | Underwater puzzle, linh thạch tinh khiết |
| **Tuyết Sơn Cấm Địa** | snow + ice + cold extreme | mới biome (split desert?) | Frostbite mechanic, ice golem, băng tinh thạch |
| **Hỏa Diệm Sơn** | volcano + lava + fire qi | mới biome | Heat dmg, fire imp, lava ore, đan dược nhiệt |
| **Cõi Mộng Cảnh** | fog + ethereal + sanity | sub-realm (portal) | Reality distortion, illusion mob, dream essence |

Những biome này đòi hỏi thêm system mới (underwater, fire dmg type, illusion mechanic) — không trong scope hiện tại, ghi để remember.

---

## 6. Cross-references

- **Code**: `Scripts/World/BiomeSO.cs`, `Scripts/World/WorldGenerator.cs`, `Scripts/Mobs/MobSpawner.cs`
- **Editor**: `Editor/BootstrapWizard.cs` (CreateBiomes + BuildExtraNodesFor)
- **Architecture**: [`ARCHITECTURE.md`](../../ARCHITECTURE.md) §1 (folder layout) + §2 (pattern)
- **Refactor**: [`REFACTOR_HISTORY.md`](../../REFACTOR_HISTORY.md) — R5 + PR #72 biome upgrade
- **MVP**: [`MVP_SCOPE.md`](MVP_SCOPE.md) — MVP chỉ dùng 1 biome, design này là post-MVP target
- **Roadmap**: [`ROADMAP.md`](ROADMAP.md) — long-term biome expansion

---

## 7. Open questions / TODO

- [ ] **Mob biome filter** — `MobSpawner` hiện global; cần extension để query `WorldGenerator.BiomeAt(spawnPos)` và chỉ spawn nếu mob có trong `biome.mobEntries`. Defer từ PR #72.
- [ ] **Edge transition tile** — biome A → B vẫn có boundary cứng (cát giáp cỏ thẳng). Cần "edge tile" sprite (vd cát-pha-cỏ) để smooth transition. Defer từ PR #72.
- [ ] **Biome-aware ambient SFX** — `AudioManager` chưa play sound theo biome (gió rừng / gió cát / gió núi). Add khi audio asset thật vào.
- [ ] **Weather x biome** — weather hệ thống có nhưng chưa biome-aware (vd snow chỉ ở highlands, sandstorm chỉ ở desert).
- [ ] **Resource respawn timer** — hiện resource node spawn 1 lần ở Generate; chưa respawn theo time. Cần thêm `ResourceRespawner` component sau.
