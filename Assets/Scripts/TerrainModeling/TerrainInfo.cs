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
    public bool isMarchingCube;
    public List<NoiseSettings> settings;
    public Transform player;
    public bool drawAsSphere;
    public bool showAll;
    public int humidityCount;
    public bool showTemperature;
    public Gradient temperatureGradient;
    public Gradient humidityGradient;


    public int levelsOfDetail { get; private set; }
    public List<int> reescaleValues { get; private set; }
    public float3[] resolutionVectors { get; private set; }
    public float3[] humidityResVec { get; private set; }
    public float maxResolution { get; private set; }
    public float3[] faceStart { get; private set; }
    public Vector3 playerRelativePosition { get; private set; }
    public float lodChange { get; private set; }

    float[] lodDistances;

    Noise noise = new Noise(0);
    TerrainManager terrainManager;
    Dictionary<double, int> decimalNoise;
    float noiseMaxHeight { get; set; }
    public float maxValue { get; private set; }
    float humidityDistance { get; set; }

    float[,,] humidityValues;   // face, x, y

    public TerrainInfo()
    {

    }

    public void InstantiateNoise()
    {
        noise = new Noise(0);
        levelsOfDetail = GetDetailCount();
        if (levelsOfDetail == -1)
            Debug.LogError("Chunks don't coincide");
        reescaleValues = new List<int>();
        resolutionVectors = new float3[6];
        faceStart = new float3[6];
        maxResolution = (planetRadius * 2) / maxChunkPerFace;
        for(int i = 0; i < 6; i++)
        {
            resolutionVectors[i] = new float3(  
                TerrainManagerData.dirMult[i].x * maxResolution,
                TerrainManagerData.dirMult[i].y * maxResolution,
                TerrainManagerData.dirMult[i].z * maxResolution);
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
        noiseMaxHeight = 2;
        SetHumidityMap();
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
            Mathf.Abs(noise.Evaluate((pos + settings[1].centre) * settings[1].scale) *
            noise.Evaluate((pos + settings[2].centre) * settings[2].scale));

        v = (v / noiseMaxHeight) * maxHeight;

        //if (levelOfDetail > 0 && settings.Count > 3)
        //    v += noise.Evaluate((pos + settings[3].centre) * settings[3].scale) * .2f;
        return v / 2;
    }

    public int GetChunkHeight()
    {
        float maxChunkSize = maxResolution * reescaleValues[levelsOfDetail - 1];
        int temp = (int)Mathf.Ceil(maxHeight / maxChunkSize);
        return temp;
    }

    public float GetNoiseValue(float x, float y, float z, int levelOfDetail)
    {
        return GetNoiseValue(new float3(x, y, z), levelOfDetail);
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

    public Color GetTemperature(float height, float yPos)
    {
        yPos = (Mathf.Abs(yPos) * .8f / planetRadius);
        float v;
        height = height - planetRadius;
        if (height < 0)
            return Color.blue;
        v = (height * .3f / noiseMaxHeight);
        v = 1 - (v + yPos);
        return temperatureGradient.Evaluate(v); 
    }

    public Color GetHumidity(int f, Vector3 p)
    {
        p = p - (Vector3)faceStart[f];
        Vector2 relativePoint = new Vector2((p[TerrainManagerData.axisIndex[f].x]) / humidityResVec[f].x,
            (p[TerrainManagerData.axisIndex[f].y]) / humidityResVec[f].y);

        Vector2 ref1 = new Vector2(Mathf.Floor(relativePoint.x), Mathf.Floor(relativePoint.y));
        Vector2 ref2 = new Vector2(Mathf.Floor(relativePoint.x), Mathf.Ceil(relativePoint.y));
        Vector2 ref3 = new Vector2(Mathf.Ceil(relativePoint.x), Mathf.Floor(relativePoint.y));
        Vector2 ref4 = new Vector2(Mathf.Ceil(relativePoint.x), Mathf.Ceil(relativePoint.y));

        float dist1 = (ref1 - relativePoint).magnitude;
        float dist2 = (ref2 - relativePoint).magnitude;
        float dist3 = (ref3 - relativePoint).magnitude;
        float dist4 = (ref4 - relativePoint).magnitude;

        float maxDist = dist1 + dist2 + dist3 + dist4;
        float value = 0;

        if(maxDist == 0)
        {
            value = GetHumidityValue(f, ref1);
            return humidityGradient.Evaluate(value);
        }
        maxDist = 1 / maxDist;
        if(ref1 == new Vector2(80.0f, 80.0f))
            Debug.Log("Error1");
        if (ref2 == new Vector2(80.0f, 80.0f))
            Debug.Log("Error2");
        if (ref3 == new Vector2(80.0f, 80.0f))
            Debug.Log("Error3");
        if (ref4 == new Vector2(80.0f, 80.0f))
            Debug.Log("Error4");
        value += GetHumidityValue(f, ref1) * (1 - (dist1 * maxDist));
        value += GetHumidityValue(f, ref2) * (1 - (dist2 * maxDist));
        value += GetHumidityValue(f, ref3) * (1 - (dist3 * maxDist));
        value += GetHumidityValue(f, ref4) * (1 - (dist4 * maxDist));

        return humidityGradient.Evaluate(value);
    }

    float GetHumidityValue(int f, Vector2 p)
    {
        int3 temp = TerrainManagerData.RotatePoint(f, (int)p.x, (int)p.y, humidityCount);
        if (temp.y < 0 || temp.y >= humidityCount)
            return 0;
        if (temp.z < 0 || temp.z >= humidityCount)
            return 0;
        if (humidityValues[temp.x, temp.y, temp.z] < 0)
            return 0;
        return humidityValues[temp.x, temp.y, temp.z];
    }

    void SetHumidityMap()
    {
        float3 point;
        float value;
        humidityResVec = new float3[6];
        humidityDistance = (planetRadius * 2) / humidityCount;
        humidityValues = new float[6, humidityCount, humidityCount];
        List<List<int3>> waterBodies = new List<List<int3>>();

        for(int i = 0; i < 6; i++)
            humidityResVec[i] = new float3(TerrainManagerData.dirMult[i].x * humidityDistance, TerrainManagerData.dirMult[i].y * humidityDistance, 0);

        for (int face = 0; face < 6; face++)
        {
            for (int i = 0; i < humidityCount; i++)
            {
                for(int j = 0; j < humidityCount; j++)
                {
                    //point = faceStart[face] + new float3(i * humidityResVec[face].x, j * humidityResVec[face].y, 0);
                    point = PlainToWorld(face, i, j);
                    if(humidityValues[face, i, j] == 0)
                    {
                        value = noise.Evaluate((point + settings[0].centre) * settings[0].scale);
                        if (value >= 0)
                            humidityValues[face, i, j] = -1;
                        else
                        {
                            humidityValues[face, i, j] = 1;
                            waterBodies.Add(SetLakeLimit(new int3(face, i, j)));
                        }
                    }

                }
            }
        }
    }

    float3 PlainToWorld(int f, int x, int y)
    {
        float3 point = faceStart[f];
        point[TerrainManagerData.axisIndex[f].x] += x * humidityResVec[f].x;
        point[TerrainManagerData.axisIndex[f].y] += y * humidityResVec[f].y;
        return point;
    }

    List<int3> SetLakeLimit(int3 startPoint)
    {
        List<int3> limitValues = new List<int3>();
        Queue<int3> nextPoint = new Queue<int3>();
        int3 current;
        bool isLimit;
        nextPoint.Enqueue(startPoint);
        while(nextPoint.Count > 0)
        {
            current = nextPoint.Dequeue();
            isLimit = false;
            if (AddToQueue(ref nextPoint, current.x, current.y + 1, current.z))
                isLimit = true;
            if(AddToQueue(ref nextPoint, current.x, current.y, current.z + 1))
                isLimit = true;
            if(AddToQueue(ref nextPoint, current.x, current.y - 1, current.z))
                isLimit = true;
            if(AddToQueue(ref nextPoint, current.x, current.y, current.z - 1))
                isLimit = true;
            if (isLimit)
                limitValues.Add(current);
        }
        return limitValues;
    }

    // Return true if is value is not water
    bool AddToQueue(ref Queue<int3> queue, int f, int x, int y)
    {
        int3 temp = TerrainManagerData.RotatePoint(f, x, y, humidityCount);
        if(humidityValues[temp.x, temp.y, temp.z] == 0)
        {
            //float3 point = faceStart[temp.x] + new float3(temp.y * humidityResVec[temp.x].x, temp.z * humidityResVec[temp.x].y, 0);
            float3 point = PlainToWorld(temp.x, temp.y, temp.z);
            float value = noise.Evaluate((point + settings[0].centre) * settings[0].scale);
            if(value >= 0)
            {
                humidityValues[temp.x, temp.y, temp.z] = -1;
                return true;
            }
            else
            {
                humidityValues[temp.x, temp.y, temp.z] = 1;
                queue.Enqueue(temp);
                return false;
            }
        }

        if (humidityValues[temp.x, temp.y, temp.z] != 1)
            return true;
        return false;
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
