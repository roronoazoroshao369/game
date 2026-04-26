using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WildernessCultivation.Audio;
using WildernessCultivation.Core;

namespace WildernessCultivation.UI
{
    /// <summary>
    /// Menu pause đơn giản cho demo MVP. Bấm Esc/P (hoặc pause button) → dim overlay
    /// với 3 nút: Tiếp tục, Lưu ngay, Thoát Demo.
    ///
    /// Time.timeScale=0 khi paused (delegate cho <see cref="GameManager.SetPaused(bool)"/>).
    /// Dừng luôn autosave tick của <see cref="SaveLoadController"/> vì Time.time không tăng.
    /// </summary>
    public class PauseMenu : MonoBehaviour
    {
        [Header("Refs")]
        public GameManager gameManager;
        public SaveLoadController saveLoad;

        [Header("UI")]
        public GameObject overlay;
        public Button pauseButton;      // luôn hiện ở góc, bấm → mở menu
        public Button resumeButton;
        public Button saveNowButton;
        public Button quitButton;
        public TMP_Text toastText;      // hiện "Đã lưu" 1.5s
        public Slider masterVolumeSlider;   // 0..1, bind tới AudioManager.SetMaster
        public TMP_Text masterVolumeLabel;  // "Âm lượng: 80%"

        [Header("Input")]
        public KeyCode toggleKey = KeyCode.Escape;
        public KeyCode altToggleKey = KeyCode.P;

        float toastHideAt;

        // Cache modal để không FindObjectOfType mỗi frame khi check collision Esc.
        StorageChestUI cachedChestUI;

        void Awake()
        {
            if (gameManager == null) gameManager = FindObjectOfType<GameManager>();
            if (saveLoad == null) saveLoad = FindObjectOfType<SaveLoadController>();
            if (overlay != null) overlay.SetActive(false);
            if (toastText != null) toastText.gameObject.SetActive(false);

            if (pauseButton != null)  pauseButton.onClick.AddListener(Toggle);
            if (resumeButton != null) resumeButton.onClick.AddListener(Resume);
            if (saveNowButton != null) saveNowButton.onClick.AddListener(SaveNow);
            if (quitButton != null)   quitButton.onClick.AddListener(QuitDemo);

            if (masterVolumeSlider != null)
            {
                // Set giá trị ban đầu từ AudioManager (đã load PlayerPrefs trong Awake).
                float v = AudioManager.Instance != null ? AudioManager.Instance.masterVolume : 0.8f;
                masterVolumeSlider.minValue = 0f;
                masterVolumeSlider.maxValue = 1f;
                masterVolumeSlider.SetValueWithoutNotify(v);
                UpdateMasterLabel(v);
                masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
            }
        }

        void OnMasterVolumeChanged(float v)
        {
            if (AudioManager.Instance != null) AudioManager.Instance.SetMaster(v);
            UpdateMasterLabel(v);
        }

        void UpdateMasterLabel(float v)
        {
            if (masterVolumeLabel != null)
                masterVolumeLabel.text = $"Âm lượng: {Mathf.RoundToInt(v * 100f)}%";
        }

        void Update()
        {
            if (Input.GetKeyDown(toggleKey) || Input.GetKeyDown(altToggleKey))
            {
                // Không toggle pause khi đang mở modal khác (StorageChest) và đang ở state unpaused —
                // tránh va chạm khi cả 2 cùng listen Esc trong 1 frame (Devin Review #30 finding).
                // Nếu pause đang mở thì Esc vẫn resume bình thường.
                if (gameManager != null && gameManager.isPaused) Toggle();
                else if (!IsAnotherModalOpen()) Toggle();
            }
            if (toastText != null && toastText.gameObject.activeSelf && Time.unscaledTime >= toastHideAt)
                toastText.gameObject.SetActive(false);
        }

        bool IsAnotherModalOpen()
        {
            if (cachedChestUI == null) cachedChestUI = FindObjectOfType<StorageChestUI>();
            if (cachedChestUI != null && cachedChestUI.panel != null && cachedChestUI.panel.activeSelf)
                return true;
            return false;
        }

        public void Toggle()
        {
            if (gameManager == null) return;
            bool newPaused = !gameManager.isPaused;
            gameManager.SetPaused(newPaused);
            if (overlay != null) overlay.SetActive(newPaused);
        }

        public void Resume()
        {
            if (gameManager == null) return;
            gameManager.SetPaused(false);
            if (overlay != null) overlay.SetActive(false);
        }

        public void SaveNow()
        {
            if (saveLoad != null)
            {
                saveLoad.Save();
                ShowToast("Đã lưu.");
            }
            else
            {
                ShowToast("Không tìm thấy SaveLoadController.");
            }
        }

        public void QuitDemo()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        void ShowToast(string msg)
        {
            if (toastText == null) return;
            toastText.text = msg;
            toastText.gameObject.SetActive(true);
            toastHideAt = Time.unscaledTime + 1.5f;
        }
    }
}
