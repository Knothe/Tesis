using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public PlanetsManager planetManager;
    public PlayerManager playerManager;

    public float maxDistance;

    float newMaxDistance;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = false;
        playerManager.SetGameManager(this);
        planetManager.SetGameManager(this);
        newMaxDistance = maxDistance * maxDistance;
        GetComponent<AudioSource>().ignoreListenerPause = true;
    }

    void Update()
    {
        if(playerManager.transform.position.sqrMagnitude > newMaxDistance)
        {
            Vector3 move = playerManager.transform.parent.position;
            planetManager.UpdatePlanets(move);
            playerManager.transform.parent.position = Vector3.zero;
        }
    }
}
