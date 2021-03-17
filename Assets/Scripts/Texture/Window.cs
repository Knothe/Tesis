#if (UNITY_EDITOR) 

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.Collections;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

public class TextureWindow : EditorWindow
{
    Color[] color1;
    Color[] color2;
    Color[] color3;

    float[] limitX;
    float[] limitY;
    float[] limitZ;
    int size;
    int l;
    int L;
    float extraValue;
    string fileName;
    Texture2D[] texture;
    float pos;
    Texture2D mainTexture;

    // 0 Selva Tropical
    // 1 Bosque Tropical
    // 2 Sabana
    // 3 Selva Templada
    // 4 Bosque Templado
    // 5 Herbazal
    // 6 Taiga
    // 7 Tundra
    // 8 Desierto
    // 9 Agua

    string[] biomeName =
    {
        "Selva Tropical",
        "Bosque Tropical",
        "Sabana",
        "Selva Templada",
        "Bosque Templado",
        "Herbazal",
        "Taiga",
        "Tundra",
        "Desierto",
        "Mar"
    };

    int[,] cellBiome = {
    { 7, 8, 8, 8},
    { 7, 5, 5, 2},
    { 7, 6, 4, 1},
    { 7, 6, 3, 0},
    { 9, 9, 9, 9}
    };

    [MenuItem("Window/TextureGenerator")]
    public static void ShowWindow()
    {
        GetWindow<TextureWindow>("Texture Generator");
    }

    private void Awake()
    {
        int quantity = 10;
        texture = new Texture2D[quantity];
        color1 = new Color[quantity];
        color2 = new Color[quantity];
        color3 = new Color[quantity];
        limitX = new float[quantity];
        limitY = new float[quantity];
        limitZ = new float[quantity];
        pos = 0;
        for (int i = 0; i < quantity; i++)
        {
            texture[i] = new Texture2D(256, 256, TextureFormat.RGBA32, true, true);
            color1[i] = Color.black; color2[i] = Color.black; color3[i] = Color.black;
        }
    }

    private void OnGUI()
    {
        EditorGUI.DrawPreviewTexture(new Rect(260, pos + 300, 255, 255), texture[0]);
        for(int i = 0; i < texture.Length; i++)
            TextureData(i);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Generate Main Texture"))
            GenerateMainTexture();
        if (GUILayout.Button("Generate Scriptable Object"))
            GenerateScriptableObject();
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        fileName = EditorGUILayout.TextField("File Name", fileName);
        size = EditorGUILayout.IntField("Size", size);
        GUILayout.EndHorizontal();
        pos = GUI.VerticalScrollbar(new Rect(position.width - 15, 0, 50, position.height), pos, 255, 0, -(255 * (texture.Length + 1)) + position.height - 25);
    }

    void SetExtraValue()
    {
        float v = Mathf.Clamp((size - 100) / 400.0f, 0, 1);
        Debug.Log(v);
        extraValue = Mathf.Lerp(.05f, .01f, v);
    }

    void GenerateScriptableObject()
    {
        BiomeColors c = ScriptableObject.CreateInstance<BiomeColors>();
        c.SetValues(color1, color2, color3, limitX, limitY, limitZ);
        if (fileName == "")
            fileName = "planetColors";
        AssetDatabase.CreateAsset(c, "Assets/" + fileName + ".asset");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = c;
    }

    void GenerateMainTexture()
    {
        mainTexture = new Texture2D(size * 4, size * 5, TextureFormat.RGBA32, true, true);
        l = (int)(size * .05f);
        L = l * 2;
        SetExtraValue();
        Debug.Log("(" + size + ", " + extraValue + ")");
        for (int i = 0; i < mainTexture.width; i++)
            for (int j = 0; j < mainTexture.height; j++)
                mainTexture.SetPixel(i, j, Color.black);

        for (int i = 0; i < 4; i++)
        {
            for(int j = 0; j < 5; j++)
            {
                SetArea(i, j, cellBiome[j, i]);
            }
        }

        mainTexture.Apply();
        SaveTexture();
        Debug.Log("Texture generated");
    }

    void PointSetting(int i, int j, ref int2 bE, ref int2 eE, ref int2 bI, ref int2 eI)
    {
        int2 startPoint = new int2(i * size, j * size);
        int2 finalPoint = new int2((i + 1) * size, (j + 1) * size);

        bE = new int2(startPoint.x - l, startPoint.y - l);
        eE = new int2(finalPoint.x + l, finalPoint.y + l);

        bI = new int2(startPoint.x + l, startPoint.y + l);
        eI = new int2(finalPoint.x - l, finalPoint.y - l);
    }

    void SetArea(int i, int j, int id)
    {
        int2 bExternal = int2.zero;
        int2 bInternal = int2.zero;
        int2 eExternal = int2.zero;
        int2 eInternal = int2.zero;

        PointSetting(i, j, ref bExternal, ref eExternal, ref bInternal, ref eInternal);

        Color c;
        float v;
        float a;
        for(int x = bExternal.x; x < eExternal.x; x++)
        {
            for(int y = bExternal.y; y < eExternal.y; y++)
            {
                if (!isInsideTexture(x, y))
                    continue;
                a = GetPointIntensity(x, y, bInternal, eInternal);
                v = UnityEngine.Random.Range(0.0f, limitZ[id]);
                if (v < limitX[id])
                    c = color1[id];
                else if (v < limitY[id])
                    c = color2[id];
                else
                    c = color3[id];
                c = (c * a) + mainTexture.GetPixel(x, y);
                mainTexture.SetPixel(x, y, c);
            }
        }
    }

    float GetPointIntensity(int x, int y, int2 start, int2 end)
    {
        bool inside = true;
        Vector2 length = Vector2.zero; 

        if (x < start.x)
        {
            length.x = start.x - x;
            inside = false;
        }
        else if (x >= end.x)
        {
            length.x = (x + 1) - end.x;
            inside = false;
        }

        if (y < start.y)
        {
            length.y = start.y - y;
            inside = false;
        }
        else if (y >= end.y)
        {
            length.y = (y + 1) - end.y;
            inside = false;
        }
        if (inside)
            return 1;

        return  1 - Mathf.Clamp(((length.magnitude) / L) - extraValue, 0, 1);
    }

    bool isInsideTexture(int x, int y)
    {
        return x >= 0 && y >= 0 && x < mainTexture.width && y < mainTexture.height;
    }

    void SaveTexture()
    {
        if (fileName == "")
            fileName = "planetTexture";
        byte[] bytes = mainTexture.EncodeToPNG();
        File.WriteAllBytes(Application.dataPath + "/" + fileName + ".png", bytes);
    }

    void TextureData(int id)
    {
        GUILayout.BeginArea(new Rect(0, pos + (255 * id) + 295, 255, 255));
        EditorGUILayout.LabelField(biomeName[id]);
        color1[id] = EditorGUILayout.ColorField("Color 1", color1[id]);
        color2[id] = EditorGUILayout.ColorField("Color 2", color2[id]);
        color3[id] = EditorGUILayout.ColorField("Color 3", color3[id]);

        limitX[id] = EditorGUILayout.FloatField("Limit X", limitX[id]);
        limitY[id] = EditorGUILayout.FloatField("Limit Y", limitY[id]);
        limitZ[id] = EditorGUILayout.FloatField("Limit Z", limitZ[id]);
        if (GUILayout.Button("Generate Texture"))
            GenerateTexture(id);
        GUILayout.EndArea();
        EditorGUI.DrawPreviewTexture(new Rect(260, pos + (255 * id) + 295, 270, 255), texture[id]);
    }

    void GenerateTexture(int id)
    {
        Color temp;
        float value;
        for (int i = 0; i < texture[id].height; i++)
        {
            for (int j = 0; j < texture[id].width; j++)
            {
                value = UnityEngine.Random.Range(0.0f, limitZ[id]);
                if (value < limitX[id])
                    temp = color1[id];
                else if (value < limitY[id])
                    temp = color2[id];
                else
                    temp = color3[id];
                texture[id].SetPixel(i, j, temp);
            }
        }
        texture[id].Apply();
    }

}

#endif