/// <summary>
/// Interfaz que define el contrato para proveer datos del jugador al menú de pausa.
/// Cuando el sistema de jugador real esté implementado, debe implementar esta interfaz.
/// </summary>
public interface IPlayerDataProvider
{
    /// <summary>
    /// Nombre del personaje del jugador.
    /// </summary>
    string PlayerName { get; }

    /// <summary>
    /// Cantidad de dinero que posee el jugador.
    /// TODO: Conectar con el sistema de dinero cuando esté desarrollado.
    /// </summary>
    int Currency { get; }
}
