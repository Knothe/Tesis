using UnityEngine;

public class TreeBase : MonoBehaviour
{
    public int id;
    public bool rotateWithFace;

    public void Activate()
    {
        if (rotateWithFace)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, transform.up * -1, out hit, Mathf.Infinity))
                transform.up = hit.normal;
        }
    }
}
