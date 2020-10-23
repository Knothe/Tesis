using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class TerrainManager : MonoBehaviour
{
    [SerializeField]
    public TerrainInfo planetData;
    public Material defaultMaterial;
    Face[] faces = new Face[6];

    private void OnValidate()
    {
        if (planetData.settings.Count < 3)
        {
            Debug.Log("Minimum Size of 3");
            for (int i = planetData.settings.Count; i < 3; i++)
                planetData.settings.Add(new NoiseSettings());
        }

        if (!planetData.CheckChunks())
            Debug.LogError("Chunks (" + planetData.minChunkPerFace + ", " + planetData.maxChunkPerFace + ") don't coincide");

    }

    public void UpdateTerrain()
    {

    }

    public void GenerateTerrain()
    {
        Debug.Log(Time.realtimeSinceStartup);
        DeleteAllChilds();
        planetData.InstantiateNoise();
        planetData.SetTerrainManager(this);
        for(int i = 0; i < 6; i++)
        {
            faces[i] = new Face(i, planetData, gameObject.transform);
            faces[i].GenerateChunks(defaultMaterial);
        }

        for (int i = 0; i < 6; i++)
            faces[i].GenerateMesh();    

        Debug.Log(Time.realtimeSinceStartup);
    }

    void DeleteAllChilds()
    {
        GameObject[] tempArray = new GameObject[transform.childCount];

        for (int i = 0; i < tempArray.Length; i++)
        {
            tempArray[i] = transform.GetChild(i).gameObject;
        }

        foreach (var child in tempArray)
        {
            DestroyImmediate(child);
        }
    }

}

public static class TerrainManagerData
{
    // c0 = axisA
    // c1 = axisB
    // c2 = Up
    public static readonly float3x3[] dir = {
        new float3x3(new float3(0, 0, 1), new float3(0, 1, 0), new float3(1, 0, 0)),
        new float3x3(new float3(-1, 0, 0), new float3(0, 1, 0), new float3(0, 0, 1)),
        new float3x3(new float3(0, 0, -1), new float3(0, 1, 0), new float3(-1, 0, 0)),
        new float3x3(new float3(1, 0, 0), new float3(0, 1, 0), new float3(0, 0, -1)),
        new float3x3(new float3(1, 0, 0), new float3(0, 0, 1), new float3(0, 1, 0)),
        new float3x3(new float3(1, 0, 0), new float3(0, 0, -1), new float3(0, -1, 0))
    };

    public static readonly int3[] dirMult =
    {
        new int3(1, 1, 1),
        new int3(-1, 1, 1),
        new int3(-1, 1, -1),
        new int3(1, 1, -1),
        new int3(1, 1, 1),
        new int3(1, -1, -1)
    };

}
