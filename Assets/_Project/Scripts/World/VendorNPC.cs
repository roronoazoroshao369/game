using System.Collections.Generic;
using UnityEngine;
using WildernessCultivation.Combat;
using WildernessCultivation.Core;
using WildernessCultivation.Items;
using WildernessCultivation.Player.Stats;

namespace WildernessCultivation.World
{
    /// <summary>
    /// NPC humanoid exemplar — Vendor (R5 follow-up). Chứng minh pattern:
    /// <list type="number">
    /// <item><b>Composition reuse</b> (DESIGN_PRINCIPLES rule 1 + 6): reuse pure stat
    /// component từ R1 (<see cref="HealthComponent"/> + <see cref="InvulnerabilityComponent"/>)
    /// — vendor cần HP + i-frame nhưng KHÔNG cần Hunger/Thirst/Wetness/Thermal của Player.</item>
    /// <item><b>CharacterBase polymorphic view</b> (R5): vendor là <see cref="CharacterBase"/>
    /// → code damage / UI HP bar generic hoạt động với vendor y như player/mob.</item>
    /// <item><b>IInteractable</b>: player E-key → open trade UI (qua <see cref="GameEvents.OnVendorOpened"/>).</item>
    /// <item><b>ISaveable dispatcher</b> (R6): vendor tự save stock per offer, không động
    /// <see cref="SaveLoadController"/>.</item>
    /// <item><b>GameEvents hub</b> (R4): fire <see cref="GameEvents.OnVendorOpened"/>
    /// + <see cref="GameEvents.OnTradeCompleted"/> — UI/Quest/Audio subscribe.</item>
    /// </list>
    ///
    /// <para><b>Barter trade</b>: game không có currency. Designer cấu hình list
    /// <see cref="TradeOffer"/> "đưa N món X nhận M món Y". Player call
    /// <see cref="TryExecuteTrade"/> atomic (verify rồi mutate).</para>
    /// </summary>
    public class VendorNPC : CharacterBase, IInteractable, ISaveable
    {
        [Header("Identity")]
        [Tooltip("ID ổn định dùng cho save lookup. Mỗi vendor instance phải unique.")]
        public string vendorId = "vendor_generic";
        [Tooltip("Tên hiển thị trên UI prompt.")]
        public string displayName = "Thương Nhân";

        [Header("Health")]
        [Tooltip("True = vendor immortal (default). False = damage bình thường.")]
        public bool invulnerable = true;
        [Tooltip("Max HP nếu vulnerable. Ignored khi invulnerable=true.")]
        public float maxHP = 100f;

        [Header("Trade")]
        [Tooltip("Danh sách offer. Designer cấu hình từ Inspector.")]
        public List<TradeOffer> offers = new();

        // Pure stat components reuse từ R1.
        HealthComponent health;
        InvulnerabilityComponent invuln;

        // ===== CharacterBase override =====
        public override float CurrentHP => health != null ? health.HP : 0f;
        public override float CurrentMaxHP => health != null ? health.maxHP : 0f;
        public override bool IsDead => health != null && health.IsDead;

        // ===== IInteractable =====
        public string InteractLabel => $"Giao dịch với {displayName}";
        public bool CanInteract(GameObject actor) => actor != null && !IsDead;

        // ===== ISaveable =====
        public string SaveKey => $"World/Vendor/{vendorId}";
        public int Order => 70; // Sau Inventory (60). Stock độc lập, không cần fixup.

        void Awake()
        {
            health = gameObject.GetComponent<HealthComponent>() ?? gameObject.AddComponent<HealthComponent>();
            invuln = gameObject.GetComponent<InvulnerabilityComponent>() ?? gameObject.AddComponent<InvulnerabilityComponent>();

            health.maxHP = maxHP;
            health.HP = maxHP;

            if (invulnerable) invuln.InvulnerableUntil = float.MaxValue;

            foreach (var o in offers) o?.CaptureInitialStock();
        }

        void OnEnable()
        {
            SaveRegistry.RegisterSaveable(this);
        }

        void OnDisable()
        {
            SaveRegistry.UnregisterSaveable(this);
        }

        public override void TakeDamage(float amount, GameObject source)
        {
            if (health == null || health.IsDead) return;
            if (invuln != null && invuln.IsInvulnerable) return;
            health.TakeRaw(amount);
            // Vendor death: hook sau (vd fail quest, remove offers). Hiện exemplar không
            // trigger chết vì default invulnerable.
        }

        /// <summary>Handshake interact — UI subscribe <see cref="GameEvents.OnVendorOpened"/>
        /// để mở trade panel. Actual trade execute qua <see cref="TryExecuteTrade"/>.</summary>
        public bool Interact(GameObject actor)
        {
            if (!CanInteract(actor)) return false;
            GameEvents.RaiseVendorOpened(this);
            return true;
        }

        /// <summary>
        /// Atomic trade execute. Verify stock + player có đủ <c>receiveItem</c> trước khi
        /// mutate. Trả về true nếu trade thành công (fire <see cref="GameEvents.OnTradeCompleted"/>).
        ///
        /// <para>Fail conditions:</para>
        /// <list type="bullet">
        /// <item>offerIndex ngoài phạm vi</item>
        /// <item>offer null / config sai (receiveItem hoặc giveItem null)</item>
        /// <item>stock hết (<see cref="TradeOffer.HasStock"/> == false)</item>
        /// <item>player không đủ <c>receiveItem</c></item>
        /// </list>
        /// </summary>
        public bool TryExecuteTrade(int offerIndex, Inventory actorInventory)
        {
            if (actorInventory == null) return false;
            if (offerIndex < 0 || offerIndex >= offers.Count) return false;
            var offer = offers[offerIndex];
            if (offer == null || offer.receiveItem == null || offer.giveItem == null) return false;
            if (!offer.HasStock) return false;
            if (actorInventory.CountOf(offer.receiveItem) < offer.receiveCount) return false;

            // Atomic: consume receive → add give → decrement stock.
            if (!actorInventory.TryConsume(offer.receiveItem, offer.receiveCount)) return false;
            int leftover = actorInventory.Add(offer.giveItem, offer.giveCount);
            if (leftover > 0)
            {
                // Inventory player đầy → rollback consume để không mất item.
                actorInventory.Add(offer.receiveItem, offer.receiveCount);
                return false;
            }
            offer.ConsumeStock();
            GameEvents.RaiseTradeCompleted(this, offerIndex);
            return true;
        }

        public void CaptureState(SaveData data)
        {
            if (data == null) return;
            data.vendors ??= new List<VendorSaveData>();
            var existing = data.vendors.Find(v => v != null && v.vendorId == vendorId);
            var entry = existing ?? new VendorSaveData { vendorId = vendorId };
            entry.stocks.Clear();
            foreach (var o in offers) entry.stocks.Add(o?.stock ?? 0);
            if (existing == null) data.vendors.Add(entry);
        }

        public void RestoreState(SaveData data)
        {
            if (data == null || data.vendors == null) return;
            var entry = data.vendors.Find(v => v != null && v.vendorId == vendorId);
            if (entry == null) return;
            int n = Mathf.Min(entry.stocks.Count, offers.Count);
            for (int i = 0; i < n; i++)
            {
                if (offers[i] != null) offers[i].stock = entry.stocks[i];
            }
        }
    }
}
