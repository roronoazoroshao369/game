using UnityEngine;
using UnityEngine.UI;

namespace WildernessCultivation.UI
{
    /// <summary>
    /// Minimap controller. Sở hữu RenderTexture (chia sẻ giữa minimap camera và RawImage),
    /// quản lý lifecycle (tạo trong Awake, release trong OnDestroy để tránh leak GPU mem).
    ///
    /// Minimap camera được parent vào player → tự follow. Component này gắn lên RawImage
    /// trong UI canvas; <see cref="minimapCamera"/> được BootstrapWizard gán reference khi
    /// dựng scene.
    ///
    /// Ortho camera nhìn xuống (z = -20) → mọi sprite world-space (player, mob, terrain,
    /// resource node) hiển thị nguyên bản. Không dùng custom layer để tránh phải sửa
    /// TagManager.asset (project-wide setting).
    /// </summary>
    [RequireComponent(typeof(RawImage))]
    public class MinimapController : MonoBehaviour
    {
        [Header("Refs")]
        public Camera minimapCamera;

        [Header("Render texture")]
        public int textureSize = 256;
        public FilterMode filterMode = FilterMode.Bilinear;

        RawImage rawImage;
        RenderTexture rt;

        void Awake()
        {
            rawImage = GetComponent<RawImage>();
            // Create render texture once, share between camera target và RawImage source.
            rt = new RenderTexture(textureSize, textureSize, 16, RenderTextureFormat.Default)
            {
                name = "MinimapRT",
                filterMode = filterMode
            };
            rt.Create();
            rawImage.texture = rt;
            if (minimapCamera != null) minimapCamera.targetTexture = rt;
        }

        void OnDestroy()
        {
            if (minimapCamera != null && minimapCamera.targetTexture == rt)
                minimapCamera.targetTexture = null;
            if (rt != null)
            {
                rt.Release();
                Object.Destroy(rt);
                rt = null;
            }
        }
    }
}
