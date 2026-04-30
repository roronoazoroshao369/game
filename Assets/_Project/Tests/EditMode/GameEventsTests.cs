using NUnit.Framework;
using UnityEngine;
using WildernessCultivation.Core;
using WildernessCultivation.Cultivation;
using WildernessCultivation.Items;
using WildernessCultivation.Player;
using WildernessCultivation.World;

namespace WildernessCultivation.Tests.EditMode
{
    /// <summary>
    /// EditMode tests cho R4 GameEvents static hub. Verify:
    /// - Subscribe/unsubscribe + Raise gọi handler đúng số lần.
    /// - <see cref="GameEvents.ClearAllSubscribers"/> đảm bảo isolation giữa test.
    /// - Publisher (PlayerStats / RealmSystem / TimeManager-like) fire GameEvents
    ///   song song với instance event cũ — không break code cũ.
    /// </summary>
    public class GameEventsTests
    {
        [SetUp]
        public void Setup() => GameEvents.ClearAllSubscribers();

        [TearDown]
        public void Teardown() => GameEvents.ClearAllSubscribers();

        // ===== Hub primitives =====

        [Test]
        public void RaisePlayerDied_InvokesSubscriber()
        {
            int count = 0;
            GameEvents.OnPlayerDied += () => count++;
            GameEvents.RaisePlayerDied();
            GameEvents.RaisePlayerDied();
            Assert.AreEqual(2, count);
        }

        [Test]
        public void Unsubscribe_StopsReceivingEvent()
        {
            int count = 0;
            System.Action h = () => count++;
            GameEvents.OnPlayerDied += h;
            GameEvents.RaisePlayerDied();
            GameEvents.OnPlayerDied -= h;
            GameEvents.RaisePlayerDied();
            Assert.AreEqual(1, count);
        }

        [Test]
        public void ClearAllSubscribers_ResetsAllChannels()
        {
            int diedCount = 0, statsCount = 0, weatherCount = 0;
            GameEvents.OnPlayerDied += () => diedCount++;
            GameEvents.OnPlayerStatsChanged += () => statsCount++;
            GameEvents.OnWeatherChanged += w => weatherCount++;

            GameEvents.ClearAllSubscribers();

            GameEvents.RaisePlayerDied();
            GameEvents.RaisePlayerStatsChanged();
            GameEvents.RaiseWeatherChanged(Weather.Rain);

            Assert.AreEqual(0, diedCount);
            Assert.AreEqual(0, statsCount);
            Assert.AreEqual(0, weatherCount);
        }

        [Test]
        public void RealmAdvanced_PassesNewTier()
        {
            int captured = -1;
            GameEvents.OnRealmAdvanced += t => captured = t;
            GameEvents.RaiseRealmAdvanced(3);
            Assert.AreEqual(3, captured);
        }

        [Test]
        public void WeatherChanged_PassesWeatherEnum()
        {
            Weather captured = Weather.Clear;
            GameEvents.OnWeatherChanged += w => captured = w;
            GameEvents.RaiseWeatherChanged(Weather.Storm);
            Assert.AreEqual(Weather.Storm, captured);
        }

        // ===== Publisher integration =====

        [Test]
        public void PlayerStats_Die_FiresGameEventsAndInstanceEvent()
        {
            var go = new GameObject("Player");
            var stats = go.AddComponent<PlayerStats>();
            TestHelpers.Boot(stats);

            int instanceCount = 0;
            int hubCount = 0;
            stats.OnDeath += () => instanceCount++;
            GameEvents.OnPlayerDied += () => hubCount++;

            stats.HP = 0.1f;
            stats.TakeDamageRaw(10f);

            Assert.AreEqual(1, instanceCount, "Instance OnDeath phải fire");
            Assert.AreEqual(1, hubCount, "GameEvents.OnPlayerDied phải fire song song");

            Object.DestroyImmediate(go);
        }

        [Test]
        public void PlayerStats_Heal_FiresStatsChangedHub()
        {
            var go = new GameObject("Player");
            var stats = go.AddComponent<PlayerStats>();
            TestHelpers.Boot(stats);

            int hubCount = 0;
            GameEvents.OnPlayerStatsChanged += () => hubCount++;

            stats.HP = 50f;
            stats.Heal(10f);

            Assert.AreEqual(1, hubCount);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void PlayerInventoryBridge_NoCrashOnSetup()
        {
            // Verify PlayerStats.Awake hook subscribe BridgeInventoryToGameEvents tới
            // Inventory.OnInventoryChanged không crash khi cả 2 component cùng GameObject.
            // End-to-end (Add ItemSO → bridge fire) đã được SaveLoadFuzzTests cover.
            var go = new GameObject("Player");
            go.AddComponent<Inventory>();
            var stats = go.AddComponent<PlayerStats>();
            Assert.DoesNotThrow(() => TestHelpers.Boot(stats));
            Object.DestroyImmediate(go);
        }
    }
}
