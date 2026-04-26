using System;
using UnityEngine;

namespace WildernessCultivation.Combat
{
    /// <summary>
    /// Hub event tĩnh cho mọi sự kiện damage trong scene. Subscriber (camera shake,
    /// damage number spawner, future combo counter…) dùng <see cref="OnDamageDealt"/> để
    /// phản ứng mà không cần ref trực tiếp tới PlayerStats / MobBase.
    ///
    /// Static event → caller phải nhớ unsubscribe trong OnDisable / OnDestroy nếu không
    /// muốn leak (subscriber sẽ vẫn nhận event khi đã destroy GameObject ⇒ NRE).
    /// </summary>
    public static class CombatEvents
    {
        /// <summary>(worldPos, amount, isCrit) — amount luôn >= 0.</summary>
        public static event Action<Vector3, float, bool> OnDamageDealt;

        public static void RaiseDamage(Vector3 worldPos, float amount, bool isCrit = false)
        {
            if (amount <= 0f) return;
            OnDamageDealt?.Invoke(worldPos, amount, isCrit);
        }

        /// <summary>Reset toàn bộ subscriber. Dùng cho EditMode test setup/teardown.</summary>
        public static void ClearAllSubscribers() => OnDamageDealt = null;
    }
}
