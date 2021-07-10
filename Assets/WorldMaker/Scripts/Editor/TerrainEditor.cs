#if (UNITY_EDITOR) 
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TerrainManager))]
public class TerrainEditor : Editor
{
    TerrainManager terrain;

    public override void OnInspectorGUI()
    {
        using (var check = new EditorGUI.ChangeCheckScope())
        {
            base.OnInspectorGUI();
        }
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Generate New Planet"))
            terrain.GenerateOnEditor();
        GUILayout.EndHorizontal();
    }

    private void OnEnable()
    {
        terrain = (TerrainManager)target;
    }
}
#endif