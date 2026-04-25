# Roadmap & Tech Stack

## 1. Đề xuất Tech Stack (cho Android)

### Lựa chọn #1 — **Unity 2022 LTS + C#** ⭐ KHUYÊN DÙNG
**Ưu:**
- Mạnh nhất cho mobile, build APK/AAB Android dễ
- Asset Store có sẵn nhiều cho 2D top-down survival
- ECS / DOTS giúp xử lý nhiều entity (yêu thú, vật phẩm)
- Cộng đồng tiếng Việt lớn
- Hỗ trợ co-op multiplayer (Mirror, Netcode for GameObjects)

**Nhược:** License (free dưới $200k revenue/năm — đủ cho indie)

**Plugin chính cần dùng:**
- `2D Tilemap Extras` — thế giới ô vuông
- `Cinemachine` — camera
- `Addressables` — load chunk
- `DOTween` — animation
- `Odin Inspector` (paid) — chỉnh data dễ
- `Mirror` / `Fish-Net` — multiplayer (giai đoạn sau)

### Lựa chọn #2 — Godot 4 + GDScript
- Free, open-source, nhẹ
- Tốt cho 2D nhưng mobile build chưa mượt bằng Unity
- Cộng đồng nhỏ hơn

### Lựa chọn #3 — Unreal Engine 5
- Đẹp 3D, nhưng nặng cho mobile, học khó
- **Không khuyến nghị** cho game 2D survival mobile

---

## 2. Roadmap 5 giai đoạn

### **GĐ 0 — Pre-production (2–4 tuần)**
- [ ] Hoàn thiện GDD chi tiết (cấp độ chỉ số, công thức damage, danh sách vật phẩm)
- [ ] Concept art: 1 nhân vật, 5 quái, 3 biome, UI mockup
- [ ] Prototype paper (cân bằng kinh tế trên giấy/Excel)
- [ ] Setup Unity project, Git repo, CI build APK

### **GĐ 1 — Vertical Slice / MVP (2–3 tháng)**
**Scope MVP rất hẹp — chỉ chứng minh core loop chạy được:**
- [ ] 1 nhân vật, di chuyển + đánh thường + né
- [ ] 1 biome (Đồng cỏ) procedural 1 chunk
- [ ] 5 tài nguyên (gỗ, đá, thảo dược, nước, thịt)
- [ ] 5 quái cơ bản (thỏ, sói, yêu thú lv1)
- [ ] Hệ thống đói/khát/HP/SAN/Linh Khí
- [ ] Crafting cơ bản: lửa trại, vũ khí gỗ, đồ ăn nấu
- [ ] Chu kỳ ngày/đêm
- [ ] **Tu tiên cảnh giới Luyện Khí 1–3 tầng** (chỉ buff stats, chưa có công pháp phức tạp)
- [ ] 1 công pháp đơn giản (vd: "Kiếm khí trảm" — tốn linh khí, gây dmg xa)
- [ ] Save/Load
- [ ] Build APK chạy được trên điện thoại Android

**Tiêu chí done:** Người chơi có thể chơi 30 phút và thấy được "đây là survival + tu tiên".

### **GĐ 2 — Alpha (3–4 tháng)**
- Thêm 2 biome (Rừng linh mộc, Hoang mạc tử khí)
- Thêm Trúc Cơ + Kim Đan
- Hệ thống luyện đan, pháp bảo
- 20+ quái, 3 boss bí cảnh
- Base building + Động phủ
- Thiên kiếp (lôi kiếp đơn giản)
- Audio: nhạc nền, SFX
- Closed alpha test (10–30 người)

### **GĐ 3 — Beta (3 tháng)**
- Cân bằng, sửa bug từ alpha
- Thêm 2 biome còn lại
- Mở rộng cảnh giới đến Nguyên Anh
- Localization (Việt / Anh / Trung)
- Public beta trên Google Play (open testing)

### **GĐ 4 — Release & Post-launch (liên tục)**
- 1.0 trên Google Play
- DLC / update content (cảnh giới mới, biome mới)
- **Co-op multiplayer 2–4 người** (sau release)
- iOS / Steam port

---

## 3. Ước lượng nhân lực (cho mỗi giai đoạn)

| Vai trò | MVP | Alpha | Beta+ |
|---|---|---|---|
| Programmer (gameplay) | 1 | 1–2 | 2 |
| Artist (2D) | 0.5 (part) | 1 | 1–2 |
| Game designer | 0.5 | 1 | 1 |
| Sound | 0 | 0.3 (freelance) | 0.5 |
| QA | 0 | 0.5 | 1 |

**Solo dev khả thi đến MVP nếu chăm chỉ + dùng nhiều asset có sẵn.**

---

## 4. Cấu trúc thư mục Unity đề xuất

```
Assets/
├── _Project/
│   ├── Art/         (sprites, animations, UI)
│   ├── Audio/
│   ├── Data/        (ScriptableObjects: Item, Recipe, Mob, Skill)
│   ├── Prefabs/
│   ├── Scenes/
│   ├── Scripts/
│   │   ├── Core/        (GameManager, SaveSystem)
│   │   ├── World/       (ChunkGen, Biome, DayNight)
│   │   ├── Entity/      (Player, Mob, ECS components)
│   │   ├── Survival/    (Hunger, Thirst, San, Temp)
│   │   ├── Cultivation/ (Realm, Technique, Tribulation)
│   │   ├── Combat/
│   │   ├── Crafting/
│   │   ├── UI/
│   │   └── Net/         (multiplayer — sau)
│   └── ThirdParty/
└── Plugins/
```

---

## 5. Bước tiếp theo (sau khi bạn confirm)

1. Bạn chọn tech stack (Unity / Godot / khác)
2. Mình sẽ:
   - Setup repo Git có cấu trúc Unity sẵn
   - Viết các script khung: `GameManager`, `PlayerController`, `Inventory`, `HungerSystem`, `RealmSystem`
   - Tạo 1 scene demo: nhân vật chạy quanh đồng cỏ, ăn thức ăn, đánh quái, tu luyện tăng cảnh giới
   - Build thử APK Android
3. Bạn cài APK lên máy → playtest → feedback → lặp.
