using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[Serializable]
public class TreeSets
{
    // Noise Data
    public float scale1;        // .01
    public Vector3 offset1;
    
    public float scale2;        // .01
    public Vector3 offset2;

    public int maxTrees;
    public int missedTreesMax;

    // Siempre 9
    public BiomeTrees[] biomeTrees = new BiomeTrees[9];
}

// Selva Tropical
// Bosque Tropical
// Sabana
// Selva Templada
// Bosque Templado
// Herbazal
// Taiga
// Tundra
// Desierto
