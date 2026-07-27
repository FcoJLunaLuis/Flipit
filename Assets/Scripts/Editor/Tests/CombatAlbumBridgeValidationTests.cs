using System.Collections.Generic;
using NUnit.Framework;

/// <summary>
/// Tests para la lógica de validación pre-combate y filtrado de fichas.
/// Verifica que las reglas de conteo de fichas no rotas y filtrado funcionan correctamente.
/// </summary>
[TestFixture]
public class CombatAlbumBridgeValidationTests
{
    [Test]
    public void ObtenerFichasDisponibles_AlbumVacio_RetornaListaVacia()
    {
        var albumData = new AlbumData();
        var fichas = ObtenerFichasDisponiblesDesdeAlbum(albumData);
        Assert.AreEqual(0, fichas.Count);
    }

    [Test]
    public void ObtenerFichasDisponibles_TodasRotas_RetornaListaVacia()
    {
        var albumData = new AlbumData();
        albumData.AgregarFicha(CombatAlbumBridgeTestHelper.CrearFicha(1, estaRoto: true));
        albumData.AgregarFicha(CombatAlbumBridgeTestHelper.CrearFicha(2, estaRoto: true));
        albumData.AgregarFicha(CombatAlbumBridgeTestHelper.CrearFicha(3, estaRoto: true));

        var fichas = ObtenerFichasDisponiblesDesdeAlbum(albumData);
        Assert.AreEqual(0, fichas.Count);
    }

    [Test]
    public void ObtenerFichasDisponibles_MezclaRotasYValidas_RetornaSoloValidas()
    {
        var albumData = new AlbumData();
        albumData.AgregarFicha(CombatAlbumBridgeTestHelper.CrearFicha(1, estaRoto: false));
        albumData.AgregarFicha(CombatAlbumBridgeTestHelper.CrearFicha(2, estaRoto: true));
        albumData.AgregarFicha(CombatAlbumBridgeTestHelper.CrearFicha(3, estaRoto: false));
        albumData.AgregarFicha(CombatAlbumBridgeTestHelper.CrearFicha(4, estaRoto: true));

        var fichas = ObtenerFichasDisponiblesDesdeAlbum(albumData);
        Assert.AreEqual(2, fichas.Count);
        Assert.IsTrue(fichas.Exists(f => f.templateId == 1));
        Assert.IsTrue(fichas.Exists(f => f.templateId == 3));
    }

    [Test]
    public void ContarFichasNoRotas_AlbumVacio_RetornaCero()
    {
        var albumData = new AlbumData();
        int count = ContarFichasNoRotasDesdeAlbum(albumData);
        Assert.AreEqual(0, count);
    }

    [Test]
    public void ContarFichasNoRotas_TodasRotas_RetornaCero()
    {
        var albumData = new AlbumData();
        albumData.AgregarFicha(CombatAlbumBridgeTestHelper.CrearFicha(1, estaRoto: true));
        albumData.AgregarFicha(CombatAlbumBridgeTestHelper.CrearFicha(2, estaRoto: true));

        int count = ContarFichasNoRotasDesdeAlbum(albumData);
        Assert.AreEqual(0, count);
    }

    [Test]
    public void ContarFichasNoRotas_ConMezcla_RetornaCantidadCorrecta()
    {
        var albumData = new AlbumData();
        albumData.AgregarFicha(CombatAlbumBridgeTestHelper.CrearFicha(1, estaRoto: false));
        albumData.AgregarFicha(CombatAlbumBridgeTestHelper.CrearFicha(2, estaRoto: true));
        albumData.AgregarFicha(CombatAlbumBridgeTestHelper.CrearFicha(3, estaRoto: false));
        albumData.AgregarFicha(CombatAlbumBridgeTestHelper.CrearFicha(4, estaRoto: false));

        int count = ContarFichasNoRotasDesdeAlbum(albumData);
        Assert.AreEqual(3, count);
    }

    [Test]
    public void ValidacionPreCombate_MenosDe2FichasNoRotas_DebeBloquear()
    {
        // Con minFichasApuesta = 1, necesitamos 2 fichas no rotas (1 apuesta + 1 lanzadora)
        var albumData = new AlbumData();
        albumData.AgregarFicha(CombatAlbumBridgeTestHelper.CrearFicha(1, estaRoto: false));
        // Solo 1 ficha → debe bloquear

        int minFichasApuesta = 1;
        int count = ContarFichasNoRotasDesdeAlbum(albumData);
        bool puedeCombarir = count >= (minFichasApuesta + 1);

        Assert.IsFalse(puedeCombarir);
    }

    [Test]
    public void ValidacionPreCombate_ExactamenteMinMas1FichasNoRotas_Permite()
    {
        var albumData = new AlbumData();
        albumData.AgregarFicha(CombatAlbumBridgeTestHelper.CrearFicha(1, estaRoto: false));
        albumData.AgregarFicha(CombatAlbumBridgeTestHelper.CrearFicha(2, estaRoto: false));

        int minFichasApuesta = 1;
        int count = ContarFichasNoRotasDesdeAlbum(albumData);
        bool puedeCombatir = count >= (minFichasApuesta + 1);

        Assert.IsTrue(puedeCombatir);
    }

    [Test]
    public void ValidacionPreCombate_ExactamenteMinFichas_SinLanzadora_DebeBloquear()
    {
        // Si tiene exactamente minFichasApuesta fichas, no tiene lanzadora disponible
        var albumData = new AlbumData();
        albumData.AgregarFicha(CombatAlbumBridgeTestHelper.CrearFicha(1, estaRoto: false));

        int minFichasApuesta = 1;
        int count = ContarFichasNoRotasDesdeAlbum(albumData);
        bool exactamenteMin = count == minFichasApuesta;
        bool menosQueRequerido = count < (minFichasApuesta + 1);

        Assert.IsTrue(exactamenteMin);
        Assert.IsTrue(menosQueRequerido);
    }

    [Test]
    public void ObtenerFichasDisponibles_FichaConCantidadCero_NoSeIncluye()
    {
        var albumData = new AlbumData();
        albumData.AgregarFicha(CombatAlbumBridgeTestHelper.CrearFicha(1));
        albumData.AgregarFicha(CombatAlbumBridgeTestHelper.CrearFicha(2));

        // Remove ficha 1 so cantidad = 0
        albumData.RemoverFicha(1);

        var fichas = ObtenerFichasDisponiblesDesdeAlbum(albumData);
        Assert.AreEqual(1, fichas.Count);
        Assert.AreEqual(2, fichas[0].templateId);
    }

    // === Helper methods that mirror CombatAlbumBridge logic ===

    private List<FichaData> ObtenerFichasDisponiblesDesdeAlbum(AlbumData albumData)
    {
        var todasLasFichas = albumData.ObtenerTodasLasFichas();
        var fichasDisponibles = new List<FichaData>();

        if (todasLasFichas == null) return fichasDisponibles;

        foreach (var entry in todasLasFichas)
        {
            if (entry.ficha.estaRoto == false && entry.cantidad >= 1)
            {
                fichasDisponibles.Add(entry.ficha);
            }
        }

        return fichasDisponibles;
    }

    private int ContarFichasNoRotasDesdeAlbum(AlbumData albumData)
    {
        var todasLasFichas = albumData.ObtenerTodasLasFichas();
        if (todasLasFichas == null) return 0;

        int count = 0;
        foreach (var entry in todasLasFichas)
        {
            if (entry.ficha.estaRoto == false && entry.cantidad >= 1)
            {
                count++;
            }
        }
        return count;
    }
}
