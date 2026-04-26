# MVP Scope — Cụ thể & Đo được

Mục tiêu MVP: **Build được 1 APK Android chạy ổn định, demo core loop trong 30 phút gameplay.**

> **Status (auto-updated khi PR merge):** Mã cho phần lớn hệ thống đã có — xem checklist bên dưới.
> Việc còn lại tập trung ở: Unity license CI + build APK thực tế + polish UX.
> Chạy thử demo 60 giây: xem section "Quickstart" trong [README.md](../README.md#-quickstart-bootstrap-default-scene-1-click).

## Feature list MVP (cắt tối đa)

### Trạng thái code hiện tại

- [x] Nhân vật: di chuyển joystick + WASD, tấn công (`PlayerCombat`), né (`DodgeAction`, i-frames + cooldown)
- [x] Camera follow top-down (parent camera vào Player trong `BootstrapWizard`)
- [x] 1 biome procedural (`WorldGenerator` + `BiomeSO`; default 40×40, mở rộng được qua `size`)
- [x] Inventory 16 ô, **drag & drop trên touch + mouse** (`InventorySlotUI` + `Inventory.SwapSlots`)
- [x] 5 tài nguyên: Gỗ, Đá, Cỏ, Thịt sống, Nước (+ Linh Thảo)
- [x] Crafting: Rìu gỗ, Lửa trại, Thịt nướng, Bình nước (`RecipeSO` + `CraftingSystem` + station-gated recipes)
- [x] 5 chỉ số: HP / Đói / Khát / SAN / Linh Khí (`PlayerStats`, có cả thân nhiệt bonus)
- [x] Chu kỳ ngày-đêm 8 phút (`TimeManager`, tunable `dayLengthSeconds`)
- [x] 3 quái: Thỏ (passive) / Sói (aggressive) / Yêu Hồ (đêm-only, drop linh dược)
- [x] **Cảnh giới: Luyện Khí 1 → 9 tầng** (12 tier total, buff HP/dmg/Linh Khí qua `RealmSystem`)
- [x] **1 công pháp: "Tụ Linh Quyết"** — ngồi thiền hồi linh khí + XP (`MeditationAction`, phím M)
- [x] **1 chiêu thức: "Kiếm Khí Trảm"** — 20 linh khí, dmg xa (`SwordQiSlashSO`)
- [x] Save/Load (`SaveSystem` + `SaveLoadController`, 1 slot JSON, round-trip test EditMode)
- [x] UI: thanh trạng thái, inventory, crafting menu, cảnh giới indicator, joystick, skill buttons
- [x] **Tutorial HUD + checklist mục tiêu** (`TutorialHUD` + `DemoObjectivesTracker`) — demo-facing onboarding
- [ ] Build APK ARM64, target Android 8.0+ — infra sẵn trong `.github/workflows/build-android.yml`, skip đến khi repo có `UNITY_LICENSE` secret
- [ ] Autosave mỗi 2 phút — `SaveLoadController` có trigger manual, chưa có timer tự động
- [ ] Art thật thay cho placeholder solid-color sprites

### ✗ KHÔNG LÀM trong MVP (cắt thẳng tay)

- Multiplayer / online
- Cảnh giới trên Luyện Khí (Trúc Cơ đã có code khung nhưng không balance cho MVP)
- Luyện đan phức tạp, pháp bảo
- Thiên kiếp
- Base building phức tạp / Động phủ
- Nhiều biome (đã có `BiomeSO`, MVP chỉ bật 1)
- Mùa
- Quest / story
- Cutscene
- Voice acting
- Cosmetic / shop
- Tutorial dài (dùng `TutorialHUD` overlay ngắn + checklist là đủ)

## Tiêu chí "Done" của MVP

1. APK < 150MB
2. Chạy ổn định 30+ FPS trên Android tầm trung (Snapdragon 6xx, 4GB RAM)
3. Không crash trong 30 phút playtest
4. Có thể: nhặt gỗ → chế lửa → giết thỏ → nướng thịt → ăn → ngủ qua đêm → ngồi thiền → đột phá Luyện Khí 2
   → `DemoObjectivesTracker` theo dõi đúng 5 checkpoint này và hiện banner "MVP Demo hoàn thành".
5. Save/load không mất dữ liệu (EditMode round-trip + fuzz test đã chạy).

## Bước còn lại trước khi coi là "demo-ready"

1. **User action:** Add `UNITY_LICENSE` (hoặc `UNITY_EMAIL`/`PASSWORD`/`SERIAL`) vào GitHub Secrets
   để CI `build-android.yml` + `test.yml` thực thi thay vì skip (xem README § GameCI).
2. **Build APK thủ công 1 lần** qua Unity Editor → verify `adb install` chạy trên máy thật.
3. **Autosave timer** trong `SaveLoadController` (2 phút/lần).
4. **Replace placeholder sprites** bằng art 2D thật (Bootstrap Wizard luôn idempotent — rerun sẽ không phá art đã thay).

## Estimate thời gian (1 dev, full-time) — để tham khảo ban đầu

| Module | Ngày |
|---|---|
| Setup project, input, camera | 3 |
| Player controller + animation | 4 |
| Inventory + crafting | 6 |
| World gen procedural 1 biome | 7 |
| Survival stats system | 3 |
| Day/night | 2 |
| Mob AI (3 loại) | 6 |
| Combat (đánh thường + skill) | 5 |
| Cultivation system (cảnh giới + công pháp) | 5 |
| UI / UX | 6 |
| Save/load | 3 |
| Audio cơ bản | 2 |
| Polish + bugfix | 8 |
| Build & test Android | 3 |
| **Tổng** | **~63 ngày (~3 tháng full-time)** |

Solo part-time (15h/tuần): **6–8 tháng.**
