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
    int lodCount;

    // Noise
    float noiseBase;
    Vector2 noiseLimits = new Vector2();
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
        EditorGUI.LabelField(r, new GUIContent("Name", "Nombre del planeta a generar, se puede dejar vacío"));
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
                t.planetData = new TerrainInfo(radius, minChunkPerFace, maxChunkPerFace, lodCount, chunkDetail, maxHeight, isMarchingCube, 
                    settings, humidityCount, humidityMove, temperatureGrad, humidityGrad, biomeQuantity, instantiateTrees,
                    chooseBiomes, biomeList.ToArray(), useCurve, curve);
                t.SetValues(p);
                g.AddComponent<PlanetaryBody>();
                g.transform.position = new Vector3(0, 0, radius * 2);
            }
        }
    }

    void General()
    {
        EditorGUILayout.LabelField("General", "Elementos base del planeta");
        EditorGUI.indentLevel++;
        SetLabel("Radius", "Distancia del centro al punto donde el terreno pasa de ser mar a tierra");
        radius = EditorGUI.Slider(r, radius, 5, 6000);

        SetLabel("Height", "Altura y profundidad máxima del terreno");
        maxHeight = EditorGUI.IntSlider(r, maxHeight, (int)(radius/7), (int)(radius/4.5f));

        SetLabel("Mesh Algorithm: ", "Algoritmo de modelado del planeta");
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
        if (minChunkPerFace < 2)
            minChunkPerFace = 2;
        r.x = r.x + r.width - 20;
        EditorGUI.LabelField(r, new GUIContent("Max"));
        r.x += 40;
        EditorGUI.LabelField(r, maxChunkPerFace.ToString());

        SetLabel("Levels of Detail", "Cantidad de niveles de detalle");
        lodCount = EditorGUI.IntSlider(r, lodCount, 1, 6);
        SetChunksPerFace();

        SetLabel("Chunk Detail", "Subdivisiones del chunk");
        chunkDetail = EditorGUI.IntSlider(r, chunkDetail, 10, 35);
        EditorGUI.indentLevel--;
        EditorGUI.indentLevel--;
    }

    void Noise()
    {
        EditorGUILayout.LabelField("Noise", "Determinan la forma y distribución del terreno");
        EditorGUI.indentLevel++;
        noiseList = EditorGUILayout.Foldout(noiseList, new GUIContent("Noise List", "Layers de ruido"));
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

            noiseBase = 1.28f / radius;
            NoiseLayer(0, noiseBase - (noiseBase * .5f), noiseBase + (noiseBase * .5f));

            noiseBase = (1.28f / radius) * 2;
            noiseLimits.x = noiseBase - (noiseBase * .5f);
            noiseLimits.y = noiseBase + (noiseBase * .5f);
            for (int i = 1; i < listSize; i++)
                NoiseLayer(i, noiseLimits.x, noiseLimits.y);

            EditorGUI.indentLevel--;
        }

        EditorGUI.indentLevel--;
    }

    void NoiseLayer(int i, float min, float max)
    {
        settingsOpen[i] = EditorGUILayout.Foldout(settingsOpen[i], "Layer " + i);
        if (settingsOpen[i])
        {
            EditorGUI.indentLevel++;
            SetLabel("Strength" ,"Impacto a la altura del terreno, el valor de cada layer afecta");

            settings[i].strength = EditorGUI.FloatField(r, settings[i].strength);

            SetLabel("Scale", "Escala de la función de ruido");
            EditorGUI.LabelField(r, settings[i].scale.ToString());
            r = EditorGUILayout.GetControlRect(true, 18);
            r.width += 55;
            settings[i].scale = EditorGUI.Slider(r, settings[i].scale, min, max);

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

    void Climate()
    {
        EditorGUILayout.LabelField("Climate", "Determina la distribución de biomas en el planeta");
        EditorGUI.indentLevel++;
        SetLabel("Definition", "Precisión de la humedad, mientras mayor sea el valor, mayor precisión hay");
        humidityCount = EditorGUI.IntSlider(r, humidityCount, 15, 50);

        SetLabel("Humidity Move", "Distancia que recorre la humedad antes de desaparecer, el valor representa radios");
        humidityMove = EditorGUI.Slider(r, humidityMove, .3f, 1);

        biomeQuantity = EditorGUILayout.IntSlider(new GUIContent("Number of biomes", "Cantidad de biomas en el planeta"), biomeQuantity, 1, 9);
        SetLabel("Choose Biomes", "Elige los biomas que quieres que aparezcan en el planeta");
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

                EditorGUI.indentLevel++;
                for (int i = 0; i < biomeList.Count; i++)
                    biomeList[i] = EditorGUILayout.IntPopup(biomeList[i], TerrainInfoData.biomeN, 
                        new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8});
                EditorGUI.indentLevel--;
                EditorGUI.indentLevel--;
            }
        }

        SetLabel("Instantiate Trees");
        instantiateTrees = EditorGUI.Toggle(r, instantiateTrees);

        SetLabel("Use Temperature Curve", "Curva que define la temperatura del planeta \n" +
            "Eje x: Latitud del planeta donde 0 es el centro y 1 son los límites del planeta\n" +
            "Eje y: Temperatura donde el 0 es la mayor temperatura posible y 1 es la menor");
        useCurve = EditorGUI.Toggle(r, useCurve);
        if(useCurve)
            curve = EditorGUILayout.CurveField(curve);

        temperatureGrad = EditorGUILayout.GradientField(new GUIContent("Temperature Gradient", "Colores para mostrar la temperatura en el debug"), temperatureGrad);
        humidityGrad = EditorGUILayout.GradientField(new GUIContent("Humidity Gradient", "Colores para mostrar la humedad en el debug"), humidityGrad);
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

    void SetChunksPerFace()
    {
        maxChunkPerFace = minChunkPerFace;
        for (int i = 1; i < lodCount; i++)
            maxChunkPerFace *= 2;
    }

    void SetLabel(string name)
    {
        r = EditorGUILayout.GetControlRect(true, 18);
        r = EditorGUI.PrefixLabel(r, new GUIContent(name));
        r.x -= 15;
        r.width += 15;
    }

    void SetLabel(string name, string tooltip)
    {
        r = EditorGUILayout.GetControlRect(true, 18);
        r = EditorGUI.PrefixLabel(r, new GUIContent(name, tooltip));
        r.x -= 15;
        r.width += 15;
    }

}

#endif