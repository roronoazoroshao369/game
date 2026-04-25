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

        [Header("Consumable effects (nếu là food/drink/consumable)")]
        public float restoreHunger;
        public float restoreThirst;
        public float restoreHP;
        public float restoreSanity;
        public float restoreMana;

        [Header("Tool/Weapon")]
        public float toolPower = 0f;     // dùng để chặt cây / đập đá nhanh hơn
        public float weaponDamage = 0f;
    }
}
