using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class TreeGenerator : MonoBehaviour, IChunkDecorator
{
    public TreeSettings treeSettings;
    private Dictionary<Vector2, List<GameObject>> trees;

    private void Start()
    {
        trees = new Dictionary<Vector2, List<GameObject>>();
    }

    public void OnHeightMapReady(TerrainChunk chunk)
    {
        trees[chunk.coord] = new List<GameObject>();

        int i = 0;
        for(int y = treeSettings.gridStep; y < chunk.MapHeight; y += treeSettings.gridStep)
        {
            for(int x = treeSettings.gridStep; x < chunk.MapWidth; x += treeSettings.gridStep)
            {  
                float scale = 1.0f / treeSettings.noiseScale;
                Vector3 point = chunk.MapToWorldPoint(x, y);
                float prob = Mathf.PerlinNoise(point.x * scale, point.z * scale);
                prob +=  Mathf.PerlinNoise(i++, 0f) * treeSettings.noiseAmplitude;

                if(prob <= treeSettings.placementThreshold)
                {
                    var tree = PlaceTree(chunk, x, y);
                    if(tree != null)
                        trees[chunk.coord].Add(tree);
                }
            }
        }

        //Debug.Log("Tile has " + trees[chunk.coord].Count + " trees");

        OnChunkVisibilityChanged(chunk, chunk.IsVisible());
    }

    private GameObject PlaceTree(TerrainChunk chunk, int x, int y)
    {
        Vector3 pos = chunk.MapToWorldPoint(x, y);
        float normHeight = Mathf.InverseLerp(chunk.MinPossibleHeight, chunk.MaxPossibleHeight, pos.y);

        var possibleTrees = treeSettings.trees.Where(t => normHeight >= t.minHeight && normHeight <= t.maxHeight).ToList();

        if(possibleTrees.Any())
        {
            var prefabs = possibleTrees.SelectMany(t => t.prefabs).ToList();

            int treeIndex = Mathf.RoundToInt(Mathf.Clamp01(Mathf.PerlinNoise(x, y)) * prefabs.Count);

            GameObject tree = Instantiate(prefabs[treeIndex]);
            tree.transform.SetParent(chunk.meshObject.transform);

            tree.transform.position = pos + new Vector3(0f, -0.05f, 0f);
            tree.transform.localScale = Vector3.one * Random.Range(0.75f, 1.25f);
            tree.transform.eulerAngles = new Vector3(Random.Range(0f, 5f), Random.Range(0f, 360f), Random.Range(0f, 5f));

            return tree;
        }

        return null;
    }

    public void OnChunkVisibilityChanged(TerrainChunk chunk, bool visible)
    {
    }
}
