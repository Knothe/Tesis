using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class TreePerChunk
{
    public List<TreeData> treeDataList { get; set; }
    public bool isActive { get; set; }
    public int activatedChunks { get; set; }
    public Vector3 center { get; private set; }
    public Transform inGameHolder { get; set; }

    public TreePerChunk()
    {
        isActive = false;
        activatedChunks = 0;
        treeDataList = new List<TreeData>();
        center = Vector3.zero;
    }

    public void AddTree(TreeData t)
    {
        treeDataList.Add(t);
        center += t.spherePos;
    }

    

    public void SetCenter()
    {
        return;
        Vector3 c = Vector3.zero;
        foreach (TreeData td in treeDataList)
            c += td.spherePos;
        center = c / treeDataList.Count;
        foreach (TreeData td in treeDataList)
            td.spherePos -= c;
    }
}

public class TreeData
{
    public int biome;
    public int id;
    public Vector3 cubePos;
    public Vector3 spherePos;
    public float radius;
}