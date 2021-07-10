using System.Collections.Generic;
using UnityEngine;

public class TreePerChunk
{
    /// <summary>
    /// Stores all the data of the trees in one chunk
    /// </summary>
    public List<TreeData> treeDataList { get; set; }
    /// <summary>
    /// Indicates if the chunk has this tree data activd
    /// </summary>
    public bool isActive { get; set; }
    /// <summary>
    /// How many chunks have this chunk of trees activated
    /// </summary>
    public int activatedChunks { get; set; }
    /// <summary>
    /// In Game object that holds all trees generated for this chunk
    /// </summary>
    public Transform inGameHolder { get; set; }
    /// <summary>
    /// Positions for these trees have been recalcullated
    /// </summary>
    public bool positionSet { get; set; }

    public TreePerChunk()
    {
        isActive = false;
        activatedChunks = 0;
        treeDataList = new List<TreeData>();
        positionSet = false;
    }

    /// <summary>
    /// Adds a tree to the treeData list
    /// </summary>
    /// <param name="t">Tree to add</param>
    public void AddTree(TreeData t)
    {
        treeDataList.Add(t);
    }
}

/// <summary>
/// Stores tree important values
/// </summary>
public class TreeData
{
    public int biome;
    public int id;
    public Vector3 cubePos;
    public Vector3 spherePos;
    public float radius;
}