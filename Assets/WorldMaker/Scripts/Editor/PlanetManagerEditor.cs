using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PlanetsManager))]
public class PlanetManagerEditor : Editor
{
    Rect r = Rect.zero;

    PlanetsManager planetManager;
    SerializedProperty[] t = new SerializedProperty[27];
    SerializedProperty[] treeSet = new SerializedProperty[7];

    bool generate, created, orbits, flora, general, tSD, biomeTree;

    public override void OnInspectorGUI()
    {
        EditorGUI.indentLevel = 0;
        serializedObject.Update();
        General();
        Generate();
        Created();
        Orbits();
        Flora();
        serializedObject.ApplyModifiedProperties();
    }

    void General()
    {
        general = EditorGUILayout.Foldout(general, "General");
        if (general)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(t[0]);
            EditorGUILayout.PropertyField(t[20]);
            EditorGUILayout.PropertyField(t[21]);
            EditorGUILayout.PropertyField(t[17]);
            EditorGUILayout.PropertyField(t[18]);
            EditorGUILayout.PropertyField(t[24]);
            EditorGUILayout.PropertyField(t[25]);
            EditorGUI.indentLevel--;
        }
    }

    void Generate()
    {
        generate = EditorGUILayout.Foldout(generate, "Generate Planets");
        if (generate)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(t[1]);
            if (t[1].boolValue)
            {
                EditorGUILayout.PropertyField(t[4]);
                SetLabel("New Planets");
                r.width = (r.width / 2) - 25;
                EditorGUI.LabelField(r, new GUIContent("Min"));
                r.x += 30;
                t[5].intValue = EditorGUI.IntField(r, t[5].intValue);
                r.x = r.x + r.width - 10;
                EditorGUI.LabelField(r, new GUIContent("Max"));
                r.x += 30;
                t[6].intValue = EditorGUI.IntField(r, t[6].intValue);

                float width;
                SetLabel("Biomes Per Planet");
                width = r.width - 50;
                r.width = 50;
                EditorGUI.LabelField(r, new GUIContent("Min"));
                r.x += 50;
                r.width = width;
                t[7].intValue = EditorGUI.IntSlider(r, t[7].intValue, 1, 8);
                r.x = r.x + r.width - 10;
                r.width = r.width / 2;

                SetLabel("                 ");
                r.width = 50;
                EditorGUI.LabelField(r, new GUIContent("Max"));
                r.width = width;
                r.x += 50;
                t[8].intValue = EditorGUI.IntSlider(r, t[8].intValue, 1, 8);
                EditorGUILayout.PropertyField(t[9]);
            }
            EditorGUI.indentLevel--;
        }
    }

    void Created()
    {
        created = EditorGUILayout.Foldout(created, "Created planets");
        if (created)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(t[2]);
            EditorGUILayout.PropertyField(t[19]);
            r = EditorGUILayout.GetControlRect(true, 18);
            if (GUI.Button(r, new GUIContent("Clean planet list")))
                planetManager.CleanPlanetsList();
            EditorGUI.indentLevel--;
        }
    }

    void Orbits()
    {
        if (t[1].boolValue)
        {
            orbits = EditorGUILayout.Foldout(orbits, "Orbtis");
            if (orbits)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(t[10]);
                EditorGUILayout.PropertyField(t[11]);
                EditorGUILayout.PropertyField(t[12]);
                EditorGUILayout.PropertyField(t[13]);
                EditorGUILayout.PropertyField(t[14]);
                EditorGUILayout.PropertyField(t[15]);
                EditorGUI.indentLevel--;
            }
        }
    }

    void Flora()
    {
        flora = EditorGUILayout.Foldout(flora, "Tree generation");
        if (flora)
        {
            EditorGUI.indentLevel++;

            EditorGUILayout.PropertyField(t[16]);
            if (t[16].boolValue)
            {
                EditorGUILayout.PropertyField(t[22]);
                SetLabel("Use Scriptable Tree Set");
                t[3].boolValue = EditorGUI.Toggle(r, t[3].boolValue);
                if(!t[3].boolValue)
                    TreeSetDraw();
                else
                {
                    EditorGUILayout.PropertyField(t[26]);
                }
            }

            EditorGUI.indentLevel--;
        }
    }

    void TreeSetDraw()
    {
        tSD = EditorGUILayout.Foldout(tSD, "Tree Set");
        if (tSD)
        {
            EditorGUI.indentLevel++;
            SetLabel("Scale");
            r.x -= 15;
            r.width = (r.width / 2) - 3;
            EditorGUI.LabelField(r, new GUIContent("1"));
            r.x += 18;
            treeSet[0].floatValue = EditorGUI.FloatField(r, treeSet[0].floatValue); 
            r.x = r.x + r.width - 15;
            EditorGUI.LabelField(r, new GUIContent("2"));
            r.x += 18;
            treeSet[2].floatValue = EditorGUI.FloatField(r, treeSet[2].floatValue);

            EditorGUILayout.PropertyField(treeSet[1]);
            EditorGUILayout.PropertyField(treeSet[3]);
            EditorGUILayout.PropertyField(treeSet[4]);
            EditorGUILayout.PropertyField(treeSet[5]);

            if (treeSet[6].arraySize != 9)
                treeSet[6].arraySize = 9;
            biomeTree = EditorGUILayout.Foldout(biomeTree, "Biome Trees");
            if (biomeTree)
            {
                EditorGUI.indentLevel++;
                for (int i = 0; i < treeSet[6].arraySize; i++)
                    EditorGUILayout.PropertyField(treeSet[6].GetArrayElementAtIndex(i), new GUIContent(TerrainInfoData.biomeName[i]));
                EditorGUI.indentLevel--;
            }
            EditorGUI.indentLevel--;
        }
    }

    void SetLabel(string name)
    {
        r = EditorGUILayout.GetControlRect(true, 18);
        r = EditorGUI.PrefixLabel(r, new GUIContent(name));
        r.x -= 15;
        r.width += 15;
    }

    private void OnEnable()
    {
        planetManager = (PlanetsManager)serializedObject.targetObject;
        t[0] = serializedObject.FindProperty("player");
        t[1] = serializedObject.FindProperty("generateNewPlanets");
        t[2] = serializedObject.FindProperty("useCreatedPlanets");
        t[4] = serializedObject.FindProperty("settingsWrapper");
        t[5] = serializedObject.FindProperty("minPlanets");
        t[6] = serializedObject.FindProperty("maxPlanets");
        t[7] = serializedObject.FindProperty("minBiomesPerPlanet");
        t[8] = serializedObject.FindProperty("maxBiomesPerPlanet");
        t[9] = serializedObject.FindProperty("useAllBiomes");
        t[10] = serializedObject.FindProperty("numOrbits");
        t[11] = serializedObject.FindProperty("firstOrbitAlone");
        t[12] = serializedObject.FindProperty("firstOrbitPos");
        t[13] = serializedObject.FindProperty("distancePerOrbits");
        t[14] = serializedObject.FindProperty("maxPlanetsPerOrbit");
        t[15] = serializedObject.FindProperty("elevationVariant");
        t[16] = serializedObject.FindProperty("instantiateTrees");
        t[17] = serializedObject.FindProperty("groundLayer");
        t[18] = serializedObject.FindProperty("groundTag");
        t[19] = serializedObject.FindProperty("planets");
        t[20] = serializedObject.FindProperty("biomeColorWrapper");
        t[21] = serializedObject.FindProperty("planetMaterial");
        t[22] = serializedObject.FindProperty("plantSizeAlteration");
        t[23] = serializedObject.FindProperty("treeSet");
        t[24] = serializedObject.FindProperty("atmosphere");
        t[25] = serializedObject.FindProperty("biomeTexture");

        t[3] = serializedObject.FindProperty("useScriptableTreeSet");
        t[26] = serializedObject.FindProperty("treeCollection");

        treeSet[0] = t[23].FindPropertyRelative("scale1");
        treeSet[1] = t[23].FindPropertyRelative("offset1");
        treeSet[2] = t[23].FindPropertyRelative("scale2");
        treeSet[3] = t[23].FindPropertyRelative("offset2");
        treeSet[4] = t[23].FindPropertyRelative("maxTrees");
        treeSet[5] = t[23].FindPropertyRelative("missedTreesMax");
        treeSet[6] = t[23].FindPropertyRelative("biomeTrees");

    }
}
