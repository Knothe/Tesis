using UnityEngine;

/// <summary> Base class for any tree generated </summary>
public class TreeBase : MonoBehaviour
{
    public int id;
    public bool rotateWithFace;

    /// <summary> When a tree is activated this runs </summary>
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
