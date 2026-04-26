using UnityEngine;
using WildernessCultivation.Audio;
using WildernessCultivation.Core;
using WildernessCultivation.Player;
using WildernessCultivation.World;

namespace WildernessCultivation.Cultivation
{
    /// <summary>
    /// Tụ Linh Quyết — ngồi thiền. Trong khi thiền:
    ///  - Hồi mana nhanh
    ///  - Tích XP tu luyện
    ///  - Linh khí ban đêm đậm hơn → hồi nhanh hơn
    ///  - Không di chuyển được, dễ bị tấn công
    /// Toggle bằng phím M (PC) hoặc nút UI.
    /// </summary>
    [RequireComponent(typeof(PlayerStats), typeof(RealmSystem))]
    public class MeditationAction : MonoBehaviour
    {
        [Header("Rates")]
        public float manaPerSec = 8f;
        public float xpPerSec = 0.8f;

        [Header("Input")]
        public KeyCode toggleKey = KeyCode.M;

        public bool IsMeditating { get; private set; }

        PlayerStats stats;
        RealmSystem realm;
        TimeManager time;
        PlayerController controller;

        void Awake()
        {
            stats = GetComponent<PlayerStats>();
            realm = GetComponent<RealmSystem>();
            controller = GetComponent<PlayerController>();
        }

        void Start()
        {
            time = GameManager.Instance != null ? GameManager.Instance.timeManager : FindObjectOfType<TimeManager>();
        }

        void Update()
        {
            if (Input.GetKeyDown(toggleKey)) Toggle();

            if (IsMeditating)
            {
                if (controller != null && controller.InputDir.sqrMagnitude > 0.05f)
                {
                    StopMeditation();
                    return;
                }

                float mult = time != null ? time.GetSpiritualEnergyMultiplier() : 1f;
                if (WorldGenerator.Instance != null)
                {
                    var biome = WorldGenerator.Instance.BiomeAt(transform.position);
                    if (biome != null) mult *= biome.spiritEnergyMultiplier;
                }
                // SpiritArray buff (đặt trận pháp) — multiply lên trên
                mult *= WildernessCultivation.Combat.SpiritArray.SpiritMultiplierAt(transform.position);

                stats.AddMana(manaPerSec * mult * Time.deltaTime);
                realm.AddCultivationXp(xpPerSec * mult * Time.deltaTime);
            }
        }

        public void Toggle()
        {
            if (IsMeditating) StopMeditation();
            else StartMeditation();
        }

        public void StartMeditation()
        {
            IsMeditating = true;
            AudioManager.Instance?.PlaySfx(AudioManager.SfxKind.MeditationStart);
            Debug.Log("[Meditation] Bắt đầu Tụ Linh Quyết.");
        }

        public void StopMeditation()
        {
            IsMeditating = false;
            Debug.Log("[Meditation] Dừng thiền.");
        }
    }
}
