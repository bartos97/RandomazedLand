using System;
using System.Collections;
using System.Collections.Generic;
using TerrainGeneration.DataStructures;
using UnityEngine;
using UnityEngine.Assertions;

namespace TerrainGeneration
{
    public class InfiniteTerrain : MonoBehaviour
    {
        public Transform playerTransform;

        private const float maxViewDistance = 250f;
        private const int chunkSize = MapGenerator.MapChunkVerticesCount - 1;
        private const int numOfVisibleChunksInViewDirection = (int)(maxViewDistance / chunkSize) + (maxViewDistance % chunkSize >= chunkSize / 2 ? 1 : 0); //round to closest int

        private readonly Dictionary<Vector2, TerrainChunk> chunksRepository = new Dictionary<Vector2, TerrainChunk>();
        private readonly List<TerrainChunk> lastVisibleChunks = new List<TerrainChunk>();

        private void Start()
        {
            Assert.IsTrue(maxViewDistance > chunkSize);
            playerTransform.position = Vector3.zero;
        }

        private void Update()
        {
            UpdateVisibleChunks();
        }

        private void UpdateVisibleChunks()
        {
            ClearLastChunks();
            int xMiddleChunkCoord = Mathf.RoundToInt(playerTransform.position.x / chunkSize);
            int yMiddleChunkCoord = Mathf.RoundToInt(playerTransform.position.z / chunkSize);

            for (int yChunkCoordIter = -numOfVisibleChunksInViewDirection; yChunkCoordIter <= numOfVisibleChunksInViewDirection; yChunkCoordIter++)
            {
                for (int xChunkCoordIter = -numOfVisibleChunksInViewDirection; xChunkCoordIter <= numOfVisibleChunksInViewDirection; xChunkCoordIter++)
                {
                    var chunkCoords = new Vector2(xMiddleChunkCoord + xChunkCoordIter, yMiddleChunkCoord + yChunkCoordIter);

                    if (chunksRepository.ContainsKey(chunkCoords))
                    {
                        chunksRepository[chunkCoords].UpdateVisibility(playerTransform.position, maxViewDistance);
                        if (chunksRepository[chunkCoords].IsVisible)
                        {
                            lastVisibleChunks.Add(chunksRepository[chunkCoords]);
                        }
                    }
                    else
                    {
                        chunksRepository.Add(chunkCoords, new TerrainChunk(chunkCoords, chunkSize, this.transform));
                    }
                }
            }
        }

        private void ClearLastChunks()
        {
            foreach (var chunk in lastVisibleChunks)
                chunk.SetVisibility(false);
            lastVisibleChunks.Clear();
        }
    }
}
