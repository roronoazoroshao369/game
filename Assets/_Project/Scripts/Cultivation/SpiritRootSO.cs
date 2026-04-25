using UnityEngine;

namespace WildernessCultivation.Cultivation
{
    public enum SpiritElement { None, Kim, Moc, Thuy, Hoa, Tho }

    public enum SpiritRootGrade
    {
        /// <summary>Tạp linh căn — 2+ ngũ hành, không bonus, đột phá XP cost x1.2.</summary>
        Tap,
        /// <summary>Đơn linh căn — 1 ngũ hành thuần, full bonus.</summary>
        Don,
        /// <summary>Thiên linh căn — siêu hiếm, all bonus x0.5 nhưng xpGain x1.5.</summary>
        Thien
    }

    /// <summary>
    /// Linh căn của 1 player. Roll 1 lần khi tạo nhân vật (hoặc gán cố định cho NPC).
    /// PlayerStats / RealmSystem / PlayerCombat đọc các multiplier dưới đây để áp dụng.
    /// Tạo asset: Right-click > Create > WildernessCultivation > Spirit Root.
    /// </summary>
    [CreateAssetMenu(menuName = "WildernessCultivation/Spirit Root", fileName = "SpiritRoot_New")]
    public class SpiritRootSO : ScriptableObject
    {
        [Header("Identity")]
        public string displayName = "Hoả Linh Căn";
        public SpiritRootGrade grade = SpiritRootGrade.Don;
        public SpiritElement primaryElement = SpiritElement.Hoa;
        public Color tintColor = Color.white;
        public Sprite icon;

        [Header("Survival modifiers")]
        [Tooltip("Cộng vào freezeThreshold của PlayerStats (vd Hoả +20 => kháng nóng cao hơn nhưng dễ lạnh hơn? thường để + nếu cùng element).")]
        public float freezeThresholdDelta = 0f;
        [Tooltip("Cộng vào heatThreshold của PlayerStats (vd Hoả +20 => chịu nóng hơn).")]
        public float heatThresholdDelta = 0f;
        [Tooltip("Multiplier vào freezeDamagePerSec (vd Thuỷ 0.5 = chịu rét tốt).")]
        public float freezeDamageMultiplier = 1f;
        [Tooltip("Multiplier vào thirstDecay (vd Thuỷ 0.7 = ít khát).")]
        public float thirstDecayMultiplier = 1f;
        [Tooltip("Multiplier vào hungerDecay (vd Mộc 0.8 = ít đói nhờ thân mộc).")]
        public float hungerDecayMultiplier = 1f;
        [Tooltip("Multiplier vào sanityNightDecay (vd Mộc 0.7 = SAN khoẻ).")]
        public float sanityDecayMultiplier = 1f;
        [Tooltip("Multiplier maxHP (vd Thổ 1.2).")]
        public float maxHPMultiplier = 1f;
        [Tooltip("Multiplier maxCarryWeight (vd Thổ 1.5).")]
        public float carryWeightMultiplier = 1f;

        [Header("Combat modifiers")]
        [Tooltip("Multiplier vào weaponDamage (vd Kim 1.15).")]
        public float weaponDamageMultiplier = 1f;
        [Tooltip("Multiplier vào durabilityPerUse khi dùng tool/weapon (Kim 0.7 = ít hao).")]
        public float durabilityWearMultiplier = 1f;
        [Tooltip("Multiplier vào damage projectile/skill cùng element.")]
        public float sameElementDamageMultiplier = 1f;
        [Tooltip("Multiplier vào damage incoming khi bị hit bởi nguyên tố tương khắc.")]
        public float counterElementVulnerability = 1f;

        [Header("Cultivation modifiers")]
        [Tooltip("Multiplier vào XP nhận được từ thiền + giết quái.")]
        public float xpGainMultiplier = 1f;
        [Tooltip("Multiplier vào breakthrough XP cost (vd Tạp 1.2 = tốn nhiều XP hơn).")]
        public float breakthroughCostMultiplier = 1f;
        [Tooltip("Multiplier vào xpGain của technique CÙNG element (FireBall + Hoả = 2x).")]
        public float techniqueAffinityMultiplier = 1f;

        /// <summary>Default Tạp linh căn cho fallback khi chưa có asset.</summary>
        public static SpiritRootSO CreateDefault()
        {
            var so = ScriptableObject.CreateInstance<SpiritRootSO>();
            // Đặt name để save/load round-trip không bị Apply skip do IsNullOrEmpty check.
            so.name = "SpiritRoot_Default";
            so.displayName = "Tạp Linh Căn";
            so.grade = SpiritRootGrade.Tap;
            so.primaryElement = SpiritElement.None;
            so.breakthroughCostMultiplier = 1.2f;
            return so;
        }
    }
}
