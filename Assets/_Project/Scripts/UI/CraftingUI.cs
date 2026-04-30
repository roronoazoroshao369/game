using System.Collections.Generic;
using WildernessCultivation.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WildernessCultivation.Crafting;

namespace WildernessCultivation.UI
{
    /// <summary>
    /// List recipe; bấm để craft.
    /// </summary>
    public class CraftingUI : MonoBehaviour
    {
        public CraftingSystem craftingSystem;
        public GameObject recipeButtonPrefab;
        public Transform listParent;

        readonly List<GameObject> spawned = new();

        void OnEnable()
        {
            if (craftingSystem == null) craftingSystem = ServiceLocator.Get<CraftingSystem>();
            BuildList();
        }

        void OnDisable()
        {
            foreach (var g in spawned) if (g != null) Destroy(g);
            spawned.Clear();
        }

        void BuildList()
        {
            if (craftingSystem == null) return;
            foreach (var r in craftingSystem.knownRecipes)
            {
                var go = Instantiate(recipeButtonPrefab, listParent);
                spawned.Add(go);

                var label = go.GetComponentInChildren<TMP_Text>();
                if (label != null) label.text = r.displayName;

                var icon = go.GetComponentInChildren<Image>();
                if (icon != null && r.icon != null) icon.sprite = r.icon;

                var btn = go.GetComponent<Button>();
                if (btn != null)
                {
                    var captured = r;
                    btn.onClick.AddListener(() => craftingSystem.TryCraft(captured));
                    btn.interactable = craftingSystem.CanCraft(r);
                }
            }
        }
    }
}
