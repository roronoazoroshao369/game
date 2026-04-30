using System.Collections;
using WildernessCultivation.Core;
using UnityEngine;
using WildernessCultivation.Cultivation;
using WildernessCultivation.UI;

namespace WildernessCultivation.Player
{
    /// <summary>
    /// Né (dodge / roll) tức thì theo hướng input — nếu không có input thì theo Facing.
    /// Trong duration: i-frames (TakeDamage bỏ qua), MovementLocked = true, velocity ép theo
    /// hướng dodge. Cooldown riêng. Tốn ít mana (configurable, mặc định 5).
    ///
    /// Yêu cầu mana đủ + không đang sleep + không MovementLocked-vì-action-khác.
    /// </summary>
    [RequireComponent(typeof(PlayerController), typeof(PlayerStats), typeof(Rigidbody2D))]
    public class DodgeAction : MonoBehaviour
    {
        [Header("Inputs")]
        [Tooltip("Phím dodge khi test trên PC.")]
        public KeyCode dodgeKey = KeyCode.LeftShift;

        [Header("UI Joystick (optional)")]
        [Tooltip("Đọc Facing từ controller; joystick chỉ dùng để xác định nếu input đang active.")]
        public VirtualJoystick joystick;

        [Header("Tuning")]
        [Tooltip("Quãng đường roll (units).")]
        public float dodgeDistance = 3f;
        [Tooltip("Thời gian roll (giây). Velocity = distance / duration.")]
        public float dodgeDuration = 0.25f;
        [Tooltip("Cooldown sau khi roll xong.")]
        public float cooldown = 1f;
        [Tooltip("Mana cost (Linh Khí). 0 = miễn phí.")]
        public float manaCost = 5f;

        [Header("I-frames")]
        [Tooltip("Thời gian invulnerable trong lúc roll. Mặc định = dodgeDuration; có thể đặt < để punish timing kém.")]
        public float invulnerabilityDuration = 0.25f;

        public bool IsDodging { get; private set; }
        public event System.Action OnDodgeStart;
        public event System.Action OnDodgeEnd;

        PlayerController controller;
        PlayerStats stats;
        Rigidbody2D rb;
        SpiritRoot spiritRoot;
        float readyAt;

        void Awake()
        {
            controller = GetComponent<PlayerController>();
            stats = GetComponent<PlayerStats>();
            rb = GetComponent<Rigidbody2D>();
            spiritRoot = GetComponent<SpiritRoot>();
            ServiceLocator.Register<DodgeAction>(this);
        }

        void OnDestroy() => ServiceLocator.Unregister<DodgeAction>(this);

        void Update()
        {
            if (Input.GetKeyDown(dodgeKey)) TryDodge();
        }

        public bool CanDodge()
        {
            if (IsDodging) return false;
            if (Time.time < readyAt) return false;
            if (stats.IsDead) return false;
            if (controller.MovementLocked) return false;
            if (stats.Mana < manaCost) return false;
            return true;
        }

        public bool TryDodge()
        {
            if (!CanDodge()) return false;
            Vector2 dir = controller.InputDir.sqrMagnitude > 0.01f
                ? controller.InputDir.normalized
                : controller.Facing;
            if (dir.sqrMagnitude < 0.01f) dir = Vector2.down;

            if (manaCost > 0f && !stats.TryConsumeMana(manaCost)) return false;
            StartCoroutine(DodgeRoutine(dir));
            return true;
        }

        IEnumerator DodgeRoutine(Vector2 dir)
        {
            IsDodging = true;
            readyAt = Time.time + dodgeDuration + cooldown;
            controller.MovementLocked = true;

            float iframes = invulnerabilityDuration > 0f ? invulnerabilityDuration : dodgeDuration;
            stats.SetInvulnerable(iframes);

            // Linh căn Mộc/Thổ có thể buff distance (carry mul placeholder); để đơn giản dùng cố định.
            float speed = dodgeDuration > 0.01f ? dodgeDistance / dodgeDuration : dodgeDistance;
            float t = 0f;
            OnDodgeStart?.Invoke();
            while (t < dodgeDuration)
            {
                rb.velocity = dir * speed;
                t += Time.deltaTime;
                yield return null;
            }
            rb.velocity = Vector2.zero;
            controller.MovementLocked = false;
            IsDodging = false;
            OnDodgeEnd?.Invoke();
        }

        void OnDisable()
        {
            if (IsDodging)
            {
                StopAllCoroutines();
                IsDodging = false;
                if (controller != null) controller.MovementLocked = false;
                if (rb != null) rb.velocity = Vector2.zero;
            }
        }
    }
}
