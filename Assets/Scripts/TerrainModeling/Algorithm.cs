using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class Algorithm
{
    protected List<Vector3> vertexList;
    protected List<Color> colors;
    protected List<Vector2> uv;
    protected List<int> biome;
    public TerrainInfo terrain { protected set; get; }
    public int axisID { get; protected set; }
    protected int level;
    protected int childPos;
    public bool voxelDataGenerated { get; protected set; }

    public Algorithm(TerrainInfo t, int a, int l, int cp)
    {
        axisID = a;
        terrain = t;
        level = l;
        childPos = cp;
        voxelDataGenerated = false;
        colors = new List<Color>();
        uv = new List<Vector2>();
        biome = new List<int>();
    }

    public virtual bool GenerateVoxelData(float3 center)
    {
        return false;
    }

    public virtual Mesh GenerateMesh(float3 center, Node[] neighbors)
    {
        return null;
    }

    public virtual void getEdgeCubes(int3x2 v, ref List<int3> c, ref List<float4> p, int3 dif, int otherLOD, int otherAxisID, int vertices)
    {
        
    }
}
