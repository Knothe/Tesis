using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(BiomeTreesWrapper))]
public class BoimeTreeWrapperEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
    }
}


[CustomPropertyDrawer(typeof(BiomeTree))]
public class BiomeTreeDrawer : PropertyDrawer
{
    SerializedProperty menuTreeList;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        menuTreeList = property.FindPropertyRelative("trees");
        EditorGUI.indentLevel = 0;
        EditorGUILayout.LabelField("Biome Tree");
        if (menuTreeList.arraySize != 4)
            menuTreeList.arraySize = 4;

        for (int i = 0; i < menuTreeList.arraySize; i++)
            EditorGUILayout.PropertyField(menuTreeList.GetArrayElementAtIndex(i));

        //SetLabel("Salu3");
        EditorGUI.EndProperty();
    }
 
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return 0;
    }
}
