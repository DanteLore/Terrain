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
        chunk.LodChange += OnLodChange;
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

    public virtual void OnLodChange(TerrainChunk chunk, int lod)
    {
        
    }
}
