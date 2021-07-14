using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshGenerator {
    public static Gradient regions;

    public static MeshData GenerateTube(NoiseData noiseData, float meshRadius, float meshLength, float rScale, AnimationCurve _heightCurve, int lod = 0) {
        float[,] radiusMap = noiseData.noiseMap;
        float[,] borderMap = noiseData.borderMap;
        AnimationCurve heightCurve = new AnimationCurve(_heightCurve.keys); // to make thread-safe
        int mapCircumference = radiusMap.GetLength(0);
        int mapLength = radiusMap.GetLength(1);

        lod = (lod == 0) ? 1 : lod * 2;

        int nVerticesC = (mapCircumference - 1) / lod + 1;
        int nVerticesT = (mapLength - 1) / lod + 1;
        MeshData meshData = new MeshData(nVerticesC, nVerticesT);
        int vertexIndex = 0;

        float tSlice = meshLength / (mapLength - 1);
        float thetaSlice = 2 * Mathf.PI / (mapCircumference - 1);
        for (int t = -lod; t < mapLength + lod; t += lod) {
            for (int c = 0; c < mapCircumference; c += lod) {
                float theta = c * thetaSlice;
                float z = t * tSlice;
                if (t == -lod) {
                    float r = (1f - heightCurve.Evaluate(borderMap[c, 0]) * rScale) * meshRadius;
                    float x = Mathf.Cos(theta) * r;
                    float y = Mathf.Sin(theta) * r;
                    meshData.AddBorder(x, y, z, 0);
                } else if (t == mapLength + lod - 1) {
                    float r = (1f - heightCurve.Evaluate(borderMap[c, 1]) * rScale) * meshRadius;
                    float x = Mathf.Cos(theta) * r;
                    float y = Mathf.Sin(theta) * r;
                    meshData.AddBorder(x, y, z, 1);
                } else {
                    float r = (1f - heightCurve.Evaluate(radiusMap[c, t]) * rScale) * meshRadius;
                    float x = Mathf.Cos(theta) * r;
                    float y = Mathf.Sin(theta) * r;
                    meshData.AddVertice(x, y, z);
                    meshData.AddUV(c / (float)mapCircumference, t / (float)mapLength);
                    meshData.AddColor(radiusMap[c, t], 0f, 0f);

                    if (c < mapCircumference - 1 && t < mapLength - 1) {
                        meshData.AddTriangle(vertexIndex, vertexIndex + nVerticesC, vertexIndex + nVerticesC + 1);
                        meshData.AddTriangle(vertexIndex + nVerticesC + 1, vertexIndex + 1, vertexIndex);
                    }
                    vertexIndex++;
                }
            }
        }
        meshData.BakeNormals();
        return meshData;
    }

}
