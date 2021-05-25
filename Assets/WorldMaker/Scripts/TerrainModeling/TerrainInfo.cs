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

    [Range(1, 9)]
    public int biomeQuantity;
    public bool instantiateTrees;
    public bool chooseBiomes;
    public int[] menuBiomeNumber;

    public bool useOwnColors;
    public BiomeColorWrapper biomeColorWrapper;
    public bool useCurve;
    public AnimationCurve curve;

    BiomeColors biomeColors { get
        {
            if (!useOwnColors || biomeColorWrapper == null)
                return terrainManager.planetManager.biomeColors;
            return biomeColorWrapper.biomeColors;
        } }

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
    public TerrainManager terrainManager { get; private set; }
    float noiseMaxHeight { get; set; }
    public float maxValue { get; private set; }
    float humidityDistance { get; set; }

    float[,,] humidityValues;   // face, x, y
    int[] biomeNumber { get; set; }
    Dictionary<int3, TreePerChunk> treeCollection { get; set; }

    public TerrainInfo()
    {
        drawAsSphere = true;
        showBiome = true;
        settings = new List<NoiseSettings>();
        for(int i = 0; i < 3; i++)
            settings.Add(new NoiseSettings());
    }

    public TerrainInfo(bool algorithm, int minCPF, int maxCPF, int chunkD, int humidityDef, bool tree)
    {
        isMarchingCube = algorithm;
        minChunkPerFace = minCPF;
        maxChunkPerFace = maxCPF;
        chunkDetail = chunkD;
        humidityCount = humidityDef;
        showBiome = true;
        drawAsSphere = true;
        showAll = false;
        instantiateTrees = tree;
    }

    public TerrainInfo(float r, int minCPF, int maxCPF, int cD, int maxH, bool algorithm, List<NoiseSettings> s, 
        int hCount, float hMove, Gradient tG, Gradient hG, int bQ, bool iT, bool cB, int[] menuBN, bool uC, AnimationCurve c)
    {
        planetRadius = r;
        minChunkPerFace = minCPF;
        maxChunkPerFace = maxCPF;
        chunkDetail = cD;
        maxHeight = maxH;
        isMarchingCube = algorithm;
        settings = s;
        humidityCount = hCount;
        humidityMove = hMove;
        temperatureGradient = tG;
        humidityGradient = hG;
        biomeQuantity = bQ;
        instantiateTrees = iT;
        chooseBiomes = cB;
        menuBiomeNumber = menuBN;
        drawAsSphere = true;
        showAll = false;
        showBiome = true;
        useCurve = uC;
        curve = c;
    }

    public TerrainInfo(float r, bool algorithm, int minCPF, int maxCPF, int cD, float hMove, int bQ)
    {
        planetRadius = r;
        isMarchingCube = algorithm;
        minChunkPerFace = minCPF;
        maxChunkPerFace = maxCPF;
        chunkDetail = cD;
        humidityMove = hMove;
        biomeQuantity = bQ;

        chooseBiomes = false;
        instantiateTrees = false;
        useCurve = false;
        drawAsSphere = true;
        showAll = false;
        showBiome = true;

        humidityCount = 20;
        maxHeight = (int)Mathf.Ceil(r / 6);
        settings = new List<NoiseSettings>();
        // 1.28 is a value for a planet of radius 1
        float v = 1.28f / r;
        settings.Add(new NoiseSettings(1, v, true));
        settings.Add(new NoiseSettings(1, v * 1.5f, true));
        settings.Add(new NoiseSettings(1, v * 1.7f, true));

    }

    public void OnValidate()
    {
        if (!CheckChunks())
            Debug.LogError("Chunks (" + minChunkPerFace + ", " + maxChunkPerFace + ") don't coincide");

        if (player == null)
            player = GameObject.FindObjectOfType<PlayerManager>().transform;
    }

    public void SetClimate(int hCount, float hMove, Gradient tGrad, Gradient hGradient, int bQuantity)
    {
        humidityCount = hCount;
        humidityMove = hMove;
        temperatureGradient = tGrad;
        humidityGradient = hGradient;
        biomeQuantity = bQuantity;
    }

    #region InitializeTerrain

    public void InitializeValues()
    {
        noise = new Noise(0);
        levelsOfDetail = GetDetailCount();
        if (levelsOfDetail == -1)
            Debug.LogError("Chunks don't coincide");
        SetResolutionValues();
        SetLoDValues();
        SetNoiseMaxHeight();
        SetHumidityMap();
        treeCollection = new Dictionary<int3, TreePerChunk>();
    }

    void SetResolutionValues()
    {
        reescaleValues = new List<int>();
        resolutionVectors = new float3[6];
        faceStart = new float3[6];
        maxResolution = (planetRadius * 2) / maxChunkPerFace;
        for (int i = 0; i < 6; i++)
        {
            resolutionVectors[i] = new float3(
                TerrainManagerData.dirMult[i].x * maxResolution,
                TerrainManagerData.dirMult[i].y * maxResolution,
                TerrainManagerData.dirMult[i].z * maxResolution);
            faceStart[i] = (TerrainManagerData.dir[i].c2 * planetRadius) -
                (TerrainManagerData.dir[i].c0 * planetRadius) -
                (TerrainManagerData.dir[i].c1 * planetRadius);
        }
    }

    void SetLoDValues()
    {
        int rTemp = 1;
        lodDistances = new float[levelsOfDetail];
        for (int i = 0; i < levelsOfDetail; i++)
        {
            reescaleValues.Add(rTemp);
            rTemp *= 2;
            lodDistances[i] = planetRadius / (i + 1);
        }
        lodDistances[levelsOfDetail - 1] = 0;
        lodChange = planetRadius / (levelsOfDetail - 1);
    }

    void SetNoiseMaxHeight()
    {
        noiseMaxHeight = 0;
        for(int i = 0; i < settings.Count; i += 2)
            noiseMaxHeight += settings[i].strength;
    }

    public void SetBiomes(int[] biomes)
    {
        biomeNumber = biomes;
    }

    public void SetBiomes()
    {
        if(biomeQuantity == 9)
        {
            biomeNumber = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 };
            return;
        }
        else
            biomeNumber = new int[9];
        List<int> tempList = new List<int>(new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 });
        List<int> selectedBiomes;

        if (chooseBiomes && menuBiomeNumber.Length == biomeQuantity)
        {
            selectedBiomes = new List<int>(menuBiomeNumber);
            foreach(int n in selectedBiomes)
            {
                biomeNumber[n] = n;
                tempList.Remove(n);
            }
        }
        else
        {
            selectedBiomes = new List<int>();
            int rand;
            for (int i = 0; i < biomeQuantity; i++)
            {
                rand = UnityEngine.Random.Range(0, tempList.Count);
                biomeNumber[tempList[rand]] = tempList[rand];
                selectedBiomes.Add(tempList[rand]);
                tempList.RemoveAt(rand);
            }
        }

        foreach(int n in tempList)
            biomeNumber[n] = selectedBiomes[UnityEngine.Random.Range(0, selectedBiomes.Count)];
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

    void SetHumidityMap()
    {
        humidityResVec = new float3[6];
        humidityDistance = (planetRadius * 2) / humidityCount;
        humidityCount++;
        humidityValues = new float[6, humidityCount, humidityCount];
        List<HashSet<int3>> waterBodies = new List<HashSet<int3>>();
        List<HashSet<int3>> otherWaterBodies = new List<HashSet<int3>>();

        for (int i = 0; i < 6; i++)
            humidityResVec[i] = new float3(TerrainManagerData.dirMult[i].x * humidityDistance, TerrainManagerData.dirMult[i].y * humidityDistance, 0);

        GenerateWaterBodies(ref waterBodies, ref otherWaterBodies);

        float humidityModifier = 1 / ((humidityCount / 2) * humidityMove);  // Modificable para que el usuario ingrese su propia distancia
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

    #endregion

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
        v = noise.Evaluate((pos * settings[0].scale) + settings[0].centre) * settings[0].strength;
        v += (Math.Sign(v) * 
            Mathf.Abs(noise.Evaluate((pos * settings[1].scale) + settings[1].centre) *
            noise.Evaluate((pos * settings[2].scale) + settings[2].centre))) * settings[2].strength;
        v = (v / noiseMaxHeight) * maxHeight;

        if(settings.Count > 3)
        {
            for(int i = 3; i < levelOfDetail + 3 && i < settings.Count; i++)
            {
                v += noise.Evaluate((pos * settings[i].scale) + settings[i].centre) * settings[i].strength;
            }
        }
        return v;
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
        SetPlayerRelativePosition();
    }

    public void SetTerrainManager(TerrainManager t, float maxDistance)
    {
        terrainManager = t;
        playerRelativePosition = player.transform.position - terrainManager.transform.position;
        playerRelativePosition = terrainManager.transform.InverseTransformDirection(playerRelativePosition);
        if (playerRelativePosition.magnitude > maxDistance)
            playerRelativePosition = playerRelativePosition.normalized * maxDistance;
    }

    public void SetPlayerRelativePosition()
    {
        playerRelativePosition = player.transform.position - terrainManager.transform.position;
        playerRelativePosition = terrainManager.transform.InverseTransformDirection(playerRelativePosition);
    }

    public Node GetNode(int faceID, int myLevel, int3 wantedPos)
    {
        return terrainManager.GetNode(faceID, myLevel, wantedPos);
    }

    public Color GetTemperature(float h, float yPos)
    {
        float v = GetT(h, yPos);
        if (v == -1)
            return Color.blue;
        return temperatureGradient.Evaluate(v); 
    }

    float GetT(float h, float yPos)
    {
        h -= planetRadius;
        if (h <= 0)
            return -1;
        yPos = Mathf.Clamp(Mathf.Abs(yPos) / planetRadius, 0, 1);
        if (useCurve)
        {
            yPos = Mathf.Clamp(curve.Evaluate(yPos), 0, 1);
        }
        float v;
        v = h / maxHeight;
        v = 1 - ((v * .3f) + (yPos * .8f));
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



    public Color GetPointColor(int i)
    {
        if (i != 9)
            i = biomeNumber[i];

        BiomeColors b = biomeColors;
        float v = UnityEngine.Random.Range(0.0f, 1.0f);
        return b.biomeList[i].Evaluate(v);
    }

    public Color GetPointColor(int f, float height, float yPos, Vector3 p) {
        float t = GetT(height, yPos);
        if (t == -1)
            return Color.blue; // Only top biomes have contact with water
        float h = GetH(f, p);
        Color c = GetBiomeTexture().GetPixel((int)(GetBiomeTexture().width * t), (int)(GetBiomeTexture().height * h));
        return c;
    }

    public int GetBiomeNumber(int f, float height, float yPos, Vector3 p)
    {
        float t = GetT(height, yPos);
        if (t == -1)
            return 9; // Only top biomes have contact with water
        float h = GetH(f, p);
        Color c = GetBiomeTexture().GetPixel((int)(GetBiomeTexture().width * t), (int)(GetBiomeTexture().height * h));
        string id = ColorUtility.ToHtmlStringRGB(c);
        int index = TerrainInfoData.colorIndexValules[id];
        return biomeNumber[index]; // Modificar después
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

    Texture2D GetBiomeTexture()
    {
        return terrainManager.planetManager.biomeTexture;
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
                        value = noise.Evaluate((point * settings[0].scale) + settings[0].centre);
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
            float value = noise.Evaluate((point * settings[0].scale) + settings[0].centre);
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

    #region TreeGeneration
    public void ActivateTrees(int3 pos, int face, float3 startPoint)
    {
        if (pos.z < 0)
            return;
        int3 key = new int3(pos.x, pos.y, face);
        if (!treeCollection.ContainsKey(key))
            GenerateChunkTrees(key, startPoint);
        TreePerChunk tpc = treeCollection[key];
        if (!tpc.positionSet)
            RecalculatePositions(tpc);
        tpc.activatedChunks++;
        if (tpc.isActive)
            return;
        InstantiateTrees(tpc);
    }

    void RecalculatePositions(TreePerChunk tpc) 
    {
        RaycastHit hit;
        float maxDistance = planetRadius + (maxHeight * 2);
        Vector3 temp;
        for(int i = tpc.treeDataList.Count - 1; i>=0; i--)
        {
            temp = terrainManager.transform.TransformPoint(tpc.treeDataList[i].spherePos);
            if (Physics.Raycast(temp, terrainManager.transform.position - temp, out hit, Mathf.Infinity, terrainManager.planetManager.groundLayer))
                tpc.treeDataList[i].spherePos = terrainManager.transform.InverseTransformPoint(hit.point);
            else
                tpc.treeDataList.RemoveAt(i);
        }
        tpc.positionSet = true;
    }

    public void DesactivateTrees(int3 pos, int face)
    {
        if (pos.z < 0)
            return;
        int3 key = new int3(pos.x, pos.y, face);
        if (treeCollection.ContainsKey(key))
        {
            TreePerChunk tpc = treeCollection[key];
            if(tpc.activatedChunks > 0)
                tpc.activatedChunks--;
            if(tpc.activatedChunks == 0)
            {
                Queue<TreeBase> childTreeList = new Queue<TreeBase>();
                List<GameObject> strayItems = new List<GameObject>();
                for (int i = 0; i < tpc.inGameHolder.childCount; i++)
                {
                    if (tpc.inGameHolder.GetChild(i).CompareTag("Item"))
                        strayItems.Add(tpc.inGameHolder.GetChild(i).gameObject);
                    else
                        childTreeList.Enqueue(tpc.inGameHolder.GetChild(i).GetComponent<TreeBase>());
                }

                int index = 0;
                foreach(GameObject g in strayItems)
                    GameObject.Destroy(g);

                while (childTreeList.Count > 0)
                {
                    if (!childTreeList.Peek().gameObject.activeInHierarchy)
                        tpc.treeDataList.RemoveAt(index);
                    else
                        index++;
                    terrainManager.planetManager.DesactivateTree(childTreeList.Dequeue());
                }
                terrainManager.planetManager.DesactivateTreeHolder(tpc.inGameHolder.transform);
                tpc.isActive = false;
                tpc.inGameHolder = null;
            }
        }
    }

    void InstantiateTrees(TreePerChunk tpc)
    {
        tpc.isActive = true;
        Transform holder = terrainManager.planetManager.GetTreeHolder();
        holder.transform.parent = terrainManager.treeHoldersParent;
        holder.localPosition = Vector3.zero;
        holder.localRotation = Quaternion.identity;
        tpc.inGameHolder = holder;
        TreeBase temp;
        Vector3 vT = Vector3.zero;
        foreach(TreeData t in tpc.treeDataList)
        {
            temp = terrainManager.planetManager.GetTree(t.biome, t.id);
            if(temp == null)
            {
                //Debug.LogError("Tree not found");
                return;
            }
            temp.transform.parent = holder;
            temp.transform.up = t.spherePos;

            vT.Set(0, UnityEngine.Random.Range(0, 360), 0);
            temp.transform.localEulerAngles = temp.transform.eulerAngles;
            temp.transform.Rotate(vT, Space.Self);

            temp.transform.localPosition = t.spherePos;

            

            temp.Activate();
        }
    }

    void GenerateChunkTrees(int3 id, float3 startPoint)
    {
        // Generate flora
        startPoint[TerrainManagerData.axisIndex[id.z][2]] = faceStart[id.z][TerrainManagerData.axisIndex[id.z][2]];
        TreePerChunk tpc = new TreePerChunk();
        float size = maxResolution;
        int treeCount = 0;
        int notCreatedCount = 0;
        float3 pos = float3.zero;
        TreeData t;
        while (notCreatedCount < terrainManager.planetManager.missedTreesMax && treeCount < terrainManager.planetManager.maxTrees)
        {
            pos.x = UnityEngine.Random.Range(0, size);
            pos.y = UnityEngine.Random.Range(0, size);
            t = GenerateTreeData(pos, startPoint, id.z);
            if (t != null && isCreatable(tpc, t))
            {
                tpc.AddTree(t);
                notCreatedCount = 0;
                treeCount++;
            }
            else
                notCreatedCount++;
        }
        treeCollection.Add(id, tpc);
    }

    TreeData GenerateTreeData(float3 pos, float3 startPoint, int f)
    {
        TreeData td = new TreeData();

        Vector3 cubePos;
        Vector3 spherePos;
        cubePos = startPoint + (pos.x * TerrainManagerData.dir[f].c0) +
            (pos.y * TerrainManagerData.dir[f].c1);

        float height = GetNoiseValue((float3)cubePos, 0);
        if (height <= 0)
            return null;

        height += planetRadius;
        spherePos = cubePos.normalized * height;
        td.biome = GetBiomeNumber(f, height, spherePos.y, cubePos);

        td.id = 0;
        if (CalculateTreeType(cubePos, terrainManager.planetManager.scale1, terrainManager.planetManager.offset1))
            td.id++;
        if (CalculateTreeType(cubePos, terrainManager.planetManager.scale2, terrainManager.planetManager.offset2))
            td.id += 2;
        td.radius = terrainManager.planetManager.GetTreeRadius(td.biome, td.id);
        td.cubePos = cubePos;
        td.spherePos = spherePos.normalized * (planetRadius + maxHeight);
        return td;
    }

    bool CalculateTreeType(Vector3 pos, float scale, Vector3 offset)
    {
        pos += offset;
        pos *= scale;
        float sample = noise.Evaluate(pos);
        return sample < 0;
    }

    bool isCreatable(TreePerChunk tpc, TreeData newTree)
    {
        float dif;
        foreach (TreeData t in tpc.treeDataList)
        {
            dif = (newTree.cubePos - t.cubePos).magnitude;
            if (dif <= t.radius || dif <= newTree.radius)
            {
                return false;
            }
        }
        return true;
    }
    #endregion

}

[Serializable]
public class NoiseSettings
{
    public float strength;
    public float scale;
    public float3 centre = new float3();

    public NoiseSettings(float st, float sc, bool r)
    {
        strength = st;
        scale = sc;
        if (r)
            centre = new float3(
                UnityEngine.Random.Range(-100.0f, 100.0f), 
                UnityEngine.Random.Range(-100.0f, 100.0f), 
                UnityEngine.Random.Range(-100.0f, 100.0f)
                );
    }

    public NoiseSettings()
    {
        strength = 1;
        scale = 1;
    }
}

public static class TerrainInfoData
{
    public static Dictionary<string, int> colorIndexValules = new Dictionary<string, int>()
    {
        {"991717", 0},  // Selva Tropical
        {"DF410D", 1},  // Bosque Tropical
        {"6FDF0D", 2},  // Sabana
        {"17997B", 3},  // Selva Templada
        {"4D1799", 4},  // Bosque Templado
        {"9A46D1", 5},  // Herbazal
        {"46D164", 6},  // Taiga
        {"4682D1", 7},  // Tundra
        {"D9C01C", 8},  // Desierto
        //         9       Agua
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

    public static string[] biomeName =
    {
        "Tropical Rainforest",          //Selva Tropical
        "Tropical Seasonal Forest",     //Bosque Tropical
        "Savannah",                     //Sabana
        "Temperate Rainforest",         //Selva Templada
        "Temperate Forest",             //Bosque Templado
        "Grassland",                    //Herbazal
        "Boreal Forest",                //Taiga
        "Tundra",                       //Tundra
        "Desert",                       //Desierto
        "Sea"                           //Mar"
    };

    public static string[] biomeN =
    {
        "Tropical Rainforest",          //Selva Tropical
        "Tropical Seasonal Forest",     //Bosque Tropical
        "Savannah",                     //Sabana
        "Temperate Rainforest",         //Selva Templada
        "Temperate Forest",             //Bosque Templado
        "Grassland",                    //Herbazal
        "Boreal Forest",                //Taiga
        "Tundra",                       //Tundra
        "Desert"                        //Desierto
    };
}
