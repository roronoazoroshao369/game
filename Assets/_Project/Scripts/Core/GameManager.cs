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
            // DontDestroyOnLoad throws InvalidOperationException in EditMode
            // (and is no-op outside Play mode) — guard so Awake stays runnable
            // from EditMode tests that invoke lifecycle reflectively.
            if (Application.isPlaying) DontDestroyOnLoad(gameObject);
            ServiceLocator.Register<GameManager>(this);
        }

        void OnDestroy() => ServiceLocator.Unregister<GameManager>(this);

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
            // Always force Instance reference to a hard null after this call
            // (Time.timeScale also normalised) — even if the singleton GO is
            // already destroyed (Unity's "fake-null") or was never set. This
            // keeps the post-condition deterministic for callers that rely on
            // ReferenceEquals(Instance, null), e.g. EditMode tests.
            try
            {
                if (Instance != null)
                {
                    var go = Instance.gameObject;
                    // Destroy() logs "may not be called from edit mode" in
                    // EditMode tests — use DestroyImmediate when not Playing
                    // so the same code path works in both modes.
                    if (Application.isPlaying) Destroy(go);
                    else DestroyImmediate(go);
                }
            }
            finally
            {
                Instance = null;
                Time.timeScale = 1f;
            }
        }
    }
}
