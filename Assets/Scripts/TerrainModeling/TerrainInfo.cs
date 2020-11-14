using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using System;

[Serializable]
public class TerrainInfo
{
    public float planetRadius;
    public int minChunkPerFace;
    public int maxChunkPerFace;
    public int chunkDetail;
    public int maxHeight;
    public float3 noiseOffset;
    public float noiseYScale;
    public bool isMarchingCube;
    public List<NoiseSettings> settings;
    public Transform player;
    public bool drawAsSphere;
    public bool removeLevelChange;

    public int levelsOfDetail { get; private set; }
    public List<int> reescaleValues { get; private set; }
    public float3x3[] resolutionVectors { get; private set; }
    public float maxResolution { get; private set; }
    public float3[] faceStart { get; private set; }
    float lodChange;
    public Vector3 playerRelativePosition { get; private set; }

    float[] lodDistances;

    Noise noise = new Noise(0);
    TerrainManager terrainManager;
    Dictionary<double, int> decimalNoise;

    public TerrainInfo(float planetR, int chunkPface, int chunkD, int maxH, float3 noiseOff, float yScale, bool isMC)
    {
        planetRadius = planetR;
        chunkDetail = chunkD;
        maxHeight = maxH;
        noiseOffset = noiseOff;
        noiseYScale = yScale;
        isMarchingCube = isMC;
    }

    public void InstantiateNoise()
    {
        noise = new Noise(0);
        levelsOfDetail = GetDetailCount();
        if (levelsOfDetail == -1)
            Debug.LogError("Chunks don't coincide");
        reescaleValues = new List<int>();
        resolutionVectors = new float3x3[6];
        faceStart = new float3[6];
        maxResolution = (planetRadius * 2) / maxChunkPerFace;
        for(int i = 0; i < 6; i++)
        {
            resolutionVectors[i] = new float3x3(
                TerrainManagerData.dir[i].c0 * maxResolution,
                TerrainManagerData.dir[i].c1 * maxResolution,
                TerrainManagerData.dir[i].c2 * maxResolution);
            faceStart[i] = (TerrainManagerData.dir[i].c2 * planetRadius) -
                (TerrainManagerData.dir[i].c0 * planetRadius) -
                (TerrainManagerData.dir[i].c1 * planetRadius);
        }

        int rTemp = 1;
        lodDistances = new float[levelsOfDetail];
        for (int i = 0; i < levelsOfDetail; i++)
        {
            reescaleValues.Add(rTemp);
            rTemp *= 2;
            lodDistances[i] = planetRadius / (i + 1);
        }
        lodDistances[levelsOfDetail - 1] = 0;
        decimalNoise = new Dictionary<double, int>();
        lodChange = planetRadius / (levelsOfDetail - 1);
    }

    int GetDetailCount()
    {
        float maxCPF = maxChunkPerFace;
        float minCPF = minChunkPerFace;
        int count = 1;
        while (maxCPF > minCPF)
        {
            maxCPF /= 2;
            count++;
        }
        if (maxCPF == minCPF)
            return count;
        else
            return -1;
    }

    public float GetLoDDistance(int level)
    {
        if (level < 0)
            return float.MaxValue;
        return lodDistances[level];
    }

    public bool CheckChunks()
    {
        return GetDetailCount() >= 0;
    }

    public float GetNoiseValue(float3 pos, int levelOfDetail)
    {
        float v;
        v = noise.Evaluate((pos + settings[0].centre) * settings[0].scale);
        v += Math.Sign(v) * 
            Mathf.Abs(noise.Evaluate((pos + settings[1].centre) * settings[2].scale) *
            noise.Evaluate((pos + settings[2].centre) * settings[2].scale));
        if (levelOfDetail > 0 && settings.Count > 3)
            v += noise.Evaluate((pos + settings[3].centre) * settings[3].scale) * .2f;
        return v / 2;
    }

    public float GetNoiseValue(float x, float y, float z, int levelOfDetail)
    {
        float v = (float)noise.Evaluate(x, y, z);
        return v;
    }

    public Vector3 GetPlayerRelativePosition()
    {
        return playerRelativePosition;
    }

    public void SetTerrainManager(TerrainManager t)
    {
        terrainManager = t;
        playerRelativePosition = player.transform.position - terrainManager.transform.position;
    }

    public void Update()
    {
        playerRelativePosition = player.transform.position - terrainManager.transform.position;
    }

    public Node GetNode(int faceID, int myLevel, int3 myPos, int3 wantedPos)
    {
        return terrainManager.GetNode(faceID, myLevel, myPos, wantedPos);
    }

}

[Serializable]
public class NoiseSettings
{
    public bool activate;
    public float strength = 1;
    public float scale = 1;         // tal vez cambiar por frequency
    public float3 centre;
}
