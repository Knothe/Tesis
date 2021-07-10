using UnityEngine;

/// <summary>
/// Player movement on planet
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    public float jumpForce;
    public float moveSpeed;
    public float mouseSensitivity;
    public Transform groundCheck;
    public float groundDistance = .4f;
    public LayerMask groundMask;
    public Transform cam;
    /// <summary>
    /// Ships spawn position when leaving the planet
    /// </summary>
    public Transform shipSpawn;

    /// <summary> Player is inside this planet atmosphere </summary>
    public PlanetaryBody currentPlanet { get; set; }
    PlayerManager playerManager;
    Rigidbody rb;

    float xRotation = 0;
    float yRotation;

    Vector3 moveDir;
    bool jump;
    bool isGrounded;


    /// <summary>
    /// Starts as the Player Manager indicates
    /// </summary>
    public void CustomStart(PlayerManager manager)
    {
        playerManager = manager;
        rb = gameObject.GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.useGravity = false;
        jump = false;
        moveDir = Vector3.zero;
    }

    private void Update()
    {
        if (Time.timeScale == 0)
            return;
        currentPlanet.Attract(transform, rb);
        Rotate();
        CheckGrounded();
        moveDir.x = GetHorizontal();
        moveDir.z = GetVertical();
        moveDir.Normalize();
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
            jump = true;
        if (Input.GetKeyDown(KeyCode.E))
            playerManager.EnterShip();
    }

    float GetVertical()
    {
        float v = 0;
        if (Input.GetKey(KeyCode.W))
            v += 1;
        if (Input.GetKey(KeyCode.S))
            v -= 1;
        return v;
    }

    float GetHorizontal()
    {
        float v = 0;
        if (Input.GetKey(KeyCode.D))
            v += 1;
        if (Input.GetKey(KeyCode.A))
            v -= 1;
        return v;
    }

    /// <summary>
    /// Rotates player view
    /// </summary>
    void Rotate()
    {
        xRotation -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        cam.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        yRotation = Input.GetAxis("Mouse X") * mouseSensitivity;
        transform.Rotate(Vector3.up * yRotation);
    }

    /// <summary>
    /// Checks if player is grounded
    /// </summary>
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

    private void OnDrawGizmos()
    {
        if (isGrounded)
            Gizmos.color = Color.green;
        else
            Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
    }
}
