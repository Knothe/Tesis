using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

/// <summary> Manages a single planet </summary>
public class TerrainManager : MonoBehaviour
{
    
    /// <summary> Planet terrain material </summary>
    public Material defaultMaterial { get; set; }

    /// <summary> Planet generation data </summary>
    [SerializeField]
    public TerrainInfo planetData;

    public PlanetsManager planetManager;

    Face[] faces = new Face[6];

    /// <summary> Updatable visible nodes </summary>
    Dictionary<int4, Node> updateVisibilityNodes = new Dictionary<int4, Node>();
    /// <summary> Updatable limit nodes </summary>
    Dictionary<int4, Node> detailLimitNode = new Dictionary<int4, Node>();
    /// <summary> Nodes in the biggest detail </summary>
    Dictionary<int4, Node> biggestDetailList = new Dictionary<int4, Node>();
    float treeDistance;

    /// <summary> In game object that functions as a parent for all generated trees </summary>
    public Transform treeHoldersParent { get; private set; }

    public bool generatedByManager { get; set; }

    public TerrainManager()
    {
        generatedByManager = false;
    }

    public void SetValues(PlanetsManager p)
    {
        if (p == null)
        {
            Debug.LogError("Missing Planet Manager");
            return;
        }
        planetManager = p;
        planetData.player = planetManager.player;
        p.planets.Add(this);
    }

    private void Start()
    {
        planetData.SetTerrainManager(this);
        if (!generatedByManager)
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
        if(planetManager == null)
            planetManager = GameObject.FindObjectOfType<PlanetsManager>();

        if (planetData == null)
            planetData = new TerrainInfo();

        planetData.OnValidate();
    }

    /// <summary> Updates chunk of the biggest detail possible to activate or desactivate trees </summary>
    void UpdateBiggestDetail()
    {
        if (!planetData.instantiateTrees)
            return;
        float dif;
        foreach (KeyValuePair<int4, Node> key in biggestDetailList)
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

    /// <summary> Updates chunks in the detail limit to change their detail </summary>
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

    /// <summary>
    /// Generates an in game chunk
    /// </summary>
    /// <param name="f">Face where it will be located</param>
    /// <param name="n">Node to represent</param>
    /// <returns>True if it could be generated</returns>
    public bool GenerateChunk(Face f, Node n)
    {
        if (n.IsVisible())
        {
            if (n.GenerateVoxelData() && n.inGameChunk == null)
            {
                Chunk c = planetManager.GetChunk();
                c.gameObject.transform.parent = f.parent;
                c.Initialize(n, planetManager.planetMaterial);
                n.inGameChunk = c;
            }
        }
        else
            return false;
        n.isActive = true;
        return true;
    }

    /// <summary>
    /// Increases the detail of a node
    /// </summary>
    /// <param name="node">Node to increment</param>
    /// <param name="remove">List of nodes to remove</param>
    /// <param name="added">List of nodes to add</param>
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

    /// <summary>
    /// Reduces the detail of a node
    /// </summary>
    /// <param name="node">Node to reduce in detail</param>
    /// <param name="remove">List of nodes to remove</param>
    /// <returns>True if it could be reduced</returns>
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

    /// <summary>
    /// Desactivate a node
    /// </summary>
    /// <param name="node">Node to desactivate</param>
    void DesactivateNode(Node node)
    {
        node.isActive = false;
        if (node.inGameChunk == null)
            return;
        planetManager.DesactivateChunk(node);
    }

    /// <summary>
    /// Removes a node from the detailLimitList
    /// </summary>
    /// <param name="n">Node to remove</param>
    void RemoveFromDetailLimit(Node n)
    {
        int4 id = n.GetIDValue();
        if (detailLimitNode.ContainsKey(id))
            detailLimitNode.Remove(id);
        if (biggestDetailList.ContainsKey(id))
            biggestDetailList.Remove(id);
    }
    
    /// <summary>
    /// Adds a node to the biggestDetailList
    /// </summary>
    /// <param name="n">Node to add</param>
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

    /// <summary>
    /// Adds a node to the detailLimitList
    /// </summary>
    /// <param name="n">Node to add</param>
    void AddToDetailLimit(Node n)
    {
        int4 id = n.GetIDValue();
        if (!detailLimitNode.ContainsKey(id))
            detailLimitNode.Add(id, n);
    }

    /// <summary>
    /// Checks if the node needs to change and how
    /// </summary>
    /// <param name="n">Node to check</param>
    /// <returns>0 if unchanged, 1 increment its detail and 2 if detail decreases</returns>
    int CheckNodeAvailability(Node n)
    {
       float dist = (planetData.GetPlayerRelativePosition() - (Vector3)n.sphereCenter).magnitude;
        if (dist < planetData.GetLoDDistance(n.level))          // LoD increments
            return 1;
        if (dist >= planetData.GetLoDDistance(n.level - 1))     // LoD decreases
            return 2;
        return 0;
    }

    /// <summary>
    /// Updates visible and invisible nodes
    /// </summary>
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

    /// <summary>
    /// Checks node that chaned before being added to the list
    /// </summary>
    /// <param name="n">Node to check</param>
    /// <param name="key">Node key</param>
    /// <param name="changed">List of changed nodes</param>
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

    /// <summary>
    /// Checks the state of a modified node and adds or deletes node from the visibility list if needed
    /// </summary>
    /// <param name="node">Node to check</param>
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

    /// <summary>
    /// Removes node from updateVisibilityNodes lsit
    /// </summary>
    /// <param name="n">Node to remove</param>
    void RemoveFromUpdateVisibility(Node n)
    {
        int4 id = n.GetIDValue();
        if (updateVisibilityNodes.ContainsKey(id))
            updateVisibilityNodes.Remove(id);
    }

    /// <summary>
    /// Adds a node to the updateVisibilityNodes lsit
    /// </summary>
    /// <param name="n">Node to add</param>
    void AddToUpdateVisibility(Node n)
    {
        int4 id = n.GetIDValue();
        if (!updateVisibilityNodes.ContainsKey(id))
            updateVisibilityNodes.Add(id, n);
        if (n.level == 0)
            if (!detailLimitNode.ContainsKey(id))
                detailLimitNode.Add(id, n);
    }

    /// <summary>
    /// Generates a terrain from the editor
    /// </summary>
    public void GenerateOnEditor()
    {
        planetData.SetTerrainManager(this);
        planetData.SetBiomes();
        GenTerrain();
        planetData.humidityCount--;
    }

    /// <summary>
    /// Generates terrain on Start
    /// </summary>
    private void GenerateTerrain()
    {
        planetData.SetTerrainManager(this, CalculateMinMaxdistance());
        GenTerrain();
    }

    /// <summary>
    /// Clears everything and generates terrain from scratch
    /// </summary>
    void GenTerrain()
    {
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
            faces[i].GenerateChunks(heightChunks);
        }
        GenerateTreeHoldersParent();
        for (int i = 0; i < 6; i++)
            faces[i].GenerateMesh(ref updateVisibilityNodes, ref detailLimitNode, ref biggestDetailList);
        treeDistance = planetData.GetLoDDistance(planetData.levelsOfDetail - 2) / 4;
        treeDistance *= treeDistance;
        UpdateBiggestDetail();
    }

    /// <summary>
    /// Calculates max distance of player before all nodes are min level of detail
    /// </summary>
    /// <returns>Max distance</returns>
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
    /// <summary>
    /// Generates an object to hold all Tree Holders
    /// </summary>
    void GenerateTreeHoldersParent()
    {
        if (treeHoldersParent != null)
            Destroy(treeHoldersParent);
        treeHoldersParent = new GameObject("TreeHoldersParent").transform;
        treeHoldersParent.parent = transform;
        treeHoldersParent.localRotation = Quaternion.identity;
        treeHoldersParent.localPosition = Vector3.zero;
    }

    /// <summary>
    /// Deletes all childs of this GameObject
    /// </summary>
    void DeleteAllChilds()
    {
        GameObject[] tempArray = new GameObject[transform.childCount];

        for (int i = 0; i < tempArray.Length; i++)
        {
            tempArray[i] = transform.GetChild(i).gameObject;
        }

        foreach (var child in tempArray)
            DestroyImmediate(child);
    }

    /// <summary>
    /// Searches a requested node
    /// </summary>
    /// <param name="faceID">Original objective face</param>
    /// <param name="myLevel">Max level of detail for search</param>
    /// <param name="wantedPos">Wanted node relative to original face</param>
    /// <returns>Found node</returns>
    public Node GetNode(int faceID, int myLevel, int3 wantedPos)
    {
        int3 temp = TerrainManagerData.RotatePoint(faceID, wantedPos.x, wantedPos.y, planetData.maxChunkPerFace);
        return faces[temp.x].GetNode(myLevel, new int3(temp.y, temp.z, wantedPos.z));
    }
}

/// <summary>
/// Preset data for easier calculations
/// </summary>
public static class TerrainManagerData
{
    // c0 = axisA = Right
    // c1 = axisB = Front
    // c2 = Up    = Up
    /// <summary>
    /// Indicates axis for a face and its direction
    /// </summary>
    public static readonly float3x3[] dir = {
        new float3x3(new float3(0, 0, 1), new float3(0, 1, 0), new float3(1, 0, 0)),
        new float3x3(new float3(-1, 0, 0), new float3(0, 1, 0), new float3(0, 0, 1)),
        new float3x3(new float3(0, 0, -1), new float3(0, 1, 0), new float3(-1, 0, 0)),
        new float3x3(new float3(1, 0, 0), new float3(0, 1, 0), new float3(0, 0, -1)),
        new float3x3(new float3(1, 0, 0), new float3(0, 0, 1), new float3(0, 1, 0)),
        new float3x3(new float3(-1, 0, 0), new float3(0, 0, 1), new float3(0, -1, 0))
    };

    /// <summary>
    /// Axis for a face
    /// </summary>
    public static readonly int3[] axisIndex =
    {
        new int3(2, 1, 0),
        new int3(0, 1, 2),
        new int3(2, 1, 0),
        new int3(0, 1, 2),
        new int3(0, 2, 1),
        new int3(0, 2, 1)
    };

    /// <summary>
    /// Direction of each axis
    /// </summary>
    public static readonly int3[] dirMult =
    {
        new int3(1, 1, 1),
        new int3(-1, 1, 1),
        new int3(-1, 1, -1),
        new int3(1, 1, -1),
        new int3(1, 1, 1),
        new int3(-1, 1, -1)
    };

    /// <summary>
    /// All possible neighbors for a cell
    /// </summary>
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

    /// <summary> Indicates wich faces are located relative to that face </summary>
    public static readonly int4[] neighborFace =
    {
        // Right, left, up, down
        new int4(1, 3, 4, 5),
        new int4(2, 0, 4, 5),
        new int4(3, 1, 4, 5),
        new int4(0, 2, 4, 5),
        new int4(0, 2, 1, 3),
        new int4(2, 0, 1, 3)
    };

    /// <summary>
    /// Rotates a point without the objective face overlapping coordinates with other faces
    /// </summary>
    /// <param name="face">Origin face</param>
    /// <param name="x">X position of the point</param>
    /// <param name="y">Y position of the point</param>
    /// <param name="maxQuantity">Max value the point can have, min is (0, 0)</param>
    /// <returns>Rotated point</returns>
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

    /// <summary>
    /// Rotates a point without knowing the objective face
    /// </summary>
    /// <param name="face">Origin face</param>
    /// <param name="x">X position of the point</param>
    /// <param name="y">Y position of the point</param>
    /// <param name="maxQuantity">Max value the point can have, min is (0, 0)</param>
    /// <returns>Rotated point</returns>
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

    /// <summary>
    /// Does a simple rotation of a point between 2 known faces
    /// </summary>
    /// <param name="face1">Origin face</param>
    /// <param name="face2">Objective face</param>
    /// <param name="x">X position of the point</param>
    /// <param name="y">Y position of the point</param>
    /// <param name="maxQuantity">Max value the point can have, min is (0, 0)</param>
    /// <returns>Rotated point</returns>
    public static int2 RotateSimple(int face1, int face2, int x, int y, int maxQuantity)
    {
        int rotation = CheckRotation(face1, face2);
        int2 r = Rotate(rotation, x, y, maxQuantity);
        return r;
    }

    /// <summary>
    /// Checks the correct way to rotate a value
    /// </summary>
    /// <param name="originFace">Source face</param>
    /// <param name="nextFace">Objective face</param>
    /// <returns></returns>
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

    /// <summary>
    /// Rotates a point 
    /// </summary>
    /// <param name="rotation">Kind of rotation</param>
    /// <param name="x">X position of the point</param>
    /// <param name="y">Y position of the point</param>
    /// <param name="maxQuantity">Max value the point can have, min is (0, 0)</param>
    /// <returns>Rotated point</returns>
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
}
