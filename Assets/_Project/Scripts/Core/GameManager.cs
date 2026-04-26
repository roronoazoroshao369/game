using UnityEngine;

namespace WildernessCultivation.Core
{
    /// <summary>
    /// Singleton điều phối toàn cục: pause, reference player, save trigger.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("References")]
        public Transform player;
        public TimeManager timeManager;

        [Header("State")]
        public bool isPaused;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void SetPaused(bool paused)
        {
            isPaused = paused;
            Time.timeScale = paused ? 0f : 1f;
        }

        public void TogglePause() => SetPaused(!isPaused);

        /// <summary>
        /// Destroy singleton + clear <see cref="Instance"/> để cho phép scene reload tạo
        /// GameManager mới. Dùng khi cần wipe hoàn toàn runtime state (vd "Bắt đầu mới").
        /// <see cref="DontDestroyOnLoad"/> khiến GO không tự biến mất khi
        /// <see cref="UnityEngine.SceneManagement.SceneManager.LoadScene"/> chạy — mọi
        /// sibling component (SaveLoadController, PauseMenu, AudioManager) cũng sẽ giữ
        /// reference stale tới object đã destroy trong scene cũ.
        /// </summary>
        public static void ResetInstanceForSceneReload()
        {
            if (Instance == null) return;
            Destroy(Instance.gameObject);
            Instance = null;
            Time.timeScale = 1f; // đề phòng đang pause lúc reload
        }
    }
}
