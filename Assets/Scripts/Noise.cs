using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Noise {
    public enum NormalizeMode { Local, Global };

    public static NoiseData GenerateCircleNoiseMap(int seed, int circumference, int length, float radius, float speed, float tOffset, int octaves, float persistance, float lacunarity, NormalizeMode normalizeMode = Noise.NormalizeMode.Local) {
        float[,] noiseMap = new float[circumference, length];
        float[,] borderMap = new float[circumference, 2];
        SimplexNoise sNoise = new SimplexNoise();
        System.Random prng = new System.Random(seed);

        float localMinNoiseHeight = float.MaxValue;
        float localMaxNoiseHeight = float.MinValue;
        float maxPossibleHeight = 0;

        float amplitude = 1f;
        float frequency = 1f;

        // generate offsets for each octave
        Vector3[] octaveOffsets = new Vector3[octaves];
        for (int o = 0; o < octaves; o++) {
            float offsetX = (float)prng.Next(-100000, 100000);
            float offsetY = (float)prng.Next(-100000, 100000);
            float offsetT = (float)prng.Next(-100000, 100000) / 100f + tOffset; // too high of t values make stripes in mesh at lower speeds
            octaveOffsets[o] = new Vector3(offsetX, offsetY, offsetT);

            // for calculating global height
            maxPossibleHeight += amplitude;
            amplitude *= persistance;
        }


        // loop through points on map and evaluate perlin noise
        float thetaSlice = 2.0f * Mathf.PI / (circumference - 1); // what each increment translates to in noise space
        float tSlice = 2.0f * Mathf.PI * radius * speed / (length - 1);
        for (int t = -1; t <= length; t++) {
            for (int c = 0; c < circumference; c++) {
                amplitude = 1f;
                frequency = 1f;
                float noiseHeight = 0;
                for (int o = 0; o < octaves; o++) {
                    // higher frequency = larger radius
                    float theta = c * thetaSlice;
                    float sampleX = Mathf.Cos(theta) * radius * frequency + octaveOffsets[o].x;
                    float sampleY = Mathf.Sin(theta) * radius * frequency + octaveOffsets[o].y;
                    float sampleT = ((float)t * tSlice + octaveOffsets[o].z) * frequency;
                    Vector3 point = new Vector3(sampleX, sampleY, sampleT);
                    float simplexValue = sNoise.Evaluate(point) + 1; // in range [-1, 1]
                    noiseHeight += simplexValue * amplitude;
                    amplitude *= persistance;
                    frequency *= lacunarity;
                }

                if (noiseHeight > localMaxNoiseHeight) localMaxNoiseHeight = noiseHeight;
                if (noiseHeight < localMinNoiseHeight) localMinNoiseHeight = noiseHeight;
                if (t == -1) {
                    borderMap[c, 0] = noiseHeight;
                } else if (t == length) {
                    borderMap[c, 1] = noiseHeight;
                } else {
                    noiseMap[c, t] = noiseHeight;
                }
            }
        }

        // normalize
        //Debug.Log($"[{localMinNoiseHeight}, {localMaxNoiseHeight}]");
        for (int t = 0; t < length; t++) {
            for (int c = 0; c < circumference; c++) {
                switch (normalizeMode) {
                    case NormalizeMode.Local:
                        noiseMap[c, t] = Mathf.InverseLerp(localMinNoiseHeight, localMaxNoiseHeight, noiseMap[c, t]);
                        break;
                    case NormalizeMode.Global:
                        float normalizedHeight = Mathf.InverseLerp(0.35f, 3.35f, noiseMap[c, t]); // have to estimate normalized value
                        noiseMap[c, t] = Mathf.Clamp(normalizedHeight, 0, int.MaxValue);
                        break;
                }
            }
        }
        for (int t = 0; t < borderMap.GetLength(1); t++) {
            for (int c = 0; c < borderMap.GetLength(0); c++) {
                switch (normalizeMode) {
                    case NormalizeMode.Local:
                        borderMap[c, t] = Mathf.InverseLerp(localMinNoiseHeight, localMaxNoiseHeight, borderMap[c, t]);
                        break;
                    case NormalizeMode.Global:
                        float normalizedHeight = Mathf.InverseLerp(0.35f, 3.35f, borderMap[c, t]); // have to estimate normalized value
                        borderMap[c, t] = Mathf.Clamp(normalizedHeight, 0, int.MaxValue);
                        break;
                }
            }
        }


        return new NoiseData(noiseMap, borderMap);
    }

    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offset, NormalizeMode normalizeMode = Noise.NormalizeMode.Local) {
        float[,] noiseMap = new float[mapWidth, mapHeight];

        System.Random prng = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];

        float maxPossibleHeight = 0;
        float amplitude = 1;
        float frequency = 1;

        for (int o = 0; o < octaves; o++) {
            float offsetX = prng.Next(-100000, 100000) - offset.x;
            float offsetY = prng.Next(-100000, 100000) - offset.y;
            octaveOffsets[o] = new Vector2(offsetX, offsetY);

            maxPossibleHeight += amplitude;
            amplitude *= persistance;
        }

        if (scale <= 0) scale = 0.0001f;

        float localMaxNoiseHeight = float.MinValue;
        float localMinNoiseHeight = float.MaxValue;

        float halfWidth = mapWidth / 2f;
        float halfHeight = mapHeight / 2f;

        for (int y = 0; y < mapHeight; y++) {
            for (int x = 0; x < mapWidth; x++) {
                amplitude = 1;
                frequency = 1;
                float noiseHeight = 0;

                for (int o = 0; o < octaves; o++) {
                    float sampleX = (x - halfWidth + octaveOffsets[o].x) / scale * frequency;
                    float sampleY = (y - halfHeight + octaveOffsets[o].y) / scale * frequency;
                    // move perlinvalue to range [-1, 1]
                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    noiseHeight += perlinValue * amplitude;

                    amplitude *= persistance;
                    frequency *= lacunarity;
                }
                if (noiseHeight > localMaxNoiseHeight) localMaxNoiseHeight = noiseHeight;
                if (noiseHeight < localMinNoiseHeight) localMinNoiseHeight = noiseHeight;
                noiseMap[x, y] = noiseHeight;
            }
        }
        for (int y = 0; y < mapHeight; y++) {
            for (int x = 0; x < mapWidth; x++) {
                // normalize
                switch (normalizeMode) {
                    case NormalizeMode.Local:
                        noiseMap[x, y] = Mathf.InverseLerp(localMinNoiseHeight, localMaxNoiseHeight, noiseMap[x, y]);
                        break;
                    case NormalizeMode.Global:
                        float normalizedHeight = (noiseMap[x, y] + 1) / (2f * maxPossibleHeight / 2f); // have to estimate normalized value
                        noiseMap[x, y] = Mathf.Clamp(normalizedHeight, 0, int.MaxValue);
                        break;
                }
            }
        }
        return noiseMap;
    }

}

public struct NoiseData {
    public readonly float[,] noiseMap;
    public readonly float[,] borderMap;

    public NoiseData(float[,] _noiseMap, float[,] _borderMap) {
        noiseMap = _noiseMap;
        borderMap = _borderMap;
    }
}
