using UnityEngine;
using UnityEditor;

/// <summary>
/// Offers the posibility for the BiomeTreewrapper ScriptableObject to instantiate
/// </summary>
public class CreateBiomeTreesWrapper
{
    [MenuItem("Assets/Create/ScriptableObjects/BiomeTree")]
    public static void CreateTreeWrapperObject()
    {
        var asset = ScriptableObject.CreateInstance<BiomeTreesWrapper>();
        AssetDatabase.CreateAsset(asset, "Assets/WorldMaker/ScriptableObjects/BiomeTree.asset");
        AssetDatabase.SaveAssets();
    }
}


/// <summary>
/// Wrapps the MenuTree List for drawer functionality
/// </summary>
public class BiomeTreesWrapper : ScriptableObject
{
    [SerializeField]
    public BiomeTree biomeTree;

    public GameObject GetPrefab(int id)
    {
        return biomeTree.trees[id].GetPrefab();
    }

    public float GetRadius(int id)
    {
        return biomeTree.trees[id].radius;
    }
}
