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
    public PauseMenuManager pauseManager;

    public int minRandomMission;
    public int maxRandomMission;
    public int lifeRecovery;

    public int maxItems;

    public AudioSource bgMusicSource;

    PlanetaryBody currentPlanet;
    GameManager gameManager;

    public int[] obtenibles { get; private set; }
    bool isPaused = false;

    public Mission recoverHealth { get; private set; }
    public Mission recoverCrash { get; private set; }

    int[] materiales = new int[] { 1, 2, 5, 10 };

    void Start()
    {
        obtenibles = new int[20];
        onPlanet.CustomStart(this);
        onSpace.CustomStart(this);
        currentPlanet = null;
        EnterShip();
        onSpace.StartRigidBody();
        pauseManager.SetAudioValues();
        pauseManager.gameObject.SetActive(false);
        SetHealthMission();
        bgMusicSource.ignoreListenerPause = true;
        bgMusicSource.Play();
    }

    public void SetGameManager(GameManager g)
    {
        gameManager = g;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            isPaused = !isPaused;
            Cursor.visible = isPaused;
            pauseManager.gameObject.SetActive(isPaused);
            if (isPaused)
            {
                Time.timeScale = 0;
                pauseManager.StartMenu();
            }
            else
                Time.timeScale = 1;
            AudioListener.pause = isPaused;
        }
    }

    public void PlayerLanded()
    {
        bgMusicSource.Stop();
        bgMusicSource.clip = gameManager.GetPlanetAudio(currentPlanet.id);
    }

    public void SetSpaceMusic()
    {
        bgMusicSource.clip = gameManager.spaceMusic;
        bgMusicSource.Play();
    }

    public void ShipCrashed()
    {
        recoverCrash = new Mission("Fix Crash", 3);
        SetMission(recoverCrash);
    }

    void SetHealthMission()
    {
        recoverHealth = new Mission("Recover Health", 3);
        SetMission(recoverHealth);
    }

    void SetMission(Mission m)
    {
        int rand = Random.Range(0, 4);
        int index = 0;
        for (int i = 0; i < 4; i++)
        {
            if (rand != i)
            {
                m.item[index] = materiales[i];
                m.quantity[index] = Random.Range(minRandomMission, maxRandomMission);
                index++;
            }
        }
    }

    public void EnteredPlanet(PlanetaryBody planet)
    {
        currentPlanet = planet;
        onSpace.SetCurrentPlanet(currentPlanet);
        Debug.Log("Entered");
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

    public void CollectedItem(int id)
    {
        obtenibles[id]++;
        if (obtenibles[id] > maxItems)
            obtenibles[id] = maxItems;
    }

    public float CalculatePlayerShipAngle()
    {
        Plane plane = new Plane(onPlanet.transform.up, onPlanet.transform.position);
        Vector3 closestPoint = plane.ClosestPointOnPlane(onSpace.transform.position);

        Debug.DrawLine(onSpace.transform.position, closestPoint, Color.red);
        Debug.DrawLine(onPlanet.transform.position, closestPoint, Color.cyan);

        return -1 * Vector3.SignedAngle(onPlanet.transform.forward, closestPoint - onPlanet.transform.position, onPlanet.transform.up);
    }

    public void ExitShip()
    {
        onSpace.Desactivate();
        onPlanet.gameObject.SetActive(true);
        onPlanet.ui.gameObject.SetActive(true);
        onPlanet.transform.position = onSpace.playerSpawnPoint.position;
        onPlanet.transform.rotation = onSpace.playerSpawnPoint.rotation;
        onPlanet.currentPlanet = currentPlanet;
        transform.parent = onPlanet.gameObject.transform;
        transform.localPosition = Vector3.zero;
        bgMusicSource.Play();
    }

    public void EnterShip()
    {
        bgMusicSource.Stop();
        onSpace.enabled = true;
        onPlanet.gameObject.SetActive(false);
        onPlanet.ui.gameObject.SetActive(false);
        onSpace.Reactivate();
        transform.parent = onSpace.gameObject.transform;
        transform.localPosition = Vector3.zero;
    }

    public void ClearMission(int id, bool cleared)
    {
        if (cleared)
            gameManager.FinishGame(true);

        if(id == 0)                     // Cambiar color UI nave
        {
            onSpace.ChangeTexture(3);
            onSpace.Upgrade(0);
        }
        else if(id == 1)                // Mejor aceleración
        {
            onSpace.Upgrade(4);
        }
        else if (id == 3)               // Mayor velocidad
        {
            onSpace.ChangeTexture(2);
            onSpace.Upgrade(1);
        }
        else if (id == 5)               // Menor daño contra choques
        {
            onSpace.ChangeTexture(0);
            onSpace.Upgrade(2);
        }
        else if (id == 6)               // Menor daño contra asteroides y árboles
        {
            onSpace.ChangeTexture(1);
            onSpace.Upgrade(3);
        }
        else if (id == 7)
        {
            onSpace.ModifyLife(lifeRecovery);
            for (int i = 0; i < recoverHealth.item.Length; i++)
                obtenibles[recoverHealth.item[i]] -= recoverHealth.quantity[i];
            SetMission(recoverHealth);
        }
        else if (id == 8)
        {
            onSpace.RepairCrash();
            for (int i = 0; i < recoverCrash.item.Length; i++)
                obtenibles[recoverCrash.item[i]] -= recoverCrash.quantity[i];
            recoverCrash = null;
        }

        pauseManager.StartMenu();
    }

    public void PlayerDeath()
    {
        gameManager.FinishGame(false);
    }
}
