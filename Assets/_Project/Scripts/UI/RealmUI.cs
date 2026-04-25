using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WildernessCultivation.Cultivation;

namespace WildernessCultivation.UI
{
    /// <summary>
    /// Hiển thị cảnh giới + XP bar + nút đột phá.
    /// </summary>
    public class RealmUI : MonoBehaviour
    {
        public RealmSystem realm;
        public TMP_Text realmLabel;
        public Image xpFill;
        public Button breakthroughButton;
        public TMP_Text breakthroughResultLabel;

        void Start()
        {
            if (realm == null) realm = FindObjectOfType<RealmSystem>();
            if (realm == null) return;

            realm.OnRealmAdvanced += _ => Refresh();
            realm.OnBreakthroughAttempted += success =>
            {
                if (breakthroughResultLabel != null)
                    breakthroughResultLabel.text = success ? "Đột phá thành công!" : "Đột phá thất bại — mất SAN.";
                Refresh();
            };

            if (breakthroughButton != null)
                breakthroughButton.onClick.AddListener(() => realm.TryBreakthrough());

            Refresh();
        }

        void Update() { Refresh(); }

        void Refresh()
        {
            if (realm == null) return;
            if (realmLabel != null) realmLabel.text = realm.Current.name;
            if (xpFill != null && realm.HasNext)
            {
                float req = Mathf.Max(1f, realm.EffectiveNextXpRequired);
                xpFill.fillAmount = Mathf.Clamp01(realm.currentXp / req);
            }
            if (breakthroughButton != null && realm.HasNext)
                breakthroughButton.interactable = realm.currentXp >= realm.EffectiveNextXpRequired;
        }
    }
}
