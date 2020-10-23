using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class Algorithm
{
    protected List<Vector3> vertexList;
    public TerrainInfo terrain { protected set; get; }
    protected int axisID;
    protected int level;

    public Algorithm(TerrainInfo t, int a, int l)
    {
        axisID = a;
        terrain = t;
        level = l;
    }

    public virtual bool GenerateVoxelData(float3 center)
    {
        return false;
    }

    public virtual Mesh GenerateMesh(float3 center)
    {
        return null;
    }
}
