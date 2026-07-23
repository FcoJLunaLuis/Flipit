using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

namespace Flipit.Combat
{
    public class Combat_Transition_UI : MonoBehaviour
    {
        [SerializeField] private Canvas _overlayCanvas;
        [SerializeField] private TMP_Text _challengeText;
        [SerializeField, Range(1.5f, 3.0f)] private float _displayDuration = 2.0f;

        private Coroutine _transitionCoroutine;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        public void ShowTransition(string sceneName, Action onComplete, Action onError)
        {
            if (_overlayCanvas != null) _overlayCanvas.enabled = true;
            if (_challengeText != null) _challengeText.text = "RETO ACEPTADO";

            if (_transitionCoroutine != null) StopCoroutine(_transitionCoroutine);
            _transitionCoroutine = StartCoroutine(TransitionCoroutine(sceneName, onComplete, onError));
        }

        public void HideOverlay()
        {
            if (_overlayCanvas != null) _overlayCanvas.enabled = false;
        }

        private IEnumerator TransitionCoroutine(string sceneName, Action onComplete, Action onError)
        {
            yield return new WaitForSeconds(_displayDuration);

            int buildIndex = SceneUtility.GetBuildIndexByScenePath(sceneName);
            if (buildIndex < 0)
                buildIndex = SceneUtility.GetBuildIndexByScenePath("Assets/Scenes/" + sceneName + ".unity");

            if (buildIndex < 0)
            {
                Debug.LogWarning($"[Combat_Transition_UI] Scene '{sceneName}' not in Build Settings. Demo mode reset.");
                HideOverlay();
                onError?.Invoke();
                _transitionCoroutine = null;
                yield break;
            }

            AsyncOperation asyncLoad = null;
            try { asyncLoad = SceneManager.LoadSceneAsync(sceneName); }
            catch (Exception ex)
            {
                Debug.LogError($"[Combat_Transition_UI] Failed: {ex.Message}");
                HideOverlay();
                onError?.Invoke();
                _transitionCoroutine = null;
                yield break;
            }

            if (asyncLoad == null)
            {
                HideOverlay();
                onError?.Invoke();
                _transitionCoroutine = null;
                yield break;
            }

            while (!asyncLoad.isDone)
                yield return null;

            onComplete?.Invoke();
            _transitionCoroutine = null;
        }
    }
}
