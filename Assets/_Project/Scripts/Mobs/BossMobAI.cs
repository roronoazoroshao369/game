using System.Collections;
using UnityEngine;
using WildernessCultivation.Combat;
using WildernessCultivation.Player;

namespace WildernessCultivation.Mobs
{
    /// <summary>
    /// Boss bí cảnh đa pha. HP threshold → đổi pattern:
    ///   Phase 1 (100% → 60%): rượt + đánh thường (melee).
    ///   Phase 2 (60%  → 25%): summon thêm minions + buff dame.
    ///   Phase 3 (25%  → 0% ): bắn projectile vòng tròn (volley) định kỳ + tốc độ x1.5.
    ///
    /// Cần prefab có Rigidbody2D + Collider2D. Có thể gắn 1 prefab projectile để bắn ở phase 3.
    /// </summary>
    public class BossMobAI : MobBase
    {
        [Header("Boss")]
        public string title = "Yêu Tướng";
        public float phase2Threshold = 0.6f;
        public float phase3Threshold = 0.25f;
        public float phase2DamageMultiplier = 1.5f;
        public float phase3SpeedMultiplier = 1.5f;

        [Header("Summons (phase 2)")]
        public GameObject minionPrefab;
        public int minionsPerSummon = 2;
        public float summonCooldown = 8f;

        [Header("Volley (phase 3)")]
        public GameObject projectilePrefab;
        public int volleyCount = 8;
        public float volleyCooldown = 4f;
        public float volleySpeed = 6f;
        public float volleyDamage = 12f;

        [Header("Drops bonus (vd pháp bảo)")]
        public WildernessCultivation.Items.ItemSO bonusDropItem;
        public int bonusDropCount = 1;

        public int CurrentPhase { get; private set; } = 1;
        float nextSummonAt;
        float nextVolleyAt;
        float baseDamage;
        float baseSpeed;

        protected override void Awake()
        {
            base.Awake();
            baseDamage = damage;
            baseSpeed = moveSpeed;
        }

        void FixedUpdate()
        {
            UpdatePhase();
            if (!TryFindPlayer()) { StopMoving(); return; }

            float dist = Vector2.Distance(transform.position, target.position);
            if (dist > attackRange) MoveTowards(target.position);
            else { StopMoving(); TryAttack(); }

            // Phase-specific actions
            if (CurrentPhase >= 2 && Time.time >= nextSummonAt) Summon();
            if (CurrentPhase >= 3 && Time.time >= nextVolleyAt) ShootVolley();
        }

        void UpdatePhase()
        {
            float frac = HP / Mathf.Max(1f, maxHP);
            int newPhase = frac > phase2Threshold ? 1
                          : frac > phase3Threshold ? 2 : 3;
            if (newPhase == CurrentPhase) return;

            CurrentPhase = newPhase;
            damage = baseDamage * (CurrentPhase >= 2 ? phase2DamageMultiplier : 1f);
            moveSpeed = baseSpeed * (CurrentPhase >= 3 ? phase3SpeedMultiplier : 1f);
            // Reset cooldowns để vào phase mới có hành động ngay
            nextSummonAt = Time.time + 1.5f;
            nextVolleyAt = Time.time + 1.5f;
            Debug.Log($"[Boss] {mobName} ({title}) → Phase {CurrentPhase}, HP {HP:F0}/{maxHP:F0}");
        }

        void TryAttack()
        {
            if (Time.time < attackReadyAt) return;
            attackReadyAt = Time.time + attackCooldown;

            var dmg = target.GetComponent<IDamageable>() ?? target.GetComponentInParent<IDamageable>();
            if (dmg != null)
            {
                dmg.TakeDamage(damage, gameObject);
            }
            else
            {
                // Player implement TakeDamage trên PlayerStats nhưng không qua IDamageable — fallback
                var ps = target.GetComponent<PlayerStats>() ?? target.GetComponentInParent<PlayerStats>();
                ps?.TakeDamage(damage);
            }
        }

        void Summon()
        {
            nextSummonAt = Time.time + summonCooldown;
            if (minionPrefab == null) return;
            for (int i = 0; i < minionsPerSummon; i++)
            {
                var offset = Random.insideUnitCircle * 1.5f;
                Instantiate(minionPrefab, transform.position + (Vector3)offset, Quaternion.identity);
            }
            Debug.Log($"[Boss] Summon {minionsPerSummon} minion.");
        }

        void ShootVolley()
        {
            nextVolleyAt = Time.time + volleyCooldown;
            if (projectilePrefab == null) return;
            for (int i = 0; i < volleyCount; i++)
            {
                float ang = (360f / Mathf.Max(1, volleyCount)) * i * Mathf.Deg2Rad;
                Vector2 dir = new(Mathf.Cos(ang), Mathf.Sin(ang));
                var go = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
                var p = go.GetComponent<Projectile>();
                if (p != null)
                {
                    p.speed = volleySpeed;
                    p.damage = volleyDamage;
                    p.lifetime = 4f;
                    p.Launch(dir, gameObject);
                }
            }
            Debug.Log($"[Boss] Volley x{volleyCount}");
        }

        protected override void Die(GameObject killer)
        {
            // Drop pháp bảo bonus
            if (bonusDropItem != null && killer != null)
            {
                var inv = killer.GetComponentInParent<WildernessCultivation.Items.Inventory>();
                if (inv != null) inv.Add(bonusDropItem, bonusDropCount);
                Debug.Log($"[Boss] Drop bonus: {bonusDropItem.displayName} x{bonusDropCount}");
            }
            base.Die(killer);
        }
    }
}
