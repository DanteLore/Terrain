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
        string key = prefab.name;

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
        obj.SetActive(false);
        objectPool[obj.name].Enqueue(obj);
    }

    protected GameObject CombineMeshesToParent(List<GameObject> items, GameObject grandparent)
    {
        if(items.Count == 0)
            return null;

        GameObject parent = new GameObject(this.GetType().Name + " Meshes");
        parent.transform.parent = grandparent.transform;
        parent.AddComponent<UnityEngine.MeshRenderer>();
        parent.AddComponent<UnityEngine.MeshFilter>();
        parent.GetComponent<Renderer>().material = items[0].GetComponent<Renderer>().material;
        
        CombineInstance[] combine = new CombineInstance[items.Count];

        for(int i = 0; i < combine.Length; i++)
        {
            items[i].transform.parent = parent.transform;

            combine[i].mesh = items[i].GetComponentInChildren<MeshFilter>().sharedMesh;
            combine[i].transform = items[i].transform.localToWorldMatrix;
            items[i].SetActive(false);
        }

        parent.transform.GetComponent<MeshFilter>().mesh = new Mesh();
        parent.transform.GetComponent<MeshFilter>().mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        parent.transform.GetComponent<MeshFilter>().mesh.CombineMeshes(combine);
        parent.transform.gameObject.SetActive(true);

        return parent;
    }
}
