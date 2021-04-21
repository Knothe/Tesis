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

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        biomeList = property.FindPropertyRelative("biomeList");
        EditorGUI.indentLevel = 0;
        EditorGUILayout.LabelField("Biome Colors");
        if (biomeList.arraySize != 10)
            biomeList.arraySize = 10;

        for(int i = 0; i < biomeList.arraySize; i++)
        {
            foldouts[i] = EditorGUILayout.Foldout(foldouts[i], TerrainInfoData.biomeName[i]);
            EditorGUI.indentLevel++;
            if(foldouts[i])
                EditorGUILayout.PropertyField(biomeList.GetArrayElementAtIndex(i));
            EditorGUI.indentLevel--;
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return 0;
    }
}

[CustomPropertyDrawer(typeof(BiomeC))]
public class BiomeCDrawer : PropertyDrawer
{
    Rect r = Rect.zero;

    SerializedProperty colors;
    SerializedProperty limits;
    bool[] colorF = new bool[10];
    bool[] limitF = new bool[10];

    int quantity;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        int index = (int)char.GetNumericValue(label.text[label.text.Length - 1]);
        colors = property.FindPropertyRelative("colors");
        limits = property.FindPropertyRelative("limits");
        quantity = colors.arraySize;
        SetLabel("Size");
        quantity = EditorGUI.IntField(r, quantity);
        if (quantity <= 0)
            quantity = 1;
        colors.arraySize = quantity;
        limits.arraySize = quantity;
        colorF[index] = EditorGUILayout.Foldout(colorF[index], "Colors");
        if (colorF[index])
        {
            EditorGUI.indentLevel++;
            for (int i = 0; i < quantity; i++)
                EditorGUILayout.PropertyField(colors.GetArrayElementAtIndex(i));
            EditorGUI.indentLevel--;
        }

        limitF[index] = EditorGUILayout.Foldout(limitF[index], "Limits");
        if (limitF[index])
        {
            EditorGUI.indentLevel++;
            for (int i = 0; i < quantity; i++)
                EditorGUILayout.PropertyField(limits.GetArrayElementAtIndex(i));
            EditorGUI.indentLevel--;
        }
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

