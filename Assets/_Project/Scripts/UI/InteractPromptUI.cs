using TMPro;
using WildernessCultivation.Core;
using UnityEngine;
using UnityEngine.UI;
using WildernessCultivation.Player;

namespace WildernessCultivation.UI
{
    /// <summary>
    /// Prompt nổi gần player gợi ý hành động tương tác hiện có (vd "E — Uống").
    /// Hide khi không có target. Có thể bind nút mobile vào <see cref="OnInteractButton"/>.
    /// </summary>
    public class InteractPromptUI : MonoBehaviour
    {
        public InteractAction interactAction;
        public GameObject promptRoot;   // Parent panel toggle (ưu tiên nếu gán)
        public TMP_Text label;
        public Button button;
        public string keyHint = "E";

        void Start()
        {
            if (interactAction == null) interactAction = ServiceLocator.Get<InteractAction>();
            if (button != null) button.onClick.AddListener(OnInteractButton);
            Hide();
        }

        void Update()
        {
            if (interactAction == null) { Hide(); return; }
            var target = interactAction.CurrentTarget;
            if (target == null) { Hide(); return; }

            if (label != null) label.text = string.IsNullOrEmpty(keyHint)
                ? target.InteractLabel
                : $"[{keyHint}] {target.InteractLabel}";
            Show();
        }

        public void OnInteractButton()
        {
            if (interactAction != null) interactAction.TryInteract();
        }

        void Show()
        {
            if (promptRoot != null) promptRoot.SetActive(true);
            if (label != null) label.enabled = true;
            if (button != null) button.gameObject.SetActive(true);
        }

        void Hide()
        {
            if (promptRoot != null) promptRoot.SetActive(false);
            if (label != null) label.enabled = false;
            if (button != null) button.gameObject.SetActive(false);
        }
    }
}
