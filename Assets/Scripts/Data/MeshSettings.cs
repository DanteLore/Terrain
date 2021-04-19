using UnityEngine;

[CreateAssetMenu]
public class MeshSettings : UpdatableData
{
    public float meshScale = 1f;
    public bool useFlatShading;
    public const int numSupportedLODs = 5;
    public const int numSupportedChunkSizes = 9;
    public const int numSupportedFlatShadedChunkSizes = 3;
    public static readonly int[] supportedChunkSizes = { 48, 72, 96, 120, 144, 168, 192, 216, 240 };

    [Range(0, numSupportedChunkSizes - 1)]
    public int chunkSizeIndex;
    [Range(0, numSupportedFlatShadedChunkSizes - 1)]
    public int flatShadedChunkSizeIndex;

    // Number of vertices per line of a mesh rendered at LOD = 0. Includes the 2 extra vertices at the edges used for normals but not rendered.
    public int NumberOfVerticesPerLine{
        get { return supportedChunkSizes[useFlatShading ? flatShadedChunkSizeIndex : chunkSizeIndex] + 1 ; }
    }
    public float MeshWorldSize {
        get { return (NumberOfVerticesPerLine - 3) * meshScale; }
    }
}
