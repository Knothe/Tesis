using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions.Must;

public class TerrainManager : MonoBehaviour
{
    public Material defaultMaterial;
    [SerializeField]
    public TerrainInfo planetData;
    public PlanetsManager planetManager;

    Face[] faces = new Face[6];

    Dictionary<int4, Node> updateVisibilityNodes = new Dictionary<int4, Node>();
    Dictionary<int4, Node> detailLimitNode = new Dictionary<int4, Node>();
    Dictionary<int4, Node> biggestDetailList = new Dictionary<int4, Node>();
    float treeDistance;

    public Transform treeHoldersParent { get; private set; }

    public TerrainManager()
    {

    }


    private void Start()
    {
        planetData.SetTerrainManager(this);
        if(gameObject.name != "Planet")
            planetData.SetBiomes();
        GenerateTerrain();
    }

    private void Update()
    {
        planetData.SetPlayerRelativePosition();
        UpdateBiggestDetail();
        UpdateChunkDetail();
        UpdateVisibleChunks();
    }

    private void OnValidate()
    {
        if (planetData == null)
            return;

        planetData.OnValidate();
    }

    void UpdateBiggestDetail()
    {
        if (!planetData.instantiateTrees)
            return;
        float dif;
        foreach(KeyValuePair<int4, Node> key in biggestDetailList)
        {
            Node n = key.Value;
            if (n.inGameChunk == null)
                continue;
            dif = (n.inGameChunk.transform.position - planetData.player.transform.position).sqrMagnitude;
            if (dif < treeDistance)
            {
                if (!n.inGameChunk.isInTreeRange)
                {
                    planetData.ActivateTrees(n.faceLocation, n.axisID, n.cubePosition);
                    n.inGameChunk.isInTreeRange = true;
                }
            }
            else
            {
                if (n.inGameChunk.isInTreeRange)
                {
                    planetData.DesactivateTrees(n.faceLocation, n.axisID);
                    n.inGameChunk.isInTreeRange = false;
                }
            }
        }
    }

    void UpdateChunkDetail()
    {
        List<Node> added = new List<Node>();
        List<Node> remove = new List<Node>();
        int i, t;
        Node node;
        foreach (KeyValuePair<int4, Node> key in detailLimitNode)
        {
            if (!key.Value.isActive)
                continue;
            i = CheckNodeAvailability(key.Value);
            if (i == 1)
                IncrementDetail(key.Value, ref remove, ref added);
            else if (i == 2)
            {
                node = key.Value.parentNode;
                t = CheckNodeAvailability(node);
                if (t == 0 || t == 2)
                {
                    if (ReduceDetail(node, ref remove))
                    {
                        added.Add(node);
                        node.isActive = true;
                    }
                }
            }
        }

        foreach (Node n in remove)
            RemoveFromDetailLimit(n);

        bool isLimit, isLimit2;
        Node neighbor;
        foreach (Node n in added)
        {
            GenerateChunk(faces[n.axisID], n);
            n.GenerateMesh();
            AddToBiggestDetail(n);
            isLimit = false;
            for (i = 0; i < 6; i++)
            {
                neighbor = n.neighbors[i];
                if (neighbor == null)
                    continue;
                else if (n.level == neighbor.level)
                {
                    if (!neighbor.isActive)
                    {
                        isLimit = true;
                        if (neighbor.childs == null)
                        {
                            Debug.Log("Child Error");
                        }
                        else
                        {
                            for (int j = 0; j < 6; j++)
                            {
                                if (neighbor.childs[j] != null)
                                    AddToDetailLimit(neighbor.childs[j]);
                            }
                        }
                    }
                    else
                    {
                        isLimit2 = false;
                        neighbor.SetNeighbors();
                        for (int j = 0; j < 6; j++)
                        {
                            if (neighbor.neighbors[j] == null)
                                continue;
                            if (n.isVisible != neighbor.neighbors[j].isVisible)
                            {
                                isLimit2 = true;
                                AddToDetailLimit(neighbor.neighbors[j]);
                            }
                            if (isLimit2) AddToDetailLimit(neighbor);
                            else AddToDetailLimit(neighbor);
                        }
                    }
                }
                else
                {
                    AddToDetailLimit(n.neighbors[i]);
                    isLimit = true;
                }
            }
            if (isLimit)
                AddToDetailLimit(n);
        }

    }

    public bool GenerateChunk(Face f, Node n)
    {
        if (n.IsVisible())
        {
            if(n.GenerateVoxelData() && n.inGameChunk == null)
            {
                Chunk c = planetManager.GetChunk();
                c.gameObject.transform.parent = f.parent;
                c.Initialize(n, defaultMaterial);
                n.SetChunk(c);
            }
        }
        else
            return false;
        n.isActive = true;
        return true;
    }

    void IncrementDetail(Node node, ref List<Node> remove, ref List<Node> added)
    {
        DesactivateNode(node);
        remove.Add(node);
        node.GenerateChilds();
        foreach (Node n in node.childs)
        {
            added.Add(n);
            n.isActive = true;
        }
    }

    bool ReduceDetail(Node node, ref List<Node> remove)
    {
        foreach (Node n in node.childs)
        {
            if (!n.isActive)
                return false;
            if (CheckNodeAvailability(n) != 2)
                return false;
        }

        foreach (Node n in node.childs)
        {
            DesactivateNode(n);
            remove.Add(n);
        }
        return true;
    }

    void DesactivateNode(Node node)
    {
        node.isActive = false;
        if (node.inGameChunk == null)
            return;
        planetManager.DesactivateChunk(node);
    }

    void RemoveFromDetailLimit(Node n)
    {
        int4 id = n.GetIDValue();
        if (detailLimitNode.ContainsKey(id))
            detailLimitNode.Remove(id);
        if (biggestDetailList.ContainsKey(id))
            biggestDetailList.Remove(id);
    }

    void AddToBiggestDetail(Node n)
    {
        
        if (n.level == planetData.levelsOfDetail - 1)
            if (n.inGameChunk != null)
            {
                int4 id = n.GetIDValue();
                if (!biggestDetailList.ContainsKey(id))
                    biggestDetailList.Add(id, n);
            }
    }

    void AddToDetailLimit(Node n)
    {
        int4 id = n.GetIDValue();
        if (!detailLimitNode.ContainsKey(id))
            detailLimitNode.Add(id, n);
    }

    int CheckNodeAvailability(Node n)
    {
       float dist = (planetData.GetPlayerRelativePosition() - (Vector3)n.sphereCenter).magnitude;
        if (dist < planetData.GetLoDDistance(n.level))          // LoD increments
            return 1;
        if (dist >= planetData.GetLoDDistance(n.level - 1))     // LoD decreases
            return 2;
        return 0;
    }

    void UpdateVisibleChunks()
    {
        if (planetData.showAll)
            return;
        List<Node> changed = new List<Node>();
        List<Node> changedLoD = new List<Node>();
        foreach (KeyValuePair<int4, Node> key in updateVisibilityNodes)
        {
            if (!key.Value.isActive)
                changedLoD.Add(key.Value);
            else
                CheckNodeChange(key.Value, key.Key, ref changed);
        }

        foreach (Node node in changedLoD)
        {
            RemoveFromUpdateVisibility(node);
            if (node.parentNode != null && node.parentNode.isActive)
                CheckNodeChange(node.parentNode, node.parentNode.GetIDValue(), ref changed);
            else if(node.childs != null)
            {
                RemoveFromUpdateVisibility(node);
                for (int i = 0; i < 6; i++)
                    CheckNodeChange(node.childs[i], node.childs[i].GetIDValue(), ref changed);
            }
        }
        foreach(Node node in changed)
            CheckState(node);
    }

    void CheckNodeChange(Node n, int4 key, ref List<Node> changed)
    {
        if (n.IsVisible())
        {
            if (n.inGameChunk == null)
            {
                changed.Add(n);
                GenerateChunk(faces[key.w], n);
                n.GenerateMesh();
            }
        }
        else if (n.inGameChunk != null)
        {
            changed.Add(n);
            planetManager.DesactivateChunk(n);
        }
    }

    void CheckState(Node node)
    {
        bool isLimit = false;
        for(int i = 0; i < 6; i++)
        {
            if (node.neighbors[i] == null)
                return;
            if(node.isVisible == node.neighbors[i].isVisible)
            {
                bool isLimit2 = false;
                for(int j = 0; j < 6; j++)
                {
                    if (node.neighbors[i].neighbors[j] == null)
                        continue;
                    if(node.isVisible != node.neighbors[i].neighbors[j].isVisible)
                    {
                        isLimit2 = true;
                        AddToUpdateVisibility(node.neighbors[i].neighbors[j]);
                    }
                }
                if (isLimit2) AddToUpdateVisibility(node.neighbors[i]);
                else RemoveFromUpdateVisibility(node.neighbors[i]);
            }
            else
            {
                isLimit = true;
                AddToUpdateVisibility(node.neighbors[i]);
            }
            if (!isLimit)
                RemoveFromUpdateVisibility(node);
        }
    }

    void RemoveFromUpdateVisibility(Node n)
    {
        int4 id = n.GetIDValue();
        if (updateVisibilityNodes.ContainsKey(id))
            updateVisibilityNodes.Remove(id);
    }

    void AddToUpdateVisibility(Node n)
    {
        int4 id = n.GetIDValue();
        if (!updateVisibilityNodes.ContainsKey(id))
            updateVisibilityNodes.Add(id, n);
        if (n.level == 0)
            if (!detailLimitNode.ContainsKey(id))
                detailLimitNode.Add(id, n);
    }

    public void UpdateTerrain()
    {

    }

    public void GenerateFromManager()
    {
        planetData.SetTerrainManager(this, CalculateMinMaxdistance());
    }

    public void GenerateOnEditor()
    {
        Debug.Log("MaxDistance: " + CalculateMaxDistance());
        planetData.SetTerrainManager(this);
        planetData.SetBiomes();
        GenTerrain();
        planetData.humidityCount--;
    }

    private void GenerateTerrain()
    {
        planetData.SetTerrainManager(this, CalculateMinMaxdistance());
        GenTerrain();
    }

    void GenTerrain()
    {
        float time = Time.realtimeSinceStartup;
        DeleteAllChilds();
        planetData.InitializeValues();
        updateVisibilityNodes = new Dictionary<int4, Node>();
        detailLimitNode = new Dictionary<int4, Node>();
        biggestDetailList = new Dictionary<int4, Node>();
        int heightChunks = planetData.GetChunkHeight();
        faces = new Face[6];
        for (int i = 0; i < 6; i++)
        {
            faces[i] = new Face(i, planetData, gameObject.transform);
            faces[i].GenerateChunks(defaultMaterial, heightChunks);
        }
        GenerateTreeHoldersParent();
        for (int i = 0; i < 6; i++)
            faces[i].GenerateMesh(ref updateVisibilityNodes, ref detailLimitNode, ref biggestDetailList);
        treeDistance = planetData.GetLoDDistance(planetData.levelsOfDetail - 2) / 4;
        treeDistance *= treeDistance;
        UpdateBiggestDetail();
        //Debug.Log(Time.realtimeSinceStartup - time);
    }
    
    float CalculateMinMaxdistance()
    {
        float res = planetData.planetRadius / planetData.minChunkPerFace;
        Vector3 center = ((Vector3.forward * planetData.planetRadius) + (Vector3.right * res) + (Vector3.up * res));
        float3 triAngles = float3.zero;
        triAngles.x = (Vector3.Angle(center, Vector3.forward)) * (Mathf.PI / 180);
        triAngles.y = Mathf.Abs(Mathf.Asin(((planetData.planetRadius + res) * Mathf.Sin(triAngles.x)) / planetData.planetRadius));
        triAngles.z = Mathf.PI - triAngles.x - triAngles.y;
        float d = Mathf.Sqrt((2 * ((planetData.planetRadius * planetData.planetRadius) + (planetData.planetRadius * res)) * (1 - Mathf.Cos(triAngles.z))) + (res * res));
        return d - 1;
    }

    public float CalculateMaxDistance()
    {
        float res = planetData.planetRadius / planetData.minChunkPerFace;
        return (2 * planetData.planetRadius) + res;
    }

    void GenerateTreeHoldersParent()
    {
        if (treeHoldersParent != null)
            Destroy(treeHoldersParent);
        treeHoldersParent = new GameObject("TreeHoldersParent").transform;
        treeHoldersParent.parent = transform;
        treeHoldersParent.localRotation = Quaternion.identity;
        treeHoldersParent.localPosition = Vector3.zero;
    }

    void DeleteAllChilds()
    {
        GameObject[] tempArray = new GameObject[transform.childCount];

        for (int i = 0; i < tempArray.Length; i++)
        {
            tempArray[i] = transform.GetChild(i).gameObject;
        }

        foreach (var child in tempArray)
        {
            DestroyImmediate(child);
        }
    }

    public Node GetNode(int faceID, int myLevel, int3 myPos, int3 wantedPos)
    {
        int3 temp = TerrainManagerData.RotatePoint(faceID, wantedPos.x, wantedPos.y, planetData.maxChunkPerFace);
        return faces[temp.x].GetNode(myLevel, myPos, new int3(temp.y, temp.z, wantedPos.z));
    }
}

public static class TerrainManagerData
{
    // c0 = axisA = Right
    // c1 = axisB = Front
    // c2 = Up    = Up
    public static readonly float3x3[] dir = {
        new float3x3(new float3(0, 0, 1), new float3(0, 1, 0), new float3(1, 0, 0)),
        new float3x3(new float3(-1, 0, 0), new float3(0, 1, 0), new float3(0, 0, 1)),
        new float3x3(new float3(0, 0, -1), new float3(0, 1, 0), new float3(-1, 0, 0)),
        new float3x3(new float3(1, 0, 0), new float3(0, 1, 0), new float3(0, 0, -1)),
        new float3x3(new float3(1, 0, 0), new float3(0, 0, 1), new float3(0, 1, 0)),
        new float3x3(new float3(-1, 0, 0), new float3(0, 0, 1), new float3(0, -1, 0))
    };

    public static readonly int3[] axisIndex =
    {
        new int3(2, 1, 0),
        new int3(0, 1, 2),
        new int3(2, 1, 0),
        new int3(0, 1, 2),
        new int3(0, 2, 1),
        new int3(0, 2, 1)
    };

    public static readonly int3[] dirMult =
    {
        new int3(1, 1, 1),
        new int3(-1, 1, 1),
        new int3(-1, 1, -1),
        new int3(1, 1, -1),
        new int3(1, 1, 1),
        new int3(-1, 1, -1)
    };

    public static readonly int3[] neigborCells =
    {
        new int3(1, 0, 0),      // 0    Right
        new int3(0, 1, 0),      // 1    Front
        new int3(0, 0, 1),      // 2    Up
        new int3(-1, 0, 0),     // 3    Left
        new int3(0, -1, 0),     // 4    Back   
        new int3(0, 0, -1),     // 5    Down

        new int3(1, 1, 0),      // 6    Right   Front
        new int3(-1, 1, 0),     // 7    Left    Front
        new int3(1, -1, 0),     // 8    Right   Back
        new int3(-1, -1, 0),    // 9    Left    Back

        new int3(1, 0, 1),      // 10   Right   Up
        new int3(-1, 0, 1),     // 11   Left    Up
        new int3(1, 0, -1),     // 12   Right   Down
        new int3(-1, 0, -1),    // 13   Left    Down

        new int3(0, 1, 1),      // 14   Front   Up
        new int3(0, -1, 1),     // 15   Back    Up
        new int3(0, 1, -1),     // 16   Front   Down
        new int3(0, -1, -1)     // 17   Back    Down
    };

    // Right
    // Left
    // Up
    // Down
    public static readonly int4[] neighborFace =
    {
        new int4(1, 3, 4, 5),
        new int4(2, 0, 4, 5),
        new int4(3, 1, 4, 5),
        new int4(0, 2, 4, 5),
        new int4(0, 2, 1, 3),
        new int4(2, 0, 1, 3)
    };

    public static int3 RotatePointHumidity(int face, int x, int y, int maxQuantity)
    {
        int checkFaceID;
        if (x < 0)
        {
            x += maxQuantity - 1;
            checkFaceID = 1;
        }
        else if (x >= maxQuantity)
        {
            x++;
            x -= maxQuantity;
            checkFaceID = 0;
        }
        else if (y < 0)
        {
            y += maxQuantity - 1;
            checkFaceID = 3;
        }
        else if (y >= maxQuantity)
        {
            y++;
            y -= maxQuantity;
            checkFaceID = 2;
        }
        else
            return new int3(face, x, y);

        int2 temp = Rotate(CheckRotation(face, neighborFace[face][checkFaceID]), x, y, maxQuantity);

        return new int3(neighborFace[face][checkFaceID], temp.x, temp.y);
    }

    public static int3 RotatePoint(int face, int x, int y, int maxQuantity)
    {
        int checkFaceID;

        if (x < 0)
        {
            x += maxQuantity;
            checkFaceID = 1;
        }
        else if (x >= maxQuantity)
        {
            x -= maxQuantity;
            checkFaceID = 0;
        }
        else if (y < 0)
        {
            y += maxQuantity;
            checkFaceID = 3;
        }
        else if (y >= maxQuantity)
        {
            y -= maxQuantity;
            checkFaceID = 2;
        }
        else
            return new int3(face, x, y);

        int2 temp = Rotate(CheckRotation(face, neighborFace[face][checkFaceID]), x, y, maxQuantity);

        return new int3(neighborFace[face][checkFaceID], temp.x, temp.y);
    }

    public static int2 RotateSimple(int face1, int face2, int x, int y, int maxQuantity)
    {
        int rotation = CheckRotation(face1, face2);
        int2 r = Rotate(rotation, x, y, maxQuantity);
        return r;
    }

    static int CheckRotation(int originFace, int nextFace)
    {
        if (originFace == 0)
        {
            if (nextFace == 5 || nextFace == 4)
                return 3;
        }
        else if (originFace == 1)
        {
            if (nextFace == 4)
                return 2;
        }
        else if (originFace == 2)
        {
            if (nextFace == 5 || nextFace == 4)
                return 1;
        }
        else if (originFace == 3)
        {
            if (nextFace == 5)
                return 2;
        }
        else if (originFace == 4)
        {
            return nextFace + 1;
        }
        else if (originFace == 5)
        {
            if (nextFace == 3)
                return 2;
            if (nextFace == 2)
                return 3;
            if (nextFace == 0)
                return 1;
        }
        return 0;
    }

    static int2 Rotate(int rotation, int x, int y, int maxQuantity)
    {
        int2 temp = new int2(x, y);
        if (rotation == 1)
        {
            temp.x = y;
            temp.y = maxQuantity - 1 - x;
        }
        else if (rotation == 2)
        {
            temp.x = maxQuantity - 1 - x;
            temp.y = maxQuantity - 1 - y;
        }
        else if (rotation == 3)
        {
            temp.x = maxQuantity - 1 - y;
            temp.y = x;
        }
        return temp;
    }

    public static Dictionary<int, Dictionary<int, Area>> UVPositions = new Dictionary<int, Dictionary<int, Area>>
    {
        {1,     new Dictionary<int, Area>(){ {0, new Area(.7625f, .61f, .9875f, .79f)} } },
        {2,     new Dictionary<int, Area>(){ {1, new Area(.7625f, .41f, .9875f, .59f)} } },
        {4,     new Dictionary<int, Area>(){ {2, new Area(.7625f, .21f, .9875f, .39f)} } },
        {8,     new Dictionary<int, Area>(){ {3, new Area(.5125f, .61f, .7375f, .79f)} } },
        {16,    new Dictionary<int, Area>(){ {4, new Area(.5125f, .41f, .7375f, .59f)} } },
        {32,    new Dictionary<int, Area>(){ {5, new Area(.5125f, .21f, .7375f, .39f)} } },
        {64,    new Dictionary<int, Area>(){ {6, new Area(.2625f, .61f, .4875f, .79f)} } },
        {128,   new Dictionary<int, Area>(){ {7, new Area(.0125f, .61f, .2375f, .79f)} } },
        {256,   new Dictionary<int, Area>(){ {8, new Area(.7625f, .01f, .9875f, .19f)} } },
        {512,   new Dictionary<int, Area>(){ {9, new Area(.7625f, .91f, .9875f, .99f)} } },
    };

}

public class Area
{
    // 0
    // 1
    // 2
    // 3
    // 4
    public float2 limit1 { get; private set; }
    public float2 limit2 { get; private set; }

    public Area(float l1x, float l1y, float l2x, float l2y)
    {
        limit1 = new float2(l1x, l1y);
        limit2 = new float2(l2x, l2y);
    }

}

