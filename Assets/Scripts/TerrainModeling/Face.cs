using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class Face
{
    Transform parent;
    int axisID;
    TerrainInfo terrain;

    public List<Chunk> activeChunks;
    public List<Chunk> inactiveChunks;
    public List<Chunk> updateableChunks;

    public List<Node> activeNodes;

    Dictionary<int3, Node> detailList;

    Material mat;

    public Face(int a, TerrainInfo t, Transform p)
    {
        terrain = t;
        axisID = a;
        activeChunks = new List<Chunk>();
        inactiveChunks = new List<Chunk>();
        updateableChunks = new List<Chunk>();
        activeNodes = new List<Node>();
        detailList = new Dictionary<int3, Node>();
        GameObject g = new GameObject("Face" + axisID);
        g.transform.parent = p;
        g.transform.localPosition = Vector3.zero;
        parent = g.transform;
    }

    public void GenerateChunks(Material m)
    {
        mat = m;
        activeChunks.Clear();
        int resolution = terrain.minChunkPerFace;
        if (resolution <= 0)
            return;
        int reescale = terrain.reescaleValues[terrain.levelsOfDetail - 1];
        float3 middlePoint = new float3(.5f, .5f, .5f) * reescale;
        int3 squarePoint;
        for (int3 temp = int3.zero; temp.x < resolution; temp.x++)
        {
            for(temp.y = 0; temp.y < resolution; temp.y++)
            {
                for(temp.z = -1; temp.z < 1; temp.z++)
                {
                    squarePoint = temp * reescale;
                    if (!detailList.ContainsKey(squarePoint))
                        GenerateChunk(squarePoint, middlePoint);
                }
            }
        }
    }

    public void GenerateMesh(ref Dictionary<int4, Node> visibilityLimitNodes, ref Dictionary<int4, Node> detailLimitNode)
    {
        foreach(Node node in activeNodes)
        {
            node.GenerateMesh2();
            Node n;
            int4 k, key = node.GetIDValue();
            if (node.faceLocation.x == 32 && node.faceLocation.y == 42 && node.faceLocation.z == -2)
                k = int4.zero;
            for(int i = 0; i < 6; i++)
            {
                n = node.neighbors[i];
                if (n == null)
                {
                    continue;
                }
                else if (!n.IsVisible())
                {
                    k = n.GetIDValue();
                    if (!visibilityLimitNodes.ContainsKey(key))
                        visibilityLimitNodes.Add(key, node);
                    if (!visibilityLimitNodes.ContainsKey(k))
                        visibilityLimitNodes.Add(k, n);
                }
                else if (!detailLimitNode.ContainsKey(key) && (n.level != node.level || !n.isActive))
                {
                    //k = n.GetIDValue();
                    detailLimitNode.Add(key, node);
                    if (node.inGameChunk != null)
                        node.inGameChunk.IsLimit();
                    //if (!detailLimitNode.ContainsKey(k))
                    //{
                    //    detailLimitNode.Add(k, n);
                    //    if(n.inGameChunk != null)
                    //        n.inGameChunk.IsLimit();
                    //}
                }
            }
        }
        activeNodes = null;
    }

    void GenerateChunk(int3 index, float3 middleAdd)
    {
        Node temp = new Node(null, 0, axisID, index, middleAdd, terrain, 0);
        detailList.Add(index, temp);
        //GenerateChunk(temp);
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

    public void GenerateChunk(Node n)
    {
        if (n.IsVisible())
        {
            if (activeNodes != null)
                activeNodes.Add(n);
            if (n.GenerateVoxelData() && n.inGameChunk == null)
            {
                GameObject g;
                Chunk c;
                if (inactiveChunks.Count > 0)
                {
                    c = inactiveChunks[0];
                    g = c.gameObject;
                    inactiveChunks.RemoveAt(0);
                }
                else
                {
                    g = new GameObject("chunk: " + n.faceLocation, typeof(MeshRenderer), typeof(MeshFilter), typeof(MeshCollider), typeof(Chunk));
                    c = g.GetComponent<Chunk>();
                }

                g.transform.parent = parent;
                c.Initialize(n, mat);
                n.SetChunk(c);
            }
        }
    }

    public void DesactivateChunk(Node n)
    {
        if (n.inGameChunk == null)
            return;
        Chunk c = n.inGameChunk;
        c.gameObject.SetActive(false);
        inactiveChunks.Add(c);
        n.inGameChunk = null;
    }

    public Node GetNode(int myLevel, int3 myPos, int3 wantedPos)
    {
        int reescale = terrain.reescaleValues[terrain.levelsOfDetail - 1];
        int3 t;
        if(wantedPos.z < 0)
        {
            int resto = wantedPos.z % reescale;
            if(resto == 0)
                t = new int3(wantedPos.x - Mathf.Abs(wantedPos.x % reescale),
                wantedPos.y - Mathf.Abs(wantedPos.y % reescale),
                wantedPos.z);
            else
                t = new int3(wantedPos.x - Mathf.Abs(wantedPos.x % reescale),
                wantedPos.y - Mathf.Abs(wantedPos.y % reescale),
                wantedPos.z - (reescale - Mathf.Abs(wantedPos.z % reescale)));
        }
        else
        {
            t = new int3(wantedPos.x - Mathf.Abs(wantedPos.x % reescale),
            wantedPos.y - Mathf.Abs(wantedPos.y % reescale),
            wantedPos.z - Mathf.Abs(wantedPos.z % reescale));
        }
        //t = wantedPos - new int3(wantedPos.x )
        Node temp = null;
        if (detailList.ContainsKey(t))
            temp = detailList[t];
        while(temp != null && !temp.isActive && temp.level != myLevel)
        {
            reescale = (terrain.levelsOfDetail - 2) - temp.level;
            temp = GetChilds(temp, wantedPos, terrain.reescaleValues[reescale]);
        }
        return temp;
    }

    Node GetChilds(Node n, int3 wanted, int reescale)
    {
        if (n.childs == null)
            return null;
        for(int i = 0; i < n.childs.Length; i++)
        {
            if(n.childs[i].faceLocation.x <= wanted.x && n.childs[i].faceLocation.x + reescale > wanted.x)
                if (n.childs[i].faceLocation.y <= wanted.y && n.childs[i].faceLocation.y + reescale > wanted.y)
                    if (n.childs[i].faceLocation.z <= wanted.z && n.childs[i].faceLocation.z + reescale > wanted.z)
                        return n.childs[i];
        }
        return null;
    }
}
