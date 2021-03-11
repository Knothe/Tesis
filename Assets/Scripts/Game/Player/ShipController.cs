using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

// Player Movement On Space
public class ShipController : MonoBehaviour
{
    // Flying values
    public float2 forwardSpeed, strafeSpeed, hoverSpeed; // x = max, y = min
    public float2 forwardAcceleration, strafeAcceleration, hoverAcceleration;
    public float camSensitivity;
    public float movementRadius;
    public GameObject shipCam;

    public float2 forwardUpgrade, strafeUpgrade, hoverUpgrade;
    public float2 forwardAccUpgrade, strafeAccUpgrade, hoverAccUpgrade;

    // Landing values
    public float landingDistance;
    public float decelerationDistance;
    public float landingSpeed;

    // Launch Values
    public float launchDistance;
    public float accelerationDistance;

    // Life Values
    public int maxLife;
    // min -> x, max -> y
    public int2 treeDamageLimit;
    public int2 asteroidDamageLimit;
    public int2 crashDamageLimit;

    int treeDamage;
    int crashDamage;
    int asteroidDamage;

    public int currentLife { get; private set; }

    public LayerMask groundMask;
    public Transform playerSpawnPoint;
    public SpaceShipUI ui;

    public MeshRenderer shipRenderer;
    public Texture2D[] main;
    public Texture2D[] sec;
    public Texture2D[] engine;
    public Texture2D[] window;
    public Texture2D extra;

    public ShipState state { get; private set; }

    PlanetaryBody currentPlanet;
    PlayerManager playerManager;
    Rigidbody rb { get; set; }
    int4 textIndex;

    float activeForwardSpeed, activeStrafeSpeed, activeHoverSpeed;
    float planetEffect;
    int landingPoinCount;
    float lastDistance;

    Vector2 mouseOffset, mouseRelative;
    public Vector3 movement { get; private set; }
    Vector3 shipToPlanet;
    LandingData landingData;

    List<LandingPoint> landingPointList = new List<LandingPoint>();

    public void CustomStart(PlayerManager manager)
    {
        playerManager = manager;
        ResetFocus();
        state = ShipState.Fly;
        planetEffect = 0;
        landingData = new LandingData();
        textIndex = int4.zero;
        currentLife = maxLife;
        SetTexture();
        treeDamage = treeDamageLimit.y;
        asteroidDamage = asteroidDamageLimit.y;
        crashDamage = crashDamageLimit.y;
    }

    private void Update()
    {
        if (Time.timeScale == 0)
            return;

        if (state == ShipState.Fly)
            FlyState();
        else if (state == ShipState.Landing)
            LandingState();
        else if (state == ShipState.Launching)
            LaunchingState();
        else if(state == ShipState.Park)
        {
            if (Input.GetKeyDown(KeyCode.E))
                SetLaunchState();
            else if (Input.GetKeyDown(KeyCode.Q))
                PlacePlayerInWorld();
        }else if(state == ShipState.Crash)
            if (Input.GetKeyDown(KeyCode.Q))
                PlacePlayerInWorld();
        ui.SetLife(currentLife);
    }

    public void RepairCrash()
    {
        state = ShipState.Park;
        ChangeUIState();
        landingData.downVector = (currentPlanet.transform.position - transform.position).normalized;
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
        Destroy(GetComponent<Rigidbody>());
    }

    public void Reactivate()
    {
        gameObject.AddComponent<Rigidbody>();
        rb = gameObject.GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeAll;
        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.centerOfMass = Vector3.zero;
        shipCam.SetActive(true);
        ui.gameObject.SetActive(true);
    }

    public void StartRigidBody()
    {
        rb.constraints = RigidbodyConstraints.FreezeRotation;
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
        if (landingPoinCount >= 3)
        {
            state = ShipState.Park;
            SetLanded();
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
            activeForwardSpeed = 0;
            activeStrafeSpeed = 0;
            activeHoverSpeed = 0;
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

    void SetLanded()
    {
        transform.parent = currentPlanet.transform;
        rb.constraints = RigidbodyConstraints.FreezeAll;
        movement = Vector3.zero;
        foreach (LandingPoint l in landingPointList)
            l.gameObject.SetActive(false);
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
        transform.parent = null;
        state = ShipState.Launching;
        landingData.downVector *= -1;
        landingData.landingPoint = transform.position + (landingData.downVector * launchDistance);
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        lastDistance = -1;
        movement = Vector3.zero;
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
        if (collision.gameObject.CompareTag("Tree"))
        {
            collision.gameObject.SetActive(false);
            ModifyLife(-treeDamage);
        }
        else
        {
            if (state == ShipState.Fly)
            {
                if (collision.gameObject.CompareTag("Ground"))
                {
                    ModifyLife(-crashDamage);
                    state = ShipState.Crash;
                    playerManager.ShipCrashed();
                    SetLanded();
                    ChangeUIState();
                }

                else if (collision.gameObject.CompareTag("Asteroid"))
                {
                    ModifyLife(-asteroidDamage);
                    collision.gameObject.GetComponent<Asteroid>().Desactivate();
                }

            }
        }
    }

    public void ChangeTexture(int id)
    {
        if (textIndex[id] < 1)
        {
            textIndex[id]++;
            SetTexture();
        }
    }

    void SetTexture()
    {
        Texture2D temp = new Texture2D(3, 3);
        Color c;
        for(int i = 0; i < temp.width; i++)
        {
            for(int j = 0; j < temp.width; j++)
            {
                c = main[textIndex[0]].GetPixel(i, j) +
                    sec[textIndex[1]].GetPixel(i, j) +
                    engine[textIndex[2]].GetPixel(i, j) +
                    window[textIndex[3]].GetPixel(i, j) +
                    extra.GetPixel(i, j);
                temp.SetPixel(i, j, c);
            }
        }
        temp.Apply();
        shipRenderer.material.SetTexture("_MainTex", temp);
    }

    public void ModifyLife(int value)
    {
        currentLife += value;
        if (currentLife > maxLife)
            currentLife = maxLife;
        else if (currentLife <= 0)
            playerManager.PlayerDeath();
    }

    public void Upgrade(int n)
    {
        if (n == 0)
        {
            ui.UpgradeColor();
        } 
        else if (n == 1)
        {
            forwardSpeed = forwardUpgrade;
            strafeUpgrade = strafeSpeed;
            hoverUpgrade = hoverSpeed;
        }
        else if (n == 2)
        {
            crashDamage = crashDamageLimit.x;
        }
        else if (n == 3)
        {
            treeDamage = treeDamageLimit.x;
            asteroidDamage = asteroidDamageLimit.x;
        }
        else if (n == 4)
        {
            forwardAcceleration = forwardAccUpgrade;
            strafeAcceleration = strafeAccUpgrade;
            hoverAcceleration = hoverAccUpgrade;
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