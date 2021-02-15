using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Player Movement On Planet
[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    public float jumpForce;
    public float moveSpeed;
    public float mouseSensitivity;
    // gravity Value
    public Transform groundCheck;
    public float groundDistance = .4f;
    public LayerMask groundMask;    // Maybe change to only ignore player
    public Transform cam;
    public OnPlanetUI ui;

    public PlanetaryBody currentPlanet { get; set; }
    PlayerManager playerManager;
    Rigidbody rb;

    float xRotation = 0;
    float yRotation;

    Vector3 moveDir;
    bool jump;
    bool isGrounded;
    bool nearShip;


    public void CustomStart(PlayerManager manager)
    {
        playerManager = manager;
        rb = gameObject.GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.useGravity = false;
        jump = false;
        moveDir = Vector3.zero;
        nearShip = false;
    }

    private void Update()
    {
        currentPlanet.Attract(transform, rb);
        Rotate();
        CheckGrounded();
        moveDir.x = Input.GetAxisRaw("Horizontal");
        moveDir.z = Input.GetAxisRaw("Vertical");
        moveDir.Normalize();
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
            jump = true;
        if (nearShip)
            if (Input.GetKeyDown(KeyCode.Q))
                playerManager.EnterShip();
    }

    void Rotate()
    {
        xRotation -= Input.GetAxis("MouseY") * mouseSensitivity;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        cam.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        yRotation = Input.GetAxis("MouseX") * mouseSensitivity;
        transform.Rotate(Vector3.up * yRotation);
    }

    void CheckGrounded()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
    }

    private void FixedUpdate()
    {
        if(jump && isGrounded)
        {
            jump = false;
            rb.AddForce(transform.up * jumpForce, ForceMode.Acceleration);
        }
        rb.MovePosition(rb.position + ((transform.TransformDirection(moveDir) * moveSpeed) * Time.deltaTime));
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ship"))
        {
            nearShip = true;
            ui.ShipProximity(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Ship"))
        {
            nearShip = false;
            ui.ShipProximity(false);
        }
    }

    private void OnDrawGizmos()
    {
        if (isGrounded)
            Gizmos.color = Color.green;
        else
            Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
    }
}
