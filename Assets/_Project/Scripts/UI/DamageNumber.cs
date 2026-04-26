using TMPro;
using UnityEngine;

namespace WildernessCultivation.UI
{
    /// <summary>
    /// Float-up TMP text. Spawn bằng <see cref="DamageNumberSpawner"/>, tự destroy sau
    /// <see cref="lifetime"/> giây. Dùng <c>Time.unscaledDeltaTime</c> để vẫn animate khi
    /// pause (Time.timeScale = 0).
    /// </summary>
    [RequireComponent(typeof(RectTransform), typeof(TMP_Text))]
    public class DamageNumber : MonoBehaviour
    {
        public float lifetime = 0.9f;
        public float floatUpSpeed = 60f;     // px/sec (UI space)
        public float fadeStartFraction = 0.5f; // sau t = lifetime*0.5, alpha bắt đầu giảm

        RectTransform rt;
        TMP_Text text;
        float spawnTime;
        Color baseColor;

        void Awake()
        {
            rt = GetComponent<RectTransform>();
            text = GetComponent<TMP_Text>();
        }

        public void Init(string content, Color color)
        {
            text.text = content;
            text.color = color;
            baseColor = color;
            spawnTime = Time.unscaledTime;
        }

        void Update()
        {
            float age = Time.unscaledTime - spawnTime;
            if (age >= lifetime) { Destroy(gameObject); return; }

            // Float lên.
            rt.anchoredPosition += new Vector2(0f, floatUpSpeed * Time.unscaledDeltaTime);

            // Fade tail half lifetime.
            float fadeStart = lifetime * fadeStartFraction;
            float a = age <= fadeStart ? 1f : 1f - (age - fadeStart) / Mathf.Max(0.0001f, lifetime - fadeStart);
            var c = baseColor;
            c.a = Mathf.Clamp01(a);
            text.color = c;
        }
    }
}
