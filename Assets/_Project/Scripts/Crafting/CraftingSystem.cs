using System.Collections.Generic;
using UnityEngine;
using WildernessCultivation.Items;

namespace WildernessCultivation.Crafting
{
    /// <summary>
    /// Trung tâm chế tạo. Gắn vào Player.
    /// </summary>
    [RequireComponent(typeof(Inventory))]
    public class CraftingSystem : MonoBehaviour
    {
        [Header("Recipes")]
        public List<RecipeSO> knownRecipes = new();

        [Header("Detect station")]
        public float stationDetectRadius = 2f;
        public LayerMask stationMask;

        Inventory inv;

        void Awake() { inv = GetComponent<Inventory>(); }

        public bool CanCraft(RecipeSO recipe)
        {
            if (recipe == null) return false;
            if (recipe.requiredStation != CraftStation.None && !IsStationInRange(recipe.requiredStation))
                return false;

            foreach (var ing in recipe.ingredients)
                if (inv.CountOf(ing.item) < ing.count) return false;
            return true;
        }

        public bool TryCraft(RecipeSO recipe)
        {
            if (!CanCraft(recipe)) return false;

            foreach (var ing in recipe.ingredients)
                inv.TryConsume(ing.item, ing.count);

            int leftover = inv.Add(recipe.output, recipe.outputCount);
            if (leftover > 0)
                Debug.LogWarning($"[Craft] Inventory full, {leftover}x {recipe.output.displayName} bị mất.");

            Debug.Log($"[Craft] Đã chế: {recipe.output.displayName} x{recipe.outputCount}");
            return true;
        }

        bool IsStationInRange(CraftStation station)
        {
            var hits = Physics2D.OverlapCircleAll(transform.position, stationDetectRadius, stationMask);
            foreach (var h in hits)
            {
                var s = h.GetComponent<CraftStationMarker>();
                if (s != null && s.station == station) return true;
            }
            return false;
        }
    }

    /// <summary>Gắn vào prefab lửa trại / lò luyện đan để CraftingSystem detect được.</summary>
    public class CraftStationMarker : MonoBehaviour
    {
        public CraftStation station = CraftStation.Campfire;
    }
}
