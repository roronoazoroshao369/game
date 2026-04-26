using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using WildernessCultivation.UI;

namespace WildernessCultivation.Tests.EditMode
{
    /// <summary>
    /// EditMode tests cho <see cref="MinimapController"/> — verify lifecycle của
    /// RenderTexture (tạo trong Awake, release trong OnDestroy).
    /// </summary>
    public class MinimapControllerTests
    {
        GameObject viewGo;
        GameObject camGo;
        MinimapController ctrl;
        Camera cam;

        [SetUp]
        public void Setup()
        {
            camGo = new GameObject("MinimapCam");
            cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.enabled = false; // không cần render thực sự cho test

            // EditMode does NOT auto-fire MonoBehaviour.Awake on AddComponent
            // OR on SetActive(true). Configure fields, then invoke Awake
            // explicitly via TestHelpers so wiring (RT creation + camera
            // targetTexture + RawImage.texture) reflects the configured fields.
            viewGo = new GameObject("View");
            viewGo.AddComponent<RectTransform>();
            viewGo.AddComponent<RawImage>();
            ctrl = viewGo.AddComponent<MinimapController>();
            ctrl.minimapCamera = cam;
            ctrl.textureSize = 64;
            TestHelpers.Boot(ctrl);
        }

        [TearDown]
        public void Teardown()
        {
            if (viewGo != null) Object.DestroyImmediate(viewGo);
            if (camGo != null) Object.DestroyImmediate(camGo);
        }

        [Test]
        public void Awake_CreatesRenderTexture_AssignsToCameraAndRawImage()
        {
            // Awake đã chạy bởi AddComponent.
            var raw = viewGo.GetComponent<RawImage>();
            Assert.IsNotNull(raw.texture, "RawImage.texture must be assigned");
            Assert.IsTrue(raw.texture is RenderTexture, "RawImage.texture must be a RenderTexture");
            Assert.AreSame(raw.texture, cam.targetTexture,
                "Camera target texture must be the same RenderTexture as the RawImage source");
        }

        [Test]
        public void RenderTextureSize_MatchesTextureSizeField()
        {
            var rt = (RenderTexture)viewGo.GetComponent<RawImage>().texture;
            Assert.AreEqual(64, rt.width);
            Assert.AreEqual(64, rt.height);
        }

        [Test]
        public void OnDestroy_DetachesCameraTarget_ReleasesRT()
        {
            var raw = viewGo.GetComponent<RawImage>();
            var rt = (RenderTexture)raw.texture;
            Assert.IsNotNull(rt);

            // EditMode does NOT auto-fire OnDestroy on DestroyImmediate for
            // user MonoBehaviours (matching the manual Awake in SetUp) —
            // invoke explicitly so the cleanup we want to verify actually
            // runs, then destroy the GO.
            TestHelpers.InvokeLifecycle(ctrl, "OnDestroy");
            Object.DestroyImmediate(viewGo);
            viewGo = null;

            // Camera phải được clear targetTexture, không còn ref tới RT đã dispose.
            Assert.IsNull(cam.targetTexture);
        }
    }
}
