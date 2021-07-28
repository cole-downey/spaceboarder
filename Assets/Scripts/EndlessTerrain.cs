using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndlessTerrain : MonoBehaviour {
    const float scale = 1f;

    const float moveThresholdForChunkUpdate = 25;
    const float sqrMoveThresholdForChunkUpdate = moveThresholdForChunkUpdate * moveThresholdForChunkUpdate; // getting square dist is faster than actual dist
    Vector2 viewerLastPos;

    public LODInfo[] detailLevels;
    public static float maxViewDistance;

    public Transform viewer;
    public Material mapMaterial;
    public PhysicMaterial physicMaterial;

    public static Vector2 viewerPosition;
    static MapGenerator mapGenerator;
    float chunkRadius;
    float chunkLength;
    int chunksVisibleInViewDistance;
    public int chunksBehind = 3;
    public bool collidersEnabled = true;

    Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
    static List<TerrainChunk> terrainChunksVisibleLastUpdate = new List<TerrainChunk>();

    void Start() {
        mapGenerator = FindObjectOfType<MapGenerator>();

        maxViewDistance = detailLevels[detailLevels.Length - 1].visibleDstThreshold;

        chunkRadius = mapGenerator.meshRadius;
        chunkLength = mapGenerator.meshLength;
        chunksVisibleInViewDistance = Mathf.RoundToInt(maxViewDistance / chunkLength);

        viewerPosition = new Vector2(viewer.position.x, viewer.position.z) / scale;
        viewerLastPos = viewerPosition;
        UpdateVisibleChunks();
    }

    void Update() {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z) / scale;
        if ((viewerLastPos - viewerPosition).sqrMagnitude > sqrMoveThresholdForChunkUpdate) {
            viewerLastPos = viewerPosition;
            UpdateVisibleChunks();
        }
    }

    void UpdateVisibleChunks() {
        foreach (var chunk in terrainChunksVisibleLastUpdate) {
            chunk.SetVisible(false);
        }
        terrainChunksVisibleLastUpdate.Clear();

        // chunk coord starting at (0, 0), +1 per chunk
        int currentChunkCoordT = Mathf.RoundToInt(viewerPosition.y / chunkLength); // update this

        for (int tOffset = -chunksBehind; tOffset <= chunksVisibleInViewDistance; tOffset++) {
            Vector2 currentChunkCoord = new Vector2(0, currentChunkCoordT + tOffset);
            if (terrainChunkDictionary.ContainsKey(currentChunkCoord)) {
                terrainChunkDictionary[currentChunkCoord].UpdateChunk();
            } else {
                terrainChunkDictionary.Add(currentChunkCoord, new TerrainChunk(currentChunkCoord, chunkRadius, chunkLength, detailLevels, transform, mapMaterial, physicMaterial, collidersEnabled));
            }
        }
    }

    public class TerrainChunk {

        GameObject meshObject;
        Vector2 position;
        Bounds bounds;

        MapData mapData;
        bool mapDataReceived;

        MeshRenderer meshRenderer;
        MeshFilter meshFilter;
        MeshCollider meshCollider;
        bool colliderEnabled;

        LODInfo[] detailLevels;
        LODMesh[] lodMeshes;
        LODMesh collisionLODMesh;
        int previousLODIndex = -1;

        public TerrainChunk(Vector2 coord, float radius, float length, LODInfo[] detailLevels, Transform parent, Material material, PhysicMaterial phyMaterial, bool colliderEnabled = true) {
            position = coord * length;
            bounds = new Bounds(position, new Vector3(radius * 2f, radius * 2f, length));
            Vector3 posV3 = new Vector3(position.x, 0, position.y);
            this.detailLevels = detailLevels;

            meshObject = new GameObject("Terrain Chunk");
            meshObject.tag = "Terrain";
            meshObject.layer = 6;
            meshRenderer = meshObject.AddComponent<MeshRenderer>(); // add component returns component it adds
            meshFilter = meshObject.AddComponent<MeshFilter>();
            meshCollider = meshObject.AddComponent<MeshCollider>();
            this.colliderEnabled = colliderEnabled;
            meshRenderer.material = material;
            meshCollider.material = phyMaterial;

            meshObject.transform.position = posV3 * scale;
            meshObject.transform.parent = parent;
            meshObject.transform.localScale = Vector3.one * scale;
            SetVisible(false);

            lodMeshes = new LODMesh[detailLevels.Length];
            for (int i = 0; i < detailLevels.Length; i++) {
                lodMeshes[i] = new LODMesh(detailLevels[i].lod, UpdateChunk);
                if (detailLevels[i].useForCollider) {
                    collisionLODMesh = lodMeshes[i];
                }
            }

            mapGenerator.RequestMapData(OnMapDataReceived, (float)coord.y * mapGenerator.tOffsetPerChunk);
        }

        void OnMapDataReceived(MapData mapData) {
            this.mapData = mapData;
            mapDataReceived = true;

            UpdateChunk();
        }

        public void UpdateChunk() {
            if (mapDataReceived) {
                float viewerDistanceFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
                bool visible = viewerDistanceFromNearestEdge <= maxViewDistance;

                if (visible) {
                    int lodIndex = 0;

                    for (int i = 0; i < detailLevels.Length - 1; i++) {
                        if (viewerDistanceFromNearestEdge > detailLevels[i].visibleDstThreshold) {
                            lodIndex++;
                        } else {
                            break;
                        }
                    }
                    if (lodIndex != previousLODIndex) {
                        LODMesh lodMesh = lodMeshes[lodIndex];
                        if (lodMesh.hasMesh) {
                            meshFilter.mesh = lodMesh.mesh;
                            previousLODIndex = lodIndex;
                        } else if (!lodMesh.hasRequestedMesh) {
                            lodMesh.RequestMesh(mapData);
                        }
                    }
                    if (colliderEnabled && lodIndex == 0) {
                        if (collisionLODMesh.hasMesh) {
                            meshCollider.sharedMesh = collisionLODMesh.mesh;
                        } else if (!collisionLODMesh.hasRequestedMesh) {
                            collisionLODMesh.RequestMesh(mapData);
                        }
                    }

                    terrainChunksVisibleLastUpdate.Add(this);
                }

                SetVisible(visible);
            }
        }

        public void SetVisible(bool visible) {
            meshObject.SetActive(visible);
        }

        public bool isVisible() {
            return meshObject.activeSelf;
        }

    }

    class LODMesh {
        public Mesh mesh;
        public bool hasRequestedMesh;
        public bool hasMesh;
        int lod;
        System.Action updateCallback;

        public LODMesh(int lod, System.Action updateCallback) {
            this.lod = lod;
            this.updateCallback = updateCallback;
        }

        public void RequestMesh(MapData mapData) {
            hasRequestedMesh = true;
            mapGenerator.RequestMeshData(OnMeshDataReceived, mapData, lod);
        }

        public void OnMeshDataReceived(MeshData meshData) {
            mesh = meshData.CreateMesh();
            hasMesh = true;
            updateCallback();
        }
    }

    [System.Serializable]
    public struct LODInfo {
        [Range(0, 6)]
        public int lod;
        public float visibleDstThreshold; // when a new lod becomes active
        public bool useForCollider;
    }

    void OnValidate() {
        if (chunksBehind < 0) chunksBehind = 0;
    }
}
