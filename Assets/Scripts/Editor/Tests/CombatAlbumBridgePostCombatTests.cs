using System.Collections.Generic;
using NUnit.Framework;

[TestFixture]
public class CombatAlbumBridgePostCombatTests
{
    [Test]
    public void AgregarFichasGanadas_AgregaAlAlbum()
    {
        var albumData = new AlbumData();
        var fichasGanadas = CombatAlbumBridgeTestHelper.CrearListaFichas(10, 11, 12);

        foreach (var ficha in fichasGanadas)
        {
            albumData.AgregarFicha(ficha);
        }

        Assert.AreEqual(1, albumData.ObtenerCantidad(10));
        Assert.AreEqual(1, albumData.ObtenerCantidad(11));
        Assert.AreEqual(1, albumData.ObtenerCantidad(12));
    }

    [Test]
    public void RestaurarLanzadora_AgregaAlAlbum()
    {
        var albumData = new AlbumData();
        var lanzadora = CombatAlbumBridgeTestHelper.CrearFicha(7, "Lanzadora");

        albumData.AgregarFicha(lanzadora);

        Assert.AreEqual(1, albumData.ObtenerCantidad(7));
    }

    [Test]
    public void FichasPerdidas_PermanecenRemovidasDelAlbum()
    {
        // Setup: album con 5 fichas, jugador apuesta 1,2,3
        var albumData = CombatAlbumBridgeTestHelper.CrearAlbumConNFichas(5);

        // Simular remoción temporal (ConfirmarApuesta)
        albumData.RemoverFicha(1);
        albumData.RemoverFicha(2);
        albumData.RemoverFicha(3);

        // NPC gana ficha 1 y 2 del jugador → deben quedarse removidas
        // Jugador gana ficha 3 → se agrega de nuevo
        var fichaGanada = CombatAlbumBridgeTestHelper.CrearFicha(3);
        albumData.AgregarFicha(fichaGanada);

        // Verificar
        Assert.AreEqual(0, albumData.ObtenerCantidad(1)); // perdida
        Assert.AreEqual(0, albumData.ObtenerCantidad(2)); // perdida
        Assert.AreEqual(1, albumData.ObtenerCantidad(3)); // recuperada
        Assert.AreEqual(1, albumData.ObtenerCantidad(4)); // no participó
        Assert.AreEqual(1, albumData.ObtenerCantidad(5)); // no participó
    }

    [Test]
    public void DerrotaTotal_TodasFichasQuedanRemovidas()
    {
        var albumData = CombatAlbumBridgeTestHelper.CrearAlbumConNFichas(3);

        // Remove all 3 (bet confirmation)
        albumData.RemoverFicha(1);
        albumData.RemoverFicha(2);
        albumData.RemoverFicha(3);

        // NPC wins all — no restore
        Assert.AreEqual(0, albumData.ObtenerCantidad(1));
        Assert.AreEqual(0, albumData.ObtenerCantidad(2));
        Assert.AreEqual(0, albumData.ObtenerCantidad(3));
        Assert.AreEqual(0, albumData.ObtenerTotalFichas());
    }

    [Test]
    public void VictoriaTotal_TodasFichasSeAgreganAlAlbum()
    {
        var albumData = CombatAlbumBridgeTestHelper.CrearAlbumConNFichas(3);

        // Remove fichas 1,2,3 during bet
        albumData.RemoverFicha(1);
        albumData.RemoverFicha(2);
        albumData.RemoverFicha(3);

        // Player wins all back + NPC fichas
        var fichasGanadas = CombatAlbumBridgeTestHelper.CrearListaFichas(1, 2, 3, 10, 11);
        foreach (var f in fichasGanadas)
        {
            albumData.AgregarFicha(f);
        }

        Assert.AreEqual(1, albumData.ObtenerCantidad(1));
        Assert.AreEqual(1, albumData.ObtenerCantidad(2));
        Assert.AreEqual(1, albumData.ObtenerCantidad(3));
        Assert.AreEqual(1, albumData.ObtenerCantidad(10));
        Assert.AreEqual(1, albumData.ObtenerCantidad(11));
    }

    [Test]
    public void IdentificarFichasPerdidas_FiltraCorrectamente()
    {
        var apostadasJugador = CombatAlbumBridgeTestHelper.CrearListaFichas(1, 2, 3);
        var ganadasNPC = CombatAlbumBridgeTestHelper.CrearListaFichas(1, 3, 10); // 10 es del NPC

        var apostadasIds = new HashSet<int>();
        foreach (var f in apostadasJugador) apostadasIds.Add(f.templateId);

        var fichasPerdidas = new List<FichaData>();
        foreach (var f in ganadasNPC)
        {
            if (apostadasIds.Contains(f.templateId))
            {
                fichasPerdidas.Add(f);
            }
        }

        Assert.AreEqual(2, fichasPerdidas.Count); // solo 1 y 3, no 10
        Assert.IsTrue(fichasPerdidas.Exists(f => f.templateId == 1));
        Assert.IsTrue(fichasPerdidas.Exists(f => f.templateId == 3));
    }
}
