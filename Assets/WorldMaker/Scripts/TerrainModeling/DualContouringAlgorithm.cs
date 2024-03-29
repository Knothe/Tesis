﻿using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public class DualContouringAlgorithm : Algorithm
{
    /// <summary> Hermite Data localizer through edge </summary>
    Dictionary<int3x2, CubeEdge> edges;
    /// <summary> Hermite Data for mesh generation </summary>
    List<HermiteData> interpVertex;
    /// <summary> Vertex index with voxel index representation, used for mesh generation </summary>
    Dictionary<int3, int> pointsSquare { get; set; }
    
    DualCell cell;
    /// <summary> Starting point of the chunk </summary>
    float3 start;

    public DualContouringAlgorithm(TerrainInfo t, int a, int l) : base(t, a, l)
    {
        vertexList = new List<Vector3>();
        pointsSquare = new Dictionary<int3, int>();
        cell = new DualCell(axisID, 0, terrain, (terrain.maxResolution * terrain.reescaleValues[terrain.levelsOfDetail - 1 - level]) / terrain.chunkDetail);
    }

    #region Voxel Data generation
    
    public override bool GenerateVoxelData(float3 start)
    {
        if (terrain.chunkDetail <= 0)
            return false;
        if(voxelDataGenerated)
            return interpVertex.Count > 0;
        this.start = start;
        InitializeValues();
        float3 temp;
        float d = (terrain.maxResolution * terrain.reescaleValues[terrain.levelsOfDetail - 1 - level]) / terrain.chunkDetail;
        for (int3 t = int3.zero; t.x < terrain.chunkDetail; t.x++)
        {
            for (t.z = 0; t.z < terrain.chunkDetail; t.z++)
            {
                temp = start + (t.x * d * TerrainManagerData.dir[axisID].c0) + ((terrain.chunkDetail - 1) * TerrainManagerData.dir[axisID].c2 * d) + (t.z * d * TerrainManagerData.dir[axisID].c1);
                cell.SetFirstValues(t, temp, d);
                if (cell.index == 255)
                    continue;
                for (t.y = 0; t.y < terrain.chunkDetail; t.y++)
                {
                    temp = start + (t.x * d * TerrainManagerData.dir[axisID].c0) + (t.y * d * TerrainManagerData.dir[axisID].c2) + (t.z * d * TerrainManagerData.dir[axisID].c1);
                    cell.SetValues(t, temp, d);
                    if (cell.index == 0)
                        break;
                    SetIntersection();
                }
            }
        }
        voxelDataGenerated = true;
        return interpVertex.Count > 0;
    }
    
    /// <summary> Initializes lists and Dictionaries </summary>
    void InitializeValues()
    {
        edges = new Dictionary<int3x2, CubeEdge>();
        interpVertex = new List<HermiteData>();
        vertexList = new List<Vector3>();
        pointsSquare = new Dictionary<int3, int>();
    }

    /// <summary>
    /// Checks the kind of mesh to form depending on cell.index
    /// Adds to vertexList and pointsSquares accordingly
    /// </summary>
    void SetIntersection()
    {
        // Positive values are ground
        // Negative are air
        List<HermiteData> datosCubo = new List<HermiteData>();
        if (MarchingTables.edges[cell.index] == 0) return;
        if ((1 & MarchingTables.edges[cell.index]) == 1)
            datosCubo.Add(AddPoint(0, 1));
        if ((2 & MarchingTables.edges[cell.index]) == 2)
            datosCubo.Add(AddPoint(1, 2));
        if ((4 & MarchingTables.edges[cell.index]) == 4)
            datosCubo.Add(AddPoint(2, 3));
        if ((8 & MarchingTables.edges[cell.index]) == 8)
            datosCubo.Add(AddPoint(3, 0));
        if ((16 & MarchingTables.edges[cell.index]) == 16)
            datosCubo.Add(AddPoint(4, 5));
        if ((32 & MarchingTables.edges[cell.index]) == 32)
            datosCubo.Add(AddPoint(5, 6));
        if ((64 & MarchingTables.edges[cell.index]) == 64)
            datosCubo.Add(AddPoint(6, 7));
        if ((128 & MarchingTables.edges[cell.index]) == 128)
            datosCubo.Add(AddPoint(7, 4));
        if ((256 & MarchingTables.edges[cell.index]) == 256)
            datosCubo.Add(AddPoint(0, 4));
        if ((512 & MarchingTables.edges[cell.index]) == 512)
            datosCubo.Add(AddPoint(1, 5));
        if ((1024 & MarchingTables.edges[cell.index]) == 1024)
            datosCubo.Add(AddPoint(2, 6));
        if ((2048 & MarchingTables.edges[cell.index]) == 2048)
            datosCubo.Add(AddPoint(3, 7));
        vertexList.Add(CalculateNewPoint(ref datosCubo));
        pointsSquare.Add(cell.GetPosition(), vertexList.Count - 1);
    }

    /// <summary>
    /// Adds edges to the list and dictionary
    /// </summary>
    /// <param name="index1">Vertex 1 of the edge</param>
    /// <param name="index2">Vertex 2 of the edge</param>
    /// <returns>Hermite Data generated from values</returns>
    HermiteData AddPoint(int index1, int index2)
    {
        int3x2 temp;
        if (cell.pointValues[index1] > cell.pointValues[index2])
            temp = new int3x2(cell.relativeIndex[index1], cell.relativeIndex[index2]);
        else
            temp = new int3x2(cell.relativeIndex[index2], cell.relativeIndex[index1]);

        if (edges.ContainsKey(temp))
        {
            edges[temp].AddCube(cell.GetPosition());
            return interpVertex[edges[temp].hermiteIndex];
        }
        float v1 = cell.pointValues[index1];
        float v2 = cell.pointValues[index2];
        float3 v = VertexInterpolation(cell.pointList[index1], cell.pointList[index2], v1, v2);
        edges.Add(temp, new CubeEdge(interpVertex.Count, cell.GetPosition(), new float3x2(cell.pointList[index1], cell.pointList[index2])));
        interpVertex.Add(new HermiteData(v, cell.GetPointNormal(v)));
        return interpVertex[edges[temp].hermiteIndex];
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

    /// <summary>
    /// Calculates new Vertex using hermite data
    /// </summary>
    /// <param name="dataList">Hermite Data for new vertex</param>
    /// <returns>Generated Vertex</returns>
    Vector3 CalculateNewPoint(ref List<HermiteData> dataList)
    {
        Vector3 newPos = Vector3.zero;
        foreach (HermiteData h in dataList)
            newPos += h.position;
        newPos = newPos / dataList.Count;

        Vector3 move = Vector3.zero;
        foreach (HermiteData h in dataList)
            move += h.normal.normalized;
        move.Normalize();

        Vector3 tempVector;
        double tempQef;
        double qef = GetQEF(newPos, ref dataList);
        for (int i = 0; i < 30; i++)
        {
            if (!(qef < .02f && qef > -.02f))
            {
                float mod = terrain.chunkDetail * Mathf.Abs((float)qef);
                tempVector = new Vector3(newPos.x + mod, newPos.y, newPos.z);
                tempQef = GetQEF(tempVector, ref dataList);
                if (tempQef < qef)
                {
                    qef = tempQef;
                    continue;
                }
                tempVector = new Vector3(newPos.x - mod, newPos.y, newPos.z);
                tempQef = GetQEF(tempVector, ref dataList);
                if (tempQef < qef)
                {
                    qef = tempQef;
                    continue;
                }
                tempVector = new Vector3(newPos.x, newPos.y + mod, newPos.z);
                tempQef = GetQEF(tempVector, ref dataList);
                if (tempQef < qef)
                {
                    qef = tempQef;
                    continue;
                }
                tempVector = new Vector3(newPos.x, newPos.y - mod, newPos.z);
                tempQef = GetQEF(tempVector, ref dataList);
                if (tempQef < qef)
                {
                    qef = tempQef;
                    continue;
                }
                tempVector = new Vector3(newPos.x, newPos.y, newPos.z + mod);
                tempQef = GetQEF(tempVector, ref dataList);
                if (tempQef < qef)
                {
                    qef = tempQef;
                    continue;
                }
                tempVector = new Vector3(newPos.x, newPos.y, newPos.z - mod);
                tempQef = GetQEF(tempVector, ref dataList);
                if (tempQef < qef)
                {
                    qef = tempQef;
                    continue;
                }
            }
        }
        return newPos;
    }

    /// <summary>
    /// Calculates the Quadratic Error Function
    /// </summary>
    /// <param name="point">Intersection point</param>
    /// <param name="dataList">Hermite Data for the intersection point</param>
    /// <returns>Error in between the point and data list</returns>
    double GetQEF(Vector3 point, ref List<HermiteData> dataList)
    {
        double qef = 0;
        double temp;
        foreach (HermiteData h in dataList)
        {
            temp = Vector3.Dot(h.normal, point - h.position);
            qef += (temp * temp);
        }
        return qef;
    }
    #endregion

    #region Mesh Generation
    public override Mesh GenerateMesh(float3 center, Node[] neighbors)
    {
        List<int> triangles = new List<int>();
        bool c;
        start = center;
        List<int3x2> keyList = new List<int3x2>();
        foreach (int3x2 key in edges.Keys)
            keyList.Insert(0, key);
        foreach (int3x2 key in keyList)
        {
            CubeEdge cubeEdge = edges[key];
            if (cubeEdge.cubes.Count < 4)
            {
                c = !AddCubes(GetNeighbourIndex(key), key, ref neighbors, cubeEdge);
                if (c)
                    continue;
            }
            if (SetPointsList(cubeEdge, key, ref triangles))
                edges[key] = new CubeEdge(edges[key]);
        }
        Mesh m = new Mesh();
        if (terrain.drawAsSphere)
            m.vertices = SquareToCircle(vertexList).ToArray();
        else
            m.vertices = vertexList.ToArray();
        m.colors = colors.ToArray();
        m.triangles = triangles.ToArray();
        m.RecalculateNormals();
        return m;
    }

    /// <summary>
    /// Finds neighbors that share the same edge
    /// </summary>
    /// <param name="key">Edge</param>
    /// <returns>Highest index value for found neighbors</returns>
    int GetNeighbourIndex(int3x2 key)
    {
        bool[] face = { Mathf.Abs(key.c0[0]) == terrain.chunkDetail && Mathf.Abs(key.c1[0]) == terrain.chunkDetail,     // 0    Right
                        Mathf.Abs(key.c0[2]) == terrain.chunkDetail && Mathf.Abs(key.c1[2]) == terrain.chunkDetail,     // 1    Front
                        Mathf.Abs(key.c0[1]) == terrain.chunkDetail && Mathf.Abs(key.c1[1]) == terrain.chunkDetail,     // 2    Up
                        key.c0[0] == 0 && key.c1[0] == 0,                                                               // 3    Left
                        key.c0[2] == 0 && key.c1[2] == 0,                                                               // 4    Back
                        key.c0[1] == 0 && key.c1[1] == 0                                                                // 5    Down
        };

        if (face[0]){
            if (face[1])        return 6;       // Right Front
            else if (face[4])   return 7;       // Right Back
            else if (face[2])   return 8;       // Right Up
            else if (face[5])   return 9;       // Right Down
            else                return 0;       // Right
        }
        else if (face[3]){
            if (face[1])        return 10;      // Left Front
            else if (face[4])   return 11;      // Left Back
            else if (face[2])   return 12;      // Left Up
            else if (face[5])   return 13;      // Left Down
            else                return 3;       // Left
        }
        else if (face[1]){
            if (face[2])        return 14;      // Front Up
            else if (face[5])   return 15;      // Front Down
            else                return 1;       // Front
        }
        else if (face[4]){
            if (face[2])        return 16;      // Back Up
            else if (face[5])   return 17;      // Back Down
            else                return 4;       // Back
        }
        else if (face[2])       return 2;       // Up
        else if (face[5])       return 5;       // Down
        return -1;                              // Somthing wrong
    }

    /// <summary>
    /// Transforms the plane chunk to a spherelike one
    /// </summary>
    /// <param name="p">List of vertices</param>
    /// <returns>Modified list of vertices</returns>
    List<Vector3> SquareToCircle(List<Vector3> p)
    {
        float height;
        Vector3 temp, newVertex;
        List<Vector3> newVertexList = new List<Vector3>();
        colors.Clear();
        chunkCenter = Vector3.zero;
        foreach (Vector3 v in p)
        {
            temp = v + (Vector3)start;
            height = Mathf.Abs(temp[TerrainManagerData.axisIndex[axisID].z]);
            temp[TerrainManagerData.axisIndex[axisID].z] = terrain.planetRadius * TerrainManagerData.dirMult[axisID].z;
            newVertex = temp.normalized * height;
            SetVertexBiome(height, newVertex.y, temp);
            chunkCenter += newVertex;
            newVertexList.Add(newVertex);
        }
        chunkCenter /= newVertexList.Count;
        for (int i = 0; i < vertexList.Count; i++)
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
        else
        {
            if (!terrain.showTemperature)
                colors.Add(terrain.GetHumidity(axisID, point));
            else
                colors.Add(terrain.GetTemperature(h, y));
        }
    }

    /// <summary>
    /// Based on edge data, sets how the triangles should be ordered
    /// </summary>
    /// <param name="c">Cube edge to set</param>
    /// <param name="e">Edge key</param>
    /// <param name="tris">List of triangles</param>
    /// <returns>True if triangles were added</returns>
    bool SetPointsList(CubeEdge c, int3x2 e, ref List<int> tris)
    {
        if (c.cubes.Count < 3)
            return false;

        int3 v1 = e.c1 - e.c0;
        int3 v2 = int3.zero;
        if (v1.x != 0)
        {
            v2.y = 1;
            v2.z = v1.x;
        }
        else if (v1.y != 0)
        {
            v2.z = 1;
            v2.x = v1.y;
        }
        else
        {
            v2.y = 1;
            v2.x = v1.z * -1;
        }

        int[] points = new int[4];

        float3 t = float3.zero;
        foreach (int3 f in c.cubes)
            t += f;

        t = t / 4f;

        foreach (int3 f in c.cubes)
        {
            float3 temp = f - t;
            if (v2.x == 0)
            {
                if (IsSignEqual(v2.z, temp.z))
                {
                    if (IsSignEqual(v2.y, temp.y))
                        points[0] = pointsSquare[f];
                    else
                        points[1] = pointsSquare[f];
                }
                else
                {
                    if (IsSignEqual(v2.y, temp.y))
                        points[3] = pointsSquare[f];
                    else
                        points[2] = pointsSquare[f];
                }
            }
            else if (v2.y == 0)
            {
                if (IsSignEqual(v2.x, temp.x))
                {
                    if (IsSignEqual(v2.z, temp.z))
                        points[0] = pointsSquare[f];
                    else
                        points[1] = pointsSquare[f];
                }
                else
                {
                    if (IsSignEqual(v2.z, temp.z))
                        points[3] = pointsSquare[f];
                    else
                        points[2] = pointsSquare[f];
                }

            }
            else
            {
                if (IsSignEqual(v2.x, temp.x))
                {
                    if (IsSignEqual(v2.y, temp.y))
                        points[0] = pointsSquare[f];
                    else
                        points[1] = pointsSquare[f];
                }
                else
                {
                    if(IsSignEqual(v2.y, temp.y))
                        points[3] = pointsSquare[f];
                    else
                        points[2] = pointsSquare[f];
                }
            }
        }

        tris.Add(points[0]);
        tris.Add(points[1]);
        tris.Add(points[2]);
        tris.Add(points[0]);
        tris.Add(points[2]);
        tris.Add(points[3]);
        return true;
    }

    /// <summary>
    /// Checks if 2 values have the same sign
    /// </summary>
    /// <param name="v1">Value 1</param>
    /// <param name="v2">Value 2</param>
    /// <returns>True if signs are equal</returns>
    bool IsSignEqual(float v1, float v2)
    {
        return Mathf.Sign(v1) == Mathf.Sign(v2);
    }

    /// <summary>
    /// Adds cubes to an edge if needed
    /// </summary>
    /// <param name="id"></param>
    /// <param name="thisEdge"></param>
    /// <param name="neighbors"></param>
    /// <param name="ce"></param>
    /// <returns></returns>
    bool AddCubes(int id, int3x2 thisEdge, ref Node[] neighbors, CubeEdge ce)
    {
        List<int3> cubes = new List<int3>();
        List<float4> cPoints = new List<float4>();
        List<int3> temp = new List<int3>();

        if (id == -1)
            return false;

        if (id < 6)
        {
            if (neighbors[id] == null || neighbors[id].level != level)
                return false;
            if (neighbors[id].data.axisID != axisID)
                DifFace(ref temp, DualContouringData.otherEdgeData[id], DualContouringData.otherEdgeData[id].cubesIndex.x, thisEdge,
                    ref neighbors, ref cubes, ref cPoints, 2);
            else
                SameFace(ref temp, DualContouringData.otherEdgeData[id], DualContouringData.otherEdgeData[id].cubesIndex.x, thisEdge,
                    ref neighbors, ref cubes, ref cPoints, 2);
        }
        else
        {
            GetOtherEdgeData otherEdgeData = DualContouringData.otherEdgeData[id];
            if (neighbors[otherEdgeData.cubesIndex.x] == null ||
                neighbors[otherEdgeData.cubesIndex.y] == null ||
                neighbors[otherEdgeData.cubesIndex.z] == null)
                return false;

            if (neighbors[otherEdgeData.cubesIndex.x].level != level ||
                neighbors[otherEdgeData.cubesIndex.y].level != level ||
                neighbors[otherEdgeData.cubesIndex.z].level != level)
                return false;

            if (neighbors[otherEdgeData.cubesIndex.x].axisID == axisID)
                SameFace(ref temp, DualContouringData.otherEdgeData[otherEdgeData.cubesIndex.x], otherEdgeData.cubesIndex.x, thisEdge, ref neighbors, ref cubes, ref cPoints, 1);
            else
                DifFace(ref temp, DualContouringData.otherEdgeData[otherEdgeData.cubesIndex.x], otherEdgeData.cubesIndex.x, thisEdge, ref neighbors, ref cubes, ref cPoints, 1);
            if (neighbors[otherEdgeData.cubesIndex.y].axisID == axisID)
                SameFace(ref temp, DualContouringData.otherEdgeData[otherEdgeData.cubesIndex.y], otherEdgeData.cubesIndex.y, thisEdge, ref neighbors, ref cubes, ref cPoints, 1);
            else
                DifFace(ref temp, DualContouringData.otherEdgeData[otherEdgeData.cubesIndex.y], otherEdgeData.cubesIndex.y, thisEdge, ref neighbors, ref cubes, ref cPoints, 1);
            if (neighbors[otherEdgeData.cubesIndex.z].axisID == axisID)
                SameFace(ref temp, otherEdgeData, otherEdgeData.cubesIndex.z, thisEdge, ref neighbors, ref cubes, ref cPoints, 1);
            else
                DifFace(ref temp, otherEdgeData, otherEdgeData.cubesIndex.z, thisEdge, ref neighbors, ref cubes, ref cPoints, 1);
        }
        int count = ce.cubes.Count + temp.Count;
        if (count != 4)
        {
            return false;
        }

        foreach (int3 v in temp)
            ce.AddCube(v);

        return true;
    }

    /// <summary>
    /// Selects cubes from an edge in another chunk in a different face
    /// </summary>
    /// <param name="ce">List where wanted cubes are added</param>
    /// <param name="otherEdgeData">Edge we want to get cubes from</param>
    /// <param name="cubeID">Cube index in the neighbor array</param>
    /// <param name="thisEdge">Original edge</param>
    /// <param name="neighbors">Neighbor chunks</param>
    /// <param name="cubes">List of cubes to add to original edge</param>
    /// <param name="cPoints">Vertices ready to be translated to other chunk</param>
    /// <param name="points">Points to add</param>
    void DifFace(ref List<int3> ce, GetOtherEdgeData otherEdgeData, int cubeID, int3x2 thisEdge, ref Node[] neighbors, ref List<int3> cubes, ref List<float4> cPoints, int points)
    {
        int3 dif = int3.zero;
        int3x2 otherEdge = thisEdge;
        dif[otherEdgeData.index.x] += otherEdgeData.dir.x * terrain.chunkDetail;
        dif[otherEdgeData.index.y] += otherEdgeData.dir.y * terrain.chunkDetail;
        otherEdge[0] -= dif;
        otherEdge[1] -= dif;
        int2 temp = TerrainManagerData.RotateSimple(axisID, neighbors[cubeID].data.axisID, otherEdge[0].x, otherEdge[0].z, terrain.chunkDetail + 1);
        otherEdge[0].x = temp.x;
        otherEdge[0].z = temp.y;

        temp = TerrainManagerData.RotateSimple(axisID, neighbors[cubeID].data.axisID, otherEdge[1].x, otherEdge[1].z, terrain.chunkDetail + 1);
        otherEdge[1].x = temp.x;
        otherEdge[1].z = temp.y;
        if (!neighbors[cubeID].data.getEdgeCubes(otherEdge, ref cubes, ref cPoints, dif, level, axisID, points))
            return;
        for (int i = 0; i < points; i++)
        {
            int cubeIndex = cubes.Count - i - 1;
            if (cubeIndex < 0 || cubeIndex >= cubes.Count)
            {
                Debug.Log("Error: Outside");
                return;
            }
            ce.Add(cubes[cubeIndex]);
            if (!pointsSquare.ContainsKey(cubes[cubeIndex]))
            {
                pointsSquare.Add(cubes[cubeIndex], vertexList.Count);
                vertexList.Add(DifFaceSphereToCube(cPoints[cubeIndex]));
            }
        }
    }

    /// <summary>
    /// Selects cubes from an edge in another chunk in the same face
    /// </summary>
    /// <param name="ce">List where wanted cubes are added</param>
    /// <param name="otherEdgeData"></param>
    /// <param name="cubeID">Edge we want to get cubes from</param>
    /// <param name="thisEdge">Original edge</param>
    /// <param name="neighbors"></param>
    /// <param name="cubes">Neighbor chunks</param>
    /// <param name="cPoints">Vertices ready to be translated to other chunk</param>
    /// <param name="points">Points to add</param>
    void SameFace(ref List<int3> ce, GetOtherEdgeData otherEdgeData, int cubeID, int3x2 thisEdge, ref Node[] neighbors, ref List<int3> cubes, ref List<float4> cPoints, int points)
    {
        int3 dif = int3.zero;
        int3x2 otherEdge = thisEdge;
        dif[otherEdgeData.index.x] += otherEdgeData.dir.x * terrain.chunkDetail;
        dif[otherEdgeData.index.y] += otherEdgeData.dir.y * terrain.chunkDetail;
        otherEdge[0] -= dif;
        otherEdge[1] -= dif;
        dif[otherEdgeData.index.x] += otherEdgeData.dir.x;
        dif[otherEdgeData.index.y] += otherEdgeData.dir.y;
        if (!neighbors[cubeID].data.getEdgeCubes(otherEdge, ref cubes, ref cPoints, dif, level, axisID, points))
            return;
        for (int i = 0; i < points; i++)
        {
            int cubeIndex = cubes.Count - i - 1;
            if (cubeIndex < 0 || cubeIndex >= cubes.Count)
                return;
            ce.Add(cubes[cubeIndex]);
            if (!pointsSquare.ContainsKey(cubes[cubeIndex]))
            {
                pointsSquare.Add(cubes[cubeIndex], vertexList.Count);
                vertexList.Add(SphereToCube(cPoints[cubeIndex]));
            }
        }
    }

    public override bool getEdgeCubes(int3x2 v, ref List<int3> c, ref List<float4> p, int3 dif, int otherLOD, int otherAxisID, int vertices)
    {
        if (edges == null) return false;
        if (level != otherLOD)
            return false;
        if (!edges.ContainsKey(v))
            return false;
        if (edges[v].cubes.Count > 2)
            return false;
        if (otherAxisID != axisID)
        {
            for (int i = 0; i < vertices && i < edges[v].cubes.Count; i++)
            {
                c.Add((RotateCube(edges[v].cubes[i], otherAxisID)) + dif);
                p.Add(DifFaceCubeToSphere(vertexList[pointsSquare[edges[v].cubes[i]]]));
            }
        }
        else
        {
            for (int i = 0; i < vertices && i < edges[v].cubes.Count; i++)
            {
                c.Add(edges[v].cubes[i] + dif);
                p.Add(CubeToSphere(vertexList[pointsSquare[edges[v].cubes[i]]]));
            }
        }
        return true;
    }

    /// <summary>
    /// Rotates a cube position acording to its own axisID and the objective axisID
    /// </summary>
    /// <param name="c">Cube to rotate</param>
    /// <param name="otherAxisID">External axisID</param>
    /// <returns></returns>
    int3 RotateCube(int3 c, int otherAxisID)
    {
        int2 temp = TerrainManagerData.RotateSimple(axisID, otherAxisID, c.x, c.z, terrain.chunkDetail);
        return new int3(temp.x, c.y, temp.y);
    }

    /// <summary>
    /// Transforms a point from a Cubic planet to a Spherical one in the same face
    /// </summary>
    /// <param name="point">Point to transform</param>
    /// <returns>Transformed point</returns>
    float4 CubeToSphere(float3 point)
    {
        return new float4(point.x + start.x, point.y + start.y, point.z + start.z, 0);
    }

    /// <summary>
    /// Transforms a point from a Spherical planet to a Cubical one in the same face
    /// </summary>
    /// <param name="point">Point to transform</param>
    /// <returns>Transformed point</returns>
    float3 SphereToCube(float4 point)
    {
        return new float3(point.x - start.x, point.y - start.y, point.z - start.z);
    }

    /// <summary>
    /// Transforms a point from a Cubic planet to a Spherical one in a different face
    /// </summary>
    /// <param name="point">Point to transform</param>
    /// <returns>Transformed point</returns>
    float4 DifFaceCubeToSphere(float3 point)
    {
        Vector3 temp = point + start;
        float height = Mathf.Abs(temp[TerrainManagerData.axisIndex[axisID].z]);
        temp[TerrainManagerData.axisIndex[axisID].z] = terrain.planetRadius * TerrainManagerData.dirMult[axisID].z;
        return new float4(temp.x, temp.y, temp.z, height);
    }

    /// <summary>
    /// Transforms a point from a Spherical planet to a Cubical one in a different face
    /// </summary>
    /// <param name="point">Point to transform</param>
    /// <returns>Transformed point</returns>
    float3 DifFaceSphereToCube(float4 point)
    {
        float height = point.w;
        Vector3 n = TerrainManagerData.dir[axisID][2];
        Vector3 p_0 = terrain.faceStart[axisID];

        Vector3 l_0 = Vector3.zero;
        Vector3 l = (new Vector3(point.x, point.y, point.z)).normalized;
        float denominator = Vector3.Dot(l, n);
        Vector3 p = Vector3.zero;
        if(denominator != 0)
        {
            float t = Vector3.Dot(p_0 - l_0, n) / denominator;
            p = l_0 + l * t;
        }
        p[TerrainManagerData.axisIndex[axisID].z] = height * TerrainManagerData.dirMult[axisID].z;
        p -= (Vector3)start;
        return p;
    }

    #endregion

}

/// <summary> Saves vertex position and it's normal </summary>
public struct HermiteData
{
    public HermiteData(Vector3 p, Vector3 n)
    {
        position = p;
        normal = n;
    }

    public Vector3 position;
    public Vector3 normal;
}

/// <summary> Represents a cube edge with 2 vertex, hermite index, cubes connecting and if it's been checked </summary>
public struct CubeEdge
{
    /// <summary> Vertices values that form its limits </summary>
    public float3x2 edgeValues;
    /// <summary> Object index in interpVertex </summary>
    public int hermiteIndex { get; set; }
    /// <summary> Cubes connecting to the edge, maximum of 4 </summary>
    public List<int3> cubes;

    public CubeEdge(CubeEdge o)
    {
        edgeValues = o.edgeValues;
        hermiteIndex = o.hermiteIndex;
        cubes = o.cubes;
    }

    public CubeEdge(int h, int3 c, float3x2 e)
    {
        hermiteIndex = h;
        cubes = new List<int3>();
        edgeValues = e;
        AddCube(c);
    }

    public void AddCube(int3 c)
    {
        cubes.Add(c);
    }
}

/// <summary>
/// Pre set values for axisIndex, axisDirection and Neighbor Index for quick search of edges
/// </summary>
public static class DualContouringData
{
    public static readonly GetOtherEdgeData[] otherEdgeData = {
        //                   Axis Index      Axis Direction     Neightbor Index   
        new GetOtherEdgeData(new int2(0, 0), new int2(1, 0),    new int3(0, 0, 0)),     // Right
        new GetOtherEdgeData(new int2(2, 0), new int2(1, 0),    new int3(1, 0, 0)),     // Front
        new GetOtherEdgeData(new int2(1, 0), new int2(1, 0),    new int3(2, 0, 0)),     // Up
        new GetOtherEdgeData(new int2(0, 0), new int2(-1, 0),   new int3(3, 0, 0)),     // Left
        new GetOtherEdgeData(new int2(2, 0), new int2(-1, 0),   new int3(4, 0, 0)),     // Back
        new GetOtherEdgeData(new int2(1, 0), new int2(-1, 0),   new int3(5, 0, 0)),     // Down

        new GetOtherEdgeData(new int2(0, 2), new int2(1, 1),    new int3(0, 1, 6)),     // Right Front
        new GetOtherEdgeData(new int2(0, 2), new int2(1, -1),   new int3(0, 4, 8)),     // Right Back
        new GetOtherEdgeData(new int2(0, 1), new int2(1, 1),    new int3(0, 2, 10)),    // Right Up
        new GetOtherEdgeData(new int2(0, 1), new int2(1, -1),   new int3(0, 5, 12)),    // Right Down

        new GetOtherEdgeData(new int2(0, 2), new int2(-1, 1),   new int3(3, 1, 7)),     // Left Front
        new GetOtherEdgeData(new int2(0, 2), new int2(-1, -1),  new int3(3, 4, 9)),     // Left Back
        new GetOtherEdgeData(new int2(0, 1), new int2(-1, 1),   new int3(3, 2, 11)),    // Left Up
        new GetOtherEdgeData(new int2(0, 1), new int2(-1, -1),  new int3(3, 5, 13)),    // Left Down

        new GetOtherEdgeData(new int2(2, 1), new int2(1, 1),    new int3(1, 2, 14)),    // Front Up
        new GetOtherEdgeData(new int2(2, 1), new int2(1, -1),   new int3(1, 5, 16)),    // Front Down

        new GetOtherEdgeData(new int2(2, 1), new int2(-1, 1),   new int3(4, 2, 15)),    // Back Up
        new GetOtherEdgeData(new int2(2, 1), new int2(-1, -1),  new int3(4, 5, 17)),    // Back Down
    };
}

/// <summary>
/// Data for quick edge search
/// </summary>
public class GetOtherEdgeData
{
    /// <summary> Axes to modify in the original edge </summary>
    public int2 index;
    /// <summary> Direction of the modification, can be positive or negative </summary>
    public int2 dir;
    /// <summary> Indexes of the cubes to search an edge </summary>
    public int3 cubesIndex;

    public GetOtherEdgeData(int2 id, int2 d, int3 c)
    {
        index = id;
        dir = d;
        cubesIndex = c;
    }
}

