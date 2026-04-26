using UnityEngine;

namespace WildernessCultivation.Crafting
{
    /// <summary>
    /// Gắn vào prefab lửa trại / lò luyện đan để <see cref="CraftingSystem"/>
    /// detect được. Marker mặc định luôn available; các trạm có yêu cầu nhiên
    /// liệu (Campfire, AlchemyFurnace) sẽ tự gắn 1 component sibling implement
    /// <see cref="IStationGate"/> để báo không khả dụng khi tắt.
    ///
    /// Tách thành file riêng (thay vì share file với <c>CraftingSystem</c>) để
    /// Unity match được class name với file name khi serialize MonoScript
    /// reference trong prefab / scene. Để chung file gây ra
    /// <c>m_Script: {fileID: 0}</c> trên prefab Campfire / Workbench → log
    /// "Script attached to '...' is missing or no valid script is attached"
    /// khi build.
    /// </summary>
    public class CraftStationMarker : MonoBehaviour
    {
        public CraftStation station = CraftStation.Campfire;

        public bool IsAvailable
        {
            get
            {
                var gate = GetComponent<IStationGate>();
                return gate == null || gate.StationActive;
            }
        }
    }

    /// <summary>Implement trên cùng GameObject với <see cref="CraftStationMarker"/>
    /// nếu trạm có thể tắt/bật (ví dụ Campfire chỉ "active" khi còn nhiên liệu).</summary>
    public interface IStationGate
    {
        bool StationActive { get; }
    }
}
