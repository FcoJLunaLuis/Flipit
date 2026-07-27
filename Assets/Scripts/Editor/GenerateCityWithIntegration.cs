using UnityEditor;

/// <summary>
/// Safe menu item that ONLY integrates systems into the existing city scene.
/// Does NOT regenerate the city — your manual scene work is preserved.
/// </summary>
public static class GenerateCityWithIntegration
{
    [MenuItem("Flipit/Integrate Systems (Safe - No Regen)", priority = 0)]
    public static void IntegrateOnly()
    {
        CitySceneIntegrationBuilder.IntegrateSystems();

        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();

        EditorUtility.DisplayDialog(
            "Integration Complete",
            "All systems integrated into current scene.\n" +
            "City was NOT regenerated.\n\n" +
            "Press Play to test!",
            "OK");
    }
}
