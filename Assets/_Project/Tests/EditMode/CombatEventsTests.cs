using NUnit.Framework;
using UnityEngine;
using WildernessCultivation.Combat;

namespace WildernessCultivation.Tests.EditMode
{
    /// <summary>EditMode tests cho hub event tĩnh <see cref="CombatEvents"/>.</summary>
    public class CombatEventsTests
    {
        [SetUp]
        public void Setup() => CombatEvents.ClearAllSubscribers();

        [TearDown]
        public void Teardown() => CombatEvents.ClearAllSubscribers();

        [Test]
        public void RaiseDamage_InvokesSubscriber_WithCorrectArgs()
        {
            Vector3 receivedPos = default;
            float receivedAmt = -1f;
            bool receivedCrit = false;
            CombatEvents.OnDamageDealt += (p, a, c) => { receivedPos = p; receivedAmt = a; receivedCrit = c; };

            CombatEvents.RaiseDamage(new Vector3(1f, 2f, 0f), 7f, true);

            Assert.AreEqual(new Vector3(1f, 2f, 0f), receivedPos);
            Assert.AreEqual(7f, receivedAmt);
            Assert.IsTrue(receivedCrit);
        }

        [Test]
        public void RaiseDamage_AmountZeroOrNegative_NoOp()
        {
            int callCount = 0;
            CombatEvents.OnDamageDealt += (_, __, ___) => callCount++;

            CombatEvents.RaiseDamage(Vector3.zero, 0f);
            CombatEvents.RaiseDamage(Vector3.zero, -5f);

            Assert.AreEqual(0, callCount);
        }

        [Test]
        public void ClearAllSubscribers_RemovesAll()
        {
            int callCount = 0;
            CombatEvents.OnDamageDealt += (_, __, ___) => callCount++;
            CombatEvents.ClearAllSubscribers();

            CombatEvents.RaiseDamage(Vector3.zero, 5f);

            Assert.AreEqual(0, callCount);
        }

        [Test]
        public void RaiseDamage_NoSubscribers_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => CombatEvents.RaiseDamage(Vector3.zero, 5f));
        }
    }
}
