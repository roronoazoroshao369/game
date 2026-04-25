using System;
using UnityEngine;
using WildernessCultivation.Core;
using WildernessCultivation.World;

namespace WildernessCultivation.Player
{
    /// <summary>
    /// 5 chỉ số sinh tồn cốt lõi: HP, Đói, Khát, SAN (tinh thần), Linh Khí (mana).
    /// Tự decay theo thời gian; HP chảy máu nếu đói/khát = 0.
    /// </summary>
    public class PlayerStats : MonoBehaviour
    {
        [Header("Max values")]
        public float maxHP = 100f;
        public float maxHunger = 100f;
        public float maxThirst = 100f;
        public float maxSanity = 100f;
        public float maxMana = 50f;

        [Header("Current values")]
        public float HP = 100f;
        public float Hunger = 100f;
        public float Thirst = 100f;
        public float Sanity = 100f;
        public float Mana = 50f;

        [Header("Decay (per second)")]
        public float hungerDecay = 0.25f;     // ~6.6 phút từ full về 0
        public float thirstDecay = 0.4f;      // ~4.1 phút (khát đến nhanh hơn)
        public float sanityNightDecay = 0.6f; // chỉ giảm khi đêm + ngoài trời + xa lửa
        public float manaRegenIdle = 0.5f;    // hồi mana chậm khi không thiền

        [Header("Damage rates when starving/dehydrated")]
        public float starveDamagePerSec = 1.5f;
        public float dehydrateDamagePerSec = 2.5f;

        public event Action OnDeath;
        public event Action OnStatsChanged;

        public bool IsDead => HP <= 0f;

        /// <summary>True nếu player đang trong aura của 1 <see cref="Campfire"/> đang cháy.</summary>
        public bool IsWarm => Campfire.FindWarmthAt(transform.position) != null;

        TimeManager timeManager;

        void Start()
        {
            timeManager = GameManager.Instance != null ? GameManager.Instance.timeManager : FindObjectOfType<TimeManager>();
        }

        void Update()
        {
            if (IsDead) return;
            float dt = Time.deltaTime;

            Hunger = Mathf.Max(0f, Hunger - hungerDecay * dt);
            Thirst = Mathf.Max(0f, Thirst - thirstDecay * dt);

            if (timeManager != null && timeManager.isNight && !IsWarm)
                Sanity = Mathf.Max(0f, Sanity - sanityNightDecay * dt);

            if (Hunger <= 0f) HP = Mathf.Max(0f, HP - starveDamagePerSec * dt);
            if (Thirst <= 0f) HP = Mathf.Max(0f, HP - dehydrateDamagePerSec * dt);

            Mana = Mathf.Min(maxMana, Mana + manaRegenIdle * dt);

            OnStatsChanged?.Invoke();

            if (HP <= 0f) Die();
        }

        public void TakeDamage(float dmg)
        {
            if (IsDead) return;
            HP = Mathf.Max(0f, HP - dmg);
            OnStatsChanged?.Invoke();
            if (HP <= 0f) Die();
        }

        public void Heal(float amount)
        {
            HP = Mathf.Min(maxHP, HP + amount);
            OnStatsChanged?.Invoke();
        }

        public void Eat(float foodValue)
        {
            Hunger = Mathf.Min(maxHunger, Hunger + foodValue);
            OnStatsChanged?.Invoke();
        }

        public void Drink(float waterValue)
        {
            Thirst = Mathf.Min(maxThirst, Thirst + waterValue);
            OnStatsChanged?.Invoke();
        }

        public void RestoreSanity(float amount)
        {
            Sanity = Mathf.Min(maxSanity, Sanity + amount);
            OnStatsChanged?.Invoke();
        }

        public bool TryConsumeMana(float cost)
        {
            if (Mana < cost) return false;
            Mana -= cost;
            OnStatsChanged?.Invoke();
            return true;
        }

        public void AddMana(float amount)
        {
            Mana = Mathf.Min(maxMana, Mana + amount);
            OnStatsChanged?.Invoke();
        }

        void Die()
        {
            OnDeath?.Invoke();
            Debug.Log("[Player] Died.");
        }
    }
}
