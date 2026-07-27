using UnityEngine;

/// <summary>
/// Se coloca en la cámara del Player.
/// Registra la cámara en el CameraManager al iniciar.
/// </summary>
[RequireComponent(typeof(Camera))]
public class PlayerCameraRegister : MonoBehaviour
{
    private void Start()
    {
        if (CameraManager.Instance == null)
        {
            Debug.LogWarning("[PlayerCameraRegister] CameraManager no encontrado. La cámara no se registrará.");
            return;
        }

        var cam = GetComponent<Camera>();
        CameraManager.Instance.RegistrarCamara(CameraManager.CameraType.Player, cam);
    }
}
