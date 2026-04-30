using System;
using WildernessCultivation.Cultivation;
using WildernessCultivation.World;

namespace WildernessCultivation.Core
{
    /// <summary>
    /// Static event hub cho domain events — subscriber không cần ref trực tiếp
    /// tới publisher (PlayerStats, RealmSystem, TimeManager…). Mục tiêu R4:
    /// decouple UI / Audio / Quest / Achievement khỏi character lifecycle để
    /// NPC humanoid (R5) cũng fire chung 1 hub.
    ///
    /// <para><b>Pattern bắt buộc:</b></para>
    /// <list type="bullet">
    /// <item>Publisher gọi <c>GameEvents.RaiseXxx(...)</c> — KHÔNG <c>OnXxx?.Invoke</c>
    /// trực tiếp ngoài hub.</item>
    /// <item>Subscriber subscribe trong <c>OnEnable</c>, unsubscribe trong <c>OnDisable</c>
    /// (KHÔNG <c>OnDestroy</c>) để tránh duplicate khi component bị tái dùng.</item>
    /// <item>EditMode/PlayMode test gọi <see cref="ClearAllSubscribers"/> trong
    /// <c>SetUp</c>/<c>TearDown</c> — static event KHÔNG tự reset giữa test, để dây
    /// ⇒ NRE khi GameObject cũ destroyed.</item>
    /// </list>
    ///
    /// <para><b>Coexistence với instance event:</b> publisher hiện vẫn fire instance event
    /// (vd <c>PlayerStats.OnDeath</c>) song song với <see cref="OnPlayerDied"/> — code cũ
    /// dùng instance event KHÔNG break. Subscriber mới prefer static hub.</para>
    ///
    /// <para><b>Coexistence với <see cref="Combat.CombatEvents"/>:</b> CombatEvents giữ
    /// nguyên cho damage / hit feedback (UI camera shake, damage number). GameEvents tập
    /// trung vào lifecycle / progression / world state — khác concern.</para>
    /// </summary>
    public static class GameEvents
    {
        // ===== Player lifecycle =====

        /// <summary>Player chết (HP &lt;= 0). Subscribed by death screen UI, audio,
        /// quest "fail on death", achievement counter.</summary>
        public static event Action OnPlayerDied;

        /// <summary>Player respawn (sau permadeath reload, hồi sinh từ pháp bảo…).
        /// Subscribed by HUD reset, tutorial replay.</summary>
        public static event Action OnPlayerRespawned;

        /// <summary>HP/Hunger/Thirst/SAN/Mana đổi. Tần suất cao — subscriber nên
        /// throttle nếu cần. Subscribed by HUD bar, save dirty flag.</summary>
        public static event Action OnPlayerStatsChanged;

        public static void RaisePlayerDied() => OnPlayerDied?.Invoke();
        public static void RaisePlayerRespawned() => OnPlayerRespawned?.Invoke();
        public static void RaisePlayerStatsChanged() => OnPlayerStatsChanged?.Invoke();

        // ===== Cultivation progression =====

        /// <summary>Player advance lên tier mới. <c>newTier</c> là index sau khi đột phá.
        /// Subscribed by realm UI, fanfare audio, achievement.</summary>
        public static event Action<int> OnRealmAdvanced;

        /// <summary>Player thử đột phá. <c>success</c> = true nếu pass. Subscribed by
        /// breakthrough UI overlay, audio (success/fail sfx), MetaStats counter.</summary>
        public static event Action<bool> OnBreakthroughAttempted;

        /// <summary>Player thử khai mở (awakening). Result chứa eligibility + grade rolled.
        /// Subscribed by awakening overlay, status HUD, MetaStats.</summary>
        public static event Action<AwakenResult> OnAwakeningAttempted;

        public static void RaiseRealmAdvanced(int newTier) => OnRealmAdvanced?.Invoke(newTier);
        public static void RaiseBreakthroughAttempted(bool success) => OnBreakthroughAttempted?.Invoke(success);
        public static void RaiseAwakeningAttempted(AwakenResult result) => OnAwakeningAttempted?.Invoke(result);

        // ===== Inventory =====

        /// <summary>Player inventory thay đổi (add/remove/swap/durability tick).
        /// Subscribed by inventory UI, encumbrance display, weight HUD.</summary>
        public static event Action OnPlayerInventoryChanged;

        public static void RaisePlayerInventoryChanged() => OnPlayerInventoryChanged?.Invoke();

        // ===== Time / world state =====

        /// <summary>Day/night transition: bắt đầu ngày mới (currentTime01 vượt 0.25 từ dưới).
        /// Subscribed by ambient audio swap, mob spawner reset, fire dim.</summary>
        public static event Action OnDayStarted;

        /// <summary>Bắt đầu đêm (currentTime01 vượt 0.75). Subscribed by ambient audio,
        /// mob aggression boost, sanity decay tick start.</summary>
        public static event Action OnNightStarted;

        /// <summary>Thời tiết đổi (Clear / Rain / Storm). Subscribed by particle system,
        /// audio rain layer, wetness tier UI.</summary>
        public static event Action<Weather> OnWeatherChanged;

        public static void RaiseDayStarted() => OnDayStarted?.Invoke();
        public static void RaiseNightStarted() => OnNightStarted?.Invoke();
        public static void RaiseWeatherChanged(Weather weather) => OnWeatherChanged?.Invoke(weather);

        // ===== NPC humanoid (R5 follow-up) =====

        /// <summary>Player tương tác với vendor — UI mở trade panel. Arg là VendorNPC;
        /// subscriber cast về <c>WildernessCultivation.World.VendorNPC</c> khi cần
        /// (tránh circular namespace reference, event dùng <see cref="object"/>).</summary>
        public static event Action<object> OnVendorOpened;

        /// <summary>Trade hoàn thành — arg1 = VendorNPC, arg2 = offerIndex.
        /// Subscribed by quest ("mua 3 ItemX từ vendor Y"), audio (coin sfx), achievement.</summary>
        public static event Action<object, int> OnTradeCompleted;

        public static void RaiseVendorOpened(object vendor) => OnVendorOpened?.Invoke(vendor);
        public static void RaiseTradeCompleted(object vendor, int offerIndex) => OnTradeCompleted?.Invoke(vendor, offerIndex);

        /// <summary>Companion chuyển mode Follow ↔ Stay. Arg = CompanionNPC.
        /// Subscribed by HUD (pet portrait), quest ("escort companion to X"), audio bark.</summary>
        public static event Action<object> OnCompanionModeChanged;

        public static void RaiseCompanionModeChanged(object companion) => OnCompanionModeChanged?.Invoke(companion);

        // ===== Test helper =====

        /// <summary>Reset toàn bộ subscriber. PHẢI gọi trong test SetUp/TearDown để tránh
        /// dây subscriber giữa test (static event không tự reset).</summary>
        public static void ClearAllSubscribers()
        {
            OnPlayerDied = null;
            OnPlayerRespawned = null;
            OnPlayerStatsChanged = null;
            OnRealmAdvanced = null;
            OnBreakthroughAttempted = null;
            OnAwakeningAttempted = null;
            OnPlayerInventoryChanged = null;
            OnDayStarted = null;
            OnNightStarted = null;
            OnWeatherChanged = null;
            OnVendorOpened = null;
            OnTradeCompleted = null;
            OnCompanionModeChanged = null;
        }
    }
}
