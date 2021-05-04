using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(PlanetGeneratorSettings))]
public class PlanetGeneratorDrawer : PropertyDrawer
{
    Rect r = Rect.zero;

    public bool general, noise, minSet, maxSet, climate;

    SerializedProperty[] t = new SerializedProperty[18];

    void SetSerializedProperty(SerializedProperty property)
    {
        t[0] = property.FindPropertyRelative("minPlanetRadius");
        t[1] = property.FindPropertyRelative("maxPlanetRadius");
        t[2] = property.FindPropertyRelative("minChunkPerFace");
        t[3] = property.FindPropertyRelative("maxChunkPerFace");
        t[4] = property.FindPropertyRelative("chunkDetail");
        t[5] = property.FindPropertyRelative("minMaxHeight");
        t[6] = property.FindPropertyRelative("maxMaxHeight");
        t[7] = property.FindPropertyRelative("isMarchingCube");
        t[8] = property.FindPropertyRelative("settingsLength");
        t[9] = property.FindPropertyRelative("minSettings");
        t[10] = property.FindPropertyRelative("maxSettings");
        t[11] = property.FindPropertyRelative("humidityCount");
        t[12] = property.FindPropertyRelative("minHumidityMove");
        t[13] = property.FindPropertyRelative("maxHumidityMove");
        t[14] = property.FindPropertyRelative("customTemperatures");
        t[15] = property.FindPropertyRelative("temperatureCurves");
        t[16] = property.FindPropertyRelative("randomRotation");
        t[17] = property.FindPropertyRelative("rotationValues");
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        SetSerializedProperty(property);
        EditorGUI.indentLevel = 0;
        EditorGUILayout.LabelField("Planet Generator Settings");
        General();
        Noise();
        Climate();
        EditorGUI.EndProperty();
    }

    void General()
    {
        general = EditorGUILayout.Foldout(general, "General");
        if (general)
        {
            EditorGUI.indentLevel++;

            SetLabel("Radius");
            r.width = (r.width / 2) - 25;
            EditorGUI.LabelField(r, new GUIContent("Min"));
            r.x += 30;
            t[0].floatValue = EditorGUI.FloatField(r, t[0].floatValue);
            r.x = r.x + r.width - 10;
            EditorGUI.LabelField(r, new GUIContent("Max"));
            r.x += 30;
            t[1].floatValue = EditorGUI.FloatField(r, t[1].floatValue);

            SetLabel("ChunkPerFace");
            r.width = (r.width / 2) - 40;
            EditorGUI.LabelField(r, new GUIContent("Min"));
            r.x += 30;
            t[2].intValue = EditorGUI.IntField(r, t[2].intValue);
            r.x = r.x + r.width - 9;
            EditorGUI.LabelField(r, new GUIContent("Max"));
            r.x += 30;
            t[3].intValue = EditorGUI.IntField(r, t[3].intValue);
            r.x = r.x + r.width + 10;
            r.width = 16;
            if (!CheckDetail(t[2].intValue, t[3].intValue))
                EditorGUI.DrawRect(r, Color.red);
            else
                EditorGUI.DrawRect(r, Color.green);

            SetLabel("Chunk Detail");
            t[4].intValue = EditorGUI.IntField(r, t[4].intValue);

            SetLabel("Max Height");
            r.width = (r.width / 2) - 25;
            EditorGUI.LabelField(r, new GUIContent("Min"));
            r.x += 30;
            t[5].intValue = EditorGUI.IntField(r, t[5].intValue);
            r.x = r.x + r.width - 10;
            EditorGUI.LabelField(r, new GUIContent("Max"));
            r.x += 30;
            t[6].intValue = EditorGUI.IntField(r, t[6].intValue);

            SetLabel("Mesh Algorithm: ");
            r.width = r.width / 2;
            if (t[7].boolValue)
                EditorGUI.LabelField(r, new GUIContent("Marching Cubes"));
            else
                EditorGUI.LabelField(r, new GUIContent("Dual Contouring"));
            r.x += r.width;
            if (GUI.Button(r, new GUIContent("Change")))
                t[7].boolValue = !t[7].boolValue;

            SetLabel("Apply random rotation");
            t[16].boolValue = EditorGUI.Toggle(r, t[16].boolValue);
            if (t[16].boolValue)
            {
                EditorGUILayout.PropertyField(t[17]);
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
            if (t[8].intValue < 3)
                t[8].intValue = 3;
            SetLabel("Settings Length");
            t[8].intValue = EditorGUI.IntField(r, t[8].intValue);
            t[9].arraySize = t[8].intValue;
            t[10].arraySize = t[8].intValue;
            minSet = EditorGUILayout.Foldout(minSet, "Min");
            if (minSet)
            {
                EditorGUI.indentLevel++;
                for (int i = 0; i < t[8].intValue; i++)
                    EditorGUILayout.PropertyField(t[9].GetArrayElementAtIndex(i));
                EditorGUI.indentLevel--;
            }
            maxSet = EditorGUILayout.Foldout(maxSet, "Max");
            if (maxSet)
            {
                EditorGUI.indentLevel++;
                for (int i = 0; i < t[8].intValue; i++)
                    EditorGUILayout.PropertyField(t[10].GetArrayElementAtIndex(i));
                EditorGUI.indentLevel--;
            }
            EditorGUI.indentLevel--;
        }
    }
    
    void Climate()
    {
        climate = EditorGUILayout.Foldout(climate, "Climate");
        if (climate)
        {
            EditorGUI.indentLevel++;

            SetLabel("Humidity Count");
            t[11].intValue = EditorGUI.IntField(r, t[11].intValue);

            SetLabel("Humidity Move");
            r.width = (r.width / 2) - 25;
            EditorGUI.LabelField(r, new GUIContent("Min"));
            r.x += 30;
            t[12].floatValue = EditorGUI.FloatField(r, t[12].floatValue);
            r.x = r.x + r.width - 10;
            EditorGUI.LabelField(r, new GUIContent("Max"));
            r.x += 30;
            t[13].floatValue = EditorGUI.FloatField(r, t[13].floatValue);

            SetLabel("Use Custom Temperatures");
            t[14].boolValue = EditorGUI.Toggle(r, t[14].boolValue);
            if (t[14].boolValue)
            {
                SetLabel("Size");
                int arS = t[15].arraySize;
                arS = EditorGUI.IntField(r, arS);
                if (arS <= 0)
                    arS = 1;
                t[15].arraySize = arS;
                for(int i = 0; i < t[15].arraySize; i++)
                    EditorGUILayout.PropertyField(t[15].GetArrayElementAtIndex(i));
            }

            EditorGUI.indentLevel--;
        }
    }

    void SetLabel(string name)
    {
        r = EditorGUILayout.GetControlRect(true, 16);
        r = EditorGUI.PrefixLabel(r, new GUIContent(name));
        r.x -= 15;
        r.width += 15;
    }

    bool CheckDetail(int min, int max)
    {
        if (min <= 0 || max <= 0)
            return false;
        float minCPF = min;
        float maxCPF = max;
        while(maxCPF > minCPF)
            maxCPF /= 2;
        return maxCPF == minCPF;
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return 0;
    }
}

[CustomEditor(typeof(GeneratorSettingsWrapper))]
public class PlanetGeneratorWrapperEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
    }
}

