using System;
using UnityEngine;

[Serializable]
public class TreeSets
{
    public float scale1;
    public Vector3 offset1;

    public float scale2;
    public Vector3 offset2;

    public int maxTrees;
    public int missedTreesMax;

    public BiomeTreesWrapper[] biomeTrees = new BiomeTreesWrapper[9];
}

// 0 Selva Tropical
// 1 Bosque Tropical
// 2 Sabana
// 3 Selva Templada
// 4 Bosque Templado
// 5 Herbazal
// 6 Taiga
// 7 Tundra
// 8 Desierto
// 9 Agua
