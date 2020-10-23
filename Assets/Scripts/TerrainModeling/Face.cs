using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class Face
{
    Transform parent;
    int axisID;
    TerrainInfo terrain;

    List<Chunk> chunkList;
    Dictionary<int3, Node> detailList;

    Material mat;

    public Face(int a, TerrainInfo t, Transform p)
    {
        terrain = t;
        axisID = a;
        chunkList = new List<Chunk>();
        detailList = new Dictionary<int3, Node>();
        GameObject g = new GameObject("Face");
        g.transform.parent = p;
        g.transform.localPosition = Vector3.zero;
        parent = g.transform;
    }

    public void GenerateChunks(Material m)
    {
        mat = m;
        chunkList.Clear();
        int resolution = terrain.minChunkPerFace;
        if (resolution <= 0)
            return;
        int3 t = TerrainManagerData.dirMult[axisID];
        int3x3 axis = (int3x3)TerrainManagerData.dir[axisID];
        int reescale = terrain.reescaleValues[terrain.levelsOfDetail - 1];
        float3 middlePoint = new float3(.5f, .5f, .5f) * reescale * t;
        int3 squarePoint;
        for (int3 temp = int3.zero; temp.x < resolution; temp.x++)
        {
            for(temp.y = 0; temp.y < resolution; temp.y++)
            {
                for(temp.z = -resolution; temp.z < resolution; temp.z++)
                {
                    squarePoint = temp * reescale;
                    //squarePoint = temp * reescale;
                    //point = terrain.faceStart[axisID] + ((float3)squarePoint * res);
                    if (!detailList.ContainsKey(squarePoint))
                        GenerateChunk(squarePoint, middlePoint);
                    //else
                        //detailList[temp * t * reescale].UpdateChunkData();
                }
            }
        }
    }

    public void GenerateMesh()
    {
        foreach(Chunk c in chunkList)
        {
            c.SetMesh();
        }
    }

    void GenerateChunk(int3 index, float3 middleAdd)
    {
        Node temp = new Node(null, 0, axisID, index, middleAdd, terrain);
        detailList.Add(index, temp);
        CheckDetail(temp);
    }

    void CheckDetail(Node n)
    {
        if (n.CheckAvailability())
        {
            GenerateChunk(n);
        }
        else
        {
            n.GenerateChilds();
            foreach(Node node in n.childs)
                CheckDetail(node);
        }
    }

    void GenerateChunk(Node n)
    {
        if (n.GenerateVoxelData())
        {
            GameObject g = new GameObject("chunk", typeof(MeshRenderer), typeof(MeshFilter), typeof(MeshCollider), typeof(Chunk));
            g.transform.parent = parent;
            Chunk c = g.GetComponent<Chunk>();
            c.Initialize(n, mat);
            chunkList.Add(c);
        }
    }
}
