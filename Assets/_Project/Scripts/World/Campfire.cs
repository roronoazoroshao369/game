using System.Collections.Generic;
using UnityEngine;
using WildernessCultivation.Crafting;
using WildernessCultivation.Items;
using WildernessCultivation.Player;

namespace WildernessCultivation.World
{
    /// <summary>
    /// Lửa trại — giữ ấm, hồi SAN cho người chơi đứng trong aura, đồng thời là
    /// <see cref="CraftStation.Campfire"/> để nướng thịt. Tự cháy cạn nhiên liệu;
    /// có thể tiếp gỗ qua <see cref="Interact"/> (phím E).
    ///
    /// Yêu cầu: GameObject có <see cref="CraftStationMarker"/> set thành Campfire,
    /// 1 SpriteRenderer (sprite ngọn lửa) và 1 Light/SpriteRenderer phụ làm hào quang
    /// được gán vào <see cref="auraRenderer"/>.
    /// </summary>
    [RequireComponent(typeof(CraftStationMarker))]
    public class Campfire : MonoBehaviour, IInteractable, IStationGate
    {
        public bool StationActive => IsLit;

        [Header("Aura")]
        [Tooltip("Bán kính hào quang lửa — trong vùng này SAN sẽ hồi nhẹ và không decay vào ban đêm.")]
        public float warmRadius = 3f;
        public float sanityRegenPerSec = 1.2f;
        public LayerMask playerMask = ~0;

        [Header("Fuel")]
        [Tooltip("Nhiên liệu hiện có (giây cháy còn lại).")]
        public float fuelSeconds = 180f;
        [Tooltip("Tối đa nhiên liệu lửa có thể giữ.")]
        public float maxFuelSeconds = 600f;
        [Tooltip("ItemSO 'wood' (gỗ) — mỗi lần tiếp 1 thanh thêm refuelPerWoodSeconds.")]
        public ItemSO woodItem;
        public float refuelPerWoodSeconds = 60f;

        [Header("Visual")]
        public SpriteRenderer flameRenderer;
        public SpriteRenderer auraRenderer;
        public Color auraColor = new Color(1f, 0.55f, 0.15f, 0.35f);

        public bool IsLit => fuelSeconds > 0f;

        public string InteractLabel => IsLit ? "Tiếp lửa (gỗ)" : "Nhóm lửa (gỗ)";

        static readonly List<Campfire> active = new();
        public static IReadOnlyList<Campfire> Active => active;

        void Awake()
        {
            if (auraRenderer != null) auraRenderer.color = auraColor;
        }

        void OnEnable() { if (!active.Contains(this)) active.Add(this); }
        void OnDisable() { active.Remove(this); }

        /// <summary>
        /// Trả về Campfire đang cháy gần <paramref name="worldPos"/> nhất nằm trong aura, hoặc null.
        /// </summary>
        public static Campfire FindWarmthAt(Vector3 worldPos)
        {
            Campfire best = null;
            float bestSqr = float.PositiveInfinity;
            foreach (var c in active)
            {
                if (c == null || !c.IsLit) continue;
                float sqr = ((Vector2)worldPos - (Vector2)c.transform.position).sqrMagnitude;
                if (sqr <= c.warmRadius * c.warmRadius && sqr < bestSqr)
                {
                    bestSqr = sqr;
                    best = c;
                }
            }
            return best;
        }

        void Update()
        {
            float dt = Time.deltaTime;
            if (IsLit)
            {
                fuelSeconds = Mathf.Max(0f, fuelSeconds - dt);
                ApplyAuraToPlayers(dt);
            }
            UpdateVisual();
        }

        void ApplyAuraToPlayers(float dt)
        {
            var hits = Physics2D.OverlapCircleAll(transform.position, warmRadius, playerMask);
            foreach (var h in hits)
            {
                var stats = h.GetComponentInParent<PlayerStats>();
                if (stats == null) continue;
                if (sanityRegenPerSec > 0f) stats.RestoreSanity(sanityRegenPerSec * dt);
            }
        }

        void UpdateVisual()
        {
            float t = Mathf.Clamp01(fuelSeconds / Mathf.Max(1f, maxFuelSeconds * 0.5f));
            if (flameRenderer != null) flameRenderer.enabled = IsLit;
            if (auraRenderer != null)
            {
                auraRenderer.enabled = IsLit;
                if (IsLit)
                {
                    var c = auraColor;
                    c.a = auraColor.a * Mathf.Lerp(0.35f, 1f, t);
                    auraRenderer.color = c;
                    float scale = warmRadius * 2f * Mathf.Lerp(0.7f, 1f, t);
                    auraRenderer.transform.localScale = new Vector3(scale, scale, 1f);
                }
            }
        }

        public bool CanInteract(GameObject actor)
        {
            if (actor == null) return false;
            // Cho phép tiếp gỗ nếu chưa đầy fuel
            return fuelSeconds < maxFuelSeconds && woodItem != null;
        }

        public bool Interact(GameObject actor)
        {
            if (woodItem == null) return false;
            var inv = actor != null ? actor.GetComponentInParent<Inventory>() : null;
            if (inv == null || !inv.TryConsume(woodItem, 1)) return false;
            fuelSeconds = Mathf.Min(maxFuelSeconds, fuelSeconds + refuelPerWoodSeconds);
            return true;
        }

        /// <summary>True nếu vị trí <paramref name="worldPos"/> nằm trong aura sưởi ấm của lửa đang cháy.</summary>
        public bool IsWithinWarmth(Vector3 worldPos)
        {
            if (!IsLit) return false;
            return ((Vector2)worldPos - (Vector2)transform.position).sqrMagnitude <= warmRadius * warmRadius;
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.5f, 0.1f, 0.6f);
            Gizmos.DrawWireSphere(transform.position, warmRadius);
        }
    }
}
