using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(fileName = "GeneratorSettings", menuName = "ScriptableObjects/Planet Generator Settings", order = 1)]
public class PlanetGeneratorSettings : ScriptableObject
{
    /*
General
	Radius                      -
	Height                      -
	Mesh Algorithm              -
	Player (Planets Manager)
	Chunk
		Chunk per face          -
		Chunk Detail            -
Noise
	Offset (Planets Manager)
	Settings                    -
Climate
	Definition                  -
	Humidity move               -     
	Biome Texture
	Use Only Colors             -
	Biome Colors
	Number of biomes
	Instantiate Trees (Planets Manager)
	Tree Set
Debug
	Shape (Planets Manager)
	Planet visible (Planets Manager)
	Show Biome (Planets Manager)
Planet Manager -
Ground Layer (Planets Manager)
     */

    // Planet Radius
    public float minPlanetRadius;                           // -
    public float maxPlanetRadius;                           // -    

    public int minChunkPerFace;                             // -
    public int maxChunkPerFace;                             // -
    public int chunkDetail;                                 // -

    // Max Height
    public int minMaxHeight;                                // -
    public int maxMaxHeight;                                // -

    public bool isMarchingCube;                             // -

    // Noise Settings
    public int settingsLength;                              // -
    public List<NoiseGeneratorSettings> minSettings;        // -
    public List<NoiseGeneratorSettings> maxSettings;        // -

    public int humidityCount;                               // -

    // Humidity Move
    public float minHumidityMove;                           // -
    public float maxHumidityMove;                           // -

    public Texture2D biomeTexture;  // Probably let planet manager decide
    public bool useColors;                                  // -

    // Planetary Body data

    private void OnValidate()
    {
        if(settingsLength <= 0)
        {
            minSettings.Clear();
            maxSettings.Clear();
        }
        else
        {
            if (settingsLength < 3)
                settingsLength = 3;
            CheckList(ref minSettings);
            CheckList(ref maxSettings);
        }
        
    }

    void CheckList(ref List<NoiseGeneratorSettings> checkList)
    {
        if (checkList.Count > settingsLength)
        {
            while (checkList.Count != settingsLength)
                checkList.RemoveAt(minSettings.Count - 1);
        }
        else if (checkList.Count < settingsLength)
        {
            while (checkList.Count != settingsLength)
                checkList.Add(new NoiseGeneratorSettings());
        }
    }
    
}

[Serializable]
public class NoiseGeneratorSettings
{
    public float strength = 1;
    public float scale = 1;
}
