using System;
using WildernessCultivation.Core;
using System.Collections.Generic;
using UnityEngine;

namespace WildernessCultivation.Player.Status
{
    /// <summary>
    /// Quản lý hiệu ứng trạng thái lên Player. Gắn cùng GameObject với <see cref="PlayerStats"/>.
    /// API:
    ///  - <see cref="Apply(StatusEffectSO, float)"/>: áp 1 effect (override duration nếu &gt;0).
    ///  - <see cref="HasEffect"/>, <see cref="GetRemaining"/>, <see cref="Clear"/>.
    /// PlayerStats query <see cref="MoveSpeedMultiplier"/> + <see cref="IncomingDamageMultiplier"/>
    /// để apply liên tục; tick damage được xử lý bên trong manager.
    /// </summary>
    [RequireComponent(typeof(PlayerStats))]
    public class StatusEffectManager : MonoBehaviour
    {
        [Serializable]
        public class ActiveEffect
        {
            public StatusEffectSO effect;
            public float endsAt;
            public float nextTickAt;

            public bool IsExpired => endsAt > 0f && Time.time >= endsAt;
            public float RemainingSec => endsAt > 0f ? Mathf.Max(0f, endsAt - Time.time) : float.PositiveInfinity;
        }

        readonly List<ActiveEffect> active = new();
        public IReadOnlyList<ActiveEffect> Active => active;

        public event Action OnEffectsChanged;

        PlayerStats stats;

        void Awake()
        {
            stats = GetComponent<PlayerStats>();
            ServiceLocator.Register<StatusEffectManager>(this);
        }

        void OnDestroy() => ServiceLocator.Unregister<StatusEffectManager>(this);

        void Update()
        {
            if (stats == null || stats.IsDead) return;
            bool dirty = false;
            for (int i = active.Count - 1; i >= 0; i--)
            {
                var a = active[i];
                if (a.effect == null || a.IsExpired) { active.RemoveAt(i); dirty = true; continue; }

                if (Time.time >= a.nextTickAt)
                {
                    a.nextTickAt = Time.time + Mathf.Max(0.05f, a.effect.tickIntervalSec);
                    if (a.effect.hpDamagePerTick > 0f) stats.TakeDamageRaw(a.effect.hpDamagePerTick);
                    if (a.effect.sanityDamagePerTick > 0f) stats.Sanity = Mathf.Max(0f, stats.Sanity - a.effect.sanityDamagePerTick);
                    if (a.effect.hungerDamagePerTick > 0f) stats.Hunger = Mathf.Max(0f, stats.Hunger - a.effect.hungerDamagePerTick);
                    if (a.effect.thirstDamagePerTick > 0f) stats.Thirst = Mathf.Max(0f, stats.Thirst - a.effect.thirstDamagePerTick);
                    if (a.effect.manaDamagePerTick > 0f) stats.Mana = Mathf.Max(0f, stats.Mana - a.effect.manaDamagePerTick);
                }
            }
            if (dirty) OnEffectsChanged?.Invoke();
        }

        public void Apply(StatusEffectSO effect, float overrideDurationSec = -1f)
        {
            if (effect == null) return;
            float dur = overrideDurationSec > 0f ? overrideDurationSec : effect.defaultDurationSec;
            float ends = dur > 0f ? Time.time + dur : 0f;

            // Tìm effect cùng id để stack
            var existing = active.Find(a => a.effect != null && a.effect.effectId == effect.effectId);
            if (existing != null)
            {
                if (effect.extendDurationOnReapply)
                    existing.endsAt = (existing.endsAt > 0f ? existing.endsAt : Time.time) + dur;
                else
                    existing.endsAt = Mathf.Max(existing.endsAt, ends);
            }
            else
            {
                active.Add(new ActiveEffect
                {
                    effect = effect,
                    endsAt = ends,
                    nextTickAt = Time.time + Mathf.Max(0.05f, effect.tickIntervalSec)
                });
            }
            Debug.Log($"[Status] +{effect.displayName} ({(dur > 0f ? dur.ToString("0.0") + "s" : "perm")})");
            OnEffectsChanged?.Invoke();
        }

        public bool HasEffect(string effectId) => active.Exists(a => a.effect != null && a.effect.effectId == effectId);

        public float GetRemaining(string effectId)
        {
            var a = active.Find(x => x.effect != null && x.effect.effectId == effectId);
            return a != null ? a.RemainingSec : 0f;
        }

        public void Clear(string effectId)
        {
            int removed = active.RemoveAll(a => a.effect != null && a.effect.effectId == effectId);
            if (removed > 0) OnEffectsChanged?.Invoke();
        }

        public void ClearAll()
        {
            if (active.Count == 0) return;
            active.Clear();
            OnEffectsChanged?.Invoke();
        }

        /// <summary>Multiplier cộng dồn vào tốc độ di chuyển (tích các effect.moveSpeedMultiplier).</summary>
        public float MoveSpeedMultiplier
        {
            get
            {
                float m = 1f;
                foreach (var a in active)
                    if (a.effect != null) m *= Mathf.Max(0.05f, a.effect.moveSpeedMultiplier);
                return m;
            }
        }

        /// <summary>Multiplier vào damage incoming (Burn=1.2, Shield=0.7…).</summary>
        public float IncomingDamageMultiplier
        {
            get
            {
                float m = 1f;
                foreach (var a in active)
                    if (a.effect != null) m *= Mathf.Max(0f, a.effect.incomingDamageMultiplier);
                return m;
            }
        }
    }
}
