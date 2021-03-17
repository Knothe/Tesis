using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public class AccomodatingPlanetsTest : MonoBehaviour
{
    public int planetCount;
    public GameObject planetPrefab;
    public Vector3 axis;
    public float angle; // grados

    List<Transform> objects;
    List<int> radius;

    List<PlanetNode> planets;

    private void Start()
    {
        if (planetCount <= 0)
            return;

        objects = new List<Transform>();
        radius = new List<int>();
        for(int i = 0; i < planetCount; i++)
        {
            int r = UnityEngine.Random.Range(5, 10);
            objects.Add(Instantiate(planetPrefab).transform);
            radius.Add(r);
            objects[i].localScale = Vector3.one * (2 * r);
        }

        LocatePlanets();
    }

    void CalculateSomething()
    {
        axis.Normalize();
        angle *= Mathf.PI / 180;

        float3x3 matrix = float3x3.zero;

        matrix.c0.x = Mathf.Cos(angle) + ((axis.x * axis.x) * (1 - Mathf.Cos(angle)));
        matrix.c0.y = (axis.x * axis.y * (1 - Mathf.Cos(angle))) - (axis.z * Mathf.Sin(angle));
        matrix.c0.z = (axis.x * axis.z * (1 - Mathf.Cos(angle))) + (axis.y * Mathf.Sin(angle));

        matrix.c0.x = (axis.x * axis.y * (1 - Mathf.Cos(angle))) + (axis.z * Mathf.Sin(angle));
        matrix.c1.y = Mathf.Cos(angle) + ((axis.y * axis.y) * (1 - Mathf.Cos(angle)));
        matrix.c1.z = (axis.z * axis.y * (1 - Mathf.Cos(angle))) - (axis.x * Mathf.Sin(angle));

        matrix.c0.x = (axis.x * axis.z * (1 - Mathf.Cos(angle))) - (axis.y * Mathf.Sin(angle));
        matrix.c2.y = (axis.z * axis.y * (1 - Mathf.Cos(angle))) + (axis.x * Mathf.Sin(angle));
        matrix.c2.z = Mathf.Cos(angle) + ((axis.z * axis.z) * (1 - Mathf.Cos(angle)));

        Vector3 v = Vector3.zero;
        for (int i = 0; i < 3; i++)
        {
            v[i] = transform.forward.x * matrix[i].x;
            v[i] += transform.forward.y * matrix[i].y;
            v[i] += transform.forward.z * matrix[i].z;
        }

        transform.forward = v;
    }

    void LocatePlanets()
    {
        FirstPlanet();

        for(int i = 1; i < objects.Count; i++)
        {

            planets.Add(new PlanetNode(objects[i], radius[i]));
        }


    }

    void FirstPlanet()
    {
        planets = new List<PlanetNode>();
        Vector3 dir = new Vector3(UnityEngine.Random.Range(-1.0f, 1.0f), UnityEngine.Random.Range(-1.0f, 1.0f), UnityEngine.Random.Range(-1.0f, 1.0f));

        objects[0].position = dir.normalized * radius[0];

        planets.Add(new PlanetNode(objects[0], radius[0]));
    }

    void SetConnections(PlanetNode n)
    {

    }

    void OnePlanet(Vector3 reference, Transform planet)
    {
        Vector3 dir = new Vector3(UnityEngine.Random.Range(-1.0f, 1.0f), UnityEngine.Random.Range(-1.0f, 1.0f), UnityEngine.Random.Range(-1.0f, 1.0f));

    }
}


public class PlanetNode
{
    public Transform planet { get; private set; }
    public float radius { get; private set; }
    public List<PlanetNode> connect { get; set; }

    public PlanetNode(Transform p, float r)
    {
        planet = p;
        radius = r;
    }
}