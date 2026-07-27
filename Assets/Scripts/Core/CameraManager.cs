using System;
using UnityEngine;

/// <summary>
/// Singleton que gestiona qué cámara está activa en cada momento.
/// Cada sistema (Player, CoinFlip, Combat) registra su cámara al iniciar.
/// Solo una cámara está activa a la vez. Gestiona AudioListener automáticamente.
/// </summary>
public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance { get; private set; }

    public enum CameraType
    {
        Player,
        CoinFlip,
        Combat
    }

    /// <summary>
    /// Se dispara cuando cambia la cámara activa. Parámetros: (anterior, nueva).
    /// </summary>
    public event Action<CameraType, CameraType> OnCameraCambiada;

    private Camera _playerCamera;
    private Camera _coinFlipCamera;
    private Camera _combatCamera;

    private CameraType _camaraActual = CameraType.Player;

    public CameraType CamaraActual => _camaraActual;

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
    }

    /// <summary>
    /// Registra una cámara para un tipo específico.
    /// Llamar desde Start() de cada sistema que tiene cámara.
    /// </summary>
    public void RegistrarCamara(CameraType tipo, Camera cam)
    {
        if (cam == null)
        {
            Debug.LogWarning($"[CameraManager] Intento de registrar cámara null para tipo {tipo}.");
            return;
        }

        switch (tipo)
        {
            case CameraType.Player:
                _playerCamera = cam;
                break;
            case CameraType.CoinFlip:
                _coinFlipCamera = cam;
                break;
            case CameraType.Combat:
                _combatCamera = cam;
                break;
        }

        Debug.Log($"[CameraManager] Cámara {tipo} registrada: {cam.gameObject.name}");

        // Si no es la cámara activa actual, desactivarla
        if (tipo != _camaraActual)
        {
            DesactivarCamara(cam);
        }
    }

    /// <summary>
    /// Activa la cámara del tipo indicado y desactiva las demás.
    /// </summary>
    public void ActivarCamara(CameraType tipo)
    {
        Camera camaraObjetivo = ObtenerCamara(tipo);
        if (camaraObjetivo == null)
        {
            Debug.LogWarning($"[CameraManager] No hay cámara registrada para tipo {tipo}. No se puede activar.");
            return;
        }

        CameraType anterior = _camaraActual;
        _camaraActual = tipo;

        // Desactivar todas
        DesactivarCamara(_playerCamera);
        DesactivarCamara(_coinFlipCamera);
        DesactivarCamara(_combatCamera);

        // Activar la solicitada
        ActivarCameraInterno(camaraObjetivo);

        if (anterior != tipo)
        {
            OnCameraCambiada?.Invoke(anterior, tipo);
            Debug.Log($"[CameraManager] Cámara cambiada: {anterior} → {tipo}");
        }
    }

    /// <summary>
    /// Obtiene la cámara registrada para un tipo.
    /// </summary>
    public Camera ObtenerCamara(CameraType tipo)
    {
        switch (tipo)
        {
            case CameraType.Player: return _playerCamera;
            case CameraType.CoinFlip: return _coinFlipCamera;
            case CameraType.Combat: return _combatCamera;
            default: return null;
        }
    }

    /// <summary>
    /// Verifica si una cámara específica está registrada.
    /// </summary>
    public bool EstaRegistrada(CameraType tipo)
    {
        return ObtenerCamara(tipo) != null;
    }

    private void ActivarCameraInterno(Camera cam)
    {
        if (cam == null) return;

        cam.gameObject.SetActive(true);

        // Activar AudioListener si existe
        var listener = cam.GetComponent<AudioListener>();
        if (listener != null)
            listener.enabled = true;
    }

    private void DesactivarCamara(Camera cam)
    {
        if (cam == null) return;

        // Desactivar AudioListener primero
        var listener = cam.GetComponent<AudioListener>();
        if (listener != null)
            listener.enabled = false;

        cam.gameObject.SetActive(false);
    }
}
