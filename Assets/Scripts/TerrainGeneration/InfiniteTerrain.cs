using System.Collections.Generic;
using DataStructures;
using UnityEngine;
using UnityEngine.Assertions;

namespace TerrainGeneration
{
    public class InfiniteTerrain : MonoBehaviour
    {
        public Transform player;
        public static MapGenerator mapGenerator;
        public Material material;

        private const float maxViewDistance = 250f;
        private const int chunkSize = MapGenerator.MapChunkVerticesCount - 1;
        private const int numOfVisibleChunksInViewDirection = (int)(maxViewDistance / chunkSize) + (maxViewDistance % chunkSize >= chunkSize / 2 ? 1 : 0); //round to closest int

        private readonly Dictionary<Vector2, TerrainChunk> chunksRepository = new Dictionary<Vector2, TerrainChunk>();
        private readonly List<TerrainChunk> lastVisibleChunks = new List<TerrainChunk>();

        private void Start()
        {
            Assert.IsTrue(maxViewDistance > chunkSize);
            mapGenerator = FindObjectOfType<MapGenerator>();
            player.position = Vector3.zero;
        }

        private void Update()
        {
            UpdateVisibleChunks();
        }

        private void UpdateVisibleChunks()
        {
            ClearLastChunks();
            int xMiddleChunkCoord = Mathf.RoundToInt(player.position.x / chunkSize);
            int yMiddleChunkCoord = Mathf.RoundToInt(player.position.z / chunkSize);

            for (int yChunkCoordIter = -numOfVisibleChunksInViewDirection; yChunkCoordIter <= numOfVisibleChunksInViewDirection; yChunkCoordIter++)
            {
                for (int xChunkCoordIter = -numOfVisibleChunksInViewDirection; xChunkCoordIter <= numOfVisibleChunksInViewDirection; xChunkCoordIter++)
                {
                    var chunkCoords = new Vector2(xMiddleChunkCoord + xChunkCoordIter, yMiddleChunkCoord + yChunkCoordIter);

                    if (chunksRepository.ContainsKey(chunkCoords))
                    {
                        chunksRepository[chunkCoords].UpdateVisibility(player.position, maxViewDistance);
                        if (chunksRepository[chunkCoords].IsVisible)
                        {
                            lastVisibleChunks.Add(chunksRepository[chunkCoords]);
                        }
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

        private class TerrainChunk
        {
            private readonly GameObject mesh;
            private readonly MeshRenderer meshRenderer;
            private readonly MeshFilter meshFilter;
            private Vector3 positionInWorld;
            private Bounds positionBounds;
            private bool _isVisible;

            public TerrainChunk(Vector2 gridCoords, int size, Transform parentObject, Material material)
            {
                positionInWorld = new Vector3(gridCoords.x * size, 0f, gridCoords.y * size);
                positionBounds = new Bounds(positionInWorld, Vector3.one * size);

                mesh = new GameObject("Terrain chunk");
                meshRenderer = mesh.AddComponent<MeshRenderer>();
                meshFilter = mesh.AddComponent<MeshFilter>();

                meshRenderer.material = material;
                mesh.transform.position = positionInWorld;
                mesh.transform.parent = parentObject;
                mesh.SetActive(true);

                mapGenerator.RequestNoiseMap(onNoiseMapReceive, positionInWorld.x, positionInWorld.z);
            }

            public bool IsVisible
            {
                get { return _isVisible; }
                set
                {
                    _isVisible = value;
                    mesh.SetActive(value);
                }
            }

            public void UpdateVisibility(Vector3 playerPosition, float maxViewDistance)
            {
                float distanceFromPlayer = positionBounds.SqrDistance(playerPosition);
                IsVisible = distanceFromPlayer <= maxViewDistance * maxViewDistance; //distanceFromPlayer is squared
            }

            private void onNoiseMapReceive(float[] noiseMap)
            {
                mapGenerator.RequestMeshData(onMeshDataReceive, noiseMap);
            }

            private void onMeshDataReceive(MeshData meshData)
            {
                meshFilter.mesh = meshData.Create();
            }
        }
    }
}
