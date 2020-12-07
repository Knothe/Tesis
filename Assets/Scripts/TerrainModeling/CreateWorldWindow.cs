using UnityEngine;
using UnityEditor;
using Unity.Mathematics;
using System.Collections.Generic;

public class CreateWorldWindow : EditorWindow
{
    float radius;
    int maxHeight;
    bool algorithm;
    // Player
    int minChunkPerFace;
    int maxChunkPerFace;
    int chunkDetail;

    float3 noiseOffset;
    int listSize = 3;
    bool isSettingsOpen;
    List<NoiseSettings> settings;
    List<bool> settingsOpen;

    int humidityCount;
    float humidityMove;
    Gradient temperatureGrad = new Gradient();
    Gradient humidityGrad = new Gradient();
    int biomeQuantity;

    Rect r = Rect.zero;

    [MenuItem("Window/World Generator")]
    public static void ShowWindow()
    {
        GetWindow<CreateWorldWindow>("World Generator");
        
    }

    private void Awake()
    {
        settings = new List<NoiseSettings>();
        settingsOpen = new List<bool>();
        for (int i = 0; i < 3; i++)
        {
            settings.Add(new NoiseSettings());
            settingsOpen.Add(false);
        }
        minChunkPerFace = 1;
        maxChunkPerFace = 2;
        chunkDetail = 1;
        radius = 1;
    }

    private void OnGUI()
    {
        EditorGUI.indentLevel = 0;
        EditorGUILayout.LabelField("General");
        EditorGUI.indentLevel++;
        SetLabel("Radius");
        radius = EditorGUI.FloatField(r, radius);

        SetLabel("Height");
        maxHeight = EditorGUI.IntField(r, maxHeight);

        SetLabel("Mesh Algorithm: ");
        r.width = r.width / 2;
        if (algorithm)
            EditorGUI.LabelField(r, new GUIContent("Marching Cubes"));
        else
            EditorGUI.LabelField(r, new GUIContent("Dual Contouring"));
        r.x += r.width;
        if (GUI.Button(r, new GUIContent("Change")))
            algorithm = !algorithm;
        EditorGUILayout.LabelField("Chunk");
        EditorGUI.indentLevel++;
        SetLabel("Chunk Per Face");
        r.width = (r.width / 2) - 20;
        EditorGUI.LabelField(r, new GUIContent("Min"));
        r.x += 30;
        minChunkPerFace = EditorGUI.IntField(r, minChunkPerFace);
        r.x = r.x + r.width - 20;
        EditorGUI.LabelField(r, new GUIContent("Max"));
        r.x += 30;
        maxChunkPerFace = EditorGUI.IntField(r, maxChunkPerFace);

        

        SetLabel("Chunk Detail");
        chunkDetail = EditorGUI.IntField(r, chunkDetail);
        EditorGUI.indentLevel--;
        EditorGUI.indentLevel--;
        EditorGUILayout.LabelField("Noise");
        EditorGUI.indentLevel++;

        SetLabel("Offset");
        if (GUI.Button(r, new GUIContent("Randomize")))
        {
            noiseOffset.x = UnityEngine.Random.Range(-1000.0f, 1000.0f);
            noiseOffset.y = UnityEngine.Random.Range(-1000.0f, 1000.0f);
            noiseOffset.z = UnityEngine.Random.Range(-1000.0f, 1000.0f);
        }
        r = EditorGUILayout.GetControlRect(true, 16);

        r.width = (r.width / 3) - 30;
        EditorGUI.LabelField(r, new GUIContent("X"));
        r.x += 10;
        noiseOffset.x = EditorGUI.FloatField(r, noiseOffset.x);
        r.x += r.width - 10;

        EditorGUI.LabelField(r, new GUIContent("Y"));
        r.x += 10;
        noiseOffset.y = EditorGUI.FloatField(r, noiseOffset.y);
        r.x += r.width - 10;

        EditorGUI.LabelField(r, new GUIContent("Z"));
        r.x += 10;
        noiseOffset.z = EditorGUI.FloatField(r, noiseOffset.z);
        r.x += r.width - 10;

        isSettingsOpen = EditorGUILayout.Foldout(isSettingsOpen, "Noise");
        if (isSettingsOpen)
        {
            EditorGUI.indentLevel++;
            SetLabel("List Size");
            listSize = EditorGUI.IntField(r, listSize);
            if (listSize < 3)
                listSize = 3;
            if(listSize > settings.Count)
            {
                while(settings.Count < listSize)
                {
                    settings.Add(new NoiseSettings());
                    settingsOpen.Add(false);
                }
            }
            else if(listSize < settings.Count)
            {
                while(settings.Count > listSize)
                {
                    settings.RemoveAt(settings.Count - 1);
                    settingsOpen.RemoveAt(settings.Count - 1);
                }
            }

            for(int i = 0; i < listSize; i++)
            {
                settingsOpen[i] = EditorGUILayout.Foldout(settingsOpen[i], "Layer " + i);
                if (settingsOpen[i])
                {
                    EditorGUI.indentLevel++;
                    settings[i].strength = EditorGUILayout.FloatField("Strength", settings[i].strength);
                    settings[i].scale = EditorGUILayout.FloatField("Scale", settings[i].scale);
                    EditorGUI.indentLevel--;
                }
            }

            EditorGUI.indentLevel--;
        }

        EditorGUI.indentLevel--;
        EditorGUILayout.LabelField("Climate");
        EditorGUI.indentLevel++;
        SetLabel("Definition");
        humidityCount = EditorGUI.IntField(r, humidityCount);

        SetLabel("Humidity Move");
        humidityMove = EditorGUI.FloatField(r,humidityMove);
        biomeQuantity = EditorGUILayout.IntSlider(new GUIContent("Number of biomes"), biomeQuantity, 1, 9);

        temperatureGrad = EditorGUILayout.GradientField(new GUIContent("Temperature Gradient"), temperatureGrad);
        humidityGrad = EditorGUILayout.GradientField(new GUIContent("Humidity Gradient"), humidityGrad);
        EditorGUI.indentLevel--;
        if (GUILayout.Button("Generate Planet"))
        {
            if (!CheckDetailValidity())
                Debug.Log("Chunks per face don't match");
            else
            {
                GameObject g = new GameObject("New Planet", typeof(TerrainManager));
                TerrainManager t = g.GetComponent<TerrainManager>();
                t.planetData = new TerrainInfo(radius, maxHeight, algorithm, minChunkPerFace, maxChunkPerFace, chunkDetail, settings, noiseOffset);
                t.planetData.SetClimate(humidityCount, humidityMove, temperatureGrad, humidityGrad, biomeQuantity);
            }
        }
    }

    bool CheckDetailValidity()
    {
        float maxCPF = maxChunkPerFace;
        float minCPF = minChunkPerFace;
        while(maxCPF > minCPF)
            maxCPF /= 2;
        return maxCPF == minCPF;
    }

    void SetLabel(string name)
    {
        r = EditorGUILayout.GetControlRect(true, 18);
        r = EditorGUI.PrefixLabel(r, new GUIContent(name));
        r.x -= 15;
        r.width += 15;
    }

}
