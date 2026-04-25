using UnityEngine;
using UnityEngine.EventSystems;

namespace WildernessCultivation.UI
{
    /// <summary>
    /// On-screen joystick cho mobile. Gắn vào background image; thumb là child.
    /// Drag thumb → đọc Direction (-1..1, normalized).
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        [SerializeField] RectTransform background;
        [SerializeField] RectTransform thumb;
        [SerializeField] float radiusPixels = 100f;

        public Vector2 Direction { get; private set; }
        public bool IsActive { get; private set; }

        void Awake()
        {
            if (background == null) background = (RectTransform)transform;
            if (thumb == null && transform.childCount > 0)
                thumb = transform.GetChild(0).GetComponent<RectTransform>();
        }

        public void OnPointerDown(PointerEventData e)
        {
            IsActive = true;
            OnDrag(e);
        }

        public void OnPointerUp(PointerEventData e)
        {
            IsActive = false;
            Direction = Vector2.zero;
            if (thumb != null) thumb.anchoredPosition = Vector2.zero;
        }

        public void OnDrag(PointerEventData e)
        {
            Vector2 local;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                background, e.position, e.pressEventCamera, out local);

            Vector2 clamped = Vector2.ClampMagnitude(local, radiusPixels);
            if (thumb != null) thumb.anchoredPosition = clamped;
            Direction = clamped / radiusPixels;
        }
    }
}
