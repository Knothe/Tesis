using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetaryBody : MonoBehaviour
{
    public float gravityValue = -10;
    public GameObject player;
    public float radius;

    float radiusSquare;

    Vector3 dif;
    PlayerMovement p;
    bool isInside;

    private void Start()
    {
        radiusSquare = radius * radius;
        p = player.GetComponent<PlayerMovement>();
        isInside = false;
    }

    private void Update()
    {
        dif = transform.position - player.transform.position;
        if (isInside)
        {
            if (dif.sqrMagnitude > radiusSquare)
            {
                Debug.Log("Salió");
                p.closestPlanet = null;
                isInside = false;
            }
        }
        else if(dif.sqrMagnitude < radiusSquare)
        {
            Debug.Log("Entró");
            p.closestPlanet = this;
            isInside = true;
        }
    }

    public void Rotate(Transform t)
    {
        Vector3 gravityUp = (t.position - transform.position).normalized;
        Vector3 bodyUp = t.up;
        Quaternion targetRotation = Quaternion.FromToRotation(bodyUp, gravityUp) * t.rotation;
        t.rotation = Quaternion.Slerp(t.rotation, targetRotation, .5f * Time.deltaTime);
    }

    public void Attract(Transform t, Rigidbody rb)
    {
        Vector3 gravityUp = (t.position - transform.position).normalized;
        Vector3 bodyUp = t.up;
        rb.AddForce(gravityUp * gravityValue);
        Quaternion targetRotation = Quaternion.FromToRotation(bodyUp, gravityUp) * t.rotation;
        t.rotation = Quaternion.Slerp(t.rotation, targetRotation, 50 * Time.deltaTime); 
    }
}
