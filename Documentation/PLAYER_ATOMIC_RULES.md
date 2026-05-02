# Player Atomic-Symbol Art Rules

Atomic-symbol composition rules cho rig-based puppet animation. **Bắt buộc** đối với
mọi PNG vào `Assets/_Project/Art/Characters/player/{E,N,S}/`. Vi phạm bất kỳ rule nào
sẽ tạo "rời rạc" effect (double-arm, baked sleeves overlapping rig sleeves, paper-doll
gaps).

> **Tại sao lại có rule này:** rig overlay separate body-part sprites trên 1 hierarchy.
> Nếu mỗi sprite có nội dung "thừa" (sleeve trong torso, neck trong head, hand trong
> forearm), rig sẽ render trùng nhiều lần → user nhìn 4 cánh tay thay vì 2.

## TL;DR — 5 luật atomic

1. **One part = one anatomical region. No more, no less.**
2. **Each PNG isolated, RGBA transparent BG, alpha bbox tight (≤5px padding).**
3. **No baked-in adjacent parts.** Torso không có shoulders/arms/neck/legs. Head không
   có neck/shoulders. Arm không có shoulder/torso/hand-detail. Forearm không có elbow
   shadow chạy lên arm. Leg không có hip/torso. Shin không có knee shadow chạy lên leg.
4. **Pivot convention.** Top-of-sprite = attach point cho parent joint (head→neck base,
   torso→hip base, arm→shoulder, forearm→elbow, leg→hip, shin→knee). Sprite hangs DOWN
   từ top pivot.
5. **3 directions consistent.** Cùng 1 part ở E/N/S phải cùng style/palette/proportion.
   N = back view (nhìn phía sau), S = front view (nhìn phía trước), E = side view
   (camera nhìn từ phải; W = flipX của E, không cần gen riêng).

---

## §1 Per-part atomic checklist

### head

| DO | DON'T |
|---|---|
| Hộp sọ + tóc + mặt only (E/S) hoặc hộp sọ + tóc only (N) | Cổ, vai, áo collar |
| Bottom edge cut clean ngay dưới cằm (jaw line) | Bottom edge tới collarbone hoặc thấp hơn |
| Hair bun / topknot có thể vượt ra ngoài hộp sọ phía trên | Râu kéo dài xuống cổ |

### torso

| DO | DON'T |
|---|---|
| Trunk only — cổ stub ngay dưới jaw đến hip line | Sleeves bell-flow extending past body width |
| Vai bịt kín ngang — top edge horizontal at shoulder height | Vai có puff hoặc tay áo dạng cánh |
| Hip cut clean — bottom edge horizontal at hip level | Hip có quần / leg fabric kéo xuống |
| Quần áo (kimono robe) bám sát thân trục — narrow profile | Quần áo flow-out, bell shape, hem |
| Sash / belt OK ở waist | Arms / hands / fingers visible anywhere |

> **Critical:** torso width chỉ là chest+belly width, không bao gồm vai mở rộng cho
> cánh tay. Vai = top horizontal edge của torso, không có "shoulder cap". Cánh tay
> riêng sẽ attach ngay tại top corner của torso.

### arm_left / arm_right (upper arm, shoulder → elbow)

| DO | DON'T |
|---|---|
| Cylindrical (≈ rectangular silhouette với rounded corners) | Bell-flow / triangular sleeve |
| Sleeve fits tight to limb — fabric đường viền song song với limb axis | Sleeve flares out, hem hanging |
| Top edge = shoulder cap (rounded), bottom edge = elbow joint (clean cut) | Hand / fingers visible at bottom |
| Width consistent từ top to bottom (chỉ 5-15% taper) | Width tapers >30% top-to-bottom |
| Sleeve color matches torso material | Solid block của sleeve cloth không có arm shape |

### forearm_left / forearm_right (elbow → wrist + hand)

| DO | DON'T |
|---|---|
| Forearm cylinder + visible hand at bottom (5 fingers visible khi neutral pose) | Hand cut off ở wrist |
| Top edge = elbow joint (rounded, fits arm bottom edge) | Continued sleeve flaring |
| Bottom = fingertips OR closed fist (consistent across 3 directions) | Floating hand không attached |
| Sleeve cuff visible ở wrist nếu kimono có cuff | Bell-flow sleeve covering hand |

### leg_left / leg_right (hip → knee, upper leg)

| DO | DON'T |
|---|---|
| Cylindrical thigh + pant fabric tight to limb | Pants flare out, hakama bell shape |
| Top edge = hip joint (clean horizontal cut) | Hip + waist + sash visible |
| Bottom edge = knee joint (rounded) | Shin / foot visible at bottom |
| Width consistent (thigh slightly wider than knee, max 20% taper) | Pants drape past knee |

### shin_left / shin_right (knee → ankle + foot)

| DO | DON'T |
|---|---|
| Shin cylinder + foot/boot at bottom | Foot cut off at ankle |
| Top edge = knee joint (rounded) | Continued thigh / hip visible |
| Bottom = sole of foot/boot (consistent ground line cross 3 dirs) | Floating foot không attached to shin |
| Boot / shoe ankle wrap visible nếu có | Pants fabric draping over boot |

---

## §2 File / folder convention

```
Assets/_Project/Art/Characters/player/
├── E/                        ← side view (right-facing). W = flipX of E auto.
│   ├── head.png              ~210×235 (range 160-250 × 200-270)
│   ├── torso.png             ~130×290 (range 100-150 × 240-340)  ← NEW narrow trunk
│   ├── arm_left.png          ~80×200  (range 60-120 × 170-230)
│   ├── arm_right.png         ~80×200
│   ├── forearm_left.png      ~80×220  (range 50-110 × 180-260)   ← NEW with hand
│   ├── forearm_right.png     ~80×220
│   ├── leg_left.png          ~95×220  (range 70-125 × 190-250)
│   ├── leg_right.png         ~95×220
│   ├── shin_left.png         ~100×210 (range 70-130 × 180-240)
│   └── shin_right.png        ~100×210
├── N/  (back view)           similar dims, head shows back-of-skull only
└── S/  (front view)          similar dims, full face visible
```

> **Width caps are STRICT for torso (max 150).** Current art has torso width 156 (E)
> and 212 (S) because sleeves are baked into the torso PNG. After regen với trunk-only
> prompt, torso width sẽ ~110-140. Validator sẽ FAIL bất kỳ torso > 150 wide.

**RGBA mode required** (transparent BG). PNG-8 indexed colors KHÔNG OK.

**Alpha bbox** phải tight: ≤ 5px transparent padding all sides. Validator script
auto-detect và recommend crop.

---

## §3 Workflow

1. Gen 30 PNGs theo prompt trong [`PLAYER_ATOMIC_ART_PROMPTS.md`](PLAYER_ATOMIC_ART_PROMPTS.md).
2. Save vào `Assets/_Project/Art/Characters/player/{E,N,S}/{part}.png`.
3. Run `python3 .agents/scripts/validate_player_art.py` từ repo root.
4. Read report. Fix any `FAIL:` items (auto-crop / re-gen part / rename).
5. Re-run validator until tất cả `OK`.
6. Re-bootstrap MainScene (`Tools → Wilderness Cultivation → Bootstrap Default Scene`)
   trong Unity.
7. Play. Walk Player ở 3 directions. Verify smooth puppet animation, no rời rạc.

## §4 Common mistakes

- **Sleeves baked into torso** (current player art has this). Fix: regen torso prompt
  EMPHASIZE "trunk only no sleeves no arms". Use negative prompts: `"no sleeves",
  "no arms", "no flowing fabric past body", "narrow profile"`.
- **Bell-flow arm sprites** (current player arm has this). Fix: regen arm prompt
  EMPHASIZE "cylindrical arm with tight sleeve, hand visible at bottom". Negative:
  `"no bell flare", "no flowing cloth", "no triangular shape"`.
- **No hand visible** in forearm. Fix: explicit "5 fingers visible" or "closed fist
  at bottom of forearm".
- **Missing N/S sprites** for arm/forearm. OK to skip — rig auto-hides arm sprites
  in N/S anyway (per `PuppetAnimController.hideArmsInFrontBackView`). But head/torso/
  leg/shin MUST exist for all 3 directions.
- **PNG-8 indexed mode** instead of RGBA. Validator will catch this. Re-export RGBA.

## §5 Reference

- Animation rig logic: `Assets/_Project/Scripts/Vfx/PuppetAnimController.cs`
- Bootstrap pipeline: `Documentation/PUPPET_PIPELINE.md`
- Style lock + palette + ink-wash spec: `Documentation/ART_STYLE.md`
- Prompts: `Documentation/PLAYER_ATOMIC_ART_PROMPTS.md`
- Validator: `.agents/scripts/validate_player_art.py`
