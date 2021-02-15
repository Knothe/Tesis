using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnPlanetUI : MonoBehaviour
{
    public GameObject shipIsNear;

    public void ShipProximity(bool b)
    {
        if (b != shipIsNear.activeInHierarchy)
            shipIsNear.SetActive(b);
    }
}
