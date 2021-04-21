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
    }


}

/// <summary>
/// Stores a biome colors and their probabilities
/// </summary>
[Serializable]
public class BiomeC
{
    public Color[] colors = new Color[3];
    public float[] limits = new float[3];

    public BiomeC()
    {

    }
}
