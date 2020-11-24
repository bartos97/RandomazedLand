using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace TerrainGeneration
{
    public class InfiniteTerrain : MonoBehaviour
    {
        public static MapGenerator mapGenerator;
        public Transform playerObject;
        public Material material;

        private const int chunkSize = MapGenerator.MapChunkVerticesCount - 1;
        private const float playerPositionThresholdForChunksUpdate = chunkSize / 2f;
        private static readonly LodDistanceInfo[] lodDistances = { 
            new LodDistanceInfo( LevelOfDetail._1, 250),
            new LodDistanceInfo( LevelOfDetail._2, 500),
            new LodDistanceInfo( LevelOfDetail._4, 750),
        };

        public static float maxViewDistance;
        private readonly int numOfVisibleChunksInDirection;

        private readonly Dictionary<Vector2, TerrainChunk> chunksRepository = new Dictionary<Vector2, TerrainChunk>();
        private static readonly List<TerrainChunk> lastVisibleChunks = new List<TerrainChunk>();
        public static Vector2 playerFlatPosition;
        private Vector2 playerPreviousFlatPosition;

        public InfiniteTerrain()
        {
            maxViewDistance = lodDistances[lodDistances.Length - 1].viewDistance;
            numOfVisibleChunksInDirection = Mathf.RoundToInt(maxViewDistance / chunkSize);
        }

        private void Start()
        {
            Assert.IsTrue(maxViewDistance > chunkSize);
            mapGenerator = FindObjectOfType<MapGenerator>();
            playerObject.position = Vector3.zero;
            playerPreviousFlatPosition = new Vector2(playerPositionThresholdForChunksUpdate * 2, playerPositionThresholdForChunksUpdate * 2);
        }

        private void Update()
        {
            playerFlatPosition.x = playerObject.position.x;
            playerFlatPosition.y = playerObject.position.z;
            float playerDeltaPosition = Vector2.Distance(playerPreviousFlatPosition, playerFlatPosition);

            if (playerDeltaPosition > playerPositionThresholdForChunksUpdate)
            {
                UpdateVisibleChunks();
                playerPreviousFlatPosition.x = playerFlatPosition.x;
                playerPreviousFlatPosition.y = playerFlatPosition.y;
            }
        }

        private void UpdateVisibleChunks()
        {
            ClearLastChunks();
            int xMiddleChunkCoord = Mathf.RoundToInt(playerFlatPosition.x / chunkSize);
            int yMiddleChunkCoord = Mathf.RoundToInt(playerFlatPosition.y / chunkSize);

            for (int yChunkCoordIter = -numOfVisibleChunksInDirection; yChunkCoordIter <= numOfVisibleChunksInDirection; yChunkCoordIter++)
            {
                for (int xChunkCoordIter = -numOfVisibleChunksInDirection; xChunkCoordIter <= numOfVisibleChunksInDirection; xChunkCoordIter++)
                {
                    var chunkCoords = new Vector2(xMiddleChunkCoord + xChunkCoordIter, yMiddleChunkCoord + yChunkCoordIter);

                    if (chunksRepository.ContainsKey(chunkCoords))
                    {
                        chunksRepository[chunkCoords].UpdateVisibility();
                    }
                    else
                    {
                        chunksRepository.Add(chunkCoords, new TerrainChunk(chunkCoords, chunkSize, transform, material));
                    }
                }
            }
        }

        private void ClearLastChunks()
        {
            foreach (var chunk in lastVisibleChunks)
                chunk.IsVisible = false;
            lastVisibleChunks.Clear();
        }

        private struct LodDistanceInfo
        {
            public LevelOfDetail lod;
            public float viewDistance;

            public LodDistanceInfo(LevelOfDetail lod, float viewDistance)
            {
                this.lod = lod;
                this.viewDistance = viewDistance;
            }
        }

        private class TerrainChunk
        {
            private readonly GameObject meshObject;
            private readonly MeshRenderer meshRenderer;
            private readonly MeshFilter meshFilter;

            private readonly LodMesh[] lodMeshes;
            private readonly Vector3 positionInWorld;
            private readonly Bounds positionBounds;

            private bool _isVisible;            
            private float[] noiseMap;
            private bool hasNoiseMap = false;

            public TerrainChunk(Vector2 gridCoords, int size, Transform parentObject, Material material)
            {
                positionInWorld = new Vector3(gridCoords.x * size, 0f, gridCoords.y * size);
                positionBounds = new Bounds(positionInWorld, Vector3.one * size);

                meshObject = new GameObject($"Terrain chunk at ({gridCoords.x}, {gridCoords.y})");
                meshRenderer = meshObject.AddComponent<MeshRenderer>();
                meshFilter = meshObject.AddComponent<MeshFilter>();

                meshRenderer.material = material;
                meshObject.transform.position = positionInWorld;
                meshObject.transform.parent = parentObject;

                lodMeshes = new LodMesh[lodDistances.Length];
                for (int i = 0; i < lodDistances.Length; i++)
                {
                    lodMeshes[i] = new LodMesh(lodDistances[i].lod, UpdateVisibility);
                }

                mapGenerator.RequestNoiseMap(OnNoiseMapReceive, positionInWorld.x, positionInWorld.z);
            }

            public bool IsVisible
            {
                get { return _isVisible; }
                set
                {
                    _isVisible = value;
                    meshObject.SetActive(value);
                }
            }

            public void UpdateVisibility()
            {
                if (!hasNoiseMap)
                    return;

                float sqrDistanceFromPlayer = positionBounds.SqrDistance(new Vector3(playerFlatPosition.x, 0f, playerFlatPosition.y));
                IsVisible = sqrDistanceFromPlayer <= maxViewDistance * maxViewDistance;

                if (!IsVisible)
                    return;

                int currentLodIndex = GetCurrentLodIndex(sqrDistanceFromPlayer);
                if (lodMeshes[currentLodIndex].HasMesh)
                    meshFilter.mesh = lodMeshes[currentLodIndex].Mesh;
                else if (!lodMeshes[currentLodIndex].HasRequestedMesh)
                    lodMeshes[currentLodIndex].MakeMeshRequest(noiseMap);

                lastVisibleChunks.Add(this);
            }

            private int GetCurrentLodIndex(float sqrDistanceFromPlayer)
            {
                for (int i = 0; i < lodDistances.Length; i++)
                {
                    if (sqrDistanceFromPlayer > lodDistances[i].viewDistance * lodDistances[i].viewDistance)
                        continue;
                    return i;
                }

                return lodDistances.Length - 1;
            }

            private void OnNoiseMapReceive(float[] noiseMap)
            {
                this.noiseMap = noiseMap;
                hasNoiseMap = true;
                UpdateVisibility();
            }
        }

        private class LodMesh
        {
            public readonly LevelOfDetail lod;
            public Mesh Mesh { get; private set; }
            public bool HasMesh { get; private set; } = false;
            public bool HasRequestedMesh { get; private set; } = false;
            private readonly Action onMeshDataReceivedCallback;

            public LodMesh(LevelOfDetail lod, Action callback)
            {
                this.lod = lod;
                this.onMeshDataReceivedCallback = callback;
            }

            public void MakeMeshRequest(float[] noiseMap)
            {
                HasRequestedMesh = true;
                mapGenerator.RequestMeshData(OnMeshDataReceived, noiseMap, lod);
            }

            private void OnMeshDataReceived(MeshData meshData)
            {
                Mesh = meshData.Create();
                HasMesh = true;
                onMeshDataReceivedCallback();
            }
        }
    }
}
