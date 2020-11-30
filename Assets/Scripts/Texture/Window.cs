using System.Collections;
using System.Collections.Generic;
using System.IO;
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
    string fileName;
    Texture2D[] texture;
    float pos;
    Texture2D mainTexture;

    // 1 Selva Tropical
    // 2 Bosque Tropical
    // 3 Sabana
    // 4 Selva Templada
    // 5 Bosque Templado
    // 6 Herbazal
    // 7 Taiga
    // 8 Tundra
    // 9 Desierto

    int[][] cellBiome = {
        new int[]{ 0, 0, 0, 0, 0, 0, 9, 9, 0, 0, 0},
        new int[]{ 0, 0, 0, 0, 0,-3, 9, 9,-8, 8, 8},
        new int[]{ 0, 0, 0, 3, 3,-3, 9, 9,-8, 8, 8},
        new int[]{ 0, 5,-3, 3, 3,-3,-6,-6,-8, 8, 8},
        new int[]{ 5, 5,-3, 3, 3,-3,-6,-6,-8, 8, 8},
        new int[]{ 5, 5,-3, 3, 3,-3, 6, 6,-8, 8, 8},
        new int[]{ 5, 5,-3,-3,-3,-3, 6, 6,-8,-8,-8},
        new int[]{ 5, 5,-2,-3,-3,-2, 6, 6,-7,-8,-8},
        new int[]{ 5, 5,-2, 2, 2,-2, 6, 6,-7, 7, 7},
        new int[]{-5,-5,-2, 2, 2,-2,-6,-6,-7, 7, 7},
        new int[]{-5,-5,-2, 2, 2,-2,-6,-6,-7, 7, 7},
        new int[]{ 4, 4,-2, 2, 2,-2, 5, 5,-7, 7, 7},
        new int[]{ 4, 4,-2,-2,-2,-2, 5, 5,-7,-4,-4},
        new int[]{ 4, 4,-1,-2,-2,-1, 5, 5,-4,-4,-4},
        new int[]{ 4, 4,-1, 1, 1,-1, 5, 5,-4, 4, 4},
        new int[]{ 0, 4,-1, 1, 1,-1, 0, 0,-4, 4, 4},
        new int[]{ 0, 0, 0, 1, 1, 0, 0, 0, 0, 4, 4}
    };

    int[][] cellBiomeAdd = {
        new int[]{ 0, 0, 0, 0, 0, 0, 9, 9, 0, 0, 0},
        new int[]{ 0, 0, 0, 0, 0,-9, 9, 9,-9, 8, 8},
        new int[]{ 0, 0, 0, 3, 3,-9, 9, 9,-9, 8, 8},
        new int[]{ 0, 5,-5, 3, 3,-9,-9,-9,-9, 8, 8},
        new int[]{ 5, 5,-5, 3, 3,-6,-9,-9,-6, 8, 8},
        new int[]{ 5, 5,-5, 3, 3,-6, 6, 6,-6, 8, 8},
        new int[]{ 5, 5,-5,-2,-2,-6, 6, 6,-6,-7,-7},
        new int[]{ 5, 5,-5,-2,-2,-6, 6, 6,-6,-7,-7},
        new int[]{ 5, 5,-5, 2, 2,-6, 6, 6,-6, 7, 7},
        new int[]{-4,-4,-5, 2, 2,-6,-5,-5,-6, 7, 7},
        new int[]{-4,-4,-4, 2, 2,-5,-5,-5,-5, 7, 7},
        new int[]{ 4, 4,-4, 2, 2,-5, 5, 5,-5, 7, 7},
        new int[]{ 4, 4,-4,-1,-1,-5, 5, 5,-5,-7,-7},
        new int[]{ 4, 4,-4,-1,-1,-5, 5, 5,-5,-7,-7},
        new int[]{ 4, 4,-4, 1, 1,-5, 5, 5,-5, 4, 4},
        new int[]{ 0, 4,-4, 1, 1,-5, 0, 0,-5, 4, 4},
        new int[]{ 0, 0, 0, 1, 1, 0, 0, 0, 0, 4, 4}
    };

    [MenuItem("Window/TextureGenerator")]
    public static void ShowWindow()
    {
        GetWindow<TextureWindow>("Texture Generator");
    }

    private void Awake()
    {
        int quantity = 9;
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
        if (GUILayout.Button("Generate Main Texture"))
            GenerateMainTexture();
        GUILayout.BeginHorizontal();
        fileName = EditorGUILayout.TextField("File Name", fileName);
        size = EditorGUILayout.IntField("Size", size);
        GUILayout.EndHorizontal();
        pos = GUI.VerticalScrollbar(new Rect(position.width - 15, 0, 50, position.height), pos, 255, 0, -(255 * (texture.Length + 1)) + position.height - 25);
    }

    void GenerateMainTexture()
    {
        int id, xMod, yMod;
        mainTexture = new Texture2D(size * 11, size * 17, TextureFormat.RGBA32, true, true);
        for (int i = 0; i < 11; i++)
        {
            for(int j = 0; j < 17; j++)
            {
                if (cellBiome[j][i] > 0)
                    SetArea(i, j, cellBiome[j][i] - 1);
                else if (cellBiome[j][i] < 0)
                    SetMixedArea(i, j, Mathf.Abs(cellBiome[j][i]) - 1, Mathf.Abs(cellBiomeAdd[j][i]) - 1);
            }
        }


        //for(int x = 0; x < 3; x++)
        //{
        //    for (int y = 0; y < 3; y++)
        //    {
        //        id = (x * 3) + y;
        //        xMod = 255 * x;
        //        yMod = 255 * y;
        //        for (int i = 0; i < texture[id].height; i++)
        //        {
        //            for (int j = 0; j < texture[id].width; j++)
        //            {
        //                mainTexture.SetPixel(i + xMod, j + yMod, texture[id].GetPixel(i, j));
        //            }
        //        }
        //    }
        //}
        mainTexture.Apply();
        SaveTexture();
    }

    void SetArea(int i, int j, int id)
    {
        int startX = i * size;
        int startY = j * size;
        Color c;
        float v;
        for(int x = 0; x < size; x++)
        {
            for(int y = 0; y < size; y++)
            {
                v = Random.Range(0.0f, limitZ[id]);
                if (v < limitX[id])
                    c = color1[id];
                else if (v < limitY[id])
                    c = color2[id];
                else
                    c = color3[id];
                mainTexture.SetPixel(startX + x, startY + y, c);
            }
        }
    }

    void SetMixedArea(int i, int j, int id1, int id2)
    {
        int startX = i * size;
        int startY = j * size;
        Color c;
        float v;
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                if(Random.Range(0.0f, 1.0f) >= .5f)
                {
                    v = Random.Range(0.0f, limitZ[id1]);
                    if (v < limitX[id1])
                        c = color1[id1];
                    else if (v < limitY[id1])
                        c = color2[id1];
                    else
                        c = color3[id1];
                }
                else
                {
                    v = Random.Range(0.0f, limitZ[id2]);
                    if (v < limitX[id2])
                        c = color1[id2];
                    else if (v < limitY[id2])
                        c = color2[id2];
                    else
                        c = color3[id2];
                }
                
                mainTexture.SetPixel(startX + x, startY + y, c);
            }
        }
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
        EditorGUILayout.LabelField("Biome 1");
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
                value = Random.Range(0.0f, limitZ[id]);
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