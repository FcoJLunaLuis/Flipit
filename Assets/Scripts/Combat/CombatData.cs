using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Estado completo de un combate en curso.
/// Gestiona las fichas apostadas, la torre, turnos y resultados.
/// </summary>
[Serializable]
public class CombatData
{
    public enum CombatPhase
    {
        BetSelection,
        CoinFlip,
        ThrowTurn,
        ResolveThrow,
        CheckEnd,
        Summary
    }

    public enum TurnOwner { Jugador, NPC }

    public List<FichaData> FichasApostadasJugador { get; private set; }
    public List<FichaData> FichasApostadasNPC { get; private set; }
    public FichaData FichaLanzadoraJugador { get; private set; }
    public FichaData FichaLanzadoraNPC { get; private set; }

    public List<TowerSlot> Torre { get; private set; }
    public List<ThrowResult> HistorialLanzamientos { get; private set; }

    public CombatPhase FaseActual { get; private set; }
    public TurnOwner TurnoActual { get; private set; }
    public int NumeroDeRonda { get; private set; }

    public List<FichaData> FichasGanadasJugador { get; private set; }
    public List<FichaData> FichasGanadasNPC { get; private set; }

    public int LanzamientosJugador { get; private set; }
    public int LanzamientosNPC { get; private set; }

    public CombatData()
    {
        FichasApostadasJugador = new List<FichaData>();
        FichasApostadasNPC = new List<FichaData>();
        Torre = new List<TowerSlot>();
        HistorialLanzamientos = new List<ThrowResult>();
        FichasGanadasJugador = new List<FichaData>();
        FichasGanadasNPC = new List<FichaData>();
        FaseActual = CombatPhase.BetSelection;
        NumeroDeRonda = 0;
        LanzamientosJugador = 0;
        LanzamientosNPC = 0;
    }

    public void SetFichasApostadas(List<FichaData> jugador, List<FichaData> npc)
    {
        FichasApostadasJugador = jugador ?? new List<FichaData>();
        FichasApostadasNPC = npc ?? new List<FichaData>();
    }

    public void SetFichaLanzadora(FichaData fichaJugador, FichaData fichaNPC)
    {
        FichaLanzadoraJugador = fichaJugador;
        FichaLanzadoraNPC = fichaNPC;
    }

    public void SetTorre(List<TowerSlot> torre)
    {
        Torre = torre ?? new List<TowerSlot>();
    }

    public void SetTurnoInicial(TurnOwner turno)
    {
        TurnoActual = turno;
    }

    public void AvanzarFase(CombatPhase nuevaFase)
    {
        FaseActual = nuevaFase;
    }

    public void CambiarTurno()
    {
        TurnoActual = TurnoActual == TurnOwner.Jugador ? TurnOwner.NPC : TurnOwner.Jugador;
        NumeroDeRonda++;
    }

    public void RegistrarLanzamiento(ThrowResult resultado)
    {
        HistorialLanzamientos.Add(resultado);

        if (TurnoActual == TurnOwner.Jugador)
        {
            LanzamientosJugador++;
            foreach (var slot in resultado.FichasVolteadas)
            {
                FichasGanadasJugador.Add(slot.Ficha);
            }
        }
        else
        {
            LanzamientosNPC++;
            foreach (var slot in resultado.FichasVolteadas)
            {
                FichasGanadasNPC.Add(slot.Ficha);
            }
        }
    }

    public bool TodasLasFichasVolteadas()
    {
        if (Torre.Count == 0) return true;
        return Torre.All(slot => slot.EstaVolteada);
    }

    public List<TowerSlot> ObtenerFichasSinVoltear()
    {
        return Torre.Where(slot => !slot.EstaVolteada).ToList();
    }

    public int TotalFichasEnTorre => Torre.Count;
    public int FichasRestantes => Torre.Count(slot => !slot.EstaVolteada);

    public override string ToString()
    {
        return $"Combate [Fase:{FaseActual}] Turno:{TurnoActual} Ronda:{NumeroDeRonda} " +
               $"Torre:{FichasRestantes}/{TotalFichasEnTorre} " +
               $"Ganadas J:{FichasGanadasJugador.Count} NPC:{FichasGanadasNPC.Count}";
    }
}
