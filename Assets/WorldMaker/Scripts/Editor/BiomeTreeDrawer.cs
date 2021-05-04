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
    SerializedProperty typesOfTrees;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        menuTreeList = property.FindPropertyRelative("trees");
        typesOfTrees = property.FindPropertyRelative("typesOfTrees");
        if (menuTreeList.arraySize != 4)
            menuTreeList.arraySize = 4;
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Types of trees");
        typesOfTrees.intValue = EditorGUILayout.IntSlider(typesOfTrees.intValue, 1, 4);
        EditorGUILayout.EndHorizontal();
        for (int i = 0; i < typesOfTrees.intValue; i++)
            EditorGUILayout.PropertyField(menuTreeList.GetArrayElementAtIndex(i));
        EditorGUI.EndProperty();
    }
 
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return 0;
    }
}
