using UnityEngine;

/// <summary>
/// Implementación temporal (mock) del proveedor de datos del jugador.
/// Retorna valores hardcodeados para testing hasta que los sistemas reales estén listos.
/// 
/// USO: Agregar este componente a un GameObject en la escena.
/// Cuando el sistema de jugador real esté listo, reemplazar este mock con la implementación real
/// que implemente IPlayerDataProvider.
/// </summary>
public class MockPlayerDataProvider : MonoBehaviour, IPlayerDataProvider
{
    [Header("Datos de Prueba")]
    [Tooltip("Nombre del personaje (placeholder).")]
    [SerializeField] private string playerName = "Jugador";

    [Tooltip("Dinero del jugador (placeholder). TODO: Conectar con sistema de dinero.")]
    [SerializeField] private int currency = 0;

    /// <summary>
    /// Nombre del personaje.
    /// </summary>
    public string PlayerName => playerName;

    /// <summary>
    /// Dinero actual del jugador.
    /// </summary>
    public int Currency => currency;

    /// <summary>
    /// Permite actualizar el nombre en runtime (para cuando el sistema real se conecte).
    /// </summary>
    /// <param name="name">Nuevo nombre del personaje.</param>
    public void SetPlayerName(string name)
    {
        playerName = name;
    }

    /// <summary>
    /// Permite actualizar el dinero en runtime (para cuando el sistema real se conecte).
    /// </summary>
    /// <param name="amount">Nueva cantidad de dinero.</param>
    public void SetCurrency(int amount)
    {
        currency = amount;
    }
}
