using UnityEngine;

/// <summary>
/// Conecta el BetSelectionUI al flujo del CombatManager.
/// Cuando la fase cambia a BetSelection, muestra el panel con datos del AlbumManager.
/// Cuando cambia a cualquier otra fase, lo oculta.
/// 
/// Este script vive en Assembly-CSharp (carpeta Integration) para poder acceder
/// tanto a Flipit.Combat (CombatManager, BetSelectionUI) como a AlbumManager.
/// </summary>
public class BetSelectionBridge : MonoBehaviour
{
    [SerializeField] private BetSelectionUI _betSelectionUI;

    private void Start()
    {
        if (_betSelectionUI == null)
            _betSelectionUI = GetComponentInChildren<BetSelectionUI>(true);

        if (CombatManager.Instance != null)
        {
            CombatManager.Instance.OnFaseCambiada += OnFaseCambiada;
            CombatManager.Instance.OnCombateIniciado += OnCombateIniciado;
        }
    }

    private void OnDestroy()
    {
        if (CombatManager.Instance != null)
        {
            CombatManager.Instance.OnFaseCambiada -= OnFaseCambiada;
            CombatManager.Instance.OnCombateIniciado -= OnCombateIniciado;
        }
    }

private void OnCombateIniciado()
    {
        // Activar cámara de combate (top-down) para la fase de selección
        if (CameraManager.Instance != null)
        {
            CameraManager.Instance.ActivarCamara(CameraManager.CameraType.Combat);
        }

        MostrarSeleccion();
    }

    private void OnFaseCambiada(CombatData.CombatPhase fase)
    {
        if (fase == CombatData.CombatPhase.BetSelection)
        {
            MostrarSeleccion();
        }
        else
        {
            OcultarSeleccion();
        }
    }

    private void MostrarSeleccion()
    {
        if (_betSelectionUI == null) return;

        AlbumData album = null;
        if (AlbumManager.Instance != null)
        {
            album = AlbumManager.Instance.ObtenerAlbumData();
        }

        if (album == null)
        {
            Debug.LogError("[BetSelectionBridge] No se pudo obtener AlbumData.");
            return;
        }

        _betSelectionUI.Mostrar(CombatManager.Instance, album);
    }

    private void OcultarSeleccion()
    {
        if (_betSelectionUI == null) return;
        _betSelectionUI.Ocultar();
    }
}
