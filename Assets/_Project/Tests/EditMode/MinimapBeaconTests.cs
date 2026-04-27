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

        /// <summary>
        /// Simulate PlayMode order: AddComponent → Awake fires đồng bộ với default
        /// (yellow / scale 2) → caller Initialize sau đó. Bug cũ: child stuck ở
        /// default. Fix: Initialize update child hiện có.
        /// </summary>
        [Test]
        public void Initialize_AfterAwake_UpdatesExistingChild()
        {
            hostGo = new GameObject("Host");
            var beacon = hostGo.AddComponent<MinimapBeacon>();
            // Step 1: Boot mô phỏng PlayMode AddComponent fire Awake đồng bộ với default.
            TestHelpers.Boot(beacon);
            var firstChild = beacon.Child;
            Assert.AreEqual(Color.yellow, firstChild.color, "Awake với default → yellow");
            Assert.AreEqual(2f, firstChild.transform.localScale.x, "Awake với default → scale 2");

            // Step 2: caller gọi Initialize (như Tombstone/SpiritSpring làm).
            beacon.Initialize(new Color(0.3f, 0.85f, 1f, 0.95f), 2.5f, "SpiritSpringBeacon");

            Assert.AreSame(firstChild, beacon.Child, "Không tạo child mới — update child cũ");
            Assert.AreEqual(new Color(0.3f, 0.85f, 1f, 0.95f), beacon.Child.color,
                "Color phải reflect Initialize, không phải default yellow");
            Assert.AreEqual(2.5f, beacon.Child.transform.localScale.x,
                "Scale phải reflect Initialize, không phải default 2");
            Assert.AreEqual("SpiritSpringBeacon", beacon.Child.gameObject.name);
            int childCount = 0;
            foreach (Transform t in hostGo.transform) childCount++;
            Assert.AreEqual(1, childCount, "Initialize idempotent — vẫn 1 child");
        }

        [Test]
        public void Initialize_BeforeAwake_CreatesChildWithCorrectProps()
        {
            // Mô phỏng EditMode: Initialize chạy trước Awake (Boot không fire vì
            // Initialize đã EnsureChild). Verify Awake không ghi đè.
            hostGo = new GameObject("Host");
            var beacon = hostGo.AddComponent<MinimapBeacon>();
            beacon.Initialize(new Color(0.55f, 0.55f, 0.6f, 0.9f), 1.5f, "TombstoneBeacon");
            var child = beacon.Child;
            Assert.IsNotNull(child);
            Assert.AreEqual(new Color(0.55f, 0.55f, 0.6f, 0.9f), child.color);
            Assert.AreEqual(1.5f, child.transform.localScale.x);

            // Awake gọi sau cũng không nên reset (EnsureChild guard `if Child != null return`).
            TestHelpers.Boot(beacon);
            Assert.AreSame(child, beacon.Child);
            Assert.AreEqual(new Color(0.55f, 0.55f, 0.6f, 0.9f), beacon.Child.color,
                "Awake sau Initialize không được ghi đè color");
        }
    }
}
