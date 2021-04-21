using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class PlanetGeneratorSettings
{
    // Planet Radius
    public float minPlanetRadius;                           
    public float maxPlanetRadius;                               

    public int minChunkPerFace;                             
    public int maxChunkPerFace;                             
    public int chunkDetail;                                 

    // Max Height
    public int minMaxHeight;                                
    public int maxMaxHeight;                               

    public bool isMarchingCube;                             

    // Noise Settings
    public int settingsLength;                              
    public List<NoiseGeneratorSettings> minSettings;        
    public List<NoiseGeneratorSettings> maxSettings;        


    // Humidity Move
    public int humidityCount;                               
    public float minHumidityMove;
    public float maxHumidityMove;                           
}

[Serializable]
public class NoiseGeneratorSettings
{
    public float strength = 1;
    public float scale = 1;
}
