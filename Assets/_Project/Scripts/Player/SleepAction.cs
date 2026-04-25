using UnityEngine;
using WildernessCultivation.Core;
using WildernessCultivation.World;

namespace WildernessCultivation.Player
{
    /// <summary>
    /// Ngủ qua đêm — chỉ hoạt động vào ban đêm và khi player đang trong aura của một
    /// <see cref="Campfire"/> đang cháy. Khi ngủ:
    ///  - Time tăng tốc <see cref="timeMultiplier"/> lần đến khi sáng.
    ///  - Mỗi giây thực hồi <see cref="hpPerSec"/>, <see cref="sanityPerSec"/>.
    ///  - Đói/Khát vẫn decay (theo Time.deltaTime đã scale) — buộc người chơi phải ăn uống đủ trước khi ngủ.
    ///  - Bị tấn công → wake.
    /// </summary>
    [RequireComponent(typeof(PlayerStats))]
    public class SleepAction : MonoBehaviour
    {
        [Header("Rates (per scaled in-game second)")]
        [Tooltip("HP hồi mỗi giây in-game khi ngủ. Vì Time.timeScale tăng, tổng HP hồi ~ hpPerSec * timeMultiplier mỗi giây thực.")]
        public float hpPerSec = 6f;
        public float sanityPerSec = 12f;

        [Header("Time accel")]
        [Tooltip("Tăng tốc thời gian khi ngủ (để qua đêm nhanh).")]
        public float timeMultiplier = 8f;

        [Header("Constraints")]
        [Tooltip("Yêu cầu vào ban đêm (theo TimeManager).")]
        public bool requireNight = true;
        [Tooltip("Yêu cầu trong aura của 1 Campfire đang cháy.")]
        public bool requireCampfire = true;

        public bool IsSleeping { get; private set; }
        public event System.Action OnSleepStart;
        public event System.Action OnSleepEnd;

        PlayerStats stats;
        TimeManager time;
        float prevTimeScale;
        float prevHP;

        void Awake() { stats = GetComponent<PlayerStats>(); }

        void Start()
        {
            time = GameManager.Instance != null ? GameManager.Instance.timeManager : FindObjectOfType<TimeManager>();
        }

        void Update()
        {
            if (!IsSleeping) return;

            // Wake conditions
            if (stats.IsDead) { Wake(); return; }
            if (stats.HP < prevHP - 0.5f) { Wake(); return; } // bị tấn công
            prevHP = stats.HP;

            if (time != null && !time.isNight) { Wake(); return; }
            if (requireCampfire && !stats.IsWarm) { Wake(); return; }

            float dt = Time.deltaTime;
            stats.Heal(hpPerSec * dt);
            stats.RestoreSanity(sanityPerSec * dt);
        }

        public bool CanSleep()
        {
            if (IsSleeping) return false;
            if (requireNight && (time == null || !time.isNight)) return false;
            if (requireCampfire && !stats.IsWarm) return false;
            return true;
        }

        public bool TrySleep()
        {
            if (!CanSleep()) return false;
            IsSleeping = true;
            prevTimeScale = Time.timeScale;
            Time.timeScale = Mathf.Max(0.01f, prevTimeScale * timeMultiplier);
            prevHP = stats.HP;
            OnSleepStart?.Invoke();
            return true;
        }

        public void Wake()
        {
            if (!IsSleeping) return;
            IsSleeping = false;
            Time.timeScale = prevTimeScale > 0f ? prevTimeScale : 1f;
            OnSleepEnd?.Invoke();
        }

        void OnDisable() { if (IsSleeping) Wake(); }
    }
}
