using UnityEngine;

/// <summary>
/// Script de prueba para verificar que FichaData se instancia correctamente desde FichaTemplate.
/// Asigna los templates en el inspector y entra en Play Mode para ver los resultados en consola.
/// </summary>
public class FichaDataTest : MonoBehaviour
{
    [Header("Arrastra los FichaTemplates aquí para probar")]
    public FichaTemplate[] fichasTemplates;

    void Start()
    {
        if (fichasTemplates == null || fichasTemplates.Length == 0)
        {
            Debug.LogWarning("[FichaDataTest] No hay templates asignados. Arrastra FichaTemplates al inspector.");
            return;
        }

        Debug.Log($"[FichaDataTest] === Iniciando prueba con {fichasTemplates.Length} templates ===");

        foreach (var template in fichasTemplates)
        {
            if (template == null)
            {
                Debug.LogWarning("[FichaDataTest] Un template es null, se omite.");
                continue;
            }

            FichaData fichaData = FichaData.CrearDesdePlantilla(template);

            // Verificar que los datos se copiaron correctamente
            bool datosCorrectos = fichaData.templateId == template.id
                && fichaData.nombre == template.nombre
                && fichaData.rareza == template.rareza
                && fichaData.rango == template.rango
                && Mathf.Approximately(fichaData.experienciaDeRango, template.experienciaDeRango)
                && fichaData.estaRoto == template.estaRoto
                && Mathf.Approximately(fichaData.desgaste, template.desgaste)
                && fichaData.perk == template.perk
                && Mathf.Approximately(fichaData.peso, template.peso)
                && Mathf.Approximately(fichaData.suerte, template.suerte);

            if (datosCorrectos)
            {
                Debug.Log($"[FichaDataTest] OK: {fichaData}");
            }
            else
            {
                Debug.LogError($"[FichaDataTest] ERROR: Los datos no coinciden para template '{template.nombre}'");
            }
        }

        Debug.Log("[FichaDataTest] === Prueba finalizada ===");
    }
}
