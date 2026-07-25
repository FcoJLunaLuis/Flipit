/// <summary>
/// Interfaz que define el contrato para proveer datos del jugador al menú de pausa.
/// </summary>
public interface IPlayerDataProvider
{
    /// <summary>
    /// Nombre del personaje del jugador.
    /// </summary>
    string PlayerName { get; }

    /// <summary>
    /// Total de dinero representado como un entero simple (para compatibilidad).
    /// Equivale al total en Sheintavos.
    /// </summary>
    int Currency { get; }

    /// <summary>
    /// Sheintavos del jugador (denominación menor).
    /// 100 Sheintavos = 1 Pejecoin.
    /// </summary>
    int Sheintavos { get; }

    /// <summary>
    /// Pejecoins del jugador (denominación media).
    /// 100 Pejecoins = 1 Ajolopeso.
    /// </summary>
    int Pejecoins { get; }

    /// <summary>
    /// Ajolopesos del jugador (denominación mayor, acumulable al infinito).
    /// </summary>
    int Ajolopesos { get; }
}
