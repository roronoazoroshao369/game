using UnityEngine;

namespace WildernessCultivation.Vfx
{
    /// <summary>
    /// Lay nhẹ theo trục Z (rotation) bằng sin-wave để cây / cỏ trông "sống". Rất rẻ
    /// trên mobile (1 phép sin + assign rotation/frame). Pha ngẫu nhiên theo
    /// instanceId → cây cạnh nhau không lay đồng pha cứng.
    ///
    /// Pure logic <see cref="ComputeAngleDegrees"/> tách static để EditMode test
    /// được không cần MonoBehaviour scene.
    /// </summary>
    public class WindSway : MonoBehaviour
    {
        [Header("Sway")]
        [Tooltip("Biên độ tối đa (độ). Cây ~2°, cỏ ~4°.")]
        public float amplitudeDegrees = 2.5f;
        [Tooltip("Tần số (Hz). Cây ~0.5-0.8, cỏ ~1-1.5.")]
        public float frequencyHz = 0.7f;
        [Tooltip("Pha ban đầu (radian). Auto-randomize trong Awake nếu = 0.")]
        public float phaseRadians;

        Quaternion baseRotation;

        void Awake()
        {
            baseRotation = transform.localRotation;
            if (Mathf.Approximately(phaseRadians, 0f))
            {
                // Pha lệch theo instanceId → cây cạnh nhau không đồng pha. Modulo 2π.
                phaseRadians = (GetInstanceID() & 0x3FF) / 1024f * Mathf.PI * 2f;
            }
        }

        void Update()
        {
            float deg = ComputeAngleDegrees(Time.time, frequencyHz, amplitudeDegrees, phaseRadians);
            transform.localRotation = baseRotation * Quaternion.Euler(0f, 0f, deg);
        }

        /// <summary>
        /// Pure: <c>amplitude * sin(2π · frequency · time + phase)</c>. Test deterministic.
        /// </summary>
        public static float ComputeAngleDegrees(float time, float frequencyHz,
            float amplitudeDegrees, float phaseRadians)
        {
            return amplitudeDegrees * Mathf.Sin(2f * Mathf.PI * frequencyHz * time + phaseRadians);
        }
    }
}
