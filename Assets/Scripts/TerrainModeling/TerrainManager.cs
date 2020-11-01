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
        float time = Time.realtimeSinceStartup;
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
        Debug.Log(Time.realtimeSinceStartup - time);
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

    public Node GetNode(int faceID, int myLevel, int3 myPos, int3 wantedPos)
    {
        if (faceID == 4 && myPos.x == 9 && myPos.y == 6 && myPos.z == -1)
            Debug.Log("Hola");
        int checkFaceID = faceID;
        if(wantedPos.x < 0)
        {
            wantedPos.x += planetData.maxChunkPerFace;
            checkFaceID = 1;
        } 
        else if(wantedPos.x >= planetData.maxChunkPerFace)
        {
            wantedPos.x -= planetData.maxChunkPerFace;
            checkFaceID = 0;
        }
        else if (wantedPos.y < 0)
        {
            wantedPos.y += planetData.maxChunkPerFace;
            checkFaceID = 3;
        }
        else if (wantedPos.y >= planetData.maxChunkPerFace)
        {
            wantedPos.y -= planetData.maxChunkPerFace;
            checkFaceID = 2;
        }
        else
            return faces[checkFaceID].GetNode(myLevel, myPos, wantedPos);

        return faces[TerrainManagerData.neighborFace[faceID][checkFaceID]].GetNode(myLevel, myPos, RotatePoint(CheckRotation(faceID, TerrainManagerData.neighborFace[faceID][checkFaceID]), wantedPos));
    }

    int CheckRotation(int originFace, int nextFace)
    {
        if(originFace == 0)
        {
            if (nextFace == 5 || nextFace == 4)
                return 3;
        } 
        else if (originFace == 1)
        {
            if (nextFace == 4)
                return 2;
        }
        else if (originFace == 2)
        {
            if (nextFace == 5 || nextFace == 4)
                return 1;
        }
        else if (originFace == 3)
        {
            if (nextFace == 5)
                return 2;
        }
        else if (originFace == 4)
        {
            return nextFace + 1;
        }
        else if (originFace == 5)
        {
            if (nextFace == 3)
                return 2;
            if (nextFace == 2)
                return 3;
            if (nextFace == 0)
                return 1;
        }
        return 0;
    }

    int3 RotatePoint(int rotation, int3 wantedPos)
    {
        int3 temp = wantedPos;
        
        if(rotation == 1)
        {
            temp.x = wantedPos.y;
            temp.y = planetData.maxChunkPerFace - 1 - wantedPos.x;
        } else if(rotation == 2)
        {
            temp.x = planetData.maxChunkPerFace - 1 - wantedPos.x;
            temp.y = planetData.maxChunkPerFace - 1 - wantedPos.y;
        } else if(rotation == 3)
        {
            temp.x = planetData.maxChunkPerFace - 1 - wantedPos.y;
            temp.y = wantedPos.x;
        }
        return temp;
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
        new float3x3(new float3(-1, 0, 0), new float3(0, 0, 1), new float3(0, -1, 0))
    };

    public static readonly int3[] axisIndex =
    {
        new int3(2, 1, 0),
        new int3(0, 1, 2),
        new int3(2, 1, 0),
        new int3(0, 1, 2),
        new int3(0, 2, 1),
        new int3(0, 2, 1)
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

    public static readonly int3[] neigborCells =
    {
        new int3(1, 0, 0),      // 0    Right
        new int3(0, 1, 0),      // 1    Front
        new int3(0, 0, 1),      // 2    Up
        new int3(-1, 0, 0),     // 3    Left
        new int3(0, -1, 0),     // 4    Back   
        new int3(0, 0, -1),     // 5    Down

        new int3(1, 1, 0),      // 6    Right   Front
        new int3(-1, 1, 0),     // 7    Left    Front
        new int3(1, -1, 0),     // 8    Right   Back
        new int3(-1, -1, 0),    // 9    Left    Back

        new int3(1, 0, 1),      // 10   Right   Up
        new int3(-1, 0, 1),     // 11   Left    Up
        new int3(1, 0, -1),     // 12   Right   Down
        new int3(-1, 0, -1),    // 13   Left    Down

        new int3(0, 1, 1),      // 14   Front   Up
        new int3(0, -1, 1),     // 15   Back    Up
        new int3(0, 1, -1),     // 16   Front   Down
        new int3(0, -1, -1)     // 17   Back    Down
    };

    // Right
    // Left
    // Up
    // Down
    public static readonly int4[] neighborFace =
    {
        new int4(1, 3, 4, 5),
        new int4(2, 0, 4, 5),
        new int4(3, 1, 4, 5),
        new int4(0, 2, 4, 5),
        new int4(0, 2, 1, 3),
        new int4(2, 0, 1, 3)
    };



}
