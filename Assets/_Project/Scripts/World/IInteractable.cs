using UnityEngine;

namespace WildernessCultivation.World
{
    /// <summary>
    /// Interface chung cho mọi vật thể trong thế giới có thể "tương tác" (uống nước,
    /// ngồi cạnh lửa, mở rương, ngủ, …). Khác với <see cref="WildernessCultivation.Combat.IDamageable"/>
    /// vốn dùng cho việc bị tấn công.
    /// </summary>
    public interface IInteractable
    {
        /// <summary>Gợi ý hiển thị trên UI prompt (vd "Uống", "Sưởi ấm", "Ngủ").</summary>
        string InteractLabel { get; }

        /// <summary>Có sẵn sàng tương tác lúc này không (cooldown, đêm/ngày, ...).</summary>
        bool CanInteract(GameObject actor);

        /// <summary>Thực hiện tương tác. Trả về true nếu hành động đã chạy.</summary>
        bool Interact(GameObject actor);
    }
}
