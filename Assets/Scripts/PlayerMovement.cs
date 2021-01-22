using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    public float forwardSpeed = 25f, strafeSpeed = 7.5f, hoverSpeed = 5f;
    private float activeForwardSpeed, activeStrafeSpeed, activeHoverSpeed;
    private float forwardAcceleration = 2.5f, strafeAcceleration = 2f, hoverAcceleration = 2f;
    public float sensitivity;
    public float movementRadius;
    public float lookRateSpeed = 90f;
    public SpaceShipUI ui;
    public PlanetaryBody closestPlanet;

    Vector2 mouseOffset, mouseRelative;

    Rigidbody rb;

    private void Start()
    {
        ResetFocus();

        rb = gameObject.GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.useGravity = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
            ResetFocus();
        Rotation();
        if (closestPlanet != null)
            closestPlanet.Rotate(transform);
        Movement();
    }

    void Rotation()
    {
        mouseOffset.x += Input.GetAxis("MouseX");
        mouseOffset.y += Input.GetAxis("MouseY");
        float magnitude = Mathf.Clamp(mouseOffset.magnitude, 0, movementRadius) / movementRadius;
        mouseRelative = mouseOffset.normalized * magnitude;
        ui.SetSmallCircle(mouseRelative);
        mouseRelative *= sensitivity;
        transform.Rotate(-mouseRelative.y * Time.deltaTime, mouseRelative.x * Time.deltaTime, 0f, Space.Self);
    }

    void Movement()
    {
        activeForwardSpeed = Mathf.Lerp(activeForwardSpeed, Input.GetAxisRaw("Vertical") * forwardSpeed, forwardAcceleration * Time.deltaTime);
        activeStrafeSpeed = Mathf.Lerp(activeStrafeSpeed, Input.GetAxisRaw("Horizontal") * strafeSpeed, strafeAcceleration * Time.deltaTime);
        activeHoverSpeed = Mathf.Lerp(activeHoverSpeed, Input.GetAxisRaw("Hover") * hoverSpeed, hoverAcceleration * Time.deltaTime);

        transform.position += transform.forward * activeForwardSpeed * Time.deltaTime;
        transform.position += transform.right * activeStrafeSpeed * Time.deltaTime;
        transform.position += transform.up * activeHoverSpeed * Time.deltaTime;
    }

    void ResetFocus()
    {
        mouseOffset = Vector2.zero;
        mouseRelative = Vector2.zero;
    }

}
