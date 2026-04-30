using UnityEngine;
using WildernessCultivation.Cultivation;
using WildernessCultivation.Items;
using WildernessCultivation.Player;
using WildernessCultivation.World;

namespace WildernessCultivation.Core
{
    /// <summary>
    /// Gắn vào GameManager. Tự gọi Save mỗi autosaveInterval; gọi Load khi scene start nếu có save.
    ///
    /// <para>R6 refactor: controller là dispatcher thuần — enumerate <see cref="SaveRegistry"/>
    /// rồi gọi <see cref="ISaveable.CaptureState"/> / <see cref="ISaveable.RestoreState"/>.
    /// Mỗi system (PlayerStats / RealmSystem / Inventory / TimeManager / WorldGenerator)
    /// tự own slice save của nó. Cross-system post-restore ordering qua
    /// <see cref="SaveRegistry.RegisterFixup"/> (vd ReapplySpiritRootMaxHP → ReapplyAccumulatedBonuses → clamp HP).</para>
    ///
    /// <para>Các public field (<see cref="playerStats"/>, <see cref="inventory"/>, …) giữ
    /// lại chỉ để legacy BootstrapWizard wire Inspector + guard <see cref="Save"/> khi
    /// player chết; KHÔNG còn dùng để serialize data.</para>
    /// </summary>
    public class SaveLoadController : MonoBehaviour
    {
        void Awake()
        {
            ServiceLocator.Register<SaveLoadController>(this);
        }

        void OnDestroy() => ServiceLocator.Unregister<SaveLoadController>(this);

        [Header("Dependencies (legacy wiring — R6 dispatcher không dùng để serialize)")]
        [Tooltip("Chỉ dùng cho Save() IsDead guard + ReapplySpiritRootMaxHP fallback khi " +
            "PlayerStats chưa register ISaveFixup. Ưu tiên ServiceLocator lookup ở Start().")]
        public PlayerStats playerStats;
        public PlayerCombat playerCombat;
        public RealmSystem realm;
        public Inventory inventory;
        public TimeManager timeManager;
        public WorldGenerator worldGenerator;
        [Tooltip("[Deprecated R6] Move vào Inventory.itemDatabase. Field giữ để BootstrapWizard legacy wire không break.")]
        public ItemDatabase itemDatabase;
        public SpiritRoot spiritRoot;
        [Tooltip("[Deprecated R6] Move vào SpiritRoot.spiritRootCatalog. Field giữ để BootstrapWizard legacy wire không break.")]
        public SpiritRootSO[] spiritRootCatalog;

        [Header("Behavior")]
        public float autosaveInterval = 120f;
        [Tooltip("Tự load save lúc scene start nếu có file.")]
        public bool autoLoadOnStart = true;

        float nextSaveAt;

        void Start()
        {
            if (playerStats == null) playerStats = ServiceLocator.Get<PlayerStats>();
            if (playerCombat == null) playerCombat = ServiceLocator.Get<PlayerCombat>();
            if (realm == null) realm = ServiceLocator.Get<RealmSystem>();
            if (inventory == null) inventory = ServiceLocator.Get<Inventory>();
            if (timeManager == null) timeManager = ServiceLocator.Get<TimeManager>();
            if (worldGenerator == null) worldGenerator = ServiceLocator.Get<WorldGenerator>();
            if (spiritRoot == null) spiritRoot = ServiceLocator.Get<SpiritRoot>();

            ForwardLegacyRefs();

            if (autoLoadOnStart) LoadAndApply();

            nextSaveAt = Time.time + autosaveInterval;
        }

        /// <summary>Forward legacy inspector refs (itemDatabase, spiritRootCatalog) sang
        /// component owners (Inventory, SpiritRoot) — BootstrapWizard có thể wire field cũ.
        /// Gọi ở Start + trước Save/LoadAndApply để test không cần chạy Start cũng forward đúng.</summary>
        void ForwardLegacyRefs()
        {
            if (itemDatabase != null && inventory != null && inventory.itemDatabase == null)
                inventory.itemDatabase = itemDatabase;
            if (spiritRootCatalog != null && spiritRootCatalog.Length > 0 &&
                spiritRoot != null && (spiritRoot.spiritRootCatalog == null || spiritRoot.spiritRootCatalog.Length == 0))
                spiritRoot.spiritRootCatalog = spiritRootCatalog;
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
            // Skip autosave khi player chết — tránh race với permadeath delay window:
            // ExecutePermadeath gọi SaveSystem.Delete() rồi defer reload 1.5s; trong
            // window đó Update() có thể fire autosave → ghi save HP=0 → reload load
            // dead state → Update() early-return vì IsDead → softlock.
            if (playerStats != null && playerStats.IsDead) return;

            ForwardLegacyRefs();
            var data = new SaveData();
            foreach (var s in SaveRegistry.OrderedSaveables()) s.CaptureState(data);
            SaveSystem.Save(data);
        }

        public void LoadAndApply()
        {
            if (!SaveSystem.TryLoad(out var data)) return;
            ForwardLegacyRefs();
            foreach (var s in SaveRegistry.OrderedSaveables()) s.RestoreState(data);
            foreach (var f in SaveRegistry.OrderedFixupActions()) f(data);
        }
    }
}
