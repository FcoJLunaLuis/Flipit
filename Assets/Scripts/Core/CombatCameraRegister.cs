using UnityEngine;

/// <summary>
/// Se coloca en la cámara de combate (CombatCamera) dentro del prefab CombatSystem.
/// Registra la cámara en el CameraManager al iniciar.
/// </summary>
[RequireComponent(typeof(Camera))]
public class CombatCameraRegister : MonoBehaviour
{
    private void Start()
    {
        if (CameraManager.Instance == null)
        {
            Debug.LogWarning("[CombatCameraRegister] CameraManager no encontrado. La cámara no se registrará.");
            return;
        }

        var cam = GetComponent<Camera>();
        CameraManager.Instance.RegistrarCamara(CameraManager.CameraType.Combat, cam);
    }
}
