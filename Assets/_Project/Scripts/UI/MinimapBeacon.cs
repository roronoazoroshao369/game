using UnityEngine;

namespace WildernessCultivation.UI
{
    /// <summary>
    /// Beacon hiển thị trên minimap (và world). Tạo child SpriteRenderer flat color
    /// + scale lớn (default 2x) để minimap camera ortho thấy rõ dù sprite gốc nhỏ.
    ///
    /// Pattern: gắn trên world object cần highlight trên minimap (Tombstone, Linh Tuyền,
    /// Linh Quả NPC). Không phụ thuộc texture asset — generate 1×1 white texture
    /// runtime rồi tint qua SpriteRenderer.color.
    ///
    /// CRITICAL: code-spawn caller PHẢI gọi <see cref="Initialize"/> ngay sau
    /// <c>AddComponent&lt;MinimapBeacon&gt;()</c>. PlayMode AddComponent fire Awake
    /// đồng bộ — assignment field sau AddComponent quá muộn (child đã tạo
    /// với default). Inspector-configured beacon vẫn hoạt động vì Awake đọc
    /// field đã serialize.
    /// </summary>
    public class MinimapBeacon : MonoBehaviour
    {
        [Tooltip("Màu beacon. Set qua inspector hoặc Initialize().")]
        public Color beaconColor = Color.yellow;

        [Tooltip("Scale tương đối so với parent. Mặc định 2× để minimap thấy rõ.")]
        public float scale = 2f;

        [Tooltip("Sorting order cao hơn world sprite để không bị che.")]
        public int sortingOrder = 1000;

        [Tooltip("Tên child GameObject để dễ debug.")]
        public string childName = "MinimapBeacon";

        public SpriteRenderer Child { get; private set; }

        void Awake()
        {
            EnsureChild();
        }

        /// <summary>
        /// Code-spawn entry point: set props rồi spawn/update child trong 1 call atomic.
        /// Idempotent — nếu child đã tạo (e.g. Awake fired trước trong PlayMode),
        /// update props trên child hiện có.
        /// </summary>
        public void Initialize(Color color, float scaleValue, string name)
        {
            beaconColor = color;
            scale = scaleValue;
            childName = name;
            if (Child == null) EnsureChild();
            else ApplyToChild();
        }

        void EnsureChild()
        {
            if (Child != null) return;
            var go = new GameObject(childName);
            go.transform.SetParent(transform, worldPositionStays: false);
            go.transform.localPosition = Vector3.zero;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = MakeWhiteSprite();
            Child = sr;
            ApplyToChild();
        }

        void ApplyToChild()
        {
            if (Child == null) return;
            Child.gameObject.name = childName;
            Child.transform.localScale = new Vector3(scale, scale, 1f);
            Child.color = beaconColor;
            Child.sortingOrder = sortingOrder;
        }

        static Sprite cachedWhite;
        static Sprite MakeWhiteSprite()
        {
            if (cachedWhite != null) return cachedWhite;
            var tex = new Texture2D(2, 2);
            tex.SetPixels(new[] { Color.white, Color.white, Color.white, Color.white });
            tex.Apply();
            cachedWhite = Sprite.Create(tex, new Rect(0, 0, 2, 2), new Vector2(0.5f, 0.5f), 16f);
            return cachedWhite;
        }
    }
}
