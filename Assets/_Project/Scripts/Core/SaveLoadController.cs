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
        public PlayerCombat playerCombat;
        public RealmSystem realm;
        public Inventory inventory;
        public TimeManager timeManager;
        public WorldGenerator worldGenerator;
        public ItemDatabase itemDatabase;
        public SpiritRoot spiritRoot;
        [Tooltip("Pool linh căn để resolve theo tên khi load.")]
        public SpiritRootSO[] spiritRootCatalog;

        [Header("Behavior")]
        public float autosaveInterval = 120f;
        [Tooltip("Tự load save lúc scene start nếu có file.")]
        public bool autoLoadOnStart = true;

        float nextSaveAt;

        void Start()
        {
            if (playerStats == null) playerStats = FindObjectOfType<PlayerStats>();
            if (playerCombat == null) playerCombat = FindObjectOfType<PlayerCombat>();
            if (realm == null) realm = FindObjectOfType<RealmSystem>();
            if (inventory == null) inventory = FindObjectOfType<Inventory>();
            if (timeManager == null) timeManager = FindObjectOfType<TimeManager>();
            if (worldGenerator == null) worldGenerator = FindObjectOfType<WorldGenerator>();
            if (spiritRoot == null) spiritRoot = FindObjectOfType<SpiritRoot>();

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
                    bodyTemp = playerStats?.BodyTemp ?? 50f,
                    realmTier = realm?.currentTier ?? 0,
                    cultivationXp = realm?.currentXp ?? 0,
                    spiritRoot = (spiritRoot != null && spiritRoot.Current != null) ? spiritRoot.Current.name : (realm?.SpiritRoot ?? "Hỏa"),
                    isAwakened = playerStats?.IsAwakened ?? false,
                },
                world = new WorldSaveData
                {
                    timeOfDay01 = timeManager?.currentTime01 ?? 0f,
                    seed = worldGenerator != null ? worldGenerator.seed : 0,
                    daysSurvived = timeManager != null ? timeManager.daysSurvived : 0,
                    seasonIndex = timeManager != null ? (int)timeManager.currentSeason : 0,
                    weatherIndex = timeManager != null ? (int)timeManager.currentWeather : 0,
                },
            };

            if (inventory != null)
            {
                foreach (var s in inventory.Slots)
                {
                    if (s.IsEmpty) continue;
                    data.inventory.Add(new InventorySlotData
                    {
                        itemId = s.item.itemId,
                        count = s.count,
                        freshRemaining = s.IsPerishable ? s.freshRemaining : -1f,
                        durability = s.IsDurable ? s.durability : -1f,
                    });
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
                playerStats.BodyTemp = data.player.bodyTemp <= 0 ? 50f : data.player.bodyTemp;
                playerStats.IsAwakened = data.player.isAwakened;
            }
            if (realm != null && data.player != null)
            {
                realm.currentTier = data.player.realmTier;
                realm.currentXp = data.player.cultivationXp;
                realm.SpiritRoot = data.player.spiritRoot;
            }
            // Re-build maxHP/maxMana từ base + linh căn + tích luỹ realm bonus, rồi set HP/Mana từ save.
            // Thứ tự: SetSpiritRoot → ReapplySpiritRootMaxHP (reset về base + spiritMul)
            //        → ReapplyAccumulatedBonuses (cộng hpBonus tier 1..currentTier) → set HP/Mana.
            if (playerStats != null)
            {
                if (spiritRoot != null && data.player != null && !string.IsNullOrEmpty(data.player.spiritRoot) && spiritRootCatalog != null)
                {
                    foreach (var so in spiritRootCatalog)
                        if (so != null && so.name == data.player.spiritRoot) { spiritRoot.SetSpiritRoot(so); break; }
                }
                playerStats.ReapplySpiritRootMaxHP();
                if (playerCombat != null) playerCombat.ResetMeleeDamageToBase();
                if (realm != null) realm.ReapplyAccumulatedBonuses();
                if (data.player != null)
                {
                    playerStats.HP = Mathf.Min(data.player.hp, playerStats.maxHP);
                    playerStats.Mana = Mathf.Min(data.player.mana, playerStats.maxMana);
                }
            }
            if (timeManager != null && data.world != null)
            {
                timeManager.currentTime01 = data.world.timeOfDay01;
                timeManager.daysSurvived = data.world.daysSurvived;
                timeManager.currentSeason = (Season)Mathf.Clamp(data.world.seasonIndex, 0, 3);
                timeManager.currentWeather = (Weather)Mathf.Clamp(data.world.weatherIndex, 0, 2);
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

            // Snapshot count per slot trước mỗi Add để định vị slot vừa được Add ghi vào
            // (perishable/durable không stack → mỗi entry tạo 1 slot mới). Tránh restore loop
            // đời cũ scan-from-zero làm overwrite slot trước (bug: 2 stack meat fresh khác nhau
            // sẽ bị stack-2 ghi đè freshRemaining của stack-1 vì cả 2 đều "match đầu tiên").
            int slotCount = inventory.Slots.Count;
            var preCounts = new int[slotCount];

            foreach (var s in data.inventory)
            {
                if (string.IsNullOrEmpty(s.itemId) || s.count <= 0) continue;
                var item = itemDatabase.GetById(s.itemId);
                if (item == null)
                {
                    Debug.LogWarning($"[Save] ItemDatabase không có itemId='{s.itemId}', bỏ qua.");
                    continue;
                }

                for (int i = 0; i < slotCount; i++) preCounts[i] = inventory.Slots[i].count;
                int leftover = inventory.Add(item, s.count);
                if (leftover > 0)
                    Debug.LogWarning($"[Save] Inventory đầy khi restore {s.itemId}, {leftover} item bị mất.");

                // Restore freshness/durability vào slot mà Add() vừa tăng count, KHÔNG quét
                // từ đầu list (tránh đụng slot cùng item đã được restore từ entry trước).
                if (s.freshRemaining >= 0f || s.durability >= 0f)
                {
                    for (int i = 0; i < slotCount; i++)
                    {
                        var slot = inventory.Slots[i];
                        if (slot.item == item && slot.count > preCounts[i])
                        {
                            if (s.freshRemaining >= 0f && slot.IsPerishable) slot.freshRemaining = s.freshRemaining;
                            if (s.durability >= 0f && slot.IsDurable) slot.durability = s.durability;
                            break;
                        }
                    }
                }
            }
        }
    }
}
