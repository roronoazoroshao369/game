using UnityEngine;
using WildernessCultivation.Core;
using WildernessCultivation.Combat;
using WildernessCultivation.Items;
using WildernessCultivation.World;

namespace WildernessCultivation.Mobs
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public abstract class MobBase : CharacterBase
    {
        [Header("Identity")]
        public string mobName = "Quái";

        [Header("Stats")]
        public float maxHP = 20f;
        public float HP = 20f;

        // R5 CharacterBase polymorphic view — read-only, delegate tới field gốc.
        // Subclass logic damage / die không đổi (TakeDamage override bên dưới giữ nguyên
        // FlashHit + drop loot + xp reward + aggro on hit).
        public override float CurrentHP => HP;
        public override float CurrentMaxHP => maxHP;
        public override bool IsDead => HP <= 0f;
        public float moveSpeed = 1.5f;
        public float damage = 5f;
        public float attackCooldown = 1f;
        public float xpReward = 5f;

        [Header("Drops")]
        public ResourceNode.Drop[] drops;

        [Header("Senses")]
        public float aggroRange = 4f;
        public float attackRange = 0.8f;
        public LayerMask playerMask;

        [Header("Refs")]
        public Transform target;
        public SpriteRenderer spriteRenderer;
        public Animator animator;

        [Header("LOD (perf)")]
        [Tooltip("Khoảng cách tới player vượt quá đây → AI tick chậm + tạm tắt physics. <= 0 → tắt LOD.")]
        public float lodFarDistance = 18f;
        [Tooltip("Khi xa hơn lodFarDistance, chỉ tick 1/N frame (mặc định 8 → giảm 87% CPU cost cho mob xa).")]
        public int lodSlowFrameMod = 8;

        protected Rigidbody2D rb;
        protected float attackReadyAt;

        // Cache player ref tránh ServiceLocator.Get fallback mỗi frame trên mọi mob.
        // Reset mỗi frame để pickup khi player respawn / scene reload.
        static Transform s_cachedPlayer;
        static int s_cachedPlayerFrame = -1;

        protected virtual void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            HP = maxHP;
        }

        /// <summary>
        /// Pure logic của LOD gating. Returns true → mob nên tick AI frame này.
        /// <paramref name="wantSimulated"/> báo physics nên bật / tắt.
        /// Tách static để EditMode test gọi được không cần Rigidbody2D.
        /// </summary>
        public static bool ComputeShouldTick(
            float distance, float lodFarDistance, int lodSlowFrameMod,
            int frameCount, int instanceId, out bool wantSimulated)
        {
            if (lodFarDistance <= 0f) { wantSimulated = true; return true; }
            bool isFar = distance > lodFarDistance;
            wantSimulated = !isFar;
            if (!isFar) return true;
            int mod = Mathf.Max(1, lodSlowFrameMod);
            // Cộng instanceId để các mob xa lệch pha nhau (tránh spike cùng frame).
            return ((frameCount + instanceId) % mod) == 0;
        }

        static Transform GetPlayerCached()
        {
            if (s_cachedPlayerFrame == Time.frameCount && s_cachedPlayer != null) return s_cachedPlayer;
            var ps = ServiceLocator.Get<WildernessCultivation.Player.PlayerStats>();
            s_cachedPlayer = ps != null ? ps.transform : null;
            s_cachedPlayerFrame = Time.frameCount;
            return s_cachedPlayer;
        }

        /// <summary>
        /// Gọi ở đầu Update(). Trả false → mob skip frame này (xa player).
        /// Tự đồng bộ rb.simulated với khoảng cách. Mob không có player ref → luôn tick.
        /// </summary>
        protected bool ShouldTickAI()
        {
            var p = GetPlayerCached();
            if (p == null) return true;
            float dist = Vector2.Distance(p.position, transform.position);
            bool tick = ComputeShouldTick(
                dist, lodFarDistance, lodSlowFrameMod,
                Time.frameCount, GetInstanceID(),
                out bool wantSim);
            if (rb != null && rb.simulated != wantSim)
            {
                if (!wantSim) rb.velocity = Vector2.zero;
                rb.simulated = wantSim;
            }
            return tick;
        }

        public override void TakeDamage(float amount, GameObject source)
        {
            // Nếu source là Projectile → resolve về Owner cho aggro/loot, không lấy projectile gameObject.
            GameObject resolvedSource = source;
            if (source != null)
            {
                var proj = source.GetComponent<Projectile>();
                if (proj != null && proj.Owner != null) resolvedSource = proj.Owner;
            }
            HP -= amount;
            FlashHit();
            // Juice event (camera shake + damage number) tại vị trí mob.
            if (amount > 0f) CombatEvents.RaiseDamage(transform.position, amount, false);
            if (resolvedSource != null && target == null) target = resolvedSource.transform; // aggro on hit
            if (HP <= 0f) Die(resolvedSource);
        }

        protected virtual void FlashHit()
        {
            if (spriteRenderer != null) StartCoroutine(FlashCoroutine());
        }

        System.Collections.IEnumerator FlashCoroutine()
        {
            var orig = spriteRenderer.color;
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.08f);
            if (spriteRenderer != null) spriteRenderer.color = orig;
        }

        protected virtual void Die(GameObject killer)
        {
            // Drop loot vào inventory người giết (nếu có)
            var inv = killer != null ? killer.GetComponentInParent<Inventory>() : null;
            foreach (var d in drops)
            {
                int n = Random.Range(d.min, d.max + 1);
                if (n <= 0 || d.item == null) continue;
                if (inv != null) inv.Add(d.item, n);
            }

            // Cho XP tu luyện — chỉ awakened (đã khai mở tu tiên) mới tích XP từ kill mob.
            // Thường Nhân không thể tu luyện qua sát phạt (giữ MVP focus survival).
            if (killer != null)
            {
                var killerStats = killer.GetComponentInParent<WildernessCultivation.Player.PlayerStats>();
                bool awakened = killerStats != null && killerStats.IsAwakened;
                if (awakened)
                {
                    var realm = killer.GetComponentInParent<WildernessCultivation.Cultivation.RealmSystem>();
                    if (realm != null) realm.AddCultivationXp(xpReward);
                }
            }

            Destroy(gameObject);
        }

        protected void MoveTowards(Vector2 dest)
        {
            Vector2 dir = ((Vector2)dest - (Vector2)transform.position).normalized;
            rb.velocity = dir * moveSpeed;
            if (spriteRenderer != null && Mathf.Abs(dir.x) > 0.05f)
                spriteRenderer.flipX = dir.x < 0;
        }

        protected void StopMoving() => rb.velocity = Vector2.zero;

        protected bool TryFindPlayer()
        {
            if (target != null) return true;
            var hit = Physics2D.OverlapCircle(transform.position, aggroRange, playerMask);
            if (hit != null) { target = hit.transform; return true; }
            return false;
        }
    }
}
