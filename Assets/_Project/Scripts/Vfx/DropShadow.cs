using UnityEngine;

namespace WildernessCultivation.Vfx
{
    /// <summary>
    /// Auto-spawn 1 child <see cref="SpriteRenderer"/> hình ellipse đen (drop shadow)
    /// đặt dưới chân entity. Cho cảm giác "grounded" giống Don't Starve / Hyper Light
    /// Drifter — hot detail visual, rất rẻ trên mobile (chỉ +1 sprite/entity).
    ///
    /// Lifecycle: <c>Awake</c> tạo child một lần (idempotent — re-Awake không nhân đôi).
    /// Không tick mỗi frame; shadow đi theo parent transform tự động qua hierarchy.
    /// </summary>
    public class DropShadow : MonoBehaviour
    {
        [Header("Refs")]
        [Tooltip("Sprite ellipse đen (BootstrapWizard tạo 'shadow' sprite). Nếu null, component skip.")]
        public Sprite shadowSprite;

        [Header("Visual")]
        [Tooltip("Offset local từ entity. Mặc định -0.35 trục Y → đặt dưới chân.")]
        public Vector2 localOffset = new Vector2(0f, -0.35f);
        [Tooltip("Scale của ellipse. X dài hơn Y → trông như đổ xuống mặt đất.")]
        public Vector2 localScale = new Vector2(0.85f, 0.35f);
        [Tooltip("Màu shadow — alpha thấp để mờ vừa phải.")]
        public Color shadowColor = new Color(0f, 0f, 0f, 0.45f);
        [Tooltip("Sorting order tương đối parent — phải âm để nằm dưới sprite chính.")]
        public int sortingOrderOffset = -1;
        [Tooltip("Tên child GameObject (idempotent: nếu đã tồn tại thì re-use).")]
        public string childName = "DropShadow";
        [Tooltip("Counter-rotate child mỗi LateUpdate để shadow luôn flat trên đất " +
            "kể cả khi parent xoay (vd: WindSway). Tắt nếu chắc chắn parent không xoay.")]
        public bool keepFlat = true;
        [Tooltip("Mức shadow phản ứng với mob bob (foot-impact feel). 0 = tắt. " +
            "0.3 = khi mob squash xuống, shadow giãn thêm 30% bob amount. Caller (MobAnimController) drive qua SetBobPhase.")]
        [Range(0f, 1f)] public float bobInfluence = 0.3f;

        SpriteRenderer cachedRenderer;
        Quaternion hostBaseRotation = Quaternion.identity;
        float currentBobFactor = 1f;

        public SpriteRenderer Renderer => cachedRenderer;

        void Awake()
        {
            hostBaseRotation = transform.localRotation;
            EnsureChild();
        }

        void LateUpdate()
        {
            if (cachedRenderer == null) return;

            if (keepFlat)
            {
                // Counter-rotate: shadow giữ rotation = hostBaseRotation trong không gian
                // parent. Nếu parent xoay (WindSway) thì shadow tự bù lại. Khi parent
                // không xoay, delta = identity → assign no-op rẻ (1 quat/frame).
                cachedRenderer.transform.localRotation =
                    Quaternion.Inverse(transform.localRotation) * hostBaseRotation;
            }

            if (bobInfluence > 0f)
            {
                // Foot-impact: khi mob squash (bob < 1) shadow giãn; khi extend (bob > 1) shadow co.
                float scaleMul = ComputeShadowBobScale(currentBobFactor, bobInfluence);
                cachedRenderer.transform.localScale = new Vector3(
                    localScale.x * scaleMul,
                    localScale.y * scaleMul,
                    1f);
            }
        }

        /// <summary>
        /// Caller (e.g., <c>MobAnimController</c>) feeds current mob bob factor (1 = neutral, &lt;1 squash, &gt;1 extend).
        /// LateUpdate uses cached value to scale shadow.
        /// </summary>
        public void SetBobPhase(float bobYFactor)
        {
            currentBobFactor = bobYFactor;
        }

        /// <summary>
        /// Pure math (EditMode testable): shadow scale multiplier in response to mob bob.
        /// bobYFactor = 1 → 1 (no change). bobYFactor &lt; 1 → &gt; 1 (shadow expand). bobYFactor &gt; 1 → &lt; 1 (shadow shrink).
        /// influence = 0 → always 1. influence = 1 → full reciprocal feedback.
        /// </summary>
        public static float ComputeShadowBobScale(float bobYFactor, float influence)
        {
            float clampedInfluence = Mathf.Clamp01(influence);
            return 1f + (1f - bobYFactor) * clampedInfluence;
        }

        /// <summary>
        /// Tìm hoặc tạo child shadow. Idempotent — gọi nhiều lần vẫn 1 child.
        /// Public để Editor tools (BootstrapWizard) có thể trigger sớm khi build prefab.
        /// </summary>
        public void EnsureChild()
        {
            if (shadowSprite == null) return;

            // Re-use child cũ nếu Awake bị gọi lại sau Editor reload.
            var existing = transform.Find(childName);
            GameObject go;
            if (existing != null)
            {
                go = existing.gameObject;
            }
            else
            {
                go = new GameObject(childName);
                go.transform.SetParent(transform, false);
            }

            go.transform.localPosition = new Vector3(localOffset.x, localOffset.y, 0f);
            go.transform.localScale = new Vector3(localScale.x, localScale.y, 1f);
            go.transform.localRotation = Quaternion.identity;

            cachedRenderer = go.GetComponent<SpriteRenderer>();
            if (cachedRenderer == null) cachedRenderer = go.AddComponent<SpriteRenderer>();
            cachedRenderer.sprite = shadowSprite;
            cachedRenderer.color = shadowColor;

            // Sort dưới sprite chính. Lấy parent SR sortingLayer + order rồi trừ offset.
            var parentSR = GetComponent<SpriteRenderer>();
            if (parentSR != null)
            {
                cachedRenderer.sortingLayerID = parentSR.sortingLayerID;
                cachedRenderer.sortingOrder = parentSR.sortingOrder + sortingOrderOffset;
            }
            else
            {
                cachedRenderer.sortingOrder = sortingOrderOffset;
            }
        }
    }
}
