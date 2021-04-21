using Unity.Mathematics;
using UnityEngine;

/// <summary>
/// Player movement on space
/// </summary>
public class ShipController : MonoBehaviour
{
    /// <summary> Max and minimum speeds on the 3 main axes </summary>
    public float2 forwardSpeed, strafeSpeed, hoverSpeed;                        // x = max, y = min
    /// <summary> Max and min acceleration on the 3 main axes </summary>
    public float2 forwardAcceleration, strafeAcceleration, hoverAcceleration;   // x = max, y = min
    /// <summary> Rotation of the spaceship </summary>
    public float camSensitivity;
    /// <summary> Mouse Rotation Radius </summary>
    public float movementRadius;

    /// <summary> Landing distance </summary>
    public float landingDistance;

    /// <summary> Ground mask for detecting terrain </summary>
    public LayerMask groundMask;

    /// <summary> Player is inside this planet atmosphere </summary>
    PlanetaryBody currentPlanet;
    PlayerManager playerManager;
    public Rigidbody rb;

    float activeForwardSpeed, activeStrafeSpeed, activeHoverSpeed;
    float planetEffect;

    Vector2 mouseOffset, mouseRelative;
    public Vector3 movement { get; private set; }
    Vector3 shipToPlanet;

    /// <summary>
    /// Starts as the Player Manager indicates
    /// </summary>
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

    /// <summary>
    /// Checks if the player can land
    /// </summary>
    /// <returns>True if player will land</returns>
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

    /// <summary>
    /// Called when a player enters an atmosphere
    /// </summary>
    /// <param name="planet">Current Planet</param>
    public void SetCurrentPlanet(PlanetaryBody planet)
    {
        currentPlanet = planet;
    }

    /// <summary>
    /// Called when a player exits an atmosphere
    /// </summary>
    public void DesactivatePlanet()
    {
        currentPlanet = null;
        planetEffect = 0;
    }

    /// <summary>
    /// Rotation of the player based on the mouse offset
    /// </summary>
    void Rotation()
    {
        mouseOffset.x += Input.GetAxis("MouseX");
        mouseOffset.y += Input.GetAxis("MouseY");

        float magnitude = Mathf.Clamp(mouseOffset.magnitude, 0, movementRadius) / movementRadius;
        mouseRelative = mouseOffset.normalized * magnitude;
        mouseRelative *= camSensitivity;
        transform.Rotate(-mouseRelative.y * Time.deltaTime, mouseRelative.x * Time.deltaTime, 0f, Space.Self);
    }

    /// <summary>
    /// Movement of the spaceship
    /// </summary>
    void Movement()
    {
        activeForwardSpeed = Mathf.Lerp(activeForwardSpeed, Input.GetAxisRaw("Vertical") * Mathf.Lerp(forwardSpeed.y, forwardSpeed.x, planetEffect), Mathf.Lerp(forwardAcceleration.y, forwardAcceleration.x, planetEffect) * Time.deltaTime);
        activeStrafeSpeed = Mathf.Lerp(activeStrafeSpeed, Input.GetAxisRaw("Horizontal") * Mathf.Lerp(strafeSpeed.y, strafeSpeed.x, planetEffect), Mathf.Lerp(strafeAcceleration.y, strafeAcceleration.x, planetEffect) * Time.deltaTime);
        activeHoverSpeed = Mathf.Lerp(activeHoverSpeed, Input.GetAxisRaw("Hover") * Mathf.Lerp(hoverSpeed.y, hoverSpeed.x, planetEffect), Mathf.Lerp(hoverAcceleration.y, hoverAcceleration.x, planetEffect) * Time.deltaTime);
        movement = (transform.forward * activeForwardSpeed) + (transform.right * activeStrafeSpeed) + (transform.up * activeHoverSpeed);
    }

    /// <summary>
    /// Resets view center to current position of mouse
    /// </summary>
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
