using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(BiomeTreeCollection))]
public class BiomeTreeCollectionEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
    }
}

[CustomPropertyDrawer(typeof(TreeCollection))]
public class TreeCollectionDrawer : PropertyDrawer
{
    SerializedProperty useScriptable;
    SerializedProperty collection;
    SerializedProperty biomeTree;
    SerializedProperty biomeTreeWrapper;
    SerializedProperty[] treeSet = new SerializedProperty[6];
    SerializedProperty p;
    bool c;
    bool[] bT = new bool[9];
    Rect r = Rect.zero;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        p = property;
        SetMainValues();
        EditorGUI.indentLevel = 0;
        EditorGUILayout.LabelField("Biome Tree Collection");
        DefaultValues();
        useScriptable.boolValue = EditorGUILayout.Toggle(new GUIContent("Use Scriptable Objects"), useScriptable.boolValue);
        EditorGUI.indentLevel++;
        if (useScriptable.boolValue)
        {
            DrawScriptable();
        }
        else
        {
            NotScriptable();
        }
        EditorGUI.indentLevel--;
        EditorGUI.EndProperty();
    }

    void DefaultValues()
    {
        SetLabel("Scale");
        r.x += 15;
        r.width = (r.width / 2) - 27;
        EditorGUI.LabelField(r, new GUIContent("1"));
        r.x += 15;
        treeSet[0].floatValue = EditorGUI.FloatField(r, treeSet[0].floatValue);
        r.x = r.x + r.width + 5;
        EditorGUI.LabelField(r, new GUIContent("2"));
        r.x += 18;
        treeSet[2].floatValue = EditorGUI.FloatField(r, treeSet[2].floatValue);

        EditorGUILayout.PropertyField(treeSet[1]);
        EditorGUILayout.PropertyField(treeSet[3]);
        if(GUILayout.Button(new GUIContent("Randomize offsets")))
        {
            treeSet[1].vector3Value = new Vector3( Random.Range(-100.0f, 100.0f), Random.Range(-100.0f, 100.0f), Random.Range(-100.0f, 100.0f));
            treeSet[3].vector3Value = new Vector3( Random.Range(-100.0f, 100.0f), Random.Range(-100.0f, 100.0f), Random.Range(-100.0f, 100.0f));
        }
        EditorGUILayout.PropertyField(treeSet[4]);
        EditorGUILayout.PropertyField(treeSet[5]);
    }

    void DrawScriptable()
    {
        biomeTreeWrapper = collection.FindPropertyRelative("biomeTrees");
        if (biomeTreeWrapper.arraySize != 9)
            biomeTreeWrapper.arraySize = 9;
        c = EditorGUILayout.Foldout(c, "Biome Trees");
        if (c)
        {
            EditorGUI.indentLevel++;
            for(int i = 0; i < biomeTreeWrapper.arraySize; i++)
                EditorGUILayout.PropertyField(biomeTreeWrapper.GetArrayElementAtIndex(i), new GUIContent(TerrainInfoData.biomeName[i]));
            EditorGUI.indentLevel--;
        }
    }

    void NotScriptable()
    {
        if (biomeTree.arraySize != 9)
            biomeTree.arraySize = 9;

        for(int i = 0; i < biomeTree.arraySize; i++)
        {
            bT[i] = EditorGUILayout.Foldout(bT[i], new GUIContent(TerrainInfoData.biomeName[i]));
            if (bT[i])
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(biomeTree.GetArrayElementAtIndex(i));
                EditorGUI.indentLevel--;
            }
        }
    }

    void SetMainValues()
    {
        useScriptable = p.FindPropertyRelative("useScriptableObjects");
        collection = p.FindPropertyRelative("collection");
        biomeTree = p.FindPropertyRelative("biomeTree");

        treeSet[0] = collection.FindPropertyRelative("scale1");
        treeSet[1] = collection.FindPropertyRelative("offset1");
        treeSet[2] = collection.FindPropertyRelative("scale2");
        treeSet[3] = collection.FindPropertyRelative("offset2");
        treeSet[4] = collection.FindPropertyRelative("maxTrees");
        treeSet[5] = collection.FindPropertyRelative("missedTreesMax");

    }

    void SetLabel(string name)
    {
        r = EditorGUILayout.GetControlRect(true, 18);
        r = EditorGUI.PrefixLabel(r, new GUIContent(name));
        r.x -= 15;
        r.width += 15;
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return 0;
    }

}


