using UnityEngine;
using Flipit.Core;

/// <summary>
/// Implementación real de IPlayerDataProvider que lee directamente del WalletData.
/// Reemplaza a MockPlayerDataProvider en escenas de producción.
/// Colocado en Scripts/Pause/ porque es parte del sistema de UI de pausa.
/// </summary>
public class WalletPlayerDataProvider : MonoBehaviour, IPlayerDataProvider
{
    [Header("Referencias")]
    [SerializeField] private WalletData walletData;

    [Header("Datos del Jugador")]
    [SerializeField] private string playerName = "Jugador";

    public string PlayerName => playerName;

    public int Currency => walletData != null ? (int)walletData.GetTotalInSheintavos() : 0;

    public int Sheintavos => walletData != null ? walletData.Sheintavos : 0;
    public int Pejecoins => walletData != null ? walletData.Pejecoins : 0;
    public int Ajolopesos => walletData != null ? walletData.Ajolopesos : 0;

    public void SetPlayerName(string name)
    {
        playerName = name;
    }
}
