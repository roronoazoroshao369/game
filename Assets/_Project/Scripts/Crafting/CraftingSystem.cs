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

        // Track recipe đang nấu để deliver output kể cả khi GameObject bị disable mid-cook.
        readonly List<RecipeSO> pendingCooks = new();

        void Awake() { inv = GetComponent<Inventory>(); }

        // Lazy-resolve so CanCraft / TryCraft remain safe if a UI component's
        // OnEnable on a different GameObject runs before our Awake.
        Inventory Inv => inv != null ? inv : (inv = GetComponent<Inventory>());

        void OnDisable()
        {
            // Tránh mất output: nếu bị disable / destroy giữa chừng → deliver ngay tất cả cook đang chờ.
            // StopAllCoroutines trước khi fast-deliver để tránh duplicate khi chỉ component (không phải GameObject) bị disable —
            // coroutine vẫn chạy tiếp đến khi GameObject inactive, sẽ Add output lần 2.
            StopAllCoroutines();
            if (pendingCooks.Count == 0 || inv == null) return;
            foreach (var recipe in pendingCooks)
            {
                if (recipe == null || recipe.output == null) continue;
                int leftover = inv.Add(recipe.output, recipe.outputCount);
                if (leftover > 0)
                    Debug.LogWarning($"[Craft] OnDisable fast-deliver {recipe.output.displayName}: inventory đầy, {leftover} bị mất.");
                else
                    Debug.Log($"[Craft] OnDisable fast-deliver {recipe.output.displayName} x{recipe.outputCount}");
            }
            pendingCooks.Clear();
        }

        public bool CanCraft(RecipeSO recipe)
        {
            if (recipe == null) return false;
            var i = Inv;
            if (i == null) return false;
            if (recipe.requiredStation != CraftStation.None && !IsStationInRange(recipe.requiredStation))
                return false;

            foreach (var ing in recipe.ingredients)
                if (i.CountOf(ing.item) < ing.count) return false;
            return true;
        }

        public bool TryCraft(RecipeSO recipe)
        {
            if (!CanCraft(recipe)) return false;
            var i = Inv;
            if (i == null) return false;

            foreach (var ing in recipe.ingredients)
                i.TryConsume(ing.item, ing.count);

            // Có cookTime → defer việc Add output bằng coroutine (vẫn tiêu liệu ngay để khoá recipe)
            if (recipe.cookTimeSeconds > 0f)
            {
                StartCoroutine(DeferredCookCoroutine(recipe));
                Debug.Log($"[Craft] Bắt đầu nấu/chế {recipe.output.displayName} (chờ {recipe.cookTimeSeconds}s)");
                return true;
            }

            int leftover = i.Add(recipe.output, recipe.outputCount);
            if (leftover > 0)
                Debug.LogWarning($"[Craft] Inventory full, {leftover}x {recipe.output.displayName} bị mất.");

            Debug.Log($"[Craft] Đã chế: {recipe.output.displayName} x{recipe.outputCount}");
            return true;
        }

        System.Collections.IEnumerator DeferredCookCoroutine(RecipeSO recipe)
        {
            pendingCooks.Add(recipe);
            // Realtime để Sleep (Time.timeScale x8) không bypass cook timer
            yield return new WaitForSecondsRealtime(recipe.cookTimeSeconds);
            pendingCooks.Remove(recipe);
            var i = Inv;
            if (i == null) yield break;
            int leftover = i.Add(recipe.output, recipe.outputCount);
            if (leftover > 0)
                Debug.LogWarning($"[Craft] Inventory full, {leftover}x {recipe.output.displayName} bị mất.");
            Debug.Log($"[Craft] Hoàn thành: {recipe.output.displayName} x{recipe.outputCount}");
        }

        bool IsStationInRange(CraftStation station)
        {
            var hits = Physics2D.OverlapCircleAll(transform.position, stationDetectRadius, stationMask);
            foreach (var h in hits)
            {
                var s = h.GetComponent<CraftStationMarker>();
                if (s != null && s.station == station && s.IsAvailable) return true;
            }
            return false;
        }
    }
}
