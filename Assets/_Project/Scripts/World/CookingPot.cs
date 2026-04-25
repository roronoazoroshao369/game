using System.Collections.Generic;
using UnityEngine;
using WildernessCultivation.Crafting;

namespace WildernessCultivation.World
{
    /// <summary>
    /// Nồi nấu nhiều bước (canh / hầm / dược thiện) — placement: đặt trên gameObject có
    /// <see cref="CraftStationMarker"/> set <see cref="CraftStation.CookingPot"/>.
    /// Nồi chỉ active (gate) khi có Campfire đang cháy gần đó (mặc định 1.5m) — phải có lửa
    /// thì mới đun được.
    /// </summary>
    [RequireComponent(typeof(CraftStationMarker))]
    public class CookingPot : MonoBehaviour, IInteractable, IStationGate
    {
        [Tooltip("Bán kính tìm Campfire để 'mượn lửa' đun nồi.")]
        public float requiredHeatRadius = 1.5f;

        public bool StationActive
        {
            get
            {
                foreach (var c in Campfire.Active)
                {
                    if (c == null || !c.IsLit) continue;
                    float sqr = ((Vector2)transform.position - (Vector2)c.transform.position).sqrMagnitude;
                    if (sqr <= requiredHeatRadius * requiredHeatRadius) return true;
                }
                return false;
            }
        }

        public string InteractLabel => StationActive ? "Mở nồi (chế biến)" : "Cần đặt gần lửa trại đang cháy";
        public bool CanInteract(GameObject actor) => StationActive;
        public bool Interact(GameObject actor) => true; // UI Crafting sẽ tự lọc recipe theo trạm trong range

        void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.4f, 0.2f, 0.4f);
            Gizmos.DrawWireSphere(transform.position, requiredHeatRadius);
        }
    }
}
