using NUnit.Framework;
using UnityEngine;
using WildernessCultivation.UI;

namespace WildernessCultivation.Tests.EditMode
{
    public class MinimapBeaconTests
    {
        GameObject hostGo;

        [TearDown]
        public void Teardown()
        {
            if (hostGo != null) Object.DestroyImmediate(hostGo);
        }

        [Test]
        public void Awake_CreatesColoredChildSpriteRenderer()
        {
            hostGo = new GameObject("Host");
            var beacon = hostGo.AddComponent<MinimapBeacon>();
            beacon.beaconColor = new Color(0.2f, 0.5f, 0.9f, 1f);
            beacon.scale = 3f;
            beacon.childName = "TestBeacon";
            // EditMode: AddComponent không gọi Awake — invoke thủ công.
            TestHelpers.Boot(beacon);

            Assert.IsNotNull(beacon.Child, "Beacon child SpriteRenderer phải tồn tại");
            Assert.AreEqual("TestBeacon", beacon.Child.gameObject.name);
            Assert.AreEqual(beacon.beaconColor, beacon.Child.color);
            Assert.AreEqual(new Vector3(3f, 3f, 1f), beacon.Child.transform.localScale);
            Assert.AreEqual(hostGo.transform, beacon.Child.transform.parent,
                "Beacon child phải parent dưới host GameObject");
        }

        [Test]
        public void Awake_Idempotent_DoesNotDuplicateChild()
        {
            hostGo = new GameObject("Host");
            var beacon = hostGo.AddComponent<MinimapBeacon>();
            TestHelpers.Boot(beacon);
            var first = beacon.Child;
            // Gọi lại Awake không tạo child thứ 2 (guard `if (Child != null) return`).
            TestHelpers.Boot(beacon);
            Assert.AreSame(first, beacon.Child, "Awake idempotent — không tạo child trùng");
            int childCount = 0;
            foreach (Transform t in hostGo.transform) childCount++;
            Assert.AreEqual(1, childCount);
        }
    }
}
