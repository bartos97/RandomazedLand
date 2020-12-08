using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace TerrainGeneration
{
    public class InfiniteTerrain
    {
        private readonly MapGenerator mapGenerator;

        private readonly Dictionary<Vector2, TerrainChunk> chunksRepository;
        private readonly List<TerrainChunk> lastVisibleChunks;

        private readonly int numOfVisibleChunksInDirection;
        private Vector2 playerFlatPosition = new Vector2(0, 0);
        private Vector2 playerPreviousFlatPosition = new Vector2(float.MaxValue, float.MaxValue);

        public InfiniteTerrain(MapGenerator mapGenerator)
        {
            Assert.IsTrue(LevelOfDetailConfig.maxViewDistance > LevelOfDetailConfig.chunkSize);

            this.mapGenerator = mapGenerator;
            numOfVisibleChunksInDirection = Mathf.RoundToInt(LevelOfDetailConfig.maxViewDistance / LevelOfDetailConfig.chunkSize / mapGenerator.terrainParameters.UniformScaleMultiplier);
            playerFlatPosition = new Vector2(0, 0);
            playerPreviousFlatPosition = new Vector2(float.MaxValue, float.MaxValue);
            chunksRepository = new Dictionary<Vector2, TerrainChunk>();
            lastVisibleChunks = new List<TerrainChunk>((numOfVisibleChunksInDirection * 2 + 1) * (numOfVisibleChunksInDirection * 2 + 1));
        }

        public void OnUpdate()
        {
            playerFlatPosition.x = mapGenerator.playerObject.position.x / mapGenerator.terrainParameters.UniformScaleMultiplier;
            playerFlatPosition.y = mapGenerator.playerObject.position.z / mapGenerator.terrainParameters.UniformScaleMultiplier;
            float playerDeltaPosition = Vector2.Distance(playerPreviousFlatPosition, playerFlatPosition);

            if (playerDeltaPosition > LevelOfDetailConfig.playerPositionThresholdForChunksUpdate)
            {
                UpdateVisibleChunks();
                playerPreviousFlatPosition = playerFlatPosition;
            }
        }

        private void UpdateVisibleChunks()
        {
            ClearLastChunks();
            int xMiddleChunkCoord = Mathf.RoundToInt(playerFlatPosition.x / LevelOfDetailConfig.chunkSize);
            int yMiddleChunkCoord = Mathf.RoundToInt(playerFlatPosition.y / LevelOfDetailConfig.chunkSize);

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
            private readonly InfiniteTerrain superiorObjectRef;

            private readonly LodMesh[] lodMeshes;
            private readonly LodMesh colliderLodMesh;
            private readonly Vector3 positionInWorld;
            private readonly Bounds positionBounds;

            private bool _isVisible;            
            private float[] noiseMap;
            private bool hasNoiseMap = false;
            private const int halfChunk = LevelOfDetailConfig.chunkSize / 2;

            public TerrainChunk(Vector2 gridCoords, InfiniteTerrain superior)
            {
                superiorObjectRef = superior;

                positionInWorld = new Vector3(gridCoords.x * LevelOfDetailConfig.chunkSize, 0f, gridCoords.y * LevelOfDetailConfig.chunkSize);
                positionBounds = new Bounds(positionInWorld, Vector3.one * LevelOfDetailConfig.chunkSize);

                meshObject = new GameObject($"Terrain chunk at ({gridCoords.x}, {gridCoords.y})");
                meshRenderer = meshObject.AddComponent<MeshRenderer>();
                meshFilter = meshObject.AddComponent<MeshFilter>();
                meshCollider = meshObject.AddComponent<MeshCollider>();

                meshRenderer.material = superiorObjectRef.mapGenerator.meshMaterial;
                meshObject.transform.position = positionInWorld * superiorObjectRef.mapGenerator.terrainParameters.UniformScaleMultiplier;
                meshObject.transform.localScale = Vector3.one * superiorObjectRef.mapGenerator.terrainParameters.UniformScaleMultiplier;
                meshObject.transform.parent = superiorObjectRef.mapGenerator.transform;

                lodMeshes = new LodMesh[LevelOfDetailConfig.distanceThresholds.Length];
                for (int i = 0; i < LevelOfDetailConfig.distanceThresholds.Length; i++)
                {
                    lodMeshes[i] = new LodMesh(LevelOfDetailConfig.distanceThresholds[i].lod, UpdateVisibility, superiorObjectRef.mapGenerator.RequestMeshData);
                }
                colliderLodMesh = lodMeshes[LevelOfDetailConfig.lodDistanceIndexForCollider];

                superiorObjectRef.mapGenerator.RequestNoiseMap(OnNoiseMapReceive, positionInWorld.x, positionInWorld.z);
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
                float sqrDistanceFromPlayer = positionBounds.SqrDistance(new Vector3(superiorObjectRef.playerFlatPosition.x, 0f, superiorObjectRef.playerFlatPosition.y));
                IsVisible = sqrDistanceFromPlayer <= LevelOfDetailConfig.maxViewDistance * LevelOfDetailConfig.maxViewDistance;

                if (!hasNoiseMap || !IsVisible)
                    return;

                int currentLodIndex = GetCurrentLodIndex(sqrDistanceFromPlayer);
                superiorObjectRef.lastVisibleChunks.Add(this);

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
                for (int i = 0; i < LevelOfDetailConfig.distanceThresholds.Length; i++)
                {
                    float dist = LevelOfDetailConfig.distanceThresholds[i].viewDistance;
                    if (sqrDistanceFromPlayer > dist * dist)
                        continue;
                    return i;
                }

                return LevelOfDetailConfig.distanceThresholds.Length - 1;
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
