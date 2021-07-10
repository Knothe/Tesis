using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class CreateBiomeTreeCollection
{
    [MenuItem("Assets/Create/ScriptableObjects/BiomeTreeCollection")]
    public static void CreateTreeWrapperObject()
    {
        var asset = ScriptableObject.CreateInstance<BiomeTreeCollection>();
        string p = AssetDatabase.GetAssetPath(Selection.activeObject);
        if (p == "")
            p = "Assets";
        else if (Path.GetExtension(p) != "")
            p = p.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
        AssetDatabase.CreateAsset(asset, AssetDatabase.GenerateUniqueAssetPath((p + "/NewBiomeTreeCollection.asset")));
        AssetDatabase.SaveAssets();
    }
}

public class BiomeTreeCollection : ScriptableObject
{
    [SerializeField]
    public TreeCollection trees;

    public float missedTreesMax { get { return trees.collection.missedTreesMax; } }
    public float maxTrees { get { return trees.collection.maxTrees; } }
    public float scale1 { get { return trees.collection.scale1; } }
    public float scale2 { get { return trees.collection.scale2; } }

    public GameObject GetPrefab(int biome, int id)
    {
        return trees.GetPrefab(biome, id);
    }

    public float GetRadius(int biome, int id)
    {
        return trees.GetRadius(biome, id);
    }

    public void Initialize()
    {
        trees.Initialize();
    }

}

[Serializable]
public class TreeCollection
{
    [SerializeField]
    public TreeSets collection;
    [SerializeField]
    public BiomeTree[] biomeTree = new BiomeTree[9];
    public bool useScriptableObjects;

    public void Initialize()
    {
        if (useScriptableObjects)
            foreach (BiomeTreesWrapper t in collection.biomeTrees)
                t.biomeTree.Initialize();
        else
            foreach (BiomeTree b in biomeTree)
                b.Initialize();
    }

    public GameObject GetPrefab(int biome, int id)
    {
        if (useScriptableObjects)
            return collection.biomeTrees[biome].GetPrefab(id);
        return biomeTree[biome].trees[id].GetPrefab();
    }

    public float GetRadius(int biome, int id)
    {
        if (useScriptableObjects)
            return collection.biomeTrees[biome].GetRadius(id);
        return biomeTree[biome].trees[id].radius;
    }

}