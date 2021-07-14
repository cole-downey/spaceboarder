using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TextureGenerator {

    public static Texture2D TextureFromColorMap(Color[] colorMap, int width, int height) {

        Texture2D texture = new Texture2D(width, height);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.SetPixels(colorMap);
        texture.Apply();

        return texture;
    }

    public static Texture2D TextureFromHeightMap(float[,] noiseMap) {

        int width = noiseMap.GetLength(0);
        int height = noiseMap.GetLength(1);

        Color[] colorMap = new Color[width * height];
        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                colorMap[y * width + x] = Color.Lerp(Color.black, Color.white, noiseMap[x, y]);
            }
        }

        return TextureFromColorMap(colorMap, width, height);
    }

    public static Texture2D ColoredTextureFromHeightMap(float[,] noiseMap, TerrainType[] regions, int resolution = 1) {
        int width = noiseMap.GetLength(0);
        int height = noiseMap.GetLength(1);

        int texWidth = width * resolution;
        int texHeight = height * resolution;

        Color[] colorMap = new Color[texWidth * texHeight];
        for (int y = 0; y < texHeight; y++) {
            for (int x = 0; x < texWidth; x++) {
                float currentHeight = noiseMap[x / resolution, y / resolution];
                for (int r = 0; r < regions.Length; r++) {
                    if (currentHeight >= regions[r].height) {
                        colorMap[y * texWidth + x] = regions[r].color;
                    } else {
                        break;
                    }
                }
            }
        }
        return TextureFromColorMap(colorMap, texWidth, texHeight);
    }



}
