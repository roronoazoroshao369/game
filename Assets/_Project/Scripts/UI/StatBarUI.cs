using UnityEngine;
using UnityEngine.UI;
using WildernessCultivation.Items;
using WildernessCultivation.Player;

namespace WildernessCultivation.UI
{
    /// <summary>
    /// Hiển thị 5 thanh stat: HP / Đói / Khát / SAN / Linh Khí.
    /// </summary>
    public class StatBarUI : MonoBehaviour
    {
        public PlayerStats stats;
        public PlayerController controller;
        public Inventory inventory;

        [Header("Fill Images")]
        public Image hpFill;
        public Image hungerFill;
        public Image thirstFill;
        public Image sanityFill;
        public Image manaFill;

        [Header("Body temperature")]
        public Image bodyTempFill;
        [Tooltip("Đổi sang màu lạnh khi BodyTemp <= freezeThreshold, đỏ khi >= heatThreshold.")]
        public Color tempColdColor = new(0.45f, 0.7f, 1f, 1f);
        public Color tempComfortColor = new(0.5f, 1f, 0.5f, 1f);
        public Color tempHotColor = new(1f, 0.5f, 0.3f, 1f);

        [Header("Encumbrance")]
        public Image encumbranceFill;
        public Color encumbranceLightColor = new(0.6f, 1f, 0.6f, 1f);
        public Color encumbranceOverColor = new(1f, 0.6f, 0.3f, 1f);

        void Start()
        {
            if (stats == null) stats = FindObjectOfType<PlayerStats>();
            if (controller == null) controller = FindObjectOfType<PlayerController>();
            if (inventory == null) inventory = FindObjectOfType<Inventory>();
            if (stats != null) stats.OnStatsChanged += Refresh;
            Refresh();
        }

        void Update() { Refresh(); }

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

            if (bodyTempFill != null)
            {
                float t01 = Mathf.Clamp01(stats.BodyTemp / 100f);
                bodyTempFill.fillAmount = t01;
                // Dùng EffectiveFreezeThreshold/EffectiveHeatThreshold (đã cộng spirit root delta)
                // để UI khớp với gameplay damage zone (Hoả căn +heatThresholdDelta...).
                float effFreeze = stats.EffectiveFreezeThreshold;
                float effHeat = stats.EffectiveHeatThreshold;
                Color c;
                if (stats.BodyTemp <= effFreeze) c = tempColdColor;
                else if (stats.BodyTemp >= effHeat) c = tempHotColor;
                else if (stats.BodyTemp < stats.comfortMin) c = Color.Lerp(tempColdColor, tempComfortColor, (stats.BodyTemp - effFreeze) / Mathf.Max(0.1f, stats.comfortMin - effFreeze));
                else if (stats.BodyTemp > stats.comfortMax) c = Color.Lerp(tempComfortColor, tempHotColor, (stats.BodyTemp - stats.comfortMax) / Mathf.Max(0.1f, effHeat - stats.comfortMax));
                else c = tempComfortColor;
                bodyTempFill.color = c;
            }

            if (encumbranceFill != null && controller != null && inventory != null)
            {
                float w = inventory.TotalWeight;
                float cap = controller.EffectiveMaxCarryWeight;
                float ratio = cap > 0.001f ? w / cap : 0f;
                encumbranceFill.fillAmount = Mathf.Clamp01(ratio);
                encumbranceFill.color = ratio <= 1f ? encumbranceLightColor : encumbranceOverColor;
            }
        }
    }
}
