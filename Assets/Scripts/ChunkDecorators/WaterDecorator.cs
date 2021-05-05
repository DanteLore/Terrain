using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterDecorator : MonoBehaviour, IChunkDecorator
{
    public float waterLevel = 0.01f;

    public GameObject waterPrefab;

    private Dictionary<Vector2, GameObject> waterGameObjects;


    private void Start()
    {
        waterGameObjects = new Dictionary<Vector2, GameObject>();
    }

    public void OnHeightMapReady(TerrainChunk chunk)
    {
        if(chunk.MinHeight < waterLevel)
        {
            Debug.Log("Adding a water plane to this chunk");

            AddPlane(chunk);
        }
        else
        {
            Debug.Log("NOT Adding a water plane to this chunk");
        }
    }

    public void OnChunkVisibilityChanged(TerrainChunk chunk, bool visible)
    {
        // Nothing to do here
    }

    private void AddPlane(TerrainChunk chunk)
    {
        if(!waterGameObjects.ContainsKey(chunk.coord))
        {
            var water = Instantiate(waterPrefab, Vector3.zero, Quaternion.identity);
            water.transform.parent = chunk.meshObject.transform;

            water.transform.position = chunk.meshObject.transform.position + new Vector3(0, waterLevel, 0);
            water.transform.localScale = new Vector3(chunk.TileSize / 10, 1, chunk.TileSize / 10); // divide by 10 because the prefab is 10x10 - hacky :(

            waterGameObjects.Add(chunk.coord, water);
        }
    }
}
