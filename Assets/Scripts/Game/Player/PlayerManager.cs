using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    Manage Player States and inventory
    Game Pauses from here

    Lista de obtenibles:
        0   Madera 1
        1   Madera 2
        2   Madera 3
        3   Madera 4
        4   Metal 1
        5   Metal 2
        6   Metal 3
        7   Metal 4
        8   Agua
        9   Leche
        10  Jugo
        11  Resina 1
        12  Resina 2
        13  Resina 3
        14  Resina 4
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
    public GameObject pauseMenu;

    PlanetaryBody currentPlanet;

    int[] obtenibles = new int[20];
    bool isPaused = false;

    void Start()
    {
        obtenibles = new int[20];
        onPlanet.CustomStart(this);
        onSpace.CustomStart(this);
        currentPlanet = null;
        EnterShip();
        pauseMenu.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            isPaused = !isPaused;
            if (isPaused)
                Time.timeScale = 0;
            else
                Time.timeScale = 1;
            pauseMenu.SetActive(isPaused);
        }
    }

    public void EnteredPlanet(PlanetaryBody planet)
    {
        Debug.Log("Entered");
        currentPlanet = planet;
        onSpace.SetCurrentPlanet(currentPlanet);
    }

    public void ExitedPlanet(PlanetaryBody planet)
    {
        if(planet == currentPlanet)
        {
            currentPlanet = null;
            onSpace.DesactivatePlanet();
            Debug.Log("Exited");
        }
    }

    public void ExitShip()
    {
        onSpace.shipCam.SetActive(false);
        onSpace.enabled = false;
        onSpace.ui.gameObject.SetActive(false);
        onPlanet.gameObject.SetActive(true);
        onPlanet.ui.gameObject.SetActive(true);
        onPlanet.transform.position = onSpace.playerSpawnPoint.position;
        onPlanet.transform.rotation = onSpace.playerSpawnPoint.rotation;
        onPlanet.currentPlanet = currentPlanet;
        transform.parent = onPlanet.gameObject.transform;
        transform.localPosition = Vector3.zero;

    }

    public void EnterShip()
    {
        onSpace.enabled = true;
        onSpace.shipCam.SetActive(true);
        onSpace.ui.gameObject.SetActive(true);
        onPlanet.gameObject.SetActive(false);
        onPlanet.ui.gameObject.SetActive(false);
        transform.parent = onSpace.gameObject.transform;
        transform.localPosition = Vector3.zero;
    }
}
