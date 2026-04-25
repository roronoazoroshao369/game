using UnityEngine;
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

        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
        }

        void Update()
        {
            input = ReadInput();
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
            rb.velocity = input * moveSpeed;
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
