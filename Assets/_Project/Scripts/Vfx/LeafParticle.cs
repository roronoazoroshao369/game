using UnityEngine;

namespace WildernessCultivation.Vfx
{
    /// <summary>
    /// Per-particle behavior cho ReactiveOnHit burst. Velocity-based motion + gravity
    /// + alpha fade theo lifetime → auto destroy. Pooling skip cho MVP (hit infrequent
    /// — 2-3 hit/tree, particle peak ~12 trên scene cùng lúc).
    /// </summary>
    public class LeafParticle : MonoBehaviour
    {
        Vector2 velocity;
        float gravity = 6f;
        float drag = 1.5f;
        float age;
        float lifetime;
        Color baseColor = Color.white;
        SpriteRenderer sr;

        void Awake() { sr = GetComponent<SpriteRenderer>(); }

        public void Launch(Vector2 initialVelocity, float lifetimeSeconds, Color tint)
        {
            velocity = initialVelocity;
            lifetime = Mathf.Max(0.05f, lifetimeSeconds);
            baseColor = tint;
            age = 0f;
            if (sr != null) sr.color = baseColor;
        }

        void Update()
        {
            float dt = Time.deltaTime;
            age += dt;

            velocity.y -= gravity * dt;
            velocity *= Mathf.Max(0f, 1f - drag * dt);
            transform.position += (Vector3)(velocity * dt);

            if (sr != null)
            {
                float t = Mathf.Clamp01(age / lifetime);
                Color c = baseColor;
                c.a = baseColor.a * (1f - t);
                sr.color = c;
            }

            if (age >= lifetime) Destroy(gameObject);
        }
    }
}
