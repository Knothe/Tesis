 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

[RequireComponent(typeof(TerrainManager))]
public class PlanetaryBody : MonoBehaviour
{
    public float gravityValue = 10;
    public float2 spaceShipRotation = new float2(.001f, 1.5f); // x -> min, y -> max
    public Vector3 rotation;

    PlayerManager player;
    float dif;
    bool isInside;
    float gravityEffectStart;
    float gravityDiv;
    float mod;

    private void Start()
    {
        isInside = false;
        TerrainManager terrain = gameObject.GetComponent<TerrainManager>();
        gravityEffectStart = terrain.planetData.planetRadius + (3 * terrain.planetData.maxHeight);
        gravityDiv = 2 * terrain.planetData.maxHeight;
        mod = 0;
        player = terrain.planetData.player.gameObject.GetComponent<PlayerManager>();
    }

    private void Update()
    {
        gameObject.transform.Rotate(rotation, Space.World);
        dif = gravityEffectStart - (transform.position - player.transform.position).magnitude;

        if (dif <= 0)
        {
            if (isInside)
                ExitedPlanet();
        }
        else
        {
            if (!isInside)
                EnteredPlanet();
            mod = Mathf.Clamp(dif, 0, gravityDiv) / gravityDiv;
            
        }
    }

    void EnteredPlanet()
    {
        isInside = true;
        player.EnteredPlanet(this);
    }

    void ExitedPlanet()
    {
        isInside = false;
        mod = 0;
        player.ExitedPlanet(this);

    }

    public float Rotate(Transform t)
    {
        Vector3 gravityUp = (t.position - transform.position).normalized;
        Vector3 bodyUp = t.up;
        Quaternion targetRotation = Quaternion.FromToRotation(bodyUp, gravityUp) * t.rotation;
        t.rotation = Quaternion.Slerp(t.rotation, targetRotation, Mathf.Lerp(spaceShipRotation.x, spaceShipRotation.y, mod) * Time.deltaTime);
        return mod;
    }

    public void Attract(Transform t, Rigidbody rb)
    {
        Vector3 gravityUp = (t.position - transform.position).normalized;
        Vector3 bodyUp = t.up;
        rb.AddForce(gravityUp * -gravityValue);
        Quaternion targetRotation = Quaternion.FromToRotation(bodyUp, gravityUp) * t.rotation;
        t.rotation = Quaternion.Slerp(t.rotation, targetRotation, 50 * Time.deltaTime); 
    }
}
