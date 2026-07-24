using UnityEngine;

namespace Flipit.Shop
{
    public class ShopInitializer : MonoBehaviour
    {
        [SerializeField] private ShopManager shopManager;
        [SerializeField] private ShopUIController uiController;

        private void Start()
        {
            if (shopManager != null)
            {
                shopManager.InitializeShop();
            }

            if (uiController != null)
            {
                uiController.OnExitShop += HandleExitShop;
            }
        }

        private void OnDestroy()
        {
            if (uiController != null)
            {
                uiController.OnExitShop -= HandleExitShop;
            }
        }

        private void HandleExitShop()
        {
            // Find the ShopSceneLoader in the other scene and tell it to unload us
            var loader = FindAnyObjectByType<ShopSceneLoader>();
            if (loader != null)
            {
                loader.UnloadShop();
            }
            else
            {
                // Fallback: if no loader found (testing directly), just log
                Debug.Log("[ShopInitializer] Exit requested but no ShopSceneLoader found.");
            }
        }
    }
}