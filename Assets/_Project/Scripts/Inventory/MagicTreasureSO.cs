using UnityEngine;
using WildernessCultivation.Player;

namespace WildernessCultivation.Items
{
    /// <summary>
    /// Pháp bảo — item có thể "sử dụng" để kích hoạt 1 hiệu ứng (heal, buff, panic-button).
    /// Khác item Consumable thông thường ở chỗ pháp bảo có cooldown riêng và có thể *không* tiêu hao mỗi lần dùng
    /// (charge-based hoặc vĩnh viễn). Tạo asset: Right-click > Create > WildernessCultivation > Magic Treasure.
    /// </summary>
    [CreateAssetMenu(menuName = "WildernessCultivation/Magic Treasure", fileName = "Treasure_New")]
    public class MagicTreasureSO : ItemSO
    {
        public enum TreasureKind
        {
            HealBurst,      // hồi HP/SAN tức thời
            ManaBurst,      // hồi mana
            BreakthroughAid,// tăng chance đột phá tiếp theo
            ShieldAura,     // miễn dame trong vài giây
        }

        [Header("Treasure")]
        public TreasureKind kind = TreasureKind.HealBurst;
        public float magnitude = 50f;       // hp/mana/duration tùy kind
        public float secondaryMagnitude = 0f; // vd HealBurst: SAN; ShieldAura: duration; BreakthroughAid: bonus chance
        public float cooldown = 30f;
        [Tooltip(">=1 = số lần dùng còn lại. 0 hoặc âm = unlimited (vd pháp bảo vĩnh viễn).")]
        public int chargesPerInstance = 1;
        [Tooltip("Tiêu hao 1 charge mỗi lần dùng. False = pháp bảo vĩnh viễn không bao giờ hết.")]
        public bool consumeChargeOnUse = true;

        public bool Activate(PlayerStats stats, RealmSystemHook realmHook)
        {
            if (stats == null) return false;
            switch (kind)
            {
                case TreasureKind.HealBurst:
                    stats.Heal(magnitude);
                    if (secondaryMagnitude > 0f) stats.RestoreSanity(secondaryMagnitude);
                    break;
                case TreasureKind.ManaBurst:
                    stats.AddMana(magnitude);
                    break;
                case TreasureKind.BreakthroughAid:
                    realmHook?.AddTemporaryBreakthroughBonus(magnitude, secondaryMagnitude > 0 ? secondaryMagnitude : 60f);
                    break;
                case TreasureKind.ShieldAura:
                    stats.AddShield(magnitude, secondaryMagnitude > 0 ? secondaryMagnitude : 5f);
                    break;
            }
            return true;
        }

        /// <summary>
        /// Hook nhẹ để MagicTreasure tương tác với <c>RealmSystem</c> mà không tạo coupling Items→Cultivation
        /// (ItemSO không nên reference Cultivation namespace trực tiếp). MagicTreasureAction sẽ truyền
        /// 1 implementation cụ thể vào.
        /// </summary>
        public interface RealmSystemHook
        {
            void AddTemporaryBreakthroughBonus(float bonusChance, float durationSec);
        }
    }
}
