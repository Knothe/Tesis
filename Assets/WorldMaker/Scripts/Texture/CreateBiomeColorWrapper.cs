using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class CreateBiomeColorWrapper
{
    [MenuItem("Assets/Create/ScriptableObjects/BiomeColor")]
    public static void CreateColorWrapperObject()
    {
        var asset = ScriptableObject.CreateInstance<BiomeColorWrapper>();
        AssetDatabase.CreateAsset(asset, "Assets/WorldMaker/ScriptableObjects/BiomeColor.asset");
        //string path = AssetDatabase.GetAssetPath(Selection.activeObject);
        //AssetDatabase.CreateAsset(asset, path);
        AssetDatabase.SaveAssets();
    }
}
