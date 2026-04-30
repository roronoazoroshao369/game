using System;
using WildernessCultivation.Core;
using UnityEngine;
using WildernessCultivation.Cultivation;
using WildernessCultivation.Items;

namespace WildernessCultivation.UI
{
    /// <summary>
    /// Tracker tiến trình demo MVP. Theo dõi 5 mục tiêu core-loop bám theo MVP_SCOPE.md:
    ///
    ///   1. Thu thập 3 Gỗ
    ///   2. Có Thịt nướng trong túi (đã chế biến tại lửa trại)
    ///   3. Bắt đầu ngồi thiền (Tụ Linh Quyết)
    ///   4. Đạt Luyện Khí Tầng 1 (tier >= 1)
    ///   5. Đạt Luyện Khí Tầng 2 (tier >= 2) → demo COMPLETE
    ///
    /// Bắn event cho UI (<see cref="TutorialHUD"/>) khi thay đổi state.
    /// Không tự destroy — UI có thể tái hiển thị checklist.
    /// </summary>
    public class DemoObjectivesTracker : MonoBehaviour
    {
        [Header("Refs (auto-find if null)")]
        public Inventory inventory;
        public RealmSystem realm;
        public MeditationAction meditation;

        [Header("Item IDs theo MVP_SCOPE")]
        public string woodItemId = "wood";
        public string cookedMeatItemId = "cooked_meat";

        [Header("Thresholds")]
        public int woodGoal = 3;
        public int realmLuyenKhi1Tier = 1;
        public int realmLuyenKhi2Tier = 2;

        public enum Objective { CollectWood, CookMeat, StartMeditation, ReachLuyenKhi1, ReachLuyenKhi2 }

        public bool[] Completed { get; private set; } = new bool[5];
        public bool AllDone => Completed[0] && Completed[1] && Completed[2] && Completed[3] && Completed[4];

        public event Action<Objective> OnObjectiveCompleted;
        public event Action OnAllObjectivesCompleted;

        void Awake()
        {
            if (inventory == null) inventory = ServiceLocator.Get<Inventory>();
            if (realm == null) realm = ServiceLocator.Get<RealmSystem>();
            if (meditation == null) meditation = ServiceLocator.Get<MeditationAction>();
            ServiceLocator.Register<DemoObjectivesTracker>(this);
        }

        void OnDestroy() => ServiceLocator.Unregister<DemoObjectivesTracker>(this);

        void OnEnable()
        {
            if (inventory != null) inventory.OnInventoryChanged += OnInventoryChanged;
            if (realm != null) realm.OnRealmAdvanced += OnRealmAdvanced;
        }

        void OnDisable()
        {
            if (inventory != null) inventory.OnInventoryChanged -= OnInventoryChanged;
            if (realm != null) realm.OnRealmAdvanced -= OnRealmAdvanced;
        }

        void Update()
        {
            // Meditation không có event → poll. Cost thấp (bool check mỗi frame).
            if (!Completed[(int)Objective.StartMeditation] && meditation != null && meditation.IsMeditating)
                Mark(Objective.StartMeditation);
        }

        void OnInventoryChanged()
        {
            if (inventory == null) return;
            if (!Completed[(int)Objective.CollectWood] && inventory.CountOf(woodItemId) >= woodGoal)
                Mark(Objective.CollectWood);
            if (!Completed[(int)Objective.CookMeat] && inventory.CountOf(cookedMeatItemId) >= 1)
                Mark(Objective.CookMeat);
        }

        void OnRealmAdvanced(int tier)
        {
            if (!Completed[(int)Objective.ReachLuyenKhi1] && tier >= realmLuyenKhi1Tier)
                Mark(Objective.ReachLuyenKhi1);
            if (!Completed[(int)Objective.ReachLuyenKhi2] && tier >= realmLuyenKhi2Tier)
                Mark(Objective.ReachLuyenKhi2);
        }

        void Mark(Objective o)
        {
            int idx = (int)o;
            if (Completed[idx]) return;
            Completed[idx] = true;
            Debug.Log($"[Demo] Mục tiêu hoàn thành: {o}");
            OnObjectiveCompleted?.Invoke(o);
            if (AllDone) OnAllObjectivesCompleted?.Invoke();
        }

        public static string Label(Objective o) => o switch
        {
            Objective.CollectWood => "Thu thập 3 Gỗ",
            Objective.CookMeat => "Nướng được Thịt tại lửa trại",
            Objective.StartMeditation => "Bắt đầu Tụ Linh Quyết (M)",
            Objective.ReachLuyenKhi1 => "Đột phá Luyện Khí Tầng 1",
            Objective.ReachLuyenKhi2 => "Đột phá Luyện Khí Tầng 2 (MVP done)",
            _ => o.ToString()
        };
    }
}
