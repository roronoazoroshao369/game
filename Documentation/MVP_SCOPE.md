# MVP Scope — Cụ thể & Đo được

Mục tiêu MVP: **Build được 1 APK Android chạy ổn định, demo core loop trong 30 phút gameplay.**

## Feature list MVP (cắt tối đa)

### ✓ MUST-HAVE (làm chắc)
- [x] Nhân vật: di chuyển virtual joystick, tấn công, né (`DodgeAction`)
- [ ] Camera follow top-down
- [ ] 1 biome procedural (Đồng cỏ) — 100x100 tile, có cây/đá/cỏ
- [ ] Inventory 16 ô, drag & drop trên touch
- [ ] 5 tài nguyên: Gỗ, Đá, Cỏ, Thịt sống, Nước
- [ ] Crafting: Rìu gỗ, Lửa trại, Thịt nướng, Bình nước
- [ ] 5 chỉ số: HP / Đói / Khát / SAN / Linh Khí
- [ ] Chu kỳ ngày-đêm 8 phút thực
- [ ] 3 quái: Thỏ (passive), Sói (aggressive), Yêu hồ (đêm, drop linh dược)
- [ ] **Cảnh giới: Luyện Khí 1 → 9 tầng** (chỉ tăng HP/dmg/Linh Khí)
- [ ] **1 công pháp: "Tụ Linh Quyết"** — ngồi thiền hồi linh khí
- [ ] **1 chiêu thức: "Kiếm Khí Trảm"** — tốn 20 linh khí, dmg xa
- [ ] Save/Load (1 slot, autosave mỗi 2 phút)
- [ ] UI: thanh trạng thái, inventory, crafting menu, cảnh giới indicator
- [ ] Build APK ARM64, target Android 8.0+

### ✗ KHÔNG LÀM trong MVP (cắt thẳng tay)
- Multiplayer / online
- Cảnh giới trên Luyện Khí
- Luyện đan, pháp bảo
- Thiên kiếp
- Base building phức tạp / Động phủ
- Nhiều biome (chỉ 1)
- Mùa
- Quest / story
- Cutscene
- Voice acting
- Cosmetic / shop
- Tutorial dài (chỉ tooltip ngắn)

## Tiêu chí "Done" của MVP

1. APK < 150MB
2. Chạy ổn định 30+ FPS trên Android tầm trung (Snapdragon 6xx, 4GB RAM)
3. Không crash trong 30 phút playtest
4. Có thể: nhặt gỗ → chế lửa → giết thỏ → nướng thịt → ăn → ngủ qua đêm → ngồi thiền → đột phá Luyện Khí 2
5. Save/load không mất dữ liệu

## Estimate thời gian (1 dev, full-time)

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
