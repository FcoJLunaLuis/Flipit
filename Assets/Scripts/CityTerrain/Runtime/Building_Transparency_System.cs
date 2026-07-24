using System.Collections.Generic;
using UnityEngine;

namespace Flipit.CityTerrain
{
    /// <summary>
    /// Detects buildings between camera and player via raycast.
    /// Fades occluding buildings to configurable alpha over a duration.
    /// Restores alpha when no longer occluding.
    /// </summary>
    public class Building_Transparency_System : MonoBehaviour
    {
        [SerializeField] private Transform _player;
        [SerializeField] private float _targetAlpha = 0.3f;
        [SerializeField] private float _fadeDuration = 0.2f;
        [SerializeField] private LayerMask _cityGeometryLayer;

        private Dictionary<Renderer, FadeState> _trackedBuildings = new Dictionary<Renderer, FadeState>();

        private struct FadeState
        {
            public float CurrentAlpha;
            public float TargetAlpha;
            public Material Material;
        }

        private void Update()
        {
            if (_player == null)
                return;

            // Detect currently occluding buildings
            HashSet<Renderer> currentlyOccluding = DetectOccludingBuildings();

            // Collect keys to iterate safely (avoid modifying dictionary during enumeration)
            var keys = new List<Renderer>(_trackedBuildings.Keys);
            var toRemove = new List<Renderer>();
            var toUpdate = new List<KeyValuePair<Renderer, FadeState>>();

            foreach (var key in keys)
            {
                if (key == null)
                {
                    toRemove.Add(key);
                    continue;
                }

                FadeState state = _trackedBuildings[key];

                if (currentlyOccluding.Contains(key))
                {
                    state.TargetAlpha = _targetAlpha;
                }
                else
                {
                    state.TargetAlpha = 1f;
                }

                // Lerp toward target
                float fadeSpeed = _fadeDuration > 0f ? (1f / _fadeDuration) * Time.deltaTime : 1f;
                state.CurrentAlpha = Mathf.MoveTowards(state.CurrentAlpha, state.TargetAlpha, fadeSpeed);

                // Apply alpha
                if (state.Material != null)
                {
                    Color color = state.Material.color;
                    color.a = state.CurrentAlpha;
                    state.Material.color = color;

                    if (state.CurrentAlpha < 1f)
                    {
                        SetMaterialTransparent(state.Material);
                    }
                    else
                    {
                        SetMaterialOpaque(state.Material);
                        // Remove from tracking when fully opaque and not occluding
                        if (!currentlyOccluding.Contains(key))
                        {
                            toRemove.Add(key);
                            continue;
                        }
                    }
                }

                toUpdate.Add(new KeyValuePair<Renderer, FadeState>(key, state));
            }

            // Apply updates after iteration
            foreach (var kvp in toUpdate)
            {
                _trackedBuildings[kvp.Key] = kvp.Value;
            }

            // Clean up removed entries
            foreach (var key in toRemove)
            {
                _trackedBuildings.Remove(key);
            }

            // Add newly occluding buildings to tracking
            foreach (var renderer in currentlyOccluding)
            {
                if (renderer == null) continue;
                if (!_trackedBuildings.ContainsKey(renderer))
                {
                    Material mat = renderer.material; // creates instance
                    _trackedBuildings[renderer] = new FadeState
                    {
                        CurrentAlpha = 1f,
                        TargetAlpha = _targetAlpha,
                        Material = mat
                    };
                }
            }
        }

        private HashSet<Renderer> DetectOccludingBuildings()
        {
            var occluding = new HashSet<Renderer>();

            if (_player == null)
                return occluding;

            Vector3 cameraPosition = transform.position;
            Vector3 playerPosition = _player.position;
            Vector3 direction = playerPosition - cameraPosition;
            float distance = direction.magnitude;

            if (distance <= 0f)
                return occluding;

            RaycastHit[] hits = Physics.RaycastAll(cameraPosition, direction.normalized, distance, _cityGeometryLayer);

            foreach (var hit in hits)
            {
                // Skip the player itself
                if (hit.collider.transform == _player || hit.collider.transform.IsChildOf(_player))
                    continue;

                Renderer renderer = hit.collider.GetComponent<Renderer>();
                if (renderer != null)
                {
                    occluding.Add(renderer);
                }
            }

            return occluding;
        }

        private void SetMaterialTransparent(Material mat)
        {
            mat.SetFloat("_Mode", 3); // Transparent mode
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = 3000;
        }

        private void SetMaterialOpaque(Material mat)
        {
            mat.SetFloat("_Mode", 0); // Opaque mode
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
            mat.SetInt("_ZWrite", 1);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.DisableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = -1;
        }
    }
}
