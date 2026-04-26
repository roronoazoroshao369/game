using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using WildernessCultivation.Audio;
using WildernessCultivation.Core;

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

        [Header("Welcome panel (đóng vai trò main menu)")]
        public GameObject welcomePanel;
        public Button welcomeDismissButton;   // "Bắt đầu mới" — xoá save cũ + reload scene
        public Button continueButton;         // "Tiếp tục" — chỉ hiện khi có save; đóng overlay (save đã auto-load trong SaveLoadController.Start)
        public Button quitButton;             // "Thoát Demo"
        public TMP_Text welcomeBodyText;

        [Header("Objectives panel")]
        public TMP_Text objectivesListText;

        [Header("Victory banner")]
        public GameObject victoryPanel;
        public TMP_Text victoryText;
        public Button victoryDismissButton;

        [Header("Objective toast (pop-up khi tick 1 mục tiêu)")]
        public GameObject objectiveToastPanel;
        public TMP_Text objectiveToastText;
        [Tooltip("Giây hiển thị toast trước khi tự ẩn. Dùng unscaledTime nên không bị ảnh hưởng pause.")]
        public float objectiveToastDuration = 2.5f;

        float objectiveToastHideAt;

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
            if (objectiveToastPanel != null) objectiveToastPanel.SetActive(false);
            if (welcomeDismissButton != null) welcomeDismissButton.onClick.AddListener(StartNewGame);
            if (continueButton != null)
            {
                continueButton.onClick.AddListener(DismissWelcome);
                // Chỉ hiện Continue khi đã có save file (không force user load scratch).
                continueButton.gameObject.SetActive(SaveSystem.HasSave);
            }
            if (quitButton != null) quitButton.onClick.AddListener(QuitDemo);
            if (victoryDismissButton != null) victoryDismissButton.onClick.AddListener(DismissVictory);
        }

        public void StartNewGame()
        {
            // Nếu không có save, scene vừa khởi tạo từ default — chỉ cần đóng overlay,
            // không reload scene (reload vô nghĩa và gây loop vì welcomePanel luôn
            // show lại trong Awake; Devin Review #33 finding).
            if (!SaveSystem.HasSave)
            {
                DismissWelcome();
                return;
            }
            // Có save cũ đã auto-load → xoá file + reload scene để wipe state runtime.
            // Trước khi reload phải destroy GameManager singleton (DontDestroyOnLoad) —
            // không thì sibling components SaveLoadController / PauseMenu / AudioManager
            // trên GO đó sẽ sống sót với reference tới UI/Player đã bị destroy, dẫn tới
            // autosave ghi null + pause UI không hiện (Devin Review #33 follow-up finding).
            SaveSystem.Delete();
            GameManager.ResetInstanceForSceneReload();
            var active = SceneManager.GetActiveScene();
            SceneManager.LoadScene(active.buildIndex >= 0 ? active.buildIndex : 0);
        }

        public void QuitDemo()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
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

        void Update()
        {
            if (objectiveToastPanel != null && objectiveToastPanel.activeSelf
                && Time.unscaledTime >= objectiveToastHideAt)
                objectiveToastPanel.SetActive(false);
        }

        void OnObjectiveCompleted(DemoObjectivesTracker.Objective o)
        {
            RefreshObjectives();
            ShowObjectiveToast(o);
            // Gợi ý audio feedback — ItemPickup tone ngắn, đã có sẵn từ AudioManager.
            AudioManager.Instance?.PlaySfx(AudioManager.SfxKind.ItemPickup);
        }

        void ShowObjectiveToast(DemoObjectivesTracker.Objective o)
        {
            if (objectiveToastPanel == null) return;
            if (objectiveToastText != null)
                objectiveToastText.text = $"<b>Hoàn thành:</b> {DemoObjectivesTracker.Label(o)}";
            objectiveToastPanel.SetActive(true);
            objectiveToastHideAt = Time.unscaledTime + Mathf.Max(0.5f, objectiveToastDuration);
        }

        void OnAllDone()
        {
            if (victoryPanel == null) return;
            if (victoryText != null)
                victoryText.text = "MVP Demo hoàn thành!\nBạn đã đột phá Luyện Khí Tầng 2 — cốt lõi loop chạy ổn.";
            victoryPanel.SetActive(true);
            // KHÔNG phát BreakthroughSuccess ở đây: objective cuối luôn là đột phá Luyện
            // Khí Tầng 2, nên AudioManager đã play tone này cùng frame qua
            // RealmSystem.OnBreakthroughAttempted. Phát thêm sẽ overlap gấp đôi âm lượng
            // (Devin Review #35 finding).
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
