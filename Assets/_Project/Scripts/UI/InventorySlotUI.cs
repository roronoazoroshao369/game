using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using WildernessCultivation.Items;

namespace WildernessCultivation.UI
{
    /// <summary>
    /// 1 ô inventory. Tap → <see cref="onClick"/> (ăn / uống / transfer).
    /// Drag &amp; drop (touch + mouse) → <see cref="onDropFromSlot"/> để swap với slot nguồn.
    /// Drag ghost icon follow con trỏ, auto-restore nếu drop ngoài slot khác.
    /// </summary>
    public class InventorySlotUI : MonoBehaviour,
        IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
    {
        public Image iconImage;
        public TMP_Text countText;
        public Button button;

        [HideInInspector] public int slotIndex;
        public System.Action<int> onClick;

        // Được caller set để slot đích gọi khi bị drop vào. Truyền <see cref="slotIndex"/> nguồn.
        // Caller chịu trách nhiệm swap/transfer thực tế (Inventory.SwapSlots / TransferSlot).
        public System.Action<int, int> onDropFromSlot;

        // Ghost icon (clone iconImage) hiển thị khi drag. Tự tạo / destroy.
        GameObject dragGhost;
        Canvas rootCanvas;

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

        // ============ DRAG & DROP ============

        public void OnBeginDrag(PointerEventData eventData)
        {
            // Chỉ drag được khi slot có icon hiển thị (ô không rỗng).
            if (iconImage == null || !iconImage.enabled || iconImage.sprite == null) return;

            rootCanvas = GetComponentInParent<Canvas>();
            if (rootCanvas == null) return;

            dragGhost = new GameObject("DragGhost", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
            dragGhost.transform.SetParent(rootCanvas.transform, false);
            var rt = (RectTransform)dragGhost.transform;
            rt.sizeDelta = ((RectTransform)iconImage.transform).sizeDelta;
            var img = dragGhost.GetComponent<Image>();
            img.sprite = iconImage.sprite;
            img.preserveAspect = true;
            img.raycastTarget = false;
            var cg = dragGhost.GetComponent<CanvasGroup>();
            cg.blocksRaycasts = false;
            cg.alpha = 0.85f;

            MoveGhostTo(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (dragGhost == null) return;
            MoveGhostTo(eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (dragGhost != null)
            {
                Destroy(dragGhost);
                dragGhost = null;
            }
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (eventData.pointerDrag == null) return;
            var src = eventData.pointerDrag.GetComponent<InventorySlotUI>();
            if (src == null || src == this) return;
            onDropFromSlot?.Invoke(src.slotIndex, slotIndex);
        }

        void MoveGhostTo(PointerEventData eventData)
        {
            if (dragGhost == null || rootCanvas == null) return;
            var rt = (RectTransform)dragGhost.transform;
            if (rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                rt.position = eventData.position;
            }
            else
            {
                var cam = rootCanvas.worldCamera != null ? rootCanvas.worldCamera : eventData.pressEventCamera;
                RectTransformUtility.ScreenPointToWorldPointInRectangle(
                    (RectTransform)rootCanvas.transform, eventData.position, cam, out var world);
                rt.position = world;
            }
        }
    }
}
