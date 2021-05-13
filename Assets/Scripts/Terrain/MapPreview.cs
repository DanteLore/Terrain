using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class MapPreview : MonoBehaviour
{
    public enum DrawMode { NoiseMap, Mesh }

    public MeshSettings meshSettings;
    public HeightMapSettings heightMapSettings;

    public Material terrainMaterial;

    [Range(0, MeshSettings.numSupportedLODs - 1)]
    public int editorPreviewLevelOfDetail;
    public bool autoUpdate = true;
 
    public DrawMode drawMode;

    public Renderer textureRenderer;
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;

    public void DrawMapInEditor()
    {
        HeightMap heightMap = HeightMapGenerator.GenerateHeightMap(meshSettings.NumberOfVerticesPerLine, meshSettings.NumberOfVerticesPerLine, heightMapSettings, Vector2.zero);

        if(drawMode == DrawMode.NoiseMap)
            DrawTexture(TextureGenerator.TextureFromHeightMap(heightMap));
        else if(drawMode == DrawMode.Mesh)
            DrawMesh(MeshGenerator.GenerateTerrainMesh(heightMap.values, meshSettings, editorPreviewLevelOfDetail));
    }

    public void DrawTexture(Texture2D texture)
    {
        textureRenderer.sharedMaterial.mainTexture = texture;
        textureRenderer.transform.localScale = new Vector3(texture.width, 1, texture.height) / 10f;

        textureRenderer.gameObject.SetActive(true);
        meshFilter.gameObject.SetActive(false);
    }

    public void DrawMesh(MeshData meshData)
    {
        meshFilter.sharedMesh = meshData.CreateMesh();

        textureRenderer.gameObject.SetActive(false);
        meshFilter.gameObject.SetActive(true);
    }

    void OnValidate()
    {
        if(meshSettings != null)
        {
            meshSettings.OnValuesUpdated -= OnValuesUpdated;
            meshSettings.OnValuesUpdated += OnValuesUpdated;
        }

        if(heightMapSettings != null)
        {
            heightMapSettings.OnValuesUpdated -= OnValuesUpdated;
            heightMapSettings.OnValuesUpdated += OnValuesUpdated;
        }
    }

    private void OnValuesUpdated() {
        if(!Application.isPlaying)
            Invoke("DrawMapInEditor", 0.5f);
    }
}
