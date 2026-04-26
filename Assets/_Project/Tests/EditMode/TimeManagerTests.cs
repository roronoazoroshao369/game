using NUnit.Framework;
using UnityEngine;
using WildernessCultivation.Core;

namespace WildernessCultivation.Tests.EditMode
{
    /// <summary>
    /// EditMode tests cho pure functions của TimeManager: isNight boundaries / GetLightIntensity /
    /// GetSpiritualEnergyMultiplier / SeasonBaselineTemperature.
    ///
    /// KHÔNG test Update tick (cần PlayMode + Time.deltaTime advancement). Để file riêng
    /// TimeManagerPlayTests cover day/night transition events + season change + RollWeather.
    /// </summary>
    public class TimeManagerTests
    {
        GameObject go;
        TimeManager tm;

        [SetUp]
        public void Setup()
        {
            go = new GameObject("Time");
            tm = go.AddComponent<TimeManager>();
        }

        [TearDown]
        public void Teardown()
        {
            if (go != null) Object.DestroyImmediate(go);
        }

        // ===== isNight boundaries =====
        // Code: isNight = currentTime01 < 0.25f || currentTime01 > 0.75f.

        [Test]
        public void IsNight_Midnight_True()
        {
            tm.currentTime01 = 0f;
            Assert.IsTrue(tm.isNight);
        }

        [Test]
        public void IsNight_JustBeforeDawn_True()
        {
            tm.currentTime01 = 0.24f;
            Assert.IsTrue(tm.isNight);
        }

        [Test]
        public void IsNight_Dawn_False()
        {
            tm.currentTime01 = 0.25f;
            Assert.IsFalse(tm.isNight, "0.25 không < 0.25 → false");
        }

        [Test]
        public void IsNight_Noon_False()
        {
            tm.currentTime01 = 0.5f;
            Assert.IsFalse(tm.isNight);
        }

        [Test]
        public void IsNight_Dusk_False()
        {
            tm.currentTime01 = 0.75f;
            Assert.IsFalse(tm.isNight, "0.75 không > 0.75 → false");
        }

        [Test]
        public void IsNight_AfterDusk_True()
        {
            tm.currentTime01 = 0.76f;
            Assert.IsTrue(tm.isNight);
        }

        // ===== dayProgress alias =====

        [Test]
        public void DayProgress_EqualsCurrentTime01()
        {
            tm.currentTime01 = 0.42f;
            Assert.AreEqual(0.42f, tm.dayProgress, 0.0001f);
        }

        // ===== GetLightIntensity (cosine-based, peak at 0.5) =====

        [Test]
        public void GetLightIntensity_Midnight_IsZero()
        {
            tm.currentTime01 = 0f;
            Assert.AreEqual(0f, tm.GetLightIntensity(), 0.001f);
        }

        [Test]
        public void GetLightIntensity_Noon_IsOne()
        {
            tm.currentTime01 = 0.5f;
            Assert.AreEqual(1f, tm.GetLightIntensity(), 0.001f);
        }

        [Test]
        public void GetLightIntensity_DawnAndDusk_AreEqual()
        {
            tm.currentTime01 = 0.25f;
            float dawn = tm.GetLightIntensity();
            tm.currentTime01 = 0.75f;
            float dusk = tm.GetLightIntensity();
            Assert.AreEqual(dawn, dusk, 0.001f, "đối xứng quanh noon");
            Assert.AreEqual(0.5f, dawn, 0.01f, "dawn/dusk ~0.5 (cosine = 0)");
        }

        [Test]
        public void GetLightIntensity_AlwaysIn0to1()
        {
            for (float t = 0f; t <= 1f; t += 0.05f)
            {
                tm.currentTime01 = t;
                float v = tm.GetLightIntensity();
                Assert.GreaterOrEqual(v, 0f, $"t={t}");
                Assert.LessOrEqual(v, 1f, $"t={t}");
            }
        }

        // ===== GetSpiritualEnergyMultiplier =====

        [Test]
        public void GetSpiritualEnergyMultiplier_Day_ReturnsOne()
        {
            tm.currentTime01 = 0.5f;
            Assert.AreEqual(1.0f, tm.GetSpiritualEnergyMultiplier(), 0.001f);
        }

        [Test]
        public void GetSpiritualEnergyMultiplier_Night_Returns1_5()
        {
            tm.currentTime01 = 0.95f;
            Assert.AreEqual(1.5f, tm.GetSpiritualEnergyMultiplier(), 0.001f);
        }

        // ===== SeasonBaselineTemperature =====

        [Test]
        public void SeasonBaselineTemperature_Spring_55()
        {
            tm.currentSeason = Season.Spring;
            Assert.AreEqual(55f, tm.SeasonBaselineTemperature, 0.001f);
        }

        [Test]
        public void SeasonBaselineTemperature_Summer_70()
        {
            tm.currentSeason = Season.Summer;
            Assert.AreEqual(70f, tm.SeasonBaselineTemperature, 0.001f);
        }

        [Test]
        public void SeasonBaselineTemperature_Autumn_50()
        {
            tm.currentSeason = Season.Autumn;
            Assert.AreEqual(50f, tm.SeasonBaselineTemperature, 0.001f);
        }

        [Test]
        public void SeasonBaselineTemperature_Winter_30()
        {
            tm.currentSeason = Season.Winter;
            Assert.AreEqual(30f, tm.SeasonBaselineTemperature, 0.001f);
        }

        // ===== Light2DProxy =====

        [Test]
        public void Light2DProxy_SetIntensity_AppliesAlphaToFallbackOverlay()
        {
            var proxyGo = new GameObject("Proxy");
            try
            {
                var sr = proxyGo.AddComponent<SpriteRenderer>();
                sr.color = new Color(0f, 0f, 0f, 0f);
                var proxy = proxyGo.AddComponent<Light2DProxy>();
                proxy.fallbackOverlay = sr;

                proxy.SetIntensity01(0f); // tối đen → alpha 1
                Assert.AreEqual(1f, sr.color.a, 0.001f);

                proxy.SetIntensity01(1f); // noon → alpha 0
                Assert.AreEqual(0f, sr.color.a, 0.001f);

                proxy.SetIntensity01(0.5f);
                Assert.AreEqual(0.5f, sr.color.a, 0.001f);
            }
            finally
            {
                Object.DestroyImmediate(proxyGo);
            }
        }

        [Test]
        public void Light2DProxy_NullOverlay_NoThrow()
        {
            var proxyGo = new GameObject("Proxy");
            try
            {
                var proxy = proxyGo.AddComponent<Light2DProxy>();
                Assert.DoesNotThrow(() => proxy.SetIntensity01(0.5f));
            }
            finally
            {
                Object.DestroyImmediate(proxyGo);
            }
        }
    }
}
