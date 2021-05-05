using UnityEngine;
public interface IChunkDecorator
{
    public void OnHeightMapReady(TerrainChunk chunk);

    public void OnChunkVisibilityChanged(TerrainChunk chunk, bool visible);
}
