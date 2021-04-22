using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetsManager : MonoBehaviour
{
    public Transform player;

    public bool generateNewPlanets;
    public bool useCreatedPlanets;

    // Generate New Planets
    public GeneratorSettingsWrapper settingsWrapper;
    public PlanetGeneratorSettings settings { get { return settingsWrapper.settings; } }

    public int minPlanets;
    public int maxPlanets;

    public int minBiomesPerPlanet;
    public int maxBiomesPerPlanet;

    public bool useAllBiomes;

    public float numOrbits;
    public bool firstOrbitAlone;        // Solo un planeta en la primera órbita
    public float firstOrbitPos;
    public float distancePerOrbits;
    public int maxPlanetsPerOrbit;
    public float elevationVariant;

    // Planets Already Generated

    public bool instantiateTrees;
    public LayerMask groundLayer;
    public string groundTag;
    public List<TerrainManager> planets = new List<TerrainManager>();
    public BiomeColorWrapper biomeColorWrapper;
    public BiomeColors biomeColors { get { return biomeColorWrapper.biomeColors; } }
    public Material planetMaterial;

    public float plantSizeAlteration;
    public TreeSets treeSet;
    public AtmosphereSettings atmosphere;

    public Texture2D biomeTexture;

    GameManager gameManager;

    Transform inactiveTree;
    Transform inactiveChunk;
    Transform inactiveTreeHolder;

    Queue<Chunk> inactiveChunkList = new Queue<Chunk>();
    Dictionary<int, Queue<TreeBase>> inactiveTreeList = new Dictionary<int, Queue<TreeBase>>();
    Queue<Transform> inactiveTreeHolderList = new Queue<Transform>();

    List<TerrainManager> unusedPlanets;
    List<TerrainManager> newPlanets;
    TerrainManager terrainTemp;

    private void OnValidate()
    {
        if (player == null)
            player = GameObject.FindObjectOfType<PlayerManager>().transform;
        foreach (TerrainManager t in planets)
        {
            if(t != null)
                t.planetManager = this;
        }
    }

    private void Awake()
    {
        CreatedPlanets();
        if (generateNewPlanets)
            GeneratePlanets();
        for(int i = 0; i < planets.Count; i++)
        {
            planets[i].gameObject.GetComponent<PlanetaryBody>().SetValues(i, planets[i], atmosphere);
        }
        Initialize();
        terrainTemp = null;
    }

    public void SetGameManager(GameManager g)
    {
        gameManager = g;
    }

    public void UpdatePlanets(Vector3 move)
    {
        foreach(TerrainManager t in planets)
            t.transform.position = t.transform.position - move;
    }


    #region GeneratePlanets
    void GeneratePlanets()
    {
        int numPlanets = Random.Range(minPlanets, maxPlanets + 1);
        newPlanets = new List<TerrainManager>();
        GameObject g;
        for(int i = 0; i < numPlanets; i++)
        {
            g = new GameObject("Planet", typeof(TerrainManager), typeof(PlanetaryBody));
            terrainTemp = g.GetComponent<TerrainManager>();
            SetPlanetData();
            newPlanets.Add(terrainTemp);
        }
        SetBiomes();
        NewPositions();
    }

    void NewPositions()
    {
        List<int> planetInOrbit = new List<int>();
        DistributePlanets(ref planetInOrbit);
        float dis, startAngle, angleMove;
        int planetCount = 0;
        Vector3 pos;

        for(int i = 0; i < planetInOrbit.Count; i++)
        {
            dis = firstOrbitPos + (i * distancePerOrbits);
            startAngle = Random.Range(0.0f, 360.0f);
            angleMove = 360.0f / planetInOrbit[i];

            for (int j = 0; j < planetInOrbit[i]; j++)
            {
                pos = Quaternion.AngleAxis(startAngle + (j * angleMove), Vector3.up) * Vector3.right;
                pos = pos.normalized * dis;
                pos.y = Random.Range(-elevationVariant, elevationVariant);
                planets[planetCount].transform.position = pos;
                planetCount++;
            }
        }

    }

    void DistributePlanets(ref List<int> planetInOrbit)
    {
        if (planets.Count == 0)
            return;

        List<int> availableOrbits = new List<int>();
        int planetCount = 0;

        for (int i = 0; i < numOrbits; i++)
        {
            availableOrbits.Add(i);
            planetInOrbit.Add(0);
        }

        if (firstOrbitAlone)
        {
            availableOrbits.RemoveAt(0);
            planetInOrbit[0] = 1;
            planetCount++;
        }

        int rand;
        for(; planetCount < planets.Count; planetCount++)
        {
            rand = Random.Range(0, availableOrbits.Count);
            planetInOrbit[availableOrbits[rand]]++;
            if (availableOrbits[rand] >= maxPlanetsPerOrbit)
                availableOrbits.RemoveAt(rand);
        }
    }

    void SetPlanetData()
    {
        terrainTemp.generatedByManager = true;
        terrainTemp.planetManager = this;
        terrainTemp.planetData = new TerrainInfo(settings.isMarchingCube, settings.minChunkPerFace, settings.maxChunkPerFace,
            settings.chunkDetail, settings.humidityCount, instantiateTrees);
        terrainTemp.planetData.player = player;
        terrainTemp.planetData.planetRadius = Random.Range(settings.minPlanetRadius, settings.maxPlanetRadius);
        terrainTemp.planetData.maxHeight = Random.Range(settings.minMaxHeight, settings.maxMaxHeight);
        SetNoise();
    }

    void SetNoise()
    {
        float firstValue = Random.Range(0, 1.0f);
        terrainTemp.planetData.humidityMove = Mathf.Lerp(settings.minHumidityMove, settings.maxHumidityMove, firstValue); // Make it mor random later
        List<NoiseSettings> noiseList = new List<NoiseSettings>();
        noiseList.Add(new NoiseSettings());
        noiseList[0].centre = new Unity.Mathematics.float3(Random.Range(-1000, 1000), Random.Range(-1000, 1000), Random.Range(-1000, 1000));
        noiseList[0].scale = Mathf.Lerp(settings.minSettings[0].scale, settings.maxSettings[0].scale, firstValue);
        noiseList[0].strength = Random.Range(settings.minSettings[0].strength, settings.maxSettings[0].strength);

        for(int i = 1; i <  settings.settingsLength; i++)
        {
            noiseList.Add(new NoiseSettings());
            noiseList[i].centre = new Unity.Mathematics.float3(Random.Range(-1000, 1000), Random.Range(-1000, 1000), Random.Range(-1000, 1000));
            noiseList[i].scale = Random.Range(settings.minSettings[i].scale, settings.maxSettings[i].scale);
            noiseList[i].strength = Random.Range(settings.minSettings[i].strength, settings.maxSettings[i].strength);
        }
        terrainTemp.planetData.settings = noiseList;
    }

    void SetBiomes()
    {
        List<BiomeDecider> planetBiomes = new List<BiomeDecider>();
        List<int> tempList = new List<int>(new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 });

        for (int i = 0; i < newPlanets.Count; i++)
            planetBiomes.Add(new BiomeDecider(Random.Range(minBiomesPerPlanet, maxBiomesPerPlanet + 1)));

        int biome, rand;
        while(planetBiomes.Count > 0)
        {
            for(int i = planetBiomes.Count - 1; i >= 0; i--)
            {
                if (tempList.Count == 0)
                    biome = Random.Range(0, 9);
                else
                {
                    rand = Random.Range(0, tempList.Count);
                    biome = tempList[rand];
                    tempList.RemoveAt(rand);
                }

                planetBiomes[i].AddBiome(biome);
                if (planetBiomes[i].hasAllBiomes())
                {
                    SetPlanetBiomes(planetBiomes[i]);
                    planetBiomes.RemoveAt(i);
                }
            }
        }
    }

    void SetPlanetBiomes(BiomeDecider b)
    {
        newPlanets[0].planetData.SetBiomes(b.FinalList());
        planets.Add(newPlanets[0]);
        newPlanets.RemoveAt(0);
    }
    #endregion

    void CreatedPlanets()
    {
        if (!useCreatedPlanets)
        {
            unusedPlanets = new List<TerrainManager>();
            foreach(TerrainManager t in planets)
            {
                if(t != null)
                {
                    t.gameObject.SetActive(false);
                    unusedPlanets.Add(t);
                }
            }
            planets.Clear();
        }
        else
        {
            for(int i = planets.Count - 1; i >= 0; i--)
            {
                if (planets[i] == null)
                    planets.RemoveAt(i);
            }
        }
    }

    public void Initialize()
    {
        if (inactiveTree == null)
            inactiveTree = CreateGameObject("InactiveTree");
        if (inactiveChunk == null)
            inactiveChunk = CreateGameObject("InactiveChunk");
        if (inactiveTreeHolder == null)
            inactiveTreeHolder = CreateGameObject("InactiveTreeHolder");
    }

    Transform CreateGameObject(string name)
    {
        GameObject g = new GameObject(name);
        g.transform.parent = transform;
        return g.transform;
    }

    public void DesactivateChunk(Node n)
    {
        n.inGameChunk.Desactivate();
        n.inGameChunk.transform.parent = inactiveChunk;
        inactiveChunkList.Enqueue(n.inGameChunk);
        n.inGameChunk = null;
    }

    public Chunk GetChunk()
    {
        Chunk c;
        if(inactiveChunkList.Count > 0)
            c = inactiveChunkList.Dequeue();
        else
        {
            GameObject g = new GameObject("Chunk", typeof(MeshRenderer), typeof(MeshFilter), typeof(MeshCollider), typeof(Chunk));
            c = g.GetComponent<Chunk>();
            g.layer = 8;
            if(groundTag != "")
                g.tag = groundTag;
        }
        c.gameObject.SetActive(true);
        return c;
    }

    public TreeBase GetTree(int biome, int id)
    {
        if (treeSet.biomeTrees[biome] == null)
            return null;
        return GetTree(treeSet.biomeTrees[biome].GetPrefab(id));
    }

    public TreeBase GetTree(GameObject prefab)
    {
        if (prefab == null)
            return null;
        TreeBase t = prefab.GetComponent<TreeBase>();
        TreeBase r;
        if (inactiveTreeList.ContainsKey(t.id))
        {
            if (inactiveTreeList[t.id].Count > 0)
                r = inactiveTreeList[t.id].Dequeue();
            else
                r = GenerateNewTree(prefab);
        }
        else
            r = GenerateNewTree(prefab);
        r.gameObject.SetActive(true);
        return r;
    }

    TreeBase GenerateNewTree(GameObject prefab)
    {
        TreeBase t = Instantiate(prefab).GetComponent<TreeBase>();
        float r = Random.Range(1 - plantSizeAlteration, 1 + plantSizeAlteration);
        t.transform.localScale = new Vector3(r, r, r);
        return t;
    }

    public void DesactivateTree(TreeBase t)
    {
        int id = t.id;
        if (!inactiveTreeList.ContainsKey(id))
        {
            inactiveTreeList.Add(id, new Queue<TreeBase>());
        }
        inactiveTreeList[id].Enqueue(t);
        t.gameObject.transform.parent = inactiveTree;
        t.gameObject.SetActive(false);
    }

    public Transform GetTreeHolder()
    {
        Transform t;
        if (inactiveTreeHolderList.Count > 0)
            t = inactiveTreeHolderList.Dequeue();
        else
            t = new GameObject("TreeHolder").transform;
        t.gameObject.SetActive(true);
        return t;
    }

    public void DesactivateTreeHolder(Transform th)
    {
        inactiveTreeHolderList.Enqueue(th);
        th.transform.parent = inactiveTreeHolder;
        th.gameObject.SetActive(false);
    }

    public Color GetPointColor(int i)
    {
        float v;
        int maxValue = biomeColors.biomeList[i].colors.Length - 1;
        //v = Random.Range(0.0f, biomeColors.biomeList[i].limits[2]);
        v = Random.Range(0.0f, biomeColors.biomeList[i].limits[maxValue]);

        for(int j = 0; j < maxValue; j++)
        {
            if (v < biomeColors.biomeList[i].limits[j])
                return biomeColors.biomeList[i].colors[j];
        }
        return biomeColors.biomeList[i].colors[maxValue];
    }
}

/// <summary>
/// Facilitates biome deciding in new planets
/// </summary>
public class BiomeDecider
{
    int biomeQuantity;
    bool[] availableBiomes = new bool[] { false, false, false, false, false, false, false ,false, false };
    List<int> currentBiomes;

    public BiomeDecider(int q)
    {
        currentBiomes = new List<int>();
        biomeQuantity = q;
    }

    public bool hasAllBiomes()
    {
        return currentBiomes.Count >= biomeQuantity;
    }

    public void AddBiome(int i)
    {
        if (!availableBiomes[i])
        {
            currentBiomes.Add(i);
            availableBiomes[i] = true;
        }
        return;
    }

    public int[] FinalList()
    {
        int[] biomes = new int[9];
        for(int i = 0; i < biomes.Length; i++)
        {
            if (availableBiomes[i])
                biomes[i] = i;
            else
                biomes[i] = currentBiomes[Random.Range(0, currentBiomes.Count)];
        }
        return biomes;
    }

}