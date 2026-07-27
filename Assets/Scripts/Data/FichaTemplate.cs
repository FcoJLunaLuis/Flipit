using UnityEngine;

public enum Rareza
{
    Comun,
    Raro,
    UltraRaro
}

[CreateAssetMenu(fileName = "NuevaFicha", menuName = "Flipit/Ficha Template")]
public class FichaTemplate : ScriptableObject
{
    [Header("Identificación")]
    public int id;
    public string nombre;
    public Rareza rareza;

    [Header("Progresión")]
    public int rango;
    public float experienciaDeRango;

    [Header("Estado")]
    public bool estaRoto;
    public float desgaste; // 0 = nueva, 100 = completamente desgastada

    [Header("Atributos")]
    public string perk; // Descripción del perk, se expandirá luego
    public float peso;
    public float suerte;

    [Header("Visual")]
    public Sprite icono; // Placeholder por ahora
}
