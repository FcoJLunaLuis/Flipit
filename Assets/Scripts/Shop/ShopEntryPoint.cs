using UnityEngine;

namespace Flipit.Shop
{
    public class ShopEntryPoint : MonoBehaviour
    {
        [SerializeField] private ShopSceneLoader shopSceneLoader;

        private void OnMouseDown()
        {
            if (shopSceneLoader != null && !shopSceneLoader.IsShopLoaded)
            {
                shopSceneLoader.LoadShop();
            }
        }

        private void OnGUI()
        {
            if (shopSceneLoader == null) return;

            if (!shopSceneLoader.IsShopLoaded)
            {
                Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);
                float labelX = screenPos.x - 75f;
                float labelY = Screen.height - screenPos.y - 50f;

                GUI.Label(new Rect(labelX, labelY, 150f, 25f), "Click para entrar");
            }
        }
    }
}