using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class TreeGenerator : MonoBehaviour
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

        for(int y = treeSettings.gridStep; y < chunk.MapHeight; y += treeSettings.gridStep)
        {
            for(int x = treeSettings.gridStep; x < chunk.MapWidth; x += treeSettings.gridStep)
            {               
                if(Random.Range(0.0f, 1.0f) <= treeSettings.placementProbability)
                {
                    var tree = PlaceTree(chunk, x, y);
                    if(tree != null)
                        trees[chunk.coord].Add(tree);
                }
            }
        }

        OnChunkVisibilityChanged(chunk, chunk.IsVisible());
    }

    private GameObject PlaceTree(TerrainChunk chunk, int x, int y)
    {
        Vector3 pos = chunk.MapToWorldPoint(x, y);
        float normHeight = Mathf.InverseLerp(chunk.MinHeight, chunk.MaxHeight, pos.y);

        var possibleTrees = treeSettings.trees.Where(t => normHeight >= t.minHeight && normHeight <= t.maxHeight).ToList();

        if(possibleTrees.Any())
        {
            var prefabs = possibleTrees.SelectMany(t => t.prefabs).ToList();

            GameObject tree = Instantiate(prefabs[Random.Range(0, prefabs.Count)]);
            tree.transform.SetParent(transform);

            tree.transform.position = pos;
            tree.transform.localScale = Vector3.one * Random.Range(0.75f, 1.25f);
            tree.transform.eulerAngles = new Vector3(Random.Range(0f, 5f), Random.Range(0f, 360f), Random.Range(0f, 5f));

            return tree;
        }

        return null;
    }

    public void OnChunkVisibilityChanged(TerrainChunk chunk, bool visible)
    {
        if(trees.ContainsKey(chunk.coord))
        {
            foreach(var tree in trees[chunk.coord])
            {
                tree.SetActive(visible);
            }
        }
    }
}