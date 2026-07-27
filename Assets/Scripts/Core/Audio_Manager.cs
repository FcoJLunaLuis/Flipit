using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Flipit.Core
{
    /// <summary>
    /// Singleton que gestiona la música de fondo del juego.
    /// Cambia automáticamente el track según la escena activa con crossfade suave.
    /// </summary>
    public class Audio_Manager : MonoBehaviour
    {
        public static Audio_Manager Instance { get; private set; }

        [Serializable]
        public class SceneAudioMapping
        {
            public string SceneName;
            public AudioClip MusicClip;
            [Range(0f, 1f)] public float Volume = 0.7f;
        }

        [Header("Scene Music Mappings")]
        [SerializeField] private SceneAudioMapping[] _sceneMappings;

        [Header("Crossfade")]
        [SerializeField, Range(0.5f, 3f)] private float _crossfadeDuration = 1.0f;

        [Header("Master Volume")]
        [SerializeField, Range(0f, 1f)] private float _masterVolume = 1.0f;

        private AudioSource _sourceA;
        private AudioSource _sourceB;
        private AudioSource _activeSource;
        private Coroutine _crossfadeCoroutine;

        // Playhead memory for resume functionality
        private AudioClip _previousClip;
        private float _previousPlayhead;
        private float _previousVolume;

        /// <summary>
        /// Volumen maestro de la música. Afecta todos los tracks.
        /// </summary>
        public float MasterVolume
        {
            get => _masterVolume;
            set
            {
                _masterVolume = Mathf.Clamp01(value);
                if (_activeSource != null && _activeSource.isPlaying)
                    _activeSource.volume = GetMappingVolume(_activeSource.clip) * _masterVolume;
            }
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            if (transform.parent == null)
                DontDestroyOnLoad(gameObject);

            _sourceA = gameObject.AddComponent<AudioSource>();
            _sourceB = gameObject.AddComponent<AudioSource>();

            ConfigureSource(_sourceA);
            ConfigureSource(_sourceB);

            _activeSource = _sourceA;
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void Start()
        {
            PlayMusicForScene(SceneManager.GetActiveScene().name);
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (mode == LoadSceneMode.Additive) return;
            PlayMusicForScene(scene.name);
        }

        /// <summary>
        /// Reproduce la música mapeada para una escena con crossfade.
        /// </summary>
        public void PlayMusicForScene(string sceneName)
        {
            SceneAudioMapping mapping = FindMapping(sceneName);
            if (mapping == null || mapping.MusicClip == null)
            {
                Debug.LogWarning($"[Audio_Manager] No music mapping for scene: {sceneName}");
                return;
            }

            if (_activeSource.clip == mapping.MusicClip && _activeSource.isPlaying)
                return;

            CrossfadeTo(mapping.MusicClip, mapping.Volume * _masterVolume);
        }

        /// <summary>
        /// Reproduce un clip específico con crossfade. Útil para créditos o momentos especiales.
        /// </summary>
        public void PlayMusic(AudioClip clip, float volume = 0.7f)
        {
            if (clip == null) return;
            if (_activeSource.clip == clip && _activeSource.isPlaying) return;

            CrossfadeTo(clip, volume * _masterVolume);
        }

        /// <summary>
        /// Detiene toda la música con fade out.
        /// </summary>
        public void StopMusic()
        {
            if (_crossfadeCoroutine != null)
                StopCoroutine(_crossfadeCoroutine);

            _crossfadeCoroutine = StartCoroutine(FadeOut(_activeSource, _crossfadeDuration));
        }

        /// <summary>
        /// Pausa la música actual.
        /// </summary>
        public void PauseMusic()
        {
            if (_activeSource != null && _activeSource.isPlaying)
                _activeSource.Pause();
        }

        /// <summary>
        /// Reanuda la música pausada.
        /// </summary>
        public void ResumeMusic()
        {
            if (_activeSource != null && !_activeSource.isPlaying && _activeSource.clip != null)
                _activeSource.UnPause();
        }

        /// <summary>
        /// Reanuda el track anterior desde donde se quedó (playhead guardado).
        /// Útil para volver de combate a exploración.
        /// </summary>
        public void ResumeLastTrack()
        {
            if (_previousClip == null)
            {
                // Fallback: play scene music from scratch
                PlayMusicForScene(SceneManager.GetActiveScene().name);
                return;
            }

            if (_crossfadeCoroutine != null)
                StopCoroutine(_crossfadeCoroutine);

            AudioSource newSource = (_activeSource == _sourceA) ? _sourceB : _sourceA;
            newSource.clip = _previousClip;
            newSource.volume = 0f;
            newSource.loop = true;
            newSource.time = _previousPlayhead;
            newSource.Play();

            float targetVolume = _previousVolume > 0f ? _previousVolume : 0.6f * _masterVolume;

            _crossfadeCoroutine = StartCoroutine(CrossfadeCoroutine(_activeSource, newSource, targetVolume));
            _activeSource = newSource;

            // Clear stored state
            _previousClip = null;
            _previousPlayhead = 0f;
            _previousVolume = 0f;
        }

        private void CrossfadeTo(AudioClip newClip, float targetVolume)
        {
            if (_crossfadeCoroutine != null)
                StopCoroutine(_crossfadeCoroutine);

            // Store previous clip state for resume
            if (_activeSource != null && _activeSource.clip != null && _activeSource.isPlaying)
            {
                _previousClip = _activeSource.clip;
                _previousPlayhead = _activeSource.time;
                _previousVolume = _activeSource.volume;
            }

            AudioSource newSource = (_activeSource == _sourceA) ? _sourceB : _sourceA;
            newSource.clip = newClip;
            newSource.volume = 0f;
            newSource.loop = true;
            newSource.Play();

            _crossfadeCoroutine = StartCoroutine(CrossfadeCoroutine(_activeSource, newSource, targetVolume));
            _activeSource = newSource;
        }

        private IEnumerator CrossfadeCoroutine(AudioSource fadeOut, AudioSource fadeIn, float targetVolume)
        {
            float elapsed = 0f;
            float startVolume = fadeOut.volume;

            while (elapsed < _crossfadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / _crossfadeDuration;

                fadeOut.volume = Mathf.Lerp(startVolume, 0f, t);
                fadeIn.volume = Mathf.Lerp(0f, targetVolume, t);

                yield return null;
            }

            fadeOut.volume = 0f;
            fadeOut.Stop();
            fadeIn.volume = targetVolume;

            _crossfadeCoroutine = null;
        }

        private IEnumerator FadeOut(AudioSource source, float duration)
        {
            float startVolume = source.volume;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                source.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
                yield return null;
            }

            source.volume = 0f;
            source.Stop();
            _crossfadeCoroutine = null;
        }

        private SceneAudioMapping FindMapping(string sceneName)
        {
            if (_sceneMappings == null) return null;

            foreach (var mapping in _sceneMappings)
            {
                if (string.Equals(mapping.SceneName, sceneName, StringComparison.OrdinalIgnoreCase))
                    return mapping;
            }
            return null;
        }

        private float GetMappingVolume(AudioClip clip)
        {
            if (_sceneMappings == null || clip == null) return 0.7f;

            foreach (var mapping in _sceneMappings)
            {
                if (mapping.MusicClip == clip)
                    return mapping.Volume;
            }
            return 0.7f;
        }

        private void ConfigureSource(AudioSource source)
        {
            source.playOnAwake = false;
            source.loop = true;
            source.spatialBlend = 0f;
            source.priority = 0;
        }
    }
}
