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
    public float humidityMove;
    public bool showTemperature;
    public bool showBiome;
    public Gradient temperatureGradient;
    public Gradient humidityGradient;
    public Texture2D biomeTexture;
    [Range(1, 9)]
    public int biomeQuantity;

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
    Color[] biomes { get; set; }

    public TerrainInfo(float r, int mH, bool algorithm, int minCPF, int maxCPF, int chunkD, List<NoiseSettings> s, float3 offset)
    {
        planetRadius = r;
        minChunkPerFace = minCPF;
        maxChunkPerFace = maxCPF;
        chunkDetail = chunkD;
        maxHeight = mH;
        isMarchingCube = algorithm;
        settings = s;
        noiseOffset = offset;
    }

    public void SetClimate(int hCount, float hMove, Gradient tGrad, Gradient hGradient, int bQuantity)
    {
        humidityCount = hCount;
        humidityMove = hMove;
        temperatureGradient = tGrad;
        humidityGradient = hGradient;
        biomeQuantity = bQuantity;
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
        SetBiomes();
    }

    void SetBiomes()
    {
        biomes = new Color[biomeQuantity];
        int temp, rand;
        for(int i = 0; i < biomeQuantity; i++)
        {
            temp = TerrainInfoData.randomBiome[biomeQuantity - 1][i].Length;
            if (temp > 0)
            {
                rand = UnityEngine.Random.Range(0, temp);
                biomes[i] = TerrainInfoData.biomeColor[TerrainInfoData.randomBiome[biomeQuantity - 1][i][rand]];
            }
            else
                biomes[i] = TerrainInfoData.biomeColor[TerrainInfoData.randomBiome[biomeQuantity - 1][i][0]];
        }
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
        playerRelativePosition = terrainManager.transform.InverseTransformDirection(playerRelativePosition);
    }

    public void Update()
    {
        playerRelativePosition = player.transform.position - terrainManager.transform.position;

        playerRelativePosition = terrainManager.transform.InverseTransformDirection(playerRelativePosition);
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

    float GetT(float h, float yPos)
    {
        yPos = (Mathf.Abs(yPos) * .8f / planetRadius);
        float v;
        h = h - planetRadius;
        if (h < 0)
            return -1;
        v = (h * .3f / noiseMaxHeight);
        v = 1 - (v + yPos);
        return v;
    }

    public Color GetHumidity(int f, Vector3 p)
    {
        Vector3 temp = p - (Vector3)faceStart[f];
        Vector2 relativePoint = new Vector2((temp[TerrainManagerData.axisIndex[f].x]) / humidityResVec[f].x,
            (temp[TerrainManagerData.axisIndex[f].y]) / humidityResVec[f].y);
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
        if (maxDist == 0)
        {
            value = GetHumidityValue(f, ref1);
            return humidityGradient.Evaluate(value);
        }
        maxDist = 1 / maxDist;
        value += GetHumidityValue(f, ref1) * ((dist1 * maxDist));
        value += GetHumidityValue(f, ref2) * ((dist2 * maxDist));
        value += GetHumidityValue(f, ref3) * ((dist3 * maxDist));
        value += GetHumidityValue(f, ref4) * ((dist4 * maxDist));

        return humidityGradient.Evaluate(value);
    }

    float GetH(int f, Vector3 p)
    {
        Vector3 temp = p - (Vector3)faceStart[f];
        Vector2 relativePoint = new Vector2((temp[TerrainManagerData.axisIndex[f].x]) / humidityResVec[f].x,
            (temp[TerrainManagerData.axisIndex[f].y]) / humidityResVec[f].y);
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
        if (maxDist == 0)
        {
            value = GetHumidityValue(f, ref1);
            return value;
        }
        maxDist = 1 / maxDist;
        value += GetHumidityValue(f, ref1) * ((dist1 * maxDist));
        value += GetHumidityValue(f, ref2) * ((dist2 * maxDist));
        value += GetHumidityValue(f, ref3) * ((dist3 * maxDist));
        value += GetHumidityValue(f, ref4) * ((dist4 * maxDist));

        return value;
    }

    public Color GetBiome(int f, float height, float yPos, Vector3 p)
    {
        float t = GetT(height, yPos);
        if (t == -1)
            return Color.blue;
        float h = GetH(f, p);
        Color c = biomeTexture.GetPixel((int)(biomeTexture.width * t), (int)(biomeTexture.height * h));
        string id = ColorUtility.ToHtmlStringRGB(c);
        if (!TerrainInfoData.colorIndexValules.ContainsKey(id))
        {
            //Debug.Log(id);
            return Color.black;
        }
        int index = TerrainInfoData.colorIndexValules[id];
        return c;
        //return biomes[TerrainInfoData.biomeIndex[biomeQuantity - 1][index]];
    }

    float GetHumidityValue(int f, Vector2 p)
    {
        int3 temp = TerrainManagerData.RotatePointHumidity(f, (int)p.x, (int)p.y, humidityCount);
        if (temp.y < 0 || temp.y >= humidityCount || temp.z < 0 || temp.z >= humidityCount)
            return 0;
        if (humidityValues[temp.x, temp.y, temp.z] < 0)
            return 0;
        return humidityValues[temp.x, temp.y, temp.z];
    }   

    void SetHumidityMap()
    {
        humidityResVec = new float3[6];
        humidityDistance = (planetRadius * 2) / humidityCount;
        humidityCount++;
        humidityValues = new float[6, humidityCount, humidityCount];
        List<HashSet<int3>> waterBodies = new List<HashSet<int3>>();
        List<HashSet<int3>>  otherWaterBodies = new List<HashSet<int3>>();

        for(int i = 0; i < 6; i++)
            humidityResVec[i] = new float3(TerrainManagerData.dirMult[i].x * humidityDistance, TerrainManagerData.dirMult[i].y * humidityDistance, 0);
        
        GenerateWaterBodies(ref waterBodies, ref otherWaterBodies);
        
        float humidityModifier = 1 / (planetRadius * humidityMove);  // Modificable para que el usuario ingrese su propia distancia
        float addHumidity = 1 - humidityModifier;
        bool changed = true;
        bool current = true;
        while (changed && addHumidity > 0)
        {
            if (current)
                changed = MoveHumidity(ref waterBodies, ref otherWaterBodies, addHumidity);
            else
                changed = MoveHumidity(ref otherWaterBodies, ref waterBodies, addHumidity);
            current = !current;
            addHumidity -= humidityModifier;
        }
    }

    #region GenerateWaterBodies
    void GenerateWaterBodies(ref List<HashSet<int3>> waterBodies, ref List<HashSet<int3>> otherWaterBodies)
    {
        float3 point;
        float value;
        for (int face = 0; face < 6; face++)
        {
            for (int i = 0; i < humidityCount; i++)
            {
                for (int j = 0; j < humidityCount; j++)
                {
                    if (humidityValues[face, i, j] == 0)
                    {
                        point = PlainToWorld(face, i, j);
                        value = noise.Evaluate((point + noiseOffset + settings[0].centre) * settings[0].scale);
                        if (value >= 0)
                            humidityValues[face, i, j] = .001f;
                        else
                        {
                            humidityValues[face, i, j] = 1;
                            waterBodies.Add(SetLakeLimit(new int3(face, i, j)));
                            otherWaterBodies.Add(new HashSet<int3>());
                        }
                    }

                }
            }
        }

    }

    HashSet<int3> SetLakeLimit(int3 startPoint)
    {
        Queue<int3> nextPoint = new Queue<int3>();
        HashSet<int3> limitValues = new HashSet<int3>();
        int3 current1;
        int3 current2 = int3.zero;
        int3 current3 = int3.zero;
        bool isLimit;
        int isEdge;
        nextPoint.Enqueue(startPoint);
        while (nextPoint.Count > 0)
        {
            current1 = nextPoint.Dequeue();
            isEdge = IsEdge(current1, ref current2, ref current3);
            isLimit = false;
            if (AddToQueue(ref nextPoint, current1.x, current1.y + 1, current1.z))
                isLimit = true;
            if (AddToQueue(ref nextPoint, current1.x, current1.y, current1.z + 1))
                isLimit = true;
            if (AddToQueue(ref nextPoint, current1.x, current1.y - 1, current1.z))
                isLimit = true;
            if (AddToQueue(ref nextPoint, current1.x, current1.y, current1.z - 1))
                isLimit = true;
            if (isLimit)
            {
                limitValues.Add(current1);
                if (isEdge >= 1)
                {
                    limitValues.Add(current2);
                    humidityValues[current2.x, current2.y, current2.z] = 1;
                }
                if (isEdge == 2)
                {
                    limitValues.Add(current3);
                    humidityValues[current3.x, current3.y, current3.z] = 1;
                }
            }
        }
        return limitValues;
    }

    int IsEdge(int3 c, ref int3 c2, ref int3 c3)
    {
        int t = 0;
        c2 = c;
        if(c.y == 0)
        {
            t++;
            c2 = TerrainManagerData.RotatePoint(c.x, c.y - 1, c.z, humidityCount);
        }
        else if(c.y == humidityCount - 1)
        {
            t++;
            c2 = TerrainManagerData.RotatePoint(c.x, c.y + 1, c.z, humidityCount);
        }

        if (c.z == 0)
        {
            if(t == 1)
                c3 = TerrainManagerData.RotatePoint(c.x, c.y, c.z - 1, humidityCount);
            else
                c2 = TerrainManagerData.RotatePoint(c.x, c.y, c.z - 1, humidityCount);
            t++;
        }
        else if (c.z == humidityCount - 1)
        {
            if(t == 1)
                c3 = TerrainManagerData.RotatePoint(c.x, c.y, c.z + 1, humidityCount);
            else
                c2 = TerrainManagerData.RotatePoint(c.x, c.y, c.z + 1, humidityCount);
            t++;
        }
        return t;
    }

    // Return true if is value is not water
    bool AddToQueue(ref Queue<int3> queue, int f, int x, int y)
    {
        int3 temp = TerrainManagerData.RotatePointHumidity(f, x, y, humidityCount);
        if (humidityValues[temp.x, temp.y, temp.z] == 0)
        {
            //float3 point = faceStart[temp.x] + new float3(temp.y * humidityResVec[temp.x].x, temp.z * humidityResVec[temp.x].y, 0);
            float3 point = PlainToWorld(temp.x, temp.y, temp.z);
            float value = noise.Evaluate((point + settings[0].centre) * settings[0].scale);
            if (value >= 0)
            {
                humidityValues[temp.x, temp.y, temp.z] = .001f;
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
    #endregion

    #region MoveHumidity
    bool MoveHumidity(ref List<HashSet<int3>> current, ref List<HashSet<int3>> previous, float addHumidity)
    {
        List<HashSet<int3>> next = new List<HashSet<int3>>();
        bool changed = false;
        for (int i = 0; i < current.Count; i++)
        {
            next.Add(new HashSet<int3>());

            foreach (int3 value in current[i])
            {
                AddToPoint(value.x, value.y + 1, value.z, ref current, ref previous, ref next, i, addHumidity);
                AddToPoint(value.x, value.y, value.z + 1, ref current, ref previous, ref next, i, addHumidity);
                AddToPoint(value.x, value.y - 1, value.z, ref current, ref previous, ref next, i, addHumidity);
                AddToPoint(value.x, value.y, value.z - 1, ref current, ref previous, ref next, i, addHumidity);
            }
            if (next[i].Count > 0)
                changed = true;
        }
        previous = next;
        return changed;
    }

    void AddToPoint(int x, int y, int z, ref List<HashSet<int3>> current, ref List<HashSet<int3>> previous, ref List<HashSet<int3>> next, int i, float addHumidity)
    {
        int3 temp1;
        int3 temp2 = int3.zero;
        int3 temp3 = int3.zero;
        temp1 = TerrainManagerData.RotatePointHumidity(x, y, z, humidityCount);
        int isEdge = IsEdge(temp1, ref temp2, ref temp3);
        if(isEdge == 0)
        {
            if (CheckValue(temp1, ref current, ref previous, ref next, i))
            {
                humidityValues[temp1.x, temp1.y, temp1.z] += addHumidity;
                next[i].Add(temp1);
            }
        }
        else if(isEdge == 1)
        {
            if (CheckValue(temp1, ref current, ref previous, ref next, i) && CheckValue(temp2, ref current, ref previous, ref next, i))
            {
                humidityValues[temp1.x, temp1.y, temp1.z] += addHumidity;
                humidityValues[temp2.x, temp2.y, temp2.z] += addHumidity;
                next[i].Add(temp1);
                next[i].Add(temp2);
            }
        }
        else if(isEdge == 2)
        {
            if (CheckValue(temp1, ref current, ref previous, ref next, i) && 
                CheckValue(temp2, ref current, ref previous, ref next, i) &&
                CheckValue(temp3, ref current, ref previous, ref next, i))
            {
                humidityValues[temp1.x, temp1.y, temp1.z] += addHumidity;
                humidityValues[temp2.x, temp2.y, temp2.z] += addHumidity;
                humidityValues[temp3.x, temp3.y, temp3.z] += addHumidity;
                next[i].Add(temp1);
                next[i].Add(temp2);
                next[i].Add(temp3);
            }

        }
    }

    bool CheckValue(int3 id, ref List<HashSet<int3>> current, ref List<HashSet<int3>> previous, ref List<HashSet<int3>> next, int i)
    {
        if (current[i].Contains(id) || previous[i].Contains(id) || next[i].Contains(id))
            return false;
        if (humidityValues[id.x, id.y, id.z] >= 1)
        {
            humidityValues[id.x, id.y, id.z] = 1;
            return false;
        }

        return true;
    }

    float3 PlainToWorld(int f, int x, int y)
    {
        float3 point = faceStart[f];
        point[TerrainManagerData.axisIndex[f].x] += x * humidityResVec[f].x;
        point[TerrainManagerData.axisIndex[f].y] += y * humidityResVec[f].y;
        return point;
    }
    #endregion
}

[Serializable]
public class NoiseSettings
{
    public bool activate;
    public float strength = 1;
    public float scale = 1;         // tal vez cambiar por frequency
    public float3 centre;
}

public static class TerrainInfoData
{
    public static Dictionary<string, int> colorIndexValules = new Dictionary<string, int>()
    {
        {"4682D1", 0},  // Tundra
        {"D9C01C", 1},  // Desierto
        {"46D164", 2},  // Taiga
        {"9A46D1", 3},  // Herbazal
        {"4D1799", 4},  // Bosque Templado
        {"17997B", 5},  // Selva Templada
        {"DF410D", 6},  // Bosque Tropical
        {"991717", 7},  // Selva Tropical
        {"6FDF0D", 8}   // Sabana
    };

    public static Color[] biomeColor = {
        new Color(.6f, .09196f, .09196f),
        new Color(.8745f, .2549f, .05098f),
        new Color(.43529f, 87451f, .05098f),
        new Color(.09196f, .6f, .48235f),
        new Color(.30196f, .09019f, .6f),
        new Color(.6039f, .274509f, .8196f),
        new Color(.2745f, .8196f, .392157f),
        new Color(.85098f, .075294f, .1098f),
        new Color(.2745f, .598f, .81961f),
    };

    public static int[][] biomeIndex = {
        new int[]{0, 0, 0, 0, 0, 0, 0, 0, 0}, // 1
        new int[]{0, 1, 1, 0, 1, 1, 0, 1, 0}, // 2
        new int[]{0, 1, 2, 0, 2, 2, 0, 1, 0}, // 3
        new int[]{0, 1, 2, 0, 2, 2, 3, 1, 3}, // 4
        new int[]{0, 1, 2, 0, 4, 2, 3, 1, 3}, // 5
        new int[]{0, 1, 2, 5, 4, 2, 3, 1, 3}, // 6
        new int[]{0, 1, 2, 5, 4, 2, 3, 1, 6}, // 7
        new int[]{0, 1, 2, 5, 4, 7, 3, 1, 6}, // 8
        new int[]{0, 1, 2, 3, 4, 5, 6, 7, 8}, // 9
    };

    public static int[][][] randomBiome =
    {
        new int[][]{
            new int[]{0, 1, 2, 3, 4, 5, 6, 7, 8}
        },
        new int[][]{
            new int[]{0, 3, 6, 8},
            new int[]{1, 2, 4, 5, 7}
        },
        new int[][]{
            new int[]{0, 3, 6, 8},
            new int[]{1, 7},
            new int[]{2, 4, 5}
        },
        new int[][]{
            new int[]{0, 3},
            new int[]{1, 7},
            new int[]{2, 4, 5},
            new int[]{6, 8},
        },
        new int[][]{
            new int[]{0, 3},
            new int[]{1, 7},
            new int[]{2, 5},
            new int[]{6, 8},
            new int[]{4},
        },
        new int[][]{
            new int[]{0},
            new int[]{1, 7},
            new int[]{2, 5},
            new int[]{6, 8},
            new int[]{4},
            new int[]{3},
        },
        new int[][]{
            new int[]{0},
            new int[]{1, 7},
            new int[]{2, 5},
            new int[]{6},
            new int[]{4},
            new int[]{3},
            new int[]{8},
        },
        new int[][]{
            new int[]{0},
            new int[]{1, 7},
            new int[]{2},
            new int[]{6},
            new int[]{4},
            new int[]{3},
            new int[]{8},
            new int[]{5},
        },
        new int[][]{
            new int[]{0},
            new int[]{1},
            new int[]{2},
            new int[]{3},
            new int[]{4},
            new int[]{5},
            new int[]{6},
            new int[]{7},
            new int[]{8}
        }
    };
}
