#if (UNITY_EDITOR) 

using UnityEngine;
using UnityEditor;
using Unity.Mathematics;
using System.Collections.Generic;

public class CreateWorldWindow : EditorWindow
{
    // General
    float radius;
    int maxHeight;
    bool isMarchingCube;
    int minChunkPerFace;
    int maxChunkPerFace;
    int chunkDetail;

    // Noise
    bool noiseList;
    int listSize = 3;
    List<NoiseSettings> settings;
    List<bool> settingsOpen;

    // Climate
    int humidityCount;      // Definition
    float humidityMove;
    int biomeQuantity;
    bool chooseBiomes;
    bool biomeListOpen;
    List<int> biomeList = new List<int>();
    bool instantiateTrees;
    Gradient temperatureGrad = new Gradient();
    Gradient humidityGrad = new Gradient();

    Vector2 scrollPosition = Vector2.zero;
    string planetName = "";
    Rect r = Rect.zero;

    bool useCurve;
    public AnimationCurve curve;

    [MenuItem("Window/World Generator/CompleteWG")]
    public static void ShowWindow()
    {
        GetWindow<CreateWorldWindow>("World Generator");
    }

    private void Awake()
    {
        settings = new List<NoiseSettings>();
        if(settingsOpen == null)
            settingsOpen = new List<bool>();
        for (int i = 0; i < listSize; i++)
            AddNoiseLayer();
        minChunkPerFace = 1;
        maxChunkPerFace = 2;
        chunkDetail = 1;
        radius = 1;
        curve = new AnimationCurve();
    }

    private void OnGUI()
    {
        TopBar();
        scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, true);
        EditorGUI.indentLevel = 0;
        General();
        Noise();
        Climate();
        GUILayout.EndScrollView();
    }

    void TopBar()
    {
        r = EditorGUILayout.GetControlRect(false, 18);
        EditorGUI.LabelField(r, "Name");
        r.x += 50;
        r.width = (r.width / 2) - 60;
        planetName = EditorGUI.TextField(r, planetName);
        r.x += r.width + 10;
        r.width += 60;
        if (GUI.Button(r, "Generate Planet"))
        {
            if (!CheckDetailValidity())
                Debug.Log("Chunks per face don't match");
            else
            {
                PlanetsManager p = GameObject.FindObjectOfType<PlanetsManager>();
                if (p == null)
                {
                    Debug.LogError("Missing Planet Manager, create manager before planets");
                    return;
                }
                GameObject g;
                if (planetName == "")
                    g = new GameObject("New Planet", typeof(TerrainManager));
                else
                    g = new GameObject(planetName, typeof(TerrainManager));
                TerrainManager t = g.GetComponent<TerrainManager>();
                t.planetData = new TerrainInfo(radius, minChunkPerFace, maxChunkPerFace, chunkDetail, maxHeight, isMarchingCube, 
                    settings, humidityCount, humidityMove, temperatureGrad, humidityGrad, biomeQuantity, instantiateTrees,
                    chooseBiomes, biomeList.ToArray(), useCurve, curve);
                t.SetValues(p);
                g.AddComponent<PlanetaryBody>();
            }
        }
    }

    void General()
    {
        EditorGUILayout.LabelField("General");
        EditorGUI.indentLevel++;
        SetLabel("Radius");
        radius = EditorGUI.FloatField(r, radius);

        SetLabel("Height");
        maxHeight = EditorGUI.IntField(r, maxHeight);

        SetLabel("Mesh Algorithm: ");
        r.width = r.width / 2;
        if (isMarchingCube)
            EditorGUI.LabelField(r, new GUIContent("Marching Cubes"));
        else
            EditorGUI.LabelField(r, new GUIContent("Dual Contouring"));
        r.x += r.width;
        if (GUI.Button(r, new GUIContent("Change")))
            isMarchingCube = !isMarchingCube;
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
    }

    void Noise()
    {
        EditorGUILayout.LabelField("Noise");
        EditorGUI.indentLevel++;
        noiseList = EditorGUILayout.Foldout(noiseList, "Noise List");
        if (noiseList)
        {
            EditorGUI.indentLevel++;
            SetLabel("List Size");
            listSize = EditorGUI.IntField(r, listSize);
            if (listSize < 3)
                listSize = 3;
            while (settings.Count < listSize)
                AddNoiseLayer();

            while (settings.Count > listSize)
            {
                settings.RemoveAt(settings.Count - 1);
                settingsOpen.RemoveAt(settings.Count - 1);
            }

            for (int i = 0; i < listSize; i++)
            {
                settingsOpen[i] = EditorGUILayout.Foldout(settingsOpen[i], "Layer " + i);
                if (settingsOpen[i])
                {
                    EditorGUI.indentLevel++;
                    settings[i].strength = EditorGUILayout.FloatField("Strength", settings[i].strength);
                    settings[i].scale = EditorGUILayout.FloatField("Scale", settings[i].scale);
                    SetLabel("Offset");
                    if (GUI.Button(r, new GUIContent("Randomize")))
                    {
                        settings[i].centre.x = UnityEngine.Random.Range(-1000.0f, 1000.0f);
                        settings[i].centre.y = UnityEngine.Random.Range(-1000.0f, 1000.0f);
                        settings[i].centre.z = UnityEngine.Random.Range(-1000.0f, 1000.0f);
                    }
                    settings[i].centre = EditorGUILayout.Vector3Field("", settings[i].centre);
                    EditorGUI.indentLevel--;
                }
            }

            EditorGUI.indentLevel--;
        }

        EditorGUI.indentLevel--;
    }

    void Climate()
    {
        EditorGUILayout.LabelField("Climate");
        EditorGUI.indentLevel++;
        SetLabel("Definition");
        humidityCount = EditorGUI.IntField(r, humidityCount);

        SetLabel("Humidity Move");
        humidityMove = EditorGUI.FloatField(r, humidityMove);
        biomeQuantity = EditorGUILayout.IntSlider(new GUIContent("Number of biomes"), biomeQuantity, 1, 9);
        SetLabel("Choose Biomes");
        chooseBiomes = EditorGUI.Toggle(r, chooseBiomes);
        if (chooseBiomes)
        {
            biomeListOpen = EditorGUILayout.Foldout(biomeListOpen, "Biome List");
            if (biomeListOpen)
            {
                EditorGUI.indentLevel++;
                if (biomeList == null)
                    biomeList = new List<int>();

                while (biomeList.Count < biomeQuantity)
                    biomeList.Add(biomeList.Count);
                while(biomeList.Count > biomeQuantity)
                    biomeList.RemoveAt(biomeList.Count - 1);

                for (int i = 0; i < biomeList.Count; i++)
                    biomeList[i] = EditorGUILayout.IntSlider(new GUIContent("Biome " + i), biomeList[i], 0, 8);
                EditorGUI.indentLevel--;
            }
        }

        SetLabel("Instantiate Trees");
        instantiateTrees = EditorGUI.Toggle(r, instantiateTrees);

        SetLabel("Use Temperature Curve");
        useCurve = EditorGUI.Toggle(r, useCurve);
        if(useCurve)
            curve = EditorGUILayout.CurveField(curve);

        temperatureGrad = EditorGUILayout.GradientField(new GUIContent("Temperature Gradient"), temperatureGrad);
        humidityGrad = EditorGUILayout.GradientField(new GUIContent("Humidity Gradient"), humidityGrad);
        EditorGUI.indentLevel--;
    }

    void AddNoiseLayer()
    {
        settings.Add(new NoiseSettings());
        settingsOpen.Add(false);
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

#endif