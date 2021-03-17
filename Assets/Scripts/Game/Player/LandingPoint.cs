using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LandingPoint : MonoBehaviour
{
    public ShipController shipController;
    int id;

    private void Start()
    {
        id = shipController.AddLandingPoint(this);
        gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Ship"))
        {
            shipController.PointTriggered(id);
        }
    }
}
