using UnityEngine;

namespace WildernessCultivation.Cultivation
{
    /// <summary>
    /// Tuning cho khai mở tu tiên. Tạo asset:
    /// Right-click > Create > WildernessCultivation > Awakening Config.
    ///
    /// Mặc định (Q4 confirmed): Phàm 50% / Tạp 35% / Đơn 13% / Thiên 2%.
    /// Min day = 7. Min HP fraction = 0.5. Min sanity = 50.
    /// </summary>
    [CreateAssetMenu(menuName = "WildernessCultivation/Awakening Config", fileName = "AwakeningConfig")]
    public class AwakeningConfigSO : ScriptableObject
    {
        [Header("Eligibility (điều kiện cần)")]
        [Tooltip("Số ngày tối thiểu phải sống sót.")]
        public int minDaysSurvived = 7;
        [Range(0f, 1f)]
        [Tooltip("Tỉ lệ HP/maxHP tối thiểu để khai mở (0.5 = 50%).")]
        public float minHpFraction = 0.5f;
        [Tooltip("Sanity tối thiểu (đầu óc tỉnh táo mới ngộ được).")]
        public float minSanity = 50f;

        [Header("Roll outcome — TỔNG phải = 1.0")]
        [Range(0f, 1f)] public float failChance = 0.50f;
        [Range(0f, 1f)] public float tapChance = 0.35f;
        [Range(0f, 1f)] public float donChance = 0.13f;
        [Range(0f, 1f)] public float thienChance = 0.02f;

        [Header("Pool linh căn để roll khi success")]
        [Tooltip("Pool Tạp linh căn (no-element / multi-element). Roll ngẫu nhiên 1 entry.")]
        public SpiritRootSO[] tapRoots;
        [Tooltip("Pool Đơn linh căn (Kim/Mộc/Thuỷ/Hoả/Thổ). Roll ngẫu nhiên 1 entry.")]
        public SpiritRootSO[] donRoots;
        [Tooltip("Pool Thiên linh căn — siêu hiếm.")]
        public SpiritRootSO[] thienRoots;

        /// <summary>Default values cho test / fallback khi user chưa tạo asset.</summary>
        public static AwakeningConfigSO CreateDefault()
        {
            var so = ScriptableObject.CreateInstance<AwakeningConfigSO>();
            so.name = "AwakeningConfig_Default";
            so.minDaysSurvived = 7;
            so.minHpFraction = 0.5f;
            so.minSanity = 50f;
            so.failChance = 0.50f;
            so.tapChance = 0.35f;
            so.donChance = 0.13f;
            so.thienChance = 0.02f;
            return so;
        }
    }
}
