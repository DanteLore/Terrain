using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class MeshGenerator
{

    public static MeshData GenerateTerrainMesh(float[,] heightMap, MeshSettings meshSettings, int levelOfDetail)
    {
        int skipIncrement = (levelOfDetail == 0) ? 1 : levelOfDetail * 2;
        int numVertsPerLine = meshSettings.NumberOfVerticesPerLine;

        Vector2 topLeft = new Vector2(-1, 1) * (meshSettings.MeshWorldSize / 2f);

        MeshData meshData = new MeshData(numVertsPerLine, skipIncrement, meshSettings.useFlatShading);

        int[,] vertexIndicesMap = new int[numVertsPerLine, numVertsPerLine];
        int meshVertexIndex = 0;
        int outOfMeshVertexIndex = -1;

        for(int y = 0; y < numVertsPerLine; y++)
        {
            for(int x = 0; x < numVertsPerLine; x++)
            {
                bool isOutOfMeshVertex = (y == 0 || y == numVertsPerLine - 1 || x == 0 || x == numVertsPerLine - 1);
                bool isSkippedVertex = (x > 2 && x < numVertsPerLine - 3 && y > 2 && y < numVertsPerLine - 3 && ((x - 2) % skipIncrement != 0 || (y - 2) % skipIncrement != 0));

                if(isOutOfMeshVertex)
                    vertexIndicesMap[x,y] = outOfMeshVertexIndex--;
                else if(!isSkippedVertex)
                    vertexIndicesMap[x,y] = meshVertexIndex++;
            }
        }

        for(int y = 0; y < numVertsPerLine; y++)
        {
            for(int x = 0; x < numVertsPerLine; x++)
            {
                bool isSkippedVertex = (x > 2 && x < numVertsPerLine - 3 && y > 2 && y < numVertsPerLine - 3 && ((x - 2) % skipIncrement != 0 || (y - 2) % skipIncrement != 0));

                if(!isSkippedVertex)
                {
                    bool isOutOfMeshVertex = (y == 0 || y == numVertsPerLine - 1 || x == 0 || x == numVertsPerLine - 1);
                    bool isMeshEdgeVertex = ((y == 1 || y == numVertsPerLine - 2 || x == 1 || x == numVertsPerLine - 2) && !isOutOfMeshVertex);
                    bool isMainVertex = ((x - 2) % skipIncrement == 0 && (y - 2) % skipIncrement == 0 && !isOutOfMeshVertex && !isMeshEdgeVertex);
                    bool isEdgeConnectionVertex = (y == 2 || y == numVertsPerLine - 3 || x == 2 || x == numVertsPerLine - 3) && !isOutOfMeshVertex && !isMeshEdgeVertex && !isMainVertex;

                    int vertexIndex = vertexIndicesMap[x, y];
                    Vector2 percent = new Vector2(x - 1, y - 1) / (numVertsPerLine - 3);
                    Vector2 vertexPosition2D = topLeft + new Vector2(percent.x, -percent.y) * meshSettings.MeshWorldSize;
                    float height = heightMap[x, y];

                    if(isEdgeConnectionVertex)
                    {
                        bool isVertical = (x == 2 || x == numVertsPerLine - 3);
                        int distToMainVertexA = (isVertical ? y - 2 : x - 2) % skipIncrement;
                        int distToMainVertexB = skipIncrement - distToMainVertexA;
                        float distancePercentAtoB = distToMainVertexA / (float)skipIncrement;

                        float heightOfMainVertexA = heightMap[(isVertical) ? x : x - distToMainVertexA, (isVertical) ? y - distToMainVertexA : y];
                        float heightOfMainVertexB = heightMap[(isVertical) ? x : x + distToMainVertexB, (isVertical) ? y + distToMainVertexB : y];

                        height = heightOfMainVertexA * (1 - distancePercentAtoB) + heightOfMainVertexB * distancePercentAtoB;
                    }
                    
                    meshData.AddVertex(new Vector3(vertexPosition2D.x, height, vertexPosition2D.y), percent, vertexIndex);

                    bool createTriangle = (x < numVertsPerLine - 1 && y < numVertsPerLine - 1 && (!isEdgeConnectionVertex || (x != 2 && y != 2)));

                    if(createTriangle)
                    {
                        int currentIncrement = (isMainVertex && x != numVertsPerLine - 3 && y != numVertsPerLine - 3) ? skipIncrement : 1;

                        int a = vertexIndicesMap[x, y];
                        int b = vertexIndicesMap[x + currentIncrement, y];
                        int c = vertexIndicesMap[x, y + currentIncrement];
                        int d = vertexIndicesMap[x + currentIncrement, y + currentIncrement];
                
                        meshData.AddTriangle(a, d, c);
                        meshData.AddTriangle(d, a, b);
                    }
                }
            }
        }

        meshData.ProcessMesh();
        
        return meshData;
    }
}

public class MeshData
{
    private Vector3[] vertices;
    private int[] triangles;
    private Vector2[] uvs;
    private Vector3[] bakedNormals;

    private int triangleIndex = 0;
    private Vector3[] outOfMeshVertices;
    private int[] outOfMeshTriangles;
    private int outOfMeshTriangleIndex = 0;

    private bool useFlatShading;

    public MeshData(int numVertsPerLine, int skipIncrement, bool useFlatShading)
    {
        this.useFlatShading = useFlatShading;

        int numberOfMeshEdgeVertices = (numVertsPerLine - 2) * 4 - 4;
        int numberOfEdgeConnectionVertices = (skipIncrement - 1) * (numVertsPerLine - 5) / skipIncrement * 4;
        int numberOfMainVerticesPerLine = (numVertsPerLine - 5) / skipIncrement + 1;
        int numMainVertices = numberOfMainVerticesPerLine * numberOfMainVerticesPerLine;
        
        vertices = new Vector3[numberOfMeshEdgeVertices + numberOfEdgeConnectionVertices + numMainVertices];
        uvs = new Vector2[vertices.Length];

        int numMeshEdgeTriangles = 8 * (numVertsPerLine - 4);
        int numMainTriangles = (numberOfMainVerticesPerLine - 1) * (numberOfMainVerticesPerLine - 1) * 2;
        triangles = new int[(numMeshEdgeTriangles + numMainTriangles) * 3];

        outOfMeshVertices = new Vector3[numVertsPerLine * 4 - 4];
        outOfMeshTriangles = new int[24 * (numVertsPerLine - 2)];
    }

    public void AddVertex(Vector3 vertexPosition, Vector2 vertexUV, int vertexIndex)
    {
        if(vertexIndex < 0)
        {
            outOfMeshVertices[-vertexIndex - 1] = vertexPosition;
        }
        else
        {
            vertices[vertexIndex] = vertexPosition;
            uvs[vertexIndex] = vertexUV;
        }
    }

    public void AddTriangle(int a, int b, int c)
    {
        if(a < 0 || b < 0 || c < 0)
        {
            outOfMeshTriangles[outOfMeshTriangleIndex] = a;
            outOfMeshTriangles[outOfMeshTriangleIndex + 1] = b;
            outOfMeshTriangles[outOfMeshTriangleIndex + 2] = c;

            outOfMeshTriangleIndex += 3;
        }
        else
        {
            triangles[triangleIndex] = a;
            triangles[triangleIndex + 1] = b;
            triangles[triangleIndex + 2] = c;

            triangleIndex += 3;
        }
    }

    private Vector3[] CalculateNormals()
    {
        Vector3[] vertexNormals = new Vector3[vertices.Length];

        int triangleCount = triangles.Length / 3;
        for(int i = 0; i < triangleCount; i++)
        {
            int normalTriangleIndex = i * 3;
            int vertexIndexA = triangles[normalTriangleIndex];
            int vertexIndexB = triangles[normalTriangleIndex + 1];
            int vertexIndexC = triangles[normalTriangleIndex + 2];

            Vector3 triangleNormal = SurfaceNormalFromIndices(vertexIndexA, vertexIndexB, vertexIndexC);
            vertexNormals[vertexIndexA] += triangleNormal;
            vertexNormals[vertexIndexB] += triangleNormal;
            vertexNormals[vertexIndexC] += triangleNormal;
        }

        int borderTriangleCount = outOfMeshTriangles.Length / 3;
        for(int i = 0; i < borderTriangleCount; i++)
        {
            int normalTriangleIndex = i * 3;
            int vertexIndexA = outOfMeshTriangles[normalTriangleIndex];
            int vertexIndexB = outOfMeshTriangles[normalTriangleIndex + 1];
            int vertexIndexC = outOfMeshTriangles[normalTriangleIndex + 2];

            Vector3 triangleNormal = SurfaceNormalFromIndices(vertexIndexA, vertexIndexB, vertexIndexC);
            if(vertexIndexA >= 0)
                vertexNormals[vertexIndexA] += triangleNormal;
            if(vertexIndexB >= 0)
                vertexNormals[vertexIndexB] += triangleNormal;
            if(vertexIndexC >= 0)
                vertexNormals[vertexIndexC] += triangleNormal;
        }

        for(int i = 0; i < vertexNormals.Length; i++)
            vertexNormals[i].Normalize();

        return vertexNormals;
    }

    private Vector3 SurfaceNormalFromIndices(int indexA, int indexB, int indexC)
    {
        Vector3 pointA = (indexA < 0) ? outOfMeshVertices[-indexA - 1] : vertices[indexA];
        Vector3 pointB = (indexB < 0) ? outOfMeshVertices[-indexB - 1] : vertices[indexB];
        Vector3 pointC = (indexC < 0) ? outOfMeshVertices[-indexC - 1] : vertices[indexC];

        Vector3 sideAB = pointB - pointA;
        Vector3 sideAC = pointC - pointA;

        return Vector3.Cross(sideAB, sideAC).normalized;
    }

    public void ProcessMesh()
    {
        if(useFlatShading)
            FlatShading();
        else
            BakeNormals();
    }

    private void BakeNormals()
    {
        bakedNormals = CalculateNormals();
    }

    private void FlatShading()
    {
        Vector3[] flatShadedVertices = new Vector3[triangles.Length];
        Vector2[] flatShadedUvs = new Vector2[triangles.Length];

        for(int i = 0; i < triangles.Length; i++)
        {
            flatShadedVertices[i] = vertices[triangles[i]];
            flatShadedUvs[i] = uvs[triangles[i]];
            triangles[i] = i;
        }

        vertices = flatShadedVertices;
        uvs = flatShadedUvs;
    }

    public Mesh CreateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        if(useFlatShading)
            mesh.RecalculateNormals();
        else
            mesh.normals = bakedNormals;
        return mesh;
    }
}