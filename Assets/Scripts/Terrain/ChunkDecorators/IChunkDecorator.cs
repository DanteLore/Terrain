using UnityEngine;
public interface IChunkDecorator
{    
    void HookEvents(TerrainChunk chunk);

    int priority { get; }
}

public class ChunkDecorator : MonoBehaviour, IChunkDecorator
{
    public int priority { get; protected set; }

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

    protected Vector3 SurfaceNormalFromPoints(Vector3 pointA, Vector3 pointB, Vector3 pointC)
    {
        Vector3 sideAB = pointB - pointA;
        Vector3 sideAC = pointC - pointA;

        return Vector3.Cross(sideAB, sideAC).normalized;
    }
}
