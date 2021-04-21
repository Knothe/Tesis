using System.Collections.Generic;
using UnityEngine;
using System;


[Serializable]
public class BiomeTree
{
    public MenuTree[] trees;

    public BiomeTree()
    {
        trees = new MenuTree[4];
    }
}

/// <summary>
/// Values a tree needs to be generated
/// </summary>
[Serializable]
public class MenuTree
{
    /// <summary>
    /// Possible models for a tree
    /// </summary>
    public List<GameObject> treePrefabs;
    /// <summary>
    /// Radius of avoidance, another tree can't be generated in a distance less than this value
    /// </summary>
    public float radius;

    /// <returns>Random prefab from the prefab list</returns>
    public GameObject GetPrefab()
    {
        if (treePrefabs.Count > 1)
        {
            int r = UnityEngine.Random.Range(0, treePrefabs.Count);
            return treePrefabs[r];
        }
        if (treePrefabs.Count == 1)
            return treePrefabs[0];
        return null;
    }
}
