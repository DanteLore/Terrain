using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterGenerator : ChunkDecorator
{
    public float waterLevel = 0.01f;

    public GameObject waterPrefab;

    private Dictionary<Vector2, GameObject> waterGameObjects;

    protected override void Awake()
    {
        base.Awake();
        
        priority = 9;
        waterGameObjects = new Dictionary<Vector2, GameObject>();
    }

    public override void OnChunkVisibilityChanged(TerrainChunk chunk, bool visible)
    {
        base.OnChunkVisibilityChanged(chunk, visible);

        if(visible && chunk.MinLocalHeight < waterLevel)
        {
            AddPlane(chunk);
        }
        else if(waterGameObjects.ContainsKey(chunk.coord))
        {
            ReleaseToPool(waterGameObjects[chunk.coord]);
            waterGameObjects.Remove(chunk.coord);
        }
    }

    private void AddPlane(TerrainChunk chunk)
    {
        if(!waterGameObjects.ContainsKey(chunk.coord))
        {
            var water = InstantiateFromPool(waterPrefab);
            water.transform.parent = chunk.meshObject.transform;
            water.layer = LayerMask.NameToLayer("Water");
            water.name = waterPrefab.name;

            water.transform.position = chunk.meshObject.transform.position + new Vector3(0, waterLevel, 0);
            water.transform.localScale = new Vector3(chunk.TileSize / 10, 1, chunk.TileSize / 10); // divide by 10 because the prefab is 10x10 - hacky :(

            waterGameObjects.Add(chunk.coord, water);
        }
    }
}
