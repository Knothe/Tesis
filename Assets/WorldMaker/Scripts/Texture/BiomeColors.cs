using UnityEngine;
using System;

/// <summary>
/// Stores the different colors and probabilities a polygon in that biome has of appearing
/// </summary>
[Serializable]
public class BiomeColors
{
    public BiomeC[] biomeList;

    public BiomeColors()
    {
        biomeList = new BiomeC[10];
        SetColorWithHex("3DD43D", 0); // Tropical Rainforest
        SetColorWithHex("98D93E", 1); // Tropical Seasonal Forest
        SetColorWithHex("980C0C", 2); // Savannah
        SetColorWithHex("B42A78", 3); // Temperate Rainforest
        SetColorWithHex("44197B", 4); // Temperate Forest
        SetColorWithHex("1B7648", 5); // Grassland 
        SetColorWithHex("BE5A15", 6); // Boreal Forest
        SetColorWithHex("939393", 7); // Tundra
        SetColorWithHex("D4C733", 8); // Dessert
        SetColorWithHex("2C7982", 9); // Sea
    }

    void SetColorWithHex(string hex, int id)
    {
        float v1 = HexToVal(hex[0].ToString() + hex[1].ToString()) / 255;
        float v2 = HexToVal(hex[2].ToString() + hex[3].ToString()) / 255;
        float v3 = HexToVal(hex[4].ToString() + hex[5].ToString()) / 255;
        biomeList[id] = new BiomeC(new Color(v1, v2, v3));
    }

    float HexToVal(string value)
    {
        float v = 0;
        for(int i = 0; i < value.Length; i++)
            v += SingleHexVal(value[value.Length - i - 1]) * (Mathf.Pow(16, i));
        return v;
    }

    int SingleHexVal(char c)
    {
        char[] hexVals = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };
        for (int i = 0; i < hexVals.Length; i++)
            if (c == hexVals[i])
                return i;
        return 0;
    }
}

/// <summary>
/// Stores a biome colors and their probabilities
/// </summary>
[Serializable]
public class BiomeC
{
    public Color[] colors;
    public float[] limits;

    public BiomeC(Color c)
    {
        colors = new Color[]{ c };
        limits = new float[1];
    }
}
