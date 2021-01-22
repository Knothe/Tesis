using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateFlora : MonoBehaviour
{
    public float squareSize;

    public BiomeTrees biomeTrees;

    List<Tree> treeList;

    void Start()
    {
        Debug.Log(biomeTrees.scale1);
        Debug.Log(biomeTrees.scale2);

        float time = Time.time;
        treeList = new List<Tree>();
        int treeCount = 0;
        int notCreatedCount = 0;
        int maxTrees = (int)((squareSize * squareSize) / biomeTrees.radius[0]);
        Vector2 position = Vector2.zero;
        Tree t;
        float newSize = squareSize / 2;
        while(notCreatedCount < 5 && treeCount < maxTrees)
        {
            treeCount++;
            notCreatedCount++;
            position.x = UnityEngine.Random.Range(-newSize, newSize);
            position.y = UnityEngine.Random.Range(-newSize, newSize);
            t = GenerateTree(position);
            if (isCreatable(t))
            {
                treeList.Add(t);
                notCreatedCount = 0;
            }
        }
        Debug.Log(notCreatedCount);
        Debug.Log(treeCount);

        GameObject g;
        foreach(Tree tree in treeList)
        {
            g = Instantiate(biomeTrees.trees[tree.id]);
            g.transform.position = new Vector3(tree.position.x, 0, tree.position.y);
        }
        Debug.Log(Time.time - time);
    }

    Tree GenerateTree(Vector2 pos)
    {
        Tree t = new Tree();
        t.position = pos;
        t.biome = 0;
        t.id = 0;
        if (CalculateColor(pos.x, pos.y, biomeTrees.scale1, biomeTrees.offset1))
            t.id += 1;
        if (CalculateColor(pos.x, pos.y, biomeTrees.scale2, biomeTrees.offset2))
            t.id += 2;
        t.radius = biomeTrees.radius[t.id];
        return t;
    }

    bool CalculateColor(float x, float y, float scale, Vector2 offset)
    {
        x += offset.x;
        y += offset.y;
        x *= scale;
        y *= scale;
        float sample = Mathf.PerlinNoise(x, y);
        return sample <.5f;
    }

    bool isCreatable(Tree newTree)
    {
        float dif;
        foreach(Tree t in treeList)
        {
            dif = (newTree.position - t.position).magnitude;
            if (dif <= t.radius || dif <= newTree.radius)
                return false;
        }
        return true;
    }
}


public struct Tree
{
    public int biome;
    public int id;
    public Vector2 position;
    public float radius;
}