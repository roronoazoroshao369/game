using System.Collections.Generic;
using UnityEngine;
using WildernessCultivation.Core;
using WildernessCultivation.Items;
using WildernessCultivation.Player;
using WildernessCultivation.UI;

namespace WildernessCultivation.World
{
    /// <summary>
    /// Mộ Phần — di vật của 1 đời player trước. Spawn từ <see cref="Graveyard"/>
    /// data ở <see cref="WorldGenerator"/>. Player E-interact để drain toàn bộ
    /// items vào inventory (skip slot đầy). Sau khi rỗng → tự destroy + remove
    /// entry khỏi Graveyard file.
    ///
    /// Visual: dùng sprite tone tối + label "Mộ Phần" — nhìn thấy trên minimap
    /// camera do không cần custom layer.
    /// </summary>
    public class Tombstone : MonoBehaviour, IInteractable
    {
        [Tooltip("Tombstone id — match với entry trong graveyard.json.")]
        public string tombstoneId;

        [Tooltip("Snapshot items còn lại trong tombstone. Drain dần khi player interact.")]
        public List<InventorySlotData> items = new();

        [Tooltip("Lookup item theo itemId khi spawn item vào player inventory.")]
        public ItemDatabase itemDatabase;

        public int previousLifeRealmTier;
        public bool previousLifeWasAwakened;
        public int daySurvived;

        public string InteractLabel => "Mộ Phần";
        public bool CanInteract(GameObject actor) => actor != null && items != null && items.Count > 0;

        void Awake()
        {
            // Beacon gray cho minimap. Phải gọi Initialize sau AddComponent vì
            // PlayMode AddComponent fire MinimapBeacon.Awake đồng bộ → child sprite
            // tạo với default yellow nếu chỉ assign field sau đó.
            var beacon = GetComponent<MinimapBeacon>();
            if (beacon == null) beacon = gameObject.AddComponent<MinimapBeacon>();
            beacon.Initialize(new Color(0.55f, 0.55f, 0.6f, 0.9f), 2f, "TombstoneBeacon");
        }

        /// <summary>Khởi tạo tombstone runtime từ <see cref="TombstoneData"/>.</summary>
        public void Initialize(TombstoneData data, ItemDatabase database)
        {
            if (data == null) return;
            tombstoneId = data.id;
            previousLifeRealmTier = data.previousLifeRealmTier;
            previousLifeWasAwakened = data.previousLifeWasAwakened;
            daySurvived = data.daySurvived;
            itemDatabase = database;
            items = new List<InventorySlotData>(data.items != null ? data.items.Count : 0);
            if (data.items != null)
            {
                foreach (var s in data.items)
                {
                    if (s == null || string.IsNullOrEmpty(s.itemId) || s.count <= 0) continue;
                    items.Add(new InventorySlotData
                    {
                        itemId = s.itemId,
                        count = s.count,
                        freshRemaining = s.freshRemaining,
                        durability = s.durability,
                    });
                }
            }
        }

        public bool Interact(GameObject actor)
        {
            if (actor == null) return false;
            var inv = actor.GetComponentInParent<Inventory>();
            if (inv == null)
            {
                Debug.LogWarning("[Tombstone] Player không có Inventory — bỏ qua.");
                return false;
            }
            if (itemDatabase == null)
            {
                Debug.LogWarning("[Tombstone] ItemDatabase null — không thể giải mã itemId.");
                return false;
            }

            // Drain từ ngược về (xoá inplace an toàn).
            for (int i = items.Count - 1; i >= 0; i--)
            {
                var snap = items[i];
                var so = itemDatabase.GetById(snap.itemId);
                if (so == null)
                {
                    Debug.LogWarning($"[Tombstone] ItemDatabase không có id='{snap.itemId}' — bỏ qua.");
                    items.RemoveAt(i);
                    continue;
                }

                int leftover = inv.Add(so, snap.count);
                int consumed = snap.count - leftover;
                if (consumed > 0)
                {
                    // TODO restore freshness/durability của slot vừa add (tương tự
                    // SaveLoadController.RestoreInventory). Phase 1: skip để giữ patch nhỏ.
                    snap.count = leftover;
                    items[i] = snap;
                }
                if (snap.count <= 0) items.RemoveAt(i);
                if (leftover > 0)
                {
                    // Inventory đầy — dừng drain, giữ phần còn lại trên tombstone.
                    break;
                }
            }

            if (items.Count == 0)
            {
                if (!string.IsNullOrEmpty(tombstoneId))
                    Graveyard.Remove(tombstoneId);
                // EditMode test gọi Interact trực tiếp — Application.isPlaying == false.
                // Destroy() throw trong EditMode; phải dùng DestroyImmediate. Production
                // (PlayMode) vẫn dùng Destroy để khỏi block frame.
                if (Application.isPlaying) Destroy(gameObject);
                else DestroyImmediate(gameObject);
            }
            return true;
        }
    }
}
