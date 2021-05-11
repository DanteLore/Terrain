using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlowerGenerator : ChunkDecorator
{
    [Range(1, 16)]
    public int gridStep = 2;

    private Dictionary<Vector2, List<GameObject>> flowers;
    private Dictionary<Vector2, List<FlowerCluster>> clusters;

    void Start()
    {
        flowers = new Dictionary<Vector2, List<GameObject>>();
        clusters = new Dictionary<Vector2, List<FlowerCluster>>();
    }

    public override void OnLodChange(TerrainChunk chunk, int lod)
    {
        base.OnLodChange(chunk, lod);
        System.Random rand = new System.Random(Mathf.RoundToInt(chunk.coord.y) * 1000000 + Mathf.RoundToInt(chunk.coord.x));

        FlowerSettings flowerSettings = chunk.BlendedBiome(chunk.sampleCenter, rand).settings.flowerSettings;

        if(lod <= flowerSettings.lodIndex)
        {
            if(flowers.ContainsKey(chunk.coord))
            {
                flowers[chunk.coord].ForEach(f => f.SetActive(true));
            }
            else
            {
                GenerateClusterCenters(chunk, rand, flowerSettings);
                GenerateFlowers(chunk, rand, flowerSettings);
            }
        }
        else if(flowers.ContainsKey(chunk.coord))
        {
            flowers[chunk.coord].ForEach(f => f.SetActive(false));
        }
    }

    private void GenerateClusterCenters(TerrainChunk chunk, System.Random rand, FlowerSettings mainFlowerSettings)
    {
        if(!clusters.ContainsKey(chunk.coord))
        {
            List<FlowerCluster> chunkClusters = new List<FlowerCluster>();
            for(int i = 0; i < rand.Next(mainFlowerSettings.maxClustersPerChunk); i++)
            {
                float radius = mainFlowerSettings.minClusterRadius + (float)rand.NextDouble() + (mainFlowerSettings.maxClusterRadius - mainFlowerSettings.minClusterRadius);
                int r = Mathf.CeilToInt(radius);
                int centerX = r + rand.Next(chunk.MapWidth - r * 2);
                int centerY = r + rand.Next(chunk.MapHeight - r * 2);
                Vector3 pos = chunk.MapToWorldPoint(centerX, centerY);

                FlowerSettings localFlowerSettings = chunk.BlendedBiome(new Vector2(pos.x, pos.z), rand).settings.flowerSettings;
                
                int index = rand.Next(localFlowerSettings.prefabs.Length);
                GameObject prefab = localFlowerSettings.prefabs[index];

                chunkClusters.Add(new FlowerCluster(centerX, centerY, radius, prefab));
            }
            
            clusters.Add(chunk.coord, chunkClusters);
        }
    }

    private void GenerateFlowers(TerrainChunk chunk, System.Random rand, FlowerSettings flowerSettings)
    {
        flowers[chunk.coord] = new List<GameObject>();

        foreach(var cluster in clusters[chunk.coord])
        {
            for(int y = cluster.minY; y < cluster.maxY; y += gridStep)
            {
                for(int x = cluster.minX; x < cluster.maxX; x += gridStep)
                {  
                    Vector2 pos = new Vector2(x, y);
                    float dist = (pos - cluster.position).sqrMagnitude;

                    if(dist <= cluster.radiusSquared)
                    {
                        float prob = (float)rand.NextDouble();

                        if(prob <= flowerSettings.placementThreshold)
                        {
                            var flower = PlaceFlower(chunk, x, y, cluster, rand, flowerSettings);
                            if(flower != null)
                                flowers[chunk.coord].Add(flower);
                        }
                    }
                }
            }
        }
    }

    private GameObject PlaceFlower(TerrainChunk chunk, int x, int y, FlowerCluster cluster, System.Random rand, FlowerSettings flowerSettings)
    {
        Vector3 pos = chunk.MapToWorldPoint(x, y);

        float normHeight = Mathf.InverseLerp(chunk.MinPossibleHeight, chunk.MaxPossibleHeight, pos.y);

        if(normHeight < flowerSettings.minHeight || normHeight > flowerSettings.maxHeight)
            return null;

        GameObject flower = Instantiate(cluster.prefab);
        flower.transform.SetParent(chunk.meshObject.transform);
        flower.layer = LayerMask.NameToLayer("Flowers");
        flower.name = "Flower on chunk " + chunk.coord + " at: " + pos;

        var randomRotation = Quaternion.Euler((float)rand.NextDouble() * flowerSettings.maxTiltAngle, (float)rand.NextDouble() * 360f, (float)rand.NextDouble() * flowerSettings.maxTiltAngle);
        flower.transform.rotation = flower.transform.rotation * randomRotation;

        flower.transform.position = pos + new Vector3(0f, -0.05f, 0f);
        flower.transform.localScale = Vector3.one * Mathf.Lerp(flowerSettings.flowerScale * 0.5f, flowerSettings.flowerScale * 1.5f, (float)rand.NextDouble());

        return flower;
    }

    private struct FlowerCluster
    {
        public int centerX;
        public int centerY;
        public float radius;
        public float radiusSquared;
        public int minX, maxX;
        public int minY, maxY;
        public Vector2 position;
        public GameObject prefab;

        public FlowerCluster(int centerX, int centerY, float radius, GameObject prefab)
        {
            this.centerX = centerX;
            this.centerY = centerY;
            this.radius = radius;
            this.prefab = prefab;
            this.radiusSquared = radius * radius;

            position = new Vector2(centerX, centerY);

            int r = Mathf.CeilToInt(radius);
            minX = centerX - r;
            maxX = centerX + r;
            minY = centerY - r;
            maxY = centerY + r;
        }
    }
}
