using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    Manage Player States and inventory
    Game Pauses from here

    Lista de obtenibles:
        0   Madera Roja
        1   Madera Café
        2   Madera Obscura
        3   Madera Blanca
        4   Metal Marrón
        5   Metal Claro
        6   Metal Obscuro
        7   Metal Rojo
        8   Agua
        9   Leche
        10  Jugo
        11  Resina Amarilla
        12  Resina Naranja
        13  Resina Verde
        14  Resina Gris
        15  Hule
        16  Vidrio
        17  Manzana
        18  Plátano 
        19  Pera
 */


// Manage Player States and inventory
// Game Pauses from here

// Lista de obtenibles

public class PlayerManager : MonoBehaviour
{
    public PlayerController onPlanet;
    public ShipController onSpace;

    public int minRandomMission;
    public int maxRandomMission;
    public int lifeRecovery;
    public int maxItems;
    public PlanetaryBody currentPlanet { get; private set; }
    GameManager gameManager;

    public bool playerIsSpace { get { return onSpace.enabled; } }

    void Start()
    {
        onPlanet.CustomStart(this);
        onSpace.CustomStart(this);
        currentPlanet = null;
        EnterShip();
    }

    public void SetGameManager(GameManager g)
    {
        gameManager = g;
    }

    void Update()
    {
        
    }

    public void PlayerLanded()
    {

    }

    public void ShipCrashed()
    {
    }

    public void EnteredPlanet(PlanetaryBody planet)
    {
        currentPlanet = planet;
        onSpace.SetCurrentPlanet(currentPlanet);
    }

    public void ExitedPlanet(PlanetaryBody planet)
    {
        if(planet == currentPlanet)
        {
            currentPlanet = null;
            onSpace.DesactivatePlanet();
        }
    }

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
