using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

// Player Movement On Space
[RequireComponent(typeof(Rigidbody))]
public class ShipController : MonoBehaviour
{
    // Flying values
    public float2 forwardSpeed, strafeSpeed, hoverSpeed; // x = max, y = min
    public float2 forwardAcceleration, strafeAcceleration, hoverAcceleration;
    public float camSensitivity;
    public float movementRadius;
    public GameObject shipCam;

    // Landing values
    public float landingDistance;
    public float decelerationDistance;
    public float landingSpeed;

    // Launch Values
    public float launchDistance;
    public float accelerationDistance;

    public LayerMask groundMask;
    public Transform playerSpawnPoint;
    public SpaceShipUI ui;

    public ShipState state { get; private set; }

    PlanetaryBody currentPlanet;
    PlayerManager playerManager;
    Rigidbody rb;

    float activeForwardSpeed, activeStrafeSpeed, activeHoverSpeed;
    float planetEffect;
    int landingPoinCount;
    float lastDistance;

    Vector2 mouseOffset, mouseRelative;
    Vector3 movement, shipToPlanet;
    LandingData landingData;

    List<LandingPoint> landingPointList = new List<LandingPoint>();

    public void CustomStart(PlayerManager manager)
    {
        playerManager = manager;
        ResetFocus();
        state = ShipState.Fly;
        rb = gameObject.GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.useGravity = false;
        planetEffect = 0;
        landingData = new LandingData();
    }

    private void Update()
    {
        if (Time.timeScale == 0)
            return;

        if (state == ShipState.Fly)
            FlyState();
        else if(state == ShipState.Landing)
            LandingState();
        else if(state == ShipState.Launching)
        {
            LaunchingState();
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.E))
                SetLaunchState();
            else if (Input.GetKeyDown(KeyCode.Q))
                PlacePlayerInWorld();
        }
    }

    void ChangeUIState()
    {
        if(ui != null)
        {
            ui.ChangeState(state);
        }
    }

    void PlacePlayerInWorld()
    {
        playerManager.ExitShip();
    }

    void FlyState()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
            ResetFocus();
        Rotation();
        if (currentPlanet != null)
        {
            planetEffect = currentPlanet.Rotate(transform);
            if (CanLand())
                return;
        }
        Movement();
    }

    void LandingState()
    {
        if (landingPoinCount >= landingPointList.Count - 1)
        {
            state = ShipState.Park;
            rb.constraints = RigidbodyConstraints.FreezeAll;
            movement = Vector3.zero;
            foreach (LandingPoint l in landingPointList)
                l.gameObject.SetActive(false);
            ChangeUIState();
        }
        else
        {
            float parkingDistance = (transform.position - landingData.landingPoint).magnitude;
            if (parkingDistance < decelerationDistance)
            {
                float mod = parkingDistance / decelerationDistance;
                if (mod < .3f)
                    mod = .3f;
                landingData.speed = landingSpeed * mod;
            }
            movement = landingData.speed * Time.deltaTime * landingData.downVector;
            currentPlanet.Rotate(transform);
        }
    }

    void LaunchingState()
    {
        float objectiveDistance = (transform.position - landingData.landingPoint).magnitude;
        if (lastDistance < 0)
            lastDistance = objectiveDistance;
        if(objectiveDistance > lastDistance)
        {
            state = ShipState.Fly;
            movement = Vector3.zero;
            ResetFocus();
            ChangeUIState();
        }
        else
        {
            float mod = objectiveDistance - accelerationDistance;
            if(mod <= 0)
            {
                landingData.speed = landingSpeed;
                currentPlanet.Rotate(transform);
            }
            else
            {
                mod = 1 - (mod / (launchDistance - accelerationDistance));
                if (mod < .2f)
                    mod = .2f;
                landingData.speed = landingSpeed * mod;
            }
            movement = landingData.speed * Time.deltaTime * landingData.downVector;
        }
        lastDistance = objectiveDistance;
    }

    public int AddLandingPoint(LandingPoint point)
    {
        landingPointList.Add(point);
        return landingPointList.Count - 1;
    }

    public void PointTriggered(int id)
    {
        landingPointList[id].gameObject.SetActive(false);
        landingPoinCount++;
    }

    bool CanLand()
    {
        shipToPlanet = (currentPlanet.transform.position - transform.position).normalized;
        RaycastHit hit;
        if(Physics.Raycast(transform.position, shipToPlanet, out hit, landingDistance, groundMask))
        {
             ui.CanLand(true);
            if (Input.GetKeyDown(KeyCode.E))
            {
                SetLandingState(hit);
                return true;
            }
        }
        else
            ui.CanLand(false);
        return false;
    }

    void SetLandingState(RaycastHit hit)
    {
        state = ShipState.Landing;
        landingData.landingPoint = hit.point;
        landingData.speed = landingSpeed;
        landingData.downVector = shipToPlanet.normalized;
        rb.constraints = RigidbodyConstraints.None;
        landingPoinCount = 0;
        foreach (LandingPoint l in landingPointList)
            l.gameObject.SetActive(true);
        ChangeUIState();
    }

    void SetLaunchState()
    {
        state = ShipState.Launching;
        landingData.downVector *= -1;
        landingData.landingPoint = transform.position + (landingData.downVector * launchDistance);
        rb.constraints = RigidbodyConstraints.None;
        lastDistance = -1;
        ChangeUIState();
    }

    public void SetCurrentPlanet(PlanetaryBody planet)
    {
        currentPlanet = planet;
    }

    public void DesactivatePlanet()
    {
        currentPlanet = null;
        planetEffect = 0;
    }

    void Rotation()
    {
        mouseOffset.x += Input.GetAxis("MouseX");
        mouseOffset.y += Input.GetAxis("MouseY");

        float magnitude = Mathf.Clamp(mouseOffset.magnitude, 0, movementRadius) / movementRadius;
        mouseRelative = mouseOffset.normalized * magnitude;
        if (ui != null)
            ui.SetSmallCircle(mouseRelative);
        mouseRelative *= camSensitivity;
        transform.Rotate(-mouseRelative.y * Time.deltaTime, mouseRelative.x * Time.deltaTime, 0f, Space.Self);
    }

    void Movement()
    {
        activeForwardSpeed = Mathf.Lerp(activeForwardSpeed, Input.GetAxisRaw("Vertical") * Mathf.Lerp(forwardSpeed.y, forwardSpeed.x, planetEffect), Mathf.Lerp(forwardAcceleration.y, forwardAcceleration.x, planetEffect) * Time.deltaTime);
        activeStrafeSpeed = Mathf.Lerp(activeStrafeSpeed, Input.GetAxisRaw("Horizontal") * Mathf.Lerp(strafeSpeed.y, strafeSpeed.x, planetEffect), Mathf.Lerp(strafeAcceleration.y, strafeAcceleration.x, planetEffect) * Time.deltaTime);
        activeHoverSpeed = Mathf.Lerp(activeHoverSpeed, Input.GetAxisRaw("Hover") * Mathf.Lerp(hoverSpeed.y, hoverSpeed.x, planetEffect), Mathf.Lerp(hoverAcceleration.y, hoverAcceleration.x, planetEffect) * Time.deltaTime);
        movement = (transform.forward * activeForwardSpeed) + (transform.right * activeStrafeSpeed) + (transform.up * activeHoverSpeed);
    }

    void ResetFocus()
    {
        mouseOffset = Vector2.zero;
        mouseRelative = Vector2.zero;
    }

    private void FixedUpdate()
    {
        if (state == ShipState.Landing || state == ShipState.Fly || state == ShipState.Launching)
        {
            rb.MovePosition(rb.position + (movement * Time.deltaTime));
            rb.velocity = Vector3.zero;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(state == ShipState.Fly)
        {
            state = ShipState.Crash;
        }
    }
}

public struct LandingData
{
    public Vector3 downVector;
    public Vector3 landingPoint;
    public float speed;
}

public enum ShipState
{
    Fly,
    Crash,
    Park,
    Landing,
    Launching
}