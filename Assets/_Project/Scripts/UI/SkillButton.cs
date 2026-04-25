using UnityEngine;
using UnityEngine.UI;
using WildernessCultivation.Player;

namespace WildernessCultivation.UI
{
    /// <summary>
    /// Nút skill mobile — bind 1 hành động (attack / cast / meditate).
    /// </summary>
    public class SkillButton : MonoBehaviour
    {
        public enum Action { MeleeAttack, CastTechnique, ToggleMeditation, Interact, Sleep }
        public Action action;
        public Button button;

        void Reset() { button = GetComponent<Button>(); }

        void Start()
        {
            if (button == null) button = GetComponent<Button>();
            if (button != null) button.onClick.AddListener(OnClick);
        }

        void OnClick()
        {
            var combat = FindObjectOfType<PlayerCombat>();
            var med = FindObjectOfType<WildernessCultivation.Cultivation.MeditationAction>();
            var interact = FindObjectOfType<InteractAction>();
            var sleep = FindObjectOfType<SleepAction>();
            switch (action)
            {
                case Action.MeleeAttack:      combat?.TryMeleeAttack(); break;
                case Action.CastTechnique:    combat?.TryCastSkill(); break;
                case Action.ToggleMeditation: med?.Toggle(); break;
                case Action.Interact:         interact?.TryInteract(); break;
                case Action.Sleep:
                    if (sleep != null && !sleep.IsSleeping) sleep.TrySleep();
                    else sleep?.Wake();
                    break;
            }
        }
    }
}
