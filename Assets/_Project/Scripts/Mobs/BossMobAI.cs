using System.Collections;
using UnityEngine;
using WildernessCultivation.Combat;
using WildernessCultivation.Cultivation;
using WildernessCultivation.Player;
using WildernessCultivation.Player.Status;

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

        [Header("Element resistance (linh căn boss)")]
        [Tooltip("Linh căn của boss. Player technique cùng element → dame x sameElementResistance (vd 0.5 = giảm 50%).")]
        public SpiritElement element = SpiritElement.None;
        [Range(0.05f, 1f)] public float sameElementResistance = 0.5f;
        [Tooltip("Multiplier dame nhận từ technique tương khắc (vd Hoả ⇄ Thuỷ → 1.5x).")]
        public float counterElementVulnerability = 1.5f;

        [Header("Aura status (apply lên player trong radius)")]
        [Tooltip("Status effect áp lên player khi đứng trong radius (vd Băng Phách Long → Freeze).")]
        public StatusEffectSO auraStatusEffect;
        public float auraRadius = 2.5f;
        public float auraTickInterval = 1.5f;
        public float auraStatusDuration = 4f;
        float nextAuraTickAt;

        [Header("On-hit status (áp khi boss đập trúng player)")]
        public StatusEffectSO onHitStatusEffect;
        public float onHitStatusDuration = 5f;

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
            if (auraStatusEffect != null && Time.time >= nextAuraTickAt) TickAura();
        }

        void TickAura()
        {
            nextAuraTickAt = Time.time + Mathf.Max(0.1f, auraTickInterval);
            if (target == null) return;
            float d = Vector2.Distance(transform.position, target.position);
            if (d > auraRadius) return;
            var mgr = target.GetComponent<StatusEffectManager>() ?? target.GetComponentInParent<StatusEffectManager>();
            if (mgr != null) mgr.Apply(auraStatusEffect, auraStatusDuration);
        }

        public override void TakeDamage(float amount, GameObject source)
        {
            // Nếu source là Projectile → đọc element + dùng owner làm "aggro source".
            GameObject aggroSource = source;
            if (source != null)
            {
                var proj = source.GetComponent<Projectile>();
                if (proj != null)
                {
                    if (element != SpiritElement.None && proj.element != SpiritElement.None)
                    {
                        if (proj.element == element) amount *= sameElementResistance;
                        else if (IsCounter(proj.element, element)) amount *= counterElementVulnerability;
                    }
                    if (proj.Owner != null) aggroSource = proj.Owner;
                }
            }
            base.TakeDamage(amount, aggroSource);
        }

        // Quan hệ tương khắc đơn giản: Hoả ⇄ Thuỷ, Mộc ⇄ Kim, Thổ ⇄ Mộc.
        static bool IsCounter(SpiritElement a, SpiritElement b)
        {
            return (a == SpiritElement.Hoa && b == SpiritElement.Thuy) ||
                   (a == SpiritElement.Thuy && b == SpiritElement.Hoa) ||
                   (a == SpiritElement.Kim && b == SpiritElement.Moc) ||
                   (a == SpiritElement.Moc && b == SpiritElement.Tho) ||
                   (a == SpiritElement.Tho && b == SpiritElement.Thuy);
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

            // On-hit status: vd boss có Bleeding nanh → áp Bleeding khi đập trúng.
            if (onHitStatusEffect != null)
            {
                var mgr = target.GetComponent<StatusEffectManager>() ?? target.GetComponentInParent<StatusEffectManager>();
                if (mgr != null) mgr.Apply(onHitStatusEffect, onHitStatusDuration);
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
