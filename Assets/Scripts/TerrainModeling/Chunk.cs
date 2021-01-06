using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter), typeof(MeshCollider))]
public class Chunk : MonoBehaviour
{
    public float3 spherePosition;

    MeshFilter meshFilter;
    MeshCollider meshCollider;
    public int chunkListIndex { get; set; }
    public Node data { get; private set; }

    public void Initialize(Node d, Material m)
    {
        data = d;
        meshFilter = gameObject.GetComponent<MeshFilter>();
        meshCollider = gameObject.GetComponent<MeshCollider>();
        gameObject.GetComponent<MeshRenderer>().material = m;
        gameObject.SetActive(true);
        //gameObject.GetComponent<MeshRenderer>().material.color = Color.white;
        //chunkListIndex = index;
    }

    public bool UpdateChunkData()
    {
        return data.GenerateVoxelData();
    }

    public void SetMesh(Mesh m)
    {
        if (data.data.terrain.drawAsSphere)
            //gameObject.transform.localPosition = data.cubePosition;
            gameObject.transform.localPosition = Vector3.zero;
        else
            gameObject.transform.localPosition = data.cubePosition;
        gameObject.transform.localRotation = Quaternion.identity;
        spherePosition = data.faceLocation;
        meshFilter.sharedMesh = m;
    }

    public void IsLimit()
    {
        //gameObject.GetComponent<MeshRenderer>().material.color = Color.red;
    }

}
