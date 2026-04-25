using System.Collections;
using UnityEngine;
using WildernessCultivation.Items;
using WildernessCultivation.World;

namespace WildernessCultivation.Player
{
    /// <summary>
    /// Câu cá — cần <see cref="rodItem"/> trong inventory + đứng gần <see cref="FishingSpot"/>.
    /// Cast thời gian random theo <c>spot.castTimeSeconds</c>; trong lúc cast không di chuyển
    /// (gọi <see cref="CancelCast"/> để huỷ — không tốn durability). Hoàn thành drop random 1
    /// loot từ <c>spot.lootTable</c> + tốn 1 durability (nếu rod có hasDurability).
    /// </summary>
    public class FishingAction : MonoBehaviour
    {
        [Tooltip("Item cần câu (vd Cần Câu Tre). Nên có hasDurability=true.")]
        public ItemSO rodItem;
        public Inventory inventory;
        public PlayerController controller;

        [Header("Input (PC)")]
        public KeyCode castKey = KeyCode.F;

        public bool IsCasting { get; private set; }
        Coroutine castCo;

        void Awake()
        {
            if (inventory == null) inventory = GetComponentInParent<Inventory>();
            if (controller == null) controller = GetComponent<PlayerController>();
        }

        void Update()
        {
            if (Input.GetKeyDown(castKey))
            {
                if (IsCasting) CancelCast();
                else TryStartCast();
            }
        }

        public bool TryStartCast()
        {
            if (IsCasting) return false;
            if (rodItem == null || inventory == null) { Debug.Log("[Fishing] Cần ItemSO rod + Inventory."); return false; }
            int rodSlot = FindRodSlot();
            if (rodSlot < 0) { Debug.Log("[Fishing] Không có cần câu trong inventory."); return false; }

            var spot = FishingSpot.NearestSpotInRange(transform.position);
            if (spot == null) { Debug.Log("[Fishing] Đứng gần FishingSpot rồi cast lại."); return false; }

            float t = Random.Range(spot.castTimeSeconds.x, spot.castTimeSeconds.y);
            castCo = StartCoroutine(CastCoroutine(spot, rodSlot, t));
            IsCasting = true;
            return true;
        }

        public void CancelCast()
        {
            if (!IsCasting) return;
            if (castCo != null) StopCoroutine(castCo);
            castCo = null;
            IsCasting = false;
            Debug.Log("[Fishing] Huỷ cast.");
        }

        IEnumerator CastCoroutine(FishingSpot spot, int rodSlot, float seconds)
        {
            Debug.Log($"[Fishing] Đang câu… ({seconds:0.0}s)");
            // Realtime: tránh sleep timeScale làm câu cá xong sau 0.5s
            yield return new WaitForSecondsRealtime(seconds);

            var entry = spot.RollLoot();
            if (entry.HasValue && entry.Value.item != null)
            {
                int n = Random.Range(entry.Value.min, entry.Value.max + 1);
                if (n > 0) inventory.Add(entry.Value.item, n);
                Debug.Log($"[Fishing] Câu được {n}x {entry.Value.item.displayName}");
            }
            else Debug.Log("[Fishing] Không có loot table — không câu được gì.");

            // Hao mòn cần — re-validate slot vì inventory có thể đã shuffle trong lúc chờ
            int validSlot = rodSlot;
            if (validSlot < 0 || validSlot >= inventory.Slots.Count
                || inventory.Slots[validSlot].item != rodItem)
            {
                validSlot = FindRodSlot();
            }
            if (validSlot >= 0) inventory.UseDurability(validSlot);
            else Debug.Log("[Fishing] Cần câu không còn trong inventory — bỏ qua durability cost.");

            IsCasting = false;
            castCo = null;
        }

        int FindRodSlot()
        {
            for (int i = 0; i < inventory.Slots.Count; i++)
            {
                var s = inventory.Slots[i];
                if (s.item == rodItem && !s.IsBroken) return i;
            }
            return -1;
        }
    }
}
