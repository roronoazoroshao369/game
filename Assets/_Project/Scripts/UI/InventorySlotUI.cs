using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WildernessCultivation.Items;

namespace WildernessCultivation.UI
{
    public class InventorySlotUI : MonoBehaviour
    {
        public Image iconImage;
        public TMP_Text countText;
        public Button button;

        [HideInInspector] public int slotIndex;
        public System.Action<int> onClick;

        void Awake()
        {
            if (button != null) button.onClick.AddListener(() => onClick?.Invoke(slotIndex));
        }

        public void Bind(InventorySlot slot)
        {
            if (slot == null || slot.IsEmpty)
            {
                if (iconImage != null) { iconImage.sprite = null; iconImage.enabled = false; }
                if (countText != null) countText.text = "";
                return;
            }
            if (iconImage != null) { iconImage.sprite = slot.item.icon; iconImage.enabled = slot.item.icon != null; }
            if (countText != null) countText.text = slot.count > 1 ? slot.count.ToString() : "";
        }
    }
}
