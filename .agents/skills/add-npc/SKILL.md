# Skill: Add NPC humanoid (R5 follow-up)

Mục tiêu: thêm 1 NPC humanoid mới (vendor / companion / quest giver / villager)
reuse pure stat component từ R1 + `CharacterBase` polymorphic view từ R5.

Exemplar: [`VendorNPC.cs`](../../../Assets/_Project/Scripts/World/VendorNPC.cs).

## Pattern chung

NPC humanoid = `CharacterBase` subclass + composition các component cần thiết:

| Loại NPC | Component cần | Đặc điểm |
|---|---|---|
| Vendor | `HealthComponent` + `InvulnerabilityComponent` (default on) | Immortal, barter trade |
| Companion | `HealthComponent` + `HungerComponent` + `InvulnerabilityComponent` + `StateMachine<T>` | AI follow + combat assist, share survival decay |
| Quest giver | `HealthComponent` + `InvulnerabilityComponent` | Immortal, dialog tree |
| Villager (hostile/neutral) | `HealthComponent` + (opt) `HungerComponent` + `StateMachine<T>` | AI wander + flee on danger |

KHÔNG kéo `PlayerStats` façade (kéo theo 10 component — vendor không cần Wetness/Thermal/Sanity).

## Bước thực hiện

1. **Định danh NPC**
   - Chọn namespace: đa số `WildernessCultivation.World` (vendor / quest giver) hoặc
     `WildernessCultivation.Mobs` (nếu AI-driven như mob).
   - Stable `npcId` string cho save lookup.

2. **Class skeleton**
   ```csharp
   public class MyNPC : CharacterBase, IInteractable, ISaveable
   {
       public string npcId = "npc_generic";
       public float maxHP = 100f;
       public bool invulnerable = true;

       HealthComponent health;
       InvulnerabilityComponent invuln;

       public override float CurrentHP => health.HP;
       public override float CurrentMaxHP => health.maxHP;
       public override bool IsDead => health.IsDead;

       public string InteractLabel => "…";
       public bool CanInteract(GameObject actor) => actor != null && !IsDead;

       public string SaveKey => $"World/NPC/{npcId}";
       public int Order => 70;

       void Awake()
       {
           health = gameObject.GetComponent<HealthComponent>() ?? gameObject.AddComponent<HealthComponent>();
           invuln = gameObject.GetComponent<InvulnerabilityComponent>() ?? gameObject.AddComponent<InvulnerabilityComponent>();
           health.maxHP = maxHP;
           health.HP = maxHP;
           if (invulnerable) invuln.InvulnerableUntil = float.MaxValue;
       }

       void OnEnable()  { SaveRegistry.RegisterSaveable(this); }
       void OnDisable() { SaveRegistry.UnregisterSaveable(this); }

       public override void TakeDamage(float amount, GameObject source)
       {
           if (health.IsDead || invuln.IsInvulnerable) return;
           health.TakeRaw(amount);
       }

       public bool Interact(GameObject actor) { /* raise event hub, open UI */ }

       public void CaptureState(SaveData data) { /* save slice */ }
       public void RestoreState(SaveData data) { /* restore slice */ }
   }
   ```

3. **Save schema** (nếu cần persist state):
   - Add `[Serializable] public class MyNPCSaveData` vào `Scripts/Core/SaveSystem.cs`.
   - Add `public List<MyNPCSaveData> myNPCs = new();` vào `SaveData` class (field mới
     không break backward compat — JsonUtility default empty list cho save cũ).
   - Capture: match by `npcId`, add new entry or update existing.
   - Restore: early-return nếu `data.myNPCs == null` (save cũ).

4. **GameEvents hook**:
   - Add 1-2 event vào `Scripts/Core/GameEvents.cs` theo pattern exemplar (vd
     `OnVendorOpened`, `OnTradeCompleted`). Dùng `object` param để tránh circular
     namespace reference (subscriber cast concrete).
   - Remember to reset trong `ClearAllSubscribers()`.

5. **AI (nếu AI-driven)**: dùng `StateMachine<T>` theo pattern R7. Xem `add-mob/` skill + `WolfStates.cs`.

6. **EditMode test** (~10-15 case):
   - Composition auto-add components
   - CharacterBase view reflect subsystem state
   - IInteractable contract (CanInteract + Interact fire event)
   - TakeDamage with/without invulnerable
   - Feature-specific API atomic behavior (trade, dialog, quest)
   - ISaveable round-trip
   - SaveRegistry auto-register OnEnable
   - Event hub fire khi action phù hợp
   - Skill exemplar: [`VendorNPCTests.cs`](../../../Assets/_Project/Tests/EditMode/VendorNPCTests.cs)

7. **Prefab**: đặt vào `Assets/_Project/Prefabs/NPCs/` (nếu chưa có folder, tạo mới).
   Bootstrap: update `BootstrapWizard` nếu NPC cần xuất hiện trong demo scene.

## Pitfalls

- **Circular reference**: KHÔNG import `WildernessCultivation.World` từ `Scripts/Core/GameEvents.cs`
  (Core là module gốc). Dùng `object` param cho event, subscriber tự cast.
- **RequireComponent không trigger trong EditMode**: test phải `TestHelpers.Boot(npc)` để
  Awake chạy auto-add. Nếu forget → `CurrentHP == 0`, NRE trên component access.
- **SaveRegistry duplicate**: nếu NPC cycle disable/enable (pool), OnEnable register 2 lần →
  SaveRegistry dedup bằng Contains — OK. Nhưng nếu dùng custom registry thì phải dedup tự.
- **Backward compat**: add field vào SaveData PHẢI optional + default empty. Tuyệt đối KHÔNG
  rename / xoá field cũ (DESIGN_PRINCIPLES rule 8).
- **Invulnerable eternal**: set `InvulnerableUntil = float.MaxValue` (không phải `Mathf.Infinity` —
  JsonUtility không serialize được Infinity sạch sẽ).

## Test checklist

- [ ] EditMode test đủ invariant (~10-15 case).
- [ ] `GameEvents.ClearAllSubscribers()` + `SaveRegistry.ClearAll()` trong SetUp/TearDown.
- [ ] Save round-trip verify stock/HP/state restore đúng.
- [ ] PlayMode nếu NPC có AI tick (`StateMachine<T>` + Physics2D).

## References

- `AGENTS.md` rule 10 — NPC humanoid composition pattern
- `ARCHITECTURE.md` §2.1 + §2.2 — Component composition + inheritance
- `DESIGN_PRINCIPLES.md` rule 1 + 6 — Composition + pure component
- `REFACTOR_HISTORY.md` R5 — CharacterBase introduction
- Skill exemplar: `VendorNPC.cs` + `VendorNPCTests.cs`
