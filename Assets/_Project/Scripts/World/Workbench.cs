using UnityEngine;
using WildernessCultivation.Crafting;
using WildernessCultivation.Items;

namespace WildernessCultivation.World
{
    /// <summary>
    /// Trạm sửa đồ. Player bấm E khi đứng cạnh → sửa món durable đầu tiên trong
    /// inventory bằng cách tốn <see cref="repairCost"/> món <see cref="repairMaterial"/>.
    /// Sửa toàn phần (durability về maxDurability) hoặc partial nếu
    /// <see cref="repairAmount"/> &gt; 0.
    ///
    /// Cũng đóng vai trò <see cref="CraftStation.Workbench"/> cho RecipeSO yêu cầu
    /// trạm này — tự gắn <see cref="CraftStationMarker"/> qua RequireComponent.
    /// </summary>
    [RequireComponent(typeof(CraftStationMarker))]
    public class Workbench : MonoBehaviour, IInteractable
    {
        [Header("Repair config")]
        [Tooltip("Item dùng làm vật liệu sửa (ví dụ Cành Cây).")]
        public ItemSO repairMaterial;
        [Tooltip("Số lượng vật liệu tốn cho mỗi lần sửa.")]
        public int repairCost = 1;
        [Tooltip("Số durability hồi sau 1 lần sửa. <=0 → hồi full về maxDurability.")]
        public float repairAmount = -1f;

        public string InteractLabel
        {
            get
            {
                string mat = repairMaterial != null ? repairMaterial.displayName : "vật liệu";
                return $"Sửa đồ (tốn {repairCost} {mat})";
            }
        }

        public bool CanInteract(GameObject actor) => actor != null;

        void Awake()
        {
            var marker = GetComponent<CraftStationMarker>();
            if (marker != null) marker.station = CraftStation.Workbench;
        }

        public bool Interact(GameObject actor)
        {
            if (actor == null) return false;
            var inv = actor.GetComponent<Inventory>();
            if (inv == null) return false;

            int dmgSlot = inv.FindFirstDamagedSlot();
            if (dmgSlot < 0)
            {
                Debug.Log("[Workbench] Không có đồ nào cần sửa.");
                return true;
            }

            var slot = inv.GetSlot(dmgSlot);
            string itemName = slot != null && slot.item != null ? slot.item.displayName : "?";
            bool needsMaterial = repairMaterial != null && repairCost > 0;

            // Tránh case repairMaterial trùng với chính món đang sửa: sau khi Repair món sẽ
            // hết IsBroken → TryConsume có thể "ăn" chính nó. Refuse upfront.
            if (needsMaterial && slot != null && slot.item == repairMaterial)
            {
                Debug.LogWarning($"[Workbench] Không thể dùng chính {repairMaterial.displayName} làm vật liệu sửa cho nó.");
                return true;
            }

            // Verify đủ vật liệu TRƯỚC khi mutate — tránh tốn material rồi Repair fail.
            if (needsMaterial && inv.CountOf(repairMaterial) < repairCost)
            {
                Debug.Log($"[Workbench] Thiếu {repairMaterial.displayName} (cần {repairCost}).");
                return true;
            }

            // Repair trước, consume sau: nếu Repair fail, không tốn material.
            if (!inv.Repair(dmgSlot, repairAmount))
            {
                Debug.LogWarning("[Workbench] Repair thất bại bất ngờ.");
                return true;
            }

            if (needsMaterial) inv.TryConsume(repairMaterial, repairCost);
            Debug.Log($"[Workbench] Đã sửa {itemName}.");
            return true;
        }
    }
}
