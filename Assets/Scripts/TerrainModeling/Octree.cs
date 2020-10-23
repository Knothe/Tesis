using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Animations;

public class Octree
{
    Node parentNode;

    Node[] childs = new Node[8];
}

public class Node
{
    public Algorithm data;
    public int level {  get; private set; }
    int axisID;

    public float3 cubePosition { get; private set; }
    public float3 spherePosition { get; private set; }
    float3 center;
    float3 sphereCenter;

    Node parentNode;
    public Node[] childs { get; private set; }
    int3 faceLocation;

    public Node(int l, int axis, int3 i, float res, TerrainInfo t)
    {
        level = l;
        axisID = axis;
        faceLocation = i;
        AddData(axis, t);
        parentNode = null;
        cubePosition = data.terrain.faceStart[axisID] + ((float3)i * res);
    }

    public Node(Node p, int l, int axis, int3 i, float3 middleAdd,TerrainInfo t)
    {
        parentNode = p;
        level = l;
        axisID = axis;
        faceLocation = i;
        AddData(axis, t);
        cubePosition = t.faceStart[axisID] + (i.x * t.resolutionVectors[axisID].c0) + (i.y * t.resolutionVectors[axisID].c1) + (i.z * t.resolutionVectors[axisID].c2);
        center = cubePosition + middleAdd;
        spherePosition = CubeToSphere(cubePosition);
        sphereCenter = CubeToSphere(center);
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

    void AddData(int axis, TerrainInfo t)
    {
        if (t.isMarchingCube)
            data = new MarchingCubesAlgorithm(t, axis, level);
        else
            data = new DualContouringAlgorithm(t, axis, level);
    }

    public bool GenerateVoxelData()
    {
        return data.GenerateVoxelData(cubePosition);
    }

    public Mesh GenerateMesh()
    {
        return data.GenerateMesh(cubePosition);
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
        int3 t = TerrainManagerData.dirMult[axisID];
        int reescale = data.terrain.reescaleValues[(data.terrain.levelsOfDetail - 1) - newLevel];
        float3 middlePoint = new float3(.5f, .5f, .5f) * reescale;
        childs[0] = new Node(this, newLevel, axisID, faceLocation, middlePoint, data.terrain);
        childs[1] = new Node(this, newLevel, axisID, faceLocation + (new int3(1, 0, 0) * t * reescale), middlePoint, data.terrain);
        childs[2] = new Node(this, newLevel, axisID, faceLocation + (new int3(0, 1, 0) * t * reescale), middlePoint, data.terrain);
        childs[3] = new Node(this, newLevel, axisID, faceLocation + (new int3(1, 1, 0) * t * reescale), middlePoint, data.terrain);

        childs[4] = new Node(this, newLevel, axisID, faceLocation + (new int3(0, 0, 1) * t * reescale), middlePoint, data.terrain);
        childs[5] = new Node(this, newLevel, axisID, faceLocation + (new int3(1, 0, 1) * t * reescale), middlePoint, data.terrain);
        childs[6] = new Node(this, newLevel, axisID, faceLocation + (new int3(0, 1, 1) * t * reescale), middlePoint, data.terrain);
        childs[7] = new Node(this, newLevel, axisID, faceLocation + (new int3(1, 1, 1) * t * reescale), middlePoint, data.terrain);
    }
}
