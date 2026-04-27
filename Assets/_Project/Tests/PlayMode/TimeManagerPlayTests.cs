using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using WildernessCultivation.Core;

namespace WildernessCultivation.Tests.PlayMode
{
    /// <summary>
    /// PlayMode tests cho Update tick của TimeManager: advance currentTime01 + day boundary
    /// wrap + OnDayStart/OnNightStart events + OnSeasonChanged.
    /// Dùng dayLengthSeconds rất ngắn (0.3s) để 1 day in-game = vài frame thực.
    /// </summary>
    public class TimeManagerPlayTests
    {
        GameObject go;
        TimeManager tm;

        [SetUp]
        public void Setup()
        {
            go = new GameObject("Time");
            tm = go.AddComponent<TimeManager>();
            tm.dayLengthSeconds = 0.3f;
        }

        [TearDown]
        public void Teardown()
        {
            if (go != null) Object.Destroy(go);
        }

        [UnityTest]
        public IEnumerator Update_AdvancesCurrentTime01_OverFrames()
        {
            tm.currentTime01 = 0.5f;
            float t0 = tm.currentTime01;
            yield return null; // 1 frame
            yield return null;
            yield return null;
            Assert.Greater(tm.currentTime01, t0, "currentTime01 phải tăng theo frame");
        }

        [UnityTest]
        public IEnumerator Update_WrapsAroundDayBoundary_IncrementsDaysSurvived()
        {
            tm.currentTime01 = 0.97f; // gần kết thúc ngày
            int initialDays = tm.daysSurvived;

            // dayLengthSeconds=0.3 → wrap đầu xảy ra ~0.009s, wrap thứ 2 ~0.309s sau đó.
            // Wait 0.15s để CHỈ xảy ra 1 wrap và assertion AreEqual(initialDays + 1) ổn định.
            yield return new WaitForSecondsRealtime(0.15f);

            Assert.AreEqual(initialDays + 1, tm.daysSurvived, "daysSurvived tăng đúng 1 sau khi wrap");
            // Chỉ assert wrap đã xảy ra (currentTime01 < 0.97 ban đầu), KHÔNG ràng buộc giá trị
            // sau wrap (frame timing nhảy ±16ms làm window 9ms quá hẹp → flaky).
            Assert.Less(tm.currentTime01, 0.97f, "currentTime01 đã wrap về phần đầu ngày mới");
        }

        [UnityTest]
        public IEnumerator Update_FiresOnNightStart_WhenCrossingFromDayToNight()
        {
            // dayLengthSeconds=2 → frame delta ~0.008 → frame đầu từ 0.70 vẫn nằm trong day
            // (0.708 < 0.75) → wasNight init = false đúng. Sau wait 0.2s, currentTime01
            // ~0.81 vượt dusk → OnNightStart fire 1 lần.
            tm.dayLengthSeconds = 2.0f;
            tm.currentTime01 = 0.70f;
            yield return null; // 1 frame để init wasNight=false (isNight=false @ 0.708)

            int nightStarts = 0;
            tm.OnNightStart += () => nightStarts++;

            yield return new WaitForSecondsRealtime(0.2f);

            Assert.GreaterOrEqual(nightStarts, 1, "OnNightStart fire ít nhất 1 lần khi vượt dusk");
        }

        [UnityTest]
        public IEnumerator Update_FiresOnDayStart_WhenCrossingFromNightToDay()
        {
            // dayLengthSeconds=2 → frame delta ~0.008 → frame đầu từ 0.20 vẫn nằm trong
            // night (0.208 < 0.25) → wasNight init = true đúng. Sau wait 0.2s,
            // currentTime01 ~0.31 vượt dawn → OnDayStart fire 1 lần.
            // KHÔNG yield trong dạng dayLengthSeconds=0.3 (default Setup) vì frame delta
            // ~0.053 → frame đầu nhảy từ 0.20 → 0.253 vượt dawn ngay → wasNight init = false
            // → transition isNight=false sau đó không fire OnDayStart (flaky).
            tm.dayLengthSeconds = 2.0f;
            tm.currentTime01 = 0.20f;
            yield return null; // 1 frame để init wasNight=true (isNight=true @ 0.208)

            int dayStarts = 0;
            tm.OnDayStart += () => dayStarts++;

            yield return new WaitForSecondsRealtime(0.2f);

            Assert.GreaterOrEqual(dayStarts, 1, "OnDayStart fire ít nhất 1 lần khi vượt dawn");
        }

        [UnityTest]
        public IEnumerator Update_FiresOnSeasonChanged_AfterDaysPerSeason()
        {
            tm.daysPerSeason = 1;
            tm.currentSeason = Season.Spring;
            tm.daysSurvived = 0;
            tm.currentTime01 = 0.97f; // wrap sắp xảy ra

            int seasonChanges = 0;
            Season newSeason = Season.Spring;
            tm.OnSeasonChanged += s => { seasonChanges++; newSeason = s; };

            // Wait 0.15s — chỉ 1 wrap xảy ra → daysSurvived=1 → seasonIdx=1=Summer.
            // Wait quá lâu (vd 0.5s) sẽ gây 2 wrap → daysSurvived=2 → seasonIdx=2=Autumn,
            // làm assertion Season.Summer fail.
            yield return new WaitForSecondsRealtime(0.15f);

            Assert.GreaterOrEqual(seasonChanges, 1, "OnSeasonChanged fire khi daysSurvived hit daysPerSeason boundary");
            Assert.AreEqual(Season.Summer, newSeason, "Spring → Summer (idx 0 → 1) sau 1 ngày");
        }

        [UnityTest]
        public IEnumerator Update_DoesNotFire_OnNightStart_WhenAlreadyNight()
        {
            tm.currentTime01 = 0f; // midnight, isNight=true
            // Force first Update để init wasNight
            yield return null;

            int nightStarts = 0;
            tm.OnNightStart += () => nightStarts++;

            // Wait 1 frame nhỏ — vẫn ở vùng night (0.05)
            yield return null;
            yield return null;

            Assert.AreEqual(0, nightStarts, "OnNightStart KHÔNG fire khi đã ở night từ trước");
        }
    }
}
