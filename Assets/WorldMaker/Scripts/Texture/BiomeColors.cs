using UnityEngine;
using System;

/// <summary>
/// Stores the different colors and probabilities a polygon in that biome has of appearing
/// </summary>
[Serializable]
public class BiomeColors
{
    public Gradient[] biomeList;

    public BiomeColors()
    {
        biomeList = new Gradient[10];
        for (int i = 0; i < 10; i++)
            biomeList[i] = new Gradient();
    }

    public void OnValidate()
    {
        foreach(Gradient g in biomeList)
        {
            g.mode = GradientMode.Fixed;
        }
    }
}
