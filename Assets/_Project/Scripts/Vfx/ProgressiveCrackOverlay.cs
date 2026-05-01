using UnityEngine;

namespace WildernessCultivation.Vfx
{
    /// <summary>
    /// Crack progression overlay cho rock-type ResourceNode. Caller gọi
    /// <see cref="SetHpRatio"/> sau mỗi TakeDamage; component map ratio → alpha
    /// child sprite "crack" overlay (sprite vô — trắng / nâu nhạt phủ lên parent).
    ///
    /// Curve: HP đầy (ratio=1) → alpha=0 (no crack visible). HP cạn (ratio=0) → alpha=maxAlpha.
    /// Tween bằng Lerp công thức <see cref="ComputeCrackAlpha"/> (pure static → EditMode test).
    ///
    /// Idempotent Awake — re-Awake không nhân đôi child overlay (PlayMode test friendly).
    /// </summary>
    public class ProgressiveCrackOverlay : MonoBehaviour
    {
        [Tooltip("Sprite overlay (crack texture). Nếu null → reuse parent SpriteRenderer.sprite (auto-tint dark).")]
        public Sprite crackSprite;

        [Tooltip("Tint cho overlay. Mặc định nâu xám (rock crack). Alpha sẽ bị override bởi curve.")]
        public Color crackTint = new Color(0.15f, 0.12f, 0.10f, 1f);

        [Tooltip("Alpha tối đa khi HP=0. 0.6 đủ nhìn rõ nhưng không che hết sprite gốc.")]
        [Range(0f, 1f)] public float maxAlpha = 0.6f;

        [Tooltip("HP ratio threshold để bắt đầu hiện crack. 1.0 = hiện ngay khi mất HP đầu tiên, " +
            "0.7 = chỉ hiện sau khi mất 30% HP. Mặc định 0.85 → hit đầu tiên thấy nhẹ.")]
        [Range(0.01f, 1f)] public float startThreshold = 0.85f;

        [Tooltip("Sorting offset so với parent — overlay nằm trên parent sprite.")]
        public int sortingOrderOffset = 1;

        [Tooltip("Tên child GameObject. Idempotent: nếu đã tồn tại thì re-use.")]
        public string childName = "CrackOverlay";

        SpriteRenderer overlayRenderer;
        SpriteRenderer parentRenderer;

        void Awake()
        {
            parentRenderer = GetComponent<SpriteRenderer>();
            EnsureChild();
            // Bắt đầu invisible — caller sẽ SetHpRatio(1) hoặc skip nếu HP đầy.
            ApplyAlpha(0f);
        }

        void EnsureChild()
        {
            // Idempotent: tìm existing child trước, tạo mới nếu chưa có.
            Transform t = transform.Find(childName);
            GameObject go;
            if (t != null) { go = t.gameObject; }
            else
            {
                go = new GameObject(childName);
                go.transform.SetParent(transform, false);
            }

            overlayRenderer = go.GetComponent<SpriteRenderer>();
            if (overlayRenderer == null) overlayRenderer = go.AddComponent<SpriteRenderer>();

            Sprite chosenSprite = crackSprite != null ? crackSprite
                : (parentRenderer != null ? parentRenderer.sprite : null);
            overlayRenderer.sprite = chosenSprite;
            if (parentRenderer != null) overlayRenderer.sortingOrder = parentRenderer.sortingOrder + sortingOrderOffset;
        }

        /// <summary>
        /// Caller (ResourceNode.TakeDamage) gọi sau khi update HP. Ratio = currentHP/maxHP ∈ [0,1].
        /// </summary>
        public void SetHpRatio(float hpRatio)
        {
            if (overlayRenderer == null) return;
            float a = ComputeCrackAlpha(hpRatio, startThreshold, maxAlpha);
            ApplyAlpha(a);
        }

        void ApplyAlpha(float alpha)
        {
            if (overlayRenderer == null) return;
            Color c = crackTint;
            c.a = alpha;
            overlayRenderer.color = c;
        }

        /// <summary>
        /// Pure: ratio>=startThreshold → 0; ratio<=0 → maxAlpha; ngược lại lerp.
        /// Tham số: hpRatio HP hiện tại / max (clamp 0..1).
        /// </summary>
        public static float ComputeCrackAlpha(float hpRatio, float startThreshold, float maxAlpha)
        {
            hpRatio = Mathf.Clamp01(hpRatio);
            startThreshold = Mathf.Clamp(startThreshold, 0.01f, 1f);
            if (hpRatio >= startThreshold) return 0f;
            // hpRatio ∈ [0, startThreshold) → linear scale → [maxAlpha, 0).
            float t = 1f - (hpRatio / startThreshold); // 0 at threshold, 1 at zero HP.
            return Mathf.Clamp01(t) * Mathf.Clamp01(maxAlpha);
        }
    }
}
