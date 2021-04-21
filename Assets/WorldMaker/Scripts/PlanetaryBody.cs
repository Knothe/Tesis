using UnityEngine;
using Unity.Mathematics;
using UnityEditor;

/// <summary>
/// Manages all effects that don't interact directly with the terrain
/// </summary>
[RequireComponent(typeof(TerrainManager))]
public class PlanetaryBody : MonoBehaviour
{
    public int id;
    public float gravityValue = 10;
    public float2 spaceShipRotation = new float2(.001f, 1.5f); // x -> min, y -> max
    public Vector3 rotation;

    public AtmosphereSettings atmosphereSettings;
    public TerrainManager terrainManager;

    // All these values are used by the atmospheric shader
    public float planetRadius { get { return terrainManager.planetData.planetRadius; } }
    /// <summary> Max height of the terrain </summary>
    public float planetHeight { get{ return terrainManager.planetData.maxHeight; } }
    /// <summary> True if player is in this planet atmosphere </summary>
    public bool planetAtmosphere { get { return !(!player.playerIsSpace && player.currentPlanet == this); } }
    /// <summary> Indicates if the atmosphere shader needs an uppdate </summary>
    public bool updateAtmosphere { get {
            bool r = lastAtmosphere != planetAtmosphere;
            lastAtmosphere = planetAtmosphere;
            return r;
        } }

    bool lastAtmosphere;


    PlayerManager player;
    float dif;
    bool isInside;
    float gravityEffectStart;
    float gravityDiv;
    float mod;
    private void OnValidate()
    {
        if(terrainManager == null)
            terrainManager = GetComponent<TerrainManager>();
        if(!Application.isPlaying && player == null)
            player = terrainManager.planetData.player.gameObject.GetComponent<PlayerManager>();
        if(atmosphereSettings == null)
            atmosphereSettings = Resources.Load<AtmosphereSettings>("AtmosphereValues");

    }

    private void Start()
    {
        isInside = false;
        terrainManager = gameObject.GetComponent<TerrainManager>();
        gravityEffectStart = terrainManager.planetData.planetRadius + (3 * terrainManager.planetData.maxHeight);
        gravityDiv = 2 * terrainManager.planetData.maxHeight;
        mod = 0;
        player = terrainManager.planetData.player.gameObject.GetComponent<PlayerManager>();
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

    /// <summary>
    /// Sets starting values
    /// </summary>
    /// <param name="i">Planet index</param>
    public void SetValues(int i, TerrainManager t, AtmosphereSettings a)
    {
        id = i;
        terrainManager = t;
        atmosphereSettings = a;
    }

    /// <summary>
    /// Called when player enters the planet
    /// </summary>
    void EnteredPlanet()
    {
        isInside = true;
        player.EnteredPlanet(this);
    }

    /// <summary>
    /// Called when player exits the planet
    /// </summary>
    void ExitedPlanet()
    {
        isInside = false;
        mod = 0;
        player.ExitedPlanet(this);

    }

    /// <summary>
    /// Rotate spaceship as a consecuence of gravitational force
    /// </summary>
    /// <param name="t">Spaceship transform</param>
    /// <returns>Value between 0 and 1, indicates force of rotation</returns>
    public float Rotate(Transform t)
    {
        Vector3 gravityUp = (t.position - transform.position).normalized;
        Vector3 bodyUp = t.up;
        Quaternion targetRotation = Quaternion.FromToRotation(bodyUp, gravityUp) * t.rotation;
        t.rotation = Quaternion.Slerp(t.rotation, targetRotation, Mathf.Lerp(spaceShipRotation.x, spaceShipRotation.y, mod) * Time.deltaTime);
        return mod;
    }

    /// <summary>
    /// Attracts a body to its surface, distance to center doesn't modify the force of attraction
    /// </summary>
    /// <param name="t">Body to attract</param>
    /// <param name="rb">Rigidbody of the object</param>
    public void Attract(Transform t, Rigidbody rb)
    {
        Vector3 gravityUp = (t.position - transform.position).normalized;
        Vector3 bodyUp = t.up;
        rb.AddForce(gravityUp * -gravityValue);
        Quaternion targetRotation = Quaternion.FromToRotation(bodyUp, gravityUp) * t.rotation;
        t.rotation = Quaternion.Slerp(t.rotation, targetRotation, 50 * Time.deltaTime); 
    }
}
