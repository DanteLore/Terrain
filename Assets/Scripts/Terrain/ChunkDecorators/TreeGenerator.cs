using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class TreeGenerator : ChunkDecorator
{
    [Range(1, 16)]
    public int gridStep = 6;
    private Dictionary<Vector2, List<GameObject>> trees;

    void Awake()
    {
        priority = 10;
        trees = new Dictionary<Vector2, List<GameObject>>();
    }

    public override void OnHeightMapReady(TerrainChunk chunk)
    {
        GenerateTrees(chunk);
    }

    private void GenerateTrees(TerrainChunk chunk)
    {
        System.Random rand = new System.Random(Mathf.RoundToInt(chunk.coord.y * 10000000 + chunk.coord.x));

        trees[chunk.coord] = new List<GameObject>();

        for(int y = 0; y <= chunk.MapHeight; y += gridStep)
        {
            for(int x = 0; x <= chunk.MapWidth; x += gridStep)
            {  
                int pX = Mathf.Clamp(x + rand.Next(gridStep) - gridStep * 2, 0, chunk.MapWidth - 1); // Don't be so regular
                int pY = Mathf.Clamp(y + rand.Next(gridStep) - gridStep * 2, 0, chunk.MapHeight - 1);

                Vector3 point = chunk.MapToWorldPoint(pX, pY); 

                if(!chunk.IsInExclusionZone(point))
                {
                    TreeSettings treeSettings = chunk.BlendedBiome(point, rand).settings.treeSettings;
                    float scale = 1.0f / treeSettings.noiseScale;
                    float prob = Mathf.PerlinNoise(point.x * scale, point.z * scale);
                    prob +=  Mathf.Lerp(-treeSettings.noiseAmplitude, treeSettings.noiseAmplitude, (float)rand.NextDouble());

                    if(prob <= treeSettings.placementThreshold)
                    {
                        var tree = PlaceTree(chunk, point, rand, treeSettings);
                        if(tree != null)
                            trees[chunk.coord].Add(tree);
                    }
                }
            }
        }
    }

    private GameObject PlaceTree(TerrainChunk chunk, Vector3 pos, System.Random rand, TreeSettings treeSettings)
    {
        float normHeight = Mathf.InverseLerp(chunk.MinPossibleHeight, chunk.MaxPossibleHeight, pos.y);

        var possibleTrees = treeSettings.trees.Where(t => normHeight >= t.minHeight && normHeight <= t.maxHeight).ToList();

        if(possibleTrees.Any())
        {
            var prefabs = possibleTrees.SelectMany(t => t.prefabs).ToList();

            GameObject tree = Instantiate(prefabs[rand.Next(prefabs.Count)]);
            tree.transform.SetParent(chunk.meshObject.transform);

            tree.transform.position = pos + new Vector3(0, -0.05f, 0); // shift down into the ground a little
            tree.transform.localScale = Vector3.one * Mathf.Lerp(0.75f, 1.25f, (float)rand.NextDouble());
            tree.transform.eulerAngles = new Vector3((float)rand.NextDouble() * 5f, (float)rand.NextDouble() * 360f, (float)rand.NextDouble() * 5f);

            return tree;
        }

        return null;
    }
}
