using UnityEngine;

namespace WildernessCultivation.Player.Stats
{
    /// <summary>
    /// Health subsystem: HP / maxHP + starvation & dehydration damage rates.
    /// Pure state + small helpers — không subscribe event, không gọi GameEvents.
    /// <see cref="PlayerStats"/> là orchestrator raise <c>OnStatsChanged</c> sau mỗi mutation.
    ///
    /// R1 phase 2 refactor: tách khỏi PlayerStats. Façade giữ <c>HP</c> / <c>maxHP</c> /
    /// <c>Heal</c> / <c>TakeDamageRaw</c> properties + methods để consumer + test cũ không break.
    /// </summary>
    public class HealthComponent : MonoBehaviour
    {
        [Header("Health")]
        public float HP = 100f;
        public float maxHP = 100f;

        [Header("Damage rates when starving / dehydrated (per second)")]
        public float starveDamagePerSec = 1.5f;
        public float dehydrateDamagePerSec = 2.5f;

        /// <summary>True khi HP <= 0.</summary>
        public bool IsDead => HP <= 0f;

        /// <summary>Hồi HP, clamp <= maxHP.</summary>
        public void Heal(float amount)
        {
            HP = Mathf.Min(maxHP, HP + amount);
        }

        /// <summary>Trừ HP thô (không đi qua shield / i-frame — callers xử lý).</summary>
        public void TakeRaw(float dmg)
        {
            if (IsDead) return;
            if (dmg > 0f) HP = Mathf.Max(0f, HP - dmg);
        }

        /// <summary>Tick starvation + dehydration damage. Gọi từ PlayerStats.Update.</summary>
        public void TickStarvation(float dt, bool hungry, bool dehydrated)
        {
            if (IsDead) return;
            if (hungry) HP = Mathf.Max(0f, HP - starveDamagePerSec * dt);
            if (dehydrated) HP = Mathf.Max(0f, HP - dehydrateDamagePerSec * dt);
        }
    }
}
