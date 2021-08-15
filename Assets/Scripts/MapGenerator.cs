using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour {
    public enum DrawMode { CircleMap, CircleMesh };
    public DrawMode drawMode;

    public Noise.NormalizeMode noiseNormalizeMode;

    public const int mapChunkSize = 241; // 240 is divisible by 2, 4, 8, 10, 12
    [Range(0, 6)]
    public int editorResolution;

    public float radiusNoiseSpace = 1.0f;
    public float speedNoiseSpace = 1.0f;
    public float tOffset = 0.0f;
    public const int mapCircumference = 241;
    public const int mapLength = 61;
    public float meshRadius = 10.0f;
    public float meshLength = 20.0f;
    public float tOffsetPerChunk { get; private set; }

    public int octaves;
    [Range(0, 1)]
    public float persistance;
    public float lacunarity;

    public int seed;
    public float meshRScale = 0.5f;
    public AnimationCurve meshHeightCurve;

    public bool autoUpdate;

    public Gradient snowGradient;
    public Material terrainMat;

    Queue<MapThreadInfo<MapData>> mapDataThreadInfoQueue = new Queue<MapThreadInfo<MapData>>();
    Queue<MapThreadInfo<MeshData>> meshDataThreadInfoQueue = new Queue<MapThreadInfo<MeshData>>();

    void Awake() {
        tOffsetPerChunk = 2.0f * Mathf.PI * radiusNoiseSpace * speedNoiseSpace;
        MapDisplay display = FindObjectOfType<MapDisplay>();
        display.SetVisible(false);
        MeshGenerator.regions = snowGradient;
        terrainMat.SetTexture("regions", TextureGenerator.TextureFromGradient(snowGradient, 100));
    }

    public void DrawMapInEditor() {
        // MapData mapData = GenerateMapData(Vector2.zero);
        MapDisplay display = FindObjectOfType<MapDisplay>();
        if (drawMode == DrawMode.CircleMap) {
            float[,] noiseMap = Noise.GenerateCircleNoiseMap(seed, mapCircumference, mapLength, radiusNoiseSpace, speedNoiseSpace, tOffset, octaves, persistance, lacunarity, noiseNormalizeMode).noiseMap;
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(noiseMap), 1, 1);
        } else if (drawMode == DrawMode.CircleMesh) {
            NoiseData noiseData = Noise.GenerateCircleNoiseMap(seed, mapCircumference, mapLength, radiusNoiseSpace, speedNoiseSpace, tOffset, octaves, persistance, lacunarity, noiseNormalizeMode);
            display.DrawMesh(MeshGenerator.GenerateTube(noiseData, meshRadius, meshLength, meshRScale, meshHeightCurve, editorResolution));
        }
    }

    public void RequestMapData(Action<MapData> callback, float t) {
        ThreadStart threadStart = delegate {
            MapDataThread(callback, t);
        };

        new Thread(threadStart).Start();
    }

    void MapDataThread(Action<MapData> callback, float t) {
        MapData mapData = GenerateMapData(t);
        lock (mapDataThreadInfoQueue) { // auto unlock
            mapDataThreadInfoQueue.Enqueue(new MapThreadInfo<MapData>(callback, mapData));
        }
    }

    public void RequestMeshData(Action<MeshData> callback, MapData mapData, int lod) {
        ThreadStart threadStart = delegate {
            MeshDataThread(callback, mapData, lod);
        };
        new Thread(threadStart).Start();
    }

    void MeshDataThread(Action<MeshData> callback, MapData mapData, int lod) {
        MeshData meshData = MeshGenerator.GenerateTube(mapData.noiseData, meshRadius, meshLength, meshRScale, meshHeightCurve, lod);
        lock (meshDataThreadInfoQueue) {
            meshDataThreadInfoQueue.Enqueue(new MapThreadInfo<MeshData>(callback, meshData));
        }
    }

    void Update() {
        while (mapDataThreadInfoQueue.Count > 0) {
            MapThreadInfo<MapData> threadInfo;
            lock (mapDataThreadInfoQueue) {
                threadInfo = mapDataThreadInfoQueue.Dequeue();
            }
            threadInfo.callback(threadInfo.parameter);
        }
        while (meshDataThreadInfoQueue.Count > 0) {
            MapThreadInfo<MeshData> threadInfo;
            lock (meshDataThreadInfoQueue) {
                threadInfo = meshDataThreadInfoQueue.Dequeue();
            }
            threadInfo.callback(threadInfo.parameter);
        }
    }

    MapData GenerateMapData(float t) {
        NoiseData noiseData = Noise.GenerateCircleNoiseMap(seed, mapCircumference, mapLength, radiusNoiseSpace, speedNoiseSpace, t, octaves, persistance, lacunarity, noiseNormalizeMode);
        return new MapData(noiseData);
    }

    void OnValidate() {
        // called whenever variables changed in editor
        if (octaves < 1) octaves = 1;
        if (lacunarity < 1f) lacunarity = 1f;
        if (radiusNoiseSpace < 0.0f) radiusNoiseSpace = 0.0f;
        if (speedNoiseSpace < 0.0f) speedNoiseSpace = 0.0f;
        if (meshLength < 2) meshLength = 2;
        if (meshRadius < 2) meshRadius = 2;
        MeshGenerator.regions = snowGradient;
        terrainMat.SetTexture("regions", TextureGenerator.TextureFromGradient(snowGradient, 100));
    }

    struct MapThreadInfo<T> {
        public readonly Action<T> callback;
        public readonly T parameter;
        public MapThreadInfo(Action<T> _callback, T _parameter) {
            callback = _callback;
            parameter = _parameter;
        }
    }
}

// this tag makes it show up in editor
[System.Serializable]
public struct TerrainType {
    public string name;
    public float height;
    public Color color;
}

public struct MapData {
    public readonly NoiseData noiseData;

    public MapData(NoiseData _noiseData) {
        noiseData = _noiseData;
    }
}