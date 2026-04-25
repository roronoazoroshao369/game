using UnityEngine;

namespace WildernessCultivation.Player.Status
{
    public enum StatusEffectType { Poison, Bleeding, Sickness, Burn, Freeze, Stun, Buff }

    /// <summary>
    /// Định nghĩa 1 hiệu ứng trạng thái áp lên Player (tick damage / debuff theo thời gian).
    /// Tạo asset: Right-click > Create > WildernessCultivation > Status Effect.
    /// </summary>
    [CreateAssetMenu(menuName = "WildernessCultivation/Status Effect", fileName = "Status_New")]
    public class StatusEffectSO : ScriptableObject
    {
        [Header("Identity")]
        public string effectId;
        public string displayName;
        [TextArea] public string description;
        public StatusEffectType type = StatusEffectType.Poison;
        public Sprite icon;
        public Color tintColor = Color.white;

        [Header("Tick (per tick)")]
        [Tooltip("Khoảng giây giữa 2 lần tick. 1 = mỗi giây 1 tick.")]
        public float tickIntervalSec = 1f;
        public float hpDamagePerTick = 0f;
        public float sanityDamagePerTick = 0f;
        public float hungerDamagePerTick = 0f;
        public float thirstDamagePerTick = 0f;
        public float manaDamagePerTick = 0f;

        [Header("Modifiers (áp dụng liên tục khi active)")]
        [Tooltip("Multiplier vào moveSpeed (vd Freeze=0.5).")]
        public float moveSpeedMultiplier = 1f;
        [Tooltip("Multiplier vào damage trừ vào player (Burn extra).")]
        public float incomingDamageMultiplier = 1f;

        [Header("Duration")]
        [Tooltip("Thời gian effect kéo dài (giây). 0 = vĩnh viễn cho đến khi clear.")]
        public float defaultDurationSec = 10f;

        [Header("Stack rule")]
        [Tooltip("True = re-apply cộng dồn duration. False = giữ duration max(cur,new).")]
        public bool extendDurationOnReapply = false;
    }
}
