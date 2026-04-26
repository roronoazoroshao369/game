using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace WildernessCultivation.UI
{
    /// <summary>
    /// HUD onboarding cho demo MVP. Gồm 3 panel:
    ///
    ///   - Welcome panel: hiển thị 1 lần khi Play, cheat sheet điều khiển + "Bắt đầu demo" dismiss.
    ///   - Objectives panel: checklist 5 mục tiêu (bám <see cref="DemoObjectivesTracker"/>) luôn hiện.
    ///   - Victory banner: hiển thị khi <see cref="DemoObjectivesTracker.AllDone"/> = true.
    ///
    /// Chỉ phụ thuộc UGUI + TMP — không prefab riêng (BootstrapWizard dựng runtime).
    /// </summary>
    public class TutorialHUD : MonoBehaviour
    {
        [Header("Refs")]
        public DemoObjectivesTracker tracker;

        [Header("Welcome panel")]
        public GameObject welcomePanel;
        public Button welcomeDismissButton;
        public TMP_Text welcomeBodyText;

        [Header("Objectives panel")]
        public TMP_Text objectivesListText;

        [Header("Victory banner")]
        public GameObject victoryPanel;
        public TMP_Text victoryText;
        public Button victoryDismissButton;

        [Header("Content")]
        [TextArea(3, 10)]
        public string welcomeMessage =
            "Hoang Vực Tu Tiên Ký — Demo MVP\n" +
            "\n" +
            "Điều khiển:\n" +
            "  • WASD / Joystick: di chuyển\n" +
            "  • J: đánh thường    K: Kiếm Khí Trảm\n" +
            "  • Shift: né          M: Tụ Linh Quyết (thiền)\n" +
            "  • E: tương tác       T: đuốc        Z: ngủ\n" +
            "\n" +
            "Mục tiêu: chặt gỗ, đốt lửa trại, săn thỏ, nướng thịt, ngủ qua đêm, thiền, đột phá Luyện Khí Tầng 2.";

        [Header("Tuỳ chọn")]
        public bool showWelcomeOnStart = true;

        void Awake()
        {
            if (tracker == null) tracker = FindObjectOfType<DemoObjectivesTracker>();
            if (welcomeBodyText != null) welcomeBodyText.text = welcomeMessage;
            if (welcomePanel != null) welcomePanel.SetActive(showWelcomeOnStart);
            if (victoryPanel != null) victoryPanel.SetActive(false);
            if (welcomeDismissButton != null) welcomeDismissButton.onClick.AddListener(DismissWelcome);
            if (victoryDismissButton != null) victoryDismissButton.onClick.AddListener(DismissVictory);
        }

        void OnEnable()
        {
            if (tracker != null)
            {
                tracker.OnObjectiveCompleted += OnObjectiveCompleted;
                tracker.OnAllObjectivesCompleted += OnAllDone;
            }
            RefreshObjectives();
        }

        void OnDisable()
        {
            if (tracker != null)
            {
                tracker.OnObjectiveCompleted -= OnObjectiveCompleted;
                tracker.OnAllObjectivesCompleted -= OnAllDone;
            }
        }

        public void DismissWelcome()
        {
            if (welcomePanel != null) welcomePanel.SetActive(false);
        }

        public void DismissVictory()
        {
            if (victoryPanel != null) victoryPanel.SetActive(false);
        }

        void OnObjectiveCompleted(DemoObjectivesTracker.Objective _) => RefreshObjectives();

        void OnAllDone()
        {
            if (victoryPanel == null) return;
            if (victoryText != null)
                victoryText.text = "MVP Demo hoàn thành!\nBạn đã đột phá Luyện Khí Tầng 2 — cốt lõi loop chạy ổn.";
            victoryPanel.SetActive(true);
        }

        void RefreshObjectives()
        {
            if (objectivesListText == null || tracker == null) return;
            var sb = new StringBuilder();
            sb.AppendLine("<b>Mục tiêu demo</b>");
            var objs = (DemoObjectivesTracker.Objective[])System.Enum.GetValues(typeof(DemoObjectivesTracker.Objective));
            for (int i = 0; i < objs.Length; i++)
            {
                bool done = i < tracker.Completed.Length && tracker.Completed[i];
                string mark = done ? "[x]" : "[ ]";
                string label = DemoObjectivesTracker.Label(objs[i]);
                if (done) sb.AppendLine($"<color=#6aff6a>{mark} {label}</color>");
                else sb.AppendLine($"{mark} {label}");
            }
            objectivesListText.text = sb.ToString();
        }
    }
}
