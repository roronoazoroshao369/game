using UnityEngine;
using UnityEngine.UI;
using WildernessCultivation.Core;
using WildernessCultivation.Cultivation;

namespace WildernessCultivation.UI
{
    /// <summary>
    /// Badge góc màn hình hiển thị: ngày sinh tồn, mùa, weather, linh căn.
    /// Dùng TMPro hoặc UnityEngine.UI.Text — chọn 1 cái cấu hình; cái còn lại để null.
    /// </summary>
    public class EnvironmentBadgeUI : MonoBehaviour
    {
        public TimeManager timeManager;
        public SpiritRoot spiritRoot;

        [Header("Optional Text (UnityEngine.UI.Text)")]
        public Text dayText;
        public Text seasonText;
        public Text weatherText;
        public Text spiritRootText;

        [Header("Optional icons")]
        public Image seasonIcon;
        public Sprite springIcon, summerIcon, autumnIcon, winterIcon;
        public Image weatherIcon;
        public Sprite clearIcon, rainIcon, stormIcon;
        public Image spiritRootIcon;

        void Start()
        {
            if (timeManager == null) timeManager = ServiceLocator.Get<TimeManager>();
            if (spiritRoot == null) spiritRoot = ServiceLocator.Get<SpiritRoot>();
            Refresh();
        }

        void Update() { Refresh(); }

        void Refresh()
        {
            if (timeManager != null)
            {
                if (dayText != null) dayText.text = $"Ngày {timeManager.daysSurvived}";
                if (seasonText != null) seasonText.text = SeasonName(timeManager.currentSeason);
                if (weatherText != null) weatherText.text = WeatherName(timeManager.currentWeather);
                if (seasonIcon != null) seasonIcon.sprite = SeasonIcon(timeManager.currentSeason);
                if (weatherIcon != null) weatherIcon.sprite = WeatherIcon(timeManager.currentWeather);
            }
            if (spiritRoot != null && spiritRoot.Current != null)
            {
                if (spiritRootText != null) spiritRootText.text = spiritRoot.Current.displayName;
                if (spiritRootIcon != null)
                {
                    spiritRootIcon.sprite = spiritRoot.Current.icon;
                    spiritRootIcon.color = spiritRoot.Current.tintColor;
                }
            }
        }

        static string SeasonName(Season s) => s switch
        {
            Season.Spring => "Xuân",
            Season.Summer => "Hạ",
            Season.Autumn => "Thu",
            Season.Winter => "Đông",
            _ => s.ToString(),
        };

        Sprite SeasonIcon(Season s) => s switch
        {
            Season.Spring => springIcon,
            Season.Summer => summerIcon,
            Season.Autumn => autumnIcon,
            Season.Winter => winterIcon,
            _ => null,
        };

        static string WeatherName(Weather w) => w switch
        {
            Weather.Clear => "Trời quang",
            Weather.Rain => "Mưa",
            Weather.Storm => "Bão",
            _ => w.ToString(),
        };

        Sprite WeatherIcon(Weather w) => w switch
        {
            Weather.Clear => clearIcon,
            Weather.Rain => rainIcon,
            Weather.Storm => stormIcon,
            _ => null,
        };
    }
}
