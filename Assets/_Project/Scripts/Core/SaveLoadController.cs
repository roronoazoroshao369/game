using UnityEngine;
using WildernessCultivation.Cultivation;
using WildernessCultivation.Items;
using WildernessCultivation.Player;
using WildernessCultivation.World;

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
        public WorldGenerator worldGenerator;
        public ItemDatabase itemDatabase;

        [Header("Behavior")]
        public float autosaveInterval = 120f;
        [Tooltip("Tự load save lúc scene start nếu có file.")]
        public bool autoLoadOnStart = true;

        float nextSaveAt;

        void Start()
        {
            if (playerStats == null) playerStats = FindObjectOfType<PlayerStats>();
            if (realm == null) realm = FindObjectOfType<RealmSystem>();
            if (inventory == null) inventory = FindObjectOfType<Inventory>();
            if (timeManager == null) timeManager = FindObjectOfType<TimeManager>();
            if (worldGenerator == null) worldGenerator = FindObjectOfType<WorldGenerator>();

            if (autoLoadOnStart) LoadAndApply();

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
                    seed = worldGenerator != null ? worldGenerator.seed : 0,
                    daysSurvived = timeManager != null ? timeManager.daysSurvived : 0,
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
            {
                timeManager.currentTime01 = data.world.timeOfDay01;
                timeManager.daysSurvived = data.world.daysSurvived;
            }
            if (worldGenerator != null && data.world != null && data.world.seed != 0)
                worldGenerator.seed = data.world.seed;

            RestoreInventory(data);
        }

        void RestoreInventory(SaveData data)
        {
            if (inventory == null || data.inventory == null) return;
            if (itemDatabase == null)
            {
                Debug.LogWarning("[Save] ItemDatabase chưa được gán → không thể restore inventory.");
                return;
            }
            // Xóa toàn bộ slot rồi nạp lại
            for (int i = 0; i < inventory.Slots.Count; i++)
                inventory.TryConsumeSlot(i, inventory.Slots[i].count);

            foreach (var s in data.inventory)
            {
                if (string.IsNullOrEmpty(s.itemId) || s.count <= 0) continue;
                var item = itemDatabase.GetById(s.itemId);
                if (item == null)
                {
                    Debug.LogWarning($"[Save] ItemDatabase không có itemId='{s.itemId}', bỏ qua.");
                    continue;
                }
                int leftover = inventory.Add(item, s.count);
                if (leftover > 0)
                    Debug.LogWarning($"[Save] Inventory đầy khi restore {s.itemId}, {leftover} item bị mất.");
            }
        }
    }
}
