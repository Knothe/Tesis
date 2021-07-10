using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class CreateSettingsWrapper
{
    [MenuItem("Assets/Create/ScriptableObjects/GeneratorSettings")]
    public static void CreateSettingsObject()
    {
        var asset = ScriptableObject.CreateInstance<GeneratorSettingsWrapper>();
        string p = AssetDatabase.GetAssetPath(Selection.activeObject);
        if (p == "")
            p = "Assets";
        else if (Path.GetExtension(p) != "")
            p = p.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
        AssetDatabase.CreateAsset(asset, AssetDatabase.GenerateUniqueAssetPath((p + "/NewGeneratorSettings.asset")));
        AssetDatabase.SaveAssets();
    }
}

public class GeneratorSettingsWrapper : ScriptableObject
{
    [SerializeField]
    public PlanetGeneratorSettings settings;
}
