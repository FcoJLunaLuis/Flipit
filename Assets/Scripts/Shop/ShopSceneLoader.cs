using UnityEngine;
using UnityEngine.SceneManagement;

namespace Flipit.Shop
{
    public class ShopSceneLoader : MonoBehaviour
    {
        [SerializeField] private string shopSceneName = "Shop";

        private bool isShopLoaded;

        public bool IsShopLoaded => isShopLoaded;

        public void LoadShop()
        {
            if (isShopLoaded) return;

            isShopLoaded = true;
            SceneManager.LoadScene(shopSceneName, LoadSceneMode.Additive);
        }

        public void UnloadShop()
        {
            if (!isShopLoaded) return;

            isShopLoaded = false;
            SceneManager.UnloadSceneAsync(shopSceneName);
        }
    }
}