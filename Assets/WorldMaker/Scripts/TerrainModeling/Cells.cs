using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

/// <summary>
/// Contain the funcionatity of a cell in an algorithm.
/// This classes manage cells id and their different values
/// </summary>
public class Cells
{
    /// <summary> Vertices integer position in chunk </summary>
    public int3[] relativeIndex = new int3[8];
    /// <summary> Vertices float position in face </summary>
    public float3[] pointList = new float3[8];
    /// <summary> Vertices terrain value </summary>
    public float[] pointValues = new float[8];
    /// <summary>
    /// Generated vertices list.
    /// Each index represents an edge in the cell, so it is imporant to keep it of size 12
    /// </summary>
    public int[] vList = new int[12];
    /// <summary> Kind of combination for de cell </summary>
    public int index { get; protected set; }
    
    /// <summary> Modifiers to calculate the relative index in the cell </summary>
    protected int3[] indexModifiers = new int3[8];
    /// <summary> Noise value of each cell </summary>
    protected float[] noiseValues = new float[8];

    /// <summary> Axis of the face </summary>
    protected int3x3 axis;
    /// <summary> Index of the Z axis in the face </summary>
    protected int ax;
    /// <summary> Level of detail value </summary>
    protected int levelOfDetail;
    /// <summary> Terrain info of the planet </summary>
    protected TerrainInfo terrain;

    public Cells(int faceID, int l, TerrainInfo t)
    {
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

    /// <summary>
    /// Sets initial noise values for column of voxels and calculates point values
    /// </summary>
    /// <param name="p">Integer index of the voxel point</param>
    /// <param name="point">Point relative to the planet</param>
    /// <param name="s">Size of the voxel data</param>
    public virtual void SetFirstValues(int3 p, float3 point, float s)
    {

    }

    /// <summary>
    /// Calculates point values with previously calculated noise values
    /// </summary>
    /// <param name="p">Integer index of the voxel point</param>
    /// <param name="point">Point relative to the planet</param>
    /// <param name="s">Size of the voxel data</param>
    public virtual void SetValues(int3 p, float3 point, float s)
    {

    }

    /// <summary>
    /// Sets the type of cell this is, by indicating wich vertices are terrain are wich are air
    /// </summary>
    protected void SetIndex()
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

/// <summary> Cells for the Marching cube Algorithm </summary>
public class MarchingCell : Cells
{
    public MarchingCell(int a, int l, TerrainInfo t) : base(a, l, t)
    {
        
    }

    public override void SetFirstValues(int3 p, float3 point, float s)
    {
        float3 noisePoint;
        p = (p.y * axis.c2) + (p.x * axis.c0) + (p.z * axis.c1);
        for (int i = 0; i < 8; i++)
        {
            pointList[i] = point + ((float3)indexModifiers[i] * s);
            relativeIndex[i] = p + indexModifiers[i];
            noisePoint = pointList[i];
            noisePoint[ax] = terrain.planetRadius * axis.c2[ax];
            noiseValues[i] = terrain.GetNoiseValue(noisePoint, levelOfDetail);
            pointValues[i] = noiseValues[i] - (Mathf.Abs(pointList[i][ax]) - terrain.planetRadius);
            pointList[i] = (float3)relativeIndex[i] * s;
        }
        SetIndex();
    }

    public override void SetValues(int3 p, float3 point, float s)
    {
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
}

/// <summary>
/// Cells for the Dual Contouring algorithm
/// </summary>
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

    public override void SetFirstValues(int3 p, float3 point, float s)
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
            noiseValues[i] = terrain.GetNoiseValue(noisePoint, levelOfDetail);
            pointValues[i] = noiseValues[i] - (Mathf.Abs(pointList[i][ax]) - terrain.planetRadius);
            pointList[i] = temp * s;
        }
        size = s;
        SetIndex();
    }

    public override void SetValues(int3 p, float3 point, float s)
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

    /// <returns>Integer index of the voxel cell</returns>
    public int3 GetPosition()
    {
        return pos;
    }

    /// <summary>
    /// Calculates vertex normal for hermite data
    /// </summary>
    /// <param name="p">Point to calculate</param>
    /// <returns>Normal of vertex</returns>
    public float3 GetPointNormal(float3 p)
    {
        float3 n = new float3(0, 0, 0);
        float d;

        d = p.y - (terrain.GetNoiseValue(p.x + (size / 2), 0, p.z, levelOfDetail));
        n.x += d;
        d = p.y - (terrain.GetNoiseValue(p.x - (size / 2), 0, p.z, levelOfDetail));
        n.x -= d;
        d = p.y - (terrain.GetNoiseValue(p.x, 0, p.z + (size / 2), levelOfDetail));
        n.z += d;
        d = p.y - (terrain.GetNoiseValue(p.x, 0, p.z - (size / 2), levelOfDetail));
        n.z -= d;
        n.y = (n.x + n.z) / 2;
        n.y = Mathf.Abs(n.y);
        n = n / 3.0f;
        return n;
    }
}
