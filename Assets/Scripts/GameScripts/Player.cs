using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Player : MonoBehaviour
{
    Rigidbody rb;

    public float jumpForce;
    public float moveSpeed;
    public float gravityValue = 10;
    public PlanetaryBody closestPlanet;
    public Transform groundCheck;
    public float groundDistance = .4f;
    public LayerMask groundMask;
    public float mouseSensitivity;
    public Transform cam;

    float xRotation = 0;
    float yRotation;

    Vector3 moveDir;
    bool jump;
    bool isGrounded;


    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.useGravity = false;
        moveDir = Vector3.zero;
        jump = false;
    }

    void Update()
    {
        closestPlanet.Attract(transform, rb);
        Rotate();
        CheckGrounded();
        moveDir.x = Input.GetAxisRaw("Horizontal");
        moveDir.z = Input.GetAxisRaw("Vertical");
        moveDir.Normalize();
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
            jump = true;
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
        if (jump && isGrounded)
        {
            jump = false;
            rb.AddForce(transform.up * jumpForce, ForceMode.Acceleration);
        }
        rb.MovePosition(rb.position + ((transform.TransformDirection(moveDir) * moveSpeed) * Time.deltaTime));
    }

}
