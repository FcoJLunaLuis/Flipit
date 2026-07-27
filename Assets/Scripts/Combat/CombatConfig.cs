using UnityEngine;

/// <summary>
/// Configuración del sistema de combate.
/// Ajusta parámetros de balance: apuestas, minijuego, desgaste y experiencia.
/// </summary>
[CreateAssetMenu(fileName = "CombatConfig", menuName = "Flipit/Combat/Combat Config")]
public class CombatConfig : ScriptableObject
{
    [Header("Apuestas")]
    [Tooltip("Máximo de fichas que se pueden apostar por combate")]
    [Range(1, 10)]
    public int maxFichasApuesta = 5;

    [Tooltip("Mínimo de fichas que se deben apostar por combate")]
    [Range(1, 5)]
    public int minFichasApuesta = 1;

    [Header("Minijuego - Mira")]
    [Tooltip("Velocidad de movimiento de la mira sobre la torre")]
    public float velocidadMira = 3f;

    [Header("Minijuego - Fuerza")]
    [Tooltip("Velocidad de oscilación de la barra de fuerza")]
    public float velocidadBarraFuerza = 2.5f;

    [Header("Minijuego - Precisión")]
    [Tooltip("Velocidad de expansión/contracción del círculo de precisión")]
    public float velocidadCirculoPrecision = 3f;

    [Tooltip("Radio mínimo del círculo (precisión perfecta)")]
    public float radioMinimoPrecision = 0.05f;

    [Tooltip("Radio máximo del círculo (máxima dispersión)")]
    public float radioMaximoPrecision = 1f;

    [Header("NPC - Rangos de precisión semi-aleatoria")]
    [Tooltip("Rango mínimo de fuerza del NPC (0-1)")]
    [Range(0f, 1f)]
    public float npcFuerzaMin = 0.4f;

    [Tooltip("Rango máximo de fuerza del NPC (0-1)")]
    [Range(0f, 1f)]
    public float npcFuerzaMax = 0.9f;

    [Tooltip("Rango mínimo de dispersión del NPC (0-1, menor = más preciso)")]
    [Range(0f, 1f)]
    public float npcDispersionMin = 0.2f;

    [Tooltip("Rango máximo de dispersión del NPC (0-1)")]
    [Range(0f, 1f)]
    public float npcDispersionMax = 0.7f;

    [Header("Desgaste y Experiencia")]
    [Tooltip("Desgaste aplicado a la ficha lanzadora por cada lanzamiento realizado")]
    public float desgastePorLanzamiento = 5f;

    [Tooltip("Experiencia ganada por cada ficha obtenida en el combate")]
    public float xpPorFichaGanada = 10f;

    [Tooltip("Desgaste máximo antes de que la ficha se rompa")]
    public float desgasteMaximo = 100f;
}
