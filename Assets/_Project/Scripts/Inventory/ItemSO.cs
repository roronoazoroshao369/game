using UnityEngine;

namespace WildernessCultivation.Items
{
    public enum ItemCategory { Resource, Food, Drink, Tool, Weapon, Equipment, Consumable, Material, SpiritHerb }

    /// <summary>
    /// Định nghĩa item. Tạo asset: Right-click > Create > WildernessCultivation > Item.
    /// </summary>
    [CreateAssetMenu(menuName = "WildernessCultivation/Item", fileName = "Item_New")]
    public class ItemSO : ScriptableObject
    {
        [Header("Identity")]
        public string itemId;        // unique, ổn định cho save (vd "wood", "raw_meat")
        public string displayName;
        [TextArea] public string description;
        public Sprite icon;

        [Header("Stack")]
        public int maxStack = 99;
        public ItemCategory category;
        [Tooltip("Khối lượng / kg per 1 đơn vị. Inventory.TotalWeight cộng dồn weight*count cho encumbrance.")]
        public float weight = 1f;

        [Header("Consumable effects (nếu là food/drink/consumable)")]
        public float restoreHunger;
        public float restoreThirst;
        public float restoreHP;
        public float restoreSanity;
        public float restoreMana;

        [Header("Tool/Weapon")]
        public float toolPower = 0f;     // dùng để chặt cây / đập đá nhanh hơn
        public float weaponDamage = 0f;

        [Header("Durability (cho tool/weapon)")]
        [Tooltip("True = item có độ bền, hỏng khi durability = 0.")]
        public bool hasDurability = false;
        [Tooltip("Độ bền tối đa khi mới tạo / craft.")]
        public float maxDurability = 50f;
        [Tooltip("Lượng durability mất mỗi lần dùng (1 đòn melee, 1 lần chop, …).")]
        public float durabilityPerUse = 1f;

        [Header("Food spoilage (cho food/drink/consumable)")]
        [Tooltip("True = item bị hỏng theo thời gian (thịt sống nhanh, đồ nướng chậm).")]
        public bool isPerishable = false;
        [Tooltip("Tổng giây tươi trước khi hỏng. 600 = 10 phút real-time.")]
        public float freshSeconds = 600f;
        [Tooltip("Khi hỏng, restore* và sanity penalty áp dụng.")]
        public float spoiledRestoreMultiplier = 0.5f;
        public float spoiledSanityPenalty = 8f;
    }
}
