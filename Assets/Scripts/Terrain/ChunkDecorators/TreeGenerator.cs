using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class TreeGenerator : ChunkDecorator
{
    [Range(1, 16)]
    public int gridStep = 6;
    private Dictionary<Vector2, List<GameObject>> trees;

    private void Start()
    {
        trees = new Dictionary<Vector2, List<GameObject>>();
    }

    public override void OnHeightMapReady(TerrainChunk chunk)
    {
        GenerateTrees(chunk);
    }

    private void GenerateTrees(TerrainChunk chunk)
    {
        System.Random rand = new System.Random(Mathf.RoundToInt(chunk.coord.y * 10000000000 + chunk.coord.x));

        trees[chunk.coord] = new List<GameObject>();

        for(int y = gridStep; y < chunk.MapHeight; y += gridStep)
        {
            for(int x = gridStep; x < chunk.MapWidth; x += gridStep)
            {  
                Vector3 point = chunk.MapToWorldPoint(x, y);
                TreeSettings treeSettings = chunk.NearestBiome(point).settings.treeSettings;
                float scale = 1.0f / treeSettings.noiseScale;
                float prob = Mathf.PerlinNoise(point.x * scale, point.z * scale);
                prob +=  Mathf.Lerp(-treeSettings.noiseAmplitude, treeSettings.noiseAmplitude, (float)rand.NextDouble());

                if(prob <= treeSettings.placementThreshold)
                {
                    var tree = PlaceTree(chunk, x, y, rand);
                    if(tree != null)
                        trees[chunk.coord].Add(tree);
                }
            }
        }
    }

    private GameObject PlaceTree(TerrainChunk chunk, int x, int y, System.Random rand)
    {
        Vector3 pos = chunk.MapToWorldPoint(x, y);
        TreeSettings treeSettings = chunk.NearestBiome(pos).settings.treeSettings;
        float normHeight = Mathf.InverseLerp(chunk.MinPossibleHeight, chunk.MaxPossibleHeight, pos.y);

        var possibleTrees = treeSettings.trees.Where(t => normHeight >= t.minHeight && normHeight <= t.maxHeight).ToList();

        if(possibleTrees.Any())
        {
            var prefabs = possibleTrees.SelectMany(t => t.prefabs).ToList();

            GameObject tree = Instantiate(prefabs[rand.Next(prefabs.Count)]);
            tree.transform.SetParent(chunk.meshObject.transform);

            tree.transform.position = pos + new Vector3(0f, -0.05f, 0f);
            tree.transform.localScale = Vector3.one * Mathf.Lerp(0.75f, 1.25f, (float)rand.NextDouble());
            tree.transform.eulerAngles = new Vector3(Mathf.Lerp(0f, 5f, (float)rand.NextDouble()), Mathf.Lerp(0f, 360f, (float)rand.NextDouble()), Mathf.Lerp(0f, 5f, (float)rand.NextDouble()));

            return tree;
        }

        return null;
    }
}
