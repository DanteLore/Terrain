using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class TerrainGenerator : MonoBehaviour
{
    const float viewerMoveThresholdForChunkUpdate = 25f;
    const float sqrViewerMoveThresholdForChunkUpdate = viewerMoveThresholdForChunkUpdate * viewerMoveThresholdForChunkUpdate;

    public int colliderLodIndex;
    public LODInfo[] detailLevels;

    public MeshSettings meshSettings;

    public HeightMapSettings heightMapSettings;

    public Material mapMaterial;

    public Transform viewer;

    private Vector2 viewerPosition;
    private Vector2 viewerPositionOld;
    float meshWorldSize;
    int chunksVisibleInViewDistance;

    private Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>(); 
    private static List<TerrainChunk> visibleTerrainChunks = new List<TerrainChunk>();

    private List<IChunkDecorator> chunkDecorators;

    public Vector2 GameToMapPos(Vector3 position)
    {
        return new Vector2(position.x / meshSettings.meshScale, position.z / meshSettings.meshScale);
    }

    public void Awake()
    {
        chunkDecorators = GetComponents<IChunkDecorator>().ToList();
    }

    public void Start()
    {
        float maxViewDistance = detailLevels[detailLevels.Length - 1].visibleDistanceThreshold;
        meshWorldSize = meshSettings.MeshWorldSize;
        chunksVisibleInViewDistance = Mathf.RoundToInt(maxViewDistance / meshWorldSize);

        UpdateVisibleChunks();
    }

    public void Update()
    {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z);

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

        int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / meshWorldSize);
        int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / meshWorldSize);

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
                        var newChunk = new TerrainChunk(viewedChunkCoord, heightMapSettings, meshSettings, detailLevels, colliderLodIndex, transform, viewer, mapMaterial);

                        terrainChunkDictionary.Add(viewedChunkCoord, newChunk);
                        newChunk.VisibilityChanged += OnTerrainChunkVisibilityChanged;

                        if(chunkDecorators != null)
                        {
                            foreach(var decorator in chunkDecorators)
                            {
                                decorator.HookEvents(newChunk);
                            }
                        }

                        newChunk.Load();
                    }
                }
            }
        }
    }

    private void OnTerrainChunkVisibilityChanged(TerrainChunk chunk, bool isVisible)
    {
        if(isVisible)
            visibleTerrainChunks.Add(chunk);
        else   
            visibleTerrainChunks.Remove(chunk);
    }
}


[System.Serializable]
public struct LODInfo 
{
    [Range(0, MeshSettings.numSupportedLODs - 1)]
    public int lod;
    public float visibleDistanceThreshold;

    public float SqrVisibleDistanceThreshold
    {
        get { return visibleDistanceThreshold * visibleDistanceThreshold; }
    }
}
