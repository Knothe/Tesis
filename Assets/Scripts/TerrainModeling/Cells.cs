using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public class Cells
{
    public int3[] relativeIndex = new int3[8];
    public float3[] pointList = new float3[8];
    public float[] pointValues = new float[8];
    public int[] vList = new int[12];
    public int index { get; protected set; }
    
    protected int3[] indexModifiers = new int3[8];
    protected float[] noiseValues = new float[8];

    protected int3x3 axis;
    protected int ax;
    protected int levelOfDetail;
    protected TerrainInfo terrain;
    protected int id;

    public Cells(int faceID, int l, TerrainInfo t)
    {
        id = faceID;
        axis = (int3x3)TerrainManagerData.dir[faceID];
        indexModifiers[0] = int3.zero;                      // Start
        indexModifiers[1] = axis.c0;                        // Right
        indexModifiers[2] = axis.c0 + axis.c1;              
        indexModifiers[3] = axis.c1;                        // Left
        indexModifiers[4] = axis.c2;                        // up
        indexModifiers[5] = axis.c0 + axis.c2;
        indexModifiers[6] = axis.c0 + axis.c1 + axis.c2;
        indexModifiers[7] = axis.c1 + axis.c2;

        ax = TerrainManagerData.axisIndex[faceID][2];
        levelOfDetail = l;
        terrain = t;
    }
}

public class MarchingCell : Cells
{
    public float3 position;

    public Dictionary<int3, float> posWeights;

    public MarchingCell(int a, int l, TerrainInfo t) : base(a, l, t)
    {
        
    }

    public void SetFirstValues(int3 p, float3 point, float s, float3 offset)
    {
        float3 noisePoint;
        position = point;
        p = (p.y * axis.c2) + (p.x * axis.c0) + (p.z * axis.c1);
        for (int i = 0; i < 8; i++)
        {
            pointList[i] = point + ((float3)indexModifiers[i] * s);
            relativeIndex[i] = p + indexModifiers[i];
            noisePoint = pointList[i];
            noisePoint[ax] = terrain.planetRadius * axis.c2[ax];
            noiseValues[i] = terrain.GetNoiseValue(noisePoint + offset, levelOfDetail);
            pointValues[i] = noiseValues[i] - (Mathf.Abs(pointList[i][ax]) - terrain.planetRadius);
            pointList[i] = (float3)relativeIndex[i] * s;
        }
        SetIndex();
    }

    public void SetValues(int3 p, float3 point, float s, float3 offset)
    {
        position = point;
        p = (p.y * axis.c2) + (p.x * axis.c0) + (p.z * axis.c1);
        for (int i = 0; i < 8; i++)
        {
            pointList[i] = point + ((float3)indexModifiers[i] * s);
            relativeIndex[i] = p + indexModifiers[i];
            pointValues[i] = noiseValues[i] - (Mathf.Abs(pointList[i][ax]) - terrain.planetRadius);
            pointList[i] = (float3)relativeIndex[i] * s;
        }
        SetIndex();
    }

    void SetIndex()
    {
        index = 0;
        if (pointValues[0] >= 0) index |= 1;
        if (pointValues[1] >= 0) index |= 2;
        if (pointValues[2] >= 0) index |= 4;
        if (pointValues[3] >= 0) index |= 8;
        if (pointValues[4] >= 0) index |= 16;
        if (pointValues[5] >= 0) index |= 32;
        if (pointValues[6] >= 0) index |= 64;
        if (pointValues[7] >= 0) index |= 128;
    }

}


public class DualCell : Cells
{
    float size;
    int3 pos;

    int3[] relativeIndexModifiers = new int3[8];

    public DualCell(int a, int l, TerrainInfo t, float s) : base(a, l, t)
    {
        size = s;
        relativeIndexModifiers[0] = new int3(0, 0, 0);   
        relativeIndexModifiers[1] = new int3(1, 0, 0);   
        relativeIndexModifiers[2] = new int3(1, 0, 1);   
        relativeIndexModifiers[3] = new int3(0, 0, 1);   
        relativeIndexModifiers[4] = new int3(0, 1, 0);   
        relativeIndexModifiers[5] = new int3(1, 1, 0);   
        relativeIndexModifiers[6] = new int3(1, 1, 1);   
        relativeIndexModifiers[7] = new int3(0, 1, 1);
    }

    public void SetFirstValues(int3 p, float3 point, float s, float3 offset)
    {
        float3 noisePoint;
        float3 temp;
        int3 t = p;
        p = (p.y * axis.c2) + (p.x * axis.c0) + (p.z * axis.c1);
        for (int i = 0; i < 8; i++)
        {
            pointList[i] = point + ((float3)indexModifiers[i] * s);
            relativeIndex[i] = t + relativeIndexModifiers[i];
            temp = p + indexModifiers[i];
            noisePoint = pointList[i];
            noisePoint[ax] = terrain.planetRadius * axis.c2[ax];
            noiseValues[i] = terrain.GetNoiseValue(noisePoint + offset, levelOfDetail);
            pointValues[i] = noiseValues[i] - (Mathf.Abs(pointList[i][ax]) - terrain.planetRadius);
            pointList[i] = temp * s;
        }
        size = s;
        SetIndex();
    }

    //pointList[i] = new float3(relativeIndex[i][TerrainManagerData.axisIndex[id][0]] * TerrainManagerData.dirMult[id][0],
    //    relativeIndex[i][TerrainManagerData.axisIndex[id][1]] * TerrainManagerData.dirMult[id][1],
    //    relativeIndex[i][TerrainManagerData.axisIndex[id][2]] * TerrainManagerData.dirMult[id][2]) * s;
        
    public void SetValues(int3 p, float3 point, float s, float3 offset)
    {
        float3 temp;
        int3 t = p;
        pos = p;
        p = (p.y * axis.c2) + (p.x * axis.c0) + (p.z * axis.c1);
        for (int i = 0; i < 8; i++)
        {
            pointList[i] = point + ((float3)indexModifiers[i] * s);
            relativeIndex[i] = t + relativeIndexModifiers[i];
            temp = p + indexModifiers[i];
            pointValues[i] = noiseValues[i] - (Mathf.Abs(pointList[i][ax]) - terrain.planetRadius);
            pointList[i] = temp * s;
        }
        SetIndex();
    }

    public int3 GetPosition()
    {
        return pos;
    }

    public void SetValues(int3 p, float x, float y, float z, float s, float3 offset, float yScale)
    {
        SetValues(p, new float3(x, y, z), s, offset);
    }

    public float3 GetPointNormal(float3 p, float3 offset, float yScale)
    {
        float3 n = new float3(0, 0, 0);
        float d;

        d = p.y - (terrain.GetNoiseValue(p.x + offset.x + (size / 2), offset.y, p.z + offset.z, levelOfDetail));
        n.x += d;
        d = p.y - (terrain.GetNoiseValue(p.x + offset.x - (size / 2), offset.y, p.z + offset.z, levelOfDetail));
        n.x -= d;
        d = p.y - (terrain.GetNoiseValue(p.x + offset.x, offset.y, p.z + offset.z + (size / 2), levelOfDetail));
        n.z += d;
        d = p.y - (terrain.GetNoiseValue(p.x + offset.x, offset.y, p.z + offset.z - (size / 2), levelOfDetail));
        n.z -= d;
        n.y = (n.x + n.z) / 2;
        n.y = Mathf.Abs(n.y);
        n = n / 3.0f;

        return n;
    }

    void SetIndex()
    {
        index = 0;
        if (pointValues[0] >= 0) index |= 1;
        if (pointValues[1] >= 0) index |= 2;
        if (pointValues[2] >= 0) index |= 4;
        if (pointValues[3] >= 0) index |= 8;
        if (pointValues[4] >= 0) index |= 16;
        if (pointValues[5] >= 0) index |= 32;
        if (pointValues[6] >= 0) index |= 64;
        if (pointValues[7] >= 0) index |= 128;
    }
}
