using NUnit.Framework;
using UnityEngine;
using WildernessCultivation.Core;
using WildernessCultivation.Cultivation;
using WildernessCultivation.Player;
using WildernessCultivation.UI;

namespace WildernessCultivation.Tests.EditMode
{
    /// <summary>
    /// EditMode test cho AwakeningStatusHUD — verify status text reflect đúng
    /// eligibility state + ẩn panel khi player đã khai mở.
    /// </summary>
    public class AwakeningStatusHUDTests
    {
        GameObject playerGo;
        GameObject timeGo;
        GameObject hudGo;
        PlayerStats stats;
        AwakeningSystem awaken;
        TimeManager time;
        AwakeningStatusHUD hud;

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

            timeGo = new GameObject("Time");
            time = timeGo.AddComponent<TimeManager>();
            time.daysSurvived = 0;
            awaken.SetTimeManager(time);

            hudGo = new GameObject("HUD");
            hud = hudGo.AddComponent<AwakeningStatusHUD>();
            hud.playerStats = stats;
            hud.awakening = awaken;
            hud.timeManager = time;
            hud.panelRoot = hudGo;
            // statusText = null OK — BuildStatusLine vẫn chạy được.
        }

        [TearDown]
        public void Teardown()
        {
            if (playerGo != null) Object.DestroyImmediate(playerGo);
            if (timeGo != null) Object.DestroyImmediate(timeGo);
            if (hudGo != null) Object.DestroyImmediate(hudGo);
        }

        [Test]
        public void NotEnoughDays_ShowsRemainingDays()
        {
            time.daysSurvived = 3;
            string line = hud.BuildStatusLine();
            StringAssert.Contains("Ngày 3/7", line);
            StringAssert.Contains("Sống sót thêm 4 ngày", line);
        }

        [Test]
        public void LowHP_ShowsHPWarning()
        {
            time.daysSurvived = 7;
            stats.HP = 30f; // < 50% maxHP
            string line = hud.BuildStatusLine();
            StringAssert.Contains("Thân thể quá yếu", line);
        }

        [Test]
        public void LowSanity_ShowsSanityWarning()
        {
            time.daysSurvived = 7;
            stats.HP = 100f;
            stats.Sanity = 20f; // < 50
            string line = hud.BuildStatusLine();
            StringAssert.Contains("Tâm trí bất ổn", line);
        }

        [Test]
        public void Eligible_ShowsKiNgoHint()
        {
            time.daysSurvived = 7;
            stats.HP = 100f;
            stats.Sanity = 100f;
            string line = hud.BuildStatusLine();
            StringAssert.Contains("Đủ duyên", line);
            StringAssert.Contains("Linh Tuyền", line);
        }

        [Test]
        public void Refresh_HidesPanelWhenAwakened()
        {
            stats.IsAwakened = true;
            hudGo.SetActive(true);
            hud.Refresh();
            Assert.IsFalse(hudGo.activeSelf, "Panel phải ẩn khi player đã khai mở");
        }

        [Test]
        public void Refresh_ShowsPanelWhenNotAwakened()
        {
            stats.IsAwakened = false;
            hudGo.SetActive(false);
            hud.Refresh();
            Assert.IsTrue(hudGo.activeSelf, "Panel phải hiện khi player còn Thường Nhân");
        }
    }
}
