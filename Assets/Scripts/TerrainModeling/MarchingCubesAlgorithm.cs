using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class MarchingCubesAlgorithm : Algorithm
{
    List<int> triangles;
    MarchingCell cell;
    Dictionary<int3x2, int> vertexDictionary;

    public MarchingCubesAlgorithm(TerrainInfo t, int a, int l) : base(t, a, l)
    {
        vertexList = new List<Vector3>();
        triangles = new List<int>();
        vertexDictionary = new Dictionary<int3x2, int>();
        cell = new MarchingCell(TerrainManagerData.dir[axisID], level, terrain);
    }

    public override bool GenerateVoxelData(float3 center)
    {
        float d = (terrain.maxResolution * terrain.reescaleValues[terrain.levelsOfDetail - 1 - level]) / terrain.chunkDetail;
        //float d = ((terrain.planetRadius * 2) / terrain.minChunkPerFace) / terrain.chunkDetail; // Modificar
        float3 start = center;
        float3 temp;
        for (int3 t = int3.zero; t.x < terrain.chunkDetail; t.x++)
        {
            for (t.z = 0; t.z < terrain.chunkDetail; t.z++)
            {
                temp = start + (t.x * d * TerrainManagerData.dir[axisID].c0) + ((terrain.chunkDetail - 1) * TerrainManagerData.dir[axisID].c2 * d) + (t.z * d * TerrainManagerData.dir[axisID].c1);
                cell.SetValues(t, temp, d, terrain.noiseOffset);
                if (cell.index == 255)
                    continue;
                for (t.y = 0; t.y < terrain.chunkDetail; t.y++)
                {
                    temp = start + (t.x * d * TerrainManagerData.dir[axisID].c0) + (t.y * TerrainManagerData.dir[axisID].c2 * d) + (t.z * d * TerrainManagerData.dir[axisID].c1);
                    cell.SetValues(t, temp, d, terrain.noiseOffset);
                    if (cell.index == 0)
                        break;
                    ApplyMarchingCube();
                }
            }
        }
        return triangles.Count > 0;
    }

    public override Mesh GenerateMesh(float3 center)
    {
        if (terrain.chunkDetail <= 0) return null;
        return GenerateBasicTerrain(center);
    }

    Mesh GenerateBasicTerrain(float3 center)
    {
        Mesh m = new Mesh();
        m.vertices = SquareToCircle(center).ToArray();
        //m.vertices = vertexList.ToArray();
        m.triangles = triangles.ToArray();
        m.RecalculateNormals();
        return m;
    }

    List<Vector3> SquareToCircle(float3 center)
    {
        float height;
        Vector3 temp, newVertex;
        float3 up = TerrainManagerData.dir[axisID].c2;
        float3 ax = TerrainManagerData.dir[axisID].c0 + TerrainManagerData.dir[axisID].c1;
        ax.x = Mathf.Abs(ax.x);
        ax.y = Mathf.Abs(ax.y);
        ax.z = Mathf.Abs(ax.z);
        List<Vector3> newVertexList = new List<Vector3>();
        for (int i = 0; i < vertexList.Count; i++)
        {
            temp = vertexList[i] + (Vector3)center;
            height = (temp.x * up.x) + (temp.y * up.y) + (temp.z * up.z);
            temp = (temp * ax) + (up * terrain.planetRadius);
            newVertex = temp.normalized * height;
            newVertexList.Add(newVertex);
        }
        return newVertexList;
    }

    void ApplyMarchingCube()
    {
        if (MarchingTables.edges[cell.index] == 0) return;
        if ((1 & MarchingTables.edges[cell.index]) == 1)
            cell.vList[0] = AddPoint(0, 1);
        if ((2 & MarchingTables.edges[cell.index]) == 2)
            cell.vList[1] = AddPoint(1, 2);
        if ((4 & MarchingTables.edges[cell.index]) == 4)
            cell.vList[2] = AddPoint(2, 3);
        if ((8 & MarchingTables.edges[cell.index]) == 8)
            cell.vList[3] = AddPoint(3, 0);
        if ((16 & MarchingTables.edges[cell.index]) == 16)
            cell.vList[4] = AddPoint(4, 5);
        if ((32 & MarchingTables.edges[cell.index]) == 32)
            cell.vList[5] = AddPoint(5, 6);
        if ((64 & MarchingTables.edges[cell.index]) == 64)
            cell.vList[6] = AddPoint(6, 7);
        if ((128 & MarchingTables.edges[cell.index]) == 128)
            cell.vList[7] = AddPoint(7, 4);
        if ((256 & MarchingTables.edges[cell.index]) == 256)
            cell.vList[8] = AddPoint(0, 4);
        if ((512 & MarchingTables.edges[cell.index]) == 512)
            cell.vList[9] = AddPoint(1, 5);
        if ((1024 & MarchingTables.edges[cell.index]) == 1024)
            cell.vList[10] = AddPoint(2, 6);
        if ((2048 & MarchingTables.edges[cell.index]) == 2048)
            cell.vList[11] = AddPoint(3, 7);
        for (int i = 0; MarchingTables.triTable[cell.index, i] != -1; i++)
            triangles.Add(cell.vList[MarchingTables.triTable[cell.index, i]]);
    }

    int AddPoint(int index1, int index2)
    {
        int3x2 temp;
        if (cell.pointValues[index1] > cell.pointValues[index2])
            temp = new int3x2(cell.relativeIndex[index1], cell.relativeIndex[index2]);
        else
            temp = new int3x2(cell.relativeIndex[index2], cell.relativeIndex[index1]);
        if (vertexDictionary.ContainsKey(temp))
            return vertexDictionary[temp];

        float3 v = VertexInterpolation(cell.pointList[index1], cell.pointList[index2], cell.pointValues[index1], cell.pointValues[index2]);
        vertexDictionary.Add(temp, vertexList.Count);
        vertexList.Add(v);
        return vertexList.Count - 1;
    }

    float3 VertexInterpolation(float3 p1, float3 p2, float v1, float v2)
    {
        float3 p = p2 - p1;
        float t = Mathf.Abs(v1) + Mathf.Abs(v2);
        p = p * (Mathf.Abs(v1) / t);
        return p1 + p;
    }

}