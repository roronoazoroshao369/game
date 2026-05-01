using UnityEngine;

namespace WildernessCultivation.Vfx
{
    /// <summary>
    /// Phản ứng visual khi resource (tree / rock / flora) hoặc mob bị hit. Bundle 3
    /// hiệu ứng có thể bật/tắt độc lập:
    ///
    /// 1. <b>Flash</b>   — SpriteRenderer.color tween về flashColor rồi restore (frame
    ///    feedback "trúng").
    /// 2. <b>Shake</b>   — Damped sin rotation tween (cây rung khi đập). Add chồng
    ///    lên base rotation (compose với WindSway nếu có).
    /// 3. <b>Burst</b>   — Spawn N small sprite particle bay ra + rơi + fade (lá rơi
    ///    cho cây, bụi cho đá).
    ///
    /// Caller (ResourceNode.TakeDamage / MobBase.TakeDamage) gọi <see cref="Hit"/> mỗi lần
    /// damage. Component tự manage lifecycle bằng coroutine — KHÔNG alloc per frame
    /// (formula deterministic, particle GO Instantiate 1 lần/hit).
    ///
    /// Pure math <see cref="ComputeShakeAngleDeg"/> static → EditMode test.
    /// </summary>
    public class ReactiveOnHit : MonoBehaviour
    {
        [Header("Flash (SpriteRenderer color tween)")]
        public bool enableFlash = true;
        public Color flashColor = new Color(1f, 1f, 1f, 0.85f);
        [Tooltip("Thời gian flash (giây). 0.05-0.12 cho hit feedback.")]
        public float flashDuration = 0.08f;

        [Header("Shake (rotation tween, damped sin)")]
        public bool enableShake = true;
        [Tooltip("Biên độ rung max (độ). Cây ~6-10°, đá ~2-4°, cỏ ~12°.")]
        public float shakeAmplitudeDeg = 8f;
        [Tooltip("Tần số rung (Hz). Cây ~6, đá ~10 (cứng → rung nhanh), cỏ ~4.")]
        public float shakeFrequencyHz = 6f;
        [Tooltip("Decay rate. Lớn = rung tắt nhanh (8 → ~0.4s), nhỏ = chậm (3 → ~1s).")]
        public float shakeDecay = 8f;
        [Tooltip("Thời gian shake max (giây). Sau khoảng này coroutine dừng dù chưa decay xong.")]
        public float shakeDuration = 0.5f;

        [Header("Particle Burst (lá / bụi rơi)")]
        public bool enableBurst = true;
        [Tooltip("Prefab particle (sprite + LeafParticle component). Nếu null → fallback white square.")]
        public GameObject burstParticlePrefab;
        [Tooltip("Số particle / hit. 3-5 cho hit thường, 8-12 cho khi node die.")]
        [Range(0, 16)] public int burstCount = 4;
        [Tooltip("Tốc độ particle min/max (unit/sec).")]
        public float burstSpeedMin = 1.5f;
        public float burstSpeedMax = 3f;
        [Tooltip("Lifetime particle (giây).")]
        public float burstLifetime = 0.6f;
        [Tooltip("Color particle (tint nhân với sprite). Lá xanh, bụi đá xám.")]
        public Color burstColor = new Color(0.4f, 0.7f, 0.3f, 1f);

        // Single-renderer fast path (resource / single-sprite mob). Khi self không có
        // SpriteRenderer (puppet character: root không sprite, body parts ở children) →
        // fallback scan children + cache list, exclude DropShadow renderer.
        SpriteRenderer cachedRenderer;
        Color cachedOriginalColor;
        SpriteRenderer[] cachedChildRenderers;
        Color[] cachedChildOriginalColors;
        Coroutine flashRoutine;
        Coroutine shakeRoutine;

        void Awake()
        {
            cachedRenderer = GetComponent<SpriteRenderer>();
            if (cachedRenderer != null)
            {
                cachedOriginalColor = cachedRenderer.color;
                return;
            }
            // Puppet path — flash all child SpriteRenderers (head, torso, arms, legs, tail)
            // trừ DropShadow child (lookup qua component reference, không dựa vào tên).
            var ds = GetComponent<DropShadow>();
            var shadowSr = ds != null ? ds.Renderer : null;
            var all = GetComponentsInChildren<SpriteRenderer>(includeInactive: false);
            var list = new System.Collections.Generic.List<SpriteRenderer>(all.Length);
            foreach (var sr in all)
            {
                if (sr == null || sr == shadowSr) continue;
                list.Add(sr);
            }
            cachedChildRenderers = list.ToArray();
            cachedChildOriginalColors = new Color[cachedChildRenderers.Length];
            for (int i = 0; i < cachedChildRenderers.Length; i++)
                cachedChildOriginalColors[i] = cachedChildRenderers[i].color;
        }

        /// <summary>
        /// Trigger reactive feedback. Có thể gọi liên tục (mỗi swing) — coroutine cũ
        /// được restart, tránh stack flash conflict.
        /// </summary>
        public void Hit()
        {
            // Single OR puppet path đều tween color — guard chỉ skip khi cả 2 null
            // (component attach nhưng GO không có renderer nào → no-op flash).
            if (enableFlash && (cachedRenderer != null || (cachedChildRenderers != null && cachedChildRenderers.Length > 0)))
            {
                if (flashRoutine != null) StopCoroutine(flashRoutine);
                flashRoutine = StartCoroutine(FlashRoutine());
            }
            if (enableShake)
            {
                if (shakeRoutine != null) StopCoroutine(shakeRoutine);
                shakeRoutine = StartCoroutine(ShakeRoutine());
            }
            if (enableBurst && burstCount > 0)
            {
                SpawnBurst();
            }
        }

        System.Collections.IEnumerator FlashRoutine()
        {
            if (cachedRenderer != null) cachedRenderer.color = flashColor;
            if (cachedChildRenderers != null)
            {
                for (int i = 0; i < cachedChildRenderers.Length; i++)
                    if (cachedChildRenderers[i] != null) cachedChildRenderers[i].color = flashColor;
            }
            yield return new WaitForSeconds(flashDuration);
            if (cachedRenderer != null) cachedRenderer.color = cachedOriginalColor;
            if (cachedChildRenderers != null)
            {
                for (int i = 0; i < cachedChildRenderers.Length; i++)
                    if (cachedChildRenderers[i] != null) cachedChildRenderers[i].color = cachedChildOriginalColors[i];
            }
            flashRoutine = null;
        }

        System.Collections.IEnumerator ShakeRoutine()
        {
            // Compose: KHÔNG ghi đè baseRotation (WindSway còn đang Update mỗi frame).
            // Approach: track delta, add vào localRotation rồi remove ở frame next.
            // Simpler: store base ở entry, write base * shake mỗi frame, restore base
            // ở exit. WindSway sẽ ghi đè ngay frame sau, nên không conflict (shake ngắn
            // hơn 1s, dominant trong window).
            Quaternion baseRot = transform.localRotation;
            float t = 0f;
            while (t < shakeDuration)
            {
                float deg = ComputeShakeAngleDeg(t, shakeAmplitudeDeg, shakeFrequencyHz, shakeDecay);
                transform.localRotation = baseRot * Quaternion.Euler(0f, 0f, deg);
                t += Time.deltaTime;
                yield return null;
            }
            transform.localRotation = baseRot;
            shakeRoutine = null;
        }

        void SpawnBurst()
        {
            for (int i = 0; i < burstCount; i++)
            {
                GameObject p = burstParticlePrefab != null
                    ? Instantiate(burstParticlePrefab, transform.position, Quaternion.identity)
                    : CreateFallbackParticle();
                p.transform.position = transform.position;

                var lp = p.GetComponent<LeafParticle>();
                if (lp == null) lp = p.AddComponent<LeafParticle>();

                // Hướng radial: scatter đều quanh vòng tròn.
                float angle = (i / (float)burstCount) * Mathf.PI * 2f
                              + Random.Range(-0.3f, 0.3f);
                float speed = Random.Range(burstSpeedMin, burstSpeedMax);
                Vector2 vel = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle) + 0.5f) * speed;

                lp.Launch(vel, burstLifetime, burstColor);
            }
        }

        GameObject CreateFallbackParticle()
        {
            // Dùng cùng sprite của parent + scale 0.3× để tạo "mảnh vỡ". Tránh tạo
            // texture mới (tốn alloc); just reuse + tint. Puppet path: lấy sprite
            // của child đầu tiên (vd Torso) — có cùng visual feel.
            var p = new GameObject("BurstParticle");
            var sr = p.AddComponent<SpriteRenderer>();
            SpriteRenderer source = cachedRenderer;
            if (source == null && cachedChildRenderers != null && cachedChildRenderers.Length > 0)
                source = cachedChildRenderers[0];
            if (source != null)
            {
                sr.sprite = source.sprite;
                sr.sortingOrder = source.sortingOrder + 1;
            }
            p.transform.localScale = Vector3.one * 0.25f;
            return p;
        }

        /// <summary>
        /// Damped sin: <c>amp · sin(2π·freq·t) · exp(-decay·t)</c>. Pure → EditMode test.
        /// Tại t=0 → 0 (sin(0)=0). Tại t=large → ~0 (exp decay). Peak ~ 1/(4·freq).
        /// </summary>
        public static float ComputeShakeAngleDeg(float t, float amplitudeDeg,
            float frequencyHz, float decay)
        {
            if (t < 0f) t = 0f;
            return amplitudeDeg
                   * Mathf.Sin(2f * Mathf.PI * frequencyHz * t)
                   * Mathf.Exp(-decay * t);
        }
    }
}
