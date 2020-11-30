using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class TerrainManager : MonoBehaviour
{
    [SerializeField]
    public TerrainInfo planetData;
    public Material defaultMaterial;
    Face[] faces = new Face[6];

    Dictionary<int4, Node> updateVisibilityNodes = new Dictionary<int4, Node>();
    Dictionary<int4, Node> detailLimitNode = new Dictionary<int4, Node>();

    private void Start()
    {
        planetData.SetTerrainManager(this);
        GenerateTerrain();
        planetData.humidityCount++;
    }

    private void Update()
    {
        planetData.Update();
        UpdateVisibleChunks();
        UpdateChunkDetail();
    }

    void UpdateChunkDetail()
    {
        List<Node> added = new List<Node>();
        List<Node> remove = new List<Node>();
        int i;
        Node node;
        foreach(KeyValuePair<int4, Node> key in detailLimitNode)
        {
            i = CheckNodeAvailability(key.Value);
            if (i == 1)
            {
                key.Value.GenerateChilds();

                faces[key.Key.w].DesactivateChunk(key.Value);
                remove.Add(key.Value);

                foreach (Node n in key.Value.childs)
                {
                    faces[key.Key.w].GenerateChunk(n);
                    added.Add(n);
                }
                
            }
            else if(i == 2)
            {
                node = key.Value.parentNode;
                if(CheckNodeAvailability(node) == 0)
                {
                    foreach (Node n in node.childs)
                    {
                        if (n == null)
                            continue;
                        faces[n.axisID].DesactivateChunk(n);
                        remove.Add(n);
                        n.isActive = false;
                    }
                    faces[node.axisID].GenerateChunk(node);
                    added.Add(node);
                }
            }
        }

        foreach (Node n in remove)
            RemoveFromDetailLimit(n);

        bool isLimit, isLimit2;
        foreach(Node n in added)
        {
            n.GenerateMesh2();
            isLimit = false;
            for(i = 0; i < 6; i++)
            {
                if (n.neighbors[i] == null)
                    continue;
                //isLimit = true;
                else if (n.level == n.neighbors[i].level)
                {
                    if (!n.neighbors[i].isActive)
                    {
                        isLimit = true;
                        for (int j = 0; j < 6; j++)
                        {
                            if (n.neighbors[i].childs[j] != null)
                                AddToDetailLimit(n.neighbors[i].childs[j]);
                        }
                    }
                    else
                    {
                        isLimit2 = false;
                        for (int j = 0; j < 6; j++)
                        {
                            if (n.neighbors[i].neighbors[j] == null)
                                continue;
                            if (n.isVisible != n.neighbors[i].neighbors[j].isVisible)
                            {
                                isLimit2 = true;
                                AddToDetailLimit(n.neighbors[i].neighbors[j]);
                            }
                            if (isLimit2) AddToDetailLimit(n.neighbors[i]);
                            else AddToDetailLimit(n.neighbors[i]);
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

    void RemoveFromDetailLimit(Node n)
    {
        int4 id = n.GetIDValue();
        if (detailLimitNode.ContainsKey(id))
            detailLimitNode.Remove(id);
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
        List<Node> changed = new List<Node>();
        foreach(KeyValuePair<int4, Node> key in updateVisibilityNodes)
        {
            if (key.Value.IsVisible())
            {
                if(key.Value.inGameChunk == null)
                {
                    changed.Add(key.Value);
                    faces[key.Key.w].GenerateChunk(key.Value);
                    key.Value.GenerateMesh2();
                }
            }
            else if(key.Value.inGameChunk != null)
            {
                changed.Add(key.Value);
                faces[key.Key.w].DesactivateChunk(key.Value);
            }
        }

        bool isLimit, isLimit2;
        foreach(Node node in changed)
        {
            isLimit = false;
            for(int i = 0; i < 6; i++)
            {
                if (node.neighbors[i] == null)
                    continue;
                if (node.isVisible == node.neighbors[i].isVisible)
                {
                    isLimit2 = false;
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
    }

    private void OnValidate()
    {
        if (planetData.settings.Count < 3)
        {
            Debug.Log("Minimum Size of 3");
            for (int i = planetData.settings.Count; i < 3; i++)
                planetData.settings.Add(new NoiseSettings());
        }

        if (!planetData.CheckChunks())
            Debug.LogError("Chunks (" + planetData.minChunkPerFace + ", " + planetData.maxChunkPerFace + ") don't coincide");

    }

    public void UpdateTerrain()
    {

    }

    public void GenerateTerrain()
    {
        float time = Time.realtimeSinceStartup;
        DeleteAllChilds();
        planetData.InstantiateNoise();
        planetData.SetTerrainManager(this);
        updateVisibilityNodes = new Dictionary<int4, Node>();
        detailLimitNode = new Dictionary<int4, Node>();
        int heightChunks = planetData.GetChunkHeight();
        for(int i = 0; i < 6; i++)
        {
            faces[i] = new Face(i, planetData, gameObject.transform);
            faces[i].GenerateChunks(defaultMaterial, heightChunks);
        }

        for (int i = 0; i < 6; i++)
            faces[i].GenerateMesh(ref updateVisibilityNodes, ref detailLimitNode);
        Debug.Log(Time.realtimeSinceStartup - time);
        planetData.humidityCount--;
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
    // c0 = axisA
    // c1 = axisB
    // c2 = Up
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
        else if(rotation == 3)
        {
            temp.x = maxQuantity - 1 - y;
            temp.y = x;
        }
        return temp;
    }

}
