using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/Biome Trees", order = 2)]
public class BiomeTrees : ScriptableObject
{
    // Max Size of 4
    [SerializeField] MenuTree tree1;
    [SerializeField] MenuTree tree2;
    [SerializeField] MenuTree tree3;
    [SerializeField] MenuTree tree4;

    public MenuTree[] trees { get; private set; }

    private void OnValidate()
    {
        if (trees == null)
            trees = new MenuTree[4];
        trees[0] = tree1;
        trees[1] = tree2;
        trees[2] = tree3;
        trees[3] = tree4;
    }
}

[Serializable]
public class MenuTree
{
    public List<GameObject> treePrefabs;
    public float radius;

    public GameObject GetPrefab()
    {
        if(treePrefabs.Count > 1)
        {
            int r = UnityEngine.Random.Range(0, treePrefabs.Count);
            return treePrefabs[r];
        }
        if (treePrefabs.Count == 1)
            return treePrefabs[0];
        return null;
    }
}
