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

    bool isLast;

    public void Initialize(Node d, Material m)
    {
        data = d;
        meshFilter = gameObject.GetComponent<MeshFilter>();
        meshCollider = gameObject.GetComponent<MeshCollider>();
        gameObject.GetComponent<MeshRenderer>().material = m;
        gameObject.SetActive(true);
        isLast = d.level == d.data.terrain.levelsOfDetail - 1;
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

    public void SetMesh(Mesh m)
    {
        if (data.data.terrain.drawAsSphere)
            //gameObject.transform.localPosition = data.cubePosition;
            gameObject.transform.localPosition = data.data.chunkCenter;
        else
            gameObject.transform.localPosition = data.cubePosition;
        gameObject.transform.localRotation = Quaternion.identity;
        spherePosition = data.faceLocation;
        meshFilter.sharedMesh = m;
        if (isLast)
        {
            meshCollider.sharedMesh = m;
            meshCollider.enabled = true;
        }
        else
            meshCollider.enabled = false;
    }

    public void IsLimit()
    {
        //gameObject.GetComponent<MeshRenderer>().material.color = Color.red;
    }

}
