using UnityEngine;
using UnityEngine.UI;
using WildernessCultivation.Player;

namespace WildernessCultivation.UI
{
    /// <summary>
    /// Hiển thị 5 thanh stat: HP / Đói / Khát / SAN / Linh Khí.
    /// </summary>
    public class StatBarUI : MonoBehaviour
    {
        public PlayerStats stats;

        [Header("Fill Images")]
        public Image hpFill;
        public Image hungerFill;
        public Image thirstFill;
        public Image sanityFill;
        public Image manaFill;

        void Start()
        {
            if (stats == null) stats = FindObjectOfType<PlayerStats>();
            if (stats != null) stats.OnStatsChanged += Refresh;
            Refresh();
        }

        void OnDestroy()
        {
            if (stats != null) stats.OnStatsChanged -= Refresh;
        }

        void Refresh()
        {
            if (stats == null) return;
            if (hpFill != null) hpFill.fillAmount = stats.HP / Mathf.Max(1f, stats.maxHP);
            if (hungerFill != null) hungerFill.fillAmount = stats.Hunger / Mathf.Max(1f, stats.maxHunger);
            if (thirstFill != null) thirstFill.fillAmount = stats.Thirst / Mathf.Max(1f, stats.maxThirst);
            if (sanityFill != null) sanityFill.fillAmount = stats.Sanity / Mathf.Max(1f, stats.maxSanity);
            if (manaFill != null) manaFill.fillAmount = stats.Mana / Mathf.Max(1f, stats.maxMana);
        }
    }
}
