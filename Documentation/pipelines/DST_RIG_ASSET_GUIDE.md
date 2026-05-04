---
name: dst-rig-asset-guide
audience: both
status: active
scope: 7 practical rules cho asset rig-ready (silhouette/pivot/overlap/depth ownership).
depends-on:
  - PUPPET_PIPELINE.md
  - ../art/AI_PROMPTS.md
---
# 7 nguyên tắc để asset player nhìn mượt như DST trong rig hiện tại

Practical guide cho repo này: làm sao để bộ PNG `head / torso / arm / forearm / leg / shin` khi ráp vào procedural puppet hiện tại nhìn **liền khối, có weight, ít cảm giác paper-doll**.

> Scope của file này là **asset-side rules** cho rig đang có sẵn trong repo. Không bàn prompt theory chung.
>
> Rig hiện tại: `PuppetAnimController` procedural rigid-limb, multi-direction `E/N/S`, West = flip East.
>
> Files liên quan:
> - `Assets/_Project/Scripts/Vfx/PuppetAnimController.cs`
> - `Assets/_Project/Scripts/Core/PuppetPlaceholderSpec.cs`
> - `Assets/_Project/Scripts/Core/CharacterRigSpec.cs`
> - `Assets/_Project/Scripts/Core/CharacterArtSpec.cs`
> - `Documentation/PUPPET_PIPELINE.md`
> - `Documentation/AI_PROMPTS.md`

---

## TL;DR

Nếu chỉ nhớ 1 câu:

> **DST-looking motion không đến từ việc từng PNG đẹp riêng, mà từ việc mọi part được vẽ để phục vụ cùng một rig.**

Cho rig hiện tại của repo này, asset player sẽ nhìn mượt hơn rất nhiều nếu:

1. mọi part sinh ra từ **cùng một source design**
2. silhouette từng part **đơn giản, rõ, đọc nhanh**
3. pivot và cut boundary **đúng khớp xoay**
4. joint overlap được chừa để **xoay không hở khớp**
5. side view có **near/far depth** rõ
6. N/S view obey rule **torso own silhouette, arms hide**
7. paint/shading support motion, không phá form khi rotate

---

## Nguyên tắc 1 — Thiết kế từ cùng một source, không để từng part “tự nghĩ riêng”

### Vì sao

`PuppetAnimController` chỉ biết xoay transform. Nó **không sửa shape** giúp bạn. Nếu `arm_left.png`, `forearm_left.png`, `torso.png` được gen như ba object độc lập với ba logic hình khối khác nhau, khi ráp lại sẽ lập tức ra cảm giác rời rạc.

### Rule

Player final nên đi theo thứ tự này:

1. **Lock 1 full-body ref**
2. **Lock 1 direction ref** cho `E`, `N`, `S`
3. Từ cùng source đó mới tách `head / torso / arm / forearm / leg / shin`

### Trong repo này nghĩa là gì

- `Documentation/AI_PROMPTS.md` nên được dùng để lock `master ref` + `direction logic`
- `Documentation/PLAYER_FULL_ASSET_SOURCE_PROMPT.md` nên được dùng để gen 1 source board đồng bộ trước khi extract
- Part production nên ưu tiên:
  - **image-edit từ cùng ref**, hoặc
  - **direction sheet rồi extract/crop**, hoặc
  - cleanup tay sau khi gen
- Không nên lấy 22 text-only prompts độc lập làm final production path

### Good

- Head, torso, limbs share cùng outline weight, brush texture, proportion, costume logic
- Cuff band của forearm đúng với cuff width thấy trên full-body ref
- Boot volume match với shin volume và leg width

### Bad

- Head painterly nhưng arm lại như clean cylinder
- Torso chibi nhưng shin lại gầy và dài theo style khác
- Forearm có cuff 12px, arm lại taper như 1 bút chì, ráp vào bị lệch hệ form

---

## Nguyên tắc 2 — Ưu tiên silhouette rõ hơn là texture đẹp

### Vì sao

DST nhìn mượt vì silhouette cực dễ đọc khi part xoay vài độ:
- đầu to
- torso như khối rõ
- tay/chân như block đơn giản
- outline mạnh

Rig hiện tại cũng dựa vào điều đó. Walk/lunge/crouch đều là transform math, nên mắt người đọc chuyển động chủ yếu qua **shape change của silhouette**, không phải qua chi tiết trong fill.

### Rule

Mỗi part phải đọc được ngay ở thumbnail nhỏ:
- `head`: mass rõ, bun rõ, forelock rõ
- `torso`: trunk rõ, robe bottom rõ, sash rõ
- `arm`: cylinder rõ
- `forearm`: sleeve → cuff → fist đọc một phát ra ngay
- `leg`: thigh mass rõ
- `shin`: shin → boot → sole đọc ngay

### Good

- Arm/leg là khối đơn giản, không quá nhiều nếp lắt nhắt
- Torso silhouette nhìn ra ngay hip-length robe
- Shin nhìn ra ngay boot top vs trouser hem

### Bad

- Một part có quá nhiều texture nhưng mất hình khối
- Robe edge lởm chởm, khi xoay silhouette rung bẩn
- Forearm shadow làm fist chìm vào cuff

### Check nhanh

Thu nhỏ ảnh xuống khoảng 15-20% zoom:
- còn đọc ra part là gì không?
- cuff / boot / fist / bun có còn tách bạch không?

---

## Nguyên tắc 3 — Cut boundary phải đúng khớp, không cắt theo chỗ “đẹp tranh”

### Vì sao

Rig xoay tại khớp. Nếu PNG bị cắt ở chỗ không trùng khớp giải phẫu, part sẽ trông như bị gãy khi animate.

`PuppetPlaceholderSpec.PivotFor()` đã lock pivot scheme của rig:

- `Head`: `(0.5, 0)` = bottom-center = neck attach
- `Torso`: `(0.5, 0.5)` = center mass
- `Arm / Forearm / Leg / Shin`: `(0.5, 1)` = top-center = shoulder / elbow / hip / knee attach

### Rule theo từng part

#### Head
- Cut ở **jaw/neck attach**, không kéo thêm vai/collar xuống dưới
- Bottom edge phải sạch để neck attach tự nhiên vào torso

#### Torso
- Chỉ là **trunk**
- Không nuốt tay vào torso side view nếu rig đang dùng arm riêng
- Top/bottom edge phải đọc ra vai/hông clean

#### Arm
- Upper arm only: shoulder → elbow
- Không lộ cuff hoặc hand ở file arm

#### Forearm
- Elbow → wrist → fist
- Cuff phải nằm trong forearm file, không nằm nửa ở arm nửa ở forearm

#### Leg
- Hip → knee only
- Không ăn xuống boot

#### Shin
- Knee → sole
- Boot phải nằm trong shin file

### Bad phổ biến

- Torso có sẵn cả half-sleeve bên ngoài → khi arm attach thành double shoulder
- Arm dưới cùng lại có hint cuff → ráp với forearm bị double cuff
- Leg kéo luôn cả gấu quần, shin lại có thêm gấu quần lần nữa → knee line bẩn

---

## Nguyên tắc 4 — Joint overlap là bắt buộc, không phải lỗi

### Vì sao

Rig hiện tại đã có support để che joint seam:
- `CharacterRigSpec.elbowOverlap`
- `CharacterRigSpec.kneeOverlap`

Mục tiêu là **top edge của child part chui nhẹ vào trong parent** để lúc xoay không hở đường cắt.

### Rule

Khi vẽ / cleanup asset, luôn nghĩ theo cặp:
- `arm` che top của `forearm`
- `leg` che top của `shin`
- `torso` che top của `leg`
- `torso/neck area` đỡ đáy của `head`

### Asset-side implication

- Top 5-10% của forearm/shin nên “hy sinh” cho overlap, đừng dồn detail quan trọng vào đó
- Detail đẹp như cuff / trouser hem / boot top nên đặt thấp hơn vùng seam một chút
- Nếu bạn đặt cuff sát mép elbow-side, overlap sẽ ăn mất cuff khi rig

### Good

- Forearm top đủ sạch để chui dưới arm
- Shin top đủ sạch để chui dưới leg
- Trouser hem và cuff visible sau khi overlap

### Bad

- Cuff đặt quá cao → khi overlap bị torso/arm nuốt mất
- Boot top đặt đúng ngay seam → knee bend làm bẩn silhouette
- Forearm top méo / rách / nhiều texture → khi chui vào arm nhìn seam rất rõ

---

## Nguyên tắc 5 — Side view phải có near/far depth, nếu không sẽ ra “paper doll”

### Vì sao

Fix lớn của rig hiện tại là side-view occlusion:
- `enableSideViewOcclusion = true`
- far limb `scale.x *= 0.92`
- far limb `sortingOrder += -2`

Nghĩa là repo đã cố tình làm giống DST: limb phía xa camera **nhỏ hơn một chút và nằm sau torso**.

### Asset-side rule

Khi vẽ E view, phải nghĩ rõ:
- part nào là **near-side**
- part nào là **far-side**

### Cụ thể

#### E direction
- `arm_right`, `leg_right`, `forearm_right`, `shin_right` = near-side
- `arm_left`, `leg_left`, `forearm_left`, `shin_left` = far-side

Near-side nên:
- silhouette đầy hơn một chút
- có quyền đọc rõ hơn một chút

Far-side nên:
- đơn hơn
- gọn hơn
- tránh chi tiết cạnh tranh với torso

### Bad

- Near/far tay giống hệt nhau 100% và cùng “đậm lực” → side view bị phẳng
- Far leg to hơn cả near leg → perspective sai
- Torso quá mỏng khiến far limb lòi hết ra ngoài, mất cảm giác body che bớt

### Practical note

Không cần vẽ 2 tay khác hẳn nhau. Chỉ cần:
- far-side nhẹ hơn
- gọn hơn
- bớt aggressive shape hơn

là rig side view sẽ believable hơn nhiều.

---

## Nguyên tắc 6 — Mỗi direction phải có ownership rõ, đặc biệt N/S

### Vì sao

Repo hiện tại có rule rất quan trọng:

- `hideArmsInFrontBackView = true`

Tức là ở `N` và `S`, arm/forearm sprites có thể bị hide để **torso own silhouette**.

### Hệ quả asset-side

#### East (E)
- Dùng full articulated puppet: head, torso, arm, forearm, leg, shin
- Đây là direction quan trọng nhất cho gameplay readability

#### North (N) / South (S)
- Torso phải tự mang silhouette front/back đủ ổn
- Nếu sleeve/cánh tay cần nhìn thấy ở front/back, chúng phải nằm trong **torso drawing logic**, không trông chờ arm sprite riêng cứu

### Rule practical

#### S torso
- front silhouette phải tự đứng được nếu arm sprites tắt
- V-neck, sash, jade, cloud sigil phải đọc rõ từ torso alone

#### N torso
- back silhouette phải gọn, không cần front details
- nếu sash tails visible thì xem như torso detail, không coi là arm detail

### Bad

- S torso quá “trần”, trông chờ arm sprite để hoàn thành silhouette → khi arms hide thì nhân vật mất form
- N view vẫn cố nhét too much front detail
- Front/back torso width không consistent với E torso

---

## Nguyên tắc 7 — Paint và shading phải support motion, không chống lại motion

### Vì sao

Rig này là rigid-limb. Nó không deform mesh như bone weight mượt. Vì vậy texture/shading phải hỗ trợ chuyển động:
- nếp lớn, rõ
- value grouping rõ
- không có line detail mâu thuẫn khi xoay

### Rule

#### Outline
- giữ thickness khá ổn định trong cùng một character
- part nào cũng cùng “họ bút”
- không để head có outline đẹp mà limb lại quá clean/vector

#### Shading
- ưu tiên 3-level tonal grouping rõ
- highlight/shadow đi theo khối lớn, không băm vụn
- không vẽ seam/highlight làm khớp trông như bị bẻ gãy

#### Limb-specific
- arm/leg: shading nên làm rõ cylinder volume
- forearm: sleeve, cuff, fist phải tách value rõ
- shin: trouser và boot phải tách bằng value + material difference

### Good

- Boot đọc ra leather mass rõ khi shin swing
- Cuff vẫn giữ identity khi forearm xoay
- Torso value grouping không “đảo sáng” kỳ lạ khi body torsion nhẹ

### Bad

- Highlight đặt đúng seam làm seam lộ hơn khi animate
- Nếp vải hướng ngược hoàn toàn trục khối → xoay lên trông sai form
- Fist và cuff cùng value, cùng hue, nhập vào nhau

---

# Mapping thẳng vào rig hiện tại

## 1. Motion behaviors asset phải chịu được

`PuppetAnimController` đang làm những thứ này:

- walk arm swing
- walk leg swing
- idle breathing bob
- body torsion
- elbow bend khi walk/lunge
- knee bend khi walk/crouch
- side-view occlusion
- front/back arm hiding

Vì vậy asset cần survive các tình huống:

### Head
- idle bob
- slight relation with torso
- neck attach không bị lộ seam

### Torso
- torso bob
- torso counter-rotate nhẹ (`bodyTorsionDeg`)
- side-view body vẫn đủ mass để che far limb

### Arm + Forearm
- arm swing
- forearm bend tại elbow
- lunge arm thrust

### Leg + Shin
- leg swing tại hip
- shin bend tại knee
- crouch body drop nhưng boot vẫn vững form

---

## 2. Hiểu đúng role order để paint cho đúng

`CharacterArtSpec.PuppetRole` đang định sorting/read order như sau:

- `LegLeft`, `LegRight`
- `Torso`
- `ArmLeft`, `ArmRight`
- `Head`
- `ShinLeft`, `ShinRight`
- `ForearmLeft`, `ForearmRight`

Ý practical:
- head luôn là readability anchor cao nhất
- torso là body mass chính
- forearm/shin là articulation detail ở đầu limb
- side view vẫn cần torso đủ khối để che far-side limb

---

# Checklist trước khi drop PNG vào rig

## A. Character-level

- [ ] Tất cả part nhìn như cùng một nhân vật, cùng một brush language
- [ ] Head / torso / limbs cùng một hệ proportion
- [ ] Palette giữa các part không drift
- [ ] Near/far limb side view có hierarchy rõ

## B. Head

- [ ] Bottom attach sạch, không kéo vai/collar xuống
- [ ] Bun + ribbon + forelock silhouette rõ ở thumbnail nhỏ
- [ ] Outline weight match torso/limb

## C. Torso

- [ ] Chỉ là trunk, không double-arm silhouette
- [ ] Hip-length robe đọc rõ
- [ ] Sash / pendant / sigil không phá khối torso
- [ ] N/S torso vẫn tự đứng được nếu arm hide

## D. Arm / Forearm

- [ ] Arm = shoulder → elbow only
- [ ] Forearm = elbow → fist only
- [ ] Cuff thuộc forearm, không bị split giữa 2 file
- [ ] Fist đọc rõ, không dính vào cuff
- [ ] Forearm top đủ sạch cho overlap

## E. Leg / Shin

- [ ] Leg = hip → knee only
- [ ] Shin = knee → sole only
- [ ] Trouser hem / boot top tách rõ
- [ ] Boot đọc ra mass vững, không clown-foot
- [ ] Shin top đủ sạch cho overlap

## F. Direction rules

- [ ] E readable nhất, full articulation
- [ ] N/S obey torso-owns-silhouette rule
- [ ] W có thể flip từ E mà vẫn hợp lý

---

# Good / bad target cho repo này

## Good target

- Nhìn từng part riêng có thể không “wow”, nhưng ráp lại rất liền
- Khi walk, tay chân có trọng lượng, không như dán giấy
- Khi lunge, forearm bend vẫn sạch silhouette
- Khi side view, near/far arm có depth
- Khi front/back, torso không bị rỗng vì arm hide

## Bad target

- Từng part trông đẹp như sticker riêng lẻ nhưng ráp lại không cùng hệ form
- Nhiều detail nhỏ nhưng seam/joint lộ mạnh
- Torso quá phẳng nên không che được far limb
- Arm/forearm/leg/shin mỗi file một “ngôn ngữ volume” khác nhau

---

# Recommendation production workflow cho player

Để đạt vibe gần DST nhất với rig hiện tại, workflow practical nên là:

1. **Lock 1 full-body master ref**
2. **Lock 3 direction refs** (`E`, `N`, `S`)
3. **Tách part từ cùng source** hoặc image-edit từ cùng ref
4. **Cleanup tay các joint** (elbow, knee, neck, hip)
5. **Kiểm tra silhouette ở walk/lunge/crouch**
6. Chỉ sau đó mới iterate prompt tiếp

Nếu phải chọn ưu tiên, thì thứ tự đúng là:

1. same-source consistency
2. silhouette clarity
3. pivot/cut correctness
4. overlap cleanup
5. side-view depth
6. N/S ownership
7. texture polish

---

# Liên kết với docs khác

- `Documentation/PUPPET_PIPELINE.md` — pipeline tổng quát drop PNG → bootstrap → run puppet
- `Documentation/AI_PROMPTS.md` — prompt generation rules + final atomic prompts
- `Documentation/PLAYER_FULL_ASSET_SOURCE_PROMPT.md` — one-source-board workflow trước khi tách part
- `Documentation/PLAYER_DST_REFERENCE.md` — visual identity lock (Soft-DST × Wuxia)
- `Documentation/BONE_RIG_GUIDE.md` — hướng khác dùng Unity 2D bones; không phải pipeline mặc định của rig player hiện tại

---

# Final note

Với repo này, asset “nhìn mượt như DST” không có nghĩa là clone đúng kỹ thuật nội bộ của Klei. Nó có nghĩa là:

- part đơn giản nhưng đúng khối
- joint sạch
- near/far rõ
- rotate lên vẫn believable
- mọi part phục vụ cùng một motion system

Nếu một PNG riêng lẻ bớt đẹp đi một chút nhưng cả character animate mượt hơn, thì đó là lựa chọn đúng.