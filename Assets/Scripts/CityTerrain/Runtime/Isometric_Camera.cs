using UnityEngine;

namespace Flipit.CityTerrain
{
    /// <summary>
    /// Orthographic camera at fixed isometric angle (30° X, 45° Y).
    /// Follows the player with configurable offset and smoothing.
    /// </summary>
    public class Isometric_Camera : MonoBehaviour
    {
        [SerializeField] private Transform _target;
        [SerializeField] private float _followOffset = 10f;
        [SerializeField] private float _smoothSpeed = 5f;
        [SerializeField] private float _orthoSize = 8f;

        private static readonly Quaternion IsometricRotation =
            Quaternion.Euler(30f, 45f, 0f);

        private void Start()
        {
            // Set fixed isometric rotation
            transform.rotation = IsometricRotation;

            // Configure orthographic size if camera is present
            var cam = GetComponent<Camera>();
            if (cam != null)
            {
                cam.orthographic = true;
                cam.orthographicSize = _orthoSize;
            }

            // If no target assigned, try to find Player
            if (_target == null)
            {
                var player = GameObject.Find("Player");
                if (player != null)
                {
                    _target = player.transform;
                }
            }
        }

        private void LateUpdate()
        {
            if (_target == null)
                return;

            // Calculate desired position: offset along camera's back-vector from player
            Vector3 offset = IsometricRotation * new Vector3(0f, 0f, -_followOffset);
            Vector3 desiredPosition = _target.position + offset;

            // Smooth follow using linear interpolation
            transform.position = Vector3.Lerp(transform.position, desiredPosition, _smoothSpeed * Time.deltaTime);
        }
    }
}
