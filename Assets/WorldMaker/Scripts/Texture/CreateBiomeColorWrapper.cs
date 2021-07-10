using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class CreateBiomeColorWrapper
{
    [MenuItem("Assets/Create/ScriptableObjects/BiomeColor")]
    public static void CreateColorWrapperObject()
    {
        var asset = ScriptableObject.CreateInstance<BiomeColorWrapper>();
        string path = AssetDatabase.GetAssetPath(Selection.activeObject);
        if (path == "")
            path = "Assets";
        else if (Path.GetExtension(path) != "")
            path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
        AssetDatabase.CreateAsset(asset, AssetDatabase.GenerateUniqueAssetPath((path + "/NewBiomeColor.asset")));
        AssetDatabase.SaveAssets();
    }
}
