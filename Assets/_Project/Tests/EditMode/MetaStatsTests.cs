using NUnit.Framework;
using WildernessCultivation.Core;

namespace WildernessCultivation.Tests.EditMode
{
    /// <summary>
    /// EditMode tests cho MetaStats — lifetime stats xuyên các đời.
    /// </summary>
    public class MetaStatsTests
    {
        [SetUp]
        public void Setup() => MetaStats.Clear();

        [TearDown]
        public void Teardown() => MetaStats.Clear();

        [Test]
        public void Load_NoFile_ReturnsDefault()
        {
            var data = MetaStats.Load();
            Assert.IsNotNull(data);
            Assert.AreEqual(0, data.totalDeaths);
            Assert.AreEqual(0, data.bestDaysSurvived);
            Assert.IsFalse(data.everAwakened);
        }

        [Test]
        public void RecordDeath_IncrementsTotalAndBumpsBest()
        {
            MetaStats.RecordDeath(daysSurvived: 3, realmTier: 0, wasAwakened: false);
            MetaStats.RecordDeath(daysSurvived: 12, realmTier: 2, wasAwakened: true);
            MetaStats.RecordDeath(daysSurvived: 7, realmTier: 1, wasAwakened: true);

            var data = MetaStats.Load();
            Assert.AreEqual(3, data.totalDeaths);
            Assert.AreEqual(12, data.bestDaysSurvived, "Bump bestDaysSurvived khi vượt");
            Assert.AreEqual(2, data.bestRealmTier);
            Assert.AreEqual(3 + 12 + 7, data.totalDaysLived);
            Assert.IsTrue(data.everAwakened);
        }

        [Test]
        public void RecordDeath_BestDaysOnlyBumpsForward()
        {
            MetaStats.RecordDeath(daysSurvived: 10, realmTier: 0, wasAwakened: false);
            MetaStats.RecordDeath(daysSurvived: 3, realmTier: 0, wasAwakened: false);
            var data = MetaStats.Load();
            Assert.AreEqual(10, data.bestDaysSurvived, "Best phải giữ kỉ lục cao nhất");
        }

        [Test]
        public void RecordDeath_FoundationTier_FlipsEverReachedFoundation()
        {
            MetaStats.RecordDeath(daysSurvived: 30, realmTier: 10, wasAwakened: true);
            var data = MetaStats.Load();
            Assert.IsTrue(data.everReachedFoundation);
        }
    }
}
