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
        if(data.data.terrain.drawAsSphere)
            gameObject.transform.localPosition = Vector3.zero;
        else
            gameObject.transform.localPosition = data.cubePosition;
        Mesh m = data.GenerateMesh();
        meshFilter.sharedMesh = m;
        if (data.data.terrain.removeLevelChange)
        {
            if (data.IsDivision())
                gameObject.SetActive(false);
        }
        //meshCollider.sharedMesh = m;
    }

    //private void OnDrawGizmos()
    //{
    //    Gizmos.color = Color.red;
    //    if(data.data.terrain.drawAsSphere)
    //        Gizmos.DrawSphere(data.sphereCenter, .01f);
    //    else
    //        Gizmos.DrawSphere(data.center, .01f);
    //}
}
