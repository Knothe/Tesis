using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(BiomeColorWrapper))]
public class BiomeColorWrapperEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
    }
}

[CustomPropertyDrawer(typeof(BiomeColors))]
public class BiomeColorDrawer : PropertyDrawer
{
    SerializedProperty biomeList;
    bool[] foldouts = new bool[10];
    Rect r = Rect.zero;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        biomeList = property.FindPropertyRelative("biomeList");
        EditorGUI.indentLevel = 0;
        EditorGUILayout.LabelField("Biome Colors");

        if (biomeList.arraySize != 10)
            biomeList.arraySize = 10;

        EditorGUI.indentLevel++;
        for(int i = 0; i < biomeList.arraySize; i++)
        {
            SetLabel(TerrainInfoData.biomeName[i]);
            EditorGUI.PropertyField(r, biomeList.GetArrayElementAtIndex(i), new GUIContent(""));
        }
        EditorGUI.indentLevel--;
        EditorGUI.EndProperty();
    }

    void SetLabel(string name)
    {
        r = EditorGUILayout.GetControlRect(true, 16);
        r = EditorGUI.PrefixLabel(r, new GUIContent(name));
        r.x -= 15;
        r.width += 15;
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return 0;
    }
}
