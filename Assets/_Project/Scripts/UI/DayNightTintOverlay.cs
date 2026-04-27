using UnityEngine;
using UnityEngine.UI;
using WildernessCultivation.Core;

namespace WildernessCultivation.UI
{
    /// <summary>
    /// Full-screen UI Image làm color tint overlay theo TimeManager. Khác với
    /// <see cref="Light2DProxy"/> (overlay world-space cho global lighting) — component
    /// này hoạt động ở canvas screen-space để tạo cảm giác "bầu trời/không khí" theo giờ
    /// trong ngày: bình minh hồng cam, trưa trong, hoàng hôn cam đậm, đêm xanh navy.
    ///
    /// Đọc <see cref="TimeManager.currentTime01"/> mỗi frame và lerp giữa 4 keypoint
    /// dawn / noon / dusk / midnight. Alpha luôn ≤ <see cref="maxAlpha"/> để không che
    /// gameplay.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class DayNightTintOverlay : MonoBehaviour
    {
        [Header("Refs")]
        public TimeManager timeManager;

        [Header("Tint keypoints (cycle: midnight=0, dawn=0.25, noon=0.5, dusk=0.75)")]
        [Tooltip("Tint giữa khuya (currentTime01 = 0 hoặc 1).")]
        public Color midnightTint = new Color(0.05f, 0.10f, 0.30f, 0.55f);
        [Tooltip("Tint bình minh (currentTime01 ≈ 0.25).")]
        public Color dawnTint = new Color(1.00f, 0.55f, 0.40f, 0.30f);
        [Tooltip("Tint giữa trưa (currentTime01 = 0.5) — thường alpha ≈ 0.")]
        public Color noonTint = new Color(1.00f, 0.95f, 0.80f, 0.00f);
        [Tooltip("Tint hoàng hôn (currentTime01 ≈ 0.75).")]
        public Color duskTint = new Color(1.00f, 0.50f, 0.25f, 0.35f);

        [Header("Limits")]
        [Range(0f, 1f)]
        [Tooltip("Cap alpha cuối cùng — không cho overlay che gameplay quá tay.")]
        public float maxAlpha = 0.55f;

        Image img;

        void Awake()
        {
            img = GetComponent<Image>();
            img.raycastTarget = false; // overlay không chặn click UI bên dưới
            if (timeManager == null) timeManager = FindObjectOfType<TimeManager>();
        }

        void Update()
        {
            if (timeManager == null)
            {
                timeManager = FindObjectOfType<TimeManager>();
                if (timeManager == null) return;
            }
            img.color = SampleTint(timeManager.currentTime01);
        }

        /// <summary>
        /// Sample 4-keypoint cyclic gradient. Public + static để test deterministically.
        /// Quy ước: 0=midnight, 0.25=dawn, 0.5=noon, 0.75=dusk, 1=midnight (wrap).
        /// </summary>
        public Color SampleTint(float t)
        {
            t = Mathf.Repeat(t, 1f);
            Color c;
            if (t < 0.25f) c = Color.Lerp(midnightTint, dawnTint, t / 0.25f);
            else if (t < 0.5f) c = Color.Lerp(dawnTint, noonTint, (t - 0.25f) / 0.25f);
            else if (t < 0.75f) c = Color.Lerp(noonTint, duskTint, (t - 0.5f) / 0.25f);
            else c = Color.Lerp(duskTint, midnightTint, (t - 0.75f) / 0.25f);
            c.a = Mathf.Min(c.a, maxAlpha);
            return c;
        }
    }
}
