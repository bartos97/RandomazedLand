using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Assertions;
using TerrainGeneration.Utils;

namespace TerrainGeneration
{
    public class InfiniteTerrain
    {
        private readonly MapGenerator mapGenerator;

        private readonly float[] flatPlane = new float[MapGenerator.mapChunkVerticesPerLineWithBorder * MapGenerator.mapChunkVerticesPerLineWithBorder];
        private Dictionary<Vector2, TerrainChunk> chunksRepository;
        private Dictionary<Vector2, BorderChunkType> borderChunksMap;
        private List<TerrainChunk> lastVisibleChunks;

        private int numOfVisibleChunksInDirection;
        private float maxViewDistance;
        private LodDistance[] distanceThresholds;

        private Vector2 playerFlatPosition = new Vector2(0, 0);
        private Vector2 playerPreviousFlatPosition = new Vector2(float.MaxValue, float.MaxValue);

        public InfiniteTerrain(MapGenerator mapGenerator)
        {
            this.mapGenerator = mapGenerator;
        }

        public void OnStart()
        {
            distanceThresholds = mapGenerator.ActiveTerrainParams.useFalloffMap ? LevelOfDetailConfig.distanceThresholdsFaloffMap : LevelOfDetailConfig.distanceThresholds;
            maxViewDistance = distanceThresholds[distanceThresholds.Length - 1].viewDistance;
            numOfVisibleChunksInDirection = Mathf.RoundToInt(maxViewDistance / InfiniteTerrainConfig.chunkSize / mapGenerator.ActiveTerrainParams.UniformScaleMultiplier);

            playerFlatPosition = new Vector2(0, 0);
            playerPreviousFlatPosition = new Vector2(float.MaxValue, float.MaxValue);

            chunksRepository = new Dictionary<Vector2, TerrainChunk>((numOfVisibleChunksInDirection * 2 + 1) * (numOfVisibleChunksInDirection * 2 + 1));
            lastVisibleChunks = new List<TerrainChunk>((numOfVisibleChunksInDirection * 2 + 1) * (numOfVisibleChunksInDirection * 2 + 1));

            InitBorderChunksMap();
        }

        public void OnUpdate()
        {
            playerFlatPosition.x = mapGenerator.playerObject.position.x / mapGenerator.ActiveTerrainParams.UniformScaleMultiplier;
            playerFlatPosition.y = mapGenerator.playerObject.position.z / mapGenerator.ActiveTerrainParams.UniformScaleMultiplier;
            float playerDeltaPosition = Vector2.Distance(playerPreviousFlatPosition, playerFlatPosition);

            if (playerDeltaPosition > LevelOfDetailConfig.playerPositionThresholdForChunksUpdate)
            {
                UpdateVisibleChunks();
                playerPreviousFlatPosition = playerFlatPosition;
            }
        }

        private void InitBorderChunksMap()
        {
            float max = (float)InfiniteTerrainConfig.maxChunkGridCoord;

            borderChunksMap = new Dictionary<Vector2, BorderChunkType>(InfiniteTerrainConfig.maxChunkGridCoord * 6 + 2)
            {
                { new Vector2(-max, -max), BorderChunkType.BottomLeft },
                { new Vector2( max, -max), BorderChunkType.BottomRight },
                { new Vector2(-max,  max), BorderChunkType.TopLeft },
                { new Vector2( max,  max), BorderChunkType.TopRight },
            };

            for (int x = -InfiniteTerrainConfig.maxChunkGridCoord + 1; x < InfiniteTerrainConfig.maxChunkGridCoord; x++)
            {
                borderChunksMap.Add(new Vector2(x, max), BorderChunkType.TopMiddle);
                borderChunksMap.Add(new Vector2(x, -max), BorderChunkType.BottomMiddle);
            }

            for (int y = -InfiniteTerrainConfig.maxChunkGridCoord + 1; y < InfiniteTerrainConfig.maxChunkGridCoord; y++)
            {
                borderChunksMap.Add(new Vector2(-max, y), BorderChunkType.MiddleLeft);
                borderChunksMap.Add(new Vector2(max, y), BorderChunkType.MiddleRight);
            }
        }

        private void UpdateVisibleChunks()
        {
            ClearLastChunks();
            int xMiddleChunkCoord = Mathf.RoundToInt(playerFlatPosition.x / InfiniteTerrainConfig.chunkSize);
            int yMiddleChunkCoord = Mathf.RoundToInt(playerFlatPosition.y / InfiniteTerrainConfig.chunkSize);

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
                        chunksRepository.Add(chunkCoords, new TerrainChunk(chunkCoords, this));
                    }
                }
            }
        }

        private void ClearLastChunks()
        {
            foreach (var chunk in lastVisibleChunks)
                chunk.IsVisible = false;
        }

        private class TerrainChunk
        {
            private readonly GameObject meshObject;
            private readonly MeshRenderer meshRenderer;
            private readonly MeshFilter meshFilter;
            private readonly MeshCollider meshCollider;
            private readonly InfiniteTerrain superior;

            private readonly LodMesh[] lodMeshes;
            private readonly LodMesh colliderLodMesh;
            private readonly Vector3 positionInWorld;
            private readonly Bounds positionBounds;

            private bool _isVisible;            
            private float[] noiseMap;
            private bool hasNoiseMap = false;
            private readonly bool isOutside;

            public TerrainChunk(Vector2 gridCoords, InfiniteTerrain superior)
            {
                this.superior = superior;

                positionInWorld = new Vector3(gridCoords.x * InfiniteTerrainConfig.chunkSize, 0f, gridCoords.y * InfiniteTerrainConfig.chunkSize);
                positionBounds = new Bounds(positionInWorld, Vector3.one * InfiniteTerrainConfig.chunkSize);

                meshObject = new GameObject($"Terrain chunk at ({gridCoords.x}, {gridCoords.y})");
                meshRenderer = meshObject.AddComponent<MeshRenderer>();
                meshFilter = meshObject.AddComponent<MeshFilter>();
                meshCollider = meshObject.AddComponent<MeshCollider>();

                meshRenderer.material = this.superior.mapGenerator.meshMaterial;
                meshObject.transform.position = positionInWorld * this.superior.mapGenerator.ActiveTerrainParams.UniformScaleMultiplier;
                meshObject.transform.localScale = Vector3.one * this.superior.mapGenerator.ActiveTerrainParams.UniformScaleMultiplier;
                meshObject.transform.parent = this.superior.mapGenerator.transform;

                isOutside = this.superior.mapGenerator.ActiveTerrainParams.useFalloffMap
                        && (Mathf.Abs(gridCoords.x) > InfiniteTerrainConfig.maxChunkGridCoord || Mathf.Abs(gridCoords.y) > InfiniteTerrainConfig.maxChunkGridCoord);

                if (isOutside)
                {
                    lodMeshes = new LodMesh[]
                    {
                        new LodMesh(superior.distanceThresholds[superior.distanceThresholds.Length - 1].lod, UpdateVisibility, this.superior.mapGenerator.RequestMeshData)
                    };
                    colliderLodMesh = lodMeshes[0];
                    OnNoiseMapReceive(superior.flatPlane);
                }
                else
                {
                    lodMeshes = new LodMesh[superior.distanceThresholds.Length];

                    for (int i = 0; i < superior.distanceThresholds.Length; i++)
                        lodMeshes[i] = new LodMesh(superior.distanceThresholds[i].lod, UpdateVisibility, this.superior.mapGenerator.RequestMeshData);

                    colliderLodMesh = lodMeshes[LevelOfDetailConfig.lodDistanceIndexForCollider];

                    BorderChunkType border = this.superior.borderChunksMap.ContainsKey(gridCoords) ? this.superior.borderChunksMap[gridCoords] : BorderChunkType.Invalid;
                    this.superior.mapGenerator.RequestNoiseMap(OnNoiseMapReceive, positionInWorld.x, positionInWorld.z, gridCoords, border);
                }

            }

            public bool IsVisible
            {
                get { return _isVisible; }
                set
                {
                    if (value == _isVisible)
                        return;
                    _isVisible = value;
                    meshObject.SetActive(value);
                }
            }

            public void UpdateVisibility()
            {
                float sqrDistanceFromPlayer = positionBounds.SqrDistance(new Vector3(superior.playerFlatPosition.x, 0f, superior.playerFlatPosition.y));
                IsVisible = sqrDistanceFromPlayer <= superior.maxViewDistance * superior.maxViewDistance;

                if (!hasNoiseMap || !IsVisible)
                    return;

                int currentLodIndex = GetCurrentLodIndex(sqrDistanceFromPlayer);
                superior.lastVisibleChunks.Add(this);

                if (lodMeshes[currentLodIndex].HasMesh)
                {
                    meshFilter.mesh = lodMeshes[currentLodIndex].Mesh;
                }
                else if (!lodMeshes[currentLodIndex].HasRequestedMesh)
                {
                    lodMeshes[currentLodIndex].MakeMeshRequest(noiseMap);
                }

                if (currentLodIndex == 0)
                {
                    if (colliderLodMesh.HasMesh && meshCollider.sharedMesh == null)
                    {
                        meshCollider.sharedMesh = colliderLodMesh.Mesh;
                    }
                    else if (!colliderLodMesh.HasRequestedMesh)
                    {
                        colliderLodMesh.MakeMeshRequest(noiseMap);
                    }
                }
            }

            private int GetCurrentLodIndex(float sqrDistanceFromPlayer)
            {
                if (isOutside)
                    return 0;

                for (int i = 0; i < superior.distanceThresholds.Length; i++)
                {
                    float dist = superior.distanceThresholds[i].viewDistance;
                    if (sqrDistanceFromPlayer > dist * dist)
                        continue;
                    return i;
                }

                return superior.distanceThresholds.Length - 1;
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
            private readonly Action<Action<MeshData>, float[], LevelOfDetail> meshRequestFunction;

            public LodMesh(LevelOfDetail lod, Action onMeshCallback, Action<Action<MeshData>, float[], LevelOfDetail> requestMeshData)
            {
                this.lod = lod;
                this.onMeshDataReceivedCallback = onMeshCallback;
                this.meshRequestFunction = requestMeshData;
            }

            public void MakeMeshRequest(float[] noiseMap)
            {
                HasRequestedMesh = true;
                meshRequestFunction(OnMeshDataReceived, noiseMap, lod);
            }

            private void OnMeshDataReceived(MeshData meshData)
            {
                Mesh = meshData.GetUnityMesh();
                HasMesh = true;
                onMeshDataReceivedCallback();
            }
        }
    }
}
