using Unity.Mathematics;
using UnityEngine;

public class Node
{
    public Algorithm data;
    public int level {  get; private set; }
    public int axisID { get; private set; }

    public float3 cubePosition { get; private set; }
    public float3 spherePosition { get; private set; }
    public float3 center { get; private set; }
    public float3 sphereCenter { get; private set; }
    public Vector3 sphereChunkDirection { get; private set; }

    public Node parentNode { get; private set; }
    public Node[] neighbors { get; private set; }

    public Node[] childs { get; private set; }
    public int3 faceLocation { get; private set; }
    public bool isActive { get; set; }
    public Chunk inGameChunk { get; set; }
    public bool isVisible { get; private set; }


    public Node(Node p, int l, int axis, int3 pos, float3 middlePos,TerrainInfo t, int cp)
    {
        inGameChunk = null;
        childs = null;
        parentNode = p;
        level = l;
        axisID = axis;
        faceLocation = pos;
        AddData(axis, t, cp);
        middlePos += pos;
        float3 temp = float3.zero;
        temp[TerrainManagerData.axisIndex[axis][0]] = pos.x * t.resolutionVectors[axisID].x;
        temp[TerrainManagerData.axisIndex[axis][1]] = pos.y * t.resolutionVectors[axisID].y;
        temp[TerrainManagerData.axisIndex[axis][2]] = pos.z * t.resolutionVectors[axisID].z;
        cubePosition = t.faceStart[axisID] + temp;

        temp[TerrainManagerData.axisIndex[axis][0]] = middlePos.x * t.resolutionVectors[axisID].x;
        temp[TerrainManagerData.axisIndex[axis][1]] = middlePos.y * t.resolutionVectors[axisID].y;
        temp[TerrainManagerData.axisIndex[axis][2]] = middlePos.z * t.resolutionVectors[axisID].z;
        center = t.faceStart[axisID] + temp;

        spherePosition = CubeToSphere(cubePosition);
        sphereCenter = CubeToSphere(center);
        isActive = true;
    }

    float3 CubeToSphere(float3 cubePoint)
    {
        Vector3 temp = cubePoint;
        float3 up = TerrainManagerData.dir[axisID].c2;
        float3 ax = TerrainManagerData.dir[axisID].c0 + TerrainManagerData.dir[axisID].c1;
        ax.x = Mathf.Abs(ax.x);
        ax.y = Mathf.Abs(ax.y);
        ax.z = Mathf.Abs(ax.z);
        float height = (temp.x * up.x) + (temp.y * up.y) + (temp.z * up.z);
        temp = (temp * ax) + (up * data.terrain.planetRadius);
        temp = temp.normalized * height;
        sphereChunkDirection = temp.normalized;
        return temp;
    }

    void AddData(int axis, TerrainInfo t, int cp)
    {
        if (t.isMarchingCube)
        {
            data = new MarchingCubesAlgorithm(t, axis, level, cp);
            neighbors = new Node[6];
        }
        else
        {
            data = new DualContouringAlgorithm(t, axis, level, cp);
            neighbors = new Node[18];
        }
    }

    public bool GenerateVoxelData()
    {
        //if (IsVisible())
        //    return false;
        return data.GenerateVoxelData(cubePosition);
    }

    public bool IsVisible()
    {
        if (!data.terrain.showAll)
        {
            Vector3 dif = data.terrain.playerRelativePosition.normalized - sphereChunkDirection;
            isVisible = dif.magnitude < 1.414f;
        }
        else
            isVisible = true;
        
        return isVisible; // sqrt(2)
    }

    public void GenerateMesh2() // Cambiar nombre
    {
        SetNeighbors();
        if (inGameChunk != null)
            inGameChunk.SetMesh(data.GenerateMesh(cubePosition, neighbors));
    }

    public void SetNeighbors()
    {
        int reescale = data.terrain.reescaleValues[(data.terrain.levelsOfDetail - 1) - level];
        for (int i = 0; i < neighbors.Length; i++)
            neighbors[i] = data.terrain.GetNode(axisID, level, faceLocation, faceLocation + (TerrainManagerData.neigborCells[i] * reescale));
    }

    public int IsDivision()
    {
        foreach(Node n in neighbors)
        {
            if (n != null)
            {
                if (n.level != level || n.childs != null)
                    return 1;   // Divide niveles de detalle
                else if (!n.IsVisible())
                    return 2;   // Divide prendido y apagado
            }
        }
        return 0; // No es división
    }

    public bool CheckAvailability()
    {
        float dist = (data.terrain.GetPlayerRelativePosition() - (Vector3)sphereCenter).magnitude;
        return dist >= data.terrain.GetLoDDistance(level);
    }

    public void GenerateChilds()
    {
        if (childs != null)
            return;
        isActive = false;
        childs = new Node[8];
        int newLevel = level + 1;
        if (data.terrain.levelsOfDetail == newLevel)
            return;
        //int3 t = TerrainManagerData.dirMult[axisID];
        int reescale = data.terrain.reescaleValues[(data.terrain.levelsOfDetail - 1) - newLevel];
        float3 middlePoint = new float3(.5f, .5f, .5f) * reescale;
        childs[0] = new Node(this, newLevel, axisID, faceLocation, middlePoint, data.terrain, 0);
        childs[1] = new Node(this, newLevel, axisID, faceLocation + (new int3(1, 0, 0) * reescale), middlePoint, data.terrain, 1);
        childs[2] = new Node(this, newLevel, axisID, faceLocation + (new int3(0, 1, 0) * reescale), middlePoint, data.terrain, 2);
        childs[3] = new Node(this, newLevel, axisID, faceLocation + (new int3(1, 1, 0) * reescale), middlePoint, data.terrain, 3);

        childs[4] = new Node(this, newLevel, axisID, faceLocation + (new int3(0, 0, 1) * reescale), middlePoint, data.terrain, 4);
        childs[5] = new Node(this, newLevel, axisID, faceLocation + (new int3(1, 0, 1) * reescale), middlePoint, data.terrain, 5);
        childs[6] = new Node(this, newLevel, axisID, faceLocation + (new int3(0, 1, 1) * reescale), middlePoint, data.terrain, 6);
        childs[7] = new Node(this, newLevel, axisID, faceLocation + (new int3(1, 1, 1) * reescale), middlePoint, data.terrain, 7);
    }

    public void SetChunk(Chunk c)
    {
        inGameChunk = c;
    }

    public int4 GetIDValue()
    {
        //return new int4((int3)cubePosition, axisID);
        return new int4(faceLocation, axisID);
    }

}
