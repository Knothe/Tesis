using UnityEngine;

[RequireComponent(typeof(PlanetsManager))]
public class GameManager : MonoBehaviour
{
    public PlanetsManager planetManager;
    public PlayerManager playerManager;

    /// <summary>
    /// Movement distance possible before moving every object in world
    /// </summary>
    public float maxDistance;

    /// <summary>
    /// maxDistance to the power of 2, makes distance calculations faster
    /// </summary>
    float newMaxDistance;

    private void OnValidate()
    {
        if (planetManager == null)
            planetManager = gameObject.GetComponent<PlanetsManager>();
        if (playerManager == null)
            playerManager = FindObjectOfType<PlayerManager>();
    }

    void Start()
    {
        planetManager = GetComponent<PlanetsManager>();
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = false;
        planetManager.SetGameManager(this);
        newMaxDistance = maxDistance * maxDistance;
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
