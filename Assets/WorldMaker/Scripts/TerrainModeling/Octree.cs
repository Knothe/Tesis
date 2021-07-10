using Unity.Mathematics;
using UnityEngine;

/// <summary> Node of the face Octree </summary>
public class Node
{
    /// <summary> Voxel data </summary>
    public Algorithm data;
    /// <summary> Level of detail </summary>
    public int level {  get; private set; }
    /// <summary> Face axis </summary>
    public int axisID { get; private set; }
    /// <summary> World position in cube form </summary>
    public float3 cubePosition { get; private set; }
    /// <summary> World position in sphere form </summary>
    public float3 spherePosition { get; private set; }
    /// <summary> Chunk center point in cube form </summary>
    public float3 center { get; private set; }
    /// <summary> Chunk center point in sphere form </summary>
    public float3 sphereCenter { get; private set; }
    /// <summary> Chunk "normal" in sphere form </summary>
    public Vector3 sphereChunkDirection { get; private set; }
    /// <summary> Parent Node, null if is lowest detail </summary>
    public Node parentNode { get; private set; }
    /// <summary> Node neighbors </summary>
    public Node[] neighbors { get; private set; }
    /// <summary> Child nodes of node, null if is biggest detail </summary>
    public Node[] childs { get; private set; }
    /// <summary> Face index location </summary>
    public int3 faceLocation { get; private set; }
    /// <summary> Node is active in world </summary>
    public bool isActive { get; set; }
    /// <summary> In game object representing this node </summary>
    public Chunk inGameChunk { get; set; }
    /// <summary> Node is visible by player </summary>
    public bool isVisible { get; private set; }

    public Node(Node p, int l, int axis, int3 pos, float3 middlePos,TerrainInfo t, int cp)
    {
        inGameChunk = null;
        childs = null;
        parentNode = p;
        level = l;
        axisID = axis;
        faceLocation = pos;
        AddData(t);
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

    /// <summary>
    /// Transforms a point from cube form to sphere form
    /// </summary>
    /// <param name="cubePoint">Point to transform</param>
    /// <returns>Transformed point</returns>
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

    /// <summary>
    /// Generates Algorithm class
    /// </summary>
    /// <param name="t">Planet Terrain data</param>
    void AddData(TerrainInfo t)
    {
        if (t.isMarchingCube)
        {
            data = new MarchingCubesAlgorithm(t, axisID, level);
            neighbors = new Node[6];
        }
        else
        {
            data = new DualContouringAlgorithm(t, axisID, level);
            neighbors = new Node[18];
        }
    }

    /// <summary>
    /// Generates voxel data in data object
    /// </summary>
    /// <returns>True if node has terrain to show</returns>
    public bool GenerateVoxelData()
    {
        return data.GenerateVoxelData(cubePosition);
    }

    /// <summary>
    /// Checks if node is visible by the player
    /// </summary>
    /// <returns>True if it's visible</returns>
    public bool IsVisible()
    {
        if (!data.terrain.showAll)
        {
            Vector3 dif = data.terrain.playerRelativePosition.normalized - sphereChunkDirection;
            isVisible = dif.magnitude < 1.414f;
        }
        else
            isVisible = true;
        
        return isVisible;
    }

    /// <summary>
    /// Generates node mesh
    /// </summary>
    public void GenerateMesh()
    {
        SetNeighbors();
        if (inGameChunk == null)
            return;
        if(data.terrain.levelsOfDetail == level + 1)
            inGameChunk.SetMeshLast(data.GenerateMesh(cubePosition, neighbors));
        else
            inGameChunk.SetMesh(data.GenerateMesh(cubePosition, neighbors));
    }

    /// <summary>
    /// Searchs node neighbors
    /// </summary>
    public void SetNeighbors()
    {
        int reescale = data.terrain.reescaleValues[(data.terrain.levelsOfDetail - 1) - level];
        for (int i = 0; i < neighbors.Length; i++)
            neighbors[i] = data.terrain.GetNode(axisID, level, faceLocation + (TerrainManagerData.neigborCells[i] * reescale));
    }

    /// <summary>
    /// Checks if the node is in correct level of detail distance
    /// </summary>
    /// <returns>True if is in the right distance for its level of detail</returns>
    public bool CheckAvailability()
    {
        float dist = (data.terrain.GetPlayerRelativePosition() - (Vector3)sphereCenter).magnitude;
        return dist >= data.terrain.GetLoDDistance(level);
    }

    /// <summary>
    /// Generates node childs
    /// </summary>
    public void GenerateChilds()
    {
        if (childs != null)
            return;
        isActive = false;
        childs = new Node[8];
        int newLevel = level + 1;
        if (data.terrain.levelsOfDetail == newLevel)
            return;
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

    /// <summary>
    /// Generates ID Value
    /// </summary>
    /// <returns>Generated ID value</returns>
    public int4 GetIDValue()
    {
        return new int4(faceLocation, axisID);
    }

}
