using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnPlanetUI : MonoBehaviour
{
    public GameObject shipIsNear;
    public Transform shipLocator;

    public void ShipProximity(bool b)
    {
        if (b != shipIsNear.activeInHierarchy)
            shipIsNear.SetActive(b);
    }

    public void SetShipLocator(float angle)
    {
        shipLocator.eulerAngles = new Vector3(0, 0, angle);
    }
}
