using UnityEngine;

namespace WildernessCultivation.Vfx
{
    /// <summary>
    /// Cỏ / bụi cây bend khỏi player khi player đi xuyên qua. Smooth tween rotation
    /// theo Vector từ player-to-self → tạo cảm giác "vẹt cỏ" giống Don't Starve.
    ///
    /// Lazy detect — Update poll OverlapCircle theo <see cref="detectRadius"/>;
    /// nếu trong range, tính bend angle từ <see cref="ComputeBendAngleDeg"/>.
    /// Compose với WindSway: sway nhẹ + bend khi player gần (ưu tiên bend).
    ///
    /// Pure <see cref="ComputeBendAngleDeg"/> static → EditMode test.
    /// </summary>
    public class PlayerOverlapSway : MonoBehaviour
    {
        [Tooltip("Bán kính detect player (unit). 0.6 = chỉ bend khi player gần như chạm.")]
        public float detectRadius = 0.6f;

        [Tooltip("Biên độ bend tối đa (độ). Cỏ ~12°, berry bush ~8°, mushroom ~5°.")]
        public float maxBendDeg = 12f;

        [Tooltip("Tốc độ smoothing rotation (rad/s tương đương). Lớn → snap, nhỏ → mượt. 8 = nhanh, 4 = mượt.")]
        public float smoothing = 8f;

        [Tooltip("Layer mask chứa player. Mặc định Everything; bootstrap có thể narrow.")]
        public LayerMask playerMask = ~0;

        Quaternion baseRotation;
        float currentBendDeg;

        void Awake() { baseRotation = transform.localRotation; }

        void Update()
        {
            float targetDeg = 0f;
            var hit = Physics2D.OverlapCircle(transform.position, detectRadius, playerMask);
            if (hit != null)
            {
                Vector2 selfXY = transform.position;
                Vector2 playerXY = hit.transform.position;
                targetDeg = ComputeBendAngleDeg(selfXY - playerXY, maxBendDeg, detectRadius);
            }

            // Exponential smoothing — frame-rate independent với (1 - exp(-k·dt)).
            float k = Mathf.Max(0.01f, smoothing);
            float lerp = 1f - Mathf.Exp(-k * Time.deltaTime);
            currentBendDeg = Mathf.Lerp(currentBendDeg, targetDeg, lerp);

            transform.localRotation = baseRotation * Quaternion.Euler(0f, 0f, currentBendDeg);
        }

        /// <summary>
        /// Pure: tính bend angle từ vector self - player.
        /// Direction: cỏ ngả ra xa player (X negative → bend +Y), proportional 1/dist.
        /// </summary>
        /// <param name="selfMinusPlayer">Vector từ player tới self (self.pos - player.pos).</param>
        /// <param name="maxBendDeg">Biên độ tối đa (độ).</param>
        /// <param name="detectRadius">Khoảng cách detect → magnitude tham chiếu.</param>
        public static float ComputeBendAngleDeg(Vector2 selfMinusPlayer, float maxBendDeg, float detectRadius)
        {
            float dist = selfMinusPlayer.magnitude;
            if (detectRadius <= 0f) return 0f;
            // Closer = stronger bend. dist=0 → max, dist=detectRadius → 0.
            float strength = Mathf.Clamp01(1f - (dist / detectRadius));

            // Sign từ direction: player bên trái self (selfMinusPlayer.x > 0) → cỏ ngả phải (positive Z rotation).
            float sign = selfMinusPlayer.x >= 0f ? 1f : -1f;

            return sign * strength * maxBendDeg;
        }
    }
}
