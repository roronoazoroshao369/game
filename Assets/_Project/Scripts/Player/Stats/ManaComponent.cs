using UnityEngine;

namespace WildernessCultivation.Player.Stats
{
    /// <summary>
    /// Mana (Linh Khí) subsystem: gauge [0..maxMana] + idle regen + TryConsume / Add.
    /// R1 phase 2 refactor. <see cref="PlayerStats"/> façade giữ <c>Mana</c> / <c>maxMana</c>
    /// / <c>manaRegenIdle</c> / <c>TryConsumeMana</c> / <c>AddMana</c>.
    /// </summary>
    public class ManaComponent : MonoBehaviour
    {
        [Header("Mana (Linh Khí)")]
        public float Mana = 50f;
        public float maxMana = 50f;
        [Tooltip("Hồi mana mỗi giây khi không thiền.")]
        public float manaRegenIdle = 0.5f;

        /// <summary>Trừ mana nếu đủ — return false nếu không đủ (caller không đổi state).</summary>
        public bool TryConsume(float cost)
        {
            if (Mana < cost) return false;
            Mana -= cost;
            return true;
        }

        /// <summary>Cộng mana, clamp <= maxMana.</summary>
        public void Add(float amount)
        {
            Mana = Mathf.Min(maxMana, Mana + amount);
        }

        /// <summary>Idle regen — gọi từ PlayerStats.Update.</summary>
        public void TickRegen(float dt)
        {
            Mana = Mathf.Min(maxMana, Mana + manaRegenIdle * dt);
        }
    }
}
