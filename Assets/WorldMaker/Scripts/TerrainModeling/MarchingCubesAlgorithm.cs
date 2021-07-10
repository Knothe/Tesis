using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

/// <summary>
/// Encapsulates the Marching Cube algorithm
/// </summary>
public class MarchingCubesAlgorithm : Algorithm
{
    /// <summary>
    /// Triangles of generated Mesh
    /// Represents the index of the veretx
    /// </summary>
    List<int> triangles;

    MarchingCell cell;
    /// <summary> Represents vertex by its edge and index position in vertexList </summary>
    Dictionary<int3x2, int> vertexDictionary { get; set; }

    public MarchingCubesAlgorithm(TerrainInfo t, int a, int l) : base(t, a, l)
    {
        vertexList = new List<Vector3>();
        triangles = new List<int>();
        vertexDictionary = new Dictionary<int3x2, int>();
        cell = new MarchingCell(axisID, level, terrain);
    }

    public override bool GenerateVoxelData(float3 start)
    {
        if (voxelDataGenerated)
            return triangles.Count > 0;
        float d = (terrain.maxResolution * terrain.reescaleValues[terrain.levelsOfDetail - 1 - level]) / terrain.chunkDetail;
        float3 temp;
        for (int3 t = int3.zero; t.x < terrain.chunkDetail; t.x++)
        {
            for (t.z = 0; t.z < terrain.chunkDetail; t.z++)
            {
                temp = start + (t.x * d * TerrainManagerData.dir[axisID].c0) +
                    ((terrain.chunkDetail - 1) * TerrainManagerData.dir[axisID].c2 * d) +
                    (t.z * d * TerrainManagerData.dir[axisID].c1);
                cell.SetFirstValues(t, temp, d);
                if (cell.index == 255)
                    continue;
                for (t.y = 0; t.y < terrain.chunkDetail; t.y++)
                {
                    temp = start + (t.x * d * TerrainManagerData.dir[axisID].c0) + (t.y * TerrainManagerData.dir[axisID].c2 * d) + (t.z * d * TerrainManagerData.dir[axisID].c1);
                    cell.SetValues(t, temp, d);
                    if (cell.index == 0)
                        break;
                    ApplyMarchingCube();
                }
            }
        }
        voxelDataGenerated = true;
        return triangles.Count > 0;
    }

    public override Mesh GenerateMesh(float3 center, Node[] neighbors)
    {
        if (terrain.chunkDetail <= 0) return null;
        if (terrain.drawAsSphere)
            return GenerateSphereTerrain(center);
        return GenerateCubeTerrain(center);
    }

    /// <summary>
    /// Sets values of mesh with modifications to form a spherelike planet.
    /// </summary>
    /// <param name="start">Starting point of the chunk</param>
    /// <returns>Generated Mesh</returns>
    Mesh GenerateSphereTerrain(float3 start)
    {
        Mesh m = new Mesh();
        m.vertices = SquareToCircle(start).ToArray();
        m.triangles = triangles.ToArray();
        m.colors = colors.ToArray();
        m.RecalculateNormals();
        return m;
    }

    /// <summary>
    /// Sets values of mesh without modification
    /// </summary>
    /// <param name="start">Starting point of the chunk</param>
    /// <returns>Generated Mesh</returns>
    Mesh GenerateCubeTerrain(float3 start)
    {
        Mesh m = new Mesh();
        m.vertices = vertexList.ToArray();
        m.triangles = triangles.ToArray();
        m.RecalculateNormals();
        return m;
    }

    /// <summary>
    /// Transforms the plane chunk to a spherelike one
    /// </summary>
    /// <param name="start">Starting point of the chunk</param>
    /// <returns>Modified list of vertices</returns>
    List<Vector3> SquareToCircle(float3 start)
    {
        float height;
        Vector3 temp, newVertex;
        List<Vector3> newVertexList = new List<Vector3>();
        colors.Clear();
        chunkCenter = Vector3.zero;
        for (int i = 0; i < vertexList.Count; i++)
        {
            temp = vertexList[i] + (Vector3)start;
            height = Mathf.Abs(temp[TerrainManagerData.axisIndex[axisID].z]);
            temp[TerrainManagerData.axisIndex[axisID].z] = terrain.planetRadius * TerrainManagerData.dirMult[axisID].z;
            newVertex = temp.normalized * height;
            SetVertexBiome(height, newVertex.y, temp);
            chunkCenter += newVertex;
            newVertexList.Add(newVertex);
        }
        chunkCenter /= newVertexList.Count;
        for (int i = 0; i < newVertexList.Count; i++)
            newVertexList[i] -= chunkCenter;
        return newVertexList;
    }

    /// <summary>
    /// Sets the color of the vertex
    /// </summary>
    /// <param name="h">Height relative to the center</param>
    /// <param name="y">Y position relative to the planet</param>
    /// <param name="point">Vertex in cube position</param>
    void SetVertexBiome(float h, float y, Vector3 point)
    {
        if (terrain.showBiome)
        {
            int b = terrain.GetBiomeNumber(axisID, h, y, point);
            biome.Add(b);
            colors.Add(terrain.GetPointColor(b));
        }
        else if (!terrain.showTemperature)
            colors.Add(terrain.GetHumidity(axisID, point));
        else
            colors.Add(terrain.GetTemperature(h, y));
        
    }

    /// <summary>
    /// Checks the kind of mesh to form depending on cell.index.
    /// Adds values to triangle accordingly
    /// </summary>
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

    /// <summary>
    /// Adds vertices to the list and dictionary.
    /// The Dictionary uses the edge as a reference for the key value
    /// </summary>
    /// <param name="index1">Vertex 1 of the edge</param>
    /// <param name="index2">Vertex 2 of the edge</param>
    /// <returns>Index of the point in vertexList</returns>
    int AddPoint(int index1, int index2)
    {
        int3x2 temp;

        // Vertex with highest value goes first
        // Keeps the keys consistent
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

    /// <summary>
    /// Interpolates between 2 points using the voxel value as reference
    /// </summary>
    /// <param name="p1">Point 1</param>
    /// <param name="p2">Point 2</param>
    /// <param name="v1">Voxel value for Point 1</param>
    /// <param name="v2">Voxel value for Point 2</param>
    /// <returns>Interpolated value</returns>
    float3 VertexInterpolation(float3 p1, float3 p2, float v1, float v2)
    {
        float3 p = p2 - p1;
        float t = Mathf.Abs(v1) + Mathf.Abs(v2);
        p = p * (Mathf.Abs(v1) / t);
        return p1 + p;
    }
    
    public override bool getEdgeCubes(int3x2 v, ref List<int3> c, ref List<float4> p, int3 dif, int otherLOD, int otherAxisID, int vertices)
    {
        // Not used in this algorithm
        return false;
    }

}