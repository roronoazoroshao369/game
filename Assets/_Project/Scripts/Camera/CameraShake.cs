using UnityEngine;
using WildernessCultivation.Combat;
using WildernessCultivation.Cultivation;

namespace WildernessCultivation.CameraFx
{
    /// <summary>
    /// Camera shake juice. Đặt trên Main Camera. Mỗi lần được trigger, camera offset
    /// một lượng ngẫu nhiên trong bán kính giảm dần (linear decay) trong
    /// <c>duration</c> giây quanh vị trí gốc.
    ///
    /// Subscribe sẵn:
    ///   - <see cref="CombatEvents.OnDamageDealt"/>: shake nhẹ (intensity tỉ lệ amount).
    ///   - <see cref="RealmSystem.OnBreakthroughAttempted"/>: shake mạnh khi success.
    ///
    /// Camera được parent vào player (BootstrapWizard) → component lưu
    /// <c>baseLocalPos</c> snapshot trong Awake và áp offset trên đó mỗi LateUpdate. Khi
    /// shake hết, camera trở lại baseLocalPos chính xác.
    /// </summary>
    public class CameraShake : MonoBehaviour
    {
        [Header("Tuning chung")]
        [Tooltip("Bán kính shake tối đa (đơn vị world units, cùng hệ với camera ortho).")]
        public float maxAmplitude = 0.4f;
        [Tooltip("Trim factor cho damage shake — amount * factor → amplitude (clamp tới max).")]
        public float damageAmplitudeFactor = 0.02f;
        [Tooltip("Thời gian shake mặc định mỗi event damage.")]
        public float damageDuration = 0.18f;
        [Tooltip("Amplitude khi đột phá thành công.")]
        public float breakthroughAmplitude = 0.35f;
        [Tooltip("Thời gian shake khi đột phá thành công.")]
        public float breakthroughDuration = 0.6f;

        Vector3 baseLocalPos;
        float shakeAmplitude;
        float shakeEndsAt;
        float shakeStartedAt;

        public bool IsShaking => Time.unscaledTime < shakeEndsAt;

        void Awake()
        {
            baseLocalPos = transform.localPosition;
        }

        void OnEnable()
        {
            CombatEvents.OnDamageDealt += OnDamageDealt;
            // RealmSystem instance có thể không tồn tại lúc enable → subscribe trễ qua FindObjectOfType.
            TryHookRealm();
        }

        void OnDisable()
        {
            CombatEvents.OnDamageDealt -= OnDamageDealt;
            if (hookedRealm != null) hookedRealm.OnBreakthroughAttempted -= OnBreakthrough;
            hookedRealm = null;
        }

        RealmSystem hookedRealm;

        void TryHookRealm()
        {
            if (hookedRealm != null) return;
            hookedRealm = FindObjectOfType<RealmSystem>();
            if (hookedRealm != null) hookedRealm.OnBreakthroughAttempted += OnBreakthrough;
        }

        void Update()
        {
            // Nếu vào scene fresh chưa có RealmSystem (trước Player Awake), retry.
            if (hookedRealm == null) TryHookRealm();
        }

        void LateUpdate()
        {
            if (!IsShaking) { transform.localPosition = baseLocalPos; return; }
            float total = Mathf.Max(0.0001f, shakeEndsAt - shakeStartedAt);
            float remaining = (shakeEndsAt - Time.unscaledTime) / total; // 1→0 linear decay
            float amp = shakeAmplitude * Mathf.Clamp01(remaining);
            // Random offset trong vòng tròn → cảm giác chao đảo đa hướng.
            Vector2 r = Random.insideUnitCircle * amp;
            transform.localPosition = baseLocalPos + new Vector3(r.x, r.y, 0f);
        }

        void OnDamageDealt(Vector3 _, float amount, bool __)
        {
            float amp = Mathf.Min(maxAmplitude, amount * damageAmplitudeFactor);
            Trigger(amp, damageDuration);
        }

        void OnBreakthrough(bool success)
        {
            if (!success) return;
            Trigger(breakthroughAmplitude, breakthroughDuration);
        }

        /// <summary>Public API cho gameplay/script khác (boss spawn, earthquake event…).</summary>
        public void Trigger(float amplitude, float duration)
        {
            if (amplitude <= 0f || duration <= 0f) return;
            // Nếu đang shake mạnh hơn, không downgrade — giữ event mạnh nhất hiện tại.
            float now = Time.unscaledTime;
            float currentRemaining = Mathf.Max(0f, shakeEndsAt - now);
            float currentAmpEffective = IsShaking
                ? shakeAmplitude * (currentRemaining / Mathf.Max(0.0001f, shakeEndsAt - shakeStartedAt))
                : 0f;
            if (amplitude < currentAmpEffective) return;

            shakeAmplitude = Mathf.Min(maxAmplitude, amplitude);
            shakeStartedAt = now;
            shakeEndsAt = now + duration;
        }
    }
}
