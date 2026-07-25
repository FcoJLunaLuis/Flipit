using UnityEngine;

public class TestCoinFlipTrigger : MonoBehaviour
{
    private void OnMouseDown()
    {
        if (CoinFlipManager.Instance == null)
        {
            Debug.LogError("[TEST] CoinFlipManager.Instance es null. Asegúrate de que existe en la escena.");
            return;
        }

        Debug.Log("[TEST] Click detectado. Iniciando flip...");
        CoinFlipManager.Instance.IniciarFlip(OnFlipCompletado);
    }

    private void OnFlipCompletado(bool jugadorGano)
    {
        Debug.Log($"[TEST] Flip terminado. Jugador {(jugadorGano ? "GANÓ" : "PERDIÓ")}. Listo para combate.");
    }
}
