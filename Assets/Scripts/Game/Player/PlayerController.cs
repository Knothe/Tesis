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
    public float shootDistance;
    public float damage = 5;
    // gravity Value
    public Transform groundCheck;
    public float groundDistance = .4f;
    public LayerMask groundMask;    // Maybe change to only ignore player
    public Transform cam;
    public OnPlanetUI ui;
    public Transform weapon;
    public LayerMask treeMask;
    public LineRenderer shootRenderer;
    public ParticleSystem particle;
    public MeshRenderer gunRenderer;
    public Color missColor;
    public Color targetColor;
    public AudioSource destroyTreeSound;

    public AudioClip treeDamage;
    public AudioClip destroyedTree;

    public PlanetaryBody currentPlanet { get; set; }
    PlayerManager playerManager;
    Rigidbody rb;
    bool isPlaying;

    float xRotation = 0;
    float yRotation;

    Vector3 moveDir;
    bool jump;
    bool isGrounded;
    bool nearShip;
    Texture2D gunTexture;
    bool past;


    public void CustomStart(PlayerManager manager)
    {
        playerManager = manager;
        rb = gameObject.GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.useGravity = false;
        jump = false;
        moveDir = Vector3.zero;
        nearShip = false;
        particle.gameObject.SetActive(false);
        gunTexture = (Texture2D)gunRenderer.material.GetTexture("_MainTex");
        past = true;
        UpdatePistolColor(false);
    }

    private void Update()
    {
        if (Time.timeScale == 0)
            return;

        currentPlanet.Attract(transform, rb);
        Rotate();
        CheckGrounded();
        moveDir.x = Input.GetAxisRaw("Horizontal");
        moveDir.z = Input.GetAxisRaw("Vertical");
        moveDir.Normalize();
        Shoot();
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
            jump = true;
        if (nearShip)
            if (Input.GetKeyDown(KeyCode.Q))
                playerManager.EnterShip();

        ui.SetShipLocator(playerManager.CalculatePlayerShipAngle());
    }

    void Shoot()
    {
        RaycastHit hit;
        shootRenderer.SetPosition(0, weapon.transform.position);
        if (Physics.Raycast(weapon.transform.position, weapon.transform.forward, out hit, shootDistance, treeMask))
        {
            UpdatePistolColor(true);
            shootRenderer.SetPosition(1, hit.point);

            if (Input.GetKey(KeyCode.E))
            {
                if (!destroyTreeSound.isPlaying)
                {
                    destroyTreeSound.clip = treeDamage;
                    destroyTreeSound.Play();
                }

                if (hit.collider.transform.gameObject.GetComponent<Tree>().DealDamage(damage))
                {
                    PauseMusic();
                    destroyTreeSound.PlayOneShot(destroyedTree);
                }

                if (!particle.gameObject.activeInHierarchy)
                    particle.gameObject.SetActive(true);
                particle.gameObject.transform.forward = hit.normal;
                particle.gameObject.transform.position = hit.point;
            }
            else if (particle.gameObject.activeInHierarchy)
                particle.gameObject.SetActive(false);
        }
        else
        {
            if (destroyTreeSound.isPlaying)
                PauseMusic();
            UpdatePistolColor(false);
            shootRenderer.SetPosition(1, weapon.transform.position + weapon.transform.forward * shootDistance);
            if (particle.gameObject.activeInHierarchy)
                particle.gameObject.SetActive(false);
        }
    }

    void PauseMusic()
    {
        destroyTreeSound.Stop();
        destroyTreeSound.clip = null;
    }

    void UpdatePistolColor(bool c)
    {
        if(c != past)
        {
            if (c)
                gunTexture.SetPixel(0, 2, targetColor);
            else
                gunTexture.SetPixel(0, 2, missColor);
            gunTexture.Apply();
            gunRenderer.material.SetTexture("_MainTex", gunTexture);
            past = c;
        }
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
        else if (other.CompareTag("Item"))
        {
            playerManager.CollectedItem(other.gameObject.GetComponent<Drop>().id);
            Destroy(other.gameObject);
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
