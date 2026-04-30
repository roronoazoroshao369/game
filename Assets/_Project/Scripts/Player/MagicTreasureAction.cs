using UnityEngine;
using WildernessCultivation.Core;
using WildernessCultivation.Cultivation;
using WildernessCultivation.Items;

namespace WildernessCultivation.Player
{
    /// <summary>
    /// Sử dụng pháp bảo (item dạng <see cref="MagicTreasureSO"/>) đang được trang bị. Có cooldown nội bộ
    /// theo từng pháp bảo, ăn 1 charge / lần dùng (nếu <c>consumeChargeOnUse</c> = true) hoặc tiêu hao
    /// 1 ItemSO khỏi inventory (nếu <c>chargesPerInstance == 1</c> và pháp bảo nằm trong inventory).
    /// </summary>
    [RequireComponent(typeof(PlayerStats))]
    public class MagicTreasureAction : MonoBehaviour
    {
        [Tooltip("Pháp bảo đang trang bị. Có thể đổi qua UI/inventory ở iteration sau.")]
        public MagicTreasureSO equipped;

        [Tooltip("Inventory để consume charge nếu pháp bảo dùng kiểu single-use.")]
        public Inventory inventory;

        [Header("Input (PC)")]
        public KeyCode useKey = KeyCode.B;

        PlayerStats stats;
        RealmSystem realm;
        float readyAt;
        int chargesLeftInstance = -1; // -1 = chưa init từ SO

        void Awake()
        {
            stats = GetComponent<PlayerStats>();
            realm = GetComponent<RealmSystem>();
            if (inventory == null) inventory = GetComponent<Inventory>() ?? ServiceLocator.Get<Inventory>();
            ServiceLocator.Register<MagicTreasureAction>(this);
        }

        void OnDestroy() => ServiceLocator.Unregister<MagicTreasureAction>(this);

        void OnValidate() { chargesLeftInstance = -1; }

        void Update()
        {
            if (Input.GetKeyDown(useKey)) TryUse();
        }

        public bool CanUse() => equipped != null && Time.time >= readyAt && HasChargeOrItem();

        public bool TryUse()
        {
            if (equipped == null) return false;
            if (Time.time < readyAt) return false;
            if (!HasChargeOrItem()) return false;

            if (!equipped.Activate(stats, realm)) return false;

            readyAt = Time.time + equipped.cooldown;
            ConsumeChargeOrItem();
            Debug.Log($"[Treasure] {equipped.displayName} kích hoạt.");
            return true;
        }

        bool HasChargeOrItem()
        {
            if (!equipped.consumeChargeOnUse) return true;
            if (equipped.chargesPerInstance <= 0) return true; // unlimited
            if (chargesLeftInstance < 0) chargesLeftInstance = equipped.chargesPerInstance;
            if (chargesLeftInstance > 0) return true;
            // Hết charge → cần 1 item mới trong inventory
            if (inventory != null && inventory.CountOf(equipped) > 0) return true;
            return false;
        }

        void ConsumeChargeOrItem()
        {
            if (!equipped.consumeChargeOnUse) return;
            if (equipped.chargesPerInstance <= 0) return;
            if (chargesLeftInstance < 0) chargesLeftInstance = equipped.chargesPerInstance;
            chargesLeftInstance--;

            if (chargesLeftInstance <= 0 && inventory != null)
            {
                if (inventory.TryConsume(equipped, 1))
                    chargesLeftInstance = equipped.chargesPerInstance;
            }
        }
    }
}
