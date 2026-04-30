using UnityEngine;
using WildernessCultivation.Core;
using UnityEngine.UI;
using WildernessCultivation.Player;

namespace WildernessCultivation.UI
{
    /// <summary>
    /// Nút skill mobile — bind 1 hành động (attack / cast / meditate).
    /// </summary>
    public class SkillButton : MonoBehaviour
    {
        public enum Action { MeleeAttack, CastTechnique, ToggleMeditation, Interact, Sleep, UseMagicTreasure, Breakthrough, ToggleTorch, Dodge }
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
            var combat = ServiceLocator.Get<PlayerCombat>();
            var med = ServiceLocator.Get<WildernessCultivation.Cultivation.MeditationAction>();
            var interact = ServiceLocator.Get<InteractAction>();
            var sleep = ServiceLocator.Get<SleepAction>();
            var treasure = ServiceLocator.Get<MagicTreasureAction>();
            var realm = ServiceLocator.Get<WildernessCultivation.Cultivation.RealmSystem>();
            switch (action)
            {
                case Action.MeleeAttack: combat?.TryMeleeAttack(); break;
                case Action.CastTechnique: combat?.TryCastSkill(); break;
                case Action.ToggleMeditation: med?.Toggle(); break;
                case Action.Interact: interact?.TryInteract(); break;
                case Action.Sleep:
                    if (sleep != null && !sleep.IsSleeping) sleep.TrySleep();
                    else sleep?.Wake();
                    break;
                case Action.UseMagicTreasure: treasure?.TryUse(); break;
                case Action.Breakthrough:
                    {
                        var stats = ServiceLocator.Get<PlayerStats>();
                        if (stats == null || stats.IsAwakened) realm?.TryBreakthrough();
                        break;
                    }
                case Action.ToggleTorch:
                    {
                        var torch = ServiceLocator.Get<TorchAction>();
                        torch?.Toggle();
                        break;
                    }
                case Action.Dodge:
                    {
                        var dodge = ServiceLocator.Get<DodgeAction>();
                        dodge?.TryDodge();
                        break;
                    }
            }
        }
    }
}
