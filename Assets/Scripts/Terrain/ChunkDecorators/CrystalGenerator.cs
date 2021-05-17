using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrystalGenerator : ChunkDecorator
{
    public CrystalSettings crystalSettings;

    private Dictionary<Vector2, List<GameObject>> crystals;
    private Dictionary<Vector2, List<CrystalCluster>> clusters;
    void Awake()
    {
        priority = 10;
        crystals = new Dictionary<Vector2, List<GameObject>>();
        clusters = new Dictionary<Vector2, List<CrystalCluster>>();
    }
    public override void OnLodChange(TerrainChunk chunk, int lod)
    {
        base.OnLodChange(chunk, lod);

        if(lod <= crystalSettings.lodIndex)
        {
            if(crystals.ContainsKey(chunk.coord))
            {
                crystals[chunk.coord].ForEach(f => f.SetActive(true));
            }
            else
            {
                System.Random rand = new System.Random(Mathf.RoundToInt(chunk.coord.y) * 1000000 + Mathf.RoundToInt(chunk.coord.x));
                GenerateClusterCenters(chunk, rand);
                GenerateCrystals(chunk, rand);
            }
        }
        else if(crystals.ContainsKey(chunk.coord))
        {
            crystals[chunk.coord].ForEach(ReleaseToPool);
            crystals.Remove(chunk.coord);
        }
    }

    private void GenerateClusterCenters(TerrainChunk chunk, System.Random rand)
    {
        if(!clusters.ContainsKey(chunk.coord))
        {
            List<CrystalCluster> chunkClusters = new List<CrystalCluster>();
            for(int i = 0; i < rand.Next(crystalSettings.maxClustersPerChunk); i++)
            {
                float radius = crystalSettings.minClusterRadius + (float)rand.NextDouble() + (crystalSettings.maxClusterRadius - crystalSettings.minClusterRadius);
                int r = Mathf.CeilToInt(radius);
                int centerX = r + rand.Next(chunk.MapWidth - r * 2);
                int centerY = r + rand.Next(chunk.MapHeight - r * 2);

                chunkClusters.Add(new CrystalCluster(centerX, centerY, radius));
            }
            
            clusters.Add(chunk.coord, chunkClusters);
        }
    }

    private void GenerateCrystals(TerrainChunk chunk, System.Random rand)
    {
        crystals[chunk.coord] = new List<GameObject>();

        foreach(var cluster in clusters[chunk.coord])
        {
            for(int y = cluster.minY; y < cluster.maxY; y += crystalSettings.gridStep)
            {
                for(int x = cluster.minX; x < cluster.maxX; x += crystalSettings.gridStep)
                {  
                    Vector2 pos = new Vector2(x, y);
                    float dist = (pos - cluster.position).sqrMagnitude;

                    if(dist <= cluster.radiusSquared)
                    {
                        float prob = (float)rand.NextDouble();

                        if(prob <= crystalSettings.placementThreshold)
                        {
                            var crystal = PlaceCrystal(chunk, x, y, cluster, rand);
                            if(crystal != null)
                                crystals[chunk.coord].Add(crystal);
                        }
                    }
                }
            }
        }
    }

    private GameObject PlaceCrystal(TerrainChunk chunk, int x, int y, CrystalCluster cluster, System.Random rand)
    {
        Vector3 p1 = chunk.MapToWorldPoint(x, y);
        Vector3 p2 = chunk.MapToWorldPoint(x + 1, y);
        Vector3 p3 = chunk.MapToWorldPoint(x, y + 1);
        Vector3 normal = SurfaceNormalFromPoints(p1, p2, p3);

        Vector3 pos = (p1 + p2 + p3) / 3; // Centre of triangle

        float normHeight = Mathf.InverseLerp(chunk.MinPossibleHeight, chunk.MaxPossibleHeight, pos.y);

        if(normHeight < crystalSettings.minHeight || normHeight > crystalSettings.maxHeight)
            return null;

        int index = rand.Next(crystalSettings.prefabs.Length);
        GameObject prefab = crystalSettings.prefabs[index];
        GameObject crystal = InstantiateFromPool(prefab);
        crystal.name = prefab.name;
        crystal.transform.SetParent(chunk.meshObject.transform);
        crystal.layer = LayerMask.NameToLayer("Crystals");

        var randomRotation = Quaternion.Euler((float)rand.NextDouble() * crystalSettings.maxTiltAngle, (float)rand.NextDouble() * 360f, (float)rand.NextDouble() * crystalSettings.maxTiltAngle);
        var layFlat = Quaternion.FromToRotation(transform.up, normal);
        crystal.transform.rotation = crystal.transform.rotation * layFlat * randomRotation;

        crystal.transform.position = pos + new Vector3(0f, -0.05f, 0f);
        crystal.transform.localScale = Vector3.one * Mathf.Lerp(crystalSettings.crystalScale * 0.5f, crystalSettings.crystalScale * 1.5f, (float)rand.NextDouble());

        return crystal;
    }

    private struct CrystalCluster
    {
        public int centerX;
        public int centerY;
        public float radius;
        public float radiusSquared;
        public int minX, maxX;
        public int minY, maxY;
        public Vector2 position;

        public CrystalCluster(int centerX, int centerY, float radius)
        {
            this.centerX = centerX;
            this.centerY = centerY;
            this.radius = radius;
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
