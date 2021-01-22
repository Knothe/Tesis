using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceShipUI : MonoBehaviour
{
    public Transform smallCircle;
    public float radius;

    Vector3 temp = Vector3.zero;

    public void SetSmallCircle(Vector2 offset)
    {
        temp.x = offset.x * radius;
        temp.y = offset.y * radius;
        smallCircle.transform.localPosition = temp;
    }
}
