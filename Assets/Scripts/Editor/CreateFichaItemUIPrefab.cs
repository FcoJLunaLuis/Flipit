using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;

/// <summary>
/// Editor utility to create the FichaItemUI prefab programmatically.
/// Run from menu: Tools/Combat/Create FichaItemUI Prefab
/// </summary>
public static class CreateFichaItemUIPrefab
{
    [MenuItem("Tools/Combat/Create FichaItemUI Prefab")]
    public static void Create()
    {
        // Root GameObject
        var root = new GameObject("FichaItemUI");
        var rootRect = root.AddComponent<RectTransform>();
        rootRect.sizeDelta = new Vector2(350f, 40f);

        // LayoutElement for container sizing
        var layoutElement = root.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = 40f;
        layoutElement.flexibleWidth = 1f;

        // Background Image
        var bgImage = root.AddComponent<Image>();
        bgImage.color = new Color(0.5f, 0.5f, 0.5f, 0.8f);
        bgImage.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
        bgImage.type = Image.Type.Sliced;

        // Text child
        var textObj = new GameObject("NombreText");
        var textRect = textObj.AddComponent<RectTransform>();
        textRect.SetParent(rootRect, false);
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(10f, 0f);
        textRect.offsetMax = new Vector2(-10f, 0f);

        var tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = "NombreFicha (Rareza)";
        tmp.fontSize = 18f;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.MidlineLeft;
        tmp.enableWordWrapping = false;
        tmp.overflowMode = TextOverflowModes.Ellipsis;

        // Add FichaItemUI component and wire references
        var fichaItem = root.AddComponent<FichaItemUI>();
        var so = new SerializedObject(fichaItem);
        so.FindProperty("_fondoImage").objectReferenceValue = bgImage;
        so.FindProperty("_nombreTexto").objectReferenceValue = tmp;
        so.ApplyModifiedPropertiesWithoutUndo();

        // Ensure folder exists
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs/UI"))
        {
            AssetDatabase.CreateFolder("Assets/Prefabs", "UI");
        }

        // Save as prefab
        string path = "Assets/Prefabs/UI/FichaItemUI.prefab";
        PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);

        AssetDatabase.Refresh();
        Debug.Log($"[CreateFichaItemUIPrefab] Prefab creado en: {path}");
    }
}
