using System;
using UnityEngine;
using WildernessCultivation.Items;

namespace WildernessCultivation.Crafting
{
    [Serializable]
    public struct RecipeIngredient
    {
        public ItemSO item;
        public int count;
    }

    /// <summary>
    /// Công thức chế tạo. MVP: cần in-range của trạm chế tạo phù hợp (vd lửa trại để nướng thịt).
    /// </summary>
    [CreateAssetMenu(menuName = "WildernessCultivation/Recipe", fileName = "Recipe_New")]
    public class RecipeSO : ScriptableObject
    {
        public string recipeId;
        public string displayName;
        public Sprite icon;

        [Header("Output")]
        public ItemSO output;
        public int outputCount = 1;

        [Header("Inputs")]
        public RecipeIngredient[] ingredients;

        [Header("Yêu cầu trạm (tùy chọn)")]
        public CraftStation requiredStation = CraftStation.None;
    }

    public enum CraftStation { None, Campfire, AlchemyFurnace, Workbench }
}
