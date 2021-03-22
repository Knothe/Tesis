using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter), typeof(MeshCollider))]
public class Chunk : MonoBehaviour
{
    MeshFilter meshFilter;
    MeshCollider meshCollider;
    public int chunkListIndex { get; set; }
    public Node data { get; private set; }
    public bool isInTreeRange { get; set; }

    public void Initialize(Node d, Material m)
    {
        data = d;
        meshFilter = gameObject.GetComponent<MeshFilter>();
        meshCollider = gameObject.GetComponent<MeshCollider>();
        gameObject.GetComponent<MeshRenderer>().material = m;
        gameObject.SetActive(true);
        isInTreeRange = false;
        //gameObject.GetComponent<MeshRenderer>().material.color = Color.white;
        //chunkListIndex = index;
    }

    public void Desactivate()
    {
        gameObject.SetActive(false);
    }

    public bool UpdateChunkData()
    {
        return data.GenerateVoxelData();
    }

    public void SetMesh(List<TreeData> treeDat, Mesh m)
    {
        SetMeshPrivate(m);
        IsLast(m);
    }

    public void SetMesh(Mesh m)
    {
        SetMeshPrivate(m);
        meshCollider.enabled = false;
    }

    void SetMeshPrivate(Mesh m)
    {
        if (data.data.terrain.drawAsSphere)
            gameObject.transform.localPosition = data.data.chunkCenter;
        else
            gameObject.transform.localPosition = data.cubePosition;
        gameObject.transform.localRotation = Quaternion.identity;
        meshFilter.sharedMesh = m;
    }

    void IsLast(Mesh m)
    {
        meshCollider.sharedMesh = m;
        meshCollider.enabled = true;
    }

    public void IsLimit()
    {
        //gameObject.GetComponent<MeshRenderer>().material.color = Color.red;
    }

}
