using UnityEngine;
using UnityEditor;

// Tutorial
// https://sandordaemen.nl/blog/unity-3d-extending-the-editor-part-1/

[CustomPropertyDrawer(typeof(TerrainInfo))]
public class PlanetInfoDrawer : PropertyDrawer
{
    Rect r = Rect.zero;
    bool general, noise, climate, debug, chunk;
    SerializedProperty[] t = new SerializedProperty[22];

    void SetSerializedProperty(SerializedProperty property)
    {
        t[0] = property.FindPropertyRelative("planetRadius");
        t[1] = property.FindPropertyRelative("maxHeight");
        t[2] = property.FindPropertyRelative("isMarchingCube");
        t[3] = property.FindPropertyRelative("player");
        t[4] = property.FindPropertyRelative("minChunkPerFace");
        t[5] = property.FindPropertyRelative("maxChunkPerFace");
        t[6] = property.FindPropertyRelative("chunkDetail");
        t[8] = property.FindPropertyRelative("settings");
        t[9] = property.FindPropertyRelative("humidityCount");
        t[10] = property.FindPropertyRelative("humidityMove");
        t[11] = property.FindPropertyRelative("temperatureGradient");
        t[12] = property.FindPropertyRelative("humidityGradient");
        t[14] = property.FindPropertyRelative("biomeQuantity");
        t[15] = property.FindPropertyRelative("drawAsSphere");
        t[16] = property.FindPropertyRelative("showAll");
        t[17] = property.FindPropertyRelative("showTemperature");
        t[18] = property.FindPropertyRelative("showBiome");
        t[19] = property.FindPropertyRelative("instantiateTrees");
        t[7] = property.FindPropertyRelative("chooseBiomes");
        t[13] = property.FindPropertyRelative("menuBiomeNumber");
        t[20] = property.FindPropertyRelative("useOwnColors");
        t[21] = property.FindPropertyRelative("biomeColorWrapper");

    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        SetSerializedProperty(property);
        EditorGUI.indentLevel = 0;
        EditorGUILayout.LabelField("Planet Data");
        General();
        Noise();
        Climate();
        DebugGUI();
        
        EditorGUI.EndProperty();
    }

    void General()
    {
        general = EditorGUILayout.Foldout(general, "General");
        if (general)
        {
            EditorGUI.indentLevel++;
            SetLabel("Radius");
            t[0].floatValue = EditorGUI.FloatField(r, t[0].floatValue);

            SetLabel("Height");
            t[1].intValue = EditorGUI.IntField(r, t[1].intValue);

            SetLabel("Mesh Algorithm: ");
            r.width = r.width / 2;
            if (t[2].boolValue)
                EditorGUI.LabelField(r, new GUIContent("Marching Cubes"));
            else
                EditorGUI.LabelField(r, new GUIContent("Dual Contouring"));
            r.x += r.width;
            if (GUI.Button(r, new GUIContent("Change")))
                t[2].boolValue = !t[2].boolValue;

            r = EditorGUILayout.GetControlRect(true, 16);
            EditorGUI.ObjectField(r, t[3]);
            chunk = EditorGUILayout.Foldout(chunk, "Chunk");
            if (chunk)
            {
                EditorGUI.indentLevel++;
                SetLabel("Chunk Per Face");
                r.width = (r.width / 2) - 20;
                EditorGUI.LabelField(r, new GUIContent("Min"));
                r.x += 30;
                t[4].intValue = EditorGUI.IntField(r, t[4].intValue);
                r.x = r.x + r.width - 20;
                EditorGUI.LabelField(r, new GUIContent("Max"));
                r.x += 30;
                t[5].intValue = EditorGUI.IntField(r, t[5].intValue);
                SetLabel("Chunk Detail");
                t[6].intValue = EditorGUI.IntField(r, t[6].intValue);
                EditorGUI.indentLevel--;
            }
            SetLabel("Use Own Colors");
            t[20].boolValue = EditorGUI.Toggle(r, t[20].boolValue);
            if (t[20].boolValue)
            {
                EditorGUILayout.PropertyField(t[21]);
            }
            EditorGUI.indentLevel--;
        }
    }

    void Noise()
    {
        noise = EditorGUILayout.Foldout(noise, "Noise");
        if (noise)
        {
            EditorGUI.indentLevel++;
            SetLabel("Layers");
            if (t[8].arraySize < 3)
                t[8].arraySize = 3;
            int quantity = EditorGUI.IntField(r, t[8].arraySize);
            if (quantity < 3)
            {
                Debug.LogError("Quantity can't be less than 3");
                return;
            }
            t[8].arraySize = quantity;
            for (int i = 0; i < quantity; i++)
                EditorGUILayout.PropertyField(t[8].GetArrayElementAtIndex(i));
            EditorGUI.indentLevel--;
        }
    }

    void Climate()
    {
        climate = EditorGUILayout.Foldout(climate, "Climate");
        if (climate)
        {
            EditorGUI.indentLevel++;
            SetLabel("Definition");
            t[9].intValue = EditorGUI.IntField(r, t[9].intValue);

            SetLabel("Humidity Move");
            t[10].floatValue = EditorGUI.FloatField(r, t[10].floatValue);
            t[14].intValue = EditorGUILayout.IntSlider(new GUIContent("Number of biomes"), t[14].intValue, 1, 9);

            t[7].boolValue = EditorGUILayout.Toggle(new GUIContent("Choose Biomes"), t[7].boolValue);
            if (t[7].boolValue)
            {
                DrawBiomeChoose();
            }
            t[19].boolValue = EditorGUILayout.Toggle(new GUIContent("Instantiate Trees"), t[19].boolValue);
            EditorGUI.indentLevel--;
        }
    }

    void DebugGUI()
    {
        debug = EditorGUILayout.Foldout(debug, "Debug");
        if (debug)
        {
            EditorGUI.indentLevel++;
            SetLabel("Shape: ");
            r.width = r.width / 2;
            if (t[15].boolValue)
                EditorGUI.LabelField(r, new GUIContent("Sphere"));
            else
                EditorGUI.LabelField(r, new GUIContent("Cube"));
            r.x += r.width;
            if (GUI.Button(r, new GUIContent("Change")))
                t[15].boolValue = !t[15].boolValue;

            SetLabel("Planet Visible: ");
            r.width = r.width / 2;
            if (t[16].boolValue)
                EditorGUI.LabelField(r, new GUIContent("All"));
            else
                EditorGUI.LabelField(r, new GUIContent("Only Visible"));
            r.x += r.width;
            if (GUI.Button(r, new GUIContent("Change")))
                t[16].boolValue = !t[16].boolValue;

            SetLabel("Show Biome: ");
            t[18].boolValue = EditorGUI.Toggle(r, t[18].boolValue);
            if (!t[18].boolValue)
            {
                SetLabel("Show Data: ");
                r.width = r.width / 2;
                if (t[17].boolValue)
                    EditorGUI.LabelField(r, new GUIContent("Temperature"));
                else
                    EditorGUI.LabelField(r, new GUIContent("Humidity"));
                r.x += r.width;
                if (GUI.Button(r, new GUIContent("Change")))
                    t[17].boolValue = !t[17].boolValue;

                if (t[17].boolValue)
                    EditorGUILayout.PropertyField(t[11]);
                else
                    EditorGUILayout.PropertyField(t[12]);
            }
            EditorGUI.indentLevel--;
        }
    }

    void DrawBiomeChoose()
    {
        EditorGUI.indentLevel++;
        t[13].arraySize = t[14].intValue;
        for (int i = 0; i < t[13].arraySize; i++)
        {
            SetLabel("Biome " + i);
            t[13].GetArrayElementAtIndex(i).intValue = EditorGUI.IntSlider(r, t[13].GetArrayElementAtIndex(i).intValue, 0, 8);
        }
        EditorGUI.indentLevel--;
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
