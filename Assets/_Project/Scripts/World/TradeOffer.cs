using System;
using UnityEngine;
using WildernessCultivation.Items;

namespace WildernessCultivation.World
{
    /// <summary>
    /// 1 giao dịch mà <see cref="VendorNPC"/> chào bán. Game hiện không có currency —
    /// pattern barter: "đưa N món X để nhận M món Y". Designer cấu hình từ Inspector.
    ///
    /// <para><b>Stock</b>: <see cref="stock"/> = -1 → infinite (vendor tái sinh vô hạn).
    /// Stock &gt; 0 → decrement mỗi trade thành công. 0 → refuse trade với lý do "hết hàng".</para>
    ///
    /// <para><b>Runtime state</b>: <see cref="stock"/> mutable (VendorNPC capture/restore
    /// qua ISaveable). Designer đặt <see cref="stock"/> ở Inspector = giá trị khởi tạo;
    /// ReloadStock() reset về giá trị designer đã set (dùng cho daily restock event).</para>
    /// </summary>
    [Serializable]
    public class TradeOffer
    {
        [Header("Player đưa (consume)")]
        public ItemSO receiveItem;
        [Min(1)] public int receiveCount = 1;

        [Header("Player nhận (add)")]
        public ItemSO giveItem;
        [Min(1)] public int giveCount = 1;

        [Header("Stock")]
        [Tooltip("Số lần trade còn có thể thực hiện. -1 = vô hạn (vendor tái sinh).")]
        public int stock = -1;

        [Tooltip("Giá trị initialStock để ReloadStock() reset về (vd daily restock). Gán lúc bootstrap.")]
        [HideInInspector] public int initialStock = -1;

        /// <summary>True nếu offer còn có thể thực hiện (infinite hoặc stock &gt; 0).</summary>
        public bool HasStock => stock < 0 || stock > 0;

        /// <summary>Capture initialStock = stock hiện tại. Gọi lúc Awake để nhớ designer value.</summary>
        public void CaptureInitialStock() => initialStock = stock;

        /// <summary>Reset stock về initialStock (daily restock event).</summary>
        public void ReloadStock() => stock = initialStock;

        /// <summary>Decrement stock nếu không infinite. No-op khi &lt; 0.</summary>
        public void ConsumeStock()
        {
            if (stock > 0) stock--;
        }
    }
}
