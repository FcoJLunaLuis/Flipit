using UnityEngine;

/// <summary>
/// Implementación temporal (mock) del proveedor de datos del jugador.
/// Retorna valores hardcodeados para testing hasta que los sistemas reales estén listos.
/// Usar WalletPlayerDataProvider para la implementación real con WalletData.
/// </summary>
public class MockPlayerDataProvider : MonoBehaviour, IPlayerDataProvider
{
    [Header("Datos de Prueba")]
    [Tooltip("Nombre del personaje (placeholder).")]
    [SerializeField] private string playerName = "Jugador";

    [Tooltip("Dinero del jugador (placeholder).")]
    [SerializeField] private int currency = 0;

    [SerializeField] private int sheintavos = 0;
    [SerializeField] private int pejecoins = 0;
    [SerializeField] private int ajolopesos = 0;

    public string PlayerName => playerName;
    public int Currency => currency;
    public int Sheintavos => sheintavos;
    public int Pejecoins => pejecoins;
    public int Ajolopesos => ajolopesos;

    public void SetPlayerName(string name)
    {
        playerName = name;
    }

    public void SetCurrency(int amount)
    {
        currency = amount;
    }
}
