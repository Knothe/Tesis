using UnityEngine;
using System;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/Biome Colors", order = 1)]
public class BiomeColors : ScriptableObject
{
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
    public BiomeC[] biomeList = new BiomeC[10];

    public BiomeColors()
    {

    }

    public void SetValues(Color[] c1, Color[] c2, Color[] c3, float[] limitX, float[] limitY, float[] limitZ)
    {
        biomeList = new BiomeC[10];
        for (int i = 0; i < biomeList.Length; i++)
        {
            biomeList[i] = new BiomeC();
            biomeList[i].colors[0] = c1[i];
            biomeList[i].colors[1] = c2[i];
            biomeList[i].colors[2] = c3[i];

            biomeList[i].limits[0] = limitX[i];
            biomeList[i].limits[1] = limitY[i];
            biomeList[i].limits[2] = limitZ[i];
        }
    }

}

[Serializable]
public class BiomeC
{
    public Color[] colors = new Color[3];
    public float[] limits = new float[3];

    public BiomeC()
    {

    }
}
