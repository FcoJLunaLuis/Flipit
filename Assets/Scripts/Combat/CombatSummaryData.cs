using System.Collections.Generic;

/// <summary>
/// Datos de resumen del combate para mostrar en la UI.
/// </summary>
public class CombatSummaryData
{
    public List<FichaData> FichasGanadas = new List<FichaData>();
    public List<FichaData> FichasPerdidasAlNPC = new List<FichaData>();
    public FichaData FichaLanzadora;
    public float XPGanada;
    public float DesgasteAplicado;
    public bool FichaSeRompio;
    public int LanzamientosRealizados;
    public bool JugadorGano;
}
