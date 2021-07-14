using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshData {
    public Vector3[] vertices;
    public Vector3[,] borderVertices;
    public int[] triangles;
    public Vector2[] uvs;
    public Color[] colors;
    public Vector3[] bakedNormals;

    int meshWidth;
    int meshHeight;

    int verticeIndex;
    int borderCIndex;
    int triangleIndex;
    int uvIndex;
    int colorIndex;

    public MeshData(int meshWidth, int meshHeight) {
        this.meshWidth = meshWidth;
        this.meshHeight = meshHeight;
        vertices = new Vector3[meshWidth * meshHeight];
        borderVertices = new Vector3[meshWidth, 2];
        triangles = new int[(meshWidth - 1) * (meshHeight - 1) * 6];
        uvs = new Vector2[meshWidth * meshHeight];
        colors = new Color[meshWidth * meshHeight];
        verticeIndex = 0;
        borderCIndex = 0;
        triangleIndex = 0;
        uvIndex = 0;
        colorIndex = 0;
    }

    public void AddTriangle(int a, int b, int c) {
        triangles[triangleIndex] = a;
        triangles[triangleIndex + 1] = b;
        triangles[triangleIndex + 2] = c;
        triangleIndex += 3;
    }

    public void AddVertice(float x, float y, float z) {
        vertices[verticeIndex] = new Vector3(x, y, z);
        verticeIndex++;
    }

    public void AddBorder(float x, float y, float z, int t) {
        borderVertices[borderCIndex, t] = new Vector3(x, y, z);
        borderCIndex++;
        if (borderCIndex >= borderVertices.GetLength(0))
            borderCIndex = 0;
    }

    public void AddUV(float u, float v) {
        uvs[uvIndex] = new Vector2(u, v);
        uvIndex++;
    }

    public void AddColor(float r, float g, float b) {
        colors[colorIndex] = new Color(r, g, b, 1f);
        colorIndex++;
    }

    public void AddColor(Color color) {
        colors[colorIndex] = color;
        colorIndex++;
    }

    public int GetVertexIndex(int x, int y) {
        return x + meshWidth * y;
    }

    Vector3[] CalculateNormalsTube() {
        Vector3[] vertexNormals = new Vector3[vertices.Length];
        int triangleCount = triangles.Length / 3;
        for (int i = 0; i < triangleCount; i++) {
            int normalTriangleIndex = i * 3;
            int vertexIndexA = triangles[normalTriangleIndex];
            int vertexIndexB = triangles[normalTriangleIndex + 1];
            int vertexIndexC = triangles[normalTriangleIndex + 2];
            Vector3 triangleNormal = SurfaceNormalFromIndices(vertexIndexA, vertexIndexB, vertexIndexC);
            vertexNormals[vertexIndexA] += triangleNormal;
            vertexNormals[vertexIndexB] += triangleNormal;
            vertexNormals[vertexIndexC] += triangleNormal;
        }
        // c edges
        for (int t = 0; t < meshHeight - 1; t++) {
            int c = meshWidth - 1;
            // last c column
            // triangle 1 = (c, t), (c, t + 1), (1, t + 1)
            // triangle 2 = (1, t + 1), (1, t), (c, t)
            Vector3 tri1Normal = SurfaceNormalFromIndices(GetVertexIndex(c, t), GetVertexIndex(c, t + 1), GetVertexIndex(1, t + 1));
            Vector3 tri2Normal = SurfaceNormalFromIndices(GetVertexIndex(1, t + 1), GetVertexIndex(1, t), GetVertexIndex(c, t));
            vertexNormals[GetVertexIndex(c, t)] += tri1Normal + tri2Normal;
            vertexNormals[GetVertexIndex(c, t + 1)] += tri1Normal;
            // first c column
            // triangle 1 = (c - 1, t), (c - 1, t + 1), (0, t + 1) 
            // triangle 2 = (0, t + 1), (0, t), (c - 1, t)
            tri1Normal = SurfaceNormalFromIndices(GetVertexIndex(c - 1, t), GetVertexIndex(c - 1, t + 1), GetVertexIndex(0, t + 1));
            tri2Normal = SurfaceNormalFromIndices(GetVertexIndex(0, t + 1), GetVertexIndex(0, t), GetVertexIndex(c - 1, t));
            vertexNormals[GetVertexIndex(0, t)] += tri1Normal;
            vertexNormals[GetVertexIndex(0, t + 1)] += tri1Normal + tri2Normal;
        }
        // t edges
        for (int c = 0; c < meshWidth - 1; c++) {
            int t = meshHeight - 1;
            // last t row
            // triangle 1 = (c, t), b(c, 1), b(c + 1, 1)
            // triangle 2 = b(c + 1, 1), (c + 1, t), (c, t)
            Vector3 tri1Normal = SurfaceNormalFromVertices(vertices[GetVertexIndex(c, t)], borderVertices[c, 1], borderVertices[c + 1, 1]);
            Vector3 tri2Normal = SurfaceNormalFromVertices(borderVertices[c + 1, 1], vertices[GetVertexIndex(c + 1, t)], vertices[GetVertexIndex(c, t)]);
            vertexNormals[GetVertexIndex(c, t)] += tri1Normal + tri2Normal;
            vertexNormals[GetVertexIndex(c + 1, t)] += tri2Normal;
            // first t row
            // triangle 1 = b(c, 0), (c, 0), (c + 1, 0)
            // triangle 2 = (c + 1, 0), b(c + 1, 0), b(c, 0)
            tri1Normal = SurfaceNormalFromVertices(borderVertices[c, 0], vertices[GetVertexIndex(c, 0)], vertices[GetVertexIndex(c + 1, 0)]);
            tri2Normal = SurfaceNormalFromVertices(vertices[GetVertexIndex(c + 1, 0)], borderVertices[c + 1, 0], borderVertices[c, 0]);
            vertexNormals[GetVertexIndex(c, 0)] += tri2Normal;
            vertexNormals[GetVertexIndex(c + 1, 0)] += tri1Normal + tri2Normal;
        }
        // corners
        {
            int c = meshWidth - 1;
            int t = meshHeight - 1;
            // (0, 0)
            // tri 1 = b(c - 1, 0), (c - 1, 0), (0, 0)
            // tri 2 = (0, 0), b(0, 0), b(c - 1, 0)
            Vector3 tri1Normal = SurfaceNormalFromVertices(borderVertices[c - 1, 0], vertices[GetVertexIndex(c - 1, 0)], vertices[GetVertexIndex(0, 0)]);
            Vector3 tri2Normal = SurfaceNormalFromVertices(vertices[GetVertexIndex(0, 0)], borderVertices[0, 0], borderVertices[c - 1, 0]);
            vertexNormals[GetVertexIndex(0, 0)] += tri1Normal + tri2Normal;
            // (c, 0)
            // tri 1 = b(c, 0), (c, 0), (1, 0)
            tri1Normal = SurfaceNormalFromVertices(borderVertices[c, 0], vertices[GetVertexIndex(c, 0)], vertices[GetVertexIndex(1, 0)]);
            vertexNormals[GetVertexIndex(c, 0)] += tri1Normal;
            // (0, t)
            // tri 2 = b(0, 1), (0, t), (c - 1, t)
            tri2Normal = SurfaceNormalFromVertices(borderVertices[0, 1], vertices[GetVertexIndex(0, t)], vertices[GetVertexIndex(c - 1, t)]);
            vertexNormals[GetVertexIndex(0, t)] += tri2Normal;
            // (c, t)
            // tri 1 = (c, t), b(c, 1), b(1, 1)
            // tri 2 = b(1, 1), (1, t), (c, t)
            tri1Normal = SurfaceNormalFromVertices(vertices[GetVertexIndex(c, t)], borderVertices[c, 1], borderVertices[1, 1]);
            tri2Normal = SurfaceNormalFromVertices(borderVertices[1, 1], vertices[GetVertexIndex(1, t)], vertices[GetVertexIndex(c, t)]);
            vertexNormals[GetVertexIndex(c, t)] += tri1Normal + tri2Normal;
        }
        // normalize
        for (int i = 0; i < vertexNormals.Length; i++) {
            vertexNormals[i].Normalize();
        }
        return vertexNormals;
    }

    Vector3 SurfaceNormalFromIndices(int indexA, int indexB, int indexC) {
        Vector3 pointA = vertices[indexA];
        Vector3 pointB = vertices[indexB];
        Vector3 pointC = vertices[indexC];

        return Vector3.Cross(pointB - pointA, pointC - pointA).normalized;
    }

    Vector3 SurfaceNormalFromVertices(Vector3 pointA, Vector3 pointB, Vector3 pointC) {
        return Vector3.Cross(pointB - pointA, pointC - pointA).normalized;
    }

    public void BakeNormals() {
        // extracting the normal calculation allows it to be run on non-main threads
        bakedNormals = CalculateNormalsTube();
    }

    public Mesh CreateMesh() {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        if (colorIndex > 0) {
            mesh.colors = colors;
        }
        //mesh.RecalculateNormals();
        mesh.normals = bakedNormals;
        return mesh;
    }
}
