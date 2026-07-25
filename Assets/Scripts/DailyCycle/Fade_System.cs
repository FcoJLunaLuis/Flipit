using System;
using System.Collections;
using UnityEngine;

namespace Flipit.DailyCycle
{
    /// <summary>
    /// Full-screen fade overlay using a CanvasGroup on Screen Space Overlay (sort order 999).
    /// Provides FadeOut (alpha 0→1) and FadeIn (alpha 1→0) with configurable duration and completion callbacks.
    /// </summary>
    [RequireComponent(typeof(Canvas))]
    public class Fade_System : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;

        private Coroutine _activeFadeCoroutine;
        private Action _activeCallback;
        private float _targetAlpha;

        private void Awake()
        {
            // Configure Canvas as Screen Space Overlay with sorting order 999
            Canvas canvas = GetComponent<Canvas>();
            if (canvas != null)
            {
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 999;
            }

            // Initialize alpha to 0 (fully transparent)
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0f;
            }
            else
            {
                Debug.LogError("[Fade_System] CanvasGroup reference not assigned.");
            }
        }

        /// <summary>
        /// Fades the overlay from transparent to opaque (alpha 0→1) over the specified duration.
        /// If a fade is already in progress, it is snapped to its target and its callback invoked before starting.
        /// </summary>
        /// <param name="duration">Fade duration in seconds, clamped to [0.1, 5.0].</param>
        /// <param name="onComplete">Optional callback invoked when fade completes.</param>
        public void FadeOut(float duration, Action onComplete = null)
        {
            duration = Mathf.Clamp(duration, 0.1f, 5.0f);
            StartFade(0f, 1f, duration, onComplete);
        }

        /// <summary>
        /// Fades the overlay from opaque to transparent (alpha 1→0) over the specified duration.
        /// If a fade is already in progress, it is snapped to its target and its callback invoked before starting.
        /// </summary>
        /// <param name="duration">Fade duration in seconds, clamped to [0.1, 5.0].</param>
        /// <param name="onComplete">Optional callback invoked when fade completes.</param>
        public void FadeIn(float duration, Action onComplete = null)
        {
            duration = Mathf.Clamp(duration, 0.1f, 5.0f);
            StartFade(1f, 0f, duration, onComplete);
        }

        private void StartFade(float from, float to, float duration, Action onComplete)
        {
            // If a fade is already in progress, snap to its target and invoke its callback
            if (_activeFadeCoroutine != null)
            {
                StopCoroutine(_activeFadeCoroutine);
                _activeFadeCoroutine = null;

                // Snap to the previous fade's target alpha
                if (_canvasGroup != null)
                {
                    _canvasGroup.alpha = _targetAlpha;
                }

                // Invoke the previous fade's callback
                Action previousCallback = _activeCallback;
                _activeCallback = null;
                previousCallback?.Invoke();
            }

            // Store new target and callback
            _targetAlpha = to;
            _activeCallback = onComplete;

            // Start the new fade coroutine
            _activeFadeCoroutine = StartCoroutine(FadeCoroutine(from, to, duration, onComplete));
        }

        private IEnumerator FadeCoroutine(float from, float to, float duration, Action onComplete)
        {
            if (_canvasGroup == null)
            {
                Debug.LogError("[Fade_System] CanvasGroup reference is null. Cannot perform fade.");
                _activeFadeCoroutine = null;
                _activeCallback = null;
                onComplete?.Invoke();
                yield break;
            }

            _canvasGroup.alpha = from;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                _canvasGroup.alpha = Mathf.Lerp(from, to, t);
                yield return null;
            }

            // Ensure we land exactly on target
            _canvasGroup.alpha = to;

            // Clean up state
            _activeFadeCoroutine = null;
            _activeCallback = null;

            // Invoke completion callback
            onComplete?.Invoke();
        }
    }
}