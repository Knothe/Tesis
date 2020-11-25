using System.Collections;
using System.Collections.Generic;
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

    Texture2D[] texture;
    float pos;
    Texture2D mainTexture;

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
        mainTexture = new Texture2D(768, 768, TextureFormat.RGBA32, true, true);
    }

    private void OnGUI()
    {
        pos = GUI.VerticalScrollbar(new Rect(600, 0, 255, position.height), pos, 255, 0, -(255 * texture.Length) + position.height - 10);
        for(int i = 0; i < texture.Length; i++)
            TextureData(i);
        if (GUILayout.Button("Generate Main Texture"))
            GenerateMainTexture();
    }

    void GenerateMainTexture()
    {
        int id, xMod, yMod;
        for(int x = 0; x < 3; x++)
        {
            for (int y = 0; y < 3; y++)
            {
                id = (x * 3) + y;
                xMod = 255 * x;
                yMod = 255 * y;
                for (int i = 0; i < texture[id].height; i++)
                {
                    for (int j = 0; j < texture[id].width; j++)
                    {
                        mainTexture.SetPixel(i + xMod, j + yMod, texture[id].GetPixel(i, j));
                    }
                }
            }
        }
        mainTexture.Apply();
    }

    void TextureData(int id)
    {
        GUILayout.BeginArea(new Rect(0, pos + (255 * id), 255, 255));
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
        EditorGUI.DrawPreviewTexture(new Rect(260, pos + (255 * id), 255, 255), texture[id]);
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