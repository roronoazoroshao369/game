# Skill: Add EditMode test

Mục tiêu: thêm test EditMode cho 1 pure logic system (không Physics2D, không coroutine).

## Khi dùng

System logic chỉ phụ thuộc state + math, không cần Unity render/physics tick. Ví dụ:
- `PlayerStats` (HP/Hunger/Thirst decay logic)
- `RealmSystem` (XP/breakthrough/tier transitions)
- `TimeManager` (pure functions: `isNight`, `GetLightIntensity`, `GetSpiritualEnergyMultiplier`)
- `Inventory` (Add/Remove/CountOf/TryConsume/Repair)
- `SaveLoadController.Apply` round-trip

## Bước

1. Tạo file `Assets/_Project/Tests/EditMode/<TargetSystem>Tests.cs`.
2. Namespace: `WildernessCultivation.Tests.EditMode`.
3. `[Test]` attribute trên method (KHÔNG `[UnityTest]`). Method trả `void`, không IEnumerator.
4. SetUp tạo GameObject + AddComponent. TearDown `Object.DestroyImmediate(go)`.
5. Assert dùng NUnit (`Assert.AreEqual`, `Assert.IsTrue`, `Assert.IsNull`, `Assert.Greater`…).

## Skeleton

```csharp
using NUnit.Framework;
using UnityEngine;
using WildernessCultivation.Player;

namespace WildernessCultivation.Tests.EditMode
{
    public class PlayerStatsTests
    {
        GameObject go;
        PlayerStats stats;

        [SetUp]
        public void Setup()
        {
            go = new GameObject("Player");
            stats = go.AddComponent<PlayerStats>();
            stats.HP = 100f;
        }

        [TearDown]
        public void Teardown()
        {
            if (go != null) Object.DestroyImmediate(go);
        }

        [Test]
        public void TakeDamage_ReducesHP()
        {
            stats.TakeDamage(30f);
            Assert.AreEqual(70f, stats.HP, 0.01f);
        }
    }
}
```

## Asmdef

Đã có sẵn `Assets/_Project/Tests/EditMode/WildernessCultivation.Tests.EditMode.asmdef`:
- `references`: `WildernessCultivation`
- `includePlatforms`: `["Editor"]`
- `defineConstraints`: `["UNITY_INCLUDE_TESTS"]`

KHÔNG tạo asmdef mới — chỉ thêm file `.cs` vào folder.

## Verify

- Local: mở Unity Editor → `Window → General → Test Runner` → tab `EditMode` → Run All.
- CI: push lên branch + tạo PR → workflow `Run EditMode + PlayMode tests` chạy.

## Pitfalls

- **Đừng dùng** `Time.time`, `Time.deltaTime` trong EditMode test — không tick. Nếu cần, dùng PlayMode.
- **Đừng dùng** Physics2D — không simulate trong EditMode mode.
- **Đừng dùng** coroutine — `[Test] void` không await được.
- ScriptableObject test data: `ScriptableObject.CreateInstance<ItemSO>()`, KHÔNG `new ItemSO()`.
  Nhớ `Object.DestroyImmediate(itemSo)` trong TearDown để tránh leak.
