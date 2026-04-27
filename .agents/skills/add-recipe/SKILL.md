# Skill: Add craft recipe

Mục tiêu: thêm 1 recipe mới (vd: smelt iron, brew potion) qua `RecipeSO` ScriptableObject.

## Bước

1. Đảm bảo `ItemSO` cho output + ingredients đã có trong `ItemDatabase`. Nếu chưa, tạo qua
   `BootstrapWizard` hoặc thủ công `Assets → Create → WildernessCultivation → Item`.
2. Tạo `RecipeSO` instance: `Assets → Create → WildernessCultivation → Recipe`.
3. Set fields:
   - `result`: `ItemSO` của output + `count`
   - `ingredients`: array `RecipeSO.Ingredient { ItemSO item, int count }`
   - `requiredStation`: `CraftStation.None / Workbench / Campfire / Furnace / AlchemyPot`
   - `cookTime`: 0 = instant; > 0 → defer add-output qua `WaitForSeconds` (xem `CraftingSystem`)
4. Add reference vào `CraftingSystem.recipes` array (Inspector của GameManager hoặc qua wizard).
5. Test EditMode hoặc PlayMode (cookTime > 0 cần PlayMode):
   ```csharp
   [Test]
   public void Craft_StickRope_ConsumesIngredients_AddsResult()
   {
       crafting.TryCraft(rope_recipe);
       Assert.AreEqual(1, inv.CountOf(rope_recipe.result.item));
       Assert.AreEqual(0, inv.CountOf(stick));
   }
   ```

## Pitfalls

- **`CraftingSystem.CountOf` SKIP slot `IsBroken==true`.** Recipe không thể consume món hỏng. Nếu test
  thấy "thiếu ingredient" trong khi đếm thấy đủ, check xem có món hỏng đè lên slot không.
- **`requiredStation`** phải khớp `CraftStationMarker.station` đặt cạnh player. Workbench prefab có
  `CraftStationMarker.station = Workbench`. Distance check qua `Physics2D.OverlapCircle`.
- **`cookTime > 0`**: `CraftingSystem.TryCraft` consume ingredient ngay nhưng add result sau wait.
  Nếu inventory đầy lúc cookTime kết thúc → result mất (warn log).
- **Không stack perishable/durable.** Recipe output có `perishable` hoặc `durable` → mỗi lần craft
  tạo slot mới (`maxStack = 1` effective). Test phải verify slot count, không chỉ `CountOf`.

## Reference

- `Assets/_Project/Scripts/Crafting/RecipeSO.cs`
- `Assets/_Project/Scripts/Crafting/CraftingSystem.cs`
- `Assets/_Project/Tests/PlayMode/CraftingSystemTests.cs`
