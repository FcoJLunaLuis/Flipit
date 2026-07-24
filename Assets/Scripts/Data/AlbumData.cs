using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Representa una entrada en el álbum: una ficha con su cantidad.
/// </summary>
[Serializable]
public class AlbumEntry
{
    public FichaData ficha;
    public int cantidad;

    public AlbumEntry(FichaData ficha, int cantidad = 1)
    {
        this.ficha = ficha;
        this.cantidad = cantidad;
    }
}

/// <summary>
/// Gestiona la colección de fichas del álbum.
/// Agrupa fichas repetidas por templateId con un contador.
/// Soporta paginación para la UI.
/// </summary>
public class AlbumData
{
    private Dictionary<int, AlbumEntry> fichas = new Dictionary<int, AlbumEntry>();

    public const int FICHAS_POR_PAGINA = 10;

    /// <summary>
    /// Agrega una ficha al álbum. Si ya existe, incrementa el contador.
    /// </summary>
    public void AgregarFicha(FichaData ficha)
    {
        if (ficha == null) return;

        if (fichas.ContainsKey(ficha.templateId))
        {
            fichas[ficha.templateId].cantidad++;
        }
        else
        {
            fichas[ficha.templateId] = new AlbumEntry(ficha);
        }
    }

    /// <summary>
    /// Agrega una ficha a partir de un template.
    /// </summary>
    public void AgregarFicha(FichaTemplate template)
    {
        if (template == null) return;
        FichaData fichaData = FichaData.CrearDesdePlantilla(template);
        AgregarFicha(fichaData);
    }

    /// <summary>
    /// Remueve una ficha del álbum. Si tiene más de 1, decrementa el contador.
    /// Retorna true si se removió exitosamente.
    /// </summary>
    public bool RemoverFicha(int templateId)
    {
        if (!fichas.ContainsKey(templateId)) return false;

        fichas[templateId].cantidad--;

        if (fichas[templateId].cantidad <= 0)
        {
            fichas.Remove(templateId);
        }

        return true;
    }

    /// <summary>
    /// Obtiene la cantidad de una ficha específica en el álbum.
    /// </summary>
    public int ObtenerCantidad(int templateId)
    {
        if (fichas.ContainsKey(templateId))
        {
            return fichas[templateId].cantidad;
        }
        return 0;
    }

    /// <summary>
    /// Obtiene el total de fichas únicas en el álbum.
    /// </summary>
    public int ObtenerTotalFichasUnicas()
    {
        return fichas.Count;
    }

    /// <summary>
    /// Obtiene el total de fichas contando repetidas.
    /// </summary>
    public int ObtenerTotalFichas()
    {
        int total = 0;
        foreach (var entry in fichas.Values)
        {
            total += entry.cantidad;
        }
        return total;
    }

    /// <summary>
    /// Obtiene el número total de páginas según las fichas únicas.
    /// </summary>
    public int ObtenerTotalPaginas()
    {
        if (fichas.Count == 0) return 1;
        return (int)Math.Ceiling((double)fichas.Count / FICHAS_POR_PAGINA);
    }

    /// <summary>
    /// Obtiene las fichas de una página específica (0-indexed).
    /// Retorna una lista de AlbumEntry para esa página.
    /// </summary>
    public List<AlbumEntry> ObtenerFichasPaginadas(int pagina)
    {
        if (pagina < 0) pagina = 0;

        int totalPaginas = ObtenerTotalPaginas();
        if (pagina >= totalPaginas) pagina = totalPaginas - 1;

        return fichas.Values
            .OrderBy(entry => entry.ficha.templateId)
            .Skip(pagina * FICHAS_POR_PAGINA)
            .Take(FICHAS_POR_PAGINA)
            .ToList();
    }

    /// <summary>
    /// Obtiene todas las entradas del álbum (sin paginación).
    /// </summary>
    public List<AlbumEntry> ObtenerTodasLasFichas()
    {
        return fichas.Values
            .OrderBy(entry => entry.ficha.templateId)
            .ToList();
    }

    /// <summary>
    /// Obtiene una entrada específica por templateId.
    /// Retorna null si no existe.
    /// </summary>
    public AlbumEntry ObtenerEntrada(int templateId)
    {
        if (fichas.ContainsKey(templateId))
        {
            return fichas[templateId];
        }
        return null;
    }

    /// <summary>
    /// Verifica si el álbum contiene una ficha con el templateId dado.
    /// </summary>
    public bool ContieneFicha(int templateId)
    {
        return fichas.ContainsKey(templateId);
    }

    /// <summary>
    /// Limpia todas las fichas del álbum.
    /// </summary>
    public void Limpiar()
    {
        fichas.Clear();
    }
}
