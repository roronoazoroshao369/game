using UnityEngine;
using WildernessCultivation.Cultivation;
using WildernessCultivation.Player.Status;

namespace WildernessCultivation.Combat
{
    /// <summary>
    /// Đạn / luồng kỹ năng bay thẳng theo direction. Va chạm với <see cref="IDamageable"/> trên
    /// layerMask → gây dame rồi tự huỷ. Tự huỷ sau <see cref="lifetime"/> nếu không trúng.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class Projectile : MonoBehaviour
    {
        public float speed = 10f;
        public float damage = 25f;
        public float lifetime = 3f;
        public LayerMask hitMask = ~0;
        [Tooltip("Có dame nhiều mục tiêu trước khi huỷ không?")]
        public bool piercing = false;

        [Header("Element / status")]
        [Tooltip("Linh tố của projectile. BossMobAI.element = same → giảm dame; tương khắc → tăng.")]
        public SpiritElement element = SpiritElement.None;
        [Tooltip("Optional status áp lên target khi trúng (vd FireBall → Burn).")]
        public StatusEffectSO onHitStatusEffect;
        public float onHitStatusDuration = 4f;

        Vector2 dir;
        GameObject owner;
        public GameObject Owner => owner;
        Rigidbody2D body;
        float dieAt;
        readonly System.Collections.Generic.HashSet<GameObject> hitOnce = new();

        void Awake() { body = GetComponent<Rigidbody2D>(); }

        public void Launch(Vector2 direction, GameObject owner)
        {
            this.owner = owner;
            dir = direction.sqrMagnitude < 0.001f ? Vector2.right : direction.normalized;
            dieAt = Time.time + lifetime;

            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
            if (body != null) body.velocity = dir * speed;
        }

        void Update()
        {
            if (Time.time >= dieAt) Destroy(gameObject);
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (other == null) return;
            if (owner != null && other.gameObject == owner) return;
            if (((1 << other.gameObject.layer) & (int)hitMask) == 0 && hitMask != ~0) return;
            if (!hitOnce.Add(other.gameObject)) return;

            var dmg = other.GetComponent<IDamageable>() ?? other.GetComponentInParent<IDamageable>();
            if (dmg != null) dmg.TakeDamage(damage, gameObject);

            if (onHitStatusEffect != null)
            {
                var mgr = other.GetComponent<StatusEffectManager>() ?? other.GetComponentInParent<StatusEffectManager>();
                if (mgr != null) mgr.Apply(onHitStatusEffect, onHitStatusDuration);
            }

            if (!piercing) Destroy(gameObject);
        }
    }
}
