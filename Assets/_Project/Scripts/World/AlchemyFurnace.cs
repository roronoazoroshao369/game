using UnityEngine;
using WildernessCultivation.Crafting;
using WildernessCultivation.Items;
using WildernessCultivation.Player;

namespace WildernessCultivation.World
{
    /// <summary>
    /// Lò luyện đan — đặt trong scene để player luyện đan dược qua <see cref="CraftingSystem"/>
    /// (RecipeSO với <c>requiredStation = AlchemyFurnace</c>). Có nhiên liệu (gỗ) và thanh tiến độ
    /// chung khi đang luyện. Khác Campfire ở chỗ KHÔNG cấp aura SAN — chỉ là craft station chuyên dụng.
    /// </summary>
    [RequireComponent(typeof(CraftStationMarker))]
    public class AlchemyFurnace : MonoBehaviour, IInteractable, IStationGate
    {
        public bool StationActive => IsLit;

        [Header("Fuel")]
        public float fuelSeconds = 0f;
        public float maxFuelSeconds = 600f;
        public ItemSO woodItem;
        public float refuelPerWoodSeconds = 60f;

        [Header("Visual")]
        public SpriteRenderer flameRenderer;
        public Color litColor = new Color(0.9f, 0.5f, 0.2f, 1f);
        public Color unlitColor = new Color(0.4f, 0.4f, 0.4f, 1f);

        public bool IsLit => fuelSeconds > 0f;
        public string InteractLabel => IsLit ? "Tiếp lửa luyện đan (gỗ)" : "Nhóm lò luyện đan (gỗ)";

        void Awake()
        {
            var marker = GetComponent<CraftStationMarker>();
            if (marker != null) marker.station = CraftStation.AlchemyFurnace;
        }

        void Update()
        {
            if (IsLit) fuelSeconds = Mathf.Max(0f, fuelSeconds - Time.deltaTime);
            UpdateVisual();
        }

        void UpdateVisual()
        {
            if (flameRenderer != null)
                flameRenderer.color = IsLit ? litColor : unlitColor;
        }

        public bool CanInteract(GameObject actor)
        {
            if (actor == null || woodItem == null) return false;
            if (fuelSeconds >= maxFuelSeconds) return false;
            var inv = actor.GetComponent<Inventory>() ?? actor.GetComponentInParent<Inventory>();
            return inv != null && inv.CountOf(woodItem) > 0;
        }

        public bool Interact(GameObject actor)
        {
            if (actor == null) return false;
            var inv = actor.GetComponent<Inventory>() ?? actor.GetComponentInParent<Inventory>();
            if (inv == null || woodItem == null) return false;
            if (!inv.TryConsume(woodItem, 1)) return false;

            fuelSeconds = Mathf.Min(maxFuelSeconds, fuelSeconds + refuelPerWoodSeconds);
            Debug.Log($"[AlchemyFurnace] +{refuelPerWoodSeconds}s fuel (now {fuelSeconds:F0}/{maxFuelSeconds}).");
            return true;
        }
    }
}
