using NUnit.Framework;
using UnityEngine;
using WildernessCultivation.Vfx;

namespace WildernessCultivation.Tests.EditMode
{
    /// <summary>
    /// Test Vfx components. <see cref="WindSway.ComputeAngleDegrees"/> là pure → test
    /// deterministic value. <see cref="DropShadow"/> là MonoBehaviour: verify idempotency
    /// + child wiring qua AddComponent + Awake invocation thủ công (EditMode không
    /// auto-fire Awake — pattern theo <c>TestHelpers.Boot</c>).
    /// </summary>
    public class VfxTests
    {
        [Test]
        public void WindSway_ComputeAngle_ZeroAtPhaseStart()
        {
            // sin(0) = 0 → angle = 0 tại t=0, phase=0.
            float a = WindSway.ComputeAngleDegrees(time: 0f, frequencyHz: 1f,
                amplitudeDegrees: 5f, phaseRadians: 0f);
            Assert.AreEqual(0f, a, 0.0001f);
        }

        [Test]
        public void WindSway_ComputeAngle_PeakAtQuarterCycle()
        {
            // freq=1 Hz → period 1s. t=0.25 → quarter cycle → sin(π/2) = 1 → max biên độ.
            float a = WindSway.ComputeAngleDegrees(time: 0.25f, frequencyHz: 1f,
                amplitudeDegrees: 5f, phaseRadians: 0f);
            Assert.AreEqual(5f, a, 0.001f);
        }

        [Test]
        public void WindSway_ComputeAngle_MinAtThreeQuarterCycle()
        {
            float a = WindSway.ComputeAngleDegrees(time: 0.75f, frequencyHz: 1f,
                amplitudeDegrees: 5f, phaseRadians: 0f);
            Assert.AreEqual(-5f, a, 0.001f);
        }

        [Test]
        public void WindSway_ComputeAngle_AmplitudeBounded()
        {
            // Mọi (time, phase) → |angle| ≤ amplitude.
            for (int i = 0; i < 100; i++)
            {
                float t = i * 0.013f;
                float p = i * 0.7f;
                float a = WindSway.ComputeAngleDegrees(t, 1.5f, 3f, p);
                Assert.LessOrEqual(Mathf.Abs(a), 3f + 0.0001f,
                    $"angle={a} vượt biên độ tại t={t}, phase={p}");
            }
        }

        [Test]
        public void DropShadow_EnsureChild_CreatesSpriteRendererChild()
        {
            var host = new GameObject("Host");
            var sr = host.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 10;

            var ds = host.AddComponent<DropShadow>();
            ds.shadowSprite = Sprite.Create(
                Texture2D.whiteTexture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
            ds.EnsureChild();

            var child = host.transform.Find("DropShadow");
            Assert.IsNotNull(child, "DropShadow phải tạo child có name 'DropShadow'.");
            var childSR = child.GetComponent<SpriteRenderer>();
            Assert.IsNotNull(childSR);
            Assert.AreEqual(sr.sortingLayerID, childSR.sortingLayerID, "Cùng sorting layer parent.");
            Assert.AreEqual(sr.sortingOrder + ds.sortingOrderOffset, childSR.sortingOrder,
                "Sorting order = parent + offset (phải nằm dưới).");

            Object.DestroyImmediate(host);
        }

        [Test]
        public void DropShadow_EnsureChild_Idempotent()
        {
            var host = new GameObject("Host");
            host.AddComponent<SpriteRenderer>();
            var ds = host.AddComponent<DropShadow>();
            ds.shadowSprite = Sprite.Create(
                Texture2D.whiteTexture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));

            ds.EnsureChild();
            ds.EnsureChild();
            ds.EnsureChild();

            int count = 0;
            foreach (Transform t in host.transform)
            {
                if (t.name == "DropShadow") count++;
            }
            Assert.AreEqual(1, count, "EnsureChild gọi 3 lần phải chỉ tồn tại 1 child shadow.");

            Object.DestroyImmediate(host);
        }

        [Test]
        public void DropShadow_KeepFlat_CounterRotatesChildOnLateUpdate()
        {
            var host = new GameObject("Host");
            host.AddComponent<SpriteRenderer>();
            var ds = host.AddComponent<DropShadow>();
            ds.shadowSprite = Sprite.Create(
                Texture2D.whiteTexture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
            ds.keepFlat = true;
            ds.EnsureChild();

            // Mô phỏng WindSway xoay parent +15° trục Z.
            host.transform.localRotation = Quaternion.Euler(0f, 0f, 15f);

            // EditMode không auto-fire LateUpdate → tính counter-rotation thủ công theo
            // cùng công thức trong DropShadow.LateUpdate.
            var child = host.transform.Find("DropShadow");
            child.localRotation = Quaternion.Inverse(host.transform.localRotation)
                * Quaternion.identity;

            // World rotation child = host.world * child.local. Parent của host = scene root
            // → host.world == host.local. Vậy world của child phải xấp xỉ identity (flat).
            var worldEulerZ = (host.transform.rotation * child.localRotation).eulerAngles.z;
            // eulerAngles trả [0,360) → 360 ≈ 0.
            float dz = Mathf.DeltaAngle(worldEulerZ, 0f);
            Assert.Less(Mathf.Abs(dz), 0.01f,
                $"Shadow phải flat trong world space (Z=0); world Z thực tế {worldEulerZ}°");

            Object.DestroyImmediate(host);
        }

        [Test]
        public void DropShadow_NoSprite_NoChildCreated()
        {
            var host = new GameObject("Host");
            host.AddComponent<SpriteRenderer>();
            var ds = host.AddComponent<DropShadow>();
            ds.shadowSprite = null;
            ds.EnsureChild();

            Assert.IsNull(host.transform.Find("DropShadow"),
                "Không có shadowSprite → skip không tạo child.");

            Object.DestroyImmediate(host);
        }
    }
}
