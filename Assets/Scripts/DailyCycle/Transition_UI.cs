using TMPro;
using UnityEngine;

namespace Flipit.DailyCycle
{
    /// <summary>
    /// Displays "End of Day X" text and Event Card during transitions.
    /// Renders on an overlay Canvas with highest sorting order (1000).
    /// </summary>
    [RequireComponent(typeof(Canvas))]
    public class Transition_UI : MonoBehaviour
    {
        [SerializeField] private TMP_Text _dayText;
        [SerializeField] private TMP_Text _eventTitleText;
        [SerializeField] private TMP_Text _eventDescriptionText;
        [SerializeField] private GameObject _dayPanel;
        [SerializeField] private GameObject _eventCardPanel;

        private void Awake()
        {
            // Configure Canvas as Screen Space Overlay with highest sorting order
            Canvas canvas = GetComponent<Canvas>();
            if (canvas != null)
            {
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 1000;
            }

            // Ensure both panels start hidden
            HideAll();
        }

        /// <summary>
        /// Activates the day summary panel and displays "End of Day {dayNumber}".
        /// </summary>
        /// <param name="dayNumber">The current day number to display.</param>
        public void ShowDaySummary(int dayNumber)
        {
            if (_dayPanel == null)
            {
                Debug.LogWarning("[Transition_UI] _dayPanel reference not assigned. Cannot show day summary.");
                return;
            }

            if (_dayText == null)
            {
                Debug.LogWarning("[Transition_UI] _dayText reference not assigned. Cannot set day text.");
                _dayPanel.SetActive(true);
                return;
            }

            _dayPanel.SetActive(true);
            _dayText.text = $"End of Day {dayNumber}";
        }

        /// <summary>
        /// Deactivates the day summary panel.
        /// </summary>
        public void HideDaySummary()
        {
            if (_dayPanel == null)
            {
                Debug.LogWarning("[Transition_UI] _dayPanel reference not assigned. Cannot hide day summary.");
                return;
            }

            _dayPanel.SetActive(false);
        }

        /// <summary>
        /// Activates the event card panel and sets the title and description text.
        /// </summary>
        /// <param name="title">The event title to display.</param>
        /// <param name="description">The event description to display.</param>
        public void ShowEventCard(string title, string description)
        {
            if (_eventCardPanel == null)
            {
                Debug.LogWarning("[Transition_UI] _eventCardPanel reference not assigned. Cannot show event card.");
                return;
            }

            _eventCardPanel.SetActive(true);

            if (_eventTitleText == null)
            {
                Debug.LogWarning("[Transition_UI] _eventTitleText reference not assigned. Cannot set event title.");
            }
            else
            {
                _eventTitleText.text = title;
            }

            if (_eventDescriptionText == null)
            {
                Debug.LogWarning("[Transition_UI] _eventDescriptionText reference not assigned. Cannot set event description.");
            }
            else
            {
                _eventDescriptionText.text = description;
            }
        }

        /// <summary>
        /// Deactivates the event card panel.
        /// </summary>
        public void HideEventCard()
        {
            if (_eventCardPanel == null)
            {
                Debug.LogWarning("[Transition_UI] _eventCardPanel reference not assigned. Cannot hide event card.");
                return;
            }

            _eventCardPanel.SetActive(false);
        }

        /// <summary>
        /// Deactivates both the day summary panel and event card panel.
        /// </summary>
        public void HideAll()
        {
            if (_dayPanel != null)
            {
                _dayPanel.SetActive(false);
            }

            if (_eventCardPanel != null)
            {
                _eventCardPanel.SetActive(false);
            }
        }
    }
}
