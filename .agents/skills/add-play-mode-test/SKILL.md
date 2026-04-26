# Skill: Add PlayMode test

Mục tiêu: thêm test PlayMode cho behavior cần `Time.deltaTime`, Physics2D, hoặc coroutine.

## Khi dùng

- Mob AI (aggro detection qua `Physics2D.OverlapCircle`, attack tick)
- `TimeManager.Update` (currentTime01 advance, OnDayStart/OnNightStart events)
- `DodgeAction` coroutine (i-frames + cooldown)
- `Workbench` station detection qua trigger collider
- `CraftingSystem` cookTime defer

## Bước

1. Tạo file `Assets/_Project/Tests/PlayMode/<TargetSystem>Tests.cs`.
2. Namespace: `WildernessCultivation.Tests.PlayMode`.
3. `[UnityTest] IEnumerator TestName()` (KHÔNG `[Test] void`).
4. `yield return null` để wait 1 frame, `yield return new WaitForFixedUpdate()` cho physics, hoặc
   `yield return new WaitForSecondsRealtime(t)` (KHÔNG `WaitForSeconds` vì Time.timeScale có thể thay đổi).
5. SetUp tạo GameObject. TearDown `Object.Destroy(go)` (KHÔNG `DestroyImmediate` — yield-incompatible).

## Skeleton

```csharp
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using WildernessCultivation.Mobs;

namespace WildernessCultivation.Tests.PlayMode
{
    public class WolfAITests
    {
        GameObject playerGo;
        const int PlayerLayer = 0; // Default
        const int MobLayer = 8;    // User1 — KHÁC player layer để OverlapCircle không self-detect

        [SetUp]
        public void Setup()
        {
            playerGo = new GameObject("Player");
            playerGo.layer = PlayerLayer;
            playerGo.AddComponent<Rigidbody2D>();
            playerGo.AddComponent<CircleCollider2D>().radius = 0.3f;
        }

        [TearDown]
        public void Teardown()
        {
            if (playerGo != null) Object.Destroy(playerGo);
        }

        [UnityTest]
        public IEnumerator Wolf_AcquiresTarget_WhenPlayerInRange()
        {
            var wolfGo = new GameObject("Wolf");
            wolfGo.layer = MobLayer;
            wolfGo.AddComponent<Rigidbody2D>();
            wolfGo.AddComponent<CircleCollider2D>().radius = 0.3f;
            var wolf = wolfGo.AddComponent<WolfAI>();
            wolf.aggroRange = 4f;
            wolf.playerMask = 1 << PlayerLayer;

            playerGo.transform.position = new Vector3(2f, 0, 0);
            yield return new WaitForFixedUpdate();
            yield return null;

            Assert.IsNotNull(wolf.target);
            Object.Destroy(wolfGo);
        }
    }
}
```

## Asmdef

`Assets/_Project/Tests/PlayMode/WildernessCultivation.Tests.PlayMode.asmdef`:
- `references`: `WildernessCultivation`
- `includePlatforms`: `[]`
- `defineConstraints`: `["UNITY_INCLUDE_TESTS"]`

## Verify

- Local: Unity Editor → `Test Runner` → tab `PlayMode` → Run All. Chậm hơn EditMode (~10s setup).
- CI: cùng workflow `Run EditMode + PlayMode tests`.

## Pitfalls

- **Layer setup.** `Physics2D.OverlapCircle(_, _, mask)` với `mask = ~0` sẽ self-detect collider của
  query origin. Player + mob phải khác layer; mask chỉ chứa target layer.
- **Frame timing.** `WaitForSecondsRealtime(t)` tròn lên frame boundary (~16.7ms @ 60fps). Đừng assert
  exact `currentTime01` value sau wait — chỉ assert state change đã xảy ra.
- **`dayLengthSeconds`** quá nhỏ (vd 0.3) khiến frame jump >0.05/frame → trong cross-boundary test có
  thể skip past dawn/dusk trước khi `wasNight` init đúng. Bump >= 1.0 cho test cross-boundary.
- **`PlayerStats` không implement `IDamageable`.** Khi test mob attack player, expect mob có fallback
  `target.GetComponent<PlayerStats>()?.TakeDamage(damage)` (`BossMobAI`/`WolfAI`/`FoxSpiritAI` đều có).
- **Coroutine vs OnDisable.** Khi disable component giữa coroutine, `OnDisable` phải `StopAllCoroutines`
  + reset state (xem `DodgeAction.OnDisable`). Test verify cleanup.
