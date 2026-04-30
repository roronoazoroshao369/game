# Skill: Add new mob (AI)

Mục tiêu: thêm 1 loại quái mới (vd: Bear / SpiderQueen / Cultist) theo pattern
`WolfAI` / `RabbitAI` (FSM, R7 pattern) hoặc `FoxSpiritAI` (Update if/else, pre-R7 legacy).

## Pattern khuyến nghị: FSM (R7)

Mob mới nên dùng `StateMachine<T>` + `IState<T>` trong `Scripts/Core/StateMachine.cs`.
Lý do: state transition rõ ràng, OnEnter/OnExit hook đúng vị trí, dễ extend Block/Parry/Charge.

### Bước

1. Tạo `Assets/_Project/Scripts/Mobs/<Name>AI.cs` extend `MobBase` với field
   `internal readonly StateMachine<NameAI> Fsm = new();`
2. Tạo `Assets/_Project/Scripts/Mobs/States/<Name>States.cs`:
   - `public static class <Name>States { public static readonly IState<NameAI> Idle = ...; ... }`
   - Mỗi state là `sealed class` implement `IState<NameAI>` (Enter/Tick/Exit).
   - State singleton (no alloc per frame).
3. Override `Awake()`: gọi `base.Awake()`, set `mobName`, `Fsm.Init(this, <Name>States.Idle)`.
4. Implement `Update()`:
   ```csharp
   void Update()
   {
       if (!ShouldTickAI()) return;
       Fsm.Tick(Time.deltaTime);
   }
   ```
5. Override `Die()`:
   ```csharp
   protected override void Die(GameObject killer)
   {
       Fsm.Shutdown();
       base.Die(killer);
   }
   ```
6. Damage qua `IDamageable` (R2). Rule 2 AGENTS:
   ```csharp
   var dmg = target.GetComponent<IDamageable>() ?? target.GetComponentInParent<IDamageable>();
   if (dmg != null) dmg.TakeDamage(damage, gameObject);
   ```
7. State truy cập field của mob qua instance param (same-assembly `internal`):
   `w.target`, `w.attackRange`, `w.MoveTowards(...)`, `w.AttackReadyAt`, …
8. Register vào `BootstrapWizard` để spawn vào default scene (optional).
9. Viết PlayMode test (xem `add-play-mode-test`):
   - Aggro within range → `target != null`
   - Aggro outside range → `target == null`
   - Attack damages player (verify `PlayerStats.HP` giảm)
   - Drop loot to killer's inventory
   - Award XP to killer's RealmSystem

## Pattern legacy (pre-R7): Update if/else

Nếu mob rất đơn giản (< 30 LoC AI logic), if/else trong `Update()` vẫn OK. Mob legacy
(FoxSpirit, Snake, Bat, Boar, Boss, Deer, Crow) dùng pattern này — sẽ dần convert sang FSM
trong PR sau.

## Reference implementations

- `Assets/_Project/Scripts/Mobs/WolfAI.cs` + `States/WolfStates.cs` — **FSM exemplar** (chase + melee, R7)
- `Assets/_Project/Scripts/Mobs/RabbitAI.cs` + `States/RabbitStates.cs` — **FSM exemplar** (wander + flee, R7)
- `Assets/_Project/Scripts/Mobs/FoxSpiritAI.cs` — night-only gating qua `TimeManager.isNight` (legacy Update)
- `Assets/_Project/Scripts/Mobs/BossMobAI.cs` — đa pha, projectile pattern, summon (legacy Update)

## Pitfalls

- **Trước R2** mob phải có fallback `GetComponent<PlayerStats>()` vì PlayerStats không implement IDamageable — PR #24 từng fix bug "mob đánh không khí". Sau R2 đã hết: mọi target damageable (mob/player/resource) đều qua một interface.
- **`playerMask` mặc định `0`** → `Physics2D.OverlapCircle` không match gì. Phải set qua wizard hoặc
  inspector về layer "Player".
- **`drops` null** → `MobBase.Die` `foreach (var d in drops)` NPE. Init `drops = Array.Empty<>()`.
- **Aggro on hit** đã handle trong `MobBase.TakeDamage` — không override trừ khi cần aggro chain.
