using UnityEngine;
public interface IChunkDecorator
{    
    void HookEvents(TerrainChunk chunk);
}

public class ChunkDecorator : MonoBehaviour, IChunkDecorator
{
    public void HookEvents(TerrainChunk chunk)
    {
        chunk.HeightMapReady += OnHeightMapReady;
        chunk.VisibilityChanged += OnChunkVisibilityChanged;
        chunk.ColliderSet += OnColliderSet;
    }

    public virtual void OnHeightMapReady(TerrainChunk chunk) 
    {

    }
    public virtual void OnColliderSet(TerrainChunk chunk) 
    {

    }
    public virtual void OnChunkVisibilityChanged(TerrainChunk chunk, bool visible) 
    {

    }
}
