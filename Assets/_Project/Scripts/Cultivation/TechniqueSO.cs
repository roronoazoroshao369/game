using UnityEngine;
using WildernessCultivation.Player;

namespace WildernessCultivation.Cultivation
{
    /// <summary>
    /// Base class cho công pháp (skill). Tạo asset: Right-click > Create > Cultivation > ...
    /// </summary>
    public abstract class TechniqueSO : ScriptableObject
    {
        [Header("Common")]
        public string techniqueId;
        public string displayName;
        [TextArea] public string description;
        public Sprite icon;

        [Header("Cost & cooldown")]
        public float manaCost = 20f;
        public float cooldown = 1.5f;

        public abstract void Cast(PlayerCombat caster);
    }
}
