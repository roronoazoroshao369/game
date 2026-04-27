using System.Collections.Generic;
using UnityEngine;
using WildernessCultivation.Items;
using WildernessCultivation.Player;

namespace WildernessCultivation.World
{
    /// <summary>
    /// Suối / vũng nước — người chơi đứng cạnh và bấm tương tác để uống tại chỗ
    /// (hồi <see cref="PlayerStats.Thirst"/>). Có cooldown nhỏ giữa các ngụm.
    /// Tuỳ chọn: cho rớt 1 <see cref="cleanWaterItem"/> vào inventory để mang đi.
    /// </summary>
    public class WaterSpring : MonoBehaviour, IInteractable
    {
        static readonly List<WaterSpring> active = new();
        void OnEnable() { if (!active.Contains(this)) active.Add(this); }
        void OnDisable() { active.Remove(this); }

        public static bool AnyWaterSpringNear(Vector3 pos, float radius)
        {
            float r2 = radius * radius;
            foreach (var w in active)
                if (w != null && ((Vector2)pos - (Vector2)w.transform.position).sqrMagnitude <= r2) return true;
            return false;
        }

        [Header("Drink")]
        [Tooltip("Lượng Khát hồi mỗi lần uống tại chỗ.")]
        public float thirstPerDrink = 25f;
        [Tooltip("Giây giữa các lần uống.")]
        public float drinkCooldown = 1f;
        [Tooltip("Splash wetness mỗi lần uống tại suối (cộng vào PlayerStats.Wetness).")]
        public float wetnessSplashOnDrink = 8f;

        [Header("Bottle (optional)")]
        [Tooltip("Nếu set, mỗi lần uống cũng cho 1 đơn vị item này vào inventory (gọi là 'Bình nước' chẳng hạn).")]
        public ItemSO cleanWaterItem;
        public bool dispenseBottleOnDrink = false;

        [Header("Effects")]
        public ParticleSystem rippleVfx;

        float nextDrinkAt;

        public string InteractLabel => "Uống nước";

        public bool CanInteract(GameObject actor)
        {
            if (Time.time < nextDrinkAt) return false;
            return actor != null && actor.GetComponentInParent<PlayerStats>() != null;
        }

        public bool Interact(GameObject actor)
        {
            if (Time.time < nextDrinkAt) return false;
            var stats = actor != null ? actor.GetComponentInParent<PlayerStats>() : null;
            if (stats == null) return false;

            stats.Drink(thirstPerDrink);
            if (wetnessSplashOnDrink > 0f) stats.AddWetness(wetnessSplashOnDrink);
            nextDrinkAt = Time.time + drinkCooldown;

            if (dispenseBottleOnDrink && cleanWaterItem != null)
            {
                var inv = actor.GetComponentInParent<Inventory>();
                if (inv != null) inv.Add(cleanWaterItem, 1);
            }

            if (rippleVfx != null) rippleVfx.Play();
            return true;
        }
    }
}
