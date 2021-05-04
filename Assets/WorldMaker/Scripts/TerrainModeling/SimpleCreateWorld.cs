using UnityEngine;
using UnityEditor;
using Unity.Mathematics;
using System.Collections.Generic;

public class SimpleCreateWorld : EditorWindow
{
    // General
    float radius;
    bool isMarchingCube;
    int minChunkPerFace;
    int maxChunkPerFace;
    int chunkDetail;
    int lodCount;

    // Climate
    float humidityMove;
    int biomeQuantity;

    Vector2 scrollPosition = Vector2.zero;
    string planetName = "";
    Rect r = Rect.zero;

    [MenuItem("Window/World Generator/SimpleWG")]
    public static void ShowWindow()
    {
        GetWindow<SimpleCreateWorld>("Simple World Generator");
    }

    private void Awake()
    {
        minChunkPerFace = 1;
        maxChunkPerFace = 2;
        radius = 1;
    }

    private void OnGUI()
    {
        TopBar();
        scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, true);
        EditorGUI.indentLevel = 0;
        General();
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
            t.planetData = new TerrainInfo(radius, isMarchingCube, minChunkPerFace, maxChunkPerFace, chunkDetail, humidityMove, biomeQuantity);
            t.SetValues(p);
            g.AddComponent<PlanetaryBody>();
        }
    }

    void General()
    {
        EditorGUILayout.LabelField("General");
        EditorGUI.indentLevel++;
        SetLabel("Radius");
        radius = EditorGUI.Slider(r, radius, 5, 6000);

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
        r.x += 40;
        EditorGUI.LabelField(r, maxChunkPerFace.ToString());

        SetLabel("Levels of Detail");
        lodCount = EditorGUI.IntSlider(r, lodCount, 1, 6);
        SetChunksPerFace();

        SetLabel("Chunk Detail");
        chunkDetail = EditorGUI.IntSlider(r, chunkDetail, 10, 35);
        EditorGUI.indentLevel--;
        EditorGUI.indentLevel--;
    }

    void Climate()
    {
        EditorGUILayout.LabelField("Climate");
        EditorGUI.indentLevel++;
        SetLabel("Humidity Move");
        humidityMove = EditorGUI.Slider(r, humidityMove, .3f, 1.0f);
        biomeQuantity = EditorGUILayout.IntSlider(new GUIContent("Number of biomes"), biomeQuantity, 1, 9);
        EditorGUI.indentLevel--;
    }

    void SetChunksPerFace()
    {
        maxChunkPerFace = minChunkPerFace;
        for(int i = 1; i < lodCount; i++)
            maxChunkPerFace *= 2;
    }

    void SetLabel(string name)
    {
        r = EditorGUILayout.GetControlRect(true, 18);
        r = EditorGUI.PrefixLabel(r, new GUIContent(name));
        r.x -= 15;
        r.width += 15;
    }

}
