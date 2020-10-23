using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public class DualContouringAlgorithm : Algorithm
{
    Dictionary<int3x2, CubeEdge> edges;
    List<HermiteData> interpVertex;
    // positions -> vertexList
    Dictionary<int3, int> pointsSquare;
    DualCell cell;
    float3 start;
    //Vector3 chunkCenter;

    public DualContouringAlgorithm(TerrainInfo t, int a, int l) : base(t, a, l)
    {
        vertexList = new List<Vector3>();
        pointsSquare = new Dictionary<int3, int>();
        cell = new DualCell(TerrainManagerData.dir[axisID], 0, terrain);
    }

    #region Voxel Data generation
    public override bool GenerateVoxelData(float3 center)
    {
        if (terrain.chunkDetail <= 0)
            return false;
        edges = new Dictionary<int3x2, CubeEdge>();
        interpVertex = new List<HermiteData>();
        vertexList = new List<Vector3>();
        pointsSquare = new Dictionary<int3, int>();
        //float3 start = new float3(center.x - terrain.chunkSize / 2f, center.y - terrain.chunkSize / 2f, center.z - terrain.chunkSize / 2f);
         start = center;
        //chunkCenter = start;
        float3 temp;
        //float d = (terrain.planetRadius / (terrain.maxChunkPerFace / 2)) / terrain.chunkDetail; // Modificar
        float d = (terrain.maxResolution * terrain.reescaleValues[terrain.levelsOfDetail - 1 - level]) / terrain.chunkDetail;
        for (int3 t = int3.zero; t.x < terrain.chunkDetail; t.x++)
        {
            for (t.z = 0; t.z < terrain.chunkDetail; t.z++)
            {
                temp = start + (t.x * d * TerrainManagerData.dir[axisID].c0) + ((terrain.chunkDetail - 1) * TerrainManagerData.dir[axisID].c2 * d) + (t.z * d * TerrainManagerData.dir[axisID].c1);
                cell.SetValues(t, temp, d, terrain.noiseOffset, terrain.noiseYScale);
                if (cell.index == 255)
                    continue;
                for (t.y = 0; t.y < terrain.chunkDetail; t.y++)
                {
                    temp = start + (t.x * d * TerrainManagerData.dir[axisID].c0) + (t.y * d * TerrainManagerData.dir[axisID].c2) + (t.z * d * TerrainManagerData.dir[axisID].c1);
                    cell.SetValues(t, temp, d, terrain.noiseOffset, terrain.noiseYScale);
                    if (cell.index == 0)
                        break;
                    SetIntersection();
                }
            }
        }
        return interpVertex.Count > 0;
    }
    
    // Se pueden modificar esta y MarchingCubesAlgorithm.ApplyMarchingCube
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
        interpVertex.Add(new HermiteData(v, cell.GetPointNormal(v, terrain.noiseOffset, terrain.noiseYScale)));
        return interpVertex[edges[temp].hermiteIndex];
    }

    float3 VertexInterpolation(float3 p1, float3 p2, float v1, float v2)
    {
        float3 p = p2 - p1;
        float t = Mathf.Abs(v1) + Mathf.Abs(v2);
        p = p * (Mathf.Abs(v1) / t);
        return p1 + p;
    }


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
            if (qef < .02f && qef > -.02f)
            {
                //break;
            }
            else
            {
                //float mod = t.chunkDimensions / ((i + 1) * 2);
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

    public override Mesh GenerateMesh(float3 center)
    {
        List<int> triangles = new List<int>();
        float v = terrain.chunkDetail;
        int3 index = int3.zero;
        if (TerrainManagerData.dir[axisID].c2[0] != 0)
        {
            index.y = 0;
            if (TerrainManagerData.dir[axisID].c0[1] != 0)
            {
                index.x = 1;
                index.z = 2;
            }
            else
            {
                index.x = 2;
                index.z = 1;
            }
        }
        else if (TerrainManagerData.dir[axisID].c2.y != 0)
        {
            index.y = 1;
            if (TerrainManagerData.dir[axisID].c0[0] != 0)
            {
                index.x = 0;
                index.z = 2;
            }
            else
            {
                index.x = 2;
                index.z = 0;
            }
        }
        else
        {
            index.y = 2;
            if (TerrainManagerData.dir[axisID].c0[0] != 0)
            {
                index.x = 0;
                index.z = 1;
            }
            else
            {
                index.x = 1;
                index.z = 0;
            }
        }

        bool c;

        int2 axisIndex, dir, cubeIndex;

        List<int3x2> keyList = new List<int3x2>();
        foreach (int3x2 key in edges.Keys)
            keyList.Insert(0, key);

        foreach (int3x2 key in keyList)
        {
            CubeEdge cubeEdge = edges[key];
            if (cubeEdge.cubes.Count < 4)
            {
                axisIndex = int2.zero;
                dir = int2.zero;
                cubeIndex = int2.zero;
                int i = 0;      // axisA
                int3 temp1 = key.c0, temp2 = key.c1;

                bool[] face = { Mathf.Abs(temp1[index.x]) == v && Mathf.Abs(temp2[index.x]) == v,
                                Mathf.Abs(temp1[index.z]) == v && Mathf.Abs(temp2[index.z]) == v,
                                Mathf.Abs(temp1[index.y]) == v && Mathf.Abs(temp2[index.y]) == v,
                                temp1[index.x] == 0 && temp2[index.x] == 0,
                                temp1[index.z] == 0 && temp2[index.z] == 0,
                                temp1[index.y] == 0 && temp2[index.y] == 0
                };

                if (face[0] || face[3])
                {
                    axisIndex.x = 0;
                    if (face[0])
                    {
                        dir.x = 1;
                        cubeIndex.x = 0;
                    }
                    else
                    {
                        dir.x = -1;
                        cubeIndex.x = 3;
                    }

                    if (face[1] || face[4])
                    {
                        axisIndex.y = 1;
                        if (face[1])
                        {
                            dir.y = 1;
                            cubeIndex.y = 1;
                        }
                        else
                        {
                            dir.y = -1;
                            cubeIndex.y = 4;
                        }
                    }
                    else if (face[2] || face[5])
                    {
                        axisIndex.y = 2;
                        if (face[2])
                        {
                            dir.y = 1;
                            cubeIndex.y = 2;
                        }
                        else
                        {
                            dir.y = -1;
                            cubeIndex.y = 5;
                        }
                    }
                }
                else if (face[1] || face[4])
                {
                    axisIndex.x = 1;
                    if (face[1])
                    {
                        dir.x = 1;
                        cubeIndex.x = 1;
                    }
                    else
                    {
                        dir.x = -1;
                        cubeIndex.x = 4;
                    }
                    if (face[2] || face[5])
                    {
                        axisIndex.y = 2;
                        if (face[2])
                        {
                            dir.y = 1;
                            cubeIndex.y = 2;
                        }
                        else
                        {
                            dir.y = -1;
                            cubeIndex.y = 5;
                        }
                    }
                }
                else
                {
                    axisIndex.x = 2;
                    if (face[2])
                    {
                        dir.x = 1;
                        cubeIndex.x = 2;
                    }
                    else
                    {
                        dir.x = -1;
                        cubeIndex.x = 5;
                    }
                }

                if (axisIndex.x == 0)
                {
                    if (axisIndex.y == 1)
                    {
                        if (dir.x == 1)
                        {
                            if (dir.y == 1)
                                i = 6;
                            else
                                i = 8;
                        }
                        else
                        {
                            if (dir.y == 1)
                                i = 7;
                            else
                                i = 9;
                        }
                    }
                    else if (axisIndex.y == 2)
                    {
                        if (dir.x == 1)
                        {
                            if (dir.y == 1)
                                i = 10;
                            else
                                i = 12;
                        }
                        else
                        {
                            if (dir.y == 1)
                                i = 11;
                            else
                                i = 13;
                        }
                    }
                    else
                    {
                        if (dir.x == 1)
                            i = 0;
                        else
                            i = 3;
                    }

                }
                else if (axisIndex.x == 1)
                {
                    if (axisIndex.y == 2)
                    {
                        if (dir.x == 1)
                        {
                            if (dir.y == 1)
                                i = 14;
                            else
                                i = 16;
                        }
                        else
                        {
                            if (dir.y == 1)
                                i = 15;
                            else
                                i = 17;
                        }
                    }
                    else
                    {
                        if (dir.x == 1)
                            i = 1;
                        else
                            i = 4;
                    }
                }
                else if (axisIndex.x == 2)
                {
                    if (dir.x == 1)
                        i = 2;
                    else
                        i = 5;
                }

                //c = !AddCubes(i, axisIndex, dir, cubeIndex, key, ref dcList, cubeEdge);

                //if (c)
                    continue;
            }

            if (SetPointsList(cubeEdge, key, ref triangles))
            {
                edges[key] = new CubeEdge(edges[key]);
            }
        }

        Mesh m = new Mesh();
        m.vertices = vertexList.ToArray();
        m.vertices = SquareToCircle(vertexList, center).ToArray();
        m.triangles = triangles.ToArray();
        m.RecalculateNormals();
        return m;
    }

    List<Vector3> SquareToCircle(List<Vector3> p, float3 c)
    {
        float height;
        Vector3 temp, newVertex;
        float3 up = TerrainManagerData.dir[axisID].c2;
        float3 ax = TerrainManagerData.dir[axisID].c0 + TerrainManagerData.dir[axisID].c1;
        ax.x = Mathf.Abs(ax.x);
        ax.y = Mathf.Abs(ax.y);
        ax.z = Mathf.Abs(ax.z);
        List<Vector3> newList = new List<Vector3>();
        foreach (Vector3 v in p)
        {
            temp = v + (Vector3)c;
            height = (temp.x * up.x) + (temp.y * up.y) + (temp.z * up.z);
            temp = (temp * ax) + (up * terrain.planetRadius);
            newVertex = temp.normalized * height;
            newList.Add(newVertex);
        }

        return newList;
    }

    bool SetPointsList(CubeEdge c, int3x2 e, ref List<int> tris)
    {
        if (c.cubes.Count < 4)
            return false;

        int3 v1 = e.c1 - e.c0;
        int3 v2 = int3.zero;
        HermiteData h = interpVertex[c.hermiteIndex];
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
                if (Mathf.Sign(v2.z) == Mathf.Sign(temp.z))
                {
                    if (Mathf.Sign(v2.y) == Mathf.Sign(temp.y))
                        points[0] = pointsSquare[f];
                    else
                        points[1] = pointsSquare[f];
                }
                else
                {
                    if (Mathf.Sign(v2.y) == Mathf.Sign(temp.y))
                        points[3] = pointsSquare[f];
                    else
                        points[2] = pointsSquare[f];
                }
            }
            else if (v2.y == 0)
            {
                if (Mathf.Sign(v2.x) == Mathf.Sign(temp.x))
                {
                    if (Mathf.Sign(v2.z) == Mathf.Sign(temp.z))
                        points[0] = pointsSquare[f];
                    else
                        points[1] = pointsSquare[f];
                }
                else
                {
                    if (Mathf.Sign(v2.z) == Mathf.Sign(temp.z))
                        points[3] = pointsSquare[f];
                    else
                        points[2] = pointsSquare[f];
                }

            }
            else
            {
                if (Mathf.Sign(v2.x) == Mathf.Sign(temp.x))
                {
                    if (Mathf.Sign(v2.y) == Mathf.Sign(temp.y))
                        points[0] = pointsSquare[f];
                    else
                        points[1] = pointsSquare[f];
                }
                else
                {
                    if (Mathf.Sign(v2.y) == Mathf.Sign(temp.y))
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

    /*
    bool AddCubes(int p, int2 index, int2 dir, int2 cubeindex, int3x2 thisEdge, ref DualChunk[] dcList, CubeEdge ce)
    {
        int3x2 otherEdge = thisEdge;
        int3x3 a = (int3x3)axis;
        int3 dif;
        List<int3> cubes = new List<int3>();
        List<float3> cPoints = new List<float3>();

        if (p < 6)
        {
            if (!dcList[cubeindex.x])
                return false;
            dif = a[index.x] * dir.x;
            otherEdge[0] = thisEdge[0] - (terrain.chunkDetail * dif);
            otherEdge[1] = thisEdge[1] - (terrain.chunkDetail * dif);
            dcList[cubeindex.x].getEdgeCubes(otherEdge, ref cubes, ref cPoints, dif);
        }
        else
        {
            if (!dcList[cubeindex.x] || !dcList[cubeindex.y] || !dcList[p] || index.x == index.y)
                return false;
            dif = a[index.x] * dir.x;
            otherEdge[0] = thisEdge[0] - (terrain.chunkDetail * dif);
            otherEdge[1] = thisEdge[1] - (terrain.chunkDetail * dif);
            dcList[cubeindex.x].getEdgeCubes(otherEdge, ref cubes, ref cPoints, dif);

            dif = a[index.y] * dir.y;
            otherEdge[0] = thisEdge[0] - (terrain.chunkDetail * dif);
            otherEdge[1] = thisEdge[1] - (terrain.chunkDetail * dif);
            dcList[cubeindex.y].getEdgeCubes(otherEdge, ref cubes, ref cPoints, dif);

            dif = (a[index.y] * dir.y) + (a[index.x] * dir.x);
            otherEdge[0] = (terrain.chunkDetail * a[index.x] * dir.x) + (terrain.chunkDetail * a[index.y] * dir.y);
            otherEdge[0] = thisEdge[0] - otherEdge[0];

            otherEdge[1] = (terrain.chunkDetail * a[index.x] * dir.x) + (terrain.chunkDetail * a[index.y] * dir.y);
            otherEdge[1] = thisEdge[1] - otherEdge[1];
            dcList[p].getEdgeCubes(otherEdge, ref cubes, ref cPoints, dif);
        }
        if (ce.cubes.Count + cubes.Count < 4)
            return false;
        for (int i = 0; i < cubes.Count; i++)
        {
            ce.AddCube(cubes[i]);
            if (!pointsSquare.ContainsKey(cubes[i]))
            {
                pointsSquare.Add(cubes[i], vertexList.Count);
                vertexList.Add(cPoints[i]);
            }
        }
        return true;
    }
    */
    #endregion

}

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


public struct CubeEdge
{
    public float3x2 edgeValues;
    public int hermiteIndex;
    public List<int3> cubes;
    public bool passed;

    public CubeEdge(CubeEdge o)
    {
        edgeValues = o.edgeValues;
        hermiteIndex = o.hermiteIndex;
        cubes = o.cubes;
        passed = true;
    }

    public CubeEdge(int h, int3 c, float3x2 e)
    {
        hermiteIndex = h;
        cubes = new List<int3>();
        edgeValues = e;
        passed = false;
        AddCube(c);
    }

    public void AddCube(int3 c)
    {
        cubes.Add(c);
    }

    public void ChangePassed(bool p)
    {
        passed = p;
    }
}



