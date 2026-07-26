using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Flipit.Dialogue.Tests
{
    /// <summary>
    /// Tests for the Dialogue_Manager state machine.
    /// Validates Property 3: State machine transition validity
    /// and basic singleton/initialization behavior.
    /// </summary>
    [TestFixture]
    public class DialogueStateMachineTests
    {
        private GameObject _managerObject;
        private Dialogue_Manager _manager;

        [SetUp]
        public void SetUp()
        {
            _managerObject = new GameObject("Dialogue_Manager");
            _manager = _managerObject.AddComponent<Dialogue_Manager>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_managerObject != null)
                UnityEngine.Object.DestroyImmediate(_managerObject);

            // Reset singleton
            if (Dialogue_Manager.Instance == _manager)
            {
                // Instance will be null after destroy, but reset just in case
            }
        }

        // --- Initialization Tests ---

        [Test]
        public void InitialState_IsIdle()
        {
            // Requirement 10.1: Initial state SHALL be Idle
            Assert.AreEqual(DialogueState.Idle, _manager.CurrentState);
        }

        [Test]
        public void Singleton_Instance_IsSetInAwake()
        {
            // Singleton should be set after Awake runs (AddComponent triggers Awake)
            Assert.AreEqual(_manager, Dialogue_Manager.Instance);
        }

        // --- Valid Transition Tests ---

        [Test]
        public void TryTransition_IdleToTyping_ReturnsTrue()
        {
            // Requirement 10.2: Idle -> Typing is valid
            bool result = _manager.TryTransition(DialogueState.Typing);

            Assert.IsTrue(result);
            Assert.AreEqual(DialogueState.Typing, _manager.CurrentState);
        }

        [Test]
        public void TryTransition_TypingToWaitingForInput_ReturnsTrue()
        {
            // Requirement 10.3: Typing -> WaitingForInput is valid
            _manager.TryTransition(DialogueState.Typing);

            bool result = _manager.TryTransition(DialogueState.WaitingForInput);

            Assert.IsTrue(result);
            Assert.AreEqual(DialogueState.WaitingForInput, _manager.CurrentState);
        }

        [Test]
        public void TryTransition_TypingToShowingChoices_ReturnsTrue()
        {
            // Requirement 10.3: Typing -> ShowingChoices is valid
            _manager.TryTransition(DialogueState.Typing);

            bool result = _manager.TryTransition(DialogueState.ShowingChoices);

            Assert.IsTrue(result);
            Assert.AreEqual(DialogueState.ShowingChoices, _manager.CurrentState);
        }

        [Test]
        public void TryTransition_TypingToClosing_ReturnsTrue()
        {
            // Requirement 10.3: Typing -> Closing is valid
            _manager.TryTransition(DialogueState.Typing);

            bool result = _manager.TryTransition(DialogueState.Closing);

            Assert.IsTrue(result);
            Assert.AreEqual(DialogueState.Closing, _manager.CurrentState);
        }

        [Test]
        public void TryTransition_WaitingForInputToTyping_ReturnsTrue()
        {
            // Requirement 10.4: WaitingForInput -> Typing is valid
            _manager.TryTransition(DialogueState.Typing);
            _manager.TryTransition(DialogueState.WaitingForInput);

            bool result = _manager.TryTransition(DialogueState.Typing);

            Assert.IsTrue(result);
            Assert.AreEqual(DialogueState.Typing, _manager.CurrentState);
        }

        [Test]
        public void TryTransition_WaitingForInputToClosing_ReturnsTrue()
        {
            // Requirement 10.4: WaitingForInput -> Closing is valid
            _manager.TryTransition(DialogueState.Typing);
            _manager.TryTransition(DialogueState.WaitingForInput);

            bool result = _manager.TryTransition(DialogueState.Closing);

            Assert.IsTrue(result);
            Assert.AreEqual(DialogueState.Closing, _manager.CurrentState);
        }

        [Test]
        public void TryTransition_ShowingChoicesToTyping_ReturnsTrue()
        {
            // Requirement 10.5: ShowingChoices -> Typing is valid
            _manager.TryTransition(DialogueState.Typing);
            _manager.TryTransition(DialogueState.ShowingChoices);

            bool result = _manager.TryTransition(DialogueState.Typing);

            Assert.IsTrue(result);
            Assert.AreEqual(DialogueState.Typing, _manager.CurrentState);
        }

        [Test]
        public void TryTransition_ShowingChoicesToClosing_ReturnsTrue()
        {
            // Requirement 10.5: ShowingChoices -> Closing is valid
            _manager.TryTransition(DialogueState.Typing);
            _manager.TryTransition(DialogueState.ShowingChoices);

            bool result = _manager.TryTransition(DialogueState.Closing);

            Assert.IsTrue(result);
            Assert.AreEqual(DialogueState.Closing, _manager.CurrentState);
        }

        [Test]
        public void TryTransition_ClosingToIdle_ReturnsTrue()
        {
            // Requirement 10.6: Closing -> Idle is valid
            _manager.TryTransition(DialogueState.Typing);
            _manager.TryTransition(DialogueState.Closing);

            bool result = _manager.TryTransition(DialogueState.Idle);

            Assert.IsTrue(result);
            Assert.AreEqual(DialogueState.Idle, _manager.CurrentState);
        }

        // --- Invalid Transition Tests ---

        [Test]
        public void TryTransition_IdleToClosing_ReturnsFalse()
        {
            // Requirement 10.7, 10.8: Invalid transition returns false
            bool result = _manager.TryTransition(DialogueState.Closing);

            Assert.IsFalse(result);
            Assert.AreEqual(DialogueState.Idle, _manager.CurrentState);
        }

        [Test]
        public void TryTransition_IdleToWaitingForInput_ReturnsFalse()
        {
            bool result = _manager.TryTransition(DialogueState.WaitingForInput);

            Assert.IsFalse(result);
            Assert.AreEqual(DialogueState.Idle, _manager.CurrentState);
        }

        [Test]
        public void TryTransition_IdleToShowingChoices_ReturnsFalse()
        {
            bool result = _manager.TryTransition(DialogueState.ShowingChoices);

            Assert.IsFalse(result);
            Assert.AreEqual(DialogueState.Idle, _manager.CurrentState);
        }

        [Test]
        public void TryTransition_TypingToIdle_ReturnsFalse()
        {
            _manager.TryTransition(DialogueState.Typing);

            bool result = _manager.TryTransition(DialogueState.Idle);

            Assert.IsFalse(result);
            Assert.AreEqual(DialogueState.Typing, _manager.CurrentState);
        }

        [Test]
        public void TryTransition_InvalidTransition_LogsWarning()
        {
            // Requirement 10.7: Log warning with current and target state
            LogAssert.Expect(LogType.Warning,
                "[Dialogue_Manager] Invalid state transition attempted: Idle -> Closing");

            _manager.TryTransition(DialogueState.Closing);
        }

        [Test]
        public void TryTransition_InvalidTransition_StateUnchanged()
        {
            // Requirement 10.7: Remain in current state
            _manager.TryTransition(DialogueState.Typing);
            var stateBefore = _manager.CurrentState;

            _manager.TryTransition(DialogueState.Idle);

            Assert.AreEqual(stateBefore, _manager.CurrentState);
        }

        // --- Property-Based Test: Transition Validity (Property 3) ---

        /// <summary>
        /// Property 3: State machine transition validity
        /// For any current DialogueState and any target DialogueState, TryTransition returns true
        /// and updates the state if and only if the pair exists in the valid transition set.
        /// 
        /// **Validates: Requirements 10.2, 10.3, 10.4, 10.5, 10.6, 10.7, 10.8**
        /// </summary>
        [Test]
        public void Property3_TransitionValidity_AllPairs()
        {
            var validTransitions = new HashSet<(DialogueState, DialogueState)>
            {
                (DialogueState.Idle, DialogueState.Typing),
                (DialogueState.Typing, DialogueState.WaitingForInput),
                (DialogueState.Typing, DialogueState.ShowingChoices),
                (DialogueState.Typing, DialogueState.Closing),
                (DialogueState.WaitingForInput, DialogueState.Typing),
                (DialogueState.WaitingForInput, DialogueState.Closing),
                (DialogueState.ShowingChoices, DialogueState.Typing),
                (DialogueState.ShowingChoices, DialogueState.Closing),
                (DialogueState.Closing, DialogueState.Idle)
            };

            var allStates = (DialogueState[])Enum.GetValues(typeof(DialogueState));

            PropertyTestUtility.ForAll(random =>
            {
                // Pick a random source state and target state
                var sourceState = random.PickFrom(allStates);
                var targetState = random.PickFrom(allStates);

                // We need a fresh manager for each iteration to set state independently
                var go = new GameObject("TestManager");
                var manager = go.AddComponent<Dialogue_Manager>();

                // Navigate to the source state using valid transitions
                bool reachedSource = NavigateToState(manager, sourceState);

                if (reachedSource)
                {
                    Assert.AreEqual(sourceState, manager.CurrentState,
                        $"Failed to set up source state {sourceState}");

                    bool isValidPair = validTransitions.Contains((sourceState, targetState));
                    bool result = manager.TryTransition(targetState);

                    if (isValidPair)
                    {
                        Assert.IsTrue(result,
                            $"Expected valid transition {sourceState} -> {targetState} to return true");
                        Assert.AreEqual(targetState, manager.CurrentState,
                            $"Expected state to be {targetState} after valid transition from {sourceState}");
                    }
                    else
                    {
                        Assert.IsFalse(result,
                            $"Expected invalid transition {sourceState} -> {targetState} to return false");
                        Assert.AreEqual(sourceState, manager.CurrentState,
                            $"Expected state to remain {sourceState} after invalid transition to {targetState}");
                    }
                }

                UnityEngine.Object.DestroyImmediate(go);
            }, iterations: 200);
        }

        /// <summary>
        /// Helper: navigate to a desired state via valid transitions from Idle.
        /// Returns false if the state is unreachable (e.g., Transitioning has no path in).
        /// </summary>
        private static bool NavigateToState(Dialogue_Manager manager, DialogueState target)
        {
            if (target == DialogueState.Idle)
                return true; // Already in Idle

            if (target == DialogueState.Typing)
            {
                manager.TryTransition(DialogueState.Typing);
                return true;
            }

            if (target == DialogueState.WaitingForInput)
            {
                manager.TryTransition(DialogueState.Typing);
                manager.TryTransition(DialogueState.WaitingForInput);
                return true;
            }

            if (target == DialogueState.ShowingChoices)
            {
                manager.TryTransition(DialogueState.Typing);
                manager.TryTransition(DialogueState.ShowingChoices);
                return true;
            }

            if (target == DialogueState.Closing)
            {
                manager.TryTransition(DialogueState.Typing);
                manager.TryTransition(DialogueState.Closing);
                return true;
            }

            // Transitioning is reserved and has no inbound transitions
            if (target == DialogueState.Transitioning)
                return false;

            return false;
        }

    }
}
