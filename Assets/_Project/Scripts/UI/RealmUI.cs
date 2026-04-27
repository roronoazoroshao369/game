using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WildernessCultivation.Cultivation;
using WildernessCultivation.Player;

namespace WildernessCultivation.UI
{
    /// <summary>
    /// Hiển thị cảnh giới + XP bar + nút đột phá. Khi player chưa khai mở
    /// (<see cref="PlayerStats.IsAwakened"/> = false) — hiển thị "Thường Nhân" và
    /// disable nút đột phá.
    /// </summary>
    public class RealmUI : MonoBehaviour
    {
        public RealmSystem realm;
        public PlayerStats playerStats;
        public TMP_Text realmLabel;
        public Image xpFill;
        public Button breakthroughButton;
        public TMP_Text breakthroughResultLabel;

        void Start()
        {
            if (realm == null) realm = FindObjectOfType<RealmSystem>();
            if (playerStats == null) playerStats = FindObjectOfType<PlayerStats>();
            if (realm == null) return;

            realm.OnRealmAdvanced += _ => Refresh();
            realm.OnBreakthroughAttempted += success =>
            {
                if (breakthroughResultLabel != null)
                    breakthroughResultLabel.text = success ? "Đột phá thành công!" : "Đột phá thất bại — mất SAN.";
                Refresh();
            };

            if (breakthroughButton != null)
                breakthroughButton.onClick.AddListener(() =>
                {
                    if (playerStats != null && !playerStats.IsAwakened) return;
                    realm.TryBreakthrough();
                });

            Refresh();
        }

        void Update() { Refresh(); }

        void Refresh()
        {
            if (realm == null) return;
            bool awakened = playerStats == null || playerStats.IsAwakened;
            if (realmLabel != null)
                realmLabel.text = awakened ? realm.Current.name : "Thường Nhân";
            if (xpFill != null)
            {
                if (awakened && realm.HasNext)
                {
                    float req = Mathf.Max(1f, realm.EffectiveNextXpRequired);
                    xpFill.fillAmount = Mathf.Clamp01(realm.currentXp / req);
                }
                else
                {
                    xpFill.fillAmount = 0f;
                }
            }
            if (breakthroughButton != null)
            {
                bool canBreak = awakened && realm.HasNext && realm.currentXp >= realm.EffectiveNextXpRequired;
                breakthroughButton.interactable = canBreak;
            }
        }
    }
}
