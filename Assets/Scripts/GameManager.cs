using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public Transform player;

    public PlanetsManager planetManager;
    public PlayerManager playerManager;

    public List<GameObject> asteroidPrefab;

    public float maxDistance;

    // Asteroids
    public int asteroidQuantity;
    public float asteroidMinDistance;
    public float asteroidMaxDistance;

    public float asteroidMinSize;
    public float asteroidMaxSize;

    public float asteroidsPerDistance;
    public float distanceForAsteroids;
    public float asteroidActiveDistance;


    bool asteroidActive;
    int asteroidCount;
    float newMaxDistance;

    Transform asteroidContainer;
    Queue<Asteroid> unusedAsteroids;
    Transform closestPlanet = null;
    float distanceAdvanced;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = false;
        playerManager.SetGameManager(this);
        planetManager.SetGameManager(this);
        newMaxDistance = maxDistance * maxDistance;
        asteroidContainer = new GameObject("Asteroid Container").transform;
        asteroidContainer.position = Vector3.zero;
        asteroidActive = true;
        distanceAdvanced = 0;
        unusedAsteroids = new Queue<Asteroid>();
        asteroidCount = 0;
        GenerateNewAsteroids();
    }

    void Update()
    {
        if(player.transform.position.sqrMagnitude > newMaxDistance)
        {
            Vector3 move = player.transform.parent.position;
            planetManager.UpdatePlanets(move);

            if (asteroidActive)
            {
                asteroidContainer.position -= move;
            }

            player.transform.parent.position = Vector3.zero;

            if(closestPlanet == null)
            {
                distanceAdvanced += move.magnitude;
                GenerateAsteroids((int)(distanceAdvanced / distanceForAsteroids));

                foreach(TerrainManager t in planetManager.planets)
                {
                    if (t.transform.position.magnitude < asteroidActiveDistance)
                    {
                        closestPlanet = t.transform;
                        break;
                    }
                }
            }
            else
            {
                if (asteroidActive && asteroidCount == unusedAsteroids.Count)
                    asteroidActive = false;

                if (closestPlanet.position.magnitude > asteroidActiveDistance)
                {
                    closestPlanet = null;
                    asteroidActive = true;
                    asteroidContainer.transform.position = Vector3.zero;
                }
            }
        }
    }

    void GenerateAsteroids(int n)
    {
        if (n <= 0)
            return;

        Vector3 v0 = playerManager.onSpace.movement;
        Vector3 v1 = new Vector3(v0.y * v0.z, v0.x * v0.z, -2 * v0.x * v0.y);
        Vector3 v2 = Vector3.Cross(v0, v1);
        Vector3 pos;
        GameObject temp;

        for (int i = 0; i < n * asteroidsPerDistance; i++)
        {
            temp = GenerateNewAsteroid();
            pos = v1.normalized * (Random.Range(-.5f, .5f)) +
            v2.normalized * (Random.Range(-.5f, .5f));
            pos = (v0.normalized + pos).normalized * (asteroidMaxDistance - distanceForAsteroids);
            temp.transform.position = pos;
        }

        distanceAdvanced = distanceAdvanced - (distanceForAsteroids * n);
    }

    public void DesactivateAsteroid(Asteroid a)
    {
        a.gameObject.SetActive(false);
        unusedAsteroids.Enqueue(a);
    }

    void GenerateNewAsteroids()
    {
        GameObject temp;
        for(int i = 0; i < asteroidQuantity; i++)
        {
            temp = GenerateNewAsteroid();
            temp.transform.position = (new Vector3(Random.Range(-1.0f, 1.0f), 
                Random.Range(-1.0f, 1.0f), 
                Random.Range(-1.0f, 1.0f))).normalized *
                Random.Range(asteroidMinDistance, asteroidMaxDistance);
        }
    }

    GameObject GenerateNewAsteroid()
    {
        GameObject obj;
        if(unusedAsteroids.Count == 0)
        {
            obj = Instantiate(asteroidPrefab[Random.Range(0, asteroidPrefab.Count)]);
            obj.GetComponent<Asteroid>().SetValues(this, asteroidMaxDistance);
            float rand = Mathf.Lerp(asteroidMinSize, asteroidMaxSize, Random.Range(0.0f, 1.0f));
            obj.transform.localScale = new Vector3(rand, rand, rand);
            obj.transform.parent = asteroidContainer;
            asteroidCount++;
        }
        else
            obj = unusedAsteroids.Dequeue().gameObject;
        obj.SetActive(true);
        return obj;
    }

    public void FinishGame(bool s)
    {
        if (s)
            SceneManager.LoadScene(0);
        else
            SceneManager.LoadScene(0);
    }

    public void ExitGame()
    {
        SceneManager.LoadScene(0);

    }
}
