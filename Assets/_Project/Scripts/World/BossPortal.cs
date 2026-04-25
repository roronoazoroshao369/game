using UnityEngine;
using WildernessCultivation.Items;
using WildernessCultivation.Player;

namespace WildernessCultivation.World
{
    /// <summary>
    /// Cổng bí cảnh — interact để vào "instance boss". MVP đơn giản: spawn boss prefab cách player ~3m
    /// rồi disable cổng cho đến khi boss chết. Yêu cầu 1 vật phẩm (vd Linh Thạch) để mở cổng nếu set.
    /// </summary>
    public class BossPortal : MonoBehaviour, IInteractable
    {
        [Header("Boss")]
        public GameObject bossPrefab;
        public Vector3 spawnOffset = new Vector3(0f, 3f, 0f);

        [Header("Cost (optional)")]
        public ItemSO unlockKeyItem;     // vd "linh_thach"
        public int keyCost = 1;
        public bool consumeKey = true;

        [Header("State")]
        public bool isOpen = true;
        public GameObject spawnedBoss;

        public string InteractLabel => isOpen
            ? (unlockKeyItem != null ? $"Mở bí cảnh ({unlockKeyItem.displayName} x{keyCost})" : "Mở bí cảnh")
            : "Bí cảnh đang khoá";

        public bool CanInteract(GameObject actor)
        {
            if (!isOpen) return false;
            if (spawnedBoss != null) return false; // boss đang còn trong instance
            if (unlockKeyItem == null) return true;
            var inv = actor != null ? actor.GetComponent<Inventory>() ?? actor.GetComponentInParent<Inventory>() : null;
            return inv != null && inv.CountOf(unlockKeyItem) >= keyCost;
        }

        public bool Interact(GameObject actor)
        {
            if (!CanInteract(actor)) return false;
            if (unlockKeyItem != null && consumeKey)
            {
                var inv = actor.GetComponent<Inventory>() ?? actor.GetComponentInParent<Inventory>();
                if (inv == null || !inv.TryConsume(unlockKeyItem, keyCost)) return false;
            }
            if (bossPrefab == null)
            {
                Debug.LogWarning("[BossPortal] thiếu bossPrefab");
                return false;
            }
            spawnedBoss = Instantiate(bossPrefab, transform.position + spawnOffset, Quaternion.identity);
            Debug.Log($"[BossPortal] Đã mở bí cảnh — boss spawn tại {spawnedBoss.transform.position}");
            return true;
        }
    }
}
