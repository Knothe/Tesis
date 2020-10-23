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

    Node data;

    public void Initialize(Node d, Material m)
    {
        data = d;
        meshFilter = gameObject.GetComponent<MeshFilter>();
        meshCollider = gameObject.GetComponent<MeshCollider>();
        gameObject.GetComponent<MeshRenderer>().material = m;
    }

    public bool UpdateChunkData()
    {
        return data.GenerateVoxelData();
    }

    public void SetMesh()
    {
        //gameObject.transform.localPosition = data.cubePosition;
        gameObject.transform.localPosition = Vector3.zero;
        Mesh m = data.GenerateMesh();
        meshFilter.sharedMesh = m;
        //meshCollider.sharedMesh = m;
    }

}
