using NUnit.Framework;
using UnityEngine;
using WildernessCultivation.Cultivation;
using WildernessCultivation.Player;

namespace WildernessCultivation.Tests.EditMode
{
    /// <summary>
    /// EditMode tests cho gating: Thường Nhân (chưa awaken) không thiền được.
    /// </summary>
    public class MeditationGatingTests
    {
        GameObject go;
        PlayerStats stats;
        MeditationAction med;

        [SetUp]
        public void Setup()
        {
            go = new GameObject("Player");
            stats = go.AddComponent<PlayerStats>();
            go.AddComponent<RealmSystem>();
            med = go.AddComponent<MeditationAction>();
            TestHelpers.Boot(stats, med);
        }

        [TearDown]
        public void Teardown()
        {
            if (go != null) Object.DestroyImmediate(go);
        }

        [Test]
        public void StartMeditation_BlockedWhenNotAwakened()
        {
            stats.IsAwakened = false;
            med.StartMeditation();
            Assert.IsFalse(med.IsMeditating, "Thường Nhân KHÔNG thiền được");
            Assert.IsFalse(med.CanMeditate);
        }

        [Test]
        public void StartMeditation_AllowedWhenAwakened()
        {
            stats.IsAwakened = true;
            med.StartMeditation();
            Assert.IsTrue(med.IsMeditating, "Tu sĩ đã khai mở thì thiền được");
            Assert.IsTrue(med.CanMeditate);
        }
    }
}
