using UnityEngine;
using WildernessCultivation.Items;

namespace WildernessCultivation.World
{
    /// <summary>
    /// Rương chứa đồ — interactable mở UI; bản thân giữ 1 component <see cref="Inventory"/>
    /// riêng, độc lập với inventory player. UI phía ngoài có thể subscribe vào event để hiển thị.
    /// </summary>
    [RequireComponent(typeof(Inventory))]
    public class StorageChest : MonoBehaviour, IInteractable
    {
        [Tooltip("Số slot rương (mặc định 12).")]
        public int slotCount = 12;

        [Tooltip("Hiển thị label khi mở. Sẽ tự thay đổi nếu đặt = null.")]
        public string interactLabel = "Mở rương";

        public string InteractLabel => interactLabel;
        public bool CanInteract(GameObject actor) => actor != null;

        public Inventory ChestInventory { get; private set; }
        public static event System.Action<StorageChest> OnAnyChestOpened;

        void Awake()
        {
            ChestInventory = GetComponent<Inventory>();
            if (ChestInventory != null) ChestInventory.slotCount = slotCount;
        }

        public bool Interact(GameObject actor)
        {
            OnAnyChestOpened?.Invoke(this);
            return true;
        }
    }
}
