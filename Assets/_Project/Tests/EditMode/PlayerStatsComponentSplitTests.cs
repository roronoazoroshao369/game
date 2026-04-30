using NUnit.Framework;
using UnityEngine;
using WildernessCultivation.Player;
using WildernessCultivation.Player.Stats;
using WildernessCultivation.World;

namespace WildernessCultivation.Tests.EditMode
{
    /// <summary>
    /// EditMode tests cho R1 component split: PlayerStats là façade, 3 subsystem
    /// (<see cref="WetnessComponent"/>, <see cref="ThermalComponent"/>,
    /// <see cref="PermadeathHandler"/>) auto-add trong Awake. Verify:
    /// - Sub-component nào không gắn sẵn vẫn có sau Awake.
    /// - Façade get/set đồng bộ với storage trên component.
    /// - Tier mapping pure-static khớp giữa PlayerStats và WetnessComponent.
    /// </summary>
    public class PlayerStatsComponentSplitTests
    {
        GameObject go;
        PlayerStats stats;

        [SetUp]
        public void Setup()
        {
            go = new GameObject("Player");
            stats = go.AddComponent<PlayerStats>();
            TestHelpers.Boot(stats);
        }

        [TearDown]
        public void Teardown()
        {
            if (go != null) Object.DestroyImmediate(go);
        }

        [Test]
        public void Awake_AutoAddsWetnessThermalPermadeath()
        {
            Assert.IsNotNull(stats.wetness, "WetnessComponent phải được Awake auto-add");
            Assert.IsNotNull(stats.thermal, "ThermalComponent phải được Awake auto-add");
            Assert.IsNotNull(stats.permadeath, "PermadeathHandler phải được Awake auto-add");

            Assert.IsNotNull(go.GetComponent<WetnessComponent>(), "WetnessComponent phải có trên GameObject");
            Assert.IsNotNull(go.GetComponent<ThermalComponent>(), "ThermalComponent phải có trên GameObject");
            Assert.IsNotNull(go.GetComponent<PermadeathHandler>(), "PermadeathHandler phải có trên GameObject");
        }

        [Test]
        public void WetnessFacade_GetSet_DelegatesToComponent()
        {
            stats.Wetness = 42f;
            Assert.AreEqual(42f, stats.wetness.Wetness, 0.001f, "Set Wetness qua façade ghi vào component");
            stats.wetness.Wetness = 73f;
            Assert.AreEqual(73f, stats.Wetness, 0.001f, "Get Wetness qua façade đọc từ component");
        }

        [Test]
        public void BodyTempFacade_GetSet_DelegatesToComponent()
        {
            stats.BodyTemp = 37.5f;
            Assert.AreEqual(37.5f, stats.thermal.BodyTemp, 0.001f);
            stats.thermal.BodyTemp = 88f;
            Assert.AreEqual(88f, stats.BodyTemp, 0.001f);
        }

        [Test]
        public void PermadeathFlagFacade_GetSet_DelegatesToHandler()
        {
            stats.permadeathEnabled = false;
            Assert.IsFalse(stats.permadeath.permadeathEnabled);
            stats.permadeath.permadeathEnabled = true;
            Assert.IsTrue(stats.permadeathEnabled);
        }

        [Test]
        public void TierStaticConsistency_FacadeAndComponent()
        {
            // Façade static phải khớp WetnessComponent.TierOf trên cùng dải input.
            for (int v = 0; v <= 100; v += 10)
            {
                Assert.AreEqual(WetnessComponent.TierOf(v), PlayerStats.WetnessTierOf(v),
                    $"Tier(v={v}) phải khớp giữa façade và component");
            }
        }

        [Test]
        public void PreInstalled_WetnessComponent_NotOverridden()
        {
            // Nếu prefab đã gắn WetnessComponent custom (vd subclass tuning), Awake KHÔNG add thêm.
            var go2 = new GameObject("PlayerCustom");
            var preWet = go2.AddComponent<WetnessComponent>();
            preWet.maxWetness = 200f; // marker
            var stats2 = go2.AddComponent<PlayerStats>();
            TestHelpers.Boot(stats2);

            Assert.AreSame(preWet, stats2.wetness, "PlayerStats phải reuse component có sẵn, không add mới");
            Assert.AreEqual(200f, stats2.maxWetness, "Cấu hình prefab phải được giữ");

            Object.DestroyImmediate(go2);
        }
    }
}
