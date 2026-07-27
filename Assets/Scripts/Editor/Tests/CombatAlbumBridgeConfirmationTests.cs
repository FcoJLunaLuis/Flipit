using System.Collections.Generic;
using NUnit.Framework;

[TestFixture]
public class CombatAlbumBridgeConfirmationTests
{
    [Test]
    public void RemoverFichas_ExitosoConFichasValidas_ReduceCantidad()
    {
        var albumData = CombatAlbumBridgeTestHelper.CrearAlbumConNFichas(5);
        var inventory = new AlbumChipInventory(albumData, null);

        bool result = inventory.RemoveChips("1", 1);

        Assert.IsTrue(result);
        Assert.AreEqual(0, albumData.ObtenerCantidad(1));
    }

    [Test]
    public void RemoverFichas_FichaInexistente_RetornaFalse()
    {
        var albumData = CombatAlbumBridgeTestHelper.CrearAlbumConNFichas(3);
        var inventory = new AlbumChipInventory(albumData, null);

        bool result = inventory.RemoveChips("99", 1);

        Assert.IsFalse(result);
    }

    [Test]
    public void RemoverFichas_CantidadInsuficiente_RetornaFalse()
    {
        var albumData = CombatAlbumBridgeTestHelper.CrearAlbumConNFichas(3);
        var inventory = new AlbumChipInventory(albumData, null);

        bool result = inventory.RemoveChips("1", 5);

        Assert.IsFalse(result);
        // Cantidad no cambió
        Assert.AreEqual(1, albumData.ObtenerCantidad(1));
    }

    [Test]
    public void Rollback_RestaurarFichasRemovidas_RestauraCantidad()
    {
        var albumData = CombatAlbumBridgeTestHelper.CrearAlbumConNFichas(5);
        var inventory = new AlbumChipInventory(albumData, null);

        // Remove ficha 1
        inventory.RemoveChips("1", 1);
        Assert.AreEqual(0, albumData.ObtenerCantidad(1));

        // Rollback: add back
        inventory.AddChips("1", 1);
        Assert.AreEqual(1, albumData.ObtenerCantidad(1));
    }

    [Test]
    public void Rollback_Transaccional_MultiplesFichas_RestauraTodas()
    {
        var albumData = CombatAlbumBridgeTestHelper.CrearAlbumConNFichas(5);
        var inventory = new AlbumChipInventory(albumData, null);

        var fichasRemovidas = new List<(string chipId, int cantidad)>();

        // Remove 3 fichas successfully
        inventory.RemoveChips("1", 1); fichasRemovidas.Add(("1", 1));
        inventory.RemoveChips("2", 1); fichasRemovidas.Add(("2", 1));
        inventory.RemoveChips("3", 1); fichasRemovidas.Add(("3", 1));

        // Simulate failure on 4th — do rollback
        bool failed = !inventory.RemoveChips("99", 1); // This will fail
        Assert.IsTrue(failed);

        // Rollback all
        foreach (var (chipId, cantidad) in fichasRemovidas)
        {
            inventory.AddChips(chipId, cantidad);
        }

        // Verify all restored
        Assert.AreEqual(1, albumData.ObtenerCantidad(1));
        Assert.AreEqual(1, albumData.ObtenerCantidad(2));
        Assert.AreEqual(1, albumData.ObtenerCantidad(3));
    }

    [Test]
    public void ConfirmarApuesta_SimulacionCompleta_RemueveTodasLasFichas()
    {
        var albumData = CombatAlbumBridgeTestHelper.CrearAlbumConNFichas(5);
        var inventory = new AlbumChipInventory(albumData, null);

        var fichasApostadas = CombatAlbumBridgeTestHelper.CrearListaFichas(1, 2, 3);
        var lanzadora = CombatAlbumBridgeTestHelper.CrearFicha(4);

        // Simulate ConfirmarApuesta logic
        foreach (var ficha in fichasApostadas)
        {
            bool ok = inventory.RemoveChips(ficha.templateId.ToString(), 1);
            Assert.IsTrue(ok);
        }
        bool lanzadoraOk = inventory.RemoveChips(lanzadora.templateId.ToString(), 1);
        Assert.IsTrue(lanzadoraOk);

        // Verify all removed
        Assert.AreEqual(0, albumData.ObtenerCantidad(1));
        Assert.AreEqual(0, albumData.ObtenerCantidad(2));
        Assert.AreEqual(0, albumData.ObtenerCantidad(3));
        Assert.AreEqual(0, albumData.ObtenerCantidad(4));
        // Ficha 5 untouched
        Assert.AreEqual(1, albumData.ObtenerCantidad(5));
    }
}
