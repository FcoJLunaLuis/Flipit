using System;
using Newtonsoft.Json;

/// <summary>
/// Representa una instancia runtime de una ficha.
/// Contiene los datos mutables que pueden cambiar durante la partida.
/// </summary>
[Serializable]
public class FichaData
{
    public int templateId;
    public string nombre;
    public Rareza rareza;
    public int rango;
    public float experienciaDeRango;
    public bool estaRoto;
    public float desgaste;
    public string perk;
    public float peso;
    public float suerte;

    /// <summary>
    /// Crea un FichaData a partir de un FichaTemplate (copia los datos base).
    /// </summary>
    public static FichaData CrearDesdePlantilla(FichaTemplate template)
    {
        return new FichaData
        {
            templateId = template.id,
            nombre = template.nombre,
            rareza = template.rareza,
            rango = template.rango,
            experienciaDeRango = template.experienciaDeRango,
            estaRoto = template.estaRoto,
            desgaste = template.desgaste,
            perk = template.perk,
            peso = template.peso,
            suerte = template.suerte
        };
    }

    public override string ToString()
    {
        return $"[{templateId}] {nombre} | Rareza: {rareza} | Rango: {rango} | Perk: {perk}";
    }
}
