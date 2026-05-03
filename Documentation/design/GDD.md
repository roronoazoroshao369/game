---
name: gdd
audience: both
status: active
scope: Game Design Document — vision, core loop, hệ thống tu tiên, sinh tồn, art direction.
depends-on:
  - MVP_SCOPE.md
  - ROADMAP.md
---
# Game Design Document (GDD) — Sơ bộ

> **Tên tạm:** *Hoang Vực Tu Tiên Ký* (Wilderness Cultivation Chronicle)
> **Nền tảng chính:** Android (mở rộng iOS/PC sau)
> **Thể loại:** Open-world Survival + Cultivation (Tu Tiên) RPG
> **Cảm hứng:** Don't Starve Together (DST) × Quỷ Cốc Bát Hoang (QCBH)
> **Mục tiêu cảm xúc:** Khắc nghiệt — Tò mò khám phá — Tiến bộ chậm nhưng sâu

---

## 1. Tầm nhìn (Vision Statement)

> *"Một thế giới hoang vu khắc nghiệt nơi người chơi vừa phải sinh tồn qua đói khát, khí hậu, yêu thú — vừa từng bước tu luyện linh khí, đột phá cảnh giới, đối đầu thiên kiếp để vươn lên thành tu sĩ mạnh nhất hoang vực."*

Người chơi không chỉ "sống sót qua đêm" mà còn phải **hiểu thiên địa, luyện đan, lập động phủ, thu thập linh dược, đột phá cảnh giới**. Mỗi lần chết là một lần học — nhưng không quá ác (có cơ chế kế thừa).

---

## 2. Core Gameplay Loop

```
┌─────────────────────────────────────────────────────────────┐
│  KHÁM PHÁ ──> THU THẬP ──> CHẾ TẠO ──> SINH TỒN ──> TU LUYỆN │
│      ▲                                                  │     │
│      └──── ĐỘT PHÁ CẢNH GIỚI <── THIÊN KIẾP <───────────┘     │
└─────────────────────────────────────────────────────────────┘
```

**Vòng ngắn (5–10 phút):** Đi săn / hái lượm → nấu ăn → ăn uống → ngủ
**Vòng vừa (1–2 giờ):** Lập trại → trồng linh dược → luyện khí cơ bản → đánh quái
**Vòng dài (10+ giờ):** Đột phá cảnh giới → vượt thiên kiếp → mở khu vực mới

---

## 3. Hệ thống chính

### 3.1. Sinh tồn (kế thừa từ DST)
| Chỉ số | Mô tả | Cách hồi |
|---|---|---|
| **HP (Khí huyết)** | Máu | Ăn, ngủ, đan dược |
| **Đói** | Cần ăn để không mất HP | Săn, trồng trọt, nấu ăn |
| **Khát** | Cần nước sạch | Suối, mưa, lọc nước |
| **SAN (Tinh thần)** | Giảm khi gặp quái dị / đêm tối | Ngồi thiền, ánh sáng, đan an thần |
| **Nhiệt độ** | Mùa đông lạnh, mùa hè nóng | Lửa trại, áo, hang động |

**Khác DST:** Có thêm chỉ số **Linh Khí (Mana)** — dùng cho công pháp / pháp bảo, hồi qua tu luyện.

### 3.2. Tu Tiên (kế thừa từ QCBH)
- **Linh căn (Spirit Root):** Random khi tạo nhân vật — Kim/Mộc/Thủy/Hỏa/Thổ/Lôi/Băng/... — quyết định công pháp tương thích.
- **Cảnh giới (Realms):**
  1. Phàm Nhân
  2. Luyện Khí (1–9 tầng) ← **MVP dừng ở đây**
  3. Trúc Cơ
  4. Kim Đan
  5. Nguyên Anh
  6. Hóa Thần
  7. (mở rộng sau)
- **Công pháp (Techniques):** Học qua sách/sư phụ/ngộ đạo. Mỗi công pháp có chiêu thức + buff.
- **Pháp bảo (Artifacts):** Vũ khí có linh tính — luyện hóa, dưỡng khí, nâng phẩm cấp.
- **Đan dược:** Luyện đan từ linh dược — hỗ trợ đột phá / hồi phục / chiến đấu.
- **Thiên kiếp:** Mỗi lần lên cảnh giới lớn → vượt kiếp (lôi kiếp, tâm ma kiếp).

### 3.3. Thế giới mở
- **Procedural generation** kết hợp **handcrafted landmarks** (giống DST: random nhưng có "set pieces").
- **Biome:**
  - Đồng cỏ (an toàn, tài nguyên cơ bản)
  - Rừng linh mộc (nhiều linh dược, có yêu thú)
  - Hoang mạc tử khí (nguy hiểm, linh khí đậm)
  - Đầm lầy oán linh (SAN giảm nhanh)
  - Tuyết sơn (lạnh giá, ẩn cốc tu tiên)
  - Bí cảnh (instance — vào theo sự kiện)
- **Ngày/đêm:** Đêm có quái nguy hiểm hơn, nhưng linh khí đậm hơn (tốt cho tu luyện).
- **Mùa:** 4 mùa, mỗi mùa ảnh hưởng tài nguyên + sự kiện.

### 3.4. Chế tạo & Cơ sở (Base Building)
- Lửa trại, lò luyện đan, đan đỉnh, trận pháp bảo vệ, linh điền (ruộng linh dược)
- **Động phủ:** Cố định 1 vị trí — buff tu luyện, kho đồ, bàn luyện đan.

### 3.5. Chiến đấu
- **Real-time, top-down 2D** (như DST) hoặc **2.5D isometric** (như QCBH).
- Skill chính: đánh thường, né, công pháp (cooldown + tốn linh khí), pháp bảo.
- Quái: từ thỏ rừng → yêu thú → ma tu → boss bí cảnh.

### 3.6. Death & Persistence
- Chết → mất đồ trên người, nhưng giữ:
  - Cảnh giới tu tiên (không mất)
  - Động phủ + đồ trong rương
  - Một phần kinh nghiệm công pháp
- Có cơ chế "**Nguyên Thần xuất khiếu**" — chết có thể chuyển sinh, giữ một phần ký ức.

---

## 4. Multiplayer (giai đoạn sau)
- **Co-op 2–4 người** giống DST: cùng sinh tồn, cùng tu luyện, cùng vượt kiếp.
- **PvP optional** ở khu vực "tranh đoạt linh mạch".
- *Không làm trong MVP — chỉ single-player trước.*

---

## 5. Tone & Art Direction

**Đề xuất:** **2D top-down hand-drawn** + tone **u tịch, mộng huyễn phương Đông** (kết hợp nét gothic-cartoon của DST với mỹ thuật thủy mặc của QCBH).

- Palette: tối, nâu/xanh rêu/đỏ máu (giống DST) nhưng điểm xuyết vàng kim/lam ngọc (linh khí).
- Animation: frame-by-frame style đơn giản, dễ scale.
- UI: phong cách trúc giản (thẻ tre) + cuộn giấy.

---

## 6. Monetization (tham khảo, không bắt buộc)
- **Premium** (mua 1 lần) — recommended cho game indie có chất.
- **Free-to-play + cosmetic** — không pay-to-win.
- *Tránh* gacha / pay-to-progress vì sẽ phá tinh thần "tu tiên gian khổ".

---

## 7. Rủi ro chính
| Rủi ro | Mức độ | Giảm thiểu |
|---|---|---|
| Scope quá lớn | **CAO** | Cắt MVP nhỏ, milestone rõ |
| Hiệu năng Android (procedural + nhiều entity) | Cao | Chunk-based world, ECS pattern |
| Cân bằng tu tiên vs sinh tồn | Trung | Playtest nhiều, tweak liên tục |
| Pin & nhiệt mobile | Trung | Cap FPS, tối ưu shader |
| Multiplayer netcode | Cao | Hoãn — làm sau MVP |
