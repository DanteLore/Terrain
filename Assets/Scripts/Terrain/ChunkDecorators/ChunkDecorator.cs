using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

public interface IChunkDecorator
{    
    void HookEvents(TerrainChunk chunk);

    int priority { get; }
}

public class ChunkDecorator : MonoBehaviour, IChunkDecorator
{
    public int priority { get; protected set; }

    private GameObject pool;

    private Dictionary<string, Queue<GameObject>> objectPool = new Dictionary<string, Queue<GameObject>>();

    private Dictionary<Vector2, List<GameObject>> meshParents = new Dictionary<Vector2, List<GameObject>>();

    private Queue<GameObject> meshPool = new Queue<GameObject>();

    protected virtual void Awake()
    {
        pool = new GameObject(this.GetType().Name + " Object Pool");
        pool.transform.parent = gameObject.transform;
    }

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
        if(!visible && meshParents.ContainsKey(chunk.coord))
        {
            foreach(var p in meshParents[chunk.coord])
            {
                p.transform.parent = pool.transform;
                meshPool.Enqueue(p);
                p.SetActive(false);
            }
        }
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
        obj.transform.SetParent(pool.transform);
        objectPool[obj.name].Enqueue(obj);
    }

    protected GameObject CombineMeshesToParent(List<GameObject> items, TerrainChunk chunk)
    {
        if(items.Count == 0)
            return null;

        if(!meshParents.ContainsKey(chunk.coord))
            meshParents.Add(chunk.coord, new List<GameObject>());

        GameObject parent = CreateParent(chunk);
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

    private GameObject CreateParent(TerrainChunk chunk)
    {
        GameObject p;
        if(meshParents.ContainsKey(chunk.coord) && meshParents[chunk.coord].Any())
            p = meshParents[chunk.coord].First();
        else if(meshPool.Count > 0)
            p = meshPool.Dequeue();
        else
            p = new GameObject();
        
        p.name = this.GetType().Name + " Meshes";
        meshParents[chunk.coord].Add(p);
        p.transform.parent = chunk.meshObject.transform;
        
        return p;
    }
}
