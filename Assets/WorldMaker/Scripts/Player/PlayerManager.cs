using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public PlayerController onPlanet;
    public ShipController onSpace;

    /// <summary> Player is inside this planet atmosphere </summary>
    public PlanetaryBody currentPlanet { get; private set; }
    /// <summary>
    /// Checks if player is in space
    /// </summary>
    public bool playerIsSpace { get { return onSpace.enabled; } }

    void Start()
    {
        onPlanet.CustomStart(this);
        onSpace.CustomStart(this);
        currentPlanet = null;
        EnterShip();
    }

    /// <summary>
    /// Called when player enters planet
    /// </summary>
    /// <param name="planet">Planet entered</param>
    public void EnteredPlanet(PlanetaryBody planet)
    {
        currentPlanet = planet;
        onSpace.SetCurrentPlanet(currentPlanet);
    }

    /// <summary>
    /// Called when player exits planet
    /// </summary>
    /// <param name="planet">Planet exited</param>
    public void ExitedPlanet(PlanetaryBody planet)
    {
        if(planet == currentPlanet)
        {
            currentPlanet = null;
            onSpace.DesactivatePlanet();
        }
    }

    /// <summary>
    /// Called when player exits ship
    /// </summary>
    /// <param name="up">Up vector of the player</param>
    /// <param name="point">Point where the player will be positioned</param>
    public void ExitShip(Vector3 up, Vector3 point)
    {
        onSpace.gameObject.SetActive(false);
        onPlanet.gameObject.SetActive(true);
        onPlanet.transform.position = point;
        onPlanet.transform.up = up;
        onPlanet.currentPlanet = currentPlanet;
        transform.parent = onPlanet.gameObject.transform;
        transform.localPosition = Vector3.zero;
    }

    /// <summary>
    /// Called when player enters ship
    /// </summary>
    public void EnterShip()
    {
        onSpace.gameObject.SetActive(true);
        onPlanet.gameObject.SetActive(false);
        onSpace.transform.position = onPlanet.shipSpawn.position;
        onSpace.transform.rotation = onPlanet.shipSpawn.rotation;
        transform.parent = onSpace.gameObject.transform;
        transform.localPosition = Vector3.zero;
    }
}
