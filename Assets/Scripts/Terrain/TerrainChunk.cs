using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

public class TerrainChunk
{
    public event System.Action<TerrainChunk, bool> VisibilityChanged;
    public event System.Action<TerrainChunk> HeightMapReady;
    public event System.Action<TerrainChunk> ColliderSet;
    public event System.Action<TerrainChunk, int> LodChange;
    const float colliderGenerationDistanceThreshold = 20;

    public Vector2 coord;

    public GameObject meshObject;
    public Vector2 sampleCenter;
    public Bounds bounds;

    private MeshRenderer meshRenderer;
    private MeshFilter meshFilter;
    private MeshCollider meshCollider;

    private LODInfo[] detailLevels;
    private LODMesh[] lodMeshes;
    int colliderLodIndex;

    public HeightMap heightMap;
    bool heightMapReceived;
    int previousLODIndex = -1;
    bool hasSetCollider;
    
    private float maxViewDistance;

    private HeightMapSettings heightMapSettings;
    private MeshSettings meshSettings;
    private Transform viewer;

    private List<Biome> biomes;

    public int MapHeight
    {
        get { return heightMap.values.GetLength(1); }
    }

    public int MapWidth
    {
        get { return heightMap.values.GetLength(0); }
    }

    public float MinPossibleHeight
    {
        get { return heightMapSettings.MinHeight; }
    }
    public float MaxPossibleHeight
    {
        get { return heightMapSettings.MaxHeight; }
    }

    public float MinLocalHeight
    {
        get { return heightMap.minValue; }
    }
    public float MaxLocalHeight
    {
        get { return heightMap.maxValue; }
    }

    public float TileSize
    {
        get { return meshSettings.MeshWorldSize; }
    }

    public bool HasSetCollider
    {
        get { return hasSetCollider; }
        private set 
        {
            hasSetCollider = value;
            if(ColliderSet != null)
                ColliderSet(this);
        }
    }

    public TerrainChunk(Vector2 coord, HeightMapSettings heightMapSettings, MeshSettings meshSettings, LODInfo[] detailLevels, int colliderLodIndex, Transform parent, Transform viewer, Material material)
    {
        this.coord = coord;
        this.heightMapSettings = heightMapSettings;
        this.meshSettings = meshSettings;
        this.detailLevels = detailLevels;
        this.colliderLodIndex = colliderLodIndex;
        this.viewer = viewer;

        this.biomes = new List<Biome>();

        sampleCenter = coord * meshSettings.MeshWorldSize / meshSettings.meshScale;
        Vector2 position = coord * meshSettings.MeshWorldSize;
        bounds = new Bounds(position, Vector2.one * meshSettings.MeshWorldSize);

        meshObject = new GameObject("Terrain Chunk " + coord);
        meshObject.layer = LayerMask.NameToLayer("Terrain");
        meshRenderer = meshObject.AddComponent<MeshRenderer>();
        meshFilter = meshObject.AddComponent<MeshFilter>();
        meshCollider = meshObject.AddComponent<MeshCollider>();

        meshObject.transform.position = new Vector3(position.x, 0, position.y);
        meshObject.transform.parent = parent;
        meshRenderer.material = material;
        SetVisible(false);

        lodMeshes = new LODMesh[detailLevels.Length];
        for(int i = 0; i < detailLevels.Length; i++)
        {
            lodMeshes[i] = new LODMesh(detailLevels[i].lod);
            lodMeshes[i].updateCallback += UpdateTerrainChunk;
            if(i == colliderLodIndex)
                lodMeshes[i].updateCallback += UpdateCollisionMesh;
        }

        maxViewDistance = detailLevels[detailLevels.Length - 1].visibleDistanceThreshold;
    }

    public Biome NearestBiome(Vector3 position)
    {
        Vector2 pos2D = new Vector2(position.x, position.z);

        if(biomes == null || !biomes.Any())
            return null;
        else if(biomes.Count == 1)
            return biomes[0];
        else
        {
            float bestDist = (pos2D - biomes[0].biomeCoords).sqrMagnitude;
            Biome best = biomes[0];
            
            foreach(Biome b in biomes.Skip(1))
            {
                float dist = (pos2D - biomes[0].biomeCoords).sqrMagnitude;
                if(dist < bestDist)
                {
                    bestDist = dist;
                    best = b;
                }
            }

            return best;
        }
    }
    
    public Biome BlendedBiome(Vector3 position, System.Random rand = null, float blendLimit = 50)
    {
        if(biomes == null || !biomes.Any())
            return null;
        else if(biomes.Count == 1)
            return biomes[0];
        else
        {
            if(rand == null)
                rand = new System.Random();

            Vector2 pos = new Vector2(position.x, position.z);

            var distances = biomes.Select(b => ((b.biomeCoords - pos).sqrMagnitude, b));
            var sorted = distances.OrderBy(x => x.Item1);
            float minDistance = sorted.Select(x => x.Item1).First();
            var withDistance = sorted.Select(x => (Mathf.Abs(x.Item1 - minDistance), x.Item2));
            var closeOnes = withDistance.Where(x => x.Item1 <= blendLimit * blendLimit);

            List<Biome> result = closeOnes.Select(x => x.Item2).ToList();

            return result[rand.Next(result.Count)];
        }
    }

    public void Load()
    {
        ThreadedDataRequester.RequestData(() => HeightMapGenerator.GenerateHeightMap(meshSettings.NumberOfVerticesPerLine, meshSettings.NumberOfVerticesPerLine, heightMapSettings, sampleCenter), OnHeightMapReceived);
    }

    public Vector3 MapToWorldPoint(int x, int y)
    {
        float height = heightMap.values[x, y];
        Vector2 topLeft = new Vector2(-1, 1) * (meshSettings.MeshWorldSize / 2f);
        Vector2 percent = new Vector2(x - 1, y - 1) / (meshSettings.NumberOfVerticesPerLine - 3);
        Vector2 vertexPosition2D = topLeft + sampleCenter + new Vector2(percent.x, -percent.y) * meshSettings.MeshWorldSize;

        return new Vector3(vertexPosition2D.x, height, vertexPosition2D.y);
    }

    private void OnHeightMapReceived(object data)
    {
        this.heightMap = (HeightMap)data;
        heightMapReceived = true;

        if(HeightMapReady != null)
            HeightMapReady(this);

        UpdateTerrainChunk();
    }

    public void AddBiomes(IEnumerable<Biome> newBiomes)
    {
        biomes = biomes.Union(newBiomes).ToList();
    }

    private Vector2 ViewerPosition { get { return new Vector2(viewer.position.x, viewer.position.z); }}

    public void UpdateTerrainChunk()
    {
        if(heightMapReceived)
        {
            float viewerDistanceFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(ViewerPosition));
            bool wasVisible = IsVisible();
            bool visible = viewerDistanceFromNearestEdge <= maxViewDistance;

            if(visible)
            {
                int lodIndex = 0;

                for(int i = 0; i < detailLevels.Length - 1; i++)
                {
                    if(viewerDistanceFromNearestEdge > detailLevels[i].visibleDistanceThreshold)
                        lodIndex = i + 1;
                    else
                        break;
                }

                if(lodIndex != previousLODIndex)
                {
                    LODMesh lodMesh = lodMeshes[lodIndex];
                    if(lodMesh.hasMesh)
                    {
                        previousLODIndex = lodIndex;
                        meshFilter.mesh = lodMesh.mesh;
                    }
                    else if(!lodMesh.hasRequestedMesh)
                    {
                        lodMesh.RequestMesh(heightMap, meshSettings);
                    }

                    if(LodChange != null)
                        LodChange(this, lodIndex);
                }
            }

            if(wasVisible != visible)
            {
                SetVisible(visible);
            }
        }
    }

    public void UpdateCollisionMesh()
    {
        if(!hasSetCollider)
        {
            float squareDistanceFromViewerToEdge = bounds.SqrDistance(ViewerPosition);
            LODMesh colliderLodMesh = lodMeshes[colliderLodIndex];

            if(squareDistanceFromViewerToEdge < detailLevels[colliderLodIndex].SqrVisibleDistanceThreshold)
            {
                if(!colliderLodMesh.hasRequestedMesh)
                    colliderLodMesh.RequestMesh(heightMap, meshSettings);
            }

            if(squareDistanceFromViewerToEdge < colliderGenerationDistanceThreshold * colliderGenerationDistanceThreshold)
            {
                if(colliderLodMesh.hasMesh)
                {
                    meshCollider.sharedMesh = colliderLodMesh.mesh;
                    HasSetCollider = true;
                }
            }
        }
    }
    public void SetVisible(bool visible)
    {
        meshObject.SetActive(visible);

        if(VisibilityChanged != null)
            VisibilityChanged(this, visible);
    }

    public bool IsVisible()
    {
        return meshObject.activeSelf;
    }
}

class LODMesh
{
    public Mesh mesh;
    public bool hasRequestedMesh;
    public bool hasMesh;
    private int lod;
    public event System.Action updateCallback;

    public LODMesh(int lod)
    {
        this.lod = lod;
    }

    private void OnMeshDataReceived(object data)
    {
        MeshData meshData = (MeshData)data;
        mesh = meshData.CreateMesh();
        hasMesh = true;

        updateCallback();
    }

    public void RequestMesh(HeightMap heightMap, MeshSettings meshSettings)
    {
        hasRequestedMesh = true;
        ThreadedDataRequester.RequestData(() => MeshGenerator.GenerateTerrainMesh(heightMap.values, meshSettings, lod), OnMeshDataReceived);
    }
}