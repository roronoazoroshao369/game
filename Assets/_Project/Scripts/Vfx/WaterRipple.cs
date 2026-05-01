using UnityEngine;

namespace WildernessCultivation.Vfx
{
    /// <summary>
    /// Ripple ring expand khi player tới gần water (suối / linh tuyền).
    /// Update poll OverlapCircle theo <see cref="triggerRadius"/>; nếu player vào
    /// & cooldown hết → spawn 1 ring sprite scale từ <see cref="ringStartScale"/>
    /// → <see cref="ringEndScale"/>, alpha fade 1→0 theo <see cref="ringLifetime"/>.
    ///
    /// Pure <see cref="ComputeRingScale"/> + <see cref="ComputeRingAlpha"/> → EditMode test.
    /// Spawn cost: 1 GameObject + SpriteRenderer / ripple — auto-destroy sau lifetime.
    /// </summary>
    public class WaterRipple : MonoBehaviour
    {
        [Header("Trigger")]
        [Tooltip("Bán kính detect player (unit). 1.0 = player vào trong 1u của water mới ripple.")]
        public float triggerRadius = 1f;

        [Tooltip("Cooldown giữa các ripple (giây). Tránh spam khi player đứng yên cạnh nước.")]
        public float spawnCooldown = 1.2f;

        [Tooltip("Layer mask chứa player. Mặc định Everything (~0). Test PlayMode set sang playerMask thật.")]
        public LayerMask playerMask = ~0;

        [Header("Ring visual")]
        [Tooltip("Sprite ring (1 đường tròn rỗng). Nếu null → CreateFallback (1×1 white tinted).")]
        public Sprite ringSprite;
        public Color ringColor = new Color(0.7f, 0.9f, 1f, 0.6f);
        public float ringStartScale = 0.3f;
        public float ringEndScale = 1.6f;
        public float ringLifetime = 0.8f;

        [Tooltip("Sorting offset so với parent water — ring nằm trên water sprite.")]
        public int sortingOrderOffset = 1;

        float nextSpawnAt;
        SpriteRenderer cachedRenderer;

        void Awake() { cachedRenderer = GetComponent<SpriteRenderer>(); }

        void Update()
        {
            if (Time.time < nextSpawnAt) return;
            // Cheap polling — water spring count low (~5-15 / map), trigger only inside scene view.
            var hit = Physics2D.OverlapCircle(transform.position, triggerRadius, playerMask);
            if (hit == null) return;
            nextSpawnAt = Time.time + spawnCooldown;
            SpawnRing();
        }

        void SpawnRing()
        {
            var go = new GameObject("WaterRipple");
            go.transform.SetParent(null, true);
            go.transform.position = transform.position;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = ringSprite != null ? ringSprite
                : (cachedRenderer != null ? cachedRenderer.sprite : null);
            sr.color = ringColor;
            if (cachedRenderer != null) sr.sortingOrder = cachedRenderer.sortingOrder + sortingOrderOffset;

            var anim = go.AddComponent<WaterRippleAnim>();
            anim.startScale = ringStartScale;
            anim.endScale = ringEndScale;
            anim.lifetime = ringLifetime;
            anim.baseColor = ringColor;
        }

        /// <summary>
        /// Pure: scale lerp tuyến tính start→end theo t/lifetime ∈ [0,1].
        /// </summary>
        public static float ComputeRingScale(float t, float lifetime, float startScale, float endScale)
        {
            if (lifetime <= 0f) return endScale;
            float u = Mathf.Clamp01(t / lifetime);
            return Mathf.Lerp(startScale, endScale, u);
        }

        /// <summary>
        /// Pure: alpha curve — peak 1 ở giữa rồi tắt về 0. <c>1 - |2u - 1|</c> (triangle).
        /// </summary>
        public static float ComputeRingAlpha(float t, float lifetime)
        {
            if (lifetime <= 0f) return 0f;
            float u = Mathf.Clamp01(t / lifetime);
            return 1f - Mathf.Abs(2f * u - 1f);
        }
    }

    /// <summary>Per-ring tween component — driven by <see cref="WaterRipple"/> spawn.</summary>
    public class WaterRippleAnim : MonoBehaviour
    {
        public float startScale = 0.3f;
        public float endScale = 1.6f;
        public float lifetime = 0.8f;
        public Color baseColor = Color.white;

        float age;
        SpriteRenderer sr;

        void Awake() { sr = GetComponent<SpriteRenderer>(); }

        void Update()
        {
            age += Time.deltaTime;
            float scale = WaterRipple.ComputeRingScale(age, lifetime, startScale, endScale);
            transform.localScale = new Vector3(scale, scale, 1f);
            if (sr != null)
            {
                Color c = baseColor;
                c.a = baseColor.a * WaterRipple.ComputeRingAlpha(age, lifetime);
                sr.color = c;
            }
            if (age >= lifetime) Destroy(gameObject);
        }
    }
}
