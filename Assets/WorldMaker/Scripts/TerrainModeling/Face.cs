using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

/// <summary>
/// Manages a part of the planet, each planet has 6 faces
/// </summary>
public class Face
{
    /// <summary> In game parent object </summary>
    public Transform parent { get; private set; }
    /// <summary> Face axis </summary>
    int axisID;
    /// <summary> Planet terrain data </summary>
    TerrainInfo terrain;
    /// <summary> Nodes active in planet </summary>
    public List<Node> activeNodes { get; set; }
    /// <summary> Face Octree for level of detail management </summary>
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

    /// <summary> Generates voxel data for all visible and active chunks at the start of the program </summary>
    /// <param name="chunkHeight">Number of chunks generated in its z axis</param>
    public void GenerateChunks(int chunkHeight)
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

    /// <summary>
    /// Generates chunk mesh for all chunks with terrain
    /// </summary>
    /// <param name="visibilityLimitNodes">List of nodes in the limit of visibility</param>
    /// <param name="detailLimitNode">List of nodes in the limit of detail</param>
    /// <param name="biggestDetailList">List of nodes in the biggest level of detail</param>
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

    /// <summary>
    /// Generates a starting node in the octree
    /// </summary>
    /// <param name="index">Index position in face</param>
    /// <param name="middleAdd">Center of the chunk</param>
    void GenerateChunk(int3 index, float3 middleAdd)
    {
        Node temp = new Node(null, 0, axisID, index, middleAdd, terrain, 0);
        detailList.Add(index, temp);
        CheckDetail(temp);
    }

    /// <summary>
    /// Checks de detail of a node and either keeps generating childs or checks its voxel data
    /// </summary>
    /// <param name="n">Node to check</param>
    void CheckDetail(Node n)
    {
        if (n.CheckAvailability())
            GenerateChunk(n);
        else
        {
            n.GenerateChilds();
            foreach(Node node in n.childs)
                CheckDetail(node);
        }
    }

    /// <summary>
    /// Generates voxel data for selected node
    /// </summary>
    /// <param name="n">Node to modify</param>
    public void GenerateChunk(Node n)
    {
        if(terrain.terrainManager.GenerateChunk(this, n))
            if (activeNodes != null)
                activeNodes.Add(n);
    }

    /// <summary>
    /// Searchs for a node in this face
    /// </summary>
    /// <param name="myLevel">Asking chunk level</param>
    /// <param name="wantedPos">Wanted chunk</param>
    /// <returns>Found chunk, can be the wanted one or a lower level of detail</returns>
    public Node GetNode(int myLevel, int3 wantedPos)
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
        Node temp = null;
        if (detailList.ContainsKey(t))
            temp = detailList[t];
        while(true)
        {
            if (temp == null || (temp.level == myLevel && !temp.isActive) || temp.isActive)
                break;
            reescale = (terrain.levelsOfDetail - 2) - temp.level;
            temp = GetChilds(temp, wantedPos, terrain.reescaleValues[reescale]);
        }
        return temp;
    }

    /// <summary>
    /// Searchs for an indicated child acording to an aproximation in its index
    /// </summary>
    /// <param name="n">Parent node</param>
    /// <param name="wanted">Index of wanted node</param>
    /// <param name="reescale">Reescalation value in index for aproximation</param>
    /// <returns>A node if found, null if not</returns>
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
