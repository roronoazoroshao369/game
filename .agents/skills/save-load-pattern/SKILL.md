# Skill: Save/Load round-trip pattern

Mục tiêu: thêm field mới vào save/load mà KHÔNG break legacy save.

## Architecture

- `SaveSystem` (Scripts/Core/) — JSON serialize/deserialize, file I/O.
- `SaveLoadController` — orchestrate snapshot/restore mỗi system: `Capture`, `Apply`,
  `RestoreInventory`, `RestoreRealm`, `RestoreTime`...
- File path: `Application.persistentDataPath/save.json`.

## Khi thêm field mới (vd: thêm `BodyTemp` snapshot)

1. Add field `float bodyTemp;` vào `SaveData` struct (Scripts/Core/SaveData.cs).
2. `SaveLoadController.Capture()`: `data.bodyTemp = playerStats.BodyTemp;`.
3. `SaveLoadController.Apply(data)`: `playerStats.BodyTemp = data.bodyTemp;`.
4. **Default value** cho legacy save: nếu JSON không có field, deserialize sẽ default 0/null.
   Field mới phải **default-safe** (0 BodyTemp = sentinel "load default" → restore từ
   `playerStats.maxBodyTemp / 2` hoặc tương đương). Test này quan trọng.
5. EditMode test:
   ```csharp
   [Test]
   public void Apply_LegacySaveWithoutBodyTemp_RestoresDefault()
   {
       var legacy = new SaveData { /* không set bodyTemp */ };
       saveController.Apply(legacy);
       Assert.AreEqual(playerStats.maxBodyTemp / 2, playerStats.BodyTemp, 0.01f);
   }
   ```

## Inventory restore — invariant

⚠️ `RestoreInventory` là production bug-prone area. Đã fix 1 bug trong PR #23 (slot scan-from-zero
ghi đè freshness/durability của 2+ stack cùng item).

**Pattern đúng:**
```csharp
foreach (var slotData in data.inventorySlots)
{
    var item = itemDb.Find(slotData.itemId);
    if (item == null) continue;

    int countBefore = inv.CountSlotsOf(item); // SNAPSHOT trước Add
    inv.Add(item, slotData.count);

    // Slot mới được Add nằm ở index countBefore (0-indexed) của các slot cùng item
    int newSlotIndex = inv.FindNthSlotOf(item, countBefore);
    if (newSlotIndex >= 0)
    {
        var s = inv.Slots[newSlotIndex];
        s.freshness = slotData.freshness;
        s.durability = slotData.durability;
        inv.Slots[newSlotIndex] = s;
    }
}
```

**KHÔNG:**
```csharp
// SAI: scan-from-zero ghi đè slot 0 mỗi lần
int slotIdx = inv.Slots.IndexOf(s => s.item == item);
inv.Slots[slotIdx].freshness = slotData.freshness; // bug khi 2+ stack
```

## Pitfalls

- **`ItemDatabase.Find(id)` null** khi save có item ID đã bị xóa khỏi DB → skip slot, log warn.
- **Realm tier mismatch**: nếu save có `realmTier = 99` nhưng `RealmSystem.tiers.Length = 12` → clamp.
- **`TimeManager.currentTime01` ngoài [0,1]**: clamp khi restore.
- **`Library/save.json` test**: dùng `Application.temporaryCachePath` thay vì `persistentDataPath`
  để tránh nhiễm save thật của developer.

## Reference

- `Assets/_Project/Scripts/Core/SaveSystem.cs`
- `Assets/_Project/Scripts/Core/SaveLoadController.cs`
- `Assets/_Project/Tests/EditMode/SaveLoadControllerTests.cs`
- `Assets/_Project/Tests/EditMode/SaveSystemTests.cs`
- PR #21 (initial test) + PR #23 (RestoreInventory fix)
