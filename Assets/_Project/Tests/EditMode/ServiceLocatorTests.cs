using NUnit.Framework;
using UnityEngine;
using WildernessCultivation.Core;

namespace WildernessCultivation.Tests.EditMode
{
    /// <summary>
    /// Verify ServiceLocator: register/get/unregister + fake-null detection khi GO destroyed +
    /// ClearAll wipe. Mục tiêu: thay thế FindObjectOfType lookup pattern.
    /// </summary>
    public class ServiceLocatorTests
    {
        sealed class FakeServiceA : MonoBehaviour { }
        sealed class FakeServiceB : MonoBehaviour { }

        GameObject host;

        [SetUp]
        public void SetUp()
        {
            ServiceLocator.ClearAll();
            host = new GameObject("Host");
        }

        [TearDown]
        public void TearDown()
        {
            ServiceLocator.ClearAll();
            if (host != null) Object.DestroyImmediate(host);
        }

        [Test]
        public void Register_ThenGet_ReturnsRegisteredInstance()
        {
            var a = host.AddComponent<FakeServiceA>();
            ServiceLocator.Register<FakeServiceA>(a);
            Assert.AreSame(a, ServiceLocator.Get<FakeServiceA>());
        }

        [Test]
        public void Register_Null_NoOp()
        {
            ServiceLocator.Register<FakeServiceA>(null);
            Assert.AreEqual(0, ServiceLocator.Count);
        }

        [Test]
        public void Get_WithoutRegister_FallbackToFindObjectOfType()
        {
            // Tạo component không tự register — Get phải fallback FindObjectOfType.
            var b = host.AddComponent<FakeServiceB>();
            var found = ServiceLocator.Get<FakeServiceB>();
            Assert.AreSame(b, found);
        }

        [Test]
        public void Get_AfterFallback_CachesResult()
        {
            host.AddComponent<FakeServiceB>();
            ServiceLocator.Get<FakeServiceB>(); // first call → fallback + cache
            Assert.AreEqual(1, ServiceLocator.Count);
        }

        [Test]
        public void Unregister_OnlyRemovesIfIdentityMatches()
        {
            var a1 = host.AddComponent<FakeServiceA>();
            var other = new GameObject("Other");
            var a2 = other.AddComponent<FakeServiceA>();

            ServiceLocator.Register<FakeServiceA>(a1);
            ServiceLocator.Unregister<FakeServiceA>(a2); // identity mismatch — no-op
            Assert.AreSame(a1, ServiceLocator.Get<FakeServiceA>());

            ServiceLocator.Unregister<FakeServiceA>(a1); // identity match — removes
            // Sau Unregister, Get fallback FindObjectOfType — vẫn tìm thấy a1 vì còn alive.
            // Để verify thực sự removed cần destroy a1 trước.
            Object.DestroyImmediate(other);
            Object.DestroyImmediate(a1);
            Assert.IsNull(ServiceLocator.Get<FakeServiceA>());
        }

        [Test]
        public void Get_AfterDestroy_RefreshesCache()
        {
            var a = host.AddComponent<FakeServiceA>();
            ServiceLocator.Register<FakeServiceA>(a);
            Assert.AreSame(a, ServiceLocator.Get<FakeServiceA>());

            // Destroy GO → Unity fake-null. Get phải refresh + trả null (vì không có instance khác).
            Object.DestroyImmediate(host);
            host = null;
            var result = ServiceLocator.Get<FakeServiceA>();
            Assert.IsNull(result);
        }

        [Test]
        public void Get_AfterDestroy_FindsReplacement()
        {
            var a1 = host.AddComponent<FakeServiceA>();
            ServiceLocator.Register<FakeServiceA>(a1);

            // Spawn replacement before destroying original.
            var replacement = new GameObject("Replacement");
            var a2 = replacement.AddComponent<FakeServiceA>();

            // Destroy original (registered) → cache stale → fake-null.
            Object.DestroyImmediate(host);
            host = replacement; // teardown will clean

            var result = ServiceLocator.Get<FakeServiceA>();
            Assert.AreSame(a2, result);
        }

        [Test]
        public void ClearAll_WipesAllEntries()
        {
            var a = host.AddComponent<FakeServiceA>();
            var b = host.AddComponent<FakeServiceB>();
            ServiceLocator.Register<FakeServiceA>(a);
            ServiceLocator.Register<FakeServiceB>(b);
            Assert.AreEqual(2, ServiceLocator.Count);

            ServiceLocator.ClearAll();
            Assert.AreEqual(0, ServiceLocator.Count);
        }

        [Test]
        public void Register_SecondTime_OverridesPrevious()
        {
            var a1 = host.AddComponent<FakeServiceA>();
            var other = new GameObject("Other");
            var a2 = other.AddComponent<FakeServiceA>();

            ServiceLocator.Register<FakeServiceA>(a1);
            ServiceLocator.Register<FakeServiceA>(a2);
            Assert.AreSame(a2, ServiceLocator.Get<FakeServiceA>());

            Object.DestroyImmediate(other);
        }
    }
}
