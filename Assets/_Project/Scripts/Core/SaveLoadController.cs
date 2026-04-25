using UnityEngine;
using WildernessCultivation.Cultivation;
using WildernessCultivation.Items;
using WildernessCultivation.Player;

namespace WildernessCultivation.Core
{
    /// <summary>
    /// Gắn vào GameManager. Tự gọi Save mỗi autosaveInterval; gọi Load khi scene start nếu có save.
    /// </summary>
    public class SaveLoadController : MonoBehaviour
    {
        public PlayerStats playerStats;
        public RealmSystem realm;
        public Inventory inventory;
        public TimeManager timeManager;

        public float autosaveInterval = 120f;
        float nextSaveAt;

        void Start()
        {
            if (playerStats == null) playerStats = FindObjectOfType<PlayerStats>();
            if (realm == null) realm = FindObjectOfType<RealmSystem>();
            if (inventory == null) inventory = FindObjectOfType<Inventory>();
            if (timeManager == null) timeManager = FindObjectOfType<TimeManager>();

            // Optional auto-load on start
            // if (SaveSystem.TryLoad(out var data)) Apply(data);

            nextSaveAt = Time.time + autosaveInterval;
        }

        void Update()
        {
            if (Time.time >= nextSaveAt)
            {
                nextSaveAt = Time.time + autosaveInterval;
                Save();
            }
        }

        public void Save()
        {
            var data = new SaveData
            {
                player = new PlayerSaveData
                {
                    position = playerStats != null ? playerStats.transform.position : Vector3.zero,
                    hp = playerStats?.HP ?? 0,
                    hunger = playerStats?.Hunger ?? 0,
                    thirst = playerStats?.Thirst ?? 0,
                    sanity = playerStats?.Sanity ?? 0,
                    mana = playerStats?.Mana ?? 0,
                    realmTier = realm?.currentTier ?? 0,
                    cultivationXp = realm?.currentXp ?? 0,
                    spiritRoot = realm?.SpiritRoot ?? "Hỏa",
                },
                world = new WorldSaveData
                {
                    timeOfDay01 = timeManager?.currentTime01 ?? 0f,
                },
            };

            if (inventory != null)
            {
                foreach (var s in inventory.Slots)
                {
                    if (s.IsEmpty) continue;
                    data.inventory.Add(new InventorySlotData { itemId = s.item.itemId, count = s.count });
                }
            }

            SaveSystem.Save(data);
        }

        public void LoadAndApply()
        {
            if (!SaveSystem.TryLoad(out var data)) return;
            Apply(data);
        }

        void Apply(SaveData data)
        {
            if (playerStats != null && data.player != null)
            {
                playerStats.transform.position = data.player.position;
                playerStats.HP = data.player.hp;
                playerStats.Hunger = data.player.hunger;
                playerStats.Thirst = data.player.thirst;
                playerStats.Sanity = data.player.sanity;
                playerStats.Mana = data.player.mana;
            }
            if (realm != null && data.player != null)
            {
                realm.currentTier = data.player.realmTier;
                realm.currentXp = data.player.cultivationXp;
                realm.SpiritRoot = data.player.spiritRoot;
            }
            if (timeManager != null && data.world != null)
                timeManager.currentTime01 = data.world.timeOfDay01;
            // Inventory restore yêu cầu lookup ItemSO bằng itemId — implement khi có item DB.
        }
    }
}
