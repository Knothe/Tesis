using Unity.Mathematics;
using UnityEngine;

public class Node
{
    public Algorithm data;
    public int level {  get; private set; }
    int axisID;

    public float3 cubePosition { get; private set; }
    public float3 spherePosition { get; private set; }
    public float3 center { get; private set; }
    public float3 sphereCenter { get; private set; }

    Node parentNode;
    Node[] neighbors;

    public Node[] childs { get; private set; }
    public int3 faceLocation { get; private set; }
    public bool isActive { get; private set; }

    public Node(Node p, int l, int axis, int3 pos, float3 middlePos,TerrainInfo t, int cp)
    {
        childs = null;
        parentNode = p;
        level = l;
        axisID = axis;
        faceLocation = pos;
        AddData(axis, t, cp);
        middlePos += pos;
        cubePosition = t.faceStart[axisID] + (pos.x * t.resolutionVectors[axisID].c0) + (pos.y * t.resolutionVectors[axisID].c1) + (pos.z * t.resolutionVectors[axisID].c2);
        center = t.faceStart[axisID] + (middlePos.x * t.resolutionVectors[axisID].c0) + (middlePos.y * t.resolutionVectors[axisID].c1) + (middlePos.z * t.resolutionVectors[axisID].c2);
        spherePosition = CubeToSphere(cubePosition);
        sphereCenter = CubeToSphere(center);
        isActive = false;
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
        isActive = true;
        return data.GenerateVoxelData(cubePosition);
    }

    public Mesh GenerateMesh()
    {
        //isActive = true;
        int reescale = data.terrain.reescaleValues[(data.terrain.levelsOfDetail - 1) - level];
        for (int i = 0; i < neighbors.Length; i++)
            neighbors[i] = data.terrain.GetNode(axisID, level, faceLocation, faceLocation + (TerrainManagerData.neigborCells[i] * reescale));
        return data.GenerateMesh(cubePosition, neighbors);
    }

    public bool IsDivision()
    {
        foreach(Node n in neighbors)
        {
            if (n != null)
                if (n.level != level || n.childs != null)
                    return true;
        }
            
        return false;
    }

    public bool CheckAvailability()
    {
        float dist = (data.terrain.GetPlayerRelativePosition() - (Vector3)sphereCenter).magnitude;
        if(level == 0)
            return dist > 2;
        else
            return true;
    }

    public void GenerateChilds()
    {
        childs = new Node[8];
        int newLevel = level + 1;
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



}
