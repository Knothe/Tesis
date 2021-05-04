using System.Collections.Generic;
using UnityEngine;
using System;


[Serializable]
public class BiomeTree
{
    public MenuTree[] trees;
    public int typesOfTrees;

    public void Initialize()
    {
        int j = 0;
        for (int i = typesOfTrees; i < trees.Length; i++)
        {
            trees[i] = new MenuTree(trees[j].treePrefabs, trees[j].radius);
            j++;
            if (j >= typesOfTrees)
                j = 0;
        }
    }

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

    public MenuTree(List<GameObject> treePrefab, float r)
    {
        treePrefabs = new List<GameObject>();
        foreach (GameObject g in treePrefab)
            treePrefabs.Add(g);
        radius = r;
    }

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
