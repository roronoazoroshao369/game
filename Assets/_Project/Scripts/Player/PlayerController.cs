using UnityEngine;
using WildernessCultivation.Core;
using WildernessCultivation.Cultivation;
using WildernessCultivation.Items;
using WildernessCultivation.Player.Status;
using WildernessCultivation.UI;

namespace WildernessCultivation.Player
{
    /// <summary>
    /// Top-down 2D controller. Hỗ trợ cả keyboard (test trên PC) và virtual joystick (mobile).
    /// Yêu cầu: Rigidbody2D (Dynamic, gravity 0), Collider2D, SpriteRenderer.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        public float moveSpeed = 4f;
        public float runMultiplier = 1.5f;

        [Header("Encumbrance")]
        [Tooltip("Reference Inventory để tính TotalWeight. Auto-detect trong Awake nếu null.")]
        public Inventory inventory;
        [Tooltip("Khối lượng tối đa không bị slow.")]
        public float maxCarryWeight = 30f;
        [Tooltip("Khối lượng vượt threshold này → đứng yên (over-encumbered cứng).")]
        public float overEncumberedHardCap = 60f;
        [Tooltip("Multiplier tốc độ khi đạt overEncumberedHardCap. Tuyến tính từ 1 → giá trị này.")]
        [Range(0.1f, 1f)] public float overweightSpeedMin = 0.4f;

        [Header("Input")]
        [Tooltip("Để trống nếu chưa có UI joystick — sẽ fallback về WASD/Arrow keys.")]
        public VirtualJoystick joystick;

        [Header("Animation (optional)")]
        public Animator animator;
        public SpriteRenderer spriteRenderer;

        Rigidbody2D rb;
        Vector2 input;
        Vector2 lastFacing = Vector2.down;

        public Vector2 Facing => lastFacing;
        public Vector2 InputDir => input;

        /// <summary>Khi true, controller bỏ qua input + zero velocity. Dùng cho action chặn move (vd câu cá).</summary>
        public bool MovementLocked { get; set; }

        SpiritRoot spiritRoot;
        StatusEffectManager statusManager;

        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            if (inventory == null) inventory = GetComponent<Inventory>();
            spiritRoot = GetComponent<SpiritRoot>();
            statusManager = GetComponent<StatusEffectManager>();
            ServiceLocator.Register<PlayerController>(this);
        }

        void OnDestroy() => ServiceLocator.Unregister<PlayerController>(this);

        /// <summary>maxCarryWeight có cộng modifier linh căn (Thổ x1.5 …).</summary>
        public float EffectiveMaxCarryWeight => maxCarryWeight * (spiritRoot != null ? spiritRoot.CarryMul : 1f);

        /// <summary>Multiplier tốc độ theo encumbrance hiện tại. 1 = bình thường, &lt;1 = chậm.</summary>
        public float EncumbranceMultiplier
        {
            get
            {
                if (inventory == null) return 1f;
                float w = inventory.TotalWeight;
                float cap = EffectiveMaxCarryWeight;
                float hard = overEncumberedHardCap * (spiritRoot != null ? spiritRoot.CarryMul : 1f);
                if (w <= cap) return 1f;
                if (w >= hard) return overweightSpeedMin;
                float t = (w - cap) / Mathf.Max(0.001f, hard - cap);
                return Mathf.Lerp(1f, overweightSpeedMin, t);
            }
        }

        public float StatusSpeedMultiplier => statusManager != null ? statusManager.MoveSpeedMultiplier : 1f;

        void Update()
        {
            input = ReadInput();
            if (MovementLocked) input = Vector2.zero;
            if (input.sqrMagnitude > 0.01f)
            {
                lastFacing = input.normalized;
                if (spriteRenderer != null && Mathf.Abs(input.x) > 0.1f)
                    spriteRenderer.flipX = input.x < 0;
            }

            if (animator != null)
            {
                animator.SetFloat("Speed", input.magnitude);
                animator.SetFloat("DirX", lastFacing.x);
                animator.SetFloat("DirY", lastFacing.y);
            }
        }

        void FixedUpdate()
        {
            if (MovementLocked) { rb.velocity = Vector2.zero; return; }
            rb.velocity = input * moveSpeed * EncumbranceMultiplier * StatusSpeedMultiplier;
        }

        Vector2 ReadInput()
        {
            if (joystick != null && joystick.IsActive)
                return Vector2.ClampMagnitude(joystick.Direction, 1f);

            float x = Input.GetAxisRaw("Horizontal");
            float y = Input.GetAxisRaw("Vertical");
            return Vector2.ClampMagnitude(new Vector2(x, y), 1f);
        }
    }
}
