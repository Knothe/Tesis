using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceShipUI : MonoBehaviour
{
    [SerializeField] GameObject flying;
    [SerializeField] GameObject crashed;
    [SerializeField] GameObject parked;
    [SerializeField] GameObject landing;
    [SerializeField] GameObject launching;

    [SerializeField] Transform smallCircle;
    [SerializeField] GameObject canLandText;
    [SerializeField] float radius;

    Vector3 temp = Vector3.zero;

    private void Start()
    {
        DesactivateAll();
        flying.SetActive(true);
        canLandText.SetActive(false);
    }

    public void CanLand(bool b)
    {
        if (b != canLandText.activeInHierarchy)
            canLandText.SetActive(b);
    }

    public void SetSmallCircle(Vector2 offset)
    {
        temp.x = offset.x * radius;
        temp.y = offset.y * radius;
        smallCircle.transform.localPosition = temp;
    }

    void DesactivateAll()
    {
        flying.SetActive(false);
        crashed.SetActive(false);
        parked.SetActive(false);
        landing.SetActive(false);
        launching.SetActive(false);
    }

    public void ChangeState(ShipState state)
    {
        DesactivateAll();
        if (state == ShipState.Fly)
            flying.SetActive(true);
        else if(state == ShipState.Crash)
            crashed.SetActive(true);
        else if (state == ShipState.Park)
            parked.SetActive(true);
        else if (state == ShipState.Landing)
            landing.SetActive(true);
        else if (state == ShipState.Launching)
            launching.SetActive(true);
    }
}
