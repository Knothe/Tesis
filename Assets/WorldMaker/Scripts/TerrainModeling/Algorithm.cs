using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

/// <summary>
/// Base Class for Marching Cubes and Dual Contouring
/// Class represents one chunk
/// </summary>
public class Algorithm
{
    /// <summary> List of generated vertex </summary>
    protected List<Vector3> vertexList;
    /// <summary> List of colors, index of color and vertex is the same </summary>
    protected List<Color> colors;
    /// <summary> List of biomes corresponding vertexList </summary>
    protected List<int> biome;
    /// <summary> Reference to planet TerrainInfo </summary>
    public TerrainInfo terrain { protected set; get; }
    /// <summary> Face of the cube the chunk is in, check values in TerrainManagerData.axisIndex </summary>
    public int axisID { get; protected set; }
    /// <summary> Level of Detail of the chunk </summary>
    protected int level;
    /// <summary>
    /// False if it's the voxel Data hasn't been generated
    /// This doesn't indicate the generation of the mesh
    /// </summary>
    public bool voxelDataGenerated { get; protected set; }

    public Vector3 chunkCenter { get; protected set; }

    public Algorithm(TerrainInfo t, int a, int l)
    {
        axisID = a;
        terrain = t;
        level = l;
        voxelDataGenerated = false;
        colors = new List<Color>();
        biome = new List<int>();
    }

    /// <summary>
    /// Generates voxel data of the chunk, used for mesh generation later
    /// </summary>
    /// <param name="start">Starting point of the chunk</param>
    /// <returns>True if chunk has terrain to form a mesh in</returns>
    public virtual bool GenerateVoxelData(float3 start)
    {
        return false;
    }

    /// <summary>
    /// Generates mesh based on voxel data
    /// </summary>
    /// <param name="start">Starting point of the chunk</param>
    /// <param name="neighbors">Chunk neighbors</param>
    /// <returns>Generated Mesh</returns>
    public virtual Mesh GenerateMesh(float3 start, Node[] neighbors)
    {
        return null;
    }

    /// <summary>
    /// Adds values of a specified edge to list from different chunk
    /// </summary>
    /// <param name="v">Wanted edge</param>
    /// <param name="c">Modifiable list of cubes</param>
    /// <param name="p">Modifiable list of vertices</param>
    /// <param name="dif">Value difference between original edge and wanted edge</param>
    /// <param name="otherLOD">Level of detail from original chunk</param>
    /// <param name="otherAxisID">AxisID from original chunk</param>
    /// <param name="vertices">Quantity of vertices the edge has</param>
    /// <returns>True if values could be added, false if not</returns>
    public virtual bool getEdgeCubes(int3x2 v, ref List<int3> c, ref List<float4> p, int3 dif, int otherLOD, int otherAxisID, int vertices)
    {
        return false;
    }
}
