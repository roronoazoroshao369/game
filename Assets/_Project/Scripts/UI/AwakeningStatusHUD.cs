using TMPro;
using UnityEngine;
using WildernessCultivation.Core;
using WildernessCultivation.Cultivation;
using WildernessCultivation.Player;

namespace WildernessCultivation.UI
{
    /// <summary>
    /// HUD panel hiển thị status awakening cho Thường Nhân — guide player biết
    /// còn bao xa mới đủ duyên + reason đang chặn (cần thêm ngày / HP yếu / sanity thấp /
    /// đủ điều kiện thì kêu tìm kì ngộ).
    ///
    /// Khi <see cref="PlayerStats.IsAwakened"/> = true → ẩn panel toàn bộ (đã khai mở,
    /// HUD này không còn vai trò).
    /// </summary>
    public class AwakeningStatusHUD : MonoBehaviour
    {
        [Header("Refs (auto-find if null)")]
        public PlayerStats playerStats;
        public AwakeningSystem awakening;
        public TimeManager timeManager;

        [Header("UI")]
        public TMP_Text statusText;
        public GameObject panelRoot;

        [Header("Refresh")]
        [Tooltip("Update interval (giây). Status không cần update mỗi frame — poll nhẹ là đủ.")]
        public float pollIntervalSeconds = 0.5f;

        float nextPollAt;

        void Awake()
        {
            if (playerStats == null) playerStats = ServiceLocator.Get<PlayerStats>();
            if (awakening == null) awakening = ServiceLocator.Get<AwakeningSystem>();
            if (timeManager == null) timeManager = GameManager.Instance != null ? GameManager.Instance.timeManager : ServiceLocator.Get<TimeManager>();
            if (panelRoot == null) panelRoot = gameObject;
        }

        void Update()
        {
            if (Time.unscaledTime < nextPollAt) return;
            nextPollAt = Time.unscaledTime + Mathf.Max(0.05f, pollIntervalSeconds);
            Refresh();
        }

        public void Refresh()
        {
            if (playerStats == null || awakening == null)
            {
                if (panelRoot != null) panelRoot.SetActive(false);
                return;
            }
            if (playerStats.IsAwakened)
            {
                if (panelRoot != null) panelRoot.SetActive(false);
                return;
            }
            if (panelRoot != null) panelRoot.SetActive(true);
            if (statusText != null) statusText.text = BuildStatusLine();
        }

        /// <summary>Compose 2-line status — public để test khỏi cần TMP wired.</summary>
        public string BuildStatusLine()
        {
            int days = timeManager != null ? timeManager.daysSurvived : 0;
            int minDays = awakening != null && awakening.config != null ? awakening.config.minDaysSurvived : 7;
            string header = $"Thường Nhân — Ngày {days}/{minDays}";

            var eligibility = awakening != null ? awakening.CheckEligibility() : AwakenEligibility.NoPlayer;
            string reason = eligibility switch
            {
                AwakenEligibility.Eligible => "<color=#7CFC00>Đủ duyên — tìm Linh Tuyền hoặc Linh Quả</color>",
                AwakenEligibility.NotEnoughDays => $"Sống sót thêm {Mathf.Max(0, minDays - days)} ngày",
                AwakenEligibility.LowHP => "Thân thể quá yếu — hồi máu trước",
                AwakenEligibility.LowSanity => "Tâm trí bất ổn — nghỉ ngơi để ổn định",
                AwakenEligibility.AlreadyAwakened => "",
                AwakenEligibility.NoPlayer => "",
                _ => "",
            };
            return header + "\n" + reason;
        }
    }
}
