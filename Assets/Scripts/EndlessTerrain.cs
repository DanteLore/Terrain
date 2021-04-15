using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class EndlessTerrain : MonoBehaviour
{
    const float viewerMoveThresholdForChunkUpdate = 25f;
    const float sqrViewerMoveThresholdForChunkUpdate = viewerMoveThresholdForChunkUpdate * viewerMoveThresholdForChunkUpdate;
    const float colliderGenerationDistanceThreashold = 5;

    public int colliderLodIndex;
    public LODInfo[] detailLevels;
    public static float maxViewDistance;

    public Transform viewer;
    public Material mapMaterial;

    public static Vector2 viewerPosition;
    public static Vector2 viewerPositionOld;
    int chunkSize;
    int chunksVisibleInViewDistance;

    private static MapGenerator mapGenerator;

    private Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>(); 
    private static List<TerrainChunk> visibleTerrainChunks = new List<TerrainChunk>();

    public void Start()
    {
        mapGenerator = FindObjectOfType<MapGenerator>();

        maxViewDistance = detailLevels[detailLevels.Length - 1].visibleDistanceThreshold;
        chunkSize = mapGenerator.MapChunkSize - 1;
        chunksVisibleInViewDistance = Mathf.RoundToInt(maxViewDistance / chunkSize);

        UpdateVisibleChunks();
    }

    public void Update()
    {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z) / mapGenerator.terrainData.uniformScale;

        if(viewerPosition != viewerPositionOld)
        {
            foreach(TerrainChunk chunk in visibleTerrainChunks)
            {
                chunk.UpdateCollisionMesh();
            }
        }

        if((viewerPosition - viewerPositionOld).sqrMagnitude > sqrViewerMoveThresholdForChunkUpdate)
        {
            viewerPositionOld = viewerPosition;
            UpdateVisibleChunks();
        }
    }

    void UpdateVisibleChunks()
    {
        HashSet<Vector2> updatedChunkCoords = new HashSet<Vector2>();

        // Loop backwards as chunks may remove themselves
        for(int i = visibleTerrainChunks.Count - 1; i >= 0; i--)
        {
            updatedChunkCoords.Add(visibleTerrainChunks[i].coord);
            visibleTerrainChunks[i].UpdateTerrainChunk();
        }

        int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / chunkSize);
        int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / chunkSize);

        for(int yOffset = -chunksVisibleInViewDistance; yOffset <= chunksVisibleInViewDistance; yOffset++)
        {
            for(int xOffset = -chunksVisibleInViewDistance; xOffset <= chunksVisibleInViewDistance; xOffset++)
            {
                Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);

                if(!updatedChunkCoords.Contains(viewedChunkCoord))
                {
                    if(terrainChunkDictionary.ContainsKey(viewedChunkCoord))
                    {
                        TerrainChunk chunk = terrainChunkDictionary[viewedChunkCoord];
                        chunk.UpdateTerrainChunk();
                    }
                    else
                    {
                        terrainChunkDictionary.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, chunkSize, detailLevels, colliderLodIndex, transform, mapMaterial));
                    }
                }
            }
        }
    }

    public class TerrainChunk
    {
        public Vector2 coord;

        public GameObject meshObject;
        public Vector2 position;
        private Bounds bounds;

        MeshRenderer meshRenderer;
        MeshFilter meshFilter;
        MeshCollider meshCollider;

        LODInfo[] detailLevels;
        LODMesh[] lodMeshes;
        int colliderLodIndex;

        MapData mapData;
        bool mapDataReceived;
        int previousLODIndex = -1;
        bool hasSetCollider;

        public TerrainChunk(Vector2 coord, int size, LODInfo[] detailLevels, int colliderLodIndex, Transform parent, Material material)
        {
            this.coord = coord;
            this.detailLevels = detailLevels;
            this.colliderLodIndex = colliderLodIndex;

            position = coord * size;
            bounds = new Bounds(position, Vector2.one * size);
            Vector3 positionV3 = new Vector3(position.x, 0, position.y);

            meshObject = new GameObject("Terrain Chunk");
            meshRenderer = meshObject.AddComponent<MeshRenderer>();
            meshFilter = meshObject.AddComponent<MeshFilter>();
            meshCollider = meshObject.AddComponent<MeshCollider>();

            meshObject.transform.position = positionV3 * mapGenerator.terrainData.uniformScale;
            meshObject.transform.parent = parent;
            meshObject.transform.localScale = Vector3.one * mapGenerator.terrainData.uniformScale;
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

            mapGenerator.RequestMapData(position, OnMapDataReceived);
        }

        private void OnMapDataReceived(MapData mapData)
        {
            this.mapData = mapData;
            mapDataReceived = true;

            UpdateTerrainChunk();
        }

        public void UpdateTerrainChunk()
        {
            if(mapDataReceived)
            {
                float viewerDistanceFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
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
                            lodMesh.RequestMesh(mapData);
                        }
                    }
                }

                if(wasVisible != visible)
                {
                    if(visible)
                        visibleTerrainChunks.Add(this);
                    else
                        visibleTerrainChunks.Remove(this);

                    SetVisible(visible);
                }
            }
        }

        public void UpdateCollisionMesh()
        {
            if(!hasSetCollider)
            {
                float squareDistanceFromViewerToEdge = bounds.SqrDistance(viewerPosition);
                var colliderLodMesh = lodMeshes[colliderLodIndex];

                if(squareDistanceFromViewerToEdge < detailLevels[colliderLodIndex].SqrVisibleDistanceThreshold)
                {
                    if(!colliderLodMesh.hasRequestedMesh)
                        colliderLodMesh.RequestMesh(mapData);
                }

                if(squareDistanceFromViewerToEdge < colliderGenerationDistanceThreashold * colliderGenerationDistanceThreashold)
                {
                    if(colliderLodMesh.hasMesh)
                        meshCollider.sharedMesh = colliderLodMesh.mesh;
                }
            }
        }

        public void SetVisible(bool visible)
        {
            meshObject.SetActive(visible);
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

        private void OnMeshDataReceived(MeshData meshData)
        {
            mesh = meshData.CreateMesh();
            hasMesh = true;

            updateCallback();
        }

        public void RequestMesh(MapData mapData)
        {
            hasRequestedMesh = true;
            mapGenerator.RequestMeshData(mapData, lod, OnMeshDataReceived);
        }
    }

    [System.Serializable]
    public struct LODInfo 
    {
        [Range(0, MeshGenerator.numSupportedLODs - 1)]
        public int lod;
        public float visibleDistanceThreshold;

        public float SqrVisibleDistanceThreshold
        {
            get { return visibleDistanceThreshold * visibleDistanceThreshold; }
        }
    }
}
