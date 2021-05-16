using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public interface IChunkDecorator
{    
    void HookEvents(TerrainChunk chunk);

    int priority { get; }
}

public class ChunkDecorator : MonoBehaviour, IChunkDecorator
{
    public int priority { get; protected set; }

    private Dictionary<string, Queue<GameObject>> objectPool = new Dictionary<string, Queue<GameObject>>();

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

    protected GameObject InstantiateFromPool(GameObject prefab)
    {
        string key = prefab.name + "(Clone)";

        if(!objectPool.ContainsKey(key))
        {
            objectPool.Add(key, new Queue<GameObject>());
        }

        var obj = (objectPool[key].Any()) ? objectPool[key].Dequeue() : Instantiate(prefab);
        obj.SetActive(true);

        return obj;
    }

    protected void ReleaseToPool(GameObject obj)
    {
        objectPool[obj.name].Enqueue(obj);
    }

    int i = 1;
    void Update()
    {
        if(--i <= 0)
        {
            foreach(var key in objectPool.Keys)
            {
                Debug.Log(key + " => " + objectPool[key].Count);
            }

            i = 1000;
        }
    }
}
