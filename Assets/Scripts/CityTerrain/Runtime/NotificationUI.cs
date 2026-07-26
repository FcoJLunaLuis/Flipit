using UnityEngine;
using TMPro;
using System.Collections;

namespace Flipit.CityTerrain
{
    /// <summary>
    /// Displays temporary notification messages on screen (e.g., "Reto Aceptado").
    /// Singleton accessible via NotificationUI.Instance.
    /// </summary>
    public class NotificationUI : MonoBehaviour
    {
        public static NotificationUI Instance { get; private set; }

        [SerializeField] private TextMeshProUGUI _messageText;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private float _displayDuration = 2f;
        [SerializeField] private float _fadeDuration = 0.5f;

        private Coroutine _currentNotification;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (_canvasGroup != null)
                _canvasGroup.alpha = 0f;
        }

        /// <summary>
        /// Shows a notification message on screen that fades in, stays, then fades out.
        /// </summary>
        public void ShowNotification(string message)
        {
            if (_currentNotification != null)
                StopCoroutine(_currentNotification);

            _currentNotification = StartCoroutine(ShowNotificationCoroutine(message));
        }

        private IEnumerator ShowNotificationCoroutine(string message)
        {
            if (_messageText != null)
                _messageText.text = message;

            if (_canvasGroup == null)
                yield break;

            // Fade in
            float elapsed = 0f;
            while (elapsed < _fadeDuration)
            {
                elapsed += Time.deltaTime;
                _canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / _fadeDuration);
                yield return null;
            }
            _canvasGroup.alpha = 1f;

            // Hold
            yield return new WaitForSeconds(_displayDuration);

            // Fade out
            elapsed = 0f;
            while (elapsed < _fadeDuration)
            {
                elapsed += Time.deltaTime;
                _canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / _fadeDuration);
                yield return null;
            }
            _canvasGroup.alpha = 0f;

            _currentNotification = null;
        }
    }
}
