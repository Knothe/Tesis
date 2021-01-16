using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/Biome Trees", order = 2)]
public class BiomeTrees : ScriptableObject
{
    // Noise Data
    public float scale1;
    public Vector2 offset1;
    public float scale2;
    public Vector2 offset2;

    // Max Size of 4
    public GameObject[] trees = new GameObject[4];
    public float[] radius = new float[4];
}
