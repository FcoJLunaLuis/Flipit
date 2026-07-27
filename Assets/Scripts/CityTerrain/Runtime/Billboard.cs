using UnityEngine;

namespace Flipit.CityTerrain
{
    /// <summary>
    /// Makes a world-space canvas always face the camera horizontally.
    /// Only rotates on the Y axis so text stays upright and readable.
    /// </summary>
    public class Billboard : MonoBehaviour
    {
        private Camera _camera;

        private void Start()
        {
            _camera = Camera.main;
        }

        private void LateUpdate()
        {
            if (_camera == null)
            {
                _camera = Camera.main;
                if (_camera == null) return;
            }

            // Only rotate on Y axis to face camera (keeps text horizontal)
            Vector3 dirToCamera = _camera.transform.position - transform.position;
            dirToCamera.y = 0f; // Flatten to horizontal plane
            if (dirToCamera.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.LookRotation(-dirToCamera, Vector3.up);
            }
        }
    }
}
