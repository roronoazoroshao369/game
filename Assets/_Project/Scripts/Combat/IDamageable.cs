using UnityEngine;

namespace WildernessCultivation.Combat
{
    /// <summary>Interface chung cho mọi thứ có thể bị tấn công (mob, player, resource node).</summary>
    public interface IDamageable
    {
        void TakeDamage(float amount, GameObject source);
    }
}
