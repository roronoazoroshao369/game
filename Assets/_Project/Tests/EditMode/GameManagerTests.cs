using NUnit.Framework;
using UnityEngine;
using WildernessCultivation.Core;

namespace WildernessCultivation.Tests.EditMode
{
    /// <summary>
    /// EditMode tests cho singleton lifecycle của <see cref="GameManager"/>.
    /// Trọng tâm: verify <see cref="GameManager.ResetInstanceForSceneReload"/> cho phép
    /// "Bắt đầu mới" tạo fresh singleton sau scene reload (Devin Review #33 follow-up).
    /// </summary>
    public class GameManagerTests
    {
        GameObject go;

        [TearDown]
        public void Teardown()
        {
            if (go != null) Object.DestroyImmediate(go);
            // EditMode không hoàn tất Destroy ngay → ép clear Instance để test sau sạch.
            GameManager.ResetInstanceForSceneReload();
            Time.timeScale = 1f;
        }

        [Test]
        public void Awake_RegistersSingleton()
        {
            go = new GameObject("GameManager");
            go.AddComponent<GameManager>();
            Assert.IsNotNull(GameManager.Instance);
            Assert.AreSame(go, GameManager.Instance.gameObject);
        }

        [Test]
        public void ResetInstance_ClearsInstance_AllowsFreshSingleton()
        {
            go = new GameObject("GameManager");
            go.AddComponent<GameManager>();
            Assert.IsNotNull(GameManager.Instance);

            GameManager.ResetInstanceForSceneReload();
            Assert.IsNull(GameManager.Instance);

            // Tạo GM mới → phải set Instance (không bị chặn bởi singleton guard cũ).
            var go2 = new GameObject("GameManager2");
            try
            {
                go2.AddComponent<GameManager>();
                Assert.IsNotNull(GameManager.Instance);
                Assert.AreSame(go2, GameManager.Instance.gameObject);
            }
            finally
            {
                Object.DestroyImmediate(go2);
            }
        }

        [Test]
        public void ResetInstance_RestoresTimeScale()
        {
            go = new GameObject("GameManager");
            var gm = go.AddComponent<GameManager>();
            gm.SetPaused(true);
            Assert.AreEqual(0f, Time.timeScale);

            GameManager.ResetInstanceForSceneReload();
            Assert.AreEqual(1f, Time.timeScale);
        }

        [Test]
        public void ResetInstance_NoSingleton_DoesNotThrow()
        {
            Assert.IsNull(GameManager.Instance);
            Assert.DoesNotThrow(() => GameManager.ResetInstanceForSceneReload());
        }
    }
}
