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

    public float2 forwardUpgrade, strafeUpgrade, hoverUpgrade;
    public float2 forwardAccUpgrade, strafeAccUpgrade, hoverAccUpgrade;

    // Landing values
    public float landingDistance;

    public float currentLife { get; private set; }

    public LayerMask groundMask;


    PlanetaryBody currentPlanet;
    PlayerManager playerManager;
    public Rigidbody rb;

    float activeForwardSpeed, activeStrafeSpeed, activeHoverSpeed;
    float planetEffect;

    Vector2 mouseOffset, mouseRelative;
    public Vector3 movement { get; private set; }
    Vector3 shipToPlanet;

    public void CustomStart(PlayerManager manager)
    {
        playerManager = manager; 
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.centerOfMass = Vector3.zero;
        ResetFocus();
        planetEffect = 0;
    }

    private void Update()
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

    bool CanLand()
    {
        shipToPlanet = (currentPlanet.transform.position - transform.position).normalized;
        RaycastHit hit;
        if(Physics.Raycast(transform.position, shipToPlanet, out hit, landingDistance, groundMask))
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                playerManager.ExitShip(shipToPlanet * -1, hit.point);
                return true;
            }
        }
        return false;
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
        rb.MovePosition(rb.position + (movement * Time.deltaTime));
        rb.velocity = Vector3.zero;
    }
}
