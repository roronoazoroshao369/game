# Skill: Add new mob (AI)

Mục tiêu: thêm 1 loại quái mới (vd: Bear / SpiderQueen / Cultist) theo pattern `WolfAI` / `FoxSpiritAI`.

## Bước

1. Tạo `Assets/_Project/Scripts/Mobs/<MobName>AI.cs` extend `MobBase`.
2. Override `Awake()` để set `mobName`.
3. Implement `Update()`:
   - `TryFindPlayer()` → return early nếu không có target
   - Compute `dist = Vector2.Distance(target.position, transform.position)`
   - Nếu `dist > attackRange` → `MoveTowards(target.position)`
   - Nếu `dist <= attackRange` + `Time.time >= attackReadyAt` → attack
4. **CRITICAL: PlayerStats fallback.** Pattern bắt buộc cho mọi mob đánh player:
   ```csharp
   var dmg = target.GetComponent<IDamageable>() ?? target.GetComponentInParent<IDamageable>();
   if (dmg != null)
   {
       dmg.TakeDamage(damage, gameObject);
   }
   else
   {
       // PlayerStats KHÔNG implement IDamageable; fallback giống BossMobAI/WolfAI.
       var ps = target.GetComponent<PlayerStats>() ?? target.GetComponentInParent<PlayerStats>();
       ps?.TakeDamage(damage);
   }
   ```
   **KHÔNG** chỉ dùng `dmg?.TakeDamage(...)` — sẽ no-op khi target là player.
5. Register vào `BootstrapWizard` để spawn vào default scene (optional).
6. Viết PlayMode test (xem `add-play-mode-test`):
   - Aggro within range → `target != null`
   - Aggro outside range → `target == null`
   - Attack damages player (verify `PlayerStats.HP` giảm)
   - Drop loot to killer's inventory
   - Award XP to killer's RealmSystem

## Reference implementations

- `Assets/_Project/Scripts/Mobs/WolfAI.cs` — chase + melee
- `Assets/_Project/Scripts/Mobs/FoxSpiritAI.cs` — night-only gating qua `TimeManager.isNight`
- `Assets/_Project/Scripts/Mobs/BossMobAI.cs` — đa pha, projectile pattern, summon

## Pitfalls

- **Quên fallback** → mob "đứng đó đánh không khí", player không mất HP. **Đây là production bug
  thật đã xảy ra với WolfAI + FoxSpiritAI ban đầu** (PR #24).
- **`playerMask` mặc định `0`** → `Physics2D.OverlapCircle` không match gì. Phải set qua wizard hoặc
  inspector về layer "Player".
- **`drops` null** → `MobBase.Die` `foreach (var d in drops)` NPE. Init `drops = Array.Empty<>()`.
- **Aggro on hit** đã handle trong `MobBase.TakeDamage` — không override trừ khi cần aggro chain.
