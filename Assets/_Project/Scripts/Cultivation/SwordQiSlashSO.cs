using UnityEngine;
using WildernessCultivation.Combat;
using WildernessCultivation.Player;

namespace WildernessCultivation.Cultivation
{
    /// <summary>
    /// Kiếm Khí Trảm — phóng 1 luồng kiếm khí thẳng theo hướng đang quay mặt.
    /// Gây dame cho mọi target trong tầm. MVP: dùng raycast cone đơn giản.
    /// </summary>
    [CreateAssetMenu(menuName = "WildernessCultivation/Techniques/Sword Qi Slash", fileName = "Tech_SwordQiSlash")]
    public class SwordQiSlashSO : TechniqueSO
    {
        [Header("Sword Qi")]
        public float range = 4f;
        public float damage = 18f;
        public float halfWidth = 0.6f;
        public LayerMask hitMask = ~0;
        public GameObject vfxPrefab; // optional

        public override void Cast(PlayerCombat caster)
        {
            Vector2 origin = caster.transform.position;
            Vector2 dir = caster.Controller.Facing;
            Vector2 size = new Vector2(range, halfWidth * 2f);
            Vector2 boxCenter = origin + dir * (range * 0.5f);
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            var hits = Physics2D.OverlapBoxAll(boxCenter, size, angle, hitMask);
            foreach (var h in hits)
            {
                if (h.gameObject == caster.gameObject) continue;
                var dmg = h.GetComponent<IDamageable>();
                dmg?.TakeDamage(damage, caster.gameObject);
            }

            if (vfxPrefab != null)
            {
                var go = Object.Instantiate(vfxPrefab, origin, Quaternion.Euler(0, 0, angle));
                Object.Destroy(go, 1.5f);
            }

            Debug.Log($"[Skill] Kiếm Khí Trảm! hit {hits.Length} targets.");
        }
    }
}
