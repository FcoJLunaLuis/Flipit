#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

/// <summary>
/// Editor custom para CombatArena. Agrega botón "Acomodar Muros" en el Inspector.
/// </summary>
[CustomEditor(typeof(CombatArena))]
public class CombatArenaEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.Space(10);

        var arena = (CombatArena)target;

        if (GUILayout.Button("Acomodar Muros al Polígono", GUILayout.Height(30)))
        {
            Undo.RecordObjects(GetMurosTransforms(arena), "Acomodar Muros");
            arena.AcomodarMuros();
            EditorUtility.SetDirty(arena);
        }
    }

    private Object[] GetMurosTransforms(CombatArena arena)
    {
        // Registrar undo para los transforms de los muros
        var field = typeof(CombatArena).GetField("_muros", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field == null) return new Object[] { arena };

        var muros = field.GetValue(arena) as GameObject[];
        if (muros == null || muros.Length == 0) return new Object[] { arena };

        var objects = new Object[muros.Length + 1];
        objects[0] = arena;
        for (int i = 0; i < muros.Length; i++)
        {
            objects[i + 1] = muros[i] != null ? muros[i].transform : (Object)arena;
        }
        return objects;
    }
}
#endif
