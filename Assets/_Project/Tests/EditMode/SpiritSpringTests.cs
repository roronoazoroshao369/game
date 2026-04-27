using NUnit.Framework;
using UnityEngine;
using WildernessCultivation.Core;
using WildernessCultivation.Cultivation;
using WildernessCultivation.Player;
using WildernessCultivation.World;

namespace WildernessCultivation.Tests.EditMode
{
    /// <summary>
    /// EditMode tests cho SpiritSpring IInteractable: gate eligibility, detach VFX
    /// trước khi destroy spring, và không leak VFX trong EditMode path.
    /// </summary>
    public class SpiritSpringTests
    {
        GameObject playerGo;
        GameObject springGo;
        GameObject vfxGo;
        PlayerStats stats;
        AwakeningSystem awaken;
        TimeManager time;
        SpiritSpring spring;

        [SetUp]
        public void Setup()
        {
            playerGo = new GameObject("Player");
            stats = playerGo.AddComponent<PlayerStats>();
            stats.maxHP = 100f;
            stats.HP = 100f;
            stats.maxSanity = 100f;
            stats.Sanity = 100f;
            stats.IsAwakened = false;
            TestHelpers.Boot(stats);
            awaken = playerGo.AddComponent<AwakeningSystem>();
            awaken.config = AwakeningConfigSO.CreateDefault();
            TestHelpers.Boot(awaken);

            var timeGo = new GameObject("Time");
            time = timeGo.AddComponent<TimeManager>();
            time.daysSurvived = 7;
            awaken.SetTimeManager(time);

            springGo = new GameObject("SpiritSpring");
            spring = springGo.AddComponent<SpiritSpring>();
            // VFX là child của spring (pattern setup pre-detach trong scene/prefab).
            vfxGo = new GameObject("AuraVFX");
            vfxGo.transform.SetParent(springGo.transform);
            spring.auraVfx = vfxGo.AddComponent<ParticleSystem>();
        }

        [TearDown]
        public void Teardown()
        {
            if (playerGo != null) Object.DestroyImmediate(playerGo);
            if (time != null && time.gameObject != null) Object.DestroyImmediate(time.gameObject);
            if (springGo != null) Object.DestroyImmediate(springGo);
            if (vfxGo != null) Object.DestroyImmediate(vfxGo);
        }

        [Test]
        public void Interact_Ineligible_KeepsSpringAndVfxAttached()
        {
            // Day < 7 → ineligible → spring giữ lại, VFX vẫn là child.
            time.daysSurvived = 3;
            bool ok = spring.Interact(playerGo);
            Assert.IsFalse(ok, "Ineligible phải trả false (TryAwaken không roll)");
            Assert.IsNotNull(springGo, "Spring không bị destroy khi chưa đủ duyên");
            Assert.AreEqual(springGo.transform, vfxGo.transform.parent,
                "VFX không bị detach khi không roll");
        }

        [Test]
        public void Interact_Eligible_DestroysSpring_AndCleansUpDetachedVfx_NoLeak()
        {
            // Force outcome cố định để khỏi flake (Fail vẫn consume spring per design).
            awaken.SetSeed(0); // 50% fail chance ⇒ seed=0 → roll thấp → Fail.
            bool ok = spring.Interact(playerGo);
            Assert.IsTrue(ok, "Eligible roll trả true bất kể outcome");

            // Spring GameObject đã DestroyImmediate (EditMode path).
            Assert.IsTrue(springGo == null, "Spring GameObject phải bị destroy");
            // VFX detach + DestroyImmediate trong EditMode → KHÔNG được orphan trong scene.
            Assert.IsTrue(vfxGo == null, "Detached VFX phải được DestroyImmediate trong EditMode (no leak)");
        }
    }
}
