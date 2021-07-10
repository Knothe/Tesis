using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(PlanetGeneratorSettings))]
public class PlanetGeneratorDrawer : PropertyDrawer
{
    Rect r = Rect.zero;

    public bool general, noise, minSet, maxSet, climate;

    SerializedProperty[] t = new SerializedProperty[19];
    SerializedProperty[] _t = new SerializedProperty[2];
    List<bool> settings1 = new List<bool>();
    List<bool> settings2 = new List<bool>();

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
        t[18] = property.FindPropertyRelative("levelOfDetail");
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
        if (general) {
            EditorGUI.indentLevel++;

            SetLabel("Radius", "Rango de radios posibles para los planetas generados");
            r.width = (r.width / 2) - 25;
            EditorGUI.LabelField(r, new GUIContent("Min"));
            r.x += 30;
            t[0].floatValue = EditorGUI.FloatField(r, t[0].floatValue);
            r.x = r.x + r.width - 10;
            EditorGUI.LabelField(r, new GUIContent("Max"));
            r.x += 30;
            t[1].floatValue = EditorGUI.FloatField(r, t[1].floatValue);

            SetLabel("ChunkPerFace");
            r.width = (r.width / 2) - 25;
            EditorGUI.LabelField(r, new GUIContent("Min"));
            r.x += 30;
            t[2].intValue = EditorGUI.IntField(r, t[2].intValue);
            r.x = r.x + r.width - 10;
            EditorGUI.LabelField(r, new GUIContent("Max"));
            r.x += 30;
            EditorGUI.LabelField(r, t[3].intValue.ToString());

            SetLabel("Levels of Detail", "Cantidad de niveles de detalle");
            t[18].intValue = EditorGUI.IntField(r, t[18].intValue);
            if (t[18].intValue < 1)
                t[18].intValue = 1;
            SetChunksPerFace();

            SetLabel("Chunk Detail", "Subdivisiones del chunk");
            t[4].intValue = EditorGUI.IntField(r, t[4].intValue);

            SetLabel("Height", "Rango de altura y profundidad máxima del terreno");
            r.width = (r.width / 2) - 25;
            EditorGUI.LabelField(r, new GUIContent("Min"));
            r.x += 30;
            t[5].intValue = EditorGUI.IntField(r, t[5].intValue);
            r.x = r.x + r.width - 10;
            EditorGUI.LabelField(r, new GUIContent("Max"));
            r.x += 30;
            t[6].intValue = EditorGUI.IntField(r, t[6].intValue);

            SetLabel("Mesh Algorithm: ", "Algoritmo de modelado del planeta");
            r.width = r.width / 2;
            if (t[7].boolValue)
                EditorGUI.LabelField(r, new GUIContent("Marching Cubes"));
            else
                EditorGUI.LabelField(r, new GUIContent("Dual Contouring"));
            r.x += r.width;
            if (GUI.Button(r, new GUIContent("Change")))
                t[7].boolValue = !t[7].boolValue;

            SetLabel("Apply random rotation", "Aplicar rotación a los planetas generados");
            t[16].boolValue = EditorGUI.Toggle(r, t[16].boolValue);
            if (t[16].boolValue)
            {
                EditorGUILayout.PropertyField(t[17], new GUIContent("Rotation Values", "Ángulo Euler de rotación aplicado a los planetas generados"));
            }

            EditorGUI.indentLevel--;
        }
    }

    void SetChunksPerFace() {
        int max = t[2].intValue;
        for (int i = 1; i < t[2].intValue; i++)
            max *= 2;
        t[3].intValue = max;
    }

    void Noise()
    {
        noise = EditorGUILayout.Foldout(noise, "Noise");
        if (noise)
        {
            EditorGUI.indentLevel++;
            if (t[8].intValue < 3)
                t[8].intValue = 3;
            SetLabel("Settings Length", "Cantidad de layers");
            t[8].intValue = EditorGUI.IntField(r, t[8].intValue);
            t[9].arraySize = t[8].intValue;
            t[10].arraySize = t[8].intValue;

            if (settings1.Count < t[8].intValue)
                while (settings1.Count != t[8].intValue)
                    settings1.Add(false);
            else if (settings1.Count > t[8].intValue)
                while (settings1.Count != t[8].intValue)
                    settings1.RemoveAt(settings1.Count - 1);

            if (settings2.Count < t[8].intValue)
                while (settings2.Count != t[8].intValue)
                    settings2.Add(false);
            else if (settings2.Count > t[8].intValue)
                while (settings2.Count != t[8].intValue)
                    settings2.RemoveAt(settings2.Count - 1);

            minSet = EditorGUILayout.Foldout(minSet, new GUIContent("Min", "Valores mínimos de ruido"));
            if (minSet)
            {
                EditorGUI.indentLevel++;
                for (int i = 0; i < t[8].intValue; i++)
                {
                    settings1[i] = EditorGUILayout.Foldout(settings1[i], "Layer " + i);
                    if (settings1[i])
                    {
                        EditorGUI.indentLevel++;
                        _t[0] = t[9].GetArrayElementAtIndex(i).FindPropertyRelative("strength");
                        _t[1] = t[9].GetArrayElementAtIndex(i).FindPropertyRelative("scale");
                        SetLabel("Strength", "Impacto a la altura del terreno, el valor de cada layer afecta");
                        _t[0].floatValue = EditorGUI.FloatField(r, _t[0].floatValue);
                        SetLabel("Scale", "Escala de la función de ruido");
                        _t[1].floatValue = EditorGUI.FloatField(r, _t[1].floatValue);
                        EditorGUI.indentLevel--;
                    }
                }
                EditorGUI.indentLevel--;
            }
            maxSet = EditorGUILayout.Foldout(maxSet, new GUIContent("Max", "Valores máximos de ruido"));
            if (maxSet)
            {
                EditorGUI.indentLevel++;
                for (int i = 0; i < t[8].intValue; i++)
                {
                    settings2[i] = EditorGUILayout.Foldout(settings2[i], "Layer " + i);
                    if (settings2[i])
                    {
                        EditorGUI.indentLevel++;
                        _t[0] = t[10].GetArrayElementAtIndex(i).FindPropertyRelative("strength");
                        _t[1] = t[10].GetArrayElementAtIndex(i).FindPropertyRelative("scale");
                        SetLabel("Strength", "Impacto a la altura del terreno, el valor de cada layer afecta");
                        _t[0].floatValue = EditorGUI.FloatField(r, _t[0].floatValue);
                        SetLabel("Scale", "Escala de la función de ruido");
                        _t[1].floatValue = EditorGUI.FloatField(r, _t[1].floatValue);
                        EditorGUI.indentLevel--;
                    }
                }
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

            SetLabel("Definition", "Precisión de la humedad, mientras mayor sea el valor, mayor precisión hay");
            t[11].intValue = EditorGUI.IntField(r, t[11].intValue);

            SetLabel("Humidity Move", "Rango de distancias que recorre la humedad antes de desaparecer, el valor representa radios");
            r.width = (r.width / 2) - 25;
            EditorGUI.LabelField(r, new GUIContent("Min"));
            r.x += 30;
            t[12].floatValue = EditorGUI.FloatField(r, t[12].floatValue);
            r.x = r.x + r.width - 10;
            EditorGUI.LabelField(r, new GUIContent("Max"));
            r.x += 30;
            t[13].floatValue = EditorGUI.FloatField(r, t[13].floatValue);

            SetLabel("Use Custom Temperatures", "Usará una lista de curvas de temperatura para representar la temperatura\n" +
                "Eje x: Latitud del planeta donde 0 es el centro y 1 son los límites del planeta\n" +
                "Eje y: Temperatura donde el 0 es la mayor temperatura posible y 1 es la menor");
            t[14].boolValue = EditorGUI.Toggle(r, t[14].boolValue);
            if (t[14].boolValue)
            {
                SetLabel("Size");
                int arS = t[15].arraySize;
                arS = EditorGUI.IntField(r, arS);
                if (arS <= 0)
                    arS = 1;
                t[15].arraySize = arS;
                EditorGUI.indentLevel++;
                for(int i = 0; i < t[15].arraySize; i++)
                    EditorGUILayout.PropertyField(t[15].GetArrayElementAtIndex(i));
                EditorGUI.indentLevel--;
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

    void SetLabel(string name, string tooltip)
    {
        r = EditorGUILayout.GetControlRect(true, 16);
        r = EditorGUI.PrefixLabel(r, new GUIContent(name, tooltip));
        r.x -= 15;
        r.width += 15;
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

