# Documentation index — Wilderness Cultivation Chronicle

> **AI agent + human map of the project knowledge base.**
> Đây là entry point chính. Mọi doc trong `Documentation/` được index dưới đây với audience tag, status, và prerequisite chain. Dùng map này khi cần định vị nhanh thay vì grep toàn repo.

---

## Reading order — first time setup

| # | Doc | Lý do đọc trước |
|---|---|---|
| 1 | [`../AGENTS.md`](../AGENTS.md) | HARD constraints + CI gates — KHÔNG vi phạm |
| 2 | [`../DESIGN_PRINCIPLES.md`](../DESIGN_PRINCIPLES.md) | 10 DO / DON'T với code example |
| 3 | [`../ARCHITECTURE.md`](../ARCHITECTURE.md) | Module map + pattern inventory + data flow |
| 4 | [`../REFACTOR_HISTORY.md`](../REFACTOR_HISTORY.md) | R1..R7 timeline (pattern từ đâu tới) |
| 5 | **This file** (`Documentation/INDEX.md`) | Map các doc design / art / pipeline |
| 6 | [`design/GDD.md`](design/GDD.md) | Game vision + core loop |
| 7 | [`design/MVP_SCOPE.md`](design/MVP_SCOPE.md) | Feature scope hiện tại |
| 8 | [`art/ART_STYLE.md`](art/ART_STYLE.md) | Style lock + palette |
| 9 | [`../.agents/skills/README.md`](../.agents/skills/README.md) | Procedures "làm X như thế nào" |

> Đây là chuỗi cho contributor lần đầu. AI agent có thể skip 6-9 nếu task không cần (ví dụ: bug-fix code thuần thì 1-4 đủ).

---

## Knowledge base layout

```
Documentation/
├── INDEX.md                    ← this file (map tổng)
├── design/                     ← game design + scope (5 docs)
├── art/                        ← visual identity + AI prompt catalog (5 docs)
├── pipelines/                  ← asset pipeline (4 docs: 3 cũ + 1 mới)
└── assets/                     ← style ref PNG (Documentation-only, không bundle vào APK)
```

---

## §1 design/ — game design + scope

| Doc | Audience | Status | Khi cần đọc |
|---|---|---|---|
| [`design/GDD.md`](design/GDD.md) | both | active | Hiểu vision, core loop, hệ thống tu tiên (linh căn / cảnh giới / công pháp / đan / thiên kiếp) |
| [`design/MVP_SCOPE.md`](design/MVP_SCOPE.md) | both | active — auto-updated mỗi PR | Check feature đã có gì, còn thiếu gì để demo APK |
| [`design/ROADMAP.md`](design/ROADMAP.md) | both | active | Long-term 5-stage roadmap (Pre-production → Release) |
| [`design/WORLD_MAP_DESIGN.md`](design/WORLD_MAP_DESIGN.md) | both | active | 3 biome chi tiết + mob spawn table + asset checklist (post-MVP target) |
| [`design/DESIGN_PERMADEATH_AWAKENING.md`](design/DESIGN_PERMADEATH_AWAKENING.md) | both | proposal | Hệ permadeath + soul awakening (chưa implement, design-stage) |

---

## §2 art/ — visual identity + AI prompt catalog

| Doc | Audience | Status | Khi cần đọc |
|---|---|---|---|
| [`art/ART_STYLE.md`](art/ART_STYLE.md) | both | active | Style bible: palette HEX, line weight, naming convention. **Đọc trước khi gen art mới.** |
| [`art/AI_PROMPTS.md`](art/AI_PROMPTS.md) | both | active — Player v2 LOCKED 10/10 PASS (May 2026) | Master AI prompt catalog (2288 line). Single source of truth cho prompt sinh art. Style identity LOCKED = "Chibi Wuxia × Soft-DST". |
| [`art/PLAYER_DST_REFERENCE.md`](art/PLAYER_DST_REFERENCE.md) | both | active — Player identity locked | Visual signature lock + reference image. Đọc trước `AI_PROMPTS.md` §3 nếu muốn re-gen player. |
| [`art/PLAYER_FULL_ASSET_SOURCE_PROMPT.md`](art/PLAYER_FULL_ASSET_SOURCE_PROMPT.md) | both | active | 1 prompt GPT image gen full asset source board cho player → nguồn cho 30-part atomic extraction. |
| [`art/PLAYER_SOURCE_BOARD_EXTRACTION.md`](art/PLAYER_SOURCE_BOARD_EXTRACTION.md) | both | active | Workflow extract 30 part từ source board (image-edit / manual crop). |

> Sequence cho **regen Player asset**:
> 1. `art/PLAYER_DST_REFERENCE.md` — visual signature lock
> 2. `art/AI_PROMPTS.md` §3 — master prompt + acceptance test
> 3. `art/PLAYER_FULL_ASSET_SOURCE_PROMPT.md` — 1 PNG có 4 angle T-pose
> 4. `art/PLAYER_SOURCE_BOARD_EXTRACTION.md` — extract 30 atomic part
> 5. Drop vào `Assets/_Project/Art/Characters/player/{E,N,S}/{part}.png`

---

## §3 pipelines/ — asset pipeline (PNG → Unity)

| Doc | Audience | Status | Khi cần đọc |
|---|---|---|---|
| [`pipelines/PUPPET_PIPELINE.md`](pipelines/PUPPET_PIPELINE.md) | both | active — default pipeline | 2D procedural rectangle puppet rig (DST-style). 30 PNG part → demo runs. **Default cho mọi character + mob.** |
| [`pipelines/BONE_RIG_GUIDE.md`](pipelines/BONE_RIG_GUIDE.md) | both | alternative | Unity 2D bone rig package (PSB layered import). Alternative cho character cần advanced animation, hiện chưa default. |
| [`pipelines/DST_RIG_ASSET_GUIDE.md`](pipelines/DST_RIG_ASSET_GUIDE.md) | both | active | 7 nguyên tắc practical về silhouette / pivot / overlap / depth ownership cho asset rig-ready. **Đọc trước khi gen art.** |
| [`pipelines/PNG_TO_3D_TO_SPRITE_PIPELINE.md`](pipelines/PNG_TO_3D_TO_SPRITE_PIPELINE.md) | both | research / proof-of-concept (2026-05) | NEW — pipeline 3D-mediated: PNG concept → Tripo3D/Meshy → Mixamo rig + 10 anim → Blender toon shader + ortho 4-dir bake → Unity sprite atlas + AnimatorController. Áp dụng cho Player + 3 hero mob (cần ≥ 6 anim clip). |

> Quyết định pipeline cho 1 asset: xem `pipelines/PNG_TO_3D_TO_SPRITE_PIPELINE.md` §2 (decision matrix) hoặc `pipelines/PUPPET_PIPELINE.md` §1.

---

## §4 assets/ — style reference media

`assets/style_refs/` chứa PNG reference cho prompt workflow (vd `player_E.png` làm `--cref` / IP-Adapter input).
`assets/preview/` chứa render preview của puppet rig.

Không bundle vào APK — các file này chỉ cho contributor + AI agent reference.

---

## §5 Cross-references ngoài Documentation/

| Doc | Path | Mục đích |
|---|---|---|
| AGENTS rules | [`../AGENTS.md`](../AGENTS.md) | HARD constraints, CI gates |
| Design principles | [`../DESIGN_PRINCIPLES.md`](../DESIGN_PRINCIPLES.md) | DO / DON'T với code example |
| Architecture | [`../ARCHITECTURE.md`](../ARCHITECTURE.md) | Module map + pattern inventory |
| Refactor history | [`../REFACTOR_HISTORY.md`](../REFACTOR_HISTORY.md) | R1..R7 timeline |
| LLM behavioral guide | [`../CLAUDE.md`](../CLAUDE.md) | Behavioral guidelines for Claude / LLM |
| Skills index | [`../.agents/skills/README.md`](../.agents/skills/README.md) | Procedures (add-mob, add-npc, add-recipe, ...) |
| Prompts directory | [`../prompts/README.md`](../prompts/README.md) | Tile / hero raw prompt files (Leonardo + GPT image) |
| Asset folder | `Assets/_Project/Art/` (in-tree) | Final character / tile / icon PNG dùng trong APK |

---

## §6 Convention cho doc mới

Khi thêm doc vào `Documentation/`:

1. **Chọn subfolder đúng**: `design/` (gameplay scope) | `art/` (visual identity) | `pipelines/` (asset workflow). Nếu không khớp → bàn với maintainer.
2. **Frontmatter YAML** ở đầu file:
   ```yaml
   ---
   name: <kebab-case-id>
   audience: ai-agent | human | both
   status: active | proposal | research | deprecated
   scope: <1 dòng tóm tắt scope>
   depends-on:
     - <relative path to prerequisite docs>
   ---
   ```
3. **Append entry vào INDEX.md** (this file) — table phù hợp + status tag.
4. **Cross-link** từ relevant doc khác (vd new pipeline → reference từ `PUPPET_PIPELINE.md` decision matrix).
5. **PR**: 1 doc / PR (DESIGN_PRINCIPLES rule 10), atomic review.

---

## §7 Audience tag — giải thích

- **ai-agent**: tối ưu cho AI assistant (Devin / Claude / Cursor) — schema rõ, code skeleton, no ambiguity. Có thể terse cho human.
- **human**: tối ưu cho contributor đọc — narrative, diagram, examples nhiều.
- **both**: cân bằng — đa số doc rơi vào nhóm này.

---

## §8 Status tag — giải thích

| Tag | Ý nghĩa |
|---|---|
| **active** | Đang dùng, source of truth. Update khi liên quan thay đổi. |
| **proposal** | Design đề xuất, chưa implement. Có thể thay đổi. |
| **research / proof-of-concept** | Khám phá tool / approach mới. Chưa ship character/feature qua. |
| **deprecated** | Lỗi thời, giữ làm lịch sử. Không dùng cho work mới. |

---

## §9 Last sync

- 2026-05-03 — Reorg flat `Documentation/*.md` thành `design/`, `art/`, `pipelines/` subfolder. Add `INDEX.md` (this file). Add `pipelines/PNG_TO_3D_TO_SPRITE_PIPELINE.md` (research doc). Update internal links + top-level pointer.
