using UnityEngine;
using WildernessCultivation.Core;
using WildernessCultivation.Items;

namespace WildernessCultivation.World
{
    /// <summary>
    /// Ô đất trồng linh thảo / thảo dược. Vòng đời:
    ///   Empty → Planted (tốn 1 seed) → Watered (tốn N nước) → đợi growDays → Grown → Harvest (drop 1 herb).
    /// Tính ngày qua TimeManager.daysSurvived để không phụ thuộc framerate.
    /// </summary>
    public class PlantNode : MonoBehaviour, IInteractable
    {
        public ItemSO seedItem;
        public ItemSO waterBucketItem;     // Optional: nếu có item nước trong inventory thì tưới ngay
        public ItemSO harvestItem;
        public int harvestCount = 1;
        public int growDays = 3;
        public int waterNeeded = 2;

        [Header("Visual stages")]
        public SpriteRenderer renderer2d;
        public Sprite spriteEmpty;
        public Sprite spritePlanted;
        public Sprite spriteWatered;
        public Sprite spriteGrown;

        public int dayPlanted = -1;
        public int waterCount = 0;
        public bool planted => dayPlanted >= 0;

        TimeManager Time => GameManager.Instance != null ? GameManager.Instance.timeManager : null;

        public bool IsGrown
        {
            get
            {
                if (!planted) return false;
                if (Time == null) return false;
                int days = Time.daysSurvived - dayPlanted;
                return days >= growDays && waterCount >= waterNeeded;
            }
        }

        public string InteractLabel
        {
            get
            {
                if (!planted) return seedItem != null ? $"Trồng {seedItem.displayName}" : "Trồng (thiếu hạt)";
                if (IsGrown) return harvestItem != null ? $"Thu hoạch {harvestItem.displayName}" : "Thu hoạch";
                return waterCount < waterNeeded ? "Tưới nước" : "Đang lớn…";
            }
        }

        public bool CanInteract(GameObject actor) => actor != null;

        public bool Interact(GameObject actor)
        {
            var inv = actor.GetComponentInParent<Inventory>();
            if (inv == null) return false;

            if (!planted)
            {
                if (seedItem == null || !inv.TryConsume(seedItem, 1)) return false;
                dayPlanted = Time != null ? Time.daysSurvived : 0;
                waterCount = 0;
                UpdateSprite();
                return true;
            }

            if (IsGrown)
            {
                if (harvestItem != null) inv.Add(harvestItem, harvestCount);
                dayPlanted = -1;
                waterCount = 0;
                UpdateSprite();
                return true;
            }

            // Cần tưới
            if (waterCount < waterNeeded)
            {
                bool didWater = false;
                if (waterBucketItem != null && inv.TryConsume(waterBucketItem, 1)) didWater = true;
                else if (WaterSpring.AnyWaterSpringNear(transform.position, 2f)) didWater = true;
                if (didWater) { waterCount++; UpdateSprite(); return true; }
            }
            return false;
        }

        void UpdateSprite()
        {
            if (renderer2d == null) return;
            if (!planted) renderer2d.sprite = spriteEmpty;
            else if (IsGrown) renderer2d.sprite = spriteGrown;
            else if (waterCount > 0) renderer2d.sprite = spriteWatered;
            else renderer2d.sprite = spritePlanted;
        }
    }
}
