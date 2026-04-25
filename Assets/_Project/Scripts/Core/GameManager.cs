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
    }
}
