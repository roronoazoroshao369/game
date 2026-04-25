using UnityEngine;
using WildernessCultivation.Combat;
using WildernessCultivation.Player;

namespace WildernessCultivation.Cultivation
{
    /// <summary>
    /// Hỏa Cầu Thuật — phóng 1 quả cầu lửa theo hướng đang quay mặt. Va chạm gây dame, có thể pierce.
    /// Cần prefab Projectile (Rigidbody2D + Collider2D trigger) gắn sẵn <see cref="Projectile"/> script.
    /// </summary>
    [CreateAssetMenu(menuName = "WildernessCultivation/Techniques/Fire Ball", fileName = "Tech_FireBall")]
    public class FireBallSO : TechniqueSO
    {
        [Header("Fire Ball")]
        public GameObject projectilePrefab;
        public float speed = 10f;
        public float damage = 22f;
        public float lifetime = 2.5f;
        public bool piercing = false;
        public LayerMask hitMask = ~0;
        [Tooltip("Số projectile bắn (>1 = bung quạt nhiều viên).")]
        [Min(1)] public int volley = 1;
        [Tooltip("Góc quạt giữa các viên khi volley > 1.")]
        public float spreadDegrees = 12f;

        public override void Cast(PlayerCombat caster)
        {
            if (projectilePrefab == null)
            {
                Debug.LogWarning($"[FireBall] {name}: thiếu projectilePrefab.");
                return;
            }

            Vector2 origin = (Vector2)caster.transform.position + caster.Controller.Facing * 0.5f;
            Vector2 baseDir = caster.Controller.Facing;
            float baseAngle = Mathf.Atan2(baseDir.y, baseDir.x) * Mathf.Rad2Deg;

            int n = Mathf.Max(1, volley);
            float startOffset = -(n - 1) * 0.5f * spreadDegrees;
            for (int i = 0; i < n; i++)
            {
                float ang = (baseAngle + startOffset + i * spreadDegrees) * Mathf.Deg2Rad;
                Vector2 dir = new(Mathf.Cos(ang), Mathf.Sin(ang));

                var go = Object.Instantiate(projectilePrefab, origin, Quaternion.identity);
                var p = go.GetComponent<Projectile>();
                if (p == null)
                {
                    Debug.LogWarning($"[FireBall] projectilePrefab thiếu Projectile script.");
                    Object.Destroy(go);
                    continue;
                }
                p.speed = speed;
                p.damage = damage;
                p.lifetime = lifetime;
                p.piercing = piercing;
                p.hitMask = hitMask;
                p.Launch(dir, caster.gameObject);
            }
        }
    }
}
