using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlowerGenerator : ChunkDecorator
{
    [Range(1, 16)]
    public int gridStep = 2;

    public bool flowersEnabled = true;

    private Dictionary<Vector2, List<GameObject>> flowers;
    private Dictionary<Vector2, List<FlowerCluster>> clusters;
    private Dictionary<Vector2, GameObject> meshParents;

    protected override void Awake()
    {
        base.Awake();

        priority = 10;
        flowers = new Dictionary<Vector2, List<GameObject>>();
        clusters = new Dictionary<Vector2, List<FlowerCluster>>();
    }

    public override void OnLodChange(TerrainChunk chunk, int lod)
    {
        base.OnLodChange(chunk, lod);

        if(!flowersEnabled)
            return;

        System.Random rand = new System.Random(Mathf.RoundToInt(chunk.coord.y) * 1000000 + Mathf.RoundToInt(chunk.coord.x));

        FlowerSettings flowerSettings = chunk.BlendedBiome(chunk.sampleCenter, rand).settings.flowerSettings;

        if(lod <= flowerSettings.lodIndex && !flowers.ContainsKey(chunk.coord))
        {
            GenerateClusterCenters(chunk, rand, flowerSettings);
            GenerateFlowers(chunk, rand, flowerSettings);
            CombineMeshesToParent(flowers[chunk.coord], chunk);
        }
        else if(lod > flowerSettings.lodIndex && flowers.ContainsKey(chunk.coord))
        {
            flowers[chunk.coord].ForEach(ReleaseToPool);
            flowers.Remove(chunk.coord);
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
                            int pX = Mathf.Clamp(x + rand.Next(gridStep) - gridStep * 2, 0, chunk.MapWidth - 1); // Don't be so regular
                            int pY = Mathf.Clamp(y + rand.Next(gridStep) - gridStep * 2, 0, chunk.MapHeight - 1);
                            
                            var flower = PlaceFlower(chunk, chunk.MapToWorldPoint(pX, pY), cluster, rand, flowerSettings);
                            if(flower != null)
                                flowers[chunk.coord].Add(flower);
                        }
                    }
                }
            }
        }
    }

    private GameObject PlaceFlower(TerrainChunk chunk, Vector3 pos, FlowerCluster cluster, System.Random rand, FlowerSettings flowerSettings)
    {
        float normHeight = Mathf.InverseLerp(chunk.MinPossibleHeight, chunk.MaxPossibleHeight, pos.y);

        if(normHeight < flowerSettings.minHeight || normHeight > flowerSettings.maxHeight)
            return null;

        GameObject flower = InstantiateFromPool(cluster.prefab);
        flower.transform.SetParent(chunk.meshObject.transform);
        flower.layer = LayerMask.NameToLayer("Flowers");
        flower.name = cluster.prefab.name;

        var randomRotation = Quaternion.Euler((float)rand.NextDouble() * flowerSettings.maxTiltAngle, (float)rand.NextDouble() * 360f, (float)rand.NextDouble() * flowerSettings.maxTiltAngle);
        flower.transform.rotation = flower.transform.rotation * randomRotation;

        flower.transform.position = pos + Vector3.down * 0.05f;
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
