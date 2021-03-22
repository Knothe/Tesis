using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class Face
{
    public Transform parent { get; private set; }
    int axisID;
    TerrainInfo terrain;

    public List<Chunk> inactiveChunks { get; set; }

    public List<Node> activeNodes { get; set; }

    Dictionary<int3, Node> detailList { get; set; }

    public Face(int a, TerrainInfo t, Transform p)
    {
        terrain = t;
        axisID = a;
        activeNodes = new List<Node>();
        detailList = new Dictionary<int3, Node>();
        GameObject g = new GameObject("Face" + axisID);
        g.transform.parent = p;
        g.transform.localPosition = Vector3.zero;
        g.transform.localRotation = Quaternion.identity;
        parent = g.transform;
    }

    public void GenerateChunks(Material m, int chunkHeight)     // Cambiar
    {
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
                for(temp.z = -chunkHeight; temp.z < chunkHeight; temp.z++)
                {
                    squarePoint = temp * reescale;
                    if (!detailList.ContainsKey(squarePoint))
                        GenerateChunk(squarePoint, middlePoint);
                }
            }
        }
    }

    public void GenerateMesh(ref Dictionary<int4, Node> visibilityLimitNodes, ref Dictionary<int4, Node> detailLimitNode, ref Dictionary<int4, Node> biggestDetailList)
    {
        foreach(Node node in activeNodes)
        {
            node.GenerateMesh();
            Node n;
            int4 k, key = node.GetIDValue();
            for(int i = 0; i < 6; i++)
            {
                n = node.neighbors[i];
                if (n == null)
                    continue;
                
                if (!n.IsVisible())
                {
                    k = n.GetIDValue();
                    if (!visibilityLimitNodes.ContainsKey(key))
                        visibilityLimitNodes.Add(key, node);
                    if (!visibilityLimitNodes.ContainsKey(k))
                        visibilityLimitNodes.Add(k, n);
                }
                else if (!detailLimitNode.ContainsKey(key) && (n.level != node.level || !n.isActive))
                    detailLimitNode.Add(key, node);
            }

            if(node.inGameChunk != null)
                if(node.level == terrain.levelsOfDetail - 1)
                    if(!biggestDetailList.ContainsKey(key))
                        biggestDetailList.Add(key, node);
        }
        
        activeNodes = null;
    }

    void GenerateChunk(int3 index, float3 middleAdd)
    {
        Node temp = new Node(null, 0, axisID, index, middleAdd, terrain, 0);
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

    public void GenerateChunk(Node n)
    {
        if(terrain.terrainManager.GenerateChunk(this, n))
            if (activeNodes != null)
                activeNodes.Add(n);
    }

    public void DesactivateChunk(Node n)
    {
        n.isActive = false;
        if (n.inGameChunk == null)
            return;
        n.inGameChunk.Desactivate();
        inactiveChunks.Add(n.inGameChunk);
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
        while(true)
        {
            if (temp == null)
                break;
            if (temp.level == myLevel && !temp.isActive)
            {
                if (temp.childs == null)
                {
                    //Debug.Log(myPos + ", " + wantedPos);
                }
                break;
            }
            if (temp.isActive)
                break;

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
