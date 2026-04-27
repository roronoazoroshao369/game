using UnityEngine;
using WildernessCultivation.Cultivation;
using WildernessCultivation.Items;

namespace WildernessCultivation.Player
{
    /// <summary>
    /// Hook tiêu thụ Linh Quả (Spirit Fruit). Linh Quả không phải item bình thường:
    /// ăn KHÔNG hồi đói/khát/HP — chỉ trigger <see cref="AwakeningSystem.TryAwaken"/>.
    ///
    /// Component này gắn vào Player. Gọi <see cref="TryConsume"/> từ Inventory UI hoặc
    /// hotkey khi player chọn Linh Quả. Nếu chưa đủ điều kiện cần → giữ item lại.
    /// </summary>
    [RequireComponent(typeof(AwakeningSystem), typeof(Inventory))]
    public class SpiritFruitConsumeAction : MonoBehaviour
    {
        [Tooltip("Item asset của Linh Quả. Tạo qua Right-click > Create > WildernessCultivation > Item, đặt itemId='spirit_fruit'.")]
        public ItemSO spiritFruitItem;

        AwakeningSystem awaken;
        Inventory inventory;

        void Awake()
        {
            awaken = GetComponent<AwakeningSystem>();
            inventory = GetComponent<Inventory>();
        }

        /// <summary>Tiêu thụ 1 Linh Quả từ inventory + roll awaken. Trả về true nếu rolled
        /// (consumed). False nếu chưa đủ điều kiện cần / không có item → KHÔNG consume.</summary>
        public bool TryConsume()
        {
            if (spiritFruitItem == null || inventory == null || awaken == null) return false;
            if (inventory.CountOf(spiritFruitItem) <= 0)
            {
                Debug.Log("[SpiritFruit] Không có Linh Quả trong inventory.");
                return false;
            }
            if (awaken.CheckEligibility() != AwakenEligibility.Eligible)
            {
                Debug.Log("[SpiritFruit] Chưa đủ điều kiện khai mở (day/HP/sanity).");
                return false;
            }
            // Consume trước khi roll: tránh exploit (cancel mid-frame).
            if (!inventory.TryConsume(spiritFruitItem, 1)) return false;
            awaken.TryAwaken(out _);
            return true;
        }
    }
}
