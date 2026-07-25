using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Flipit.Combat;

namespace Flipit.DailyCycle
{
    /// <summary>
    /// Singleton orchestrator for the daily gameplay loop.
    /// Manages day counter, transition sequence, event selection, and player respawn.
    /// </summary>
    public class Daily_Cycle_Manager : MonoBehaviour
    {
        /// <summary>
        /// Singleton instance accessible from anywhere.
        /// </summary>
        public static Daily_Cycle_Manager Instance { get; private set; }

        // === Serialized Configuration ===

        [SerializeField] private Transform _schoolSpawnPoint;
        [SerializeField] private GameObject _playerGameObject;
        [SerializeField] private PlayerInput _playerInput;
        [SerializeField] private Fade_System _fadeSystem;
        [SerializeField] private Transition_UI _transitionUI;


        [Header("Events")]
        [SerializeField] private Daily_Event[] _eventPool;
        [SerializeField] private float _positiveWeight = 30f;
        [SerializeField] private float _neutralWeight = 50f;
        [SerializeField] private float _negativeWeight = 20f;

        [Header("Timing")]
        [SerializeField] private float _fadeOutDuration = 1.0f;
        [SerializeField] private float _fadeInDuration = 1.0f;
        [SerializeField] private float _daySummaryDuration = 2.5f;
        [SerializeField] private float _eventCardDuration = 3.0f;

        // === Runtime State ===

        private int _dayCounter = 1;
        private bool _isTransitioning = false;
        private string _lastEventName = "None";

        // === Public API ===

        /// <summary>
        /// The current day number (starts at 1).
        /// </summary>
        public int CurrentDay => _dayCounter;

        /// <summary>
        /// Whether an end-of-day transition is currently in progress.
        /// </summary>
        public bool IsTransitioning => _isTransitioning;

        /// <summary>
        /// The title of the last executed daily event.
        /// </summary>
        public string LastEventName => _lastEventName;

        /// <summary>
        /// Serializable snapshot of current state for future Save/Load integration.
        /// </summary>
        public Daily_Cycle_Save_Data SaveData => new Daily_Cycle_Save_Data(_dayCounter, _lastEventName);

        /// <summary>
        /// Returns the current day number.
        /// </summary>
        /// <returns>Current day (starts at 1).</returns>
        public int GetCurrentDay() => _dayCounter;

        /// <summary>
        /// Sets the day counter to a specific value. Editor debug use only.
        /// </summary>
        /// <param name="day">Target day number (clamped to 1–9999).</param>
        public void SetDayCounterForDebug(int day)
        {
            _dayCounter = Mathf.Clamp(day, 1, 9999);
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// Advances the day counter by one, resets temporary effects, and broadcasts the DayStarted event.
        /// </summary>
        public void NextDay()
        {
            ResetTemporaryEffects();
            _dayCounter++;
            Daily_Cycle_Event_Bus.BroadcastDayStarted(_dayCounter);
        }

        /// <summary>
        /// Resets all temporary effects applied by daily events.
        /// No debuff persists across day boundaries (Req 7.6).
        /// When real temporary effects exist (movement speed, combat multiplier, vendor discount),
        /// reset them here.
        /// </summary>
        private void ResetTemporaryEffects()
        {
            // Placeholder: When real temporary effects exist (movement speed, combat reward multiplier,
            // vendor discount), reset them here. No debuff persists across days (Req 7.6).
            _lastEventName = "None";
        }

        /// <summary>
        /// Repositions the player at the school spawn point and broadcasts the PlayerRespawned event.
        /// Logs warnings and skips if references are not assigned.
        /// </summary>
        public void RespawnPlayer()
        {
            if (_schoolSpawnPoint == null)
            {
                Debug.LogWarning("[Daily_Cycle_Manager] School Spawn Point not assigned. Skipping respawn.");
                return;
            }
            if (_playerGameObject == null)
            {
                Debug.LogWarning("[Daily_Cycle_Manager] Player reference not assigned. Skipping respawn.");
                return;
            }

            // Disable CharacterController before moving (it blocks direct transform changes)
            var characterController = _playerGameObject.GetComponent<CharacterController>();
            if (characterController != null)
                characterController.enabled = false;

            _playerGameObject.transform.position = _schoolSpawnPoint.position;
            _playerGameObject.transform.rotation = _schoolSpawnPoint.rotation;

            if (characterController != null)
                characterController.enabled = true;

            Daily_Cycle_Event_Bus.BroadcastPlayerRespawned();
        }

        /// <summary>
        /// Initiates the end-of-day transition sequence.
        /// Guards against duplicate calls while a transition is already in progress.
        /// </summary>
        public void EndCurrentDay()
        {
            if (_isTransitioning)
            {
                Debug.LogWarning("[Daily_Cycle_Manager] Transition already in progress. Ignoring duplicate call.");
                return;
            }
            StartCoroutine(EndOfDayCoroutine());
        }

        /// <summary>
        /// Selects one event from the pool using weighted random category selection,
        /// then uniform selection within that category. Broadcasts the event title.
        /// </summary>
        public void TriggerRandomEvent()
        {
            if (_eventPool == null || _eventPool.Length == 0)
            {
                Debug.LogWarning("[Daily_Cycle_Manager] No events configured. Skipping event execution.");
                return;
            }

            // Normalize category weights
            float total = _positiveWeight + _neutralWeight + _negativeWeight;
            if (total <= 0f)
            {
                Debug.LogWarning("[Daily_Cycle_Manager] All event weights are zero. Skipping event execution.");
                return;
            }
            float normPositive = _positiveWeight / total;
            float normNeutral = _neutralWeight / total;

            // Roll for category
            float roll = Random.value;
            Daily_Event_Category selectedCategory;
            if (roll < normPositive)
                selectedCategory = Daily_Event_Category.Positive;
            else if (roll < normPositive + normNeutral)
                selectedCategory = Daily_Event_Category.Neutral;
            else
                selectedCategory = Daily_Event_Category.Negative;

            // Filter events by category
            var candidates = System.Array.FindAll(_eventPool, e => e != null && e.Category == selectedCategory);

            // Fallback to Neutral if empty
            if (candidates.Length == 0)
            {
                candidates = System.Array.FindAll(_eventPool, e => e != null && e.Category == Daily_Event_Category.Neutral);
                if (candidates.Length == 0)
                    return; // skip entirely
            }

            // Uniform random selection within category
            var selected = candidates[Random.Range(0, candidates.Length)];

            // Execute
            var context = BuildEventContext();
            try
            {
                selected.Execute(context);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[Daily_Cycle_Manager] Event '{selected.Title}' threw exception: {ex.Message}");
            }

            _lastEventName = selected.Title;
            Daily_Cycle_Event_Bus.BroadcastEventTriggered(selected.Title);
        }

        /// <summary>
        /// The full end-of-day transition coroutine sequence.
        /// Waits for combat idle, disables input, fades out, shows summary,
        /// triggers event, shows event card, advances day, respawns, fades in, restores input.
        /// </summary>
        private IEnumerator EndOfDayCoroutine()
        {
            _isTransitioning = true;
            Daily_Cycle_Event_Bus.BroadcastDayEnding(_dayCounter);

            // Wait for combat to finish if active (Req 11.3)
            while (Combat_System.Instance != null && Combat_System.Instance.CurrentState != CombatState.Idle)
            {
                yield return null;
            }

            // Disable player input (Req 4.1, 11.2)
            DisablePlayerInput();

            // Fade out (Req 4.2)
            if (_fadeSystem != null)
            {
                bool fadeOutComplete = false;
                _fadeSystem.FadeOut(_fadeOutDuration, () => fadeOutComplete = true);
                yield return new WaitUntil(() => fadeOutComplete);
            }
            else
            {
                Debug.LogWarning("[Daily_Cycle_Manager] Fade System not assigned. Skipping fade out.");
            }

            // Show day summary (Req 4.3, 9.1)
            if (_transitionUI != null)
            {
                _transitionUI.ShowDaySummary(_dayCounter);
            }
            else
            {
                Debug.LogWarning("[Daily_Cycle_Manager] Transition UI not assigned. Skipping day summary display.");
            }
            yield return new WaitForSeconds(_daySummaryDuration);
            if (_transitionUI != null)
            {
                _transitionUI.HideDaySummary();
            }

            // Trigger random event (Req 4.4)
            TriggerRandomEvent();

            // Show event card (Req 4.5, 9.2, 9.3)
            if (_transitionUI != null && _lastEventName != "None")
            {
                var lastEvent = GetLastExecutedEvent();
                if (lastEvent != null)
                {
                    _transitionUI.ShowEventCard(lastEvent.Title, lastEvent.Description);
                }
            }
            yield return new WaitForSeconds(_eventCardDuration);
            if (_transitionUI != null)
            {
                _transitionUI.HideEventCard();
            }

            // Advance day + respawn (Req 4.6, 4.7)
            NextDay();
            RespawnPlayer();

            // Fade in (Req 4.8)
            if (_fadeSystem != null)
            {
                bool fadeInComplete = false;
                _fadeSystem.FadeIn(_fadeInDuration, () => fadeInComplete = true);
                yield return new WaitUntil(() => fadeInComplete);
            }
            else
            {
                Debug.LogWarning("[Daily_Cycle_Manager] Fade System not assigned. Skipping fade in.");
            }

            // Restore input (Req 4.9)
            RestorePlayerInput();
            _isTransitioning = false;

            Daily_Cycle_Event_Bus.BroadcastDayEnded(_dayCounter - 1);
        }

        /// <summary>
        /// Builds the event context with current game state data.
        /// </summary>
        private IDailyEventContext BuildEventContext()
        {
            return new DailyEventContextData(_dayCounter, 0, 0);
        }

        /// <summary>
        /// Retrieves the last executed Daily_Event from the event pool by title.
        /// </summary>
        private Daily_Event GetLastExecutedEvent()
        {
            if (_eventPool == null || _lastEventName == "None")
                return null;

            return System.Array.Find(_eventPool, e => e != null && e.Title == _lastEventName);
        }

        /// <summary>
        /// Disables player input by switching to the UI action map.
        /// </summary>
        private void DisablePlayerInput()
        {
            if (_playerInput != null)
            {
                _playerInput.SwitchCurrentActionMap("UI");
            }
            else
            {
                Debug.LogWarning("[Daily_Cycle_Manager] PlayerInput not assigned. Cannot disable input.");
            }
        }

        /// <summary>
        /// Restores player input by switching back to the Player action map.
        /// </summary>
        private void RestorePlayerInput()
        {
            if (_playerInput != null)
            {
                _playerInput.SwitchCurrentActionMap("Player");
            }
            else
            {
                Debug.LogWarning("[Daily_Cycle_Manager] PlayerInput not assigned. Cannot restore input.");
            }
        }

        /// <summary>
        /// Lightweight context struct implementing IDailyEventContext.
        /// </summary>
        private struct DailyEventContextData : IDailyEventContext
        {
            public int CurrentDay { get; }
            public int PlayerCoins { get; }
            public int PlayerFlipits { get; }

            public DailyEventContextData(int day, int coins, int flipits)
            {
                CurrentDay = day;
                PlayerCoins = coins;
                PlayerFlipits = flipits;
            }
        }
    }
}
