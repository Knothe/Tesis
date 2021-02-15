using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TreeManager
{
    List<TreeHolder> treeHolderList;
    List<GameObject> treePrefab;

    [SerializeField]
    List<TreeSets> treeList;

    public TreeManager(ref List<GameObject> prefabs)
    {
        for(int i = 0; i < 8; i++)
            treeHolderList.Add(new TreeHolder());

        treePrefab = prefabs;
    }

    public GameObject GetTree(int biome, int type)
    {
        return treeHolderList[biome].GetTree(type);
    }
}

// Per biome
public class TreeHolder
{
    public TreeHolder()
    {

    }

    public GameObject GetTree(int type)
    {
        return null;
    }




}
