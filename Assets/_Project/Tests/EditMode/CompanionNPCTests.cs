using NUnit.Framework;
using UnityEngine;
using WildernessCultivation.Core;
using WildernessCultivation.Player.Stats;
using WildernessCultivation.World;
using WildernessCultivation.World.States;

namespace WildernessCultivation.Tests.EditMode
{
    /// <summary>
    /// R5 follow-up: exemplar NPC humanoid thứ 2 — CompanionNPC.
    /// Verify:
    /// <list type="bullet">
    /// <item>Multi-component composition (Health + Hunger + Invuln auto-add)</item>
    /// <item>CharacterBase polymorphic view + Hunger delegate</item>
    /// <item>FSM Idle ↔ Follow ↔ Dead transitions</item>
    /// <item>Hunger decay + starvation damage (TickSurvival)</item>
    /// <item>IInteractable toggle Follow/Stay + event fire</item>
    /// <item>ISaveable round-trip (position + hp + hunger + mode)</item>
    /// <item>Invulnerability TakeDamage</item>
    /// <item>Death triggers FSM transition to Dead</item>
    /// </list>
    /// </summary>
    public class CompanionNPCTests
    {
        GameObject companionGo;
        GameObject playerGo;
        CompanionNPC companion;

        [SetUp]
        public void Setup()
        {
            GameEvents.ClearAllSubscribers();
            SaveRegistry.ClearAll();

            companionGo = new GameObject("Companion");
            companion = companionGo.AddComponent<CompanionNPC>();
            companion.companionId = "companion_test";
            companion.displayName = "Test Dog";
            companion.maxHP = 40f;
            companion.maxHunger = 100f;
            companion.hungerDecayMul = 1f;
            companion.followDistance = 3f;
            companion.stopDistance = 1.5f;
            companion.moveSpeed = 2f;
            companion.invulnerable = false;
            companion.mode = CompanionMode.Follow;

            playerGo = new GameObject("Player");
            playerGo.transform.position = Vector3.zero;
            companionGo.transform.position = Vector3.zero;
            companion.followTarget = playerGo.transform;

            TestHelpers.Boot(companion);
        }

        [TearDown]
        public void Teardown()
        {
            GameEvents.ClearAllSubscribers();
            SaveRegistry.ClearAll();
            SaveSystem.Delete();
            if (companionGo != null) Object.DestroyImmediate(companionGo);
            if (playerGo != null) Object.DestroyImmediate(playerGo);
        }

        // ===== Composition =====

        [Test]
        public void Awake_AutoAdds_ThreeComponents()
        {
            Assert.IsNotNull(companionGo.GetComponent<HealthComponent>());
            Assert.IsNotNull(companionGo.GetComponent<HungerComponent>());
            Assert.IsNotNull(companionGo.GetComponent<InvulnerabilityComponent>());
        }

        [Test]
        public void CharacterBase_View_ReflectsHealthComponent()
        {
            Assert.AreEqual(40f, companion.CurrentMaxHP);
            Assert.AreEqual(40f, companion.CurrentHP);
            Assert.IsFalse(companion.IsDead);
        }

        [Test]
        public void Hunger_Property_DelegatesToHungerComponent()
        {
            Assert.AreEqual(100f, companion.MaxHunger);
            Assert.AreEqual(100f, companion.Hunger);
            Assert.IsFalse(companion.IsStarving);
        }

        // ===== TakeDamage =====

        [Test]
        public void TakeDamage_Vulnerable_ReducesHP()
        {
            companion.TakeDamage(15f, null);
            Assert.AreEqual(25f, companion.CurrentHP, 0.01f);
            Assert.IsFalse(companion.IsDead);
        }

        [Test]
        public void TakeDamage_Fatal_TransitionsToDeadState()
        {
            companion.TakeDamage(100f, null);
            Assert.IsTrue(companion.IsDead);
            Assert.AreSame(CompanionStates.Dead, companion.Fsm.Current);
        }

        [Test]
        public void TakeDamage_WhenInvulnerable_NoHPChange()
        {
            var invuln = companionGo.GetComponent<InvulnerabilityComponent>();
            invuln.InvulnerableUntil = float.MaxValue;
            companion.TakeDamage(30f, null);
            Assert.AreEqual(40f, companion.CurrentHP);
        }

        // ===== FSM transitions =====

        [Test]
        public void FSM_StartsInIdle()
        {
            Assert.AreSame(CompanionStates.Idle, companion.Fsm.Current);
        }

        [Test]
        public void FSM_FarFromPlayer_IdleTransitionsToFollow()
        {
            playerGo.transform.position = new Vector3(10f, 0f, 0f); // far
            companion.Fsm.Tick(0.016f);
            Assert.AreSame(CompanionStates.Follow, companion.Fsm.Current);
        }

        [Test]
        public void FSM_Follow_MovesCompanionTowardTarget()
        {
            playerGo.transform.position = new Vector3(10f, 0f, 0f);
            companion.Fsm.Tick(0.016f); // Idle -> Follow
            Vector3 before = companion.transform.position;
            companion.Fsm.Tick(0.1f);   // Follow tick, moveSpeed=2 * 0.1 = 0.2 units
            float dx = companion.transform.position.x - before.x;
            Assert.Greater(dx, 0.1f, "Companion phải di chuyển về phía player");
            Assert.LessOrEqual(dx, 0.25f, "Speed bound cap");
        }

        [Test]
        public void FSM_Follow_NearTarget_TransitionsToIdle()
        {
            playerGo.transform.position = new Vector3(10f, 0f, 0f);
            companion.Fsm.Tick(0.016f); // Idle -> Follow
            // Jump companion gần player trong stopDistance.
            companion.transform.position = new Vector3(9.5f, 0f, 0f);
            companion.Fsm.Tick(0.016f);
            Assert.AreSame(CompanionStates.Idle, companion.Fsm.Current);
        }

        [Test]
        public void FSM_Stay_FollowTransitionsToIdle()
        {
            playerGo.transform.position = new Vector3(10f, 0f, 0f);
            companion.Fsm.Tick(0.016f); // enter Follow
            Assert.AreSame(CompanionStates.Follow, companion.Fsm.Current);
            companion.mode = CompanionMode.Stay;
            companion.Fsm.Tick(0.016f);
            Assert.AreSame(CompanionStates.Idle, companion.Fsm.Current);
        }

        [Test]
        public void FSM_Dead_StaysDead()
        {
            companion.TakeDamage(100f, null);
            companion.Fsm.Tick(1f);
            Assert.AreSame(CompanionStates.Dead, companion.Fsm.Current);
        }

        // ===== Hunger decay + starvation =====

        [Test]
        public void TickSurvival_DecaysHunger()
        {
            float before = companion.Hunger;
            companion.TickSurvival(10f);
            Assert.Less(companion.Hunger, before, "Hunger phải giảm theo time");
        }

        [Test]
        public void TickSurvival_Starving_DamagesHP()
        {
            var hunger = companionGo.GetComponent<HungerComponent>();
            hunger.Hunger = 0f;
            float hpBefore = companion.CurrentHP;
            companion.TickSurvival(2f);
            Assert.Less(companion.CurrentHP, hpBefore, "Starving → TickStarvation trừ HP");
        }

        [Test]
        public void Eat_RestoresHunger()
        {
            var hunger = companionGo.GetComponent<HungerComponent>();
            hunger.Hunger = 20f;
            companion.Eat(50f);
            Assert.AreEqual(70f, companion.Hunger, 0.01f);
        }

        [Test]
        public void TickSurvival_Dead_NoOp()
        {
            companion.TakeDamage(100f, null);
            float hungerBefore = companion.Hunger;
            companion.TickSurvival(5f);
            Assert.AreEqual(hungerBefore, companion.Hunger, 0.01f, "Dead companion → no survival tick");
        }

        // ===== IInteractable =====

        [Test]
        public void CanInteract_NullActor_False()
        {
            Assert.IsFalse(companion.CanInteract(null));
            Assert.IsTrue(companion.CanInteract(playerGo));
        }

        [Test]
        public void CanInteract_Dead_False()
        {
            companion.TakeDamage(100f, null);
            Assert.IsFalse(companion.CanInteract(playerGo));
        }

        [Test]
        public void Interact_TogglesMode_AndRaisesEvent()
        {
            int eventCount = 0;
            object lastArg = null;
            GameEvents.OnCompanionModeChanged += c => { eventCount++; lastArg = c; };

            Assert.AreEqual(CompanionMode.Follow, companion.mode);
            Assert.IsTrue(companion.Interact(playerGo));
            Assert.AreEqual(CompanionMode.Stay, companion.mode);
            Assert.AreEqual(1, eventCount);
            Assert.AreSame(companion, lastArg);

            companion.Interact(playerGo);
            Assert.AreEqual(CompanionMode.Follow, companion.mode);
            Assert.AreEqual(2, eventCount);
        }

        [Test]
        public void InteractLabel_ReflectsCurrentMode()
        {
            companion.mode = CompanionMode.Follow;
            StringAssert.Contains("ở lại", companion.InteractLabel);
            companion.mode = CompanionMode.Stay;
            StringAssert.Contains("đi theo", companion.InteractLabel);
        }

        // ===== ISaveable round-trip =====

        [Test]
        public void ISaveable_CaptureAndRestore_PersistsState()
        {
            companionGo.transform.position = new Vector3(5f, 3f, 0f);
            var hunger = companionGo.GetComponent<HungerComponent>();
            hunger.Hunger = 42f;
            companion.TakeDamage(10f, null);
            companion.mode = CompanionMode.Stay;

            var data = new SaveData();
            companion.CaptureState(data);
            Assert.AreEqual(1, data.companions.Count);
            Assert.AreEqual("companion_test", data.companions[0].companionId);
            Assert.AreEqual(30f, data.companions[0].hp, 0.01f);
            Assert.AreEqual(42f, data.companions[0].hunger, 0.01f);
            Assert.AreEqual((int)CompanionMode.Stay, data.companions[0].mode);

            // Scramble state then restore.
            companionGo.transform.position = Vector3.zero;
            hunger.Hunger = 0f;
            companion.TakeDamage(100f, null);
            companion.mode = CompanionMode.Follow;
            companion.RestoreState(data);

            Assert.AreEqual(new Vector3(5f, 3f, 0f), companionGo.transform.position);
            Assert.AreEqual(30f, companion.CurrentHP, 0.01f);
            Assert.AreEqual(42f, companion.Hunger, 0.01f);
            Assert.AreEqual(CompanionMode.Stay, companion.mode);
        }

        [Test]
        public void ISaveable_RestoreState_Null_SafeEarlyReturn()
        {
            Assert.DoesNotThrow(() => companion.RestoreState(null));
            Assert.DoesNotThrow(() => companion.RestoreState(new SaveData { companions = null }));
        }

        [Test]
        public void ISaveable_RestoreState_DeadCompanion_TransitionsToDeadState()
        {
            var data = new SaveData();
            data.companions.Add(new CompanionSaveData
            {
                companionId = "companion_test",
                position = Vector3.zero,
                hp = 0f,
                hunger = 50f,
                mode = (int)CompanionMode.Follow,
            });
            companion.RestoreState(data);
            Assert.IsTrue(companion.IsDead);
            Assert.AreSame(CompanionStates.Dead, companion.Fsm.Current);
        }

        [Test]
        public void ISaveable_OnEnable_RegistersWithSaveRegistry()
        {
            bool found = false;
            foreach (var s in SaveRegistry.OrderedSaveables())
                if (ReferenceEquals(s, companion)) { found = true; break; }
            Assert.IsTrue(found);
        }
    }
}
