using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class CreateSettingsWrapper
{
    [MenuItem("Assets/Create/ScriptableObjects/GeneratorSettings")]
    public static void CreateSettingsObject()
    {
        var asset = ScriptableObject.CreateInstance<GeneratorSettingsWrapper>();
        AssetDatabase.CreateAsset(asset, "Assets/WorldMaker/ScriptableObjects/GeneratorSettings.asset");
        AssetDatabase.SaveAssets();
    }
}

public class GeneratorSettingsWrapper : ScriptableObject
{
    [SerializeField]
    public PlanetGeneratorSettings settings;
}
