using UnityEngine;
using WildernessCultivation.Combat;
using WildernessCultivation.Player;

namespace WildernessCultivation.Mobs
{
    /// <summary>
    /// Heo Rừng — neutral. Wander quanh khu vực; aggro khi player vào tầm gần
    /// HOẶC khi bị tấn công. State machine 3 phase: Idle/Wander → Aggro chase →
    /// Charge (burst speed cố định hướng), sau charge stun ngắn rồi quay về aggro.
    /// </summary>
    public class BoarAI : MobBase
    {
        public enum BoarState { Wander, Aggro, Charge, Stunned }

        [Header("Boar tuning")]
        [Tooltip("Phạm vi player vào sẽ provoke heo rừng (gần hơn aggroRange của wolf).")]
        public float provokeRange = 2.5f;
        [Tooltip("Tốc độ × moveSpeed khi charge.")]
        public float chargeSpeedMultiplier = 3f;
        [Tooltip("Charge kéo dài bao lâu trước khi stun.")]
        public float chargeDurationSec = 1.2f;
        [Tooltip("Stun nghỉ giữa các charge.")]
        public float chargeStunSec = 1.5f;
        [Tooltip("Cooldown giữa 2 lần khởi động charge (sau stun).")]
        public float chargeCooldownSec = 3f;
        [Tooltip("Damage charge khi va player (ngoài melee bình thường).")]
        public float chargeDamage = 14f;
        [Tooltip("Wander radius khi chưa aggro.")]
        public float wanderRadius = 2.5f;
        [Tooltip("Khoảng giữa 2 lần đổi đích wander.")]
        public float wanderInterval = 4f;

        public BoarState State { get; private set; } = BoarState.Wander;

        Vector2 wanderTarget;
        Vector2 chargeDir;
        float nextWanderAt;
        float stateUntil;
        float chargeReadyAt;
        bool hitDuringCharge;

        protected override void Awake()
        {
            base.Awake();
            if (string.IsNullOrEmpty(mobName) || mobName == "Quái") mobName = "Heo Rừng";
            wanderTarget = transform.position;
        }

        public override void TakeDamage(float amount, GameObject source)
        {
            base.TakeDamage(amount, source);
            // Bị tấn công → aggro ngay (giải mã Projectile owner đã làm trong base).
            if (target == null && source != null) target = source.transform;
            if (HP > 0f && State == BoarState.Wander) EnterAggro();
        }

        void Update()
        {
            if (!ShouldTickAI()) return;

            switch (State)
            {
                case BoarState.Wander: TickWander(); break;
                case BoarState.Aggro: TickAggro(); break;
                case BoarState.Charge: TickCharge(); break;
                case BoarState.Stunned: TickStunned(); break;
            }
        }

        void TickWander()
        {
            // Provoke check: player vào quá gần.
            var hit = Physics2D.OverlapCircle(transform.position, provokeRange, playerMask);
            if (hit != null) { target = hit.transform; EnterAggro(); return; }

            if (Time.time >= nextWanderAt || Vector2.Distance(transform.position, wanderTarget) < 0.2f)
            {
                wanderTarget = (Vector2)transform.position + Random.insideUnitCircle * wanderRadius;
                nextWanderAt = Time.time + wanderInterval;
            }
            MoveTowards(wanderTarget);
        }

        void TickAggro()
        {
            if (!TryFindPlayer()) { State = BoarState.Wander; StopMoving(); return; }

            float dist = Vector2.Distance(target.position, transform.position);
            if (dist <= attackRange)
            {
                StopMoving();
                if (Time.time >= attackReadyAt)
                {
                    attackReadyAt = Time.time + attackCooldown;
                    DealMeleeDamage(damage);
                }
                return;
            }

            // Đủ xa + cooldown đã ready → charge.
            if (Time.time >= chargeReadyAt)
            {
                EnterCharge();
                return;
            }
            MoveTowards(target.position);
        }

        void EnterAggro()
        {
            State = BoarState.Aggro;
            chargeReadyAt = Time.time + 0.5f; // Delay nhỏ trước khi cho phép charge đầu tiên.
        }

        void EnterCharge()
        {
            if (target == null) { State = BoarState.Aggro; return; }
            chargeDir = ((Vector2)target.position - (Vector2)transform.position).normalized;
            if (chargeDir.sqrMagnitude < 0.001f) chargeDir = Vector2.right;
            State = BoarState.Charge;
            stateUntil = Time.time + chargeDurationSec;
            hitDuringCharge = false;
            if (spriteRenderer != null) spriteRenderer.flipX = chargeDir.x < 0;
        }

        void TickCharge()
        {
            rb.velocity = chargeDir * (moveSpeed * chargeSpeedMultiplier);

            // Va player trong charge → 1 lần damage charge mạnh, không spam.
            if (!hitDuringCharge && target != null)
            {
                float d = Vector2.Distance(transform.position, target.position);
                if (d <= attackRange + 0.2f)
                {
                    DealMeleeDamage(chargeDamage);
                    hitDuringCharge = true;
                }
            }

            if (Time.time >= stateUntil) EnterStun();
        }

        void EnterStun()
        {
            State = BoarState.Stunned;
            stateUntil = Time.time + chargeStunSec;
            chargeReadyAt = Time.time + chargeStunSec + chargeCooldownSec;
            StopMoving();
        }

        void TickStunned()
        {
            StopMoving();
            if (Time.time >= stateUntil) State = BoarState.Aggro;
        }

        void DealMeleeDamage(float amount)
        {
            if (target == null) return;
            var dmg = target.GetComponent<IDamageable>() ?? target.GetComponentInParent<IDamageable>();
            if (dmg != null) dmg.TakeDamage(amount, gameObject);
        }
    }
}
