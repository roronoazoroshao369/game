using NUnit.Framework;
using UnityEngine;
using WildernessCultivation.CameraFx;
using WildernessCultivation.Combat;

namespace WildernessCultivation.Tests.EditMode
{
    /// <summary>
    /// EditMode tests cho <see cref="CameraShake"/>. Không test offset thực tế (cần
    /// PlayMode update loop) — chỉ verify state machine của Trigger() + sub/unsub event.
    /// </summary>
    public class CameraShakeTests
    {
        GameObject go;
        CameraShake shake;

        [SetUp]
        public void Setup()
        {
            CombatEvents.ClearAllSubscribers();
            go = new GameObject("Cam");
            shake = go.AddComponent<CameraShake>();
            // Awake đã chạy → baseLocalPos snapshot, OnEnable cũng đã subscribe events.
        }

        [TearDown]
        public void Teardown()
        {
            if (go != null) Object.DestroyImmediate(go);
            CombatEvents.ClearAllSubscribers();
        }

        [Test]
        public void Trigger_SetsIsShaking_True()
        {
            Assert.IsFalse(shake.IsShaking);
            shake.Trigger(0.3f, 0.5f);
            Assert.IsTrue(shake.IsShaking);
        }

        [Test]
        public void Trigger_AmplitudeZero_NoOp()
        {
            shake.Trigger(0f, 0.5f);
            Assert.IsFalse(shake.IsShaking);
        }

        [Test]
        public void Trigger_DurationZero_NoOp()
        {
            shake.Trigger(0.3f, 0f);
            Assert.IsFalse(shake.IsShaking);
        }

        [Test]
        public void Trigger_SubscribesToCombatEvents_RaiseDamageStartsShake()
        {
            Assert.IsFalse(shake.IsShaking);
            CombatEvents.RaiseDamage(Vector3.zero, 50f);
            Assert.IsTrue(shake.IsShaking);
        }

        [Test]
        public void OnDisable_Unsubscribes_RaiseDamageNoLongerStartsShake()
        {
            shake.enabled = false;
            CombatEvents.RaiseDamage(Vector3.zero, 50f);
            Assert.IsFalse(shake.IsShaking);
        }
    }
}
