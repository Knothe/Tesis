using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using UnityEngine.UIElements;
using UnityEngine.Animations;

public class DualContouringAlgorithm : Algorithm
{
    Dictionary<int3x2, CubeEdge> edges;
    List<HermiteData> interpVertex;
    // positions -> vertexList
    Dictionary<int3, int> pointsSquare;
    DualCell cell;
    float3 start;
    //Vector3 chunkCenter;

    public DualContouringAlgorithm(TerrainInfo t, int a, int l, int cp) : base(t, a, l, cp)
    {
        vertexList = new List<Vector3>();
        pointsSquare = new Dictionary<int3, int>();
        cell = new DualCell(axisID, 0, terrain);
    }

    #region Voxel Data generation
    public override bool GenerateVoxelData(float3 center)
    {
        if (terrain.chunkDetail <= 0)
            return false;
        if(voxelDataGenerated)
            return interpVertex.Count > 0;
        edges = new Dictionary<int3x2, CubeEdge>();
        interpVertex = new List<HermiteData>();
        vertexList = new List<Vector3>();
        pointsSquare = new Dictionary<int3, int>();
        start = center;
        float3 temp;
        float d = (terrain.maxResolution * terrain.reescaleValues[terrain.levelsOfDetail - 1 - level]) / terrain.chunkDetail;
        for (int3 t = int3.zero; t.x < terrain.chunkDetail; t.x++)
        {
            for (t.z = 0; t.z < terrain.chunkDetail; t.z++)
            {
                temp = start + (t.x * d * TerrainManagerData.dir[axisID].c0) + ((terrain.chunkDetail - 1) * TerrainManagerData.dir[axisID].c2 * d) + (t.z * d * TerrainManagerData.dir[axisID].c1);
                cell.SetFirstValues(t, temp, d, terrain.noiseOffset);
                if (cell.index == 255)
                    continue;
                for (t.y = 0; t.y < terrain.chunkDetail; t.y++)
                {
                    temp = start + (t.x * d * TerrainManagerData.dir[axisID].c0) + (t.y * d * TerrainManagerData.dir[axisID].c2) + (t.z * d * TerrainManagerData.dir[axisID].c1);
                    cell.SetValues(t, temp, d, terrain.noiseOffset);
                    if (cell.index == 0)
                        break;
                    SetIntersection();
                }
            }
        }
        voxelDataGenerated = true;
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
    public override Mesh GenerateMesh(float3 center, Node[] neighbors)
    {
        List<int> triangles = new List<int>();
        bool c;

        List<int3x2> keyList = new List<int3x2>();
        foreach (int3x2 key in edges.Keys)
            keyList.Insert(0, key);
        foreach (int3x2 key in keyList)
        {
            CubeEdge cubeEdge = edges[key];
            if (cubeEdge.cubes.Count < 4)
            {
                c = !AddCubes(DualContouringData.otherEdgeData[GetNeighbourIndex(key)], key, ref neighbors, cubeEdge);
                if (c)
                    continue;
            }
            if (SetPointsList(cubeEdge, key, ref triangles))
                edges[key] = new CubeEdge(edges[key]);
        }
        Mesh m = new Mesh();
        if(terrain.drawAsSphere)
            m.vertices = SquareToCircle(vertexList, center).ToArray();
        else
            m.vertices = vertexList.ToArray();
        m.triangles = triangles.ToArray();
        m.RecalculateNormals();
        return m;
    }

    int GetNeighbourIndex(int3x2 key)
    {
        int3 index = TerrainManagerData.axisIndex[axisID];
        bool[] face = { Mathf.Abs(key.c0[index.x]) == terrain.chunkDetail && Mathf.Abs(key.c1[index.x]) == terrain.chunkDetail,     // 0
                        Mathf.Abs(key.c0[index.y]) == terrain.chunkDetail && Mathf.Abs(key.c1[index.y]) == terrain.chunkDetail,     // 1
                        Mathf.Abs(key.c0[index.z]) == terrain.chunkDetail && Mathf.Abs(key.c1[index.z]) == terrain.chunkDetail,     // 2
                        key.c0[index.x] == 0 && key.c1[index.x] == 0,                                                               // 3
                        key.c0[index.y] == 0 && key.c1[index.y] == 0,                                                               // 4
                        key.c0[index.z] == 0 && key.c1[index.z] == 0                                                                // 5
        };

        if (face[0]){
            if (face[1])        return 6;
            else if (face[4])   return 8;
            else if (face[2])   return 10;
            else if (face[5])   return 12;
            else                return 0;
        }
        else if (face[3]){
            if (face[1])        return 7;
            else if (face[4])   return 9;
            else if (face[2])   return 11;
            else if (face[5])   return 13;
            else                return 3;
        }
        else if (face[1]){
            if (face[2])        return 14;
            else if (face[5])   return 16;
            else                return 1;
        }
        else if (face[4]){
            if (face[2])        return 15;
            else if (face[5])   return 17;
            else                return 4;
        }
        else if (face[2])       return 2;
        else if (face[5])       return 5;
        return -1;
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

    bool IsSignEqual(float v1, float v2)
    {
        return Mathf.Sign(v1) == Mathf.Sign(v2);
    }

    bool AddCubes(GetOtherEdgeData data, int3x2 thisEdge, ref Node[] neighbors, CubeEdge ce)
    {
        int2 index = data.index;
        int2 dir = data.dir;
        int3 cubeindex = data.cubesIndex;
        int3x2 otherEdge = thisEdge;
        int3x3 a = (int3x3)TerrainManagerData.dir[axisID];
        int3 dif;
        List<int3> cubes = new List<int3>();
        List<float3> cPoints = new List<float3>();

        if (cubeindex.x < 6)
        {
            if (neighbors[cubeindex.x] == null)
                return false;
            dif = a[index.x] * dir.x;
            otherEdge[0] = thisEdge[0] - (terrain.chunkDetail * dif);
            otherEdge[1] = thisEdge[1] - (terrain.chunkDetail * dif);
            neighbors[cubeindex.x].data.getEdgeCubes(otherEdge, ref cubes, ref cPoints, dif, level, axisID);
        }
        else
        {
            if (neighbors[cubeindex.x] == null || neighbors[cubeindex.y] == null || neighbors[cubeindex.z] == null || index.x == index.y)
                return false;
            dif = a[index.x] * dir.x;
            otherEdge[0] = thisEdge[0] - (terrain.chunkDetail * dif);
            otherEdge[1] = thisEdge[1] - (terrain.chunkDetail * dif);
            neighbors[cubeindex.y].data.getEdgeCubes(otherEdge, ref cubes, ref cPoints, dif, level, axisID);

            dif = a[index.y] * dir.y;
            otherEdge[0] = thisEdge[0] - (terrain.chunkDetail * dif);
            otherEdge[1] = thisEdge[1] - (terrain.chunkDetail * dif);
            neighbors[cubeindex.z].data.getEdgeCubes(otherEdge, ref cubes, ref cPoints, dif, level, axisID);

            dif = (a[index.y] * dir.y) + (a[index.x] * dir.x);
            otherEdge[0] = (terrain.chunkDetail * a[index.x] * dir.x) + (terrain.chunkDetail * a[index.y] * dir.y);
            otherEdge[0] = thisEdge[0] - otherEdge[0];

            otherEdge[1] = (terrain.chunkDetail * a[index.x] * dir.x) + (terrain.chunkDetail * a[index.y] * dir.y);
            otherEdge[1] = thisEdge[1] - otherEdge[1];
            neighbors[cubeindex.x].data.getEdgeCubes(otherEdge, ref cubes, ref cPoints, dif, level, axisID);
        }
        if (ce.cubes.Count + cubes.Count != 4)
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

    public override void getEdgeCubes(int3x2 v, ref List<int3> c, ref List<float3> p, int3 dif, int otherLOD, int otherAxisID)
    {
        if (edges == null) return;
        if (level < otherLOD)
            return;
        if (otherAxisID != axisID)
            RotateValues(ref v, ref dif, otherAxisID);
        int3 t = dif * (terrain.chunkDetail);
        float d = (terrain.maxResolution * terrain.reescaleValues[terrain.levelsOfDetail - 1 - level]);
        Vector3 temp = new Vector3(dif.x * d, dif.y * d, dif.z * d);
        if (edges.ContainsKey(v))
        {
            //if (edges[v].passed)
            //    return;
            for (int i = 0; i < edges[v].cubes.Count; i++)
            {
                c.Add(edges[v].cubes[i] + t);
                p.Add(vertexList[pointsSquare[edges[v].cubes[i]]] + temp);
            }
        }
    }

    float3 ChangeAxis(float3 v, int otherAxis)
    {
        float3 temp = float3.zero;

        int3 axisIndex = TerrainManagerData.axisIndex[otherAxis];

        return temp;
    }

    void RotateValues(ref int3x2 v, ref int3 dif, int otherAxisID)
    {
        int3x2 temp = new int3x2(0, 0, 0, 0, 0, 0);
        int3 axisIndex = TerrainManagerData.axisIndex[otherAxisID];
        temp.c0.x = Mathf.Abs(v.c0[axisIndex.x]);
        temp.c0.y = Mathf.Abs(v.c0[axisIndex.y]);
        temp.c0.z = Mathf.Abs(v.c0[axisIndex.z]);

        temp.c1.x = Mathf.Abs(v.c1[axisIndex.x]);
        temp.c1.y = Mathf.Abs(v.c1[axisIndex.y]);
        temp.c1.z = Mathf.Abs(v.c1[axisIndex.z]);

        int3 tempDif = new int3(0, 0, 0);
        tempDif.x = Mathf.Abs(dif[axisIndex.x]) * (int)(Mathf.Sign(TerrainManagerData.dirMult[otherAxisID][axisIndex.x]) * Mathf.Sign(dif[axisIndex.x]));
        tempDif.y = Mathf.Abs(dif[axisIndex.y]) * (int)(Mathf.Sign(TerrainManagerData.dirMult[otherAxisID][axisIndex.y]) * Mathf.Sign(dif[axisIndex.y]));
        tempDif.z = Mathf.Abs(dif[axisIndex.z]) * (int)(Mathf.Sign(TerrainManagerData.dirMult[otherAxisID][axisIndex.z]) * Mathf.Sign(dif[axisIndex.z]));

        axisIndex = TerrainManagerData.axisIndex[axisID];
        v.c0[axisIndex.x] = temp.c0.x * TerrainManagerData.dirMult[axisID][axisIndex.x];
        v.c0[axisIndex.y] = temp.c0.y * TerrainManagerData.dirMult[axisID][axisIndex.y];
        v.c0[axisIndex.z] = temp.c0.z * TerrainManagerData.dirMult[axisID][axisIndex.z];

        v.c1[axisIndex.x] = temp.c1.x * TerrainManagerData.dirMult[axisID][axisIndex.x];
        v.c1[axisIndex.y] = temp.c1.y * TerrainManagerData.dirMult[axisID][axisIndex.y];
        v.c1[axisIndex.z] = temp.c1.z * TerrainManagerData.dirMult[axisID][axisIndex.z];

        dif[axisIndex.x] = tempDif.x * TerrainManagerData.dirMult[axisID][axisIndex.x];
        dif[axisIndex.y] = tempDif.y * TerrainManagerData.dirMult[axisID][axisIndex.y];
        dif[axisIndex.z] = tempDif.z * TerrainManagerData.dirMult[axisID][axisIndex.z];

    }

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

public static class DualContouringData
{
    public static readonly GetOtherEdgeData[] otherEdgeData = {
        new GetOtherEdgeData(new int2(0, 0), new int2(1, 0),    new int3(0, 0, 0)),
        new GetOtherEdgeData(new int2(1, 0), new int2(1, 0),    new int3(1, 0, 0)),
        new GetOtherEdgeData(new int2(2, 0), new int2(1, 0),    new int3(2, 0, 0)),
        new GetOtherEdgeData(new int2(0, 0), new int2(-1, 0),   new int3(3, 0, 0)),
        new GetOtherEdgeData(new int2(1, 0), new int2(-1, 0),   new int3(4, 0, 0)),
        new GetOtherEdgeData(new int2(2, 0), new int2(-1, 0),   new int3(5, 0, 0)),
        new GetOtherEdgeData(new int2(0, 1), new int2(1, 1),    new int3(6, 0, 1)),
        new GetOtherEdgeData(new int2(0, 1), new int2(-1, 1),   new int3(7, 3, 1)),
        new GetOtherEdgeData(new int2(0, 1), new int2(1, -1),   new int3(8, 0, 4)),
        new GetOtherEdgeData(new int2(0, 1), new int2(-1, -1),  new int3(9, 3, 4)),
        new GetOtherEdgeData(new int2(0, 2), new int2(1, 1),    new int3(10, 0, 2)),
        new GetOtherEdgeData(new int2(0, 2), new int2(-1, 1),   new int3(11, 3, 2)),
        new GetOtherEdgeData(new int2(0, 2), new int2(1, -1),   new int3(12, 0, 5)),
        new GetOtherEdgeData(new int2(0, 2), new int2(-1, -1),  new int3(13, 3, 5)),
        new GetOtherEdgeData(new int2(1, 2), new int2(1, 1),    new int3(14, 1, 2)),
        new GetOtherEdgeData(new int2(1, 2), new int2(-1, 1),   new int3(15, 4, 2)),
        new GetOtherEdgeData(new int2(1, 2), new int2(1, -1),   new int3(16, 1, 5)),
        new GetOtherEdgeData(new int2(1, 2), new int2(-1, -1),  new int3(17, 4, 5)),
    };
}

public class GetOtherEdgeData
{
    public int2 index;
    public int2 dir;
    public int3 cubesIndex;

    public GetOtherEdgeData(int2 id, int2 d, int3 c)
    {
        index = id;
        dir = d;
        cubesIndex = c;
    }
}

