using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterDecorator : MonoBehaviour, IChunkDecorator
{
    public float waterLevel = 0.1f;

    public Material waterMaterial;
    public void OnHeightMapReady(TerrainChunk chunk)
    {
        if(chunk.MinHeight < waterLevel)
        {
            Debug.Log("Adding a water plane to this chunk");

            AddPlane(chunk);
        }
        else
        {
            Debug.Log("NOT Adding a water plane to this chunk");
        }
    }

    public void OnChunkVisibilityChanged(TerrainChunk chunk, bool visible)
    {
        // Nothing to do here
    }

    private void AddPlane(TerrainChunk chunk)
    {
        var parent = new GameObject("Water Chunk");
        parent.transform.parent = chunk.meshObject.transform;

        MeshRenderer meshRenderer = parent.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = waterMaterial;

        MeshFilter meshFilter = parent.AddComponent<MeshFilter>();

        meshFilter.transform.position = chunk.meshObject.transform.position;
        meshFilter.transform.localScale = chunk.meshObject.transform.localScale;

        Mesh mesh = new Mesh();

        float meshSizeX = chunk.MapWidth - 3;
        float meshSizeZ = chunk.MapWidth - 3;

        Vector3[] vertices = new Vector3[4]
        {
            new Vector3(0, waterLevel, 0),
            new Vector3(meshSizeX, waterLevel, 0),
            new Vector3(0, waterLevel, meshSizeZ),
            new Vector3(meshSizeX, waterLevel, meshSizeZ)
        };
        mesh.vertices = vertices;

        int[] tris = new int[6]
        {
            // lower left triangle
            0, 2, 1,
            // upper right triangle
            2, 3, 1
        };
        mesh.triangles = tris;

        Vector3[] normals = new Vector3[4]
        {
            -Vector3.forward,
            -Vector3.forward,
            -Vector3.forward,
            -Vector3.forward
        };
        mesh.normals = normals;

        Vector2[] uv = new Vector2[4]
        {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(0, 1),
            new Vector2(1, 1)
        };
        mesh.uv = uv;

        meshFilter.mesh = mesh;
    }
}
