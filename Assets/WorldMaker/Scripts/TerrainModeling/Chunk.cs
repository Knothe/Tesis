using System.Collections.Generic;
using UnityEngine;

/// <summary> Represents a node from the Level of Detail octree in game </summary>
[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter), typeof(MeshCollider))]
public class Chunk : MonoBehaviour
{
    /// <summary> In game representation of the terrain </summary>
    MeshFilter meshFilter;
    /// <summary> Only used in the biggest level of detail </summary>
    MeshCollider meshCollider;
    /// <summary> Node to represent in game </summary>
    public Node data { get; private set; }
    /// <summary> Indicates if the biggest level of detail can generate trees </summary>
    public bool isInTreeRange { get; set; }

    /// <summary> Initializes values for the chunk </summary>
    /// <param name="d">Node the chunk will store</param>
    /// <param name="m">Mesh material</param>
    public void Initialize(Node d, Material m)
    {
        data = d;
        meshFilter = gameObject.GetComponent<MeshFilter>();
        meshCollider = gameObject.GetComponent<MeshCollider>();
        gameObject.GetComponent<MeshRenderer>().material = m;
        gameObject.SetActive(true);
        isInTreeRange = false;
    }

    /// <summary>
    /// Desactivates the chunk
    /// </summary>
    public void Desactivate()
    {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Sets mesh values for biggest level of detail
    /// </summary>
    /// <param name="m">Terrain Mesh</param>
    public void SetMeshLast(Mesh m)
    {
        SetMeshPrivate(m);
        meshCollider.sharedMesh = m;
        meshCollider.enabled = true;
    }

    /// <summary>
    /// Sets mesh values for all but biggest level of detail
    /// </summary>
    /// <param name="m">Terrain Mesh</param>
    public void SetMesh(Mesh m)
    {
        SetMeshPrivate(m);
        meshCollider.enabled = false;
    }

    /// <summary>
    /// Sets values all chunks need
    /// </summary>
    /// <param name="m">Terrain Mesh</param>
    void SetMeshPrivate(Mesh m)
    {
        if (data.data.terrain.drawAsSphere)
            gameObject.transform.localPosition = data.data.chunkCenter;
        else
            gameObject.transform.localPosition = data.cubePosition;
        gameObject.transform.localRotation = Quaternion.identity;
        meshFilter.sharedMesh = m;
    }
}
