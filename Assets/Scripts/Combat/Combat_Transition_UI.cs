using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

namespace Flipit.Combat
{
    /// <summary>
    /// Muestra la viñeta "RETO ACEPTADO" como overlay fullscreen
    /// y gestiona la carga asíncrona de la escena de combate.
    /// </summary>
    public class Combat_Transition_UI : MonoBehaviour
    {
        [SerializeField] private Canvas _overlayCanvas;
        [SerializeField] private TMP_Text _challengeText;

        [SerializeField, Range(1.5f, 3.0f)]
        private float _displayDuration = 2.0f;

        private Coroutine _transitionCoroutine;

        /// <summary>
        /// Displays the "RETO ACEPTADO" overlay, waits for the configured duration,
        /// validates the scene exists in Build Settings, and loads it asynchronously.
        /// </summary>
        /// <param name="sceneName">Target scene name to load.</param>
        /// <param name="onComplete">Invoked when the scene loads successfully.</param>
        /// <param name="onError">Invoked if scene validation or loading fails.</param>
        public void ShowTransition(string sceneName, Action onComplete, Action onError)
        {
            if (_overlayCanvas == null)
            {
                Debug.LogWarning("[Combat_Transition_UI] _overlayCanvas is null. Attempting scene load without overlay.");
            }
            else
            {
                _overlayCanvas.enabled = true;
            }

            if (_challengeText == null)
            {
                Debug.LogWarning("[Combat_Transition_UI] _challengeText is null. Skipping text assignment.");
            }
            else
            {
                _challengeText.text = "RETO ACEPTADO";
            }

            if (_transitionCoroutine != null)
            {
                StopCoroutine(_transitionCoroutine);
            }

            _transitionCoroutine = StartCoroutine(TransitionCoroutine(sceneName, onComplete, onError));
        }

        /// <summary>
        /// Disables the overlay Canvas.
        /// </summary>
        public void HideOverlay()
        {
            if (_overlayCanvas != null)
            {
                _overlayCanvas.enabled = false;
            }
        }

        private IEnumerator TransitionCoroutine(string sceneName, Action onComplete, Action onError)
        {
            yield return new WaitForSeconds(_displayDuration);

            // Validate scene exists in Build Settings
            int buildIndex = SceneUtility.GetBuildIndexByScenePath(sceneName);

            if (buildIndex < 0)
            {
                // Try with full path format as fallback
                buildIndex = SceneUtility.GetBuildIndexByScenePath("Assets/Scenes/" + sceneName + ".unity");
            }

            if (buildIndex < 0)
            {
                // Scene not in Build Settings — demo mode: show overlay then reset
                Debug.LogWarning($"[Combat_Transition_UI] Scene '{sceneName}' not found in Build Settings. Resetting to normal gameplay (demo mode).");
                HideOverlay();
                onError?.Invoke();
                _transitionCoroutine = null;
                yield break;
            }

            // Attempt to load the scene asynchronously
            AsyncOperation asyncLoad = null;
            try
            {
                asyncLoad = SceneManager.LoadSceneAsync(sceneName);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Combat_Transition_UI] Failed to load scene '{sceneName}': {ex.Message}");
                HideOverlay();
                onError?.Invoke();
                _transitionCoroutine = null;
                yield break;
            }

            if (asyncLoad == null)
            {
                Debug.LogError($"[Combat_Transition_UI] LoadSceneAsync returned null for scene '{sceneName}'.");
                HideOverlay();
                onError?.Invoke();
                _transitionCoroutine = null;
                yield break;
            }

            // Wait for scene to finish loading
            while (!asyncLoad.isDone)
            {
                yield return null;
            }

            onComplete?.Invoke();
            _transitionCoroutine = null;
        }
    }
}
